using System.Text;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>Everything the decoder is currently working from (§0.0.1).</summary>
/// <param name="ToneHz">The pitch it is following.</param>
/// <param name="NoiseFloorDb">Where it believes the noise sits.</param>
/// <param name="PeakDb">Where it believes a keyed signal sits.</param>
/// <param name="ThresholdDb">The level it is deciding against.</param>
/// <param name="WordsPerMinute">The sending speed it believes it is tracking.</param>
/// <param name="HasSignal">Whether there is enough separation to decide anything.</param>
/// <param name="Elapsed">How much audio has gone through, from sample counts.</param>
public readonly record struct CwDecoderState(
    double ToneHz,
    double NoiseFloorDb,
    double PeakDb,
    double ThresholdDb,
    int WordsPerMinute,
    bool HasSignal,
    TimeSpan Elapsed);

/// <summary>
/// Reads Morse out of receive audio, and says how sure it is about every
/// character.
/// </summary>
/// <remarks>
/// <para>WHY THIS FEATURE IS THE ONE THAT MATTERS. CW is the last part of this
/// hobby still guarded by the claim that you have to develop an ear for it, and
/// that if you cannot read twenty words a minute you are not really doing it. A
/// decoder that works turns that from a gate into a preference. It does not
/// stop anybody learning to copy by ear, and it lets somebody who has held a
/// license for six years without making a contact read what is on the air
/// tonight.</para>
/// <para>The chain is the standard one and there is no cleverness in it,
/// deliberately. A bank of Goertzel filters finds the note and follows it
/// (<see cref="CwToneTracker"/>). An adaptive gate decides where the key is
/// down and keeps adapting so a fade does not silently end the decode
/// (<see cref="CwGate"/>). Runs of key-down and key-up are clustered into dits,
/// dahs and the three gap lengths by re-deriving the speed from what was
/// actually heard (<see cref="CwSpeedEstimator"/>). Patterns become characters
/// through a table that is allowed to say no
/// (<see cref="MorseAlphabet"/>).</para>
/// <para>WHAT IS NOT STANDARD IS THE CONFIDENCE, and it is the feature rather
/// than a decoration on it. Every character carries a score the decoder can
/// actually justify: how far its elements sat from the decisions made about
/// them, and how far the weakest of them stood above the noise and above any
/// station near enough to be confused with it. On top of that sits one veto,
/// for a character that arrived while somebody else was within a few decibels
/// of the same note. Nothing anywhere rounds a score up
/// (<see cref="CwConfidenceModel"/>).</para>
/// <para>NO CLOCK IS READ ANYWHERE BELOW THIS LINE. Elapsed time comes from
/// counting samples, so the same audio always decodes to the same text, on any
/// machine, at any speed, forever. That is what makes a WAV fixture worth
/// committing and what turns a decoder bug into a regression test rather than
/// an anecdote (HM-DEC-007, §5).</para>
/// <para>Characters are held back until there is enough evidence to decode them
/// against. The first dozen marks of a transmission are as likely to be three
/// dahs as three dits, so they are buffered and decoded once the speed is
/// known, which is why a decode arrives a couple of characters after the audio
/// rather than instantly.</para>
/// </remarks>
public sealed class CwDecoder
{
    /// <summary>How long a silence forces a decision on a short transmission.</summary>
    /// <remarks>
    /// Somebody who sends two characters and stops never produces the twelve
    /// marks the speed estimator wants. After this much silence the decoder
    /// works with what it has, and the confidence says what that was worth.
    /// </remarks>
    private const double ForcedFlushSeconds = 1.5;

    /// <summary>Marks below which a forced flush cannot name anything.</summary>
    /// <remarks>
    /// With fewer than this there is genuinely no way to tell a dit from a dah,
    /// because there is nothing to compare them against. The honest output is
    /// the placeholder (§0.0).
    /// </remarks>
    private const int MinimumForcedMarks = 4;

    /// <summary>
    /// How many dits of silence mean the sender has stopped rather than paused.
    /// </summary>
    /// <remarks>
    /// A CHARACTER IS NORMALLY HELD UNTIL ITS GAP ENDS, which is to say until
    /// the next element starts. That costs about three dits of latency and buys
    /// the thing the confidence model needs most: the actual length of the gap
    /// that ended the character. Whether a silence was one dit or three is what
    /// separates "U" from "IT", and a decoder that committed at the boundary
    /// would have no way to say how close a call it was.
    /// <para>Which leaves the case where nothing follows, because the sender
    /// stopped. Past this much silence the gap is unambiguous however long it
    /// eventually turns out to be, so the character is released and the
    /// terminating gap counts as certain.</para>
    /// </remarks>
    private const double EndOfTransmissionDits = 8;

    private readonly CwToneTracker _tracker;
    private readonly CwGate _gate = new();
    private readonly CwSpeedEstimator _speed;
    private readonly Action<ToneReading> _onReading;

    private readonly StringBuilder _pattern = new();
    private readonly List<PendingElement> _pending = new();
    private readonly List<double> _clarities = new();

    private IAudioSource? _attached;

    private bool _keyDown;
    private double _runSamples;
    private double _runSnrSum;
    private int _runHops;
    private double _worstSnrDb = double.MaxValue;
    private double _runContestedDb = double.MinValue;
    private double _contestedDb = double.MaxValue;
    private bool _silenceFlushed;
    private bool _sawAnyMark;
    private long _lastSample;

    /// <summary>Creates a decoder.</summary>
    /// <param name="sampleRate">Samples per second of the audio it will be fed.</param>
    /// <param name="expectedToneHz">
    /// The operator's CW pitch, as a place to start looking. The tracker hunts
    /// either side of it, since nobody tunes exactly.
    /// </param>
    public CwDecoder(int sampleRate, double expectedToneHz = 600)
    {
        SampleRate = Math.Max(1_000, sampleRate);
        _tracker = new CwToneTracker(SampleRate, expectedToneHz);
        _speed = new CwSpeedEstimator(SampleRate);
        _onReading = OnReading;
    }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>Raised for every character, space and placeholder, in order.</summary>
    public event Action<CwCharacter>? CharacterDecoded;

    /// <summary>What the decoder is currently working from.</summary>
    public CwDecoderState State => new(
        _tracker.ToneHz,
        _gate.NoiseFloorDb,
        _gate.PeakDb,
        _gate.NoiseFloorDb + ((_gate.PeakDb - _gate.NoiseFloorDb) * 0.5),
        _speed.IsReady ? _speed.WordsPerMinute : 0,
        _gate.HasSignal,
        TimeSpan.FromSeconds((double)_lastSample / SampleRate));

    /// <summary>How the signal has been behaving, for the plain-language note.</summary>
    public CwSignalWatch Watch { get; } = new();

    /// <summary>
    /// Listen to a source. Replaces any previous one.
    /// </summary>
    /// <param name="source">The source, or null to stop listening.</param>
    /// <exception cref="ArgumentException">The source runs at a different rate.</exception>
    public void Listen(IAudioSource? source)
    {
        if (ReferenceEquals(_attached, source))
        {
            return;
        }

        if (_attached is not null)
        {
            _attached.SamplesReady -= OnSamples;
        }

        if (source is not null && source.SampleRate != SampleRate)
        {
            throw new ArgumentException(
                $"the decoder was built for {SampleRate} Hz and this source runs at "
                + $"{source.SampleRate} Hz",
                nameof(source));
        }

        _attached = source;

        if (_attached is not null)
        {
            _attached.SamplesReady += OnSamples;
        }
    }

    /// <summary>
    /// The last half minute of exactly what the decoder was fed (HM-DEC-088).
    /// </summary>
    /// <remarks>
    /// **THE TAP IS HERE RATHER THAN AT THE SOUND CARD** so that what a capture
    /// contains is what the decoder received, not what something upstream
    /// believes it sent. A recording of a nearly-but-not-quite identical signal
    /// would settle nothing.
    /// </remarks>
    public AudioTap Tap { get; } = new();

    /// <summary>What is arriving, and what can be seen in it.</summary>
    public CwDecodeReport Report => new(
        Tap.Level,
        _tracker.ToneHz,
        _lastSnrDb,
        HasTone: !double.IsNaN(_lastSnrDb)
            && _lastSnrDb >= CwDecodeReport.ToneThresholdDb,
        _elementsSeen,
        _elementsResolved,
        _charactersEmitted,
        _charactersUnsure);

    private double _lastSnrDb = double.NaN;
    private int _elementsSeen;
    private int _elementsResolved;
    private int _charactersEmitted;
    private int _charactersUnsure;

    /// <summary>Feed samples directly, without a source.</summary>
    /// <param name="chunk">The samples.</param>
    public void Process(in AudioChunk chunk)
    {
        Tap.Take(chunk.Samples, chunk.SampleRate);
        _tracker.Process(chunk.Samples, chunk.FirstSampleIndex, _onReading);
    }

    /// <summary>
    /// Finish: decode anything still held and emit it.
    /// </summary>
    /// <remarks>
    /// Called at the end of a fixture, so the last character of a recording is
    /// not silently dropped. Live audio reaches the same place through the
    /// silence after the last element.
    /// </remarks>
    public void Flush()
    {
        Drain(force: true);

        // The recording simply ran out. Whatever silence it ended on is the
        // best evidence there is about the gap that closed the last character,
        // so it counts rather than being thrown away.
        if (!_keyDown && _runSamples > 0 && _speed.DitSamples > 0)
        {
            _clarities.Add(_speed.EndOfCharacterClarity(_runSamples));
        }

        FlushCharacter(force: true);
        _pending.Clear();
    }

    private void OnSamples(in AudioChunk chunk) => Process(in chunk);

    /// <summary>
    /// One measurement of the tone, five milliseconds after the last.
    /// </summary>
    private void OnReading(ToneReading reading)
    {
        _lastSample = reading.SampleIndex;

        // THE DE-GLITCH IS SIZED FROM THE ELEMENT (HM-DEC-088). The speed
        // estimator already knows how long a dit is here, and telling the gate
        // lets it integrate over a third of one instead of over a fixed
        // twenty-five milliseconds that is too short at twelve words a minute
        // and too long at sixty.
        _gate.FollowSpeed(_speed.DitSamples / _tracker.HopSamples);

        // SMOOTHED, BECAUSE ONE MEASUREMENT OF A RATIO OF TWO NOISY THINGS IS
        // NOISIER THAN EITHER. This is read by a screen rather than by the gate,
        // and a figure that jumps ten decibels five times a second cannot be
        // read by anybody (HM-DEC-088).
        if (reading.HasNoise)
        {
            var snr = reading.SnrDb;

            _lastSnrDb = double.IsNaN(_lastSnrDb)
                ? snr
                : _lastSnrDb + ((snr - _lastSnrDb) * SnrSmoothing);
        }

        var gate = _gate.Judge(reading.PowerDb, reading.NoiseDb);
        Watch.Observe(gate, _tracker.HopSamples, SampleRate);

        if (gate.KeyDown != _keyDown)
        {
            if (_keyDown)
            {
                OnMarkEnded();
            }
            else
            {
                OnGapEnded();
            }

            _keyDown = gate.KeyDown;
            _runSamples = 0;
            _runSnrSum = 0;
            _runHops = 0;
            _runContestedDb = double.MinValue;
        }

        _runSamples += _tracker.HopSamples;
        _runHops++;

        if (gate.KeyDown)
        {
            // HOW FAR ABOVE EVERYTHING ELSE, not just above the noise. A second
            // station a couple of hundred hertz away is not noise, and a decoder
            // that only measured its margin over the noise floor would report a
            // beautiful signal while quietly merging somebody else's dits into
            // the character. Taking the worse of the two margins is what turns
            // that from a confident wrong letter into an honest uncertain one.
            _runSnrSum += Math.Min(
                gate.SignalToNoiseDb, reading.MarginOverCompetitorDb);

            // THE STRONGEST MOMENT OF THE MARK, not the weakest. A keyed
            // element's rising and falling edges are amplitude transients, and a
            // transient throws energy right across the band, so the quietest
            // instant of any mark always looks contested even on an empty one.
            // What the veto is asking is whether another station was comparable
            // in strength to this one, and the fair place to ask that is where
            // this one was at full height.
            _runContestedDb = Math.Max(
                _runContestedDb, reading.RawMarginOverCompetitorDb);

            return;
        }

        // Still key-up. A gap that has grown past the character boundary means
        // whatever was pending is finished, whether or not the operator ever
        // sends anything else.
        CheckSilence();
    }

    /// <summary>How fast the displayed signal-to-noise figure follows.</summary>
    /// <remarks>About a second, which is slower than the eye needs and faster
    /// than a band changes.</remarks>
    private const double SnrSmoothing = 0.02;

    private void OnMarkEnded()
    {
        var snr = _runHops > 0 ? _runSnrSum / _runHops : 0;

        _sawAnyMark = true;
        _elementsSeen++;
        _speed.AddMark(_runSamples);
        _pending.Add(new PendingElement(IsMark: true, _runSamples, snr, _runContestedDb));

        Drain();
    }

    private void OnGapEnded()
    {
        if (!_sawAnyMark)
        {
            // Leading silence before anything has been heard. There is nothing
            // to measure and nothing to say.
            return;
        }

        _elementsSeen++;
        _speed.AddGap(_runSamples);
        _pending.Add(
            new PendingElement(IsMark: false, _runSamples, 0, double.MaxValue));

        Drain();
    }

    /// <summary>
    /// Emit whatever the growing silence has settled.
    /// </summary>
    private void CheckSilence()
    {
        if (!_sawAnyMark)
        {
            return;
        }

        if (!_speed.IsReady)
        {
            // A short transmission that will never reach the evidence the
            // estimator wants. Decide on what there is once the silence is long
            // enough that nothing more is coming.
            if (!_silenceFlushed
                && _runSamples >= ForcedFlushSeconds * SampleRate
                && _pending.Count > 0)
            {
                _silenceFlushed = true;
                Drain(force: true);
                FlushCharacter(force: true);
            }

            return;
        }

        if (_silenceFlushed || _runSamples < EndOfTransmissionDits * _speed.DitSamples)
        {
            return;
        }

        // The sender has stopped. However long this silence eventually runs it
        // is already past every boundary there is, so the gap that ended this
        // character is as certain as a gap gets.
        _silenceFlushed = true;
        _clarities.Add(1.0);
        FlushCharacter(force: false);
        EmitWordGap(1.0);
    }

    /// <summary>
    /// Process everything held back, once there is enough to process it
    /// against.
    /// </summary>
    /// <param name="force">
    /// Decide on thin evidence because the transmission has ended.
    /// </param>
    private void Drain(bool force = false)
    {
        if (!_speed.IsReady && !force)
        {
            return;
        }

        var nameable = _speed.IsReady || _speed.MarkCount >= MinimumForcedMarks;

        foreach (var element in _pending)
        {
            if (element.IsMark)
            {
                var mark = _speed.ClassifyMark(element.Samples);
                _pattern.Append(mark == CwElement.Dit ? '.' : '-');
                _clarities.Add(nameable ? _speed.Clarity(mark, element.Samples) : 0);
                _worstSnrDb = Math.Min(_worstSnrDb, element.SignalToNoiseDb);

                _contestedDb = Math.Min(_contestedDb, element.ContestedMarginDb);
                _silenceFlushed = false;
                continue;
            }

            var gap = _speed.ClassifyGap(element.Samples);
            var clarity = nameable ? _speed.Clarity(gap, element.Samples) : 0;

            if (gap == CwElement.ElementGap)
            {
                _clarities.Add(clarity);
                continue;
            }

            if (_silenceFlushed)
            {
                // The sender stopped, this character was already released, and
                // the gap has only now ended because they started again.
                _silenceFlushed = false;
                continue;
            }

            // The gap ended the character, and how sure the decoder is about
            // that is part of how sure it is about the character: a silence
            // halfway between one dit and three is the difference between "U"
            // and "IT". How far past three it went is a separate question and
            // belongs to the space, not to the letter.
            _clarities.Add(
                nameable ? _speed.EndOfCharacterClarity(element.Samples) : 0);

            // The force flag has to travel down here too. Without it a short
            // transmission being decided on at the end of its silence would
            // hold every buffered character back and run them together into one
            // impossible pattern.
            FlushCharacter(force);

            if (gap == CwElement.WordGap)
            {
                EmitWordGap(clarity);
            }
        }

        _pending.Clear();
    }

    /// <summary>
    /// Turn the pattern gathered so far into a character and emit it.
    /// </summary>
    private void FlushCharacter(bool force)
    {
        if (_pattern.Length == 0)
        {
            return;
        }

        if (!_speed.IsReady && !force)
        {
            return;
        }

        var pattern = _pattern.ToString();
        var text = MorseAlphabet.Lookup(pattern);
        var timing = WorstClarity();
        var snr = _worstSnrDb == double.MaxValue ? 0 : _worstSnrDb;

        var score = CwConfidenceModel.Score(timing, snr);
        var confidence = CwConfidenceModel.Rate(score, text is not null, _contestedDb);

        _pattern.Clear();
        _clarities.Clear();
        _worstSnrDb = double.MaxValue;
        _contestedDb = double.MaxValue;

        // NOTHING IS CLAIMED WHEN THE TIMINGS DO NOT LOOK LIKE MORSE. Noise
        // makes runs of key-down and key-up too, and a gate will happily chop
        // an empty band into letters if nothing is watching for whether they
        // are actually spaced like Morse. Suppressing the whole character is
        // right rather than marking it unreadable: an unreadable mark says
        // something was heard, and on an empty band nothing was (§0.0).
        if (!_speed.LooksLikeMorse)
        {
            return;
        }

        Emit(new CwCharacter(
            // A character the decoder will not stand behind shows the
            // placeholder and NOT the letter its pattern happened to spell.
            // Showing the letter dimmed would still be putting a guess on
            // screen, and the reader would copy it down.
            confidence == CwConfidence.Unreadable ? MorseAlphabet.Unreadable : text!,
            confidence,
            score,
            pattern,
            snr,
            _speed.WordsPerMinute,
            TimeSpan.FromSeconds((double)_lastSample / SampleRate)));
    }

    /// <summary>
    /// The worst any element of this character managed.
    /// </summary>
    /// <remarks>
    /// The worst rather than the average, because a character is only as
    /// certain as its least certain element. One element on a decision boundary
    /// is enough to make the whole letter a different letter, and averaging it
    /// away would hide exactly the case the reader needs to see.
    /// </remarks>
    private double WorstClarity()
    {
        if (_clarities.Count == 0)
        {
            return 0;
        }

        var worst = 1.0;
        foreach (var c in _clarities)
        {
            worst = Math.Min(worst, c);
        }

        return worst;
    }

    /// <summary>
    /// Emit the space between two words.
    /// </summary>
    /// <param name="clarity">
    /// How clearly the silence was a word gap rather than a long pause between
    /// characters. This is where that ambiguity belongs, since it decides the
    /// spacing and nothing about the letters either side of it.
    /// </param>
    private void EmitWordGap(double clarity)
    {
        if (!_speed.LooksLikeMorse)
        {
            return;
        }

        Emit(new CwCharacter(
            MorseAlphabet.WordGap,
            clarity >= CwConfidenceModel.HighAbove ? CwConfidence.High : CwConfidence.Low,
            clarity,
            string.Empty,
            0,
            _speed.WordsPerMinute,
            TimeSpan.FromSeconds((double)_lastSample / SampleRate)));
    }

    private void Emit(CwCharacter character)
    {
        // COUNTED HERE, WHERE EVERY CHARACTER GOES PAST, so the metrics cannot
        // drift away from what actually reached the screen (HM-DEC-088). Word
        // gaps are spacing rather than copy and are not counted as either.
        if (!character.IsWordGap)
        {
            _charactersEmitted++;

            if (character.IsUnreadable || character.Confidence != CwConfidence.High)
            {
                _charactersUnsure++;
            }

            // A character that resolved consumed the elements behind it, which
            // is what makes the resolved count mean something next to the seen
            // one: the gap between them is what the decoder measured and could
            // not turn into letters.
            _elementsResolved += Math.Max(1, character.Pattern.Length);
        }

        Watch.Observe(character);
        CharacterDecoded?.Invoke(character);
    }

    /// <summary>A measured run, waiting for something to be measured against.</summary>
    /// <param name="IsMark">True when the key was down.</param>
    /// <param name="Samples">How long it lasted.</param>
    /// <param name="SignalToNoiseDb">
    /// How far it stood above the noise and above any rival station, averaged
    /// across the run.
    /// </param>
    /// <param name="ContestedMarginDb">
    /// The closest another station came during this run, before the filter is
    /// credited with rejecting any of it.
    /// </param>
    private readonly record struct PendingElement(
        bool IsMark, double Samples, double SignalToNoiseDb, double ContestedMarginDb);
}

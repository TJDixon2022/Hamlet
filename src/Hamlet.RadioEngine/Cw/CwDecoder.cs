using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Finds a station, feeds it to the decoder, and keeps the record of what
/// arrived.
/// </summary>
/// <remarks>
/// <para>**THIS CLASS USED TO BE A DECODER AND IS NOW A HOST.** The decoding is
/// `CwProbabilisticStream`'s; what is left here is the tone tracker that finds a
/// station, the tap that keeps the audio a capture is written from, the counters
/// a roster row is scored against, and the rule that Hamlet stops listening while
/// the operator is sending (HM-DEC-147).</para>
/// <para>**WHAT WAS REMOVED AND WHY IT MATTERED.** The old decode path — the
/// thresholding gate, the run-length clock fit and the settled second pass — was
/// ruled out on 2026-08-21 and kept running for its counters. It decoded nothing
/// anybody could see, and it went on producing numbers that read as measurements
/// of the reading: a capture sheet said `clockFit dah 15.72 dits`, `decoderWpm
/// rolling 50` and `chars 0 emitted` beside text on screen that a completely
/// different decoder had produced, and a whole work order was written from them.
/// **Two decoders in one tree is two answers to every question and no way to tell
/// which one a sheet is about** (HM-DEC-091).</para>
/// </remarks>
public sealed class CwDecoder
{
    private readonly CwToneTracker _tracker;
    private readonly CwProbabilisticStream _probabilistic;
    private readonly Action<ToneReading> _onReading;

    private IAudioSource? _attached;
    private long _lastSample;
    /// <summary>Where the tracker last moved to a different station.</summary>
    private long _samplesAtDiscontinuity;

    /// <summary>
    /// True once the tracker has moved to a different station at least once.
    /// </summary>
    /// <remarks>
    /// **BEFORE THE FIRST MOVE, NOUGHT IS NOT A MOMENT.** Reading the sample
    /// index alone makes a fresh decoder look as though somebody else had just
    /// started transmitting at sample nought, which held the speed unnamed for
    /// the first twelve seconds of every recording. Not having heard anything yet
    /// and having just lost the station are both states in which no speed may be
    /// named, and they are not the same state (§0.0).
    /// </remarks>
    private bool _hasFollowed;
    private int _lastFollows;

    private double _lastSnrDb = double.NaN;
    private bool _toneLatched;
    private int _charactersEmitted;
    private int _charactersUnsure;
    private int _elementsResolved;

    private bool _transmitting;
    private DateTime _transmitEndedUtc = DateTime.MinValue;
    private long _suspendedChunks;

    private readonly double[] _snrHistory = new double[5];
    private int _snrWrite;
    private int _snrFilled;

    /// <summary>
    /// How fast the held signal-to-noise figure falls away, per measurement.
    /// </summary>
    /// <remarks>
    /// Measurements arrive two hundred times a second, so this decays about ten
    /// decibels in ten seconds: long enough to hold across the gaps inside a
    /// message and short enough that a station going away is noticed.
    /// </remarks>
    private const double SnrDecayDbPerHop = 0.005;

    /// <summary>Creates a decoder.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="expectedToneHz">
    /// The operator's CW pitch, as a place to start looking. The tracker hunts
    /// either side of it, since nobody tunes exactly.
    /// </param>
    public CwDecoder(int sampleRate, double expectedToneHz = 600)
    {
        SampleRate = Math.Max(1_000, sampleRate);
        _tracker = new CwToneTracker(SampleRate, expectedToneHz);
        _onReading = OnReading;
        _probabilistic = new CwProbabilisticStream(SampleRate);

        _probabilistic.CharacterSettled += c =>
        {
            // **THE COUNTERS COUNT WHAT REACHED THE SCREEN** (HM-DEC-091). They
            // used to be incremented on the old path's own emit, which raised
            // nothing anybody could see, so a capture sidecar said `0 characters
            // emitted` about an instant when the terminal was showing text.
            if (!c.IsWordGap)
            {
                _charactersEmitted++;

                if (c.IsUnreadable || c.Confidence != CwConfidence.High)
                {
                    _charactersUnsure++;
                }

                _elementsResolved += Math.Max(1, c.Pattern.Length);
            }

            CharacterSettled?.Invoke(c);
        };

        _probabilistic.LeadingEdgeChanged += e =>
        {
            LeadingEdge?.Invoke(e);

            foreach (var character in e)
            {
                CharacterDecoded?.Invoke(character);
            }
        };
    }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>What the decoder last made of the audio.</summary>
    /// <remarks>
    /// **THE ONE SOURCE FOR EVERY NUMBER ABOUT THE READING** (HM-DEC-091): the
    /// winning speed hypothesis, how much better than silence that reading is,
    /// and the text itself.
    /// </remarks>
    public CwProbabilisticResult Reading => _probabilistic.Last;

    /// <summary>The tone tracker, which is what finds a station.</summary>
    public CwToneTracker Tracker => _tracker;

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
    /// <remarks>
    /// **THE ELEMENT COUNTS ARE THE SAME NUMBER TWICE, DELIBERATELY.** The gate
    /// that counted elements it could not resolve is gone, and this decoder never
    /// commits to an element it does not use: every dit and dah it counts is one
    /// that became part of a character. A pair of figures where the gap between
    /// them used to mean something now says the gap is nought, which is true.
    /// </remarks>
    public CwDecodeReport Report => new(
        Tap.Level,
        _tracker.ToneHz,
        _lastSnrDb,
        HasTone: _toneLatched,
        _elementsResolved,
        _elementsResolved,
        _charactersEmitted,
        _charactersUnsure,
        _tracker.HasKeying,
        _tracker.Verdict.Interference,
        (double)_tracker.Guard.BlockedHops * _tracker.HopSamples / SampleRate);

    /// <summary>Everything inside the decision delay, handed over whole.</summary>
    /// <remarks>
    /// **IT IS REPLACED, NOT APPENDED TO.** The whole point of deciding late is
    /// that a letter can be read differently once the next one arrives, so a
    /// consumer takes this list as the current state of the leading edge rather
    /// than as news.
    /// </remarks>
    public event Action<IReadOnlyList<CwCharacter>>? LeadingEdge;

    /// <summary>The same leading edge, one character at a time.</summary>
    public event Action<CwCharacter>? CharacterDecoded;

    /// <summary>A character that is final and will not be revised.</summary>
    public event Action<CwCharacter>? CharacterSettled;

    /// <summary>
    /// How long decoding stays suspended after the radio stops transmitting.
    /// </summary>
    /// <remarks>
    /// <para>**HALF A SECOND, AND THE EVIDENCE FOR IT IS THE POLL AND NOT THE
    /// KEYING** (HM-DEC-147). Transmit status is a live field, asked for four
    /// times a second, so the state Hamlet holds can be a quarter of a second old
    /// before the reply is parsed. Full break-in switches between elements, which
    /// is tens of milliseconds: **the poll cannot see that at all**.</para>
    /// <para>What the figure is measured against is two poll intervals, so one
    /// dropped reply cannot resume decoding mid-transmission, and the receiver's
    /// own recovery, which <see cref="CwTransmitGuard"/> measured at about
    /// twenty-four milliseconds of transmit-receive hang with a ramp behind
    /// it.</para>
    /// <para>**IT IS ASYMMETRIC ON PURPOSE.** Suspension is immediate, because a
    /// late suspension puts the operator's own sending on the screen as somebody
    /// else's; resumption waits, because an early one does the same with the tail
    /// of it.</para>
    /// </remarks>
    public static TimeSpan ResumeAfter { get; } = TimeSpan.FromMilliseconds(500);

    /// <summary>True while the radio is transmitting or has just stopped.</summary>
    public bool DecodingSuspended { get; private set; }

    /// <summary>How many chunks of audio were dropped rather than decoded.</summary>
    public long SuspendedChunks => _suspendedChunks;

    /// <summary>The slowest speed anybody would call a speed.</summary>
    public const int SlowestPlausibleWpm = 6;

    /// <summary>The fastest the radio's own keyer sends.</summary>
    public const int FastestPlausibleWpm = 48;

    /// <summary>
    /// True while the decoder's window still holds the station before this one.
    /// </summary>
    /// <remarks>
    /// **THE TEST IS WHETHER THE TRACKER HAS MOVED WITHIN A WINDOW'S WORTH OF
    /// AUDIO**, which is exact rather than a settling delay picked by hand. It
    /// used to be counted in marks the old estimator had seen; the tracker knows
    /// the same thing and survives the removal. A surface showing the speed
    /// leaves the field blank while this holds (§0.0).
    /// </remarks>
    public bool SpeedIsReacquiring
        => _hasFollowed
            ? _lastSample - _samplesAtDiscontinuity
              < (long)(CwProbabilisticStream.WindowSeconds * SampleRate)
            : _probabilistic.Last.Text.Length == 0;

    /// <summary>
    /// The sending speed, or null when nothing has earned the right to name one.
    /// </summary>
    /// <remarks>
    /// <para>**ONE GUARDED ANSWER, READ BY EVERY SURFACE** (HM-DEC-090). The
    /// speed reached three separate screens as a settled fact while nothing was
    /// being received, and guarding each of them would have left the fourth.</para>
    /// <para>**AND IT IS NOT NAMED ACROSS A HANDOVER.** The decoder reads a
    /// window several seconds long, so while that window still holds audio from
    /// the station before this one it names a speed between the two, which
    /// describes neither: measured, it named 18 where one station sends 16.</para>
    /// </remarks>
    public int? WordsPerMinute
    {
        get
        {
            var reading = _probabilistic.Last;

            if (reading.Text.Length == 0 || SpeedIsReacquiring)
            {
                return null;
            }

            var wpm = (int)Math.Round(reading.WordsPerMinute);

            return wpm >= SlowestPlausibleWpm && wpm <= FastestPlausibleWpm
                ? wpm
                : null;
        }
    }

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

    /// <summary>Feed samples directly, without a source.</summary>
    /// <param name="chunk">The samples.</param>
    public void Process(in AudioChunk chunk)
    {
        // **THE TAP STILL TAKES IT.** A capture is the raw evidence of what
        // arrived at the sound card, and audio the operator made himself is part
        // of that: a recording that quietly omitted his own sending would be
        // worth less, not more (§0.0.1). What it does not do is reach a decoder.
        Tap.Take(chunk.Samples, chunk.SampleRate);

        if (DecodingSuspended)
        {
            // **NOT DECODED, NOT HELD, NOT RELEASED LATER** (HM-DEC-147). The
            // sidetone of the operator's own transmission is not something
            // anybody sent to him. The tracker is skipped along with everything
            // else, so the survey cannot retune to a sidetone and the pitch that
            // was being read is still there when he stops.
            _suspendedChunks++;

            // **BUT THE AUDIO CLOCK KEEPS RUNNING.** Dropping the samples without
            // letting time pass would stamp every character read afterwards as
            // though the transmission had never happened.
            _probabilistic.Skip(chunk.Samples.Length);
            return;
        }

        _tracker.Process(chunk.Samples, chunk.FirstSampleIndex, _onReading);

        // **AND THE SAME AUDIO GOES TO THE DECODER THAT READS IT** (HM-DEC-091:
        // one source). The tracker has already moved to wherever the station is
        // for this chunk, so the pitch handed over is the current one.
        _probabilistic.ToneHz = _tracker.ToneHz;
        _probabilistic.Process(chunk.Samples);
    }

    /// <summary>
    /// Finish: settle anything still inside the decision delay, because nothing
    /// more is coming to revise it.
    /// </summary>
    public void Flush() => _probabilistic.Flush();

    /// <summary>
    /// Tell the decoder what the radio says about its own transmitter.
    /// </summary>
    /// <param name="transmitting">
    /// True when the radio reports the transmitter keyed, false when it reports
    /// it not, and null when nobody knows.
    /// </param>
    /// <param name="nowUtc">The clock.</param>
    /// <remarks>
    /// <para>**THE RADIO SAYS SO AND THE AUDIO NEVER DOES** (HM-DEC-091,
    /// HM-DEC-147). Not the level, not the sidetone's pitch, not a change in the
    /// noise floor: each of those is a guess about the transmitter made from the
    /// thing the transmitter is drowning out.</para>
    /// <para>**AND NOT KNOWING IS NOT TRANSMITTING.** An unknown state leaves
    /// decoding running, because a decoder silenced by a link that has gone quiet
    /// is a band that reads as empty (§0.0).</para>
    /// </remarks>
    public void RadioIsTransmitting(bool? transmitting, DateTime nowUtc)
    {
        if (transmitting == true)
        {
            _transmitting = true;
            DecodingSuspended = true;
            return;
        }

        if (_transmitting)
        {
            _transmitting = false;
            _transmitEndedUtc = nowUtc;
        }

        DecodingSuspended = _transmitEndedUtc != DateTime.MinValue
            && nowUtc - _transmitEndedUtc < ResumeAfter;
    }

    private void OnSamples(in AudioChunk chunk) => Process(chunk);

    private void OnReading(ToneReading reading)
    {
        _lastSample = reading.SampleIndex;

        // **THE TRACKER MOVED, SO THE WINDOW HOLDS SOMEBODY ELSE** (HM-DEC-095).
        // Nobody tunes exactly, and a signal found two or three hundred hertz
        // from where Hamlet started listening spends its first seconds being
        // measured through a filter pointed at empty band. A refinement within
        // the station being read is not a move in that sense (HM-DEC-123), which
        // is why this counts follows rather than every retune.
        if (_tracker.Follows != _lastFollows)
        {
            _lastFollows = _tracker.Follows;
            _samplesAtDiscontinuity = reading.SampleIndex;
            _hasFollowed = true;
        }

        // **THE OLD DECODER FED THE TRACKER TWO THINGS AND NEITHER HAS AN
        // HONEST SUPPLIER NOW.** Both are recorded here rather than filled with
        // something plausible, because a survey quietly driven by a made-up
        // number is worse than one driven by nothing (§0.0).
        //
        // `MidCharacter` is HM-DEC-096 phase 3's interlock: the tracker may not
        // jump to another part of the band while a character is part-read,
        // because the rest of that character is then assembled from a different
        // station. It was set from the elements the gate had in flight. The
        // working decoder reads a twelve second window retrospectively and has no
        // live notion of a character in progress, and the tracker's own keying
        // verdict takes three seconds to form, which is exactly the stretch where
        // the damage happens. **Measured, not guessed**: with it unset the
        // sensitivity sweep returns 0.81 of the message right and 0.11 wrong at
        // eighteen decibels, where it returned all of it and nothing wrong.
        //
        // `FollowSpeed` chose the survey's analysis window from the fitted speed.
        // Feeding it the working decoder's own speed was built and measured and
        // is not obviously right either: that speed is slower than the old clock
        // fit's, so the window comes out longer, and it cost the tone on a low
        // duty cycle fixture and two retune classifications while fixing a
        // similar number elsewhere. Left uncalled, the survey stays at its
        // acquiring width.
        //
        // What replaces either is a decision about what the display asserts, and
        // those are Tim's without exception (§12.1). Both are in the report.

        // **HOW FAR THE TONE STANDS ABOVE THE BAND WHILE IT IS KEYED**, which is
        // not the same question as how far it stands above it on average, and the
        // difference is why real stations were being missed (HM-DEC-090).
        //
        // A station answering a call keys for a second and a half in thirty
        // seconds. Averaged across all of it, a signal fifty decibels out of the
        // noise reported minus nought point six, because for ninety-six per cent
        // of the time the bin holds nothing but noise.
        //
        // So it is a held peak: up at once, down over about ten seconds. Three
        // measurements have to agree before it counts, which is what stops one
        // burst of static setting it.
        if (!reading.HasNoise)
        {
            return;
        }

        _snrHistory[_snrWrite] = reading.SnrDb;
        _snrWrite = (_snrWrite + 1) % _snrHistory.Length;
        _snrFilled = Math.Min(_snrFilled + 1, _snrHistory.Length);

        if (_snrFilled < _snrHistory.Length)
        {
            return;
        }

        var sustained = Median(_snrHistory);

        _lastSnrDb = double.IsNaN(_lastSnrDb) || sustained > _lastSnrDb
            ? sustained
            : _lastSnrDb - SnrDecayDbPerHop;

        // Opens high and closes low, so a marginal signal is not dropped in the
        // quiet parts of its own message (HM-DEC-090).
        _toneLatched = _lastSnrDb >= (_toneLatched
            ? CwDecodeReport.ToneReleaseDb
            : CwDecodeReport.ToneThresholdDb);
    }

    /// <summary>The middle of five, without allocating.</summary>
    private static double Median(double[] values)
    {
        Span<double> copy = stackalloc double[values.Length];

        values.CopyTo(copy);
        copy.Sort();

        return copy[copy.Length / 2];
    }
}

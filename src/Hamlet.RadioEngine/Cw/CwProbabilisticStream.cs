namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// The probabilistic decoder run over live audio, with a decision delay.
/// </summary>
/// <remarks>
/// <para>**THE REFERENCE RUNS OFFLINE AND THE TERMINAL CANNOT.** What it needs is
/// a sliding window that is re-decoded as audio arrives, and a delay before
/// anything is called final, because deciding late is the entire point of the
/// architecture: a letter read one way can be read another when the next
/// character arrives and the boundaries are all chosen together.</para>
/// <para>**ONE SECOND OF DELAY, WHICH IS BELL'S OWN FIGURE.** Anything already
/// behind it is settled and never changes under the operator; anything inside it
/// is offered as provisional and may be revised. The terminal already draws those
/// two differently, so the difference is visible without inventing a new
/// display.</para>
/// <para>**AND IT IS AFFORDABLE, MEASURED RATHER THAN HOPED.** The whole speed
/// search over twelve hypotheses reads thirty seconds of audio in about a fifth
/// of a second, so a twelve second window re-decoded twice a second costs a few
/// per cent of one core. That was the one piece the reference said nobody had
/// measured.</para>
/// </remarks>
public sealed class CwProbabilisticStream
{
    /// <summary>How much audio the decoder looks back over, in seconds.</summary>
    /// <remarks>
    /// Twelve. Long enough to hold several characters of a slow fist, so the
    /// word-gap hypothesis has something to work with, and short enough that the
    /// noise scale is taken from a stretch of band that has not changed.
    /// </remarks>
    public const double WindowSeconds = 12.0;

    /// <summary>How often the window is read again, in seconds.</summary>
    public const double ReadEverySeconds = 0.5;

    /// <summary>
    /// How far back a character has to be before it stops being revisable.
    /// </summary>
    /// <remarks>
    /// **ONE SECOND, WHICH IS WHAT BELL USED.** Shorter and the last letter of
    /// every character group settles before the gap after it has been seen, which
    /// is exactly the evidence that decides where the character ended. Longer and
    /// the operator watches text sit provisional while he is trying to read it.
    /// </remarks>
    public const double DecisionDelaySeconds = 1.0;

    /// <summary>
    /// How much audio the window has to hold again after being emptied before
    /// anything is read from it.
    /// </summary>
    /// <remarks>
    /// **LESS EVIDENCE HAS TO MEAN SILENCE RATHER THAN GUESSES** (HM-DEC-120).
    /// The per-hop likelihood ratio is an average, and the noise scale and the
    /// signal amplitude behind it are taken from the window's own lower quartile
    /// and upper tail. On two seconds of audio those estimates rest on a handful
    /// of elements and can be badly wrong in either direction, so a short window
    /// does not merely read less: it reads confidently and incorrectly.
    /// </remarks>
    /// <para>**AND IT IS A CONSTANT AGAIN.** It was briefly settable so a sweep
    /// could measure what each length was worth; the answer was nothing at any
    /// length from half a second to twelve, and a mutable static that the whole
    /// suite shares is a way for one test to change another test's numbers
    /// without either of them saying so.</para>
    public const double RefillSeconds = 3.0;

    private readonly int _sampleRate;
    private readonly int _hopSamples;
    private readonly int _windowSamples;
    private readonly int _windowHops;
    private readonly int _readEveryHops;
    private readonly int _delayHops;

    private readonly float[] _mixedI;
    private readonly float[] _mixedQ;
    private readonly double[] _envelope;

    private int _mixWrite;
    private int _mixFilled;
    private int _sampleInHop;
    private int _envelopeCount;
    private long _samplesSeen;
    private long _hopsSeen;
    private int _hopsSinceRead;
    private int _refillHops;
    private double _phase;

    /// <summary>How many characters have been settled since this stream started.</summary>
    private long _settledCount;

    /// <summary>Creates a stream.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    public CwProbabilisticStream(int sampleRate)
    {
        _sampleRate = Math.Max(1_000, sampleRate);
        _hopSamples = Math.Max(
            1, (int)(_sampleRate * CwProbabilisticDecoder.HopMilliseconds / 1000.0));

        _windowSamples = Math.Max(
            1, (int)(_sampleRate / CwProbabilisticDecoder.BandwidthHz));

        _windowHops = Math.Max(64, (int)(WindowSeconds * 1000.0
            / CwProbabilisticDecoder.HopMilliseconds));

        _readEveryHops = Math.Max(1, (int)(ReadEverySeconds * 1000.0
            / CwProbabilisticDecoder.HopMilliseconds));

        _delayHops = Math.Max(1, (int)(DecisionDelaySeconds * 1000.0
            / CwProbabilisticDecoder.HopMilliseconds));

        _mixedI = new float[_windowSamples];
        _mixedQ = new float[_windowSamples];
        _envelope = new double[_windowHops];
    }

    /// <summary>Where the station is, from the tone tracker.</summary>
    /// <remarks>
    /// **THE TRACKER STILL FINDS THE STATION.** Nothing here searches for a pitch;
    /// finding one is the survey's job and it is the one part of the old chain
    /// that works.
    /// </remarks>
    public double ToneHz { get; set; } = 600;

    /// <summary>What the last read made of the window.</summary>
    public CwProbabilisticResult Last { get; private set; } = CwProbabilisticResult.None;

    /// <summary>
    /// True when the last read left the winning path inside a character.
    /// </summary>
    /// <remarks>
    /// **THE INTERLOCK'S QUESTION, ANSWERED BY THE PATH ITSELF** (HM-DEC-096
    /// phase 3). The decoder chooses where every element and every character
    /// begins and ends, over the whole window up to the newest audio it has, so
    /// the last segment of that choice says what the newest audio is inside of.
    /// Nothing is inferred and no threshold is formed.
    /// <para>**IT IS AS OLD AS THE LAST READ AND NO OLDER**, which is half a
    /// second, and the tracker asks the question on the same half second, so the
    /// answer is never more than one survey behind the question. The one second
    /// decision delay does not apply: that governs which characters are settled
    /// enough to emit, not how far the path reaches.</para>
    /// </remarks>
    public bool InsideCharacter => Last.EndsInsideCharacter;

    /// <summary>How many hops have gone by since that answer was worked out.</summary>
    public int HopsSinceAnswer => _hopsSinceRead;

    /// <summary>How many characters have been settled.</summary>
    public long SettledCharacters => _settledCount;

    /// <summary>How many hops of envelope the window is holding.</summary>
    public int EnvelopeHops => _envelopeCount;

    /// <summary>A character that is final and will not be revised.</summary>
    public event Action<CwCharacter>? CharacterSettled;

    /// <summary>
    /// Everything inside the decision delay, offered again on every read.
    /// </summary>
    /// <remarks>
    /// Handed over whole rather than one at a time, because the whole tail can
    /// change when the next character arrives and a consumer needs to replace it
    /// rather than append to it.
    /// </remarks>
    public event Action<IReadOnlyList<CwCharacter>>? LeadingEdgeChanged;

    /// <summary>Feed audio.</summary>
    /// <param name="samples">The samples.</param>
    public void Process(ReadOnlySpan<float> samples)
    {
        var step = 2 * Math.PI * ToneHz / _sampleRate;

        foreach (var sample in samples)
        {
            // Quadrature mixdown, then a boxcar over the arms, which is what a
            // filter of this bandwidth amounts to. The phase is carried rather
            // than recomputed from the sample index so it stays exact over hours.
            _mixedI[_mixWrite] = (float)(sample * Math.Cos(_phase));
            _mixedQ[_mixWrite] = (float)(sample * -Math.Sin(_phase));

            _phase += step;

            if (_phase > 2 * Math.PI)
            {
                _phase -= 2 * Math.PI;
            }

            _mixWrite = (_mixWrite + 1) % _windowSamples;
            _mixFilled = Math.Min(_mixFilled + 1, _windowSamples);
            _samplesSeen++;

            if (++_sampleInHop < _hopSamples)
            {
                continue;
            }

            _sampleInHop = 0;
            PushEnvelope();
        }
    }

    /// <summary>
    /// Let audio time pass without decoding any of it.
    /// </summary>
    /// <param name="samples">How many samples went by.</param>
    /// <remarks>
    /// **THE CLOCK IS THE AUDIO'S AND IT MUST NOT STOP WHEN THE DECODER DOES.**
    /// While the operator is transmitting his own sending is dropped rather than
    /// decoded, and if the hop count stopped with it, every character read
    /// afterwards would be stamped as though the transmission had never
    /// happened. A moment somebody could point at has to be a moment (§0.0.1).
    /// **The envelope is untouched**, so the evidence either side of a
    /// transmission is still there when it ends and the station being read is not
    /// lost to a few seconds of keying.
    /// </remarks>
    public void Skip(int samples)
    {
        _samplesSeen += samples;
        _sampleInHop += samples;

        var hops = _sampleInHop / _hopSamples;

        _sampleInHop -= hops * _hopSamples;
        _hopsSeen += hops;
        _settledThrough += hops;
    }

    /// <summary>
    /// Drop the held audio and start listening afresh at the new pitch.
    /// </summary>
    /// <remarks>
    /// <para>**THE WINDOW HELD TWELVE SECONDS MIXED DOWN AT WHATEVER PITCH THE
    /// TRACKER WAS ON AT THE TIME.** When the tracker follows somebody else, the
    /// earlier hops were taken at the old pitch and the later ones at the new,
    /// and the decode is made over the mixture. Measured on the sensitivity
    /// sweep: one such move at every level, and from eleven decibels down it
    /// costs characters and produces wrong ones — 0.06 of the message wrong at
    /// eleven, 0.19 at three, 0.64 at minus four. A confident wrong character at
    /// the moment somebody answers a call is exactly what HM-DEC-009 exists to
    /// prevent.</para>
    /// <para>**NOTHING ALREADY SETTLED IS RETRACTED.** The audio clock and the
    /// settled mark both keep running, so characters read before the move stand
    /// and are not read again; what goes is the envelope, and the leading edge
    /// with it, because the tip was read through a filter pointed somewhere
    /// else.</para>
    /// <para>The cost is up to twelve seconds of reach every time Hamlet follows
    /// somebody, and it is said on screen rather than hidden.</para>
    /// </remarks>
    public void Restart()
    {
        _envelopeCount = 0;
        _mixWrite = 0;
        _mixFilled = 0;
        _hopsSinceRead = 0;
        _phase = 0;
        _refillHops = Math.Max(
            1, (int)(RefillSeconds * 1000.0 / CwProbabilisticDecoder.HopMilliseconds));

        Last = CwProbabilisticResult.None;

        // The tip belonged to the station that is no longer being read.
        LeadingEdgeChanged?.Invoke(Array.Empty<CwCharacter>());
    }

    /// <summary>Settle everything still inside the delay, because nothing else is coming.</summary>
    public void Flush()
    {
        if (_envelopeCount == 0)
        {
            return;
        }

        Read(settleEverything: true);
    }

    private void PushEnvelope()
    {
        double i = 0;
        double q = 0;

        for (var n = 0; n < _mixFilled; n++)
        {
            i += _mixedI[n];
            q += _mixedQ[n];
        }

        var magnitude = Math.Sqrt((i * i) + (q * q)) / _windowSamples;

        if (_envelopeCount < _windowHops)
        {
            _envelope[_envelopeCount++] = magnitude;
        }
        else
        {
            Array.Copy(_envelope, 1, _envelope, 0, _windowHops - 1);
            _envelope[_windowHops - 1] = magnitude;
        }

        _hopsSeen++;

        if (++_hopsSinceRead < _readEveryHops)
        {
            return;
        }

        _hopsSinceRead = 0;

        // **NOTHING IS READ FROM A WINDOW THAT HAS NOT REFILLED.** Emptying it on
        // a station change is what stops two pitches being decoded as one, and
        // reading the first two seconds back would trade that for a different
        // wrong answer (HM-DEC-120).
        if (_envelopeCount < _refillHops)
        {
            return;
        }

        Read(settleEverything: false);
    }

    private void Read(bool settleEverything)
    {
        var window = new double[_envelopeCount];

        Array.Copy(_envelope, window, _envelopeCount);

        // **THE UNIT IS MEASURED FROM THE WINDOW RATHER THAN SEARCHED FOR.**
        // The speed grid was scored 0.05 apart out of 33 across its whole range
        // on a real capture, so which hypothesis won was decided in the fourth
        // significant figure. The same information is sitting in two medians:
        // any level the envelope is cut at makes a mark read long and the gap
        // beside it read short by the same amount, so the average of the two
        // short clusters is the dit with the bias cancelled. Measured against
        // generated audio of known speed it returns 12.0, 18.0 and 25.0 for
        // true 12, 18 and 25.
        //
        // **THE GRID IS STILL THERE FOR WHEN THE WINDOW HOLDS TOO LITTLE
        // KEYING** to cluster, which is what a window holding noise looks like,
        // and it is what decides in that case.
        var measured = CwUnitEstimator.Measure(
            window, CwProbabilisticDecoder.HopMilliseconds);

        var speed = measured.IsReady
            && measured.WordsPerMinute >= CwProbabilisticDecoder.SlowestWpm
            && measured.WordsPerMinute <= CwProbabilisticDecoder.FastestWpm
                ? measured.WordsPerMinute
                : (double?)null;

        var result = CwProbabilisticDecoder.Decode(window, ToneHz, speed);

        Last = result;

        // Where the window starts on the audio clock, so a character's hop can be
        // turned into a moment somebody can point at (§0.0.1).
        var windowStartHop = _hopsSeen - _envelopeCount;
        var settleBefore = settleEverything
            ? _envelopeCount + 1
            : _envelopeCount - _delayHops;

        var edge = new List<CwCharacter>();

        foreach (var character in result.Characters)
        {
            var absolute = windowStartHop + character.EndHop;
            var at = TimeSpan.FromSeconds(
                absolute * CwProbabilisticDecoder.HopMilliseconds / 1000.0);

            if (character.EndHop < settleBefore)
            {
                // **ALREADY SAID, AND IT DOES NOT MOVE AGAIN.** Only characters
                // this stream has not settled before are announced, so a window
                // re-read twice a second does not repeat itself.
                if (absolute <= _settledThrough)
                {
                    continue;
                }

                _settledThrough = absolute;
                _settledCount++;
                CharacterSettled?.Invoke(Character(character, result, at));
                continue;
            }

            edge.Add(Character(character, result, at));
        }

        LeadingEdgeChanged?.Invoke(edge);
    }

    private long _settledThrough = -1;

    /// <summary>
    /// One character, in the shape every surface in this application already
    /// reads.
    /// </summary>
    /// <remarks>
    /// **THE CONFIDENCE IS THE LIKELIHOOD RATIO AND IT SAYS SO** (HM-DEC-091).
    /// It is not the old decoder's clarity, which measured how far an element sat
    /// from a boundary that had been guessed at; it is how much better this
    /// reading explains the audio than silence does. A character the alphabet
    /// does not know is unreadable and renders as a placeholder rather than a
    /// guessed letter (HM-DEC-048).
    /// </remarks>
    private CwCharacter Character(
        CwProbabilisticCharacter character, CwProbabilisticResult result, TimeSpan at)
    {
        var known = character.Text != "#";

        return new CwCharacter(
            known ? character.Text : MorseAlphabet.Unreadable,
            known ? CwConfidence.High : CwConfidence.Unreadable,
            result.LikelihoodRatio,
            character.Pattern,
            double.NaN,
            (int)Math.Round(result.WordsPerMinute),
            at);
    }
}

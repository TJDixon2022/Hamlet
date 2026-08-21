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
        Read(settleEverything: false);
    }

    private void Read(bool settleEverything)
    {
        var window = new double[_envelopeCount];

        Array.Copy(_envelope, window, _envelopeCount);

        var result = CwProbabilisticDecoder.Decode(window, ToneHz);

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

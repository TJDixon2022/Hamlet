using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Rsid;

/// <summary>One RSID burst heard.</summary>
/// <param name="Code">The mode code, as the file carries it.</param>
/// <param name="Name">fldigi's name for the mode, as the file carries it.</param>
/// <param name="Mode">The mode part of the name.</param>
/// <param name="Variant">The variant part of the name, or empty where the name has none.</param>
/// <param name="CenterHz">Where the burst is centered in the passband.</param>
/// <param name="Quality">How much of the tone energy sat on the announced tones, 0 to 1.</param>
/// <param name="TonesRight">How many of the burst's tones were the strongest of their symbol.</param>
/// <param name="StartSeconds">Where the first tone began, from the start of the audio fed.</param>
/// <remarks>
/// **NO CALLSIGN AND NO TEXT CAN BE IN HERE** (HM-DEC-018). An RSID burst carries a mode code
/// and nothing else, and this record has nowhere to put anything more.
/// </remarks>
public sealed record RsidDetection(
    int Code,
    string Name,
    string Mode,
    string Variant,
    double CenterHz,
    double Quality,
    int TonesRight,
    double StartSeconds);

/// <summary>
/// **Hears RSID bursts anywhere in a passband, and names the mode each one announces.**
/// </summary>
/// <remarks>
/// <para>**HAMLET'S OWN, AND EVERY RSID NUMBER IS THE FILE'S** (`PHASE_PLAN.md` R27, PSK31 plan
/// §R5). The symbol rate, which is also the tone spacing, the burst's length, where tone 0 sits
/// against the center, and every tone sequence come from <see cref="RsidCodes"/>. fldigi's
/// `rsid.cxx` could not be read from the session that wrote this, and nothing was taken from
/// it. A code the file carries without a sequence is not detected, and none is derived.</para>
/// <para>**THE SHAPE, IN THREE SENTENCES.** Every quarter of a symbol, the energy of one
/// symbol's worth of audio is measured on a grid of frequencies half a tone apart across the
/// passband, so each tone of a burst lands on the grid wherever its center is. For every grid
/// point that could be a center, the strongest of the tones around it is noted per frame. A
/// burst is fifteen of those notes, one symbol apart, agreeing with a sequence from the file
/// in all but <see cref="WrongSymbolsAllowed"/> places.</para>
/// <para>**THE CENTER AND THE START ARE REFINED, NOT ROUNDED TO THE GRID.** The energy on the
/// announced tones at the grid points either side, and at the frames either side, is fitted
/// with a parabola, which is what holds the center within a few hertz between grid points.</para>
/// <para>**IT HEARS AND DOES NOTHING ELSE** (§0.2). It keys nothing, tunes nothing, reads no
/// clock and changes no mode; a detection is a value handed back.</para>
/// </remarks>
public sealed class RsidDetector
{
    /// <summary>Frames per symbol: a quarter of a symbol between measurements.</summary>
    /// <remarks>
    /// A burst that starts between two frames is at most an eighth of a symbol from one of
    /// them, which leaves at least seven eighths of each symbol's window on its own tone.
    /// </remarks>
    public const int FramesPerSymbol = 4;

    /// <summary>Grid points per tone step: half a tone between frequencies measured.</summary>
    public const int StepsPerTone = 2;

    /// <summary>How many of a burst's symbols may be wrong and it still be read.</summary>
    /// <remarks>
    /// <para>**FOUR, AND IT IS THIS UNIT'S NUMBER** (work instruction 359 task 2, a mechanism the
    /// arbiter's rules leave to the unit). A symbol right by chance is one tone in the span, so
    /// eleven or more right by chance in one place is odds of well under one in a billion per
    /// place looked, while a burst sixteen decibels below the noise across 2500 Hz still has
    /// several decibels on each tone.</para>
    /// <para>**THE MEASUREMENT IS PRINTED BY THE TESTS**: the tones right on every burst heard.</para>
    /// </remarks>
    public const int WrongSymbolsAllowed = 4;

    private readonly Burst[] _bursts;
    private readonly int _sampleRate;
    private readonly int _symbols;
    private readonly int _toneCount;
    private readonly int _firstStep;
    private readonly int _window;
    private readonly double _hop;
    private readonly double _lowestHz;
    private readonly double _stepHz;
    private readonly double[] _coefficients;
    private readonly int _centerLow;
    private readonly int _centerCount;
    private readonly int _span;
    private readonly int _ring;
    private readonly double[][] _energy;
    private readonly byte[][] _loudest;
    private readonly double[][] _total;
    private readonly List<Candidate> _pending = new();

    private float[] _buffer = new float[1 << 16];
    private long _bufferStart;
    private int _bufferCount;
    private long _frames;
    private long _nextStart;

    /// <summary>A detector for one stream of audio.</summary>
    /// <param name="codes">The codes and tone sequences.</param>
    /// <param name="sampleRate">The stream's samples a second.</param>
    /// <param name="lowestHz">The bottom of the passband searched.</param>
    /// <param name="highestHz">The top of the passband searched.</param>
    /// <exception cref="ArgumentNullException">There are no codes.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The rate or the passband is unusable.</exception>
    public RsidDetector(RsidCodes codes, int sampleRate, double lowestHz, double highestHz)
    {
        ArgumentNullException.ThrowIfNull(codes);

        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        }

        highestHz = Math.Min(highestHz, sampleRate / 2.0);

        if (lowestHz < 0 || highestHz <= lowestHz)
        {
            throw new ArgumentOutOfRangeException(nameof(lowestHz));
        }

        _sampleRate = sampleRate;
        _symbols = codes.Symbols;
        _lowestHz = lowestHz;

        _bursts = codes.ToneSequences
            .Where(s => codes.Codes.ContainsKey(s.Key) && s.Value.Count == codes.Symbols)
            .Select(s => Burst.Named(s.Key, codes.Codes[s.Key], s.Value))
            .ToArray();

        // **THE TONE SPAN IS THE SEQUENCES' OWN**: the file states where tone 0 is and each
        // sequence states which tones it uses, so the span is what they reach.
        _toneCount = _bursts.Length == 0 ? 1 : _bursts.Max(b => b.Tones.Max()) + 1;
        _firstStep = codes.FirstToneOffsetSymbols * StepsPerTone;

        var perSymbol = sampleRate / codes.SymbolRateHz;

        _window = (int)Math.Round(perSymbol);
        _hop = perSymbol / FramesPerSymbol;
        _stepHz = codes.SymbolRateHz / StepsPerTone;

        var gridCount = (int)Math.Floor((highestHz - lowestHz) / _stepHz) + 1;

        _coefficients = new double[gridCount];

        for (var m = 0; m < gridCount; m++)
        {
            _coefficients[m] = 2 * Math.Cos(2 * Math.PI * (lowestHz + (m * _stepHz)) / sampleRate);
        }

        // **ONE GRID POINT OF ROOM EITHER SIDE**, so the refinement can look at its neighbors.
        var lowestStep = _firstStep;
        var highestStep = _firstStep + ((_toneCount - 1) * StepsPerTone);

        _centerLow = 1 - lowestStep;
        _centerCount = Math.Max(0, gridCount - 1 - highestStep - 1 - _centerLow + 1);

        _span = FramesPerSymbol * (_symbols - 1);
        _ring = _span + 3;

        _energy = new double[_ring][];
        _loudest = new byte[_ring][];
        _total = new double[_ring][];

        for (var i = 0; i < _ring; i++)
        {
            _energy[i] = new double[gridCount];
            _loudest[i] = new byte[_centerCount];
            _total[i] = new double[_centerCount];
        }
    }

    /// <summary>The stream's samples a second.</summary>
    public int SampleRate => _sampleRate;

    /// <summary>Every burst in a whole recording.</summary>
    /// <param name="codes">The codes and tone sequences.</param>
    /// <param name="audio">The recording.</param>
    /// <param name="lowestHz">The bottom of the passband searched.</param>
    /// <param name="highestHz">The top of the passband searched.</param>
    /// <returns>The bursts, in the order they were heard.</returns>
    public static IReadOnlyList<RsidDetection> Detect(
        RsidCodes codes, MonoAudio audio, double lowestHz, double highestHz)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var detector = new RsidDetector(codes, audio.SampleRate, lowestHz, highestHz);
        var heard = new List<RsidDetection>(detector.Feed(audio.Samples));

        heard.AddRange(detector.Flush());

        return heard;
    }

    /// <summary>Hand the detector the next piece of the stream.</summary>
    /// <param name="samples">The samples, at <see cref="SampleRate"/>.</param>
    /// <returns>The bursts this piece completed, which is usually none.</returns>
    /// <remarks>
    /// A burst is handed back once a burst's length of audio after it has been heard, because
    /// until then a better reading of the same burst could still arrive.
    /// </remarks>
    public IReadOnlyList<RsidDetection> Feed(ReadOnlySpan<float> samples)
    {
        Append(samples);

        var heard = new List<RsidDetection>();

        while (true)
        {
            var start = (long)Math.Round(_frames * _hop);

            if (start + _window > _bufferStart + _bufferCount)
            {
                break;
            }

            Measure(_frames, (int)(start - _bufferStart));
            _frames++;

            while (_nextStart + _span + 1 < _frames)
            {
                Look(_nextStart, heard);
                _nextStart++;
            }
        }

        return heard;
    }

    /// <summary>The stream has ended: hand back whatever is still being held.</summary>
    /// <returns>The bursts not yet handed back.</returns>
    public IReadOnlyList<RsidDetection> Flush()
    {
        // **A BURST AT THE VERY END STILL NEEDS ITS LAST WINDOW**, so the stream is closed with
        // silence rather than by leaving the last tone unmeasured.
        var heard = new List<RsidDetection>(Feed(new float[_window + (int)Math.Ceiling(2 * _hop)]));

        heard.AddRange(_pending.OrderBy(p => p.Start).Select(Named));
        _pending.Clear();

        return heard;
    }

    private void Append(ReadOnlySpan<float> samples)
    {
        var keepFrom = (long)Math.Round(_frames * _hop);
        var drop = (int)Math.Clamp(keepFrom - _bufferStart, 0, _bufferCount);

        if (drop > 0)
        {
            Array.Copy(_buffer, drop, _buffer, 0, _bufferCount - drop);
            _bufferCount -= drop;
            _bufferStart += drop;
        }

        if (_bufferCount + samples.Length > _buffer.Length)
        {
            Array.Resize(ref _buffer, Math.Max(_buffer.Length * 2, _bufferCount + samples.Length));
        }

        samples.CopyTo(_buffer.AsSpan(_bufferCount));
        _bufferCount += samples.Length;
    }

    private void Measure(long frame, int offset)
    {
        var slot = (int)(frame % _ring);
        var energy = _energy[slot];

        for (var m = 0; m < energy.Length; m++)
        {
            var coefficient = _coefficients[m];
            double s1 = 0;
            double s2 = 0;

            for (var i = offset; i < offset + _window; i++)
            {
                var s0 = _buffer[i] + (coefficient * s1) - s2;
                s2 = s1;
                s1 = s0;
            }

            energy[m] = (s1 * s1) + (s2 * s2) - (coefficient * s1 * s2);
        }

        var loudest = _loudest[slot];
        var total = _total[slot];

        for (var ci = 0; ci < _centerCount; ci++)
        {
            var at = _centerLow + ci + _firstStep;
            var best = 0;
            var bestEnergy = -1.0;
            var sum = 0.0;

            for (var k = 0; k < _toneCount; k++)
            {
                var e = energy[at + (k * StepsPerTone)];

                sum += e;

                if (e > bestEnergy)
                {
                    bestEnergy = e;
                    best = k;
                }
            }

            loudest[ci] = (byte)best;
            total[ci] = sum;
        }
    }

    /// <summary>Is there a burst whose first tone starts at this frame?</summary>
    private void Look(long start, List<RsidDetection> heard)
    {
        for (var ci = 0; ci < _centerCount; ci++)
        {
            foreach (var burst in _bursts)
            {
                var wrong = 0;

                for (var i = 0; i < _symbols && wrong <= WrongSymbolsAllowed; i++)
                {
                    if (_loudest[Slot(start + (i * FramesPerSymbol))][ci] != burst.Tones[i])
                    {
                        wrong++;
                    }
                }

                if (wrong <= WrongSymbolsAllowed)
                {
                    Consider(Read(start, ci, burst, wrong));
                }
            }
        }

        // **A HELD BURST IS HANDED BACK ONCE NOTHING LATER COULD STILL BE THE SAME BURST.**
        for (var i = _pending.Count - 1; i >= 0; i--)
        {
            if (_pending[i].Frame + (FramesPerSymbol * _symbols) <= start)
            {
                heard.Add(Named(_pending[i]));
                _pending.RemoveAt(i);
            }
        }
    }

    private Candidate Read(long start, int ci, Burst burst, int wrong)
    {
        var quality = 0.0;

        for (var i = 0; i < _symbols; i++)
        {
            var slot = Slot(start + (i * FramesPerSymbol));
            var total = _total[slot][ci];

            quality += total > 0
                ? _energy[slot][_centerLow + ci + _firstStep + (burst.Tones[i] * StepsPerTone)] / total
                : 0;
        }

        var center = _centerLow + ci;
        var across = Peak(
            Strength(start, center - 1, burst), Strength(start, center, burst), Strength(start, center + 1, burst));

        var along = start == 0
            ? 0
            : Peak(Strength(start - 1, center, burst), Strength(start, center, burst), Strength(start + 1, center, burst));

        return new Candidate(
            start,
            ci,
            burst,
            wrong,
            quality / _symbols,
            _lowestHz + ((center + across) * _stepHz),
            (start + along) * _hop / _sampleRate);
    }

    private double Strength(long start, int center, Burst burst)
    {
        var sum = 0.0;

        for (var i = 0; i < _symbols; i++)
        {
            sum += _energy[Slot(start + (i * FramesPerSymbol))][center + _firstStep + (burst.Tones[i] * StepsPerTone)];
        }

        return sum;
    }

    /// <summary>Where the top of a parabola through three even points lies, from -0.5 to 0.5.</summary>
    private static double Peak(double left, double middle, double right)
    {
        var bend = left - (2 * middle) + right;

        return bend >= 0 ? 0 : Math.Clamp(0.5 * (left - right) / bend, -0.5, 0.5);
    }

    private void Consider(Candidate found)
    {
        var width = _toneCount * StepsPerTone;
        var length = FramesPerSymbol * _symbols;
        var beaten = new List<Candidate>();

        foreach (var held in _pending)
        {
            if (Math.Abs(held.Frame - found.Frame) >= length || Math.Abs(held.CenterIndex - found.CenterIndex) >= width)
            {
                continue;
            }

            if (!found.IsBetterThan(held))
            {
                return;
            }

            beaten.Add(held);
        }

        foreach (var held in beaten)
        {
            _pending.Remove(held);
        }

        _pending.Add(found);
    }

    private int Slot(long frame) => (int)(frame % _ring);

    private RsidDetection Named(Candidate c)
        => new(
            c.Burst.Code,
            c.Burst.Name,
            c.Burst.Mode,
            c.Burst.Variant,
            c.CenterHz,
            c.Quality,
            _symbols - c.Wrong,
            c.Start);

    private sealed record Burst(string Name, int Code, string Mode, string Variant, int[] Tones)
    {
        /// <summary>
        /// **THE MODE AND VARIANT ARE READ OFF FLDIGI'S NAME**: the part before the first
        /// underscore is the mode, and the rest, joined with a slash, is the variant -
        /// `OLIVIA_8_250` is Olivia 8/250, and `BPSK31` has no variant.
        /// </summary>
        public static Burst Named(string name, int code, IReadOnlyList<int> tones)
            => new(name, code, RsidCodes.ModeOf(name), RsidCodes.VariantOf(name), tones.ToArray());
    }

    private sealed record Candidate(
        long Frame, int CenterIndex, Burst Burst, int Wrong, double Quality, double CenterHz, double Start)
    {
        public bool IsBetterThan(Candidate other)
            => Wrong < other.Wrong || (Wrong == other.Wrong && Quality > other.Quality);
    }
}

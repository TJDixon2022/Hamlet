namespace Hamlet.RadioEngine.Training;

/// <summary>
/// Paints a spectrum sweep: a noise floor with slow weather in it, plus each
/// signal drawn with its own mode's behavior.
/// </summary>
/// <remarks>
/// <para>Deterministic, and strictly so. Every varying quantity — the noise,
/// the fades, the interference — is a pure function of the seed and the
/// elapsed time handed in. Nothing here reads a clock, so the same seed and
/// the same elapsed time always paint the same frame, a test can assert on
/// exact bytes, and a practice session can be replayed (§5).</para>
/// <para>Allocation-free once constructed: <see cref="Render"/> writes into a
/// span the caller owns and allocates nothing per frame, because this runs
/// twenty-five times a second forever (HM-DEC-006).</para>
/// </remarks>
public sealed class SignalSynthesizer
{
    /// <summary>FT8's transmission cycle: every quarter-minute, worldwide.</summary>
    public static readonly TimeSpan Ft8Period = TimeSpan.FromSeconds(15);

    /// <summary>How much of an FT8 cycle carries signal.</summary>
    public static readonly TimeSpan Ft8Transmission = TimeSpan.FromSeconds(12.64);

    /// <summary>RTTY's classic 45.45 baud.</summary>
    public const double RttyBaud = 45.45;

    /// <summary>RTTY's mark/space separation.</summary>
    public const int RttyShiftHz = 170;

    /// <summary>Amplitude of the noise floor, before weather.</summary>
    private const double NoiseFloor = 0.10;

    private readonly IReadOnlyList<SyntheticSignal> _signals;
    private readonly long _lowHz;
    private readonly long _highHz;
    private readonly int _seed;
    private readonly IReadOnlyList<int> _cwPatternDits;
    private readonly Dictionary<int, (IReadOnlyList<int> Pattern, int Total)> _cwPatterns = new();

    /// <summary>Creates a synthesiser for a fixed band and signal set.</summary>
    /// <param name="signals">Signals to paint.</param>
    /// <param name="lowHz">Lower edge of the span.</param>
    /// <param name="highHz">Upper edge of the span.</param>
    /// <param name="seed">Seed for every pseudo-random quantity.</param>
    public SignalSynthesizer(
        IReadOnlyList<SyntheticSignal> signals, long lowHz, long highHz, int seed)
    {
        _signals = signals;
        _lowHz = lowHz;
        _highHz = Math.Max(lowHz + 1, highHz);
        _seed = seed;
        _cwPatternDits = Array.Empty<int>();

        for (var i = 0; i < signals.Count; i++)
        {
            if (signals[i].Mode != TrainingMode.Cw)
            {
                continue;
            }

            var pattern = MorseCode.KeyPattern(signals[i].Text);
            var total = 0;
            foreach (var d in pattern)
            {
                total += d;
            }

            _cwPatterns[i] = (pattern, total);
        }
    }

    /// <summary>The signals being painted.</summary>
    public IReadOnlyList<SyntheticSignal> Signals => _signals;

    /// <summary>Lower edge of the span in hertz.</summary>
    public long LowHz => _lowHz;

    /// <summary>Upper edge of the span in hertz.</summary>
    public long HighHz => _highHz;

    /// <summary>
    /// Paint one sweep.
    /// </summary>
    /// <param name="elapsed">Time since the session began. Passed in, never
    /// read from a clock, so the output is reproducible (§5).</param>
    /// <param name="bins">Destination; one byte per bin, filled completely.</param>
    public void Render(TimeSpan elapsed, Span<byte> bins)
    {
        if (bins.Length == 0)
        {
            return;
        }

        var seconds = elapsed.TotalSeconds;
        var binWidthHz = (double)(_highHz - _lowHz) / bins.Length;

        PaintNoise(seconds, bins, binWidthHz);

        for (var i = 0; i < _signals.Count; i++)
        {
            PaintSignal(i, _signals[i], seconds, bins, binWidthHz);
        }
    }

    /// <summary>
    /// The bin an absolute frequency falls in, or -1 when off-span.
    /// </summary>
    /// <param name="hz">Frequency in hertz.</param>
    /// <param name="binCount">How many bins the sweep has.</param>
    /// <returns>Bin index, or -1.</returns>
    public int BinFor(long hz, int binCount)
    {
        if (binCount <= 0 || hz < _lowHz || hz > _highHz)
        {
            return -1;
        }

        var index = (int)((hz - _lowHz) / (double)(_highHz - _lowHz) * binCount);
        return Math.Clamp(index, 0, binCount - 1);
    }

    /// <summary>
    /// A noise floor that breathes, plus the occasional burst of QRM.
    /// </summary>
    /// <remarks>
    /// A flat floor reads as a screensaver. Real receiver noise wanders with
    /// the band, so the floor here carries two slow waves and a per-bin
    /// hash-based speckle, and every few tens of seconds a wide, brief lump
    /// of interference crosses part of the span.
    /// </remarks>
    private void PaintNoise(double seconds, Span<byte> bins, double binWidthHz)
    {
        // Two slow waves, different periods, so the floor never repeats
        // visibly on a short watch.
        var weather = 1.0
            + (0.22 * Math.Sin(seconds * 2 * Math.PI / 47.0))
            + (0.13 * Math.Sin((seconds * 2 * Math.PI / 13.7) + 1.1));

        // A wandering slope across the band, as a real receiver shows.
        var tilt = 0.10 * Math.Sin((seconds * 2 * Math.PI / 71.0) + 0.4);

        // QRM: a broad lump that appears, crosses, and goes.
        var qrmCycle = 37.0;
        var qrmPhase = (seconds % qrmCycle) / qrmCycle;
        var qrmActive = qrmPhase < 0.18;
        var qrmCenter = 0.0;
        var qrmStrength = 0.0;
        if (qrmActive)
        {
            var burst = (int)(seconds / qrmCycle);
            qrmCenter = Hash01(_seed, burst, 991) * bins.Length;
            qrmStrength = 0.22 * Math.Sin(qrmPhase / 0.18 * Math.PI);
        }

        for (var i = 0; i < bins.Length; i++)
        {
            var across = (double)i / bins.Length;

            // Deterministic speckle: a hash of the bin and a coarse time
            // slice, so it shimmers without a random number generator whose
            // state would make replay impossible.
            var slice = (int)(seconds * 8);
            var speckle = Hash01(_seed, i, slice) * 0.055;

            var level = (NoiseFloor * weather) + speckle + (tilt * (across - 0.5));

            if (qrmActive)
            {
                var d = (i - qrmCenter) / (bins.Length * 0.06);
                level += qrmStrength * Math.Exp(-d * d);
            }

            bins[i] = ToByte(level);
        }
    }

    private void PaintSignal(
        int index, SyntheticSignal signal, double seconds, Span<byte> bins, double binWidthHz)
    {
        var envelope = Envelope(index, signal, seconds);
        if (envelope <= 0.001)
        {
            return;
        }

        // QSB: a slow fade, so a signal is not a permanent fixture.
        var fade = 1.0;
        if (signal.FadePeriod > TimeSpan.Zero)
        {
            var p = signal.FadePeriod.TotalSeconds;
            fade = 0.62 + (0.38 * Math.Sin((seconds * 2 * Math.PI / p)
                + (signal.PhaseOffset * 2 * Math.PI)));
        }

        var amplitude = signal.Strength * envelope * fade;
        if (amplitude <= 0.001)
        {
            return;
        }

        switch (signal.Mode)
        {
            case TrainingMode.Rtty:
                // Two rails: mark and space, only one lit at a time.
                var markUp = RttyMarkUp(index, signal, seconds);
                PaintLobe(bins, binWidthHz,
                    signal.CenterHz + (RttyShiftHz / 2), 45, markUp ? amplitude : amplitude * 0.18);
                PaintLobe(bins, binWidthHz,
                    signal.CenterHz - (RttyShiftHz / 2), 45, markUp ? amplitude * 0.18 : amplitude);
                break;

            case TrainingMode.Ssb:
                PaintSsb(signal, seconds, bins, binWidthHz, amplitude);
                break;

            default:
                PaintLobe(bins, binWidthHz, signal.CenterHz, signal.WidthHz, amplitude);
                break;
        }
    }

    /// <summary>
    /// How hard the transmitter is running right now, 0 to 1 — the part that
    /// gives each mode its rhythm.
    /// </summary>
    private double Envelope(int index, SyntheticSignal signal, double seconds)
    {
        switch (signal.Mode)
        {
            case TrainingMode.Cw:
            {
                if (!_cwPatterns.TryGetValue(index, out var keyed))
                {
                    return 0;
                }

                var ditSeconds = MorseCode.Dit(signal.WordsPerMinute).TotalSeconds;
                var offset = signal.PhaseOffset * keyed.Total;
                var dits = (seconds / ditSeconds) + offset;

                // Seven dits of silence between repeats: a real operator
                // pauses before calling again.
                return MorseCode.IsKeyDown(keyed.Pattern, keyed.Total, 14, dits) ? 1.0 : 0.0;
            }

            case TrainingMode.Ft8:
            {
                // Aligned to the UTC quarter-minute, which is what makes the
                // waterfall look like rain: everybody starts together.
                var period = Ft8Period.TotalSeconds;
                var within = seconds % period;
                if (within < 0)
                {
                    within += period;
                }

                if (within >= Ft8Transmission.TotalSeconds)
                {
                    return 0.0;
                }

                // Not every station transmits in every slot.
                var slot = (int)(seconds / period);
                return Hash01(_seed, index * 31, slot) < 0.72 ? 1.0 : 0.0;
            }

            case TrainingMode.Psk31:
            {
                // Near-continuous while transmitting, with idle gaps between
                // overs.
                var cycle = 42.0;
                var within = (seconds + (signal.PhaseOffset * cycle)) % cycle;
                return within < 30.0 ? 0.95 : 0.0;
            }

            case TrainingMode.Rtty:
            {
                var cycle = 26.0;
                var within = (seconds + (signal.PhaseOffset * cycle)) % cycle;
                return within < 17.0 ? 1.0 : 0.0;
            }

            case TrainingMode.Ssb:
            {
                // Speech: syllables inside phrases inside overs.
                var cycle = 34.0;
                var within = (seconds + (signal.PhaseOffset * cycle)) % cycle;
                if (within > 21.0)
                {
                    return 0.0;
                }

                var syllable = 0.5 + (0.5 * Math.Sin(seconds * 2 * Math.PI * 3.1));
                var phrase = 0.55 + (0.45 * Math.Sin((seconds * 2 * Math.PI / 2.7) + 0.7));
                var breath = Math.Sin(within / 21.0 * Math.PI);
                return Math.Clamp(syllable * phrase * breath * 1.6, 0, 1);
            }

            default:
                return 1.0;
        }
    }

    private bool RttyMarkUp(int index, SyntheticSignal signal, double seconds)
    {
        var bit = (long)(seconds * RttyBaud);
        return Hash01(_seed, (index * 17) + 3, (int)(bit & 0xFFFF)) < 0.5;
    }

    /// <summary>
    /// SSB: a wide, ragged smear rather than a clean lobe, because that is
    /// what makes it identifiable at a glance.
    /// </summary>
    private void PaintSsb(
        SyntheticSignal signal, double seconds, Span<byte> bins,
        double binWidthHz, double amplitude)
    {
        var lowBin = (int)((signal.LowHz - _lowHz) / binWidthHz);
        var highBin = (int)((signal.HighHz - _lowHz) / binWidthHz);

        for (var i = Math.Max(0, lowBin); i <= Math.Min(bins.Length - 1, highBin); i++)
        {
            var across = (i - lowBin) / Math.Max(1.0, highBin - lowBin);

            // Voice energy is bottom-heavy, and rough across the band.
            var shape = Math.Sin(across * Math.PI);
            var formant = 0.55
                + (0.45 * Math.Sin((across * 9.0) + (seconds * 5.0)))
                + (0.25 * Math.Sin((across * 23.0) - (seconds * 3.0)));

            var level = amplitude * shape * Math.Clamp(formant, 0, 1.4) * 0.85;
            Accumulate(bins, i, level);
        }
    }

    /// <summary>A signal's energy across its own bandwidth.</summary>
    private void PaintLobe(
        Span<byte> bins, double binWidthHz, long centerHz, int widthHz, double amplitude)
    {
        var halfWidth = Math.Max(binWidthHz * 0.6, widthHz / 2.0);
        var lowBin = (int)((centerHz - halfWidth - _lowHz) / binWidthHz);
        var highBin = (int)((centerHz + halfWidth - _lowHz) / binWidthHz) + 1;

        for (var i = Math.Max(0, lowBin); i <= Math.Min(bins.Length - 1, highBin); i++)
        {
            var binCenterHz = _lowHz + ((i + 0.5) * binWidthHz);
            var offset = (binCenterHz - centerHz) / halfWidth;

            // A raised cosine out to the edge, then nothing: narrow modes
            // must actually look narrow.
            if (Math.Abs(offset) > 1.0)
            {
                continue;
            }

            var level = amplitude * 0.5 * (1 + Math.Cos(offset * Math.PI));
            Accumulate(bins, i, level);
        }
    }

    private static void Accumulate(Span<byte> bins, int index, double level)
    {
        var current = bins[index] / 255.0;
        bins[index] = ToByte(current + level);
    }

    private static byte ToByte(double level)
        => (byte)Math.Clamp(level * 255.0, 0, 255);

    /// <summary>
    /// A stable hash in [0,1) from three integers.
    /// </summary>
    /// <remarks>
    /// Used everywhere a random number would be. A generator carries state,
    /// and state means the frame you get depends on how many frames you asked
    /// for first — which would defeat replay and make the exact-bytes test
    /// impossible to write.
    /// </remarks>
    private static double Hash01(int seed, int a, int b)
    {
        unchecked
        {
            var h = (uint)seed;
            h = (h ^ (uint)a) * 2654435761u;
            h ^= h >> 15;
            h = (h ^ (uint)b) * 2246822519u;
            h ^= h >> 13;
            h *= 3266489917u;
            h ^= h >> 16;
            return h / 4294967296.0;
        }
    }
}

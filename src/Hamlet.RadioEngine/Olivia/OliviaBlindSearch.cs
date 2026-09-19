using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.RadioEngine.Olivia;

/// <summary>One variant tried by decoding the audio the search had consumed.</summary>
/// <param name="Variant">The row tried.</param>
/// <param name="SyncSnr">The mean signal-to-noise of the blocks that cleared the threshold, or 0 where none did.</param>
/// <param name="BlocksDecoded">Blocks that cleared the threshold.</param>
/// <param name="BlocksRejected">Blocks that did not.</param>
/// <param name="HighestBlockSnr">The best block's signal-to-noise, cleared or not.</param>
public sealed record OliviaTrial(string Variant, double SyncSnr, int BlocksDecoded, int BlocksRejected, double HighestBlockSnr);

/// <summary>An Olivia carrier the blind search found, and what it found it with.</summary>
/// <param name="Variant">The variant it named.</param>
/// <param name="CenterHz">The center it named, the trial decode's refinement of the measured one.</param>
/// <param name="Confidence">The chosen trial's sync signal-to-noise, in the demodulator's units.</param>
/// <param name="AudioSeconds">Seconds of audio from the start that the search had consumed when it named the carrier.</param>
/// <param name="MeasuredCenterHz">The middle of the tones, measured off the spectrum.</param>
/// <param name="ToneSpacingHz">The tone spacing measured off the spectrum, or NaN where fewer than two tones stood out.</param>
/// <param name="TonesCounted">How many tones stood out of the loudest-frequency histogram.</param>
/// <param name="OccupiedLowHz">The low edge of the band 20 dB down from its peak.</param>
/// <param name="OccupiedHighHz">The high edge of the same band.</param>
/// <param name="Trials">Every row weighed, in the order it was ranked, with its trial decode.</param>
public sealed record OliviaCandidate(
    OliviaVariant Variant,
    double CenterHz,
    double Confidence,
    double AudioSeconds,
    double MeasuredCenterHz,
    double ToneSpacingHz,
    int TonesCounted,
    double OccupiedLowHz,
    double OccupiedHighHz,
    IReadOnlyList<OliviaTrial> Trials);

/// <summary>What one blind search found, and how much audio it looked at.</summary>
/// <param name="Candidates">Every carrier named, strongest first; empty where nothing was.</param>
/// <param name="AudioSeconds">Seconds of audio from the start that the search consumed.</param>
/// <param name="FloorDb">The passband's median power in its last look, in dB of the transform's own units.</param>
/// <param name="PeakOverFloorDb">How far the loudest bin in the passband stood over that median, in dB.</param>
public sealed record OliviaSearch(IReadOnlyList<OliviaCandidate> Candidates, double AudioSeconds, double FloorDb, double PeakOverFloorDb);

/// <summary>
/// **Finds an Olivia carrier that never announced itself, and names its variant and center from
/// the audio alone.**
/// </summary>
/// <remarks>
/// <para>**NEVER TOLD THE ANSWER** (work instruction 362 decision O, `PHASE_PLAN.md` R27). It is
/// handed audio and a passband and nothing else - no variant, no center, no start time - and it
/// can name only a row of <see cref="OliviaFormat.Variants"/>. **No tone count, spacing, symbol
/// length or bandwidth is a literal here**: the resolutions it looks with are derived from the
/// rows, and the rows are the file's.</para>
/// <para>**THE SHAPE, MEASURED THEN CONFIRMED** (decision P). The audio is taken from the start a
/// block of the slowest row at a time, two to begin with. Over what has been taken: a long-window
/// average spectrum finds every region of the passband standing clear of its median, and the
/// band 20 dB down from each region's peak is the occupied band. Frame by frame, over windows two
/// of the longest symbols long, the loudest frequency in that band is piled into a histogram whose
/// peaks are the tones: their count, their mean spacing and their middle. The rows within half an
/// octave of the measured band or of the measured spacing are weighed, those that fit both first,
/// and each is tried with <see cref="OliviaDemodulator"/> at the measured middle. The row whose
/// blocks stand furthest out of the noise on the measure <see cref="OliviaDemodulator.SyncThreshold"/>
/// uses is named; **a region where no row clears that threshold is not a detection**, and more
/// audio is taken until one does or the audio ends.</para>
/// <para>**TWO GATES AGAINST NOISE** (decision R). A region must stand <see cref="GateSigmas"/>
/// standard deviations of the averaged noise over the passband's median and be at least half the
/// narrowest row's bandwidth wide before any row is tried; and a tried row must then clear the
/// demodulator's own threshold.</para>
/// <para>**IT HEARS AND DOES NOTHING ELSE** (§0.2): it keys nothing, tunes nothing and sets no
/// mode. What it found is a value handed back, and it is wired to nothing in the app (decision
/// L).</para>
/// </remarks>
public sealed class OliviaBlindSearch
{
    /// <summary>
    /// How many standard deviations of the averaged noise a region must stand over the passband's
    /// median to be looked at.
    /// </summary>
    /// <remarks>
    /// **THE UNIT'S NUMBER** (work instruction 362 task 3, a mechanism the arbiter leaves to the
    /// unit). A bin of an average of K power spectra of noise has a spread of about its mean over
    /// the root of K. Over thirty seconds of the noise-only fixture the loudest bin stood 0.86 dB
    /// over the median, where this gate stands at about 2 dB.
    /// </remarks>
    public const double GateSigmas = 8.0;

    /// <summary>How many blocks of a trial decode must clear the demodulator's threshold before a row is named.</summary>
    /// <remarks>
    /// **THE UNIT'S NUMBER, AND WHY IT IS NOT ONE** (work instruction 362 tasks 1 and 3). Read as
    /// 16/500, the 8/250 no-RSID file put one block through at 4.10 against the threshold's 4.0, and
    /// the clean 16/500 file, searched blind, was first named on one block at 4.15: one block barely
    /// through is what a wrong row can do. Two are asked for, and more audio is taken until they
    /// come.
    /// </remarks>
    public const int ConfirmBlocks = 2;

    /// <summary>How far below a region's peak its occupied band ends, as a power ratio: 20 dB.</summary>
    public const double OccupiedRatio = 100.0;

    /// <summary>How far, in octaves, a row's bandwidth or spacing may sit from the measured one and still be weighed.</summary>
    /// <remarks>The rows' bandwidths and spacings are powers of two apart, so half an octave admits the nearest and no other.</remarks>
    public const double FitOctaves = 0.5;

    private readonly OliviaFormat _format;
    private readonly ITelemetry? _telemetry;

    /// <summary>A search over the rows of one format.</summary>
    /// <param name="format">The format's facts, whose rows are the only variants it can name.</param>
    /// <param name="telemetry">Where the search's event is written, or null.</param>
    public OliviaBlindSearch(OliviaFormat format, ITelemetry? telemetry = null)
    {
        ArgumentNullException.ThrowIfNull(format);

        _format = format;
        _telemetry = telemetry;
    }

    /// <summary>Search a recording's passband.</summary>
    /// <param name="audio">The recording.</param>
    /// <param name="lowestHz">The passband's low edge.</param>
    /// <param name="highestHz">The passband's high edge.</param>
    /// <returns>What was found.</returns>
    public OliviaSearch Search(MonoAudio audio, double lowestHz, double highestHz)
    {
        ArgumentNullException.ThrowIfNull(audio);

        var rate = audio.SampleRate;
        var total = audio.Samples.Length;
        var step = Math.Max(1, (int)Math.Round(_format.Variants.Max(v => v.SymbolSeconds) * _format.SymbolsPerBlock * rate));
        var taken = Math.Min(total, 2 * step);
        Look look;

        while (true)
        {
            look = LookAt(new MonoAudio(rate, audio.Samples[..taken]), lowestHz, highestHz);

            if (look.Candidates.Count > 0 || taken == total)
            {
                break;
            }

            taken = Math.Min(total, taken + step);
        }

        var result = new OliviaSearch(look.Candidates, taken / (double)rate, look.FloorDb, look.PeakOverFloorDb);

        Write(result, look.Regions, lowestHz, highestHz);

        return result;
    }

    private Look LookAt(MonoAudio audio, double lowestHz, double highestHz)
    {
        var rate = audio.SampleRate;
        var samples = audio.Samples;
        var seconds = samples.Length / (double)rate;
        var narrowestSpacing = _format.Variants.Min(v => v.ToneSpacingHz);
        var narrowestBand = _format.Variants.Min(v => v.BandwidthHz);

        // 1. The long view: an average of power spectra with bins an eighth of the narrowest spacing.
        var size = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)Math.Ceiling(rate / (narrowestSpacing / 8)));
        var (average, count) = Welch(samples, size);
        var binHz = (double)rate / size;
        var low = Math.Max(1, (int)Math.Ceiling(lowestHz / binHz));
        var high = Math.Min(average.Length - 2, (int)Math.Floor(highestHz / binHz));

        if (count == 0 || high <= low)
        {
            return new Look([], [], double.NaN, double.NaN);
        }

        var band = average[low..(high + 1)];
        var floor = Median(band);
        var loudest = band.Max();

        if (floor <= 0)
        {
            return new Look([], [], double.NaN, double.NaN);
        }

        var floorDb = 10 * Math.Log10(floor);
        var peakDb = 10 * Math.Log10(loudest / floor);
        var gate = floor * (1 + (GateSigmas / Math.Sqrt(count)));
        var claimed = new bool[average.Length];
        var regions = new List<Region>();
        var candidates = new List<OliviaCandidate>();

        // 2. Every region standing clear of the gate, loudest first.
        while (true)
        {
            var peakBin = -1;

            for (var b = low; b <= high; b++)
            {
                if (!claimed[b] && average[b] >= gate && (peakBin < 0 || average[b] > average[peakBin]))
                {
                    peakBin = b;
                }
            }

            if (peakBin < 0)
            {
                break;
            }

            var from = peakBin;
            var to = peakBin;

            while (from > low && !claimed[from - 1] && average[from - 1] >= gate)
            {
                from--;
            }

            while (to < high && !claimed[to + 1] && average[to + 1] >= gate)
            {
                to++;
            }

            for (var b = from; b <= to; b++)
            {
                claimed[b] = true;
            }

            var edge = Math.Max(gate, average[peakBin] / OccupiedRatio);
            var lowEdge = peakBin;
            var highEdge = peakBin;

            while (lowEdge > from && average[lowEdge - 1] >= edge)
            {
                lowEdge--;
            }

            while (highEdge < to && average[highEdge + 1] >= edge)
            {
                highEdge++;
            }

            var occupiedLow = (lowEdge - 0.5) * binHz;
            var occupiedHigh = (highEdge + 0.5) * binHz;

            if (occupiedHigh - occupiedLow < narrowestBand / 2.0)
            {
                continue;
            }

            // 3. The tones, from where the loudest frequency sits frame by frame.
            var (tones, spacing, middle) = Tones(samples, rate, occupiedLow, occupiedHigh);

            if (double.IsNaN(middle))
            {
                middle = (occupiedLow + occupiedHigh) / 2;
            }

            // 4. The rows that fit, ranked, each tried by decoding what has been taken.
            var width = occupiedHigh - occupiedLow;
            var ranked = _format.Variants
                .Select(v => (Row: v, Band: Octaves(v.BandwidthHz, width), Spacing: double.IsNaN(spacing) ? double.NaN : Octaves(v.ToneSpacingHz, spacing)))
                .Where(r => r.Band <= FitOctaves || r.Spacing <= FitOctaves)
                .OrderByDescending(r => (r.Band <= FitOctaves ? 1 : 0) + (r.Spacing <= FitOctaves ? 1 : 0))
                .ThenBy(r => r.Band + (double.IsNaN(r.Spacing) ? 0 : r.Spacing))
                .Select(r => r.Row)
                .ToList();
            var trials = new List<OliviaTrial>();
            OliviaCandidate? chosen = null;

            foreach (var row in ranked)
            {
                OliviaDecoding decoding;

                try
                {
                    decoding = new OliviaDemodulator(_format, row, middle, rate).Decode(audio, 0);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // The row's tones would not fit under the rate at this middle; it cannot be this.
                    continue;
                }

                trials.Add(new OliviaTrial(
                    row.Name,
                    decoding.SyncSnr,
                    decoding.BlocksDecoded,
                    decoding.BlocksRejected,
                    decoding.BlockSnrs.DefaultIfEmpty(0).Max()));

                // **A ROW NO BETTER THAN NOISE IS NOT A DETECTION**: it must clear the threshold.
                if (decoding.BlocksDecoded >= ConfirmBlocks && (chosen is null || decoding.SyncSnr > chosen.Confidence))
                {
                    chosen = new OliviaCandidate(
                        row,
                        middle + decoding.FrequencyOffsetHz,
                        decoding.SyncSnr,
                        seconds,
                        middle,
                        spacing,
                        tones,
                        occupiedLow,
                        occupiedHigh,
                        trials);
                }
            }

            regions.Add(new Region(middle, spacing, tones, occupiedLow, occupiedHigh, trials, chosen));

            if (chosen is not null)
            {
                candidates.Add(chosen with { Trials = trials.ToArray() });
            }
        }

        return new Look(candidates, regions, floorDb, peakDb);
    }

    /// <summary>
    /// The tones in an occupied band: how many stand out, their mean spacing and their middle.
    /// </summary>
    /// <remarks>
    /// <para>**THE LOUDEST FREQUENCY, FRAME BY FRAME, PILES UP ON THE TONES.** Each window is two of
    /// the longest row's symbols long, hopped a quarter of the shortest row's, zero-padded four
    /// times; the loudest bin in the band is placed to a fraction of a bin by a parabola through
    /// its log power and its neighbours'. A window across a symbol change lands between tones, so
    /// the histogram is not clean, but its peaks are: a peak a quarter of the tallest or more, and
    /// taller than anything within half the narrowest spacing of it, is a tone.</para>
    /// <para>**MEASURED THIS WAY BECAUSE THE OTHERS WERE NOT** (work instruction 362 task 1): the
    /// ripple across the averaged spectrum read 35.16 Hz and the histogram's own autocorrelation
    /// 62.50 Hz on a file whose tones are 31.25 Hz apart, and the peaks read 31.250.</para>
    /// </remarks>
    private (int Tones, double SpacingHz, double MiddleHz) Tones(float[] samples, int rate, double lowHz, double highHz)
    {
        var longest = _format.Variants.Max(v => v.SymbolSeconds);
        var shortest = _format.Variants.Min(v => v.SymbolSeconds);
        var narrowestSpacing = _format.Variants.Min(v => v.ToneSpacingHz);
        var window = 2 * (int)Math.Round(longest * rate);
        var hop = Math.Max(1, (int)Math.Round(shortest * rate / 4));
        var size = 4 * (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)window);
        var binHz = (double)rate / size;
        var from = Math.Max(1, (int)Math.Floor(lowHz / binHz));
        var to = Math.Min((size / 2) - 1, (int)Math.Ceiling(highHz / binHz));

        if (samples.Length < window || to - from < 2)
        {
            return (0, double.NaN, double.NaN);
        }

        var fft = new RealFft(size);
        var magnitudes = new double[fft.BinCount];
        var real = new double[size];
        var imaginary = new double[size];
        var shaped = new float[size];
        var shape = new double[window];

        for (var i = 0; i < window; i++)
        {
            shape[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / window));
        }

        var frames = ((samples.Length - window) / hop) + 1;
        var peaks = new double[frames];
        var levels = new double[frames];

        for (var f = 0; f < frames; f++)
        {
            for (var i = 0; i < window; i++)
            {
                shaped[i] = (float)(samples[(f * hop) + i] * shape[i]);
            }

            fft.Magnitudes(shaped, magnitudes, real, imaginary);

            var top = from;
            double level = 0;

            for (var b = from; b <= to; b++)
            {
                var p = magnitudes[b] * magnitudes[b];

                level += p;

                if (p > magnitudes[top] * magnitudes[top])
                {
                    top = b;
                }
            }

            var delta = 0.0;

            if (top > from && top < to)
            {
                var l = Math.Log((magnitudes[top - 1] * magnitudes[top - 1]) + double.Epsilon);
                var c = Math.Log((magnitudes[top] * magnitudes[top]) + double.Epsilon);
                var r = Math.Log((magnitudes[top + 1] * magnitudes[top + 1]) + double.Epsilon);
                var d = l - (2 * c) + r;

                delta = d == 0 ? 0 : 0.5 * (l - r) / d;
            }

            peaks[f] = (top + delta) * binHz;
            levels[f] = level;
        }

        // Frames with the carrier in them: a tenth of the median band power or more.
        var quiet = Median(levels) / 10;
        var cell = binHz / 4;
        var histogram = new double[(int)Math.Ceiling((to - from + 1) * binHz / cell) + 1];

        for (var f = 0; f < frames; f++)
        {
            if (levels[f] > 0 && levels[f] >= quiet)
            {
                histogram[Math.Clamp((int)Math.Round((peaks[f] - (from * binHz)) / cell), 0, histogram.Length - 1)]++;
            }
        }

        // A few seconds put only a few dozen frames on each tone, so the histogram is summed over
        // one transform bin, the width a peak's parabola places it within.
        var smoothed = new double[histogram.Length];

        for (var i = 0; i < histogram.Length; i++)
        {
            for (var j = Math.Max(0, i - 2); j <= Math.Min(histogram.Length - 1, i + 2); j++)
            {
                smoothed[i] += histogram[j];
            }
        }

        histogram = smoothed;

        var tall = histogram.Max() / 4;
        var apart = (int)Math.Floor(narrowestSpacing / 2 / cell);
        var tones = new List<double>();

        for (var i = 0; i < histogram.Length; i++)
        {
            if (histogram[i] < tall || histogram[i] == 0)
            {
                continue;
            }

            var tallest = true;

            for (var j = Math.Max(0, i - apart); j <= Math.Min(histogram.Length - 1, i + apart) && tallest; j++)
            {
                tallest = j == i || histogram[j] < histogram[i] || (histogram[j] == histogram[i] && j > i);
            }

            if (tallest)
            {
                tones.Add((from * binHz) + (i * cell));
            }
        }

        if (tones.Count < 2)
        {
            return (tones.Count, double.NaN, double.NaN);
        }

        // **THE MEDIAN GAP, NOT THE SPAN OVER THE COUNT**: a tone that did not stand out doubles
        // one gap and leaves the median where it was.
        var gaps = tones.Zip(tones.Skip(1), (a, b) => b - a).ToArray();

        return (tones.Count, Median(gaps), (tones[0] + tones[^1]) / 2);
    }

    private static double Octaves(double a, double b) => Math.Abs(Math.Log2(a / b));

    private static (double[] Average, int Count) Welch(float[] samples, int size)
    {
        var fft = new RealFft(size);
        var magnitudes = new double[fft.BinCount];
        var real = new double[size];
        var imaginary = new double[size];
        var shaped = new float[size];
        var sum = new double[fft.BinCount];
        var count = 0;

        for (var at = 0; at + size <= samples.Length; at += size / 2)
        {
            for (var i = 0; i < size; i++)
            {
                shaped[i] = (float)(samples[at + i] * 0.5 * (1 - Math.Cos(2 * Math.PI * i / size)));
            }

            fft.Magnitudes(shaped, magnitudes, real, imaginary);

            for (var b = 0; b < sum.Length; b++)
            {
                sum[b] += magnitudes[b] * magnitudes[b];
            }

            count++;
        }

        for (var b = 0; b < sum.Length && count > 0; b++)
        {
            sum[b] /= count;
        }

        return (sum, count);
    }

    private static double Median(double[] values)
    {
        if (values.Length == 0)
        {
            return 0;
        }

        var sorted = (double[])values.Clone();

        Array.Sort(sorted);

        return sorted[sorted.Length / 2];
    }

    /// <summary>
    /// The search's one event per region weighed, or one saying nothing stood out: what it
    /// measured, the rows it weighed with their trial figures, and the one it named.
    /// </summary>
    /// <remarks>**NO TEXT AND NO CALLSIGN** (HM-DEC-018, PSK31 plan §R13): the trial decodes'
    /// characters are never read here, only their counts and figures.</remarks>
    private void Write(OliviaSearch result, IReadOnlyList<Region> regions, double lowestHz, double highestHz)
    {
        if (_telemetry is null)
        {
            return;
        }

        if (regions.Count == 0)
        {
            _telemetry.Write(TelemetryCategory.Psk31, "olivia_search", Common(result, lowestHz, highestHz, null));

            return;
        }

        foreach (var region in regions)
        {
            var data = Common(result, lowestHz, highestHz, region.Chosen);

            data["centerHz"] = region.Chosen is null ? null : Math.Round(region.Chosen.CenterHz, 2);
            data["confidence"] = region.Chosen is null ? null : Math.Round(region.Chosen.Confidence, 2);
            data["measuredCenterHz"] = Math.Round(region.MiddleHz, 2);
            data["toneSpacingHz"] = double.IsNaN(region.SpacingHz) ? null : Math.Round(region.SpacingHz, 3);
            data["tonesCounted"] = region.Tones;
            data["occupiedLowHz"] = Math.Round(region.OccupiedLowHz, 2);
            data["occupiedHighHz"] = Math.Round(region.OccupiedHighHz, 2);
            data["weighed"] = region.Trials.Select(t => t.Variant).ToArray();
            data["weighedSnr"] = region.Trials.Select(t => Math.Round(t.SyncSnr, 2)).ToArray();
            data["weighedBlocks"] = region.Trials.Select(t => t.BlocksDecoded).ToArray();

            _telemetry.Write(TelemetryCategory.Psk31, "olivia_search", data);
        }
    }

    private static Dictionary<string, object?> Common(OliviaSearch result, double lowestHz, double highestHz, OliviaCandidate? chosen)
        => new()
        {
            ["mode"] = "olivia",
            ["variant"] = chosen?.Variant.Name,
            ["found"] = result.Candidates.Count,
            ["audioSeconds"] = Math.Round(result.AudioSeconds, 3),
            ["passbandLowHz"] = lowestHz,
            ["passbandHighHz"] = highestHz,
            ["floorDb"] = double.IsNaN(result.FloorDb) ? null : Math.Round(result.FloorDb, 2),
            ["peakOverFloorDb"] = double.IsNaN(result.PeakOverFloorDb) ? null : Math.Round(result.PeakOverFloorDb, 2),
            ["gateSigmas"] = GateSigmas,
            ["threshold"] = OliviaDemodulator.SyncThreshold,
        };

    private sealed record Region(
        double MiddleHz,
        double SpacingHz,
        int Tones,
        double OccupiedLowHz,
        double OccupiedHighHz,
        IReadOnlyList<OliviaTrial> Trials,
        OliviaCandidate? Chosen);

    private sealed record Look(IReadOnlyList<OliviaCandidate> Candidates, IReadOnlyList<Region> Regions, double FloorDb, double PeakOverFloorDb);
}

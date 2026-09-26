namespace Hamlet.RadioEngine.Tests.Cw.Instruments;

/// <summary>
/// One analysis window in which the instrument found keying, and the pitch it
/// measured there.
/// </summary>
/// <param name="StartSeconds">Where the window starts in the audio.</param>
/// <param name="EndSeconds">Where it ends.</param>
/// <param name="EffectiveSeconds">
/// The instant the measured pitch belongs to: the centroid of the samples the fine
/// stage weighed, corrected for their skew. On a note that slides, the measured
/// pitch is the note's pitch at this instant; on a steady note it does not matter.
/// </param>
/// <param name="Hz">The measured pitch.</param>
/// <param name="CoarseHz">The centre of the coarse bin that keyed hardest.</param>
/// <param name="KeyedSeconds">How much of the window was judged keyed and weighed.</param>
/// <param name="ContrastDb">How hard that coarse bin keyed: its 90th over its 20th percentile.</param>
public sealed record PitchWindow(
    double StartSeconds,
    double EndSeconds,
    double EffectiveSeconds,
    double Hz,
    double CoarseHz,
    double KeyedSeconds,
    double ContrastDb);

/// <summary>
/// Measures the note a station is keying, to a fraction of a hertz, without the
/// tracker (work instruction 447; PHASE_PLAN.md 4.1, R76; HM-REQ-092).
/// </summary>
/// <remarks>
/// <para>**A MEASURING INSTRUMENT, UNDER THE TESTS, AND NEVER IN THE DECODE PATH.**
/// It exists so that MET-PITCH-ERR can be read against something the tracker did
/// not compute (CLAUDE.md 12.5). It lives here and not under `src` because nothing
/// in the product calls it, and a type under `src` would invite something to.</para>
/// <para>**IT SHARES NO CODE WITH `CwToneTracker`, `CwToneSurvey` OR `CwPitchChoice`**
/// and reads none of their state. It references only the base class library:
/// `Math`, arrays of `float` and `double`, `List`, `Array.Sort` and this file's
/// own record. Its transform is a radix-2 FFT written here, its window a Hann
/// written here, and its fine stage a direct sum written here.</para>
/// <para>**TWO STAGES.**</para>
/// <list type="number">
/// <item>**Where keying is.** Frames of about 30 ms (the next power of two in
/// samples), a quarter-frame apart, Hann-windowed and transformed. Each bin
/// from 300 to 900 Hz is put relative to the frame's median bin, so a fade, the
/// radio's AGC or the operator's own transmit mute moves every bin together and
/// cancels, and smoothed over three frames. Frames more than 30 dB below the
/// recording's median frame are the receiver muted and are left out. Within each
/// analysis window the bin whose level swings hardest - 90th percentile over
/// 20th, in decibels - is where somebody is keying, if that swing is at least
/// <see cref="ContrastFloorDb"/>. A steady carrier does not swing, however loud;
/// noise swings a few decibels. Its frames above the midpoint of the two
/// percentiles are the marks.</item>
/// <item>**What the note is.** Only the samples inside those marks, under a Hann
/// over the whole window, go into a discrete-time Fourier sum evaluated directly:
/// every 0.25 Hz across one coarse bin and 2 Hz either side of the bin that keyed
/// hardest, then every 0.02 Hz
/// across a quarter-hertz either side of the best, then a parabola through the
/// best three. Because the marks' own weights are never negative, a pure keyed
/// tone's sum peaks exactly at its pitch, and the keying's sidebands can only sit
/// lower.</item>
/// </list>
/// <para>**ITS BIN IS <see cref="BinHz"/>, HALF A HERTZ**: one over the two-second
/// window, the spacing at which a window this long resolves two tones. It is fifty
/// times finer than the tracker's 25 Hz coarse bank and ten times finer than its
/// 5 Hz fine bank.</para>
/// </remarks>
public static class CwPitchInstrument
{
    /// <summary>Lowest pitch searched, in hertz: HM-REQ-090's bottom.</summary>
    public const double MinimumHz = 300;

    /// <summary>Highest pitch searched, in hertz: HM-REQ-090's top.</summary>
    public const double MaximumHz = 900;

    /// <summary>Each analysis window, in seconds.</summary>
    public const double WindowSeconds = 2.0;

    /// <summary>How far one analysis window starts after the one before, in seconds.</summary>
    public const double HopSeconds = 1.0;

    /// <summary>The instrument's bin, in hertz: one over the window.</summary>
    public const double BinHz = 1.0 / WindowSeconds;

    /// <summary>The least swing, in decibels, that counts as somebody keying.</summary>
    public const double ContrastFloorDb = 12;

    /// <summary>The least keyed time a window must hold to be measured, in seconds.</summary>
    public const double LeastKeyedSeconds = 0.3;

    /// <summary>How far below the median frame a frame is the receiver muted, in decibels.</summary>
    private const double MutedBelowDb = 30;

    /// <summary>The coarse frame, in seconds, before rounding up to a power of two.</summary>
    private const double FrameSeconds = 0.03;

    /// <summary>How far past one coarse bin, either side, the fine stage looks, in hertz.</summary>
    /// <remarks>
    /// The reach is a whole coarse bin plus this, because the bin that keys hardest
    /// in a window is sometimes the neighbour of the nearest one: at a flat 20 Hz
    /// the first run missed 400 Hz from the 375 bin and 900 Hz from the 875 bin.
    /// </remarks>
    private const double SearchMarginHz = 2;

    /// <summary>The fine stage's first grid, in hertz.</summary>
    private const double FirstStepHz = 0.25;

    /// <summary>The fine stage's second grid, in hertz.</summary>
    private const double SecondStepHz = 0.02;

    /// <summary>Measure every window of a recording.</summary>
    /// <param name="samples">The audio.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>Each window where keying was found, in order; none where it was not.</returns>
    public static IReadOnlyList<PitchWindow> Measure(float[] samples, int sampleRate)
    {
        var frame = 1;

        while (frame < FrameSeconds * sampleRate)
        {
            frame *= 2;
        }

        var hop = frame / 4;
        var frames = samples.Length < frame ? 0 : ((samples.Length - frame) / hop) + 1;
        var low = (int)Math.Ceiling(MinimumHz * frame / sampleRate);
        var high = (int)Math.Floor(MaximumHz * frame / sampleRate);
        var bins = high - low + 1;

        var results = new List<PitchWindow>();

        if (frames < 3 || bins < 1)
        {
            return results;
        }

        var hann = new double[frame];

        for (var i = 0; i < frame; i++)
        {
            hann[i] = 0.5 - (0.5 * Math.Cos(2 * Math.PI * i / frame));
        }

        // The coarse stage: every frame's bins relative to its own median bin.
        var relative = new double[frames][];
        var frameDb = new double[frames];
        var re = new double[frame];
        var im = new double[frame];
        var levels = new double[bins];

        for (var f = 0; f < frames; f++)
        {
            var at = f * hop;
            var energy = 0.0;

            for (var i = 0; i < frame; i++)
            {
                var x = (double)samples[at + i];
                energy += x * x;
                re[i] = x * hann[i];
                im[i] = 0;
            }

            frameDb[f] = 10 * Math.Log10((energy / frame) + 1e-30);
            Fft(re, im);

            for (var b = 0; b < bins; b++)
            {
                var k = low + b;
                levels[b] = 10 * Math.Log10((re[k] * re[k]) + (im[k] * im[k]) + 1e-30);
            }

            var sorted = (double[])levels.Clone();
            Array.Sort(sorted);
            var median = sorted[bins / 2];
            var row = new double[bins];

            for (var b = 0; b < bins; b++)
            {
                row[b] = levels[b] - median;
            }

            relative[f] = row;
        }

        var smoothed = new double[frames][];

        for (var f = 0; f < frames; f++)
        {
            var row = new double[bins];
            var from = Math.Max(0, f - 1);
            var to = Math.Min(frames - 1, f + 1);

            for (var b = 0; b < bins; b++)
            {
                var sum = 0.0;

                for (var g = from; g <= to; g++)
                {
                    sum += relative[g][b];
                }

                row[b] = sum / (to - from + 1);
            }

            smoothed[f] = row;
        }

        var frameSorted = (double[])frameDb.Clone();
        Array.Sort(frameSorted);
        var mutedBelow = frameSorted[frames / 2] - MutedBelowDb;

        var window = (int)Math.Round(WindowSeconds * sampleRate);
        var step = (int)Math.Round(HopSeconds * sampleRate);
        var column = new List<double>();
        var gated = new List<(int Start, int End)>();

        for (var start = 0; start + window <= samples.Length; start += step)
        {
            // Frames whose middle falls inside the window and whose receiver was not muted.
            var inside = new List<int>();

            for (var f = 0; f < frames; f++)
            {
                var middle = (f * hop) + (frame / 2);

                if (middle >= start && middle < start + window && frameDb[f] >= mutedBelow)
                {
                    inside.Add(f);
                }
            }

            if (inside.Count < 10)
            {
                continue;
            }

            var bestBin = -1;
            var bestContrast = double.NegativeInfinity;
            var bestLow = 0.0;
            var bestHigh = 0.0;

            for (var b = 0; b < bins; b++)
            {
                column.Clear();

                foreach (var f in inside)
                {
                    column.Add(smoothed[f][b]);
                }

                column.Sort();
                var p20 = column[(int)(0.2 * (column.Count - 1))];
                var p90 = column[(int)(0.9 * (column.Count - 1))];

                if (p90 - p20 > bestContrast)
                {
                    bestContrast = p90 - p20;
                    bestBin = b;
                    bestLow = p20;
                    bestHigh = p90;
                }
            }

            if (bestContrast < ContrastFloorDb)
            {
                continue;
            }

            // The marks: frames above the midpoint, each owning the hop around its middle.
            var threshold = (bestLow + bestHigh) / 2;
            gated.Clear();

            foreach (var f in inside)
            {
                if (smoothed[f][bestBin] <= threshold)
                {
                    continue;
                }

                var middle = (f * hop) + (frame / 2);
                var from = Math.Max(start, middle - (hop / 2));
                var to = Math.Min(start + window, middle + hop - (hop / 2));

                if (gated.Count > 0 && gated[^1].End >= from)
                {
                    gated[^1] = (gated[^1].Start, Math.Max(gated[^1].End, to));
                }
                else
                {
                    gated.Add((from, to));
                }
            }

            var keyed = 0;

            foreach (var (from, to) in gated)
            {
                keyed += to - from;
            }

            if (keyed < LeastKeyedSeconds * sampleRate)
            {
                continue;
            }

            var coarseHz = (double)(low + bestBin) * sampleRate / frame;
            var reach = ((double)sampleRate / frame) + SearchMarginHz;
            var hz = FinePeak(samples, sampleRate, start, window, gated, coarseHz, reach);
            var effective = EffectiveSeconds(sampleRate, start, window, gated);

            results.Add(new PitchWindow(
                (double)start / sampleRate,
                (double)(start + window) / sampleRate,
                effective,
                hz,
                coarseHz,
                (double)keyed / sampleRate,
                bestContrast));
        }

        return results;
    }

    /// <summary>The fine stage: the peak of the marks' own Fourier sum near the coarse bin.</summary>
    private static double FinePeak(
        float[] samples, int sampleRate, int start, int window, List<(int Start, int End)> gated, double coarseHz, double reach)
    {
        // The marks' samples under the window's Hann, once.
        var weighted = new List<double[]>();

        foreach (var (from, to) in gated)
        {
            var run = new double[to - from];

            for (var n = from; n < to; n++)
            {
                run[n - from] = samples[n] * Taper(n - start, window);
            }

            weighted.Add(run);
        }

        double Power(double hz)
        {
            var w = 2 * Math.PI * hz / sampleRate;
            var stepRe = Math.Cos(w);
            var stepIm = -Math.Sin(w);
            double sumRe = 0, sumIm = 0;

            for (var r = 0; r < gated.Count; r++)
            {
                // A fresh phasor at every run keeps the rotation's rounding short.
                var from = gated[r].Start;
                var run = weighted[r];
                var pRe = Math.Cos(w * from);
                var pIm = -Math.Sin(w * from);

                for (var i = 0; i < run.Length; i++)
                {
                    var v = run[i];
                    sumRe += v * pRe;
                    sumIm += v * pIm;

                    var nextRe = (pRe * stepRe) - (pIm * stepIm);
                    pIm = (pRe * stepIm) + (pIm * stepRe);
                    pRe = nextRe;
                }
            }

            return (sumRe * sumRe) + (sumIm * sumIm);
        }

        var best = coarseHz;
        var bestPower = double.NegativeInfinity;

        for (var hz = coarseHz - reach; hz <= coarseHz + reach + 1e-9; hz += FirstStepHz)
        {
            var p = Power(hz);

            if (p > bestPower)
            {
                bestPower = p;
                best = hz;
            }
        }

        var centre = best;
        var fine = new List<(double Hz, double Power)>();

        for (var hz = centre - FirstStepHz; hz <= centre + FirstStepHz + 1e-9; hz += SecondStepHz)
        {
            fine.Add((hz, Power(hz)));
        }

        var top = 0;

        for (var i = 1; i < fine.Count; i++)
        {
            if (fine[i].Power > fine[top].Power)
            {
                top = i;
            }
        }

        if (top == 0 || top == fine.Count - 1)
        {
            return fine[top].Hz;
        }

        // A parabola through the best three, in decibels.
        var a = 10 * Math.Log10(fine[top - 1].Power + 1e-300);
        var b = 10 * Math.Log10(fine[top].Power + 1e-300);
        var c = 10 * Math.Log10(fine[top + 1].Power + 1e-300);
        var denominator = a - (2 * b) + c;
        var offset = denominator == 0 ? 0 : 0.5 * (a - c) / denominator;

        return fine[top].Hz + (Math.Clamp(offset, -0.5, 0.5) * SecondStepHz);
    }

    /// <summary>The instant the measured pitch belongs to, from the same weights the fine stage used.</summary>
    private static double EffectiveSeconds(int sampleRate, int start, int window, List<(int Start, int End)> gated)
    {
        double sum = 0, first = 0;

        foreach (var (from, to) in gated)
        {
            for (var n = from; n < to; n++)
            {
                var w = Taper(n - start, window);
                sum += w;
                first += w * n;
            }
        }

        if (sum <= 0)
        {
            return (start + (window / 2.0)) / sampleRate;
        }

        var mean = first / sum;
        double second = 0, third = 0;

        foreach (var (from, to) in gated)
        {
            for (var n = from; n < to; n++)
            {
                var w = Taper(n - start, window);
                var u = n - mean;
                second += w * u * u;
                third += w * u * u * u;
            }
        }

        second /= sum;
        third /= sum;

        var instant = second > 0 ? mean + (third / (2 * second)) : mean;

        return instant / sampleRate;
    }

    /// <summary>The Hann over the whole analysis window.</summary>
    private static double Taper(int n, int length)
        => 0.5 - (0.5 * Math.Cos(2 * Math.PI * n / (length - 1)));

    /// <summary>An in-place radix-2 FFT; the length must be a power of two.</summary>
    private static void Fft(double[] re, double[] im)
    {
        var n = re.Length;

        for (int i = 1, j = 0; i < n; i++)
        {
            var bit = n >> 1;

            for (; (j & bit) != 0; bit >>= 1)
            {
                j ^= bit;
            }

            j ^= bit;

            if (i < j)
            {
                (re[i], re[j]) = (re[j], re[i]);
                (im[i], im[j]) = (im[j], im[i]);
            }
        }

        for (var length = 2; length <= n; length <<= 1)
        {
            var angle = -2 * Math.PI / length;
            var wRe = Math.Cos(angle);
            var wIm = Math.Sin(angle);

            for (var i = 0; i < n; i += length)
            {
                double uRe = 1, uIm = 0;

                for (var k = 0; k < length / 2; k++)
                {
                    var aRe = re[i + k];
                    var aIm = im[i + k];
                    var bRe = (re[i + k + (length / 2)] * uRe) - (im[i + k + (length / 2)] * uIm);
                    var bIm = (re[i + k + (length / 2)] * uIm) + (im[i + k + (length / 2)] * uRe);

                    re[i + k] = aRe + bRe;
                    im[i + k] = aIm + bIm;
                    re[i + k + (length / 2)] = aRe - bRe;
                    im[i + k + (length / 2)] = aIm - bIm;

                    var nextRe = (uRe * wRe) - (uIm * wIm);
                    uIm = (uRe * wIm) + (uIm * wRe);
                    uRe = nextRe;
                }
            }
        }
    }
}

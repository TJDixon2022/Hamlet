using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Psk31;
using Hamlet.RadioEngine.Rsid;

namespace Hamlet.RadioEngine.Tests.Olivia;

/// <summary>What one Olivia signal measures, by <see cref="OliviaSignalMeasure"/>.</summary>
/// <param name="ToneSpacingHz">The least-squares spacing of the tone peaks in the long-term spectrum.</param>
/// <param name="Tones">How many tone peaks were found.</param>
/// <param name="SymbolRateHz">The strongest line in the squared signal's spectrum between 10 and 100 Hz.</param>
/// <param name="OccupiedBandwidthHz">The width holding 99% of the power, 0.5% cut from each side.</param>
/// <param name="PreambleSeconds">From the end of the RSID burst's tones to the onset of the first data symbol.</param>
/// <param name="DataSeconds">From that onset to the last sample above the onset threshold.</param>
/// <param name="BurstCode">The RSID code the detector read.</param>
/// <param name="BurstCenterHz">Where the detector put the burst.</param>
public sealed record OliviaSignalMeasurement(
    double ToneSpacingHz,
    int Tones,
    double SymbolRateHz,
    double OccupiedBandwidthHz,
    double PreambleSeconds,
    double DataSeconds,
    int BurstCode,
    double BurstCenterHz);

/// <summary>
/// **One measuring code for the mode author's audio and for Hamlet's** (work instruction 365
/// decision AS). Task 0 measures the three clean fixtures with it and task 2 measures Hamlet's
/// signal with it, so a difference between the two is a difference in the signals.
/// </summary>
/// <remarks>
/// <para>**NOTHING HERE IS TOLD THE VARIANT.** The tones, the spacing, the symbol rate and the
/// bandwidth are read off the samples; the only thing located first is the RSID burst, by Hamlet's
/// own detector, whose tones' end is where the preamble is measured from.</para>
/// <para>**THE FOUR METHODS.** The spectrum is averaged over the data (Hann segments of about a
/// second, 8192 samples at 8000 Hz, a quarter apart). Tone spacing: one Hann frame a symbol
/// long per symbol, at whichever of eight timings puts the most of each frame's power in its
/// loudest bin; each frame's loudest frequency, parabola-refined; those clustered wherever a gap
/// exceeds 8 Hz, a cluster under 1% of the frames dropped, and the clusters' medians fitted by
/// least squares against their index. (The averaged spectrum's own peaks were tried first and
/// found 11 on the 8-tone fixture, so they are not used for this.) Symbol rate: the
/// strongest line of the squared signal's spectrum between 10 and 100 Hz, parabola-refined; the
/// symbol shaping repeats once a symbol, so that is where its line is. Occupied bandwidth: where
/// the cumulative power of the averaged spectrum crosses 0.5% and 99.5%. Preamble: the first
/// sample after the burst's tones at 1% of the data's peak or more.</para>
/// </remarks>
public static class OliviaSignalMeasure
{
    private const double OnsetFraction = 0.01;

    /// <summary>Measure a recording that begins with an RSID burst and carries one Olivia signal.</summary>
    /// <param name="audio">The recording.</param>
    /// <returns>The measurement.</returns>
    /// <exception cref="InvalidOperationException">No burst, or more than one, was heard.</exception>
    public static OliviaSignalMeasurement Measure(MonoAudio audio)
    {
        var codes = Hamlet.RadioEngine.Olivia.OliviaData.Current.Rsid
            ?? throw new InvalidOperationException(Hamlet.RadioEngine.Olivia.OliviaData.Current.Problem);
        var heard = RsidDetector.Detect(codes, audio, Psk31CarrierSearch.PassbandLowHz, Psk31CarrierSearch.PassbandHighHz);

        if (heard.Count != 1)
        {
            throw new InvalidOperationException("the detector heard " + heard.Count + " bursts, not one");
        }

        var burst = heard[0];
        var burstEnd = burst.StartSeconds + (codes.Symbols / codes.SymbolRateHz);
        var samples = audio.Samples;
        var rate = audio.SampleRate;
        var from = Math.Min(samples.Length, (int)Math.Ceiling(burstEnd * rate));
        var peak = 0.0;

        for (var n = from; n < samples.Length; n++)
        {
            peak = Math.Max(peak, Math.Abs(samples[n]));
        }

        var onset = from;

        while (onset < samples.Length && Math.Abs(samples[onset]) < OnsetFraction * peak)
        {
            onset++;
        }

        var end = samples.Length - 1;

        while (end > onset && Math.Abs(samples[end]) < OnsetFraction * peak)
        {
            end--;
        }

        var data = samples.AsSpan(onset, end - onset + 1);
        var spectrum = AveragedSpectrum(data, rate, out var binHz);
        var symbolRate = SymbolRate(data, rate);
        var (spacing, tones) = ToneSpacing(data, rate, symbolRate);

        return new OliviaSignalMeasurement(
            spacing,
            tones,
            symbolRate,
            OccupiedBandwidth(spectrum, binHz),
            (onset / (double)rate) - burstEnd,
            (end - onset + 1) / (double)rate,
            burst.Code,
            burst.CenterHz);
    }

    /// <summary>The Hann-windowed power spectrum, averaged over every segment of the data.</summary>
    private static double[] AveragedSpectrum(ReadOnlySpan<float> data, int rate, out double binHz)
    {
        // About a second a segment at any rate: 8192 samples at 8000 Hz.
        var segment = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)rate);
        var hop = segment / 4;
        var fft = new RealFft(segment);
        var magnitudes = new double[fft.BinCount];
        var real = new double[segment];
        var imaginary = new double[segment];
        var window = new float[segment];
        var sum = new double[fft.BinCount];
        var count = 0;

        binHz = (double)rate / segment;

        for (var at = 0; at + segment <= data.Length; at += hop)
        {
            for (var i = 0; i < segment; i++)
            {
                window[i] = (float)(data[at + i] * 0.5 * (1 - Math.Cos(2 * Math.PI * i / segment)));
            }

            fft.Magnitudes(window, magnitudes, real, imaginary);

            for (var b = 0; b < sum.Length; b++)
            {
                sum[b] += magnitudes[b] * magnitudes[b];
            }

            count++;
        }

        for (var b = 0; b < sum.Length; b++)
        {
            sum[b] /= Math.Max(1, count);
        }

        return sum;
    }

    /// <summary>
    /// The tones' spacing: one symbol-long frame per symbol at the timing that puts most of each
    /// frame's power in one bin, each frame's loudest frequency, clustered, fitted.
    /// </summary>
    private static (double SpacingHz, int Tones) ToneSpacing(ReadOnlySpan<float> data, int rate, double symbolRate)
    {
        const int Phases = 8;
        const double ClusterGapHz = 8;

        var perSymbol = rate / symbolRate;
        var length = (int)Math.Round(perSymbol);
        var size = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)length) * 8;
        var fft = new RealFft(size);
        var magnitudes = new double[fft.BinCount];
        var real = new double[size];
        var imaginary = new double[size];
        var frame = new float[size];
        var binHz = (double)rate / size;
        var bestPurity = double.MinValue;
        List<double> best = new();

        for (var phase = 0; phase < Phases; phase++)
        {
            var found = new List<double>();
            var purity = 0.0;

            for (var k = 0; ; k++)
            {
                var at = (int)Math.Round((phase * perSymbol / Phases) + (k * perSymbol));

                if (at + length > data.Length)
                {
                    break;
                }

                Array.Clear(frame);

                for (var i = 0; i < length; i++)
                {
                    frame[i] = (float)(data[at + i] * 0.5 * (1 - Math.Cos(2 * Math.PI * (i + 0.5) / length)));
                }

                fft.Magnitudes(frame, magnitudes, real, imaginary);

                var loudest = 1;
                var total = 0.0;

                for (var b = 1; b < fft.BinCount - 1; b++)
                {
                    var p = magnitudes[b] * magnitudes[b];

                    total += p;

                    if (magnitudes[b] > magnitudes[loudest])
                    {
                        loudest = b;
                    }
                }

                purity += total > 0 ? magnitudes[loudest] * magnitudes[loudest] / total : 0;
                found.Add(Refined(magnitudes, loudest) * binHz);
            }

            if (found.Count > 0 && purity / found.Count > bestPurity)
            {
                bestPurity = purity / found.Count;
                best = found;
            }
        }

        // Clusters: a new tone wherever the sorted frequencies leave a gap wider than 8 Hz; a
        // cluster holding under 1% of the frames is the smear between two tones and is dropped.
        var sorted = best.OrderBy(f => f).ToArray();
        var clusters = new List<List<double>>();

        foreach (var hz in sorted)
        {
            if (clusters.Count == 0 || hz - clusters[^1][^1] > ClusterGapHz)
            {
                clusters.Add(new List<double>());
            }

            clusters[^1].Add(hz);
        }

        var tones = clusters
            .Where(c => c.Count >= 0.01 * sorted.Length)
            .Select(c => c[c.Count / 2])
            .ToArray();

        if (tones.Length < 2)
        {
            return (double.NaN, tones.Length);
        }

        var step = tones.Zip(tones.Skip(1), (a, b) => b - a).Min();
        var index = tones.Select(t => Math.Round((t - tones[0]) / step)).ToArray();
        var meanIndex = index.Average();
        var meanHz = tones.Average();
        var top = 0.0;
        var bottom = 0.0;

        for (var i = 0; i < tones.Length; i++)
        {
            top += (index[i] - meanIndex) * (tones[i] - meanHz);
            bottom += (index[i] - meanIndex) * (index[i] - meanIndex);
        }

        return (top / bottom, tones.Length);
    }

    private static double SymbolRate(ReadOnlySpan<float> data, int rate)
    {
        var size = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)data.Length) * 2;
        var fft = new RealFft(size);
        var squared = new float[size];
        var mean = 0.0;

        for (var i = 0; i < data.Length; i++)
        {
            mean += data[i] * (double)data[i];
        }

        mean /= data.Length;

        for (var i = 0; i < data.Length; i++)
        {
            squared[i] = (float)((data[i] * (double)data[i]) - mean);
        }

        var magnitudes = new double[fft.BinCount];

        fft.Magnitudes(squared, magnitudes, new double[size], new double[size]);

        var binHz = (double)rate / size;
        var low = (int)Math.Ceiling(10 / binHz);
        var high = (int)Math.Floor(100 / binHz);
        var best = low;

        for (var b = low; b <= high; b++)
        {
            if (magnitudes[b] > magnitudes[best])
            {
                best = b;
            }
        }

        return Refined(magnitudes, best) * binHz;
    }

    private static double OccupiedBandwidth(double[] spectrum, double binHz)
    {
        var total = spectrum.Sum();
        var running = 0.0;
        double? low = null;
        double? high = null;

        for (var b = 0; b < spectrum.Length; b++)
        {
            var before = running;

            running += spectrum[b];

            if (low is null && running >= 0.005 * total)
            {
                low = (b - 1 + ((0.005 * total) - before) / spectrum[b] + 0.5) * binHz;
            }

            if (high is null && running >= 0.995 * total)
            {
                high = (b - 1 + ((0.995 * total) - before) / spectrum[b] + 0.5) * binHz;
            }
        }

        return (high ?? double.NaN) - (low ?? double.NaN);
    }

    /// <summary>A peak's bin, refined by the parabola through it and its neighbors.</summary>
    private static double Refined(double[] values, int b)
    {
        if (b <= 0 || b >= values.Length - 1)
        {
            return b;
        }

        var a = values[b - 1];
        var c = values[b + 1];
        var d = a - (2 * values[b]) + c;

        return d == 0 ? b : b + (0.5 * (a - c) / d);
    }
}

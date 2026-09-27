using System.Globalization;
using System.Numerics;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// The generator's noise band, measured: its equivalent noise bandwidth and the
/// offset that refers its in-passband signal-to-noise to CW_SPEC.md 8.1's
/// 2500 Hz reference (work instruction 461, task 1; V-06).
/// </summary>
/// <remarks>
/// <para>**EVERY SNR THE CHANNEL LAYER STATES RESTS ON THIS NUMBER.** The generator
/// sets its tone against the RMS of the shaped band, which is a signal-to-noise in
/// the band's own bandwidth; 8.1 asks for it as if the noise were measured in
/// 2500 Hz. The band is three cascaded bandpass sections, not a brick wall, so its
/// bandwidth is measured, not taken as the 520 Hz between its named edges.</para>
/// <para>**MEASURED FROM <c>ShapedNoise</c>'s OWN OUTPUT**, rendered through
/// <see cref="CwFixtureGenerator.Generate"/> with the tone four hundred decibels
/// down, <see cref="WhatTheGeneratorMakesTests"/>' way of taking the noise alone:
/// same seed, same noise sample for sample, a tone too small to move a float. The
/// generator is not touched. The response the three biquads predict is printed
/// beside the measurement so a disagreement shows.</para>
/// <para>A printer. It asserts nothing.</para>
/// </remarks>
public sealed class TheNoiseBandIsMeasuredFact
{
    private const int Segment = 4096;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the measurement is printed.</param>
    public TheNoiseBandIsMeasuredFact(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// Prints the band's total power, its peak density, its equivalent noise
    /// bandwidth referred to the peak and to the tones the tree uses, and each
    /// offset to the 2500 Hz reference, measured and predicted.
    /// </summary>
    [Fact]
    public void TheBandsEquivalentBandwidthIsPrinted()
    {
        var i = CultureInfo.InvariantCulture;
        var rate = CwFixtureGenerator.SampleRate;

        // Two long seeded runs of band alone, about eight minutes each.
        var text = string.Join(' ', Enumerable.Repeat("CQ", 200));
        var seeds = new[] { 461001, 461002 };

        double[]? hz = null;
        double[]? density = null;
        double power = 0;
        long samples = 0;

        foreach (var seed in seeds)
        {
            var recipe = SyntheticCq.Recipe(20, -400, seed) with { Text = text };
            var (noise, _) = CwFixtureGenerator.Generate(recipe);
            var (h, d) = CwSpectrum.WelchReal(noise.Samples, rate, Segment);

            hz = h;
            density ??= new double[d.Length];

            for (var k = 0; k < d.Length; k++)
            {
                density[k] += d[k] / seeds.Length;
            }

            foreach (var v in noise.Samples)
            {
                power += (double)v * v;
            }

            samples += noise.Samples.Length;

            _output.WriteLine(string.Create(i,
                $"run | seed {seed} | {noise.Samples.Length / (double)rate:0.0} s | rms {10 * Math.Log10(noise.Samples.Sum(v => (double)v * v) / noise.Samples.Length):0.00} dBFS"));
        }

        power /= samples;

        var peakAt = 0;

        for (var k = 1; k < density!.Length; k++)
        {
            if (density[k] > density[peakAt])
            {
                peakAt = k;
            }
        }

        // The peak and the tones, each as the mean over the bins within 4 Hz, so
        // one bin's scatter does not set the figure.
        var peak = CwSpectrum.DensityNear(hz!, density, hz![peakAt], 4);
        var integrated = density.Sum() * rate / Segment;

        _output.WriteLine(string.Create(i,
            $"band | total power {10 * Math.Log10(power):0.00} dBFS (rms {Math.Sqrt(power):0.00000}) | spectrum integrates to {10 * Math.Log10(integrated):0.00} dBFS | target rms 0.02000 is {20 * Math.Log10(0.02):0.00} dBFS"));
        _output.WriteLine(string.Create(i,
            $"band | peak density at {hz[peakAt]:0.0} Hz, {10 * Math.Log10(peak):0.00} dB/Hz | -3 dB from {Edge(hz, density, peakAt, peak, -1):0} to {Edge(hz, density, peakAt, peak, +1):0} Hz | -30 dB skirt at 100 Hz {10 * Math.Log10(CwSpectrum.DensityNear(hz, density, 100, 4) / peak):0.0} dB, at 2000 Hz {10 * Math.Log10(CwSpectrum.DensityNear(hz, density, 2000, 4) / peak):0.0} dB"));

        var (predictedTotal, predictedAt) = Predicted();

        foreach (var (label, at) in new[] { ("peak", hz[peakAt]), ("600 Hz (SyntheticCq.StartingPitchHz)", 600.0), ("615 Hz (the recipe default)", 615.0) })
        {
            var n0 = label == "peak" ? peak : CwSpectrum.DensityNear(hz, density, at, 4);
            var enbw = power / n0;
            var predicted = predictedTotal / predictedAt(at);

            _output.WriteLine(string.Create(i,
                $"enbw | referred to {label} | measured {enbw:0.0} Hz, offset to 2500 Hz {-10 * Math.Log10(2500 / enbw):+0.00;-0.00} dB | predicted from the biquads {predicted:0.0} Hz, offset {-10 * Math.Log10(2500 / predicted):+0.00;-0.00} dB"));
        }

        var at615 = power / CwSpectrum.DensityNear(hz, density, 615, 4);

        foreach (var (tier, db) in new[] { ("EasyDb", CwFixtureCatalogue.EasyDb), ("WorkingDb", CwFixtureCatalogue.WorkingDb), ("EdgeDb", CwFixtureCatalogue.EdgeDb) })
        {
            _output.WriteLine(string.Create(i,
                $"relabel | {tier} {db:0.0} dB in the passband at 615 Hz | {db - (10 * Math.Log10(2500 / at615)):0.0} dB reference"));
        }
    }

    // Where the density first falls three decibels below the peak, walking out.
    private static double Edge(double[] hz, double[] density, int from, double peak, int way)
    {
        var k = from;

        while (k > 0 && k < density.Length - 1 && density[k] > peak / 2)
        {
            k += way;
        }

        return hz[k];
    }

    // The band as the generator's three biquads and its skirt predict it: the
    // shaped white noise normalised to its expected RMS, the skirt added
    // coherently, both one-sided over nought to 4000 Hz.
    private static (double Total, Func<double, double> At) Predicted()
    {
        var rate = CwFixtureGenerator.SampleRate;
        var centre = Math.Sqrt(CwFixtureGenerator.PassbandLowHz * CwFixtureGenerator.PassbandHighHz);
        var q = centre / (CwFixtureGenerator.PassbandHighHz - CwFixtureGenerator.PassbandLowHz);
        var w0 = 2 * Math.PI * centre / rate;
        var alpha = Math.Sin(w0) / (2 * q);

        Complex H(double f)
        {
            var z1 = Complex.FromPolarCoordinates(1, -2 * Math.PI * f / rate);
            var one = (alpha - (alpha * z1 * z1)) / ((1 + alpha) - (2 * Math.Cos(w0) * z1) + ((1 - alpha) * z1 * z1));
            return one * one * one;
        }

        const double Step = 0.05;
        var nyquist = rate / 2.0;
        double shaped = 0;

        for (var f = Step / 2; f < nyquist; f += Step)
        {
            shaped += Math.Pow(H(f).Magnitude, 2) * Step / nyquist;
        }

        const double Target = 0.02;
        var g = Target / Math.Sqrt(shaped);
        var s = Target * Math.Pow(10, -CwFixtureGenerator.OutOfBandDropDb / 20);

        double Density(double f) => Math.Pow(((g * H(f)) + s).Magnitude, 2) / nyquist;

        double total = 0;

        for (var f = Step / 2; f < nyquist; f += Step)
        {
            total += Density(f) * Step;
        }

        return (total, Density);
    }
}

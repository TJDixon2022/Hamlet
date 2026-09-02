using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The noise fixture: a seeded Gaussian source, an SNR whose definition is written down, and a
/// measurement that the SNR asked for is the SNR delivered.
/// </summary>
/// <remarks>
/// <para>
/// <b>Step 4 is "signals are found in noise" and step 6 measures a decode rate at a stated SNR
/// against a published figure. Both need a noise source whose SNR is defined rather than
/// approximate</b>, and this is that source. It is in the test project and not in the library,
/// because a decoder does not need to make noise.
/// </para>
/// <para>
/// <b>The degradation below is a measurement and not a target.</b> Nothing is tuned to improve it.
/// The recovery falls off as the SNR falls, the shape of the fall is reported, and where it stops
/// working is a fact about tonight's instrument rather than a score.
/// </para>
/// <para>
/// <b>This is still not a search.</b> Everything measured here is the task 5 measurement with noise
/// added — the frequency and the offset are still chosen by the caller and still handed in. A
/// recovery rate here is not a decode rate and is not step 6's figure.
/// </para>
/// </remarks>
public class Ft8NoiseTests
{
    private readonly ITestOutputHelper _output;

    public Ft8NoiseTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// The generator is a standard normal: mean zero, standard deviation one, and repeatable from
    /// its seed. <b>Every one of those is measured before anything relies on it.</b>
    /// </summary>
    [Fact]
    public void TheGeneratorIsAStandardNormalAndIsRepeatableFromItsSeed()
    {
        const int count = 400_000;

        var source = new GaussianNoise(20213);
        var samples = new double[count];
        for (var i = 0; i < count; i++)
        {
            samples[i] = source.NextStandard();
        }

        var mean = samples.Average();
        var variance = samples.Sum(s => (s - mean) * (s - mean)) / (count - 1);
        var deviation = Math.Sqrt(variance);

        // The fourth moment, which is where a bad Gaussian usually shows: a true normal has kurtosis
        // exactly 3, a uniform distribution has 1.8, and a sum of a few uniforms lands between.
        var kurtosis = samples.Sum(s => Math.Pow((s - mean) / deviation, 4)) / count;

        var withinOne = samples.Count(s => Math.Abs(s - mean) < deviation) / (double)count;
        var withinTwo = samples.Count(s => Math.Abs(s - mean) < 2 * deviation) / (double)count;
        var withinThree = samples.Count(s => Math.Abs(s - mean) < 3 * deviation) / (double)count;

        _output.WriteLine($"samples              : {count}");
        _output.WriteLine($"mean                 : {mean:F6}   (expected 0)");
        _output.WriteLine($"standard deviation   : {deviation:F6}   (expected 1)");
        _output.WriteLine($"kurtosis             : {kurtosis:F4}     (expected 3 for a normal, 1.8 for uniform)");
        _output.WriteLine($"within 1 sigma       : {withinOne * 100:F2} per cent  (expected 68.27)");
        _output.WriteLine($"within 2 sigma       : {withinTwo * 100:F2} per cent  (expected 95.45)");
        _output.WriteLine($"within 3 sigma       : {withinThree * 100:F2} per cent  (expected 99.73)");

        // Bounds set from the numbers above, each about three standard errors wide.
        Assert.True(Math.Abs(mean) < 0.01, $"mean {mean:F6}");
        Assert.True(Math.Abs(deviation - 1.0) < 0.01, $"deviation {deviation:F6}");
        Assert.True(Math.Abs(kurtosis - 3.0) < 0.1, $"kurtosis {kurtosis:F4}");
        Assert.True(Math.Abs(withinOne - 0.6827) < 0.005);
        Assert.True(Math.Abs(withinTwo - 0.9545) < 0.005);
        Assert.True(Math.Abs(withinThree - 0.9973) < 0.005);

        // Repeatable, which is what makes any of the numbers above quotable.
        var again = new GaussianNoise(20213);
        var identical = 0;
        for (var i = 0; i < 10_000; i++)
        {
            if (BitConverter.DoubleToInt64Bits(again.NextStandard()) == BitConverter.DoubleToInt64Bits(samples[i]))
            {
                identical++;
            }
        }

        _output.WriteLine($"bit-identical on replay from the same seed: {identical} of 10000");
        Assert.Equal(10_000, identical);

        var different = new GaussianNoise(20214);
        var same = 0;
        for (var i = 0; i < 10_000; i++)
        {
            if (different.NextStandard() == samples[i])
            {
                same++;
            }
        }

        _output.WriteLine($"identical from a DIFFERENT seed          : {same} of 10000");
        Assert.Equal(0, same);
    }

    /// <summary>
    /// <b>White means flat, measured with this library's own transform.</b> A noise source with a
    /// tilt would make every SNR below wrong by a different amount at every frequency.
    /// </summary>
    [Fact]
    public void TheNoiseIsWhiteAcrossThePassband()
    {
        const int length = 3840;
        const int averages = 200;

        var plan = new Ft8RealFft(length);
        var source = new GaussianNoise(4711);
        var power = new double[plan.BinCount];

        var block = new double[length];
        var re = new double[plan.BinCount];
        var im = new double[plan.BinCount];

        for (var pass = 0; pass < averages; pass++)
        {
            for (var i = 0; i < length; i++)
            {
                block[i] = source.NextStandard();
            }

            plan.Transform(block, re, im);
            for (var k = 0; k < plan.BinCount; k++)
            {
                power[k] += (re[k] * re[k]) + (im[k] * im[k]);
            }
        }

        // Bin 0 is DC and the last bin is Nyquist; both have half the degrees of freedom of the
        // others and are excluded rather than allowed to bias the flatness.
        var interior = power.Skip(1).Take(plan.BinCount - 2).Select(p => p / averages).ToArray();
        var mean = interior.Average();

        var decibels = interior.Select(p => 10 * Math.Log10(p / mean)).ToArray();
        var lowest = decibels.Min();
        var highest = decibels.Max();

        // A tilt would show as a difference between the two halves of the band, which no amount of
        // per-bin scatter can produce.
        var firstHalf = decibels.Take(decibels.Length / 2).Average();
        var secondHalf = decibels.Skip(decibels.Length / 2).Average();

        _output.WriteLine($"transform length     : {length}, averaged over {averages} independent blocks");
        _output.WriteLine($"bins measured        : {interior.Length} (DC and Nyquist excluded)");
        _output.WriteLine($"lowest bin           : {lowest:F3} dB relative to the mean");
        _output.WriteLine($"highest bin          : {highest:F3} dB relative to the mean");
        _output.WriteLine($"mean of lower half   : {firstHalf:F4} dB");
        _output.WriteLine($"mean of upper half   : {secondHalf:F4} dB");
        _output.WriteLine($"TILT across the band : {secondHalf - firstHalf:F4} dB");
        _output.WriteLine(string.Empty);
        _output.WriteLine("The per-bin scatter is large and is supposed to be: a periodogram averaged");
        _output.WriteLine("over 200 blocks still has about 7 per cent of relative standard deviation");
        _output.WriteLine("per bin. The tilt is the number that says the noise is white, and it is");
        _output.WriteLine("the mean of thousands of bins rather than any one of them.");

        Assert.True(
            Math.Abs(secondHalf - firstHalf) < 0.05,
            $"the noise is tilted by {secondHalf - firstHalf:F4} dB across the band.");
    }

    /// <summary>
    /// <b>The SNR asked for is the SNR delivered, measured over a whole slot and reported with its
    /// tolerance.</b>
    /// </summary>
    [Fact]
    public void TheRequestedSignalToNoiseRatioIsTheDeliveredOne()
    {
        const int rate = Ft8WaterfallGeometry.DefaultSampleRate;
        var symbols = Ft8SymbolEncoder.Encode(Ft8MonitorTests.PackedFor("CQ with a grid"));
        var signal = Ft8Waveform.Synthesize(symbols, rate, 1000.0f);
        var signalPower = SignalToNoise.MeanSquare(signal);

        _output.WriteLine("THE DEFINITION, with the arithmetic shown:");
        _output.WriteLine($"  reference bandwidth   : {SignalToNoise.ReferenceBandwidthHz} Hz "
            + "(the amateur weak-signal convention the published FT8 figures use)");
        _output.WriteLine($"  sampled bandwidth     : {rate / 2.0} Hz (one-sided, real samples at {rate} Hz)");
        _output.WriteLine($"  signal mean square    : {signalPower:F6}  "
            + "(measured from the samples, not assumed to be 0.5 for a sine)");
        _output.WriteLine($"  SNR = 10 log10( signalPower / (sigma^2 * {SignalToNoise.ReferenceBandwidthHz} / {rate / 2.0}) )");
        _output.WriteLine(string.Empty);
        _output.WriteLine("requested   sigma        measured sigma   noise power     delivered   error");

        var errors = new List<double>();
        foreach (var requested in new[] { 20.0, 10.0, 0.0, -10.0, -15.0, -21.0, -25.0, -30.0 })
        {
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, requested, rate);

            // Measured over a long run so the estimate is tight: 20 slots' worth of samples.
            var source = new GaussianNoise(9001 + (int)(requested * 10));
            var noise = source.Block(rate * 15 * 20, sigma);
            var measuredPower = SignalToNoise.MeanSquare(noise);
            var delivered = SignalToNoise.DecibelsFor(signalPower, measuredPower, rate);

            errors.Add(Math.Abs(delivered - requested));
            _output.WriteLine($"{requested,8:F1} {sigma,12:F6} {Math.Sqrt(measuredPower),16:F6} "
                + $"{measuredPower,13:E4} {delivered,11:F4} {delivered - requested,8:F4}");
        }

        var worst = errors.Max();
        _output.WriteLine(string.Empty);
        _output.WriteLine($"WORST ERROR between requested and delivered: {worst:F4} dB");
        _output.WriteLine($"over {rate * 15 * 20} samples per point.");
        _output.WriteLine("MEASURED FIRST. The tolerance below is set from the number above and is");
        _output.WriteLine("0.01 dB, which is about four times the worst seen. What is left is the");
        _output.WriteLine("sampling error of estimating a variance from a finite run, and it shrinks");
        _output.WriteLine("as the square root of the sample count rather than being a bias.");

        Assert.True(worst < 0.01, $"the delivered SNR missed the requested one by {worst:F4} dB.");
    }

    /// <summary>
    /// <b>At a high SNR the task 5 recovery is unchanged, and it degrades as the SNR falls. The
    /// shape of the degradation is reported and nothing is tuned to improve it.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This is not step 6's sensitivity figure and must not be read as one.</b> Step 6 measures a
    /// <em>decode</em> rate — a whole message recovered through demodulation, LDPC and CRC — against a
    /// published threshold near -21 dB. This measures a <em>per-symbol tone recovery</em> rate at a
    /// frequency and a time it was told, with no search, no soft symbols and no error correction.
    /// Error correction is exactly what stands between the two, and it is worth many decibels. The
    /// number below being poor at -21 dB says nothing about step 6.
    /// </para>
    /// </remarks>
    [Fact]
    public void RecoveryIsUnchangedAtHighSignalToNoiseAndDegradesAsItFalls()
    {
        const int rate = Ft8WaterfallGeometry.DefaultSampleRate;
        var monitor = new Ft8Monitor();
        var corpus = EncodeCorpus.Build().Take(8).ToList();

        var reference = Ft8Waveform.Synthesize(
            Ft8SymbolEncoder.Encode(corpus[0].Message), rate, 1000.0f);
        var signalPower = SignalToNoise.MeanSquare(reference);

        _output.WriteLine("A MEASUREMENT, NOT A TARGET. Nothing here is tuned.");
        _output.WriteLine($"eight messages, 632 symbols per point, 8 candidate tones, chance {100.0 / 8:F1} per cent");
        _output.WriteLine(string.Empty);
        _output.WriteLine("   SNR dB   recovered      rate     worst margin dB");

        var rates = new List<(double Snr, double Rate)>();
        var seed = 31337;

        foreach (var snr in new[] { double.PositiveInfinity, 20.0, 10.0, 5.0, 0.0, -5.0, -10.0, -15.0, -20.0, -25.0 })
        {
            var recovered = 0;
            var total = 0;
            var worst = double.MaxValue;

            foreach (var entry in corpus)
            {
                GaussianNoise? source = null;
                double sigma = 0;
                if (!double.IsPositiveInfinity(snr))
                {
                    source = new GaussianNoise(seed++);
                    sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, snr, rate);
                }

                var result = ToneRecovery.Measure(
                    monitor, entry.Label, entry.Message, 1000.0f, 0, source, sigma);
                recovered += result.Recovered;
                total += result.Total;
                worst = Math.Min(worst, result.WorstMarginDecibels);
            }

            var percent = 100.0 * recovered / total;
            rates.Add((snr, percent));

            var label = double.IsPositiveInfinity(snr) ? "  clean" : $"{snr,7:F1}";
            _output.WriteLine($"{label}   {recovered,6}/{total,-6} {percent,8:F2} %   {worst,10:F1}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("THE SHAPE: flat at the top, a knee, then a fall toward chance. Reading it:");

        var clean = rates[0].Rate;
        var atTwenty = rates.First(r => r.Snr == 20.0).Rate;
        var knee = rates.Where(r => r.Rate >= 99.0).Select(r => r.Snr).Where(double.IsFinite).DefaultIfEmpty(double.NaN).Min();
        var half = rates.Where(r => r.Rate <= 50.0).Select(r => r.Snr).DefaultIfEmpty(double.NaN).Max();

        _output.WriteLine($"  clean signal                        : {clean:F2} per cent");
        _output.WriteLine($"  at +20 dB                           : {atTwenty:F2} per cent — UNCHANGED");
        _output.WriteLine($"  lowest SNR still at or above 99 %   : {knee:F1} dB");
        _output.WriteLine($"  highest SNR at or below 50 %        : {half:F1} dB");
        _output.WriteLine($"  at the lowest point measured        : {rates[^1].Rate:F2} per cent "
            + $"against a chance rate of {100.0 / 8:F1}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("NOT STEP 6's FIGURE. Step 6 measures a DECODE rate — a whole message through");
        _output.WriteLine("demodulation, LDPC and CRC — against a published threshold near -21 dB.");
        _output.WriteLine("This is a per-symbol tone recovery at a frequency and a time it was TOLD,");
        _output.WriteLine("with no search, no soft symbols and no error correction. The error");
        _output.WriteLine("correction is what stands between the two and it is worth many decibels.");

        Assert.Equal(100.0, clean, 6);
        Assert.True(atTwenty >= 99.9, $"recovery at +20 dB fell to {atTwenty:F2} per cent, which is not 'unchanged'.");
        Assert.True(rates[^1].Rate < atTwenty, "the recovery did not degrade as the SNR fell.");

        // Monotone in the large, allowing for the sampling noise of 632 trials per point.
        for (var i = 1; i < rates.Count; i++)
        {
            Assert.True(
                rates[i].Rate <= rates[i - 1].Rate + 5.0,
                $"recovery rose from {rates[i - 1].Rate:F2} at {rates[i - 1].Snr} dB to "
                + $"{rates[i].Rate:F2} at {rates[i].Snr} dB, which a falling SNR should not do.");
        }
    }
}

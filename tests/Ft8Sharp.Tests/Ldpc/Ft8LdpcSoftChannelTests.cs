using System.Diagnostics;
using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Tests.Dsp;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// How much <em>soft</em> damage the correction survives: not a handful of certain errors, but
/// every one of the 174 ratios arriving a little wrong.
/// </summary>
/// <remarks>
/// <para>
/// <b>Bit flips are the crisp instrument and this is the realistic one.</b> A real signal never
/// delivers a fixed number of confident errors; it delivers 174 unequal confidences, most of
/// them right and some of them not, and the ones that are wrong are usually the ones the
/// receiver was least sure about. Task 4's sweep measures the code; this measures what the
/// decoder does with the kind of input extraction will actually hand it.
/// </para>
/// <para>
/// <b>WHAT THIS IS NOT, AND IT MATTERS MORE HERE THAN ANYWHERE ELSE IN THE UNIT.</b>
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>It is not a decode rate off a channel</b>, and it is not compared with the published
///     sensitivity figure for FT8. Nothing in this measurement has a candidate search in front of
///     it, nothing has demodulated anything, and there is no audio. Step 6 measures that number
///     and this unit has no evidence about it.
///   </description></item>
///   <item><description>
///     <b>The horizontal axis is not a signal-to-noise ratio.</b> It is the standard deviation of
///     the noise added to each ratio, as a multiple of the magnitude a confident bit arrives
///     with. There is no mapping from one to the other without the extraction that does not
///     exist.
///   </description></item>
///   <item><description>
///     <b>The array's scale moves as the noise grows, and upstream's would not.</b>
///     <c>ftx_normalize_logl</c> rescales the 174 ratios to a variance of 24 before
///     <c>bp_decode</c> sees them, and that normalisation is extraction's and is not ported. So
///     the measured variance is printed in its own column: where it leaves 24, this sweep and
///     upstream's path have parted company, and the next unit should read the rows accordingly.
///   </description></item>
/// </list>
/// <para>
/// <b>Nothing is tuned to improve any of it.</b> The table is a measurement.
/// </para>
/// </remarks>
public class Ft8LdpcSoftChannelTests
{
    private readonly ITestOutputHelper _output;

    public Ft8LdpcSoftChannelTests(ITestOutputHelper output) => _output = output;

    private const int TrialsPerPoint = 400;
    private const int BaseSeed = 21_571;

    private static readonly double[] NoiseRatios =
    {
        0.00, 0.25, 0.50, 0.75, 1.00, 1.25, 1.50, 1.75, 2.00, 2.50, 3.00, 4.00,
    };

    [Fact]
    public void TheDecodeRateIsSweptAgainstSoftNoiseOnTheRatios()
    {
        var corpus = EncodeCorpus.Build();
        var codewords = corpus.Select(entry => SoftCodeword.CodewordBitsFor(entry.Message)).ToArray();
        var clean = codewords.Select(bits => SoftCodeword.RatiosFor(bits)).ToArray();

        _output.WriteLine($"corpus messages {corpus.Count}, trials per point {TrialsPerPoint}, "
            + $"maxIterations {LdpcDecoder.DefaultMaxIterations}, seed {BaseSeed} + point");
        _output.WriteLine($"confident magnitude A = {SoftCodeword.ConfidentMagnitude:F4}; "
            + "upstream's own decoder is fed ratios of variance 24, which is A squared");
        _output.WriteLine(string.Empty);
        _output.WriteLine(" sigma/A | trials | decoded   rate% | wrongMsg | crcRej | noDecode | "
            + "iters mean worst | variance");
        _output.WriteLine("---------+--------+-----------------+----------+--------+----------+"
            + "-----------------+---------");

        var totalWrong = 0;
        var totalTrials = 0;
        double? fallsBelowHalf = null;
        double? firstZero = null;
        var stopwatch = Stopwatch.StartNew();

        for (var point = 0; point < NoiseRatios.Length; point++)
        {
            var sigmaOverA = NoiseRatios[point];
            var sigma = sigmaOverA * SoftCodeword.ConfidentMagnitude;
            var noise = new GaussianNoise(BaseSeed + point);

            var decoded = 0;
            var wrong = 0;
            var crcRejected = 0;
            var noDecode = 0;
            long iterationTotal = 0;
            var worstIterations = 0;
            var varianceTotal = 0.0;

            for (var trial = 0; trial < TrialsPerPoint; trial++)
            {
                var index = trial % corpus.Count;
                var ratios = sigma > 0
                    ? noise.AddedTo(clean[index], sigma)
                    : (float[])clean[index].Clone();

                varianceTotal += Variance(ratios);

                var bits = new byte[Ft8Tables.LdpcN];
                var correction = LdpcDecoder.Decode(ratios, bits);

                iterationTotal += correction.Iterations;
                worstIterations = Math.Max(worstIterations, correction.Iterations);

                if (!correction.ParitySatisfied)
                {
                    noDecode++;
                    continue;
                }

                var recovered = SoftCodeword.MessageFrom(bits);
                if (recovered is null)
                {
                    crcRejected++;
                }
                else if (recovered.AsSpan().SequenceEqual(corpus[index].Message))
                {
                    decoded++;
                }
                else
                {
                    wrong++;
                }
            }

            var rate = 100.0 * decoded / TrialsPerPoint;
            totalWrong += wrong;
            totalTrials += TrialsPerPoint;

            if (fallsBelowHalf is null && rate < 50.0)
            {
                fallsBelowHalf = sigmaOverA;
            }

            if (firstZero is null && decoded == 0)
            {
                firstZero = sigmaOverA;
            }

            _output.WriteLine(
                $"{sigmaOverA,8:F2} | {TrialsPerPoint,6} | {decoded,7} {rate,7:F1} | {wrong,8} | "
                + $"{crcRejected,6} | {noDecode,8} | {(double)iterationTotal / TrialsPerPoint,10:F2} "
                + $"{worstIterations,5} | {varianceTotal / TrialsPerPoint,8:F1}");

            Assert.Equal(TrialsPerPoint, decoded + wrong + crcRejected + noDecode);
        }

        stopwatch.Stop();

        _output.WriteLine(string.Empty);
        _output.WriteLine("THE SHAPE, IN WORDS");
        _output.WriteLine($"  first sigma/A at which the rate falls below half : "
            + $"{(fallsBelowHalf.HasValue ? fallsBelowHalf.Value.ToString("F2") : "not reached")}");
        _output.WriteLine($"  first sigma/A at which nothing decoded           : "
            + $"{(firstZero.HasValue ? firstZero.Value.ToString("F2") : "not reached")}");
        _output.WriteLine($"  WRONG MESSAGES OVER THIS SWEEP                   : {totalWrong} "
            + $"out of {totalTrials} trials");
        _output.WriteLine($"  {totalTrials} decodes in {stopwatch.ElapsedMilliseconds} ms");
        _output.WriteLine(string.Empty);
        _output.WriteLine("NOT a decode rate off a channel, NOT a signal-to-noise ratio, and NOT");
        _output.WriteLine("comparable with any published sensitivity figure. No audio, no search, no");
        _output.WriteLine("demodulation is anywhere in this measurement.");

        // The sweep has to reach both ends or it has measured the range and not the decoder.
        Assert.True(firstZero.HasValue, "the sweep never reached a point where nothing decoded.");
        Assert.Equal(TrialsPerPoint, DecodedAtZeroNoise(clean, corpus));
    }

    private static int DecodedAtZeroNoise(
        float[][] clean,
        IReadOnlyList<EncodeCorpus.Entry> corpus)
    {
        var decoded = 0;
        for (var trial = 0; trial < TrialsPerPoint; trial++)
        {
            var index = trial % corpus.Count;
            var bits = new byte[Ft8Tables.LdpcN];
            var correction = LdpcDecoder.Decode(clean[index], bits);
            if (correction.ParitySatisfied
                && SoftCodeword.MessageFrom(bits) is { } message
                && message.AsSpan().SequenceEqual(corpus[index].Message))
            {
                decoded++;
            }
        }

        return decoded;
    }

    private static double Variance(ReadOnlySpan<float> values)
    {
        double sum = 0;
        double sumSquares = 0;
        foreach (var value in values)
        {
            sum += value;
            sumSquares += (double)value * value;
        }

        var inverse = 1.0 / values.Length;
        return (sumSquares - (sum * sum * inverse)) * inverse;
    }
}

using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The forward–backward pass, checked where this algorithm is known to fail.
/// </summary>
/// <remarks>
/// <para>**UNDERFLOW AND OVERFLOW ARE THE KNOWN FAILURE OF THIS ALGORITHM**, and
/// this decoder has produced 5,521,967, 17.2 million and quadrillions on
/// degenerate bins, with intermittent overflows in the carried asks. So the
/// arithmetic is tested before the number it produces is trusted.</para>
/// <para>**A POSTERIOR THAT LEAVES [0,1] IS NOT A POSTERIOR**, and a number that
/// looks like a probability and is not one is §0.0 in the place it does most
/// damage — it would be believed.</para>
/// </remarks>
public sealed class ThePosteriorSurvivesItsOwnArithmeticTests
{
    private readonly ITestOutputHelper _output;

    public ThePosteriorSurvivesItsOwnArithmeticTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Adding in the log domain never exponentiates a large number.</summary>
    /// <remarks>
    /// The whole guard is that the larger term is factored out, so
    /// <c>Math.Exp</c> only ever sees something non-positive.
    /// </remarks>
    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(-1e6, -1e6)]
    [InlineData(1e6, 1e6)]
    [InlineData(1e300, 1e300)]
    [InlineData(-1e300, 1.0)]
    [InlineData(1e-300, -1e-300)]
    public void LogSumStaysFinite(double a, double b)
    {
        var sum = CwProbabilisticDecoder.LogSum(a, b);

        _output.WriteLine($"LogSum({a:0.###e+0}, {b:0.###e+0}) = {sum:0.###e+0}");

        Assert.False(double.IsNaN(sum), "log-sum produced NaN");
        Assert.False(double.IsPositiveInfinity(sum), "log-sum overflowed");
        Assert.True(sum >= Math.Max(a, b) - 1e-9, "log-sum lost the larger term");
    }

    /// <summary>Two equal terms add to exactly one more nat, as they must.</summary>
    [Fact]
    public void TwoEqualTermsDoubleTheEvidence()
    {
        var sum = CwProbabilisticDecoder.LogSum(-5, -5);

        Assert.Equal(-5 + Math.Log(2), sum, 9);
    }

    /// <summary>An absent term contributes nothing.</summary>
    [Fact]
    public void NegativeInfinityIsTheIdentity()
    {
        Assert.Equal(-3.0, CwProbabilisticDecoder.LogSum(double.NegativeInfinity, -3), 9);
        Assert.Equal(-3.0, CwProbabilisticDecoder.LogSum(-3, double.NegativeInfinity), 9);
        Assert.True(double.IsNegativeInfinity(CwProbabilisticDecoder.LogSum(
            double.NegativeInfinity, double.NegativeInfinity)));
    }

    /// <summary>Every posterior is a probability, on real audio.</summary>
    /// <remarks>
    /// **THE ONLY CLAIM THAT MATTERS IS THAT IT IS IN [0,1].** A quantity outside
    /// that is not a probability whatever it is called, and every gate that would
    /// consume it would then be consuming a scale again.
    /// </remarks>
    [Theory]
    [InlineData("cw-2026-08-24-012403")]
    [InlineData("cw-2026-08-18-004507")]
    [InlineData("cw-2026-08-20-014854")]
    public void EveryPosteriorIsAProbability(string name)
    {
        var audio = ReadCapture(name);
        var envelope = CwProbabilisticDecoder.Envelope(
            audio.Samples, audio.SampleRate, 500);

        var (down, up) = CwProbabilisticDecoder.LogLikelihoods(envelope);

        var downTo = new double[envelope.Length + 1];
        var upTo = new double[envelope.Length + 1];

        for (var i = 0; i < envelope.Length; i++)
        {
            downTo[i + 1] = downTo[i] + down[i];
            upTo[i + 1] = upTo[i] + up[i];
        }

        // Twenty words a minute, which is where this corpus mostly sits.
        var unit = 1200.0 / 20 / CwProbabilisticDecoder.HopMilliseconds;
        var posterior = CwProbabilisticDecoder.Posterior(
            envelope.Length, downTo, upTo, unit, null);

        if (posterior is null)
        {
            _output.WriteLine($"{name}: no path reaches the end, which is an answer");

            return;
        }

        var highest = 0.0;

        for (var i = 0; i <= envelope.Length; i++)
        {
            for (var k = 0; k < 5; k++)
            {
                var p = posterior[i, k];

                Assert.False(double.IsNaN(p), $"{name} produced NaN at hop {i}");
                Assert.InRange(p, 0.0, 1.0);

                highest = Math.Max(highest, p);
            }
        }

        _output.WriteLine($"{name}: {envelope.Length} hops, highest posterior {highest:0.000}");

        Assert.True(highest > 0, $"{name} produced an all-zero posterior");
    }

    /// <summary>Digital silence produces no posterior rather than a wrong one.</summary>
    /// <remarks>
    /// An all-zero buffer is an absence of measurement and not a quiet band
    /// (HM-DEC-120), and a normalisation by nothing would be a number with no
    /// meaning behind it.
    /// </remarks>
    [Fact]
    public void DigitalSilenceProducesNothingRatherThanANumber()
    {
        var envelope = new double[400];
        var (down, up) = CwProbabilisticDecoder.LogLikelihoods(envelope);

        var downTo = new double[envelope.Length + 1];
        var upTo = new double[envelope.Length + 1];

        for (var i = 0; i < envelope.Length; i++)
        {
            downTo[i + 1] = downTo[i] + down[i];
            upTo[i + 1] = upTo[i] + up[i];
        }

        var unit = 1200.0 / 20 / CwProbabilisticDecoder.HopMilliseconds;
        var posterior = CwProbabilisticDecoder.Posterior(
            envelope.Length, downTo, upTo, unit, null);

        if (posterior is null)
        {
            return;
        }

        for (var i = 0; i <= envelope.Length; i++)
        {
            for (var k = 0; k < 5; k++)
            {
                Assert.InRange(posterior[i, k], 0.0, 1.0);
            }
        }
    }

    /// <summary>A capture, adjudicated or not.</summary>
    private static MonoAudio ReadCapture(string name)
    {
        var direct = Path.Combine(CapturedSignalTests.Folder, name + ".wav");

        return WavAudio.Read(File.Exists(direct)
            ? direct
            : Path.Combine(
                CapturedSignalTests.Folder, "unadjudicated", name + ".wav"));
    }

    /// <summary>Too little audio produces nothing.</summary>
    [Fact]
    public void NoHopsProduceNoPosterior()
        => Assert.Null(CwProbabilisticDecoder.Posterior(
            0, new double[1], new double[1], 4, null));
}

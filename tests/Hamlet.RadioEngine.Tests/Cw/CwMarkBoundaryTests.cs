using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The dit-or-dah boundary is fitted, not multiplied (HM-DEC-112 part 3).
/// </summary>
/// <remarks>
/// <para>**THE SAME ARGUMENT HM-DEC-115 MADE ABOUT GAPS, ONE LEVEL UP.** A
/// boundary read off a multiple of the dit rather than off the marks themselves
/// moves only when the dit moves, and the moment anything changes how the dit is
/// measured the boundary is left behind. That is not hypothetical: correcting
/// the mark and the gap while leaving this at two dits took the suite from nine
/// failures to twenty-three.</para>
/// <para>Fitted per signal rather than per window and seeded on percentiles
/// rather than on the extremes, which are the two findings that made the gap fit
/// work and which marks want for the same reasons.</para>
/// </remarks>
public sealed class CwMarkBoundaryTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the boundary is printed.</param>
    public CwMarkBoundaryTests(ITestOutputHelper output) => _output = output;

    private static CwSpeedEstimator Fed(double ditSamples, int dits, int dahs)
    {
        var speed = new CwSpeedEstimator(8_000);

        for (var i = 0; i < Math.Max(dits, dahs); i++)
        {
            if (i < dits)
            {
                speed.AddMark(ditSamples);
                speed.AddGap(ditSamples);
            }

            if (i < dahs)
            {
                speed.AddMark(ditSamples * 3);
                speed.AddGap(ditSamples);
            }
        }

        return speed;
    }

    /// <remarks>
    /// <para>Proves the boundary lands between the two clusters rather than on a
    /// multiple of one. A textbook fist puts it near the geometric mean of one
    /// and three dits, which is about 1.73 dits and not 2.</para>
    /// </remarks>
    [Fact]
    public void TheBoundarySitsBetweenTheMeasuredClusters()
    {
        var speed = Fed(ditSamples: 400, dits: 40, dahs: 20);

        _output.WriteLine($"dit {speed.DitSamples:0} samples, "
            + $"boundary {speed.MarkBoundary:0} = "
            + $"{speed.MarkBoundary / speed.DitSamples:0.00} dits");

        Assert.True(speed.MarkBoundary > 400, "the boundary is at or below a dit");
        Assert.True(speed.MarkBoundary < 1200, "the boundary is at or above a dah");

        // And it separates them: every dit below, every dah above.
        Assert.Equal(CwElement.Dit, speed.ClassifyMark(400));
        Assert.Equal(CwElement.Dah, speed.ClassifyMark(1200));
    }

    /// <remarks>
    /// <para>**PROVES IT IS FITTED RATHER THAN MULTIPLIED, WHICH IS THE WHOLE
    /// POINT.** A tight fist sending dahs at two and a half dits rather than
    /// three still separates cleanly, and a boundary at two dits would sit far
    /// closer to its dahs than the data warrants.</para>
    /// </remarks>
    [Fact]
    public void ATightFistIsStillSeparatedCorrectly()
    {
        var speed = new CwSpeedEstimator(8_000);

        for (var i = 0; i < 40; i++)
        {
            speed.AddMark(400);
            speed.AddGap(400);
            speed.AddMark(1000);
            speed.AddGap(400);
        }

        _output.WriteLine($"boundary {speed.MarkBoundary:0} samples against a "
            + $"dit of 400 and a dah of 1000");

        Assert.Equal(CwElement.Dit, speed.ClassifyMark(400));
        Assert.Equal(CwElement.Dah, speed.ClassifyMark(1000));
    }

    /// <remarks>
    /// <para>Proves §0.0: **before there is anything to fit, the multiple is the
    /// honest guess.** A sender who has only sent dits has no dah to find, and
    /// inventing a boundary from one cluster would be inventing a dah. Two dits
    /// is what this used to do always, and it is right for exactly this window.
    /// </para>
    /// </remarks>
    [Fact]
    public void BeforeThereIsAFitTheMultipleIsUsed()
    {
        var speed = new CwSpeedEstimator(8_000);

        for (var i = 0; i < 6; i++)
        {
            speed.AddMark(400);
            speed.AddGap(400);
        }

        _output.WriteLine($"with only dits: boundary {speed.MarkBoundary:0}, "
            + $"two dits is {2 * speed.DitSamples:0}");

        Assert.Equal(2 * speed.DitSamples, speed.MarkBoundary);
    }

    /// <remarks>
    /// Proves the clusters belong to a sender: a retune forgets them, because
    /// two fists averaged together describe neither (HM-DEC-095).
    /// </remarks>
    [Fact]
    public void ARetuneForgetsTheClusters()
    {
        var speed = Fed(ditSamples: 400, dits: 40, dahs: 20);

        Assert.NotEqual(2 * speed.DitSamples, speed.MarkBoundary);

        speed.Forget();

        for (var i = 0; i < 6; i++)
        {
            speed.AddMark(400);
            speed.AddGap(400);
        }

        Assert.Equal(2 * speed.DitSamples, speed.MarkBoundary);
    }
}

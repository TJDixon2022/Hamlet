using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The third confidence measurement (HM-DEC-108).
/// </summary>
/// <remarks>
/// <para>**THE TWO EXISTING SCORES ARE BOTH ABOUT THE ELEMENTS.** Timing clarity
/// asks how far each mark sat from the dit-or-dah decision; signal margin asks
/// how far the weakest part stood above the noise. Neither can see a character
/// that was divided in the wrong place, because the elements of a lone dah are
/// perfect and the timing margin of a dah that really is a dah is one.</para>
/// <para>So the missing measurement is of the boundary rather than of the
/// elements, and it obeys the same two rules as the other two: the worst of them
/// wins, and nothing anywhere may raise a score (§0.0, HM-DEC-048).</para>
/// </remarks>
public sealed class CwBoundaryMarginTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the margins are printed.</param>
    public CwBoundaryMarginTests(ITestOutputHelper output) => _output = output;

    /// <summary>A hundred millisecond dit, so the cut sits at about 173.</summary>
    private const double Cut = 173.2;

    /// <remarks>
    /// <para>Proves HM-DEC-108: **a gap that landed on the boundary scores
    /// nothing.** That is the case the measurement exists for. Whether the
    /// character ended there was a coin toss, and a character produced by a coin
    /// toss may not be shown as read however clean its elements were.</para>
    /// </remarks>
    [Fact]
    public void AGapOnTheBoundaryIsWorthNothing()
    {
        var margin = CwSettledPass.BoundaryMargin(Cut, Cut);

        _output.WriteLine($"a gap exactly on the cut scores {margin:0.000}");

        Assert.Equal(0, margin, 3);
    }

    /// <remarks>
    /// Proves HM-DEC-108: **a gap on either textbook spacing scores full marks.**
    /// One dit inside a character and three between them are what the decision is
    /// drawn between, so landing on either is as much evidence as there is.
    /// </remarks>
    [Theory]
    [InlineData(100.0)]
    [InlineData(300.0)]
    public void AGapOnEitherTextbookSpacingIsWorthEverything(double gapMs)
    {
        var margin = CwSettledPass.BoundaryMargin(gapMs, Cut);

        _output.WriteLine($"a {gapMs:0} ms gap against a {Cut:0} ms cut "
            + $"scores {margin:0.000}");

        Assert.Equal(1.0, margin, 2);
    }

    /// <remarks>
    /// <para>Proves HM-DEC-108: **it runs both ways.** A gap a little under the
    /// cut welds two characters into one and a gap a little over splits one into
    /// two, and both are the same decision made marginally. The measurement is a
    /// distance, so it does not care which side of the cut the gap fell.</para>
    /// </remarks>
    [Fact]
    public void ItIsADistanceRatherThanADirection()
    {
        var under = CwSettledPass.BoundaryMargin(Cut / 1.2, Cut);
        var over = CwSettledPass.BoundaryMargin(Cut * 1.2, Cut);

        _output.WriteLine($"under {under:0.000}, over {over:0.000}");

        Assert.Equal(under, over, 3);
        Assert.InRange(under, 0.1, 0.9);
    }

    /// <remarks>
    /// <para>Proves §0.0: **a gap nobody could measure is not evidence against a
    /// character, and it is not evidence for one either.** The measurement
    /// returns full marks so it cannot lower a score it knows nothing about, and
    /// the case it would otherwise have to judge is handled where it belongs:
    /// a character whose end the window never saw is held for the next window
    /// rather than scored at all.</para>
    /// </remarks>
    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.MaxValue)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NaN)]
    public void AGapNobodyMeasuredLowersNothing(double gapMs)
    {
        Assert.Equal(1.0, CwSettledPass.BoundaryMargin(gapMs, Cut));
    }

    /// <remarks>
    /// Proves §0.0: with no clock there is no boundary to measure a distance
    /// from, and nothing is claimed either way.
    /// </remarks>
    [Fact]
    public void WithNoCutThereIsNothingToMeasureAgainst()
    {
        Assert.Equal(1.0, CwSettledPass.BoundaryMargin(150, 0));
        Assert.Equal(1.0, CwSettledPass.BoundaryMargin(150, -5));
    }

    /// <remarks>
    /// <para>Proves HM-DEC-108's own words: **nothing here raises a
    /// confidence.** The measurement is bounded at one, so the worst-of-three
    /// can only ever land at or below what the worst of the existing two already
    /// was. A third score that could exceed them would be a way of talking a
    /// doubtful character up, which is the one thing the model may never do.
    /// </para>
    /// </remarks>
    [Theory]
    [InlineData(1.0)]
    [InlineData(50.0)]
    [InlineData(173.2)]
    [InlineData(400.0)]
    [InlineData(5000.0)]
    public void ItCanOnlyEverLowerAScore(double gapMs)
    {
        var margin = CwSettledPass.BoundaryMargin(gapMs, Cut);

        Assert.InRange(margin, 0.0, 1.0);
    }
}

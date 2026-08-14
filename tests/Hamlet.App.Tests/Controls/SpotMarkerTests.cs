using Avalonia;
using Hamlet.App.Controls;
using Xunit;

namespace Hamlet.App.Tests.Controls;

/// <summary>
/// The spot markers on the dial tape, and the axis they share with the
/// neighborhood map and the waterfall (HM-DEC-047).
/// </summary>
/// <remarks>
/// The claim these guard is the one the operator can check with their own
/// eyes: the tape is the zoomed view of the map's axis, so a station visible on
/// both is at the same frequency on both. Geometry is the testable half of it,
/// and it is the half that goes wrong silently.
/// </remarks>
public sealed class SpotMarkerTests
{
    private const long BandLow = 7_000_000;
    private const long BandHigh = 7_300_000;
    private const double MapWidth = 900;
    private const double TapeWidth = 860;

    /// <remarks>
    /// THE AGREEMENT (HM-DEC-047). A spot placed by the map's axis and the same
    /// spot placed by the tape's axis both read back as the frequency they
    /// actually are. This is what a single <see cref="FrequencyAxis"/> buys:
    /// the surfaces cannot drift apart, because there is only one arithmetic.
    /// </remarks>
    [Theory]
    [InlineData(7_030_000L)]
    [InlineData(7_030_500L)]
    [InlineData(7_029_100L)]
    [InlineData(7_031_900L)]
    public void TheMapAndTheTapePlaceASpotAtTheSameFrequency(long spotHz)
    {
        var map = FrequencyAxis.Across(BandLow, BandHigh, MapWidth);
        var tape = FrequencyAxis.Zoomed(7_030_000, DialTapeControl.PixelsPerHz, TapeWidth);

        Assert.True(map.Covers(spotHz));
        Assert.True(tape.Covers(spotHz));

        // Read each surface's own pixel back as a frequency. The map's is
        // coarse enough that a pixel is worth a few hundred hertz, so it is
        // held to a pixel rather than to the hertz.
        var mapHz = map.HzAt(map.XOf(spotHz));
        var tapeHz = tape.HzAt(tape.XOf(spotHz));

        Assert.Equal(spotHz, tapeHz);
        Assert.True(
            Math.Abs(mapHz - spotHz) <= map.SpanHz / map.Width,
            $"the map put {spotHz} at {mapHz}, more than a pixel out");
    }

    /// <remarks>
    /// Proves the tape's window is centered on the radio's frequency, which is
    /// what makes the hairline mean anything. Tuning to a spot has to put its
    /// marker under the hairline, because that is the gesture the waterfall
    /// inherits in phase 2.
    /// </remarks>
    [Fact]
    public void TuningToAMarkerPutsItUnderTheHairline()
    {
        var strip = new SpotMarkerStrip();
        var spot = Dot(7_028_400);

        strip.Rebuild(
            new[] { spot },
            FrequencyAxis.Zoomed(spot.FrequencyHz, DialTapeControl.PixelsPerHz, TapeWidth));

        Assert.Single(strip.Markers);
        Assert.Equal(TapeWidth / 2, strip.Markers[0].X, 6);
    }

    /// <remarks>
    /// Proves a spot outside the window is dropped rather than pinned to an
    /// edge. A marker held at the edge would be claiming a frequency the
    /// station is not on, which is the prime directive broken for the sake of
    /// a tidier picture (§0.0).
    /// </remarks>
    [Fact]
    public void SpotsOutsideTheWindowAreDroppedRatherThanPinned()
    {
        var strip = new SpotMarkerStrip();
        var axis = FrequencyAxis.Zoomed(7_030_000, DialTapeControl.PixelsPerHz, TapeWidth);

        // The window is a little over five kilohertz wide at this zoom, so one
        // of these is on screen and the other two are nowhere near it.
        strip.Rebuild(
            new[] { Dot(7_030_400), Dot(7_120_000), Dot(7_002_000) }, axis);

        Assert.Single(strip.Markers);
        Assert.Equal(7_030_400, strip.Markers[0].Dot.FrequencyHz);
    }

    /// <remarks>
    /// Proves the rail is empty when there is nothing to say. An empty groove
    /// drawn anyway would read as "nobody is here", and Hamlet cannot tell that
    /// apart from every spot feed being down at once (HM-DEC-025).
    /// </remarks>
    [Fact]
    public void NoSpotsMeansNoRail()
    {
        var strip = new SpotMarkerStrip();
        var axis = FrequencyAxis.Zoomed(7_030_000, DialTapeControl.PixelsPerHz, TapeWidth);

        strip.Rebuild(null, axis);
        Assert.False(strip.HasMarkers);

        strip.Rebuild(Array.Empty<ActivityDot>(), axis);
        Assert.False(strip.HasMarkers);
    }

    /// <remarks>
    /// Proves the rail answers to the pointer where it is drawn and stays
    /// quiet elsewhere. The reach is taller than the bar on purpose: a
    /// three-pixel mark is an honest way to show a frequency and a cruel thing
    /// to ask somebody to hit.
    /// </remarks>
    [Fact]
    public void AMarkerIsFoundOnTheRailAndNowhereElse()
    {
        var strip = new SpotMarkerStrip();
        var axis = FrequencyAxis.Zoomed(7_030_000, DialTapeControl.PixelsPerHz, TapeWidth);
        var reach = new Rect(0, 0, TapeWidth, 16);

        strip.Rebuild(new[] { Dot(7_030_000) }, axis);
        var x = strip.Markers[0].X;

        Assert.NotNull(strip.At(new Point(x, 4), reach));
        Assert.NotNull(strip.At(new Point(x + 5, 4), reach));

        // Far enough along the rail to be a different frequency.
        Assert.Null(strip.At(new Point(x + 40, 4), reach));

        // Below the rail is the frequency scale, which belongs to dragging.
        Assert.Null(strip.At(new Point(x, 50), reach));
    }

    /// <remarks>
    /// Proves the nearest marker wins when two sit close together, so a click
    /// lands on the station the operator was pointing at rather than on
    /// whichever one happened to be first in the list.
    /// </remarks>
    [Fact]
    public void TheNearestMarkerWinsWhenTwoAreClose()
    {
        var strip = new SpotMarkerStrip();
        var axis = FrequencyAxis.Zoomed(7_030_000, DialTapeControl.PixelsPerHz, TapeWidth);
        var reach = new Rect(0, 0, TapeWidth, 16);

        strip.Rebuild(new[] { Dot(7_030_000), Dot(7_030_030) }, axis);

        var far = strip.Markers.Single(m => m.Dot.FrequencyHz == 7_030_030);
        var hit = strip.At(new Point(far.X, 4), reach);

        Assert.NotNull(hit);
        Assert.Equal(7_030_030, hit!.Dot.FrequencyHz);
    }

    /// <remarks>
    /// Proves the ranking reaches the rail, so the tape and the map and the
    /// list all say the same thing about what matters (HM-DEC-023). Also proves
    /// the ink is cached: the tape relays its rail on every frame of a flick,
    /// and a brush allocated per marker per frame is the churn HM-DEC-006 keeps
    /// off the render path.
    /// </remarks>
    [Fact]
    public void TheRankingReachesTheRailAndTheInkIsCached()
    {
        var strip = new SpotMarkerStrip();
        var axis = FrequencyAxis.Zoomed(7_030_000, DialTapeControl.PixelsPerHz, TapeWidth);

        strip.Rebuild(
            new[] { Dot(7_030_000, 1.0), Dot(7_030_400, 0.1) }, axis);

        var best = strip.Markers.Single(m => m.Dot.FrequencyHz == 7_030_000);
        var rest = strip.Markers.Single(m => m.Dot.FrequencyHz == 7_030_400);

        Assert.True(best.Prominence > rest.Prominence);
        Assert.Same(SpotMarkerStrip.BrushFor(1.0), SpotMarkerStrip.BrushFor(1.0));
        Assert.NotSame(SpotMarkerStrip.BrushFor(1.0), SpotMarkerStrip.BrushFor(0.1));
    }

    /// <remarks>
    /// Proves an axis with nothing to map says so rather than dividing by zero.
    /// A control gets laid out before it has a width, and a band's edges arrive
    /// from a binding that starts empty.
    /// </remarks>
    [Fact]
    public void AnAxisWithNoWidthOrNoSpanIsUnusable()
    {
        Assert.False(FrequencyAxis.Across(BandLow, BandHigh, 0).IsUsable);
        Assert.False(FrequencyAxis.Across(BandLow, BandLow, MapWidth).IsUsable);
        Assert.True(FrequencyAxis.Across(BandLow, BandHigh, MapWidth).IsUsable);
    }

    /// <remarks>
    /// Proves a pointer beyond either end of a control still names a frequency
    /// inside the band. Pointer capture keeps sending positions after the
    /// pointer has left the control, and the radio must not be asked to go
    /// somewhere the band does not reach.
    /// </remarks>
    [Fact]
    public void APointerPastTheEdgeStillNamesAFrequencyInsideTheBand()
    {
        var axis = FrequencyAxis.Across(BandLow, BandHigh, MapWidth);

        Assert.Equal(BandLow, axis.HzAtClamped(-500));
        Assert.Equal(BandHigh, axis.HzAtClamped(MapWidth + 500));
    }

    private static ActivityDot Dot(long hz, double prominence = 0.8)
        => new(hz, "somebody calling CQ", "CW", "RBN", "3 min ago", "strong and recent", prominence);
}

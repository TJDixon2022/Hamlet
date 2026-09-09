using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 299, task 3: **the globe places both stations by the
/// coastline's own projection.**
/// </summary>
/// <remarks>
/// <para>**§0.0 BINDS A PICTURE AS HARD AS A SENTENCE** (HM-DEC-092), and this
/// unit's exposure is **a dot in the wrong place**, which nobody would ever check.
/// A wrong sentence gets argued with; a wrong dot gets believed.</para>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT** is the obvious one: writing a
/// second projection. `x = (longitude + 180) * 2` is four characters away from
/// `* 4` and from `(longitude + 90)`, and either would put every dot somewhere
/// plausible and wrong over a coastline that is still drawn correctly.</para>
/// <para>**IT WAS WATCHED FAILING.** With `Ft8GlobePlot.X` returning
/// `(longitude + 180) * 4`, `BothDotsArePlacedByTheCoastlineOwnProjection` reports
/// the operator at x=1240 on a map 720 wide.</para>
/// </remarks>
public sealed class Unit299GlobeTests
{
    /// <summary>The operator's own square, as Settings has it on his machine.</summary>
    private const string HisGrid = "FN00";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the plots are printed.</param>
    public Unit299GlobeTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// **The projection this code uses is the one written in the coastline itself.**
    /// </summary>
    /// <remarks>
    /// **THE ASSET IS READ AND ITS OWN `desc` IS THE ORACLE** (the instruction: *use
    /// it, do not invent a second one*). If somebody redraws the map on a different
    /// projection and forgets this file, this fails.
    /// </remarks>
    [Fact]
    public void TheProjectionIsTheCoastlineOwn()
    {
        var svg = XDocument.Load(
            @"C:\Source\HamLet\assets\world-coastline.svg");

        var desc = svg.Root!.Elements()
            .First(e => e.Name.LocalName == "desc").Value;

        _output.WriteLine(desc);

        Assert.Contains(
            "x = (longitude + 180) * 2", desc, StringComparison.Ordinal);
        Assert.Contains("y = (90 - latitude) * 2", desc, StringComparison.Ordinal);

        var box = svg.Root.Attribute("viewBox")!.Value;

        Assert.Equal("0 0 720 360", box);
        Assert.Equal(720, Ft8GlobePlot.MapWidth);
        Assert.Equal(360, Ft8GlobePlot.MapHeight);

        // The corners the projection itself defines.
        Assert.Equal(0, Ft8GlobePlot.X(-180));
        Assert.Equal(720, Ft8GlobePlot.X(180));
        Assert.Equal(0, Ft8GlobePlot.Y(90));
        Assert.Equal(360, Ft8GlobePlot.Y(-90));
    }

    /// <summary>**Both dots land where the projection says, to the pixel.**</summary>
    [Fact]
    public void BothDotsArePlacedByTheCoastlineOwnProjection()
    {
        var plot = new Ft8GlobePlot(HisGrid, "IO63", "EI4GNB", "Ireland");

        var here = OperatorLocation.FromGrid(HisGrid)!.Value;
        var there = OperatorLocation.FromGrid("IO63")!.Value;

        _output.WriteLine(
            "operator FN00 at " + here.Latitude.ToString("0.###", CultureInfo.InvariantCulture)
            + ", " + here.Longitude.ToString("0.###", CultureInfo.InvariantCulture)
            + " -> x " + plot.OperatorX.ToString("0.##", CultureInfo.InvariantCulture)
            + ", y " + plot.OperatorY.ToString("0.##", CultureInfo.InvariantCulture));

        _output.WriteLine(
            "station  IO63 at " + there.Latitude.ToString("0.###", CultureInfo.InvariantCulture)
            + ", " + there.Longitude.ToString("0.###", CultureInfo.InvariantCulture)
            + " -> x " + plot.StationX.ToString("0.##", CultureInfo.InvariantCulture)
            + ", y " + plot.StationY.ToString("0.##", CultureInfo.InvariantCulture));

        _output.WriteLine("frame: " + plot.FrameLine);

        Assert.True(plot.HasOperator);
        Assert.True(plot.HasStation);
        Assert.True(plot.HasPath);

        Assert.Equal((here.Longitude + 180) * 2, plot.OperatorX, 6);
        Assert.Equal((90 - here.Latitude) * 2, plot.OperatorY, 6);
        Assert.Equal((there.Longitude + 180) * 2, plot.StationX, 6);
        Assert.Equal((90 - there.Latitude) * 2, plot.StationY, 6);

        // **AND THEY ARE ON THE MAP**, which a second projection would break.
        Assert.InRange(plot.OperatorX, 0, Ft8GlobePlot.MapWidth);
        Assert.InRange(plot.StationX, 0, Ft8GlobePlot.MapWidth);
        Assert.InRange(plot.OperatorY, 0, Ft8GlobePlot.MapHeight);
        Assert.InRange(plot.StationY, 0, Ft8GlobePlot.MapHeight);
    }

    /// <summary>**The frame holds both points, with room round them.**</summary>
    [Fact]
    public void TheFrameHoldsBothPoints()
    {
        foreach (var (grid, what) in new[]
        {
            ("IO63", "Ireland, across an ocean"),
            ("PM95", "Japan, most of the way round"),
            ("FK03", "the Caribbean, close to home"),
            ("FN31", "the next state"),
        })
        {
            var plot = new Ft8GlobePlot(HisGrid, grid, "TEST", "");

            var (left, top, width, height) = plot.Frame;

            _output.WriteLine(what.PadRight(28) + " " + plot.FrameLine);

            Assert.InRange(plot.OperatorX, left, left + width);
            Assert.InRange(plot.StationX, left, left + width);
            Assert.InRange(plot.OperatorY, top, top + height);
            Assert.InRange(plot.StationY, top, top + height);

            // **NEVER OFF THE EDGE OF THE MAP**, which would frame empty space.
            Assert.InRange(left, 0, Ft8GlobePlot.MapWidth);
            Assert.InRange(top, 0, Ft8GlobePlot.MapHeight);
            Assert.InRange(left + width, 0, Ft8GlobePlot.MapWidth + 0.001);
            Assert.InRange(top + height, 0, Ft8GlobePlot.MapHeight + 0.001);
        }
    }

    /// <summary>**A close pair does not zoom to a field.**</summary>
    /// <remarks>
    /// This coastline is a simplified outline; below a certain frame there is
    /// nothing in it to recognise and the picture stops meaning anything.
    /// </remarks>
    [Fact]
    public void ACloseContactStopsZoomingBeforeThePictureStopsMeaningAnything()
    {
        var plot = new Ft8GlobePlot(HisGrid, "FN00", "W1ABC", "");

        _output.WriteLine(plot.FrameLine);

        Assert.True(
            plot.Frame.Width >= Ft8GlobePlot.SmallestFrame,
            "the frame shrank to " + plot.Frame.Width + " map units");
    }

    /// <summary>**A station with no grid gets no dot, and the hover says so.**</summary>
    [Fact]
    public void AStationWithNoGridGetsNoDot()
    {
        var plot = new Ft8GlobePlot(HisGrid, null, "K9XP", "the United States");

        _output.WriteLine(plot.Caption);

        Assert.False(plot.HasStation);
        Assert.False(plot.HasPath);
        Assert.True(plot.HasOperator);

        Assert.Contains(
            "does not know where K9XP is", plot.Caption, StringComparison.Ordinal);

        // **AND IT SAYS WHY A CALLSIGN IS NOT ENOUGH**, which is the guess the
        // instruction forbids: an entity is a country and a country is not a point.
        Assert.Contains("only names a country", plot.Caption, StringComparison.Ordinal);

        Assert.DoesNotContain("miles", plot.Caption, StringComparison.Ordinal);
    }

    /// <summary>
    /// **The straight line is never presented as the path the signal took.**
    /// </summary>
    /// <remarks>
    /// **THE INSTRUCTION IS EXPLICIT.** A great circle curves on an equirectangular
    /// map and a path near a pole looks nothing like a straight line, so the caption
    /// carries the caveat wherever a line is drawn.
    /// </remarks>
    [Fact]
    public void TheStraightLineSaysWhatItIsNot()
    {
        var drawn = new Ft8GlobePlot(HisGrid, "IO63", "EI4GNB", "Ireland");

        _output.WriteLine(drawn.Caption);

        Assert.True(drawn.HasPath);
        Assert.Contains(
            "rather than the path the signal took", drawn.Caption,
            StringComparison.Ordinal);

        // **AND IT IS NOT SAID WHERE NO LINE IS DRAWN**, because there is nothing
        // to caveat.
        var none = new Ft8GlobePlot(HisGrid, null, "K9XP", "");

        Assert.DoesNotContain(
            "the path the signal took", none.Caption, StringComparison.Ordinal);
    }

    /// <summary>**The caption names the place, the grid and the distance.**</summary>
    [Fact]
    public void TheCaptionNamesThePlaceTheGridAndTheDistance()
    {
        var plot = new Ft8GlobePlot(HisGrid, "IO63", "EI4GNB", "Ireland");

        _output.WriteLine(plot.Caption);

        Assert.Contains("EI4GNB", plot.Caption, StringComparison.Ordinal);
        Assert.Contains("Ireland", plot.Caption, StringComparison.Ordinal);
        Assert.Contains("grid IO63", plot.Caption, StringComparison.Ordinal);
        Assert.Contains("miles from you", plot.Caption, StringComparison.Ordinal);
    }

    /// <summary>**With no grid in Settings the operator has no dot either.**</summary>
    [Fact]
    public void WithNoGridInSettingsTheOperatorIsNotPlaced()
    {
        var plot = new Ft8GlobePlot(null, "IO63", "EI4GNB", "Ireland");

        _output.WriteLine(plot.Caption);

        Assert.False(plot.HasOperator);
        Assert.True(plot.HasStation);
        Assert.False(plot.HasPath);
        Assert.Null(plot.Miles);

        Assert.Contains(
            "needs your own grid square in Settings", plot.Caption,
            StringComparison.Ordinal);
    }
}

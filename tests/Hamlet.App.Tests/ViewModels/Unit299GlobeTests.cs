using System;
using System.Globalization;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 299, task 3: **the globe places both stations by the map's own
/// projection** - rewritten by work instruction 306 onto the picture that replaced
/// the drawn coastline.
/// </summary>
/// <remarks>
/// <para>**WHAT THIS TYPE GUARDS HAS NOT CHANGED.** Both dots are placed by one
/// projection, that projection belongs to the map being drawn, a station with no
/// grid gets no dot, and a caption says in words what the picture cannot. **What
/// moved is which map.**</para>
/// <para>**THE COASTLINE ORACLE IS GONE BECAUSE THE COASTLINE IS GONE.** Unit 299
/// read the projection out of `world-coastline.svg`'s own `desc` element, which was
/// exactly right while that file was what got drawn. Unit 306 draws a photograph,
/// and `TheAzimuthalAssetTests` is the oracle now: fourteen city dots found in the
/// image itself, matched to published coordinates, tolerance two pixels.</para>
/// <para>**THE FRAME TESTS ARE GONE WITH THE FRAMING.** The old plot zoomed to fit
/// both points, which a photograph cannot do without cropping the land the operator
/// is reading. The frame is the whole picture now, and that is asserted once here
/// rather than in four tests about a zoom that no longer exists.</para>
/// </remarks>
public sealed class Unit299GlobeTests
{
    private const string HisGrid = "FN00DJ";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the placements are printed.</param>
    public Unit299GlobeTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Both dots land where the map's own projection says.**</summary>
    [Fact]
    public void BothDotsArePlacedByTheMapOwnProjection()
    {
        var plot = new Ft8GlobePlot(HisGrid, "IO63", "EI4GNB", "Ireland");

        var here = OperatorLocation.FromGrid(HisGrid)!.Value;
        var there = OperatorLocation.FromGrid("IO63")!.Value;

        var mine = AzimuthalMap.NorthPolar.Place(here.Latitude, here.Longitude)!.Value;
        var his = AzimuthalMap.NorthPolar.Place(there.Latitude, there.Longitude)!.Value;

        _output.WriteLine("operator " + HisGrid + " -> " + Where(plot.OperatorX, plot.OperatorY));
        _output.WriteLine("station  IO63   -> " + Where(plot.StationX, plot.StationY));
        _output.WriteLine("frame: " + plot.FrameLine);

        Assert.True(plot.HasOperator);
        Assert.True(plot.HasStation);
        Assert.True(plot.HasPath);

        // **ONE PROJECTION, AND IT IS THE MAP'S.**
        Assert.Equal(mine.X, plot.OperatorX, 6);
        Assert.Equal(mine.Y, plot.OperatorY, 6);
        Assert.Equal(his.X, plot.StationX, 6);
        Assert.Equal(his.Y, plot.StationY, 6);

        // **AND THEY ARE ON THE PICTURE**, which is a separate question from being
        // inside the projection: this disc is cropped by its own frame.
        Assert.InRange(plot.OperatorX, 0, AzimuthalMap.NorthPolar.WidthPixels);
        Assert.InRange(plot.StationX, 0, AzimuthalMap.NorthPolar.WidthPixels);
        Assert.InRange(plot.OperatorY, 0, AzimuthalMap.NorthPolar.HeightPixels);
        Assert.InRange(plot.StationY, 0, AzimuthalMap.NorthPolar.HeightPixels);
    }

    /// <summary>**The frame is the whole picture.**</summary>
    /// <remarks>
    /// **A PHOTOGRAPH IS NOT ZOOMED TO FIT TWO DOTS.** The old plot framed to the
    /// two points with a margin and a floor; on a raster that either scales badly or
    /// crops the coastline the operator is reading it against.
    /// </remarks>
    [Fact]
    public void TheFrameIsTheWholePicture()
    {
        foreach (var grid in new[] { "IO63", "FN20", "BP51" })
        {
            var plot = new Ft8GlobePlot(HisGrid, grid, "TEST", "");

            _output.WriteLine(grid + " -> " + plot.FrameLine);

            Assert.Equal(0, plot.Frame.Left);
            Assert.Equal(0, plot.Frame.Top);
            Assert.Equal(AzimuthalMap.NorthPolar.WidthPixels, plot.Frame.Width);
            Assert.Equal(AzimuthalMap.NorthPolar.HeightPixels, plot.Frame.Height);
        }
    }

    /// <summary>**A station with no grid gets no dot.**</summary>
    [Fact]
    public void AStationWithNoGridGetsNoDot()
    {
        var plot = new Ft8GlobePlot(HisGrid, null, "K9XP", "");

        _output.WriteLine("caption: " + plot.Caption);

        Assert.True(plot.HasOperator);
        Assert.False(plot.HasStation);
        Assert.False(plot.HasPath);

        // **AND THE CAPTION SAYS SO IN WORDS**, because a picture of one dot is a
        // picture the reader has to interpret.
        Assert.Contains(
            "does not know where", plot.Caption, StringComparison.Ordinal);
    }

    /// <summary>**With no grid in Settings the operator is not placed.**</summary>
    /// <remarks>
    /// **AND THE STATION STILL IS.** On a pole-centred map his own position is not
    /// needed to place somebody else, which is a change from the framed coastline
    /// where the two were fitted together.
    /// </remarks>
    [Fact]
    public void WithNoGridInSettingsTheOperatorIsNotPlaced()
    {
        var plot = new Ft8GlobePlot(null, "IO63", "EI4GNB", "Ireland");

        _output.WriteLine("caption: " + plot.Caption);

        Assert.False(plot.HasOperator);
        Assert.True(plot.HasStation);
        Assert.False(plot.HasPath);

        Assert.Contains(
            "your own grid square", plot.Caption, StringComparison.Ordinal);
    }

    /// <summary>**The caption names the place, the grid and the distance.**</summary>
    [Fact]
    public void TheCaptionNamesThePlaceTheGridAndTheDistance()
    {
        var plot = new Ft8GlobePlot(HisGrid, "IO63", "EI4GNB", "Ireland");

        _output.WriteLine(plot.Caption);

        Assert.Contains("EI4GNB", plot.Caption, StringComparison.Ordinal);
        Assert.Contains("Ireland", plot.Caption, StringComparison.Ordinal);
        Assert.Contains("IO63", plot.Caption, StringComparison.Ordinal);
        Assert.Contains("miles", plot.Caption, StringComparison.Ordinal);
    }

    private static string Where(double x, double y)
        => x.ToString("0.##", CultureInfo.InvariantCulture) + ", "
           + y.ToString("0.##", CultureInfo.InvariantCulture);
}

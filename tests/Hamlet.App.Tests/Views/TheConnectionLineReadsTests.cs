using System;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Headless.XUnit;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 309 task 2: **the connection line reads at the size the card
/// draws it.**
/// </summary>
/// <remarks>
/// <para>**WHAT TASK 1 MEASURED, AND IT IS NOT WHAT THE INSTRUCTION EXPECTED.** The
/// path and the operator marker were **drawn the whole time**: `HasOperator` and
/// `HasPath` are true for the exact card on the operator's screen and for every shape
/// of his grid. The control pushes no transform, so the stroke width and the marker
/// radius were already in control units and reached the screen at their stated
/// sizes. **The fault is contrast, not size** - an amber ring on bright orange land
/// and a thin muted grey dash over dark blue ocean.</para>
/// <para>**SO THE FIX IS CASING.** A dark stroke under a light one reads over both
/// the ocean and the land, where one colour reads over one or the other. Shape
/// carries the difference between the two markers - a ring and a filled dot - so
/// neither depends on hue (§0.6).</para>
/// <para>**EVERY ASSERTION HERE IS COMPUTED, NOT SEEN.** Nothing in this repository
/// can look at a picture. What is asserted is what the drawing code is handed and
/// what it decides; whether the result is legible on his screen is a question only he
/// can answer, and this unit says so rather than claiming it.</para>
/// </remarks>
public sealed class TheConnectionLineReadsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sizes are printed.</param>
    public TheConnectionLineReadsTests(ITestOutputHelper output) => _output = output;

    /// <summary>The size the card draws the map at, from unit 309's instruction.</summary>
    private const double CardWidth = 240;

    /// <summary>**The polyline reaches the drawing surface with both ends placed.**</summary>
    [AvaloniaFact]
    public void ThePolylineReachesTheSurfaceWithBothEndsPlaced()
    {
        var plot = new Ft8GlobePlot("FN00DJ", "KN98", "RD6OB", "European Russia");
        var drawn = Ft8GlobeControl.WhatWouldBeDrawn(plot, CardWidth, 131);

        _output.WriteLine("path runs drawn   : " + drawn.PathRuns);
        _output.WriteLine("path segments     : " + drawn.PathSegments);
        _output.WriteLine("operator marker   : " + drawn.HasOperatorMarker);
        _output.WriteLine("station marker    : " + drawn.HasStationMarker);

        Assert.True(drawn.PathRuns > 0, "no run of the path reached the surface");
        Assert.True(drawn.PathSegments > 100, "the path drew only " + drawn.PathSegments);
    }

    /// <summary>**The operator marker does not depend on the station being placeable.**</summary>
    /// <remarks>
    /// **HIS OWN POSITION IS HIS OWN.** A station in Antarctica cannot be drawn on
    /// this picture, and that says nothing about where the operator is.
    /// </remarks>
    [AvaloniaFact]
    public void TheOperatorMarkerIsThereWheneverHisGridResolves()
    {
        foreach (var (station, why) in new[]
        {
            ("KN98", "a station on the picture"),
            ("RB32", "a station in Antarctica, which cannot be drawn"),
            (null, "a station who sent no grid"),
        })
        {
            var plot = new Ft8GlobePlot("FN00DJ", station, "TEST", "");
            var drawn = Ft8GlobeControl.WhatWouldBeDrawn(plot, CardWidth, 131);

            _output.WriteLine(
                (station ?? "(none)").PadRight(8)
                + "operator marker: " + drawn.HasOperatorMarker
                + ", station marker: " + drawn.HasStationMarker
                + "   (" + why + ")");

            Assert.True(
                drawn.HasOperatorMarker,
                "the operator vanished because of " + why);
        }
    }

    /// <summary>**Size is in control units; position is not.**</summary>
    /// <remarks>
    /// **THE CARD MAY BE DRAWN AT OTHER SIZES, NOW OR LATER.** Nothing here reads a
    /// hard-coded 240 or 0.344: the sizes are constants in the control's own units
    /// and the positions come from the render size, so the same card at half the
    /// width still shows the line.
    /// </remarks>
    [AvaloniaFact]
    public void SizeHoldsWhenTheRenderSizeChangesAndPositionDoesNot()
    {
        var plot = new Ft8GlobePlot("FN00DJ", "KN98", "RD6OB", "European Russia");

        var small = Ft8GlobeControl.WhatWouldBeDrawn(plot, CardWidth, 131);
        var large = Ft8GlobeControl.WhatWouldBeDrawn(plot, CardWidth * 2, 262);

        _output.WriteLine(
            "at " + CardWidth + " wide : stroke "
            + small.StrokeWidth.ToString("0.##", CultureInfo.InvariantCulture)
            + ", casing " + small.CasingWidth.ToString("0.##", CultureInfo.InvariantCulture)
            + ", marker " + small.MarkerRadius.ToString("0.##", CultureInfo.InvariantCulture)
            + ", operator at " + At(small.OperatorAt));

        _output.WriteLine(
            "at " + (CardWidth * 2) + " wide : stroke "
            + large.StrokeWidth.ToString("0.##", CultureInfo.InvariantCulture)
            + ", casing " + large.CasingWidth.ToString("0.##", CultureInfo.InvariantCulture)
            + ", marker " + large.MarkerRadius.ToString("0.##", CultureInfo.InvariantCulture)
            + ", operator at " + At(large.OperatorAt));

        // **THE INK DOES NOT CHANGE SIZE.**
        Assert.Equal(small.StrokeWidth, large.StrokeWidth, 6);
        Assert.Equal(small.CasingWidth, large.CasingWidth, 6);
        Assert.Equal(small.MarkerRadius, large.MarkerRadius, 6);

        // **AND THE POSITIONS DO.**
        Assert.True(
            large.OperatorAt.X > small.OperatorAt.X * 1.5,
            "the marker did not move with the render size");
    }

    /// <summary>**The casing exists: two strokes, the under one wider.**</summary>
    [AvaloniaFact]
    public void TheCasingIsTwoStrokesAndTheUnderOneIsWider()
    {
        var plot = new Ft8GlobePlot("FN00DJ", "KN98", "RD6OB", "European Russia");
        var drawn = Ft8GlobeControl.WhatWouldBeDrawn(plot, CardWidth, 131);

        _output.WriteLine(
            "casing " + drawn.CasingWidth.ToString("0.##", CultureInfo.InvariantCulture)
            + " under stroke "
            + drawn.StrokeWidth.ToString("0.##", CultureInfo.InvariantCulture));

        Assert.True(
            drawn.CasingWidth > drawn.StrokeWidth,
            "the casing is not wider than the stroke it cases");

        // **AND IT IS DRAWN TWICE**, which is what makes it a casing rather than a
        // thicker line.
        Assert.Equal(drawn.PathSegments * 2, drawn.PathStrokes);
    }

    /// <summary>**A station with no place gets no marker and no path.**</summary>
    [AvaloniaFact]
    public void AStationWithNoPlaceGetsNoMarkerAndNoPath()
    {
        foreach (var (station, expected, reason) in new[]
        {
            ("RB32", "nowhere on it", "Antarctica, which this picture does not reach"),
            (null, "does not know where", "he sent no grid"),
        })
        {
            var plot = new Ft8GlobePlot("FN00DJ", station, "TEST", "");
            var drawn = Ft8GlobeControl.WhatWouldBeDrawn(plot, CardWidth, 131);

            _output.WriteLine(
                (station ?? "(none)").PadRight(8)
                + "station marker: " + drawn.HasStationMarker
                + ", path runs: " + drawn.PathRuns
                + "   " + plot.Caption);

            Assert.False(drawn.HasStationMarker);
            Assert.Equal(0, drawn.PathRuns);

            // **AND THE CARD SAYS WHICH OF THE TWO REASONS IT IS.**
            Assert.Contains(expected, plot.Caption, StringComparison.Ordinal);
        }
    }

    private static string At(Point p)
        => p.X.ToString("0.#", CultureInfo.InvariantCulture) + ", "
           + p.Y.ToString("0.#", CultureInfo.InvariantCulture);
}

using System;
using System.Globalization;
using System.Linq;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 313 task 1: **what the popup actually does today, measured.**
/// </summary>
/// <remarks>
/// <para>**IT ASSERTS THE PRESENT AND NOT THE FUTURE.** These are the readings task 1
/// was asked for, taken before a line was changed, so the report can say what the
/// magnification *was* rather than describing it. Task 2's own tests assert what it
/// should be.</para>
/// <para>**COMPUTED, NOT SEEN** - with one exception recorded in the report: the
/// screenshot Tim supplied was read directly, and the glyph colour was sampled from its
/// pixels. Nothing in this file looks at anything.</para>
/// </remarks>
public sealed class Unit313TraceTests
{
    /// <summary>The popup's ceiling, from `MainWindow.axaml`.</summary>
    private const double PopupWidth = 720;

    /// <summary>The popup's ceiling, from `MainWindow.axaml`.</summary>
    private const double PopupHeight = 400;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the trace.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public Unit313TraceTests(ITestOutputHelper output) => _output = output;

    /// <summary>**What magnification the `F4DIA` case gets today.**</summary>
    /// <remarks>
    /// **THE AUTHOR MEASURES 3.9x FROM A 185 x 99 CROP.** This reports what the code
    /// does, from the code.
    /// </remarks>
    [Fact]
    public void WhatMagnificationTheFranceCaseGetsToday()
    {
        var plot = new Ft8GlobePlot("FN00DJ", "JN36", "F4DIA", "France");

        var (left, top, width, height) = plot.OpenFrame;

        var drawn = Fitted(width, height);
        var magnification = drawn.Width / width;

        _output.WriteLine("FN00DJ places at "
            + plot.OperatorX.ToString("0.0", CultureInfo.InvariantCulture) + ", "
            + plot.OperatorY.ToString("0.0", CultureInfo.InvariantCulture));

        _output.WriteLine("JN36   places at "
            + plot.StationX.ToString("0.0", CultureInfo.InvariantCulture) + ", "
            + plot.StationY.ToString("0.0", CultureInfo.InvariantCulture));

        var points = plot.Path.SelectMany(r => r).ToList();

        _output.WriteLine("the path box is "
            + (points.Max(p => p.X) - points.Min(p => p.X))
                .ToString("0.0", CultureInfo.InvariantCulture)
            + " by "
            + (points.Max(p => p.Y) - points.Min(p => p.Y))
                .ToString("0.0", CultureInfo.InvariantCulture));

        _output.WriteLine("the frame is " + plot.OpenFrameLine);

        _output.WriteLine("drawn at "
            + drawn.Width.ToString("0.0", CultureInfo.InvariantCulture) + " by "
            + drawn.Height.ToString("0.0", CultureInfo.InvariantCulture)
            + " in a popup of " + PopupWidth + " by " + PopupHeight);

        _output.WriteLine("MAGNIFICATION: "
            + magnification.ToString("0.00", CultureInfo.InvariantCulture) + "x");

        // **THE READING IS THE POINT; THIS ONLY PINS IT** so the report cannot quote a
        // number the code stopped producing.
        Assert.True(magnification > 3.0, "the trace expected a magnification over 3x");
    }

    /// <summary>**What the zoom floor unit 310 set actually permits.**</summary>
    /// <remarks>
    /// **A FLOOR ON THE FRAME IS A CAP ON THE MAGNIFICATION, AND THIS ONE IS LOOSE.**
    /// A quarter of a 698 px file is 174.5 px, and 174.5 px enlarged into a 720 px
    /// popup is more than four times. **The floor was the wrong instrument**: set tight
    /// enough to stay sharp it would be a floor of the whole file, which is no zoom at
    /// all.
    /// </remarks>
    [Fact]
    public void WhatTheZoomFloorPermits()
    {
        var floorWidth = Ft8GlobePlot.Map.WidthPixels * Ft8GlobePlot.ZoomFloorShare;
        var floorHeight = Ft8GlobePlot.Map.HeightPixels * Ft8GlobePlot.ZoomFloorShare;

        _output.WriteLine("ZoomFloorShare = " + Ft8GlobePlot.ZoomFloorShare
            + ", so the smallest frame is "
            + floorWidth.ToString("0.0", CultureInfo.InvariantCulture) + " by "
            + floorHeight.ToString("0.0", CultureInfo.InvariantCulture));

        var worst = Fitted(floorWidth, floorHeight).Width / floorWidth;

        _output.WriteLine("which permits up to "
            + worst.ToString("0.00", CultureInfo.InvariantCulture) + "x");

        Assert.True(worst > 4.0, "the floor was expected to permit over 4x");
    }

    /// <summary>**What every long path gets today.**</summary>
    [Fact]
    public void WhatEveryLongPathGetsToday()
    {
        foreach (var (call, grid, place) in new[]
        {
            ("F4DIA", "JN36", "France"),
            ("PY2ABC", "GG66", "Brazil"),
            ("ZS1ABC", "JF96", "South Africa"),
            ("JA1ABC", "PM95", "Japan"),
            ("ZL1ABC", "RF73", "New Zealand"),
            ("W2ABC", "FN20", "United States of America"),
        })
        {
            var plot = new Ft8GlobePlot("FN00DJ", grid, call, place);
            var (_, _, width, height) = plot.OpenFrame;

            var magnification = Fitted(width, height).Width / width;

            _output.WriteLine(
                call.PadRight(7) + grid + "  runs " + plot.Path.Count
                + "  frame " + plot.OpenFrameLine
                + "  -> " + magnification.ToString("0.00", CultureInfo.InvariantCulture)
                + "x");
        }
    }

    /// <summary>The box the popup draws the frame into, at its own ceiling.</summary>
    private static Avalonia.Size Fitted(double frameWidth, double frameHeight)
    {
        var scale = Math.Min(
            PopupWidth / frameWidth, PopupHeight / frameHeight);

        return new Avalonia.Size(frameWidth * scale, frameHeight * scale);
    }
}

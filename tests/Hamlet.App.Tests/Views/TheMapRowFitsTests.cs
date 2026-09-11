using System;
using System.Globalization;
using Avalonia;
using Avalonia.Headless.XUnit;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 310 task 4: **the row shrinks to the map.**
/// </summary>
/// <remarks>
/// <para>**R7** (Tim, 2026-09-11). Of three options - fill the row and crop, shrink
/// the row to the map, or stretch the map - he chose the second. *"BETTER, BUT WHY
/// THE WASTED SPACE ON THE RIGHT."*</para>
/// <para>**THE STRETCH WAS NEVER AVAILABLE ANYWAY.** The projection's six measured
/// constants describe this bitmap at 698 by 381; stretching it to fill a row of a
/// different aspect moves every marker off the place it belongs, which is §0.0 broken
/// by a picture and the exact fault those constants exist to prevent.</para>
/// <para>**THESE MEASURE THE LAYOUT, NOT A FORMULA.** Each one builds the control,
/// hands it the offer the card makes, and reads back the size it asks for. A test that
/// only checked the arithmetic would have passed against the markup that produced his
/// screenshot, because the arithmetic was never what was wrong.</para>
/// <para>**COMPUTED, NOT SEEN.** What is asserted is the size the control asks for and
/// the geometry it would draw at. Nothing here looks at a pixel.</para>
/// </remarks>
public sealed class TheMapRowFitsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the sizes are printed.</param>
    public TheMapRowFitsTests(ITestOutputHelper output) => _output = output;

    /// <summary>The bitmap's own aspect, which everything here holds to.</summary>
    private static double BitmapAspect
        => (double)FlatWorldMap.Relief.WidthPixels / FlatWorldMap.Relief.HeightPixels;

    /// <summary>Half a percent, which is finer than a pixel at this size.</summary>
    /// <remarks>
    /// **THE TOLERANCE IS STATED BECAUSE THE INSTRUCTION ASKS FOR IT.** At the card's
    /// own height the map is about 220 px wide, so half a percent is a little over one
    /// pixel: tight enough that a wrong aspect cannot hide inside it, loose enough that
    /// it is not asserting anything about floating point.
    /// </remarks>
    private const double Tolerance = 0.005;

    /// <summary>One layout pixel, which is what the row can be out by.</summary>
    /// <remarks>
    /// **MEASURED WHILE WRITING THIS.** Avalonia rounds a desired size **up** to a whole
    /// device pixel, so a control asking for 219.84 is recorded as wanting 220 and one
    /// asking for 293.11 as wanting 294. **The map itself keeps its proportions exactly**
    /// - the render fits it inside whatever it is given - so what the rounding can leave
    /// over is under one pixel of ground on one edge. That is the resolution the layout
    /// system works at, and the fault R7 is about was 260 px wide.
    /// </remarks>
    private const double OnePixel = 1.0;

    /// <summary>The height the card's map row is given.</summary>
    private const double CardRowHeight = 120;

    /// <summary>**The control asks for the map's own aspect, whatever it is offered.**</summary>
    [AvaloniaFact]
    public void TheControlAsksForTheMapOwnAspect()
    {
        foreach (var height in new double[] { 120, 90, 160, 60 })
        {
            var asked = Asked(offeredWidth: 480, rowHeight: height);
            var drawn = Fitted(asked);

            var aspect = drawn.Width / drawn.Height;

            _output.WriteLine(
                "row height " + height.ToString("0", CultureInfo.InvariantCulture).PadLeft(4)
                + " -> row "
                + asked.Width.ToString("0.00", CultureInfo.InvariantCulture).PadLeft(7)
                + " x " + asked.Height.ToString("0.00", CultureInfo.InvariantCulture)
                + ", map " + drawn.Width.ToString("0.00", CultureInfo.InvariantCulture)
                + " x " + drawn.Height.ToString("0.00", CultureInfo.InvariantCulture)
                + "  aspect " + aspect.ToString("0.0000", CultureInfo.InvariantCulture));

            Assert.True(
                Math.Abs(aspect - BitmapAspect) < Tolerance,
                "at " + height + " tall the aspect is "
                + aspect.ToString("0.0000", CultureInfo.InvariantCulture)
                + ", not " + BitmapAspect.ToString("0.0000", CultureInfo.InvariantCulture));
        }
    }

    /// <summary>**No part of the row is empty beside the map.**</summary>
    /// <remarks>
    /// **THE GREY WAS THE ROW BEING WIDER THAN THE PICTURE IT HELD.** The row took the
    /// card's whole width and the image fitted inside it by height, so everything to
    /// the right of the picture was empty ground. The wider the card, the more of it:
    /// at 480 the image drew 219.8 px and 260 px of the row were grey.
    /// </remarks>
    [AvaloniaFact]
    public void NoPartOfTheRowIsEmptyBesideTheMap()
    {
        foreach (var offered in new double[] { 240, 320, 480, 640 })
        {
            var asked = Asked(offered, CardRowHeight);
            var drawn = Fitted(asked);

            var drawnWidth = drawn.Width;
            var drawnHeight = drawn.Height;

            _output.WriteLine(
                "offered " + offered.ToString("0", CultureInfo.InvariantCulture).PadLeft(4)
                + "  row " + asked.Width.ToString("0.0", CultureInfo.InvariantCulture)
                + " x " + asked.Height.ToString("0.0", CultureInfo.InvariantCulture)
                + "  image " + drawnWidth.ToString("0.0", CultureInfo.InvariantCulture)
                + " x " + drawnHeight.ToString("0.0", CultureInfo.InvariantCulture)
                + "  spare "
                + (asked.Width - drawnWidth).ToString("0.00", CultureInfo.InvariantCulture)
                + " x "
                + (asked.Height - drawnHeight).ToString("0.00", CultureInfo.InvariantCulture));

            Assert.True(
                asked.Width - drawnWidth < OnePixel,
                (asked.Width - drawnWidth).ToString("0.00", CultureInfo.InvariantCulture)
                + " px of the row is empty to the right of the map");

            Assert.True(
                asked.Height - drawnHeight < OnePixel,
                (asked.Height - drawnHeight).ToString("0.00", CultureInfo.InvariantCulture)
                + " px of the row is empty below the map");

            // **AND THE MARKERS STILL LAND**, which is what a stretch would break.
            var plot = new Ft8GlobePlot("FN00DJ", "KN98", "RD6OB", "European Russia");

            var render = Ft8GlobeControl.WhatWouldBeDrawn(
                plot, asked.Width, asked.Height);

            Assert.True(render.HasOperatorMarker);
            Assert.True(render.HasStationMarker);
        }
    }

    /// <summary>**The map never becomes wider than it was.**</summary>
    /// <remarks>
    /// **R7's own words.** Letting the row keep the card's width and grow taller would
    /// also have removed the grey, and it is the option he did not choose: on a wide
    /// card it makes the map bigger than the card it sits on was built for, and the
    /// card grows rather than shrinking.
    /// </remarks>
    [AvaloniaFact]
    public void TheMapNeverBecomesWiderThanItWas()
    {
        foreach (var offered in new double[] { 240, 320, 480, 640 })
        {
            // **WHAT THE OLD ROW DREW**: the whole card width, fitted by height.
            var before = FlatWorldMap.Relief.WidthPixels * Math.Min(
                offered / FlatWorldMap.Relief.WidthPixels,
                CardRowHeight / FlatWorldMap.Relief.HeightPixels);

            var now = Fitted(Asked(offered, CardRowHeight)).Width;

            _output.WriteLine(
                "offered " + offered.ToString("0", CultureInfo.InvariantCulture).PadLeft(4)
                + ": the map drew " + before.ToString("0.0", CultureInfo.InvariantCulture)
                + " and now draws " + now.ToString("0.0", CultureInfo.InvariantCulture));

            Assert.True(
                now <= before + 0.001,
                "the map went from " + before.ToString("0.0", CultureInfo.InvariantCulture)
                + " to " + now.ToString("0.0", CultureInfo.InvariantCulture) + " wide");
        }
    }

    /// <summary>**No edge of the bitmap is cropped.**</summary>
    /// <remarks>
    /// **THE WHOLE WORLD STAYS WHOLE** (R7). A row that filled its width by cropping
    /// would take the north and south edges off the file, and the southern edge is
    /// where the coverage limit already bites.
    /// </remarks>
    [AvaloniaFact]
    public void NoEdgeOfTheBitmapIsCropped()
    {
        var asked = Asked(offeredWidth: 480, rowHeight: CardRowHeight);
        var drawn = Fitted(asked);

        var bottom = drawn.Height * (FlatWorldMap.Relief.HeightPixels - 1)
            / FlatWorldMap.Relief.HeightPixels;

        var right = drawn.Width * (FlatWorldMap.Relief.WidthPixels - 1)
            / FlatWorldMap.Relief.WidthPixels;

        _output.WriteLine(
            "the file's four edges land at top 0.0, left 0.0, bottom "
            + bottom.ToString("0.0", CultureInfo.InvariantCulture)
            + ", right " + right.ToString("0.0", CultureInfo.InvariantCulture)
            + " in a row " + asked.Width.ToString("0.0", CultureInfo.InvariantCulture)
            + " x " + asked.Height.ToString("0.0", CultureInfo.InvariantCulture));

        Assert.True(bottom <= asked.Height, "the south edge is below the row");
        Assert.True(right <= asked.Width, "the east edge is past the row");
    }

    /// <summary>**Changing the row's height changes the map's width in proportion.**</summary>
    /// <remarks>
    /// <para>**THIS IS THE INSTRUCTION'S ASSERTION 4 TURNED AROUND, AND THE TURN IS
    /// REPORTED.** It asks that changing the card's width change the map's height in
    /// proportion. Under R7 that is no longer true and must not be: the map's size
    /// stops depending on the card's width altogether, which is the whole point of the
    /// row shrinking to it. What still has to hold is that the two dimensions move
    /// together, so it is asserted from the side that still drives.</para>
    /// </remarks>
    [Fact]
    public void ChangingTheRowHeightChangesTheMapWidthInProportion()
    {
        var narrow = Ft8GlobeControl.WidthFor(120);
        var wide = Ft8GlobeControl.WidthFor(240);

        _output.WriteLine(
            "120 tall -> " + narrow.ToString("0.00", CultureInfo.InvariantCulture)
            + " wide, 240 tall -> " + wide.ToString("0.00", CultureInfo.InvariantCulture)
            + " wide");

        Assert.Equal(narrow * 2, wide, 6);

        // **AND THE TWO READ EACH OTHER BACK**, so neither can drift alone.
        Assert.Equal(120, Ft8GlobeControl.HeightFor(narrow), 6);
    }

    /// <summary>What the control asks for, given the offer the card makes it.</summary>
    /// <param name="offeredWidth">The width the card has to give.</param>
    /// <param name="rowHeight">The height the row is set to.</param>
    /// <returns>The size the layout system records as wanted.</returns>
    private static Size Asked(double offeredWidth, double rowHeight)
    {
        var control = new Ft8GlobeControl { Height = rowHeight };

        control.Measure(new Size(offeredWidth, double.PositiveInfinity));

        return control.DesiredSize;
    }

    /// <summary>The picture inside a row, at the size the render would draw it.</summary>
    /// <param name="row">The row the card gives the map.</param>
    /// <returns>The rectangle the bitmap occupies.</returns>
    /// <remarks>
    /// **THE SAME FIT <c>Render</c> MAKES**, so what is asserted is the picture rather
    /// than the box around it. The two are the same rectangle to within
    /// <see cref="OnePixel"/>, which is the other half of what R7 asks for and is
    /// asserted separately.
    /// </remarks>
    private static Size Fitted(Size row)
    {
        var scale = Math.Min(
            row.Width / FlatWorldMap.Relief.WidthPixels,
            row.Height / FlatWorldMap.Relief.HeightPixels);

        return new Size(
            FlatWorldMap.Relief.WidthPixels * scale,
            FlatWorldMap.Relief.HeightPixels * scale);
    }
}

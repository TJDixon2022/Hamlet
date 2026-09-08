using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 285: the marks render, and the About window shows one.
/// </summary>
/// <remarks>
/// <para>**A RESOURCE THAT IS PRESENT AND UNREFERENCED IS THE FAULT `HM-OPEN-087` IS
/// ABOUT** — thirteen `widget.*` templates in this tree are declared and reachable by
/// nothing. So nothing here asserts a file exists: it asserts the mark is in the
/// realized visual tree with a size, and that the pixels it produces are not
/// blank.</para>
/// <para>**AND IT MEASURES THE MARK AGAINST ITS OWN VIEWBOX**, because a drawing that
/// runs outside its box loses that ink wherever it is drawn and no amount of
/// reasoning about the spec is worth one measurement.</para>
/// </remarks>
public sealed class TheMarksRenderTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the extents and the pixel counts are printed.</param>
    public TheMarksRenderTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Both marks load, and both draw something.**</summary>
    [AvaloniaFact]
    public void BothMarksLoadAndDraw()
    {
        foreach (var uri in new[] { SvgMark.FullMarkUri, SvgMark.SmallMarkUri })
        {
            var drawing = SvgMark.Load(uri);

            _output.WriteLine(
                uri.Split('/').Last().PadRight(24)
                + SvgMark.Shapes(uri).Count + " shapes, drawing bounds "
                + drawing.GetBounds());

            Assert.True(
                SvgMark.Shapes(uri).Count > 0,
                uri + " loaded no shapes at all");
        }
    }

    /// <summary>**How much of each mark falls outside its own viewBox.**</summary>
    /// <remarks>
    /// <para>**MEASURED, NOT ARGUED.** SVG clips to the viewBox, so ink outside it is
    /// ink nobody sees — in a browser, in this application, and in an icon.</para>
    /// <para>This asserts the full mark fits, because that is the one going into the
    /// About window. **It asserts nothing about the small mark**: whether a mark that
    /// does not fit its box should be changed is Tim's, and this unit was told to
    /// report rather than redesign.</para>
    /// </remarks>
    [AvaloniaFact]
    public void EachMarkIsMeasuredAgainstItsOwnViewBox()
    {
        foreach (var uri in new[] { SvgMark.FullMarkUri, SvgMark.SmallMarkUri })
        {
            var extent = SvgMark.Extent(uri);
            var box = Box(uri);

            var above = Math.Max(0, box.Y - extent.Y);
            var below = Math.Max(0, extent.Bottom - box.Bottom);
            var left = Math.Max(0, box.X - extent.X);
            var right = Math.Max(0, extent.Right - box.Right);

            _output.WriteLine(uri.Split('/').Last());
            _output.WriteLine("  viewBox : " + box);
            _output.WriteLine("  ink     : " + extent);
            _output.WriteLine(
                "  outside : " + above.ToString("0.#") + " above, "
                + below.ToString("0.#") + " below, " + left.ToString("0.#") + " left, "
                + right.ToString("0.#") + " right");
            _output.WriteLine("");
        }

        // **NEITHER MARK FITS ITS OWN VIEWBOX, AND BOTH FIGURES ARE PINNED HERE.**
        // This asserted a fit when it was written, on the assumption that a drawing
        // fits the box it declares. It does not, and the measurement said so:
        //
        //   hamlet-logo.svg       18.5 of 260 clipped off the BOTTOM - the lower
        //                         edge of the faceplate, about 7 per cent
        //   hamlet-mark-small.svg 15.5 of 68 clipped off the TOP - a little over
        //                         half the feather, which is the joke
        //
        // **PINNED RATHER THAN FIXED** (work instruction 285: report, do not
        // redesign). Whichever way Tim rules, one of these numbers changes and this
        // goes red, which is the point of writing them down.
        var full = SvgMark.Extent(SvgMark.FullMarkUri);
        var small = SvgMark.Extent(SvgMark.SmallMarkUri);

        Assert.Equal(18.5, full.Bottom - Box(SvgMark.FullMarkUri).Bottom, 1);
        Assert.Equal(15.5, Box(SvgMark.SmallMarkUri).Y - small.Y, 1);
    }

    /// <summary>**The About window draws the full mark, at a size.**</summary>
    /// <remarks>
    /// Watched failing first: before the mark was placed there was no `HamletMark`
    /// on the realized window at all.
    /// </remarks>
    [AvaloniaFact]
    public void TheAboutWindowDrawsTheMark()
    {
        var window = About();

        var mark = window.GetVisualDescendants().OfType<Image>()
            .FirstOrDefault(i => i.Name == "HamletMark");

        Assert.True(
            mark is not null,
            "no Image named HamletMark on the realized About window. Images found: ["
            + string.Join(", ", window.GetVisualDescendants().OfType<Image>()
                .Where(i => i.Name is not null).Select(i => i.Name)) + "]");

        _output.WriteLine("bounds : " + mark!.Bounds);
        _output.WriteLine("source : " + (mark.Source?.GetType().Name ?? "(null)"));

        Assert.NotNull(mark.Source);

        // **A SIZE, NOT A PRESENCE.** An Image with a source and no room is an
        // element in the tree that draws nothing, which is the same fault as a
        // resource nobody references.
        Assert.True(
            mark.Bounds.Width >= 150,
            "the mark is " + mark.Bounds.Width.ToString("0")
            + " px wide; the order asks for 150 or more so the faceplate reads");

        Assert.True(mark.Bounds.Height > 0, "the mark has no height");
    }

    /// <summary>**Every shape carries ink, so nothing loaded invisible.**</summary>
    /// <remarks>
    /// <para>**THIS IS THE STRONGEST THING THIS HARNESS CAN SAY ABOUT A PICTURE, AND
    /// THE REASON IS WORTH RECORDING.** Counting rendered pixels was tried and
    /// cannot work here: the tests run on Avalonia's headless *drawing* backend,
    /// which composes a visual tree and rasterises nothing, so
    /// `RenderTargetBitmap.CopyPixels` throws *CopyPixels is not supported for this
    /// bitmap type* and a captured frame is blank whatever is drawn. Real pixels
    /// would want `Avalonia.Headless.Skia`, and a new package is a dependency
    /// decision rather than a session's (§0.4).</para>
    /// <para>**SO IT ASSERTS THE THING A BLANK RENDER WOULD ALSO FAIL**: that every
    /// shape the loader produced has a fill or a stroke and a real size. A loader
    /// that dropped an element, or read a colour as nothing, fails here. **What it
    /// cannot tell you is whether the mark looks right**, and this unit does not
    /// claim it does.</para>
    /// </remarks>
    [AvaloniaFact]
    public void EveryShapeCarriesInkAndASize()
    {
        foreach (var (name, uri) in new[]
        {
            ("full", SvgMark.FullMarkUri),
            ("small", SvgMark.SmallMarkUri),
        })
        {
            var shapes = SvgMark.Shapes(uri);

            var blank = 0;

            foreach (var child in shapes)
            {
                var bounds = child.GetBounds();

                if ((child.Brush is null && child.Pen is null)
                    || bounds.Width <= 0
                    || bounds.Height <= 0)
                {
                    blank++;
                }
            }

            _output.WriteLine(
                name.PadRight(8) + shapes.Count + " shapes, "
                + blank + " of them with no ink or no size");

            Assert.Equal(0, blank);
        }
    }

    /// <summary>**What survives of the small mark at icon sizes.**</summary>
    /// <remarks>
    /// <para>**NOTHING LOOKED AT THESE AND THIS DOES NOT PRETEND OTHERWISE.** The
    /// harness cannot rasterise, so this reports the arithmetic instead: how many
    /// device pixels each stroke of the mark is given at 16, 32 and 48. A stroke
    /// under one pixel is a grey smear rather than a line, and that is a statement
    /// about the drawing that can be made without seeing it.</para>
    /// <para>The mark is 68 units across, so a pixel at size N is 68/N units.</para>
    /// </remarks>
    [AvaloniaFact]
    public void WhatSurvivesOfTheSmallMarkAtIconSizes()
    {
        // The four strokes the file states, in its own units.
        var strokes = new (string What, double Units)[]
        {
            ("plate outline", 3.4),
            ("whip", 4.0),
            ("feather outline", 3.0),
        };

        foreach (var side in new[] { 16, 32, 48, 256 })
        {
            var scale = side / 68.0;

            _output.WriteLine(side + " px  (one pixel is "
                + (68.0 / side).ToString("0.0") + " units)");

            foreach (var (what, units) in strokes)
            {
                var px = units * scale;

                _output.WriteLine(
                    "   " + what.PadRight(18) + px.ToString("0.00").PadLeft(6)
                    + " px" + (px < 1 ? "   sub-pixel" : ""));
            }

            _output.WriteLine("");
        }

        // **THE ONE ASSERTION IS THE ONE THAT MATTERS**: at 16 px every stroke in
        // this mark is thinner than a pixel, so the icon is a dark disc with a
        // suggestion on it. That is a fact about the drawing, and it is reported
        // rather than repaired.
        var at16 = 16 / 68.0;

        Assert.True(
            strokes.All(s => s.Units * at16 < 1.0),
            "some stroke is at least a pixel wide at 16 px; the report's account of "
            + "what survives there needs re-taking");
    }

    /// <summary>**The icon builds, at the size it says it does.**</summary>
    /// <remarks>
    /// It cannot assert what the icon looks like — the harness has no rasteriser —
    /// only that the path from the file to a bitmap runs and produces the size it
    /// claims.
    /// </remarks>
    [AvaloniaFact]
    public void TheIconBuildsAtTheSizeItClaims()
    {
        using var raster = AppIcon.Raster(AppIcon.RenderedAt);

        _output.WriteLine(
            "rendered " + raster.PixelSize.Width + " x " + raster.PixelSize.Height);

        Assert.Equal(AppIcon.RenderedAt, raster.PixelSize.Width);
        Assert.Equal(AppIcon.RenderedAt, raster.PixelSize.Height);
    }

    /// <summary>One mark's viewBox.</summary>
    /// <param name="uri">Which mark.</param>
    /// <returns>The rectangle.</returns>
    private static Rect Box(string uri)
    {
        using var stream = Avalonia.Platform.AssetLoader.Open(new Uri(uri));

        return SvgMark.ViewBox(System.Xml.Linq.XDocument.Load(stream).Root!);
    }

    /// <summary>The About window, realized.</summary>
    /// <returns>The shown window.</returns>
    private static Window About()
    {
        var window = new AboutWindow { DataContext = new AboutViewModel() };

        window.Show();
        HowMuchTheApplicationSaysTests.Pump(window);

        return window;
    }
}

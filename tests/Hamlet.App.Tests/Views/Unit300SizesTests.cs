using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Hamlet.App.Controls;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 300 task 5: **what the mark renders as at 16, 24 and 32 px, in
/// both states - and what nobody looked at.**
/// </summary>
/// <remarks>
/// <para>**THE INSTRUCTION'S OWN WARNING IS THE POINT OF THIS CLASS**: *do not
/// compute a stroke width and call it a look.* So this does not read the renderer's
/// arithmetic back out of the renderer. **It runs `Render` and records what came
/// out of it**, into a <see cref="DrawingGroup"/>, and reports the drawings the
/// control actually emitted at each size.</para>
/// <para>**AND IT ATTEMPTS A REAL RASTER FIRST**, so the claim that nobody looked is
/// a measurement rather than an inherited belief. Units 285 and 286 recorded that
/// the headless drawing backend composes without rasterising;
/// <see cref="WhetherAnythingHereCanActuallyLook"/> tries it again and prints
/// whatever happens, because a limitation carried forward without being re-tested is
/// the same shape as a convention nobody ruled on.</para>
/// </remarks>
public sealed class Unit300SizesTests
{
    /// <summary>The three sizes the instruction names.</summary>
    private static readonly double[] Sizes = { 16, 24, 32 };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the drawings are printed.</param>
    public Unit300SizesTests(ITestOutputHelper output) => _output = output;

    /// <summary>**What the mark emits at each size, in each state.**</summary>
    [AvaloniaFact]
    public void WhatTheMarkDrawsAtEachSize()
    {
        foreach (var side in Sizes)
        {
            foreach (var lit in new[] { false, true })
            {
                var drawings = Drawn(side, lit);

                _output.WriteLine(
                    side.ToString("0", CultureInfo.InvariantCulture) + " px, "
                    + (lit ? "something new" : "at rest      ")
                    + " : " + drawings.Count + " drawings");

                foreach (var line in Describe(drawings))
                {
                    _output.WriteLine("      " + line);
                }

                _output.WriteLine("");
            }
        }

        // **THE TWO STATES ARE TWO DIFFERENT SETS OF SHAPES AT EVERY SIZE**, which
        // is §0.6 measured rather than asserted: a greyscale printer keeps a ring
        // that is present or absent.
        //
        // **RECONCILED WITH `Unit303OptionBTests` UNDER §R12, WORK INSTRUCTION 330
        // TASK 1.** This class used to assert `rest.All(d => d.Brush is null)` - the
        // resting mark fills nothing - and `Unit303OptionBTests.TheQuillIsFilledInBothStates`
        // asserts that it fills something. **Both were written from a ruling and the
        // rulings are a day apart**: unit 300 built an outlined mark at rest, and on
        // 2026-09-10 Tim was shown three treatments of it and chose option B, *filled
        // green at rest*, because the outline was *not noticeable*. Option B is the
        // later ruling and it is the one on the screen, so the assertion below is the
        // one that goes, and this comment is here so the next reader knows it was a
        // contradiction that was resolved rather than a check that was quietly dropped.
        //
        // **WHAT UNIT 300 PUT HERE THAT SURVIVES IS THE RING**, which was always the
        // carrier that mattered and is now the only one.
        foreach (var side in Sizes)
        {
            var rest = Drawn(side, lit: false);
            var lit = Drawn(side, lit: true);

            Assert.True(
                lit.Count > rest.Count,
                "at " + side + " px the lit mark draws " + lit.Count
                + " shapes and the resting one draws " + rest.Count
                + ", so nothing about the difference survives greyscale");

            Assert.True(
                rest.Count > 0,
                "the resting mark drew nothing at all at " + side + " px");

            // **FILLED IN BOTH STATES** (Tim, 2026-09-10, option B of three).
            Assert.True(
                rest.Any(d => d.Brush is not null),
                "the resting mark filled nothing at " + side
                + " px, so it is a sliver again and option B is undone");

            Assert.True(
                lit.Any(d => d.Brush is not null),
                "the lit mark filled nothing at " + side + " px");
        }
    }

    /// <summary>**The drawn quill is the height that was ruled, and the box is not.**</summary>
    /// <remarks>
    /// <para>**THIS IS THE MEASUREMENT UNITS 300 TO 303 NEVER TOOK** (work instruction
    /// 330 task 1). Three tests in this repository checked that the mark's **box** was
    /// 27 px and every one of them passed while the **drawing** in it was 10.6 px, which
    /// is why Tim made the same complaint on 2026-09-09 and again on 2026-09-12.</para>
    /// <para>**IT ASSERTS THE INK.** The vane's height is taken off the geometry the
    /// renderer emitted, scaled by the transform it was emitted under, which is the
    /// figure a ruler would give.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheDrawnQuillIsTheRuledHeight()
    {
        var side = AchievementMarkControl.TraySide;

        foreach (var lit in new[] { false, true })
        {
            var mark = Arranged(side, lit);

            _output.WriteLine(
                (lit ? "something new" : "at rest      ")
                + " : box " + side.ToString("0.0", CultureInfo.InvariantCulture)
                + " px, quill "
                + mark.DrawnQuillHeight.ToString("0.0", CultureInfo.InvariantCulture)
                + " px tall");

            Assert.Equal(AchievementMarkControl.TrayQuill, mark.DrawnQuillHeight, 1);
        }

        // **AND IT FOLLOWS THE BOX AT ANY SIZE**, so a surface that asks for a smaller
        // mark gets a smaller quill rather than the same 10.6 px it always got.
        foreach (var box in Sizes)
        {
            var mark = Arranged(box, lit: false);

            _output.WriteLine(
                box.ToString("0", CultureInfo.InvariantCulture).PadLeft(3)
                + " px box: quill "
                + mark.DrawnQuillHeight.ToString("0.0", CultureInfo.InvariantCulture)
                + " px tall");

            Assert.True(
                mark.DrawnQuillHeight > box * 0.8,
                "at a " + box + " px box the quill is only "
                + mark.DrawnQuillHeight.ToString("0.0", CultureInfo.InvariantCulture)
                + " px tall, so the box is still bigger than the mark in it");
        }
    }

    /// <summary>
    /// **What the quill works out to on the glass - which is arithmetic and is not a
    /// look.**
    /// </summary>
    /// <remarks>
    /// <para>**THE RECORDER GIVES THE QUILL'S GEOMETRY IN THE FILE'S OWN COORDINATE
    /// SPACE**, 44 units square, because the scaling lives in the transform the
    /// drawing sits under rather than in the geometry itself. So
    /// <see cref="WhatTheMarkDrawsAtEachSize"/> prints `14.0 x 28.0` at every size,
    /// and that is correct and is not the size on the glass.</para>
    /// <para>**THIS DOES THE MULTIPLICATION AND LABELS IT AS MULTIPLICATION** (the
    /// instruction: *do not compute a stroke width and call it a look*). It is here
    /// so the figure in the report is reproducible and so the one number worth
    /// worrying about is written down: **at 16 px the quill's own outline works out
    /// to about half a device pixel**, and whether that renders as a line or as
    /// nothing is exactly the question nothing in this repository can answer.</para>
    /// </remarks>
    [AvaloniaFact]
    public void WhatTheQuillWorksOutToOnTheGlass()
    {
        _output.WriteLine(
            "ARITHMETIC, NOT A LOOK. Nothing below was rasterised or looked at.");
        _output.WriteLine("");

        // **THE ARITHMETIC IS THE RENDERER'S AND IT CHANGED IN 330 TASK 1.** It used
        // to scale the file's whole 44-unit viewBox to 62 per cent of the box, and the
        // vane is 28 of those 44 units, so the drawn quill came out at 39 per cent of
        // whatever number the markup asked for - a 27 px mark drew 10.6 px of quill.
        // It now scales the **vane's own height** to `TrayQuill / TraySide` of the box.
        foreach (var side in new[] { 16.0, 24.0, 32.0, AchievementMarkControl.TraySide })
        {
            var tall = side * AchievementMarkControl.TrayQuill
                / AchievementMarkControl.TraySide;
            var scale = tall / 28.0;

            _output.WriteLine(
                side.ToString("0", CultureInfo.InvariantCulture).PadLeft(3)
                + " px box: quill "
                + (14.0 * scale).ToString("0.0", CultureInfo.InvariantCulture)
                + " x " + tall.ToString("0.0", CultureInfo.InvariantCulture)
                + " px, its outline "
                + (2.2 * scale).ToString("0.00", CultureInfo.InvariantCulture)
                + " px, the spine "
                + (1.7 * scale).ToString("0.00", CultureInfo.InvariantCulture)
                + " px, the ring "
                + (side - 2).ToString("0.0", CultureInfo.InvariantCulture)
                + " px across at 1.6 px, its bead "
                + (side * 0.22).ToString("0.0", CultureInfo.InvariantCulture)
                + " px");

            _output.WriteLine(
                "         what it drew before 330: quill "
                + (14.0 * side * 0.62 / AchievementQuill.Side)
                    .ToString("0.0", CultureInfo.InvariantCulture)
                + " x "
                + (28.0 * side * 0.62 / AchievementQuill.Side)
                    .ToString("0.0", CultureInfo.InvariantCulture)
                + " px");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "The bead scales with the box. It did not until this measurement was "
            + "taken: at a fixed 4.4 px it was a third of the width of the 7 px "
            + "ring it runs round in a 16 px box.");
        _output.WriteLine(
            "The vane's tip is half of the quill's height from the middle, and the "
            + "ring's radius is one less than half the box, which is why the box is "
            + AchievementMarkControl.TraySide.ToString("0", CultureInfo.InvariantCulture)
            + " and the ink is "
            + AchievementMarkControl.TrayQuill.ToString("0", CultureInfo.InvariantCulture)
            + ".");
    }

    /// <summary>**Whether anything in this repository can look at a pixel.**</summary>
    /// <remarks>
    /// **THIS TEST PASSES WHATEVER IT FINDS** and exists to print the finding. If it
    /// ever reports a real raster, the honest answer to *did anybody look* changes,
    /// and a unit that carried the old answer forward without re-testing would never
    /// notice.
    /// </remarks>
    [AvaloniaFact]
    public void WhetherAnythingHereCanActuallyLook()
    {
        var mark = Arranged(32, lit: true);

        try
        {
            var bitmap = new RenderTargetBitmap(new PixelSize(32, 32));

            bitmap.Render(mark);

            var file = Path.Combine(
                Path.GetTempPath(), "hamlet-unit300-mark.png");

            bitmap.Save(file);

            var length = new FileInfo(file).Length;

            _output.WriteLine("a raster WAS produced: " + file
                + ", " + length + " bytes");

            var pixels = new byte[32 * 32 * 4];

            bitmap.CopyPixels(
                new PixelRect(0, 0, 32, 32), Marshal(pixels), pixels.Length, 32 * 4);

            var ink = 0;

            for (var i = 3; i < pixels.Length; i += 4)
            {
                if (pixels[i] > 0)
                {
                    ink++;
                }
            }

            _output.WriteLine("pixels carrying ink: " + ink + " of 1024");
        }
        catch (Exception error)
        {
            // **THE MEASUREMENT IS THE EXCEPTION**, and it is printed verbatim
            // rather than summarised, because the next unit to ask this question
            // should be able to tell whether it is the same wall.
            _output.WriteLine(
                "nothing here can look at a pixel: "
                + error.GetType().Name + ": " + error.Message);
        }
    }

    private static IntPtr Marshal(byte[] buffer)
        => System.Runtime.InteropServices.GCHandle
            .Alloc(buffer, System.Runtime.InteropServices.GCHandleType.Pinned)
            .AddrOfPinnedObject();

    /// <summary>The mark, laid out at one size, in one state.</summary>
    private static AchievementMarkControl Arranged(double side, bool lit)
    {
        var mark = new AchievementMarkControl
        {
            Width = side,
            Height = side,
            IsNew = lit,
        };

        mark.Measure(new Size(side, side));
        mark.Arrange(new Rect(0, 0, side, side));

        return mark;
    }

    /// <summary>
    /// **What the control's own `Render` emitted**, recorded rather than recomputed.
    /// </summary>
    private static IReadOnlyList<GeometryDrawing> Drawn(double side, bool lit)
    {
        var mark = Arranged(side, lit);
        var group = new DrawingGroup();

        using (var context = group.Open())
        {
            mark.Render(context);
        }

        // **THE QUILL IS DRAWN INSIDE A TRANSFORM AND THE RECORDER NESTS IT.**
        // Measured: reading only the top level reports the resting mark drawing
        // **nothing at all**, because everything but the ring and its bead is under
        // one `PushTransform`. A count taken at the top would have said the mark was
        // empty at rest, which is exactly the wrong thing to report about a mark
        // whose resting state is half of what this unit is for.
        var flat = new List<GeometryDrawing>();

        Flatten(group, flat);

        return flat;
    }

    /// <summary>Every geometry the recorder holds, however deeply it nested it.</summary>
    private static void Flatten(DrawingGroup group, List<GeometryDrawing> into)
    {
        foreach (var child in group.Children)
        {
            switch (child)
            {
                case GeometryDrawing drawing:
                    into.Add(drawing);
                    break;

                case DrawingGroup nested:
                    Flatten(nested, into);
                    break;

                default:
                    break;
            }
        }
    }

    /// <summary>One line per drawing: what it is, how big, and how it is inked.</summary>
    private static IEnumerable<string> Describe(IReadOnlyList<GeometryDrawing> drawings)
        => drawings.Select(d =>
        {
            var bounds = d.Geometry?.Bounds ?? default;

            return (d.Geometry?.GetType().Name ?? "(none)").PadRight(16)
                + bounds.Width.ToString("0.0", CultureInfo.InvariantCulture)
                + " x " + bounds.Height.ToString("0.0", CultureInfo.InvariantCulture)
                + "   fill " + (d.Brush is null ? "(none)" : "yes")
                + "   pen " + (d.Pen is null
                    ? "(none)"
                    : d.Pen.Thickness.ToString("0.0", CultureInfo.InvariantCulture));
        });
}

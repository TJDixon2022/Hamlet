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
        // that is present or absent and a body that is filled or hollow.
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

            Assert.True(
                rest.All(d => d.Brush is null),
                "the resting mark filled something at " + side + " px");

            Assert.True(
                lit.Any(d => d.Brush is not null),
                "the lit mark filled nothing at " + side + " px");
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

        foreach (var side in Sizes)
        {
            // The renderer's own two lines, run here rather than read off the
            // screen: the art is 62 per cent of the box, in a file 44 units square.
            var art = side * 0.62;
            var scale = art / AchievementQuill.Side;

            _output.WriteLine(
                side.ToString("0", CultureInfo.InvariantCulture).PadLeft(3)
                + " px box: quill "
                + (14.0 * scale).ToString("0.0", CultureInfo.InvariantCulture)
                + " x " + (28.0 * scale).ToString("0.0", CultureInfo.InvariantCulture)
                + " px, its outline "
                + (2.2 * scale).ToString("0.00", CultureInfo.InvariantCulture)
                + " px, the spine "
                + (1.7 * scale).ToString("0.00", CultureInfo.InvariantCulture)
                + " px, the ring "
                + (side / 2 - 1).ToString("0.0", CultureInfo.InvariantCulture)
                + " px across at 1.6 px, its bead "
                + (side * 0.22).ToString("0.0", CultureInfo.InvariantCulture)
                + " px");
        }

        _output.WriteLine("");
        _output.WriteLine(
            "The bead scales with the box. It did not until this measurement was "
            + "taken: at a fixed 4.4 px it was a third of the width of the 7 px "
            + "ring it runs round in a 16 px box.");
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

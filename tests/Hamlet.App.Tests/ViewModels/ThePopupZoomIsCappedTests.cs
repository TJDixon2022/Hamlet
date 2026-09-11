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
/// Work instruction 313 task 2: **the popup zoom is capped.**
/// </summary>
/// <remarks>
/// <para>**R9, Tim 2026-09-11**: *"cap zoom"*. Offered a larger source image or a cap on
/// magnification, he ruled the cap. **The image is not replaced in this unit.**</para>
/// <para>**MEASURED BEFORE** (`Unit313TraceTests`): the `F4DIA` case was framed at
/// 180.7 by 95.3 and drawn at 720 by 379.5, which is **3.98x**, and unit 310's zoom
/// floor permitted **4.13x** at its worst. A floor on the frame was the wrong
/// instrument: tight enough to stay sharp is a floor of the whole file, which is no zoom
/// at all.</para>
/// <para>**THE CAP IS THE AUTHOR'S NUMBER AND NOT TIM'S.** He ruled that there be one
/// and did not name it. It is reproduced in `output.md` section 4 so one word changes
/// it.</para>
/// <para>**COMPUTED, NOT SEEN.** What is asserted is the crop the code chooses and the
/// magnification that follows from it. Nothing here looks at a pixel, and nothing here
/// can say whether the result is sharp.</para>
/// </remarks>
public sealed class ThePopupZoomIsCappedTests
{
    /// <summary>The popup's ceiling, read from `MainWindow.axaml`.</summary>
    /// <remarks>
    /// **THE MARKUP'S OWN NUMBERS, NAMED HERE SO THE TEST SAYS WHERE THEY CAME FROM.**
    /// The code does not hold them: the control caps against whatever it is offered, and
    /// these are what the popup offers it today.
    /// </remarks>
    private const double PopupWidth = 720;

    /// <summary>The popup's ceiling, read from `MainWindow.axaml`.</summary>
    private const double PopupHeight = 400;

    /// <summary>Half a pixel of the source bitmap.</summary>
    /// <remarks>
    /// **THE TOLERANCE, STATED BECAUSE THE INSTRUCTION ASKS.** The worked example is
    /// given to one decimal place and the arithmetic is in doubles, so half a source
    /// pixel is tighter than the example is quoted and far looser than floating point.
    /// </remarks>
    private const double HalfAPixel = 0.5;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the crops are printed.</param>
    public ThePopupZoomIsCappedTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The France case is framed at the cap, not at 3.98x.**</summary>
    /// <remarks>
    /// <para>**THE INSTRUCTION'S WORKED EXAMPLE.** The path box centres on 259.0, 118.3
    /// and at a 2.0x cap the crop is given as x 78.5 to 439.5, y 22.1 to 214.6 - North
    /// America to eastern Europe, wholly inside the bitmap, with the path in it.</para>
    /// <para>**THE CENTRE MATCHES EXACTLY AND THE FOUR EDGES DO NOT**, and the reason is
    /// arithmetic rather than behaviour: the example is worked for a popup of about
    /// 722 by 385, which gives a crop of 361 by 192.5, and `MainWindow.axaml` offers the
    /// map 720 by 400, which gives 360 by 200. **Reported as a mismatch.** What is
    /// asserted here is the centre, the cap, and the crop being the popup's own size
    /// divided by the cap - none of which bakes a ceiling into an expected number.</para>
    /// </remarks>
    [Fact]
    public void TheFranceCaseIsFramedAtTheCap()
    {
        var plot = France();

        var (left, top, width, height) = plot.OpenFrameFor(PopupWidth, PopupHeight);

        _output.WriteLine("was  : " + plot.OpenFrameLine + "  (3.98x, measured)");
        _output.WriteLine("now  : left " + Say(left) + ", top " + Say(top)
            + ", " + Say(width) + " by " + Say(height));
        _output.WriteLine("      x " + Say(left) + " to " + Say(left + width)
            + ", y " + Say(top) + " to " + Say(top + height));
        _output.WriteLine("mag  : " + Say(Magnification(width, height)) + "x");

        Assert.Equal(Ft8GlobePlot.ZoomCap, Magnification(width, height), 3);

        // **THE WORKED EXAMPLE'S CENTRE, WHICH MATCHES EXACTLY.** The instruction gives
        // the crop as x 78.5 to 439.5, y 22.1 to 214.6, centred on 259.0, 118.3.
        var acrossCentre = left + (width / 2);
        var downCentre = top + (height / 2);

        _output.WriteLine("centre: " + Say(acrossCentre) + ", " + Say(downCentre)
            + "   the worked example says 259.0, 118.3");

        Assert.True(
            Math.Abs(acrossCentre - 259.0) < HalfAPixel,
            "the frame centres across at " + Say(acrossCentre));

        Assert.True(
            Math.Abs(downCentre - 118.3) < HalfAPixel,
            "the frame centres down at " + Say(downCentre));

        // **AND THE SIZE IS THE POPUP'S OWN, DIVIDED BY THE CAP** - which is what makes
        // the magnification the cap, and is asserted from the popup rather than from
        // the four numbers the example quotes. **Those four do not match and the report
        // says why**: the example is worked for a popup of about 722 by 385 and the
        // markup offers 720 by 400, so its crop is 361 by 192.5 where this one is
        // 360 by 200. **Same centre, same cap, a different box.**
        Assert.Equal(PopupWidth / Ft8GlobePlot.ZoomCap, width, 6);
        Assert.Equal(PopupHeight / Ft8GlobePlot.ZoomCap, height, 6);
    }

    /// <summary>**The magnification never exceeds the cap.**</summary>
    [Fact]
    public void TheMagnificationNeverExceedsTheCap()
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

            var (left, top, width, height) = plot.OpenFrameFor(PopupWidth, PopupHeight);
            var magnification = Magnification(width, height);

            _output.WriteLine(call.PadRight(7) + grid
                + "  runs " + plot.Path.Count
                + "  frame " + Say(width) + " by " + Say(height)
                + " at " + Say(left) + ", " + Say(top)
                + "  -> " + Say(magnification) + "x");

            Assert.True(
                magnification <= Ft8GlobePlot.ZoomCap + 0.001,
                call + " is magnified " + Say(magnification) + "x");
        }
    }

    /// <summary>**Every sampled path point is still inside the frame.**</summary>
    [Fact]
    public void EverySampledPathPointIsStillInsideTheFrame()
    {
        foreach (var (call, grid) in new[]
        {
            ("F4DIA", "JN36"), ("PY2ABC", "GG66"), ("ZS1ABC", "JF96"),
            ("JA1ABC", "PM95"), ("ZL1ABC", "RF73"), ("W2ABC", "FN20"),
            ("G0ABC", "IO91"), ("UA3ABC", "KO85"),
        })
        {
            var plot = new Ft8GlobePlot("FN00DJ", grid, call, "");

            var (left, top, width, height) = plot.OpenFrameFor(PopupWidth, PopupHeight);

            var points = plot.Path.SelectMany(r => r).ToList();

            Assert.NotEmpty(points);

            if (plot.HasOperator)
            {
                points.Add((plot.OperatorX, plot.OperatorY));
            }

            if (plot.HasStation)
            {
                points.Add((plot.StationX, plot.StationY));
            }

            var outside = points
                .Where(p => p.Item1 < left || p.Item1 > left + width
                    || p.Item2 < top || p.Item2 > top + height)
                .ToList();

            _output.WriteLine(call.PadRight(7) + grid + "  " + points.Count
                + " points, " + outside.Count + " outside");

            Assert.Empty(outside);
        }
    }

    /// <summary>**A frame that would run off an edge is slid inside.**</summary>
    /// <remarks>
    /// **THE MAP COVERS x 0 TO 697 AND y 0 TO 380 AND NOTHING MAY HANG OFF IT.** A path
    /// near an edge pushes the enlarged frame out, and drawing past the edge of the
    /// picture would be drawing ground Hamlet has no picture of.
    /// </remarks>
    [Fact]
    public void AFrameThatWouldRunOffAnEdgeIsSlidInside()
    {
        // **GRIDS NEAR THE EDGES OF WHAT THE FILE COVERS.** A path to each of them
        // pushes the enlarged frame against a different side.
        foreach (var (call, grid) in new[]
        {
            ("KL7ABC", "BP51"), ("ZS1ABC", "JF96"), ("PY2ABC", "GG66"),
            ("OX3ABC", "GP60"), ("VE8ABC", "DP38"),
        })
        {
            var plot = new Ft8GlobePlot("FN00DJ", grid, call, "");

            if (plot.Path.Count == 0)
            {
                _output.WriteLine(call.PadRight(7) + grid + "  no path, skipped");
                continue;
            }

            var (left, top, width, height) = plot.OpenFrameFor(PopupWidth, PopupHeight);

            _output.WriteLine(call.PadRight(7) + grid
                + "  x " + Say(left) + " to " + Say(left + width)
                + ", y " + Say(top) + " to " + Say(top + height));

            Assert.True(left >= -0.001, call + ": left is " + Say(left));
            Assert.True(top >= -0.001, call + ": top is " + Say(top));

            Assert.True(
                left + width <= Ft8GlobePlot.Map.WidthPixels + 0.001,
                call + ": right is " + Say(left + width));

            Assert.True(
                top + height <= Ft8GlobePlot.Map.HeightPixels + 0.001,
                call + ": bottom is " + Say(top + height));

            // **AND THE PATH IS STILL IN IT AFTER THE SLIDE**, which is the half a
            // clamp most easily breaks.
            foreach (var (x, y) in plot.Path.SelectMany(r => r))
            {
                Assert.True(
                    x >= left - 0.001 && x <= left + width + 0.001
                    && y >= top - 0.001 && y <= top + height + 0.001,
                    call + ": the slide put a path point outside the frame");
            }
        }
    }

    /// <summary>**A date-line path still shows the whole world.**</summary>
    [Fact]
    public void ADateLinePathStillShowsTheWholeWorld()
    {
        var plot = new Ft8GlobePlot("FN00DJ", "RE78", "ZL1ABC", "");

        Assert.True(plot.Path.Count > 1, "this path does not cross the date line");

        var (left, top, width, height) = plot.OpenFrameFor(PopupWidth, PopupHeight);

        _output.WriteLine("runs " + plot.Path.Count + ", frame "
            + Say(width) + " by " + Say(height) + " at " + Say(left) + ", " + Say(top));

        Assert.Equal(0, left);
        Assert.Equal(0, top);
        Assert.Equal(Ft8GlobePlot.Map.WidthPixels, width);
        Assert.Equal(Ft8GlobePlot.Map.HeightPixels, height);
    }

    /// <summary>**The cap lives in one named constant.**</summary>
    /// <remarks>
    /// **ONE PLACE, SO ONE WORD CHANGES IT.** A number in two places is a number that
    /// gets changed in one of them.
    /// </remarks>
    [Fact]
    public void TheCapLivesInOneNamedConstant()
    {
        _output.WriteLine("Ft8GlobePlot.ZoomCap = " + Ft8GlobePlot.ZoomCap);

        Assert.Equal(2.0, Ft8GlobePlot.ZoomCap);

        // **AND THE CONTROL ASKS THE PLOT RATHER THAN HOLDING ITS OWN.** A second copy
        // here would be the drift this test exists to prevent.
        var plot = France();

        var mine = plot.OpenFrameFor(PopupWidth, PopupHeight);
        var theirs = Ft8GlobeControl.FrameOf(plot, true, PopupWidth, PopupHeight);

        _output.WriteLine("the plot says  " + Say(mine.Width) + " by " + Say(mine.Height));
        _output.WriteLine("the control says " + Say(theirs.Width) + " by " + Say(theirs.Height));

        Assert.Equal(mine.Left, theirs.Left, 6);
        Assert.Equal(mine.Top, theirs.Top, 6);
        Assert.Equal(mine.Width, theirs.Width, 6);
        Assert.Equal(mine.Height, theirs.Height, 6);
    }

    private static Ft8GlobePlot France()
        => new("FN00DJ", "JN36", "F4DIA", "France");

    /// <summary>How much the bitmap is enlarged, drawing this frame in the popup.</summary>
    private static double Magnification(double width, double height)
        => Math.Min(PopupWidth / width, PopupHeight / height);

    private static string Say(double value)
        => value.ToString("0.0", CultureInfo.InvariantCulture);
}

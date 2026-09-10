using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Work instruction 301 task 1: **the `i` mark can be hit.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT IS THE ONE TIM FOUND BY USING IT.**
/// The mark drew its ring with `DrawEllipse(null, pen, ...)` - **a null brush, so
/// nothing was painted inside it and nothing was there to hit.** Unit 299 measured
/// the tooltip and found it bound and full; **the sentence was never the problem and
/// the target always was.** The mark is used 42 times across eight windows, so every
/// one of them has been this hard to hit since it was built.</para>
/// <para>**IT WAS WATCHED FAILING**, and the measured answer was worse than the
/// instruction's: against the tree as it stood, **nine points across the mark
/// including the centre found nothing at all**, so it was not a one-pixel outline
/// but no target whatever.</para>
/// <para>**AND THE HARNESS IS CARRIED IN THE TEST RATHER THAN TRUSTED**, because it
/// cannot be. `InputHitTest` here answers about whichever headless window the
/// harness believes is on top, so the same assertion passes alone and fails in a
/// class: **a plain `Border` with a transparent background - the shape unit 299's
/// globe uses, and the one Tim says works - reports `nothing` too under exactly
/// those conditions.** A red from a bare hit test would therefore be no evidence
/// about this control at all. So the known-good `Border` sits in the same window as
/// the mark and is hit at the same moment: **where the harness cannot see the
/// `Border` it is not asked about the mark**, and where it can, the two must agree.
/// </para>
/// </remarks>
public sealed class Unit301HintTargetTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where what the pointer found is printed.</param>
    public Unit301HintTargetTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The mark is as hittable as the shape Tim says works.**</summary>
    [AvaloniaFact]
    public void TheMarkIsAsHittableAsTheShapeThatWorks()
    {
        var mark = new HintMarkControl
        {
            Kind = HintKind.Detail,
            Text = "The detail this mark is holding.",
        };

        // **THE CONTROL FOR THE HARNESS**, in the same window and hit in the same
        // breath: unit 299's globe is a `Border` with a transparent background, Tim
        // compared the two by hand, and the globe is the one that works.
        var known = new Border { Background = Brushes.Transparent };

        var window = Shown(mark, known);

        try
        {
            var onKnown = At(known, 0.5, 0.5);
            var onMark = At(mark, 0.5, 0.5);

            _output.WriteLine("mark bounds : " + mark.Bounds);
            _output.WriteLine("transparent Border, centre : "
                + (onKnown?.GetType().Name ?? "nothing"));
            _output.WriteLine("HintMarkControl,    centre : "
                + (onMark?.GetType().Name ?? "nothing"));

            if (onKnown is null)
            {
                _output.WriteLine("");
                _output.WriteLine(
                    "THE HARNESS COULD NOT SEE THE KNOWN-GOOD SHAPE EITHER, so it "
                    + "was not asked about the mark. Nothing here is evidence "
                    + "against the control.");

                return;
            }

            Assert.True(
                onMark is HintMarkControl,
                "the harness found the known-good Border but the middle of the mark "
                + "found " + (onMark?.GetType().Name ?? "nothing")
                + ", so the mark's middle still belongs to whatever is behind it");

            // **A CENTRE THAT WORKS AND AN EDGE THAT DOES NOT IS STILL A BAD
            // TARGET**, so nine points are tried rather than one.
            var missed = new List<string>();

            foreach (var dx in new[] { 0.25, 0.5, 0.75 })
            {
                foreach (var dy in new[] { 0.25, 0.5, 0.75 })
                {
                    if (At(mark, dx, dy) is not HintMarkControl)
                    {
                        missed.Add(dx + "," + dy);
                    }
                }
            }

            _output.WriteLine("points tried : 9");
            _output.WriteLine("points missed: " + missed.Count
                + (missed.Count == 0 ? "" : "  " + string.Join(" ", missed)));

            Assert.Empty(missed);
        }
        finally
        {
            Done(window);
        }
    }

    /// <summary>
    /// **The mark paints something over its whole box, which is what a target is.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE DETERMINISTIC HALF AND IT IS THE ONE THAT CANNOT LIE.**
    /// Where the hit test depends on which window the harness thinks is on top, what
    /// `Render` emits does not depend on anything: either there is a fill covering
    /// the control's own bounds or there is not, and before this unit there was
    /// not.</para>
    /// <para>**IT IS A RECTANGLE AND NOT THE ELLIPSE, AND THAT WAS MEASURED.** Giving
    /// the drawn ring a transparent fill was tried first and did not make the mark
    /// hittable; a transparent rectangle over the bounds - the same shape a `Border`
    /// paints for its background - did.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheMarkPaintsATargetOverItsWholeBox()
    {
        var mark = new HintMarkControl
        {
            Kind = HintKind.Detail,
            Text = "Something to say.",
        };

        var window = Shown(mark, new Border());

        try
        {
            var drawings = Recorded(mark);

            foreach (var drawing in drawings)
            {
                var bounds = drawing.Geometry?.Bounds ?? default;

                _output.WriteLine(
                    (drawing.Geometry?.GetType().Name ?? "(none)").PadRight(18)
                    + bounds.Width.ToString("0.0") + " x "
                    + bounds.Height.ToString("0.0")
                    + "   pen " + (drawing.Pen is null ? "(none)" : "yes")
                    + "   fill " + Describe(drawing.Brush));
            }

            var target = drawings.FirstOrDefault(
                d => d.Brush is not null
                     && (d.Geometry?.Bounds.Width ?? 0) >= mark.Bounds.Width
                     && (d.Geometry?.Bounds.Height ?? 0) >= mark.Bounds.Height);

            Assert.True(
                target is not null,
                "nothing the mark draws covers its own box, so there is nothing "
                + "under the pointer anywhere except the outline");

            // **AND IT IS INVISIBLE**, which is the instruction's other half: only
            // the target changes, never the drawing.
            Assert.True(
                target!.Brush is ISolidColorBrush { Color.A: 0 },
                "the mark's target is painted in something a person can see");

            // **THE RING IS STILL A RING** rather than having become a filled dot.
            Assert.Contains(drawings, d => d.Pen is not null && d.Brush is null);
        }
        finally
        {
            Done(window);
        }
    }

    /// <summary>**The drawing did not change: same size.**</summary>
    [AvaloniaFact]
    public void TheMarkIsStillFourteenAcross()
    {
        var mark = new HintMarkControl
        {
            Kind = HintKind.Detail,
            Text = "Something to say.",
        };

        var window = Shown(mark, new Border());

        try
        {
            _output.WriteLine(
                "size: " + mark.Bounds.Width + " by " + mark.Bounds.Height);

            Assert.Equal(14, mark.Bounds.Width, 3);
            Assert.Equal(14, mark.Bounds.Height, 3);
        }
        finally
        {
            Done(window);
        }
    }

    /// <summary>**A mark holding nothing is still not a target.**</summary>
    /// <remarks>
    /// **AN EMPTY MARK IS A HOVER TARGET FOR A SENTENCE THAT DOES NOT EXIST**, which
    /// teaches somebody that hovering is not worth it. That rule is older than this
    /// unit and giving the mark a background must not quietly break it.
    /// </remarks>
    [AvaloniaFact]
    public void AMarkHoldingNothingIsStillNotATarget()
    {
        var mark = new HintMarkControl { Kind = HintKind.Detail, Text = "" };

        var window = Shown(mark, new Border());

        try
        {
            _output.WriteLine("empty mark bounds: " + mark.Bounds);

            Assert.True(
                mark.Bounds.Width <= 0 || mark.Bounds.Height <= 0,
                "an empty mark took up " + mark.Bounds.Width + " by "
                + mark.Bounds.Height);
        }
        finally
        {
            Done(window);
        }
    }

    /// <summary>Hit test one control in its own coordinates.</summary>
    private static IInputElement? At(Control control, double dx, double dy)
        => control.InputHitTest(
            new Point(control.Bounds.Width * dx, control.Bounds.Height * dy));

    /// <summary>What a brush is, in a word, for the printout.</summary>
    private static string Describe(IBrush? brush)
        => brush switch
        {
            null => "(none)",
            ISolidColorBrush solid when solid.Color.A == 0 => "transparent",
            ISolidColorBrush solid => solid.Color.ToString(),
            _ => brush.GetType().Name,
        };

    /// <summary>What the control's own `Render` emitted.</summary>
    private static IReadOnlyList<GeometryDrawing> Recorded(Control control)
    {
        var group = new DrawingGroup();

        using (var context = group.Open())
        {
            control.Render(context);
        }

        var flat = new List<GeometryDrawing>();

        Flatten(group, flat);

        return flat;
    }

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

    /// <summary>A shown window holding the mark and the known-good shape.</summary>
    private static Window Shown(Control mark, Control known)
    {
        known.Width = 14;
        known.Height = 14;

        var window = new Window
        {
            Width = 140,
            Height = 80,
            Content = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 20,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Children = { mark, known },
            },
        };

        window.Show();

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }

        return window;
    }

    /// <summary>Close a window and let the harness actually let go of it.</summary>
    private static void Done(Window window)
    {
        window.Close();

        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        }
    }
}

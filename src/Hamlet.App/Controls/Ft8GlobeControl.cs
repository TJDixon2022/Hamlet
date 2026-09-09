using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Hamlet.App.ViewModels;

namespace Hamlet.App.Controls;

/// <summary>
/// **A small map: the coastline, the operator, the station, and a line between.**
/// </summary>
/// <remarks>
/// <para>**§0.0 BINDS A PICTURE AS HARD AS A SENTENCE** (HM-DEC-092), and this
/// unit's exposure is a dot in the wrong place, which nobody would ever check. So
/// the dots are placed by <see cref="Ft8GlobePlot"/>, which uses **the coastline's
/// own projection, written in its own `&lt;desc&gt;`** - there is not a second one
/// anywhere in this control.</para>
/// <para>**IT DRAWS WHAT IS KNOWN AND NOTHING ELSE.** No station grid means no
/// station dot and no line; no operator grid means no operator dot. The caption
/// beside it says which, so an absence is never left as a picture of one dot the
/// reader has to interpret.</para>
/// <para>**THE COASTLINE IS NOT SURVEY DATA** and `assets/world-coastline.md` says
/// so. At this size fidelity buys nothing, and drawing it removed a download, a
/// licence question and a network call from the unit that built it.</para>
/// <para>**IT IS A `Control` AND NOT A COMPOSITION.** A Border wrapping a Canvas
/// wrapping shapes would hand the pointer several hit targets for one picture, and
/// the whole thing lives inside a tooltip that must behave as one object.</para>
/// </remarks>
public sealed class Ft8GlobeControl : Control
{
    /// <summary>Where the coastline lives.</summary>
    public const string CoastlineUri =
        "avares://Hamlet.App/Assets/world-coastline.svg";

    /// <summary>What is being plotted.</summary>
    public static readonly StyledProperty<Ft8GlobePlot?> PlotProperty =
        AvaloniaProperty.Register<Ft8GlobeControl, Ft8GlobePlot?>(nameof(Plot));

    private static readonly Lazy<IReadOnlyList<GeometryDrawing>> Land =
        new(() => SvgMark.Shapes(CoastlineUri));

    private static readonly IBrush Sea = new SolidColorBrush(Color.Parse("#DDE6EC"));
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#5F5C53"));
    private static readonly IBrush Mine = new SolidColorBrush(Color.Parse("#C8842A"));
    private static readonly IBrush Theirs = new SolidColorBrush(Color.Parse("#2F7D4F"));

    static Ft8GlobeControl() => AffectsRender<Ft8GlobeControl>(PlotProperty);

    /// <summary>What is being plotted.</summary>
    public Ft8GlobePlot? Plot
    {
        get => GetValue(PlotProperty);
        set => SetValue(PlotProperty, value);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (Plot is not { } plot || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        var (left, top, width, height) = plot.Frame;

        if (width <= 0 || height <= 0)
        {
            return;
        }

        var scale = Math.Min(Bounds.Width / width, Bounds.Height / height);

        context.FillRectangle(Sea, new Rect(Bounds.Size));

        // **THE COASTLINE, THROUGH THE FRAME**, so the land and the dots are placed
        // by one transform and cannot come apart.
        using (context.PushClip(new Rect(Bounds.Size)))
        using (context.PushTransform(
            Matrix.CreateTranslation(-left, -top) * Matrix.CreateScale(scale, scale)))
        {
            foreach (var shape in Land.Value)
            {
                context.DrawGeometry(shape.Brush, shape.Pen, shape.Geometry!);
            }
        }

        // **THE LINE AND THE DOTS ARE DRAWN AT SCREEN SCALE**, so a wide frame does
        // not turn a dot into a smear or a hairline.
        Point At(double x, double y)
            => new((x - left) * scale, (y - top) * scale);

        if (plot.HasPath)
        {
            context.DrawLine(
                new Pen(Ink, 1.4, new DashStyle(new double[] { 3, 2 }, 0)),
                At(plot.OperatorX, plot.OperatorY),
                At(plot.StationX, plot.StationY));
        }

        if (plot.HasOperator)
        {
            Dot(context, At(plot.OperatorX, plot.OperatorY), Mine);
        }

        if (plot.HasStation)
        {
            Dot(context, At(plot.StationX, plot.StationY), Theirs);
        }
    }

    /// <summary>One station, drawn so it survives grayscale.</summary>
    /// <remarks>
    /// **THE TWO DOTS DIFFER BY MORE THAN A HUE** (§0.6): the operator's is a ring
    /// and the station's is filled, so a reader who cannot tell the colors apart can
    /// still tell which is which, and the caption names them in words as well.
    /// </remarks>
    private static void Dot(DrawingContext context, Point at, IBrush ink)
    {
        var filled = ReferenceEquals(ink, Theirs);

        context.DrawEllipse(
            filled ? ink : null, new Pen(ink, 2), at, 4, 4);
    }
}

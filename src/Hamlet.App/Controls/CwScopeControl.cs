using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.App.Controls;

/// <summary>What one labelled line on the scope is.</summary>
public enum CwScopeLineKind
{
    /// <summary>The energy across the passband, hop by hop: a thin trace.</summary>
    Envelope,

    /// <summary>The tracked noise floor: a dashed line.</summary>
    Floor,

    /// <summary>The floor plus the margin: a solid, heavier line.</summary>
    Threshold,

    /// <summary>Where the energy is summed: a shaded band on the rail at the top.</summary>
    Passband,
}

/// <summary>One line the scope draws, with its words.</summary>
/// <param name="Kind">What it is.</param>
/// <param name="Label">The words it carries.</param>
public sealed record CwScopeLine(CwScopeLineKind Kind, string Label);

/// <summary>One mark, as a bar along the bottom.</summary>
/// <param name="X">Where it starts, in pixels from the left.</param>
/// <param name="X2">Where it ends.</param>
/// <param name="Label">"mark 180 ms".</param>
public sealed record CwScopeBar(double X, double X2, string Label);

/// <summary>
/// **THE OSCILLOSCOPE: THE LAST FOUR SECONDS, AND A BAR WHEREVER THE TRACE STOOD OVER THE
/// THRESHOLD** (work instruction 476 task 2, step 12 criterion 12.2, R90, HM-DEC-185).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"Think you're an oscilloscope."* The envelope is drawn as a
/// trace, the floor as a dashed line under it, the threshold as a solid line above the floor
/// labelled with its margin, and the marks as bars along the bottom, so the owner sees dits
/// and dahs light up as he hears them.</para>
/// <para>**EVERY HOP IS DRAWN; THE SCREEN IS REDRAWN TWENTY TIMES A SECOND.** The detector
/// works at 5 ms, two hundred hops a second, and a frame carries all of them, so a dit is
/// its true width; only how often the picture is refreshed is 20 a second.</para>
/// <para>**WORDS ON EVERY MARK, NEVER COLOR ALONE** (§0.6): the lines differ in shape - thin,
/// dashed, heavy - and each is named at the right-hand end; every bar carries its length
/// where it fits and on the line beneath; the passband is labelled with its numbers.</para>
/// <para>**IT SHOWS AND CHANGES NOTHING.** Everything here is the detector's, read through the
/// view model; nothing is handed back.</para>
/// </remarks>
public sealed class CwScopeControl : Control
{
    /// <summary>The margin at each side, in pixels.</summary>
    public const double Pad = 6;

    /// <summary>What to draw.</summary>
    public static readonly StyledProperty<CwScopeFrame?> FrameProperty =
        AvaloniaProperty.Register<CwScopeControl, CwScopeFrame?>(nameof(Frame));

    private const double RailHeight = 14;
    private const double PlotHeight = 84;
    private const double BarHeight = 10;
    private const double BarLabelHeight = 12;
    private const double Gap = 3;
    private const double LabelRoom = 150;
    private const double DbSpanAtLeast = 30;

    private static readonly IBrush Rail = new SolidColorBrush(Color.Parse("#E8ECEF"));
    private static readonly IBrush Passband = new SolidColorBrush(Color.Parse("#C9DDF0"));
    private static readonly IBrush Plot = new SolidColorBrush(Color.Parse("#F6F8F9"));
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#44505A"));
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#7A8590"));
    private static readonly IBrush MarkInk = new SolidColorBrush(Color.Parse("#3B6D11"));
    private static readonly IBrush ThresholdInk = new SolidColorBrush(Color.Parse("#B06A10"));
    private static readonly Pen EnvelopePen = new(Ink, 1);
    private static readonly Pen FloorPen = new(Muted, 1, new DashStyle(new double[] { 3, 3 }, 0));
    private static readonly Pen ThresholdPen = new(ThresholdInk, 2);
    private static readonly Pen EdgePen = new(new SolidColorBrush(Color.Parse("#9AA5AF")), 1);
    private static readonly Pen TonePen = new(MarkInk, 2);

    static CwScopeControl()
    {
        AffectsRender<CwScopeControl>(FrameProperty);
    }

    /// <summary>What to draw.</summary>
    public CwScopeFrame? Frame
    {
        get => GetValue(FrameProperty);
        set => SetValue(FrameProperty, value);
    }

    /// <summary>How many hops four seconds hold at this frame's hop.</summary>
    /// <param name="frame">The frame.</param>
    /// <returns>The hops the width stands for.</returns>
    public static int HopsAcross(CwScopeFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        return frame.HopMs > 0
            ? (int)Math.Ceiling(CwEnvelopeDetector.HistorySeconds * 1000 / frame.HopMs)
            : Math.Max(1, frame.Hops.Count);
    }

    /// <summary>Where the start of a hop lands; the newest hop ends at the right edge.</summary>
    /// <param name="index">The hop, oldest 0; <c>Hops.Count</c> is the right edge.</param>
    /// <param name="frame">The frame.</param>
    /// <param name="width">The control's width.</param>
    /// <returns>Pixels from the left.</returns>
    public static double XOfHop(int index, CwScopeFrame frame, double width)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var across = Math.Max(HopsAcross(frame), frame.Hops.Count);
        var slot = index + (across - frame.Hops.Count);

        return Pad + ((double)slot / across * Math.Max(0, width - (2 * Pad)));
    }

    /// <summary>Every mark in the frame, as a bar with its length.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="width">The control's width.</param>
    /// <returns>One bar per run of marked hops, oldest first.</returns>
    public static IReadOnlyList<CwScopeBar> Bars(CwScopeFrame frame, double width)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var bars = new List<CwScopeBar>();
        var start = -1;

        for (var i = 0; i <= frame.Hops.Count; i++)
        {
            var mark = i < frame.Hops.Count && frame.Hops[i].Mark;

            if (mark && start < 0)
            {
                start = i;
            }
            else if (!mark && start >= 0)
            {
                bars.Add(new CwScopeBar(
                    XOfHop(start, frame, width),
                    XOfHop(i, frame, width),
                    string.Create(CultureInfo.InvariantCulture, $"mark {(i - start) * frame.HopMs:0} ms")));
                start = -1;
            }
        }

        return bars;
    }

    /// <summary>Every line the scope draws, with its words.</summary>
    /// <param name="frame">The frame.</param>
    /// <returns>The trace, the floor, the threshold and the passband.</returns>
    public static IReadOnlyList<CwScopeLine> Lines(CwScopeFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        return new[]
        {
            new CwScopeLine(CwScopeLineKind.Envelope, "envelope"),
            new CwScopeLine(CwScopeLineKind.Floor, "floor"),
            new CwScopeLine(CwScopeLineKind.Threshold, frame.ThresholdLabel),
            new CwScopeLine(
                CwScopeLineKind.Passband,
                frame.PassbandLabel.Length > 0 ? frame.PassbandLabel : "not listening"),
        };
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => new(
            double.IsInfinity(availableSize.Width) ? 400 : availableSize.Width,
            RailHeight + Gap + PlotHeight + Gap + BarHeight + BarLabelHeight);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (Frame is not { } frame || Bounds.Width <= (2 * Pad) + LabelRoom)
        {
            return;
        }

        var width = Bounds.Width;
        var plotRight = width - LabelRoom;
        var lines = Lines(frame);

        DrawRail(context, frame, width, lines.Single(l => l.Kind == CwScopeLineKind.Passband).Label);

        var plotTop = RailHeight + Gap;
        var plotBottom = plotTop + PlotHeight;

        context.FillRectangle(Plot, new Rect(Pad, plotTop, plotRight - Pad, PlotHeight));

        if (frame.Hops.Count > 1)
        {
            DrawTraces(context, frame, plotRight, plotTop, plotBottom, lines);
        }
        else
        {
            Text(context, frame.ToneLine, Pad + 4, plotTop + 4, Muted, 10, left: true);
        }

        // The marks, along the bottom, each labelled where it is wide enough to carry it.
        var barTop = plotBottom + Gap;

        foreach (var bar in Bars(frame, plotRight + Pad))
        {
            var w = Math.Max(1, bar.X2 - bar.X);
            context.FillRectangle(MarkInk, new Rect(bar.X, barTop, w, BarHeight));

            var label = new FormattedText(
                bar.Label.Replace("mark ", "", StringComparison.Ordinal), CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, Typeface.Default, 9, MarkInk);

            if (label.Width <= w + 4)
            {
                context.DrawText(label, new Point(bar.X, barTop + BarHeight));
            }
        }

        Text(context, "marks", plotRight + 4, barTop - 1, MarkInk, 10, left: true);
    }

    private void DrawRail(DrawingContext context, CwScopeFrame frame, double width, string label)
    {
        var reading = frame.Reading;
        var topHz = Math.Max(CwEnvelopeDetector.WholeBandHighHz, double.IsNaN(reading.PassbandHighHz) ? 0 : reading.PassbandHighHz);

        double X(double hz) => Pad + (Math.Clamp(hz / topHz, 0, 1) * (width - (2 * Pad)));

        context.FillRectangle(Rail, new Rect(Pad, 0, width - (2 * Pad), RailHeight));

        if (!double.IsNaN(reading.PassbandLowHz) && !double.IsNaN(reading.PassbandHighHz))
        {
            var x1 = X(reading.PassbandLowHz);
            var x2 = X(reading.PassbandHighHz);

            context.FillRectangle(Passband, new Rect(x1, 0, Math.Max(1, x2 - x1), RailHeight));
            context.DrawLine(EdgePen, new Point(x1, 0), new Point(x1, RailHeight));
            context.DrawLine(EdgePen, new Point(x2, 0), new Point(x2, RailHeight));
        }

        if (reading.Mark && !double.IsNaN(reading.PitchHz))
        {
            var x = X(reading.PitchHz);
            context.DrawLine(TonePen, new Point(x, 0), new Point(x, RailHeight));
        }

        Text(context, label, width / 2, 1, Ink, 10, left: false);
    }

    private void DrawTraces(
        DrawingContext context, CwScopeFrame frame, double plotRight, double plotTop, double plotBottom,
        IReadOnlyList<CwScopeLine> lines)
    {
        var low = double.PositiveInfinity;
        var high = double.NegativeInfinity;

        foreach (var hop in frame.Hops)
        {
            low = Math.Min(low, hop.FloorDb);
            high = Math.Max(high, Math.Max(hop.EnvelopeDb, hop.ThresholdDb));
        }

        low -= 6;
        high = Math.Max(high + 3, low + DbSpanAtLeast);

        double Y(double db) => plotBottom - ((Math.Clamp(db, low, high) - low) / (high - low) * (plotBottom - plotTop));

        var envelope = Polyline(frame, plotRight, h => h.EnvelopeDb, Y);
        var floor = Polyline(frame, plotRight, h => h.FloorDb, Y);
        var threshold = Polyline(frame, plotRight, h => h.ThresholdDb, Y);

        context.DrawGeometry(null, FloorPen, floor);
        context.DrawGeometry(null, ThresholdPen, threshold);
        context.DrawGeometry(null, EnvelopePen, envelope);

        // Each line named at its right-hand end, beside where it stands now.
        var last = frame.Hops[^1];
        var floorY = Y(last.FloorDb);
        var thresholdY = Y(last.ThresholdDb);

        Text(context, "- - " + lines.Single(l => l.Kind == CwScopeLineKind.Floor).Label, plotRight + 4, floorY - 6, Muted, 10, left: true);
        Text(context, "━ " + lines.Single(l => l.Kind == CwScopeLineKind.Threshold).Label, plotRight + 4, thresholdY - 6, ThresholdInk, 10, left: true);
        Text(context, "─ " + lines.Single(l => l.Kind == CwScopeLineKind.Envelope).Label, plotRight + 4, plotTop, Ink, 10, left: true);
        Text(context, frame.ToneLine, Pad + 4, plotTop + 2, frame.Reading.Mark ? MarkInk : Muted, 11, left: true);
    }

    private static StreamGeometry Polyline(
        CwScopeFrame frame, double plotRight, Func<CwScopeHop, double> value, Func<double, double> y)
    {
        var geometry = new StreamGeometry();

        using var g = geometry.Open();

        for (var i = 0; i < frame.Hops.Count; i++)
        {
            var point = new Point(XOfHop(i, frame, plotRight + Pad), y(value(frame.Hops[i])));

            if (i == 0)
            {
                g.BeginFigure(point, false);
            }
            else
            {
                g.LineTo(point);
            }
        }

        g.EndFigure(false);

        return geometry;
    }

    private void Text(DrawingContext context, string words, double x, double top, IBrush brush, double size, bool left)
    {
        var text = new FormattedText(
            words, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            Typeface.Default, size, brush);

        var at = left ? x : x - (text.Width / 2);
        at = Math.Clamp(at, 0, Math.Max(0, Bounds.Width - text.Width));

        context.DrawText(text, new Point(at, top));
    }
}

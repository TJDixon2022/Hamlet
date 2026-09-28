using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Cw;

namespace Hamlet.App.Controls;

/// <summary>What one thing the scope draws is.</summary>
public enum CwScopeLineKind
{
    /// <summary>The level of the bin the detector reads, hop by hop: one line.</summary>
    Trace,

    /// <summary>The marks: a filled block along the bottom under every marked hop.</summary>
    Bars,

    /// <summary>The detector's pitch, as text in the corner.</summary>
    Tone,

    /// <summary>The tracker's pitch, as text beside it.</summary>
    Mixing,
}

/// <summary>One thing the scope draws, with its words.</summary>
/// <param name="Kind">What it is.</param>
/// <param name="Label">The words it carries.</param>
public sealed record CwScopeLine(CwScopeLineKind Kind, string Label);

/// <summary>What one thing on the training graph's canvas is.</summary>
public enum CwScopeItemKind
{
    /// <summary>The level trace of the watched bin, across the window (R94).</summary>
    Trace,

    /// <summary>A bar the detector found, its true length.</summary>
    Bar,

    /// <summary>A character the decoder settled, over its own span.</summary>
    Letter,

    /// <summary>The small words that fill the empty graph: "listening".</summary>
    Listening,

    /// <summary>The detector's pitch line, as words in the corner.</summary>
    Tone,

    /// <summary>The tracker's pitch line, as words beside it.</summary>
    Mixing,
}

/// <summary>One thing on the canvas, where it is, and its words.</summary>
/// <param name="Kind">What it is.</param>
/// <param name="X">Its left edge, or its center for a letter.</param>
/// <param name="X2">Its right edge; the same as <paramref name="X"/> for words.</param>
/// <param name="Text">What it says, or its hover for a bar.</param>
public sealed record CwScopeItem(CwScopeItemKind Kind, double X, double X2, string Text);

/// <summary>One mark, as a bar along the bottom.</summary>
/// <param name="X">Where it starts, in pixels from the left.</param>
/// <param name="X2">Where it ends.</param>
/// <param name="Label">"mark 180 ms".</param>
public sealed record CwScopeBar(double X, double X2, string Label);

/// <summary>
/// **THE OSCILLOSCOPE: THE LEVEL TRACE, AND A BAR UNDER EVERY MARK** (work instruction 478
/// task 1, step 12 criterion 12.2 rewritten, R92).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-28**: *"We should have replaced the temp controls with something that
/// looks like #2"* - the middle panel of the bars-not-waves picture: a keyed station's trace
/// with flat tops and flat bottoms, and the dits and dahs marked underneath. **That is all it
/// draws.** Units 476 and 477 drew a floor line, a threshold line and a shaded passband; since
/// 477 none of them decides anything (R91), so they are gone (§0.0). The detector still
/// computes them; the verdict row still carries them.</para>
/// <para>**THE TRACE IS ONE BIN**, the one the detector reads. Before any station is found
/// that is the middle of the passband, and after one stops it stays on the last station's
/// pitch, so the trace is never blank; the detector chooses it and this only draws it.</para>
/// <para>**EVERY HOP IS DRAWN; THE SCREEN IS REDRAWN TWENTY TIMES A SECOND.** A dit is its
/// true width.</para>
/// <para>**WORDS FOR EVERYTHING, NEVER COLOR ALONE** (§0.6): the trace and the bars are
/// named at the right-hand end, every bar carries its length where it fits, the pitch is
/// text, and the hover says what the trace is or what a bar is under the pointer.</para>
/// <para>**IT SHOWS AND CHANGES NOTHING.**</para>
/// <para>**SINCE WORK INSTRUCTION 480 IT IS THE TRAINING GRAPH**: it draws <see cref="Items"/>
/// and nothing else, over eight seconds, and the four-second helpers below stay for the frame's
/// own hop arithmetic.</para>
/// <para>**THE TRACE IS BACK** (work instruction 480, the letter over the bars, R94). R95 took it
/// off and drew bars alone, with each settled letter 26 point in a row of its own; on the owner's
/// tab at 14.0685 the detector marked nothing, so the panel was blank and the only thing on it
/// was <c>T T</c>. Now the level of the watched bin is drawn across the eight seconds whether or
/// not anything is marked, the bars sit under it, and the letter row is gone.</para>
/// </remarks>
public sealed class CwScopeControl : Control
{
    /// <summary>The margin at each side, in pixels.</summary>
    public const double Pad = 6;

    /// <summary>The height of the plot: the words at its top, then the trace.</summary>
    public const double PlotHeight = 110;

    /// <summary>Where the trace's band starts, from the top: under the words in the corner.</summary>
    public const double TraceTop = 38;

    /// <summary>Where the row of bars starts, from the top: under the plot.</summary>
    public const double BarRowTop = PlotHeight + Gap;

    /// <summary>Where the training graph's bars start, from the top.</summary>
    public const double BarTop = BarRowTop;

    /// <summary>How tall a bar is on the training graph; its length is its time.</summary>
    public const double TrainingBarHeight = 14;

    /// <summary>A span wide enough that noise alone does not fill the trace's band, in dB.</summary>
    public const double DbSpanAtLeast = 30;

    /// <summary>What to draw.</summary>
    public static readonly StyledProperty<CwScopeFrame?> FrameProperty =
        AvaloniaProperty.Register<CwScopeControl, CwScopeFrame?>(nameof(Frame));

    private const double BarLabelHeight = 12;
    private const double Gap = 3;

    private static readonly IBrush Plot = new SolidColorBrush(Color.Parse("#F6F8F9"));
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#44505A"));
    private static readonly IBrush Muted = new SolidColorBrush(Color.Parse("#7A8590"));
    private static readonly IBrush MarkInk = new SolidColorBrush(Color.Parse("#3B6D11"));
    private static readonly Pen TracePen = new(Ink, 1);

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
    /// <param name="width">The width the four seconds are laid across, pads included.</param>
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
    /// <param name="width">The width the four seconds are laid across, pads included.</param>
    /// <returns>One bar per run of marked hops, oldest first; nothing under a gap.</returns>
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

    /// <summary>The kinds of thing the graph draws, with their words; the trace again since R94.</summary>
    /// <param name="frame">The frame.</param>
    /// <returns>The trace, the bars, the detector's pitch and the tracker's.</returns>
    public static IReadOnlyList<CwScopeLine> Lines(CwScopeFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        return new[]
        {
            new CwScopeLine(CwScopeLineKind.Trace, "level"),
            new CwScopeLine(CwScopeLineKind.Bars, "marks"),
            new CwScopeLine(CwScopeLineKind.Tone, frame.ToneLine),
            new CwScopeLine(CwScopeLineKind.Mixing, frame.MixingLine),
        };
    }

    /// <summary>Everything on the canvas, as items: nothing is drawn that is not here.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="width">The control's width.</param>
    /// <returns>The items.</returns>
    /// <remarks>
    /// <para>**THE TRACE, AND A BAR UNDER EVERY MARK** (R94). The level of the watched bin is
    /// drawn across the eight seconds whenever the detector has heard anything, marked or not,
    /// so a band with nothing keyed still shows its noise and a keyed station shows flat tops.
    /// Bars are drawn at their true length under it, newest at the right, and a gap is empty
    /// space as long as the gap was. No floor, no threshold. "listening" only before anything
    /// has been heard at all.</para>
    /// </remarks>
    public static IReadOnlyList<CwScopeItem> Items(CwScopeFrame frame, double width)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var training = frame.Training;
        var items = new List<CwScopeItem>
        {
            new(CwScopeItemKind.Tone, Pad, Pad, frame.ToneLine),
            new(CwScopeItemKind.Mixing, Pad, Pad, frame.MixingLine),
        };

        if (training.Listening)
        {
            items.Add(new CwScopeItem(CwScopeItemKind.Listening, width / 2, width / 2, ListeningWords));
            return items;
        }

        if (training.Trace.Count > 1)
        {
            items.Add(new CwScopeItem(
                CwScopeItemKind.Trace,
                XOfTime(training.Trace[0].StartUtc, training.NowUtc, width),
                XOfTime(training.NowUtc, training.NowUtc, width),
                CwHearingViewModel.ScopeTraceTip));
        }

        foreach (var bar in training.Bars)
        {
            items.Add(new CwScopeItem(
                CwScopeItemKind.Bar,
                XOfTime(bar.StartUtc, training.NowUtc, width),
                XOfTime(bar.EndUtc, training.NowUtc, width),
                string.Create(CultureInfo.InvariantCulture, $"{(bar.Dah ? "dah" : "dit")}, {bar.LengthMs:0} ms")));
        }

        return items;
    }

    /// <summary>The trace as points: one per hop, its level scaled into the trace's band.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="width">The control's width.</param>
    /// <returns>The points, oldest first; empty where fewer than two hops were heard.</returns>
    /// <remarks>
    /// The band spans the window's own lowest level less three dB to at least thirty dB above
    /// it, as unit 478's trace did, so noise alone never fills it and a keyed tone's flat top sits
    /// high. A hop with no level is drawn at the bottom.
    /// </remarks>
    public static IReadOnlyList<Point> TracePoints(CwScopeFrame frame, double width)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var trace = frame.Training.Trace;
        var finite = trace.Where(p => double.IsFinite(p.EnvelopeDb)).Select(p => p.EnvelopeDb).ToList();

        if (trace.Count < 2 || finite.Count == 0)
        {
            return Array.Empty<Point>();
        }

        var low = finite.Min() - 3;
        var high = Math.Max(finite.Max() + 8, low + DbSpanAtLeast);
        var now = frame.Training.NowUtc;

        double Y(double db) => PlotHeight
            - ((Math.Clamp(double.IsFinite(db) ? db : low, low, high) - low) / (high - low) * (PlotHeight - TraceTop));

        return trace.Select(p => new Point(XOfTime(p.StartUtc, now, width), Y(p.EnvelopeDb))).ToList();
    }

    /// <summary>The small word on an empty graph.</summary>
    public const string ListeningWords = "listening";

    /// <summary>Where a moment lands on the eight-second axis; now is the right edge.</summary>
    /// <param name="atUtc">The moment.</param>
    /// <param name="nowUtc">Now.</param>
    /// <param name="width">The control's width.</param>
    /// <returns>Pixels from the left, held inside the pads.</returns>
    public static double XOfTime(DateTime atUtc, DateTime nowUtc, double width)
    {
        var fraction = 1 - ((nowUtc - atUtc).TotalSeconds / CwTrainingGraph.WindowSeconds);

        return Pad + (Math.Clamp(fraction, 0, 1) * Math.Max(0, width - (2 * Pad)));
    }

    /// <summary>What the hover says at a point: a bar's words over a bar, the trace's over the plot.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="width">The control's width.</param>
    /// <param name="point">The pointer, in the control's pixels.</param>
    /// <returns>The words.</returns>
    public static string TipAt(CwScopeFrame frame, double width, Point point)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var items = Items(frame, width);

        if (point.Y >= BarTop && point.Y < BarTop + TrainingBarHeight)
        {
            foreach (var item in items.Where(i => i.Kind == CwScopeItemKind.Bar))
            {
                if (point.X >= item.X && point.X <= Math.Max(item.X2, item.X + 1))
                {
                    return item.Text;
                }
            }
        }

        if (point.Y >= TraceTop && point.Y < PlotHeight
            && items.Any(i => i.Kind == CwScopeItemKind.Trace && point.X >= i.X && point.X <= i.X2))
        {
            return CwHearingViewModel.ScopeTraceTip;
        }

        return CwHearingViewModel.ScopeTip;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => new(
            double.IsInfinity(availableSize.Width) ? 400 : availableSize.Width,
            BarTop + TrainingBarHeight + BarLabelHeight);

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (Frame is { } frame)
        {
            ToolTip.SetTip(this, TipAt(frame, Bounds.Width, e.GetPosition(this)));
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        ToolTip.SetTip(this, CwHearingViewModel.ScopeTip);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (Frame is not { } frame || Bounds.Width <= 2 * Pad)
        {
            return;
        }

        var width = Bounds.Width;
        var toneRight = Pad + 4;

        context.FillRectangle(Plot, new Rect(Pad, 0, width - (2 * Pad), BarTop + TrainingBarHeight + BarLabelHeight));

        foreach (var item in Items(frame, width))
        {
            switch (item.Kind)
            {
                case CwScopeItemKind.Tone:
                    toneRight = Text(context, item.Text, Pad + 4, 1, double.IsNaN(frame.ToneHz) ? Muted : MarkInk, 11);
                    break;

                case CwScopeItemKind.Mixing:
                    Text(context, "· " + item.Text, toneRight + 6, 2, Muted, 10);
                    break;

                case CwScopeItemKind.Listening:
                    Text(context, item.Text, item.X - 20, BarTop, Muted, 10);
                    break;

                case CwScopeItemKind.Trace:
                {
                    var points = TracePoints(frame, width);
                    var geometry = new StreamGeometry();

                    using (var g = geometry.Open())
                    {
                        g.BeginFigure(points[0], false);

                        for (var i = 1; i < points.Count; i++)
                        {
                            g.LineTo(points[i]);
                        }

                        g.EndFigure(false);
                    }

                    context.DrawGeometry(null, TracePen, geometry);
                    break;
                }

                case CwScopeItemKind.Bar:
                {
                    // The bar at its true length; its milliseconds under it where they fit.
                    var w = Math.Max(1, item.X2 - item.X);
                    context.FillRectangle(MarkInk, new Rect(item.X, BarTop, w, TrainingBarHeight));

                    var label = new FormattedText(
                        item.Text[(item.Text.IndexOf(", ", StringComparison.Ordinal) + 2)..], CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight, Typeface.Default, 9, MarkInk);

                    if (label.Width <= w + 4)
                    {
                        context.DrawText(label, new Point(item.X, BarTop + TrainingBarHeight));
                    }

                    break;
                }
            }
        }
    }

    /// <summary>Draw words; returns where they end, so the next words can follow them.</summary>
    private double Text(DrawingContext context, string words, double x, double top, IBrush brush, double size)
    {
        var text = new FormattedText(
            words, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            Typeface.Default, size, brush);
        var left = Math.Clamp(x, 0, Math.Max(0, Bounds.Width - text.Width));

        context.DrawText(text, new Point(left, top));

        return left + text.Width;
    }
}

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
/// </remarks>
public sealed class CwScopeControl : Control
{
    /// <summary>The margin at each side, in pixels.</summary>
    public const double Pad = 6;

    /// <summary>The height of the plot the trace is drawn in.</summary>
    public const double PlotHeight = 84;

    /// <summary>Where the row of bars starts, from the top.</summary>
    public const double BarRowTop = PlotHeight + Gap;

    /// <summary>The room at the right for the words naming the trace and the bars.</summary>
    public const double LabelRoom = 60;

    /// <summary>What to draw.</summary>
    public static readonly StyledProperty<CwScopeFrame?> FrameProperty =
        AvaloniaProperty.Register<CwScopeControl, CwScopeFrame?>(nameof(Frame));

    private const double BarHeight = 10;
    private const double BarLabelHeight = 12;
    private const double Gap = 3;
    private const double DbSpanAtLeast = 30;

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

    /// <summary>Everything the scope draws, with its words: nothing else is drawn.</summary>
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

    /// <summary>What the hover says at a point: a bar's words over a bar, the trace's over the plot.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="width">The width the four seconds are laid across, as <see cref="Bars"/> takes it.</param>
    /// <param name="point">The pointer, in the control's pixels.</param>
    /// <returns>The words.</returns>
    public static string TipAt(CwScopeFrame frame, double width, Point point)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (point.Y < PlotHeight && point.X <= width - Pad)
        {
            return CwHearingViewModel.ScopeTraceTip;
        }

        if (point.Y >= BarRowTop
            && point.Y < BarRowTop + BarHeight + BarLabelHeight
            && Bars(frame, width).Any(b => point.X >= b.X && point.X <= Math.Max(b.X2, b.X + 1)))
        {
            return CwHearingViewModel.ScopeBarTip;
        }

        return CwHearingViewModel.ScopeTip;
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => new(
            double.IsInfinity(availableSize.Width) ? 400 : availableSize.Width,
            BarRowTop + BarHeight + BarLabelHeight);

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (Frame is { } frame)
        {
            ToolTip.SetTip(this, TipAt(frame, PlotWidth(Bounds.Width), e.GetPosition(this)));
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
        if (Frame is not { } frame || Bounds.Width <= (2 * Pad) + LabelRoom)
        {
            return;
        }

        var across = PlotWidth(Bounds.Width);
        var plotRight = across - Pad;
        var lines = Lines(frame);

        context.FillRectangle(Plot, new Rect(Pad, 0, plotRight - Pad, PlotHeight));

        if (frame.Hops.Count > 1)
        {
            DrawTrace(context, frame, across);
        }

        Text(context, "─ " + Label(lines, CwScopeLineKind.Trace), plotRight + 4, 0, Ink, 10);

        // The detector's pitch and the tracker's, in the corner, as words.
        var tone = new FormattedText(
            Label(lines, CwScopeLineKind.Tone), CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            Typeface.Default, 11, double.IsNaN(frame.ToneHz) ? Muted : MarkInk);

        context.DrawText(tone, new Point(Pad + 4, 2));
        Text(context, "· " + Label(lines, CwScopeLineKind.Mixing), Pad + 4 + tone.Width + 6, 3, Muted, 10);

        // The marks, along the bottom, each labelled where it is wide enough to carry it.
        foreach (var bar in Bars(frame, across))
        {
            var w = Math.Max(1, bar.X2 - bar.X);
            context.FillRectangle(MarkInk, new Rect(bar.X, BarRowTop, w, BarHeight));

            var label = new FormattedText(
                bar.Label.Replace("mark ", "", StringComparison.Ordinal), CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, Typeface.Default, 9, MarkInk);

            if (label.Width <= w + 4)
            {
                context.DrawText(label, new Point(bar.X, BarRowTop + BarHeight));
            }
        }

        Text(context, "▬ " + Label(lines, CwScopeLineKind.Bars), plotRight + 4, BarRowTop - 1, MarkInk, 10);
    }

    /// <summary>The width the four seconds are laid across: the control less the words at the right.</summary>
    private static double PlotWidth(double controlWidth) => controlWidth - LabelRoom + Pad;

    private static string Label(IReadOnlyList<CwScopeLine> lines, CwScopeLineKind kind)
        => lines.Single(l => l.Kind == kind).Label;

    private static void DrawTrace(DrawingContext context, CwScopeFrame frame, double across)
    {
        var low = double.PositiveInfinity;
        var high = double.NegativeInfinity;

        foreach (var hop in frame.Hops)
        {
            if (double.IsFinite(hop.EnvelopeDb))
            {
                low = Math.Min(low, hop.EnvelopeDb);
                high = Math.Max(high, hop.EnvelopeDb);
            }
        }

        if (!double.IsFinite(low))
        {
            return;
        }

        // Room above for the words in the corner, and a span wide enough that noise alone
        // does not fill the plot.
        low -= 3;
        high = Math.Max(high + 8, low + DbSpanAtLeast);

        double Y(double db) => PlotHeight - ((Math.Clamp(db, low, high) - low) / (high - low) * PlotHeight);

        var geometry = new StreamGeometry();

        using (var g = geometry.Open())
        {
            for (var i = 0; i < frame.Hops.Count; i++)
            {
                var db = frame.Hops[i].EnvelopeDb;
                var point = new Point(XOfHop(i, frame, across), Y(double.IsFinite(db) ? db : low));

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
        }

        context.DrawGeometry(null, TracePen, geometry);
    }

    private void Text(DrawingContext context, string words, double x, double top, IBrush brush, double size)
    {
        var text = new FormattedText(
            words, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            Typeface.Default, size, brush);

        context.DrawText(text, new Point(Math.Clamp(x, 0, Math.Max(0, Bounds.Width - text.Width)), top));
    }
}

using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.App.Controls;

/// <summary>
/// The band as a neighborhood map (HM-DEC-016): tinted named regions, live
/// activity dots at real spot frequencies, the orange frequency marker.
/// Click a neighborhood to hear its story; drag to tune. Successor to the
/// plain band ribbon — same axis the waterfall inherits in phase 2.
/// </summary>
/// <remarks>
/// <para>The dots are the part that draws the eye, so they are made to earn
/// it (HM-DEC-023). Each one hit-tests on its own with a few pixels of
/// tolerance: hovering shows that spot's story, frequency, mode, source and
/// age, and clicking tunes straight to it. Clicking the background between
/// dots still opens the neighborhood's story, which is what it always
/// did.</para>
/// <para>Best-ranked spots draw larger and brighter, so a glance at the map
/// and a glance at the list say the same thing about what matters.</para>
/// <para>Positions are computed once per data or size change and cached, never
/// per render pass. A busy 40 m evening puts a few hundred dots on this
/// control and it is redrawn on every frequency change, every hover and every
/// one-second age tick; recomputing the layout inside
/// <see cref="Render"/> would turn tuning into a slideshow.</para>
/// </remarks>
public sealed class NeighborhoodMapControl : Control
{
    /// <summary>How far from a dot's center still counts as hovering it.</summary>
    private const double HitTolerance = 5.0;

    private static readonly Pen EdgePen = new(new SolidColorBrush(Color.Parse("#AECBEA")), 0.8);
    private static readonly Pen SeamPen = new(new SolidColorBrush(Color.Parse("#40000000")), 0.5);
    private static readonly IBrush MarkerBrush = new SolidColorBrush(Color.Parse("#C25E00"));

    /// <summary>The marker when the frequency is outside transmit privileges.</summary>
    private static readonly IBrush MarkerOutsideBrush =
        new SolidColorBrush(Color.Parse("#B3261E"));

    /// <summary>
    /// The listen-only hatch. Deliberately faint: the neighborhood color
    /// stays visible through it, so a segment reads as "not yours yet" rather
    /// than as a forbidden zone (HM-DEC-029).
    /// </summary>
    private static readonly IBrush VeilBrush =
        new SolidColorBrush(Color.FromArgb(0x33, 0x3A, 0x3A, 0x44));

    private static readonly IBrush VeilLabelBrush =
        new SolidColorBrush(Color.FromArgb(0xB0, 0x3A, 0x3A, 0x44));

    /// <summary>Dots outside privileges: still there, just quieter.</summary>
    private static readonly IBrush DimDotBrush =
        new SolidColorBrush(Color.FromArgb(0x88, 0x7A, 0x7A, 0x82));
    private static readonly IBrush LabelBrush = new SolidColorBrush(Color.Parse("#55534E"));
    private static readonly IBrush HoverBrush = new SolidColorBrush(Color.Parse("#1A1A18"));
    private static readonly Pen HoverPen = new(new SolidColorBrush(Color.Parse("#FFFFFF")), 1.5);
    private static readonly Typeface Sans = new("Segoe UI,Inter,sans-serif");

    /// <summary>The hatch stroke. Thin and pale on purpose.</summary>
    private static readonly Pen VeilPen = new(VeilBrush, 2.5);

    /// <summary>Current frequency in hertz. Two-way: dragging writes it back.</summary>
    public static readonly StyledProperty<long> FrequencyHzProperty =
        AvaloniaProperty.Register<NeighborhoodMapControl, long>(
            nameof(FrequencyHz), 7_030_000, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Band lower edge in hertz.</summary>
    public static readonly StyledProperty<long> BandLowHzProperty =
        AvaloniaProperty.Register<NeighborhoodMapControl, long>(nameof(BandLowHz), 7_000_000);

    /// <summary>Band upper edge in hertz.</summary>
    public static readonly StyledProperty<long> BandHighHzProperty =
        AvaloniaProperty.Register<NeighborhoodMapControl, long>(nameof(BandHighHz), 7_300_000);

    /// <summary>The neighborhoods to draw, tiling the band.</summary>
    public static readonly StyledProperty<IReadOnlyList<Neighborhood>?> NeighborhoodsProperty =
        AvaloniaProperty.Register<NeighborhoodMapControl, IReadOnlyList<Neighborhood>?>(
            nameof(Neighborhoods));

    /// <summary>Current activity spots. Real spot data, never decoration.</summary>
    public static readonly StyledProperty<IReadOnlyList<ActivityDot>?> ActivityDotsProperty =
        AvaloniaProperty.Register<NeighborhoodMapControl, IReadOnlyList<ActivityDot>?>(
            nameof(ActivityDots));

    /// <summary>Executed with the clicked <see cref="Neighborhood"/> when the
    /// pointer goes down and up on the background without dragging.</summary>
    public static readonly StyledProperty<ICommand?> SelectCommandProperty =
        AvaloniaProperty.Register<NeighborhoodMapControl, ICommand?>(nameof(SelectCommand));

    /// <summary>Executed with a dot's frequency in hertz when it is clicked.</summary>
    public static readonly StyledProperty<ICommand?> TuneCommandProperty =
        AvaloniaProperty.Register<NeighborhoodMapControl, ICommand?>(nameof(TuneCommand));

    /// <summary>
    /// Where this license class may and may not transmit, from the engine.
    /// </summary>
    /// <remarks>
    /// THE ONE SET OF BOUNDARIES (HM-DEC-029). These spans are computed once
    /// from the cited Part 97 data and handed to every surface that shows
    /// privileges — this map today, the waterfall or the dial tape whenever
    /// they want it. Nothing recomputes them locally, because two renderings
    /// of the same law that disagreed would be worse than either alone.
    /// An empty list means the class is unknown and NOTHING is drawn.
    /// </remarks>
    public static readonly StyledProperty<IReadOnlyList<PrivilegeSpan>?> PrivilegeSpansProperty =
        AvaloniaProperty.Register<NeighborhoodMapControl, IReadOnlyList<PrivilegeSpan>?>(
            nameof(PrivilegeSpans));

    private readonly Cursor _handCursor = new(StandardCursorType.Hand);
    private readonly Cursor _dotCursor = new(StandardCursorType.Cross);

    private DotLayout[] _layout = Array.Empty<DotLayout>();
    private DotLayout? _hovered;
    private bool _pointerDown;
    private bool _draggedBeyondClick;
    private Point _downPoint;

    static NeighborhoodMapControl()
    {
        AffectsRender<NeighborhoodMapControl>(
            FrequencyHzProperty, BandLowHzProperty, BandHighHzProperty,
            NeighborhoodsProperty, ActivityDotsProperty, PrivilegeSpansProperty);
    }

    /// <summary>Creates the map.</summary>
    public NeighborhoodMapControl()
    {
        Cursor = _handCursor;
        ClipToBounds = true;
        ToolTip.SetShowDelay(this, 120);
    }

    /// <summary>Current frequency in hertz.</summary>
    public long FrequencyHz
    {
        get => GetValue(FrequencyHzProperty);
        set => SetValue(FrequencyHzProperty, value);
    }

    /// <summary>Band lower edge in hertz.</summary>
    public long BandLowHz
    {
        get => GetValue(BandLowHzProperty);
        set => SetValue(BandLowHzProperty, value);
    }

    /// <summary>Band upper edge in hertz.</summary>
    public long BandHighHz
    {
        get => GetValue(BandHighHzProperty);
        set => SetValue(BandHighHzProperty, value);
    }

    /// <summary>The neighborhoods to draw.</summary>
    public IReadOnlyList<Neighborhood>? Neighborhoods
    {
        get => GetValue(NeighborhoodsProperty);
        set => SetValue(NeighborhoodsProperty, value);
    }

    /// <summary>Activity spots to draw as dots.</summary>
    public IReadOnlyList<ActivityDot>? ActivityDots
    {
        get => GetValue(ActivityDotsProperty);
        set => SetValue(ActivityDotsProperty, value);
    }

    /// <summary>Neighborhood click command.</summary>
    public ICommand? SelectCommand
    {
        get => GetValue(SelectCommandProperty);
        set => SetValue(SelectCommandProperty, value);
    }

    /// <summary>Dot click command; the parameter is a frequency in hertz.</summary>
    public ICommand? TuneCommand
    {
        get => GetValue(TuneCommandProperty);
        set => SetValue(TuneCommandProperty, value);
    }

    /// <summary>Transmit privileges across this band; empty draws nothing.</summary>
    public IReadOnlyList<PrivilegeSpan>? PrivilegeSpans
    {
        get => GetValue(PrivilegeSpansProperty);
        set => SetValue(PrivilegeSpansProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // Layout depends on the dots, the band window and the control's own
        // width — and on nothing else, which is why the frequency marker
        // moving does not rebuild it.
        if (change.Property == ActivityDotsProperty
            || change.Property == BandLowHzProperty
            || change.Property == BandHighHzProperty
            || change.Property == BoundsProperty)
        {
            RebuildLayout();
        }
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0 || BandHighHz <= BandLowHz)
        {
            return;
        }

        double span = BandHighHz - BandLowHz;

        // Tinted neighborhoods with short labels.
        if (Neighborhoods is { Count: > 0 } hoods)
        {
            foreach (var hood in hoods)
            {
                var left = (hood.LowHz - BandLowHz) / span * w;
                var right = (hood.HighHz - BandLowHz) / span * w;
                var rect = new Rect(left, 0, Math.Max(0, right - left), h);
                context.FillRectangle(
                    new SolidColorBrush(Color.Parse(hood.ColorHex)), rect);
                context.DrawLine(SeamPen, new Point(right, 0), new Point(right, h));

                if (hood.ShortName.Length > 0)
                {
                    var label = new FormattedText(hood.ShortName,
                        CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        Sans, 11, LabelBrush);
                    if (label.Width < rect.Width - 6)
                    {
                        context.DrawText(label,
                            new Point(left + (rect.Width - label.Width) / 2, 4));
                    }
                }
            }
        }

        // The listen-only veil, over the culture tints and under the dots.
        // Nothing is drawn when the class is unknown: an empty span list is
        // how "do not guess" is expressed structurally (HM-DEC-029).
        DrawListenOnlyVeil(context, w, h, span);

        // Activity dots from the cached layout. Dots outside privileges are
        // dimmed rather than removed — the operator still needs to see where
        // the action is, even where they cannot answer it yet.
        foreach (var dot in _layout)
        {
            var brush = MayTransmitAt(dot.Dot.FrequencyHz) ? dot.Brush : DimDotBrush;
            context.DrawEllipse(brush, null, dot.Center, dot.Radius, dot.Radius);
        }

        if (_hovered is not null)
        {
            context.DrawEllipse(
                HoverBrush, HoverPen, _hovered.Center,
                _hovered.Radius + 2, _hovered.Radius + 2);
        }

        context.DrawRectangle(null, EdgePen, new Rect(0.5, 0.5, w - 1, h - 1), 6, 6);

        // The marker turns red outside privileges, with a small flag. It
        // never stops anybody tuning there — it is a fact on the screen, not
        // a barrier (HM-DEC-029).
        var markerX = (FrequencyHz - BandLowHz) / span * w;
        var outside = HasPrivilegeData && !MayTransmitAt(FrequencyHz);
        var markerBrush = outside ? MarkerOutsideBrush : MarkerBrush;

        context.FillRectangle(markerBrush, new Rect(markerX - 1, 0, 2, h));

        if (outside)
        {
            var flag = new StreamGeometry();
            using (var g = flag.Open())
            {
                g.BeginFigure(new Point(markerX, 0), isFilled: true);
                g.LineTo(new Point(markerX + 8, 0));
                g.LineTo(new Point(markerX, 8));
                g.EndFigure(true);
            }

            context.DrawGeometry(markerBrush, null, flag);
        }
    }

    /// <summary>True when privilege data is available to draw at all.</summary>
    private bool HasPrivilegeData => PrivilegeSpans is { Count: > 0 };

    /// <summary>
    /// Whether the operator may transmit at a frequency, from the spans.
    /// </summary>
    /// <remarks>
    /// With no spans the answer is "yes" so that nothing is dimmed or
    /// flagged. An unknown license class must produce a map that looks
    /// exactly as it did before privileges existed.
    /// </remarks>
    private bool MayTransmitAt(long hz)
    {
        if (PrivilegeSpans is not { Count: > 0 } spans)
        {
            return true;
        }

        foreach (var s in spans)
        {
            if (s.Contains(hz))
            {
                return s.MayTransmit;
            }
        }

        // Past the last span's exclusive upper edge: take the last answer.
        return spans[^1].MayTransmit;
    }

    /// <summary>
    /// Hatch the stretches this class may not transmit in.
    /// </summary>
    /// <remarks>
    /// A 135° diagonal at low opacity, so the neighborhood color reads
    /// through it. "Listen only" is written where there is room, because the
    /// fact that matters most is that listening is never restricted — the
    /// veil marks where transmitting stops, not where the operator may not
    /// go.
    /// </remarks>
    private void DrawListenOnlyVeil(DrawingContext context, double w, double h, double span)
    {
        if (PrivilegeSpans is not { Count: > 0 } spans)
        {
            return;
        }

        foreach (var s in spans)
        {
            if (s.MayTransmit)
            {
                continue;
            }

            var left = (s.LowHz - BandLowHz) / span * w;
            var right = (s.HighHz - BandLowHz) / span * w;
            var width = Math.Max(0, right - left);
            if (width <= 0)
            {
                continue;
            }

            var rect = new Rect(left, 0, width, h);

            using (context.PushClip(rect))
            {
                // 135°: lines running down-left to up-right across the block.
                for (var x = left - h; x < right + h; x += 6)
                {
                    context.DrawLine(VeilPen, new Point(x, h), new Point(x + h, 0));
                }
            }

            if (width > 66)
            {
                var label = new FormattedText(
                    "listen only", CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, Sans, 10, VeilLabelBrush);

                if (label.Width < width - 8)
                {
                    context.DrawText(
                        label,
                        new Point(left + ((width - label.Width) / 2), h - label.Height - 3));
                }
            }
        }
    }

    /// <summary>
    /// Recompute every dot's position, size and color.
    /// </summary>
    /// <remarks>
    /// Called only when the dots, the band window or the control's size
    /// changes — see the type remarks on why this is not done during
    /// rendering.
    /// </remarks>
    private void RebuildLayout()
    {
        var w = Bounds.Width;
        var h = Bounds.Height;

        if (ActivityDots is not { Count: > 0 } dots
            || w <= 0 || h <= 0 || BandHighHz <= BandLowHz)
        {
            _layout = Array.Empty<DotLayout>();
            _hovered = null;
            return;
        }

        double span = BandHighHz - BandLowHz;
        var built = new List<DotLayout>(dots.Count);

        foreach (var dot in dots)
        {
            if (dot.FrequencyHz < BandLowHz || dot.FrequencyHz > BandHighHz)
            {
                continue;
            }

            var x = (dot.FrequencyHz - BandLowHz) / span * w;

            // Scatter vertically, deterministically from the frequency, so
            // neighbors on the same kilohertz do not stack into one blob.
            var y = h * (0.42 + 0.4 * (dot.FrequencyHz / 100 % 7) / 7.0);

            var prominence = Math.Clamp(dot.Prominence, 0, 1);
            var radius = 2.5 + (2.5 * prominence);

            built.Add(new DotLayout(
                dot, new Point(x, y), radius, BrushFor(prominence)));
        }

        _layout = built.ToArray();

        // The dot under the pointer may have moved or vanished.
        _hovered = null;
        ToolTip.SetIsOpen(this, false);
    }

    /// <summary>
    /// Best-ranked dots are darker and fully opaque; the rest fade back so the
    /// eye lands on what the ranking chose.
    /// </summary>
    private static IBrush BrushFor(double prominence)
    {
        var alpha = (byte)Math.Clamp(110 + (145 * prominence), 90, 255);
        return new SolidColorBrush(Color.FromArgb(alpha, 0xC2, 0x5E, 0x00));
    }

    private DotLayout? DotAt(Point p)
    {
        DotLayout? best = null;
        var bestDistance = double.MaxValue;

        foreach (var dot in _layout)
        {
            var dx = dot.Center.X - p.X;
            var dy = dot.Center.Y - p.Y;
            var distance = Math.Sqrt((dx * dx) + (dy * dy));

            if (distance <= dot.Radius + HitTolerance && distance < bestDistance)
            {
                best = dot;
                bestDistance = distance;
            }
        }

        return best;
    }

    private void SetHover(DotLayout? dot)
    {
        if (ReferenceEquals(dot, _hovered))
        {
            return;
        }

        _hovered = dot;

        if (dot is null)
        {
            ToolTip.SetIsOpen(this, false);
            Cursor = _handCursor;
        }
        else
        {
            ToolTip.SetIsOpen(this, false);
            ToolTip.SetTip(this, dot.Dot.TooltipText);
            ToolTip.SetIsOpen(this, true);
            Cursor = _dotCursor;
        }

        InvalidateVisual();
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _pointerDown = true;
        _draggedBeyondClick = false;
        _downPoint = e.GetPosition(this);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var p = e.GetPosition(this);

        if (!_pointerDown)
        {
            SetHover(DotAt(p));
            return;
        }

        if (!_draggedBeyondClick && Math.Abs(p.X - _downPoint.X) < 4)
        {
            return;
        }

        _draggedBeyondClick = true;
        SetHover(null);
        TuneToPointer(p.X);
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        SetHover(null);
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_pointerDown)
        {
            return;
        }

        _pointerDown = false;
        e.Pointer.Capture(null);

        if (!_draggedBeyondClick)
        {
            var point = e.GetPosition(this);
            var dot = DotAt(point);

            if (dot is not null)
            {
                // A dot is a specific station, so it wins over the
                // neighborhood it happens to sit in.
                if (TuneCommand?.CanExecute(dot.Dot.FrequencyHz) == true)
                {
                    TuneCommand.Execute(dot.Dot.FrequencyHz);
                }
            }
            else
            {
                var hood = HoodAt(point.X);
                if (hood is not null && SelectCommand?.CanExecute(hood) == true)
                {
                    SelectCommand.Execute(hood);
                }
            }
        }

        e.Handled = true;
    }

    private Neighborhood? HoodAt(double x)
    {
        if (Neighborhoods is not { Count: > 0 } hoods || Bounds.Width <= 0)
        {
            return null;
        }

        var hz = BandLowHz + (long)(x / Bounds.Width * (BandHighHz - BandLowHz));
        return hoods.FirstOrDefault(n => n.Contains(hz));
    }

    private void TuneToPointer(double x)
    {
        var frac = Math.Min(1.0, Math.Max(0.0, x / Math.Max(1, Bounds.Width)));
        var hz = BandLowHz + (long)(frac * (BandHighHz - BandLowHz));
        SetCurrentValue(FrequencyHzProperty, hz / 100 * 100);
    }

    /// <summary>A dot's precomputed screen geometry.</summary>
    private sealed record DotLayout(
        ActivityDot Dot, Point Center, double Radius, IBrush Brush);
}

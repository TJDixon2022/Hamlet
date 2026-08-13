using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Hamlet.RadioEngine.Explore;

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
    /// <summary>How far from a dot's centre still counts as hovering it.</summary>
    private const double HitTolerance = 5.0;

    private static readonly Pen EdgePen = new(new SolidColorBrush(Color.Parse("#AECBEA")), 0.8);
    private static readonly Pen SeamPen = new(new SolidColorBrush(Color.Parse("#40000000")), 0.5);
    private static readonly IBrush MarkerBrush = new SolidColorBrush(Color.Parse("#C25E00"));
    private static readonly IBrush LabelBrush = new SolidColorBrush(Color.Parse("#55534E"));
    private static readonly IBrush HoverBrush = new SolidColorBrush(Color.Parse("#1A1A18"));
    private static readonly Pen HoverPen = new(new SolidColorBrush(Color.Parse("#FFFFFF")), 1.5);
    private static readonly Typeface Sans = new("Segoe UI,Inter,sans-serif");

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
            NeighborhoodsProperty, ActivityDotsProperty);
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

        // Activity dots from the cached layout.
        foreach (var dot in _layout)
        {
            context.DrawEllipse(dot.Brush, null, dot.Centre, dot.Radius, dot.Radius);
        }

        if (_hovered is not null)
        {
            context.DrawEllipse(
                HoverBrush, HoverPen, _hovered.Centre,
                _hovered.Radius + 2, _hovered.Radius + 2);
        }

        context.DrawRectangle(null, EdgePen, new Rect(0.5, 0.5, w - 1, h - 1), 6, 6);

        var markerX = (FrequencyHz - BandLowHz) / span * w;
        context.FillRectangle(MarkerBrush, new Rect(markerX - 1, 0, 2, h));
    }

    /// <summary>
    /// Recompute every dot's position, size and colour.
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
            // neighbours on the same kilohertz do not stack into one blob.
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
            var dx = dot.Centre.X - p.X;
            var dy = dot.Centre.Y - p.Y;
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
        ActivityDot Dot, Point Centre, double Radius, IBrush Brush);
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;

namespace Hamlet.App.Controls;

/// <summary>
/// The dial tape: a fixed center hairline with the frequency scale dragged
/// underneath it — a slide-rule VFO with flick momentum. Dress rehearsal for
/// the phase 2 waterfall, which paints behind this same axis.
/// Approved design: HM-DEC-015.
/// </summary>
/// <remarks>
/// <para>Spots ride along the top edge on a <see cref="SpotMarkerStrip"/>, out
/// of the frequency scale's way. They are the same spots the neighborhood map
/// draws, placed by the same <see cref="FrequencyAxis"/> arithmetic, so a
/// station visible on both surfaces is at the same frequency on both. The tape
/// is simply the zoomed window: a few kilohertz across its width where the map
/// carries the whole band.</para>
/// <para>The tape showing nothing while the map showed dots was the whole
/// reason for this. A newcomer who has just clicked a spot on the map arrives
/// at a scale with no landmarks on it, which teaches them the tape is
/// decoration.</para>
/// <para>The gesture is the one the waterfall inherits in phase 2. Drag a
/// marker under the hairline and the radio is on it; click it and the radio
/// jumps there. When the waterfall starts drawing real spectrum, a marker over
/// a smear is what tells the operator that somebody has already worked out who
/// that is.</para>
/// </remarks>
public sealed class DialTapeControl : Control
{
    /// <summary>
    /// The tape's zoom. Public because it is half of what the tape's window
    /// is, and anything reasoning about where a marker lands needs both halves.
    /// </summary>
    public const double PixelsPerHz = 0.16;

    private const double MomentumDecayPerFrame = 0.94;

    /// <summary>How tall the spot rail along the top edge is drawn.</summary>
    private const double RailHeight = 8;

    /// <summary>How far down from the top edge a pointer still counts as on the rail.</summary>
    private const double RailReach = 16;

    /// <summary>Below this the tape is too short for a rail and gets none.</summary>
    private const double RailMinimumTapeHeight = 40;

    /// <summary>
    /// How far a press that landed on a marker may wander before it is a drag
    /// rather than a click.
    /// </summary>
    private const double ClickSlop = 4;

    // HM-DEC-012 palette.
    private static readonly IBrush CwSegmentBrush = PanelPalette.Green.FillBrush;
    private static readonly IBrush LabelBrush = PanelPalette.Blue.TitleBrush;
    private static readonly Pen MinorTickPen = new(PanelPalette.Blue.EdgeBrush, 0.8);
    private static readonly Pen MajorTickPen = new(new SolidColorBrush(Color.Parse("#7FA8D6")), 1.4);
    private static readonly IBrush HairlineBrush = PanelPalette.Amber.TitleBrush;
    private static readonly Typeface Mono = new("Consolas,Menlo,DejaVu Sans Mono,monospace");

    /// <summary>Current frequency in hertz. Two-way: dragging the tape writes it back.</summary>
    public static readonly StyledProperty<long> FrequencyHzProperty =
        AvaloniaProperty.Register<DialTapeControl, long>(
            nameof(FrequencyHz), 7_030_000, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Band lower edge in hertz; tuning clamps here.</summary>
    public static readonly StyledProperty<long> BandLowHzProperty =
        AvaloniaProperty.Register<DialTapeControl, long>(nameof(BandLowHz), 7_000_000);

    /// <summary>Band upper edge in hertz; tuning clamps here.</summary>
    public static readonly StyledProperty<long> BandHighHzProperty =
        AvaloniaProperty.Register<DialTapeControl, long>(nameof(BandHighHz), 7_300_000);

    /// <summary>CW segment lower edge in hertz (green shading).</summary>
    public static readonly StyledProperty<long> CwLowHzProperty =
        AvaloniaProperty.Register<DialTapeControl, long>(nameof(CwLowHz), 7_000_000);

    /// <summary>CW segment upper edge in hertz (green shading).</summary>
    public static readonly StyledProperty<long> CwHighHzProperty =
        AvaloniaProperty.Register<DialTapeControl, long>(nameof(CwHighHz), 7_125_000);

    /// <summary>
    /// The spots to mark. The same list the neighborhood map draws, so the two
    /// surfaces can never be showing different activity.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<ActivityDot>?> ActivityDotsProperty =
        AvaloniaProperty.Register<DialTapeControl, IReadOnlyList<ActivityDot>?>(
            nameof(ActivityDots));

    /// <summary>Executed with a marker's frequency in hertz when it is clicked.</summary>
    public static readonly StyledProperty<ICommand?> TuneCommandProperty =
        AvaloniaProperty.Register<DialTapeControl, ICommand?>(nameof(TuneCommand));

    private readonly DispatcherTimer _coastTimer;
    private readonly SpotMarkerStrip _rail = new();
    private readonly Cursor _dragCursor = new(StandardCursorType.SizeWestEast);
    private readonly Cursor _markerCursor = new(StandardCursorType.Cross);

    private bool _dragging;
    private double _lastX;
    private double _downX;
    private SpotMarker? _pressedMarker;
    private long _lastMoveTicks;
    private double _velocityHzPerMs;
    private long _coastLastTicks;

    static DialTapeControl()
    {
        AffectsRender<DialTapeControl>(
            FrequencyHzProperty, BandLowHzProperty, BandHighHzProperty,
            CwLowHzProperty, CwHighHzProperty, ActivityDotsProperty);
    }

    /// <summary>Creates the tape with its momentum timer stopped.</summary>
    public DialTapeControl()
    {
        _coastTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, OnCoastTick);
        _coastTimer.Stop();
        Cursor = _dragCursor;
        ClipToBounds = true;
        Focusable = true;
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

    /// <summary>CW segment lower edge in hertz.</summary>
    public long CwLowHz
    {
        get => GetValue(CwLowHzProperty);
        set => SetValue(CwLowHzProperty, value);
    }

    /// <summary>CW segment upper edge in hertz.</summary>
    public long CwHighHz
    {
        get => GetValue(CwHighHzProperty);
        set => SetValue(CwHighHzProperty, value);
    }

    /// <summary>The spots to mark along the top edge.</summary>
    public IReadOnlyList<ActivityDot>? ActivityDots
    {
        get => GetValue(ActivityDotsProperty);
        set => SetValue(ActivityDotsProperty, value);
    }

    /// <summary>Marker click command; the parameter is a frequency in hertz.</summary>
    public ICommand? TuneCommand
    {
        get => GetValue(TuneCommandProperty);
        set => SetValue(TuneCommandProperty, value);
    }

    /// <summary>The window of the band currently under the tape.</summary>
    private FrequencyAxis Axis
        => FrequencyAxis.Zoomed(FrequencyHz, PixelsPerHz, Bounds.Width);

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // Unlike the map's, this window moves with the frequency, so the rail
        // is relaid on every tuning step as well as on new data.
        if (change.Property == FrequencyHzProperty
            || change.Property == ActivityDotsProperty
            || change.Property == BoundsProperty)
        {
            _rail.Rebuild(ActivityDots, Axis);
            ToolTip.SetIsOpen(this, false);
        }
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
        {
            return;
        }

        var axis = Axis;
        var railHeight = h >= RailMinimumTapeHeight ? RailHeight : 0;

        // CW segment shading.
        var segLeft = Math.Max(0, axis.XOf(CwLowHz));
        var segRight = Math.Min(w, axis.XOf(CwHighHz));
        if (segRight > segLeft)
        {
            context.FillRectangle(CwSegmentBrush, new Rect(segLeft, 0, segRight - segLeft, h));
        }

        // Ticks: minor every 100 Hz, major with label every 500 Hz. The labels
        // clear the rail whether or not anything is on it, because a scale that
        // shifted when a spot arrived would be worse than either position.
        var labelTop = railHeight > 0 ? railHeight + 4 : h * 0.12;
        var firstTick = (long)Math.Ceiling(axis.LowHz / 100) * 100;

        for (var f = firstTick; f <= axis.HighHz; f += 100)
        {
            var x = axis.XOf(f);
            var major = f % 500 == 0;
            context.DrawLine(
                major ? MajorTickPen : MinorTickPen,
                new Point(x, h),
                new Point(x, h - (major ? h * 0.42 : h * 0.22)));

            if (major)
            {
                var mhz = f / 1_000_000.0;
                var label = new FormattedText(
                    mhz.ToString("0.0####", CultureInfo.InvariantCulture),
                    CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    Mono, 11, LabelBrush);
                var lx = x - label.Width / 2;
                if (lx >= 2 && lx + label.Width <= w - 2)
                {
                    context.DrawText(label, new Point(lx, labelTop));
                }
            }
        }

        // The hairline and its pointer.
        var cx = w / 2;
        context.DrawLine(new Pen(HairlineBrush, 2), new Point(cx, 0), new Point(cx, h));
        var pointer = new StreamGeometry();
        using (var g = pointer.Open())
        {
            g.BeginFigure(new Point(cx - 7, 0), isFilled: true);
            g.LineTo(new Point(cx + 7, 0));
            g.LineTo(new Point(cx, 8));
            g.EndFigure(isClosed: true);
        }

        context.DrawGeometry(HairlineBrush, null, pointer);

        // The rail goes on last, over the hairline rather than under it. A
        // marker dragged under the hairline is the whole gesture, and it has to
        // stay visible at the moment it arrives.
        if (railHeight > 0)
        {
            _rail.Render(context, new Rect(0, 0, w, railHeight));
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var p = e.GetPosition(this);

        _pressedMarker = MarkerAt(p);
        SetHover(null);

        _dragging = true;
        _coastTimer.Stop();
        _velocityHzPerMs = 0;
        _lastX = p.X;
        _downX = p.X;
        _lastMoveTicks = Environment.TickCount64;
        e.Pointer.Capture(this);
        Focus();
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var p = e.GetPosition(this);

        if (!_dragging)
        {
            SetHover(MarkerAt(p));
            return;
        }

        var x = p.X;
        var now = Environment.TickCount64;

        // A press that landed on a marker holds the tape still until the
        // operator plainly means to drag. Without it a three-pixel bar is
        // almost impossible to click without nudging the radio first.
        if (_pressedMarker is not null)
        {
            if (Math.Abs(x - _downX) < ClickSlop)
            {
                _lastX = x;
                _lastMoveTicks = now;
                e.Handled = true;
                return;
            }

            _pressedMarker = null;
        }

        var dx = x - _lastX;
        if (dx != 0)
        {
            var dtMs = Math.Max(1, now - _lastMoveTicks);
            _velocityHzPerMs = -dx / PixelsPerHz / dtMs;
            Tune(FrequencyHz - (long)Math.Round(dx / PixelsPerHz));
        }

        _lastX = x;
        _lastMoveTicks = now;
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (!_dragging)
        {
            return;
        }

        _dragging = false;
        e.Pointer.Capture(null);

        // A marker click is a specific station, so it wins over the snap the
        // tape would otherwise do. Same rule the map's dots follow.
        if (_pressedMarker is not null)
        {
            var marker = _pressedMarker;
            _pressedMarker = null;
            TuneToMarker(marker);
            e.Handled = true;
            return;
        }

        if (Math.Abs(_velocityHzPerMs) > 5)
        {
            _coastLastTicks = Environment.TickCount64;
            _coastTimer.Start();
        }
        else
        {
            SnapToTen();
        }

        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        SetHover(null);
    }

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        _coastTimer.Stop();
        Tune(FrequencyHz + (e.Delta.Y > 0 ? 10 : -10));
        e.Handled = true;
    }

    private SpotMarker? MarkerAt(Point p)
        => Bounds.Height >= RailMinimumTapeHeight
            ? _rail.At(p, new Rect(0, 0, Bounds.Width, RailReach))
            : null;

    private void SetHover(SpotMarker? marker)
    {
        if (!_rail.SetHover(marker))
        {
            return;
        }

        if (marker is null)
        {
            ToolTip.SetIsOpen(this, false);
            Cursor = _dragCursor;
        }
        else
        {
            // The map's tooltip verbatim: story, frequency, mode, the reason it
            // ranked where it did, and who heard it when (HM-DEC-009).
            ToolTip.SetIsOpen(this, false);
            ToolTip.SetTip(this, marker.Dot.TooltipText);
            ToolTip.SetIsOpen(this, true);
            Cursor = _markerCursor;
        }

        InvalidateVisual();
    }

    private void TuneToMarker(SpotMarker marker)
    {
        var hz = marker.Dot.FrequencyHz;

        if (TuneCommand?.CanExecute(hz) == true)
        {
            TuneCommand.Execute(hz);
        }
        else
        {
            Tune(hz);
        }
    }

    private void OnCoastTick(object? sender, EventArgs e)
    {
        var now = Environment.TickCount64;
        var dtMs = Math.Max(1, now - _coastLastTicks);
        _coastLastTicks = now;

        _velocityHzPerMs *= Math.Pow(MomentumDecayPerFrame, dtMs / 16.0);
        if (Math.Abs(_velocityHzPerMs) < 2 || _dragging)
        {
            _coastTimer.Stop();
            SnapToTen();
            return;
        }

        Tune(FrequencyHz + (long)Math.Round(_velocityHzPerMs * dtMs));
    }

    private void SnapToTen() => Tune((FrequencyHz + 5) / 10 * 10);

    private void Tune(long hz)
        => SetCurrentValue(FrequencyHzProperty,
            Math.Min(BandHighHz, Math.Max(BandLowHz, hz)));
}

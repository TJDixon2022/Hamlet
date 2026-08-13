using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System.Globalization;

namespace HamManager.App.Controls;

/// <summary>
/// The dial tape: a fixed center hairline with the frequency scale dragged
/// underneath it — a slide-rule VFO with flick momentum. Dress rehearsal for
/// the phase 2 waterfall, which paints behind this same axis.
/// Approved design: HM-DEC-015.
/// </summary>
public sealed class DialTapeControl : Control
{
    private const double PixelsPerHz = 0.16;
    private const double MomentumDecayPerFrame = 0.94;

    // HM-DEC-012 palette.
    private static readonly IBrush CwSegmentBrush = new SolidColorBrush(Color.Parse("#EAF6EF"));
    private static readonly IBrush LabelBrush = new SolidColorBrush(Color.Parse("#1F5FA8"));
    private static readonly Pen MinorTickPen = new(new SolidColorBrush(Color.Parse("#AECBEA")), 0.8);
    private static readonly Pen MajorTickPen = new(new SolidColorBrush(Color.Parse("#7FA8D6")), 1.4);
    private static readonly IBrush HairlineBrush = new SolidColorBrush(Color.Parse("#C25E00"));
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

    private readonly DispatcherTimer _coastTimer;
    private bool _dragging;
    private double _lastX;
    private long _lastMoveTicks;
    private double _velocityHzPerMs;
    private long _coastLastTicks;

    static DialTapeControl()
    {
        AffectsRender<DialTapeControl>(
            FrequencyHzProperty, BandLowHzProperty, BandHighHzProperty,
            CwLowHzProperty, CwHighHzProperty);
    }

    /// <summary>Creates the tape with its momentum timer stopped.</summary>
    public DialTapeControl()
    {
        _coastTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, OnCoastTick);
        _coastTimer.Stop();
        Cursor = new Cursor(StandardCursorType.SizeWestEast);
        ClipToBounds = true;
        Focusable = true;
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

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
        {
            return;
        }

        var spanHz = w / PixelsPerHz;
        var loHz = FrequencyHz - spanHz / 2;
        var hiHz = FrequencyHz + spanHz / 2;

        // CW segment shading.
        var segLeft = Math.Max(0, (CwLowHz - loHz) * PixelsPerHz);
        var segRight = Math.Min(w, (CwHighHz - loHz) * PixelsPerHz);
        if (segRight > segLeft)
        {
            context.FillRectangle(CwSegmentBrush, new Rect(segLeft, 0, segRight - segLeft, h));
        }

        // Ticks: minor every 100 Hz, major with label every 500 Hz.
        var firstTick = (long)Math.Ceiling(loHz / 100) * 100;
        for (var f = firstTick; f <= hiHz; f += 100)
        {
            var x = (f - loHz) * PixelsPerHz;
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
                    context.DrawText(label, new Point(lx, h * 0.12));
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
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _dragging = true;
        _coastTimer.Stop();
        _velocityHzPerMs = 0;
        _lastX = e.GetPosition(this).X;
        _lastMoveTicks = Environment.TickCount64;
        e.Pointer.Capture(this);
        Focus();
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_dragging)
        {
            return;
        }

        var x = e.GetPosition(this).X;
        var now = Environment.TickCount64;
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
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        _coastTimer.Stop();
        Tune(FrequencyHz + (e.Delta.Y > 0 ? 10 : -10));
        e.Handled = true;
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

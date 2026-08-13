using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace HamManager.App.Controls;

/// <summary>
/// The band ribbon: the whole band as a horizontal map with the CW segment
/// shaded and an orange marker at the current frequency. Click or drag to
/// jump; the dial tape does the fine work. In phase 2 the waterfall inherits
/// this control's axis and gesture. Approved design: HM-DEC-015.
/// </summary>
public sealed class BandRibbonControl : Control
{
    // HM-DEC-012 palette.
    private static readonly IBrush TrackBrush = new SolidColorBrush(Color.Parse("#FFFFFF"));
    private static readonly IBrush CwSegmentBrush = new SolidColorBrush(Color.Parse("#EAF6EF"));
    private static readonly Pen EdgePen = new(new SolidColorBrush(Color.Parse("#AECBEA")), 0.8);
    private static readonly IBrush MarkerBrush = new SolidColorBrush(Color.Parse("#C25E00"));

    /// <summary>Current frequency in hertz. Two-way: clicking writes it back.</summary>
    public static readonly StyledProperty<long> FrequencyHzProperty =
        AvaloniaProperty.Register<BandRibbonControl, long>(
            nameof(FrequencyHz), 7_030_000, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Band lower edge in hertz.</summary>
    public static readonly StyledProperty<long> BandLowHzProperty =
        AvaloniaProperty.Register<BandRibbonControl, long>(nameof(BandLowHz), 7_000_000);

    /// <summary>Band upper edge in hertz.</summary>
    public static readonly StyledProperty<long> BandHighHzProperty =
        AvaloniaProperty.Register<BandRibbonControl, long>(nameof(BandHighHz), 7_300_000);

    /// <summary>CW segment lower edge in hertz.</summary>
    public static readonly StyledProperty<long> CwLowHzProperty =
        AvaloniaProperty.Register<BandRibbonControl, long>(nameof(CwLowHz), 7_000_000);

    /// <summary>CW segment upper edge in hertz.</summary>
    public static readonly StyledProperty<long> CwHighHzProperty =
        AvaloniaProperty.Register<BandRibbonControl, long>(nameof(CwHighHz), 7_125_000);

    private bool _dragging;

    static BandRibbonControl()
    {
        AffectsRender<BandRibbonControl>(
            FrequencyHzProperty, BandLowHzProperty, BandHighHzProperty,
            CwLowHzProperty, CwHighHzProperty);
    }

    /// <summary>Creates the ribbon.</summary>
    public BandRibbonControl()
    {
        Cursor = new Cursor(StandardCursorType.Cross);
        ClipToBounds = true;
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
        if (w <= 0 || h <= 0 || BandHighHz <= BandLowHz)
        {
            return;
        }

        context.FillRectangle(TrackBrush, new Rect(0, 0, w, h));

        double span = BandHighHz - BandLowHz;
        var segLeft = (CwLowHz - BandLowHz) / span * w;
        var segRight = (CwHighHz - BandLowHz) / span * w;
        context.FillRectangle(CwSegmentBrush,
            new Rect(segLeft, 0, Math.Max(0, segRight - segLeft), h));

        context.DrawRectangle(null, EdgePen, new Rect(0.5, 0.5, w - 1, h - 1), 6, 6);

        var markerX = (FrequencyHz - BandLowHz) / span * w;
        context.FillRectangle(MarkerBrush, new Rect(markerX - 1, 0, 2, h));
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        _dragging = true;
        e.Pointer.Capture(this);
        TuneToPointer(e.GetPosition(this).X);
        e.Handled = true;
    }

    /// <inheritdoc/>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragging)
        {
            TuneToPointer(e.GetPosition(this).X);
            e.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragging = false;
        e.Pointer.Capture(null);
    }

    private void TuneToPointer(double x)
    {
        var frac = Math.Min(1.0, Math.Max(0.0, x / Math.Max(1, Bounds.Width)));
        var hz = BandLowHz + (long)(frac * (BandHighHz - BandLowHz));
        SetCurrentValue(FrequencyHzProperty, hz / 100 * 100);
    }
}

using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using HamManager.RadioEngine.Explore;

namespace HamManager.App.Controls;

/// <summary>
/// The band as a neighborhood map (HM-DEC-016): tinted named regions, live
/// activity dots at real spot frequencies, the orange frequency marker.
/// Click a neighborhood to hear its story; drag to tune. Successor to the
/// plain band ribbon — same axis the waterfall inherits in phase 2.
/// </summary>
public sealed class NeighborhoodMapControl : Control
{
    private static readonly Pen EdgePen = new(new SolidColorBrush(Color.Parse("#AECBEA")), 0.8);
    private static readonly Pen SeamPen = new(new SolidColorBrush(Color.Parse("#40000000")), 0.5);
    private static readonly IBrush MarkerBrush = new SolidColorBrush(Color.Parse("#C25E00"));
    private static readonly IBrush DotBrush = new SolidColorBrush(Color.Parse("#C25E00"));
    private static readonly IBrush LabelBrush = new SolidColorBrush(Color.Parse("#55534E"));
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

    /// <summary>Frequencies of current activity spots; each in-band entry
    /// draws a glowing dot. Real spot data, never decoration.</summary>
    public static readonly StyledProperty<IReadOnlyList<long>?> ActivityFrequenciesProperty =
        AvaloniaProperty.Register<NeighborhoodMapControl, IReadOnlyList<long>?>(
            nameof(ActivityFrequencies));

    /// <summary>Executed with the clicked <see cref="Neighborhood"/> when the
    /// pointer goes down and up without dragging.</summary>
    public static readonly StyledProperty<ICommand?> SelectCommandProperty =
        AvaloniaProperty.Register<NeighborhoodMapControl, ICommand?>(nameof(SelectCommand));

    private bool _pointerDown;
    private bool _draggedBeyondClick;
    private Point _downPoint;

    static NeighborhoodMapControl()
    {
        AffectsRender<NeighborhoodMapControl>(
            FrequencyHzProperty, BandLowHzProperty, BandHighHzProperty,
            NeighborhoodsProperty, ActivityFrequenciesProperty);
    }

    /// <summary>Creates the map.</summary>
    public NeighborhoodMapControl()
    {
        Cursor = new Cursor(StandardCursorType.Hand);
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

    /// <summary>The neighborhoods to draw.</summary>
    public IReadOnlyList<Neighborhood>? Neighborhoods
    {
        get => GetValue(NeighborhoodsProperty);
        set => SetValue(NeighborhoodsProperty, value);
    }

    /// <summary>Activity spot frequencies.</summary>
    public IReadOnlyList<long>? ActivityFrequencies
    {
        get => GetValue(ActivityFrequenciesProperty);
        set => SetValue(ActivityFrequenciesProperty, value);
    }

    /// <summary>Neighborhood click command.</summary>
    public ICommand? SelectCommand
    {
        get => GetValue(SelectCommandProperty);
        set => SetValue(SelectCommandProperty, value);
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

        // Activity dots at real spot frequencies; y scattered deterministically.
        if (ActivityFrequencies is { Count: > 0 } dots)
        {
            foreach (var hz in dots)
            {
                if (hz < BandLowHz || hz > BandHighHz)
                {
                    continue;
                }

                var x = (hz - BandLowHz) / span * w;
                var y = h * (0.42 + 0.4 * (hz / 100 % 7) / 7.0);
                context.DrawEllipse(DotBrush, null, new Point(x, y), 3, 3);
            }
        }

        context.DrawRectangle(null, EdgePen, new Rect(0.5, 0.5, w - 1, h - 1), 6, 6);

        var markerX = (FrequencyHz - BandLowHz) / span * w;
        context.FillRectangle(MarkerBrush, new Rect(markerX - 1, 0, 2, h));
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
        if (!_pointerDown)
        {
            return;
        }

        var p = e.GetPosition(this);
        if (!_draggedBeyondClick && Math.Abs(p.X - _downPoint.X) < 4)
        {
            return;
        }

        _draggedBeyondClick = true;
        TuneToPointer(p.X);
        e.Handled = true;
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
            var hood = HoodAt(e.GetPosition(this).X);
            if (hood is not null && SelectCommand?.CanExecute(hood) == true)
            {
                SelectCommand.Execute(hood);
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
}

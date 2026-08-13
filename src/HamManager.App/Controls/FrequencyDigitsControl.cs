using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using System.Globalization;

namespace HamManager.App.Controls;

/// <summary>
/// The frequency face: eight digits with group separators, each digit its
/// own knob — the mouse wheel over a digit tunes by that digit's place
/// value. Low-order digits render dimmed, the radio-face convention.
/// Approved design: HM-DEC-015.
/// </summary>
public sealed class FrequencyDigitsControl : Control
{
    private static readonly long[] Places =
    {
        10_000_000, 1_000_000, 100_000, 10_000, 1_000, 100, 10, 1,
    };

    // HM-DEC-012 palette.
    private static readonly IBrush DigitBrush = new SolidColorBrush(Color.Parse("#9A4A00"));
    private static readonly IBrush DimDigitBrush = new SolidColorBrush(Color.Parse("#C58A55"));
    private static readonly IBrush SeparatorBrush = new SolidColorBrush(Color.Parse("#E8C093"));
    private static readonly Typeface Mono = new("Consolas,Menlo,DejaVu Sans Mono,monospace");
    private const double FontSize = 40;

    /// <summary>Current frequency in hertz. Two-way: wheel tuning writes it back.</summary>
    public static readonly StyledProperty<long> FrequencyHzProperty =
        AvaloniaProperty.Register<FrequencyDigitsControl, long>(
            nameof(FrequencyHz), 7_030_000, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Band lower edge in hertz; tuning clamps here.</summary>
    public static readonly StyledProperty<long> BandLowHzProperty =
        AvaloniaProperty.Register<FrequencyDigitsControl, long>(nameof(BandLowHz), 0);

    /// <summary>Band upper edge in hertz; tuning clamps here.</summary>
    public static readonly StyledProperty<long> BandHighHzProperty =
        AvaloniaProperty.Register<FrequencyDigitsControl, long>(
            nameof(BandHighHz), long.MaxValue);

    private readonly double _digitWidth;
    private readonly double _separatorWidth;

    static FrequencyDigitsControl()
    {
        AffectsRender<FrequencyDigitsControl>(
            FrequencyHzProperty, BandLowHzProperty, BandHighHzProperty);
    }

    /// <summary>Creates the face and measures the monospace glyph cell.</summary>
    public FrequencyDigitsControl()
    {
        Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
        _digitWidth = Measure("0");
        _separatorWidth = Measure(".");
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

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => new(Places.Length * _digitWidth + 2 * _separatorWidth, FontSize * 1.2);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var x = 0.0;
        foreach (var place in Places)
        {
            var digit = (char)('0' + (int)(FrequencyHz / place % 10));
            var text = new FormattedText(
                digit.ToString(), CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, Mono, FontSize,
                place < 100 ? DimDigitBrush : DigitBrush);
            context.DrawText(text, new Point(x, 0));
            x += _digitWidth;

            if (place is 1_000_000 or 1_000)
            {
                var sep = new FormattedText(
                    ".", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    Mono, FontSize, SeparatorBrush);
                context.DrawText(sep, new Point(x, 0));
                x += _separatorWidth;
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        var place = PlaceAt(e.GetPosition(this).X);
        if (place is null)
        {
            return;
        }

        var next = FrequencyHz + (e.Delta.Y > 0 ? place.Value : -place.Value);
        SetCurrentValue(FrequencyHzProperty,
            Math.Min(BandHighHz, Math.Max(BandLowHz, next)));
        e.Handled = true;
    }

    private long? PlaceAt(double x)
    {
        var cursor = 0.0;
        foreach (var place in Places)
        {
            if (x >= cursor && x < cursor + _digitWidth)
            {
                return place;
            }

            cursor += _digitWidth;
            if (place is 1_000_000 or 1_000)
            {
                cursor += _separatorWidth;
            }
        }

        return null;
    }

    private static double Measure(string s)
        => new FormattedText(
            s, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            Mono, FontSize, Brushes.Black).WidthIncludingTrailingWhitespace;
}

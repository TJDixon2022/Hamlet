using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using System.Globalization;

namespace HamManager.App.Controls;

/// <summary>
/// The frequency face, styled after the IC-7300's own display: a dark LCD
/// behind bright digits, the final two (tens and ones of Hz) at half size
/// the way the rig renders them, leading zeros blanked, and a small amber
/// mode badge. Each digit is still its own knob — the mouse wheel over a
/// digit tunes by that digit's place value (HM-DEC-015).
/// </summary>
public sealed class FrequencyDigitsControl : Control
{
    private static readonly long[] Places =
    {
        10_000_000, 1_000_000, 100_000, 10_000, 1_000, 100, 10, 1,
    };

    // IC-7300 LCD look: near-black screen, cool white digits, amber mode text.
    private static readonly IBrush ScreenBrush = new SolidColorBrush(Color.Parse("#12161A"));
    private static readonly Pen BezelPen = new(new SolidColorBrush(Color.Parse("#3A424A")), 1.5);
    private static readonly IBrush DigitBrush = new SolidColorBrush(Color.Parse("#EDF2F5"));
    private static readonly IBrush BlankBrush = new SolidColorBrush(Color.Parse("#232A30"));
    private static readonly IBrush SeparatorBrush = new SolidColorBrush(Color.Parse("#9FB0BC"));
    private static readonly IBrush ModeBrush = new SolidColorBrush(Color.Parse("#FFB13B"));
    private static readonly Typeface Mono = new("Consolas,Menlo,DejaVu Sans Mono,monospace");

    private const double BigSize = 44;
    private const double SmallSize = 28;
    private const double PadX = 22;
    private const double PadTop = 26;
    private const double PadBottom = 12;

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

    /// <summary>Mode badge drawn top-left of the LCD, rig-style.</summary>
    public static readonly StyledProperty<string> ModeTextProperty =
        AvaloniaProperty.Register<FrequencyDigitsControl, string>(nameof(ModeText), "CW");

    private readonly double _bigWidth;
    private readonly double _smallWidth;
    private readonly double _sepWidth;
    private readonly double _bigHeight;
    private readonly double _smallHeight;

    static FrequencyDigitsControl()
    {
        AffectsRender<FrequencyDigitsControl>(
            FrequencyHzProperty, BandLowHzProperty, BandHighHzProperty, ModeTextProperty);
    }

    /// <summary>Creates the face and measures the two glyph cells.</summary>
    public FrequencyDigitsControl()
    {
        Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
        var big = Make("0", BigSize, DigitBrush);
        var small = Make("0", SmallSize, DigitBrush);
        _bigWidth = big.WidthIncludingTrailingWhitespace;
        _smallWidth = small.WidthIncludingTrailingWhitespace;
        _bigHeight = big.Height;
        _smallHeight = small.Height;
        _sepWidth = Make(".", BigSize, DigitBrush).WidthIncludingTrailingWhitespace;
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

    /// <summary>Mode badge text.</summary>
    public string ModeText
    {
        get => GetValue(ModeTextProperty);
        set => SetValue(ModeTextProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => new(ContentWidth() + 2 * PadX, PadTop + _bigHeight + PadBottom);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;

        // The LCD and its bezel.
        var screen = new Rect(0, 0, w, h);
        context.DrawRectangle(ScreenBrush, BezelPen, screen, 10, 10);

        // Mode badge, rig-style top-left.
        var mode = Make(ModeText, 13, ModeBrush);
        context.DrawText(mode, new Point(PadX, 7));

        // Digits, centered, right-anchored layout.
        var x = (w - ContentWidth()) / 2;
        var yBig = PadTop;
        var ySmall = PadTop + (_bigHeight - _smallHeight);
        var leadingBlank = true;

        foreach (var place in Places)
        {
            var digitValue = (int)(FrequencyHz / place % 10);
            var small = place < 100;
            var size = small ? SmallSize : BigSize;
            var y = small ? ySmall : yBig;

            if (digitValue != 0 || place == 1 || FrequencyHz >= place * 10)
            {
                leadingBlank = false;
            }

            if (leadingBlank && place > 1_000_000)
            {
                // The rig blanks leading zeros; a ghost cell keeps layout stable.
                var ghost = Make("8", size, BlankBrush);
                context.DrawText(ghost, new Point(x, y));
            }
            else
            {
                var text = Make(digitValue.ToString(CultureInfo.InvariantCulture),
                    size, DigitBrush);
                context.DrawText(text, new Point(x, y));
            }

            x += small ? _smallWidth : _bigWidth;

            if (place is 1_000_000 or 1_000)
            {
                var sep = Make(".", BigSize, SeparatorBrush);
                context.DrawText(sep, new Point(x, yBig));
                x += _sepWidth;
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
        var cursor = (Bounds.Width - ContentWidth()) / 2;
        foreach (var place in Places)
        {
            var cell = place < 100 ? _smallWidth : _bigWidth;
            if (x >= cursor && x < cursor + cell)
            {
                return place;
            }

            cursor += cell;
            if (place is 1_000_000 or 1_000)
            {
                cursor += _sepWidth;
            }
        }

        return null;
    }

    private double ContentWidth()
        => 6 * _bigWidth + 2 * _smallWidth + 2 * _sepWidth;

    private static FormattedText Make(string s, double size, IBrush brush)
        => new(s, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Mono, size, brush);
}

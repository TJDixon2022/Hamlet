using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System.Globalization;

namespace Hamlet.App.Controls;

/// <summary>
/// The IC-7300's display, drawn as the rig draws it: a status strip (mode,
/// filter box, RX badge, UTC clock), the frequency readout with the Hz pair
/// at half size and leading zeros as unlit ghost segments, and the S-meter
/// wedge with the S1–9 / +20/+40/+60 dB scale. The first of the rig's
/// screens Hamlet reproduces (HM-DEC-015 iteration). Wheel over a
/// digit tunes that digit.
/// </summary>
public sealed class RigDisplayControl : Control
{
    private static readonly long[] Places =
    {
        10_000_000, 1_000_000, 100_000, 10_000, 1_000, 100, 10, 1,
    };

    // The 7300 LCD: near-black glass, cool-white digits, amber accents.
    private static readonly IBrush ScreenBrush = new SolidColorBrush(Color.Parse("#0B0E11"));
    private static readonly Pen BezelPen = new(new SolidColorBrush(Color.Parse("#3A424A")), 1.5);
    private static readonly IBrush DigitBrush = new SolidColorBrush(Color.Parse("#F2F6F9"));
    private static readonly IBrush BlankBrush = new SolidColorBrush(Color.Parse("#1D242B"));
    private static readonly IBrush SeparatorBrush = new SolidColorBrush(Color.Parse("#9FB0BC"));
    private static readonly IBrush ModeBrush = new SolidColorBrush(Color.Parse("#F2F6F9"));
    private static readonly IBrush AmberBrush = new SolidColorBrush(Color.Parse("#FFB13B"));
    private static readonly IBrush RxBrush = new SolidColorBrush(Color.Parse("#39C46E"));
    private static readonly IBrush ScaleBrush = new SolidColorBrush(Color.Parse("#8FA0AC"));
    private static readonly IBrush MeterTrackBrush = new SolidColorBrush(Color.Parse("#1B2228"));

    /// <summary>The scale when there is nothing to show on it.</summary>
    private static readonly IBrush UnreadBrush = new SolidColorBrush(Color.Parse("#4A5A66"));
    private static readonly IBrush MeterFillBrush = new SolidColorBrush(Color.Parse("#DDEBF4"));
    private static readonly IBrush MeterOverBrush = new SolidColorBrush(Color.Parse("#E2483D"));
    private static readonly Pen FilterBoxPen = new(new SolidColorBrush(Color.Parse("#8FA0AC")), 1);
    private static readonly Typeface Mono = new("Consolas,Menlo,DejaVu Sans Mono,monospace");

    private const double BigSize = 44;
    private const double SmallSize = 28;
    private const double PadX = 22;
    private const double StripHeight = 30;
    private const double MeterHeight = 34;
    private const double PadBottom = 10;

    /// <summary>Current frequency in hertz. Two-way: wheel tuning writes it back.</summary>
    public static readonly StyledProperty<long> FrequencyHzProperty =
        AvaloniaProperty.Register<RigDisplayControl, long>(
            nameof(FrequencyHz), 7_030_000, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Band lower edge in hertz; tuning clamps here.</summary>
    public static readonly StyledProperty<long> BandLowHzProperty =
        AvaloniaProperty.Register<RigDisplayControl, long>(nameof(BandLowHz), 0);

    /// <summary>Band upper edge in hertz; tuning clamps here.</summary>
    public static readonly StyledProperty<long> BandHighHzProperty =
        AvaloniaProperty.Register<RigDisplayControl, long>(
            nameof(BandHighHz), long.MaxValue);

    /// <summary>
    /// Mode indicator, rig top-left. Empty until the radio has been asked.
    /// </summary>
    /// <remarks>
    /// THIS USED TO DEFAULT TO "CW" and was bound to the literal "CW" in the
    /// window besides, so the screen said CW whatever the radio was set to. It
    /// was the app's oldest prime-directive violation and it survived because
    /// nothing ever read the real mode (HM-DEC-050). The default is empty now:
    /// a blank badge is somebody not having asked yet, which is true, and a
    /// badge reading CW is a claim.
    /// </remarks>
    public static readonly StyledProperty<string> ModeTextProperty =
        AvaloniaProperty.Register<RigDisplayControl, string>(nameof(ModeText), "");

    /// <summary>Filter indicator in the rig's bordered box. Empty until read.</summary>
    /// <remarks>Same story as the mode: it always read FIL2.</remarks>
    public static readonly StyledProperty<string> FilterTextProperty =
        AvaloniaProperty.Register<RigDisplayControl, string>(nameof(FilterText), "");

    /// <summary>
    /// S-meter deflection, 0.0 to 1.0 of full scale, or null when there is no
    /// reading. 0.6 is S9 and above that is the red decibels-over region.
    /// </summary>
    /// <remarks>
    /// NULL IS NOT ZERO, and the meter draws them differently. Zero is a
    /// measurement of a quiet band; null is nobody having asked. They would look
    /// identical as an unlit bar, so the scale itself dims and the meter says
    /// "no reading" instead of leaving somebody to read a resting needle as
    /// silence on the air (§0.0).
    /// </remarks>
    public static readonly StyledProperty<double?> SMeterLevelProperty =
        AvaloniaProperty.Register<RigDisplayControl, double?>(nameof(SMeterLevel), null);

    private readonly double _bigWidth;
    private readonly double _smallWidth;
    private readonly double _sepWidth;
    private readonly double _bigHeight;
    private readonly double _smallHeight;
    private readonly DispatcherTimer _clockTimer;

    static RigDisplayControl()
    {
        AffectsRender<RigDisplayControl>(
            FrequencyHzProperty, BandLowHzProperty, BandHighHzProperty,
            ModeTextProperty, FilterTextProperty, SMeterLevelProperty);
    }

    /// <summary>Creates the display; the UTC clock repaints once a second.</summary>
    public RigDisplayControl()
    {
        Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
        var big = Make("0", BigSize, DigitBrush);
        var small = Make("0", SmallSize, DigitBrush);
        _bigWidth = big.WidthIncludingTrailingWhitespace;
        _smallWidth = small.WidthIncludingTrailingWhitespace;
        _bigHeight = big.Height;
        _smallHeight = small.Height;
        _sepWidth = Make(".", BigSize, DigitBrush).WidthIncludingTrailingWhitespace;

        _clockTimer = new DispatcherTimer(
            TimeSpan.FromSeconds(1), DispatcherPriority.Background,
            (_, _) => InvalidateVisual());
        AttachedToVisualTree += (_, _) => _clockTimer.Start();
        DetachedFromVisualTree += (_, _) => _clockTimer.Stop();
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

    /// <summary>Mode indicator text.</summary>
    public string ModeText
    {
        get => GetValue(ModeTextProperty);
        set => SetValue(ModeTextProperty, value);
    }

    /// <summary>Filter indicator text.</summary>
    public string FilterText
    {
        get => GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    /// <summary>S-meter deflection, 0.0 to 1.0, or null when unknown.</summary>
    public double? SMeterLevel
    {
        get => GetValue(SMeterLevelProperty);
        set => SetValue(SMeterLevelProperty, value);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => new(
            Math.Max(ContentWidth() + 2 * PadX, 520),
            StripHeight + _bigHeight + MeterHeight + PadBottom);

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;

        context.DrawRectangle(ScreenBrush, BezelPen, new Rect(0, 0, w, h), 10, 10);

        DrawStatusStrip(context, w);
        DrawFrequency(context, w);
        DrawSMeter(context, w, h);
    }

    private void DrawStatusStrip(DrawingContext context, double w)
    {
        var x = PadX;

        var mode = Make(ModeText, 15, ModeBrush);
        context.DrawText(mode, new Point(x, 8));
        x += mode.WidthIncludingTrailingWhitespace + 10;

        // Filter designator in the rig's bordered box, and no box at all when
        // there is no designator to put in it. An empty box invites the reader
        // to wonder what is missing; nothing at all says the radio has not been
        // asked, which is what a blank mode badge beside it is already saying.
        if (FilterText.Length > 0)
        {
            var fil = Make(FilterText, 11, ModeBrush);
            var box = new Rect(x, 8, fil.WidthIncludingTrailingWhitespace + 10, 17);
            context.DrawRectangle(null, FilterBoxPen, box, 3, 3);
            context.DrawText(fil, new Point(x + 5, 10));
            x += box.Width + 12;
        }

        var rx = Make("RX", 12, RxBrush);
        context.DrawText(rx, new Point(x, 9));

        // UTC clock, right corner — real time, the one clock hams live on.
        var clock = Make(DateTime.UtcNow.ToString("HH:mm", CultureInfo.InvariantCulture)
            + " UTC", 12, ScaleBrush);
        context.DrawText(clock,
            new Point(w - PadX - clock.WidthIncludingTrailingWhitespace, 9));
    }

    private void DrawFrequency(DrawingContext context, double w)
    {
        var x = (w - ContentWidth()) / 2;
        var yBig = StripHeight;
        var ySmall = StripHeight + (_bigHeight - _smallHeight);
        var leadingBlank = true;

        foreach (var place in Places)
        {
            var digitValue = (int)(FrequencyHz / place % 10);
            var small = place < 100;
            var size = small ? SmallSize : BigSize;
            var y = small ? ySmall : yBig;

            if (digitValue != 0 || FrequencyHz >= place * 10)
            {
                leadingBlank = false;
            }

            if (leadingBlank && place > 1_000_000)
            {
                context.DrawText(Make("8", size, BlankBrush), new Point(x, y));
            }
            else
            {
                context.DrawText(
                    Make(digitValue.ToString(CultureInfo.InvariantCulture), size, DigitBrush),
                    new Point(x, y));
            }

            x += small ? _smallWidth : _bigWidth;

            if (place is 1_000_000 or 1_000)
            {
                context.DrawText(Make(".", BigSize, SeparatorBrush), new Point(x, yBig));
                x += _sepWidth;
            }
        }
    }

    private void DrawSMeter(DrawingContext context, double w, double h)
    {
        var left = PadX + 26;
        var right = w - PadX;
        var baseY = h - PadBottom - 4;
        var span = right - left;
        var s9X = left + span * 0.6;

        // The whole scale dims when there is no reading, which is the only
        // thing that tells "nobody asked" apart from "the band is quiet": both
        // draw an unlit bar (§0.0).
        var reading = SMeterLevel;
        var scaleBrush = reading is null ? UnreadBrush : ScaleBrush;

        context.DrawText(Make("S", 13, scaleBrush), new Point(PadX, baseY - 16));

        // Scale: S1..S9 white region, +20/+40/+60 dB red region.
        var ticks = new (double Frac, string Label)[]
        {
            (0.0 / 15, "1"), (2.0 / 15, "3"), (4.0 / 15, "5"),
            (6.0 / 15, "7"), (0.6, "9"),
            (0.6 + 0.4 / 3, "+20"), (0.6 + 0.8 / 3, "+40"), (1.0, "+60"),
        };
        foreach (var (frac, label) in ticks)
        {
            var tx = left + span * frac;
            var t = Make(
                label, 9,
                reading is null ? UnreadBrush : frac > 0.55 ? MeterOverBrush : ScaleBrush);
            context.DrawText(t,
                new Point(Math.Min(tx - t.Width / 2, right - t.Width), baseY - 26));
        }

        // The wedge: segmented bar that grows taller toward full scale.
        const int segments = 30;
        var lit = reading is null
            ? 0
            : (int)Math.Round(Math.Clamp(reading.Value, 0, 1) * segments);
        var segW = span / segments;
        for (var i = 0; i < segments; i++)
        {
            var sx = left + i * segW;
            var frac = (i + 1) / (double)segments;
            var segH = 4 + frac * 8;
            var brush = i < lit
                ? (frac > 0.6 ? MeterOverBrush : MeterFillBrush)
                : MeterTrackBrush;
            context.FillRectangle(brush,
                new Rect(sx + 0.5, baseY - segH, segW - 1.5, segH));
        }

        // S9 marker line.
        context.DrawLine(new Pen(scaleBrush, 1),
            new Point(s9X, baseY - 14), new Point(s9X, baseY));

        if (reading is null)
        {
            var note = Make("no reading", 10, UnreadBrush);
            context.DrawText(note, new Point(right - note.Width, baseY - 14));
        }
    }

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        var place = PlaceAt(e.GetPosition(this));
        if (place is null)
        {
            return;
        }

        var next = FrequencyHz + (e.Delta.Y > 0 ? place.Value : -place.Value);
        SetCurrentValue(FrequencyHzProperty,
            Math.Min(BandHighHz, Math.Max(BandLowHz, next)));
        e.Handled = true;
    }

    private long? PlaceAt(Point p)
    {
        if (p.Y < StripHeight || p.Y > StripHeight + _bigHeight)
        {
            return null;
        }

        var cursor = (Bounds.Width - ContentWidth()) / 2;
        foreach (var place in Places)
        {
            var cell = place < 100 ? _smallWidth : _bigWidth;
            if (p.X >= cursor && p.X < cursor + cell)
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

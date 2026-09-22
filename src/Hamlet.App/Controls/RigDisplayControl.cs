using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

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

    /// <summary>The star once this frequency is saved. The brightest thing here.</summary>
    private static readonly IBrush StarOnBrush = new SolidColorBrush(Color.Parse("#FFC65C"));

    /// <summary>The star before it is saved: findable, and plainly not lit.</summary>
    private static readonly IBrush StarOffBrush = new SolidColorBrush(Color.Parse("#C99A4A"));

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

    /// <summary>
    /// The blank under the S-meter's wedge - **6 px since work instruction 376**, from 10.
    /// </summary>
    /// <remarks>
    /// **R39: THE RIG DISPLAY SHORTER** (task 2), and this is the only part of the face that is
    /// blank. Nothing the LCD draws moved: the status strip is placed from the top at
    /// <see cref="StripHeight"/>, the digits under it, and the meter from the bottom at
    /// <c>h - PadBottom - 4</c> - so the wedge, its scale and its labels keep the same distance
    /// from the digits above them and only the empty band beneath the wedge is shorter. The
    /// star is where HM-DEC-070 put it and the face is still not collapsible (HM-DEC-021).
    /// </remarks>
    private const double PadBottom = 6;

    /// <summary>
    /// True once the operator has tuned with the wheel (HM-DEC-141).
    /// </summary>
    /// <remarks>
    /// Two-way, so the view model can remember it across launches. The hint that
    /// explains the wheel retires when this goes true, and the readout's tooltip
    /// carries it from then on.
    /// </remarks>
    public static readonly StyledProperty<bool> HasTunedByWheelProperty =
        AvaloniaProperty.Register<RigDisplayControl, bool>(
            nameof(HasTunedByWheel), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>True once the wheel has been used to tune.</summary>
    public bool HasTunedByWheel
    {
        get => GetValue(HasTunedByWheelProperty);
        set => SetValue(HasTunedByWheelProperty, value);
    }

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

    /// <summary>
    /// True when the dial is sitting on a saved frequency (HM-DEC-070).
    /// </summary>
    public static readonly StyledProperty<bool> IsFavoriteProperty =
        AvaloniaProperty.Register<RigDisplayControl, bool>(nameof(IsFavorite));

    /// <summary>What the star says: two short words at most.</summary>
    public static readonly StyledProperty<string> FavoriteLabelProperty =
        AvaloniaProperty.Register<RigDisplayControl, string>(nameof(FavoriteLabel), "save");

    /// <summary>Invoked when the star is pressed. Saves, or un-saves.</summary>
    public static readonly StyledProperty<ICommand?> ToggleFavoriteCommandProperty =
        AvaloniaProperty.Register<RigDisplayControl, ICommand?>(nameof(ToggleFavoriteCommand));

    /// <summary>
    /// **The saved places, each with its own way there** - R46(a), work instruction 387.
    /// </summary>
    /// <remarks>
    /// <para>**THE VIEW MODEL DECIDES AND THIS DRAWS**, the rule already in force at
    /// <c>MainWindow.axaml.cs</c>. Every line's words and every line's command are
    /// <c>MainWindowViewModel.FavoriteMenu</c>'s, which is the same list the Radio menu shows
    /// and is built by the same <c>RebuildMenus</c>; **the list a test reads and the list Tim
    /// sees are one list**, and nothing here decides what a favorite is called or what tuning
    /// to one does.</para>
    /// <para>**NOTHING IS REWRITTEN** (R14). Each line already carries <c>TuneToFavorite</c>
    /// with its own <c>FavoriteTuned</c> telemetry; this only gives that list a door on the rig
    /// face, which is where R46(a) says Tim had it.</para>
    /// </remarks>
    public static readonly StyledProperty<IEnumerable<ViewModels.TuneMenuItem>?> FavoritesProperty =
        AvaloniaProperty.Register<RigDisplayControl, IEnumerable<ViewModels.TuneMenuItem>?>(nameof(Favorites));

    /// <summary>Rename, reorder and delete - the window the Radio menu already opens.</summary>
    public static readonly StyledProperty<ICommand?> ManageFavoritesCommandProperty =
        AvaloniaProperty.Register<RigDisplayControl, ICommand?>(nameof(ManageFavoritesCommand));

    private readonly double _bigWidth;
    private readonly double _smallWidth;
    private readonly double _sepWidth;
    private readonly double _bigHeight;
    private readonly double _smallHeight;
    private readonly DispatcherTimer _clockTimer;

    /// <summary>Where the star was last drawn, for hit testing.</summary>
    /// <remarks>
    /// Set during render rather than computed twice. The glyph and the word move
    /// with the mode badge beside them, so the only honest source for where it is
    /// is where it was actually put.
    /// </remarks>
    private Rect _starRect = default;

    /// <summary>
    /// Where the caret that opens the saved list was last drawn, for hit testing - `default`
    /// where the strip ran too short to draw it.
    /// </summary>
    /// <remarks>
    /// **IT IS A SECOND TARGET AND NOT A SECOND STAR.** The star saves where you are; this
    /// opens where you have been. They are adjacent and they do not overlap, which is asserted
    /// rather than eyeballed.
    /// </remarks>
    private Rect _listRect = default;

    static RigDisplayControl()
    {
        AffectsRender<RigDisplayControl>(
            FrequencyHzProperty, BandLowHzProperty, BandHighHzProperty,
            ModeTextProperty, FilterTextProperty, SMeterLevelProperty,
            IsFavoriteProperty, FavoriteLabelProperty, FavoritesProperty);
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

    /// <summary>True when this frequency is already saved.</summary>
    public bool IsFavorite
    {
        get => GetValue(IsFavoriteProperty);
        set => SetValue(IsFavoriteProperty, value);
    }

    /// <summary>What the star says.</summary>
    public string FavoriteLabel
    {
        get => GetValue(FavoriteLabelProperty);
        set => SetValue(FavoriteLabelProperty, value);
    }

    /// <summary>What pressing the star runs.</summary>
    public ICommand? ToggleFavoriteCommand
    {
        get => GetValue(ToggleFavoriteCommandProperty);
        set => SetValue(ToggleFavoriteCommandProperty, value);
    }

    /// <summary>The saved places, each with its own way there.</summary>
    public IEnumerable<ViewModels.TuneMenuItem>? Favorites
    {
        get => GetValue(FavoritesProperty);
        set => SetValue(FavoritesProperty, value);
    }

    /// <summary>What the list's last line runs.</summary>
    public ICommand? ManageFavoritesCommand
    {
        get => GetValue(ManageFavoritesCommandProperty);
        set => SetValue(ManageFavoritesCommandProperty, value);
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
        x += rx.WidthIncludingTrailingWhitespace + 16;

        // UTC clock, right corner — real time, the one clock hams live on.
        var clock = Make(DateTime.UtcNow.ToString("HH:mm", CultureInfo.InvariantCulture)
            + " UTC", 12, ScaleBrush);
        var clockX = w - PadX - clock.WidthIncludingTrailingWhitespace;
        context.DrawText(clock, new Point(clockX, 9));

        DrawStar(context, x, clockX);
    }

    /// <summary>
    /// The star, inside the black, where it is the most findable thing here
    /// (HM-DEC-070).
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="x">Where the strip's left cluster ended.</param>
    /// <param name="clockX">Where the clock starts, which is the hard limit.</param>
    /// <remarks>
    /// <para>INSIDE THE LCD ON PURPOSE, which reverses what HM-DEC-060 built.
    /// That ruling kept Hamlet's own chrome off the black so the picture stayed a
    /// faithful IC-7300 face, and Tim weighed that against being able to find the
    /// thing and chose being able to find it. A star against near-black is the
    /// brightest object on the panel.</para>
    /// <para>TWO SHORT WORDS AT MOST, so the strip cannot outgrow itself. The
    /// word is dropped rather than overlapped if the clock ever gets close, which
    /// the minimum width already prevents; a layout that is only correct because
    /// of a constant somewhere else is one refactor from being wrong.</para>
    /// <para>**AND THE CARET BESIDE IT OPENS THE LIST** (R46(a), work instruction 387). Tim had
    /// a favorites list and a unit took it off the screen on 2026-08-27 without ruling that it
    /// should go; it comes back **here**, on the rig face, because that is where he had the star
    /// and because **the top band may not grow by one pixel** (6.1, and the carry-forward guard
    /// that ratchets it at 214). A glyph inside the black is the one door that costs no height
    /// at all. The Radio menu keeps its own copy: two ways in is not a defect.</para>
    /// <para>**THE CARET IS DROPPED BEFORE THE WORD IS**, on the same rule as the word, and its
    /// hit rectangle is zeroed with it - so a caret that is not drawn cannot be hit.</para>
    /// </remarks>
    private void DrawStar(DrawingContext context, double x, double clockX)
    {
        var brush = IsFavorite ? StarOnBrush : StarOffBrush;
        var glyph = Make(IsFavorite ? "★" : "☆", 15, brush);
        var word = Make(FavoriteLabel, 12, brush);
        var caret = Make("▾", 13, brush);

        var wordWidth = word.WidthIncludingTrailingWhitespace;
        var caretWidth = caret.WidthIncludingTrailingWhitespace;
        var full = glyph.WidthIncludingTrailingWhitespace + 5 + wordWidth;

        // A gap the clock is never allowed inside.
        var room = clockX - 14 - x;

        if (room < glyph.WidthIncludingTrailingWhitespace)
        {
            _starRect = default;
            _listRect = default;
            return;
        }

        context.DrawText(glyph, new Point(x, 6));

        var drawWord = room >= full;

        if (drawWord)
        {
            context.DrawText(
                word, new Point(x + glyph.WidthIncludingTrailingWhitespace + 5, 9));
        }

        // **THE CARET SITS CLEAR OF THE STAR'S OWN TARGET**, which runs to `x + full + 6`, so
        // the two rectangles cannot overlap and a press cannot mean both things.
        var caretX = x + (drawWord ? full : glyph.WidthIncludingTrailingWhitespace) + 11;

        if (room >= caretX + caretWidth - x)
        {
            context.DrawText(caret, new Point(caretX, 8));

            _listRect = new Rect(caretX - 4, 2, caretWidth + 8, StripHeight - 4);
        }
        else
        {
            _listRect = default;
        }

        // Padded outward, because a target the size of a glyph is a target
        // somebody misses.
        _starRect = new Rect(
            x - 6, 2,
            (drawWord ? full : glyph.WidthIncludingTrailingWhitespace) + 12,
            StripHeight - 4);
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
    /// <remarks>
    /// The star is the one thing in here that is pressed rather than tuned, so
    /// the pointer says so before the click rather than after (HM-DEC-070).
    /// </remarks>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var at = e.GetPosition(this);

        Cursor = new Cursor(
            _starRect.Contains(at) || _listRect.Contains(at)
                ? StandardCursorType.Hand
                : StandardCursorType.SizeNorthSouth);
    }

    /// <inheritdoc/>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var at = e.GetPosition(this);

        // **THE CARET IS ASKED FIRST**, because it is the smaller of the two targets and the
        // rectangles are built not to overlap; asking it first means a fault in that arithmetic
        // shows up as the list opening rather than as a frequency silently being saved.
        if (_listRect.Contains(at))
        {
            OpenTheSavedList();
            e.Handled = true;
            return;
        }

        if (!_starRect.Contains(at))
        {
            return;
        }

        var command = ToggleFavoriteCommand;

        if (command?.CanExecute(null) == true)
        {
            command.Execute(null);
        }

        e.Handled = true;
    }

    /// <summary>
    /// **The saved places, on the rig face, one click from where the dial is** - R46(a).
    /// </summary>
    /// <remarks>
    /// <para>**EVERY LINE IS THE VIEW MODEL'S** - <c>FavoriteMenu</c>'s words and
    /// <c>FavoriteMenu</c>'s commands, which are the Radio menu's own. Nothing here decides what a
    /// favorite is called, what tuning to one does or what it records.</para>
    /// <para>**NOTHING IS GREYED, HIDDEN, SORTED AWAY OR DISABLED** (ruled 2026-09-06). With
    /// nothing saved the list is not an empty box and not a disabled item: it is a note saying so,
    /// and a note carries no command and cannot be hit.</para>
    /// <para>**IT OPENS A LIST AND SENDS NOTHING** (§0.2). No composer, no arming, no keying.</para>
    /// </remarks>
    private void OpenTheSavedList()
    {
        SavedListUnderThePointer = null;

        var flyout = new MenuFlyout();
        var saved = Favorites?.ToList() ?? new List<ViewModels.TuneMenuItem>();

        if (saved.Count == 0)
        {
            flyout.Items.Add(new MenuItem
            {
                Header = "Nothing saved here yet - press the star to save where you are.",
                IsHitTestVisible = false,
            });
        }
        else
        {
            foreach (var item in saved)
            {
                flyout.Items.Add(new MenuItem { Header = item.Label, Command = item.Tune });
            }
        }

        if (ManageFavoritesCommand is { } manage)
        {
            flyout.Items.Add(new Separator());
            flyout.Items.Add(new MenuItem { Header = "Manage favorites…", Command = manage });
        }

        SavedListUnderThePointer = flyout;

        flyout.ShowAt(this, showAtPointer: true);
    }

    /// <summary>The list the last press on the caret put up, for a test to read.</summary>
    /// <remarks>
    /// **RECORDED FOR A TEST AND READ BY NOTHING IN THE APPLICATION**, the shape
    /// <c>MainWindow.SendFlyoutUnderTheMouse</c> already uses. A headless test can press the real
    /// caret on a real rig face and read back exactly what appeared; without it the only evidence
    /// would be a popup's presence in a visual tree, which says nothing about what was in it.
    /// </remarks>
    internal MenuFlyout? SavedListUnderThePointer { get; private set; }

    /// <inheritdoc/>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        var at = e.GetPosition(this);

        if (_starRect.Contains(at) || _listRect.Contains(at))
        {
            return;
        }

        var place = PlaceAt(at);
        if (place is null)
        {
            return;
        }

        var next = FrequencyHz + (e.Delta.Y > 0 ? place.Value : -place.Value);
        SetCurrentValue(FrequencyHzProperty,
            Math.Min(BandHighHz, Math.Max(BandLowHz, next)));

        // THE HINT HAS DONE ITS JOB (HM-DEC-141). A line telling somebody how to
        // do a thing they have just done is a line that teaches them to stop
        // reading that part of the window.
        SetCurrentValue(HasTunedByWheelProperty, true);

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

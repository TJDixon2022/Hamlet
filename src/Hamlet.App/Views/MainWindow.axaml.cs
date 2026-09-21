using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;

namespace Hamlet.App.Views;

/// <summary>
/// The main window. Its code-behind owns only the facts the view knows and the
/// ViewModel cannot: which keys were pressed, whether anybody is looking at it,
/// and **where the mouse was when the operator asked a row for its menu**.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>Creates the window and wires the view-only facts.</summary>
    public MainWindow()
    {
        InitializeComponent();

        // **THE WINDOW AND THE TASKBAR CARRY THE MARK** (work instruction 285 task
        // 3). It is rendered from `Assets/hamlet-mark-small.svg` rather than shipped
        // as a second drawing, and it is set here rather than in the markup because
        // rasterising wants a platform that can draw. **Null where one cannot** -
        // a headless run has no rasteriser, and a window that refused to open
        // because it could not draw a picture of itself would be a poor trade.
        if (Controls.AppIcon.Small is { } mark)
        {
            Icon = mark;
        }

        // **A BADGE SAYS SO ON THE SCREEN** (Tim, 2026-09-08, work instruction 286
        // task 2). Subscribed here rather than in the view model, so the view model
        // decides what he is told and the view decides what a notice looks like.
        DataContextChanged += (_, _) =>
        {
            if (DataContext is MainWindowViewModel panel)
            {
                panel.BadgeEarned -= OnBadgeEarned;
                panel.BadgeEarned += OnBadgeEarned;
            }
        };

        // Arrow keys = ±10 Hz, the headphone-tuning path (HM-DEC-015).
        AddHandler(KeyDownEvent, OnTuneKey, handledEventsToo: false);

        // **THE SPARKLINE GIVES WAY WHERE THE GREEN BLOCK'S TEXT WOULD WRAP** (work instruction
        // 338 task 2, the arbiter's ruling 3). How wide the words are drawn is the view's fact,
        // so the view decides; `FitTheHeardCount` states the rule.
        LayoutUpdated += (_, _) => FitTheHeardCount();

        // The feed pauses when nobody is looking (HM-DEC-020). Visibility is
        // the view's fact, so the view pushes it; the ViewModel owns what to
        // do about it.
        PropertyChanged += OnWindowPropertyChanged;
        Opened += (_, _) =>
        {
            PushVisibility();
            StartReconnect();
            };

        // A LAST SAVE ON THE WAY OUT (HM-DEC-089). Every change to the canvas
        // already saves as it happens, so this is a backstop rather than the
        // mechanism, and a backstop is worth having for the one thing the
        // operator would have to rebuild by hand.
        Closing += (_, _) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.KeepTheCanvas();
            }
        };

        // **WHERE THE DECODED LIST HAS BEEN SCROLLED TO** (R36, criterion 3.1). Which
        // rows are inside a viewport is a fact the view knows and the view model
        // cannot, exactly as the mouse position is, so the view reads it and the view
        // model decides what it means. **It draws nothing and nothing on screen
        // depends on it** (§0.2).
        DigitalDecodedScroller.ScrollChanged += (_, _) => TheDecodedListMoved();
    }

    /// <summary>How still a scroller has to be before the settle counts.</summary>
    /// <remarks>
    /// **A QUARTER OF A SECOND** (work instruction 380 section 6 ruling 1 item 3). A drag
    /// raises `ScrollChanged` on every frame; a line each would be hundreds of lines about
    /// one gesture, against a measured budget of 191 lines an hour. What a diagnosis needs
    /// is where he came to rest.
    /// </remarks>
    private static readonly TimeSpan Settle = TimeSpan.FromMilliseconds(250);

    /// <summary>The timer waiting for the decoded list to come to rest, or null.</summary>
    private DispatcherTimer? _decodedSettle;

    /// <summary>Restart the settle clock, because the list has just moved.</summary>
    private void TheDecodedListMoved()
    {
        _decodedSettle ??= new DispatcherTimer { Interval = Settle };

        _decodedSettle.Stop();
        _decodedSettle.Tick -= OnDecodedListSettled;
        _decodedSettle.Tick += OnDecodedListSettled;
        _decodedSettle.Start();
    }

    /// <summary>The decoded list has been still for a quarter of a second.</summary>
    /// <param name="sender">The timer.</param>
    /// <param name="e">Nothing.</param>
    /// <remarks>
    /// **THE INDEX RANGE IS ARITHMETIC ON CONTAINERS THAT ALREADY EXIST.** The decoded
    /// `ItemsControl` does not virtualize, so every bound row has a realized container, and
    /// a container's bounds against the scroller's offset and viewport say whether it is
    /// inside. **Nothing new is drawn to work it out** (work instruction 380 section 6 ruling
    /// 2 item 3), and a row with no container yet is simply not counted rather than guessed
    /// at (§0.0).
    /// </remarks>
    private void OnDecodedListSettled(object? sender, EventArgs e)
    {
        _decodedSettle?.Stop();

        if (DataContext is not MainWindowViewModel panel)
        {
            return;
        }

        var scroller = DigitalDecodedScroller;
        var rows = DigitalDecodedRows;

        var top = scroller.Offset.Y;
        var bottom = top + scroller.Viewport.Height;

        var first = -1;
        var last = -1;

        for (var at = 0; at < rows.ItemCount; at++)
        {
            if (rows.ContainerFromIndex(at) is not Control container)
            {
                continue;
            }

            var start = container.Bounds.Y;
            var end = start + container.Bounds.Height;

            if (end <= top || start >= bottom)
            {
                continue;
            }

            if (first < 0)
            {
                first = at;
            }

            last = at;
        }

        panel.DecodedPanelScrolled(
            first, last, scroller.Extent.Height, scroller.Viewport.Height, top);
    }


    /// <summary>The gap between the sparkline and the count, as the markup has it.</summary>
    private const double HeardGap = 10;

    /// <summary>
    /// **Hide the sparkline where the green block's text column, beside it, would wrap** - the
    /// arbiter's ruling 3 in work instruction 338, with the width rule the unit's own.
    /// </summary>
    /// <remarks>
    /// <para>**THE RULE, MARKED AS THE UNIT'S OWN AND OVERRULABLE.** The text column is what the
    /// green block's regions leave once the right-hand column takes its wide width - *heard just
    /// now* or the sparkline, the 10 px gap, and the count or *last minute*, or the best-bet line
    /// if that is wider - less the text column's own right margin. Where that is narrower than
    /// the widest line the column holds on one line - the license phrase, the rule of thumb, or
    /// the band, the frequency and the verdict together - the sparkline hides and *heard just
    /// now* stands over the count, so the right-hand column is only as wide as its words. **The
    /// count stays at every width.**</para>
    /// <para>**MEASURED FROM THE WORDS, NOT FROM A WINDOW WIDTH**, because the test host and the
    /// glass draw the same words at different widths, and a breakpoint picked on one would be
    /// wrong on the other. The decision reads the width the right-hand column would have if it
    /// were wide, never its current width, so hiding the sparkline cannot undo itself on the
    /// next layout pass.</para>
    /// <para>**THE BEST BET ROW FOLLOWS THE SAME RULE SINCE WORK INSTRUCTION 351, MARKED AS THAT
    /// UNIT'S OWN AND OVERRULABLE.** Where the sparkline hides, the best bet row is held to the widest
    /// of the heard words, or of *best bet now:* or the band where one of those is wider, so the band
    /// stands under *best bet now:* and the right-hand column stays as wide as its heard words
    /// whatever the best bet says. Measured at 1400 on PSK31 with the best bet on his band, the check
    /// widened the column 180 to 200 px and the verdict took a second line: the top row 247 px against
    /// 238.4. Held, the column is 140 px and the row 228. **Nothing hides and no word changes**; at
    /// 1920 the row is one line, as before.</para>
    /// </remarks>
    private void FitTheHeardCount()
    {
        if (Named<Grid>("GreenZoneRegions") is not { } regions
            || Named<StackPanel>("GreenZoneLeft") is not { } left
            || Named<Control>("GreenZoneSparkline") is not { } sparkline
            || Named<TextBlock>("GreenZoneHeardLabel") is not { } label
            || Named<TextBlock>("GreenZoneHeard") is not { } count
            || Named<TextBlock>("GreenZoneHeardWindow") is not { } lastMinute
            || regions.Bounds.Width <= 0)
        {
            return;
        }

        var heardWide = System.Math.Max(TextWidth(label), sparkline.Width) + HeardGap
            + System.Math.Max(TextWidth(count), TextWidth(lastMinute));

        var prefix = Named<TextBlock>("GreenZoneBestBetPrefix");
        var bet = Named<Button>("GreenZoneBestBet");
        var betShown = prefix is { IsVisible: true } && bet is { IsVisible: true };
        var prefixWidth = betShown ? TextWidth(prefix!) : 0;
        var betWidth = betShown ? TextWidth(bet!.Content as string, bet) : 0;
        var bestBet = prefixWidth + betWidth;

        var column = regions.Bounds.Width - System.Math.Max(heardWide, bestBet) - left.Margin.Right;

        var bandLine = 0.0;

        foreach (var name in new[] { "GreenZoneBand", "GreenZoneFrequency", "GreenZoneModeLine" })
        {
            if (Named<TextBlock>(name) is { } part)
            {
                bandLine += TextWidth(part) + (part.IsVisible ? part.Margin.Right : 0);
            }
        }

        var need = System.Math.Max(
            bandLine,
            System.Math.Max(
                Named<TextBlock>("GreenZoneLicenseLine") is { } license ? TextWidth(license) : 0,
                Named<TextBlock>("GreenZoneRuleOfThumb") is { } rule ? TextWidth(rule) : 0));

        var narrow = column < need;

        // **THE BEST BET ROW HELD TO THE HEARD WORDS WHERE THE SPARKLINE HIDES** (work instruction
        // 351): the widest of *heard just now*, the count and *last minute*, or the prefix or the band
        // alone where one of those is wider, so the row wraps and nothing is clipped. Set on every
        // pass, because the best bet's word changes while the sparkline's state does not.
        if (Named<WrapPanel>("GreenZoneBestBetRow") is { } row)
        {
            var held = narrow
                ? System.Math.Max(
                    System.Math.Max(TextWidth(label), System.Math.Max(TextWidth(count), TextWidth(lastMinute))),
                    System.Math.Max(prefixWidth, betWidth))
                : double.PositiveInfinity;

            if (row.MaxWidth != held)
            {
                row.MaxWidth = held;
            }
        }

        if (sparkline.IsVisible != narrow)
        {
            return;
        }

        sparkline.IsVisible = !narrow;
        Grid.SetColumn(label, narrow ? 1 : 0);
        label.Margin = narrow ? default : new Avalonia.Thickness(0, 0, HeardGap, 0);
    }

    /// <summary>The controls the width rule reads, found once and kept.</summary>
    private readonly System.Collections.Generic.Dictionary<string, Control> _widthRuleControls = new();

    /// <summary>
    /// A named control in this window, by its name scope and then by a walk of the visual tree,
    /// kept once found so the walk is not repeated on every layout pass.
    /// </summary>
    private T? Named<T>(string name)
        where T : Control
    {
        if (_widthRuleControls.TryGetValue(name, out var known))
        {
            return known as T;
        }

        var found = this.FindControl<T>(name)
            ?? System.Linq.Enumerable.FirstOrDefault(
                System.Linq.Enumerable.OfType<T>(Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(this)),
                c => c.Name == name);

        if (found is not null)
        {
            _widthRuleControls[name] = found;
        }

        return found;
    }

    /// <summary>How wide a text block's own words are drawn on one line, or 0 where it is hidden.</summary>
    private static double TextWidth(TextBlock block)
        => block.IsVisible ? TextWidth(block.Text, block) : 0;

    /// <summary>How wide some words are drawn on one line in a control's own face and size.</summary>
    private static double TextWidth(string? text, Avalonia.Controls.Primitives.TemplatedControl face)
        => TextWidth(text, face.FontFamily, face.FontStyle, face.FontWeight, face.FontSize);

    private static double TextWidth(string? text, TextBlock face)
        => TextWidth(text, face.FontFamily, face.FontStyle, face.FontWeight, face.FontSize);

    private static double TextWidth(
        string? text,
        Avalonia.Media.FontFamily family,
        Avalonia.Media.FontStyle style,
        Avalonia.Media.FontWeight weight,
        double size)
        => string.IsNullOrEmpty(text)
            ? 0
            : new Avalonia.Media.FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                Avalonia.Media.FlowDirection.LeftToRight,
                new Avalonia.Media.Typeface(family, style, weight),
                size,
                Avalonia.Media.Brushes.Black).Width;

    /// <summary>
    /// Kicks off the startup reconnect without waiting for it (HM-DEC-052).
    /// </summary>
    /// <remarks>
    /// Deliberately not awaited. Opening a COM port and waiting for a radio to
    /// answer takes as long as it takes, and a window that will not paint until
    /// the radio replies is a window that looks broken to anybody whose rig is
    /// switched off. So the window comes up first and the status line fills in
    /// afterwards.
    /// </remarks>
    private void StartReconnect()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            _ = vm.ReconnectOnStartupAsync();
        }
    }

    private void OnWindowPropertyChanged(
        object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty || e.Property == IsVisibleProperty)
        {
            PushVisibility();
        }
    }

    private void PushVisibility()
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SetWindowVisible(IsVisible && WindowState != WindowState.Minimized);
        }
    }

    // ---------------------------------------------------------------------
    // THE MENU UNDER THE MOUSE. Step 5's criterion 2 (ruled 2026-09-06).
    // ---------------------------------------------------------------------

    /// <summary>What the last right-click on a decoded row put under the mouse.</summary>
    /// <remarks>
    /// **RECORDED FOR A TEST AND READ BY NOTHING IN THE APPLICATION**, the shape
    /// <c>MainWindowViewModel.UseArmedSendForTests</c> already uses. A headless
    /// test can raise a real context request on a real row and read back exactly
    /// what appeared; without it the only evidence would be a popup's presence in
    /// a visual tree, which says nothing about what was in it. Null where the row
    /// named no station.
    /// </remarks>
    internal MenuFlyout? SendFlyoutUnderTheMouse { get; private set; }

    /// <summary>
    /// **A right-click on a decoded row, answered with the menu for that row at
    /// that moment.**
    /// </summary>
    /// <param name="sender">The row's own control, whose DataContext is the row.</param>
    /// <param name="e">The context request.</param>
    /// <remarks>
    /// <para>**THE MENU IS BUILT HERE, AT THE CLICK, AND IT IS NEVER KEPT.**
    /// <c>MainWindowViewModel.SendMenuFor</c> reads the contact ledger when it is
    /// called, so a menu built now carries the repeat counts as of now. **This was
    /// watched failing the other way**: a flyout built once when the row arrived,
    /// hung on the control and reopened, shows the counts the row was born with -
    /// so an operator who has already sent `RRR` twice is offered it as a first
    /// send. That is a menu lying about what has already gone out, and it is why
    /// nothing here assigns <c>ContextFlyout</c>.</para>
    /// <para>**IT IS THE VIEW'S JOB BECAUSE A POINTER IS A VIEW FACT** (§0.2), the
    /// same reason <see cref="OnTuneKey"/> lives here. The ViewModel is asked one
    /// question and answers it; the code-behind knows which row the mouse was
    /// over.</para>
    /// <para>**NO CONFIRMATION AND NO SECOND CLICK** (ruled 2026-09-06). Choosing
    /// an item calls <c>SendMessageCommand</c> with that message's text and does
    /// nothing else - it is the one entry point that arms, and this adds no second
    /// route to it.</para>
    /// <para>**A ROW THAT NAMES NO STATION SHOWS NOTHING**, rather than an empty
    /// box: free text and telemetry are not booked by the ledger at all, so there
    /// is no station to send anything to.</para>
    /// </remarks>
    private void OnDecodedRowContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        SendFlyoutUnderTheMouse = null;

        if (DataContext is not MainWindowViewModel vm
            || sender is not Control control
            || control.DataContext is not DigitalDecodeRow row)
        {
            return;
        }

        // **THE RIGHT-CLICK ITSELF MAKES THE CARD** (R29, Tim 2026-09-14: *"I right click
        // on a message in the Everything list and it creates a card"*), and it does so on
        // any PSK31 row, CQ or not, live or ended. It runs before the menu is built because
        // the card is the thing he asked for; the menu is what it offers afterwards.
        //
        // **IT SENDS NOTHING.** The command opens a card and returns; every transmission on
        // this panel still goes through one click on a named button.
        vm.OpenPsk31CardCommand.Execute(row);

        var flyout = SendFlyoutFor(vm, row);

        // **HANDLED EITHER WAY.** A row with no station has no menu, and letting
        // the request travel on would hand it to whatever is above the table.
        e.Handled = true;

        if (flyout is null)
        {
            return;
        }

        SendFlyoutUnderTheMouse = flyout;
        flyout.ShowAt(control, showAtPointer: true);
    }

    /// <summary>Builds one row's menu, exactly as the ViewModel gives it.</summary>
    /// <param name="vm">The panel, which owns the ledger and the operator's own settings.</param>
    /// <param name="row">The row the mouse was over.</param>
    /// <returns>The flyout, or null where the row names no station.</returns>
    /// <remarks>
    /// <para>**NOTHING IS GREYED, HIDDEN, SORTED AWAY OR DISABLED** (ruled
    /// 2026-09-06). Every option in <c>Ft8SendMenu.Options</c> becomes a clickable
    /// item, in the order they arrive, and there is no <c>IsEnabled</c> on any of
    /// them - not for the contact state, not for the licence, not for a repeat.
    /// A complete contact still offers `73` and everything else.</para>
    /// <para>**AN ABSENT MESSAGE IS A MESSAGE THAT DOES NOT EXIST, NOT A MESSAGE
    /// WITHHELD** (§0.0). With no grid in Settings there is no grid message to
    /// send, and the reason is said as a note. A note is not an option: it carries
    /// no command and cannot be hit, which is a different thing from an option
    /// drawn grey.</para>
    /// <para>**THE LICENCE APPEARS AS A NOTE AND TAKES NOTHING AWAY** (criterion
    /// 6's first half). The refusal that actually stops a transmission is inside
    /// <c>Ft8TransmitSequence.RunAsync</c> and stays there; this is a second
    /// reader of the rule, not a second copy of it.</para>
    /// </remarks>
    internal static MenuFlyout? SendFlyoutFor(MainWindowViewModel vm, DigitalDecodeRow? row)
    {
        // **A PSK31 OR OLIVIA ROW THAT NAMES A STATION OFFERS R39's SEVEN LINES** (Tim,
        // 2026-09-21, ruled C; work instruction 378 section 6 ruling 1 item 8). Until unit 378
        // this was a ONE-ITEM menu on a certain CQ and nothing at all on every other row: five
        // of the seven rows that named a station on unit 378's own trace had no menu.
        //
        // **IT IS NOT THE FT8 MENU**, which is why `SendMenuFor` answers null for these rows and
        // still does: the FT8 options are message shapes packed into 77 bits and none of them is
        // a thing to send on PSK31.
        //
        // **THE VIEW MODEL DECIDES AND THIS DRAWS.** What each line reads, which command it
        // carries and which of them are absent with a reason is `Psk31CannedMenuFor`'s answer, so
        // the menu a test reads and the menu Tim sees are one list.
        //
        // **NOTHING IS GREYED, HIDDEN, SORTED AWAY OR DISABLED** (ruled 2026-09-06). An entry
        // with no command is a note - the same `Note` an absent FT8 message already gets - and a
        // note cannot be hit.
        if (vm?.Psk31CannedMenuFor(row) is { Count: > 0 } canned)
        {
            var offered = new MenuFlyout();

            foreach (var entry in canned)
            {
                offered.Items.Add(entry.IsNote
                    ? Note(entry.Label)
                    : new MenuItem
                    {
                        Header = entry.Label,
                        Command = entry.Command,
                        CommandParameter = entry.Parameter,
                    });
            }

            return offered;
        }

        var menu = vm?.SendMenuFor(row);

        if (menu is null)
        {
            return null;
        }

        var flyout = new MenuFlyout();

        foreach (var option in menu.Options)
        {
            flyout.Items.Add(new MenuItem
            {
                Header = HeaderFor(option),

                // THE ONE ENTRY POINT THAT ARMS, AND THE ONLY THING THIS DOES.
                Command = vm!.SendMessageCommand,
                CommandParameter = option.Text,
            });
        }

        foreach (var reason in menu.Absent)
        {
            flyout.Items.Add(Note(reason));
        }

        if (vm!.HasDigitalSendLicenceLine)
        {
            flyout.Items.Add(Note(vm.DigitalSendLicenceLine));
        }

        // **`Log` JOINS THE MENU STEP B BUILT RATHER THAN GETTING ONE OF ITS OWN**
        // (work instruction 274 task 3). A second right-click menu on the same row
        // is two menus that can disagree about what a row offers.
        //
        // **ONLY ON A ROW ADDRESSED TO HIM.** `CanLogRow` asks
        // `Ft8MessageSplit.IsAddressedTo`, the same question the mine side and the
        // contact column ask; a Log item on a CQ would offer to write down a
        // contact that has not happened.
        //
        // **IT IS NOT A SEND OPTION AND CARRIES A DIFFERENT COMMAND.** Everything
        // above hands `option.Text` to `SendMessageCommand`, which arms a
        // transmission. This opens a dialog and transmits nothing, so it is
        // separated by a rule the eye can see as well as by its wording.
        if (vm.CanLogRow(row))
        {
            flyout.Items.Add(new Separator());

            flyout.Items.Add(new MenuItem
            {
                Header = "Log this contact...",
                Command = vm.LogContactCommand,
                CommandParameter = row,
            });
        }

        return flyout;
    }

    /// <summary>What one option reads as, without anybody having to hover.</summary>
    /// <remarks>
    /// <para>**THE MARK IS WORDS AND NOT ONLY WEIGHT** (§0.6). The expected one is
    /// bold *and* says so, so the distinction survives being printed in grey or
    /// read by somebody who cannot tell bold from regular.</para>
    /// <para>**A REPEAT CARRIES ITS COUNT IN WORDS** - *acknowledge, 2nd time* -
    /// which is the wording `docs/unit259-send-path-trace.md` predicted. FT8 loses
    /// transmissions constantly, so a repeat is correct operating and the count is
    /// there to be read, never to withhold anything.</para>
    /// </remarks>
    private static string HeaderFor(Ft8SendOption option)
    {
        var said = option.Text + "   " + option.Label;

        if (option.SentBefore > 0)
        {
            said += ", " + Ordinal(option.SentBefore + 1) + " time";
        }

        return option.IsExpected ? said + " - the one that comes next" : said;
    }

    /// <summary>2nd, 3rd, 4th - how many times this message will have gone.</summary>
    private static string Ordinal(int n)
    {
        var suffix = (n % 100) is >= 11 and <= 13
            ? "th"
            : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };

        return n.ToString(CultureInfo.InvariantCulture) + suffix;
    }

    /// <summary>A line in the menu that says something and cannot be clicked.</summary>
    /// <remarks>
    /// **NOT AN OPTION AND NOT A GREYED ONE.** It carries no command and is not
    /// hit-testable, so there is nothing to click and nothing that looks like a
    /// message the app has taken away.
    /// </remarks>
    private static MenuItem Note(string text)
        => new() { Header = text, IsHitTestVisible = false };

    private void OnTuneKey(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        // **ESCAPE STOPS THE TRANSMITTER, AND IT IS HANDLED HERE BECAUSE A KEY
        // IS A VIEW FACT** (§0.2, phase 4). It is the one keystroke that has to
        // work whatever has focus and whatever is scrolled where, so it is taken
        // before anything else in this handler and it awaits nothing. Stopping a
        // cycle that is not running costs a call to a method that returns.
        if (e.Key == Key.Escape)
        {
            vm.AutoCall.StopNow();
            vm.Scan.StopNow();
            e.Handled = true;
            return;
        }

        var delta = e.Key switch
        {
            Key.Right or Key.Up => 10,
            Key.Left or Key.Down => -10,
            _ => 0,
        };

        if (delta != 0)
        {
            vm.FrequencyHz += delta;
            e.Handled = true;
        }
    }

    /// <summary>Show the notice, on his thread, without taking his focus.</summary>
    /// <param name="sender">The panel.</param>
    /// <param name="award">What was earned.</param>
    private void OnBadgeEarned(object? sender, ViewModels.BadgeAward award)
        => Avalonia.Threading.Dispatcher.UIThread.Post(
            () => BadgeWindow.Announce(this, award));
}

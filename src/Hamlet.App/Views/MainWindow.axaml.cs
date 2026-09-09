using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
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
    }


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

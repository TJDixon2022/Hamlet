using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// Send sits beside Receive, composes a line, and has a button that keys the
/// radio.
/// </summary>
/// <remarks>
/// <para>**THE BUTTON CAME BACK** (Tim's ruling of 2026-08-27: *"I've sent with
/// it hundreds of times. It worked great."*). Work instructions 026, 027 and 028
/// each forbade wiring Send to the transmitter, citing §0.2 and HM-DEC-098 as
/// though the interlock work were still ahead of the project. **HM-DEC-098
/// governs the automated cycle**, and **HM-DEC-059 authorises the operator
/// keying by hand** and always did.</para>
/// <para>This file used to assert that nothing left the radio. That assertion
/// was true of what unit 1.11.24 built and wrong about what the application had
/// been doing for dozens of builds.</para>
/// <para>**NOTHING HERE TRANSMITS.** The tests assert what the panel offers and
/// what the interlocks refuse; no test presses a button that could key a rig,
/// and no rig is connected. Tim verifies at the radio.</para>
/// <para>Nothing hit-tests, per unit 1.11.13's rule.</para>
/// </remarks>
public sealed class TheSendPanelComposesAndDoesNotKeyTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the geometry is printed.</param>
    public TheSendPanelComposesAndDoesNotKeyTests(ITestOutputHelper output)
        => _output = output;

    private static Rect OnScreen(Visual visual, Visual root)
    {
        var x = 0.0;
        var y = 0.0;

        for (var at = visual; at is not null && at != root; at = at.GetVisualParent())
        {
            x += at.Bounds.X;
            y += at.Bounds.Y;
        }

        return new Rect(x, y, visual.Bounds.Width, visual.Bounds.Height);
    }

    private static void With(Action<MainWindow, MainWindowViewModel> check)
    {
        var model = new MainWindowViewModel(new AppSettings(), null);

        var window = new MainWindow
        {
            DataContext = model,
            Width = 1400,
            Height = 900,
        };

        window.Show();

        Settle(window);

        check(window, model);

        window.Close();
    }

    private static void Settle(MainWindow window)
    {
        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    /// <summary>Press a tab the way the operator does.</summary>
    /// <param name="window">The window.</param>
    /// <param name="name">Which tab.</param>
    /// <remarks>
    /// **THROUGH THE CONTROL, NEVER THE PROPERTY** (Tim's ruling of
    /// 2026-08-27). Setting `OperatingMode` on the view model is how unit
    /// 1.11.25 passed over a blank screen: the fault lived in the tab strip's
    /// binding and a test that never pressed a tab could not reach it.
    /// </remarks>
    private static void Press(MainWindow window, string name)
    {
        window.GetVisualDescendants()
            .OfType<RadioButton>()
            .First(b => b.Classes.Contains("hm-tab")
                && (string?)b.Content == name)
            .IsChecked = true;

        Settle(window);
    }

    /// <summary>Click a button by its face, the way the operator does.</summary>
    /// <param name="window">The window.</param>
    /// <param name="face">What the button says.</param>
    private static void Click(MainWindow window, string face)
    {
        // **SEARCHED INSIDE THE SEND PANEL AND NOT THE WHOLE WINDOW.** `Clear`
        // is not a unique word on this screen, and a helper that takes whichever
        // button visual order reaches first is a test that works by luck.
        //
        // `as string` and not a cast, because plenty of buttons here hold a whole
        // layout as their content and a cast throws on the first one walked past.
        var panel = Send(window);

        Assert.NotNull(panel);

        var button = panel!.GetVisualDescendants()
            .OfType<Button>()
            .First(b => b.Content as string == face);

        Assert.True(
            button.IsEffectivelyVisible,
            $"the {face} button is not on the screen to be pressed");

        Assert.True(
            button.IsEnabled, $"the {face} button is not enabled");

        button.Command?.Execute(button.CommandParameter);

        Settle(window);
    }

    private static Border? Send(MainWindow window)
        => window.GetVisualDescendants()
            .OfType<Border>()
            .FirstOrDefault(b => b.Name == "SendPanel");

    /// <remarks>
    /// <para>Proves the CW workspace's own order: **Send on the left, Receive on
    /// the right** (Tim's ruling of 2026-08-27). Unit 1.11.23 had Send outside
    /// the workspace entirely, to the right of everything, which is why it read
    /// as detached from the tab it belongs to.</para>
    /// <para>**HE NAMED THE ORDER AND NOT THE WIDTHS**, and Receive is the panel
    /// he reads, so Receive is still the wider — Send is a fixed column and
    /// Receive takes what is left.</para>
    /// </remarks>
    [AvaloniaFact]
    public void SendIsOnTheLeftAndReceiveOnTheRightAndWider()
    {
        With((window, _) =>
        {
            var send = Send(window);
            // **RECEIVE IS A PANEL NOW, NOT THE CANVAS** (Tim, 2026-08-27).
            // Unit 1.11.23 measured the canvas as the Receive side, and the
            // canvas still carried the whole previous arrangement, so Receive
            // got whatever was left over.
            var receive = window.GetVisualDescendants()
                .OfType<Border>()
                .FirstOrDefault(c => c.Name == "ReceivePanel");

            Assert.NotNull(send);
            Assert.NotNull(receive);

            var sendAt = OnScreen(send!, window);
            var receiveAt = OnScreen(receive!, window);

            _output.WriteLine(
                $"receive x={receiveAt.X:0} w={receiveAt.Width:0}, "
                + $"send x={sendAt.X:0} w={sendAt.Width:0}");

            Assert.True(
                sendAt.Right <= receiveAt.X + 0.5,
                $"send reaches {sendAt.Right:0} and receive starts at "
                + $"{receiveAt.X:0}, so one is drawn over the other");

            Assert.True(
                sendAt.Width < receiveAt.Width,
                $"send is {sendAt.Width:0} wide and receive {receiveAt.Width:0}, "
                + "but receive is meant to be the wider");
        });
    }

    /// <remarks>
    /// <para>Proves Send belongs to the CW tab: on screen in CW and off it in
    /// Digital.</para>
    /// <para>**ASKED OF EFFECTIVE VISIBILITY AND NOT THE LOCAL PROPERTY.** The
    /// panel's own `IsVisible` stays true when the workspace containing it is
    /// hidden, so a test reading that would have reported Send as showing on
    /// every tab. What the operator sees is whether the whole chain is visible.</para>
    /// </remarks>
    [AvaloniaFact]
    public void SendBelongsToTheCwTab()
    {
        With((window, model) =>
        {
            Assert.Equal("CW", model.OperatingMode);
            Assert.True(
                Send(window)?.IsEffectivelyVisible,
                "send is missing on the CW tab");

            Press(window, "Digital");

            _output.WriteLine(
                $"on Digital, send on screen: "
                + $"{Send(window)?.IsEffectivelyVisible}");

            Assert.False(
                Send(window)?.IsEffectivelyVisible,
                "send is showing on the Digital tab");

            Press(window, "CW");

            Assert.True(
                Send(window)?.IsEffectivelyVisible,
                "send did not come back on CW");
        });
    }

    /// <remarks>
    /// Proves the buttons compose. Each fills the line with what would go out,
    /// so the operator can read it before anything ever does.
    /// </remarks>
    [AvaloniaFact]
    public void TheButtonsComposeWhatWouldGoOut()
    {
        With((window, model) =>
        {
            // **AND WHAT IS ASSERTED IS WHAT THE BOX SHOWS**, not what the view
            // model holds: the operator reads the line, and a property that is
            // right behind a box that is not bound to it is no use to him.
            TextBox Line() => window.GetVisualDescendants()
                .OfType<TextBox>()
                .First(t => t.Watermark == "what you would send");

            foreach (var (face, wanted) in new[]
            {
                ("CQ", "CQ"), ("RST", "599"), ("73", "73"),
            })
            {
                Click(window, face);

                _output.WriteLine($"{face,-5} -> {Line().Text}");

                Assert.Contains(
                    wanted, Line().Text ?? "", StringComparison.Ordinal);
            }

            Click(window, "Clear");

            _output.WriteLine($"Clear -> \"{Line().Text}\"");

            Assert.True(string.IsNullOrEmpty(Line().Text));

            // **AND THE LINE THE MACROS FILL IS THE ONE THE BUTTON SENDS.**
            // The box, the macros and the send button's parameter are one
            // message; if they were three the operator could read one thing and
            // transmit another (§0.0).
            Click(window, "CQ");

            Assert.Equal(model.Transmit.OwnWords.Message, Line().Text);
        });
    }

    /// <remarks>
    /// <para>Proves the panel explains each control in plain terms, and that it
    /// no longer claims nothing leaves the radio — because something does.</para>
    /// <para>Asserted on the meanings rather than on the exact copy, so the
    /// wording can be improved without the test having an opinion about
    /// prose.</para>
    /// </remarks>
    [AvaloniaFact]
    public void ThePanelSaysWhatEachControlDoes()
    {
        With((window, _) =>
        {
            var said = string.Join(" ", window.GetVisualDescendants()
                .OfType<TextBlock>()
                .Where(t => t.IsEffectivelyVisible)
                .Select(t => t.Text)
                .Where(t => t is not null));

            _output.WriteLine(said.Length > 400 ? said[..400] + "…" : said);

            Assert.DoesNotContain(
                "nothing leaves the radio", said, StringComparison.Ordinal);

            foreach (var meaning in new[]
            {
                "callsign",       // what CQ puts on the band
                "signal report",  // what RST is
                "best wishes",    // what 73 means
                "sends nothing",  // what Clear does
                "on the air",     // what Send does
            })
            {
                Assert.Contains(meaning, said, StringComparison.Ordinal);
            }
        });
    }

    /// <remarks>
    /// <para>Proves the send button is there, is wired to the transmit path, and
    /// carries the operator's own line as what it would send.</para>
    /// <para>**IT IS NEVER PRESSED.** Its command is inspected and its parameter
    /// checked; pressing it is what the operator does at the rig (§0.2).</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheSendButtonIsWiredToTheTransmitPath()
    {
        With((window, model) =>
        {
            var send = Send(window)!
                .GetVisualDescendants()
                .OfType<Button>()
                .First(b => b.Name == "TransmitButton");

            _output.WriteLine(
                $"command={send.Command?.GetType().Name}, "
                + $"parameter={send.CommandParameter?.GetType().Name}, "
                + $"on screen={send.IsEffectivelyVisible}");

            Assert.True(send.IsEffectivelyVisible);
            Assert.NotNull(send.Command);

            // The parameter is the operator's own line, and it is the same object
            // the text box edits — one message, not a copy that drifts.
            Assert.Same(model.Transmit.OwnWords, send.CommandParameter);
        });
    }

    /// <remarks>
    /// <para>**PROVES THE INTERLOCKS REACH THE CONTROL**, which is the half of
    /// the guard the engine tests cannot see. `EveryInterlockStillRefusesTests`
    /// proves the readiness check refuses; this proves a refusal arrives at the
    /// button the operator's hand is on.</para>
    /// <para>No radio is connected on this machine, so the first interlock —
    /// nothing to send with — is the live one, and it is the honest case to
    /// assert here rather than a contrived one.</para>
    /// <para>**THE BUTTON IS ASKED, NEVER PRESSED.** `CanExecute` is what decides
    /// whether a press does anything, so asking it is asking the interlock.
    /// Pressing is what Tim does at the rig (§0.2).</para>
    /// </remarks>
    [AvaloniaFact]
    public void ARefusedSendReachesTheButtonAndSaysWhy()
    {
        With((window, model) =>
        {
            // Arranging state that arrives from the radio, which has no control
            // behind it: the operator cannot press a radio into being absent.
            model.Transmit.Refresh();

            Settle(window);

            var send = Send(window)!
                .GetVisualDescendants()
                .OfType<Button>()
                .First(b => b.Name == "TransmitButton");

            _output.WriteLine(
                $"canSend={model.Transmit.CanSend}, "
                + $"isRefusal={model.Transmit.IsRefusal}, "
                + $"enabled={send.IsEnabled}, "
                + $"effectively={send.IsEffectivelyEnabled}");
            _output.WriteLine($"says: {model.Transmit.Status}");

            Assert.False(
                model.Transmit.CanSend,
                "nothing is connected and the panel thinks it may send");

            Assert.False(
                send.Command?.CanExecute(send.CommandParameter) ?? false,
                "the send button would act with no radio behind it");

            // **`IsEffectivelyEnabled` AND NOT `IsEnabled`**, which is the same
            // trap unit 1.11.25 recorded for visibility and it caught this test
            // on its first run. `IsEnabled` is the local value nobody has set,
            // so it reads true forever; what a command's `CanExecute` drives is
            // the effective one, and the effective one is what the operator's
            // hand meets.
            Assert.False(
                send.IsEffectivelyEnabled,
                "the send button is live while the interlock refuses");

            // **AND THE REFUSAL PRINTS ITS REASON** (HM-DEC-080). A grey button
            // with nothing beside it is the fault Tim reported three times.
            Assert.True(
                model.Transmit.IsRefusal,
                "the send is refused and the panel does not call it a refusal");

            var said = string.Join(" ", Send(window)!
                .GetVisualDescendants()
                .OfType<TextBlock>()
                .Where(t => t.IsEffectivelyVisible)
                .Select(t => t.Text)
                .Where(t => t is not null));

            Assert.Contains(
                model.Transmit.Status, said, StringComparison.Ordinal);
        });
    }
}

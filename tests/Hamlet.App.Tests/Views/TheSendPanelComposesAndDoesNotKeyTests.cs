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
/// Send sits to the right of Receive, composes what would go out, and reaches
/// no transmitter.
/// </summary>
/// <remarks>
/// <para>**NOTHING HERE KEYS ANYTHING** and that is the assertion, not a
/// caveat. §0.2 stands and HM-DEC-098 stands: an automatic transmit path is a
/// separate ruling taken after every interlock has been watched to fire into a
/// dummy load. A panel that looked live and was not would be worse than no
/// panel, so it says on its face what it does.</para>
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
        With((window, _) =>
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
        });
    }

    /// <remarks>
    /// <para>Proves the panel says what it is. A button that looks live and is
    /// not is worse than no button (§0.5.1), and the one thing this panel must
    /// never imply is that pressing something puts a signal on the air.</para>
    /// </remarks>
    [AvaloniaFact]
    public void ThePanelSaysNothingLeavesTheRadio()
    {
        With((window, _) =>
        {
            var said = window.GetVisualDescendants()
                .OfType<TextBlock>()
                .Select(t => t.Text)
                .FirstOrDefault(t => t is not null
                    && t.Contains("nothing leaves the radio", StringComparison.Ordinal));

            _output.WriteLine(said ?? "(the panel does not say what it does)");

            Assert.NotNull(said);
        });
    }
}

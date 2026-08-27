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

            model.OperatingMode = "Digital";

            Settle(window);

            _output.WriteLine(
                $"on Digital, send on screen: "
                + $"{Send(window)?.IsEffectivelyVisible}");

            Assert.False(
                Send(window)?.IsEffectivelyVisible,
                "send is showing on the Digital tab");

            model.OperatingMode = "CW";

            Settle(window);

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
        With((_, model) =>
        {
            model.ComposeCqCommand.Execute(null);
            _output.WriteLine($"CQ  -> {model.SendText}");
            Assert.Contains("CQ", model.SendText, StringComparison.Ordinal);

            model.ComposeRstCommand.Execute(null);
            _output.WriteLine($"RST -> {model.SendText}");
            Assert.Contains("599", model.SendText, StringComparison.Ordinal);

            model.ComposeSeventyThreeCommand.Execute(null);
            _output.WriteLine($"73  -> {model.SendText}");
            Assert.Contains("73", model.SendText, StringComparison.Ordinal);

            model.ComposeClearCommand.Execute(null);
            _output.WriteLine($"Clear -> \"{model.SendText}\"");
            Assert.Equal("", model.SendText);
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

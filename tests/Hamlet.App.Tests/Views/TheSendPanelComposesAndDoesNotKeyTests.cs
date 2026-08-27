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
        var layouts = Hamlet.App.Layout.LayoutStore.Path;

        Hamlet.App.Layout.LayoutStore.Path =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        try
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
        finally
        {
            Hamlet.App.Layout.LayoutStore.Path = layouts;
        }
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
    /// Proves Send is to the right of the area Receive occupies, and narrower —
    /// the ruling makes Receive the wider of the two.
    /// </remarks>
    [AvaloniaFact]
    public void SendSitsToTheRightOfReceiveAndIsNarrower()
    {
        With((window, _) =>
        {
            var send = Send(window);
            var receive = window.GetVisualDescendants()
                .OfType<ScrollViewer>()
                .FirstOrDefault(c => c.Name == "CanvasView");

            Assert.NotNull(send);
            Assert.NotNull(receive);

            var sendAt = OnScreen(send!, window);
            var receiveAt = OnScreen(receive!, window);

            _output.WriteLine(
                $"receive x={receiveAt.X:0} w={receiveAt.Width:0}, "
                + $"send x={sendAt.X:0} w={sendAt.Width:0}");

            Assert.True(
                receiveAt.Right <= sendAt.X + 0.5,
                $"receive reaches {receiveAt.Right:0} and send starts at "
                + $"{sendAt.X:0}, so one is drawn over the other");

            Assert.True(
                sendAt.Width < receiveAt.Width,
                $"send is {sendAt.Width:0} wide and receive {receiveAt.Width:0}, "
                + "but receive is meant to be the wider");
        });
    }

    /// <remarks>
    /// Proves Send belongs to the CW tab: it is there in CW and gone in Digital.
    /// </remarks>
    [AvaloniaFact]
    public void SendBelongsToTheCwTab()
    {
        With((window, model) =>
        {
            Assert.Equal("CW", model.OperatingMode);
            Assert.True(Send(window)?.IsVisible, "send is missing on the CW tab");

            model.OperatingMode = "Digital";

            Settle(window);

            _output.WriteLine(
                $"on Digital, send visible: {Send(window)?.IsVisible}");

            Assert.False(
                Send(window)?.IsVisible, "send is showing on the Digital tab");

            model.OperatingMode = "CW";

            Settle(window);

            Assert.True(Send(window)?.IsVisible, "send did not come back on CW");
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

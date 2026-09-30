using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Hamlet.App.Controls;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **The CW terminal scrolls, and says so** (work instruction numbered 509, run as unit 510, task 1,
/// HM-DEC-214).
/// </summary>
/// <remarks>
/// The real window on the CW tab; forty lines of text into its transcript, written here, nothing
/// read from disk (R96).
/// </remarks>
public sealed class TheTerminalScrollsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the offsets are printed.</param>
    public TheTerminalScrollsTests(ITestOutputHelper output) => _output = output;

    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }

    /// <summary>One line's worth of letters, as the settled pass hands them over.</summary>
    private static void Line(CwTranscript transcript, int n)
    {
        foreach (var c in $"LINE {n:00} CQ CQ DE N0CALL N0CALL K QRZ DE W1AW PSE K ")
        {
            transcript.Append(new CwCharacter(
                c == ' ' ? MorseAlphabet.WordGap : c.ToString(), CwConfidence.High, 1, "", double.NaN, 20, TimeSpan.Zero));
        }
    }

    private static string At(ScrollViewer s)
        => $"offset {s.Offset.Y:0}, viewport {s.Viewport.Height:0}, extent {s.Extent.Height:0}";

    private static bool AtTheEnd(ScrollViewer s) => s.Offset.Y + s.Viewport.Height >= s.Extent.Height - 2;

    /// <remarks>
    /// Task 1: past the box, the bar is shown and stays shown, it says what it does on hover, and the
    /// newest line is in view; scrolled up it holds while ten more lines arrive; back at the bottom it
    /// follows again.
    /// </remarks>
    [AvaloniaFact]
    public void TheTerminalHasABarThatFollowsUnlessHeScrollsUp()
    {
        var panel = new MainWindowViewModel(new AppSettings(), null) { OperatingMode = "CW", TerminalExpanded = true };
        var window = new MainWindow { DataContext = panel, Width = 1400, Height = 1400 };

        window.Show();
        Pump(window);

        var terminal = window.GetVisualDescendants().OfType<CwTerminalControl>().Single();
        var scroller = terminal.FindAncestorOfType<ScrollViewer>()!;

        for (var n = 1; n <= 40; n++)
        {
            Line(panel.Transcript, n);
        }

        terminal.Draw();
        Pump(window);

        var bar = scroller.GetVisualDescendants().OfType<ScrollBar>().FirstOrDefault(b => b.Orientation == Avalonia.Layout.Orientation.Vertical);
        _output.WriteLine($"forty lines: {At(scroller)}; auto-hide {scroller.AllowAutoHide}; bar {(bar is null ? "none" : bar.IsVisible ? "visible" : "hidden")}; tip `{(bar is null ? null : ToolTip.GetTip(bar))}`");

        Assert.True(scroller.Extent.Height > scroller.Viewport.Height, "forty lines do not overflow the box");
        Assert.False(scroller.AllowAutoHide, "the bar hides itself until the pointer finds it");
        Assert.NotNull(bar);
        Assert.True(bar!.IsVisible, "no vertical bar is shown");
        Assert.Equal(CwTerminalScroll.BarTip, ToolTip.GetTip(bar) as string);
        Assert.True(AtTheEnd(scroller), "the newest line is not in view: " + At(scroller));

        scroller.Offset = scroller.Offset.WithY(0);
        Pump(window);

        for (var n = 41; n <= 50; n++)
        {
            Line(panel.Transcript, n);
        }

        terminal.Draw();
        Pump(window);
        _output.WriteLine($"scrolled up, ten more: {At(scroller)}");

        Assert.True(scroller.Offset.Y < 1, "the view moved while he was reading: " + At(scroller));

        scroller.ScrollToEnd();
        Pump(window);

        for (var n = 51; n <= 60; n++)
        {
            Line(panel.Transcript, n);
        }

        terminal.Draw();
        Pump(window);
        _output.WriteLine($"back at the bottom, ten more: {At(scroller)}");

        Assert.True(AtTheEnd(scroller), "the view stopped following at the bottom: " + At(scroller));

        window.Close();
    }
}

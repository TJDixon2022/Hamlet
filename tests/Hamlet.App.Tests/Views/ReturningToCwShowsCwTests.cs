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
/// Pressing the tabs and coming back to CW shows CW.
/// </summary>
/// <remarks>
/// <para>**THE OPERATOR PHOTOGRAPHED A BLANK WORKSPACE** on 2026-08-27: one
/// click on Digital and back to CW left nothing on the screen — no Send, no
/// Receive, and it never recovered.</para>
/// <para>**UNIT 1.11.25 ASSERTED THIS AREA AND PASSED OVER IT**, for two reasons
/// that are both worth keeping written down. It set `OperatingMode` on the view
/// model directly, so the tab buttons were never pressed and the fault — which
/// lives entirely in the strip's binding — could not be reached. And it asserted
/// that the workspace was the same object on return, which was true throughout:
/// the container survived and stopped being shown.</para>
/// <para>**SO THIS PRESSES THE BUTTONS AND ASKS WHAT IS ON SCREEN.** Effective
/// visibility and non-zero render bounds, twice round, because a fault that only
/// appears on the second circuit is the kind this exists to catch.</para>
/// </remarks>
public sealed class ReturningToCwShowsCwTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the round trip is printed.</param>
    public ReturningToCwShowsCwTests(ITestOutputHelper output)
        => _output = output;

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

    private static List<RadioButton> Tabs(MainWindow window)
        => window.GetVisualDescendants()
            .OfType<RadioButton>()
            .Where(b => b.Classes.Contains("hm-tab"))
            .ToList();

    private static Border? Panel(MainWindow window, string name)
        => window.GetVisualDescendants()
            .OfType<Border>()
            .FirstOrDefault(b => b.Name == name);

    private static Grid? Workspace(MainWindow window, string name)
        => window.GetVisualDescendants()
            .OfType<Grid>()
            .FirstOrDefault(g => g.Name == name);

    private static void Press(MainWindow window, string name)
    {
        Tabs(window).First(t => (string?)t.Content == name).IsChecked = true;

        Settle(window);
    }

    /// <remarks>
    /// <para>Proves the fault itself, twice round. **Asked of what the operator
    /// sees** — effective visibility and real render bounds — never a control's
    /// own `IsVisible`, which stays true inside a hidden container and is exactly
    /// how a blank screen passed a test.</para>
    /// </remarks>
    [AvaloniaFact]
    public void TheRoundTripComesBackToSendAndReceive()
    {
        With((window, model) =>
        {
            void OnScreen(string when)
            {
                var send = Panel(window, "SendPanel");
                var receive = Panel(window, "ReceivePanel");

                _output.WriteLine(
                    $"{when,-24} mode=\"{model.OperatingMode}\"  "
                    + $"send {send?.Bounds.Width:0}x{send?.Bounds.Height:0}  "
                    + $"receive {receive?.Bounds.Width:0}x{receive?.Bounds.Height:0}");

                Assert.True(
                    send?.IsEffectivelyVisible,
                    $"{when}: Send is not on the screen");

                Assert.True(
                    receive?.IsEffectivelyVisible,
                    $"{when}: Receive is not on the screen");

                Assert.True(
                    send!.Bounds.Width > 0 && send.Bounds.Height > 0,
                    $"{when}: Send is on the screen with no size");

                Assert.True(
                    receive!.Bounds.Width > 0 && receive.Bounds.Height > 0,
                    $"{when}: Receive is on the screen with no size");
            }

            OnScreen("fresh");

            for (var lap = 1; lap <= 2; lap++)
            {
                Press(window, "Digital");
                Press(window, "Voice");
                Press(window, "CW");

                OnScreen($"back on CW, lap {lap}");
            }
        });
    }

    /// <remarks>
    /// Proves the tab strip agrees with the workspace. A fresh window used to
    /// show all three tabs unchecked, because the binding that decided it never
    /// resolved.
    /// </remarks>
    [AvaloniaFact]
    public void ExactlyOneTabIsCheckedAndItIsTheOneShowing()
    {
        With((window, model) =>
        {
            foreach (var name in new[] { "CW", "Digital", "Voice", "CW" })
            {
                if (name != model.OperatingMode)
                {
                    Press(window, name);
                }

                var checkedTabs = Tabs(window)
                    .Where(t => t.IsChecked == true)
                    .Select(t => (string?)t.Content)
                    .ToList();

                _output.WriteLine(
                    $"mode \"{model.OperatingMode}\", checked: "
                    + string.Join(", ", checkedTabs));

                Assert.Single(checkedTabs);
                Assert.Equal(model.OperatingMode, checkedTabs[0]);
            }
        });
    }

    /// <remarks>
    /// Proves Digital and Voice stay empty on every visit — the CW workspace off
    /// the screen, and nothing of their own put in its place.
    /// </remarks>
    [AvaloniaFact]
    public void DigitalAndVoiceStayEmpty()
    {
        With((window, _) =>
        {
            foreach (var lap in new[] { 1, 2 })
            {
                foreach (var name in new[] { "Digital", "Voice" })
                {
                    Press(window, name);

                    var cw = Workspace(window, "CwWorkspace");
                    var mine = Workspace(window, name + "Workspace");

                    _output.WriteLine(
                        $"lap {lap} on {name}: CW on screen "
                        + $"{cw?.IsEffectivelyVisible}, {name} on screen "
                        + $"{mine?.IsEffectivelyVisible}");

                    Assert.False(
                        cw?.IsEffectivelyVisible,
                        $"the CW workspace is showing on {name}");

                    Assert.True(
                        mine?.IsEffectivelyVisible,
                        $"the {name} workspace is not showing on {name}");

                    // Empty means empty: nothing drawn inside it.
                    Assert.Empty(mine!.GetVisualDescendants().OfType<Control>());
                }

                Press(window, "CW");
            }
        });
    }
}

using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// **THE RECORD BUTTON IS ON THE SCREEN** (work instruction 527, HM-DEC-231). Tim, 2026-10-02: *"I can't record it
/// anymore. You took the record button away."* Unit 526 found it in the code; on the screen it sat below the CW
/// workspace's fixed height, pushed there by the rows added above it, and at 1100 by 800 below the window.
/// </summary>
/// <remarks>Judged on the real window, built headless, at the two sizes the order names.</remarks>
public sealed class TheRecordButtonIsOnTheScreenTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where each size's measurement is printed.</param>
    public TheRecordButtonIsOnTheScreenTests(ITestOutputHelper output) => _output = output;

    private static (Window Window, MainWindowViewModel Panel) Open(int width, int height)
    {
        var panel = new MainWindowViewModel(TheControlsTimCanPress.Fixture(), null) { OperatingMode = "CW" };
        var window = new MainWindow { DataContext = panel, Width = width, Height = height };

        window.Show();
        TheControlsTimCanPress.Settle(window);

        return (window, panel);
    }

    private static Button Record(Window window)
        => window.GetVisualDescendants().OfType<Button>().Single(b => b.Content as string == "Record");

    /// <remarks>
    /// The button named Record is effectively visible, and its whole rectangle lies inside the window and inside the
    /// CW workspace, at 1600 by 900 and at 1100 by 800.
    /// </remarks>
    /// <param name="width">The window's width.</param>
    /// <param name="height">The window's height.</param>
    [AvaloniaTheory]
    [InlineData(1600, 900)]
    [InlineData(1100, 800)]
    public void TheRecordButtonIsInsideTheWindow(int width, int height)
    {
        var (window, _) = Open(width, height);

        try
        {
            var button = Record(window);
            var at = button.TranslatePoint(default, window)!.Value;
            var workspace = TheTopRowTests.Named<Control>(window, "WorkspaceBoundary");
            var space = workspace.TranslatePoint(default, window)!.Value;

            _output.WriteLine($"{width} by {height}: Record at {at}, {button.Bounds.Size}; visible {button.IsEffectivelyVisible}, enabled {button.IsEnabled}; workspace {space}, {workspace.Bounds.Size}");

            Assert.True(button.IsEffectivelyVisible, "Record is not visible");
            Assert.True(at.X >= 0 && at.Y >= 0 && at.X + button.Bounds.Width <= window.ClientSize.Width && at.Y + button.Bounds.Height <= window.ClientSize.Height, "Record lies outside the window");
            Assert.True(at.Y >= space.Y && at.Y + button.Bounds.Height <= space.Y + workspace.Bounds.Height, "Record lies outside the CW workspace");
        }
        finally
        {
            window.Close();
        }
    }

    /// <remarks>
    /// Greyed while not listening, and its hover says why; live while listening to the training radio's synthetic
    /// Morse, and a press writes cw-&lt;time&gt;.wav and cw-&lt;time&gt;.txt in the capture folder.
    /// </remarks>
    [AvaloniaFact]
    public async Task PressingRecordWhileListeningWritesTheTwoFiles()
    {
        var was = MainWindowViewModel.CaptureFolder;
        var folder = Path.Combine(Path.GetTempPath(), "hamlet-unit527-record-" + Guid.NewGuid().ToString("N")[..8]);
        var (window, panel) = Open(1600, 900);

        try
        {
            MainWindowViewModel.CaptureFolder = folder;

            var button = Record(window);

            Assert.False(button.IsEnabled, "Record is live while nothing is listening");
            Assert.StartsWith("Grayed because Hamlet is not listening", ToolTip.GetTip(button) as string, StringComparison.Ordinal);

            await TheControlsTimCanPress.ConnectAsync(window, panel);

            var source = (TrainingAudioSource)typeof(MainWindowViewModel)
                .GetField("_audioInput", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(panel)!;

            source.Stop();
            source.PumpOnce(source.SampleRate * 5);
            TheControlsTimCanPress.Settle(window);

            Assert.True(button.IsEnabled, "Record is greyed while listening");

            await ((IAsyncRelayCommand)button.Command!).ExecuteAsync(null);

            var wav = Assert.Single(Directory.GetFiles(folder, "cw-*.wav"));
            var sheet = Assert.Single(Directory.GetFiles(folder, "cw-*.txt"));

            _output.WriteLine($"wrote {Path.GetFileName(wav)} ({new FileInfo(wav).Length} bytes) and {Path.GetFileName(sheet)}; status: {panel.StatusText}");

            Assert.Equal(Path.GetFileNameWithoutExtension(wav), Path.GetFileNameWithoutExtension(sheet));
        }
        finally
        {
            await TheControlsTimCanPress.DisconnectAsync(window, panel);
            window.Close();
            MainWindowViewModel.CaptureFolder = was;

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }
        }
    }
}

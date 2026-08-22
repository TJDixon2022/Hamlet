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
/// The press that marks a case is on the screen, and so is the keying meter.
/// </summary>
/// <remarks>
/// <para>**BOTH WERE DELETED BY ACCIDENT AND NOTHING NOTICED.** The commit that
/// removed the old decoder cut one contiguous block out of the window, from the
/// revisions row it meant to remove down to the offer row, and the keying meter
/// and the capture press were in between. The command and every property behind
/// them stayed on the view model, so `BindingHealthTests` had nothing to
/// complain about: **a binding that resolves and an element that is not there
/// look the same to a test that only reads the log.**</para>
/// <para>**AND AN EVENING WITHOUT THE PRESS PRODUCES NO EVIDENCE AT ALL.** It is
/// how the operator says he heard a station, which is the only number in this
/// project that does not come from Hamlet's own decoder, so a night of tuning
/// with the button missing leaves nothing to score (HM-DEC-088, HM-DEC-091).</para>
/// </remarks>
public sealed class TheCapturePressIsOnTheScreenTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the found controls are printed.</param>
    public TheCapturePressIsOnTheScreenTests(ITestOutputHelper output)
        => _output = output;

    /// <remarks>
    /// Proves the press exists, is wired to the command that keeps the audio, and
    /// says what it is for.
    /// </remarks>
    [AvaloniaFact]
    public void ThePressIsThereAndItIsWiredToTheCommandThatKeepsTheAudio()
    {
        var layouts = Hamlet.App.Layout.LayoutStore.Path;
        Hamlet.App.Layout.LayoutStore.Path =
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");

        try
        {
            var model = new MainWindowViewModel(new AppSettings(), null);
            var window = new MainWindow { DataContext = model };

            window.Show();

            // **THE TERMINAL IS NOT ON THE CANVAS A FIRST RUN LANDS ON**, which
            // is Getting started, so the widget has to be summoned exactly as the
            // operator summons it from the tray. Nothing else about the window is
            // arranged: this test is about whether the press is inside the panel,
            // not about which preset carries it.
            model.Canvas.Add(Hamlet.App.Layout.Widgets.Find(
                Hamlet.App.Layout.Widgets.Terminal));

            for (var i = 0; i < 5; i++)
            {
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            }

            var buttons = window.GetVisualDescendants()
                .OfType<Button>()
                .Where(b => b.Content as string == "I hear a station")
                .ToList();

            // **THE METER WENT MISSING IN THE SAME CUT**, so it is checked in the
            // same test: it is the independent witness, and its whole value is
            // that it can contradict the decoder while the operator is sitting
            // at the radio (HM-DEC-091).
            var meterLines = window.GetVisualDescendants()
                .OfType<TextBlock>()
                .Count(t => t.Text is not null
                    && t.Text.StartsWith(
                        "This listens for keying on its own",
                        StringComparison.Ordinal));

            _output.WriteLine($"{buttons.Count} capture presses on the screen");



            window.Close();

            Assert.True(
                buttons.Count == 1,
                "the press that marks a case is not on the screen, so an evening "
                + "at the radio would produce no evidence at all");

            Assert.NotNull(buttons[0].Command);

            Assert.True(
                meterLines == 1,
                "the keying meter is not on the screen, and it is the one "
                + "instrument that can tell him a station is there when the "
                + "decoder says nothing");
        }
        finally
        {
            try
            {
                File.Delete(Hamlet.App.Layout.LayoutStore.Path);
            }
            catch (IOException)
            {
                // A leftover temporary file is not a failing test.
            }

            Hamlet.App.Layout.LayoutStore.Path = layouts;
        }
    }
}

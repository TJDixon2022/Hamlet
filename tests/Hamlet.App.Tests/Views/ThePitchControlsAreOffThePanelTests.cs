using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Views;

/// <summary>
/// The operator is no longer asked to decide the decode pitch.
/// </summary>
/// <remarks>
/// <para>**IT WAS A WORKAROUND DRESSED AS A FEATURE** (Tim's ruling of
/// 2026-08-27, in his words: *"It shouldn't involve me randomly clicking on a
/// button I don't understand."*). "Hold this pitch" and the pitch half of "I
/// hear a station" existed because acquisition does not work — six families of
/// admission statistic were measured across five units and none of them can find
/// a station he can plainly hear. Putting that in his hands made a decoder
/// problem his to solve.</para>
/// <para>**THE ENGINE KEEPS THE CAPABILITY.** `CwDecoder.AssertStation` and
/// `AssertAt` are untouched and still reachable, so unit 1.11.21's measurement
/// is not lost. What has gone is the panel using it.</para>
/// </remarks>
public sealed class ThePitchControlsAreOffThePanelTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the findings are printed.</param>
    public ThePitchControlsAreOffThePanelTests(ITestOutputHelper output)
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

        for (var i = 0; i < 6; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        check(window, model);

        window.Close();
    }

    /// <remarks>
    /// Proves the button is not on the panel — not merely hidden, but absent
    /// from the visual tree, so no state can bring it back.
    /// </remarks>
    [AvaloniaFact]
    public void HoldThisPitchIsNotOnThePanel()
    {
        With((window, _) =>
        {
            var found = window.GetVisualDescendants()
                .OfType<ContentControl>()
                .Select(c => c.Content as string)
                .Where(t => t is not null)
                .Concat(window.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(t => t.Text))
                .Where(t => t is not null && (
                    t.Contains("Hold this pitch", StringComparison.Ordinal)
                    || t.Contains("Follow again", StringComparison.Ordinal)))
                .ToList();

            _output.WriteLine(
                found.Count == 0
                    ? "no pitch control anywhere in the window"
                    : string.Join(", ", found));

            Assert.Empty(found);
        });
    }

    /// <remarks>
    /// <para>Proves the sheet cannot say the operator asserted a pitch, because
    /// he can no longer assert one from the panel. A capture reporting an
    /// operator-asserted pitch after this would be the sheet claiming a decision
    /// nobody made (§0.0).</para>
    /// <para>Asserted on the report the sidecar is written from, which is the
    /// one place the claim could enter.</para>
    /// </remarks>
    [AvaloniaFact]
    public void ACaptureCannotReportAnAssertedPitch()
    {
        With((_, model) =>
        {
            var report = model.DecodeReport;

            _output.WriteLine(
                $"PitchWasAsserted={report.PitchWasAsserted}, "
                + $"PitchWasMeasured={report.PitchWasMeasured}");

            Assert.False(
                report.PitchWasAsserted,
                "the panel no longer lets the operator assert a pitch, so no "
                + "capture may report one");
        });
    }

    /// <remarks>
    /// Proves the engine kept what the panel gave up. The ruling takes the
    /// control off the screen; it does not delete the measurement unit 1.11.21
    /// produced, and a later unit will want it.
    /// </remarks>
    [Fact]
    public void TheEngineStillCarriesTheCapability()
    {
        var decoder = new CwDecoder(48000, 600);

        Assert.False(decoder.PitchWasAsserted);

        decoder.AssertAt(500);

        _output.WriteLine(
            $"after AssertAt(500): asserted={decoder.PitchWasAsserted}, "
            + $"locked={decoder.LockedToneHz:0.0}");

        Assert.True(decoder.PitchWasAsserted);
        Assert.Equal(500, decoder.LockedToneHz);

        decoder.Unlock();

        Assert.False(decoder.PitchWasAsserted);
    }
}

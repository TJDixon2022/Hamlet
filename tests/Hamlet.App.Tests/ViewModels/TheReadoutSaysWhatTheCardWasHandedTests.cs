using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.App.Views;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 269, task 3: **after a send, under the waterfall, the level
/// that reached the card - and the screen says which number that is.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT: A READOUT THAT SHOWS THE
/// OPERATOR HIS OWN SETTING AND CALLS IT A MEASUREMENT.** The one level Hamlet
/// showed after a send was <c>MainWindowViewModel.LevelLine</c>, which reads
/// <c>Ft8Transmission.PeakSample</c> - the peak of the array the composer
/// produced, which is the drive setting read back - and counted clipping over
/// those same composed samples, which the composer built inside the rails. At the
/// rig that is the difference between a level he has verified and a number he
/// typed, and **nothing in the tree before tonight could tell them apart**: unit
/// 265 recorded that every reader of <c>WasapiTransmitSink.PeakWritten</c> and
/// <c>ClippedSamples</c> in the whole repository was a test.</para>
/// <para>**SO THE FAKE REPORTS A PEAK DELIBERATELY DIFFERENT FROM THE COMPOSED
/// ONE.** A readout that showed the setting would read -12.0 dBFS here and the
/// assertion is on -6.0, so the two cannot be confused into passing.</para>
/// <para>**THE KEYING PATH IS NOT TOUCHED TO GET THE NUMBER**, which is what
/// unit 265 could not see a way to avoid. The route does not go through
/// <c>Ft8TransmitSequence</c> at all: the application constructs the sink itself
/// at <c>MainWindowViewModel.cs:8026</c> and dropped the reference, so it keeps
/// it and reads the two figures off it after the boundary has already returned,
/// with nothing keyed. <c>Ft8TransmitSequence.RunAsync</c>, the key, the sink
/// call, the <c>finally</c>, the abort, the stop, <c>Ft8ArmedSend</c> and
/// <c>ITransmitAudioSink.PlayAsync</c> are all unchanged.</para>
/// <para>**NOTHING IS OPENED, NOTHING IS KEYED, AND NO SOUND IS MADE**
/// (`SHACK_FACTS.md` FACT-004). The port is <see cref="FakePort"/> and the sink
/// arrives through the substituted factory. **No figure here says anything about
/// the IC-7300.**</para>
/// </remarks>
public sealed class TheReadoutSaysWhatTheCardWasHandedTests
{
    /// <summary>An endpoint id of the shape Windows uses. Never opened.</summary>
    private const string NamedEndpoint = "{0.0.0.00000000}.{a-render-endpoint}";

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readout and every figure are printed.</param>
    public TheReadoutSaysWhatTheCardWasHandedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **One clicked send through the application leaves the readout under the
    /// waterfall reading what the sink reported** - and saying which number that
    /// is.
    /// </summary>
    /// <remarks>
    /// The click is <c>SendCallToAnyoneCommand</c> and the boundary is
    /// <c>AtSlotBoundaryAsync</c>, the shape
    /// <c>TheApplicationSendsAtTheLevelTheOperatorSetTests.cs:90</c> already
    /// uses, driven on a **realized window** so the readout is asserted where the
    /// operator would read it and not only on a view-model string.
    /// </remarks>
    [AvaloniaFact]
    public async Task AfterOneClickedSendTheReadoutIsWhatTheSinkReported()
    {
        var scene = Scene();

        // **NOT THE COMPOSED PEAK.** The default drive of 0.25 composes at
        // -12.04 dBFS; the fake says the endpoint was handed 0.5, which is
        // -6.02 dBFS, and clamped four samples getting there.
        scene.Sink.ReportsPeakWritten = 0.5;
        scene.Sink.ReportsClippedSamples = 4;

        var send = await ClickCqAsync(scene);

        Pump(scene.Window);

        var readout = Named<TextBlock>(scene.Window, "DigitalTransmitLevelText");
        var text = readout.Text ?? "";

        _output.WriteLine("composed peak : " + send.Transmission.PeakSample.ToString("F6")
            + "  (" + (20.0 * Math.Log10(send.Transmission.PeakSample)).ToString("F2")
            + " dBFS)");
        _output.WriteLine("sink reported : " + scene.Sink.ReportsPeakWritten.ToString("F6")
            + "  (" + (20.0 * Math.Log10(scene.Sink.ReportsPeakWritten)).ToString("F2")
            + " dBFS), " + scene.Sink.ReportsClippedSamples + " clamped");
        _output.WriteLine("the readout under the waterfall reads:");
        _output.WriteLine("  " + text);

        // **THE MEASUREMENT, NOT THE SETTING.** -6.0 is the sink's figure;
        // -12.0 would be the drive read back, which is the whole failure.
        Assert.Contains("-6.0 dBFS", text, StringComparison.Ordinal);
        Assert.DoesNotContain("-12.0 dBFS", text, StringComparison.Ordinal);

        // **AND THE SINK'S OWN CLAMP COUNT**, not the composed array's zero.
        Assert.Contains("4 samples", text, StringComparison.Ordinal);

        scene.Window.Close();
    }

    /// <summary>
    /// **The readout says which number it is**, so an operator is never left
    /// guessing whether he is reading his own setting back.
    /// </summary>
    /// <remarks>
    /// Composed peak and written peak are different quantities and the screen
    /// carries both, in two sentences that are worded apart: unit 268's 456 ms
    /// against unit 263's 15-20 ms is the precedent for saying so on the face of
    /// it. **The clip count is named for what it counts too** - clamping on the
    /// way out of the sink, which is not the same thing as the composed array
    /// having a sample outside the rails.
    /// </remarks>
    [AvaloniaFact]
    public async Task TheReadoutNamesTheQuantityItIsShowing()
    {
        var scene = Scene();

        scene.Sink.ReportsPeakWritten = 0.5;

        await ClickCqAsync(scene);

        Pump(scene.Window);

        var measured = Named<TextBlock>(scene.Window, "DigitalTransmitLevelText").Text ?? "";
        var composed = Named<TextBlock>(scene.Window, "DigitalSendReservedLine").Text ?? "";

        _output.WriteLine("the measured line reads:");
        _output.WriteLine("  " + measured);
        _output.WriteLine("the composed line beside it reads:");
        _output.WriteLine("  " + composed);

        // **THE MEASURED ONE SAYS THE CARD WAS HANDED IT.**
        Assert.Contains("handed", measured, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sound card", measured, StringComparison.OrdinalIgnoreCase);

        // **AND SAYS WHAT THE CLIP COUNT COUNTS.**
        Assert.Contains("clamp", measured, StringComparison.OrdinalIgnoreCase);

        // **AND THE OTHER ONE STILL SAYS IT IS THE COMPOSED LEVEL**, so the two
        // quantities on the screen are never one number wearing two hats.
        Assert.Contains("composed", composed, StringComparison.OrdinalIgnoreCase);

        scene.Window.Close();
    }

    /// <summary>
    /// **Nothing was transmitted that was not clicked**: a second boundary with
    /// nothing armed reaches no sink and puts no byte on the wire.
    /// </summary>
    /// <remarks>
    /// The readout is a new reason to hold a reference to the sink, and a
    /// reference held is a second thing that could reach it. This is the
    /// one-click rule (§0.2) asserted against exactly that: the counts are read
    /// after the clicked send and again after an unclicked boundary, and they are
    /// the same counts.
    /// </remarks>
    [AvaloniaFact]
    public async Task ASecondBoundaryWithNothingArmedReachesNothing()
    {
        var scene = Scene();

        scene.Sink.ReportsPeakWritten = 0.5;

        await ClickCqAsync(scene);

        var callsAfterTheClick = scene.Sink.TimesCalled;
        var framesAfterTheClick = scene.Port.Written.Count;

        // **A LATER BOUNDARY, WITH NOTHING ARMED.** Nobody clicked anything.
        var later = await scene.Panel.AtSlotBoundaryAsync(
            Ft8Slots.SlotStart(DateTime.UtcNow).AddSeconds(15));

        Pump(scene.Window);

        _output.WriteLine("after the click      : " + callsAfterTheClick
            + " sink calls, " + framesAfterTheClick + " frames on the wire");
        _output.WriteLine("after the unclicked boundary : " + scene.Sink.TimesCalled
            + " sink calls, " + scene.Port.Written.Count + " frames on the wire");
        _output.WriteLine("the boundary returned : "
            + (later is null ? "null - nothing was armed" : later.Outcome.ToString()));

        Assert.Equal(1, callsAfterTheClick);
        Assert.Equal(callsAfterTheClick, scene.Sink.TimesCalled);
        Assert.Equal(framesAfterTheClick, scene.Port.Written.Count);

        scene.Window.Close();
    }

    /// <summary>The CQ button, clicked, taken through its slot boundary.</summary>
    /// <param name="scene">The window and the fakes behind it.</param>
    /// <returns>The send the application armed.</returns>
    private static async Task<OperatorSend> ClickCqAsync(Built scene)
    {
        scene.Panel.SendCallToAnyoneCommand.Execute(null);

        var slot = scene.Panel.ArmedForSlotUtc;

        Assert.True(
            slot is not null,
            "the CQ button armed nothing: " + scene.Panel.DigitalSendLine);

        var result = await scene.Panel.AtSlotBoundaryAsync(slot!.Value);

        Assert.NotNull(result);
        Assert.Equal(Ft8ArmOutcome.Ran, result!.Outcome);
        Assert.True(result.Run!.Sent, result.Run.Reason);
        Assert.NotNull(result.Send);

        return result.Send!;
    }

    /// <summary>One realized control, found on the window by its own name.</summary>
    /// <typeparam name="T">What kind of control it should be.</typeparam>
    /// <param name="window">The shown window.</param>
    /// <param name="name">Its <c>x:Name</c>.</param>
    /// <returns>The control.</returns>
    private static T Named<T>(MainWindow window, string name)
        where T : Control
    {
        Pump(window);

        var found = window.GetVisualDescendants().OfType<T>()
            .FirstOrDefault(c => c.Name == name);

        Assert.True(
            found is not null,
            "there is no " + typeof(T).Name + " called \"" + name
            + "\" on the realized window.");

        return found!;
    }

    /// <summary>The window, the panel and the fakes behind them.</summary>
    private sealed record Built(
        MainWindow Window, MainWindowViewModel Panel, AppSettings Settings,
        FakePort Port, FakeSink Sink);

    /// <summary>
    /// The real window on the Digital tab, on 20 m, with a fake port and a fake
    /// sink reached through the application's own <c>BuildTheArmedSend</c>.
    /// </summary>
    /// <returns>The window and everything behind it.</returns>
    private static Built Scene()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = "KC3QIS";
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;
        settings.AudioOutputDeviceId = NamedEndpoint;

        var panel = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalDecodedExpanded = true,
        };

        var window = new MainWindow { DataContext = panel, Width = 1400, Height = 1400 };

        window.Show();
        Pump(window);

        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        var port = new FakePort();
        var sink = new FakeSink();

        panel.TransmitSinkFactory = _ => sink;

        // **THE APPLICATION'S OWN ROUTE**, and it is the route that has to keep
        // the reference: `BuildTheArmedSend` is where the sink is constructed.
        panel.BuildTheArmedSend(port);

        Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);

        Pump(window);

        return new Built(window, panel, settings, port, sink);
    }

    /// <summary>Runs the dispatcher queue and lays the window out.</summary>
    /// <param name="window">The window.</param>
    private static void Pump(Window window)
    {
        for (var i = 0; i < 5; i++)
        {
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }
    }
}

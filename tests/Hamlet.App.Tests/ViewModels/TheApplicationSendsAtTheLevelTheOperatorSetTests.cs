using System.Globalization;
using Avalonia.Threading;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 265, tasks 2 and 4: **what the application transmits at is
/// what the operator set, by both send routes, and afterwards he is told what it
/// went out at.**
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT.** Two of them, and they are the
/// two halves of the same criterion. First: Tim's first transmission goes into
/// the radio's USB input at 0 dBFS, a heavily overdriven FT8 signal goes out over
/// other people's band, and there is no control in Hamlet to turn it down.
/// Second: he turns the drive down and then has no way to tell what level
/// actually reached the card, so he is setting his radio's ALC against a number
/// Hamlet knows and never shows him.</para>
/// <para>**BOTH SEND ROUTES ARE DRIVEN, AND THEY ARE DIFFERENT DOORS.** The CQ
/// button is <c>SendCallToAnyoneCommand</c>
/// (<c>MainWindowViewModel.cs:8471</c>) and the right-click send takes its text
/// off <c>SendMenuFor</c> and goes through <c>SendMessageCommand</c>. They meet
/// at the one <c>ComposeSignal</c> call site in the whole of `src/`, and this
/// asserts they arrive there with the same level rather than trusting that they
/// do.</para>
/// <para>**NOTHING IS OPENED, NOTHING IS KEYED, AND NO SOUND IS MADE**
/// (`SHACK_FACTS.md` FACT-004). The port is <see cref="FakePort"/> and the sink
/// arrives through the substituted factory units 260 and 262 left on the view
/// model. **Every figure here was measured on the development machine, which has
/// never had a radio attached to it, and none of it says anything about what the
/// IC-7300's USB modulation input expects.**</para>
/// </remarks>
public sealed class TheApplicationSendsAtTheLevelTheOperatorSetTests : IDisposable
{
    /// <summary>An endpoint id of the shape Windows uses. Never opened.</summary>
    private const string NamedEndpoint = "{0.0.0.00000000}.{a-render-endpoint}";

    /// <summary>FT8's watering hole on 20 m, where a General may send data.</summary>
    private const long Ft8On20m = 14_074_000;

    /// <summary>The operator.</summary>
    private const string Mine = "KC3QIS";

    /// <summary>The station he works.</summary>
    private const string His = "W1ABC";

    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-unit265-" + Guid.NewGuid().ToString("N")[..8]);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every measured figure is printed.</param>
    public TheApplicationSendsAtTheLevelTheOperatorSetTests(ITestOutputHelper output)
        => _output = output;

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (Exception)
        {
            // Test cleanup only.
        }
    }

    /// <summary>
    /// **The CQ button and the right-click send both go out at the level in
    /// Settings, and it is not full scale.**
    /// </summary>
    /// <remarks>
    /// The level is read off <c>Ft8Transmission.PeakSample</c> on the send the
    /// application actually armed, which is measured over the array every time it
    /// is asked for - so it is the peak of the samples that were handed to the
    /// sink and not a figure carried alongside them.
    /// </remarks>
    [Fact]
    public async Task BothSendRoutesTransmitAtTheDriveLevelInSettings()
    {
        var (panel, settings, _, telemetry) = Panel();
        var port = new FakePort();

        // A level that is neither the default nor full scale, so a pass cannot
        // come from the composer's own fallback or from nothing happening.
        const float chosen = 0.4f;

        settings.AudioOutputDeviceId = NamedEndpoint;
        settings.TransmitDrivePeak = chosen;
        panel.BuildTheArmedSend(port);

        Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);

        // ---- ROUTE 1: THE CQ BUTTON.
        var cq = await ClickCqAsync(panel);

        // ---- ROUTE 2: THE RIGHT-CLICK SEND, off a row's own menu.
        var slot0 = Ft8Slots.SlotStart(DateTime.UtcNow);
        var heard = panel.AddDecodeRowForTests(
            slot0.ToString("HHmmss", CultureInfo.InvariantCulture),
            "-14", "0.2", "1240", "CQ " + His + " EM12", slot0);

        var answer = await ClickRowAsync(panel, heard, Ft8SendShape.Grid);

        telemetry.Dispose();

        var cqPeak = cq.Transmission.PeakSample;
        var answerPeak = answer.Transmission.PeakSample;

        _output.WriteLine("drive in settings : " + chosen.ToString("F6")
            + "  (" + (20.0 * Math.Log10(chosen)).ToString("F2") + " dBFS)");
        _output.WriteLine("CQ button   \"" + cq.Transmission.Text + "\" peak "
            + cqPeak.ToString("F6") + "  ("
            + (20.0 * Math.Log10(cqPeak)).ToString("F2") + " dBFS)");
        _output.WriteLine("right-click \"" + answer.Transmission.Text + "\" peak "
            + answerPeak.ToString("F6") + "  ("
            + (20.0 * Math.Log10(answerPeak)).ToString("F2") + " dBFS)");

        // **BOTH ROUTES GOT THE LEVEL THAT WAS SET.**
        Assert.True(
            Math.Abs(cqPeak - chosen) <= 0.005f,
            "the CQ button went out at " + cqPeak.ToString("F6")
            + " with " + chosen.ToString("F6") + " set in Settings.");

        Assert.True(
            Math.Abs(answerPeak - chosen) <= 0.005f,
            "the right-click send went out at " + answerPeak.ToString("F6")
            + " with " + chosen.ToString("F6") + " set in Settings.");

        // **AND THEY GOT THE SAME ONE**, which is the property that fails if a
        // second compose site is ever added on one of the two doors.
        Assert.Equal(cqPeak, answerPeak, 4);

        // **AND IT IS NOT FULL SCALE**, which is what this unit exists to change.
        Assert.True(cqPeak < 0.99f, "the CQ button still went out at full scale.");
        Assert.True(answerPeak < 0.99f, "the right-click send still went out at full scale.");
    }

    /// <summary>
    /// **The default, untouched, is the conservative one and not full scale.**
    /// </summary>
    /// <remarks>
    /// The test above sets a level. This one sets nothing at all - a fresh
    /// <see cref="AppSettings"/>, the way a first run reads it - because *the
    /// operator who never opens Settings* is exactly the operator this unit is
    /// protecting.
    /// </remarks>
    [Fact]
    public async Task AnOperatorWhoSetsNothingStillDoesNotTransmitAtFullScale()
    {
        var (panel, settings, _, telemetry) = Panel();
        var port = new FakePort();

        settings.AudioOutputDeviceId = NamedEndpoint;
        panel.BuildTheArmedSend(port);

        var cq = await ClickCqAsync(panel);

        telemetry.Dispose();

        var peak = cq.Transmission.PeakSample;
        var dbfs = 20.0 * Math.Log10(peak);

        _output.WriteLine("settings drive : " + settings.TransmitDrivePeak.ToString("F6"));
        _output.WriteLine("peak sent      : " + peak.ToString("F6")
            + "  (" + dbfs.ToString("F2") + " dBFS)");
        _output.WriteLine("below full scale by : " + (-dbfs).ToString("F2") + " dB");

        Assert.Equal(Ft8Composer.DefaultDrivePeak, settings.TransmitDrivePeak);
        Assert.True(
            dbfs <= -6.0,
            "an operator who set nothing transmitted at " + dbfs.ToString("F2")
            + " dBFS, which is not at least 6 dB below full scale.");
    }

    /// <summary>
    /// **After a send, the line under the waterfall says what level it went out
    /// at and how much was clipped.**
    /// </summary>
    /// <remarks>
    /// <para>Task 4. The assertion is on the string an operator would actually
    /// read - <see cref="MainWindowViewModel.DigitalSendLine"/> - driven through
    /// the application's own send path on the fake port and the substituted sink
    /// factory. **No device is opened.**</para>
    /// <para>**IT IS THE COMPOSED PEAK AND THE LINE SAYS SO.** Unit 265 task 1
    /// question 3 measured that nothing in `src/` reads
    /// <c>WasapiTransmitSink.PeakWritten</c> and that there is no route to it
    /// from what <c>Ft8TransmitSequence</c> returns; building one would mean
    /// touching the keying path, which units 255, 261 and 263 proved and this
    /// unit may not disturb.</para>
    /// </remarks>
    [Fact]
    public async Task AfterASendTheLineSaysWhatLevelItWentOutAt()
    {
        var (panel, settings, _, telemetry) = Panel();
        var port = new FakePort();

        settings.AudioOutputDeviceId = NamedEndpoint;
        panel.BuildTheArmedSend(port);

        await ClickCqAsync(panel);

        telemetry.Dispose();

        // **THE LINE IS POSTED TO THE UI THREAD** at
        // `MainWindowViewModel.cs:8309`, the way every send-area line has been
        // since unit 261. Nothing pumps that queue in a test process, so the
        // queue is run here - the string asserted below is the one the posted
        // job writes and not a stand-in for it.
        Dispatcher.UIThread.RunJobs();

        var line = panel.DigitalSendLine;

        _output.WriteLine("the line the operator reads:");
        _output.WriteLine("  " + line);

        // -12.0 dBFS, from the default drive of 0.25.
        var expected = (20.0 * Math.Log10(Ft8Composer.DefaultDrivePeak)).ToString("0.0")
            + " dBFS";

        Assert.Contains(expected, line, StringComparison.Ordinal);
        Assert.Contains("clipped", line, StringComparison.OrdinalIgnoreCase);

        // **IT SAYS WHICH NUMBER IT IS.** A level the operator sets his ALC by
        // must not be read as the level that left the machine when it is the
        // level Hamlet composed at.
        Assert.Contains("composed", line, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The CQ button, clicked, taken through its slot boundary.
    /// </summary>
    /// <param name="panel">The view model.</param>
    /// <returns>The send the application armed and what the boundary did.</returns>
    private async Task<OperatorSend> ClickCqAsync(MainWindowViewModel panel)
    {
        panel.SendCallToAnyoneCommand.Execute(null);

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null, "the CQ button armed nothing: " + panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot!.Value);

        Assert.NotNull(result);
        Assert.Equal(Ft8ArmOutcome.Ran, result!.Outcome);
        Assert.True(result.Run!.AudioWentOut, result.Run.Reason);
        Assert.NotNull(result.Send);

        return result.Send!;
    }

    /// <summary>
    /// One option off a row's own right-click menu, clicked, taken through its
    /// slot boundary.
    /// </summary>
    /// <param name="panel">The view model.</param>
    /// <param name="row">The row the operator right-clicked.</param>
    /// <param name="shape">Which of the menu's options he picked.</param>
    /// <returns>The send the application armed.</returns>
    private async Task<OperatorSend> ClickRowAsync(
        MainWindowViewModel panel, DigitalDecodeRow row, Ft8SendShape shape)
    {
        var menu = panel.SendMenuFor(row);

        Assert.True(menu is not null, "no menu for the row \"" + row.Message + "\".");

        var option = menu!.Options.FirstOrDefault(o => o.Shape == shape);

        Assert.True(option is not null, "the menu offered no " + shape + " option.");

        // **THE TEXT IS THE MENU'S, NOT THIS TEST'S.**
        panel.SendMessageCommand.Execute(option!.Text);

        var slot = panel.ArmedForSlotUtc;

        Assert.True(slot is not null,
            "nothing was armed for \"" + option.Text + "\": " + panel.DigitalSendLine);

        var result = await panel.AtSlotBoundaryAsync(slot!.Value);

        Assert.NotNull(result);
        Assert.Equal(Ft8ArmOutcome.Ran, result!.Outcome);
        Assert.True(result.Run!.AudioWentOut, result.Run.Reason);
        Assert.NotNull(result.Send);

        return result.Send!;
    }

    /// <summary>
    /// A panel on 20 m, a licence that permits, a fake sink and the application's
    /// own telemetry writer pointed at a temporary folder.
    /// </summary>
    /// <returns>The panel, its settings, the sink factory and the writer.</returns>
    private (MainWindowViewModel Panel, AppSettings Settings,
        RecordingSinkFactory Factory, JsonlTelemetry Telemetry) Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;

        var telemetry = new JsonlTelemetry(
            _folder,
            "1.12.90",
            category => settings.IsTelemetryEnabled(category),
            settings.TelemetryMaxMegabytes * 1024L * 1024L);

        var panel = new MainWindowViewModel(settings, telemetry);

        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= Ft8On20m && b.Band.HighHz >= Ft8On20m);
        panel.FrequencyHz = Ft8On20m;

        var factory = new RecordingSinkFactory();

        panel.TransmitSinkFactory = factory.Make;

        return (panel, settings, factory, telemetry);
    }

    /// <summary>A sink factory that hands back a fake and opens nothing.</summary>
    private sealed class RecordingSinkFactory
    {
        /// <summary>Every endpoint name it was asked for, in order.</summary>
        public List<string> Calls { get; } = [];

        /// <summary>The one fake it hands back.</summary>
        public FakeSink Sink { get; } = new();

        /// <summary>The factory itself.</summary>
        public ITransmitAudioSink Make(string endpoint)
        {
            Calls.Add(endpoint);

            return Sink;
        }
    }
}

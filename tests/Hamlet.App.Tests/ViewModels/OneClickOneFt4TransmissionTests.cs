using System.Globalization;
using Ft8Sharp.Encode;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// **What the transmit path does when the operator has chosen FT4.** Work
/// instruction 293, task 1 - the trace.
/// </summary>
/// <remarks>
/// <para>**IT IS A MEASUREMENT OF THE STARTING POSITION AND NOTHING LATER CAN
/// RECOVER IT.** Unit 292's arbiter split step 4 on the reading that the transmit
/// half "has no FT4 in it at all". That is true of the code and it is **not** the
/// same as saying nothing happens: with FT4 chosen the send path is reachable and
/// every line of it is FT8's.</para>
/// <para>**THE BREAKAGE IT WOULD HAVE CAUGHT** (`CLAUDE.md`: a unit may not add a
/// test without naming it): **Hamlet keying the transmitter on an FT4 calling
/// frequency with an FT8 waveform at an FT8 boundary** - a signal on the band that
/// no station can read, and that no sentence on screen distinguishes from a good
/// one. Before this unit there was no test in the tree that would have failed if
/// pressing FT4 and right-clicking a row had done exactly that.</para>
/// <para>**NOTHING HERE OPENS A DEVICE OR A PORT** (`SHACK_FACTS.md` FACT-004).
/// The wire is <see cref="FakePort"/> and the card is <see cref="FakeSink"/>,
/// through <c>MainWindowViewModel.TransmitSinkFactory</c>. **Nothing measured here
/// says anything about the IC-7300.**</para>
/// </remarks>
public sealed class OneClickOneFt4TransmissionTests
{
    /// <summary>The operator.</summary>
    private const string Mine = "KC3QIS";

    /// <summary>The station on the row.</summary>
    private const string His = "W1ABC";

    /// <summary>FT4's watering hole on 20 m, from the cited band data.</summary>
    private const long Ft4On20m = 14_080_000;

    /// <summary>FT8's watering hole on 20 m, where the control runs.</summary>
    private const long Ft8On20m = 14_074_000;

    /// <summary>A moment inside an FT4 slot that is not an FT8 boundary.</summary>
    private static readonly DateTime SlotZero =
        new(2026, 9, 9, 18, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every measured figure is printed.</param>
    public OneClickOneFt4TransmissionTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **With FT4 chosen, the send path composes an FT8 waveform and arms it for a
    /// fifteen-second boundary on an FT4 frequency.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS WHAT TIM'S RADIO WOULD HAVE DONE**, and it is the number
    /// task 1 exists to produce. It is asserted rather than described so that the
    /// starting position is on the record when tasks 2 and 3 move it.</para>
    /// <para>**IT DOES NOT REFUSE.** `Ft8TransmitSequence.Sendable` measures
    /// against `Ft8Slots` literals - 0.5 s into a 15 s slot leaves 14.5 s and
    /// 12.64 s fits - so nothing between the click and the keying frame notices
    /// that the operator asked for FT4.</para>
    /// </remarks>
    [Fact]
    public async Task TodayAnFt4PressComposesFt8TonesAndArmsThemForAnFt8Boundary()
    {
        var scene = Scene(DigitalMode.Ft4, Ft4On20m);

        var message = His + " " + Mine + " FN00";

        scene.Panel.SendMessageCommand.Execute(message);

        var armed = scene.Panel.ArmedForSlotUtc;

        Assert.True(armed is not null, scene.Panel.DigitalSendLine);

        // **THE SEND IS READ OFF THE BOUNDARY'S OWN RESULT**, which is the same
        // object the arm holds, rather than through a new accessor on the panel.
        var result = await scene.Panel.AtSlotBoundaryAsync(armed.Value);
        var send = result!.Send!;
        var audioSeconds =
            send.Transmission.Samples.Length / (double)send.Transmission.SampleRate;

        _output.WriteLine("---- the starting position, before this unit changed anything ----");
        _output.WriteLine("mode chosen      : " + scene.Panel.DigitalMode);
        _output.WriteLine("grid the tab runs: " + scene.Panel.DigitalGrid.Describe());
        _output.WriteLine("dial             : " + scene.Panel.FrequencyHz + " Hz (FT4's 20 m row)");
        _output.WriteLine("message          : \"" + message + "\"");
        _output.WriteLine("composed samples : " + send.Transmission.Samples.Length
            + " at " + send.Transmission.SampleRate + " Hz");
        _output.WriteLine("which is         : " + audioSeconds.ToString("0.###", CultureInfo.InvariantCulture) + " s of tones");
        _output.WriteLine("FT8 synthesises  : " + Ft8Waveform.SymbolCount + " symbols, "
            + Ft8Waveform.ToneCount + " tones at "
            + Ft8Waveform.ToneSpacingHz.ToString("0.####", CultureInfo.InvariantCulture) + " Hz");
        _output.WriteLine("FT4 synthesises  : " + Ft4Waveform.SymbolCount + " symbols, "
            + Ft4Waveform.ToneCount + " tones at "
            + Ft4Waveform.ToneSpacingHz.ToString("0.####", CultureInfo.InvariantCulture) + " Hz");
        _output.WriteLine("armed for        : " + armed.Value.ToString("O", CultureInfo.InvariantCulture));
        _output.WriteLine("seconds into min : "
            + (armed.Value.TimeOfDay.TotalSeconds % 60).ToString("0.###", CultureInfo.InvariantCulture));

        // **IT IS FT8'S WAVEFORM.** 79 symbols at 0.16 s, not 105 at 0.048 s.
        Assert.Equal(
            Ft8Waveform.SymbolCount * Ft8Waveform.SamplesPerSymbol(send.Transmission.SampleRate),
            send.Transmission.Samples.Length);

        Assert.NotEqual(
            Ft4Waveform.SymbolCount * Ft4Waveform.SamplesPerSymbol(send.Transmission.SampleRate),
            send.Transmission.Samples.Length);

        // **AND IT IS ARMED FOR A FIFTEEN-SECOND BOUNDARY.** Every FT8 boundary is
        // on a quarter-minute; half of FT4's eight are not, and this one is.
        Assert.Equal(0.0, armed.Value.TimeOfDay.TotalSeconds % Ft8Slots.SlotSeconds, 6);

        // ---- AND NOTHING BETWEEN THE CLICK AND THE KEYING FRAME REFUSES --------
        _output.WriteLine(string.Empty);
        _output.WriteLine("boundary outcome : " + result!.Outcome);
        _output.WriteLine("run outcome      : " + result.Run?.Outcome);
        _output.WriteLine("run reason       : \"" + result.Run?.Reason + "\"");
        _output.WriteLine("keyed            : " + result.Run?.Keyed);
        _output.WriteLine("came out of tx   : " + result.Run?.CameOutOfTransmit);
        _output.WriteLine("samples to sink  : " + scene.Sink.SamplesHandedOver);

        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.True(result.Run!.Sent, result.Run.Reason);
        Assert.True(result.Run.Keyed);

        _output.WriteLine(string.Empty);
        _output.WriteLine(
            "READ AS A SENTENCE: with FT4 chosen and the dial on FT4's 20 m frequency, "
            + "Hamlet composed " + audioSeconds.ToString("0.##", CultureInfo.InvariantCulture)
            + " s of FT8 tones, armed them for a fifteen-second boundary, keyed the radio, "
            + "played them and unkeyed - and told the operator it had sent.");
    }

    /// <summary>
    /// **The fit guard measures against FT8's slot whatever mode is chosen.**
    /// </summary>
    /// <remarks>
    /// The arithmetic in `Ft8TransmitSequence.Sendable` is FT8's in four places and
    /// the two refusal sentences say *an FT8 transmission needs 12.64 s*. On a
    /// 7.5-second grid a 0.5 s offset leaves 7.0 s, which does not hold 12.64 s of
    /// tones - so a transmission that is refused on FT4's own arithmetic is accepted
    /// here.
    /// </remarks>
    [Fact]
    public void TodayTheFitGuardAsksAboutFifteenSecondsWhateverModeIsRunning()
    {
        var composed = Ft8Composer.ComposeSignal(His + " " + Mine + " FN00");

        Assert.True(composed.Composed, composed.Explanation);

        var left = Ft8Slots.SlotSeconds - MainWindowViewModel.StartSecondsIntoSlot;
        var leftOnFt4 = SlotGrid.Ft4.SlotSeconds - MainWindowViewModel.StartSecondsIntoSlot;

        _output.WriteLine("start into slot  : " + MainWindowViewModel.StartSecondsIntoSlot + " s");
        _output.WriteLine("FT8 slot leaves  : " + left + " s, needs "
            + SlotGrid.Ft8.TransmissionSeconds + " s");
        _output.WriteLine("FT4 slot leaves  : " + leftOnFt4 + " s, needs "
            + SlotGrid.Ft4.TransmissionSeconds + " s");
        _output.WriteLine("FT8 audio is     : "
            + (composed.Transmission!.Samples.Length
               / (double)composed.Transmission.SampleRate)
                .ToString("0.###", CultureInfo.InvariantCulture) + " s");

        // The guard the sequence actually asks, which is FT8's forwarder.
        Assert.True(Ft8Slots.TransmissionFits(left));

        // **AND THE SAME AUDIO DOES NOT FIT AN FT4 SLOT AT ALL**, which is the
        // arithmetic nothing on the send path performs today.
        Assert.False(leftOnFt4 >= SlotGrid.Ft8.TransmissionSeconds);

        // **WHILE FT4'S OWN TRANSMISSION FITS WITH ROOM**, on either figure of the
        // open 4.48-against-5.04 question - which is why the 0.5 s offset needs no
        // ruling.
        Assert.True(SlotGrid.Ft4.TransmissionFits(leftOnFt4));
        Assert.True(leftOnFt4 >= 5.04);
        Assert.True(leftOnFt4 >= 4.48);
    }

    /// <summary>
    /// **What the right-click menu offers on an FT4 row, counted.**
    /// </summary>
    /// <remarks>
    /// **THE TWO MISSING SHAPES ARE A NAMED GAP AND NOT THIS UNIT'S WORK.** Every
    /// FT4 row's ratio is null - `Ft8DeepSignalToNoise.Estimate` is welded to
    /// `Ft8SymbolEncoder.SymbolCount` and `ToneCount` and there is no FT4 equivalent
    /// in this tree - so `MainWindowViewModel.MeasuredReport` returns null and
    /// `Ft8SendOptions.For` returns null text for `Report` and `RogerAndReport`.
    /// **The absence is said out loud in `Ft8SendMenu.Absent`**, which is 0.0
    /// satisfied rather than breached.
    /// </remarks>
    [Fact]
    public void TheFt4MenuIsShortTwoShapesAndSaysWhy()
    {
        var scene = Scene(DigitalMode.Ft4, Ft4On20m);

        scene.Settings.Operator.GridSquare = "FN00";

        var ft4Row = Add(scene.Panel, 2, His + " heard on FT4", Mine + " " + His + " FN42",
            snr: DigitalDecodeRow.NoMeasurement);
        var ft4Menu = scene.Panel.SendMenuFor(ft4Row)!;

        var ft8Scene = Scene(DigitalMode.Ft8, Ft8On20m);

        ft8Scene.Settings.Operator.GridSquare = "FN00";

        var ft8Row = Add(ft8Scene.Panel, 2, His + " heard on FT8", Mine + " " + His + " FN42",
            snr: "-14");
        var ft8Menu = ft8Scene.Panel.SendMenuFor(ft8Row)!;

        _output.WriteLine("FT4 row snr      : \"" + ft4Row.Snr + "\"");
        _output.WriteLine("FT4 menu offers  : " + ft4Menu.Options.Count);

        foreach (var option in ft4Menu.Options)
        {
            _output.WriteLine("    " + option.Label + " : " + option.Text);
        }

        foreach (var absent in ft4Menu.Absent)
        {
            _output.WriteLine("    ABSENT: " + absent);
        }

        _output.WriteLine("FT8 menu offers  : " + ft8Menu.Options.Count);

        foreach (var option in ft8Menu.Options)
        {
            _output.WriteLine("    " + option.Label + " : " + option.Text);
        }

        Assert.Equal(3, ft4Menu.Options.Count);
        Assert.Equal(5, ft8Menu.Options.Count);

        Assert.DoesNotContain(
            ft4Menu.Options,
            o => o.Shape is Ft8SendShape.Report or Ft8SendShape.RogerAndReport);

        // **SAID OUT LOUD**, which is what keeps a short menu from being a silent one.
        Assert.Contains(ft4Menu.Absent, r => r.Contains("report", StringComparison.Ordinal));
    }

    // -------------------------------------------------------------------------
    // The scene: a panel with a fake wire and a fake card, and no device at all.
    // -------------------------------------------------------------------------

    /// <summary>The panel, the settings, the wire and the card.</summary>
    private sealed record Built(
        MainWindowViewModel Panel, AppSettings Settings, FakePort Port, FakeSink Sink);

    /// <summary>A panel running one mode, with something to transmit through.</summary>
    private static Built Scene(DigitalMode mode, long frequencyHz)
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = Mine;
        settings.Operator.GridSquare = "FN00";
        settings.Operator.LicenseClass = LicenseClass.General;
        settings.AudioOutputDeviceId = "{0.0.0.00000000}.{a-fake-endpoint}";

        var panel = new MainWindowViewModel(settings, null) { OperatingMode = "Digital" };

        panel.UseModeForTests(mode);

        panel.SelectedBand = panel.Bands.First(
            b => b.Band.LowHz <= frequencyHz && b.Band.HighHz >= frequencyHz);
        panel.FrequencyHz = frequencyHz;

        var port = new FakePort();
        var sink = new FakeSink();

        panel.TransmitSinkFactory = _ => sink;
        panel.UseRigPortForTests(port);
        panel.BuildTheArmedSend(port);

        Assert.True(panel.HasSomethingToTransmitThrough, panel.DigitalSendLine);

        return new Built(panel, settings, port, sink);
    }

    /// <summary>One row on the table, placed the way a decode places one.</summary>
    private static DigitalDecodeRow Add(
        MainWindowViewModel panel, int slot, string _, string message, string snr)
    {
        var utc = SlotZero.AddSeconds(slot * 15);

        return panel.AddDecodeRowForTests(
            utc.ToString("HHmmss", CultureInfo.InvariantCulture),
            snr, "0.2", "1240", message, utc);
    }
}

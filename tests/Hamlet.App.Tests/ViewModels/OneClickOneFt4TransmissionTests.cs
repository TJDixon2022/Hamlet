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
/// **One click, one FT4 transmission, on FT4's own grid.** Work instruction 293,
/// tasks 1 and 3 - step 4's criterion 3, the arithmetic half.
/// </summary>
/// <remarks>
/// <para>**WHAT THIS MEASURED BEFORE IT ASSERTED ANYTHING** (task 1, the trace).
/// With FT4 chosen and the dial on 14.080 MHz, `SendMessage` composed 151 680
/// samples - 12.64 s of FT8 tones, 79 symbols of 8 tones at 6.25 Hz - armed them
/// for a quarter-minute boundary, keyed the radio, played them, unkeyed and told
/// the operator it had sent. **Nothing between the click and the keying frame
/// noticed that the operator had asked for FT4.** That is what Tim's radio would
/// have done, and the assertions below are what it does instead.</para>
/// <para>**THE BREAKAGE THESE CATCH** (`CLAUDE.md`: a unit may not add a test
/// without naming it): **an FT4 transmission armed for a fifteen-second boundary**
/// - keyed up to 7.5 s late, landing across the boundary into the next slot,
/// decodable by nobody, with the log saying `Sent`; and its twin, **an FT8 waveform
/// on an FT4 calling frequency**, which is a signal on the band no station can read
/// and which no sentence on screen distinguishes from a good one.</para>
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

    /// <summary>A slot the seeded rows sit in.</summary>
    private static readonly DateTime SlotZero =
        new(2026, 9, 9, 18, 0, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where every measured figure is printed.</param>
    public OneClickOneFt4TransmissionTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **With FT4 chosen, one click composes FT4 tones and arms them for the next
    /// 7.5-second boundary.**
    /// </summary>
    /// <remarks>
    /// **THE BOUNDARY IS CHECKED AS A BOUNDARY AND NOT AS A NUMBER.** It is
    /// asserted to be on FT4's grid - a whole number of 7.5-second slots from the
    /// top of the minute - which is the same test for the four boundaries that are
    /// also quarter-minutes and the four that are not.
    /// </remarks>
    [Fact]
    public async Task AnFt4PressComposesFt4TonesAndArmsThemOnFt4sGrid()
    {
        var scene = Scene(DigitalMode.Ft4, Ft4On20m);

        var message = His + " " + Mine + " FN00";

        scene.Panel.SendMessageCommand.Execute(message);

        var armed = scene.Panel.ArmedForSlotUtc;

        Assert.True(armed is not null, scene.Panel.DigitalSendLine);

        var result = await scene.Panel.AtSlotBoundaryAsync(armed.Value);
        var send = result!.Send!;
        var audioSeconds =
            send.Transmission.Samples.Length / (double)send.Transmission.SampleRate;

        _output.WriteLine("mode chosen      : " + scene.Panel.DigitalMode);
        _output.WriteLine("grid the tab runs: " + scene.Panel.DigitalGrid.Describe());
        _output.WriteLine("grid on the send : " + send.Grid.Describe()
            + ", named " + send.Grid.Name);
        _output.WriteLine("dial             : " + scene.Panel.FrequencyHz + " Hz");
        _output.WriteLine("message          : \"" + message + "\"");
        _output.WriteLine("composed samples : " + send.Transmission.Samples.Length
            + " at " + send.Transmission.SampleRate + " Hz");
        _output.WriteLine("which is         : "
            + audioSeconds.ToString("0.###", CultureInfo.InvariantCulture) + " s of tones");
        _output.WriteLine("armed for        : "
            + armed.Value.ToString("O", CultureInfo.InvariantCulture));
        _output.WriteLine("seconds into min : "
            + (armed.Value.TimeOfDay.TotalSeconds % 60)
                .ToString("0.###", CultureInfo.InvariantCulture));
        _output.WriteLine("the operator reads: " + scene.Panel.DigitalSendLine);

        // ---- 1. IT IS FT4'S WAVEFORM, FROM THE PORT ----------------------------
        Assert.Equal(
            Ft4Waveform.SymbolCount * Ft4Waveform.SamplesPerSymbol(send.Transmission.SampleRate),
            send.Transmission.Samples.Length);

        Assert.NotEqual(
            Ft8Waveform.SymbolCount * Ft8Waveform.SamplesPerSymbol(send.Transmission.SampleRate),
            send.Transmission.Samples.Length);

        // ---- 2. AND IT IS ARMED FOR A 7.5-SECOND BOUNDARY ----------------------
        var intoMinute = armed.Value.Ticks % TimeSpan.TicksPerMinute;
        var slotTicks = (long)Math.Round(SlotGrid.Ft4.SlotSeconds * TimeSpan.TicksPerSecond);

        Assert.Equal(0, intoMinute % slotTicks);
        Assert.Equal(SlotGrid.Ft4, send.Grid);

        // ---- 3. AND NOTHING BETWEEN THE CLICK AND THE UNKEY REFUSED ------------
        _output.WriteLine(string.Empty);
        _output.WriteLine("boundary outcome : " + result.Outcome);
        _output.WriteLine("run outcome      : " + result.Run?.Outcome);
        _output.WriteLine("run reason       : \"" + result.Run?.Reason + "\"");
        _output.WriteLine("keyed            : " + result.Run?.Keyed);
        _output.WriteLine("came out of tx   : " + result.Run?.CameOutOfTransmit);
        _output.WriteLine("samples to sink  : " + scene.Sink.SamplesHandedOver);
        _output.WriteLine("radio in receive : " + result.Run?.RadioIsInReceive);

        Assert.Equal(Ft8ArmOutcome.Ran, result.Outcome);
        Assert.True(result.Run!.Sent, result.Run.Reason);
        Assert.True(result.Run.Keyed);
        Assert.Equal(UnkeyRoute.OrdinaryUnkey, result.Run.CameOutOfTransmit);
        Assert.True(result.Run.RadioIsInReceive);
        Assert.Equal(send.Transmission.Samples.Length, scene.Sink.SamplesHandedOver);

        // ---- 4. AND THE SENTENCE NAMES THE MOMENT IT ACTUALLY STARTS AT --------
        // **FOUR OF FT4'S EIGHT BOUNDARIES A MINUTE END IN .5**, and `HH:mm:ss`
        // read `:22.5` as `22` - a sentence naming a moment the transmission does
        // not start at, in the one line telling him what is about to go out.
        Assert.Contains(
            armed.Value.ToString(
                armed.Value.Millisecond == 0 ? "HH:mm:ss" : "HH:mm:ss.f",
                CultureInfo.InvariantCulture),
            scene.Panel.DigitalSendLine,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// **The tick recognises the boundary an FT4 send was armed for.**
    /// </summary>
    /// <remarks>
    /// **THE HALF OF THE ARITHMETIC THAT WOULD HAVE FAILED SILENTLY.**
    /// `DriveTheArmedSend` computed `Ft8Slots.SlotStart`, a quarter-minute whatever
    /// mode was running. An FT4 send armed for `:07.5` would then be compared
    /// against `:00` - <c>NotDue</c> - and against `:15` - <c>TooLate</c>, discarded
    /// rather than sent late, which is correct behaviour producing a wrong outcome:
    /// **the operator clicked and nothing ever went out.** The boundary now comes
    /// off the armed send's own grid.
    /// </remarks>
    [Fact]
    public void EveryFt4BoundaryIsOneTheTickCanRecogniseAndFourOfEightAreNotQuarterMinutes()
    {
        var topOfMinute = new DateTime(2026, 9, 9, 18, 0, 0, DateTimeKind.Utc);
        var quarterMinutes = 0;

        for (var slot = 0; slot < 8; slot++)
        {
            var inside = topOfMinute.AddSeconds((slot * SlotGrid.Ft4.SlotSeconds) + 1.0);
            var boundary = SlotGrid.Ft4.SlotStart(inside);
            var isQuarterMinute = boundary.TimeOfDay.TotalSeconds % Ft8Slots.SlotSeconds == 0;

            if (isQuarterMinute)
            {
                quarterMinutes++;
            }

            _output.WriteLine(
                "slot " + slot + " : boundary "
                + boundary.ToString("mm:ss.f", CultureInfo.InvariantCulture)
                + (isQuarterMinute ? "  (also an FT8 boundary)" : "  (FT4 only)"));

            Assert.Equal(
                topOfMinute.AddSeconds(slot * SlotGrid.Ft4.SlotSeconds), boundary);

            // **AND FT8'S GRID WOULD HAVE GOT IT WRONG FOR FOUR OF THE EIGHT.**
            if (!isQuarterMinute)
            {
                Assert.NotEqual(boundary, Ft8Slots.SlotStart(inside));
            }
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("of FT4's eight boundaries a minute, " + quarterMinutes
            + " are also FT8 boundaries and " + (8 - quarterMinutes) + " are not");

        Assert.Equal(4, quarterMinutes);
    }

    /// <summary>
    /// **THE CONTROL: an FT8 press composes the same array and arms the same
    /// boundary it did before this unit.**
    /// </summary>
    /// <remarks>
    /// **THE TWO REFUSAL SENTENCES ARE PINNED IN THE ENGINE**, where the guard
    /// lives, by <c>TheFitGuardAsksAboutTheGridTheSendIsOnTests</c>. What this pins
    /// is the half only the application can answer: which array a press composes and
    /// which boundary it arms for.
    /// </remarks>
    [Fact]
    public async Task AnFt8PressLeavesTheBoundaryAndTheArrayWhereTheyWere()
    {
        var scene = Scene(DigitalMode.Ft8, Ft8On20m);

        var message = His + " " + Mine + " FN00";

        scene.Panel.SendMessageCommand.Execute(message);

        var armed = scene.Panel.ArmedForSlotUtc;

        Assert.True(armed is not null, scene.Panel.DigitalSendLine);

        var result = await scene.Panel.AtSlotBoundaryAsync(armed.Value);
        var send = result!.Send!;

        _output.WriteLine("composed samples : " + send.Transmission.Samples.Length);
        _output.WriteLine("armed for        : "
            + armed.Value.ToString("O", CultureInfo.InvariantCulture));
        _output.WriteLine("grid on the send : " + send.Grid.Describe());
        _output.WriteLine("run outcome      : " + result.Run?.Outcome);

        Assert.Equal(151_680, send.Transmission.Samples.Length);
        Assert.Equal(
            Ft8Waveform.SymbolCount * Ft8Waveform.SamplesPerSymbol(send.Transmission.SampleRate),
            send.Transmission.Samples.Length);
        Assert.Equal(0.0, armed.Value.TimeOfDay.TotalSeconds % Ft8Slots.SlotSeconds, 6);
        Assert.Equal(SlotGrid.Ft8, send.Grid);
        Assert.True(result.Run!.Sent, result.Run.Reason);
        Assert.Equal(UnkeyRoute.OrdinaryUnkey, result.Run.CameOutOfTransmit);
    }

    /// <summary>
    /// **On FT4, a decode, a slot tick and a countdown reaching zero arm nothing.**
    /// </summary>
    /// <remarks>
    /// <para>**§0.2, and `PHASE_PLAN.md` says FT4 is where the temptation is
    /// twice as strong** - the slots are half as long, so an operator has half the
    /// time to answer and the pressure to answer for him is doubled. **Hamlet
    /// transmits because the operator clicked and for no other reason.**</para>
    /// <para>**IT IS COUNTED ON THE WIRE, NOT ON A FLAG.** Rows arrive, four FT4
    /// boundaries go by and the countdown is read to zero; the port is asked what it
    /// was given, and the answer is nothing at all.</para>
    /// </remarks>
    [Fact]
    public async Task OnFt4ADecodeATickAndACountdownAllArmNothing()
    {
        var scene = Scene(DigitalMode.Ft4, Ft4On20m);

        Assert.Null(scene.Panel.ArmedForSlotUtc);

        // ---- 1. A DECODE ARRIVES, ADDRESSED TO HIM -----------------------------
        Add(scene.Panel, 1, Mine + " " + His + " FN42", snr: DigitalDecodeRow.NoMeasurement);
        Add(scene.Panel, 2, Mine + " " + His + " R-15", snr: DigitalDecodeRow.NoMeasurement);
        Add(scene.Panel, 3, "CQ " + His + " FN42", snr: DigitalDecodeRow.NoMeasurement);

        Assert.Null(scene.Panel.ArmedForSlotUtc);

        // ---- 2. THE COUNTDOWN IS READ, INCLUDING TO ZERO -----------------------
        var turn = scene.Panel.DigitalTurnLine;
        var secondsLeft = scene.Panel.DigitalTurnSecondsLeft;

        // ---- 3. FOUR FT4 BOUNDARIES GO BY --------------------------------------
        var boundary = SlotGrid.Ft4.SlotStart(DateTime.UtcNow);
        var outcomes = new List<Ft8ArmOutcome>();

        for (var slot = 0; slot < 4; slot++)
        {
            var result = await scene.Panel.AtSlotBoundaryAsync(
                boundary.AddSeconds(slot * SlotGrid.Ft4.SlotSeconds));

            outcomes.Add(result!.Outcome);

            Assert.Null(result.Run);
        }

        _output.WriteLine("rows on the table : " + scene.Panel.DigitalDecodes.Count);
        _output.WriteLine("turn line         : " + turn);
        _output.WriteLine("countdown         : " + secondsLeft + " s left");
        _output.WriteLine("FT4 boundaries    : " + string.Join(", ", outcomes));
        _output.WriteLine("armed             : "
            + (scene.Panel.ArmedForSlotUtc?.ToString("O", CultureInfo.InvariantCulture) ?? "nothing"));
        _output.WriteLine("frames on the wire: " + scene.Port.Written.Count);
        _output.WriteLine("times sink played : " + scene.Sink.TimesCalled);

        Assert.All(outcomes, o => Assert.Equal(Ft8ArmOutcome.NothingArmed, o));
        Assert.Null(scene.Panel.ArmedForSlotUtc);
        Assert.Empty(scene.Port.Written);
        Assert.Equal(0, scene.Sink.TimesCalled);
    }

    /// <summary>
    /// **A message no receiver could read back arms nothing and keys nothing on
    /// FT4.**
    /// </summary>
    /// <remarks>
    /// **THE THIRD REFUSAL ON THE PATH, AND IT IS BEFORE THE ARM.** A callsign that
    /// can only travel as a 22-bit hash reads back as itself only for a receiver
    /// that heard the full call in the same slot, so the transmission would be
    /// undecodable on the band - which is what happened twice on 14.074 on
    /// 2026-09-07 with the log saying `Sent`. `Ft8ReadBack` is the message layer's
    /// question and is shared between the two modes, so FT4 gets the same refusal
    /// and gets it for the same reason.
    /// </remarks>
    [Fact]
    public void AnUnreadableMessageOnFt4ArmsNothingAndKeysNothing()
    {
        var scene = Scene(DigitalMode.Ft4, Ft4On20m);

        // His grid whole, which is what forced the hash on 2026-09-07.
        scene.Panel.SendMessageCommand.Execute("VP2MAA " + Mine + " FN00DJ");

        _output.WriteLine("the operator reads: " + scene.Panel.DigitalSendLine);
        _output.WriteLine("armed             : "
            + (scene.Panel.ArmedForSlotUtc?.ToString("O", CultureInfo.InvariantCulture) ?? "nothing"));
        _output.WriteLine("frames on the wire: " + scene.Port.Written.Count);

        Assert.Null(scene.Panel.ArmedForSlotUtc);
        Assert.Empty(scene.Port.Written);
        Assert.Equal(0, scene.Sink.TimesCalled);

        // **AND IT IS SAID OUT LOUD RATHER THAN SWALLOWED.**
        Assert.NotEqual(MainWindowViewModel.NothingHasBeenSent, scene.Panel.DigitalSendLine);
        Assert.NotEmpty(scene.Panel.DigitalSendLine);
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
    /// **The absence is said out loud in `Ft8SendMenu.Absent`**, which is §0.0
    /// satisfied rather than breached.
    /// </remarks>
    [Fact]
    public void TheFt4MenuIsShortTwoShapesAndSaysWhy()
    {
        var scene = Scene(DigitalMode.Ft4, Ft4On20m);
        var ft4Row = Add(scene.Panel, 2, Mine + " " + His + " FN42",
            snr: DigitalDecodeRow.NoMeasurement);
        var ft4Menu = scene.Panel.SendMenuFor(ft4Row)!;

        var ft8Scene = Scene(DigitalMode.Ft8, Ft8On20m);
        var ft8Row = Add(ft8Scene.Panel, 2, Mine + " " + His + " FN42", snr: "-14");
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

        Assert.Contains(ft4Menu.Absent, r => r.Contains("report", StringComparison.Ordinal));
    }

    // -------------------------------------------------------------------------
    // The scene: a panel with a fake wire and a fake card, and no device at all.
    // -------------------------------------------------------------------------

    /// <summary>The panel, the settings, the wire and the card.</summary>
    internal sealed record Built(
        MainWindowViewModel Panel, AppSettings Settings, FakePort Port, FakeSink Sink);

    /// <summary>A panel running one mode, with something to transmit through.</summary>
    internal static Built Scene(DigitalMode mode, long frequencyHz)
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
    internal static DigitalDecodeRow Add(
        MainWindowViewModel panel, int slot, string message, string snr)
    {
        var utc = SlotZero.AddSeconds(slot * 15);

        return panel.AddDecodeRowForTests(
            utc.ToString("HHmmss", CultureInfo.InvariantCulture),
            snr, "0.2", "1240", message, utc);
    }
}

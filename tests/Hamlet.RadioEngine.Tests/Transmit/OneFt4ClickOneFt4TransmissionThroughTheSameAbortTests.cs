using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **One click, one FT4 transmission, through the same abort.** Work instruction
/// 293, task 5 - step 4's criterion 3 entire.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THESE CATCH** (`CLAUDE.md`: a unit may not add a test
/// without naming it): **an FT4 transmission that keys the radio and has no way to
/// unkey it** - a stuck transmitter on a live antenna, which is the failure the
/// abort ruling exists to prevent and which FT4's shorter slots give twice as many
/// chances to reach. And beside it, **a transmission nobody asked for**: FT4's
/// slots are half as long and the temptation to let a decode, a tick or a countdown
/// start one is twice as strong, which `PHASE_PLAN.md` says in those words.</para>
/// <para>**THE ABORT ITSELF IS NOT CHANGED AND IS NOT REWRITTEN.**
/// <c>TransmitAbort</c> is units 257's and 263's, built and proven, and
/// <c>TheAbortFiresFromEveryStateTests</c> is its proof. **It is protocol-neutral**
/// - CI-V `0x17` with `0xFF` and a PTT fallback, knowing nothing about slots,
/// waveforms or modes - so what these prove is that FT4 goes through it, not that
/// it does anything new.</para>
/// <para>**NOTHING HERE OPENS A DEVICE OR A PORT** (`SHACK_FACTS.md` FACT-004).
/// <see cref="FakeSerialPort"/> and <see cref="FakeTransmitAudioSink"/>. **Nothing
/// measured here says anything about the IC-7300.**</para>
/// </remarks>
public sealed class OneFt4ClickOneFt4TransmissionThroughTheSameAbortTests
{
    /// <summary>An FT4 boundary that is not also an FT8 one.</summary>
    private static readonly DateTime Boundary =
        new DateTime(2026, 9, 9, 18, 0, 0, DateTimeKind.Utc).AddSeconds(7.5);

    /// <summary>What the operator asks to send.</summary>
    private const string Message = "W1ABC KC3QIS FN00";

    /// <summary>FT4's watering hole on 20 m, from the cited band data.</summary>
    private const long Ft4On20m = 14_080_000;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the wire is quoted.</param>
    public OneFt4ClickOneFt4TransmissionThroughTheSameAbortTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **Nothing but an arm arms, and a second arm replaces rather than adds.**
    /// </summary>
    [Fact]
    public void NothingButAnArmArmsAndASecondArmReplacesRatherThanAdding()
    {
        var (armed, port, sink) = Armed();

        Assert.False(armed.IsArmed);
        Assert.Null(armed.Armed);

        armed.Arm(SendAt(Boundary));

        Assert.True(armed.IsArmed);

        var first = armed.Armed!;

        armed.Arm(SendAt(Boundary, "W1ABC KC3QIS 73"));

        var second = armed.Armed!;

        _output.WriteLine("after one arm : \"" + first.Transmission.Text + "\"");
        _output.WriteLine("after two arms: \"" + second.Transmission.Text + "\"");
        _output.WriteLine("grid on it    : " + second.Grid.Describe());
        _output.WriteLine("wire so far   : " + Hex(port.Written));
        _output.WriteLine("sink so far   : " + sink.TimesCalled + " plays");

        // **ONE FIELD, SO TWO CLICKS A SECOND APART ARE ONE TRANSMISSION.**
        Assert.Equal("W1ABC KC3QIS 73", second.Transmission.Text);
        Assert.Equal(SlotGrid.Ft4, second.Grid);

        // And arming keys nothing at all.
        Assert.Empty(port.Written);
        Assert.Equal(0, sink.TimesCalled);
    }

    /// <summary>
    /// **A slot tick with nothing armed transmits nothing, and one armed FT4 send
    /// produces exactly one keying.**
    /// </summary>
    /// <remarks>
    /// **THE BOUNDARIES ARE FT4'S, 7.5 s APART.** Two of the four below are not
    /// quarter-minutes at all, which is the case FT8's grid could not express and
    /// where a send would previously have been discarded unsent.
    /// </remarks>
    [Fact]
    public async Task FourFt4BoundariesAfterOneArmProduceExactlyOneKeying()
    {
        var (armed, port, sink) = Armed();

        // ---- BOUNDARIES BEFORE ANYTHING IS ARMED -------------------------------
        var before = await armed.AtBoundaryAsync(Boundary.AddSeconds(-7.5));

        Assert.Equal(Ft8ArmOutcome.NothingArmed, before.Outcome);
        Assert.Null(before.Run);
        Assert.Empty(port.Written);

        armed.Arm(SendAt(Boundary));

        var at = await armed.AtBoundaryAsync(Boundary);
        var next = await armed.AtBoundaryAsync(Boundary.AddSeconds(7.5));
        var after = await armed.AtBoundaryAsync(Boundary.AddSeconds(15.0));

        _output.WriteLine("arms                 : 1");
        _output.WriteLine("FT4 boundaries driven: 4");
        _output.WriteLine("before the arm       : " + before.Outcome);
        _output.WriteLine("its own boundary     : " + at.Outcome + " / " + at.Run?.Outcome);
        _output.WriteLine("the next one         : " + next.Outcome);
        _output.WriteLine("the one after        : " + after.Outcome);
        _output.WriteLine("times the sink played: " + sink.TimesCalled);
        _output.WriteLine("samples handed over  : " + sink.SamplesHandedOver);
        _output.WriteLine("wire                 : " + Hex(port.Written));

        Assert.Equal(Ft8ArmOutcome.Ran, at.Outcome);
        Assert.True(at.Run!.Sent, at.Run.Reason);

        // **THE WHOLE POINT.** The send was consumed, so nothing further went out.
        Assert.Equal(Ft8ArmOutcome.NothingArmed, next.Outcome);
        Assert.Equal(Ft8ArmOutcome.NothingArmed, after.Outcome);
        Assert.Null(next.Run);
        Assert.Null(after.Run);

        Assert.Equal(1, sink.TimesCalled);

        // Keyed once, unkeyed once, counted on the bytes rather than on a flag.
        Assert.Equal(
            Hex([.. KeyFrame, .. UnkeyFrame]), Hex(port.Written));
    }

    /// <summary>
    /// **A boundary that has gone by discards an FT4 send rather than sending it
    /// late.**
    /// </summary>
    /// <remarks>
    /// **ON FT4 THIS IS THE HALF-SLOT CASE.** A send armed for `:07.5` and offered
    /// `:15` is 7.5 s late, which is a whole FT4 slot; the operator chose a slot and
    /// putting his message in a different one is a transmission he did not ask for.
    /// </remarks>
    [Fact]
    public async Task AnFt4SendOfferedALaterBoundaryIsDiscardedRatherThanSentLate()
    {
        var (armed, port, sink) = Armed();

        armed.Arm(SendAt(Boundary));

        var late = await armed.AtBoundaryAsync(Boundary.AddSeconds(7.5));

        _output.WriteLine("armed for : "
            + Boundary.ToString("HH:mm:ss.f", CultureInfo.InvariantCulture));
        _output.WriteLine("offered   : "
            + Boundary.AddSeconds(7.5).ToString("HH:mm:ss.f", CultureInfo.InvariantCulture));
        _output.WriteLine("outcome   : " + late.Outcome);
        _output.WriteLine("wire      : " + Hex(port.Written));

        Assert.Equal(Ft8ArmOutcome.TooLate, late.Outcome);
        Assert.Null(late.Run);
        Assert.Empty(port.Written);
        Assert.Equal(0, sink.TimesCalled);

        // And it is gone rather than held for the boundary after.
        Assert.False(armed.IsArmed);
    }

    /// <summary>
    /// **The abort fires on the FT4 path, and it is the same abort.**
    /// </summary>
    /// <remarks>
    /// <para>**DRIVEN INTO THE FAILURE PATH WITH FT4 AUDIO.** The sink throws
    /// part-way through 5.04 s of FT4 tones; the radio comes out of transmit through
    /// <c>TransmitAbort.Fire</c>, which is CI-V `0x17 0xFF` followed by PTT off -
    /// **the same two frames, in the same order, that an FT8 failure produces**,
    /// because the abort knows nothing about slots or waveforms.</para>
    /// <para>**THE RECORD IS CARRIED OUT RATHER THAN SWALLOWED**, which is what
    /// makes an abort diagnosable (§0.0.1).</para>
    /// </remarks>
    [Fact]
    public async Task TheAbortFiresOnTheFt4PathAndItIsTheSameTwoFrames()
    {
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink
        {
            Throws = new InvalidOperationException("the render client died"),
        };

        var run = await new Ft8TransmitSequence(port, sink).RunAsync(SendAt(Boundary));

        _output.WriteLine("outcome        : " + run.Outcome);
        _output.WriteLine("keyed          : " + run.Keyed
            + ", unkeyed normally: " + run.UnkeyedNormally);
        _output.WriteLine("came out of tx : " + run.CameOutOfTransmit);
        _output.WriteLine("radio in receive: " + run.RadioIsInReceive);
        _output.WriteLine("wire           : " + Hex(port.Written));
        _output.WriteLine("abort cw stop  : written " + run.Abort?.CwStop.Written);
        _output.WriteLine("abort ptt off  : written " + run.Abort?.PttOff.Written);

        Assert.Equal(Ft8TransmitOutcome.AudioFailed, run.Outcome);
        Assert.True(run.Keyed);
        Assert.False(run.UnkeyedNormally);

        // **IT CAME OUT THROUGH THE ABORT AND THE RECORD SAYS SO.**
        Assert.NotNull(run.Abort);
        Assert.Equal(UnkeyRoute.TheAbort, run.CameOutOfTransmit);
        Assert.True(run.RadioIsInReceive);
        Assert.True(run.Abort!.CwStop.Written);
        Assert.True(run.Abort.PttOff.Written);

        // **THE SAME BYTES AN FT8 FAILURE PRODUCES**, read rather than counted.
        Assert.Equal(
            Hex([.. KeyFrame, .. AbortCwStopFrame, .. UnkeyFrame]), Hex(port.Written));
    }

    /// <summary>
    /// **The abort's path takes no lock and waits for nothing, on FT4 as on FT8.**
    /// </summary>
    /// <remarks>
    /// **THE SHAPE, NOT JUST THE OUTCOME.** <c>TransmitAbort.Fire</c> is read out of
    /// the tree: no <c>await</c>, no <c>Task</c>, no <c>lock</c> and no
    /// <c>WriteAsync</c>. **A stop that has to wait for something is a stop that can
    /// be made to wait forever**, and FT4's shorter slots mean the operator reaches
    /// for it sooner. Its own class already carries
    /// <c>NothingOnTheAbortPathWaitsForAnything</c>; this asserts the same property
    /// from the FT4 side rather than rewriting it.
    /// </remarks>
    [Fact]
    public void NothingOnTheAbortPathWaitsForAnythingWhicheverModeReachedIt()
    {
        var path = Path.Combine(
            TheUnkeyHappensWhateverGoesWrongTests.RepositoryRoot(),
            "src", "Hamlet.RadioEngine", "Civ", "TransmitAbort.cs");

        var body = TheUnkeyHappensWhateverGoesWrongTests.CodeOnly(File.ReadAllText(path));

        string[] forbidden = ["await", "async", "Task", "lock", "WriteAsync", "Ft4", "Ft8", "Slot"];

        var found = forbidden
            .Where(name => body.Contains(name, StringComparison.Ordinal))
            .ToList();

        _output.WriteLine("abort          : " + path);
        _output.WriteLine("patterns tried : " + string.Join(", ", forbidden));
        _output.WriteLine("found in code  : "
            + (found.Count == 0 ? "none" : string.Join(", ", found)));

        // **AND THE LAST THREE ARE WHY IT IS PROTOCOL-NEUTRAL.** It names no mode,
        // no grid and no slot, so there is nothing in it for FT4 to have needed.
        Assert.Empty(found);
    }

    /// <summary>
    /// **The operator's stop takes an FT4 transmission off the air: it unarms, it
    /// fires the abort, and it tells the audio to stop.**
    /// </summary>
    /// <remarks>
    /// All three, and <see cref="Ft8StopResult.Outcome"/> naming which happened.
    /// **The three states are asserted separately** because a stop that reports the
    /// same thing whatever it found is the defect unit 253 found in
    /// <c>Ic7300Rig.AbortCw</c>.
    /// </remarks>
    [Fact]
    public void TheStopUnarmsFiresTheAbortAndSaysWhichHappenedOnFt4()
    {
        // ---- 1. NOTHING ARMED, NO PORT: NOTHING HAPPENED AND NONE IS CLAIMED ---
        var (idle, _, _) = Armed();
        var nothing = idle.StopNow(null);

        Assert.Equal(Ft8StopOutcome.NothingToStop, nothing.Outcome);
        Assert.False(nothing.Unarmed);
        Assert.Null(nothing.Abort);

        // ---- 2. AN FT4 SEND ARMED, AND A PORT TO TELL --------------------------
        var (armed, port, _) = Armed();

        armed.Arm(SendAt(Boundary));

        Assert.True(armed.IsArmed);

        var stopped = armed.StopNow(port);

        _output.WriteLine("nothing armed    : " + nothing.Outcome);
        _output.WriteLine("an FT4 send armed: " + stopped.Outcome);
        _output.WriteLine("unarmed          : " + stopped.Unarmed);
        _output.WriteLine("reached the radio: " + stopped.AnythingReachedTheRadio);
        _output.WriteLine("audio told to stop: " + stopped.AudioToldToStop);
        _output.WriteLine("wire             : " + Hex(port.Written));

        Assert.Equal(Ft8StopOutcome.UnarmedAndToldTheRadio, stopped.Outcome);
        Assert.True(stopped.Unarmed);
        Assert.False(armed.IsArmed);
        Assert.True(stopped.AnythingReachedTheRadio);

        // **THE ABORT'S TWO FRAMES, WITH NOTHING KEYED FIRST.** The send never
        // reached a boundary, so `1C 00 01` is nowhere on this wire.
        Assert.Equal(Hex([.. AbortCwStopFrame, .. UnkeyFrame]), Hex(port.Written));

        // ---- 3. AND NOTHING IS ARMED TO FIRE AT THE BOUNDARY IT HAD ------------
        Assert.Null(armed.Armed);
    }

    /// <summary>
    /// **A refusal keys nothing at all - not a key-on followed by an abort.**
    /// </summary>
    /// <remarks>
    /// **TWO REFUSALS, BOTH BEFORE ANYTHING KEYS.** A licence the guard will not
    /// stand behind, and a transmission that does not fit the slot it is on. **The
    /// wire is read, not counted**: a count of zero and a wire holding `1C 00 01`
    /// followed by an abort are different facts, and only the first is a refusal.
    /// The third refusal on this path - a message no receiver could read back - is
    /// in the application, before the arm, and is asserted by
    /// <c>HamletDoesNotKeyWhatNobodyCanReadTests</c>.
    /// </remarks>
    [Fact]
    public async Task ALicenceRefusalAndAFitRefusalEachLeaveTheWireEmpty()
    {
        // ---- 1. THE LICENCE -----------------------------------------------------
        var licencePort = new FakeSerialPort();
        var licenceSink = new FakeTransmitAudioSink();

        var refusedByLicence = await new Ft8TransmitSequence(licencePort, licenceSink)
            .RunAsync(SendAt(Boundary, licenseClass: LicenseClass.Technician));

        _output.WriteLine("licence refusal  : " + refusedByLicence.Outcome);
        _output.WriteLine("reason           : " + refusedByLicence.Reason);
        _output.WriteLine("wire             : \"" + Hex(licencePort.Written) + "\"");
        _output.WriteLine("times sink played: " + licenceSink.TimesCalled);

        Assert.Equal(Ft8TransmitOutcome.RefusedByLicence, refusedByLicence.Outcome);
        Assert.False(refusedByLicence.Keyed);
        Assert.Empty(licencePort.Written);
        Assert.Equal(0, licenceSink.TimesCalled);
        Assert.Null(refusedByLicence.Abort);
        Assert.Equal(UnkeyRoute.NothingWasKeyed, refusedByLicence.CameOutOfTransmit);

        // ---- 2. THE FIT ---------------------------------------------------------
        var fitPort = new FakeSerialPort();
        var fitSink = new FakeTransmitAudioSink();

        var ft8Audio = Ft8Composer.ComposeSignal(Message);

        var refusedAsUnsendable = await new Ft8TransmitSequence(fitPort, fitSink).RunAsync(
            new OperatorSend(
                ft8Audio.Transmission!, Ft4On20m, LicenseClass.General, true, Boundary, 0.5)
            {
                Grid = SlotGrid.Ft4,
            });

        _output.WriteLine(string.Empty);
        _output.WriteLine("fit refusal      : " + refusedAsUnsendable.Outcome);
        _output.WriteLine("reason           : " + refusedAsUnsendable.Reason);
        _output.WriteLine("wire             : \"" + Hex(fitPort.Written) + "\"");
        _output.WriteLine("times sink played: " + fitSink.TimesCalled);

        Assert.Equal(Ft8TransmitOutcome.RefusedAsUnsendable, refusedAsUnsendable.Outcome);
        Assert.False(refusedAsUnsendable.Keyed);
        Assert.Empty(fitPort.Written);
        Assert.Equal(0, fitSink.TimesCalled);
        Assert.Null(refusedAsUnsendable.Abort);
        Assert.Equal(UnkeyRoute.NothingWasKeyed, refusedAsUnsendable.CameOutOfTransmit);
    }

    // -------------------------------------------------------------------------

    /// <summary>What the radio sees when it is keyed.</summary>
    private static byte[] KeyFrame => Frame(CivConstants.PttOn);

    /// <summary>The ordinary unkey, and the abort's second frame.</summary>
    private static byte[] UnkeyFrame => Frame(CivConstants.PttOff);

    /// <summary>The abort's first frame - CI-V `0x17` with `0xFF`.</summary>
    private static byte[] AbortCwStopFrame => new CivFrame(
        CivConstants.DefaultRadioAddress,
        CivConstants.DefaultControllerAddress,
        CivConstants.CmdSendCwMessage,
        new[] { CivConstants.CwStopByte }).ToWireBytes();

    private static byte[] Frame(byte state) => new CivFrame(
        CivConstants.DefaultRadioAddress,
        CivConstants.DefaultControllerAddress,
        CivConstants.CmdTransceiverControl,
        new[] { CivConstants.SubPtt, state }).ToWireBytes();

    private static string Hex(IEnumerable<byte> bytes)
        => TheUnkeyHappensWhateverGoesWrongTests.Hex(bytes);

    /// <summary>One armed send over a fake wire and a fake card.</summary>
    private static (Ft8ArmedSend Armed, FakeSerialPort Port, FakeTransmitAudioSink Sink)
        Armed()
    {
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink { Took = TimeSpan.Zero };

        return (new Ft8ArmedSend(new Ft8TransmitSequence(port, sink)), port, sink);
    }

    /// <summary>One FT4 send, armed for a stated FT4 boundary.</summary>
    private static OperatorSend SendAt(
        DateTime slotStartUtc,
        string text = Message,
        LicenseClass licenseClass = LicenseClass.General)
    {
        var composed = Ft4Composer.ComposeSignal(text);

        Assert.True(composed.Composed, composed.Explanation);

        return new OperatorSend(
            composed.Transmission!,
            Ft4On20m,
            licenseClass,
            true,
            slotStartUtc,
            0.5)
        {
            Grid = SlotGrid.Ft4,
        };
    }
}

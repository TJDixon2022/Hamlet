using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Tests.Rig;
using Hamlet.RadioEngine.Transmit;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **One click, one message - and the second boundary sends nothing.**
/// </summary>
/// <remarks>
/// <para>**THIS IS THE PHASE'S ONE UNRECOVERABLE FAULT, CAUGHT WHERE IT COSTS
/// NOTHING.** A transmission the operator did not ask for goes out over other
/// people's band and cannot be taken back. It was watched happening here, on a
/// fake port, before it was made impossible.</para>
/// <para>**THE BREAKAGE THESE WOULD HAVE CAUGHT**, and it was watched: an armed
/// send that is not cleared once it has fired. Two slot boundaries were driven
/// past one click and **two transmissions went out** - the wire took a second
/// <c>FE FE 94 E0 1C 00 01 FD</c> and the sink was handed a second slot of
/// samples, with nobody having clicked anything. `Expected: 1, Actual: 2`.
/// </para>
/// <para>**NOTHING HERE OPENS A DEVICE OR A PORT.** <see cref="FakeSerialPort"/>
/// and <see cref="FakeTransmitAudioSink"/>, both already in the tree. No radio
/// has ever been attached to this machine (SHACK_FACTS.md FACT-004), so criterion
/// 3 is met **on the fake wire** - the keying frame, the samples, the unkey and
/// the recorded slot being the next boundary - and the last mile to a real radio
/// is step 6's and Tim's.</para>
/// </remarks>
public sealed class OneClickSendsExactlyOneMessageTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the wire is quoted.</param>
    public OneClickSendsExactlyOneMessageTests(ITestOutputHelper output) =>
        _output = output;

    /// <summary>A slot boundary in the middle of a minute, and the next one.</summary>
    private static readonly DateTime Boundary =
        new(2026, 9, 6, 23, 45, 0, DateTimeKind.Utc);

    /// <summary>
    /// **After a send, nothing further transmits without another click.**
    /// </summary>
    /// <remarks>
    /// Step 5's criterion 4, and the must-pass. Two boundaries, one click, one
    /// transmission - counted on the bytes the port took, not on a flag.
    /// </remarks>
    [Fact]
    public async Task TwoBoundariesAfterOneClickProduceOneTransmission()
    {
        var (armed, port, sink) = Armed();

        armed.Arm(SendAt(Boundary));

        var first = await armed.AtBoundaryAsync(Boundary);
        var second = await armed.AtBoundaryAsync(Boundary.AddSeconds(Ft8Slots.SlotSeconds));
        var third = await armed.AtBoundaryAsync(Boundary.AddSeconds(2 * Ft8Slots.SlotSeconds));

        _output.WriteLine("clicks              : 1");
        _output.WriteLine("boundaries driven   : 3");
        _output.WriteLine("boundary 1          : " + first.Outcome);
        _output.WriteLine("boundary 2          : " + second.Outcome);
        _output.WriteLine("boundary 3          : " + third.Outcome);
        _output.WriteLine("times the sink played: " + sink.TimesCalled);
        _output.WriteLine("wire                : "
            + TheUnkeyHappensWhateverGoesWrongTests.Hex(port.Written));

        Assert.Equal(Ft8ArmOutcome.Ran, first.Outcome);
        Assert.True(first.Run!.Sent, first.Run.Reason);

        // THE WHOLE POINT. Nothing was armed any more, so nothing went out.
        Assert.Equal(Ft8ArmOutcome.NothingArmed, second.Outcome);
        Assert.Equal(Ft8ArmOutcome.NothingArmed, third.Outcome);
        Assert.Null(second.Run);
        Assert.Null(third.Run);

        Assert.Equal(1, sink.TimesCalled);
        Assert.False(armed.IsArmed);

        // ONE KEY AND ONE UNKEY ON THE WIRE, AND NO SECOND PAIR.
        Assert.Equal(2, port.WritesAttempted);
    }

    /// <summary>**A second click replaces the armed send; it never adds one.**</summary>
    /// <remarks>
    /// The breakage: a queue instead of a field. Two clicks a second apart would
    /// then be two transmissions - the first in the slot the operator chose and
    /// the second in one he never asked about.
    /// </remarks>
    [Fact]
    public async Task ASecondClickReplacesTheArmedSendAndNeverAddsOne()
    {
        var (armed, port, sink) = Armed();

        armed.Arm(SendAt(Boundary, "N5TT KC3QIS -10"));
        armed.Arm(SendAt(Boundary, "N5TT KC3QIS R-10"));

        var first = await armed.AtBoundaryAsync(Boundary);
        var second = await armed.AtBoundaryAsync(Boundary.AddSeconds(Ft8Slots.SlotSeconds));

        _output.WriteLine("clicks               : 2, one second apart");
        _output.WriteLine("times the sink played: " + sink.TimesCalled);
        _output.WriteLine("wire                 : "
            + TheUnkeyHappensWhateverGoesWrongTests.Hex(port.Written));

        Assert.Equal(Ft8ArmOutcome.Ran, first.Outcome);
        Assert.Equal(Ft8ArmOutcome.NothingArmed, second.Outcome);
        Assert.Equal(1, sink.TimesCalled);
        Assert.Equal(2, port.WritesAttempted);
    }

    /// <summary>**A boundary before the one it was armed for sends nothing.**</summary>
    [Fact]
    public async Task ABoundaryEarlierThanTheArmedOneDoesNotTransmit()
    {
        var (armed, _, sink) = Armed();

        armed.Arm(SendAt(Boundary.AddSeconds(2 * Ft8Slots.SlotSeconds)));

        var early = await armed.AtBoundaryAsync(Boundary);

        _output.WriteLine("armed for : " + armed.Armed!.SlotStartUtc.ToString("O"));
        _output.WriteLine("boundary  : " + Boundary.ToString("O"));
        _output.WriteLine("outcome   : " + early.Outcome);

        Assert.Equal(Ft8ArmOutcome.NotDue, early.Outcome);
        Assert.True(sink.WasNeverTouched);
        Assert.True(armed.IsArmed);
    }

    /// <summary>
    /// **A boundary that has gone by discards the send rather than sending late.**
    /// </summary>
    /// <remarks>
    /// The breakage: a missed tick putting the operator's message out in a slot
    /// he did not choose. A transmission in the wrong slot is still a
    /// transmission he did not ask for.
    /// </remarks>
    [Fact]
    public async Task ABoundaryAfterTheArmedOneDiscardsItRatherThanSendingLate()
    {
        var (armed, port, sink) = Armed();

        armed.Arm(SendAt(Boundary));

        var late = await armed.AtBoundaryAsync(
            Boundary.AddSeconds(2 * Ft8Slots.SlotSeconds));

        _output.WriteLine("armed for : " + Boundary.ToString("O"));
        _output.WriteLine("boundary  : "
            + Boundary.AddSeconds(2 * Ft8Slots.SlotSeconds).ToString("O"));
        _output.WriteLine("outcome   : " + late.Outcome);
        _output.WriteLine("wire      : "
            + TheUnkeyHappensWhateverGoesWrongTests.Hex(port.Written));

        Assert.Equal(Ft8ArmOutcome.TooLate, late.Outcome);
        Assert.True(sink.WasNeverTouched);
        Assert.Empty(port.Written);
        Assert.False(armed.IsArmed);
    }

    /// <summary>**An armed send that has not keyed can be cancelled.**</summary>
    /// <remarks>
    /// A flag cleared on the calling thread with nothing awaited. Once it has
    /// keyed the route out is the sequence's own abort, and nothing is added
    /// beside it.
    /// </remarks>
    [Fact]
    public async Task AnArmedSendThatHasNotKeyedCanBeCancelled()
    {
        var (armed, port, sink) = Armed();

        armed.Arm(SendAt(Boundary));

        Assert.True(armed.Cancel());
        Assert.False(armed.Cancel());

        var boundary = await armed.AtBoundaryAsync(Boundary);

        _output.WriteLine("outcome after cancel: " + boundary.Outcome);

        Assert.Equal(Ft8ArmOutcome.NothingArmed, boundary.Outcome);
        Assert.True(sink.WasNeverTouched);
        Assert.Empty(port.Written);
    }

    /// <summary>
    /// **Out of licence privileges: not a byte reaches the port and the sink is
    /// never touched.**
    /// </summary>
    /// <remarks>
    /// Step 5's criterion 6, taken at the gate inside
    /// <see cref="Ft8TransmitSequence.RunAsync"/>, which is where it already
    /// lives. No second copy of the licence rule is written anywhere in the send
    /// path.
    /// </remarks>
    [Fact]
    public async Task OutOfPrivilegesNothingReachesThePortOrTheSink()
    {
        var (armed, port, sink) = Armed();

        // 14.074 MHz is FT8's own watering hole and is outside a Technician's
        // HF phone-and-data privileges.
        armed.Arm(SendAt(Boundary, licenseClass: LicenseClass.Technician));

        var boundary = await armed.AtBoundaryAsync(Boundary);

        _output.WriteLine("outcome  : " + boundary.Run!.Outcome);
        _output.WriteLine("reason   : " + boundary.Run.Reason);
        _output.WriteLine("citation : " + boundary.Run.Citation);
        _output.WriteLine("bytes at the port : " + port.Written.Length);
        _output.WriteLine("sink touched      : " + !sink.WasNeverTouched);

        Assert.Equal(Ft8TransmitOutcome.RefusedByLicence, boundary.Run.Outcome);
        Assert.False(boundary.Run.Sent);
        Assert.False(boundary.Run.Keyed);

        Assert.Empty(port.Written);
        Assert.True(sink.WasNeverTouched);

        Assert.NotEmpty(boundary.Run.Reason);
        Assert.NotEmpty(boundary.Run.Citation);
    }

    /// <summary>
    /// **The slot recorded is the boundary it was armed for.**
    /// </summary>
    /// <remarks>
    /// Criterion 3's *in the next slot*, on the fake wire: the send carries the
    /// boundary and the 0.5 s offset unit 255 recorded, and the sequence took
    /// exactly that.
    /// </remarks>
    [Fact]
    public async Task TheSlotItWentInIsTheBoundaryItWasArmedFor()
    {
        var (armed, _, sink) = Armed();

        var next = Ft8Slots.SlotStart(Boundary.AddSeconds(1))
            .AddSeconds(Ft8Slots.SlotSeconds);

        armed.Arm(SendAt(next));

        var early = await armed.AtBoundaryAsync(Boundary);
        var due = await armed.AtBoundaryAsync(next);

        _output.WriteLine("armed for   : " + next.ToString("O"));
        _output.WriteLine("this slot   : " + early.Outcome);
        _output.WriteLine("next slot   : " + due.Outcome);
        _output.WriteLine("offset      : " + due.Send!.StartSecondsIntoSlot + " s");
        _output.WriteLine("samples     : " + sink.SamplesHandedOver
            + " at " + sink.RateAskedFor + " Hz");

        Assert.Equal(Ft8ArmOutcome.NotDue, early.Outcome);
        Assert.Equal(Ft8ArmOutcome.Ran, due.Outcome);
        Assert.Equal(next, due.Send!.SlotStartUtc);
        Assert.Equal(0.5, due.Send.StartSecondsIntoSlot);
        Assert.True(Ft8Slots.TransmissionFits(
            Ft8Slots.SlotSeconds - due.Send.StartSecondsIntoSlot));
    }

    /// <summary>
    /// **Nothing in the armed send can start a transmission on its own.**
    /// </summary>
    /// <remarks>
    /// The same file-read <c>NothingInTheSequenceCanStartATransmissionOnItsOwn</c>
    /// does of the sequence. A clock read, a wait, a schedule or a repetition is a
    /// way for this code to decide the moment has come, and the moment is the
    /// operator's alone. Doc comments are stripped first, so the words may be
    /// discussed and may not be used.
    /// </remarks>
    [Fact]
    public void NothingInTheArmedSendCanStartATransmissionOnItsOwn()
    {
        var path = Path.Combine(
            TheUnkeyHappensWhateverGoesWrongTests.RepositoryRoot(),
            "src", "Hamlet.RadioEngine", "Transmit", "Ft8ArmedSend.cs");

        var body = TheUnkeyHappensWhateverGoesWrongTests.CodeOnly(File.ReadAllText(path));

        string[] forbidden =
        [
            "Timer", "Delay", "Interval", "Elapsed", "Schedul", "Periodic", "Recurring",
            "Sleep", "Stopwatch", "TickCount", "DateTime.UtcNow", "DateTime.Now",
            "Environment.Tick", "PeriodicTimer", "OnTick", "AutoCall", "Repeat",
        ];

        var found = forbidden
            .Where(name => body.Contains(name, StringComparison.Ordinal))
            .ToList();

        _output.WriteLine("armed send     : " + path);
        _output.WriteLine("patterns tried : " + forbidden.Length);
        _output.WriteLine("found in code  : "
            + (found.Count == 0 ? "none" : string.Join(", ", found)));

        Assert.Empty(found);

        // AND ONE WAY IN. Arm is the only member that writes the armed field.
        var writes = body.Split('\n')
            .Where(line => line.Contains("_armed =", StringComparison.Ordinal))
            .Select(line => line.Trim())
            .ToList();

        foreach (var write in writes)
        {
            _output.WriteLine("writes the field: " + write);
        }

        // Arm sets it; Cancel and AtBoundaryAsync clear it. Nothing else.
        Assert.Equal(3, writes.Count);
        Assert.Single(writes, line => !line.Contains("null", StringComparison.Ordinal));
    }

    /// <summary>An armed send over the two fakes, with the wire and the sink.</summary>
    private static (Ft8ArmedSend Armed, FakeSerialPort Port, FakeTransmitAudioSink Sink)
        Armed()
    {
        var port = new FakeSerialPort();
        var sink = new FakeTransmitAudioSink();

        return (new Ft8ArmedSend(new Ft8TransmitSequence(port, sink)), port, sink);
    }

    /// <summary>One send, armed for a stated boundary.</summary>
    private static OperatorSend SendAt(
        DateTime slotStartUtc,
        string text = "W1ABC KC3QIS -10",
        LicenseClass licenseClass = LicenseClass.General)
    {
        var composed = Ft8Composer.ComposeSignal(text);

        Assert.True(composed.Composed, composed.Explanation);

        return new OperatorSend(
            composed.Transmission!,
            14_074_000,
            licenseClass,
            true,
            slotStartUtc,
            0.5);
    }
}

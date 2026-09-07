using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Transmit;
using Hamlet.RadioEngine.Transport;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// **The operator can stop it - from every state a transmission can be in.**
/// </summary>
/// <remarks>
/// <para>**WHAT THIS EXISTS FOR.** Until work instruction 261,
/// <c>TransmitAbort.Fire</c> had exactly one call site in <c>src/</c> and it was
/// inside <see cref="Ft8TransmitSequence"/>'s own <c>catch</c>, and
/// <see cref="Ft8ArmedSend.Cancel"/> had none at all. So the abort was reachable
/// only by something happening to throw, which is an abort *conditional on a
/// fault* - and `PHASE_PLAN.md` step 1 says it cannot be disabled, deferred, or
/// made conditional.</para>
/// <para>**THE RED THAT WAS WATCHED FIRST**, and it is the one that costs an
/// operator a transmission he did not want on other people's band:
/// <see cref="StopNowUnarmsAndDoesNotAbort"/> is that defect, kept as a live
/// contrast. Built that way, the stop returns *un-armed*, the operator reads a
/// line saying it stopped, and the wire shows
/// <c>FE FE 94 E0 1C 00 01 FD</c> with nothing after it - **a keyed radio
/// staying keyed.** During a transmission <c>_armed</c> is already null
/// (<c>Ft8ArmedSend.cs:148</c> clears it before the await), so an un-arm-only
/// stop has nothing to find and does nothing at all.</para>
/// <para>**NO DEVICE, NO PORT, NO SOUND** (`SHACK_FACTS.md` FACT-004). Two fakes:
/// <see cref="StagedPort"/>, which lets the stop land at a chosen point on the
/// wire, and <see cref="ParkingSink"/>, which holds a transmission open in memory
/// for as long as a test needs and plays nothing.</para>
/// </remarks>
public sealed class TheOperatorsStopFiresFromEveryStateTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fixture.</summary>
    /// <param name="output">Where the wire is quoted.</param>
    public TheOperatorsStopFiresFromEveryStateTests(ITestOutputHelper output) =>
        _output = output;

    /// <summary>A slot boundary in the middle of a minute.</summary>
    private static readonly DateTime Boundary =
        new(2026, 9, 6, 23, 45, 0, DateTimeKind.Utc);

    /// <summary>What the radio sees when it is keyed.</summary>
    private const string KeyOn = "FE FE 94 E0 1C 00 01 FD";

    /// <summary>The abort's first frame: stop whatever the keyer is sending.</summary>
    private const string CwStop = "FE FE 94 E0 17 FF FD";

    /// <summary>The abort's second frame, and the ordinary unkey: back to receive.</summary>
    private const string PttOff = "FE FE 94 E0 1C 00 00 FD";

    // ---- state 1: nothing armed ------------------------------------------

    /// <summary>
    /// **Nothing armed and no radio: it says so, and not a byte is written.**
    /// </summary>
    /// <remarks>
    /// The honest floor. There is nothing to un-arm and no wire to say anything
    /// on, and the result is <see cref="Ft8StopOutcome.NothingToStop"/> rather
    /// than a claim that something was stopped.
    /// </remarks>
    [Fact]
    public void NothingArmedAndNoRadioIsNothingToStop()
    {
        var (armed, port, sink) = Armed();

        var stop = armed.StopNow(null);

        Report("nothing armed, no port", stop, port);

        Assert.Equal(Ft8StopOutcome.NothingToStop, stop.Outcome);
        Assert.False(stop.Unarmed);
        Assert.Null(stop.Abort);
        Assert.False(stop.AnythingReachedTheRadio);
        Assert.Empty(port.Frames);
        Assert.True(sink.WasNeverTouched);
    }

    /// <summary>
    /// **Nothing armed, but a radio is connected: the frames go out anyway.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE DELIBERATE PART AND IT IS WHERE THE INSTRUCTION AND
    /// THE RULE MEET.** Work instruction 261's task 3 table says *nothing armed -
    /// say so, write nothing*, and its *What not to do* item 6 says the stop may
    /// not be conditional on a state flag or on whether the application believes
    /// it is transmitting. The two cannot both be honoured, because **"nothing
    /// armed" and "keyed and transmitting" are the same field value**:
    /// <c>Ft8ArmedSend.cs:148</c> clears <c>_armed</c> before awaiting
    /// <c>RunAsync</c>. A stop that wrote nothing when nothing was armed would
    /// write nothing for the whole 12.64 seconds the radio is keyed, which is
    /// exactly the defect <see cref="StopNowUnarmsAndDoesNotAbort"/> demonstrates.
    /// The rule wins, and the frames cost nothing.</para>
    /// <para>**AND THEY REALLY DO COST NOTHING.** <c>1C 00 00</c> is *be in
    /// receive* - an absolute state, not a toggle - so a radio already in receive
    /// stays there and answers OK. <c>17 FF</c> is the stop code for a keyer
    /// message the radio is sending itself, which a radio sending none has
    /// nothing to apply it to. That is `TransmitAbort`'s own stated reasoning and
    /// `docs/unit261-stop-trace.md` Q5 argues it from the bytes.</para>
    /// </remarks>
    [Fact]
    public void NothingArmedWithARadioStillTellsTheRadio()
    {
        var (armed, port, _) = Armed();

        var stop = armed.StopNow(port);

        Report("nothing armed, port present", stop, port);

        Assert.Equal(Ft8StopOutcome.ToldTheRadio, stop.Outcome);
        Assert.False(stop.Unarmed);
        Assert.True(stop.AnythingReachedTheRadio);
        Assert.Equal(new[] { CwStop, PttOff }, Wire(port));
    }

    // ---- state 2: armed, boundary not yet arrived -------------------------

    /// <summary>
    /// **Armed and not yet keyed: it is un-armed, and the boundary then finds
    /// nothing.**
    /// </summary>
    /// <remarks>
    /// The half an operator reaches for most - he right-clicked the wrong station
    /// and has up to fifteen seconds to think better of it. **The proof is not
    /// that a flag changed**, it is that the boundary afterwards returns
    /// <see cref="Ft8ArmOutcome.NothingArmed"/>, the sink was never touched, and
    /// no keying frame is anywhere on the wire.
    /// </remarks>
    [Fact]
    public async Task ArmedBeforeTheBoundaryIsUnarmedAndTheBoundaryFindsNothing()
    {
        var (armed, port, sink) = Armed();

        armed.Arm(SendAt(Boundary));

        var stop = armed.StopNow(port);
        var boundary = await armed.AtBoundaryAsync(Boundary);

        Report("armed, boundary not yet arrived", stop, port);
        _output.WriteLine("boundary afterwards : " + boundary.Outcome);

        Assert.Equal(Ft8StopOutcome.UnarmedAndToldTheRadio, stop.Outcome);
        Assert.True(stop.Unarmed);
        Assert.True(stop.AnythingReachedTheRadio);

        // THE ONE THAT MATTERS. Nothing was armed when the slot came round.
        Assert.Equal(Ft8ArmOutcome.NothingArmed, boundary.Outcome);
        Assert.Null(boundary.Run);
        Assert.True(sink.WasNeverTouched);

        // Two frames, and neither of them keys anything.
        Assert.Equal(new[] { CwStop, PttOff }, Wire(port));
        Assert.DoesNotContain(KeyOn, Wire(port));
    }

    // ---- state 3: about to key -------------------------------------------

    /// <summary>
    /// **The stop lands in the gap between deciding to key and the frame
    /// arriving: the abort still fires.**
    /// </summary>
    /// <remarks>
    /// The narrowest window there is, and the one where a state flag would be
    /// wrong in both directions: the sequence has committed to transmitting and
    /// the radio has not heard about it yet. <see cref="StagedPort"/> runs the
    /// stop from inside the keying <c>WriteAsync</c>, before that frame is
    /// recorded, so the order on the wire is exactly what a radio would have
    /// seen.
    /// </remarks>
    [Fact]
    public async Task AboutToKeyTheAbortStillFires()
    {
        var (armed, port, sink) = Armed();

        Ft8StopResult? stop = null;
        port.WhenAboutToWrite = frame =>
        {
            if (Hex(frame) == KeyOn)
            {
                stop = armed.StopNow(port);
            }
        };

        armed.Arm(SendAt(Boundary));
        var boundary = await armed.AtBoundaryAsync(Boundary);

        Report("about to key", stop!, port);
        _output.WriteLine("the run said       : " + boundary.Run!.Outcome);

        Assert.NotNull(stop);

        // UNIT 263 CHANGED WHAT THIS VALUE MEANS, AND THE BYTES BELOW ARE
        // UNCHANGED. The stop lands after the boundary has committed to a
        // transmission - the cancellation source is installed under the lock
        // before anything keys - so it now reports that it stopped a transmission
        // as well as telling the radio, where before there was nothing it could
        // have done about the audio and every mid-slot state read alike.
        Assert.Equal(Ft8StopOutcome.StoppedTheTransmissionAndToldTheRadio, stop!.Outcome);
        Assert.True(stop.AudioToldToStop);
        Assert.True(stop.AnythingReachedTheRadio);

        // The abort's two frames reached the radio BEFORE the keying frame did.
        Assert.Equal(new[] { CwStop, PttOff, KeyOn, PttOff }, Wire(port));
        Assert.Equal(1, sink.TimesCalled);
    }

    // ---- state 4: keyed, mid-transmission --------------------------------

    /// <summary>
    /// **The radio is keyed and playing, the operator presses stop, and both
    /// frames go out while the transmission is still running.**
    /// </summary>
    /// <remarks>
    /// <para>**THE ROW THE WHOLE UNIT IS FOR.** The transmission is parked inside
    /// the sink - keyed, mid-audio, exactly where the 12.64 seconds are spent -
    /// and the stop is called from another thread while it is parked, which is
    /// what a click on a window does.</para>
    /// <para>**AND IT IS CALLED WITHOUT WAITING FOR THE TRANSMISSION.** The
    /// assertion that the abort's frames are on the wire *before the sink is
    /// released* is the whole property: an abort that landed after the audio
    /// finished would be indistinguishable from no abort at all.</para>
    /// </remarks>
    [Fact]
    public async Task KeyedMidTransmissionTheAbortFiresWhileItIsStillRunning()
    {
        var port = new StagedPort();
        var sink = new ParkingSink();
        var armed = new Ft8ArmedSend(new Ft8TransmitSequence(port, sink));

        armed.Arm(SendAt(Boundary));

        var running = armed.AtBoundaryAsync(Boundary);

        // The radio is keyed and the tones are going out.
        Assert.True(sink.Entered.Wait(TimeSpan.FromSeconds(10)), "the sink was never reached");
        Assert.Equal(new[] { KeyOn }, Wire(port));

        var stop = armed.StopNow(port);

        // MEASURED BEFORE THE SINK IS LET GO. The frames are on the wire while
        // the transmission is still inside PlayAsync.
        var wireWhileRunning = Wire(port);

        sink.Release();
        var boundary = await running;

        Report("keyed, mid-transmission", stop, port);
        _output.WriteLine("wire while running : " + string.Join(" | ", wireWhileRunning));
        _output.WriteLine("the run said       : " + boundary.Run!.Outcome);
        _output.WriteLine("came out of transmit: " + boundary.Run.CameOutOfTransmit);

        // UNIT 263 CHANGED WHAT THIS VALUE MEANS. This is the state the new value
        // exists for: a transmission in progress, told to stop, at a radio that
        // was told too. The bytes below are unchanged.
        Assert.Equal(Ft8StopOutcome.StoppedTheTransmissionAndToldTheRadio, stop.Outcome);
        Assert.True(stop.AudioToldToStop);
        Assert.True(stop.AnythingReachedTheRadio);

        // BOTH FRAMES ATTEMPTED, BOTH LANDED, WHILE THE RADIO WAS STILL KEYED.
        Assert.True(stop.Abort!.CwStop.Written);
        Assert.True(stop.Abort.PttOff.Written);
        Assert.Equal(new[] { KeyOn, CwStop, PttOff }, wireWhileRunning);

        // And the sequence's own finally then ran, redundantly and harmlessly.
        Assert.Equal(new[] { KeyOn, CwStop, PttOff, PttOff }, Wire(port));
    }

    /// <summary>
    /// **The defect, kept alive as a contrast: un-arming alone leaves the radio
    /// keyed.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE RED THAT WAS WATCHED FIRST**, pinned so that a later
    /// session cannot quietly reduce <c>StopNow</c> to a call to
    /// <see cref="Ft8ArmedSend.Cancel"/> and still have the suite pass. It runs
    /// the crippled stop against a keyed radio and asserts what it actually
    /// does: **nothing.** No frame, no un-arm - because during a transmission
    /// <c>_armed</c> is already null - and a transmitter still on the air.</para>
    /// <para>Read it beside
    /// <see cref="KeyedMidTransmissionTheAbortFiresWhileItIsStillRunning"/>: same
    /// setup, same moment, one line of difference, and the difference is whether
    /// the operator gets his transmitter back.</para>
    /// </remarks>
    [Fact]
    public async Task StopNowUnarmsAndDoesNotAbort()
    {
        var port = new StagedPort();
        var sink = new ParkingSink();
        var armed = new Ft8ArmedSend(new Ft8TransmitSequence(port, sink));

        armed.Arm(SendAt(Boundary));

        var running = armed.AtBoundaryAsync(Boundary);

        Assert.True(sink.Entered.Wait(TimeSpan.FromSeconds(10)), "the sink was never reached");

        // THE CRIPPLED STOP: un-arm only, which is all the tree could do before
        // this unit, because Cancel() was the only half that had a caller.
        var unarmed = armed.Cancel();
        var wireAfterTheCrippledStop = Wire(port);

        _output.WriteLine("state              : keyed, mid-transmission");
        _output.WriteLine("stop was           : un-arm only (the defect)");
        _output.WriteLine("it un-armed        : " + unarmed);
        _output.WriteLine("wire after the stop: "
            + string.Join(" | ", wireAfterTheCrippledStop));
        _output.WriteLine("radio is           : "
            + (wireAfterTheCrippledStop is [KeyOn] ? "STILL KEYED" : "back in receive"));

        // It found nothing to un-arm, because the boundary already took the send.
        Assert.False(unarmed);

        // AND THE RADIO IS STILL TRANSMITTING. One frame on the wire: PTT on.
        Assert.Equal(new[] { KeyOn }, wireAfterTheCrippledStop);

        sink.Release();
        await running;
    }

    // ---- state 5: waiting for unkey --------------------------------------

    /// <summary>
    /// **The audio is finished and the sequence is on its way to the unkey: the
    /// abort still fires.**
    /// </summary>
    /// <remarks>
    /// The last moment a radio can still be on the air, and the one an operator
    /// reaches at the end of a transmission he changed his mind about. The stop
    /// is run from inside the ordinary unkey's own write, before that frame is
    /// recorded, so the abort's two frames precede it on the wire.
    /// </remarks>
    [Fact]
    public async Task WaitingForTheUnkeyTheAbortStillFires()
    {
        var (armed, port, _) = Armed();

        Ft8StopResult? stop = null;
        port.WhenAboutToWrite = frame =>
        {
            if (Hex(frame) == PttOff)
            {
                stop = armed.StopNow(port);
            }
        };

        armed.Arm(SendAt(Boundary));
        var boundary = await armed.AtBoundaryAsync(Boundary);

        Report("waiting for unkey", stop!, port);
        _output.WriteLine("the run said       : " + boundary.Run!.Outcome);

        Assert.NotNull(stop);

        // UNIT 263 CHANGED WHAT THIS VALUE MEANS, and it is worth being exact
        // about why here: the audio has finished, but the run has not, so the
        // cancellation source is still installed and the stop cancels it. **What
        // the value claims is that the audio was told to stop, not that anything
        // was still playing** - Ft8StopResult.AudioToldToStop is named for that
        // distinction, and telling a finished transmission to stop costs a flag
        // write and changes nothing, exactly as firing the abort at a radio
        // already in receive does. The bytes below are unchanged.
        Assert.Equal(Ft8StopOutcome.StoppedTheTransmissionAndToldTheRadio, stop!.Outcome);
        Assert.True(stop.AnythingReachedTheRadio);
        Assert.Equal(new[] { KeyOn, CwStop, PttOff, PttOff }, Wire(port));
    }

    // ---- state 6: the port is dead ---------------------------------------

    /// <summary>
    /// **A port that throws on every write: the stop still returns, and it never
    /// throws.**
    /// </summary>
    /// <remarks>
    /// <para>The pulled USB lead, the disposed transport, the driver that has
    /// given up. **An abort that can throw is not an abort** - and one that
    /// reported success would be worse, because the operator would stop looking
    /// at the radio.</para>
    /// <para>Both frames are still *attempted*: the second is not reached for
    /// because the first failed, it is reached for because it is the other half
    /// of stopping.</para>
    /// </remarks>
    [Fact]
    public void ADeadPortStillReturnsAndStillNeverThrows()
    {
        var (armed, port, _) = Armed();

        armed.Arm(SendAt(Boundary));
        port.EveryWriteThrows = new IOException("the port is gone (scripted)");

        var stop = armed.StopNow(port);

        Report("port dead / gone / throwing", stop, port);
        _output.WriteLine("cw stop failure    : " + stop.Abort!.CwStop.Failure);
        _output.WriteLine("ptt off failure    : " + stop.Abort.PttOff.Failure);

        // It came back, with an answer, having thrown nothing.
        Assert.Equal(Ft8StopOutcome.UnarmedAndToldTheRadio, stop.Outcome);
        Assert.True(stop.Unarmed);

        // Both attempted, neither landed, and it says so rather than pretending.
        Assert.Equal(2, port.WritesAttempted);
        Assert.False(stop.Abort.CwStop.Written);
        Assert.False(stop.Abort.PttOff.Written);
        Assert.False(stop.AnythingReachedTheRadio);
        Assert.Empty(port.Frames);
    }

    /// <summary>
    /// **A disposed port is the same answer: no throw, and it says nothing
    /// landed.**
    /// </summary>
    [Fact]
    public void ADisposedPortIsNotACrash()
    {
        var (armed, port, _) = Armed();

        port.Dispose();

        var stop = armed.StopNow(port);

        Report("port disposed", stop, port);

        Assert.Equal(Ft8StopOutcome.ToldTheRadio, stop.Outcome);
        Assert.False(stop.AnythingReachedTheRadio);
    }

    // ---- the properties a later session could quietly remove -------------

    /// <summary>
    /// **THIS IS THE TEST THAT FAILS IF SOMEBODY PUTS A WAIT ON THE STOP.**
    /// </summary>
    /// <remarks>
    /// <para>Unit 253's instrument, pointed at the new entry point: reflection
    /// catches a compiler-generated state machine and a task-shaped return, and
    /// a scan of <c>StopNow</c>'s own body catches the keyword - including in a
    /// helper reflection would have to be told about. **The body is extracted by
    /// brace matching rather than the whole file being scanned**, because
    /// <c>AtBoundaryAsync</c> lives beside it and is legitimately
    /// <c>async</c>.</para>
    /// <para>**IT WAS WATCHED FIRING.** Built with a single
    /// <c>await Task.Yield();</c> at the top of <c>StopNow</c>, this failed on
    /// both instruments at once: the reflection arm on
    /// <c>StopNow returns a task</c>, and the source arm on <c>await </c>.</para>
    /// <para>**AND SINCE UNIT 263 IT COVERS EVERY LINE THE STOP REACHES, NOT JUST
    /// THE ONES IN <c>StopNow</c>.** The stop grew a second half - it cancels the
    /// running transmission's source as well as un-arming and firing the frames -
    /// and that half lives in the private <c>StopTheAudio</c>. A scan that stopped
    /// at <c>StopNow</c>'s own braces would have declared the stop wait-free while
    /// the line that actually touches a running transmission went unread, which is
    /// the shape of hole this whole file exists to close. <c>Cancel</c> is read
    /// too, because <c>StopNow</c> calls it and unit 261's *one field, one lock,
    /// one line* remark is the reason it may never grow one.</para>
    /// </remarks>
    [Fact]
    public void NothingOnTheStopPathWaitsForAnything()
    {
        var method = typeof(Ft8ArmedSend).GetMethod(
            nameof(Ft8ArmedSend.StopNow),
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.NotNull(method);
        Assert.Null(method!.GetCustomAttribute<AsyncStateMachineAttribute>());

        Assert.False(
            typeof(Task).IsAssignableFrom(method.ReturnType),
            $"{method.Name} returns a task");

        Assert.False(
            method.ReturnType == typeof(ValueTask)
            || (method.ReturnType.IsGenericType
                && method.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>)),
            $"{method.Name} returns a value task");

        // It hands back a plain record, so a caller cannot forget to await it.
        Assert.Equal(typeof(Ft8StopResult), method.ReturnType);

        // The seam it writes through returns void, and widening that shows here.
        var seam = typeof(ISerialPort).GetMethod("Write", new[] { typeof(ReadOnlySpan<byte>) });
        Assert.NotNull(seam);
        Assert.Equal(typeof(void), seam!.ReturnType);

        // EVERY MEMBER THE STOP REACHES, NOT JUST THE ONE IT ENTERS BY.
        string[] onThePath = ["StopNow", "StopTheAudio", "Cancel"];

        foreach (var name in onThePath)
        {
            var body = BodyOf(name);

            _output.WriteLine("---- " + name + ", comments stripped ----");
            _output.WriteLine(body);

            Assert.DoesNotContain("await ", body, StringComparison.Ordinal);
            Assert.DoesNotContain("async ", body, StringComparison.Ordinal);
            Assert.DoesNotContain("Task", body, StringComparison.Ordinal);
            Assert.DoesNotContain(".Wait(", body, StringComparison.Ordinal);
            Assert.DoesNotContain(".Result", body, StringComparison.Ordinal);
            Assert.DoesNotContain("GetAwaiter", body, StringComparison.Ordinal);
            Assert.DoesNotContain("Sleep", body, StringComparison.Ordinal);
            Assert.DoesNotContain("Delay", body, StringComparison.Ordinal);
            Assert.DoesNotContain("Join(", body, StringComparison.Ordinal);
        }

        // AND THE HELPER REALLY IS ON THE PATH. A scan of three members proves
        // nothing if the stop stopped calling one of them.
        Assert.Contains("StopTheAudio()", BodyOf("StopNow"), StringComparison.Ordinal);
        Assert.Contains("Cancel()", BodyOf("StopNow"), StringComparison.Ordinal);
    }

    /// <summary>
    /// **No flag turns it off, and there is no second way to un-arm.**
    /// </summary>
    /// <remarks>
    /// `PHASE_PLAN.md` step 1: the abort *cannot be disabled, deferred, or made
    /// conditional*. So <c>StopNow</c> takes no boolean, and its body calls
    /// <see cref="Ft8ArmedSend.Cancel"/> rather than touching <c>_armed</c> a
    /// second time - which also keeps
    /// <c>OneClickSendsExactlyOneMessageTests</c>'s count of three writes to that
    /// field intact.
    /// </remarks>
    [Fact]
    public void NoFlagCanTurnTheStopOffAndItAddsNoSecondWayToUnarm()
    {
        var method = typeof(Ft8ArmedSend).GetMethod(nameof(Ft8ArmedSend.StopNow))!;

        foreach (var parameter in method.GetParameters())
        {
            Assert.False(
                parameter.ParameterType == typeof(bool)
                || parameter.ParameterType == typeof(bool?),
                $"StopNow takes a switch called {parameter.Name}");
        }

        var body = StopNowBody();

        // It reuses Cancel and writes no field of its own.
        Assert.Contains("Cancel()", body, StringComparison.Ordinal);
        Assert.DoesNotContain("_armed", body, StringComparison.Ordinal);

        // The only branch in it is "is there a wire at all", which is an absence
        // and not a condition on state.
        var branches = body.Split('\n')
            .Where(line => line.Contains(" is null", StringComparison.Ordinal)
                || line.Contains("if (", StringComparison.Ordinal))
            .Select(line => line.Trim())
            .ToList();

        foreach (var branch in branches)
        {
            _output.WriteLine("branch: " + branch);
        }

        Assert.Single(branches);
        Assert.Contains("port is null", branches[0], StringComparison.Ordinal);
    }

    /// <summary>
    /// **The stop is a route out and never a route in.**
    /// </summary>
    /// <remarks>
    /// Task 5's property, asserted here on the source as well as counted by grep:
    /// nothing in <c>StopNow</c> arms, composes, keys or reaches the sequence.
    /// </remarks>
    [Fact]
    public void TheStopAddsNoRouteToATransmission()
    {
        var body = StopNowBody();

        string[] forbidden =
        [
            "Arm(", "_sequence", "RunAsync", "PttOn", "WriteAsync", "new OperatorSend",
        ];

        var found = forbidden
            .Where(name => body.Contains(name, StringComparison.Ordinal))
            .ToList();

        _output.WriteLine("patterns tried: " + forbidden.Length);
        _output.WriteLine("found in code : "
            + (found.Count == 0 ? "none" : string.Join(", ", found)));

        Assert.Empty(found);
    }

    // ---- helpers ---------------------------------------------------------

    /// <summary>What one state did, in the register the report quotes.</summary>
    private void Report(string state, Ft8StopResult stop, StagedPort port)
    {
        _output.WriteLine("state              : " + state);
        _output.WriteLine("outcome            : " + stop.Outcome);
        _output.WriteLine("un-armed           : " + stop.Unarmed);
        _output.WriteLine("reached the radio  : " + stop.AnythingReachedTheRadio);
        _output.WriteLine("frames attempted   : " + port.WritesAttempted);
        _output.WriteLine("wire               : "
            + (port.Frames.Count == 0 ? "(nothing)" : string.Join(" | ", Wire(port))));
    }

    /// <summary>Every frame the port took, as hex, in order.</summary>
    private static string[] Wire(StagedPort port)
        => port.Frames.Select(Hex).ToArray();

    /// <summary>Bytes as the record shows them.</summary>
    private static string Hex(IEnumerable<byte> bytes)
        => string.Join(' ', bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));

    /// <summary>
    /// <c>StopNow</c>'s own body, comment lines removed, by brace matching.
    /// </summary>
    /// <remarks>
    /// **THE WHOLE FILE WOULD BE THE WRONG THING TO SCAN.**
    /// <see cref="Ft8ArmedSend.AtBoundaryAsync"/> is legitimately <c>async</c>
    /// and sits ten lines away; a file-wide scan for <c>await</c> would either
    /// fail forever or have to be weakened until it caught nothing.
    /// </remarks>
    private static string StopNowBody() => BodyOf("StopNow");

    /// <summary>
    /// One named member of <see cref="Ft8ArmedSend"/>, comment lines removed, by
    /// brace matching.
    /// </summary>
    /// <param name="member">The member's name, as it is declared.</param>
    /// <returns>Its declaration and body, as source.</returns>
    /// <remarks>
    /// <para>**IT FINDS THE DECLARATION AND NOT A CALL.** <c>Cancel()</c> and
    /// <c>StopTheAudio()</c> both appear inside <c>StopNow</c>, so a search for
    /// the bare name would extract the wrong braces; the declaring line is the one
    /// whose first word is an access modifier.</para>
    /// <para>**IT TOOK A NAME FROM UNIT 263 ONWARD** because the stop grew a
    /// second half. Scanning only the method the operator enters by would leave
    /// the line that touches a running transmission unread.</para>
    /// </remarks>
    private static string BodyOf(string member)
    {
        var path = Path.Combine(
            TheUnkeyHappensWhateverGoesWrongTests.RepositoryRoot(),
            "src", "Hamlet.RadioEngine", "Transmit", "Ft8ArmedSend.cs");

        Assert.True(File.Exists(path), $"the armed send's source is not at {path}");

        var source = TheUnkeyHappensWhateverGoesWrongTests.CodeOnly(File.ReadAllText(path));

        var start = -1;

        for (var at = source.IndexOf(member + "(", StringComparison.Ordinal);
             at >= 0;
             at = source.IndexOf(member + "(", at + 1, StringComparison.Ordinal))
        {
            var lineStart = source.LastIndexOf('\n', at) + 1;
            var line = source[lineStart..at].TrimStart();

            if (line.StartsWith("public ", StringComparison.Ordinal)
                || line.StartsWith("private ", StringComparison.Ordinal)
                || line.StartsWith("internal ", StringComparison.Ordinal)
                || line.StartsWith("protected ", StringComparison.Ordinal))
            {
                start = lineStart;
                break;
            }
        }

        Assert.True(start >= 0, $"{member} is not declared in the source");

        var open = source.IndexOf('{', start);
        Assert.True(open > start, $"{member} has no body");

        var depth = 0;
        var end = open;

        for (; end < source.Length; end++)
        {
            if (source[end] == '{')
            {
                depth++;
            }
            else if (source[end] == '}' && --depth == 0)
            {
                break;
            }
        }

        return source[start..(end + 1)];
    }

    /// <summary>An armed send over the staged port and a sink that returns.</summary>
    private static (Ft8ArmedSend Armed, StagedPort Port, FakeTransmitAudioSink Sink) Armed()
    {
        var port = new StagedPort();
        var sink = new FakeTransmitAudioSink();

        return (new Ft8ArmedSend(new Ft8TransmitSequence(port, sink)), port, sink);
    }

    /// <summary>One send, armed for a stated boundary.</summary>
    private static OperatorSend SendAt(DateTime slotStartUtc)
    {
        var composed = Ft8Composer.ComposeSignal("W1ABC KC3QIS -10");

        Assert.True(composed.Composed, composed.Explanation);

        return new OperatorSend(
            composed.Transmission!, 14_074_000, LicenseClass.General, true, slotStartUtc, 0.5);
    }

    // ---- the fakes -------------------------------------------------------

    /// <summary>
    /// A CI-V transport that keeps whole frames and can hand control to a test
    /// at a chosen point on the wire.
    /// </summary>
    /// <remarks>
    /// <para>**NOTHING HERE OPENS A PORT** (`SHACK_FACTS.md` FACT-004).</para>
    /// <para><see cref="WhenAboutToWrite"/> runs only on the asynchronous path,
    /// which is the one the sequence uses. **The abort's synchronous
    /// <see cref="Write"/> does not trigger it**, or a stop fired from inside the
    /// hook would call itself forever.</para>
    /// </remarks>
    private sealed class StagedPort : ISerialPort
    {
        private readonly List<byte[]> _frames = [];
        private readonly object _gate = new();
        private bool _inHook;

        /// <summary>Every frame taken, whole and in order.</summary>
        public IReadOnlyList<byte[]> Frames
        {
            get { lock (_gate) { return _frames.ToArray(); } }
        }

        /// <summary>How many writes were attempted, thrown or not.</summary>
        public int WritesAttempted { get; private set; }

        /// <summary>What every write throws, or null where the port is well.</summary>
        public Exception? EveryWriteThrows { get; set; }

        /// <summary>
        /// Run just before an asynchronous frame is recorded, so a test can land
        /// a stop at that exact point on the wire.
        /// </summary>
        public Action<byte[]>? WhenAboutToWrite { get; set; }

        /// <inheritdoc/>
        public bool IsOpen { get; private set; } = true;

        /// <inheritdoc/>
        public string PortName => "COM-STOP";

        /// <inheritdoc/>
        public int BaudRate => 115_200;

        /// <inheritdoc/>
        public void Open() => IsOpen = true;

        /// <inheritdoc/>
        public void Close() => IsOpen = false;

        /// <inheritdoc/>
        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
            => ValueTask.FromResult(0);

        /// <inheritdoc/>
        public ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            var frame = buffer.ToArray();

            if (WhenAboutToWrite is not null && !_inHook)
            {
                _inHook = true;

                try
                {
                    WhenAboutToWrite(frame);
                }
                finally
                {
                    _inHook = false;
                }
            }

            Take(frame);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public void Write(ReadOnlySpan<byte> buffer) => Take(buffer.ToArray());

        /// <inheritdoc/>
        public void Dispose()
        {
            IsOpen = false;
            EveryWriteThrows ??= new ObjectDisposedException(nameof(StagedPort));
        }

        /// <summary>Count it, refuse it where scripted, and otherwise keep it.</summary>
        private void Take(byte[] frame)
        {
            lock (_gate)
            {
                WritesAttempted++;

                if (EveryWriteThrows is not null)
                {
                    // The bytes of a refused write never reach Frames, because a
                    // port that threw did not take them.
                    throw EveryWriteThrows;
                }

                _frames.Add(frame);
            }
        }
    }

    /// <summary>
    /// A sink that holds a transmission open until a test lets it go, and plays
    /// nothing.
    /// </summary>
    /// <remarks>
    /// **THIS IS HOW "KEYED, MID-TRANSMISSION" IS REACHED WITHOUT WAITING 12.64
    /// SECONDS OR OPENING A DEVICE.** The radio is keyed, the samples have been
    /// handed over, and the sequence is parked exactly where it spends the
    /// transmission - which is the moment the operator reaches for the stop.
    /// </remarks>
    private sealed class ParkingSink : ITransmitAudioSink
    {
        private readonly ManualResetEventSlim _release = new(false);

        /// <summary>Set once something is actually inside the play.</summary>
        public ManualResetEventSlim Entered { get; } = new(false);

        /// <summary>Let the parked transmission finish.</summary>
        public void Release() => _release.Set();

        /// <inheritdoc/>
        /// <remarks>
        /// The decoder's rate, which is what these tests compose at. **This sink
        /// refuses nothing** - what it stands for is a transmission in progress,
        /// not an endpoint.
        /// </remarks>
        public int EndpointSampleRate => Ft8Composer.DefaultSampleRate;

        /// <inheritdoc/>
        public async Task<PlayedAudio> PlayAsync(
            ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
        {
            var count = samples.Length;

            return await Task.Run(
                () =>
                {
                    Entered.Set();

                    Assert.True(
                        _release.Wait(TimeSpan.FromSeconds(30)),
                        "the parked transmission was never released");

                    return new PlayedAudio(count, TimeSpan.FromSeconds(12.64));
                },
                CancellationToken.None).ConfigureAwait(false);
        }
    }
}

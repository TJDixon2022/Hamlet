using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Transport;

namespace Hamlet.RadioEngine.Transmit;

/// <summary>What a slot boundary did about the armed send.</summary>
public enum Ft8ArmOutcome
{
    /// <summary>Nothing was armed, so nothing happened.</summary>
    NothingArmed,

    /// <summary>Something is armed, for a later boundary than this one.</summary>
    NotDue,

    /// <summary>
    /// The boundary it was armed for has gone by. **It is discarded, not sent
    /// late.**
    /// </summary>
    TooLate,

    /// <summary>It was its boundary, and the sequence ran.</summary>
    Ran,
}

/// <summary>What one slot boundary did.</summary>
/// <param name="Outcome">Which of the four.</param>
/// <param name="Send">What was armed, or null where nothing was.</param>
/// <param name="Run">What the sequence did, or null where it was not reached.</param>
public sealed record Ft8BoundaryResult(
    Ft8ArmOutcome Outcome, OperatorSend? Send, TransmitRun? Run);

/// <summary>What the operator's stop actually stopped.</summary>
/// <remarks>
/// **A STOP THAT CANNOT SAY WHAT IT STOPPED IS THE DEFECT UNIT 253 FOUND IN
/// <c>Ic7300Rig.AbortCw</c>**, where a failed abort and a successful one left the
/// same trace, which was nothing (§0.0.1). These four are distinguishable at the
/// call site and each one is a different sentence to put in front of an operator.
/// </remarks>
public enum Ft8StopOutcome
{
    /// <summary>
    /// Nothing was armed and there was no port to say anything on. **Nothing
    /// happened and nothing is claimed to have happened.**
    /// </summary>
    NothingToStop,

    /// <summary>
    /// Something was armed and now is not, and there was no port to tell.
    /// Nothing was ever going to key, so nothing needed telling.
    /// </summary>
    Unarmed,

    /// <summary>
    /// Nothing was armed, and the abort's frames went at the port anyway.
    /// **That is not a mistake** - see <see cref="Ft8ArmedSend.StopNow"/>.
    /// </summary>
    ToldTheRadio,

    /// <summary>Both: an armed send was taken away *and* the radio was told.</summary>
    UnarmedAndToldTheRadio,

    /// <summary>
    /// A transmission was going out and was told to stop, and there was no port
    /// to tell as well.
    /// </summary>
    /// <remarks>
    /// **THE SIXTH STATE, AND IT DID NOT EXIST UNTIL UNIT 263** because there was
    /// nothing that could stop a transmission's audio. Everything mid-slot read as
    /// <see cref="ToldTheRadio"/>, which was true and was half the story: the
    /// carrier came off and Hamlet went on feeding the radio for the rest of the
    /// 12.64 seconds.
    /// </remarks>
    StoppedTheTransmission,

    /// <summary>
    /// **Both halves of a transmission in progress**: the audio was told to stop
    /// and the radio was told to stop.
    /// </summary>
    /// <remarks>
    /// The state an operator who presses stop mid-slot is actually in, and the one
    /// this unit exists to be able to report. It is distinguished from
    /// <see cref="ToldTheRadio"/> precisely because those two used to be the same
    /// value while being very different situations.
    /// </remarks>
    StoppedTheTransmissionAndToldTheRadio,
}

/// <summary>What one press of the operator's stop did, in full.</summary>
/// <param name="Unarmed">
/// True where a send was armed when the stop arrived and is not armed now.
/// **Nothing it was carrying will ever go out**: the field is the only thing a
/// boundary reads.
/// </param>
/// <param name="Abort">
/// What <see cref="TransmitAbort.Fire"/> did - both frames and what became of
/// each - or null where there was no port to fire it at. **Carried rather than
/// reduced to a boolean**, because the operator's next question after "did it
/// stop" is "did anything reach the radio".
/// </param>
/// <param name="AudioToldToStop">
/// True where a transmission was in progress and its audio was told to stop.
/// **"Told to", not "stopped"** (§0.0): the sink polls the flag at the top of
/// its own loop, on its own thread, so what this can honestly claim is that the
/// message was sent and not that the last sample has left the card. How long
/// that takes is measured rather than assumed - 15 ms against the fake at real
/// time, and <c>WasapiTransmitSink</c>'s own arithmetic in
/// <c>docs/unit263-stop-audio-trace.md</c> Q3 for the endpoint.
/// </param>
/// <remarks>
/// <para>**THREE FACTS, NOT TWO** (work instruction 263, task 4). Until this unit
/// the stop reported what it had un-armed and what its frames did, and said
/// nothing at all about the sound - because it did nothing at all about the
/// sound. The operator read *"Stopped"* while Hamlet went on feeding the rest of
/// a 12.64 second transmission into a radio it had just unkeyed, and **a sentence
/// that says "stopped" when only half of it stopped is the failure this record
/// exists to prevent** (§0.0.1).</para>
/// <para>**FALSE IS NOT THE SAME AS "FAILED".** <see cref="AudioToldToStop"/> is
/// false both when nothing was playing - the ordinary case, an operator changing
/// his mind before the boundary - and when the cancel itself would not take.
/// <see cref="Outcome"/> tells those apart by reading it beside
/// <see cref="Unarmed"/>.</para>
/// </remarks>
public sealed record Ft8StopResult(bool Unarmed, AbortRecord? Abort, bool AudioToldToStop)
{
    /// <summary>Which of the six this was.</summary>
    /// <remarks>
    /// <para>**THE AUDIO IS READ FIRST, BECAUSE IT IS THE ONE THAT MEANS THE
    /// RADIO WAS ON THE AIR.** A stop that found a transmission running is a
    /// different event from one that found a send waiting for its boundary, and
    /// until unit 263 they were the same value - during a transmission
    /// <c>_armed</c> is already null, so a mid-slot stop reported itself as
    /// <see cref="Ft8StopOutcome.ToldTheRadio"/>, indistinguishable from a stop
    /// pressed at an idle radio.</para>
    /// <para>**<see cref="Unarmed"/> AND <see cref="AudioToldToStop"/> ARE NEVER
    /// BOTH TRUE IN THE SHIPPED TREE** - the field is cleared under the lock
    /// before anything keys - and the discards above say what would be reported if
    /// they ever were: a transmission in progress outranks a send that has not
    /// keyed, because it is the one already on somebody else's band.</para>
    /// </remarks>
    public Ft8StopOutcome Outcome => (Unarmed, Abort is not null, AudioToldToStop) switch
    {
        (_, true, true) => Ft8StopOutcome.StoppedTheTransmissionAndToldTheRadio,
        (_, false, true) => Ft8StopOutcome.StoppedTheTransmission,
        (true, true, false) => Ft8StopOutcome.UnarmedAndToldTheRadio,
        (true, false, false) => Ft8StopOutcome.Unarmed,
        (false, true, false) => Ft8StopOutcome.ToldTheRadio,
        _ => Ft8StopOutcome.NothingToStop,
    };

    /// <summary>True where at least one of the abort's two frames got out.</summary>
    /// <remarks>
    /// **FALSE IS NOT THE SAME AS "NOT NEEDED".** It is false both when there was
    /// no port and when there was one that took neither frame, and the second of
    /// those means the radio may still be transmitting. <see cref="Abort"/> tells
    /// the two apart and the caller is expected to look.
    /// </remarks>
    public bool AnythingReachedTheRadio => Abort?.AnythingReachedTheRadio ?? false;
}

/// <summary>
/// **One armed transmission, fired at one boundary, and then gone.**
/// </summary>
/// <remarks>
/// <para>**ONE CLICK, ONE MESSAGE** (ruled 2026-09-06). At most one send is
/// armed at a time; arming again replaces it rather than adding to it; and
/// **firing consumes it**, so nothing further goes out until somebody arms
/// again.</para>
/// <para>**NOTHING HERE DECIDES THAT THE MOMENT HAS COME.** There is no clock
/// read, no timer, no wait and no repetition: the boundary arrives as a
/// parameter to <see cref="AtBoundaryAsync"/>, exactly as the slot and the offset
/// arrive as values in <see cref="OperatorSend"/> and for the same reason - *a
/// path that waits for a moment is a path that can arrive at one on its own*.
/// <c>NothingInTheArmedSendCanStartATransmissionOnItsOwn</c> reads this file and
/// fails on a hit.</para>
/// <para>**THERE IS ONE WAY IN AND IT IS <see cref="Arm"/>.** No decode, no
/// timer, no state change and no retry may set the armed send, because
/// <see cref="Arm"/> is the only member that writes the field and the only caller
/// of it in the shipped tree is the operator's own command.</para>
/// <para>**AND THERE IS ONE WAY TO A KEYING FRAME**, which is
/// <see cref="Ft8TransmitSequence.RunAsync"/> inside
/// <see cref="AtBoundaryAsync"/>. This type adds no second route: it keys
/// nothing itself, names no CI-V constant and touches no port.</para>
/// <para>**AN ARMED SEND THAT HAS NOT KEYED CAN BE CANCELLED**, and
/// <see cref="Cancel"/> is a field cleared under a lock on the calling thread
/// with nothing awaited.</para>
/// <para>**AND THE OPERATOR REACHES BOTH HALVES THROUGH ONE DOOR**, which is
/// <see cref="StopNow"/>: it un-arms what has not keyed and fires
/// <see cref="TransmitAbort.Fire"/> at the port for what has, on the calling
/// thread, with no <c>await</c> on the path and no question asked about which of
/// the two the application believes it is in. Until unit 261 built it,
/// <see cref="Cancel"/> had no caller in <c>src/</c> at all and
/// <c>TransmitAbort.Fire</c> had exactly one - inside
/// <see cref="Ft8TransmitSequence"/>'s own <c>catch</c> - so **no operator
/// gesture reached the abort**.</para>
/// <para>**A BOUNDARY THAT HAS GONE BY DISCARDS THE SEND RATHER THAN
/// TRANSMITTING LATE.** The operator chose a slot; putting his message out in a
/// different one is a transmission he did not ask for, which is this phase's one
/// unrecoverable fault.</para>
/// </remarks>
public sealed class Ft8ArmedSend
{
    private readonly Ft8TransmitSequence _sequence;
    private readonly object _gate = new();

    private OperatorSend? _armed;

    /// <summary>
    /// The running transmission's cancellation source, or null where none is
    /// running.
    /// </summary>
    /// <remarks>
    /// <para>**THE OTHER HALF OF THE STOP, AND IT SITS BESIDE <c>_armed</c> FOR A
    /// REASON.** Those two fields are the whole of what an operator can be holding:
    /// a send waiting for its boundary, and a send that is going out of the radio
    /// right now. <see cref="Cancel"/> takes the first away and this one takes the
    /// second away, and <see cref="StopNow"/> does both without asking which he is
    /// in - because the one moment the answer would be wrong is the moment it
    /// matters.</para>
    /// <para>**IT IS SET UNDER <c>_gate</c> IN THE SAME BLOCK THAT TAKES THE
    /// SEND**, so there is no instant in which a transmission has been committed
    /// to and the stop cannot reach it. Setting it afterwards would leave exactly
    /// such a window, and it is the window in which the operator presses the
    /// button.</para>
    /// <para>**AND IT ADDS NO SECOND WAY TO UN-ARM.** It cannot un-arm anything:
    /// <c>_armed</c> is already null by the time this is set, and nothing reads
    /// this but <see cref="StopTheAudio"/>.</para>
    /// </remarks>
    private CancellationTokenSource? _transmitting;

    /// <summary>Arms over one sequence.</summary>
    /// <param name="sequence">The one path to a keying frame.</param>
    /// <exception cref="ArgumentNullException">There is no sequence.</exception>
    public Ft8ArmedSend(Ft8TransmitSequence sequence)
        => _sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));

    /// <summary>What is armed, or null where nothing is.</summary>
    public OperatorSend? Armed
    {
        get { lock (_gate) { return _armed; } }
    }

    /// <summary>True where a transmission is waiting for its boundary.</summary>
    public bool IsArmed => Armed is not null;

    /// <summary>
    /// Arms one transmission, replacing anything already armed.
    /// </summary>
    /// <param name="send">What the operator asked for.</param>
    /// <exception cref="ArgumentNullException">There is nothing to send.</exception>
    /// <remarks>
    /// **IT REPLACES AND IT NEVER ADDS.** Two clicks a second apart are one
    /// transmission in the next slot, not two: there is one field and this
    /// assigns it.
    /// </remarks>
    public void Arm(OperatorSend send)
    {
        ArgumentNullException.ThrowIfNull(send);

        lock (_gate)
        {
            _armed = send;
        }
    }

    /// <summary>Cancels an armed send that has not keyed.</summary>
    /// <returns>True where something was armed and now is not.</returns>
    public bool Cancel()
    {
        lock (_gate)
        {
            var had = _armed is not null;
            _armed = null;

            return had;
        }
    }

    /// <summary>
    /// **The operator's stop. Both halves, on this thread, waiting for nothing.**
    /// </summary>
    /// <param name="port">
    /// The radio's CI-V transport, in whatever state it is in, or null where no
    /// radio is connected.
    /// </param>
    /// <param name="radioAddress">The radio's CI-V address.</param>
    /// <param name="controllerAddress">This application's CI-V address.</param>
    /// <returns>Which of the four happened, and what the abort's frames did.</returns>
    /// <remarks>
    /// <para>**IT IS NOT CONDITIONAL ON THE APPLICATION BELIEVING IT IS
    /// TRANSMITTING** (`PHASE_PLAN.md` step 1: *it cannot be disabled, deferred,
    /// or made conditional*). There is no flag read here, no state machine
    /// consulted and no "am I sending?" question asked, because the one moment the
    /// answer would be wrong is the moment it matters. **Given a port, the abort
    /// always fires.** Firing it at a radio already in receive costs two frames
    /// and changes nothing - `1C 00 00` is *be in receive*, an absolute state and
    /// not a toggle, and `17 FF` is a stop code a radio sending nothing has
    /// nothing to apply it to (`docs/unit261-stop-trace.md` Q5).</para>
    /// <para>**THE ORDER IS UN-ARM, ABORT, AUDIO, AND EVERY STEP OF IT IS
    /// DELIBERATE.**</para>
    /// <para>*The un-arm comes first.* Between the halves a slot boundary can
    /// arrive on another thread; clearing the field before the frames go out means
    /// the boundary finds <see cref="Ft8ArmOutcome.NothingArmed"/> rather than
    /// keying a radio one microsecond after it was told to stop.</para>
    /// <para>*The abort comes before the audio, and that is the answer to "do not
    /// put the abort behind anything new".* **The carrier is what is on other
    /// people's band**; the sound is only going into a radio. So the two frames go
    /// at the wire before this unit's new line runs at all, and **nothing added
    /// here sits between the operator and his abort** - not a lock, not a null
    /// check, not a cancel that might throw. Reversed, a cancel that hung or threw
    /// would delay or lose the frames, and step 1's criterion is that the abort
    /// cannot be deferred or made conditional. The cost of this order is the few
    /// microseconds of audio that go into an already-unkeyed radio, which is
    /// nothing at all.</para>
    /// <para>*The audio last, and it cannot throw past this method.*
    /// <see cref="StopTheAudio"/> swallows everything - see its own remarks - so
    /// the result is always returned and the frames are always already gone.
    /// <c>ACancelThatThrowsDoesNotCostTheOperatorHisAbort</c> makes the cancel
    /// really throw, by registering a callback on the token from the sink's side,
    /// and reads the wire.</para>
    /// <para>**AND IT CANNOT WAIT ON THE TRANSMISSION IT IS STOPPING.** The one
    /// wait on this path is <c>_gate</c>, and **no member of this type holds
    /// <c>_gate</c> across an <c>await</c>** - <see cref="AtBoundaryAsync"/> takes
    /// it and releases it before the await, so a running transmission never owns
    /// it. The abort itself goes at
    /// <see cref="ISerialPort.Write(ReadOnlySpan{byte})"/>, which returns void and
    /// does not queue behind the in-flight <see cref="ISerialPort.WriteAsync"/>
    /// the sequence uses.</para>
    /// <para>**IT REUSES <see cref="Cancel"/> AND ADDS NO SECOND WAY TO UN-ARM.**
    /// One field, one lock, one line that clears it.</para>
    /// <para>**NULL PORT IS NOT A REFUSAL, IT IS AN ABSENCE.** With no radio
    /// connected there is no wire to write a stop onto, and the result says so
    /// with <see cref="Ft8StopOutcome.NothingToStop"/> or
    /// <see cref="Ft8StopOutcome.Unarmed"/> rather than pretending a frame
    /// went out.</para>
    /// <para>**NOTHING ON THIS PATH IS AWAITED**, and
    /// <c>TheOperatorsStopFiresFromEveryStateTests.NothingOnTheStopPathWaitsForAnything</c>
    /// reads this method's own body and fails on the keyword.</para>
    /// </remarks>
    public Ft8StopResult StopNow(
        ISerialPort? port,
        byte radioAddress = CivConstants.DefaultRadioAddress,
        byte controllerAddress = CivConstants.DefaultControllerAddress)
    {
        var unarmed = Cancel();

        var abort = port is null
            ? null
            : TransmitAbort.Fire(port, radioAddress, controllerAddress);

        var audioToldToStop = StopTheAudio();

        return new Ft8StopResult(unarmed, abort, audioToldToStop);
    }

    /// <summary>Tells a running transmission's audio to stop.</summary>
    /// <returns>True where something was playing and it was told.</returns>
    /// <remarks>
    /// <para>**IT SETS A FLAG AND RETURNS, AND THAT IS THE WHOLE OF IT.**
    /// <c>Cancel()</c> on a source runs every callback registered on it
    /// synchronously on this thread - which is the hazard unit 261 named, and the
    /// reason it recorded this work as a finding rather than doing it. **There are
    /// no callbacks.** `grep -rn "Register("` over <c>WasapiTransmitSink.cs</c> and
    /// the whole of <c>Transmit/</c> returns nothing
    /// (<c>docs/unit263-stop-audio-trace.md</c> Q2 and Q5), so nothing runs here
    /// but the state transition, and no WASAPI call touches the operator's thread.
    /// The sink stops itself, on its own thread, by reading that flag at the top of
    /// the loop it is already in - <c>WasapiTransmitSink.cs:339</c> and
    /// <c>:372</c>.</para>
    /// <para>**THE CANCEL IS NOT DONE UNDER <c>_gate</c>.** The field is read
    /// under it and the <c>Cancel()</c> happens outside, because if a registration
    /// ever did appear on this path, running somebody else's callback while
    /// holding this type's lock is how a stop turns into a deadlock.</para>
    /// <para>**AND NOTHING IT DOES CAN COST THE OPERATOR HIS ABORT.** The frames
    /// have already gone by the time this is called - see
    /// <see cref="StopNow"/>'s remarks on the order - and it swallows everything,
    /// because a stop that threw on the way out would take the operator's whole
    /// stop with it. A cancel that would not take reports false rather than
    /// claiming the sound stopped.</para>
    /// </remarks>
    private bool StopTheAudio()
    {
        CancellationTokenSource? source;

        lock (_gate)
        {
            source = _transmitting;
        }

        if (source is null)
        {
            return false;
        }

        try
        {
            source.Cancel();

            return true;
        }
        catch (Exception)
        {
            // AN UNDERSTATEMENT, DELIBERATELY. A disposed source is a
            // transmission that has already ended; a callback that threw has
            // still marked the token. Neither is worth claiming as a success in
            // front of an operator who wants to know whether he is off the air.
            return false;
        }
    }

    /// <summary>
    /// **The one entry point by which a transmission can begin.**
    /// </summary>
    /// <param name="boundaryUtc">The slot boundary that has just arrived, in true UTC.</param>
    /// <param name="cancellationToken">Stops the transmission.</param>
    /// <returns>What this boundary did.</returns>
    /// <remarks>
    /// **THE SEND IS TAKEN AND THE FIELD IS CLEARED BEFORE ANYTHING IS AWAITED**,
    /// under the lock, in one step. **This was watched failing the other way.**
    /// Built without the clear, one click and three boundaries produced *three*
    /// transmissions - the sink played three times and the port took
    /// `1C 00 01` / `1C 00 00` three times over - with nobody having clicked
    /// twice. `Expected: NothingArmed, Actual: Ran`. Clearing afterwards would
    /// not do either: the await is a window in which another boundary can arrive
    /// and find the same send still armed.
    /// </remarks>
    public async Task<Ft8BoundaryResult> AtBoundaryAsync(
        DateTime boundaryUtc, CancellationToken cancellationToken = default)
    {
        OperatorSend? send;
        CancellationTokenSource source;

        lock (_gate)
        {
            send = _armed;

            // NOT DUE YET IS THE ONE CASE THAT KEEPS IT. Everything else takes
            // it, so a send can be consumed exactly once whatever happens next.
            if (send is not null && boundaryUtc < send.SlotStartUtc)
            {
                return new Ft8BoundaryResult(Ft8ArmOutcome.NotDue, send, null);
            }

            _armed = null;

            if (send is null)
            {
                return new Ft8BoundaryResult(Ft8ArmOutcome.NothingArmed, null, null);
            }

            // THE STOP'S REACH INTO THE TRANSMISSION, INSTALLED BEFORE THE LOCK
            // IS LET GO. The send has been taken and something is about to key;
            // installing this afterwards would leave an instant in which a
            // transmission is committed to and StopNow can find nothing to stop,
            // and that instant is exactly when the operator presses the button.
            // LINKED, NOT STANDALONE: a caller that has a token of its own keeps
            // it, and the operator's stop is a second, independent way in.
            source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _transmitting = source;
        }

        try
        {
            if (boundaryUtc > send.SlotStartUtc)
            {
                // DISCARDED, NOT SENT LATE. The operator chose a slot; putting his
                // message out in a different one is a transmission he did not ask
                // for, and it is gone rather than moved.
                return new Ft8BoundaryResult(Ft8ArmOutcome.TooLate, send, null);
            }

            var run = await _sequence.RunAsync(send, source.Token).ConfigureAwait(false);

            return new Ft8BoundaryResult(Ft8ArmOutcome.Ran, send, run);
        }
        finally
        {
            // CLEARED ONLY IF IT IS STILL OURS. Another boundary cannot overlap
            // this one - the send was consumed under the lock - but a field that
            // is only ever cleared by the run that set it cannot be got wrong
            // later, and a stale source would be a stop that cancelled nothing.
            lock (_gate)
            {
                if (ReferenceEquals(_transmitting, source))
                {
                    _transmitting = null;
                }
            }

            source.Dispose();
        }
    }
}

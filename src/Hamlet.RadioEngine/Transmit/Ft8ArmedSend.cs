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
public sealed record Ft8StopResult(bool Unarmed, AbortRecord? Abort)
{
    /// <summary>Which of the four this was.</summary>
    public Ft8StopOutcome Outcome => (Unarmed, Abort is not null) switch
    {
        (true, true) => Ft8StopOutcome.UnarmedAndToldTheRadio,
        (true, false) => Ft8StopOutcome.Unarmed,
        (false, true) => Ft8StopOutcome.ToldTheRadio,
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
    /// <para>**THE UN-ARM COMES FIRST, AND THAT ORDER IS DELIBERATE.** Between the
    /// two halves a slot boundary can arrive on another thread; clearing the field
    /// before the frames go out means the boundary finds
    /// <see cref="Ft8ArmOutcome.NothingArmed"/> rather than keying a radio one
    /// microsecond after it was told to stop.</para>
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

        return new Ft8StopResult(unarmed, abort);
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
        }

        if (send is null)
        {
            return new Ft8BoundaryResult(Ft8ArmOutcome.NothingArmed, null, null);
        }

        if (boundaryUtc > send.SlotStartUtc)
        {
            // DISCARDED, NOT SENT LATE. The operator chose a slot; putting his
            // message out in a different one is a transmission he did not ask
            // for, and it is gone rather than moved.
            return new Ft8BoundaryResult(Ft8ArmOutcome.TooLate, send, null);
        }

        var run = await _sequence.RunAsync(send, cancellationToken).ConfigureAwait(false);

        return new Ft8BoundaryResult(Ft8ArmOutcome.Ran, send, run);
    }
}

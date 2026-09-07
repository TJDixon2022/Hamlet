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
/// with nothing awaited. Once it has keyed, the route out is the sequence's own
/// abort and nothing is added beside it.</para>
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

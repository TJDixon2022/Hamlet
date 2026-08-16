using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Cw;

/// <summary>Everything Hamlet needs to know before it will key.</summary>
/// <param name="LicenseClass">The operator's class.</param>
/// <param name="FrequencyHz">Where they would transmit.</param>
/// <param name="GuardEnabled">Their "only where my license allows" setting.</param>
/// <param name="Connected">Whether a radio is connected.</param>
/// <param name="Capabilities">What that radio can do, or null.</param>
/// <param name="State">Everything read from it.</param>
public sealed record TransmitContext(
    LicenseClass LicenseClass,
    long FrequencyHz,
    bool GuardEnabled,
    bool Connected,
    RigCapabilities? Capabilities,
    RigState State);

/// <summary>Why a send did not happen, or that it did.</summary>
/// <param name="Sent">True only when something went on the air.</param>
/// <param name="Detail">What to tell the operator.</param>
/// <param name="Citation">The paragraph or page behind a refusal, or "".</param>
/// <param name="Result">The sender's own outcome, or null when it never got there.</param>
/// <param name="GuardOverridden">
/// True when the operator had the privilege guard switched off and the transmit
/// proceeded on their own authority (HM-DEC-029).
/// </param>
/// <param name="Readiness">
/// What the precondition check saw, carried so the record and the screen can
/// both name it without evaluating it a second time (HM-DEC-077).
/// </param>
public sealed record TransmitOutcome(
    bool Sent,
    string Detail,
    string Citation,
    CwSendResult? Result,
    bool GuardOverridden,
    CwReadiness? Readiness = null);

/// <summary>
/// The one door to the transmitter (HM-DEC-059, §0.2).
/// </summary>
/// <remarks>
/// <para>EVERY PATH THAT KEYS GOES THROUGH HERE, and here calls the transmit
/// guard first, every time, before it touches the radio (HM-DEC-029). There is
/// no second way in and no bypass: the sender below this refuses to be reached
/// any other way because nothing else holds a reference to it.</para>
/// <para>THE ORDER MATTERS AND IS NOT AN IMPLEMENTATION DETAIL. The guard
/// answers first, because it is the question with legal consequences. The
/// break-in precondition answers second, because a message the radio accepts
/// and does not send is a success the app would otherwise report (HM-DEC-049,
/// p. 19-7 footnote 2). Only then does anything go out.</para>
/// <para>NOTHING HERE HAS A TIMER, A RETRY OR A LOOP. A send happens because a
/// person pressed something. No reconnect path, no refresh, no scan and no
/// schedule may reach this class, and there is nothing in it that would let
/// them: it does what it is asked, once, and returns (§0.2).</para>
/// <para>The abort is passed straight through, synchronously, so the path from
/// somebody's finger to the stop frame has nothing in it that can wait.</para>
/// </remarks>
public sealed class CwTransmitter
{
    private readonly ICwSender _sender;
    private readonly TransmitGuard _guard;

    /// <summary>Create a transmitter over a sender.</summary>
    /// <param name="sender">Whatever keys the radio.</param>
    /// <param name="guard">The privilege guard, or a fresh one.</param>
    public CwTransmitter(ICwSender sender, TransmitGuard? guard = null)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _guard = guard ?? new TransmitGuard();
    }

    /// <summary>Whether this path can widen the gaps between characters.</summary>
    public bool SupportsCharacterSpacing => _sender.SupportsCharacterSpacing;

    /// <summary>What this path is called.</summary>
    public string PathName => _sender.PathName;

    /// <summary>
    /// Would this send go out, and may it?
    /// </summary>
    /// <param name="context">Everything Hamlet knows.</param>
    /// <returns>The verdict, without touching the radio.</returns>
    /// <remarks>
    /// SEPARATE FROM SENDING ON PURPOSE, so the UI can show the answer beside
    /// the button rather than discovering it at the moment of keying. Somebody
    /// about to make their first call should read "break-in is off" before they
    /// press, not after.
    /// </remarks>
    public TransmitOutcome Check(TransmitContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var decision = _guard.Check(
            context.LicenseClass, context.FrequencyHz, TransmitMode.Cw,
            context.GuardEnabled);

        // THE PRIVILEGE CHECK IS A READINESS PRECONDITION NOW (HM-DEC-089), not
        // a separate answer arriving before one. It refused here before, which
        // disabled the buttons correctly and left the record unable to say why:
        // the outcome carried no readiness at all, so a refusal on privileges
        // and a refusal nobody evaluated looked identical in the file
        // (HM-DEC-077).
        var ready = TransmitReadiness.Check(
            context.Connected, context.Capabilities, context.State, DateTime.UtcNow,
            new TransmitPrivileges(
                context.LicenseClass, context.FrequencyHz, context.GuardEnabled));

        return ready.MaySend
            ? new TransmitOutcome(
                true, "", decision.Citation, null, decision.WasOverridden, ready)
            : new TransmitOutcome(
                false, ready.Detail, ready.Citation, null, decision.WasOverridden, ready);
    }

    /// <summary>
    /// Send a message, if everything says it may.
    /// </summary>
    /// <param name="message">What to send.</param>
    /// <param name="context">Everything Hamlet knows.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>What happened. Never throws.</returns>
    public async Task<TransmitOutcome> SendAsync(
        string message,
        TransmitContext context,
        CancellationToken cancellationToken = default)
    {
        var check = Check(context);

        if (!check.Sent)
        {
            return check;
        }

        var result = await _sender.SendAsync(message, cancellationToken).ConfigureAwait(false);

        return new TransmitOutcome(
            result.Worked,
            result.Detail,
            check.Citation,
            result,
            check.GuardOverridden);
    }

    /// <summary>
    /// Stop whatever is going out, now, on this thread.
    /// </summary>
    /// <remarks>
    /// No guard, no check, no await (§0.2). Stopping is never refused: the guard
    /// exists to keep a signal off the air and an abort is on its side.
    /// </remarks>
    public void Abort() => _sender.Abort();
}

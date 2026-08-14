namespace Hamlet.RadioEngine.Cw;

/// <summary>What became of a send.</summary>
public enum CwSendOutcome
{
    /// <summary>The radio took every piece and acknowledged them.</summary>
    Sent,

    /// <summary>Stopped part way, because somebody pressed the abort.</summary>
    Aborted,

    /// <summary>The radio answered and refused.</summary>
    Refused,

    /// <summary>Nothing came back inside the timeout.</summary>
    NoAnswer,

    /// <summary>There was nothing sendable in the message.</summary>
    NothingToSend,

    /// <summary>This radio cannot send Morse (HM-DEC-030).</summary>
    NotSupported,
}

/// <summary>The outcome of one send, with enough to say why.</summary>
/// <param name="Outcome">What became of it.</param>
/// <param name="Detail">What to tell the operator, or "" when it simply worked.</param>
/// <param name="PiecesSent">How many keyer messages actually went.</param>
/// <param name="PiecesTotal">How many the message needed.</param>
public sealed record CwSendResult(
    CwSendOutcome Outcome, string Detail, int PiecesSent, int PiecesTotal)
{
    /// <summary>True only when the whole message went.</summary>
    public bool Worked => Outcome == CwSendOutcome.Sent;

    /// <summary>Nothing sendable was in the message.</summary>
    public static CwSendResult Nothing { get; } =
        new(CwSendOutcome.NothingToSend, "There was nothing in that to send.", 0, 0);

    /// <summary>This radio does not send Morse.</summary>
    /// <param name="why">What decided that.</param>
    /// <returns>The result.</returns>
    public static CwSendResult NotSupported(string why)
        => new(CwSendOutcome.NotSupported, why, 0, 0);
}

/// <summary>
/// Whatever keys the transmitter, behind one seam (HM-DEC-059).
/// </summary>
/// <remarks>
/// <para>TWO PATHS EXIST AND ONLY ONE IS BUILT. Command 17 hands up to thirty
/// characters to the radio's own keyer, which sends them at the radio's keyer
/// speed with the radio's own clean timing. USB keying, where the PC raises and
/// drops the keying line itself and owns every element, is the second path and
/// is deliberately deferred to its own ruling and its own session, after 17 is
/// proven at a dummy load (HM-DEC-008).</para>
/// <para>So this interface exists today with one implementation behind it. What
/// it buys is that adding USB keying later is a new implementation rather than a
/// rewrite of everything above it, and nothing above it learns which path it is
/// on except through <see cref="SupportsCharacterSpacing"/>.</para>
/// <para>THE ABORT IS NOT ASYNCHRONOUS AND THAT IS DELIBERATE (§0.2). It is a
/// same-thread call that returns nothing and awaits nothing, because the moment
/// somebody wants a transmitter to stop is the moment they cannot wait for a
/// task to be scheduled. It must work while a send is in flight and it must not
/// depend on the send loop noticing anything.</para>
/// </remarks>
public interface ICwSender
{
    /// <summary>
    /// Whether this path can widen the gaps between characters, which is what
    /// Farnsworth means.
    /// </summary>
    /// <remarks>
    /// FALSE FOR THE KEYER PATH, AND SAID OUT LOUD RATHER THAN HIDDEN. The
    /// radio's CW-KEY SET menu offers dot/dash ratio, rise time, paddle polarity
    /// and key type, and nothing at all for character spacing (Full Manual,
    /// keyer set menu, p. 4-21). Farnsworth needs control of the timing between
    /// characters, which only the USB keying path has. The UI reads this and
    /// says so where speed is chosen; it never offers a control that silently
    /// does nothing (§0.0).
    /// </remarks>
    bool SupportsCharacterSpacing { get; }

    /// <summary>How many characters one message may carry.</summary>
    int MaximumMessageLength { get; }

    /// <summary>What this path is called, for the record and the log.</summary>
    string PathName { get; }

    /// <summary>
    /// Send a message, splitting it into pieces the radio will take.
    /// </summary>
    /// <param name="message">What to send.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>What became of it. Never throws.</returns>
    Task<CwSendResult> SendAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop a send in progress, now, on this thread.
    /// </summary>
    /// <remarks>
    /// Returns nothing and awaits nothing (§0.2). Safe to call when nothing is
    /// sending, safe to call twice, and it never throws: an abort that could
    /// fail is not an abort.
    /// </remarks>
    void Abort();
}

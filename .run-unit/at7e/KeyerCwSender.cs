using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// Sending Morse by handing text to the radio's own keyer, command 17
/// (HM-DEC-059).
/// </summary>
/// <remarks>
/// <para>THE RADIO DOES THE KEYING. Hamlet hands over up to thirty characters
/// and the radio sends them at its own keyer speed with its own clean timing,
/// which is better timing than a PC can produce over a serial line and is the
/// reason this path is built first (Full Manual p. 19-11).</para>
/// <para>WHAT IT CANNOT DO IS FARNSWORTH, and that is a fact about the radio
/// rather than about this class. The CW-KEY SET menu offers dot/dash ratio, rise
/// time, paddle polarity and key type, and nothing for the gaps between
/// characters (p. 4-21). Farnsworth means characters at a brisk speed with wide
/// gaps between them, which is how a learner hears a whole letter as one shape
/// rather than counting elements, and it needs control of the timing between
/// characters. Only the USB keying path has that, and it is deliberately
/// deferred.</para>
/// <para>NOTHING HERE DECIDES WHETHER IT IS ALLOWED TO SEND. The guard and the
/// break-in precondition are the caller's, checked once, above this. What this
/// class owns is turning one message into pieces the radio will take, sending
/// them in order, and stopping the moment it is told to.</para>
/// </remarks>
public sealed class KeyerCwSender : ICwSender
{
    private readonly IRig _rig;
    private volatile bool _aborting;

    /// <summary>Create a sender over a rig.</summary>
    /// <param name="rig">The radio.</param>
    public KeyerCwSender(IRig rig) => _rig = rig ?? throw new ArgumentNullException(nameof(rig));

    /// <inheritdoc/>
    /// <remarks>
    /// False, and the UI says so rather than offering a control that does
    /// nothing. See the class remarks for why the radio cannot do it.
    /// </remarks>
    public bool SupportsCharacterSpacing => false;

    /// <inheritdoc/>
    public int MaximumMessageLength => CwMessage.MaximumLength;

    /// <inheritdoc/>
    public string PathName => "the radio's own keyer";

    /// <summary>True while a send is in flight.</summary>
    public bool IsSending { get; private set; }

    /// <inheritdoc/>
    public async Task<CwSendResult> SendAsync(
        string message, CancellationToken cancellationToken = default)
    {
        if (!_rig.Capabilities.HasBuiltInCwKeyer || !_rig.Capabilities.CanTransmit)
        {
            return CwSendResult.NotSupported(
                $"{_rig.Capabilities.Model} has no keyer to send Morse with.");
        }

        var pieces = CwMessage.Split(message);

        if (pieces.Count == 0)
        {
            return CwSendResult.Nothing;
        }

        _aborting = false;
        IsSending = true;

        try
        {
            for (var i = 0; i < pieces.Count; i++)
            {
                // Checked before every piece, so an abort part way through a
                // long message stops at the next boundary as well as stopping
                // the piece already going out (§0.2).
                if (_aborting || cancellationToken.IsCancellationRequested)
                {
                    return new CwSendResult(
                        CwSendOutcome.Aborted,
                        i == 0
                            ? "Stopped before anything went out."
                            : "Stopped part way through.",
                        i, pieces.Count);
                }

                if (!await _rig.SendCwAsync(pieces[i], cancellationToken).ConfigureAwait(false))
                {
                    return new CwSendResult(
                        CwSendOutcome.NoAnswer,
                        "The radio did not take that, so Hamlet cannot say what went "
                        + "out. Nothing is repeated automatically.",
                        i, pieces.Count);
                }
            }

            return new CwSendResult(CwSendOutcome.Sent, "", pieces.Count, pieces.Count);
        }
        catch (OperationCanceledException)
        {
            return new CwSendResult(
                CwSendOutcome.Aborted, "Stopped part way through.", 0, pieces.Count);
        }
        catch (Exception ex)
        {
            // Never-throw discipline (§8). A send that failed is a sentence.
            return new CwSendResult(
                CwSendOutcome.NoAnswer,
                $"Hamlet could not send that: {ex.Message}", 0, pieces.Count);
        }
        finally
        {
            IsSending = false;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Sets the flag first and then keys the stop, in that order, so that a send
    /// loop which happens to be between pieces stops even if the radio never
    /// hears the stop frame. Neither half depends on the other noticing
    /// anything (§0.2).
    /// </remarks>
    public void Abort()
    {
        _aborting = true;
        _rig.AbortCw();
    }
}

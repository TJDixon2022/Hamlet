using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.RadioEngine.Cw;

/// <summary>Why a keyer message would not actually go out.</summary>
public enum CwReadyState
{
    /// <summary>Everything the radio needs is in place.</summary>
    Ready,

    /// <summary>Nothing is connected.</summary>
    NotConnected,

    /// <summary>This radio cannot transmit at all (HM-DEC-030).</summary>
    RadioCannotTransmit,

    /// <summary>The radio is not in a Morse mode.</summary>
    NotInMorse,

    /// <summary>
    /// The operator's license does not cover transmitting here (HM-DEC-089).
    /// </summary>
    /// <remarks>
    /// **THE ONE STATE WHERE A DISABLED CONTROL IS THE RIGHT ANSWER** rather
    /// than a fault. Everywhere else in the application grey means something
    /// broke (HM-DEC-087). Here it means the law, and it still says why and what
    /// would change it.
    /// </remarks>
    OutsidePrivileges,

    /// <summary>
    /// This stretch is receive only for the operator's class (HM-DEC-029).
    /// </summary>
    /// <remarks>
    /// Kept apart from being outside the band entirely, because they call for
    /// different things: one is a stretch to move out of, the other is a stretch
    /// to listen in.
    /// </remarks>
    ListenOnly,

    /// <summary>
    /// Hamlet does not know the operator's license class (HM-DEC-089).
    /// </summary>
    /// <remarks>
    /// **THIS SUPERSEDES HM-DEC-065**, which had an unresolved class permit and
    /// warn. It refuses while the privilege guard is on, because a frequency
    /// cannot be checked against a class nobody has, and unknown is not
    /// permission (HM-DEC-050). The guard remains the operator's to switch off,
    /// which is what keeps them from ever being locked out of their own
    /// transmitter.
    /// </remarks>
    LicenseClassUnknown,

    /// <summary>
    /// Hamlet does not know where the radio is (HM-DEC-089).
    /// </summary>
    /// <remarks>
    /// A different ignorance from not knowing the class. Transmitting on
    /// Hamlet's own idea of where the radio is tuned would be a guess in the one
    /// place a confident error has legal consequences (§0.0, HM-DEC-029).
    /// </remarks>
    FrequencyUnknown,

    /// <summary>
    /// Break-in is off and nothing is holding the transmitter on, so a message
    /// sent with command 17 would be keyed into a radio that is still receiving.
    /// </summary>
    /// <remarks>
    /// THE OPERATOR CAN WALK OVER AND FIX THIS ONE, which is exactly why it is
    /// no longer the same state as never having read it (HM-DEC-077). The two
    /// produced one verdict and one message, so a file could not tell them apart
    /// and neither could the person reading the screen.
    /// </remarks>
    BreakInOff,

    /// <summary>Hamlet has not read break-in yet, which is not permission.</summary>
    /// <remarks>
    /// Refusing on unknown is correct (HM-DEC-050) and it calls for something
    /// completely different from refusing on off: waiting, or asking the radio
    /// again, rather than walking across the room.
    /// </remarks>
    BreakInUnknown,

    /// <summary>Hamlet has not read the mode yet.</summary>
    ModeUnknown,

    /// <summary>The radio is already transmitting.</summary>
    AlreadyTransmitting,
}

/// <summary>What Hamlet can say about whether a send would reach the air.</summary>
/// <param name="State">The verdict.</param>
/// <param name="MaySend">True only when everything is in place.</param>
/// <param name="Detail">
/// What to tell the operator, in one sentence, or "" when all is well.
/// </param>
/// <param name="Citation">The manual page behind it, or "".</param>
/// <param name="DeterminedBy">
/// Every precondition that was checked, with the value seen, its provenance and
/// its age (HM-DEC-077). Carried on the verdict rather than recomputed, so the
/// record cannot disagree with the decision it describes.
/// </param>
public sealed record CwReadiness(
    CwReadyState State, bool MaySend, string Detail, string Citation,
    IReadOnlyList<DeterminedBy>? DeterminedBy = null)
{
    /// <summary>
    /// The stable machine token for this verdict (HM-DEC-077).
    /// </summary>
    /// <remarks>
    /// A token rather than the sentence, because the sentence is written for a
    /// person and gets reworded the next time somebody improves the copy, taking
    /// every comparison across sessions with it.
    /// </remarks>
    public string Reason => State switch
    {
        CwReadyState.Ready => OutcomeEvent.Ok,
        CwReadyState.NotConnected => "not_connected",
        CwReadyState.RadioCannotTransmit => "radio_cannot_transmit",
        CwReadyState.NotInMorse => "not_in_morse",
        CwReadyState.BreakInOff => "break_in_off",
        CwReadyState.BreakInUnknown => "break_in_unknown",
        CwReadyState.ModeUnknown => "mode_unknown",
        _ => "already_transmitting",
    };

    /// <summary>The outcome this verdict is.</summary>
    public Outcome Outcome
        => MaySend ? Outcome.Proceeded : Outcome.Refused;

    /// <summary>The event body for this verdict.</summary>
    public OutcomeEvent AsEvent()
        => new(Outcome, Reason,
            DeterminedBy ?? Array.Empty<DeterminedBy>());
}

/// <summary>
/// The precondition nobody had written down (HM-DEC-059, HM-DEC-049).
/// </summary>
/// <remarks>
/// <para>THE FAILURE THIS EXISTS TO PREVENT. In CW mode a message sent with
/// command 17 is transmitted only when TRANSMIT or an external TX switch is on,
/// or Break-in is on (Full Manual, command table footnote 2, p. 19-7). Without
/// it Hamlet sends a correct frame, gets a correct acknowledgement, and the
/// radio stays silent. A correct frame with a correct acknowledgement and no
/// signal is the prime directive broken by omission: the app would report a
/// success that never left the antenna, and somebody making their first call
/// would sit there wondering why nobody answered.</para>
/// <para>SO IT IS CHECKED BEFORE THE SEND, NOT AFTER. Hamlet already reads
/// break-in state and transmit status (HM-DEC-050), so it can answer this
/// rather than guess at it. Where it has not read them, it says so and refuses,
/// because "I do not know whether this will go out" is a different answer from
/// "it will" and only one of them is honest.</para>
/// <para>Pure: a rig's capabilities and its state in, a verdict out. No clock,
/// no radio (§5).</para>
/// </remarks>
public static class TransmitReadiness
{
    /// <summary>The manual page behind the break-in precondition.</summary>
    public const string BreakInCitation =
        "IC-7300 Full Manual, command table footnote 2, p. 19-7";

    /// <summary>Can a keyer message actually reach the air right now?</summary>
    /// <param name="connected">Whether a radio is connected.</param>
    /// <param name="capabilities">What the connected radio can do.</param>
    /// <param name="state">Everything Hamlet has read from it.</param>
    /// <returns>The verdict, never null.</returns>
    public static CwReadiness Check(
        bool connected, RigCapabilities? capabilities, RigState state)
        => Check(connected, capabilities, state, DateTime.UtcNow);

    /// <summary>Can a keyer message actually reach the air right now?</summary>
    /// <param name="connected">Whether a radio is connected.</param>
    /// <param name="capabilities">What the connected radio can do.</param>
    /// <param name="state">Everything Hamlet has read from it.</param>
    /// <param name="nowUtc">The moment, for the ages in the record.</param>
    /// <returns>The verdict, never null, carrying what decided it.</returns>
    /// <remarks>
    /// EVERY PRECONDITION IT LOOKED AT TRAVELS WITH THE VERDICT (HM-DEC-077),
    /// including the ones that passed. A record of only the failing condition
    /// cannot tell "break-in was read as on and something else refused" from
    /// "break-in was never reached", and those need different fixes.
    /// </remarks>
    public static CwReadiness Check(
        bool connected, RigCapabilities? capabilities, RigState state, DateTime nowUtc)
        => Check(connected, capabilities, state, nowUtc, null);

    /// <summary>Can a keyer message actually reach the air right now?</summary>
    /// <param name="connected">Whether a radio is connected.</param>
    /// <param name="capabilities">What the connected radio can do.</param>
    /// <param name="state">Everything Hamlet has read from it.</param>
    /// <param name="nowUtc">The moment, for the ages in the record.</param>
    /// <param name="privileges">
    /// Where the operator is and what their license covers, or null to skip the
    /// check entirely (HM-DEC-089).
    /// </param>
    /// <returns>The verdict, never null, carrying what decided it.</returns>
    /// <remarks>
    /// **PRIVILEGES ARE SETTLED BEFORE THE RADIO IS BLAMED.** Being in the wrong
    /// place is the operator's to fix and break-in is the radio's, and sending
    /// somebody across the room to change a setting for a transmission that was
    /// never allowed wastes their evening and teaches them nothing.
    /// </remarks>
    public static CwReadiness Check(
        bool connected,
        RigCapabilities? capabilities,
        RigState state,
        DateTime nowUtc,
        TransmitPrivileges? privileges)
    {
        ArgumentNullException.ThrowIfNull(state);

        var saw = new List<DeterminedBy>
        {
            Telemetry.DeterminedBy.Fact("connected", connected ? 1 : 0),
        };

        if (!connected || capabilities is null)
        {
            return new CwReadiness(
                CwReadyState.NotConnected, false,
                "There is no radio connected, so there is nothing to send with.", "",
                saw);
        }

        saw.Add(Telemetry.DeterminedBy.Fact(
            "canTransmit", capabilities.CanTransmit ? 1 : 0));

        if (!capabilities.CanTransmit)
        {
            return new CwReadiness(
                CwReadyState.RadioCannotTransmit, false,
                $"{capabilities.Model} does not transmit, so this is receive only.", "",
                saw);
        }

        var transmitting = state[RigField.TransmitStatus];
        saw.Add(Telemetry.DeterminedBy.From(transmitting, nowUtc, RigSnapshot.FreshFor));

        if (state.IsTransmitting)
        {
            return new CwReadiness(
                CwReadyState.AlreadyTransmitting, false,
                "The radio is already transmitting, so this will wait until it stops.",
                "", saw);
        }

        var modeValue = state[RigField.Mode];
        saw.Add(Telemetry.DeterminedBy.From(modeValue, nowUtc, RigSnapshot.FreshFor));

        if (!modeValue.IsKnown)
        {
            return new CwReadiness(
                CwReadyState.ModeUnknown, false,
                "Hamlet has not read the radio's mode yet, so it cannot tell whether "
                + "the keyer would send anything. It asks on connect, so give it a "
                + "moment.",
                "", saw);
        }

        if (state.Mode is not { } mode || !CivValues.IsCw(mode))
        {
            return new CwReadiness(
                CwReadyState.NotInMorse, false,
                $"The radio is in {modeValue.Text} rather than Morse, and the keyer "
                + "only sends Morse. Switch to CW and it will be ready.",
                "", saw);
        }

        if (privileges is { } may)
        {
            var verdict = may.Judge(saw);

            if (verdict is not null)
            {
                return verdict;
            }
        }

        // THE ONE THAT COSTS AN EVENING IF IT IS SKIPPED. An unread break-in
        // setting is not permission: it is a thing nobody has looked at, and
        // saying "this will go out" from it would be a guess (§0.0).
        var breakIn = state[RigField.BreakIn];
        saw.Add(Telemetry.DeterminedBy.From(breakIn, nowUtc, RigSnapshot.FreshFor));

        if (!breakIn.IsKnown)
        {
            return new CwReadiness(
                CwReadyState.BreakInUnknown, false,
                "Hamlet has not read whether break-in is on, and without it the "
                + "radio takes the message and sends nothing. It reads that on "
                + "connect, so give it a moment or open what the radio is doing.",
                BreakInCitation, saw);
        }

        if (breakIn.Number is 0)
        {
            return new CwReadiness(
                CwReadyState.BreakInOff, false,
                "Break-in is off on the radio, and with it off the radio accepts "
                + "the message and stays quiet. Turn break-in on at the radio, or "
                + "hold the transmitter on yourself, and the same button will work.",
                BreakInCitation, saw);
        }

        return new CwReadiness(CwReadyState.Ready, true, "", "", saw);
    }
}

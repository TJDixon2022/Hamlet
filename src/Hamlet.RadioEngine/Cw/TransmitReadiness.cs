using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;

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
    /// Break-in is off and nothing is holding the transmitter on, so a message
    /// sent with command 17 would be keyed into a radio that is still receiving.
    /// </summary>
    BreakInOff,

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
public sealed record CwReadiness(
    CwReadyState State, bool MaySend, string Detail, string Citation);

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
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!connected || capabilities is null)
        {
            return new CwReadiness(
                CwReadyState.NotConnected, false,
                "There is no radio connected, so there is nothing to send with.", "");
        }

        if (!capabilities.CanTransmit)
        {
            return new CwReadiness(
                CwReadyState.RadioCannotTransmit, false,
                $"{capabilities.Model} does not transmit, so this is receive only.", "");
        }

        if (state.IsTransmitting)
        {
            return new CwReadiness(
                CwReadyState.AlreadyTransmitting, false,
                "The radio is already transmitting, so this will wait until it stops.",
                "");
        }

        if (state.Mode is not { } mode || !CivValues.IsCw(mode))
        {
            var what = state[RigField.Mode] is { IsKnown: true } read
                ? $"The radio is in {read.Text} rather than Morse"
                : "Hamlet has not read the radio's mode yet";

            return new CwReadiness(
                CwReadyState.NotInMorse, false,
                $"{what}, and the keyer only sends Morse. Switch to CW and it will "
                + "be ready.",
                "");
        }

        // THE ONE THAT COSTS AN EVENING IF IT IS SKIPPED. An unread break-in
        // setting is not permission: it is a thing nobody has looked at, and
        // saying "this will go out" from it would be a guess (§0.0).
        var breakIn = state[RigField.BreakIn];

        if (!breakIn.IsKnown)
        {
            return new CwReadiness(
                CwReadyState.BreakInOff, false,
                "Hamlet has not read whether break-in is on, and without it the "
                + "radio takes the message and sends nothing. It reads that on "
                + "connect, so give it a moment or open what the radio is doing.",
                BreakInCitation);
        }

        if (breakIn.Number is 0)
        {
            return new CwReadiness(
                CwReadyState.BreakInOff, false,
                "Break-in is off on the radio, and with it off the radio accepts "
                + "the message and stays quiet. Turn break-in on, or hold the "
                + "transmitter on yourself, and the same button will work.",
                BreakInCitation);
        }

        return new CwReadiness(CwReadyState.Ready, true, "", "");
    }
}

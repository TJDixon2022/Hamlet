using Hamlet.RadioEngine.Contacts;

namespace Hamlet.RadioEngine.Psk31;

/// <summary>Whose turn it is on a PSK31 exchange, in `PHASE_PLAN.md` §3.1's four.</summary>
public enum Psk31TurnState
{
    /// <summary>Hamlet cannot tell.</summary>
    Unknown,

    /// <summary>The other station handed the turn to the operator.</summary>
    YourTurn,

    /// <summary>The operator handed the turn to the other station.</summary>
    HisTurn,

    /// <summary>Characters are still arriving after the last complete message.</summary>
    HeIsSending,
}

/// <summary>Whose turn it is, and whether Hamlet is sure.</summary>
/// <param name="State">Which of the four.</param>
/// <param name="IsCertain">
/// True only where the reading rests on a certain parse or on the demodulator's own fact. **False
/// is a guess or an unknown, and a reader must show it as one** (§R1).
/// </param>
public sealed record Psk31TurnReading(Psk31TurnState State, bool IsCertain)
{
    /// <summary>Nothing to read.</summary>
    public static readonly Psk31TurnReading Unknown = new(Psk31TurnState.Unknown, false);
}

/// <summary>Reads whose turn it is from one channel's messages.</summary>
/// <remarks>
/// <para>**MESSAGES, A FACT AND A CALLSIGN IN; A READING OUT** (§0.1). It is not told about cards,
/// rows, tabs or settings.</para>
/// <para>**THE RULE IS <see cref="Rule"/>.** It uses §R3's vocabulary as it stands, through the
/// parse: the turnover and closing words, the calls, and whether the addressee is the operator. A
/// roger is still text (ask 28), so nothing here reads one.</para>
/// <para>**NO STATE FROM TEXT THE SQUELCH DID NOT PASS** (§3.5). This reads only messages
/// <see cref="Psk31MessageSplitter"/> cut, and the splitter is fed a channel's
/// <see cref="Psk31Channel.Text"/>, which holds only characters the demodulator's squelch opened
/// for and <see cref="Psk31Listener"/> still vouched for. No raw character reaches it.</para>
/// <para>**HE IS STILL SENDING IS CERTAIN, AND IT IS THE ONLY STATE THAT IS NOT THE PARSER'S**
/// (§3.1: *carrier-present is a fact from the demodulator; the rest is from the parser*). Characters
/// arriving is a measurement. Who is sending them is not claimed.</para>
/// </remarks>
public static class Psk31Turn
{
    /// <summary>The turn rule, in one sentence.</summary>
    public const string Rule =
        "Whoever last handed over gave the turn away: after a message that hands over, the operator's "
        + "own makes it his turn and another station's addressed to the operator makes it your turn, "
        + "characters arriving after any message make it he is still sending, and everything else - no "
        + "message yet, or a last message neither from nor to the operator - is unknown; a turn is "
        + "certain only where that message's parse is certain, and he is still sending is certain "
        + "because it is the demodulator's fact and not the parser's.";

    /// <summary>Read whose turn it is.</summary>
    /// <param name="messages">The channel's complete messages, oldest first.</param>
    /// <param name="charactersSinceLastMessage">
    /// Whether characters have arrived since the last of them - the caller's carrier-present fact.
    /// </param>
    /// <param name="operatorCallsign">The operator's own callsign, or null.</param>
    /// <returns>The reading.</returns>
    /// <exception cref="ArgumentNullException">There is no message list.</exception>
    public static Psk31TurnReading Read(
        IReadOnlyList<Psk31Message> messages, bool charactersSinceLastMessage, string? operatorCallsign)
    {
        ArgumentNullException.ThrowIfNull(messages);

        // **NOTHING FINISHED, NOTHING TO READ**, whatever is arriving: text before the first
        // turnover has no owner (`Psk31ExchangeParser`'s remarks).
        if (messages.Count == 0)
        {
            return Psk31TurnReading.Unknown;
        }

        if (charactersSinceLastMessage)
        {
            return new Psk31TurnReading(Psk31TurnState.HeIsSending, true);
        }

        var last = messages[^1].Exchange;

        if (!last.HandsOver || last.Speaker is null)
        {
            return Psk31TurnReading.Unknown;
        }

        if (Ft8MessageSplit.IsSameStation(last.Speaker, operatorCallsign))
        {
            return new Psk31TurnReading(Psk31TurnState.HisTurn, last.IsCertain);
        }

        return last.IsForOperator
            ? new Psk31TurnReading(Psk31TurnState.YourTurn, last.IsCertain)
            : Psk31TurnReading.Unknown;
    }
}

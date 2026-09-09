using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>
/// **Everything about one station that a card may say, read off the ledger and
/// derived from nothing else.**
/// </summary>
/// <remarks>
/// <para>**IT HOLDS FACTS AND NOT WORDS** (§0.1). Every member below is a count, a
/// moment, a flag or a payload the ledger already holds; the sentence a reader sees
/// is composed in the application, where the voice lives. The engine knows what
/// passed and the shell knows how to say it, and this type is the seam.</para>
/// <para>**IT DERIVES; IT DOES NOT RULE.** Nothing here hides a station, closes a
/// contact, withholds an option or returns a verdict about anybody's operating -
/// the same promise <see cref="Ft8ContactLedger"/> and <see cref="Ft8ContactStates"/>
/// make, kept for the same reason. A card is a view of a record.</para>
/// <para>**IT INTERPRETS NOTHING** (§12.1). Every flag below is a shape test on a
/// payload field or a count of list entries. There is no member that asks what a
/// station meant.</para>
/// <para>**THE SIGN-OFF IS ITS OWN FACT AND IS NOT READ OFF THE STATE** (work
/// instruction 297 task 1). <see cref="Ft8ContactStates.IsComplete"/> counts `RRR`
/// as an acknowledgement and says so in its own remarks, so a card that read *he
/// signed off* off <see cref="Ft8ContactState.Complete"/> would be wrong every time
/// an exchange ended that way. <see cref="HeSignedOff"/> comes from
/// <see cref="Ft8MessageSplit.IsSignOff"/> over what he actually sent.</para>
/// </remarks>
/// <param name="Callsign">The station.</param>
/// <param name="State">Which of the four the ledger reads.</param>
/// <param name="Slots">The slot count that state is counted over.</param>
/// <param name="LastAtUtc">
/// The slot boundary the card's time line refers to: the newest message either way
/// that is part of this conversation, or the newest thing heard from him where
/// nothing has passed between the two of them. Null where no message carries a
/// boundary.
/// </param>
/// <param name="FirstAtUtc">The oldest boundary in the conversation, or null.</param>
/// <param name="YouCalledHim">Whether Hamlet has transmitted to this station.</param>
/// <param name="HeCameBack">Whether he has sent anything addressed to the operator.</param>
/// <param name="HisMessages">How many he sent the operator.</param>
/// <param name="YourMessages">How many the operator sent him.</param>
/// <param name="HeardInAll">
/// How many transmissions from him the ledger holds, whoever they were addressed
/// to. **The evidence he is still on the air**, which is what
/// <see cref="Ft8ContactState.GoneQuiet"/> is measured from.
/// </param>
/// <param name="ReportFromHim">The last report he sent the operator, or null.</param>
/// <param name="ReportToHim">The last report the operator sent him, or null.</param>
/// <param name="HeRogered">Whether anything he sent the operator carried a roger.</param>
/// <param name="YouRogered">Whether anything the operator sent him carried a roger.</param>
/// <param name="HeSignedOff">Whether anything he sent the operator ends in `73`.</param>
/// <param name="YouSignedOff">Whether anything the operator sent him ends in `73`.</param>
/// <param name="Grid">
/// The last grid square he put on the air, whoever he was calling, or null. **His
/// own grid is his wherever he sent it**, which is why this reads everything heard
/// rather than only what came to us - the same argument
/// <see cref="Ft8ContactLogEntry"/> makes one file over.
/// </param>
/// <param name="HisLastPayload">
/// The payload of the last thing he sent the operator, or null. **What closed the
/// exchange**, where anything did.
/// </param>
/// <param name="YourLastMessage">
/// The whole text of the last thing the operator sent him, exactly as it went out,
/// or null. It is what a *send it again* would send.
/// </param>
public sealed record Ft8CardFacts(
    string Callsign,
    Ft8ContactState State,
    int Slots,
    DateTime? LastAtUtc,
    DateTime? FirstAtUtc,
    bool YouCalledHim,
    bool HeCameBack,
    int HisMessages,
    int YourMessages,
    int HeardInAll,
    int? ReportFromHim,
    int? ReportToHim,
    bool HeRogered,
    bool YouRogered,
    bool HeSignedOff,
    bool YouSignedOff,
    string? Grid,
    string? HisLastPayload,
    string? YourLastMessage)
{
    /// <summary>How many messages the conversation holds, both directions.</summary>
    /// <remarks>
    /// **WHAT *show the 6 messages* COUNTS.** It is the two directions of this
    /// conversation and never <see cref="HeardInAll"/>: a station working three
    /// others at once has transmissions in the ledger that are not part of this
    /// exchange, and offering to show six when four of them are somebody else's
    /// traffic would be a count the panel cannot honour.
    /// </remarks>
    public int Messages => HisMessages + YourMessages;

    /// <summary>Whether either side sent a report.</summary>
    public bool AnyReport => ReportFromHim is not null || ReportToHim is not null;

    /// <summary>Whether reports went both ways.</summary>
    public bool ReportsBothWays
        => ReportFromHim is not null && ReportToHim is not null;

    /// <summary>Read one station's card facts off the ledger.</summary>
    /// <param name="record">What passed with the station.</param>
    /// <param name="nowUtc">The moment the card is being read at, in corrected UTC.</param>
    /// <param name="grid">The slot grid the tab is running: 15 s on FT8, 7.5 s on FT4.</param>
    /// <returns>The facts.</returns>
    /// <exception cref="ArgumentNullException">There is no record.</exception>
    /// <remarks>
    /// <para>**THE STATE AND THE SLOT COUNT ARE <see cref="Ft8ContactStates.Read"/>'S
    /// AND ARE NOT RE-DERIVED HERE** (§0). One answer to *where does this contact
    /// stand*, and a card that disagreed with the contact line about it would be two
    /// surfaces contradicting each other on the screen he watches hardest.</para>
    /// <para>**THE REPORTS ARE READ OFF `HeardToUs` AND `Sent` AND NEVER OFF
    /// `Heard`**, which is <see cref="Ft8ContactLogEntry"/>'s rule and holds for the
    /// same reason: `-12` inside a message to a third station is what he heard
    /// **that** station at, and putting it on this card would show the operator a
    /// stranger's number as his own.</para>
    /// <para>**THE LAST REPORT EACH WAY, NOT THE FIRST**, again matching the log
    /// entry: a repeated exchange is one contact and the final answer is the one
    /// that stood.</para>
    /// </remarks>
    public static Ft8CardFacts For(
        Ft8StationRecord record, DateTime nowUtc, SlotGrid grid)
    {
        ArgumentNullException.ThrowIfNull(record);

        var read = Ft8ContactStates.Read(record, nowUtc, grid);

        var his = record.HeardToUs;
        var ours = record.Sent;

        var moments = his.Concat(ours)
            .Select(m => m.SlotStartUtc)
            .Where(at => at != default)
            .ToList();

        // **WHERE NOTHING HAS PASSED BETWEEN THE TWO OF THEM, THE CARD STILL HAS A
        // TIME.** A station calling anyone is on the card - `Ft8ContactStates.Read`
        // puts him in `YourMove` and says so - and his CQ is the only moment there
        // is. Falling back to `LastHeard` here is what stops such a card printing no
        // time at all beside a state that is entirely about him having spoken.
        var last = moments.Count > 0
            ? moments.Max()
            : record.LastHeard?.SlotStartUtc;

        return new Ft8CardFacts(
            Callsign: record.Callsign,
            State: read.State,
            Slots: read.Slots,
            LastAtUtc: last == default ? null : last,
            FirstAtUtc: moments.Count == 0 ? null : moments.Min(),
            YouCalledHim: ours.Count > 0,
            HeCameBack: record.LastHeardToUs is not null,
            HisMessages: his.Count,
            YourMessages: ours.Count,
            HeardInAll: record.Heard.Count,
            ReportFromHim: LastReport(his),
            ReportToHim: LastReport(ours),
            HeRogered: AnyRoger(his),
            YouRogered: AnyRoger(ours),
            HeSignedOff: AnySignOff(his),
            YouSignedOff: AnySignOff(ours),
            Grid: LastGrid(record.Heard),
            HisLastPayload: record.LastHeardToUs?.Fields?.Payload,
            YourLastMessage: record.LastSent?.Message);
    }

    /// <summary>The last signed report in a list, or null.</summary>
    private static int? LastReport(IReadOnlyList<Ft8LedgerMessage> messages)
    {
        for (var at = messages.Count - 1; at >= 0; at--)
        {
            var payload = messages[at].Fields?.Payload;

            if (payload is not null
                && !Ft8MessageSplit.IsCourtesy(payload)
                && Ft8MessageSplit.IsReport(payload, out _, out var decibels))
            {
                return decibels;
            }
        }

        return null;
    }

    /// <summary>Whether anything in a list rogered.</summary>
    /// <remarks>
    /// **A ROGER IS EITHER A COURTESY OR AN `R` IN FRONT OF A REPORT**, which is
    /// <see cref="Ft8ContactStates.IsComplete"/>'s own rule and is not a second copy
    /// of it: `R-15` is a roger and a report in one field because FT8 says so.
    /// </remarks>
    private static bool AnyRoger(IReadOnlyList<Ft8LedgerMessage> messages)
        => messages.Any(m => m.Fields is { } fields
            && (Ft8MessageSplit.IsCourtesy(fields.Payload)
                || (Ft8MessageSplit.IsReport(fields.Payload, out var rogered, out _)
                    && rogered)));

    /// <summary>Whether anything in a list said goodbye.</summary>
    private static bool AnySignOff(IReadOnlyList<Ft8LedgerMessage> messages)
        => messages.Any(
            m => m.Fields is { } fields && Ft8MessageSplit.IsSignOff(fields.Payload));

    /// <summary>The last grid square in a list, or null.</summary>
    private static string? LastGrid(IReadOnlyList<Ft8LedgerMessage> messages)
    {
        for (var at = messages.Count - 1; at >= 0; at--)
        {
            var payload = messages[at].Fields?.Payload;

            if (payload is not null
                && !Ft8MessageSplit.IsCourtesy(payload)
                && Ft8MessageSplit.IsGrid(payload))
            {
                return payload;
            }
        }

        return null;
    }
}

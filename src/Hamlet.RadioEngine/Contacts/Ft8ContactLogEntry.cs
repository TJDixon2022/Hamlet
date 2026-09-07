namespace Hamlet.RadioEngine.Contacts;

/// <summary>
/// The facts about the radio a log entry needs and the ledger does not hold.
/// </summary>
/// <param name="FrequencyHz">The dial, in hertz, or null where it is not known.</param>
/// <param name="Band">The band's name, or null.</param>
/// <param name="Mode">The mode, or null.</param>
/// <param name="OperatorGridSquare">The operator's own locator, or null.</param>
/// <remarks>
/// **THESE ARE HANDED IN BECAUSE THE LEDGER IS NOT A RADIO** (work instruction
/// 274 task 1). It is mode-neutral and dial-neutral on purpose, so CW inherits it
/// when CW send arrives, and a ledger holding a frequency would hold the one it
/// was constructed at rather than the one a contact happened on.
/// </remarks>
public sealed record Ft8StationConditions(
    long? FrequencyHz,
    string? Band,
    string? Mode,
    string? OperatorGridSquare);

/// <summary>
/// One contact, built out of what the ledger heard and what the radio was doing.
/// </summary>
/// <remarks>
/// <para>**IT COUNTS. IT DOES NOT INFER** (§12.1). Every field below is either a
/// message that passed or a fact handed in; nothing is a reading of what anybody
/// meant, and **a field with no evidence behind it comes back null** so the dialog
/// leaves it empty and the ADIF record omits it.</para>
/// <para>**THIS IS THE SURFACE §0.0 IS MOST EXPOSED TO IN THIS UNIT.** A log entry
/// carrying a report Hamlet never heard is a false record of what happened on the
/// air, and it outlives everything else in this project: there is nothing later
/// that could tell it from a true one.</para>
/// </remarks>
public static class Ft8ContactLogEntry
{
    /// <summary>Build the entry for one station out of the ledger.</summary>
    /// <param name="record">What passed with that station.</param>
    /// <param name="operatorCallsign">The operator's own callsign.</param>
    /// <param name="conditions">What the radio was doing.</param>
    /// <returns>The contact, with unobserved fields null.</returns>
    /// <exception cref="ArgumentNullException">The record is null.</exception>
    /// <remarks>
    /// <para>**THE REPORTS ARE READ OFF `HeardToUs` AND `Sent`, AND NEVER OFF
    /// `Heard`.** A station working three others at once spends most of its
    /// transmissions on somebody else, and a report inside one of those is a
    /// report to somebody else. Putting it in this entry would be a false record
    /// of what passed between these two.</para>
    /// <para>**THE LAST REPORT EACH WAY, NOT THE FIRST.** A repeated exchange is
    /// one contact and the operator's final answer is the one that stood; taking
    /// the first would log a report that was superseded while both were on the
    /// air.</para>
    /// <para>**THE OPERATOR'S OWN REPORT EXISTS ONLY WHERE HAMLET SENT IT.**
    /// `Ft8ContactLedger.RecordSent` has one call site and it is on the send path,
    /// booked from what actually went out. A contact answered on another program
    /// has no sent report, and it stays null rather than being filled from what
    /// the menu offered: **an offer is not a transmission.**</para>
    /// <para>**THE TIMES SPAN EVERY MESSAGE EITHER WAY**, so a contact that opened
    /// with his CQ and closed with the operator's `73` reads as the whole exchange
    /// rather than the half of it the operator heard.</para>
    /// </remarks>
    public static AdifContact For(
        Ft8StationRecord record,
        string? operatorCallsign,
        Ft8StationConditions? conditions = null)
    {
        ArgumentNullException.ThrowIfNull(record);

        var his = record.HeardToUs;
        var ours = record.Sent;

        var moments = his.Concat(ours).Select(m => m.SlotStartUtc).ToList();

        return new AdifContact
        {
            Call = Blank(record.Callsign),
            StationCallsign = Blank(operatorCallsign),

            StartedUtc = moments.Count == 0 ? null : moments.Min(),
            EndedUtc = moments.Count == 0 ? null : moments.Max(),

            // **A GRID IS THE SENDER'S OWN AND A REPORT IS THE PAIR'S**, which is
            // why these two read different lists. `CQ IK4LZH JN54` states where
            // IK4LZH is, and it is where he is whoever he was calling — so the
            // grid may be read off anything he sent. A report cannot: `-12` in a
            // message to W1ABC is what he heard W1ABC at, and putting it in this
            // entry would be a stranger's number in the operator's log.
            GridSquare = LastGrid(record.Heard),
            ReportReceived = LastReport(his),
            ReportSent = LastReport(ours),

            Band = Blank(conditions?.Band),
            Mode = Blank(conditions?.Mode),

            // **HERTZ TO MEGAHERTZ HERE AND NOWHERE ELSE**, so the one place that
            // knows what ADIF wants is the one place that converts.
            FrequencyMhz = conditions?.FrequencyHz is { } hz && hz > 0
                ? hz / 1_000_000.0
                : null,

            MyGridSquare = Blank(conditions?.OperatorGridSquare),

            // **THE NOTES ARE HIS AND ARE NEVER FILLED HERE.** Nothing observed
            // goes in the comment and nothing typed goes anywhere else.
            Comment = null,
        };
    }

    /// <summary>The last grid square this station sent, or null.</summary>
    private static string? LastGrid(IReadOnlyList<Ft8LedgerMessage> messages)
    {
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            var payload = messages[i].Fields?.Payload.Trim();

            // **THE COURTESIES ARE TESTED FIRST BY `IsGrid`'S OWN CALLERS** and
            // `RR73` is letters then digits, so it is excluded here the way
            // `Ft8Vocabulary.Explain` and the ledger exclude it: by asking whether
            // it is a report or a courtesy before asking whether it is a grid.
            if (payload is null || IsCourtesy(payload))
            {
                continue;
            }

            if (Ft8MessageSplit.IsGrid(payload))
            {
                return payload.ToUpperInvariant();
            }
        }

        return null;
    }

    /// <summary>The last signal report in these messages, or null.</summary>
    /// <remarks>
    /// **THE REPORT AND NOT THE ROGER.** `R-15` carries both, and what a log wants
    /// in `RST_SENT` is the number. The roger is the acknowledgement the contact
    /// state counts and it is not part of the report.
    /// </remarks>
    private static string? LastReport(IReadOnlyList<Ft8LedgerMessage> messages)
    {
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            var payload = messages[i].Fields?.Payload.Trim();

            if (payload is null || IsCourtesy(payload))
            {
                continue;
            }

            if (Ft8MessageSplit.IsReport(payload, out _, out var decibels))
            {
                return decibels.ToString("+00;-00;+00",
                    System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        return null;
    }

    /// <summary>`RRR`, `RR73` and `73`, which are neither grids nor reports.</summary>
    private static bool IsCourtesy(string payload)
        => payload.Equals("RRR", StringComparison.OrdinalIgnoreCase)
           || payload.Equals("RR73", StringComparison.OrdinalIgnoreCase)
           || payload.Equals("73", StringComparison.OrdinalIgnoreCase);

    private static string? Blank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

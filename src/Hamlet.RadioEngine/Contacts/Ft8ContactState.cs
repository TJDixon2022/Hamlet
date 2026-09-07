using System.Globalization;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>Which of the four things a row says about a station.</summary>
/// <remarks>
/// **THE WORDS ARE `PHASE_PLAN.md`'S AND ARE NOT TO BE RENAMED**, and there are
/// four of them. A fifth is not permitted.
/// </remarks>
public enum Ft8ContactState
{
    /// <summary>The operator spoke last and the station has not come back.</summary>
    WaitingOnHim,

    /// <summary>The station spoke to the operator last and it is his to answer.</summary>
    YourMove,

    /// <summary>The exchange has what a QSO needs.</summary>
    Complete,

    /// <summary>Nothing has been heard from the station for a stated count of slots.</summary>
    GoneQuiet,
}

/// <summary>What a row says about one station, and the slot count beside it.</summary>
/// <param name="Callsign">The station.</param>
/// <param name="State">Which of the four.</param>
/// <param name="Slots">How many slots the state is counted over.</param>
public sealed record Ft8ContactRead(string Callsign, Ft8ContactState State, int Slots)
{
    /// <summary>The four states in the plan's own words.</summary>
    public string Words => State switch
    {
        Ft8ContactState.WaitingOnHim => "waiting on him",
        Ft8ContactState.YourMove => "your move",
        Ft8ContactState.Complete => "complete",
        Ft8ContactState.GoneQuiet => "gone quiet",
        _ => "",
    };

    /// <summary>The state and its slot count, as a reader sees it.</summary>
    /// <remarks>
    /// **THE COUNT IS ALWAYS SHOWN AND IS ALWAYS A COUNT OF SLOTS**, never a
    /// duration in words and never a verdict. *Gone quiet, 11 slots* is a
    /// measurement; *gone quiet* on its own invites the reader to supply a reason.
    /// </remarks>
    public string Text => Words + ", " + Slots.ToString(CultureInfo.InvariantCulture)
        + (Slots == 1 ? " slot" : " slots");
}

/// <summary>
/// **The four states, derived from the ledger and from nothing else.**
/// </summary>
/// <remarks>
/// <para>**IT REPORTS; IT DOES NOT RULE** (`PHASE_PLAN.md`). A contact is never
/// closed by the app. `73` is politeness, not a requirement - nobody is obliged
/// to send one, an operator may be working three stations at once, and Hamlet is
/// not the radio police. **Nothing here hides, closes, forbids, greys out or
/// sorts away anything**, and there is no member on this type or on
/// <see cref="Ft8ContactRead"/> that could: they answer what state a row shows
/// and offer no way to withhold anything from anybody.</para>
/// <para>**IT INTERPRETS NOTHING** (12.1). Completeness is counted off the shapes
/// of the payload fields - a grid is four Maidenhead characters, a report carries
/// its sign, an acknowledgement is one of the standard courtesies - which is
/// bookkeeping about the format, not a reading of what a station meant.</para>
/// <para>**GONE QUIET IS MEASURED FROM HIS SILENCE AND NEVER FROM THE
/// OPERATOR'S.** <see cref="Ft8StationRecord.Heard"/> holds every transmission
/// from a station whoever it was addressed to, and that is what the clock runs
/// on. A station working three others at once is transmitting constantly while
/// answering us rarely; a rule that counted the gaps in his replies *to us* would
/// call him gone quiet while he was on the air, and would pass every other case
/// in the corpus while doing it.</para>
/// </remarks>
public static class Ft8ContactStates
{
    /// <summary>
    /// How many slots of silence before a station reads as gone quiet.
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS A CHOICE AND NOT A SPECIFICATION.** No document in this
    /// repository says when an FT8 station has gone quiet - there is no pinned
    /// ruling, no standard clause and no shack fact - so the number is argued
    /// here and is the owner's to change.</para>
    /// <para>**THE ARITHMETIC.** A slot is `Ft8Slots.SlotSeconds`, fifteen
    /// seconds, so four slots is **sixty seconds**. An FT8 station transmits in
    /// alternate slots, so its own transmission opportunities come round every
    /// two slots: four slots is **two consecutive opportunities gone by without
    /// the station using either**. One missed opportunity is ordinary - a lost
    /// decode, a station listening, an operator typing. Two in a row is the point
    /// at which *how long ago* is worth showing instead of *whose turn it is*.
    /// </para>
    /// <para>**IT IS DELIBERATELY NOT LARGE.** A generous threshold makes the row
    /// say *your move* about a station that left twenty minutes ago, which is the
    /// row being wrong quietly. The count of slots is shown beside the state
    /// either way, so a reader can always see how stale the answer is.</para>
    /// </remarks>
    public const int GoneQuietAfterSlots = 4;

    /// <summary>How many seconds of silence that is.</summary>
    public const double GoneQuietAfterSeconds =
        GoneQuietAfterSlots * Ft8Slots.SlotSeconds;

    /// <summary>What a row says about one station at a given moment.</summary>
    /// <param name="record">What passed with the station.</param>
    /// <param name="nowUtc">The moment the row is being read at.</param>
    /// <returns>The state and the slot count shown with it.</returns>
    /// <exception cref="ArgumentNullException">There is no record.</exception>
    /// <remarks>
    /// **THE ORDER THE FOUR ARE TESTED IN, AND WHY.** Complete first, because an
    /// exchange that has what a QSO needs stays complete however long the silence
    /// afterwards runs - which is what makes `73` arriving late, or never,
    /// change nothing. Gone quiet next, because *how long since he transmitted at
    /// all* outranks *whose turn it is* once the silence is long enough to matter.
    /// Then the turn, which is simply whichever of the two directions spoke last.
    /// </remarks>
    public static Ft8ContactRead Read(Ft8StationRecord record, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(record);

        var heard = record.LastHeard;
        var heardToUs = record.LastHeardToUs;
        var sent = record.LastSent;

        // COMPLETE. Both calls, both grids or reports, both acknowledgements -
        // and 73's absence never withholds it.
        if (IsComplete(record))
        {
            var last = Later(heardToUs?.SlotStartUtc, sent?.SlotStartUtc) ?? nowUtc;

            return new Ft8ContactRead(
                record.Callsign,
                Ft8ContactState.Complete,
                Ft8StationRecord.SlotsAgo(last, nowUtc));
        }

        // GONE QUIET. A count of slots since he last transmitted, whoever he was
        // transmitting to. Never a verdict about the station and never a reason.
        var sinceHeard = record.SlotsSinceHeard(nowUtc);

        if (sinceHeard >= GoneQuietAfterSlots)
        {
            return new Ft8ContactRead(
                record.Callsign, Ft8ContactState.GoneQuiet, sinceHeard.Value);
        }

        // WHOSE TURN IT IS. Whichever direction spoke last, where speaking to us
        // means a message addressed to the operator - a station's CQ is addressed
        // to anyone and does not answer the operator's last transmission.
        var his = heardToUs?.SlotStartUtc;
        var ours = sent?.SlotStartUtc;

        if (ours is not null && (his is null || ours >= his))
        {
            return new Ft8ContactRead(
                record.Callsign,
                Ft8ContactState.WaitingOnHim,
                Ft8StationRecord.SlotsAgo(ours.Value, nowUtc));
        }

        // YOUR MOVE, including a station heard calling anyone and not yet
        // answered: he has spoken and nothing has gone back.
        var theirs = his ?? heard?.SlotStartUtc ?? nowUtc;

        return new Ft8ContactRead(
            record.Callsign,
            Ft8ContactState.YourMove,
            Ft8StationRecord.SlotsAgo(theirs, nowUtc));
    }

    /// <summary>Whether an exchange has what a QSO needs.</summary>
    /// <param name="record">What passed with the station.</param>
    /// <returns>True when it is complete.</returns>
    /// <remarks>
    /// <para>**BOTH CALLS, BOTH GRIDS OR REPORTS, BOTH ACKNOWLEDGEMENTS** - the
    /// plan's own three, counted over the messages that passed **between the two
    /// stations** and not over a CQ addressed to anybody.</para>
    /// <para>**`73` IS NOT ON THE LIST AND ITS ABSENCE NEVER WITHHOLDS
    /// COMPLETE.** It is politeness. Nobody is obliged to send one, and a
    /// contact that ends `RRR` is a contact. A `73` that arrives afterwards is
    /// another message in the ledger and changes nothing.</para>
    /// <para>**AND ONE MESSAGE MAY DO TWO JOBS**, because FT8 says so: `R-15` is
    /// a roger and a report in one field, and it counts as both. That is the
    /// format's arithmetic, not Hamlet reading intent into it.</para>
    /// </remarks>
    public static bool IsComplete(Ft8StationRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        // BOTH CALLS: he has addressed the operator and the operator has
        // addressed him, so each has the other's callsign.
        if (record.LastHeardToUs is null || record.Sent.Count == 0)
        {
            return false;
        }

        var his = record.HeardToUs
            .Where(m => m.Fields is not null)
            .Select(m => m.Fields!.Payload)
            .ToList();

        var ours = record.Sent
            .Where(m => m.Fields is not null)
            .Select(m => m.Fields!.Payload)
            .ToList();

        return his.Any(IsGridOrReport) && his.Any(IsAcknowledgement)
            && ours.Any(IsGridOrReport) && ours.Any(IsAcknowledgement);
    }

    private static DateTime? Later(DateTime? left, DateTime? right)
        => left is null ? right
            : right is null ? left
            : left > right ? left : right;

    /// <summary>A grid square or a signal report, by the field's own shape.</summary>
    /// <param name="payload">The payload field.</param>
    /// <returns>True when it carries a grid or a report.</returns>
    /// <remarks>
    /// **THE COURTESIES ARE TESTED FIRST**, because `RR73` has a grid square's
    /// shape - two letters in A to R, then two digits - and is not one.
    /// `Ft8Vocabulary.Explain` has always ordered its tests the same way.
    /// </remarks>
    private static bool IsGridOrReport(string payload)
    {
        if (IsCourtesy(payload))
        {
            return false;
        }

        return Ft8MessageSplit.IsReport(payload, out _, out _)
            || Ft8MessageSplit.IsGrid(payload);
    }

    /// <summary>An acknowledgement: a roger, in any of the shapes FT8 sends one.</summary>
    /// <param name="payload">The payload field.</param>
    /// <returns>True when it acknowledges.</returns>
    private static bool IsAcknowledgement(string payload)
        => IsCourtesy(payload)
            || (Ft8MessageSplit.IsReport(payload, out var rogered, out _) && rogered);

    private static bool IsCourtesy(string payload)
        => payload is "RRR" or "RR73" or "R73" or "73";
}

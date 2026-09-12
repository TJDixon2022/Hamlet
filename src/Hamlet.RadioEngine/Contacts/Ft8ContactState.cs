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
    /// <para>**THE ARITHMETIC, AND IT IS IN SLOTS RATHER THAN IN SECONDS.** A
    /// station transmits in alternate slots, so its own transmission opportunities
    /// come round every two slots: four slots is **two consecutive opportunities
    /// gone by without the station using either**. One missed opportunity is
    /// ordinary - a lost decode, a station listening, an operator typing. Two in a
    /// row is the point at which *how long ago* is worth showing instead of *whose
    /// turn it is*. **That argument is about opportunities and not about seconds**,
    /// which is why it carries unchanged from FT8's fifteen-second slot to FT4's
    /// seven-and-a-half-second one: sixty seconds on FT8 and thirty on FT4, and
    /// two missed turns on both.</para>
    /// <para>**IT IS DELIBERATELY NOT LARGE.** A generous threshold makes the row
    /// say *your move* about a station that left twenty minutes ago, which is the
    /// row being wrong quietly. The count of slots is shown beside the state
    /// either way, so a reader can always see how stale the answer is.</para>
    /// </remarks>
    public const int GoneQuietAfterSlots = 4;

    /// <summary>
    /// How many slots must pass after the operator transmits before the station
    /// he called can be said to have had his turn and not used it.
    /// </summary>
    /// <remarks>
    /// <para>**§R18, TIM 2026-09-11.** He answered a station at 21:56:15 and
    /// twenty-three seconds later the card said *Gone quiet* and dimmed, because
    /// the clock above was counting from **the station's** last transmission and
    /// the station had been quiet for a minute before he ever called it. The
    /// card was reporting a silence that predated the question.</para>
    /// <para>**WHY TWO AND NOT ONE.** A station transmits in the slot after the
    /// one you transmitted in. If the operator sends in slot N, the station's
    /// opportunity is slot N+1, and that opportunity is not spent until N+1 has
    /// ended - which is when the count reaches two. At a count of one the
    /// operator is inside the very slot the station is answering in, and calling
    /// that silence would dim a card while the reply was on the air.</para>
    /// <para>**IT IS A FLOOR AND NEVER A CEILING.** Both this and
    /// <see cref="GoneQuietAfterSlots"/> have to be satisfied, so calling a
    /// station changes nothing about a station who was already long gone: the
    /// count from his own silence still has to reach four.</para>
    /// </remarks>
    public const int HisTurnAfterOurSendSlots = 2;

    /// <summary>How many seconds of silence that is, on the grid the tab is running.</summary>
    /// <param name="grid">The grid the tab is running.</param>
    /// <returns>Sixty seconds on FT8, thirty on FT4.</returns>
    /// <remarks>
    /// **A DERIVED FIGURE AND NOT THE DECISION** (work instruction 294 task 2). It
    /// was `const double GoneQuietAfterSeconds = GoneQuietAfterSlots *
    /// Ft8Slots.SlotSeconds`, sixty on both modes, and it was misleading rather
    /// than wrong: **nothing in `src/` ever read it**. <see cref="Read"/> decides
    /// in slots, at the line below, and always did. Two test messages print it, and
    /// a figure printed beside a row is a thing Hamlet asserts to the operator, so
    /// it follows the grid rather than staying at FT8's number.
    /// </remarks>
    public static double GoneQuietAfterSeconds(SlotGrid grid)
        => GoneQuietAfterSlots * grid.SlotSeconds;

    /// <summary>What a row says about one station at a given moment.</summary>
    /// <param name="record">What passed with the station.</param>
    /// <param name="nowUtc">The moment the row is being read at.</param>
    /// <param name="grid">The grid the tab is running: 15 s on FT8, 7.5 s on FT4.</param>
    /// <returns>The state and the slot count shown with it.</returns>
    /// <exception cref="ArgumentNullException">There is no record.</exception>
    /// <remarks>
    /// <para>**THE ORDER THE FOUR ARE TESTED IN, AND WHY.** Complete first, because
    /// an exchange that has what a QSO needs stays complete however long the
    /// silence afterwards runs - which is what makes `73` arriving late, or never,
    /// change nothing. Gone quiet next, because *how long since he transmitted at
    /// all* outranks *whose turn it is* once the silence is long enough to matter.
    /// Then the turn, which is simply whichever of the two directions spoke last.
    /// </para>
    /// <para>**EVERY COUNT ON THIS PATH IS ON ONE GRID AND IT IS THE ONE PASSED
    /// IN.** The four counts below and the threshold they are tested against all
    /// come from <see cref="Ft8StationRecord.SlotsAgo"/> with this grid, so *gone
    /// quiet* and *slots ago* cannot follow different modes. They sit in the same
    /// row and one following FT4 while the other followed FT8 would be worse than
    /// neither.</para>
    /// </remarks>
    public static Ft8ContactRead Read(Ft8StationRecord record, DateTime nowUtc, SlotGrid grid)
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
                Ft8StationRecord.SlotsAgo(last, nowUtc, grid));
        }

        // GONE QUIET. A count of slots since he last transmitted, whoever he was
        // transmitting to. Never a verdict about the station and never a reason.
        //
        // **IN SLOTS, WHICH IS WHY THE THRESHOLD NEEDED NO REPAIR OF ITS OWN**
        // (work instruction 294 task 2). `sinceHeard` is SlotsAgo's answer on the
        // grid above, so putting the grid into SlotsAgo put it here too: on FT4
        // this used to trip after eight slots because it was counting FT8's.
        var sinceHeard = record.SlotsSinceHeard(nowUtc, grid);

        // **AND THE CLOCK STARTS AT THE OPERATOR'S TRANSMISSION WHERE HE SPOKE
        // LAST** (§R18). *Gone quiet* is about a station who had his turn and did
        // not use it. A station the operator has just called has not had his turn
        // yet, however long he was silent beforehand, so the count from his own
        // silence is not the whole question once we have spoken after him.
        if (sinceHeard >= GoneQuietAfterSlots && HeHasHadHisTurn(record, nowUtc, grid))
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
                Ft8StationRecord.SlotsAgo(ours.Value, nowUtc, grid));
        }

        // YOUR MOVE, including a station heard calling anyone and not yet
        // answered: he has spoken and nothing has gone back.
        var theirs = his ?? heard?.SlotStartUtc ?? nowUtc;

        return new Ft8ContactRead(
            record.Callsign,
            Ft8ContactState.YourMove,
            Ft8StationRecord.SlotsAgo(theirs, nowUtc, grid));
    }

    /// <summary>
    /// **What the contact column shows for one decoded message, which is nothing
    /// unless the message is addressed to the operator.**
    /// </summary>
    /// <param name="message">The message exactly as it decoded.</param>
    /// <param name="operatorCallsign">The operator's own callsign.</param>
    /// <param name="record">What passed with the sender, or null where nothing has.</param>
    /// <param name="slotUtc">The boundary of the slot the message was in.</param>
    /// <param name="grid">The grid the tab is running: 15 s on FT8, 7.5 s on FT4.</param>
    /// <returns>The state and its slot count, or "" where the column says nothing.</returns>
    /// <remarks>
    /// <para>**TIM'S RULING, 2026-09-07: THE CONTACT COLUMN SPEAKS ONLY ABOUT
    /// CONTACTS HE IS IN.** *"The your move text is obnoxious."* Unit 266 put a
    /// state on every row that had a sender, so `K9TC KJ6IX RRR` - KJ6IX telling
    /// K9TC he received, with Tim in none of it - read `your move, 0 slots`, as
    /// though he owed a stranger's conversation an answer.</para>
    /// <para>**THREE THINGS GET NOTHING, AND EACH FOR ITS OWN REASON.** A message
    /// that is not three plain fields names nobody. **A CQ is not a contact** - it
    /// is an invitation, and answering it is what would begin one. **Two other
    /// stations working each other** are not his contact whatever they are saying
    /// to one another.</para>
    /// <para>**THE LEDGER IS UNCHANGED AND IS NOT CONSULTED ABOUT THIS.** Unit 266
    /// built it and it goes on booking every sender it hears, including stations
    /// Tim is not working, because *how long since he transmitted at all* is
    /// measured from all of them. **This is what the column shows, not what is
    /// tracked** - so the right-click menu, the send options and the Send area's
    /// own line are all untouched, and a station whose column is blank still has a
    /// full record behind it.</para>
    /// <para>**THE ADDRESSEE IS ASKED OF <see cref="Ft8MessageSplit"/> AND OF
    /// NOTHING ELSE**, by the same two methods
    /// <see cref="Ft8ContactLedger.RecordHeard"/> asks - so the column and the
    /// ledger cannot come to different answers about who a message was addressed
    /// to. Compound and portable forms of his call are his
    /// (<see cref="Ft8MessageSplit.IsSameStation"/>): a row addressed to
    /// `KC3QIS/P` while he is running portable is addressed to him.</para>
    /// <para>**IT REPORTS AND IT RULES NOTHING** (`PHASE_PLAN.md`). An empty column
    /// hides no row, closes no contact and withholds no message; the row is on the
    /// table with its callsign, its ratio and its whole right-click menu exactly as
    /// before. The column simply has nothing to say about a conversation he is not
    /// in.</para>
    /// </remarks>
    public static string ColumnTextFor(
        string? message,
        string? operatorCallsign,
        Ft8StationRecord? record,
        DateTime slotUtc,
        SlotGrid grid)
    {
        if (record is null || string.IsNullOrWhiteSpace(operatorCallsign))
        {
            return "";
        }

        // **THE THREE REFUSALS ARE ONE PREDICATE SINCE WORK INSTRUCTION 273**,
        // `Ft8MessageSplit.IsAddressedTo`: not three plain fields names nobody, a
        // CQ is an invitation rather than a contact, and two other stations
        // working each other are not his. The behaviour is exactly what unit 271
        // wrote and every one of its tests still asserts it.
        //
        // **IT MOVED BECAUSE THE DECODED AREA NOW ASKS THE SAME QUESTION.** Work
        // instruction 273 gives the panel a right-hand list of what is addressed
        // to him, and two copies of *is this for him* would disagree on the
        // screen: a row on that side with a blank contact column, or a state
        // beside a row he cannot find.
        if (!Ft8MessageSplit.IsAddressedTo(message, operatorCallsign))
        {
            return "";
        }

        return Read(record, slotUtc, grid).Text;
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

    /// <summary>Whether the station has had a turn since the operator called him.</summary>
    /// <param name="record">What passed with the station.</param>
    /// <param name="nowUtc">The moment the row is being read at.</param>
    /// <param name="grid">The grid the tab is running.</param>
    /// <returns>
    /// True where the operator did not speak last, or where he did and at least
    /// <see cref="HisTurnAfterOurSendSlots"/> slots have passed since.
    /// </returns>
    /// <remarks>
    /// <para>**THE COMPARISON IS AGAINST EVERYTHING HE SENT, NOT AGAINST WHAT HE
    /// SENT US**, which is the same rule the gone-quiet count itself follows. A
    /// station who transmitted to somebody else after the operator called him has
    /// been on the air since, and the operator's send is no longer the last thing
    /// that happened.</para>
    /// <para>**NO SEND AT ALL MEANS NOTHING IS DEFERRED.** A station the operator
    /// has never called reads exactly as it did before this rule existed.</para>
    /// </remarks>
    private static bool HeHasHadHisTurn(
        Ft8StationRecord record, DateTime nowUtc, SlotGrid grid)
    {
        if (record.LastSent is not { } ours)
        {
            return true;
        }

        if (record.LastHeard is { } his && his.SlotStartUtc > ours.SlotStartUtc)
        {
            return true;
        }

        return Ft8StationRecord.SlotsAgo(ours.SlotStartUtc, nowUtc, grid)
            >= HisTurnAfterOurSendSlots;
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

    /// <remarks>
    /// **THE FOUR STRINGS MOVED TO <see cref="Ft8MessageSplit.IsCourtesy"/>**
    /// (work instruction 297 task 2), so that the sign-off test can sit beside
    /// them and there is one answer to *what is a courtesy* rather than two.
    /// Behaviour here is unchanged.
    /// </remarks>
    private static bool IsCourtesy(string payload)
        => Ft8MessageSplit.IsCourtesy(payload);
}

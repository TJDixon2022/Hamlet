using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>One message the ledger holds, and the slot it was in.</summary>
/// <param name="Message">The text, exactly as it was sent or decoded.</param>
/// <param name="Fields">Its three fields, or null where the splitter refused it.</param>
/// <param name="SlotStartUtc">The boundary of the slot it occupied.</param>
public sealed record Ft8LedgerMessage(
    string Message, Ft8MessageFields? Fields, DateTime SlotStartUtc);

/// <summary>
/// What passed each way between the operator and one station, and when.
/// </summary>
/// <remarks>
/// **IT COUNTS. IT DOES NOT RULE.** Nothing here hides a station, closes a
/// contact, forbids a message or returns a verdict about anybody's operating.
/// </remarks>
public sealed class Ft8StationRecord
{
    private readonly List<Ft8LedgerMessage> _heard = [];
    private readonly List<Ft8LedgerMessage> _heardToUs = [];
    private readonly List<Ft8LedgerMessage> _sent = [];

    internal Ft8StationRecord(string callsign) => Callsign = callsign;

    /// <summary>Whose station this is.</summary>
    public string Callsign { get; }

    /// <summary>
    /// Every message heard from this station, in the order it arrived.
    /// </summary>
    /// <remarks>
    /// **WHOEVER IT WAS ADDRESSED TO.** A station working three others at once
    /// spends most of its transmissions on somebody else, and those
    /// transmissions are the evidence that it has not gone quiet. Filtering them
    /// out here would leave the ledger measuring the operator's silence and
    /// calling it the station's.
    /// </remarks>
    public IReadOnlyList<Ft8LedgerMessage> Heard => _heard;

    /// <summary>
    /// Every message heard from this station addressed to the operator, in order.
    /// </summary>
    /// <remarks>
    /// A subset of <see cref="Heard"/>, kept separately because *whose turn it
    /// is* is about what passed between the two of them while *how long since he
    /// transmitted* is about everything he sent. **A station's CQ is addressed to
    /// anyone and is not in here**, so it never reads as an answer to the
    /// operator's last transmission.
    /// </remarks>
    public IReadOnlyList<Ft8LedgerMessage> HeardToUs => _heardToUs;

    /// <summary>Every message the operator sent to this station, in order.</summary>
    public IReadOnlyList<Ft8LedgerMessage> Sent => _sent;

    /// <summary>The last message heard from this station, whoever it was to.</summary>
    public Ft8LedgerMessage? LastHeard => _heard.Count == 0 ? null : _heard[^1];

    /// <summary>The last message heard from this station addressed to the operator.</summary>
    public Ft8LedgerMessage? LastHeardToUs
        => _heardToUs.Count == 0 ? null : _heardToUs[^1];

    /// <summary>The last message the operator sent to this station.</summary>
    public Ft8LedgerMessage? LastSent => _sent.Count == 0 ? null : _sent[^1];

    /// <summary>How many slots ago this station was last heard.</summary>
    /// <param name="nowUtc">The moment being read at.</param>
    /// <param name="grid">The grid the tab is running: 15 s on FT8, 7.5 s on FT4.</param>
    /// <returns>The count of slot boundaries crossed, or null where never heard.</returns>
    public int? SlotsSinceHeard(DateTime nowUtc, SlotGrid grid)
        => LastHeard is null ? null : SlotsAgo(LastHeard.SlotStartUtc, nowUtc, grid);

    /// <summary>How many slots ago the operator last sent to this station.</summary>
    /// <param name="nowUtc">The moment being read at.</param>
    /// <param name="grid">The grid the tab is running: 15 s on FT8, 7.5 s on FT4.</param>
    /// <returns>The count of slot boundaries crossed, or null where never sent.</returns>
    public int? SlotsSinceSent(DateTime nowUtc, SlotGrid grid)
        => LastSent is null ? null : SlotsAgo(LastSent.SlotStartUtc, nowUtc, grid);

    /// <summary>How many slots ago a moment was, on the grid the tab is running.</summary>
    /// <param name="thenUtc">The earlier slot boundary.</param>
    /// <param name="nowUtc">The moment being read at.</param>
    /// <param name="grid">The grid the tab is running.</param>
    /// <returns>The number of slot boundaries strictly after <paramref name="thenUtc"/>.</returns>
    /// <remarks>
    /// <para>**THE ARITHMETIC IS `SlotGrid.BoundariesBetween` AND THERE IS NO
    /// SECOND COPY OF IT.** Not `(now - then).TotalSeconds / 15`: that is a
    /// second copy of the slot period, it drifts on the rounding
    /// `Ft8Slots`'s own epsilon remark was written about, and it answers 0 for
    /// two slot starts 14.9 s apart. **Work instruction 294 task 2 found the
    /// second copy this paragraph says does not exist**, at
    /// `MainWindowViewModel.Quiet`, doing exactly the division named above; it was
    /// removed rather than threaded, and that method now calls this one.</para>
    /// <para>**THE GRID IS A PARAMETER AND HAS NO DEFAULT** (work instruction 294
    /// task 2). It used to be `Ft8Slots.BoundariesBetween`, the static
    /// fifteen-second one, whatever mode the tab was running: **a station heard
    /// four FT4 slots ago read as two**, and *gone quiet* - which is this count
    /// against <see cref="Ft8ContactStates.GoneQuietAfterSlots"/>, in slots -
    /// tripped after eight FT4 slots rather than four, so a row said *your move*
    /// about a station that had been silent for half a minute. A default of FT8's
    /// grid would have kept every existing caller compiling and would have let the
    /// next FT4 caller reintroduce the same silence, so there is none.</para>
    /// <para>**BOUNDARIES STRICTLY AFTER `then`.** `BoundariesBetween` includes
    /// its own start when the start is itself a boundary - measured against the
    /// tree, and unit 257's survey says otherwise - so the count is filtered
    /// rather than decremented, which keeps this free of arithmetic that could
    /// be wrong by one in the other direction.</para>
    /// <para>**PUBLIC BECAUSE THE COPY HAD TO GO SOMEWHERE.** It was internal, and
    /// the one caller outside this assembly that wanted it - the view model's
    /// *heard N slots ago* line - wrote its own division instead. It is a query
    /// over two moments and a grid and it decides nothing.</para>
    /// </remarks>
    public static int SlotsAgo(DateTime thenUtc, DateTime nowUtc, SlotGrid grid)
        => grid.BoundariesBetween(thenUtc, nowUtc).Count(at => at > thenUtc);

    /// <remarks>
    /// **THE WHOLE HISTORY, AND THIS WAS WATCHED FAILING THE OTHER WAY.** Built
    /// first holding the last message alone, `K9RST`'s three transmissions came
    /// back as `["KC3QIS K9RST 73"]` against an expected
    /// `["KC3QIS K9RST EM12", "KC3QIS K9RST R-09", "KC3QIS K9RST 73"]` - his grid
    /// and his roger gone, so a completed exchange read as though it had never
    /// happened, `G4XYZ`'s six transmissions read as one, and a repeat could
    /// never be counted. **Nothing here overwrites.**
    /// </remarks>
    internal void AddHeard(Ft8LedgerMessage message, bool toUs)
    {
        _heard.Add(message);

        if (toUs)
        {
            _heardToUs.Add(message);
        }
    }

    internal void AddSent(Ft8LedgerMessage message) => _sent.Add(message);
}

/// <summary>
/// **Per station, which messages passed each way, when, and how many slots ago.**
/// </summary>
/// <remarks>
/// <para>**IT REPORTS AND IT DOES NOT RULE** (`PHASE_PLAN.md`). There is no
/// method here that hides a station, closes a contact, forbids a message or
/// returns a verdict about anybody's operating, and there is not to be one. A
/// contact is never closed by the app; `73` is politeness; a station working
/// three others at once is a station being busy.</para>
/// <para>**IT INTERPRETS NOTHING** (12.1). It divides a message into fields with
/// `Ft8MessageSplit`, which is a fact about the format, and counts what it has
/// seen. It never words what a station meant.</para>
/// <para>**THE OPERATOR'S CALLSIGN ARRIVES AS A CONSTRUCTOR PARAMETER.** This
/// type does not read settings, does not know a tab exists, opens nothing, and
/// subscribes to no telemetry (0.1). `OperatorProfile.Callsign` holds the value
/// in the app and the app passes it in.</para>
/// <para>**TWO DOORS, DIFFERING ONLY IN DIRECTION.**
/// <see cref="RecordHeard(string?, DateTime)"/> for what came out of the decoder
/// and <see cref="RecordSent(string?, DateTime)"/> for what the operator sent.
/// **Nothing calls `RecordSent` today** and nothing in unit 258 makes anything
/// call it: it exists so that step 5's send path adds one line beside the line
/// that hands the samples to the sink. `TransmitRecord` cannot tell us - it has
/// no string parameter at all, by HM-DEC-018, and that is not to be weakened.
/// </para>
/// <para>**`CQ` IS AN ADDRESSEE AND NOT A STATION**, tested by
/// `Ft8MessageSplit.IsCallToAnyone` and by no second rule. A CQ books the sender
/// and never books `CQ`.</para>
/// <para>**A MESSAGE THE SPLITTER REFUSES IS DROPPED AND NOTHING BREAKS.** Free
/// text is not a contact between two stations and there is nothing to book; the
/// alternative is guessing at the shape of a message the format does not
/// recognise.</para>
/// </remarks>
public sealed class Ft8ContactLedger
{
    private readonly Dictionary<string, Ft8StationRecord> _stations =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly List<string> _order = [];

    /// <summary>Opens a ledger for one operator.</summary>
    /// <param name="operatorCallsign">The operator's own callsign.</param>
    /// <exception cref="ArgumentException">The callsign is blank.</exception>
    public Ft8ContactLedger(string operatorCallsign)
    {
        if (string.IsNullOrWhiteSpace(operatorCallsign))
        {
            throw new ArgumentException(
                "a ledger is kept from somebody's station and needs that station's "
                + "callsign", nameof(operatorCallsign));
        }

        OperatorCallsign = operatorCallsign.Trim();
    }

    /// <summary>The operator's own callsign, as it was passed in.</summary>
    public string OperatorCallsign { get; }

    /// <summary>The key a call to anybody is booked under.</summary>
    /// <remarks>
    /// <para>**A CQ IS ADDRESSED TO NOBODY AND USED TO BOOK NOBODY** (work
    /// instruction 305 task 2). That was defensible while a card meant a contact
    /// with a station, and it meant the operator pressed CQ on a live radio, put a
    /// transmission on the air, and watched a panel that stayed completely empty.
    /// **The transmission is a fact and it now has somewhere to live.**</para>
    /// <para>**IT IS NOT A CALLSIGN AND NOTHING TREATS IT AS ONE.** No lookup is
    /// made against it, nothing is claimed about where it is, and the first station
    /// to answer takes the record over - see <see cref="Adopt"/>.</para>
    /// </remarks>
    public const string CallToAnyone = "CQ";

    /// <summary>Every station booked, in the order each was first seen.</summary>
    public IReadOnlyList<string> Stations => _order;

    /// <summary>What passed with one station, or null where none has.</summary>
    /// <param name="callsign">The station.</param>
    /// <returns>The record, or null.</returns>
    public Ft8StationRecord? For(string? callsign)
        => callsign is not null && _stations.TryGetValue(callsign.Trim(), out var found)
            ? found
            : null;

    /// <summary>Books a message the decoder returned.</summary>
    /// <param name="message">The text exactly as it was decoded.</param>
    /// <param name="slotStartUtc">The boundary of the slot it was in.</param>
    /// <remarks>
    /// The station booked is the **sender**, whoever the message was addressed
    /// to. Nothing is booked for a message the splitter refuses, for a sender
    /// that is a call to anyone, or for the operator's own callsign coming back.
    /// </remarks>
    public void RecordHeard(string? message, DateTime slotStartUtc)
    {
        var fields = Ft8MessageSplit.Split(message);

        if (fields is null || Ft8MessageSplit.IsCallToAnyone(fields.From))
        {
            return;
        }

        if (IsOperator(fields.From))
        {
            return;
        }

        var record = Book(fields.From);
        var toUs = IsOperator(fields.To);

        // **THE FIRST STATION TO ANSWER TAKES THE CQ CARD OVER** (Tim, 2026-09-10:
        // *pressing CQ must make a card of its own, and when somebody answers it
        // becomes their card and the exchange continues in it*). It happens before
        // his message is booked, so the CQ really is the first thing in the
        // exchange rather than something filed behind it.
        if (toUs)
        {
            Adopt(record);
        }

        record.AddHeard(
            new Ft8LedgerMessage(message!.Trim(), fields, slotStartUtc), toUs);
    }

    /// <summary>Books a message the operator sent.</summary>
    /// <param name="message">The text exactly as it was sent.</param>
    /// <param name="slotStartUtc">The boundary of the slot it went out in.</param>
    /// <remarks>
    /// The station booked is the **addressee**. The operator's own CQ is
    /// addressed to anyone and books nobody, which is correct: a CQ is not a
    /// contact with a station yet.
    /// </remarks>
    public void RecordSent(string? message, DateTime slotStartUtc)
    {
        var fields = Ft8MessageSplit.Split(message);

        if (fields is null)
        {
            return;
        }

        if (IsOperator(fields.To))
        {
            return;
        }

        // **A CALL TO ANYBODY IS BOOKED UNDER ITS OWN KEY** rather than dropped.
        // See <see cref="CallToAnyone"/> for what changed and why.
        var who = Ft8MessageSplit.IsCallToAnyone(fields.To)
            ? CallToAnyone
            : fields.To;

        Book(who).AddSent(
            new Ft8LedgerMessage(message!.Trim(), fields, slotStartUtc));
    }

    /// <summary>Hand the call-to-anybody record to the station that answered.</summary>
    /// <param name="record">The answering station.</param>
    /// <remarks>
    /// <para>**IT MOVES, IT DOES NOT COPY.** Two cards carrying the same
    /// transmission would say the operator called twice, which is a claim about
    /// what went on the air and is not true (§0.0).</para>
    /// <para>**AND ONLY THE FIRST ANSWER GETS IT.** A second station answering the
    /// same CQ finds nothing to adopt and opens its own record in the ordinary way -
    /// nothing is discarded and nothing is swallowed, because nothing is hidden by
    /// the application. **That half is the author's proposal and not a ruling.**
    /// </para>
    /// </remarks>
    private void Adopt(Ft8StationRecord record)
    {
        if (!_stations.TryGetValue(CallToAnyone, out var cq))
        {
            return;
        }

        foreach (var message in cq.Sent)
        {
            record.AddSent(message);
        }

        _stations.Remove(CallToAnyone);
        _order.Remove(CallToAnyone);
    }

    private bool IsOperator(string field)
        => string.Equals(field, OperatorCallsign, StringComparison.OrdinalIgnoreCase);

    private Ft8StationRecord Book(string callsign)
    {
        if (_stations.TryGetValue(callsign, out var found))
        {
            return found;
        }

        var made = new Ft8StationRecord(callsign);

        _stations[callsign] = made;
        _order.Add(callsign);

        return made;
    }
}

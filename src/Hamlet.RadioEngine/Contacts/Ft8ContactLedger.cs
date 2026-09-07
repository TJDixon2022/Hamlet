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

    /// <summary>Every message the operator sent to this station, in order.</summary>
    public IReadOnlyList<Ft8LedgerMessage> Sent => _sent;

    /// <summary>The last message heard from this station, whoever it was to.</summary>
    public Ft8LedgerMessage? LastHeard => _heard.Count == 0 ? null : _heard[^1];

    /// <summary>The last message heard from this station addressed to the operator.</summary>
    public Ft8LedgerMessage? LastHeardToUs { get; private set; }

    /// <summary>The last message the operator sent to this station.</summary>
    public Ft8LedgerMessage? LastSent => _sent.Count == 0 ? null : _sent[^1];

    /// <summary>How many slots ago this station was last heard.</summary>
    /// <param name="nowUtc">The moment being read at.</param>
    /// <returns>The count of slot boundaries crossed, or null where never heard.</returns>
    public int? SlotsSinceHeard(DateTime nowUtc)
        => LastHeard is null ? null : SlotsAgo(LastHeard.SlotStartUtc, nowUtc);

    /// <summary>How many slots ago the operator last sent to this station.</summary>
    /// <param name="nowUtc">The moment being read at.</param>
    /// <returns>The count of slot boundaries crossed, or null where never sent.</returns>
    public int? SlotsSinceSent(DateTime nowUtc)
        => LastSent is null ? null : SlotsAgo(LastSent.SlotStartUtc, nowUtc);

    /// <summary>How many slots ago a moment was.</summary>
    /// <param name="thenUtc">The earlier slot boundary.</param>
    /// <param name="nowUtc">The moment being read at.</param>
    /// <returns>The number of slot boundaries strictly after <paramref name="thenUtc"/>.</returns>
    /// <remarks>
    /// <para>**THE ARITHMETIC IS `Ft8Slots.BoundariesBetween` AND THERE IS NO
    /// SECOND COPY OF IT.** Not `(now - then).TotalSeconds / 15`: that is a
    /// second copy of the slot period, it drifts on the rounding
    /// `Ft8Slots`'s own epsilon remark was written about, and it answers 0 for
    /// two slot starts 14.9 s apart.</para>
    /// <para>**BOUNDARIES STRICTLY AFTER `then`.** `BoundariesBetween` includes
    /// its own start when the start is itself a boundary - measured against the
    /// tree, and unit 257's survey says otherwise - so the count is filtered
    /// rather than decremented, which keeps this free of arithmetic that could
    /// be wrong by one in the other direction.</para>
    /// </remarks>
    internal static int SlotsAgo(DateTime thenUtc, DateTime nowUtc)
        => Ft8Slots.BoundariesBetween(thenUtc, nowUtc).Count(at => at > thenUtc);

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
            LastHeardToUs = message;
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

        if (fields is null || Ft8MessageSplit.IsCallToAnyone(fields.To))
        {
            return;
        }

        if (IsOperator(fields.To))
        {
            return;
        }

        Book(fields.To).AddSent(
            new Ft8LedgerMessage(message!.Trim(), fields, slotStartUtc));
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

using System.Globalization;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>Which field shape a message's payload is.</summary>
public enum Ft8SendShape
{
    /// <summary>A four-character Maidenhead field.</summary>
    Grid,

    /// <summary>A signal report, signed.</summary>
    Report,

    /// <summary>A signal report carrying its roger.</summary>
    RogerAndReport,

    /// <summary>A roger on its own.</summary>
    Acknowledge,

    /// <summary>The courtesy.</summary>
    Seventy3,
}

/// <summary>One message the operator may send, as it would go on the air.</summary>
/// <param name="Shape">Which field shape its payload is.</param>
/// <param name="Text">The message exactly as it would be transmitted.</param>
/// <param name="Label">What a reader sees: the field shape, named.</param>
/// <param name="IsExpected">Whether this is the one that conventionally comes next.</param>
/// <param name="SentBefore">How many times this exact text has already gone to this station.</param>
public sealed record Ft8SendOption(
    Ft8SendShape Shape, string Text, string Label, bool IsExpected, int SentBefore);

/// <summary>Every message that is valid toward one station at one moment.</summary>
/// <param name="Callsign">The station.</param>
/// <param name="Options">The messages, in the order an exchange runs in.</param>
/// <param name="Absent">
/// Why a message the format has is not here - a missing grid, a report never
/// measured. **Said out loud rather than left blank** (0.0).
/// </param>
public sealed record Ft8SendMenu(
    string Callsign,
    IReadOnlyList<Ft8SendOption> Options,
    IReadOnlyList<string> Absent);

/// <summary>
/// **Every message that is valid toward a station, and which one conventionally
/// comes next.**
/// </summary>
/// <remarks>
/// <para>**NOTHING IS EVER WITHHELD ON ACCOUNT OF THE CONTACT STATE** (ruled
/// 2026-09-06). A complete exchange still offers `73` and everything else; a
/// station that has gone quiet offers everything. **There is no member here that
/// could hide, close, forbid, deny, block, grey, dim, disable or exclude an
/// option**, and <c>NothingOnTheseTypesCanWithholdAnOption</c> reads every public
/// member against those nine verbs and fails on a hit.</para>
/// <para>**IT WAS WATCHED FAILING THE OTHER WAY.** Built first returning only the
/// message that conventionally comes next, with a complete exchange having none,
/// <c>K9RST</c> - complete at slot 5 - came back with an empty list: no `73`, and
/// no way to send his grid a second time when the first was lost. `Expected: 5,
/// Actual: 0`. **A contact is never closed by the app**, and that version closed
/// one.</para>
/// <para>**A REPEAT IS CORRECT OPERATING AND CARRIES ITS COUNT.** FT8 loses
/// transmissions constantly, so the same message going a second time is what an
/// operator does. <see cref="Ft8SendOption.SentBefore"/> says how many times that
/// exact text has already gone; it is never a reason to remove anything.</para>
/// <para>**IT INTERPRETS NOTHING** (12.1). A label names which field shape a
/// payload is, the way <see cref="Ft8MessageSplit"/> divides fields. The expected
/// one is whatever conventionally answers the station's last message to the
/// operator - a grid answered by a report, a report by a roger, a roger by an
/// acknowledgement, an acknowledgement by `73`. That is arithmetic about the
/// format's shapes and it never says what a station meant.</para>
/// <para>**IT INVENTS NOTHING** (0.0). With no grid in Settings the grid-bearing
/// messages are **absent with the reason said out loud**, and the same is true of
/// a report that has never been measured. Absent is not forbidden: there is no
/// message to send, rather than a message withheld.</para>
/// <para>**THE ENGINE STILL DOES NOT KNOW A TAB EXISTS** (0.1). Everything
/// arrives as a parameter; nothing here reads settings, names a view or opens
/// anything.</para>
/// </remarks>
public static class Ft8SendOptions
{
    /// <summary>The order an FT8 exchange runs in, and the order the menu shows.</summary>
    private static readonly Ft8SendShape[] InExchangeOrder =
    [
        Ft8SendShape.Grid,
        Ft8SendShape.Report,
        Ft8SendShape.RogerAndReport,
        Ft8SendShape.Acknowledge,
        Ft8SendShape.Seventy3,
    ];

    /// <summary>Every message that may be sent to one station.</summary>
    /// <param name="record">What passed with the station.</param>
    /// <param name="operatorCallsign">The operator's own callsign.</param>
    /// <param name="gridSquare">His grid, or empty where Settings has none.</param>
    /// <param name="reportDecibels">The report to send, or null where none was measured.</param>
    /// <returns>The menu: the options in exchange order, and what is absent and why.</returns>
    /// <exception cref="ArgumentNullException">There is no record.</exception>
    public static Ft8SendMenu For(
        Ft8StationRecord record,
        string operatorCallsign,
        string? gridSquare,
        int? reportDecibels)
    {
        ArgumentNullException.ThrowIfNull(record);

        var expected = Expect(record);
        var options = new List<Ft8SendOption>();
        var absent = new List<string>();

        foreach (var shape in InExchangeOrder)
        {
            var text = TextFor(
                shape, record.Callsign, operatorCallsign, gridSquare, reportDecibels);

            if (text is null)
            {
                continue;
            }

            options.Add(new Ft8SendOption(
                shape, text, LabelFor(shape), shape == expected,
                AlreadySent(record, text)));
        }

        if (string.IsNullOrWhiteSpace(gridSquare))
        {
            absent.Add(
                "the grid square is not set in Settings, so the messages that "
                + "carry one are not offered");
        }

        if (reportDecibels is null)
        {
            absent.Add(
                "no signal report has been measured for this station, so the "
                + "messages that carry one are not offered");
        }

        return new Ft8SendMenu(record.Callsign, options, absent);
    }

    /// <summary>How many times that exact message has already gone to a station.</summary>
    private static int AlreadySent(Ft8StationRecord record, string text)
        => record.Sent.Count(
            m => string.Equals(m.Message, text, StringComparison.OrdinalIgnoreCase));

    /// <summary>The call to anyone, from the operator's own settings.</summary>
    /// <param name="operatorCallsign">His callsign.</param>
    /// <param name="gridSquare">His grid, or empty.</param>
    /// <returns>The message, exactly as it would go on the air.</returns>
    public static string CallToAnyone(string operatorCallsign, string? gridSquare)
    {
        var call = (operatorCallsign ?? "").Trim();
        var grid = (gridSquare ?? "").Trim();

        return grid.Length == 0 ? "CQ " + call : "CQ " + call + " " + grid;
    }

    /// <summary>A signal report in the shape FT8 sends one.</summary>
    /// <param name="decibels">The level, signed.</param>
    /// <returns>The payload field.</returns>
    public static string Report(int decibels)
        => (decibels < 0 ? "-" : "+")
            + Math.Abs(decibels).ToString("00", CultureInfo.InvariantCulture);

    /// <summary>What conventionally answers the station's last message to us.</summary>
    /// <remarks>
    /// **DERIVED FROM THE LAST MESSAGE HEARD FROM HIM ADDRESSED TO THE OPERATOR,
    /// AND FROM NOTHING ELSE** - not from the contact state, and never from
    /// whether the operator has already replied. **Completeness is not consulted
    /// here and must not be**: consulting it is what produced the empty menu for
    /// <c>K9RST</c>. A station heard only calling anyone is answered with the
    /// grid, which is how an FT8 operator takes a CQ; a station never heard from
    /// at all has nothing that conventionally comes next, and none is marked.
    /// </remarks>
    private static Ft8SendShape? Expect(Ft8StationRecord record)
    {
        var last = record.LastHeardToUs;

        if (last is null)
        {
            return record.LastHeard is null ? null : Ft8SendShape.Grid;
        }

        var payload = last.Fields?.Payload ?? "";

        if (payload is "RRR" or "RR73" or "R73" or "73")
        {
            return Ft8SendShape.Seventy3;
        }

        if (Ft8MessageSplit.IsReport(payload, out var rogered, out _))
        {
            return rogered ? Ft8SendShape.Acknowledge : Ft8SendShape.RogerAndReport;
        }

        return Ft8MessageSplit.IsGrid(payload) ? Ft8SendShape.Report : null;
    }

    private static string? TextFor(
        Ft8SendShape shape,
        string callsign,
        string operatorCallsign,
        string? gridSquare,
        int? reportDecibels)
    {
        var head = callsign + " " + operatorCallsign + " ";
        var grid = (gridSquare ?? "").Trim();

        return shape switch
        {
            Ft8SendShape.Grid => grid.Length == 0 ? null : head + grid,
            Ft8SendShape.Report => reportDecibels is null
                ? null : head + Report(reportDecibels.Value),
            Ft8SendShape.RogerAndReport => reportDecibels is null
                ? null : head + "R" + Report(reportDecibels.Value),
            Ft8SendShape.Acknowledge => head + "RRR",
            Ft8SendShape.Seventy3 => head + "73",
            _ => null,
        };
    }

    private static string LabelFor(Ft8SendShape shape) => shape switch
    {
        Ft8SendShape.Grid => "grid",
        Ft8SendShape.Report => "report",
        Ft8SendShape.RogerAndReport => "roger and report",
        Ft8SendShape.Acknowledge => "acknowledge",
        Ft8SendShape.Seventy3 => "73",
        _ => "",
    };
}

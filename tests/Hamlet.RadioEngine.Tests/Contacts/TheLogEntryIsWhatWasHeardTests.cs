using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// Work instruction 274, task 3: the entry is built from what the ledger heard,
/// and a field Hamlet did not observe stays empty.
/// </summary>
/// <remarks>
/// <para>**THIS IS THE SURFACE §0.0 IS MOST EXPOSED TO IN THIS UNIT.** A log entry
/// carrying a report Hamlet never heard is a false record of what happened on the
/// air, and it outlives everything else in this project: there is nothing later
/// that could tell it from a true one.</para>
/// <para>**THE REPORTS COME OFF `HeardToUs` AND `Sent`, NEVER OFF `Heard`.** A
/// station working three others at once spends most of its transmissions on
/// somebody else, and a report inside one of those is a report to somebody
/// else.</para>
/// </remarks>
public sealed class TheLogEntryIsWhatWasHeardTests
{
    private const string His = "IK4LZH";
    private const string Mine = "KC3QIS";

    private static readonly DateTime Slot =
        new(2026, 9, 7, 21, 41, 30, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the entries are printed.</param>
    public TheLogEntryIsWhatWasHeardTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A complete exchange fills every observable field.</summary>
    [Fact]
    public void ACompleteExchangeFillsEveryObservableField()
    {
        var ledger = new Ft8ContactLedger(Mine);

        // The exchange as it actually runs: his CQ, the operator's grid, his
        // report, the operator's rogered report, his roger and sign-off.
        ledger.RecordHeard($"CQ {His} JN54", Slot);
        ledger.RecordSent($"{His} {Mine} FN00", Slot.AddSeconds(15));
        ledger.RecordHeard($"{Mine} {His} -12", Slot.AddSeconds(30));
        ledger.RecordSent($"{His} {Mine} R-09", Slot.AddSeconds(45));
        ledger.RecordHeard($"{Mine} {His} RR73", Slot.AddSeconds(60));

        var entry = Ft8ContactLogEntry.For(
            ledger.For(His)!,
            Mine,
            new Ft8StationConditions(
                14_074_000, "20m", ContactModes.Named("FT8"), "FN00DJ"));

        _output.WriteLine(AdifLog.Record(entry));

        Assert.Equal(His, entry.Call);
        Assert.Equal(Mine, entry.StationCallsign);
        Assert.Equal("JN54", entry.GridSquare);
        Assert.Equal("-12", entry.ReportReceived);
        Assert.Equal("-09", entry.ReportSent);
        Assert.Equal("20m", entry.Band);
        Assert.Equal("FT8", entry.Mode);
        Assert.Equal(14.074, entry.FrequencyMhz);
        Assert.Equal("FN00DJ", entry.MyGridSquare);

        // **THE TIMES SPAN EVERY MESSAGE EITHER WAY.** His CQ is not addressed to
        // the operator, so the contact starts at the operator's own first
        // transmission and ends at his sign-off.
        Assert.Equal(Slot.AddSeconds(15), entry.StartedUtc);
        Assert.Equal(Slot.AddSeconds(60), entry.EndedUtc);

        // **THE NOTES ARE NEVER FILLED HERE.** They are his.
        Assert.Null(entry.Comment);
    }

    /// <summary>
    /// A contact answered somewhere else has no sent report, and it stays empty.
    /// </summary>
    /// <remarks>
    /// **THE CASE TASK 1 FOUND.** `RecordSent` has one call site and it is on
    /// Hamlet's own send path, booked from what actually went out. A contact he
    /// answered on another program has nothing in `Sent`, and the field must not
    /// be filled from what the menu offered — **an offer is not a
    /// transmission.**
    /// </remarks>
    [Fact]
    public void AContactAnsweredElsewhereHasNoSentReport()
    {
        var ledger = new Ft8ContactLedger(Mine);

        ledger.RecordHeard($"{Mine} {His} -12", Slot);
        ledger.RecordHeard($"{Mine} {His} RR73", Slot.AddSeconds(30));

        var entry = Ft8ContactLogEntry.For(ledger.For(His)!, Mine);

        _output.WriteLine(AdifLog.Record(entry));

        Assert.Equal("-12", entry.ReportReceived);
        Assert.Null(entry.ReportSent);

        // And the record leaves it out entirely rather than writing it blank.
        Assert.DoesNotContain(
            "RST_SENT", AdifLog.Record(entry), StringComparison.Ordinal);
    }

    /// <summary>A report to somebody else is not this contact's report.</summary>
    /// <remarks>
    /// **THE ASSERTION THIS WHOLE FILE EXISTS FOR.** `Heard` carries everything
    /// the station sent, whoever it was to, because *how long since he transmitted
    /// at all* is measured from all of it. Reading a report out of that would put
    /// a stranger's number in the operator's log.
    /// </remarks>
    [Fact]
    public void AReportToSomebodyElseIsNotOurs()
    {
        var ledger = new Ft8ContactLedger(Mine);

        // He is working W1ABC and reports +05 to him. Then he answers the
        // operator with -12.
        ledger.RecordHeard($"W1ABC {His} +05", Slot);
        ledger.RecordHeard($"{Mine} {His} -12", Slot.AddSeconds(15));

        var entry = Ft8ContactLogEntry.For(ledger.For(His)!, Mine);

        _output.WriteLine("report received : " + entry.ReportReceived);

        Assert.Equal("-12", entry.ReportReceived);
        Assert.NotEqual("+05", entry.ReportReceived);

        // The start is the operator's own exchange and not the stranger's slot.
        Assert.Equal(Slot.AddSeconds(15), entry.StartedUtc);
    }

    /// <summary>
    /// His grid is his wherever he sent it, and the report is only ours.
    /// </summary>
    /// <remarks>
    /// <para>**THE TWO READ DIFFERENT LISTS AND THE REASON IS THE FORMAT, NOT A
    /// PREFERENCE.** `CQ IK4LZH JN54` states where IK4LZH is, and it is where he
    /// is whoever he was calling — a grid is a property of the sender. A report is
    /// a property of the pair: `-12` in a message to W1ABC is what he heard W1ABC
    /// at, and it is not this contact's.</para>
    /// <para>**THE FIRST DRAFT READ BOTH OFF `HeardToUs` AND LOST THE GRID**, on a
    /// complete exchange that opened with his CQ — which is how nearly every
    /// contact opens. The test caught it; the reasoning above is what the fix
    /// rests on rather than the failure.</para>
    /// </remarks>
    [Fact]
    public void HisGridIsHisWhereverHeSentItAndTheReportIsOnlyOurs()
    {
        var ledger = new Ft8ContactLedger(Mine);

        // A grid in a CQ, addressed to nobody in particular.
        ledger.RecordHeard($"CQ {His} JN54", Slot);

        // And a report to a third station, which is not ours.
        ledger.RecordHeard($"W1ABC {His} +05", Slot.AddSeconds(15));

        var entry = Ft8ContactLogEntry.For(ledger.For(His)!, Mine);

        _output.WriteLine("grid " + entry.GridSquare + ", report " + entry.ReportReceived);

        Assert.Equal("JN54", entry.GridSquare);
        Assert.Null(entry.ReportReceived);
    }

    /// <summary>The last report each way is the one logged.</summary>
    /// <remarks>
    /// **A REPEATED EXCHANGE IS ONE CONTACT.** FT8 loses transmissions constantly
    /// and a station repeats; the report that stood is the last one, and taking
    /// the first would log a number that was superseded while both were still on
    /// the air.
    /// </remarks>
    [Fact]
    public void TheLastReportEachWayIsTheOneLogged()
    {
        var ledger = new Ft8ContactLedger(Mine);

        ledger.RecordHeard($"{Mine} {His} -15", Slot);
        ledger.RecordHeard($"{Mine} {His} -12", Slot.AddSeconds(15));
        ledger.RecordSent($"{His} {Mine} -11", Slot.AddSeconds(30));
        ledger.RecordSent($"{His} {Mine} R-09", Slot.AddSeconds(45));

        var entry = Ft8ContactLogEntry.For(ledger.For(His)!, Mine);

        _output.WriteLine("sent " + entry.ReportSent + ", received " + entry.ReportReceived);

        Assert.Equal("-12", entry.ReportReceived);
        Assert.Equal("-09", entry.ReportSent);
    }

    /// <summary>A courtesy is neither a grid nor a report.</summary>
    /// <remarks>
    /// **`RR73` IS LETTERS THEN DIGITS AND WOULD READ AS A GRID SQUARE**, which is
    /// the trap `Ft8MessageSplit.IsGrid`'s own remarks name. It is tested for
    /// before the grid, the way every other caller in the tree tests for it.
    /// </remarks>
    [Fact]
    public void ACourtesyIsNeitherAGridNorAReport()
    {
        var ledger = new Ft8ContactLedger(Mine);

        ledger.RecordHeard($"{Mine} {His} JN54", Slot);
        ledger.RecordHeard($"{Mine} {His} RR73", Slot.AddSeconds(15));
        ledger.RecordHeard($"{Mine} {His} 73", Slot.AddSeconds(30));

        var entry = Ft8ContactLogEntry.For(ledger.For(His)!, Mine);

        _output.WriteLine("grid " + entry.GridSquare + ", report " + entry.ReportReceived);

        Assert.Equal("JN54", entry.GridSquare);
        Assert.Null(entry.ReportReceived);
    }

    /// <summary>With no radio facts, four fields are simply absent.</summary>
    /// <remarks>
    /// **THE LEDGER IS NOT A RADIO** (task 1). Frequency, band, mode and the
    /// operator's grid are handed in, and where nobody hands them in the record
    /// carries what was heard and nothing invented.
    /// </remarks>
    [Fact]
    public void WithNoRadioFactsThoseFieldsAreAbsent()
    {
        var ledger = new Ft8ContactLedger(Mine);

        ledger.RecordHeard($"{Mine} {His} -12", Slot);

        var entry = Ft8ContactLogEntry.For(ledger.For(His)!, Mine);
        var text = AdifLog.Record(entry);

        _output.WriteLine(text);

        Assert.Null(entry.Band);
        Assert.Null(entry.Mode);
        Assert.Null(entry.FrequencyMhz);
        Assert.Null(entry.MyGridSquare);

        foreach (var absent in new[] { "BAND", "MODE", "FREQ", "MY_GRIDSQUARE" })
        {
            Assert.DoesNotContain("<" + absent + ":", text, StringComparison.Ordinal);
        }

        // What was heard is still there.
        Assert.Contains("<CALL:6>IK4LZH", text, StringComparison.Ordinal);
        Assert.Contains("<RST_RCVD:3>-12", text, StringComparison.Ordinal);
    }

    /// <summary>Nothing at all heard gives an entry with no times.</summary>
    [Fact]
    public void AStationWithNoExchangeHasNoTimes()
    {
        var ledger = new Ft8ContactLedger(Mine);

        // A CQ is heard from him, but nothing passed between the two of them.
        ledger.RecordHeard($"CQ {His} JN54", Slot);

        var entry = Ft8ContactLogEntry.For(ledger.For(His)!, Mine);

        Assert.Equal(His, entry.Call);

        // **NO EXCHANGE, NO TIMES**, because the times span the messages that
        // passed between the two of them and none did.
        Assert.Null(entry.StartedUtc);
        Assert.Null(entry.EndedUtc);
        Assert.Null(entry.ReportReceived);

        // **BUT HIS GRID IS KNOWN, BECAUSE HE SAID IT.** A grid is a property of
        // the sender, so a CQ carries it whether or not anything came of the call.
        // Logging a contact that never happened is the operator's business and
        // not this code's; what this code must not do is invent a fact, and it
        // has not.
        Assert.Equal("JN54", entry.GridSquare);
    }
}

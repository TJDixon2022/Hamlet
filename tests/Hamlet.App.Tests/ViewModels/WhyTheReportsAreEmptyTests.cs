using System;
using System.Linq;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 282, task 4: whether the reports were not recorded, or
/// recorded and not reaching the log entry.
/// </summary>
/// <remarks>
/// <para>**THESE ARE TWO DIFFERENT FAULTS AND ONLY ONE IS FIXABLE HERE.** If the
/// ledger holds a report the entry does not carry, that is a defect in the path and
/// it gets fixed. If the ledger never held one, the entry is right to say nothing
/// and the answer is to name what is not recorded and where it would have to
/// be.</para>
/// <para>**SO THIS DRIVES THE WHOLE PATH.** A ledger is fed the messages of a real
/// exchange, the entry is built from it, and the reports are read back. Nothing here
/// is hand-assembled past the messages themselves — which is where the evidence
/// either survives or is lost.</para>
/// <para>**HIS REAL LOG IS NOT ON THIS MACHINE** (`SHACK_FACTS.md` FACT-006), so
/// this cannot inspect the five records. What it can do is establish whether the
/// mechanism carries a report when one exists, which is the question that decides
/// which of the two faults he is looking at.</para>
/// </remarks>
public sealed class WhyTheReportsAreEmptyTests
{
    private const string His = "KC3QIS";
    private const string Theirs = "W3YNI";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the entry's fields are printed.</param>
    public WhyTheReportsAreEmptyTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A full exchange puts both reports in the entry.**</summary>
    /// <remarks>
    /// This is the test that decides the question. If the path carries both reports
    /// when both were observed, then a record missing one is a record where one was
    /// not observed — and no amount of repair here would put it back.
    /// </remarks>
    [Fact]
    public void AFullExchangePutsBothReportsInTheEntry()
    {
        var ledger = new Ft8ContactLedger(His);
        var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

        // He answers a CQ, they report, he reports back, they roger, he signs off.
        ledger.RecordSent($"{Theirs} {His} FN00", slot);
        ledger.RecordHeard($"{His} {Theirs} -09", slot.AddSeconds(15));
        ledger.RecordSent($"{Theirs} {His} R-12", slot.AddSeconds(30));
        ledger.RecordHeard($"{His} {Theirs} RR73", slot.AddSeconds(45));
        ledger.RecordSent($"{Theirs} {His} 73", slot.AddSeconds(60));

        var entry = Entry(ledger);

        _output.WriteLine("call  : " + entry.Call);
        _output.WriteLine("sent  : " + (entry.ReportSent ?? "(null)"));
        _output.WriteLine("rcvd  : " + (entry.ReportReceived ?? "(null)"));

        Assert.Equal("-12", entry.ReportSent);
        Assert.Equal("-09", entry.ReportReceived);
    }

    /// <summary>
    /// **A contact whose own messages carried no number has no report to log.**
    /// </summary>
    /// <remarks>
    /// <para>The other half of the answer, and the shape a real record takes. A grid
    /// exchange, a roger and a sign-off are a complete contact and **not one of them
    /// contains a signal report**, so there is nothing for the entry to carry and
    /// `not recorded` is the true answer rather than a hole.</para>
    /// <para>**AND HAMLET NEVER FILLS ONE IN.** A plausible report in a permanent
    /// record is §0.0 broken in the one file that outlives everything else here.
    /// </para>
    /// </remarks>
    [Fact]
    public void AContactWithNoNumbersInItHasNoReportToLog()
    {
        var ledger = new Ft8ContactLedger(His);
        var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

        ledger.RecordSent($"{Theirs} {His} FN00", slot);
        ledger.RecordHeard($"{His} {Theirs} RR73", slot.AddSeconds(15));
        ledger.RecordSent($"{Theirs} {His} 73", slot.AddSeconds(30));

        var entry = Entry(ledger);

        _output.WriteLine("sent  : " + (entry.ReportSent ?? "(null)"));
        _output.WriteLine("rcvd  : " + (entry.ReportReceived ?? "(null)"));

        Assert.Null(entry.ReportSent);
        Assert.Null(entry.ReportReceived);

        // And a null field is left out of the record entirely rather than written
        // blank, which is what the log window reads as `not recorded`.
        var written = AdifLog.Record(entry);

        _output.WriteLine("the record : " + written.Trim());

        Assert.DoesNotContain("RST_SENT", written, StringComparison.Ordinal);
        Assert.DoesNotContain("RST_RCVD", written, StringComparison.Ordinal);
    }

    /// <summary>
    /// **A contact Hamlet did not transmit for has a received report and no sent one.**
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS THE SHAPE OF FOUR OF HIS FIVE RECORDS**, and it is not a
    /// defect. `Ft8ContactLedger.RecordSent` has one call site — beside the
    /// transmission that actually went out — so a station worked on another program,
    /// or one whose exchange Hamlet only listened to, leaves the sent side empty
    /// while the received side fills normally.</para>
    /// <para>**AN OFFER IS NOT A TRANSMISSION.** The entry is not filled from what
    /// the right-click menu proposed, which is what would make the log say he sent
    /// something he did not.</para>
    /// </remarks>
    [Fact]
    public void AContactHamletDidNotTransmitForHasNoSentReport()
    {
        var ledger = new Ft8ContactLedger(His);
        var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

        ledger.RecordHeard($"{His} {Theirs} -09", slot);
        ledger.RecordHeard($"{His} {Theirs} RR73", slot.AddSeconds(30));

        var entry = Entry(ledger);

        _output.WriteLine("sent  : " + (entry.ReportSent ?? "(null)"));
        _output.WriteLine("rcvd  : " + (entry.ReportReceived ?? "(null)"));

        Assert.Null(entry.ReportSent);
        Assert.Equal("-09", entry.ReportReceived);
    }

    /// <summary>**A report meant for somebody else never reaches his log.**</summary>
    /// <remarks>
    /// The guard that makes the empty field trustworthy. A station working three
    /// others spends most of its transmissions on somebody else, and a number inside
    /// one of those is a stranger's report — putting it here would be a false record
    /// of what passed between these two.
    /// </remarks>
    [Fact]
    public void AReportMeantForSomebodyElseNeverReachesHisLog()
    {
        var ledger = new Ft8ContactLedger(His);
        var slot = new DateTime(2026, 9, 8, 21, 41, 30, DateTimeKind.Utc);

        ledger.RecordHeard($"W1ABC {Theirs} -03", slot);
        ledger.RecordHeard($"{His} {Theirs} RR73", slot.AddSeconds(15));

        var entry = Entry(ledger);

        _output.WriteLine("rcvd  : " + (entry.ReportReceived ?? "(null)"));

        Assert.Null(entry.ReportReceived);
    }

    /// <summary>The log entry for the one station in a ledger.</summary>
    /// <param name="ledger">The ledger.</param>
    /// <returns>The contact the dialog would be opened on.</returns>
    private static AdifContact Entry(Ft8ContactLedger ledger)
        => Ft8ContactLogEntry.For(
            ledger.For(Theirs)!,
            His,
            new Ft8StationConditions(
                14_074_000, "20 m", ContactModes.Named("FT8"), "FN00DJ"));
}

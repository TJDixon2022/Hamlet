using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 278, task 2: his own log, in a window, with a missing field
/// shown as missing and a damaged record shown rather than dropped.
/// </summary>
/// <remarks>
/// <para>**HE HAD LOGGED CONTACTS AND COULD NOT SEE THEM.** `AdifLog` has written
/// `contacts.adi` since unit 274 and read it back for the *worked* mark, and
/// nothing in the application displayed it. Reading his own log meant opening the
/// file in Notepad.</para>
/// <para>**THE TWO THINGS THIS CLASS EXISTS FOR** are the two the reader used to
/// swallow: a field Hamlet never observed and a field it wrote and cannot read back
/// both arrived as null, and a record with no terminator vanished with no trace.
/// Drawn alike, the first tells him his radio did not know the band when the truth
/// is that the file is damaged.</para>
/// </remarks>
public sealed class TheLogHasAWindowTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the window's rows are printed.</param>
    public TheLogHasAWindowTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>
    /// **A whole record comes back field for field, newest first.**
    /// </summary>
    [Fact]
    public void EveryFieldComesBackAndTheNewestIsFirst()
    {
        var view = View(
            Written("K9XP", "20260908", "021315", "20m", "FT8", "-09", "-14", "FN00", "first one"),
            Written("W1ABC", "20260908", "031500", "40m", "FT8", "-11", "-08", "FN00", "second"));

        Print(view);

        Assert.Equal(2, view.Count);
        Assert.True(view.HasContacts);

        var newest = view.Contacts[0];

        Assert.Equal("W1ABC", newest.Call);
        Assert.Equal("2026-09-08 03:15:00", newest.WhenUtc);
        Assert.Equal("40m", newest.Band);
        Assert.Equal("FT8", newest.Mode);
        Assert.Equal("-11", newest.ReportSent);
        Assert.Equal("-08", newest.ReportReceived);
        Assert.Equal("FN00", newest.MyGridSquare);
        Assert.Equal("second", newest.Comment);

        Assert.Equal("K9XP", view.Contacts[1].Call);
    }

    /// <summary>
    /// **A pre-275 record has no band and the window says so in words.**
    /// </summary>
    /// <remarks>
    /// §0.0. Unit 275 made a contact record the dial it was heard on; before that
    /// `FREQ` and `BAND` were left out entirely rather than guessed, because Hamlet
    /// did not observe where the contact happened. **A blank cell is a hole a
    /// reader fills in**, and the habit here is to assume the band he is on now.
    /// </remarks>
    [Fact]
    public void AFieldTheRecordDoesNotCarryIsShownAsAbsent()
    {
        // A record as unit 274 wrote them: no BAND, no FREQ.
        var view = View(
            "<CALL:4>K9XP<QSO_DATE:8>20260908<TIME_ON:6>021315<MODE:3>FT8<EOR>\n");

        Print(view);

        var row = Assert.Single(view.Contacts);

        Assert.Equal("K9XP", row.Call);
        Assert.Equal("FT8", row.Mode);

        // **THE WORD, NOT A BLANK AND NOT A DASH.** A dash in a column of reports
        // reads as a report.
        Assert.Equal(ContactLogRow.Absent, row.Band);
        Assert.Equal(ContactLogRow.Absent, row.ReportSent);
        Assert.Equal(ContactLogRow.Absent, row.MyGridSquare);
        Assert.Equal("not recorded", row.Band);

        // And it is not damage: the record is sound, Hamlet simply never knew.
        Assert.True(row.IsSound);
        Assert.False(row.HasFault);
        Assert.False(view.HasDamage);
    }

    /// <summary>
    /// **A record with no end marker is shown, marked, rather than dropped.**
    /// </summary>
    /// <remarks>
    /// **THIS IS THE ONE THAT USED TO VANISH.** Fields accumulated and were turned
    /// into a contact only at `&lt;EOR&gt;`, so a file cut off mid-record lost that
    /// record with no trace anywhere. A log that quietly drops a record is worse
    /// than one that shows a bad one.
    /// </remarks>
    [Fact]
    public void ATruncatedRecordIsShownAndNotSwallowed()
    {
        var view = View(
            "<CALL:4>K9XP<QSO_DATE:8>20260908<TIME_ON:6>021315<BAND:3>20m<EOR>\n"
            + "<CALL:5>W1ABC<QSO_DATE:8>20260908<TIME_ON:6>0315");

        Print(view);

        Assert.Equal(2, view.Count);
        Assert.True(view.HasDamage);
        Assert.Equal(1, view.Damaged);

        var broken = Assert.Single(view.Contacts, r => r.HasFault);

        Assert.Equal("W1ABC", broken.Call);
        Assert.False(broken.IsSound);
        Assert.Contains("no end marker", broken.Fault, StringComparison.Ordinal);

        // **AND THE PLAIN READER STILL LEAVES IT OUT**, which is what every other
        // caller wants: contacts the operator finished writing.
        Assert.Single(AdifLog.Read(
            "<CALL:4>K9XP<QSO_DATE:8>20260908<TIME_ON:6>021315<BAND:3>20m<EOR>\n"
            + "<CALL:5>W1ABC<QSO_DATE:8>20260908<TIME_ON:6>0315"));
    }

    /// <summary>
    /// **A field with an unreadable length is named, not silently absent.**
    /// </summary>
    /// <remarks>
    /// The distinction this whole class turns on. Before this, a broken `BAND` and
    /// a `BAND` that was never written both came back null, and the window would
    /// have told him Hamlet did not know the band when in fact it wrote one and
    /// cannot read it back.
    /// </remarks>
    [Fact]
    public void ABrokenFieldIsNotTheSameAsAnAbsentOne()
    {
        var view = View(
            "<CALL:4>K9XP<QSO_DATE:8>20260908<TIME_ON:6>021315<BAND>20m<EOR>\n");

        Print(view);

        var row = Assert.Single(view.Contacts);

        Assert.Equal("K9XP", row.Call);

        // The cell still says the band was not recorded, because it could not be
        // read; **but the row also says why**, which is the difference.
        Assert.Equal(ContactLogRow.Absent, row.Band);
        Assert.True(row.HasFault);
        Assert.False(row.IsSound);
        Assert.Contains("BAND", row.Fault, StringComparison.Ordinal);
        Assert.Contains("no readable length", row.Fault, StringComparison.Ordinal);
    }

    /// <summary>
    /// **An empty log says so in his words rather than showing an empty table.**
    /// </summary>
    [Fact]
    public void AnEmptyLogSaysSo()
    {
        var view = View();

        _output.WriteLine(view.Idle);

        Assert.False(view.HasContacts);
        Assert.Equal(0, view.Count);
        Assert.Equal("0 contacts logged.", view.CountLine);
        Assert.Contains("Nothing here yet", view.Idle, StringComparison.Ordinal);

        // §0.7: at most one em dash in a passage, and this one has none.
        Assert.DoesNotContain("—", view.Idle);
    }

    /// <summary>
    /// **The header's fields are not a contact**, which the parser already knew.
    /// </summary>
    /// <remarks>
    /// Written because the window's count is the file's record count, and a header
    /// counted as a record would make his very first contact read as his second.
    /// </remarks>
    [Fact]
    public void TheHeaderIsNotAContact()
    {
        var view = View(
            AdifLog.Header("1.12.150")
            + "<CALL:4>K9XP<QSO_DATE:8>20260908<TIME_ON:6>021315<EOR>\n");

        Print(view);

        Assert.Equal(1, view.Count);
        Assert.False(view.HasDamage);
    }

    /// <summary>A view over a file's text.</summary>
    private static ContactLogViewModel View(string text = "")
        => new(AdifLog.ReadRecords(text), @"C:\test\contacts.adi");

    /// <summary>A view over records written by `AdifLog` itself.</summary>
    /// <remarks>
    /// **THROUGH THE WRITER, NOT BY HAND**, so the fixture cannot drift from the
    /// format the application actually produces. That is §12.5: a fixture built
    /// from the same misunderstanding as the code proves nothing.
    /// </remarks>
    private static ContactLogViewModel View(params string[] records)
        => new(AdifLog.ReadRecords(string.Concat(records)), @"C:\test\contacts.adi");

    /// <summary>One record, written by the writer under test's own sibling.</summary>
    private static string Written(
        string call, string date, string time, string band, string mode,
        string sent, string received, string myGrid, string comment)
        => AdifLog.Record(new AdifContact
        {
            Call = call,
            StartedUtc = DateTime.ParseExact(
                date + time, "yyyyMMddHHmmss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AdjustToUniversal
                    | System.Globalization.DateTimeStyles.AssumeUniversal),
            Band = band,
            Mode = mode,
            ReportSent = sent,
            ReportReceived = received,
            MyGridSquare = myGrid,
            Comment = comment,
        });

    /// <summary>The window's rows, so a failure shows what was drawn.</summary>
    private void Print(ContactLogViewModel view)
    {
        _output.WriteLine(view.CountLine);

        if (view.HasDamage)
        {
            _output.WriteLine(view.DamageLine);
        }

        _output.WriteLine(
            "  call     when (UTC)           band  mode  sent  rcvd  my grid  notes");

        foreach (var row in view.Contacts)
        {
            _output.WriteLine(
                "  " + row.Call.PadRight(9) + row.WhenUtc.PadRight(21)
                + row.Band.PadRight(6) + row.Mode.PadRight(6)
                + row.ReportSent.PadRight(6) + row.ReportReceived.PadRight(6)
                + row.MyGridSquare.PadRight(9) + row.Comment);

            if (row.HasFault)
            {
                _output.WriteLine("      ! " + row.Fault);
            }
        }

        _output.WriteLine("");
    }
}

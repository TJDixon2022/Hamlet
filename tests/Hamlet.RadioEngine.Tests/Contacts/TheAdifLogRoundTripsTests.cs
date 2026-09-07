using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// Work instruction 274, task 2: the ADIF writer, and the round trip that is the
/// test.
/// </summary>
/// <remarks>
/// <para>**ADIF IS FUSSY ABOUT LENGTHS, HEADERS AND RECORD TERMINATORS**, and
/// getting it quietly wrong produces a file other programs reject without saying
/// why. Writing a record and reading it back is the only thing that catches a
/// length off by one.</para>
/// <para>**A FIELD HAMLET DID NOT OBSERVE IS ABSENT.** Not written empty, not
/// written with a plausible value. A log outlives everything else in this project
/// and an entry carrying a report Hamlet never heard is a false record of what
/// happened on the air (§0.0).</para>
/// </remarks>
public sealed class TheAdifLogRoundTripsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the records are printed.</param>
    public TheAdifLogRoundTripsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A complete contact round-trips every field.</summary>
    [Fact]
    public void ACompleteContactRoundTripsEveryField()
    {
        var contact = Complete();

        var text = AdifLog.Header("1.12.132") + AdifLog.Record(contact);

        _output.WriteLine(text);

        var read = Assert.Single(AdifLog.Read(text));

        Assert.Equal(contact.Call, read.Call);
        Assert.Equal(contact.StationCallsign, read.StationCallsign);
        Assert.Equal(contact.StartedUtc, read.StartedUtc);
        Assert.Equal(contact.EndedUtc, read.EndedUtc);
        Assert.Equal(contact.Band, read.Band);
        Assert.Equal(contact.Mode, read.Mode);
        Assert.Equal(contact.FrequencyMhz, read.FrequencyMhz);
        Assert.Equal(contact.ReportSent, read.ReportSent);
        Assert.Equal(contact.ReportReceived, read.ReportReceived);
        Assert.Equal(contact.GridSquare, read.GridSquare);
        Assert.Equal(contact.MyGridSquare, read.MyGridSquare);
        Assert.Equal(contact.Comment, read.Comment);

        // **THE WHOLE RECORD, NOT FIELD BY FIELD.** Comparing the records
        // themselves catches a field this test forgot to name, which is the
        // failure a list of assertions cannot see.
        Assert.Equal(contact, read);
    }

    /// <summary>
    /// A contact missing the report he sent round-trips without that field.
    /// </summary>
    /// <remarks>
    /// **THE CASE TASK 1 FOUND.** `RecordSent` has one call site, on Hamlet's own
    /// send path, so a contact answered on another program has no sent report. The
    /// field must be absent rather than empty and rather than a plausible `59`.
    /// </remarks>
    [Fact]
    public void AContactWithNoSentReportOmitsThatFieldEntirely()
    {
        var contact = Complete() with { ReportSent = null };

        var text = AdifLog.Record(contact);

        _output.WriteLine(text);

        // **ABSENT, NOT EMPTY.** `<RST_SENT:0>` would assert an empty report was
        // exchanged, which is a thing that did not happen.
        Assert.DoesNotContain("RST_SENT", text, StringComparison.Ordinal);
        Assert.DoesNotContain("<RST_SENT:0>", text, StringComparison.Ordinal);

        // The report he received is still there, so the absence is the missing
        // fact rather than the writer having given up on the pair.
        Assert.Contains("RST_RCVD", text, StringComparison.Ordinal);

        var read = Assert.Single(AdifLog.Read(text));

        Assert.Null(read.ReportSent);
        Assert.Equal("-12", read.ReportReceived);
        Assert.Equal(contact, read);
    }

    /// <summary>Every field can be missing, one at a time, and the rest survive.</summary>
    /// <remarks>
    /// **ONE AT A TIME RATHER THAN ALL AT ONCE**, because what this is looking for
    /// is a field whose absence corrupts the record after it — a length written
    /// from the wrong string, or a separator that was carrying the parse.
    /// </remarks>
    [Fact]
    public void AnyOneFieldCanBeMissing()
    {
        var withouts = new (string Name, AdifContact Contact)[]
        {
            ("CALL", Complete() with { Call = null }),
            ("STATION_CALLSIGN", Complete() with { StationCallsign = null }),
            // **NO START MEANS NO END EITHER, AND THAT IS THE FORMAT.**
            // `TIME_OFF` has no date of its own — the record's date is
            // `QSO_DATE`, which comes from the start — so an end time written
            // without one is a time of day belonging to no day. The writer
            // refuses it rather than letting a reader invent the date, which is
            // why this row expects both to come back null.
            ("QSO_DATE", Complete() with { StartedUtc = null, EndedUtc = null }),
            ("TIME_OFF", Complete() with { EndedUtc = null }),
            ("BAND", Complete() with { Band = null }),
            ("MODE", Complete() with { Mode = null }),
            ("FREQ", Complete() with { FrequencyMhz = null }),
            ("RST_SENT", Complete() with { ReportSent = null }),
            ("RST_RCVD", Complete() with { ReportReceived = null }),
            ("GRIDSQUARE", Complete() with { GridSquare = null }),
            ("MY_GRIDSQUARE", Complete() with { MyGridSquare = null }),
            ("COMMENT", Complete() with { Comment = null }),
        };

        foreach (var (name, contact) in withouts)
        {
            var text = AdifLog.Record(contact);
            var read = Assert.Single(AdifLog.Read(text));

            _output.WriteLine(
                "without " + name.PadRight(18) + " -> "
                + text.Replace("\n", " ").Trim());

            Assert.Equal(contact, read);
        }
    }

    /// <summary>
    /// Notes carrying characters ADIF treats specially survive intact.
    /// </summary>
    /// <remarks>
    /// <para>**THE LENGTH IS THE ESCAPE, WHICH IS WHY NOTHING IS ESCAPED.** ADI has
    /// no escape character: a reader takes exactly `length` characters after the
    /// `&gt;`. So an angle bracket inside a note is ordinary text, and a writer
    /// that quoted it would corrupt the operator's own words.</para>
    /// <para>**`&lt;EOR&gt;` INSIDE A NOTE IS THE CASE THAT DECIDES IT.** A reader
    /// that searched for the terminator rather than counting would end the record
    /// in the middle of his sentence and swallow every field after it.</para>
    /// </remarks>
    [Theory]
    [InlineData("Nothing special here")]
    [InlineData("angle brackets <like this> in the middle")]
    [InlineData("a colon: and a length:12 that looks like a tag")]
    [InlineData("<EOR> in the middle of a note")]
    [InlineData("<CALL:5>FAKE1 pretending to be a field")]
    [InlineData("two lines\nand the second one")]
    [InlineData("trailing spaces   ")]
    [InlineData("100% <> :: <EOH> <EOR>")]
    public void NotesWithSpecialCharactersSurvive(string note)
    {
        var contact = Complete() with { Comment = note };

        var text = AdifLog.Record(contact);
        var read = Assert.Single(AdifLog.Read(text));

        _output.WriteLine("[" + note + "] -> [" + read.Comment + "]");

        Assert.Equal(note, read.Comment);

        // And nothing after the notes was lost, which is what a terminator found
        // inside them would have cost.
        Assert.Equal(contact, read);
    }

    /// <summary>The header is a header and the reader skips it.</summary>
    /// <remarks>
    /// **A HEADER FIELD IS NOT A QSO FIELD.** `ADIF_VER` and `PROGRAMID` live
    /// above `&lt;EOH&gt;`, and a reader that took them as part of the first record
    /// would produce one contact with a version number in it.
    /// </remarks>
    [Fact]
    public void TheHeaderIsSkippedAndSaysWhatWroteTheFile()
    {
        var header = AdifLog.Header("1.12.132");

        _output.WriteLine(header);

        Assert.Contains("<ADIF_VER:5>3.1.4", header, StringComparison.Ordinal);
        Assert.Contains("<PROGRAMID:6>Hamlet", header, StringComparison.Ordinal);
        Assert.Contains("<PROGRAMVERSION:8>1.12.132", header, StringComparison.Ordinal);
        Assert.Contains("<EOH>", header, StringComparison.Ordinal);

        // The comment before the first field names the specification, so somebody
        // opening this in an editor in five years does not have to guess.
        Assert.Contains("3.1.4", AdifLog.Specification, StringComparison.Ordinal);
        Assert.Contains("adif.org", AdifLog.Specification, StringComparison.Ordinal);

        // A header alone holds no contacts.
        Assert.Empty(AdifLog.Read(header));

        // And with records after it, only the records come back.
        var whole = header + AdifLog.Record(Complete()) + AdifLog.Record(
            Complete() with { Call = "W1ABC" });

        var read = AdifLog.Read(whole);

        Assert.Equal(2, read.Count);
        Assert.Equal("IK4LZH", read[0].Call);
        Assert.Equal("W1ABC", read[1].Call);
    }

    /// <summary>The fields carry the names the specification gives them.</summary>
    /// <remarks>
    /// **THE TAG NAMES ARE NOT THIS PROJECT'S TO INVENT** and they were not
    /// written from memory. Every one below was read from ADIF 3.1.4 at
    /// `adif.org` on 2026-09-07, and this is the test that would fail if a later
    /// edit "tidied" one.
    /// </remarks>
    [Fact]
    public void TheTagNamesAreTheSpecificationsOwn()
    {
        var text = AdifLog.Record(Complete());

        foreach (var name in new[]
                 {
                     "CALL", "STATION_CALLSIGN", "QSO_DATE", "TIME_ON", "TIME_OFF",
                     "BAND", "MODE", "FREQ", "RST_SENT", "RST_RCVD", "GRIDSQUARE",
                     "MY_GRIDSQUARE", "COMMENT",
                 })
        {
            Assert.Contains("<" + name + ":", text, StringComparison.Ordinal);
        }

        // The date and time shapes the specification states: 8 digits YYYYMMDD,
        // and 6 digits HHMMSS rather than the 4 it also permits, because a slot
        // boundary has seconds and dropping them loses a fact the ledger holds.
        Assert.Contains("<QSO_DATE:8>20260907", text, StringComparison.Ordinal);
        Assert.Contains("<TIME_ON:6>214130", text, StringComparison.Ordinal);
        Assert.Contains("<TIME_OFF:6>214300", text, StringComparison.Ordinal);

        // The Band enumeration's own spelling for 14 MHz.
        Assert.Contains("<BAND:3>20m", text, StringComparison.Ordinal);
        Assert.Contains("<MODE:3>FT8", text, StringComparison.Ordinal);

        Assert.Contains("<EOR>", text, StringComparison.Ordinal);
    }

    /// <summary>Every declared length is the true length of its value.</summary>
    /// <remarks>
    /// **THIS IS THE ONE THAT CATCHES AN OFF-BY-ONE**, which is the failure that
    /// makes another program reject the file without saying why. It is asserted
    /// independently of the reader, so a writer and a reader that are wrong the
    /// same way cannot both pass (§12.5).
    /// </remarks>
    [Fact]
    public void EveryDeclaredLengthIsTrue()
    {
        var text = AdifLog.Record(
            Complete() with { Comment = "a note with <brackets> and a : colon" });

        var i = 0;
        var checkedFields = 0;

        while (true)
        {
            var open = text.IndexOf('<', i);

            if (open < 0)
            {
                break;
            }

            var close = text.IndexOf('>', open);
            var tag = text[(open + 1)..close];
            var parts = tag.Split(':');

            if (parts.Length < 2)
            {
                i = close + 1;
                continue;
            }

            var declared = int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
            var value = text.Substring(close + 1, declared);

            // The character after the declared run must be the newline this
            // writer puts between fields. One too few or one too many and it is
            // part of the value instead.
            var next = text[close + 1 + declared];

            _output.WriteLine(
                parts[0].PadRight(18) + declared.ToString().PadLeft(3)
                + "  [" + value + "]  next=" + (next == '\n' ? "newline" : next.ToString()));

            Assert.Equal('\n', next);

            checkedFields++;
            i = close + 1 + declared;
        }

        Assert.True(checkedFields >= 12, "only " + checkedFields + " fields were checked");
    }

    /// <summary>A complete contact, as an evening on 20 metres produces one.</summary>
    private static AdifContact Complete() => new()
    {
        Call = "IK4LZH",
        StationCallsign = "KC3QIS",
        StartedUtc = new DateTime(2026, 9, 7, 21, 41, 30, DateTimeKind.Utc),
        EndedUtc = new DateTime(2026, 9, 7, 21, 43, 0, DateTimeKind.Utc),
        Band = "20m",
        Mode = "FT8",
        FrequencyMhz = 14.074,
        ReportSent = "-09",
        ReportReceived = "-12",
        GridSquare = "JN54",
        MyGridSquare = "FN00DJ",
        Comment = "First one into Italy.",
    };
}

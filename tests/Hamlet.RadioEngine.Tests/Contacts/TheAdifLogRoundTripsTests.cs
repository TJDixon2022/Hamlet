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
        Assert.Equal(contact.Submode, read.Submode);
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

        // **AND THE SAME WALK WITH A SUBMODE ON IT** (work instruction 291 task
        // 2). `Complete()` is FT8 and FT8 takes no submode, so every assertion
        // above compares null with null for the new field — which is exactly the
        // fixture-built-from-the-same-assumption fault §12.5 names, and the
        // reason this is extended rather than trusted.
        var ft4 = CompleteFt4();

        var ft4Text = AdifLog.Record(ft4);

        _output.WriteLine(ft4Text);

        var ft4Read = Assert.Single(AdifLog.Read(ft4Text));

        Assert.Equal("MFSK", ft4Read.Mode);
        Assert.Equal("FT4", ft4Read.Submode);
        Assert.Equal(ft4, ft4Read);
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

            // **THE ABSENT SUBMODE, TAKEN OFF A RECORD THAT HAD ONE** (work
            // instruction 291 task 2, step 3's third criterion). Dropping it from
            // `Complete()` would prove nothing: FT8 never had one. Dropped from
            // the FT4 record it leaves `MODE=MFSK` behind, which is the shape a
            // careless writer produces and which no row may light from.
            ("SUBMODE", CompleteFt4() with { Submode = null }),
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

        // **`SUBMODE` IS ABSENT FROM AN FT8 RECORD AND THE STRING DOES NOT
        // APPEAR** (work instruction 291 task 2). Not `<SUBMODE:0>`, which
        // asserts an empty submode was observed. `FT8` is a Mode and takes no
        // submode, so this is the honest shape rather than a missing one.
        Assert.DoesNotContain("SUBMODE", text, StringComparison.Ordinal);

        // **AND ON A RECORD THAT HAS ONE, THE TAG IS THE SPECIFICATION'S.**
        // `SUBMODE`, read from ADIF 3.1.4 as unit 287 read it and carried at
        // `ContactModes.Cite`, and never written from memory. `MODE=FT4` is not
        // valid ADIF; `MODE=MFSK` with `SUBMODE=FT4` is FT4.
        var ft4 = AdifLog.Record(CompleteFt4());

        _output.WriteLine(ft4);

        Assert.Contains("<MODE:4>MFSK", ft4, StringComparison.Ordinal);
        Assert.Contains("<SUBMODE:3>FT4", ft4, StringComparison.Ordinal);
        Assert.DoesNotContain("<MODE:3>FT4", ft4, StringComparison.Ordinal);
    }

    /// <summary>An FT8 record is byte-for-byte what it was before unit 291.</summary>
    /// <remarks>
    /// <para>**NOT A TAG, NOT A LENGTH, NOT A BYTE** (work instruction 291 task
    /// 2). The log outlives everything else here, and a file the operator has
    /// already got must keep reading the way it reads today.</para>
    /// <para>**IT IS A WHOLE-STRING COMPARISON AND THAT IS THE POINT.** A
    /// `Contains` check would pass with a field inserted, a length changed or a
    /// separator moved. This is the assertion that cannot.</para>
    /// <para>**HOW THE LITERAL WAS ESTABLISHED, SAID PLAINLY BECAUSE IT IS
    /// EVIDENCE.** The pre-unit commit could not be built here — this session's
    /// shell refused `git worktree` — so the baseline is not a dump of the old
    /// binary. It rests on two things instead. First, `git diff f13b644 --
    /// AdifLog.cs` shows exactly three code changes: the `Submode` property, the
    /// one `Field(text, "SUBMODE", contact.Submode)` call, and one `Get` in
    /// `From`. Second, `Field` returns without writing on a null or empty value,
    /// and `Submode` is null on every record written before unit 291. **So the
    /// added call is a no-op for an FT8 contact by construction**, and the literal
    /// below is that construction written out. It is an argument plus a pin,
    /// rather than a measurement, and it is marked as one.</para>
    /// </remarks>
    [Fact]
    public void AnFt8RecordIsByteIdenticalToWhatItWasBeforeTheSubmode()
    {
        const string Before =
            "<CALL:6>IK4LZH\n"
            + "<STATION_CALLSIGN:6>KC3QIS\n"
            + "<QSO_DATE:8>20260907\n"
            + "<TIME_ON:6>214130\n"
            + "<TIME_OFF:6>214300\n"
            + "<BAND:3>20m\n"
            + "<MODE:3>FT8\n"
            + "<FREQ:9>14.074000\n"
            + "<RST_SENT:3>-09\n"
            + "<RST_RCVD:3>-12\n"
            + "<GRIDSQUARE:4>JN54\n"
            + "<MY_GRIDSQUARE:6>FN00DJ\n"
            + "<COMMENT:21>First one into Italy.\n"
            + "<EOR>\n";

        var now = AdifLog.Record(Complete());

        _output.WriteLine(now);

        Assert.Equal(Before, now);
        Assert.Equal(Before.Length, now.Length);
    }

    /// <summary>Every declared length is the true length of its value.</summary>
    /// <remarks>
    /// <para>**THIS IS THE ONE THAT CATCHES AN OFF-BY-ONE**, which is the failure
    /// that makes another program reject the file without saying why. It is
    /// asserted independently of the reader, so a writer and a reader that are
    /// wrong the same way cannot both pass (§12.5).</para>
    /// <para>**AND IT NOW RUNS OVER THE SUBMODE RECORD TOO** (work instruction 291
    /// task 2). A `SUBMODE` written one character short is the exact breakage this
    /// unit was told to catch, and it would be invisible in a round trip whose
    /// writer and reader are both off by the same one. Unit 274 shipped `20 m` for
    /// `20m` and the round trip did not notice, for that reason.</para>
    /// </remarks>
    [Theory]
    [InlineData("FT8")]
    [InlineData("FT4")]
    public void EveryDeclaredLengthIsTrue(string which)
    {
        var contact = which == "FT4" ? CompleteFt4() : Complete();

        var text = AdifLog.Record(
            contact with { Comment = "a note with <brackets> and a : colon" });

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

        _output.WriteLine(which + ": " + checkedFields + " fields checked");

        // Twelve on an FT8 record and thirteen once the submode is on it.
        Assert.True(
            checkedFields >= (which == "FT4" ? 13 : 12),
            "only " + checkedFields + " fields were checked on the " + which + " record");
    }

    /// <summary>
    /// Hamlet's own band names become the enumeration's, or nothing.
    /// </summary>
    /// <remarks>
    /// <para>**UNIT 274 WROTE `20 m` INTO THE FILE AND THIS FILE DID NOT NOTICE.**
    /// `HfBands` names bands for the screen and the log took the name straight
    /// through; ADIF's Band enumeration gives `20m`, and a logger has no row for
    /// `20 m`.</para>
    /// <para>**WHY THE ROUND TRIP MISSED IT, WHICH IS THE LESSON.** Every test in
    /// this file wrote a hand-made `"20m"` and read back `"20m"` — the writer and
    /// the reader agreeing perfectly about a value the application never produces.
    /// A fixture built from the same assumption as the code proves nothing about
    /// the code (§12.5). What caught it was pushing the app's own name through for
    /// the first time, in work instruction 275 task 3.</para>
    /// <para>**THE HF BANDS BELOW ARE `HfBands`' OWN NAMES**, copied from
    /// `HfBands.cs:76`, so this fails if either side is renamed.</para>
    /// </remarks>
    [Theory]
    [InlineData("20 m", "20m")]
    [InlineData("80 m", "80m")]
    [InlineData("40 m", "40m")]
    [InlineData("30 m", "30m")]
    [InlineData("17 m", "17m")]
    [InlineData("15 m", "15m")]
    [InlineData("10 m", "10m")]
    [InlineData("20m", "20m")]
    [InlineData("  40 m  ", "40m")]
    [InlineData("70 cm", "70cm")]
    // **AND WHAT IT REFUSES.** A name the enumeration has no row for is left out
    // rather than written wrong, which is the rule every field here follows.
    [InlineData("", null)]
    [InlineData(null, null)]
    [InlineData("the twenty metre band", null)]
    [InlineData("HF", null)]
    [InlineData("m", null)]
    public void HamletsBandNamesBecomeTheEnumerationsOrNothing(
        string? given, string? expected)
    {
        var got = AdifLog.BandValueFor(given);

        _output.WriteLine("[" + given + "] -> " + (got ?? "(nothing)"));

        Assert.Equal(expected, got);
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

    /// <summary>The same contact worked on FT4, which needs both halves.</summary>
    /// <remarks>
    /// **THE PAIR IS READ OUT OF `ContactModes` AND NOT TYPED HERE** (work
    /// instruction 291 task 2). A fixture that spelled `MFSK` and `FT4` by hand
    /// would agree with a writer that spelled them by hand, and neither would be
    /// the specification's — which is unit 274's `20 m` exactly.
    /// </remarks>
    private static AdifContact CompleteFt4()
    {
        var ft4 = ContactModes.Named("FT4")!;

        return Complete() with
        {
            Band = "40m",
            FrequencyMhz = 7.0475,
            Mode = ft4.AdifModes[0],
            Submode = ft4.AdifSubmode,
        };
    }
}

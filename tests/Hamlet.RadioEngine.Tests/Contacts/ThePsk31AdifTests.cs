using System;
using System.Linq;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Contacts;

/// <summary>
/// Work instruction 326 task 4: **the ADIF export spells PSK31 the way another logger
/// reads it, and carries the RST that passed.**
/// </summary>
/// <remarks>
/// <para>**ASSERTED, NOT REBUILT.** `MODE=PSK` with `SUBMODE=PSK31` has been in
/// <see cref="ContactModes"/> since unit 287, and step 5's second exit criterion says in
/// those words that this unit asserts it rather than building it again. What was never
/// asserted is that a contact driven through the **write path** comes out spelled that
/// way - a table holding the right pair and a record carrying it are two different
/// facts, and unit 291 found exactly that gap for FT4.</para>
/// <para>**`MODE=PSK31` IS NOT VALID ADIF**, which is why the pair matters: `PSK` alone
/// is *some kind of phase-shift keying*, and a record saying `MODE=PSK31` names nothing
/// any other program has a row for.</para>
/// <para>**AND THE RST TAGS COME FROM TASK 3'S FIELDS**, which are not the decibel ones
/// (§3.2). A report that was never read writes no tag rather than an empty one (§0.0).
/// </para>
/// </remarks>
public sealed class ThePsk31AdifTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the records are printed.</param>
    public ThePsk31AdifTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A PSK31 contact exports as `MODE=PSK` with `SUBMODE=PSK31`.**</summary>
    [Fact]
    public void APsk31ContactComesOutAsPskWithTheSubmode()
    {
        var text = AdifLog.Record(Psk31Contact());

        _output.WriteLine(text);

        Assert.Contains("<MODE:3>PSK", text, StringComparison.Ordinal);
        Assert.Contains("<SUBMODE:5>PSK31", text, StringComparison.Ordinal);

        // **AND NEVER THE THING THAT LOOKS RIGHT.** `MODE=PSK31` is not valid ADIF.
        Assert.DoesNotContain("<MODE:5>PSK31", text, StringComparison.Ordinal);

        var read = Assert.Single(AdifLog.Read(text));

        Assert.Equal("PSK", read.Mode);
        Assert.Equal("PSK31", read.Submode);

        // And the record matches the table it was spelled from, through the file.
        Assert.True(ContactModes.Named("PSK31")!.Matches(read.Mode, read.Submode));
    }

    /// <summary>**The RST tags are there when a report was read, and absent when not.**</summary>
    [Fact]
    public void TheRstTagsArePresentWhenReadAndAbsentWhenNot()
    {
        var worked = Psk31Contact() with { RstSent = "599", RstReceived = "589" };

        var text = AdifLog.Record(worked);

        _output.WriteLine(text);

        Assert.Contains("<RST_SENT:3>599", text, StringComparison.Ordinal);
        Assert.Contains("<RST_RCVD:3>589", text, StringComparison.Ordinal);

        // **AND THEY COME BACK AS RSTS AND NOT AS DECIBELS.** One ADIF tag carries
        // both kinds of report and the record's own mode says which this is.
        var read = Assert.Single(AdifLog.Read(text));

        Assert.Equal("599", read.RstSent);
        Assert.Equal("589", read.RstReceived);
        Assert.Null(read.ReportSent);
        Assert.Null(read.ReportReceived);

        // **AN UNREAD REPORT WRITES NO TAG AT ALL** (§0.0), not `<RST_RCVD:0>`.
        var half = AdifLog.Record(worked with { RstReceived = null });

        _output.WriteLine(half);

        Assert.Contains("<RST_SENT:3>599", half, StringComparison.Ordinal);
        Assert.DoesNotContain("RST_RCVD", half, StringComparison.Ordinal);

        var neither = AdifLog.Record(Psk31Contact());

        Assert.DoesNotContain("RST_SENT", neither, StringComparison.Ordinal);
        Assert.DoesNotContain("RST_RCVD", neither, StringComparison.Ordinal);
    }

    /// <summary>**An FT8 and an FT4 record are byte for byte what they were.**</summary>
    /// <remarks>
    /// **THE FIELDS ADDED FOR PSK31 ARE PINNED TO PSK31 BY THIS TEST.** Both kinds of
    /// report project onto the same two ADIF tags, so the way this change could go
    /// quietly wrong is an FT8 record growing, losing or reordering a tag. The whole
    /// record is compared rather than one field, because a reordering is what another
    /// program's importer notices and a field assertion does not.
    /// </remarks>
    [Fact]
    public void AnFt8AndAnFt4RecordAreByteIdenticalToWhatTheyWere()
    {
        var ft8 = AdifLog.Record(Contact(ContactModes.Named("FT8")));
        var ft4 = AdifLog.Record(Contact(ContactModes.Named("FT4")));

        _output.WriteLine(ft8);
        _output.WriteLine(ft4);

        const string Expected =
            "<CALL:6>IK4LZH\n"
            + "<STATION_CALLSIGN:6>KC3QIS\n"
            + "<QSO_DATE:8>20260909\n"
            + "<TIME_ON:6>142238\n"
            + "<TIME_OFF:6>142245\n"
            + "<BAND:3>40m\n"
            + "<MODE:3>FT8\n"
            + "<FREQ:8>7.047500\n"
            + "<RST_SENT:3>+00\n"
            + "<RST_RCVD:3>-12\n"
            + "<GRIDSQUARE:4>JN54\n"
            + "<MY_GRIDSQUARE:6>FN00DJ\n"
            + "<EOR>\n";

        Assert.Equal(Expected, ft8);

        // FT4 is the same record with the pair that names it, and nothing else moves.
        Assert.Equal(
            Expected
                .Replace("<MODE:3>FT8\n", "<MODE:4>MFSK\n<SUBMODE:3>FT4\n", StringComparison.Ordinal),
            ft4);

        // **AND NEITHER CARRIES AN RST FIELD**, because neither exchanged one: the
        // decibel report is what is in those tags on these two records.
        foreach (var record in new[] { ft8, ft4 })
        {
            var read = Assert.Single(AdifLog.Read(record));

            Assert.Null(read.RstSent);
            Assert.Null(read.RstReceived);
            Assert.Equal("+00", read.ReportSent);
            Assert.Equal("-12", read.ReportReceived);
        }
    }

    /// <summary>A PSK31 contact as the one write path would produce it.</summary>
    /// <remarks>
    /// **NOTHING IS SET ON THE CONTACT AFTERWARDS BUT THE REPORT.** The mode goes in
    /// through the conditions as one object and both tags come out of it, so no
    /// assignment here could make `MODE` and `SUBMODE` disagree. The ledger holds the
    /// PSK31 messages the way the application books them - handed in already read by
    /// `Psk31ExchangeParser`, with no FT8 fields (see
    /// <see cref="Ft8ContactLedger.RecordPsk31"/>).
    /// </remarks>
    private static AdifContact Psk31Contact()
    {
        var ledger = new Ft8ContactLedger("KC3QIS");
        var at = new DateTime(2026, 9, 9, 14, 22, 30, DateTimeKind.Utc);

        ledger.RecordPsk31(
            "G4XYZ", "KC3QIS DE G4XYZ you are 589 589 here KC3QIS DE G4XYZ KN",
            fromTheOperator: false, at);

        ledger.RecordPsk31(
            "G4XYZ", "G4XYZ de KC3QIS  RST 599 599  BTU G4XYZ de KC3QIS K",
            fromTheOperator: true, at.AddSeconds(45));

        return Ft8ContactLogEntry.For(
            ledger.For("G4XYZ")!,
            "KC3QIS",
            new Ft8StationConditions(
                14_070_000, "20 m", ContactModes.Named("PSK31"), "FN00DJ"));
    }

    /// <summary>One FT8-shaped contact off the ledger, in the mode handed in.</summary>
    private static AdifContact Contact(ContactMode? mode)
    {
        var ledger = new Ft8ContactLedger("KC3QIS");
        var slot = new DateTime(2026, 9, 9, 14, 22, 30, DateTimeKind.Utc);

        ledger.RecordHeard("CQ IK4LZH JN54", slot);
        ledger.RecordSent("IK4LZH KC3QIS R+00", slot.AddSeconds(8));
        ledger.RecordHeard("KC3QIS IK4LZH -12", slot.AddSeconds(15));

        return Ft8ContactLogEntry.For(
            ledger.For("IK4LZH")!,
            "KC3QIS",
            new Ft8StationConditions(7_047_500, "40 m", mode, "FN00DJ"));
    }
}

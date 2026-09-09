using System;
using System.Linq;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 292, task 6: an FT4 contact reads as FT4 on both ledger surfaces,
/// and an FT8 contact is exactly what it was.
/// </summary>
/// <remarks>
/// <para>**THE BREAKAGE THIS CATCHES.** `MFSK` is the ADIF mode FT4, JS8, MSK144 and
/// half a dozen others share, so a log window drawing the `MODE` field alone showed
/// three different modes under one word - in the file whose whole purpose is to say
/// what he worked, and which outlives everything else in this project. Unit 291 gave
/// the record its `SUBMODE`, proved it round-trips, and named these two surfaces as
/// the ones that still dropped it.</para>
/// <para>**AND THE OTHER HALF, WHICH IS THE ONE THAT COULD GO QUIETLY WRONG.** An FT8
/// contact has no submode and must not be made to look as though it were missing one.
/// Absent is absent on screen exactly as it is in the file: no row in the dialog, no
/// separator in the cell, and `Summary` still saying Hamlet heard all of them
/// (§0.0, `AdifLog.cs:8-13`).</para>
/// <para>**BOTH SURFACES OR NEITHER.** A log window that shows the submode beside a
/// dialog that does not is worse than neither, because the operator learns to trust
/// one screen and not the other. Every assertion below is made twice, once on each.
/// </para>
/// </remarks>
public sealed class TheLedgerShowsTheSubmodeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cells and the fields are printed.</param>
    public TheLedgerShowsTheSubmodeTests(ITestOutputHelper output) => _output = output;

    /// <summary>An FT4 contact says FT4 in the log window, not `MFSK`.</summary>
    [Fact]
    public void TheWindowsModeCellNamesTheSubmodeWhereThereIsOne()
    {
        var row = Row(Worked("MFSK", "FT4"));

        _output.WriteLine("mode cell : " + row.Mode);
        _output.WriteLine("submode   : " + Quoted(row.Submode));

        Assert.Equal("MFSK · FT4", row.Mode);
        Assert.Equal("FT4", row.Submode);

        // The ADIF mode is still there and still readable, because it is what the file
        // says and what another logger would import.
        Assert.Contains("MFSK", row.Mode, StringComparison.Ordinal);
    }

    /// <summary>An FT8 contact's cell is what it was, and its submode is empty.</summary>
    /// <remarks>
    /// **AND EMPTY RATHER THAN `not recorded`.** Every other absent cell in that table
    /// means *Hamlet did not record this*; an FT8 submode is not unrecorded, it does
    /// not exist, and `not recorded` there would assert a hole where there is none.
    /// </remarks>
    [Fact]
    public void TheWindowsModeCellIsUntouchedWhereThereIsNoSubmode()
    {
        var row = Row(Worked("FT8", null));

        _output.WriteLine("mode cell : " + row.Mode);
        _output.WriteLine("submode   : " + Quoted(row.Submode));

        Assert.Equal("FT8", row.Mode);
        Assert.Equal("", row.Submode);

        Assert.DoesNotContain("·", row.Mode, StringComparison.Ordinal);
        Assert.DoesNotContain(ContactLogRow.Absent, row.Mode, StringComparison.Ordinal);
    }

    /// <summary>The dialog carries a Submode row, with its ADIF tag, for FT4.</summary>
    /// <remarks>
    /// **THE TAG IS BESIDE THE VALUE THE WAY EVERY OTHER ROW'S IS.** The dialog shows
    /// what will be written and under which name, so a row without its tag would be the
    /// one field he could not check against the file.
    /// </remarks>
    [Fact]
    public void TheDialogCarriesASubmodeRowWithItsAdifTag()
    {
        var dialog = new LogContactViewModel(Worked("MFSK", "FT4"), Dial);

        foreach (var field in dialog.Fields)
        {
            _output.WriteLine($"  {field.Label,-16} {field.AdifField,-24} {field.Shown}");
        }

        var submode = dialog.Fields.Single(field => field.AdifField == "SUBMODE");

        Assert.Equal("Submode", submode.Label);
        Assert.Equal("FT4", submode.Value);
        Assert.True(submode.Observed);

        // The mode row is still there and still says MFSK, so the two travel together
        // on screen exactly as they do in the record.
        var mode = dialog.Fields.Single(field => field.AdifField == "MODE");

        Assert.Equal("MFSK", mode.Value);

        // **AND THE SUBMODE COMES STRAIGHT AFTER THE MODE.** A reader checking what is
        // about to be written reads them as one fact; separating them by four rows
        // invites reading either one alone.
        Assert.Equal(
            dialog.Fields.IndexOf(mode) + 1, dialog.Fields.IndexOf(submode));
    }

    /// <summary>
    /// An FT8 contact has no Submode row at all, and the dialog still says Hamlet heard
    /// all of them.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE ASSERTION THE INSTRUCTION ASKED FOR BY NAME.** An always-present
    /// row would print `Hamlet did not hear this` beside `SUBMODE` about a contact where
    /// it heard everything there was, and `Summary` would count it - *Hamlet heard 11 of
    /// these 12* - which is the dialog claiming a failure that did not happen, in the
    /// window that decides what goes into the permanent record.
    /// </remarks>
    [Fact]
    public void AnFt8ContactHasNoSubmodeRowAndNoUnheardFieldBecauseOfIt()
    {
        // **THE DIAL IS SUPPLIED, SO EVERY OTHER ROW IS OBSERVED.** Without it the
        // Frequency row is legitimately empty and the summary says *10 of these 11*
        // about a row that has nothing to do with the submode - which would leave this
        // test unable to tell the two absences apart.
        var ft8 = new LogContactViewModel(Worked("FT8", null), Dial);
        var ft4 = new LogContactViewModel(Worked("MFSK", "FT4"), Dial);

        _output.WriteLine("FT8 : " + ft8.Summary);
        _output.WriteLine("FT4 : " + ft4.Summary);

        Assert.DoesNotContain(ft8.Fields, field => field.AdifField == "SUBMODE");
        Assert.Equal(ft4.Fields.Count - 1, ft8.Fields.Count);

        // Every field of a whole FT8 contact was heard, and the summary still says so.
        Assert.Equal(ft8.Fields.Count, ft8.ObservedCount);
        Assert.Contains("heard all", ft8.Summary, StringComparison.Ordinal);
        Assert.DoesNotContain("SUBMODE", ft8.Summary, StringComparison.Ordinal);

        // And nowhere on the FT8 dialog does the word appear beside an empty value.
        Assert.DoesNotContain(
            ft8.Fields,
            field => field.Shown.Contains("did not hear", StringComparison.Ordinal));
    }

    /// <summary>How the dial is described on screen, as the Digital tab supplies it.</summary>
    private const string Dial = "14.080000 MHz, where this was heard";

    /// <summary>A contact whose mode and submode are what the record carries.</summary>
    private static AdifContact Worked(string mode, string? submode)
        => new()
        {
            Call = "IK4LZH",
            StationCallsign = "KC3QIS",
            StartedUtc = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc),
            EndedUtc = new DateTime(2026, 9, 9, 12, 1, 0, DateTimeKind.Utc),
            Band = "20m",
            Mode = mode,
            Submode = submode,
            ReportSent = "-11",
            ReportReceived = "-09",
            GridSquare = "JN54",
            MyGridSquare = "FN00DJ",
        };

    /// <summary>One row, through the writer and the reader (§12.5).</summary>
    /// <remarks>
    /// **THROUGH `AdifLog` BOTH WAYS**, the way `TheLogShowsBothGridsTests` does. A
    /// hand-built record would prove the row reads a property, and the question is
    /// whether the field survives the file.
    /// </remarks>
    private static ContactLogRow Row(AdifContact contact)
    {
        var text = AdifLog.Header("1.12.234") + AdifLog.Record(contact);

        return new ContactLogRow(AdifLog.ReadRecords(text).Single());
    }

    private static string Quoted(string value) => "\"" + value + "\"";
}

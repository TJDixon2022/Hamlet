using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 299, task 2: **the `i` carries every fact the ledger holds and
/// none it does not.**
/// </summary>
/// <remarks>
/// <para>**THE INSTRUCTION SAYS THE `i` IS EMPTY AND IT IS NOT.**
/// `Unit299HeaderProbeTests` reads the tooltip off the realized control in the real
/// window: the mark draws at 14 by 14 pixels and carried **444 characters** before
/// this unit. The claim is a mismatch and it is reported rather than repaired.
/// </para>
/// <para>**WHAT WAS ACTUALLY WRONG IS SMALLER AND REAL: THE HOVER WENT THIN.** The
/// band paragraph - where his tone sat and what the dial was on - came off the newest
/// decoded row for that station. **The decoded table is bounded and clears on a band
/// change**, and the cards are rebuilt from the ledger, which persists; so after a
/// retune the `i` quietly lost the frequency and the time offset while keeping the
/// rest. Measured in the window: 444 characters before, **875 after**.</para>
/// <para>**WHAT IS REMEMBERED IS A MEASUREMENT AND NOT A GUESS** (§0.0). Only a
/// decoded row writes into that memory, so every figure it hands back was on a row
/// that really decoded from that station - never the dial the radio happens to be
/// standing on now.</para>
/// </remarks>
public sealed class Unit299HoverTests
{
    private const string HisCall = "KC3QIS";
    private const string Station = "K9XP";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the hovers are printed.</param>
    public Unit299HoverTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A full exchange's hover names every fact its ledger holds.**</summary>
    [Fact]
    public void TheHoverNamesEveryFactTheLedgerHolds()
    {
        var card = AFullExchange().DigitalCards.Single();

        _output.WriteLine(card.Detail);

        foreach (var (fact, expected) in new[]
        {
            ("the report he gave", "-9 dB"),
            ("the report you gave", "-12 dB"),
            ("where the decoder reads to", "-21"),
            ("his grid", "EN52"),
            ("the distance", "miles"),
            ("the bearing", "bearing of"),
            ("the audio offset", "1240 Hz"),
            ("the dial", "14.074000 MHz"),
            ("the time offset", "0.2 seconds into the slot"),
            ("the slot times", "02:11:00 to"),
            // The wording is singular at one slot, which is why this matches the
            // stem rather than the plural.
            ("the slot count", "slot ago"),
            ("what closed it", "RR73"),
        })
        {
            Assert.True(
                card.Detail.Contains(expected, StringComparison.Ordinal),
                "the hover does not carry " + fact + " (" + expected + ")");
        }
    }

    /// <summary>**And it names no fact the ledger does not hold.**</summary>
    /// <remarks>
    /// **A FACT HAMLET DID NOT OBSERVE IS ABSENT FROM THE HOVER TOO**, not shown as
    /// blank. This exchange carries no report either way and no grid, so the hover
    /// has no decibel figure and no distance at all - and says why rather than
    /// leaving a hole.
    /// </remarks>
    [Fact]
    public void TheHoverNamesNoFactTheLedgerDoesNotHold()
    {
        var card = ABareExchange().DigitalCards.Single();

        _output.WriteLine(card.Detail);

        Assert.DoesNotContain(" dB", card.Detail, StringComparison.Ordinal);
        Assert.DoesNotContain(" miles", card.Detail, StringComparison.Ordinal);
        Assert.DoesNotContain("bearing", card.Detail, StringComparison.Ordinal);

        // **AND IT SAYS SO** rather than leaving a gap.
        Assert.Contains(
            "has not put a grid square on the air", card.Detail,
            StringComparison.Ordinal);
    }

    /// <summary>**The band paragraph survives the decoded table being cleared.**</summary>
    /// <remarks>
    /// **THIS IS THE DEFECT THE PROBE FOUND.** A card is rebuilt from the ledger and
    /// outlives its rows; before this unit the hover lost the frequency and the time
    /// offset the moment the table did.
    /// </remarks>
    [Fact]
    public void TheBandParagraphSurvivesTheRowsGoingAway()
    {
        var model = AFullExchange();

        var before = model.DigitalCards.Single().Detail;

        Assert.Contains("1240 Hz", before, StringComparison.Ordinal);

        // **THE TABLE CLEARS**, which a band change does on its own.
        model.ClearDigitalDecodesCommand.Execute(null);
        model.RebuildCardsForTests();

        var after = model.DigitalCards.Single().Detail;

        _output.WriteLine("before " + before.Length + " characters, after "
            + after.Length);

        Assert.Contains("1240 Hz", after, StringComparison.Ordinal);
        Assert.Contains("14.074000 MHz", after, StringComparison.Ordinal);
    }

    /// <summary>**A station never heard has no band paragraph at all.**</summary>
    /// <remarks>
    /// **NOTHING MEASURED ONE, SO NOTHING CLAIMS ONE** (§0.0). He called a station
    /// that has never come back: there is no audio offset, no dial and no time
    /// offset to report, and the memory is empty for it.
    /// </remarks>
    [Fact]
    public void AStationNeverHeardHasNoBandParagraph()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");

        var card = At(model, "02:11:15").DigitalCards.Single();

        _output.WriteLine(card.Detail);

        Assert.DoesNotContain("Hz", card.Detail, StringComparison.Ordinal);
        Assert.DoesNotContain("MHz", card.Detail, StringComparison.Ordinal);
    }

    /// <summary>A whole exchange with reports, a grid and a sign-off.</summary>
    private static MainWindowViewModel AFullExchange()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " EN52");
        Sent(model, "02:11:30", Station + " " + HisCall + " R-12");
        Heard(model, "02:11:45", HisCall + " " + Station + " -09");
        Heard(model, "02:12:00", HisCall + " " + Station + " RR73");

        return At(model, "02:12:15");
    }

    /// <summary>An exchange that carries no report, no grid and no measurements.</summary>
    private static MainWindowViewModel ABareExchange()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        model.AddDecodeRowForTests(
            "021115", "", "", "", HisCall + " " + Station + " RRR",
            Slot("02:11:15"));

        return At(model, "02:11:30");
    }

    /// <summary>Read the panel at a moment.</summary>
    private static MainWindowViewModel At(MainWindowViewModel model, string at)
    {
        model.CardsNowForTests = Slot(at);
        model.RebuildCardsForTests();

        return model;
    }

    /// <summary>An empty panel with his callsign and no log.</summary>
    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings();

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00";

        var model = new MainWindowViewModel(settings, null) { DigitalNewestFirst = false };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        return model;
    }

    /// <summary>One message off the air, in its slot.</summary>
    private static void Heard(MainWindowViewModel model, string at, string message)
        => model.AddDecodeRowForTests(
            Stamp(at), "-09", "0.2", "1240", message, Slot(at), 14_074_000);

    /// <summary>One message this station transmitted, both halves.</summary>
    private static void Sent(MainWindowViewModel model, string at, string message)
    {
        model.AddSentRowForTests(message, Slot(at));
        model.RecordSentForTests(message, Slot(at));
    }

    /// <summary>A slot boundary on the evening in question, in true UTC.</summary>
    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-08 " + at,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

    /// <summary>The `HHmmss` cell for a moment.</summary>
    private static string Stamp(string at)
        => Slot(at).ToString("HHmmss", CultureInfo.InvariantCulture);
}

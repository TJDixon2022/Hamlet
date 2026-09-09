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
/// Work instruction 299, task 1: **the Log option is on every card, in every state.**
/// </summary>
/// <remarks>
/// <para>**TIM RULED IT FROM USE, 2026-09-09**: *"the log option is always
/// available... I just may be interested in everyone who responded to me, even if
/// they do not respond back to me responding to them. It should be up to me what I
/// want to log."*</para>
/// <para>**THE BREAKAGE THIS WOULD HAVE CAUGHT** is the application asserting a
/// standard he did not set. Unit 297 put Log on a card only when
/// `Ft8ContactStates.IsComplete` was satisfied - both callsigns, a grid or report
/// each way and an acknowledgement each way - so a station that came back once and
/// then went quiet could not be written down at all. **That is Hamlet deciding what
/// counts as a contact**, and it does not.</para>
/// <para>**AND THE SECOND HALF, WHICH IS §0.0**: the record it offers must stay
/// honest. A partial exchange writes a partial record, with every unobserved field
/// left out of the file entirely rather than written empty or filled in.</para>
/// <para>**IT WAS WATCHED FAILING.** With `ShowsLogLink` returning
/// `ActionKind == Ft8CardActionKind.Log`, `EveryStateOffersLog` reports three of the
/// four states with no Log at all.</para>
/// </remarks>
public sealed class Unit299LogAlwaysTests
{
    private const string HisCall = "KC3QIS";
    private const string Station = "K9XP";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the four states are printed.</param>
    public Unit299LogAlwaysTests(ITestOutputHelper output) => _output = output;

    /// <summary>**All four states offer Log.**</summary>
    [Fact]
    public void EveryStateOffersLog()
    {
        foreach (var (state, model) in new (string, MainWindowViewModel)[]
        {
            ("Your turn", HeAnswered()),
            ("Waiting on him", HeAnsweredAndSoDidWe()),
            ("Finished", AFinishedContact()),
            ("Gone quiet", HeStopped()),
        })
        {
            var card = model.DigitalCards.Single();

            var offered = card.ActionKind == Ft8CardActionKind.Log
                ? "as the action"
                : card.ShowsLogLink ? "as the link" : "NOT AT ALL";

            _output.WriteLine(state.PadRight(16) + " -> Log " + offered
                + "   (action: " + card.ActionLabel + ")");

            Assert.Equal(state, card.StateWord);

            Assert.True(
                card.ActionKind == Ft8CardActionKind.Log || card.ShowsLogLink,
                state + " withholds the Log option, and Hamlet does not decide what "
                + "counts as a contact");
        }
    }

    /// <summary>**And it is never drawn twice on one card.**</summary>
    [Fact]
    public void ItIsNeverOfferedTwiceOnOneCard()
    {
        var finished = AFinishedContact().DigitalCards.Single();

        Assert.Equal(Ft8CardActionKind.Log, finished.ActionKind);
        Assert.False(finished.ShowsLogLink);
    }

    /// <summary>
    /// **A partial contact builds a record with the missing fields absent.**
    /// </summary>
    /// <remarks>
    /// **ABSENT, NOT EMPTY AND NOT PLAUSIBLE** (§0.0, and `AdifContact`'s own rule).
    /// The exchange here got as far as his grid and stopped: there is no report
    /// either way, so both report fields are null and neither reaches the file.
    /// </remarks>
    [Fact]
    public void APartialContactSavesWhatHappenedAndNoMore()
    {
        var model = HeAnswered();

        var entry = model.ContactLogEntryForStation(Station, 14_074_000);

        Assert.True(entry is not null, "a partial contact produced no record at all");

        _output.WriteLine("CALL          : " + entry!.Call);
        _output.WriteLine("GRIDSQUARE    : " + (entry.GridSquare ?? "(not recorded)"));
        _output.WriteLine("RST_SENT      : " + (entry.ReportSent ?? "(not recorded)"));
        _output.WriteLine("RST_RCVD      : " + (entry.ReportReceived ?? "(not recorded)"));
        _output.WriteLine("BAND          : " + (entry.Band ?? "(not recorded)"));

        Assert.Equal(Station, entry.Call);
        Assert.Equal("EN52", entry.GridSquare);

        // **NEITHER REPORT PASSED, SO NEITHER IS IN THE RECORD.**
        Assert.Null(entry.ReportSent);
        Assert.Null(entry.ReportReceived);

        var written = AdifLog.Record(entry);

        _output.WriteLine("");
        _output.WriteLine(written.Trim());

        Assert.DoesNotContain("<RST_SENT", written, StringComparison.Ordinal);
        Assert.DoesNotContain("<RST_RCVD", written, StringComparison.Ordinal);
        Assert.Contains("<GRIDSQUARE:4>EN52", written, StringComparison.Ordinal);
    }

    /// <summary>
    /// **A card whose rows have gone still builds a record, with no frequency.**
    /// </summary>
    /// <remarks>
    /// **THE DECODED TABLE IS BOUNDED AND CLEARS ON A BAND CHANGE**, so a card can
    /// outlive the rows behind it. Before this unit the Log path needed a row and
    /// simply did nothing when there was none. **The dial goes out of the record
    /// rather than being taken from where the radio is standing now** (§0.0): a band
    /// he retuned to is not the band he worked the station on.
    /// </remarks>
    [Fact]
    public void AStationWithNoRowsStillLogsAndTheDialIsAbsent()
    {
        var model = HeAnswered();

        var entry = model.ContactLogEntryForStation(Station, 0);

        Assert.True(entry is not null, "the ledger held the contact and produced no record");

        Assert.Null(entry!.FrequencyMhz);
        Assert.Null(entry.Band);

        var written = AdifLog.Record(entry);

        _output.WriteLine(written.Trim());

        Assert.DoesNotContain("<FREQ", written, StringComparison.Ordinal);
        Assert.DoesNotContain("<BAND", written, StringComparison.Ordinal);
        Assert.Contains("<CALL:4>" + Station, written, StringComparison.Ordinal);
    }

    /// <summary>**The X keeps its guard on a finished, unlogged card.**</summary>
    /// <remarks>
    /// Unit 297's rule, re-asserted because this unit changed what a finished card
    /// offers and the guard rides on the same state.
    /// </remarks>
    [Fact]
    public void ClearingAFinishedUnloggedCardStillWarns()
    {
        var model = AFinishedContact();

        model.ClearCardCommand.Execute(Station);

        var card = model.DigitalCards.Single();

        Assert.True(card.WarnsBeforeClearing, "the X lost its guard");
        Assert.Contains("not in your log", card.ClearWarning, StringComparison.Ordinal);
    }

    /// <summary>He answered the operator with a grid, and nothing more passed.</summary>
    private static MainWindowViewModel HeAnswered()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " EN52");

        return At(model, "02:11:30");
    }

    /// <summary>He answered and the operator answered back.</summary>
    private static MainWindowViewModel HeAnsweredAndSoDidWe()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " EN52");
        Sent(model, "02:11:30", Station + " " + HisCall + " R-12");

        return At(model, "02:11:45");
    }

    /// <summary>A whole exchange, ending in his roger.</summary>
    private static MainWindowViewModel AFinishedContact()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " EN52");
        Sent(model, "02:11:30", Station + " " + HisCall + " R-12");
        Heard(model, "02:11:45", HisCall + " " + Station + " RRR");

        return At(model, "02:12:00");
    }

    /// <summary>He answered once and then stopped transmitting.</summary>
    private static MainWindowViewModel HeStopped()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " EN52");

        return At(model, "02:12:30");
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

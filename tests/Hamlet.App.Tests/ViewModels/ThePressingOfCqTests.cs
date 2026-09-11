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
/// Work instruction 305 task 2: **pressing CQ makes a card of its own, and when
/// somebody answers it becomes their card.**
/// </summary>
/// <remarks>
/// <para>**HE PRESSED CQ ON THE LIVE BUILD AND THE SCREEN TOLD HIM NOTHING.** The
/// transmission went out - the send chain is proved from his own telemetry - and the
/// card panel stayed empty, because the ledger is keyed by callsign and a CQ is
/// addressed to nobody. **Measured before this unit: 0 cards.**</para>
/// <para>**NOTHING HERE INTERPRETS THE MESSAGE.** The card carries what was sent,
/// when, and how many times. It makes no claim about whether anybody will answer,
/// and there is no *waiting for a reply* anywhere in it - the elapsed time and the
/// count are facts and everything beyond them would be a guess (§0.0).</para>
/// <para>**THE TWO-ANSWER RULE IS THE AUTHOR'S PROPOSAL AND IS MARKED AS ONE.** The
/// first station to answer adopts the card; a second station answering the same CQ
/// opens its own card in the ordinary way. Nothing is discarded and nothing is
/// swallowed, because nothing is hidden by the application. It is reproduced in the
/// report for Tim to overrule.</para>
/// <para>**NOTHING HERE TRANSMITS** (§0.2). The send path is proved and parked; this
/// drives the ledger through the same seam the tests already use.</para>
/// </remarks>
public sealed class ThePressingOfCqTests
{
    private const string HisCall = "KC3QIS";
    private const string First = "K9XP";
    private const string Second = "W1ABC";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the cards are printed.</param>
    public ThePressingOfCqTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A CQ makes a card of its own.**</summary>
    [Fact]
    public void ACqMakesACardOfItsOwn()
    {
        var model = Panel();

        Cq(model, "02:11:00");

        var card = Assert.Single(At(model, "02:11:15").DigitalCards);

        Print(model);

        // **IT IS THE CQ'S OWN CARD**, not a station's, and it says so on its face
        // rather than borrowing somebody's callsign.
        Assert.Equal(Ft8ContactLedger.CallToAnyone, card.Callsign);

        // **AND IT CARRIES WHAT WAS SENT AND WHEN.**
        Assert.Contains("CQ", card.Detail, StringComparison.Ordinal);
    }

    /// <summary>**A second CQ counts up in the same card.**</summary>
    [Fact]
    public void ASecondCqCountsUpInTheSameCard()
    {
        var model = Panel();

        Cq(model, "02:11:00");
        Cq(model, "02:11:30");
        Cq(model, "02:12:00");

        var card = Assert.Single(At(model, "02:12:15").DigitalCards);

        Print(model);

        // **ONE CARD, NOT THREE**, and the count is a fact rather than a claim.
        Assert.Contains("3", card.Detail, StringComparison.Ordinal);
    }

    /// <summary>**An answer retires the receipt and opens the station's own card.**</summary>
    /// <remarks>
    /// **REWRITTEN BY UNIT 310, BECAUSE R5 WITHDREW WHAT IT USED TO ASSERT.** It said
    /// the first answering station adopted the CQ card so the call stayed inside that
    /// exchange - the unit 305 proposal, never a ruling. Tim, 2026-09-11: *"No, the
    /// placeholder goes away and two conversation card appear."* **A call to everybody
    /// belongs to nobody**, and giving it to whichever station answered first is a
    /// claim about who it was for.
    /// </remarks>
    [Fact]
    public void AnAnswerRetiresTheReceiptAndOpensTheStationOwnCard()
    {
        var model = Panel();

        Cq(model, "02:11:00");
        Heard(model, "02:11:15", HisCall + " " + First + " EN52");

        var card = Assert.Single(At(model, "02:11:30").DigitalCards);

        Print(model);

        // **IT BECAME HIS CARD.**
        Assert.Equal(First, card.Callsign);

        // **AND THE CQ IS NOT INSIDE IT** (R5). His record holds what passed
        // between the two of them and nothing that was addressed to everybody.
        var record = model.LedgerForTests!.For(First)!;

        Assert.DoesNotContain(
            record.Sent, m => m.Message.StartsWith("CQ ", StringComparison.Ordinal));

        // **AND THE RECEIPT IS GONE**, because it retired rather than being handed
        // over.
        Assert.Null(model.LedgerForTests.For(Ft8ContactLedger.CallToAnyone));
    }

    /// <summary>**A second answering station gets its own card.**</summary>
    [Fact]
    public void ASecondAnsweringStationGetsItsOwnCard()
    {
        var model = Panel();

        Cq(model, "02:11:00");
        Heard(model, "02:11:15", HisCall + " " + First + " EN52");
        Heard(model, "02:11:15", HisCall + " " + Second + " FN31");

        var cards = At(model, "02:11:30").DigitalCards;

        Print(model);

        // **NOTHING IS DISCARDED AND NOTHING IS SWALLOWED.**
        Assert.Equal(2, cards.Count);

        Assert.Contains(cards, c => c.Callsign == First);
        Assert.Contains(cards, c => c.Callsign == Second);

        // **AND THE CQ WENT TO NEITHER OF THEM** (R5, Tim 2026-09-11, rewritten by
        // unit 310). This used to assert that the first to answer took the call over.
        // **Two stations answering is exactly where that proposal shows its seam**:
        // the call went to everybody, so putting it in one of these two records is a
        // claim about who it was for. The receipt retires instead.
        foreach (var who in new[] { First, Second })
        {
            Assert.DoesNotContain(
                model.LedgerForTests!.For(who)!.Sent,
                m => m.Message.StartsWith("CQ ", StringComparison.Ordinal));
        }

        Assert.Null(model.LedgerForTests!.For(Ft8ContactLedger.CallToAnyone));
    }

    /// <summary>**The CQ card carries no way to log, and the station's card does.**</summary>
    /// <remarks>
    /// <para>**REWRITTEN 2026-09-11 BECAUSE TIM WITHDREW WHAT IT ASSERTED.** It read
    /// *the CQ card carries the Log option before anybody answers*, on unit 305's
    /// reading of *"It should be up to me what I want to log"* - that the option is on
    /// every card in every state and a CQ card is a card. His ruling of 2026-09-11 is
    /// the opposite: *"CQ should not have log option, that is
    /// self-gratification."*</para>
    /// <para>**A CQ IS NOT A CONTACT**, so there is nothing to write down. What the
    /// earlier reading was protecting is kept, and this test now asserts it from the
    /// other side: **the moment somebody answers, the Log is on their card**, where
    /// there is a contact to log.</para>
    /// </remarks>
    [Fact]
    public void TheCqCardCarriesNoWayToLogAndTheStationCardDoes()
    {
        var model = Panel();

        Cq(model, "02:11:00");

        var card = Assert.Single(At(model, "02:11:15").DigitalCards);

        _output.WriteLine("card         : " + card.Callsign);
        _output.WriteLine("action       : " + card.ActionKind);
        _output.WriteLine("log link     : " + card.ShowsLogLink);

        Assert.True(card.IsCallToAnyone);

        // **NEITHER THE BUTTON NOR THE LINK.**
        Assert.Equal(Ft8CardActionKind.None, card.ActionKind);
        Assert.False(card.ShowsLogLink);

        // **AND WHEN SOMEBODY ANSWERS, THEIR CARD HAS IT.** The receipt retires, a
        // conversation card opens, and that is where a contact can be logged.
        Heard(model, "02:11:15", HisCall + " K9XP EN52");

        var answered = At(model, "02:11:30").DigitalCards
            .Single(c => c.Callsign == "K9XP");

        _output.WriteLine("answered     : " + answered.Callsign
            + ", action " + answered.ActionKind
            + ", log link " + answered.ShowsLogLink);

        Assert.True(
            answered.ActionKind == Ft8CardActionKind.Log || answered.ShowsLogLink,
            "the station's own card offers no way to log at all");
    }

    private void Print(MainWindowViewModel model)
    {
        foreach (var card in model.DigitalCards)
        {
            _output.WriteLine("CARD  " + card.Callsign);
            _output.WriteLine("  face   : " + card.TimeLine);
            _output.WriteLine("  action : " + card.ActionLabel);
            _output.WriteLine("  detail : " + card.Detail);
            _output.WriteLine("");
        }
    }

    /// <summary>Press CQ, the way the send path books it.</summary>
    private static void Cq(MainWindowViewModel model, string at)
        => model.RecordSentForTests("CQ " + HisCall + " FN00", Slot(at));

    private static void Heard(MainWindowViewModel model, string at, string message)
        => model.AddDecodeRowForTests(
            at.Replace(":", "", StringComparison.Ordinal),
            "-09", "0.2", "1240", message, Slot(at), heardOnHz: 14_074_000);

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

        var model = new MainWindowViewModel(settings, null)
        {
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        return model;
    }

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-10 " + at, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}

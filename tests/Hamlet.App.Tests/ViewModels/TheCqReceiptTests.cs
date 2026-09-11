using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 310 task 2: **a receipt is not a conversation card with blanks in
/// it.**
/// </summary>
/// <remarks>
/// <para>**R1 AND R2** (Tim, 2026-09-11): *"There are two card type: my CQ and a
/// conversation"*, and *"the CQ Card is short - just a note that I transmitted it.
/// Placeholder until it is answered"*.</para>
/// <para>**EMPTY SLOTS ARE WHAT PRODUCED PORTUGAL.** The receipt was the conversation
/// frame with the other station set to the literal string `CQ`, and `CQ` is a real
/// Portuguese prefix - CT, CR and CQ all belong to Portugal - so every station slot
/// filled itself in: a country, a state, a pronoun and a caption about where a thing
/// that does not exist is.</para>
/// <para>**R4** - *"just the last I don't want reminders of failures"*. No count of
/// calls, and no sentence that frames ordinary silence as a station failing to come
/// back. `ACHIEVEMENTS_PHILOSOPHY.md` §2 is the same instinct one screen over.</para>
/// </remarks>
public sealed class TheCqReceiptTests
{
    private const string HisCall = "KC3QIS";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the receipt is printed.</param>
    public TheCqReceiptTests(ITestOutputHelper output) => _output = output;

    /// <summary>**A press makes a receipt, and it exposes no station facts.**</summary>
    [Fact]
    public void APressMakesAReceiptAndItExposesNoStationFacts()
    {
        var model = Panel();

        model.SendCallToAnyoneCommand.Execute(null);

        var receipt = Assert.Single(model.DigitalCards);

        _output.WriteLine("is a receipt : " + receipt.IsCallToAnyone);
        _output.WriteLine("callsign     : [" + receipt.Callsign + "]");
        _output.WriteLine("place        : [" + receipt.Place + "]");
        _output.WriteLine("state word   : [" + receipt.StateWord + "]");
        _output.WriteLine("sentence     : [" + receipt.Sentence + "]");
        _output.WriteLine("time line    : [" + receipt.TimeLine + "]");
        _output.WriteLine("action label : [" + receipt.ActionLabel + "]");
        _output.WriteLine("shows globe  : " + receipt.ShowsGlobe);
        _output.WriteLine("detail       : [" + receipt.Detail + "]");

        Assert.True(receipt.IsCallToAnyone);

        // **NO PLACE, NO COUNTRY, NO ENTITY.** This is where Portugal was.
        Assert.Empty(receipt.Place);
        Assert.False(receipt.HasPlace);

        // **NO MAP ROW**, because there is nowhere to draw and nothing to draw it for.
        Assert.False(receipt.ShowsGlobe);

        // **NO GRID, NO DISTANCE, NO BEARING** anywhere on it.
        foreach (var forbidden in new[] { "grid", "miles", "north", "south", "Portugal" })
        {
            Assert.DoesNotContain(
                forbidden, Everything(receipt), StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**`CQ` never reaches the entity lookup.**</summary>
    /// <remarks>
    /// **THE GUARD IS WHERE THE QUESTION IS ASKED, NOT WHERE THE HEADER IS DRAWN.**
    /// Seven sites asked `DxccPrefixes.EntityOf` with no guard and one of them was
    /// the CQ-list nudge, which would have marked the operator own call as a country
    /// to chase.
    /// </remarks>
    [Fact]
    public void CqNeverReachesTheEntityLookup()
    {
        _output.WriteLine(
            "EntityOf(CQ)          : " + (DxccPrefixes.EntityOf("CQ") ?? "(null)"));

        _output.WriteLine(
            "ForCallsign(CQ)       : " + (DxccContinents.ForCallsign("CQ") ?? "(null)"));

        var nudges = new NudgeSet(Array.Empty<string>(), Array.Empty<string>());

        _output.WriteLine("WouldOpen(CQ)         : " + nudges.WouldOpen("CQ").Kind);

        // **A CALL TO ANYBODY HAS NO ENTITY**, at the lookup itself, so every caller
        // is covered whether or not it remembered to ask.
        Assert.Null(DxccPrefixes.EntityOf("CQ"));
        Assert.Null(DxccContinents.ForCallsign("CQ"));

        // **AND THE NUDGE CANNOT MARK IT.**
        Assert.Equal(NudgeKind.None, nudges.WouldOpen("CQ").Kind);

        // **THE STATION CALLSIGNS AROUND IT ARE UNTOUCHED**, which is the thing a
        // guard like this most easily breaks.
        Assert.Equal("Portugal", DxccPrefixes.EntityOf("CT1ABC"));
        Assert.Equal("Portugal", DxccPrefixes.EntityOf("CQ7ABC"));
    }

    /// <summary>**The receipt offers no Log.**</summary>
    /// <remarks>
    /// <para>**TIM, 2026-09-11**: *"CQ should not have log option, that is
    /// self-gratification."* **This supersedes unit 305's *Log from first appearance*
    /// and unit 310's R2**, both of which were the author's reasoning rather than his
    /// ruling.</para>
    /// <para>**A CQ IS NOT A CONTACT.** Nothing passed between two stations, so there is
    /// nothing to write down. The moment somebody answers, the Log is on their
    /// conversation card, where there is a contact to log - which
    /// <see cref="AConversationCardStillCarriesEveryStationFact"/> keeps true.</para>
    /// </remarks>
    [Fact]
    public void TheReceiptOffersNoLog()
    {
        var model = Panel();

        model.SendCallToAnyoneCommand.Execute(null);

        var receipt = Assert.Single(model.DigitalCards);

        _output.WriteLine("kind        : " + receipt.ActionKind);
        _output.WriteLine("has action  : " + receipt.HasAction);
        _output.WriteLine("label       : [" + receipt.ActionLabel + "]");
        _output.WriteLine("shows link  : " + receipt.ShowsLogLink);

        Assert.True(receipt.IsCallToAnyone);

        // **NOTHING IS OFFERED AT ALL.** Not a Log, and not a Send either - a call to
        // everybody has no addressee for a reply.
        Assert.Equal(Ft8CardActionKind.None, receipt.ActionKind);
        Assert.False(receipt.HasAction);
        Assert.Equal("", receipt.ActionLabel);

        // **AND NOT BY THE BACK DOOR.** The card carries a second route to the log
        // window as a quiet link; it must not reappear there.
        Assert.False(
            receipt.ShowsLogLink,
            "the receipt still offers a log, by the link rather than the button");

        // **THE BUTTON'S OWN WORDS ARE GONE.**
        Assert.DoesNotContain(
            "Log this call", Everything(receipt), StringComparison.OrdinalIgnoreCase);

        // **AND `LogLabel` IS NOT SWEPT FOR**, deliberately. It is a constant on every
        // card and the markup draws it only where `ShowsLogLink` is true
        // (`MainWindow.axaml:4702`), so a sweep of every property finds a string the
        // receipt never puts on screen. **What is asserted is the flag the markup
        // reads**, which is the thing that decides whether he sees it.
    }

    /// <summary>**The log hover does not name a station either.**</summary>
    /// <remarks>
    /// **THE SAME FAULT AS THE COUNTRY LINE, ONE HOVER FURTHER DOWN** (R1, R2). It read
    /// *what passed between you and CQ*, which is the card's conversation wording with
    /// the literal string dropped into it. Found while quoting the receipt word for word
    /// into the report, which is the value of quoting it.
    /// </remarks>
    [Fact]
    public void TheLogHoverDoesNotNameAStationEither()
    {
        var model = Panel();

        model.SendCallToAnyoneCommand.Execute(null);

        var receipt = Assert.Single(model.DigitalCards);

        _output.WriteLine("tip: [" + receipt.ActionTip + "]");

        Assert.DoesNotContain("CQ", receipt.ActionTip, StringComparison.Ordinal);

        Assert.DoesNotContain(
            "between you and", receipt.ActionTip, StringComparison.Ordinal);

        // **AND SINCE 2026-09-11 THERE IS NO HOVER AT ALL**, because there is no
        // control to hover. The wording this test was written for - *what passed
        // between you and CQ* - went with the Log button it belonged to, and what is
        // asserted now is that nothing took its place.
        Assert.Equal("", receipt.ActionTip);
    }

    /// <summary>**Nothing on the receipt says *he*, or *not answered*.**</summary>
    [Fact]
    public void NothingOnTheReceiptSaysHeOrNotAnswered()
    {
        var model = Panel();

        model.SendCallToAnyoneCommand.Execute(null);

        var everything = Everything(Assert.Single(model.DigitalCards));

        _output.WriteLine(everything);

        foreach (var pronoun in new[]
        {
            " he ", " him ", " his ", "He ", "Him ", "His ",
        })
        {
            Assert.DoesNotContain(pronoun, everything, StringComparison.Ordinal);
        }

        // **AND NOTHING FRAMES SILENCE AS A FAILURE** (R4).
        foreach (var failure in new[]
        {
            "not answered", "no answer", "waiting on", "nobody", "no reply",
        })
        {
            Assert.DoesNotContain(
                failure, everything, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**No count of calls appears on it, however many have gone out.**</summary>
    /// <remarks>
    /// **R4** - *"just the last I don't want reminders of failures"*. Unit 305 built a
    /// *sent n times* count into this card; it comes off. **The time alone says what
    /// is useful** - that it is live and went out seconds ago - without scoring the
    /// silence.
    /// </remarks>
    [Fact]
    public void NoCountOfCallsAppearsOnIt()
    {
        var model = Panel();

        for (var press = 0; press < 5; press++)
        {
            model.SendCallToAnyoneCommand.Execute(null);
        }

        var receipt = Assert.Single(model.DigitalCards);
        var everything = Everything(receipt);

        _output.WriteLine("after five presses:");
        _output.WriteLine(everything);

        foreach (var counted in new[]
        {
            "5 times", "five times", "twice", "3 times", "Sent once", "times",
        })
        {
            Assert.DoesNotContain(
                counted, everything, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**A conversation card still carries every station fact.**</summary>
    /// <remarks>
    /// **THE GUARD MUST NOT NARROW THE REAL CASE.** A receipt losing its station
    /// facts is the point; a conversation losing them would be this unit breaking the
    /// thing it was asked to protect.
    /// </remarks>
    [Fact]
    public void AConversationCardStillCarriesEveryStationFact()
    {
        var model = Panel();

        Sent(model, "02:11:00", "K9XP " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " K9XP EN52");
        Heard(model, "02:11:30", HisCall + " K9XP RR73");

        model.CardsNowForTests = Slot("02:11:45");
        model.RebuildCardsForTests();

        var card = Assert.Single(model.DigitalCards);

        _output.WriteLine("callsign     : " + card.Callsign);
        _output.WriteLine("place        : " + card.Place);
        _output.WriteLine("state word   : " + card.StateWord);
        _output.WriteLine("sentence     : " + card.Sentence);
        _output.WriteLine("shows globe  : " + card.ShowsGlobe);
        _output.WriteLine("detail       : " + card.Detail);

        Assert.False(card.IsCallToAnyone);
        Assert.Equal("K9XP", card.Callsign);

        // **THE PLACE, THE MAP AND THE FACTS ARE ALL STILL THERE.**
        Assert.True(card.HasPlace);
        Assert.True(card.ShowsGlobe);
        Assert.Contains("EN52", card.Detail, StringComparison.Ordinal);
        Assert.Contains("miles", card.Detail, StringComparison.Ordinal);
    }

    /// <summary>Every word a card puts on the screen, in one string.</summary>
    private static string Everything(Ft8ContactCard card)
        => string.Join(
            " | ",
            card.Callsign, card.Place, card.StateWord, card.Sentence,
            card.TimeLine, card.ActionLabel, card.ActionTip, card.Detail,
            card.LogLabel, card.ShowsGlobe ? card.Globe.Caption : "");

    private static MainWindowViewModel Panel()
    {
        var settings = new AppSettings { ReconnectOnStartup = false };

        settings.Operator.Callsign = HisCall;
        settings.Operator.GridSquare = "FN00DJ";

        var model = new MainWindowViewModel(settings, null)
        {
            OperatingMode = "Digital",
            DigitalNewestFirst = false,
        };

        model.UseWorkedBeforeForTests(new Dictionary<string, AdifContact>());

        model.ClockOffset = new Hamlet.RadioEngine.Audio.ClockOffset(
            0.033, DateTime.UtcNow);

        return model;
    }

    private static void Sent(MainWindowViewModel model, string at, string message)
        => model.RecordSentForTests(message, Slot(at));

    private static void Heard(MainWindowViewModel model, string at, string message)
        => model.AddDecodeRowForTests(
            at.Replace(":", "", StringComparison.Ordinal),
            "-09", "0.2", "1240", message, Slot(at), heardOnHz: 14_074_000);

    private static DateTime Slot(string at)
        => DateTime.ParseExact(
            "2026-09-10 " + at, "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
}

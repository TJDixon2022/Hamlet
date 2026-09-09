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
/// Work instruction 297, task 5: **a ledger in each of the four states, the
/// sentence the card produces quoted whole, and every clause traced to the fact it
/// came from.**
/// </summary>
/// <remarks>
/// <para>**THIS IS THE §0.0 CHECK ON TASK 2** and it is the one thing on a card that
/// could be confidently wrong. A card is more persuasive than a list of messages,
/// exactly because it has already done the reading for him, so a sentence with a
/// clause behind it that the ledger never said is a belief he forms from the screen
/// and acts on without checking.</para>
/// <para>**THE BREAKAGE IT WOULD HAVE CAUGHT IS NAMED AND MEASURED:** a Finished
/// card saying *he signed off* over an exchange that ended `RRR`.
/// `Ft8ContactStates.IsComplete` counts `RRR` as an acknowledgement and says in its
/// own remarks that `73`'s absence never withholds completeness, so the obvious
/// implementation - read the goodbye off the state - is wrong on an ordinary way for
/// an FT8 contact to end. <c>TheFinishedSentenceNeverInventsAGoodbye</c> is that
/// case.</para>
/// <para>**IT PRINTS EVERY CARD IT BUILDS**, so the trace in the report is this
/// test's own output rather than a second transcription of it.</para>
/// </remarks>
public sealed class Unit297CardSentenceTests
{
    /// <summary>The operator's own callsign, as Settings has it on his machine.</summary>
    private const string HisCall = "KC3QIS";

    /// <summary>The station in every fixture.</summary>
    private const string Station = "K9XP";

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the four cards are printed.</param>
    public Unit297CardSentenceTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Your turn: he came back and it is the operator's move.**</summary>
    /// <remarks>
    /// Clauses, and the fact behind each:
    /// *K9XP* is `Ft8CardFacts.Callsign`, the ledger's own key;
    /// *came back to you* is `HeCameBack`, which is `LastHeardToUs is not null` and
    /// so cannot be satisfied by a CQ;
    /// *it is your turn* is `State == YourMove`, which `Ft8ContactStates.Read`
    /// derives from his message being newer than anything sent.
    /// </remarks>
    [Fact]
    public void YourTurnSaysHeCameBackAndNothingMore()
    {
        var card = Print(HeAnswered());

        Assert.Equal("Your turn", card.StateWord);

        Assert.Equal(
            "K9XP came back to you, and it is your turn to answer him.",
            card.Sentence);

        Assert.True(card.Facts.HeCameBack, "the *came back* clause has no fact");
        Assert.Equal(Ft8ContactState.YourMove, card.Facts.State);
    }

    /// <summary>**A bare CQ from a stranger is not a contact and gets no card.**</summary>
    /// <remarks>
    /// <para>**MEASURED RATHER THAN ASSUMED, AND IT CORRECTED THIS TEST.** The first
    /// draft asserted a *K9XP is calling and nobody has answered him yet* card. There
    /// is none, and there should not be: a card is one station **he is in contact
    /// with**, and Tim's ruling behind the contact column already says a CQ is an
    /// invitation and that answering it is what would begin a contact. A CQ is not
    /// addressed to the operator, so it never enters the mine side and never becomes
    /// a card.</para>
    /// <para>**THE PANEL DOES NOT HIDE HIM.** He is on the decoded list to the left
    /// with every other station calling anyone, which is where an invitation belongs
    /// and where answering one starts from.</para>
    /// <para>**`Ft8ContactStates.Read` STILL HAS THAT BRANCH** - *he has spoken and
    /// nothing has gone back* - and `Ft8ContactCard` still words it separately, so
    /// the two cannot come apart if a later unit does put an unanswered caller on a
    /// card. Reported in section 4 as a wording no card reaches today.</para>
    /// </remarks>
    [Fact]
    public void ABareCallToAnyoneIsNotACard()
    {
        var model = HeIsCalling();

        Assert.Empty(model.DigitalCards);
    }

    /// <summary>**Waiting on him: the operator answered and nothing came back.**</summary>
    /// <remarks>
    /// *You answered* is `HeCameBack` and `YouCalledHim` together, and
    /// *he has not come back yet* is `State == WaitingOnHim`, which
    /// `Ft8ContactStates.Read` gives when what was sent is newer than what he sent.
    /// </remarks>
    [Fact]
    public void WaitingOnHimSaysTheOperatorAnsweredAndIsWaiting()
    {
        var card = Print(HeAnsweredAndSoDidWe());

        Assert.Equal("Waiting on him", card.StateWord);

        Assert.Equal("You answered K9XP and he has not come back yet.", card.Sentence);

        Assert.True(card.Facts.YouCalledHim);
        Assert.Equal(Ft8ContactState.WaitingOnHim, card.Facts.State);
    }

    /// <summary>**Finished: what was swapped, and no goodbye that was not sent.**</summary>
    /// <remarks>
    /// **THIS IS THE ONE THE INSTRUCTION WARNS ABOUT.** The exchange ends `RRR`,
    /// which is a roger and not a farewell, and `IsComplete` is satisfied by it. A
    /// card reading the goodbye off the state would say *he said goodbye* here.
    /// </remarks>
    [Fact]
    public void TheFinishedSentenceNeverInventsAGoodbye()
    {
        var card = Print(AFinishedContactEndingInRoger());

        Assert.Equal("Finished", card.StateWord);

        // **THE CARD WAS RIGHT AND THE FIRST DRAFT OF THIS LINE WAS NOT.** In this
        // exchange he sent a grid and the operator sent the only report, so reports
        // did not go both ways and the card does not say they did. The wording that
        // names both reports is asserted separately, below.
        Assert.Equal(
            "You and K9XP got through to each other and you both confirmed it.",
            card.Sentence);

        Assert.False(
            card.Facts.HeSignedOff,
            "RRR was read as a sign-off, and it is a roger");

        Assert.DoesNotContain("goodbye", card.Sentence, StringComparison.Ordinal);
    }

    /// <summary>**And it does say so where he did say goodbye.**</summary>
    /// <remarks>
    /// The same exchange ending `RR73`. *He said goodbye* is
    /// `Ft8MessageSplit.IsSignOff` over what he actually sent, which is a shape test
    /// on the payload field and never a reading of what he meant (§12.1).
    /// </remarks>
    [Fact]
    public void TheFinishedSentenceSaysGoodbyeWhereHeSaidOne()
    {
        var card = Print(AFinishedContactEndingInSignOff());

        Assert.Equal("Finished", card.StateWord);
        Assert.True(card.Facts.HeSignedOff);
        Assert.EndsWith("He said goodbye.", card.Sentence, StringComparison.Ordinal);
    }

    /// <summary>**Gone quiet says he stopped transmitting, and never why.**</summary>
    /// <remarks>
    /// *Nothing more has been heard* is `State == GoneQuiet`, which
    /// `Ft8ContactStates.Read` derives from `SlotsSinceHeard` against a threshold of
    /// four slots - and it is counted from **everything** he transmitted, not from
    /// what he sent the operator, so a station busy with somebody else is not called
    /// quiet. **No clause says why**, because nothing in the ledger knows.
    /// </remarks>
    [Fact]
    public void GoneQuietSaysNothingAboutWhy()
    {
        var card = Print(HeStopped());

        Assert.Equal("Gone quiet", card.StateWord);
        Assert.True(card.IsDim, "a quiet card is drawn back so live ones lead");

        Assert.Contains("Nothing more has been heard from K9XP", card.Sentence,
            StringComparison.Ordinal);

        foreach (var guess in new[] { "propagation", "band", "radio", "antenna", "gave up" })
        {
            Assert.DoesNotContain(guess, card.Sentence, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// **No face member carries a decibel, a hertz, a time offset or a courtesy
    /// token.**
    /// </summary>
    /// <remarks>
    /// **TIM'S RULING, 2026-09-08, ASSERTED RATHER THAN REMEMBERED.** The face is
    /// the callsign, the place, the state word, the sentence, the time line, the
    /// button and the messages link. Every one is swept over all six fixtures.
    /// **`dB` and `Hz` are matched case-sensitively** because *dB* is the token and
    /// ordinary words contain those letters.
    /// </remarks>
    [Fact]
    public void NothingOnAnyFaceIsNerdy()
    {
        foreach (var card in Every())
        {
            var face = string.Join(" | ", new[]
            {
                card.Callsign, card.Place, card.StateWord, card.Sentence,
                card.TimeLine, card.ActionLabel, card.MessagesLabel,
            });

            _output.WriteLine(face);

            foreach (var token in new[] { "dB", " Hz", "RR73", "RRR", "dt " })
            {
                Assert.False(
                    face.Contains(token, StringComparison.Ordinal),
                    $"the face of {card.Callsign} carries \"{token}\": {face}");
            }

            Assert.DoesNotContain("bearing", face, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("degrees", face, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>**Every one of those is on the hover instead, and none is lost.**</summary>
    /// <remarks>
    /// **NOTHING IS DELETED** (Tim's ruling, 2026-09-08). The face gave the numbers
    /// up and the `i` mark took them, so the test that the face is clean is only
    /// half of it: without this one, deleting the facts would pass.
    /// </remarks>
    [Fact]
    public void TheDetailKeepsWhatTheFaceGaveUp()
    {
        var card = Print(AFinishedContactEndingInSignOff());

        _output.WriteLine("");
        _output.WriteLine(card.Detail);

        Assert.Contains("dB", card.Detail, StringComparison.Ordinal);
        Assert.Contains("Hz", card.Detail, StringComparison.Ordinal);
        Assert.Contains("bearing", card.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("slot", card.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("RR73", card.Detail, StringComparison.Ordinal);
    }

    /// <summary>
    /// **The time line carries the wall clock and how long ago, and drops the
    /// second half where no clock has been measured.**
    /// </summary>
    /// <remarks>
    /// **§0.0.** Without a measured offset there is no corrected now, so a relative
    /// age would be counted against a reading nobody took. The UTC came off the
    /// decode and stays.
    /// </remarks>
    [Fact]
    public void TheTimeLineDropsTheRelativeHalfWithNoClock()
    {
        var card = Print(HeAnswered());

        Assert.Equal("02:11:15 UTC · 45 seconds ago", card.TimeLine);

        card.Retime(null);

        Assert.Equal("02:11:15 UTC", card.TimeLine);
    }

    /// <summary>**A station that never sent a grid gets no distance claimed.**</summary>
    [Fact]
    public void NoGridMeansNoDistance()
    {
        var card = Print(HeAnswered());

        Assert.DoesNotContain("miles", card.Place, StringComparison.Ordinal);

        Assert.Contains(
            "has not put a grid square on the air", card.Detail,
            StringComparison.Ordinal);
    }


    /// <summary>**The your-turn hover, printed whole for the report.**</summary>
    /// <remarks>
    /// **THE CARD THE REPORT QUOTES IS THE ONE HE IS DECIDING ABOUT.** A hover that
    /// teaches is worth reading on the card that has a button on it, and the
    /// finished card's hover is already printed by
    /// <c>TheDetailKeepsWhatTheFaceGaveUp</c>.
    /// </remarks>
    [Fact]
    public void TheYourTurnHoverTeaches()
    {
        var card = Print(HeAnswered());

        _output.WriteLine("");
        _output.WriteLine(card.Detail);

        Assert.Contains("-9 dB", card.Detail, StringComparison.Ordinal);
        Assert.Contains("-21", card.Detail, StringComparison.Ordinal);
        Assert.Contains("1240 Hz", card.Detail, StringComparison.Ordinal);
        Assert.Contains("14.074000 MHz", card.Detail, StringComparison.Ordinal);
    }

    /// <summary>Every fixture, for the sweeps.</summary>
    private IEnumerable<Ft8ContactCard> Every()
    {
        // **THE CQ FIXTURE IS NOT HERE BECAUSE IT MAKES NO CARD**, which
        // `ABareCallToAnyoneIsNotACard` asserts directly.
        yield return Card(HeAnswered());
        yield return Card(HeAnsweredAndSoDidWe());
        yield return Card(AFinishedContactEndingInRoger());
        yield return Card(AFinishedContactEndingInSignOff());
        yield return Card(HeStopped());
    }

    /// <summary>He called anyone and nobody has answered.</summary>
    private static MainWindowViewModel HeIsCalling()
    {
        var model = Panel();

        Heard(model, "02:11:15", "CQ " + Station + " EM12");

        return At(model, "02:11:30");
    }

    /// <summary>He answered the operator's call.</summary>
    private static MainWindowViewModel HeAnswered()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " -09");

        return At(model, "02:12:00");
    }

    /// <summary>He answered and the operator answered him back.</summary>
    private static MainWindowViewModel HeAnsweredAndSoDidWe()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " -09");
        Sent(model, "02:11:30", Station + " " + HisCall + " R-12");

        return At(model, "02:11:45");
    }

    /// <summary>A whole exchange ending in his roger. Complete, and no goodbye.</summary>
    private static MainWindowViewModel AFinishedContactEndingInRoger()
        => AFinished("RRR");

    /// <summary>The same exchange ending in his goodbye.</summary>
    private static MainWindowViewModel AFinishedContactEndingInSignOff()
        => AFinished("RR73");

    /// <summary>A whole exchange closed by whatever he sent last.</summary>
    private static MainWindowViewModel AFinished(string closing)
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " FN31");
        Sent(model, "02:11:30", Station + " " + HisCall + " R-12");
        Heard(model, "02:11:45", HisCall + " " + Station + " " + closing);

        return At(model, "02:12:00");
    }

    /// <summary>He answered once and then stopped transmitting altogether.</summary>
    private static MainWindowViewModel HeStopped()
    {
        var model = Panel();

        Sent(model, "02:11:00", Station + " " + HisCall + " FN00");
        Heard(model, "02:11:15", HisCall + " " + Station + " -09");

        // **FIVE SLOTS LATER**, which is past the four `GoneQuietAfterSlots` holds.
        return At(model, "02:12:30");
    }

    /// <summary>Read the panel at a moment, and hand it back.</summary>
    private static MainWindowViewModel At(MainWindowViewModel model, string at)
    {
        model.CardsNowForTests = Slot(at);
        model.RebuildCardsForTests();

        return model;
    }

    /// <summary>The only card, or a failure saying what is there.</summary>
    private static Ft8ContactCard Card(MainWindowViewModel model)
    {
        Assert.True(
            model.DigitalCards.Count == 1,
            "expected one card and the panel holds ["
            + string.Join(", ", model.DigitalCards.Select(
                c => c.Callsign + " " + c.StateWord)) + "]");

        return model.DigitalCards[0];
    }

    /// <summary>The only card, printed as the report quotes it.</summary>
    private Ft8ContactCard Print(MainWindowViewModel model)
    {
        var card = Card(model);

        _output.WriteLine(
            card.Callsign
            + (card.HasPlace ? "   " + card.Place : "")
            + "   [" + card.StateWord + "]");
        _output.WriteLine("  " + card.Sentence);
        _output.WriteLine("  " + card.TimeLine);
        _output.WriteLine(
            "  [" + card.ActionLabel + "]"
            + (card.ShowsRing ? "  (ring)" : "")
            + "   " + card.MessagesLabel);

        return card;
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

    /// <summary>One message this station transmitted, in its slot, both halves.</summary>
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

    /// <summary>The `HHmmss` cell for a moment, as the panel draws it.</summary>
    private static string Stamp(string at)
        => Slot(at).ToString("HHmmss", CultureInfo.InvariantCulture);
}

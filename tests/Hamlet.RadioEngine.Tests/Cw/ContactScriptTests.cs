using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What Hamlet offers to send and when, the phrasebook behind it, and the card
/// at the end (HM-DEC-059, HM-DEC-043).
/// </summary>
/// <remarks>
/// The terror is not the radio, it is not knowing what to say. So the app offers
/// the one or two things anybody would send next rather than the whole ritual at
/// once, and every one of them is sendable by the radio it is offered to.
/// </remarks>
public sealed class ContactScriptTests
{
    private const string You = "KC3QIS";

    /// <remarks>
    /// CONTEXTUAL, NOT A MENU OF EVERYTHING (HM-DEC-059). Each stage offers a
    /// small number of things, and none of them belongs to another stage. A wall
    /// of choices is the same problem as no guidance, wearing a different coat.
    /// </remarks>
    [Theory]
    [InlineData(ContactStage.Calling)]
    [InlineData(ContactStage.Answering)]
    [InlineData(ContactStage.Exchanging)]
    [InlineData(ContactStage.Confirming)]
    [InlineData(ContactStage.SigningOff)]
    public void EachStageOffersOnlyWhatBelongsToIt(ContactStage stage)
    {
        var options = ContactScript.Offer(stage, You, "W1ABC");

        Assert.NotEmpty(options);
        Assert.True(options.Count <= 3, $"{stage} offered {options.Count} things at once");
        Assert.All(options, o => Assert.Equal(stage, o.Stage));
    }

    /// <remarks>
    /// EVERY MESSAGE IS ONE THE RADIO WILL ACTUALLY TAKE. The UI never presents
    /// a message it cannot send, so each one is checked against the keyer's own
    /// character set (Full Manual p. 19-11) and against the split.
    /// </remarks>
    [Fact]
    public void EveryOfferedMessageIsSendableAndSplitsCleanly()
    {
        foreach (var stage in Enum.GetValues<ContactStage>())
        {
            foreach (var option in ContactScript.Offer(stage, You, "W1ABC", qth: "TRAFFORD PA"))
            {
                Assert.True(
                    CwMessage.IsSendable(option.Message),
                    $"{stage}/{option.Label} has a character the keyer cannot send: "
                    + option.Message);

                var pieces = CwMessage.Split(option.Message);

                Assert.NotEmpty(pieces);
                Assert.All(pieces, p => Assert.True(
                    p.Length <= CwMessage.MaximumLength,
                    $"{stage}/{option.Label} piece '{p}' is {p.Length} characters"));

                Assert.Equal(pieces.Count, option.Pieces);
            }
        }
    }

    /// <remarks>
    /// Proves the stage follows what actually happened rather than a wizard step
    /// counter, so an operator who does something out of order is followed
    /// rather than corrected. Nobody is grading this (HM-DEC-043).
    /// </remarks>
    [Theory]
    [InlineData(false, false, false, false, ContactStage.Calling)]
    [InlineData(true, false, false, false, ContactStage.Answering)]
    [InlineData(true, true, false, false, ContactStage.Exchanging)]
    [InlineData(true, true, true, false, ContactStage.Confirming)]
    [InlineData(true, true, true, true, ContactStage.SigningOff)]
    public void TheStageFollowsWhatHasActuallyHappened(
        bool calling, bool answered, bool youSent, bool theySent, ContactStage expected)
        => Assert.Equal(
            expected, ContactScript.StageOf(calling, answered, youSent, theySent));

    /// <remarks>
    /// THERE IS ALWAYS A WAY TO SAY YOU ARE NEW. "QRS PSE, I am new" is a real
    /// and welcome thing to send, and a beginner who knows that sentence exists
    /// is far more likely to call at all. It is offered at the stages where it
    /// would come up rather than buried in a reference.
    /// </remarks>
    [Fact]
    public void AskingForSlowerIsOfferedWhereItWouldComeUp()
    {
        foreach (var stage in new[]
                 {
                     ContactStage.Calling, ContactStage.Answering,
                     ContactStage.Exchanging, ContactStage.Confirming,
                 })
        {
            var options = ContactScript.Offer(stage, You, "W1ABC");

            Assert.Contains(
                options,
                o => o.Message.Contains("QRS", StringComparison.Ordinal));
        }
    }

    /// <remarks>
    /// Proves the operator's own callsign is in the messages, so what they read
    /// in the worked example and what the button sends are recognizably one
    /// thing (HM-DEC-043).
    /// </remarks>
    [Fact]
    public void TheMessagesAreInTheOperatorsOwnCallsign()
    {
        var options = ContactScript.Offer(ContactStage.Calling, "W3XYZ");

        Assert.All(options, o => Assert.Contains(
            "W3XYZ", o.Message, StringComparison.Ordinal));

        // And with no callsign set, it falls back rather than sending an empty
        // call into the air.
        Assert.All(
            ContactScript.Offer(ContactStage.Calling, ""),
            o => Assert.True(o.Message.Length > 4));
    }

    /// <remarks>
    /// Proves every offer explains itself. Somebody who has never seen "K" or
    /// "BK" needs to be told once, on the card, rather than sent to look it up
    /// (HM-DEC-041).
    /// </remarks>
    [Fact]
    public void EveryOfferSaysWhatItMeansAndWhyItIsLikeThat()
    {
        foreach (var stage in Enum.GetValues<ContactStage>())
        {
            Assert.All(ContactScript.Offer(stage, You, "W1ABC"), o =>
            {
                Assert.False(string.IsNullOrWhiteSpace(o.Label));
                Assert.False(string.IsNullOrWhiteSpace(o.Meaning));
                Assert.True(o.Note.Length > 20, $"{o.Label} explains almost nothing");
            });
        }
    }

    // ---- The phrasebook ------------------------------------------------

    /// <remarks>
    /// THE COLUMN FOR ADMITTING YOU ARE NEW is the reason the phrasebook exists.
    /// A beginner who does not know these assumes the band is a room full of
    /// experts who will be annoyed with them.
    /// </remarks>
    [Fact]
    public void ThePhrasebookHasAColumnForSayingYouAreNew()
    {
        var newcomer = CwPhrasebook.OfKind(PhraseKind.NewOperator);

        Assert.True(newcomer.Count >= 4, "there should be more than a token few");
        Assert.Contains(newcomer, p => p.Sent.Contains("QRS", StringComparison.Ordinal));
        Assert.Contains(newcomer, p => p.Sent.Contains("AGN", StringComparison.Ordinal));

        Assert.Contains("ordinary", CwPhrasebook.NewOperatorNote, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves every phrase carries its meaning and when to use it, and that the
    /// collapsed summary says how many there are and how many are for somebody
    /// new (§0.5).
    /// </remarks>
    [Fact]
    public void EveryPhraseIsExplainedAndTheSummarySaysHowMany()
    {
        Assert.All(CwPhrasebook.All, p =>
        {
            Assert.False(string.IsNullOrWhiteSpace(p.Sent));
            Assert.False(string.IsNullOrWhiteSpace(p.Meaning));
            Assert.False(string.IsNullOrWhiteSpace(p.When));
        });

        var summary = CwPhrasebook.Summary();

        Assert.Contains("phrases", summary, StringComparison.Ordinal);
        Assert.Contains("new", summary, StringComparison.Ordinal);

        // And every phrase is one the radio could actually send.
        Assert.All(CwPhrasebook.All, p => Assert.True(
            CwMessage.IsSendable(p.Sent), $"'{p.Sent}' is not sendable"));
    }

    // ---- The closing card ----------------------------------------------

    /// <remarks>
    /// FOR SOMEBODY'S FIRST CONTACT THIS IS THE THING THEY WILL LOOK AT
    /// AFTERWARD (HM-DEC-059). It reads like a friend telling them it went fine,
    /// not like a log entry, and it says what actually happened.
    /// </remarks>
    [Fact]
    public void TheClosingCardSaysWhoWhereAndWhatWasExchanged()
    {
        var card = ContactClosing.Build(new ContactSummary(
            "W1ABC", "Boston MA", "40 m", 7_032_000, "579", "599", 15));

        Assert.Contains("W1ABC", card.Headline, StringComparison.Ordinal);
        Assert.Contains("40 m", card.Headline, StringComparison.Ordinal);
        Assert.Contains("7.032", card.Detail, StringComparison.Ordinal);
        Assert.Contains("Boston MA", card.Detail, StringComparison.Ordinal);
        Assert.Contains("579", card.Detail, StringComparison.Ordinal);
        Assert.Contains("599", card.Detail, StringComparison.Ordinal);
        Assert.NotEmpty(card.Encouragement);
    }

    /// <remarks>
    /// NOTHING IS CLAIMED THAT WAS NOT OBSERVED (§0.0). A report nobody sent is
    /// not mentioned, a speed nobody measured is not stated, and a callsign
    /// Hamlet did not hear is left out rather than filled in.
    /// </remarks>
    [Fact]
    public void WhatWasNotObservedIsLeftOutRatherThanFilledIn()
    {
        var card = ContactClosing.Build(new ContactSummary(
            "", "", "40 m", 7_032_000, "", "", null));

        Assert.DoesNotContain("words a minute", card.Detail, StringComparison.Ordinal);
        Assert.DoesNotContain("they were in", card.Detail, StringComparison.Ordinal);
        Assert.DoesNotContain("you gave them", card.Detail, StringComparison.Ordinal);

        // It still says something rather than showing an empty card.
        Assert.Contains("40 m", card.Headline, StringComparison.Ordinal);
        Assert.NotEmpty(card.Encouragement);
    }

    /// <remarks>
    /// IT IS NOT A LOGBOOK AND DOES NOT PRETEND TO BE (FG-004). It says what
    /// happened and goes away, so there is nothing here about saving, confirming
    /// or filing anything.
    /// </remarks>
    [Fact]
    public void TheClosingCardIsNotALogEntry()
    {
        var card = ContactClosing.Build(new ContactSummary(
            "W1ABC", "Boston MA", "40 m", 7_032_000, "579", "599", 15));

        foreach (var line in new[] { card.Headline, card.Detail, card.Encouragement })
        {
            foreach (var ledger in new[] { "logged", "saved", "QSL card", "confirm" })
            {
                Assert.DoesNotContain(ledger, line, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 319 task 4: **§R1's strict side - a macro offered only when Hamlet is certain.**
/// </summary>
/// <remarks>
/// <para>**EVERY TRANSCRIPT WALKED CHARACTER BY CHARACTER** through task 2's walk, and the offer
/// asked after every character.</para>
/// <para>**THE CORPUS IS WRITTEN, NOT RECORDED** (FACT-004).</para>
/// </remarks>
public sealed class ThePsk31OfferTests
{
    private static readonly Psk31TurnReading CertainYourTurn = new(Psk31TurnState.YourTurn, true);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the offers are printed.</param>
    public ThePsk31OfferTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Assertion 1: a certain your turn offers the macro the exchange calls for next, by name.**</summary>
    [Fact]
    public void ACertainYourTurnOffersTheMacroTheExchangeCallsForNext()
    {
        var corpus = Psk31Corpus.Load();
        var offered = new List<string>();

        _output.WriteLine("table: " + Psk31Offer.Table);

        foreach (var transcript in corpus.Transcripts)
        {
            var number = 0;

            foreach (var step in ThePsk31TurnTests.Walk(corpus.Operator, ThePsk31TurnTests.Stream(transcript)))
            {
                if (step.Completed is null)
                {
                    continue;
                }

                number++;

                if (step.Turn != CertainYourTurn)
                {
                    continue;
                }

                var macro = Psk31Offer.For(step.Messages, step.Turn, corpus.Operator);

                _output.WriteLine(transcript.Name + " message " + number + " (" + step.Completed.Exchange.Kind + "): offers " + macro);

                offered.Add(transcript.Name + " " + number + " " + macro);
            }
        }

        Assert.Equal(
            new[]
            {
                "01-textbook 3 Report", "01-textbook 5 Confirm", "02-chatty 3 Report", "02-chatty 5 Confirm",
                "03-no-report 3 Confirm", "07-odd-ending 3 Report", "08-lowercase-and-slashes 3 Report",
            },
            offered);

        // **HIS ANSWER TO THE OPERATOR'S OWN CQ**, which the corpus does not carry.
        const string answered = "CQ CQ CQ de KC3QIS KC3QIS KC3QIS pse K\nKC3QIS de W1AW W1AW K\n";

        var last = ThePsk31TurnTests.Walk(corpus.Operator, answered)[^1];
        var reply = Psk31Offer.For(last.Messages, last.Turn, corpus.Operator);

        _output.WriteLine("his answer to your CQ (" + last.Messages[^1].Exchange.Kind + "): offers " + reply);

        Assert.Equal(CertainYourTurn, last.Turn);
        Assert.Equal(Psk31Macro.Report, reply);
    }

    /// <summary>
    /// **Assertion 2, rewritten under R12 by work instruction 371 task 1: nothing is offered
    /// unless it is his turn, and on a guessed turn only a Report is.**
    /// </summary>
    /// <remarks>
    /// It read *every state but a certain your turn offers nothing*, which is the door this unit
    /// opens: a guessed hand-back to the operator offers the Report, and the card says the doubt
    /// beside the button. A Confirm still needs certainty, and that is counted here.
    /// </remarks>
    [Fact]
    public void NothingIsOfferedUnlessItIsHisTurnAndAGuessOffersOnlyAReport()
    {
        var corpus = Psk31Corpus.Load();
        var onCertain = 0;
        var onAnythingElse = 0;
        var onGuessedYourTurn = 0;
        var confirmsOnAGuess = 0;

        foreach (var transcript in corpus.Transcripts)
        {
            var previous = Psk31Macro.None;

            foreach (var step in ThePsk31TurnTests.Walk(corpus.Operator, ThePsk31TurnTests.Stream(transcript)))
            {
                var macro = Psk31Offer.For(step.Messages, step.Turn, corpus.Operator);

                if (macro != Psk31Macro.None && step.Turn.State != Psk31TurnState.YourTurn)
                {
                    onAnythingElse++;
                }

                if (macro != Psk31Macro.None && step.Turn is { State: Psk31TurnState.YourTurn, IsCertain: false })
                {
                    onGuessedYourTurn++;
                    confirmsOnAGuess += macro == Psk31Macro.Confirm ? 1 : 0;
                }

                if (macro != Psk31Macro.None && previous == Psk31Macro.None)
                {
                    onCertain += step.Turn == CertainYourTurn ? 1 : 0;
                }

                previous = macro;
            }
        }

        _output.WriteLine("offers made on a certain your turn : " + onCertain);
        _output.WriteLine("offers made on anything else       : " + onAnythingElse);

        // **REWRITTEN UNDER R12 BY WORK INSTRUCTION 371 TASK 1.** It counted every offer made on
        // anything but a certain *your turn* and required none. The door it guarded is open now:
        // a guessed hand-back to the operator offers a Report, which goes out on his click with
        // the doubt beside it. **What it guards now is the rule, not the shut door** - an offer is
        // made only where it is his turn, and on a guessed turn only a Report is offered.
        _output.WriteLine("offers made on a guessed your turn : " + onGuessedYourTurn);
        _output.WriteLine("of those, a Confirm                : " + confirmsOnAGuess);

        Assert.Equal(0, onAnythingElse);
        Assert.True(onCertain > 0);
        Assert.True(onGuessedYourTurn > 0);
        Assert.Equal(0, confirmsOnAGuess);

        // **EACH OF THE OTHER FOUR, BY NAME, OVER A CONVERSATION THAT WOULD OTHERWISE OFFER.**
        var textbook = corpus.Transcripts.Single(t => t.Name == "01-textbook");
        var third = ThePsk31TurnTests.Walk(corpus.Operator, ThePsk31TurnTests.Stream(textbook))
            .First(s => s.Completed is not null && s.Messages.Count == 3);

        Assert.Equal(Psk31Macro.Report, Psk31Offer.For(third.Messages, CertainYourTurn, corpus.Operator));

        // **A GUESSED YOUR TURN OFFERS THE REPORT**, which is this unit's change, and the doubt is
        // said on the card rather than the offer being withheld.
        Assert.Equal(
            Psk31Macro.Report,
            Psk31Offer.For(third.Messages, new Psk31TurnReading(Psk31TurnState.YourTurn, false), corpus.Operator));

        foreach (var other in new[]
        {
            new Psk31TurnReading(Psk31TurnState.HisTurn, true),
            new Psk31TurnReading(Psk31TurnState.HisTurn, false),
            new Psk31TurnReading(Psk31TurnState.HeIsSending, true),
            Psk31TurnReading.Unknown,
        })
        {
            Assert.Equal(Psk31Macro.None, Psk31Offer.For(third.Messages, other, corpus.Operator));
        }

        Assert.Equal(Psk31Macro.None, Psk31Offer.For(Array.Empty<Psk31Message>(), CertainYourTurn, corpus.Operator));
    }

    /// <summary>
    /// **Assertion 3, rewritten under R12 by work instruction 371 task 1: `04-not-for-me` produces
    /// no offer at all, and `05-garbled` offers a Report and never a Confirm.**
    /// </summary>
    /// <remarks>
    /// **THE DOOR THIS GUARDED IS OPEN.** It required silence on the garbled transcript, whose
    /// third line is a report addressed to the operator that hands the turn back and is a guess
    /// because its digits are damaged - the very case of Tim's record of 2026-09-20. A report says
    /// how a station is coming through, which is true of the audio whoever he is; a Confirm claims
    /// a contact, so it keeps §R1's gate. **`04-not-for-me` is unchanged**: nothing in it is
    /// addressed to the operator, so nothing is offered however certain it is.
    /// </remarks>
    [Fact]
    public void TheNotForMeTranscriptOffersNothingAndTheGarbledOneOffersOnlyAReport()
    {
        var corpus = Psk31Corpus.Load();

        var notForMe = corpus.Transcripts.Single(t => t.Name == "04-not-for-me");
        var notForMeSteps = ThePsk31TurnTests.Walk(corpus.Operator, ThePsk31TurnTests.Stream(notForMe));

        _output.WriteLine("04-not-for-me: " + notForMeSteps.Count + " characters walked");

        Assert.All(
            notForMeSteps,
            s => Assert.Equal(Psk31Macro.None, Psk31Offer.For(s.Messages, s.Turn, corpus.Operator)));

        var garbled = corpus.Transcripts.Single(t => t.Name == "05-garbled");
        var steps = ThePsk31TurnTests.Walk(corpus.Operator, ThePsk31TurnTests.Stream(garbled));
        var offers = steps
            .Select(s => Psk31Offer.For(s.Messages, s.Turn, corpus.Operator))
            .Where(m => m != Psk31Macro.None)
            .ToList();

        _output.WriteLine("05-garbled   : " + steps.Count + " characters walked, "
            + offers.Count + " offers, kinds " + string.Join(", ", offers.Distinct()));

        Assert.NotEmpty(offers);
        Assert.All(offers, m => Assert.Equal(Psk31Macro.Report, m));
    }

    /// <summary>**Assertion 5: §6's number - messages to the operator beside offers made.**</summary>
    [Fact]
    public void MessagesToTheOperatorBesideOffersMade()
    {
        var corpus = Psk31Corpus.Load();
        var total = 0;
        var totalOffers = 0;

        _output.WriteLine("transcript".PadRight(26) + "to the operator  offers made");

        foreach (var transcript in corpus.Transcripts)
        {
            var completions = ThePsk31TurnTests.Walk(corpus.Operator, ThePsk31TurnTests.Stream(transcript))
                .Where(s => s.Completed is not null)
                .ToList();

            var toHim = completions.Count(s => s.Completed!.Exchange.IsForOperator
                && !string.Equals(s.Completed.Exchange.Speaker, corpus.Operator, StringComparison.OrdinalIgnoreCase));

            var offers = completions.Count(s => Psk31Offer.For(s.Messages, s.Turn, corpus.Operator) != Psk31Macro.None);

            _output.WriteLine(transcript.Name.PadRight(26) + toHim.ToString().PadRight(17) + offers);

            Assert.True(offers <= toHim);

            total += toHim;
            totalOffers += offers;
        }

        _output.WriteLine("all".PadRight(26) + total.ToString().PadRight(17) + totalOffers);
    }
}

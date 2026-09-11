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

    /// <summary>**Assertion 2: every other state offers nothing, and the two numbers walked.**</summary>
    [Fact]
    public void EveryOtherStateOffersNothing()
    {
        var corpus = Psk31Corpus.Load();
        var onCertain = 0;
        var onAnythingElse = 0;

        foreach (var transcript in corpus.Transcripts)
        {
            var previous = Psk31Macro.None;

            foreach (var step in ThePsk31TurnTests.Walk(corpus.Operator, ThePsk31TurnTests.Stream(transcript)))
            {
                var macro = Psk31Offer.For(step.Messages, step.Turn, corpus.Operator);

                if (macro != Psk31Macro.None && step.Turn != CertainYourTurn)
                {
                    onAnythingElse++;
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

        Assert.Equal(0, onAnythingElse);
        Assert.True(onCertain > 0);

        // **EACH OF THE OTHER FOUR, BY NAME, OVER A CONVERSATION THAT WOULD OTHERWISE OFFER.**
        var textbook = corpus.Transcripts.Single(t => t.Name == "01-textbook");
        var third = ThePsk31TurnTests.Walk(corpus.Operator, ThePsk31TurnTests.Stream(textbook))
            .First(s => s.Completed is not null && s.Messages.Count == 3);

        Assert.Equal(Psk31Macro.Report, Psk31Offer.For(third.Messages, CertainYourTurn, corpus.Operator));

        foreach (var other in new[]
        {
            new Psk31TurnReading(Psk31TurnState.YourTurn, false),
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

    /// <summary>**Assertion 3: `05-garbled` and `04-not-for-me` produce no offer.**</summary>
    [Fact]
    public void TheGarbledAndTheNotForMeTranscriptsProduceNoOffer()
    {
        var corpus = Psk31Corpus.Load();

        foreach (var name in new[] { "05-garbled", "04-not-for-me" })
        {
            var transcript = corpus.Transcripts.Single(t => t.Name == name);
            var steps = ThePsk31TurnTests.Walk(corpus.Operator, ThePsk31TurnTests.Stream(transcript));

            _output.WriteLine(name + ": " + steps.Count + " characters walked");

            Assert.All(steps, s => Assert.Equal(Psk31Macro.None, Psk31Offer.For(s.Messages, s.Turn, corpus.Operator)));
        }
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

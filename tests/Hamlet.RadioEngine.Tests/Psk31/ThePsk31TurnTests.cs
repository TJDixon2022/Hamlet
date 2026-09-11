using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 319 task 2: **whose turn it is, read from the parse.**
/// </summary>
/// <remarks>
/// <para>**EACH TRANSCRIPT JOINED INTO ONE STREAM, AS ONE FREQUENCY WOULD CARRY IT**, the way unit
/// 316's split tests join it, fed one character at a time, and the turn read after every
/// character from the messages so far and from whether anything has arrived since the last of
/// them.</para>
/// <para>**THE CORPUS IS WRITTEN, NOT RECORDED** (FACT-004).</para>
/// </remarks>
public sealed class ThePsk31TurnTests
{
    private static readonly Psk31TurnReading CertainYourTurn = new(Psk31TurnState.YourTurn, true);

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the readings are printed.</param>
    public ThePsk31TurnTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Assertion 1: a certain message from another station, to the operator, that hands over, is a certain your turn.**</summary>
    [Fact]
    public void ACertainMessageToTheOperatorThatHandsOverIsACertainYourTurn()
    {
        var corpus = Psk31Corpus.Load();
        var seen = 0;

        _output.WriteLine("rule: " + Psk31Turn.Rule);

        foreach (var transcript in corpus.Transcripts)
        {
            foreach (var step in Walk(corpus.Operator, Stream(transcript)).Where(s => s.Completed is not null))
            {
                var read = step.Completed!.Exchange;

                if (!read.IsCertain || !read.IsForOperator || !read.HandsOver || IsOperator(read.Speaker, corpus.Operator))
                {
                    continue;
                }

                _output.WriteLine(transcript.Name + ": " + read.Speaker + " > " + read.Addressee + " " + read.Kind + " -> " + Describe(step.Turn));

                Assert.Equal(CertainYourTurn, step.Turn);
                seen++;
            }
        }

        Assert.True(seen > 0, "no certain message to the operator was walked");
    }

    /// <summary>**Assertion 2: a certain message the operator spoke that hands over is a certain his turn.**</summary>
    [Fact]
    public void ACertainMessageTheOperatorSpokeThatHandsOverIsACertainHisTurn()
    {
        var corpus = Psk31Corpus.Load();
        var seen = 0;

        foreach (var transcript in corpus.Transcripts)
        {
            foreach (var step in Walk(corpus.Operator, Stream(transcript)).Where(s => s.Completed is not null))
            {
                var read = step.Completed!.Exchange;

                if (!read.IsCertain || !read.HandsOver || !IsOperator(read.Speaker, corpus.Operator))
                {
                    continue;
                }

                _output.WriteLine(transcript.Name + ": " + read.Speaker + " > " + read.Addressee + " " + read.Kind + " -> " + Describe(step.Turn));

                Assert.Equal(new Psk31TurnReading(Psk31TurnState.HisTurn, true), step.Turn);
                seen++;
            }
        }

        Assert.True(seen > 0, "no certain message from the operator was walked");
    }

    /// <summary>**Assertion 3: characters after a turnover, with no new turnover yet, are he is still sending.**</summary>
    [Fact]
    public void CharactersAfterATurnoverWithNoNewTurnoverYetAreHeIsStillSending()
    {
        var corpus = Psk31Corpus.Load();
        var sending = 0;

        foreach (var transcript in corpus.Transcripts)
        {
            foreach (var step in Walk(corpus.Operator, Stream(transcript)))
            {
                if (step.Messages.Count > 0 && step.Sending)
                {
                    Assert.Equal(new Psk31TurnReading(Psk31TurnState.HeIsSending, true), step.Turn);
                    sending++;
                }
            }
        }

        // **BY NAME: `01-textbook`, THE LAST CHARACTER OF THE ANSWER'S OWN `K`**, one message in
        // and the second not yet complete.
        var textbook = corpus.Transcripts.Single(t => t.Name == "01-textbook");
        var steps = Walk(corpus.Operator, Stream(textbook));
        var second = steps.First(s => s.Messages.Count == 2);
        var before = steps[second.At - 1];

        _output.WriteLine("character steps read as still sending: " + sending);
        _output.WriteLine("01-textbook at [" + before.Character + "] before message 2: " + Describe(before.Turn)
            + "; at the whitespace after it: " + Describe(second.Turn));

        Assert.True(sending > 0);
        Assert.Equal(Psk31TurnState.HeIsSending, before.Turn.State);
        Assert.NotEqual(Psk31TurnState.HeIsSending, second.Turn.State);
    }

    /// <summary>**Assertion 4: an uncertain message is never a certain your turn.**</summary>
    [Fact]
    public void AnUncertainMessageIsNeverACertainYourTurn()
    {
        var corpus = Psk31Corpus.Load();

        foreach (var transcript in corpus.Transcripts)
        {
            foreach (var step in Walk(corpus.Operator, Stream(transcript)))
            {
                if (step.Turn == CertainYourTurn)
                {
                    Assert.True(step.Messages[^1].Exchange.IsCertain, transcript.Name + " at " + step.At + " is a certain your turn over an uncertain message");
                }
            }
        }

        // **`05-garbled` LINE 3, AND LINES 4 AND 5 MERGED, BY NAME.**
        var garbled = corpus.Transcripts.Single(t => t.Name == "05-garbled");
        var uncertain = 0;

        foreach (var step in Walk(corpus.Operator, Stream(garbled)).Where(s => s.Completed is { Exchange.IsCertain: false }))
        {
            _output.WriteLine("05-garbled uncertain message -> " + Describe(step.Turn) + " | " + step.Completed!.Text);

            Assert.NotEqual(CertainYourTurn, step.Turn);
            uncertain++;
        }

        Assert.True(uncertain >= 2, "05-garbled gave " + uncertain + " uncertain messages");
    }

    /// <summary>**Assertion 5: `04-not-for-me` never yields your turn.**</summary>
    [Fact]
    public void TheTranscriptNotForTheOperatorNeverYieldsYourTurn()
    {
        var corpus = Psk31Corpus.Load();
        var transcript = corpus.Transcripts.Single(t => t.Name == "04-not-for-me");
        var states = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var step in Walk(corpus.Operator, Stream(transcript)))
        {
            states.Add(Describe(step.Turn));

            Assert.NotEqual(Psk31TurnState.YourTurn, step.Turn.State);
        }

        _output.WriteLine("04-not-for-me read as: " + string.Join(", ", states));
    }

    /// <summary>**Assertion 6: unknown before any message.**</summary>
    [Fact]
    public void UnknownBeforeAnyMessage()
    {
        var corpus = Psk31Corpus.Load();

        Assert.Equal(Psk31TurnReading.Unknown, Psk31Turn.Read(Array.Empty<Psk31Message>(), false, corpus.Operator));
        Assert.Equal(Psk31TurnReading.Unknown, Psk31Turn.Read(Array.Empty<Psk31Message>(), true, corpus.Operator));

        var steps = 0;

        foreach (var transcript in corpus.Transcripts)
        {
            foreach (var step in Walk(corpus.Operator, Stream(transcript)).Where(s => s.Messages.Count == 0))
            {
                Assert.Equal(Psk31TurnReading.Unknown, step.Turn);
                steps++;
            }
        }

        _output.WriteLine("character steps before any message, all unknown: " + steps);
    }

    /// <summary>**Assertion 7: §6's number, printed whatever it is.**</summary>
    /// <remarks>
    /// **A NUMBER WITH NO CEILING.** What is asserted is that the certain count is exactly the
    /// messages to the operator whose parse is certain - no more, which would be a guess promoted,
    /// and no fewer, which would be the reader losing a certainty the parser had.
    /// </remarks>
    [Fact]
    public void MessagesToTheOperatorAndOnHowManyYourTurnIsCertain()
    {
        var corpus = Psk31Corpus.Load();
        var total = 0;
        var totalCertain = 0;

        _output.WriteLine("transcript".PadRight(26) + "to the operator  certain your turn");

        foreach (var transcript in corpus.Transcripts)
        {
            var toHim = Walk(corpus.Operator, Stream(transcript))
                .Where(s => s.Completed is { Exchange.IsForOperator: true }
                    && !IsOperator(s.Completed.Exchange.Speaker, corpus.Operator))
                .ToList();

            var certain = toHim.Count(s => s.Turn == CertainYourTurn);

            _output.WriteLine(transcript.Name.PadRight(26) + toHim.Count.ToString().PadRight(17) + certain);

            Assert.Equal(toHim.Count(s => s.Completed!.Exchange.IsCertain), certain);

            total += toHim.Count;
            totalCertain += certain;
        }

        _output.WriteLine("all".PadRight(26) + total.ToString().PadRight(17) + totalCertain);

        Assert.True(totalCertain <= total);
    }

    /// <summary>**Assertion 8, the nice-to-pass: the state changes within one character of the final turnover word.**</summary>
    [Fact]
    public void TheStateChangesWithinOneCharacterOfTheFinalTurnoverWord()
    {
        var corpus = Psk31Corpus.Load();
        var worst = 0;
        var measured = 0;

        foreach (var transcript in corpus.Transcripts)
        {
            var stream = Stream(transcript);
            var steps = Walk(corpus.Operator, stream);
            var from = 0;

            foreach (var step in steps.Where(s => s.Completed is not null))
            {
                var text = step.Completed!.Text;
                var start = stream.IndexOf(text, from, StringComparison.Ordinal);

                Assert.True(start >= 0, transcript.Name + ": message not in the stream");

                var lastOfTheTurnover = start + text.Length - 1;

                from = start + text.Length;

                if (!step.Completed.Exchange.IsForOperator || IsOperator(step.Completed.Exchange.Speaker, corpus.Operator))
                {
                    continue;
                }

                var changed = steps.Skip(lastOfTheTurnover).First(s => s.Turn.State == Psk31TurnState.YourTurn).At;
                var after = changed - lastOfTheTurnover;

                _output.WriteLine(transcript.Name + ": your turn " + after + " character(s) after the last letter of [" + text.Split(' ')[^1] + "]");

                worst = Math.Max(worst, after);
                measured++;
            }
        }

        _output.WriteLine("measured " + measured + ", worst " + worst + " character(s)");

        Assert.True(measured > 0);
        Assert.True(worst <= 1, "the state changed " + worst + " characters after the turnover word");
    }

    /// <summary>One character fed, and what was read after it.</summary>
    internal sealed record Step(
        int At, char Character, Psk31Message? Completed, IReadOnlyList<Psk31Message> Messages, bool Sending, Psk31TurnReading Turn);

    /// <summary>Feed a stream one character at a time and read the turn after each.</summary>
    internal static List<Step> Walk(string operatorCallsign, string stream)
    {
        var splitter = new Psk31MessageSplitter(operatorCallsign);
        var messages = new List<Psk31Message>();
        var steps = new List<Step>();

        for (var at = 0; at < stream.Length; at++)
        {
            var message = splitter.Add(stream[at]);

            if (message is not null)
            {
                messages.Add(message);
            }

            // **THE CARRIER-PRESENT FACT, AS THE CALLER HAS IT**: characters the splitter holds
            // that belong to no complete message yet.
            var sending = splitter.Pending.Trim().Length > 0;

            steps.Add(new Step(at, stream[at], message, messages.ToList(), sending, Psk31Turn.Read(messages, sending, operatorCallsign)));
        }

        return steps;
    }

    /// <summary>One transcript as one frequency would carry it.</summary>
    internal static string Stream(Psk31CorpusTranscript transcript)
        => string.Concat(transcript.Lines.Select(l => l.Text + "\n"));

    private static bool IsOperator(string? speaker, string operatorCallsign)
        => string.Equals(speaker, operatorCallsign, StringComparison.OrdinalIgnoreCase);

    private static string Describe(Psk31TurnReading turn)
        => turn.State + (turn.IsCertain ? " certain" : " guess");
}

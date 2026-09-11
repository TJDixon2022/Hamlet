using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 316 task 3: **a message out of a growing row.**
/// </summary>
/// <remarks>
/// <para>**EACH TRANSCRIPT JOINED INTO ONE STREAM, AS ONE FREQUENCY WOULD CARRY IT**, one line
/// after another with a line break between and after, and fed one character at a time. What
/// comes out is parsed by task 2's parser and held against the corpus line by line.</para>
/// <para>**THE LINE BREAK IS NOT WHAT SPLITS.** It is whitespace like any other, and
/// `05-garbled` line 4, which has no turnover, is the case that shows it: it runs straight into
/// line 5.</para>
/// <para>**THE CORPUS IS WRITTEN, NOT RECORDED** (FACT-004).</para>
/// </remarks>
public sealed class ThePsk31MessageSplitTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the messages are printed.</param>
    public ThePsk31MessageSplitTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Assertion 1: seven transcripts come back in order, each message parsing as the corpus states.**</summary>
    [Fact]
    public void SevenTranscriptsComeBackInOrderEachParsingAsTheCorpusStates()
    {
        var corpus = Psk31Corpus.Load();
        var wrong = new List<string>();

        _output.WriteLine("rule: " + Psk31MessageSplitter.SplitRule);

        foreach (var transcript in corpus.Transcripts.Where(t => t.Name != "05-garbled"))
        {
            var messages = Feed(new Psk31MessageSplitter(corpus.Operator), Stream(transcript));

            _output.WriteLine(transcript.Name + ": " + messages.Count + " messages from " + transcript.Lines.Count + " lines");

            for (var i = 0; i < Math.Max(messages.Count, transcript.Lines.Count); i++)
            {
                if (i >= messages.Count || i >= transcript.Lines.Count)
                {
                    wrong.Add(transcript.Name + " message " + (i + 1) + (i >= messages.Count ? " missing" : " extra"));
                    continue;
                }

                var line = transcript.Lines[i];
                var read = messages[i].Exchange;

                var same = read.Speaker == line.Speaker
                    && read.Addressee == line.Addressee
                    && read.Kind == line.ExpectedKind
                    && read.HandsOver == line.Turnover
                    && line.Text.StartsWith(messages[i].Text, StringComparison.Ordinal);

                _output.WriteLine("  " + (same ? "ok    " : "WRONG ") + (i + 1) + ": " + Describe(read) + " | " + messages[i].Text);

                if (!same)
                {
                    wrong.Add(line.Where);
                }
            }
        }

        Assert.Empty(wrong);
    }

    /// <summary>**Assertion 2: on `05-garbled`, nothing comes back more certain than the least certain line it holds.**</summary>
    [Fact]
    public void OnTheGarbledTranscriptNothingIsMoreCertainThanTheLinesItHolds()
    {
        var corpus = Psk31Corpus.Load();
        var transcript = corpus.Transcripts.Single(t => t.Name == "05-garbled");
        var stream = Stream(transcript);

        var ranges = new List<(int Start, int End, Psk31CorpusLine Line)>();
        var position = 0;

        foreach (var line in transcript.Lines)
        {
            ranges.Add((position, position + line.Text.Length, line));
            position += line.Text.Length + 1;
        }

        var messages = Feed(new Psk31MessageSplitter(corpus.Operator), stream);
        var from = 0;

        Assert.NotEmpty(messages);

        for (var k = 0; k < messages.Count; k++)
        {
            var start = stream.IndexOf(messages[k].Text, from, StringComparison.Ordinal);

            Assert.True(start >= 0, "message " + (k + 1) + " is not in the stream");

            var end = start + messages[k].Text.Length;
            from = end;

            var holds = ranges.Where(r => r.Start < end && start < r.End).Select(r => r.Line).ToList();
            var allCertain = holds.All(l => l.Certain);

            _output.WriteLine("message " + (k + 1) + " holds line(s) " + string.Join("+", holds.Select(l => l.Number))
                + " (corpus " + (allCertain ? "certain" : "uncertain") + "): " + Describe(messages[k].Exchange)
                + " | " + messages[k].Text);

            if (!allCertain)
            {
                Assert.False(messages[k].Exchange.IsCertain, "message " + (k + 1) + " is certain over an uncertain line");
            }
        }
    }

    /// <summary>**Assertion 3: text with no turnover yet yields no message, and the text is still there.**</summary>
    /// <remarks>
    /// **AND THE THREE WORDS THE RULE HAS TO REFUSE**: `K` inside `OK`, `SK` in the middle of a
    /// sentence, and `BTU` with no sign before it.
    /// </remarks>
    [Fact]
    public void TextWithNoTurnoverYetYieldsNoMessage()
    {
        var splitter = new Psk31MessageSplitter("KC3QIS");

        const string half = "CQ CQ CQ de W1AW W1AW W1AW pse";

        Assert.Empty(Feed(splitter, half));
        Assert.Equal(half, splitter.Pending);

        // **THE K HAS ARRIVED AND IS NOT YET A WORD** - it could be the start of `KC3QIS`.
        Assert.Empty(Feed(splitter, " K"));
        Assert.Equal(half + " K", splitter.Pending);

        var done = Feed(splitter, " ");

        _output.WriteLine("after the space: " + string.Join(" / ", done.Select(m => Describe(m.Exchange) + " | " + m.Text)));

        var cq = Assert.Single(done);

        Assert.Equal(Psk31LineKind.Cq, cq.Exchange.Kind);
        Assert.Equal("", splitter.Pending);

        var prose = new Psk31MessageSplitter("KC3QIS");

        const string text = "W1AW de KC3QIS OK so I will SK the rig at nine and BTU later, 73 ";

        Assert.Empty(Feed(prose, text));
        Assert.Equal(text, prose.Pending);

        _output.WriteLine("prose pending: " + prose.Pending);
    }

    private static string Stream(Psk31CorpusTranscript transcript)
        => string.Concat(transcript.Lines.Select(l => l.Text + "\n"));

    private static List<Psk31Message> Feed(Psk31MessageSplitter splitter, string text)
    {
        var messages = new List<Psk31Message>();

        foreach (var character in text)
        {
            if (splitter.Add(character) is { } message)
            {
                messages.Add(message);
            }
        }

        return messages;
    }

    private static string Describe(Psk31Exchange read)
        => (read.Speaker ?? "unknown") + " > " + (read.Addressee ?? "unknown") + " " + read.Kind
            + (read.HandsOver ? " hands over" : " keeps")
            + (read.IsCertain ? " certain" : " UNCERTAIN");
}

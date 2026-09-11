using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Hamlet.RadioEngine.Psk31;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Psk31;

/// <summary>
/// Work instruction 316 task 2: **the parser, against the corpus.**
/// </summary>
/// <remarks>
/// <para>**ONE MESSAGE'S TEXT AND THE OPERATOR'S CALLSIGN IN; WHO, TO WHOM, WHAT KIND, WHOSE
/// TURN AND HOW SURE OUT.** Proved line by line against
/// `assets/fixtures/psk31-transcripts/corpus.json`, whose hash is printed rather than
/// checked because nothing records one.</para>
/// <para>**THE CORPUS IS WRITTEN, NOT RECORDED** (FACT-004). Every number here is about
/// typed text.</para>
/// </remarks>
public sealed class ThePsk31ExchangeParserTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the table is printed.</param>
    public ThePsk31ExchangeParserTests(ITestOutputHelper output) => _output = output;

    /// <summary>**Assertion 1: every line yields the speaker, addressee, kind and turnover the corpus states.**</summary>
    [Fact]
    public void EveryLineYieldsTheSpeakerAddresseeKindAndTurnoverTheCorpusStates()
    {
        var corpus = Psk31Corpus.Load();

        _output.WriteLine("corpus.json SHA-256 " + corpus.Sha256 + ", operator " + corpus.Operator);

        var wrong = new List<string>();

        foreach (var line in corpus.Lines)
        {
            var read = Psk31ExchangeParser.Read(line.Text, corpus.Operator);
            var same = read.Speaker == line.Speaker
                && read.Addressee == line.Addressee
                && read.Kind == line.ExpectedKind
                && read.HandsOver == line.Turnover;

            _output.WriteLine((same ? "ok   " : "WRONG ") + line.Where.PadRight(34) + Describe(read)
                + (same ? "" : "   corpus: " + line.Frm + " > " + line.To + " " + line.Kind + " turnover " + line.Turnover));

            if (!same)
            {
                wrong.Add(line.Where);
            }
        }

        _output.WriteLine("matched " + (corpus.Lines.Count - wrong.Count) + " of " + corpus.Lines.Count);

        Assert.Equal(32, corpus.Lines.Count);
        Assert.Empty(wrong);
    }

    /// <summary>**Assertion 2: no line the corpus marks uncertain comes back certain.**</summary>
    [Fact]
    public void NoLineTheCorpusMarksUncertainComesBackCertain()
    {
        var corpus = Psk31Corpus.Load();
        var wronglyCertain = new List<string>();
        var moreCautious = new List<string>();

        foreach (var line in corpus.Lines)
        {
            var read = Psk31ExchangeParser.Read(line.Text, corpus.Operator);

            if (!line.Certain && read.IsCertain)
            {
                wronglyCertain.Add(line.Where);
            }

            if (line.Certain && !read.IsCertain)
            {
                moreCautious.Add(line.Where);
            }
        }

        _output.WriteLine("asserted certain where the corpus says uncertain : " + wronglyCertain.Count
            + (wronglyCertain.Count > 0 ? " - " + string.Join(", ", wronglyCertain) : ""));
        _output.WriteLine("uncertain where the corpus says certain (reported): " + moreCautious.Count
            + (moreCautious.Count > 0 ? " - " + string.Join(", ", moreCautious) : ""));

        Assert.Empty(wronglyCertain);

        // **THE DAMAGED REPORT IS UNKNOWN, NOT THE CLEAN 599 BESIDE IT.**
        var garbled = corpus.Lines.Single(l => l.Transcript == "05-garbled" && l.Number == 3);
        var third = Psk31ExchangeParser.Read(garbled.Text, corpus.Operator);

        _output.WriteLine("05-garbled line 3: " + Describe(third));

        Assert.False(third.IsCertain);
        Assert.Null(third.Rst);
    }

    /// <summary>**Assertion 3: RST and grid where the corpus states them.**</summary>
    [Fact]
    public void RstAndGridWhereTheCorpusStatesThem()
    {
        var corpus = Psk31Corpus.Load();
        var stated = 0;

        foreach (var line in corpus.Lines.Where(l => l.Rst is not null || l.Grid is not null))
        {
            var read = Psk31ExchangeParser.Read(line.Text, corpus.Operator);

            _output.WriteLine(line.Where.PadRight(34) + "rst " + Show(read.Rst) + " (corpus " + Show(line.Rst)
                + "), grid " + Show(read.Grid) + " (corpus " + Show(line.Grid) + ")");

            if (line.Rst is not null)
            {
                Assert.Equal(line.Rst, read.Rst);
                stated++;
            }

            if (line.Grid is not null)
            {
                Assert.Equal(line.Grid, read.Grid);
                stated++;
            }
        }

        _output.WriteLine("fields the corpus states: " + stated);

        // **`5nn` IS 599, `/P` IS KEPT, AND LOWER CASE READS AS UPPER.**
        var portable = corpus.Lines.Single(l => l.Transcript == "08-lowercase-and-slashes" && l.Number == 3);
        var read08 = Psk31ExchangeParser.Read(portable.Text, corpus.Operator);

        Assert.Equal("599", read08.Rst);
        Assert.Equal("KD8WYT/P", read08.Speaker);
        Assert.Equal("KC3QIS", read08.Addressee);
        Assert.Equal("EN91", read08.Grid);
    }

    /// <summary>**Assertion 4: the unknown rate per transcript, beside what the corpus expects.**</summary>
    /// <remarks>
    /// **BOTH READINGS OF THE CORPUS'S OWN MISMATCH ARE PRINTED.** `unknown_rate_expected` is
    /// 0.2 on `05-garbled`; the fraction of its lines at `certain: false` is 0.4; the fraction
    /// with no speaker is 0.2. The must-pass is *not lower*, and it is asserted against the
    /// higher of the two so that it holds under either reading.
    /// </remarks>
    [Fact]
    public void TheUnknownRatePerTranscriptIsNotLowerThanTheCorpusExpects()
    {
        var corpus = Psk31Corpus.Load();

        _output.WriteLine("transcript                 lines  uncertain  no-speaker  expected  corpus-uncertain  corpus-no-speaker");

        foreach (var transcript in corpus.Transcripts)
        {
            var n = (double)transcript.Lines.Count;
            var reads = transcript.Lines.Select(l => Psk31ExchangeParser.Read(l.Text, corpus.Operator)).ToList();

            var uncertain = reads.Count(r => !r.IsCertain) / n;
            var noSpeaker = reads.Count(r => r.Speaker is null) / n;
            var corpusUncertain = transcript.Lines.Count(l => !l.Certain) / n;
            var corpusNoSpeaker = transcript.Lines.Count(l => l.Speaker is null) / n;

            _output.WriteLine(transcript.Name.PadRight(27)
                + transcript.Lines.Count.ToString(CultureInfo.InvariantCulture).PadRight(7)
                + Rate(uncertain).PadRight(11) + Rate(noSpeaker).PadRight(12)
                + Rate(transcript.UnknownRateExpected).PadRight(10)
                + Rate(corpusUncertain).PadRight(18) + Rate(corpusNoSpeaker));

            var floor = Math.Max(transcript.UnknownRateExpected, corpusUncertain);

            Assert.True(
                uncertain >= floor - 1e-9,
                transcript.Name + " marks " + Rate(uncertain) + " uncertain, under the corpus's " + Rate(floor));
        }
    }

    /// <summary>**Assertion 5: addressed-to-the-operator comes from the corpus's `operator` field.**</summary>
    [Fact]
    public void AddressedToTheOperatorComesFromTheCorpusOperatorField()
    {
        var corpus = Psk31Corpus.Load();

        foreach (var line in corpus.Lines)
        {
            var read = Psk31ExchangeParser.Read(line.Text, corpus.Operator);
            var expected = string.Equals(line.Addressee, corpus.Operator, StringComparison.OrdinalIgnoreCase);

            Assert.True(expected == read.IsForOperator, line.Where + " for the operator: " + read.IsForOperator);
        }

        var forHim = corpus.Lines.Count(l => Psk31ExchangeParser.Read(l.Text, corpus.Operator).IsForOperator);

        _output.WriteLine("lines for " + corpus.Operator + ": " + forHim);

        // **NOT FOR HIM, WHOEVER IS TALKING.**
        foreach (var line in corpus.Lines.Where(l => l.Transcript == "04-not-for-me"))
        {
            Assert.False(Psk31ExchangeParser.Read(line.Text, corpus.Operator).IsForOperator, line.Where);
        }

        // **AND A DIFFERENT OPERATOR GETS HIS OWN LINES**, which a constant could not do.
        var w1aw = corpus.Lines.Where(l => l.Transcript == "01-textbook")
            .Where(l => Psk31ExchangeParser.Read(l.Text, "W1AW").IsForOperator)
            .Select(l => l.Number)
            .ToList();

        _output.WriteLine("01-textbook lines for W1AW: " + string.Join(", ", w1aw));

        Assert.Equal(new[] { 2, 4 }, w1aw);

        var source = File.ReadAllText(Path.Combine(
            Psk31Corpus.Root(), "src", "Hamlet.RadioEngine", "Psk31", "Psk31ExchangeParser.cs"));

        Assert.DoesNotContain(corpus.Operator, source, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>**Assertion 6: the parse has no member for name or QTH.**</summary>
    [Fact]
    public void TheParseHasNoMemberForNameOrQth()
    {
        var members = typeof(Psk31Exchange).GetProperties().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal).ToList();

        _output.WriteLine("members: " + string.Join(", ", members));

        Assert.DoesNotContain(members, m => m.Contains("Name", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(members, m => m.Contains("Qth", StringComparison.OrdinalIgnoreCase));

        // **THE WHOLE SHAPE**, so a member added later is seen here rather than slipping past.
        Assert.Equal(
            new[] { "Addressee", "Grid", "HandsOver", "IsCertain", "IsForOperator", "Kind", "Rst", "Speaker" },
            members);
    }

    /// <summary>**Assertion 7, the nice-to-pass: `03-no-report` ends on 73 with no report invented.**</summary>
    [Fact]
    public void AnExchangeWithNoReportStillEndsOn73()
    {
        var corpus = Psk31Corpus.Load();

        foreach (var number in new[] { 3, 4 })
        {
            var line = corpus.Lines.Single(l => l.Transcript == "03-no-report" && l.Number == number);
            var read = Psk31ExchangeParser.Read(line.Text, corpus.Operator);

            _output.WriteLine(line.Where + ": " + Describe(read));

            Assert.Equal(Psk31LineKind.End, read.Kind);
            Assert.Null(read.Rst);
        }
    }

    /// <summary>**The callsign rule is the one the tree already uses, character for character.**</summary>
    /// <remarks>
    /// §R3: *callsigns, the same rule the decoded list uses today.* The tree holds that
    /// pattern privately in three places, and this is a fourth; the test is what stops the
    /// four drifting.
    /// </remarks>
    [Fact]
    public void TheCallsignRuleIsTheOneTheTreeAlreadyUses()
    {
        foreach (var file in new[]
        {
            Path.Combine("Cw", "CallsignResolver.cs"),
            Path.Combine("Cw", "AutoCallAnswers.cs"),
            Path.Combine("Scan", "ScanStop.cs"),
        })
        {
            var source = File.ReadAllText(Path.Combine(Psk31Corpus.Root(), "src", "Hamlet.RadioEngine", file));

            Assert.Contains("@\"" + Psk31ExchangeParser.CallsignPattern + "\"", source, StringComparison.Ordinal);
        }
    }

    private static string Describe(Psk31Exchange read)
        => Show(read.Speaker) + " > " + Show(read.Addressee) + " " + read.Kind
            + (read.HandsOver ? " hands over" : " keeps")
            + (read.IsCertain ? " certain" : " UNCERTAIN")
            + (read.Rst is null ? "" : " rst " + read.Rst)
            + (read.Grid is null ? "" : " grid " + read.Grid)
            + (read.IsForOperator ? " for-operator" : "");

    private static string Show(string? value) => value ?? "unknown";

    private static string Rate(double rate) => rate.ToString("0.00", CultureInfo.InvariantCulture);
}

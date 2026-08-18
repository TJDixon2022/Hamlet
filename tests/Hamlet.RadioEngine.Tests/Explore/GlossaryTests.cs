using System.Text;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The hover glossary: automatic marking over a data file (HM-DEC-041).
/// </summary>
public sealed class GlossaryTests
{
    private static readonly IReadOnlyDictionary<string, GlossaryTerm> Sample =
        Glossary.BuildIndex(new[]
        {
            new GlossaryTerm("CQ", "seek you", "A general call to anybody listening."),
            new GlossaryTerm("QSO", "", "One conversation between two stations."),
            new GlossaryTerm("band", "", "A slice of the spectrum set aside for amateurs."),
            new GlossaryTerm("grid square", "", "A short code for where you are."),
            new GlossaryTerm("grid", "", "Should lose to the longer term."),
            new GlossaryTerm("break-in", "", "Hearing between your own dots and dashes."),
        });

    private static string Marked(string text)
        => string.Concat(
            Glossary.Mark(text, Sample).Where(s => s.IsTerm).Select(s => s.Text));

    private static string Rebuilt(string text)
        => string.Concat(Glossary.Mark(text, Sample).Select(s => s.Text));

    /// <remarks>
    /// THE INVARIANT EVERYTHING ELSE RESTS ON. Proves the runs reassemble into
    /// exactly the input, so a renderer using this cannot drop or duplicate a
    /// character. Swept over every case in this file rather than asserted
    /// once, because the failure would be invisible on screen until somebody
    /// noticed a missing word.
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData("nothing to mark here")]
    [InlineData("Calling CQ on the band.")]
    [InlineData("CQ CQ CQ")]
    [InlineData("A grid square and a grid.")]
    [InlineData("break-in, band, QSO")]
    [InlineData("   leading and trailing   ")]
    [InlineData("K3CQ at 7.030 on 40 m")]
    [InlineData("line one\nline two with CQ")]
    public void RunsAlwaysReassembleIntoTheInput(string text)
        => Assert.Equal(text, Rebuilt(text));

    /// <remarks>
    /// Proves whole words only. "band" must not fire inside "bandwidth" or
    /// "headband", which is the difference between a helpful mark and the app
    /// looking like it cannot read.
    /// </remarks>
    [Theory]
    [InlineData("The bandwidth is narrow.", "")]
    [InlineData("A headband is not radio.", "")]
    [InlineData("banded together", "")]
    [InlineData("The band is busy.", "band")]
    [InlineData("on this band.", "band")]
    [InlineData("(band)", "band")]
    public void MatchingIsWholeWordsOnly(string text, string expected)
        => Assert.Equal(expected, Marked(text));

    /// <remarks>
    /// Proves matching ignores case, so copy written in any register picks up
    /// the same terms.
    /// </remarks>
    [Theory]
    [InlineData("Calling cq now")]
    [InlineData("Calling CQ now")]
    [InlineData("Calling Cq now")]
    public void MatchingIsCaseInsensitive(string text)
        => Assert.Single(Glossary.Mark(text, Sample), s => s.IsTerm);

    /// <remarks>
    /// Proves the mark keeps the copy's own casing rather than the glossary's.
    /// Rewriting the operator's sentence to say "CQ" where it said "cq" would
    /// be the app editing prose it was only supposed to annotate.
    /// </remarks>
    [Fact]
    public void TheCopysOwnCasingSurvives()
    {
        var span = Glossary.Mark("calling cq now", Sample).Single(s => s.IsTerm);

        Assert.Equal("cq", span.Text);
        Assert.Equal("CQ", span.Term!.Term);
    }

    /// <remarks>
    /// FIRST OCCURRENCE ONLY. Proves a paragraph does not turn into a sea of
    /// dots: the second and third CQ are left alone, because they teach
    /// nobody anything the first did not.
    /// </remarks>
    [Fact]
    public void OnlyTheFirstOccurrenceInABlockIsMarked()
    {
        var spans = Glossary.Mark("CQ, then CQ again, and CQ once more.", Sample);
        var marked = spans.Where(s => s.IsTerm).ToList();

        Assert.Single(marked);
        Assert.Equal("CQ", marked[0].Text);
    }

    /// <remarks>
    /// Proves "first occurrence" is per block rather than global, so a second
    /// panel of copy still explains the term to somebody who never hovered the
    /// first one.
    /// </remarks>
    [Fact]
    public void EachBlockGetsItsOwnFirstOccurrence()
    {
        Assert.Single(Glossary.Mark("A CQ here.", Sample), s => s.IsTerm);
        Assert.Single(Glossary.Mark("A CQ there.", Sample), s => s.IsTerm);
    }

    /// <remarks>
    /// Proves each term gets its own first occurrence, so a passage using
    /// three different terms marks all three.
    /// </remarks>
    [Fact]
    public void DifferentTermsAreEachMarkedOnce()
    {
        var marked = Glossary.Mark("A QSO on the band, calling CQ.", Sample)
            .Where(s => s.IsTerm)
            .Select(s => s.Term!.Term)
            .ToList();

        Assert.Equal(new[] { "QSO", "band", "CQ" }, marked);
    }

    /// <remarks>
    /// NEVER INSIDE A CALLSIGN. Proves "K3CQ" is left alone even though it
    /// ends in CQ. Underlining part of a callsign would look like the app had
    /// misread something the operator can plainly see.
    /// </remarks>
    [Theory]
    [InlineData("K3CQ is calling")]
    [InlineData("Worked N0QSO today")]
    [InlineData("JO4MJO/4 is activating")]
    [InlineData("W1ABC/P on a summit")]
    [InlineData("VE3QSO and KC3QIS")]
    public void NeverMatchesInsideACallsign(string text)
        => Assert.DoesNotContain(Glossary.Mark(text, Sample), s => s.IsTerm);

    /// <remarks>
    /// NEVER INSIDE A FREQUENCY. Proves numbers are left entirely alone, so
    /// "7.030" and "14.074" carry no marks however the digits fall.
    /// </remarks>
    [Theory]
    [InlineData("Tune to 7.030 now")]
    [InlineData("14.074 MHz")]
    [InlineData("28.030.000")]
    [InlineData("7.030CQ")]
    [InlineData("CQ7.030")]
    public void NeverMatchesInsideAFrequency(string text)
        => Assert.DoesNotContain(Glossary.Mark(text, Sample), s => s.IsTerm);

    /// <remarks>
    /// The other half of the frequency guard. Proves a term that ends a
    /// sentence is still marked, because the period closing it is not a
    /// decimal point. Getting this backwards would silently stop marking any
    /// term that happened to fall at the end of a sentence, which in this
    /// codebase is most of them.
    /// </remarks>
    [Theory]
    [InlineData("Somebody is calling CQ.")]
    [InlineData("It is busy on the band.")]
    [InlineData("Worth one QSO.")]
    public void ATermEndingASentenceIsStillMarked(string text)
        => Assert.Single(Glossary.Mark(text, Sample), s => s.IsTerm);

    /// <remarks>
    /// Proves a real callsign next to a real term still marks the term. The
    /// callsign guard must protect callsigns without swallowing the sentence
    /// around them.
    /// </remarks>
    [Fact]
    public void ACallsignBesideATermStillMarksTheTerm()
    {
        var marked = Glossary.Mark("K3ABC is calling CQ on the band.", Sample)
            .Where(s => s.IsTerm)
            .Select(s => s.Term!.Term)
            .ToList();

        Assert.Equal(new[] { "CQ", "band" }, marked);
    }

    /// <remarks>
    /// Proves the longest term wins, so "grid square" is marked as one thing
    /// rather than leaving "square" dangling after a mark on "grid".
    /// </remarks>
    [Fact]
    public void TheLongestTermWins()
    {
        var span = Glossary.Mark("Your grid square is FN00.", Sample).Single(s => s.IsTerm);

        Assert.Equal("grid square", span.Text);
    }

    /// <remarks>
    /// Proves a hyphenated term matches as one word, since "break-in" would
    /// otherwise be found as "break" and leave the rest bare.
    /// </remarks>
    [Fact]
    public void HyphenatedTermsMatchWhole()
    {
        var span = Glossary.Mark("Full break-in makes it a conversation.", Sample)
            .Single(s => s.IsTerm);

        Assert.Equal("break-in", span.Text);
    }

    /// <remarks>
    /// Proves an empty term set leaves the text whole rather than throwing or
    /// returning nothing, which is what a failed glossary load looks like.
    /// </remarks>
    [Fact]
    public void NoTermsLeavesTheTextAlone()
    {
        var empty = Glossary.BuildIndex(Array.Empty<GlossaryTerm>());
        var spans = Glossary.Mark("Calling CQ on the band.", empty);

        Assert.Single(spans);
        Assert.False(spans[0].IsTerm);
        Assert.Equal("Calling CQ on the band.", spans[0].Text);
    }

    /// <remarks>
    /// Proves null and empty copy do not throw. This runs on every render.
    /// </remarks>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NullAndEmptyAreHarmless(string? text)
        => Assert.Empty(Glossary.Mark(text, Sample));

    /// <remarks>
    /// A MALFORMED FILE YIELDS UNMARKED TEXT AND NO CRASH. Proves every way a
    /// hand-edited glossary can be broken ends in an empty term list rather
    /// than an exception, because this loads while the window is being built.
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("[1,2,3]")]
    [InlineData("{\"terms\": \"not an array\"}")]
    [InlineData("{\"terms\": [null, null]}")]
    [InlineData("{\"nothing\": true}")]
    public void AMalformedFileYieldsNoTerms(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var terms = Glossary.Parse(stream);

        Assert.Empty(terms);

        var spans = Glossary.Mark("Calling CQ.", Glossary.BuildIndex(terms));
        Assert.Single(spans);
        Assert.False(spans[0].IsTerm);
    }

    /// <remarks>
    /// Proves a half-written entry is dropped rather than marking a word and
    /// then saying nothing about it, which would be worse than not marking it.
    /// </remarks>
    [Fact]
    public void EntriesWithoutAnExplanationAreDropped()
    {
        const string json = """
            {
              "terms": [
                { "term": "CQ", "explanation": "A general call." },
                { "term": "QSO" },
                { "term": "  ", "explanation": "no term" },
                { "term": "DX", "explanation": "   " }
              ]
            }
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var terms = Glossary.Parse(stream);

        Assert.Single(terms);
        Assert.Equal("CQ", terms[0].Term);
    }

    /// <remarks>
    /// Proves the shipped file loads and that every term in it explains
    /// itself. "If Hamlet says it, Hamlet explains it" is only true if every
    /// entry actually carries an explanation.
    /// </remarks>
    [Fact]
    public void EveryShippedTermHasANonEmptyExplanation()
    {
        Assert.NotEmpty(Glossary.All);

        Assert.All(Glossary.All, t =>
        {
            Assert.False(string.IsNullOrWhiteSpace(t.Term));
            Assert.False(string.IsNullOrWhiteSpace(t.Explanation));
        });
    }

    /// <remarks>
    /// Proves the terms the brief named are all present, so the coverage
    /// promise is checked rather than assumed.
    /// </remarks>
    [Theory]
    [InlineData("CQ")]
    [InlineData("QSO")]
    [InlineData("CW")]
    [InlineData("SSB")]
    [InlineData("WPM")]
    [InlineData("QRP")]
    [InlineData("QRO")]
    [InlineData("QSB")]
    [InlineData("QRM")]
    [InlineData("QRN")]
    [InlineData("DX")]
    [InlineData("DXpedition")]
    [InlineData("split")]
    [InlineData("rag chew")]
    [InlineData("elmer")]
    [InlineData("QSL")]
    [InlineData("LoTW")]
    [InlineData("73")]
    [InlineData("88")]
    [InlineData("RST")]
    [InlineData("S-meter")]
    [InlineData("activator")]
    [InlineData("chaser")]
    [InlineData("POTA")]
    [InlineData("SOTA")]
    [InlineData("RBN")]
    [InlineData("skimmer")]
    [InlineData("watering hole")]
    [InlineData("band")]
    [InlineData("mode")]
    [InlineData("net")]
    [InlineData("pile-up")]
    [InlineData("worked")]
    [InlineData("break-in")]
    [InlineData("straight key")]
    [InlineData("paddle")]
    [InlineData("keyer")]
    [InlineData("dummy load")]
    [InlineData("SWR")]
    [InlineData("ERP")]
    [InlineData("grayline")]
    [InlineData("ionosphere")]
    [InlineData("propagation")]
    [InlineData("grid square")]
    [InlineData("Maidenhead")]
    [InlineData("ADIF")]
    [InlineData("simplex")]
    [InlineData("repeater")]
    [InlineData("QTH")]
    [InlineData("QRZ")]
    public void TheNamedTermsAreAllCovered(string term)
        => Assert.NotNull(Glossary.Find(term));

    /// <remarks>
    /// Proves the explanations are written for a person rather than copied
    /// from a dictionary. They do emotional work as well as semantic, which
    /// is the difference between handing somebody a glossary and being on
    /// their side, so a one-line definition is a bug (HM-DEC-041).
    /// </remarks>
    [Fact]
    public void TheExplanationsAreWrittenForAPerson()
    {
        Assert.All(Glossary.All, t =>
            Assert.True(
                t.Explanation.Length >= 60,
                $"'{t.Term}' is only {t.Explanation.Length} characters: {t.Explanation}"));

        // At least two sentences apiece: the fact, and then why it is like
        // that or what it means for you.
        Assert.All(Glossary.All, t =>
            Assert.True(
                t.Explanation.Count(c => c == '.') >= 2,
                $"'{t.Term}' is a single sentence: {t.Explanation}"));
    }

    /// <remarks>
    /// Proves the glossary obeys the voice standard it was written under: at
    /// most one em dash in a passage (HM-DEC-040).
    /// </remarks>
    [Fact]
    public void TheExplanationsObeyTheDashRule()
        => Assert.All(Glossary.All, t =>
            Assert.True(
                t.Explanation.Count(c => c == '—') <= 1,
                $"'{t.Term}' carries too many dashes: {t.Explanation}"));

    /// <remarks>
    /// Proves an expansion is shown for initials and not invented for words
    /// that are not initials, so the heading never reads "band (band)".
    /// </remarks>
    [Fact]
    public void ExpansionsAppearOnlyWhereTheyMakeSense()
    {
        Assert.Equal("CQ (seek you)", Glossary.Find("CQ")!.Heading);
        Assert.Equal("band", Glossary.Find("band")!.Heading);

        Assert.All(Glossary.All.Where(t => t.HasExpansion), t =>
            Assert.NotEqual(
                t.Term.ToUpperInvariant(), t.Expansion.ToUpperInvariant()));
    }

    /// <remarks>
    /// Proves marking is pure (§5): the same copy always produces the same
    /// runs, with no state carried between calls.
    /// </remarks>
    [Fact]
    public void MarkingIsDeterministic()
    {
        const string text = "A QSO on the band, calling CQ from a grid square.";

        var first = Glossary.Mark(text, Sample).Select(s => s.Text).ToList();
        var second = Glossary.Mark(text, Sample).Select(s => s.Text).ToList();

        Assert.Equal(first, second);
    }

    /// <remarks>
    /// Proves the marker runs over the app's real copy without throwing or
    /// mangling it. The band character passages are the longest prose Hamlet
    /// has, so they are the ones most likely to find an edge.
    /// </remarks>
    [Fact]
    public void TheAppsOwnCopySurvivesMarking()
    {
        foreach (var band in RadioEngine.Bands.HfBands.Bands)
        {
            var passage = BandCharacter.Describe(band.Name, RadioEngine.Solar.SolarSnapshot.Unknown, 8, 40);

            Assert.Equal(passage, string.Concat(Glossary.Mark(passage).Select(s => s.Text)));
        }

        foreach (var mode in ModeGuide.Modes)
        {
            Assert.Equal(mode.Why, string.Concat(Glossary.Mark(mode.Why).Select(s => s.Text)));
        }
    }
}

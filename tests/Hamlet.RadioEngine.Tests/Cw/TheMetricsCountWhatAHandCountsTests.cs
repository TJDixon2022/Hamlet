using Hamlet.RadioEngine.Cw;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The four requirement metrics against pairs whose answer is known by
/// construction, each count written into the test by hand (work instruction 439,
/// tasks 3 and 4; PHASE_PLAN.md 1.1 to 1.4).
/// </summary>
/// <remarks>
/// <para>**EVERY PAIR HERE IS BUILT, NOT DECODED.** A decode is a list of symbols
/// with the class the decoder would have claimed for each, so what is invented,
/// wrong, sure or misplaced can be counted on the page before the metric counts it.
/// Each was watched failing against a metric that counted nothing before the
/// metric was written.</para>
/// <para>Method T, truth grade synthetic, key exact (`CW_REQUIREMENTS.md` section V).</para>
/// </remarks>
public sealed class TheMetricsCountWhatAHandCountsTests
{
    private static CwSymbol S(string text) => CwSymbol.Sure(text);

    private static CwSymbol Gap => CwSymbol.Gap;

    private static CwSymbol Block => CwSymbol.Placeholder;

    private static CwMetricAlignment Align(string key, params CwSymbol[] decoded)
        => CwMetrics.Align(decoded, key, CwKeyKind.Exact);

    /// <remarks>
    /// Proves HM-REQ-011's metric counts a sure insertion: `CQ DE K` sent, five
    /// characters; `CQ DEE K` decoded all sure. One `E` was never sent, so one
    /// invented of five.
    /// </remarks>
    [Fact]
    public void ASureLetterNobodySentIsInvented()
    {
        var invented = CwMetrics.Invented(Align("CQ DE K", S("C"), S("Q"), Gap, S("D"), S("E"), S("E"), Gap, S("K")));

        Assert.Equal(1, invented.SureAdded);
        Assert.Equal(0, invented.SureWrong);
        Assert.Equal(5, invented.Sent);
        Assert.Equal(1.0 / 5, invented.Share);
        Assert.Equal(CwKeyKind.Exact, invented.Kind);
    }

    /// <remarks>
    /// Proves HM-REQ-011's metric counts a sure substitution: `CQ DE K` sent,
    /// `CQ DI K` decoded all sure. `I` stands where `E` was sent: one invented.
    /// </remarks>
    [Fact]
    public void ASureWrongLetterIsInvented()
    {
        var invented = CwMetrics.Invented(Align("CQ DE K", S("C"), S("Q"), Gap, S("D"), S("I"), Gap, S("K")));

        Assert.Equal(0, invented.SureAdded);
        Assert.Equal(1, invented.SureWrong);
        Assert.Equal(1, invented.Count);
    }

    /// <remarks>
    /// Proves `CW_SPEC.md` 5.3 inside HM-REQ-011's metric: a placeholder where `E`
    /// was sent is never scored wrong, so nothing is invented.
    /// </remarks>
    [Fact]
    public void APlaceholderIsNeverInvented()
    {
        var invented = CwMetrics.Invented(Align("CQ DE K", S("C"), S("Q"), Gap, S("D"), Block, Gap, S("K")));

        Assert.Equal(0, invented.Count);
        Assert.Equal(5, invented.Sent);
    }

    /// <remarks>
    /// Proves a prosign is one character (`CW_SPEC.md` 6.2): `CQ &lt;BT&gt; K` is four
    /// characters sent. Read whole, nothing is invented; read as `B` and `T`, one
    /// stands where the prosign was sent and the other was never sent, two invented.
    /// </remarks>
    [Fact]
    public void AProsignIsOneCharacterAndItsLettersAreTwo()
    {
        var whole = CwMetrics.Invented(Align("CQ <BT> K", S("C"), S("Q"), Gap, S("<BT>"), Gap, S("K")));
        var split = CwMetrics.Invented(Align("CQ <BT> K", S("C"), S("Q"), Gap, S("B"), S("T"), Gap, S("K")));
        var caret = CwMetrics.Invented(Align("CQ ^BT K", S("C"), S("Q"), Gap, S("<BT>"), Gap, S("K")));

        Assert.Equal((0, 4), (whole.Count, whole.Sent));
        Assert.Equal((2, 4), (split.Count, split.Sent));
        Assert.Equal((0, 4), (caret.Count, caret.Sent));
    }

    /// <remarks>
    /// Proves HM-REQ-082's separation on HM-REQ-011's side: `CQDE K` against
    /// `CQ DE K` loses a boundary and invents no character.
    /// </remarks>
    [Fact]
    public void AMissingSpaceInventsNothing()
    {
        var invented = CwMetrics.Invented(Align("CQ DE K", S("C"), S("Q"), S("D"), S("E"), Gap, S("K")));

        Assert.Equal(0, invented.Count);
    }

    /// <remarks>
    /// Proves the metric over a whole call: `CQ CQ DE N0CALL K`, thirteen characters
    /// sent, decoded with a sure `E` before it and a sure `T` after it and a
    /// placeholder for the `0`. Two invented of thirteen.
    /// </remarks>
    [Fact]
    public void AWholeCallCountsItsLitterAndNotItsBlocks()
    {
        var decoded = new List<CwSymbol> { S("E"), Gap };

        decoded.AddRange("CQ CQ DE N".Select(c => c == ' ' ? Gap : S(c.ToString())));
        decoded.Add(Block);
        decoded.AddRange("CALL K".Select(c => c == ' ' ? Gap : S(c.ToString())));
        decoded.AddRange(new[] { Gap, S("T") });

        var invented = CwMetrics.Invented(CwMetrics.Align(decoded, "CQ CQ DE N0CALL K", CwKeyKind.Inferred));

        Assert.Equal(2, invented.SureAdded);
        Assert.Equal(0, invented.SureWrong);
        Assert.Equal(13, invented.Sent);
        Assert.Equal(CwKeyKind.Inferred, invented.Kind);
    }

    /// <remarks>
    /// Proves the class mapping from the decoder's own characters: a high letter is
    /// sure, an unreadable one a placeholder, a low one not sure and never invented,
    /// and a gap a boundary.
    /// </remarks>
    [Fact]
    public void TheDecodersOwnCharactersCarryTheirClasses()
    {
        CwCharacter C(string text, CwConfidence confidence)
            => new(text, confidence, 1, ".", 20, 20, TimeSpan.Zero);

        var symbols = CwMetrics.Symbols(new[]
        {
            C("E", CwConfidence.High), C(MorseAlphabet.WordGap, CwConfidence.High),
            C("T", CwConfidence.Low), C(MorseAlphabet.Unreadable, CwConfidence.Unreadable),
        });

        Assert.Equal(
            new[] { CwSymbolClass.Sure, CwSymbolClass.WordGap, CwSymbolClass.NotSure, CwSymbolClass.Placeholder },
            symbols.Select(s => s.Class));

        var invented = CwMetrics.Invented(CwMetrics.Align(symbols, "K", CwKeyKind.Exact));

        Assert.Equal(1, invented.Count);
    }

    /// <remarks>
    /// Proves HM-REQ-010's metric: `CQ DEE K` all sure is six sure characters of
    /// which one was never sent, and `CQ DI K` five of which one is wrong.
    /// </remarks>
    [Fact]
    public void TheSureErrorRateIsOverWhatWasEmittedSure()
    {
        var added = CwMetrics.SureErrors(Align("CQ DE K", S("C"), S("Q"), Gap, S("D"), S("E"), S("E"), Gap, S("K")));
        var wrong = CwMetrics.SureErrors(Align("CQ DE K", S("C"), S("Q"), Gap, S("D"), S("I"), Gap, S("K")));

        Assert.Equal((0, 1, 6), (added.SureWrong, added.SureAdded, added.SureEmitted));
        Assert.Equal(1.0 / 6, added.Rate);
        Assert.Equal((1, 0, 5), (wrong.SureWrong, wrong.SureAdded, wrong.SureEmitted));
        Assert.Equal(1.0 / 5, wrong.Rate);
    }

    /// <remarks>
    /// Proves `CW_SPEC.md` 5.3 inside HM-REQ-010's metric: a placeholder is neither
    /// counted sure nor wrong, and a decode of nothing but placeholders has no rate
    /// at all rather than a rate of nought (0.0).
    /// </remarks>
    [Fact]
    public void APlaceholderIsOutsideTheSureErrorRate()
    {
        var one = CwMetrics.SureErrors(Align("CQ DE K", S("C"), S("Q"), Gap, S("D"), Block, Gap, S("K")));
        var none = CwMetrics.SureErrors(Align("CQ DE K", Block, Block));

        Assert.Equal((0, 4), (one.Errors, one.SureEmitted));
        Assert.Equal(0.0, one.Rate);
        Assert.Equal(0, none.SureEmitted);
        Assert.Null(none.Rate);
    }

    /// <remarks>
    /// Proves HM-REQ-012's metric: four sure characters over five sent is 0.8; a
    /// sure extra letter counts toward coverage as the spec writes it, so six over
    /// five is 1.2, with the five right carried beside it.
    /// </remarks>
    [Fact]
    public void CoverageIsSureCharactersOverCharactersSent()
    {
        var blocked = CwMetrics.Coverage(Align("CQ DE K", S("C"), S("Q"), Gap, S("D"), Block, Gap, S("K")));
        var extra = CwMetrics.Coverage(Align("CQ DE K", S("C"), S("Q"), Gap, S("D"), S("E"), S("E"), Gap, S("K")));

        Assert.Equal((4, 4, 5), (blocked.SureEmitted, blocked.SureRight, blocked.Sent));
        Assert.Equal(0.8, blocked.Share);
        Assert.Equal((6, 5, 5), (extra.SureEmitted, extra.SureRight, extra.Sent));
        Assert.Equal(1.2, extra.Share);
    }

    /// <remarks>
    /// Proves the cheat HM-REQ-012 exists to catch is caught: a decode that claims
    /// nothing sure has no sure error rate and a coverage of nought.
    /// </remarks>
    [Fact]
    public void DimmingEverythingLeavesNoCoverage()
    {
        var dim = new[] { "C", "Q", " ", "D", "E", " ", "K" }
            .Select(t => t == " " ? Gap : new CwSymbol(t, CwSymbolClass.NotSure))
            .ToArray();
        var a = CwMetrics.Align(dim, "CQ DE K", CwKeyKind.Exact);

        Assert.Null(CwMetrics.SureErrors(a).Rate);
        Assert.Equal(0.0, CwMetrics.Coverage(a).Share);
    }

    /// <remarks>
    /// Proves HM-REQ-081's metric counts a deleted boundary and an inserted one, each
    /// over the words sent: `CQDE K` and `C Q DE K` against `CQ DE K`, three words.
    /// </remarks>
    [Fact]
    public void ABoundaryMissingOrAddedIsOneOverTheWordsSent()
    {
        var deleted = CwMetrics.WordBoundaries(Align("CQ DE K", S("C"), S("Q"), S("D"), S("E"), Gap, S("K")));
        var inserted = CwMetrics.WordBoundaries(Align("CQ DE K", S("C"), Gap, S("Q"), Gap, S("D"), S("E"), Gap, S("K")));
        var right = CwMetrics.WordBoundaries(Align("CQ DE K", S("C"), S("Q"), Gap, S("D"), S("E"), Gap, S("K")));

        Assert.Equal((0, 1, 3), (deleted.Inserted, deleted.Deleted, deleted.WordsSent));
        Assert.Equal(1.0 / 3, deleted.Rate);
        Assert.Equal((1, 0, 3), (inserted.Inserted, inserted.Deleted, inserted.WordsSent));
        Assert.Equal(0, right.Errors);
    }

    /// <remarks>
    /// Proves HM-REQ-082: the boundaries are scored apart from the letters. The
    /// scorer's own hand count, `DEW B 6 RE D` against `DE WB6RED`, is every letter
    /// right and five boundaries wrong - four inserted and the one after `DE`
    /// deleted - over two words; a wrong letter costs no boundary, and a missing
    /// letter does not move one.
    /// </remarks>
    [Fact]
    public void BoundariesAreScoredApartFromLetters()
    {
        var split = new List<CwSymbol>();

        split.AddRange("DEW B 6 RE D".Select(c => c == ' ' ? Gap : S(c.ToString())));

        var cut = CwMetrics.Align(split, "DE WB6RED", CwKeyKind.Exact);
        var letter = CwMetrics.WordBoundaries(Align("CQ DE K", S("C"), S("Q"), Gap, S("D"), S("I"), Gap, S("K")));
        var missing = CwMetrics.WordBoundaries(Align("AB CD", S("A"), S("B"), Gap, S("D")));

        Assert.Equal((4, 1, 2), (CwMetrics.WordBoundaries(cut).Inserted, CwMetrics.WordBoundaries(cut).Deleted, CwMetrics.WordBoundaries(cut).WordsSent));
        Assert.Equal(0, CwMetrics.Invented(cut).Count);
        Assert.Equal(0, letter.Errors);
        Assert.Equal(0, missing.Errors);
    }
}

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
}

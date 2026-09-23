using Xunit;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The scorer gives the answer a hand gives, on pairs whose edit distance can be
/// counted by eye (work instruction 410, task 1; PHASE_PLAN.md 0.1).
/// </summary>
/// <remarks>
/// <para>**EVERY ANSWER HERE IS KNOWN BY CONSTRUCTION, NOT MEASURED.** Each pair
/// is short enough that the minimum is plain: the edits needed are at least the
/// difference in length, or at least the characters the other side does not
/// have, and one alignment reaching that bound is written beside it. Watched red
/// against a scorer that did not exist yet before the scorer was written.</para>
/// </remarks>
public sealed class TheScorerCountsWhatAHandCountsTests
{
    /// <summary>Pairs scored whole, with the edits a hand counts.</summary>
    public static TheoryData<string, string, int> WholePairs { get; } = new()
    {
        // Nothing to change.
        { "CQ", "CQ", 0 },
        // Nothing read: both characters of the key are missing.
        { "", "CQ", 2 },
        // The textbook pair: K to S, E to I, and a G added.
        { "KITTEN", "SITTING", 3 },
        // One letter wrong in place.
        { "CQ DX", "CQ DE", 1 },
        // One space not read: the lengths differ by one and one insertion mends it.
        { "DEWB", "DE WB", 1 },
        // One space read where none was sent.
        { "D E", "DE", 1 },
        // One letter too many.
        { "CQQ", "CQ", 1 },
    };

    /// <remarks>Proves 0.1's edits: Levenshtein over the region, spaces included.</remarks>
    [Theory]
    [MemberData(nameof(WholePairs))]
    public void TheEditsAreWhatAHandCounts(string region, string key, int edits)
    {
        var score = CwScorer.Whole(region, key, CwKeyKind.Exact);

        Assert.Equal(edits, score.Edits);
        Assert.Equal(key.Length, score.ScoredLength);
        Assert.Equal(region, score.Region);
    }

    /// <remarks>Proves 0.1's third part: the key's kind comes out as it went in.</remarks>
    [Fact]
    public void TheKeysKindIsCarriedThrough()
    {
        Assert.Equal(CwKeyKind.Inferred, CwScorer.Whole("CQ", "CQ", CwKeyKind.Inferred).Kind);
        Assert.Equal(CwKeyKind.Exact, CwScorer.Whole("CQ", "CQ", CwKeyKind.Exact).Kind);
    }

    /// <summary>A key inside a longer decode, with the edits and the stretch a hand finds.</summary>
    public static TheoryData<string, string, int, string> WithinPairs { get; } = new()
    {
        // The key whole inside the decode: nothing outside it is scored.
        { "XXVA3VRRYY", "VA3VRR", 0, "VA3VRR" },
        // A near miss first and the key whole after it: the whole one is found.
        { "E WVRR VA3VRRT", "VA3VRR", 0, "VA3VRR" },
        // A, A and 4 appear nowhere before MP, so each costs one edit. Three
        // missing and two wrong-plus-one-missing tie; the scorer takes a letter
        // in place before a letter missing, so the two Es are in the region.
        { "EEMP/4 QNIKK", "AA4MP/4 QNIK", 3, "EEMP/4 QNIK" },
    };

    /// <remarks>
    /// Proves the region for a key that covers a fragment: only the stretch of
    /// the decode the key is aligned to is scored, and nothing either side of it
    /// is counted against the decoder (R61).
    /// </remarks>
    [Theory]
    [MemberData(nameof(WithinPairs))]
    public void AKeyInsideADecodeScoresOnlyItsStretch(
        string decode, string key, int edits, string region)
    {
        var score = CwScorer.Within(decode, key, CwKeyKind.Inferred);

        Assert.Equal(edits, score.Edits);
        Assert.Equal(key.Length, score.ScoredLength);
        Assert.Equal(region, score.Region);
    }

    /// <remarks>
    /// Proves the 17:37 key file's own rule: from the first `C` of the first `CQ`
    /// to the last character emitted, and nothing before it.
    /// </remarks>
    [Fact]
    public void TheRegionRunsFromTheFirstOpeningToTheLastCharacter()
    {
        Assert.Equal("CQ CQ DE X", CwScorer.FromFirst("E WB6RED CQ CQ DE X ", "CQ"));
        Assert.Equal("", CwScorer.FromFirst("E WB6RED", "CQ"));
    }
}

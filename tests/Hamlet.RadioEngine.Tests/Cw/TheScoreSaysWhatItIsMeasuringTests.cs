using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// The scoring harness, checked on cases whose answer can be counted by hand.
/// </summary>
/// <remarks>
/// <para>**A HARNESS THAT SCORES THE DECODER HAS TO BE SCORED ITSELF FIRST.**
/// Every number unit 045 publishes comes out of this, so an error here would not
/// look like an error — it would look like a decoder result, which is the failure
/// §12.5 exists for.</para>
/// <para>**AND THE TWO NUMBERS ARE NEVER REPORTED ALONE.** Yield counts every
/// truth character, so refusing everything scores nought; precision counts only
/// what Hamlet asserted, so refusing almost everything scores one. Either on its
/// own can be made to look good by a decoder that is useless.</para>
/// </remarks>
public sealed class TheScoreSaysWhatItIsMeasuringTests
{
    private readonly ITestOutputHelper _output;

    public TheScoreSaysWhatItIsMeasuringTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>A perfect read scores one on both numbers.</summary>
    [Fact]
    public void APerfectReadScoresOne()
    {
        var score = CwAccuracy.Score("CQ DE W1AW K", "CQ DE W1AW K");

        Assert.Equal(1.0, score.Yield, 9);
        Assert.Equal(1.0, score.Precision, 9);
        Assert.Equal(0, score.Substitutions);
        Assert.Equal(0, score.Insertions);
        Assert.Equal(0, score.Deletions);
    }

    /// <summary>
    /// A block is a deletion, and it does not count against precision.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE TRADE UNIT 036 WAS RULED ON.** Hamlet declining to name a
    /// character costs the operator a character he can see is missing. Naming it
    /// wrongly hands him a plausible letter he cannot tell from a right one. The
    /// score has to be able to show the difference or it cannot inform that
    /// ruling.
    /// </remarks>
    [Fact]
    public void ABlockIsADeletionAndNotASubstitution()
    {
        var blocked = CwAccuracy.Score("W1A■", "W1AW");
        var wrong = CwAccuracy.Score("W1AX", "W1AW");

        _output.WriteLine(
            $"blocked: yield {blocked.Yield:0.00} precision {blocked.Precision:0.00} "
            + $"subs {blocked.Substitutions} dels {blocked.Deletions}");
        _output.WriteLine(
            $"wrong  : yield {wrong.Yield:0.00} precision {wrong.Precision:0.00} "
            + $"subs {wrong.Substitutions} dels {wrong.Deletions}");

        Assert.Equal(1, blocked.Deletions);
        Assert.Equal(0, blocked.Substitutions);

        Assert.Equal(0, wrong.Deletions);
        Assert.Equal(1, wrong.Substitutions);

        // Both lose the same yield — the character is gone either way.
        Assert.Equal(blocked.Yield, wrong.Yield, 9);

        // **AND THE BLOCK KEEPS ITS PRECISION WHERE THE GUESS DOES NOT.** Three
        // asserted and three right against four asserted and three right.
        Assert.Equal(1.0, blocked.Precision, 9);
        Assert.Equal(0.75, wrong.Precision, 9);
    }

    /// <summary>
    /// Refusing everything scores nought yield and is not rewarded by precision.
    /// </summary>
    [Fact]
    public void RefusingEverythingYieldsNothing()
    {
        var score = CwAccuracy.Score("■■■■", "W1AW");

        Assert.Equal(0.0, score.Yield, 9);
        Assert.Equal(0.0, score.Precision, 9);
        Assert.Equal(4, score.Deletions);
        Assert.Equal(0, score.Substitutions);
    }

    /// <summary>
    /// The truth is a span, so what the decoder read around it is not scored.
    /// </summary>
    /// <remarks>
    /// **WHAT IS ADJUDICATED IS A FRAGMENT INSIDE THIRTY SECONDS OF AUDIO.**
    /// Scoring the rest of the transmission as error would measure the truth
    /// being partial rather than the decoder being wrong.
    /// </remarks>
    [Fact]
    public void OnlyTheSpanWithTruthIsScored()
    {
        var score = CwAccuracy.Score(
            "EEIE TTT DE KD0UN KD0UN K TTEIE EE", "DE KD0UN KD0UN K");

        _output.WriteLine(
            $"yield {score.Yield:0.00} precision {score.Precision:0.00}, "
            + $"scored {score.ScoredCharacters} of a 33-character read");

        Assert.Equal(1.0, score.Yield, 9);
        Assert.Equal(0, score.Substitutions);
        Assert.Equal(0, score.Insertions);
    }

    /// <summary>Insertions and deletions are counted apart.</summary>
    [Fact]
    public void InsertionsAndDeletionsAreCountedApart()
    {
        var inserted = CwAccuracy.Score("W1XAW", "W1AW");
        var dropped = CwAccuracy.Score("W1W", "W1AW");

        Assert.Equal(1, inserted.Insertions);
        Assert.Equal(0, inserted.Deletions);
        Assert.Equal(4, inserted.Correct);

        Assert.Equal(0, dropped.Insertions);
        Assert.Equal(1, dropped.Deletions);
        Assert.Equal(3, dropped.Correct);
    }

    /// <summary>Nothing read against real truth is a total loss, not a crash.</summary>
    [Fact]
    public void NothingReadIsScoredAsEveryCharacterLost()
    {
        var score = CwAccuracy.Score("", "DE KD0UN K");

        Assert.Equal(10, score.TruthCharacters);
        Assert.Equal(10, score.Deletions);
        Assert.Equal(0.0, score.Yield, 9);
    }

    /// <summary>Word spacing is not this score's question.</summary>
    /// <remarks>
    /// HM-DEC-142: a sender who leaves no word gaps produces an unspaced
    /// transcript a ham reads perfectly well, and counting every missing space as
    /// an error would rank that below a decoder that invented spaces.
    /// </remarks>
    [Fact]
    public void SpacingIsNormalizedRatherThanScored()
    {
        var score = CwAccuracy.Score("DE  KD0UN   K", "DE KD0UN K");

        Assert.Equal(1.0, score.Yield, 9);
    }

    /// <summary>The score is the same every run.</summary>
    /// <remarks>§5.4: wall-clock dependence in anything below the UI is a defect.</remarks>
    [Fact]
    public void TheScoreIsDeterministic()
    {
        var first = CwAccuracy.Score("E DEQ 6Q E ■Q DE KD0UN KD0UN K", "DE KD0UN KD0UN K");
        var second = CwAccuracy.Score("E DEQ 6Q E ■Q DE KD0UN KD0UN K", "DE KD0UN KD0UN K");

        Assert.Equal(first, second);
    }
}

using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The stated copy speed, and the line it may not cross (HM-DEC-066,
/// HM-OPEN-006).
/// </summary>
/// <remarks>
/// A stated speed is a preference and a measured ability is a different kind of
/// fact. Hamlet may compare a station's speed to a number in the settings,
/// because both are stated figures. It may not turn that comparison into a
/// verdict about the person reading it, and that is what these hold.
/// </remarks>
public sealed class CopySpeedTests
{
    private static readonly DateTime Now = new(2026, 8, 14, 20, 0, 0, DateTimeKind.Utc);

    private static ActivitySpot Morse(int wpm)
        => new("W1AW calling CQ", 7_030_000, "CW", "RBN", Now, wpm)
        {
            CallType = SpotCallType.Cq,
        };

    private static string ReasonFor(int wpm, int? stated)
        => SpotRanking.Evaluate(Morse(wpm), liveness: 1.0, null, stated).Reason;

    /// <remarks>
    /// Proves HM-DEC-066: the shipped default reproduces exactly the scale the
    /// ranking has always used, so the setting arriving changed nobody's list
    /// on its own. The bands are derived from the scale rather than typed again
    /// beside it, which is what makes this hold.
    /// </remarks>
    [Theory]
    [InlineData(10)]
    [InlineData(13)]
    [InlineData(18)]
    [InlineData(24)]
    [InlineData(35)]
    public void TheShippedDefaultScoresExactlyAsTheOldScaleDid(int wpm)
    {
        var withNothingStated = SpotRanking.Evaluate(Morse(wpm), 1.0).Score;
        var withTheDefault = SpotRanking
            .Evaluate(Morse(wpm), 1.0, null, SpotRankWeights.RelaxedWpm).Score;

        Assert.Equal(withNothingStated, withTheDefault);
    }

    /// <remarks>
    /// Proves HM-DEC-066: the bands move with the stated speed rather than
    /// staying where Morse itself draws them, so somebody who set 20 sees a
    /// station at 20 ranked the way somebody at the default sees one at 13.
    /// </remarks>
    [Fact]
    public void TheBandsFollowWhateverSpeedWasStated()
    {
        var atTheirPace = SpotRanking.Evaluate(Morse(20), 1.0, null, 20).Score;
        var atTheDefaultPace = SpotRanking.Evaluate(Morse(13), 1.0, null, 13).Score;

        Assert.Equal(atTheDefaultPace, atTheirPace);

        // And the same station is worth less to somebody who asked for slower.
        Assert.True(
            SpotRanking.Evaluate(Morse(20), 1.0, null, 10).Score < atTheirPace,
            "20 WPM should not score the same against a stated 10 and a stated 20");
    }

    /// <remarks>
    /// Proves HM-DEC-066: the card compares two stated numbers and never
    /// pronounces on the operator. "Faster than the 13 in your settings" is a
    /// comparison; "too fast for you" is a claim against a measurement nobody
    /// ever took, and §0.0 forbids it.
    /// </remarks>
    [Theory]
    [InlineData(8)]
    [InlineData(13)]
    [InlineData(17)]
    [InlineData(22)]
    [InlineData(30)]
    public void TheCardNeverSaysWhatTheOperatorCanCopy(int wpm)
    {
        var reason = ReasonFor(wpm, 13).ToLowerInvariant();

        foreach (var claim in new[]
                 {
                     "for you", "you can", "you cannot", "you can't", "too fast",
                     "too quick", "slow enough", "within your", "beyond you",
                     "you should manage", "comfortable for you",
                 })
        {
            Assert.False(reason.Contains(claim, StringComparison.Ordinal),
                $"'{reason}' claims something about the operator: '{claim}'");
        }
    }

    /// <remarks>
    /// Proves HM-DEC-066: Hamlet may say a station is sending faster than the
    /// stated number, because the operator stated it. Saying nothing at all
    /// would leave the ranking unexplained, which is its own failure
    /// (HM-DEC-025).
    /// </remarks>
    [Fact]
    public void AFasterStationIsSaidToBeFasterThanTheNumberInTheSettings()
    {
        Assert.Contains("28 WPM", ReasonFor(28, 13), StringComparison.Ordinal);
        Assert.Contains("over the 13", ReasonFor(28, 13), StringComparison.Ordinal);
        Assert.Contains("at or under the 13", ReasonFor(11, 13), StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-066: nothing is filtered by the stated speed. A station
    /// sending far faster than somebody asked for still appears, ranked lower,
    /// with the reason on it. Hiding it would be Hamlet deciding what they are
    /// capable of, which is exactly the claim it may not make.
    /// </remarks>
    [Fact]
    public void NothingIsHiddenByTheStatedSpeed()
    {
        var spots = new[] { Morse(12), Morse(22), Morse(35) };

        var ranked = SpotRanking.Rank(spots, Now, null, 12);

        Assert.Equal(3, ranked.Count);
        Assert.Equal(12, ranked[0].Spot.Wpm);
    }
}

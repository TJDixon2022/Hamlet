using Hamlet.App.ViewModels;
using Xunit;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// The happening-now freshness rule (HM-DEC-020). Every case here is a pure
/// function of an elapsed time and an interval — no clock, no waiting, no
/// flake (§5).
/// </summary>
public sealed class SpotFreshnessTests
{
    /// <remarks>Proves HM-DEC-020: the feed reads fresh inside twice the
    /// refresh interval, amber past it, and stale past four times it. The
    /// boundaries are exact so a future change to the multiples is a visible
    /// test change, not a silent drift.</remarks>
    [Theory]
    [InlineData(5, 0, FreshnessLevel.Fresh)]
    [InlineData(5, 5, FreshnessLevel.Fresh)]
    [InlineData(5, 10, FreshnessLevel.Fresh)]
    [InlineData(5, 11, FreshnessLevel.Aging)]
    [InlineData(5, 20, FreshnessLevel.Aging)]
    [InlineData(5, 21, FreshnessLevel.Stale)]
    [InlineData(1, 2, FreshnessLevel.Fresh)]
    [InlineData(1, 3, FreshnessLevel.Aging)]
    [InlineData(1, 5, FreshnessLevel.Stale)]
    [InlineData(15, 45, FreshnessLevel.Aging)]
    [InlineData(15, 61, FreshnessLevel.Stale)]
    public void Evaluate_CrossesAtTwiceAndFourTimesTheInterval(
        int intervalMinutes, int elapsedMinutes, FreshnessLevel expected)
        => Assert.Equal(
            expected,
            SpotFreshness.Evaluate(TimeSpan.FromMinutes(elapsedMinutes), intervalMinutes));

    /// <remarks>Proves HM-DEC-020: switching auto-refresh off does not switch
    /// off aging. With the interval at 0 the panel still measures against the
    /// shipped five minutes and still goes stale — the operator turned off the
    /// refresh, not the passage of time.</remarks>
    [Theory]
    [InlineData(3, FreshnessLevel.Fresh)]
    [InlineData(12, FreshnessLevel.Aging)]
    [InlineData(25, FreshnessLevel.Stale)]
    public void Evaluate_WithRefreshOff_StillAges(
        int elapsedMinutes, FreshnessLevel expected)
        => Assert.Equal(
            expected,
            SpotFreshness.Evaluate(TimeSpan.FromMinutes(elapsedMinutes), 0));

    /// <remarks>Proves HM-DEC-020: a negative elapsed time — a clock stepping
    /// backwards over a DST change or an NTP correction — reads fresh rather
    /// than throwing or reporting a negative age.</remarks>
    [Fact]
    public void Evaluate_WithClockGoingBackwards_ReadsFresh()
    {
        Assert.Equal(
            FreshnessLevel.Fresh,
            SpotFreshness.Evaluate(TimeSpan.FromMinutes(-90), 5));
        Assert.Equal("just now", SpotFreshness.Describe(TimeSpan.FromMinutes(-90)));
    }

    /// <remarks>Proves HM-DEC-020: the age line is plain language at every
    /// scale, so the panel never shows a raw timespan.</remarks>
    [Theory]
    [InlineData(0, "just now")]
    [InlineData(9, "just now")]
    [InlineData(30, "30s ago")]
    [InlineData(59, "59s ago")]
    [InlineData(60, "1 min ago")]
    [InlineData(4 * 60, "4 min ago")]
    [InlineData(89 * 60, "89 min ago")]
    [InlineData(90 * 60, "1 hour ago")]
    [InlineData(200 * 60, "3 hours ago")]
    public void Describe_ReadsAsEnglish(int elapsedSeconds, string expected)
        => Assert.Equal(expected, SpotFreshness.Describe(TimeSpan.FromSeconds(elapsedSeconds)));

    /// <remarks>Proves HM-DEC-021: the collapsed header still carries the
    /// count and the age, and HM-DEC-020: past four intervals it says so out
    /// loud rather than presenting an old count as current.</remarks>
    [Fact]
    public void Summary_CarriesCountAndAge_AndSaysStaleWhenItIs()
    {
        Assert.Equal(
            "7 spots · updated 30s ago",
            SpotFreshness.Summary(7, TimeSpan.FromSeconds(30), 5));

        Assert.Equal(
            "1 spot · updated 2 min ago",
            SpotFreshness.Summary(1, TimeSpan.FromMinutes(2), 5));

        Assert.Equal(
            "7 spots · stale · updated 42 min ago",
            SpotFreshness.Summary(7, TimeSpan.FromMinutes(42), 5));
    }

    /// <remarks>Proves the prime directive on the feed's first moments: before
    /// anything has loaded the panel says "loading…", not "0 spots", which
    /// would be a claim about the air.</remarks>
    [Fact]
    public void Summary_BeforeFirstLoad_DoesNotClaimZeroSpots()
        => Assert.Equal(
            "loading…",
            SpotFreshness.Summary(0, TimeSpan.Zero, 5, everLoaded: false));
}

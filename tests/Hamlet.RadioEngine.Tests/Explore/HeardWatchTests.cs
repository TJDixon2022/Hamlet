using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Did anybody hear me (HM-DEC-075, closing FG-008).
/// </summary>
/// <remarks>
/// The rule these all serve is one line: never manufacture the feeling, only
/// report the fact. Hamlet says he was heard because receivers really heard
/// him, and it does not inflate, round up, or soften a silence into something
/// warmer than the truth.
/// </remarks>
public sealed class HeardWatchTests
{
    private static readonly DateTime Called = new(2026, 8, 15, 15, 0, 0, DateTimeKind.Utc);

    private static HeardReport Report(
        string receiver, int? db = 19, int? wpm = 15, int afterSeconds = 90)
        => new(receiver, 7_030_000, db, wpm, Called.AddSeconds(afterSeconds));

    // ---- Only his own callsign ------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-075: the match is exact. Telling this operator he was heard
    /// when the machine heard a different station would be the cruelest bug in
    /// the application, and a near match is somebody else.
    /// </remarks>
    [Theory]
    [InlineData("KC3QIS", true)]
    [InlineData("kc3qis", true)]
    [InlineData("KC3QI", false)]
    [InlineData("KC3QISX", false)]
    [InlineData("W1AW", false)]
    public void OnlyHisOwnCallsignCounts(string heard, bool mine)
    {
        var spot = new RbnSpot(
            "WE9V", 7_030_000, heard, "CW", 19, 15, SpotCallType.Cq, Called);

        Assert.Equal(mine, HeardWatch.IsMine(spot, "KC3QIS"));
    }

    /// <remarks>
    /// Proves HM-DEC-075: with no callsign in the profile nothing is ever
    /// matched, rather than everything being matched.
    /// </remarks>
    [Fact]
    public void WithNoCallsignNothingIsMatched()
    {
        var spot = new RbnSpot(
            "WE9V", 7_030_000, "KC3QIS", "CW", 19, 15, SpotCallType.Cq, Called);

        Assert.False(HeardWatch.IsMine(spot, ""));
        Assert.False(HeardWatch.IsMine(spot, null));
        Assert.False(HeardWatch.IsMine(null, "KC3QIS"));
    }

    // ---- The three states ------------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-075: the waiting state holds the time honestly and says
    /// what is normal. Thirty to ninety seconds of silence is exactly where a
    /// beginner concludes it is not working and gives up, so the wait says that
    /// reports take a minute or two and that a person takes longer because they
    /// have to finish listening first.
    /// </remarks>
    [Fact]
    public void TheWaitSaysWhatIsNormalRatherThanJustSpinning()
    {
        var waiting = HeardWatch.Describe(
            Called, Array.Empty<HeardReport>(), Called.AddSeconds(40));

        Assert.Equal(HeardState.Waiting, waiting.State);
        Assert.Contains("minute or two", waiting.Detail, StringComparison.Ordinal);
        Assert.Contains("finish listening", waiting.Detail, StringComparison.Ordinal);
        Assert.Contains("completely ordinary", waiting.Detail, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-075: reports arriving move it to heard, and the headline
    /// counts the receivers rather than the reports, since one machine reporting
    /// twice is not two machines.
    /// </remarks>
    [Fact]
    public void ReportsArrivingSayWhoHeardHim()
    {
        var one = HeardWatch.Describe(
            Called, new[] { Report("WE9V") }, Called.AddMinutes(2));

        Assert.Equal(HeardState.Heard, one.State);
        Assert.Contains("WE9V", one.Headline, StringComparison.Ordinal);

        var several = HeardWatch.Describe(
            Called,
            new[] { Report("WE9V"), Report("K5TR"), Report("DL8LAS") },
            Called.AddMinutes(2));

        Assert.Contains("3 receivers", several.Headline, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-075: the strongest report is named rather than averaged,
    /// because an average describes a signal nobody actually received.
    /// </remarks>
    [Fact]
    public void TheStrongestReportIsNamedRatherThanAveraged()
    {
        var summary = HeardWatch.Describe(
            Called,
            new[] { Report("WE9V", db: 4), Report("K5TR", db: 28) },
            Called.AddMinutes(2));

        Assert.Contains(
            SignalReport.Describe(28).ToLowerInvariant(), summary.Detail,
            StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-075: silence is reported as information rather than as
    /// consolation, and it says plainly what it does and does not mean. Skimmer
    /// coverage is uneven, so no report is not proof nobody heard him.
    /// </remarks>
    [Fact]
    public void SilenceIsSaidPlainlyAndIsNotProofOfAnything()
    {
        var nothing = HeardWatch.Describe(
            Called, Array.Empty<HeardReport>(), Called + HeardWatch.Window);

        Assert.Equal(HeardState.Nothing, nothing.State);
        Assert.Contains("coverage", nothing.Detail, StringComparison.Ordinal);
        Assert.Contains(
            "not say your signal went nowhere", nothing.Detail, StringComparison.Ordinal);

        // Information, never a pat on the head.
        foreach (var soft in new[]
                 { "do not worry", "don't worry", "try again", "better luck", "keep trying" })
        {
            Assert.DoesNotContain(soft, nothing.Detail, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <remarks>
    /// Proves HM-DEC-075: silence is never called before the window is out. Ten
    /// minutes is the window and ninety seconds is not a verdict on anything.
    /// </remarks>
    [Theory]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(300)]
    public void SilenceIsNeverCalledEarly(int seconds)
    {
        var summary = HeardWatch.Describe(
            Called, Array.Empty<HeardReport>(), Called.AddSeconds(seconds));

        Assert.Equal(HeardState.Waiting, summary.State);
    }

    /// <remarks>
    /// Proves HM-DEC-075: with nothing called there is nothing to watch, and the
    /// panel explains what it will do rather than sitting blank.
    /// </remarks>
    [Fact]
    public void WithNothingCalledThereIsNothingToWatch()
    {
        var idle = HeardWatch.Describe(null, Array.Empty<HeardReport>(), Called);

        Assert.Equal(HeardState.Idle, idle.State);
        Assert.Contains("When you call CQ", idle.Detail, StringComparison.Ordinal);
    }

    // ---- Never inflating -------------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-075: the report carries what the receiver measured and
    /// nothing more. A missing figure produces an empty string rather than a
    /// cheerful stand-in.
    /// </remarks>
    [Fact]
    public void AMissingFigureIsSaidAsNothingRatherThanInvented()
    {
        var bare = Report("WE9V", db: null, wpm: null);

        Assert.Equal("", bare.Signal);
        Assert.Equal("", bare.Speed);

        var summary = HeardWatch.Describe(
            Called, new[] { bare }, Called.AddMinutes(2));

        Assert.Equal(HeardState.Heard, summary.State);
        Assert.Contains("WE9V", summary.Headline, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-075: the speed a receiver read is offered as the
    /// independent check on his keying that it is. A machine that timed the
    /// characters read them cleanly, which is the first feedback on his sending
    /// he has ever had.
    /// </remarks>
    [Fact]
    public void TheSpeedAReceiverReadIsOffered()
    {
        Assert.Equal("read at 15 words a minute", Report("WE9V", wpm: 15).Speed);

        var summary = HeardWatch.Describe(
            Called, new[] { Report("WE9V", db: 19, wpm: 15) }, Called.AddMinutes(2));

        Assert.Contains("15 words a minute", summary.Detail, StringComparison.Ordinal);
    }
}

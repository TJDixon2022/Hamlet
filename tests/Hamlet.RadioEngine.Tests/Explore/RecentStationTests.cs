using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Where the operator has been, and what Hamlet is allowed to say about it
/// (HM-DEC-072).
/// </summary>
/// <remarks>
/// The clock is injected throughout, the way the age rules already do it, so
/// the twenty-second dwell is proved to the second without waiting twenty of
/// them (§5).
/// </remarks>
public sealed class RecentStationTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc);

    private static RecentStation Visit(
        long hz, string station = "", DateTime? at = null)
        => RecentStations.From(hz, station, "CW", null, at ?? Now);

    // ---- Dwell, not landing -------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-072: hunting across a band records nothing. The dial is a
    /// scroll wheel, so a literal history would fill with near-identical
    /// entries between 7.029 and 7.031 and be useless inside a minute.
    /// </remarks>
    [Fact]
    public void AFrequencyPassedThroughIsNeverRemembered()
    {
        var dwell = new DwellTracker();
        var at = Now;

        // Somebody spinning the wheel across the band, a second per step.
        for (var hz = 7_028_000L; hz <= 7_032_000L; hz += 100)
        {
            dwell.Moved(hz, at);
            at = at.AddSeconds(1);

            Assert.False(
                dwell.Settled(at),
                $"{hz} was remembered after a second on it");
        }
    }

    /// <remarks>
    /// Proves HM-DEC-072: stopping somewhere is what makes it worth
    /// remembering, and the threshold is crossed rather than approached.
    /// </remarks>
    [Fact]
    public void AFrequencySatOnIsRemembered()
    {
        var dwell = new DwellTracker();
        dwell.Moved(7_030_000, Now);

        Assert.False(dwell.Settled(Now + RecentStations.Dwell.Subtract(
            TimeSpan.FromSeconds(1))));

        Assert.True(dwell.Settled(Now + RecentStations.Dwell));
    }

    /// <remarks>
    /// Proves HM-DEC-072: one stop is one place. Somebody who leaves the radio
    /// on one frequency all evening has been somewhere once, not two hundred
    /// times, and a list that filled with the same entry would be useless in
    /// exactly the way the dwell rule exists to prevent.
    /// </remarks>
    [Fact]
    public void SittingStillLongerDoesNotKeepReporting()
    {
        var dwell = new DwellTracker();
        dwell.Moved(7_030_000, Now);

        Assert.True(dwell.Settled(Now + RecentStations.Dwell));

        for (var minutes = 1; minutes <= 60; minutes++)
        {
            Assert.False(dwell.Settled(Now.AddMinutes(minutes)));
        }
    }

    /// <remarks>
    /// Proves HM-DEC-072: several surfaces announcing the same frequency in one
    /// gesture must not restart the count. The band buttons, the map and the
    /// tape can each report where the dial is, and if every one of them reset
    /// the clock the dial would never settle at all.
    /// </remarks>
    [Fact]
    public void RepeatingTheSameFrequencyDoesNotRestartTheClock()
    {
        var dwell = new DwellTracker();
        dwell.Moved(7_030_000, Now);
        dwell.Moved(7_030_000, Now.AddSeconds(10));
        dwell.Moved(7_030_000, Now.AddSeconds(15));

        Assert.True(dwell.Settled(Now + RecentStations.Dwell));
    }

    // ---- The list ------------------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-072: returning somewhere moves the entry rather than
    /// adding a second copy. A list with the same place in it three times has
    /// stopped being ten places.
    /// </remarks>
    [Fact]
    public void ReturningSomewhereMovesTheEntryRatherThanDuplicatingIt()
    {
        var list = RecentStations.Remember(
            Array.Empty<RecentStation>(), Visit(7_030_000));

        list = RecentStations.Remember(list, Visit(14_074_000));
        list = RecentStations.Remember(
            list, Visit(7_030_000, at: Now.AddMinutes(5)));

        Assert.Equal(2, list.Count);
        Assert.Equal(7_030_000, list[0].FrequencyHz);
        Assert.Equal(14_074_000, list[1].FrequencyHz);
    }

    /// <remarks>
    /// Proves HM-DEC-072: near enough is the same place, and the rule is a
    /// tolerance rather than a bucket so it does not flip at an invisible
    /// boundary. The wheel moves in small steps and nobody zero-beats twice to
    /// the same hertz.
    /// </remarks>
    [Theory]
    [InlineData(7_030_000, true)]
    [InlineData(7_030_150, true)]
    [InlineData(7_029_850, true)]
    [InlineData(7_030_200, true)]
    [InlineData(7_030_400, false)]
    [InlineData(7_029_500, false)]
    public void NearEnoughIsTheSamePlace(long other, bool same)
    {
        Assert.Equal(same, RecentStations.IsSamePlace(7_030_000, other));

        var list = RecentStations.Remember(
            new[] { Visit(7_030_000) }, Visit(other, at: Now.AddMinutes(1)));

        Assert.Equal(same ? 1 : 2, list.Count);
    }

    /// <remarks>
    /// Proves HM-DEC-072: the list holds at ten and the oldest falls off. Ten
    /// is the last few places he was, and a list long enough to need scrolling
    /// has stopped answering "where was I just now".
    /// </remarks>
    [Fact]
    public void TheListHoldsAtTenAndDropsTheOldest()
    {
        IReadOnlyList<RecentStation> list = Array.Empty<RecentStation>();

        for (var i = 0; i < 15; i++)
        {
            list = RecentStations.Remember(
                list, Visit(7_000_000 + (i * 1_000), at: Now.AddMinutes(i)));
        }

        Assert.Equal(RecentStations.Maximum, list.Count);

        // Newest first, and the five oldest are gone.
        Assert.Equal(7_014_000, list[0].FrequencyHz);
        Assert.Equal(7_005_000, list[^1].FrequencyHz);
        Assert.DoesNotContain(list, e => e.FrequencyHz == 7_004_000);
    }

    // ---- Named where Hamlet knows, honest where it does not -------------

    /// <remarks>
    /// Proves HM-DEC-072: an entry with no identified station never renders as
    /// though it had one. An entry reading like a callsign for a frequency the
    /// operator merely sat on would be Hamlet putting a station on the air that
    /// nobody heard, which is §0.0 broken on a surface built for navigation.
    /// </remarks>
    [Fact]
    public void AnEntryWithNoStationReadsAsAPlace()
    {
        var hood = new Neighborhood(
            "QRP watering hole", "QRP", 7_030_000, 7_040_000,
            "", "", 7_030_000, ModeFamily.Cw);

        var bare = RecentStations.From(7_030_000, null, "CW", hood, Now);

        Assert.False(bare.IsIdentified);
        Assert.Equal("", bare.Station);
        Assert.Equal("7.030, QRP watering hole", bare.Label);
        Assert.DoesNotContain(" on 7.030", bare.Label, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves HM-DEC-072: where something did identify a station, the entry
    /// says so, which is the whole reason to keep the list.
    /// </remarks>
    [Fact]
    public void AnEntryWithAStationReadsAsAStation()
    {
        var named = RecentStations.From(7_030_000, "w1aw", "CW", null, Now);

        Assert.True(named.IsIdentified);
        Assert.Equal("W1AW", named.Station);
        Assert.Equal("W1AW on 7.030", named.Label);
    }

    /// <remarks>
    /// Proves HM-DEC-072: the newest visit's identification wins, including
    /// when it is empty. Keeping a callsign from an earlier visit would say
    /// that station is there now, and nothing checked.
    /// </remarks>
    [Fact]
    public void ReturningWithNothingKnownDropsTheOldCallsign()
    {
        var list = RecentStations.Remember(
            Array.Empty<RecentStation>(), Visit(7_030_000, "W1AW"));

        Assert.Equal("W1AW", list[0].Station);

        list = RecentStations.Remember(
            list, Visit(7_030_000, at: Now.AddHours(2)));

        Assert.Single(list);
        Assert.False(list[0].IsIdentified);
        Assert.DoesNotContain("W1AW", list[0].Label, StringComparison.Ordinal);
    }

    // ---- Starring ------------------------------------------------------

    /// <remarks>
    /// Proves HM-DEC-072: starring a place he has been produces a favorite
    /// carrying exactly what a directly saved one carries. A favorite born this
    /// way has to be indistinguishable from one born at the star, because this
    /// is how most of them will actually be born.
    /// </remarks>
    [Fact]
    public void StarringAnEntryMakesTheSameFavoriteADirectSaveWouldMake()
    {
        var hood = new Neighborhood(
            "FT8 city", "FT8", 14_070_000, 14_099_000,
            "", "", 14_074_000, ModeFamily.Digital);

        var entry = RecentStations.From(14_074_000, "W1AW", "USB-D", hood, Now);

        var starred = RecentStations.ToFavorite(entry, hood, Now);
        var direct = Favorites.From(14_074_000, "USB-D", hood, Now);

        Assert.Equal(direct, starred);
        Assert.Equal(direct.Name, starred.Name);
        Assert.Equal(direct.BandName, starred.BandName);
        Assert.Equal(direct.Neighborhood, starred.Neighborhood);
    }

    /// <remarks>
    /// Proves HM-DEC-072: the callsign does not leak into the favorite's name.
    /// A favorite is a place, and naming one after whoever happened to be there
    /// once would be wrong the next evening.
    /// </remarks>
    [Fact]
    public void StarringDoesNotNameTheFavoriteAfterWhoeverWasThere()
    {
        var entry = RecentStations.From(7_030_000, "W1AW", "CW", null, Now);

        var starred = RecentStations.ToFavorite(entry, null, Now);

        Assert.DoesNotContain("W1AW", starred.Name, StringComparison.Ordinal);
    }
}

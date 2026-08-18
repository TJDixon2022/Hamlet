using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// A return lands on the entry that is already there, and the operator can take
/// any of them out (HM-DEC-134).
/// </summary>
/// <remarks>
/// <para>**THE FIXTURE IS A REAL EVENING, NOT AN INVENTED ONE.** The six dwells
/// below are session `9f9d23eb` of 2026-08-18, read off `tune_requested`
/// timestamps and recorded in HM-OPEN-039: three places visited twice each,
/// with the two 7.059 visits a hundred hertz apart. That gap is the one the
/// operator saw as a near duplicate and it is well inside HM-DEC-072's two
/// hundred, which is why 134 rules the fold rather than the tolerance.</para>
/// <para>Pure throughout: a list and a visit in, a list out, with the moment
/// passed rather than read (§5).</para>
/// </remarks>
public sealed class RecentReturnTests
{
    private static readonly DateTime Now = new(2026, 8, 18, 20, 30, 0, DateTimeKind.Utc);

    /// <summary>The evening, as HM-OPEN-039 measured it.</summary>
    private static readonly long[] Evening =
    {
        7_047_000, 7_059_600, 7_030_100, 7_059_500, 7_030_100, 7_059_600,
    };

    private static RecentStation Visit(long hz, int minute = 0, string station = "")
        => RecentStations.From(
            hz, station, "CW", null, Now.AddMinutes(minute),
            station.Length > 0 ? StationSource.Decoder : StationSource.None);

    /// <remarks>
    /// Proves HM-DEC-134: six qualifying dwells at three places leave three
    /// entries, which is the whole of what the operator reported and what the
    /// record could not tell apart (HM-OPEN-039).
    /// </remarks>
    [Fact]
    public void TheEveningLeavesThreePlacesAndNotSix()
    {
        IReadOnlyList<RecentStation> list = new List<RecentStation>();

        for (var i = 0; i < Evening.Length; i++)
        {
            list = RecentStations.Remember(list, Visit(Evening[i], i));
        }

        Assert.Equal(3, list.Count);
    }

    /// <remarks>
    /// Proves HM-DEC-134: the second visit is a fact about the entry that is
    /// there, so the count moves rather than the list growing. The evening's own
    /// arithmetic is the assertion — 7.047 once, 7.030 twice, 7.059 three times,
    /// six dwells across three places.
    /// </remarks>
    [Fact]
    public void AReturnCountsOnTheEntryAlreadyThere()
    {
        IReadOnlyList<RecentStation> list = new List<RecentStation>();

        for (var i = 0; i < Evening.Length; i++)
        {
            list = RecentStations.Remember(list, Visit(Evening[i], i));
        }

        Assert.Equal(1, Assert.Single(list, e => e.FrequencyHz == 7_047_000).Visits);
        Assert.Equal(2, Assert.Single(list, e => e.FrequencyHz == 7_030_100).Visits);
        Assert.Equal(3, Assert.Single(list, e => e.FrequencyHz == 7_059_600).Visits);

        Assert.Equal(6, list.Sum(e => e.Visits));
    }

    /// <remarks>
    /// Proves HM-DEC-134: a hundred hertz is one place, so the fold happens
    /// across the tolerance rather than only on an exact match. This is the
    /// pair the operator saw twice.
    /// </remarks>
    [Fact]
    public void AHundredHertzApartIsTheSamePlaceAndFolds()
    {
        var list = RecentStations.Remember(
            new List<RecentStation>(), Visit(7_059_500));

        list = RecentStations.Remember(list, Visit(7_059_600, 1));

        var only = Assert.Single(list);
        Assert.Equal(2, only.Visits);
        Assert.Equal(7_059_600, only.FrequencyHz);
    }

    /// <remarks>
    /// Proves HM-DEC-134: somewhere new starts at one and says nothing, because
    /// "you have been here once" is what every entry already means.
    /// </remarks>
    [Fact]
    public void AFirstVisitSaysNothingAboutComingBack()
    {
        var list = RecentStations.Remember(
            new List<RecentStation>(), Visit(7_030_000));

        Assert.Equal(1, list[0].Visits);
        Assert.False(list[0].IsReturn);
        Assert.Equal("", list[0].ReturnNote);
    }

    /// <remarks>
    /// Proves HM-DEC-134 and §0.7: the return is spoken rather than counted, so
    /// no surface has to render a bare number the operator must interpret.
    /// </remarks>
    [Fact]
    public void TheReturnIsSaidInWords()
    {
        var second = Visit(7_030_000) with { Visits = 2 };
        var many = Visit(7_030_000) with { Visits = 5 };

        Assert.Equal("you have been back here", second.ReturnNote);
        Assert.Equal("you keep coming back to this one", many.ReturnNote);
    }

    /// <remarks>
    /// Proves HM-DEC-072 survives HM-DEC-134: the newest visit's identification
    /// still wins, including when it is empty, and the count rides along with
    /// it rather than being tied to the name.
    /// </remarks>
    [Fact]
    public void TheNewestVisitStillWinsWhileTheCountCarries()
    {
        var list = RecentStations.Remember(
            new List<RecentStation>(), Visit(7_030_000, 0, "W1AW"));

        list = RecentStations.Remember(list, Visit(7_030_000, 1));

        var only = Assert.Single(list);
        Assert.Equal("", only.Station);
        Assert.False(only.IsIdentified);
        Assert.Equal(2, only.Visits);
    }

    /// <remarks>
    /// Proves HM-DEC-134: the operator can take one out, and it goes.
    /// </remarks>
    [Fact]
    public void OneEntryCanBeRemoved()
    {
        IReadOnlyList<RecentStation> list = new List<RecentStation>();

        foreach (var hz in new[] { 7_030_000L, 7_047_000L, 7_059_000L })
        {
            list = RecentStations.Remember(list, Visit(hz));
        }

        var kept = RecentStations.Remove(list, 7_047_000);

        Assert.Equal(2, kept.Count);
        Assert.DoesNotContain(kept, e => e.FrequencyHz == 7_047_000);
    }

    /// <remarks>
    /// Proves HM-DEC-134: removal matches exactly rather than by the tolerance.
    /// The operator is pointing at a row he can see, and taking a neighbor two
    /// hundred hertz away with it would be doing something he did not ask for.
    /// </remarks>
    [Fact]
    public void RemovingDoesNotTakeANeighborWithIt()
    {
        var list = RecentStations.Remove(
            new List<RecentStation> { Visit(7_030_100) }, 7_030_000);

        Assert.Single(list);
    }

    /// <remarks>
    /// Proves HM-DEC-134: a place visited again after being removed comes back
    /// counting from one, with no memory of having been dismissed.
    /// </remarks>
    [Fact]
    public void APlaceComesBackWithNoMemoryOfBeingForgotten()
    {
        var list = RecentStations.Remember(
            new List<RecentStation>(), Visit(7_030_000));

        list = RecentStations.Remember(list, Visit(7_030_000, 1));
        Assert.Equal(2, list[0].Visits);

        list = RecentStations.Remove(list, list[0].FrequencyHz);
        list = RecentStations.Remember(list, Visit(7_030_000, 2));

        Assert.Equal(1, Assert.Single(list).Visits);
    }

    /// <remarks>
    /// Proves HM-OPEN-039: leaving somewhere before the dwell is met is handed
    /// back to be recorded, with how far short it fell, because a list that
    /// stays empty while somebody sits still looks identical to a broken one
    /// (§0.0.1).
    /// </remarks>
    [Fact]
    public void LeavingEarlyReportsHowFarShortItFell()
    {
        var dwell = new DwellTracker();

        dwell.Moved(7_030_000, Now);
        var left = dwell.Moved(7_047_000, Now.AddSeconds(5));

        Assert.NotNull(left);
        Assert.Equal(7_030_000, left!.FrequencyHz);
        Assert.Equal(
            RecentStations.Dwell.TotalSeconds - 5, left.ShortBySeconds, 3);
    }

    /// <remarks>
    /// Proves HM-OPEN-039: a place that met the dwell was not abandoned, so
    /// moving on from it reports nothing. Otherwise every remembered entry
    /// would also file a near miss and the record would say both.
    /// </remarks>
    [Fact]
    public void APlaceThatSettledIsNotReportedAsLeftEarly()
    {
        var dwell = new DwellTracker();

        dwell.Moved(7_030_000, Now);
        Assert.True(dwell.Settled(Now + RecentStations.Dwell));

        Assert.Null(dwell.Moved(7_047_000, Now.AddSeconds(60)));
    }
}

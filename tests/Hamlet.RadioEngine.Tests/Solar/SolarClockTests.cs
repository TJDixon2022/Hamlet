using Hamlet.RadioEngine.Solar;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Solar;

/// <summary>
/// Sunrise and sunset, checked against published times (HM-DEC-033).
/// </summary>
/// <remarks>
/// <para>Every expected value here is read out of a vendored US Naval
/// Observatory response in <c>data/vendor/usno/</c> — not from memory, and not
/// from a service the test suite calls (§4). The file named in each comment is
/// the one it came from.</para>
/// <para>Tolerance is two minutes. The measured agreement is within one minute
/// at every point checked, so the extra minute is headroom rather than a
/// concession — and both are far tighter than anything Hamlet says out loud,
/// which is of the order of "the sun's been up about four hours".</para>
/// </remarks>
public sealed class SolarClockTests
{
    /// <summary>Trafford, Pennsylvania — the operator's own back yard.</summary>
    private const double PittsburghLat = 40.38;
    private const double PittsburghLon = -79.71;

    private static readonly TimeSpan Tolerance = TimeSpan.FromMinutes(2);

    private static void AssertClose(DateTime? actual, DateTime expected, string what)
    {
        Assert.True(actual is not null, $"{what} should have been computed");

        var drift = (actual!.Value - expected).Duration();
        Assert.True(
            drift <= Tolerance,
            $"{what}: computed {actual.Value:HH:mm} against published {expected:HH:mm} "
            + $"({drift.TotalMinutes:0.0} minutes out)");
    }

    /// <remarks>
    /// Proves the equation lands on published sunrise and sunset for the
    /// operator's own location, at both solstices and an equinox — the three
    /// dates where an error in the declination term would show up worst.
    /// </remarks>
    [Fact]
    public void Pittsburgh_MatchesPublishedTimes()
    {
        // pittsburgh-2026-06-21.json: Rise=09:49.
        AssertClose(
            SolarClock.EventUtc(PittsburghLat, PittsburghLon, new DateTime(2026, 6, 21), true),
            new DateTime(2026, 6, 21, 9, 49, 0, DateTimeKind.Utc),
            "summer solstice sunrise");

        // The sunset of that LOCAL evening falls after midnight UTC, so it is
        // pittsburgh-2026-06-22.json: Set=00:52.
        AssertClose(
            SolarClock.EventUtc(PittsburghLat, PittsburghLon, new DateTime(2026, 6, 21), false),
            new DateTime(2026, 6, 22, 0, 52, 0, DateTimeKind.Utc),
            "summer solstice sunset");

        // pittsburgh-2026-12-21.json: Rise=12:38, Set=21:56 — both on the same
        // UTC day in winter, when there is no wrap.
        AssertClose(
            SolarClock.EventUtc(PittsburghLat, PittsburghLon, new DateTime(2026, 12, 21), true),
            new DateTime(2026, 12, 21, 12, 38, 0, DateTimeKind.Utc),
            "winter solstice sunrise");

        AssertClose(
            SolarClock.EventUtc(PittsburghLat, PittsburghLon, new DateTime(2026, 12, 21), false),
            new DateTime(2026, 12, 21, 21, 56, 0, DateTimeKind.Utc),
            "winter solstice sunset");

        // pittsburgh-2026-08-13.json: Rise=10:28. Today, and the date every
        // other test in this file uses.
        AssertClose(
            SolarClock.EventUtc(PittsburghLat, PittsburghLon, new DateTime(2026, 8, 13), true),
            new DateTime(2026, 8, 13, 10, 28, 0, DateTimeKind.Utc),
            "August sunrise");

        // pittsburgh-2026-08-14.json: Set=00:19.
        AssertClose(
            SolarClock.EventUtc(PittsburghLat, PittsburghLon, new DateTime(2026, 8, 13), false),
            new DateTime(2026, 8, 14, 0, 19, 0, DateTimeKind.Utc),
            "August sunset");
    }

    /// <remarks>
    /// Proves the equation is not tuned to one place: the equator barely moves
    /// across the year, London swings hugely, and Tokyo is east of Greenwich,
    /// where the day wrap goes the other way — its sunrise is the evening
    /// before in UTC.
    /// </remarks>
    [Fact]
    public void OtherLongitudes_MatchPublishedTimes()
    {
        // london-2026-06-21.json: Rise=03:43, Set=20:22.
        AssertClose(
            SolarClock.EventUtc(51.51, -0.13, new DateTime(2026, 6, 21), true),
            new DateTime(2026, 6, 21, 3, 43, 0, DateTimeKind.Utc),
            "London midsummer sunrise");

        AssertClose(
            SolarClock.EventUtc(51.51, -0.13, new DateTime(2026, 6, 21), false),
            new DateTime(2026, 6, 21, 20, 22, 0, DateTimeKind.Utc),
            "London midsummer sunset");

        // quito-2026-03-21.json: Rise=11:18.
        AssertClose(
            SolarClock.EventUtc(-0.18, -78.47, new DateTime(2026, 3, 21), true),
            new DateTime(2026, 3, 21, 11, 18, 0, DateTimeKind.Utc),
            "Quito equinox sunrise");

        // tokyo-2026-06-20.json: Rise=19:26 — which is 04:26 on 21 June in
        // Tokyo, the local date asked for here.
        AssertClose(
            SolarClock.EventUtc(35.68, 139.69, new DateTime(2026, 6, 21), true),
            new DateTime(2026, 6, 20, 19, 26, 0, DateTimeKind.Utc),
            "Tokyo midsummer sunrise");
    }

    /// <remarks>
    /// Proves the day is longer in June than in December in the north, and
    /// that the two solstices bracket the year. A sign error in the
    /// declination would pass a single-date check and fail this.
    /// </remarks>
    [Fact]
    public void DayLength_SwingsTheRightWayAcrossTheYear()
    {
        static TimeSpan Length(DateTime date)
        {
            var rise = SolarClock.EventUtc(PittsburghLat, PittsburghLon, date, true)!.Value;
            var set = SolarClock.EventUtc(PittsburghLat, PittsburghLon, date, false)!.Value;
            return set - rise;
        }

        var june = Length(new DateTime(2026, 6, 21));
        var december = Length(new DateTime(2026, 12, 21));
        var march = Length(new DateTime(2026, 3, 21));

        Assert.True(june > TimeSpan.FromHours(14), $"midsummer day was {june}");
        Assert.True(december < TimeSpan.FromHours(10), $"midwinter day was {december}");
        Assert.InRange(march, TimeSpan.FromHours(11.5), TimeSpan.FromHours(12.5));
    }

    /// <remarks>
    /// <para>Proves the state is right at the three moments the band cards care
    /// about: mid-afternoon, after dusk, and the small hours.</para>
    /// <para>On 13 August 2026 in Pittsburgh the sun rises about 10:32 UTC and
    /// sets about 00:22 UTC the following day. Each row gives the local time it
    /// stands for, because a UTC hour on its own says nothing to a reader.</para>
    /// </remarks>
    [Theory]
    [InlineData(13, 18, 0, SunState.Day)]     // 2pm EDT on the 13th
    [InlineData(14, 1, 30, SunState.Night)]   // 9:30pm EDT on the 13th, after dusk
    [InlineData(13, 5, 0, SunState.Night)]    // 1am EDT on the 13th
    public void DayAndNight_AreCorrectAtKnownMoments(
        int utcDay, int utcHour, int utcMinute, SunState expected)
    {
        var instant = new DateTime(2026, 8, utcDay, utcHour, utcMinute, 0, DateTimeKind.Utc);
        var sun = SolarClock.At(PittsburghLat, PittsburghLon, instant);

        Assert.Equal(expected, sun.State);
    }

    /// <remarks>
    /// <para>Regression guard. The equation produces a time of day with no date
    /// attached; stamping it on the local date puts a Pennsylvania summer sunset
    /// at 00:54 on the MORNING of the same day, which is nine hours before that
    /// day's sunrise. The state check then reads "night" for every hour of every
    /// summer day, and the band cards say so on screen.</para>
    /// <para>Swept across the year and both hemispheres, plus a longitude either
    /// side of the date line where the wrap goes the other way.</para>
    /// </remarks>
    [Theory]
    [InlineData(PittsburghLat, PittsburghLon)]
    [InlineData(51.51, -0.13)]      // London
    [InlineData(35.68, 139.69)]     // Tokyo, where sunrise is the previous UTC day
    [InlineData(-33.87, 151.21)]    // Sydney
    [InlineData(21.31, -157.86)]    // Honolulu, furthest west
    public void SunsetAlwaysFollowsSunrise(double latitude, double longitude)
    {
        for (var day = 0; day < 365; day += 1)
        {
            var date = new DateTime(2026, 1, 1).AddDays(day);

            var rise = SolarClock.EventUtc(latitude, longitude, date, true);
            var set = SolarClock.EventUtc(latitude, longitude, date, false);

            if (rise is null || set is null)
            {
                continue;
            }

            var length = set.Value - rise.Value;

            Assert.True(
                length > TimeSpan.Zero,
                $"{latitude},{longitude} on {date:yyyy-MM-dd}: sunset {set:yyyy-MM-dd HH:mm} "
                + $"came before sunrise {rise:yyyy-MM-dd HH:mm}");

            Assert.True(
                length < TimeSpan.FromHours(24),
                $"{latitude},{longitude} on {date:yyyy-MM-dd}: day was {length}");
        }
    }

    /// <remarks>
    /// Proves the state is never stuck: sampled every hour through a summer day
    /// and a winter day, the operator's location sees both daylight and
    /// darkness. The wrap bug this guards against showed as a permanent night.
    /// </remarks>
    [Theory]
    [InlineData(6, 21)]
    [InlineData(12, 21)]
    public void BothStatesOccurAcrossADay(int month, int day)
    {
        var states = Enumerable.Range(0, 24)
            .Select(h => SolarClock.At(
                PittsburghLat, PittsburghLon,
                new DateTime(2026, month, day, h, 0, 0, DateTimeKind.Utc)).State)
            .ToList();

        Assert.Contains(SunState.Day, states);
        Assert.Contains(SunState.Night, states);
    }

    /// <remarks>
    /// Proves the snapshot knows how long it has been since the sun went down,
    /// which is what the hover text says out loud.
    /// </remarks>
    [Fact]
    public void Snapshot_KnowsHowLongSinceSunset()
    {
        var date = new DateTime(2026, 8, 13);
        var sunset = SolarClock.EventUtc(PittsburghLat, PittsburghLon, date, false)!.Value;

        var later = sunset.AddMinutes(95);
        var sun = SolarClock.At(PittsburghLat, PittsburghLon, later);

        Assert.Equal(SunState.Night, sun.State);
        Assert.NotNull(sun.SinceSunset);
        Assert.InRange(sun.SinceSunset!.Value.TotalMinutes, 94, 96);
    }

    /// <remarks>
    /// Proves daylight carries how long the sun has been up and how long is
    /// left, and that night does not pretend to.
    /// </remarks>
    [Fact]
    public void Snapshot_SeparatesDayFieldsFromNightFields()
    {
        var noon = new DateTime(2026, 8, 13, 16, 0, 0, DateTimeKind.Utc);
        var day = SolarClock.At(PittsburghLat, PittsburghLon, noon);

        Assert.True(day.IsDaylight);
        Assert.NotNull(day.SinceSunrise);
        Assert.NotNull(day.UntilSunset);
        Assert.Null(day.SinceSunset);

        var night = SolarClock.At(
            PittsburghLat, PittsburghLon,
            new DateTime(2026, 8, 13, 5, 0, 0, DateTimeKind.Utc));

        Assert.False(night.IsDaylight);
        Assert.Null(night.SinceSunrise);
    }

    /// <remarks>
    /// Proves polar summer and polar winter do not produce an invented
    /// sunrise. No station Hamlet serves is likely to be there, but a
    /// confident wrong answer is worse than none (HM-DEC-009).
    /// </remarks>
    [Fact]
    public void InsideThePolarCircles_NoTimeIsInvented()
    {
        // Longyearbyen in June: the sun does not set.
        var midnightSun = SolarClock.At(78.22, 15.65, new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc));
        Assert.Equal(SunState.Day, midnightSun.State);
        Assert.Null(midnightSun.SunriseUtc);
        Assert.Null(midnightSun.SunsetUtc);

        // And in December it does not rise.
        var polarNight = SolarClock.At(78.22, 15.65, new DateTime(2026, 12, 21, 12, 0, 0, DateTimeKind.Utc));
        Assert.Equal(SunState.Night, polarNight.State);
        Assert.Null(polarNight.SunriseUtc);
    }

    /// <remarks>
    /// Proves determinism (§5): the same place and instant always give the same
    /// answer, because nothing here reads a clock.
    /// </remarks>
    [Fact]
    public void Snapshot_IsDeterministic()
    {
        var instant = new DateTime(2026, 8, 13, 19, 0, 0, DateTimeKind.Utc);

        Assert.Equal(
            SolarClock.At(PittsburghLat, PittsburghLon, instant),
            SolarClock.At(PittsburghLat, PittsburghLon, instant));
    }

    /// <remarks>
    /// Proves an unknown location claims nothing at all.
    /// </remarks>
    [Fact]
    public void UnknownSnapshot_ClaimsNothing()
    {
        Assert.Equal(SunState.Unknown, SolarSnapshot.Unknown.State);
        Assert.False(SolarSnapshot.Unknown.IsKnown);
        Assert.False(SolarSnapshot.Unknown.IsDaylight);
    }
}

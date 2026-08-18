using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Solar;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// The band character passages: what they may say, and what they may never say
/// (HM-DEC-033, HM-DEC-034).
/// </summary>
public sealed class BandCharacterTests
{
    private const double PittsburghLat = 40.38;
    private const double PittsburghLon = -79.71;

    private static readonly IReadOnlyList<string> AllBands =
        HfBands.Bands.Select(b => b.Name).ToList();

    /// <summary>
    /// Claims about the ionosphere, which Hamlet cannot see (FG-007).
    /// </summary>
    /// <remarks>
    /// Same list as the activity tooltips are held to, plus the phrases that
    /// character prose is specifically tempted by: it is describing what a band
    /// is like, and "40 m will get you across the country tonight" is one word
    /// away from a promise about tonight's ionosphere.
    /// </remarks>
    private static readonly string[] Banned =
    {
        "is closed", "is open", "band is dead", "propagation", "the ionosphere",
        "will not hear", "cannot hear", "you can work", "you will reach",
        "you'll reach", "guaranteed", "band is dead", "conditions are",
        "is wide open", "no one is on",
    };

    private static SolarSnapshot SunAt(int month, int day, int utcHour)
        => SolarClock.At(
            PittsburghLat, PittsburghLon,
            new DateTime(2026, month, day, utcHour, 0, 0, DateTimeKind.Utc));

    /// <remarks>
    /// The sweep the brief asks for: every band, at every hour of a day in
    /// every season, plus the no-location case. Nothing in any of those
    /// passages may claim a band is open, closed, or reaching anywhere.
    /// </remarks>
    [Fact]
    public void NoPassageEverClaimsWhatTheIonosphereIsDoing()
    {
        var checkedPassages = 0;

        // One day in each meteorological season, north and south.
        var days = new[] { (1, 15), (4, 15), (7, 15), (10, 15) };

        foreach (var band in AllBands)
        {
            foreach (var (month, day) in days)
            {
                for (var hour = 0; hour < 24; hour++)
                {
                    foreach (var latitude in new double?[] { PittsburghLat, -33.87, null })
                    {
                        var sun = latitude is null
                            ? SolarSnapshot.Unknown
                            : SunAt(month, day, hour);

                        var text = BandCharacter.Describe(band, sun, month, latitude);

                        Assert.False(string.IsNullOrWhiteSpace(text));

                        foreach (var phrase in Banned)
                        {
                            Assert.DoesNotContain(
                                phrase, text, StringComparison.OrdinalIgnoreCase);
                        }

                        checkedPassages++;
                    }
                }
            }
        }

        // A sweep that silently covered nothing would pass every assertion in
        // it, so the count is asserted too.
        Assert.Equal(AllBands.Count * days.Length * 24 * 3, checkedPassages);
        Assert.True(checkedPassages > 2000);
    }

    /// <remarks>
    /// Proves the passage matches the hour it was asked about: daylight text at
    /// two in the afternoon, after-dark text at one in the morning. A card that
    /// says "the sun's up" at midnight is the kind of thing nobody reports and
    /// everybody stops trusting.
    /// </remarks>
    [Theory]
    [InlineData(13, 18, true)]   // 2pm EDT
    [InlineData(13, 5, false)]   // 1am EDT
    [InlineData(14, 1, false)]   // 9pm EDT the evening before
    public void PassageMatchesTheHour(int utcDay, int utcHour, bool daylight)
    {
        var sun = SolarClock.At(
            PittsburghLat, PittsburghLon,
            new DateTime(2026, 8, utcDay, utcHour, 0, 0, DateTimeKind.Utc));

        Assert.Equal(daylight, sun.IsDaylight);

        foreach (var band in AllBands)
        {
            var text = BandCharacter.DescribeNow(band, sun);

            // Only the claims about NOW are checked. Daytime text is free to
            // say "come back after dark" — that is an invitation, not a claim
            // about the present hour.
            if (daylight)
            {
                Assert.DoesNotContain("sun went down", text, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("the sun's down", text, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                Assert.DoesNotContain("sun's up", text, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("with the sun up", text, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("daylight is the first", text, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    /// <remarks>
    /// Proves an unknown location says nothing about the sun and tells the
    /// operator how to fix that, rather than guessing at a hemisphere
    /// (HM-DEC-009).
    /// </remarks>
    [Fact]
    public void WithoutALocation_NothingAboutTheSunIsClaimed()
    {
        foreach (var band in AllBands)
        {
            var text = BandCharacter.Describe(band, SolarSnapshot.Unknown, 8, null);

            Assert.DoesNotContain("right now the sun", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("sun went down", text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Settings", text, StringComparison.Ordinal);
        }
    }

    /// <remarks>
    /// Proves the season line is dropped entirely without a hemisphere, rather
    /// than defaulting to the northern one. August is summer in Pennsylvania
    /// and winter in Sydney, and picking one on no evidence would be a guess
    /// presented as a fact.
    /// </remarks>
    [Fact]
    public void WithoutALatitude_NoSeasonIsClaimed()
    {
        Assert.Equal(Season.Unknown, BandCharacter.SeasonAt(8, null));
        Assert.Equal("", BandCharacter.DescribeSeason("80 m", Season.Unknown));

        Assert.Equal(Season.Summer, BandCharacter.SeasonAt(8, PittsburghLat));
        Assert.Equal(Season.Winter, BandCharacter.SeasonAt(8, -33.87));
    }

    /// <remarks>
    /// Proves the hemispheres are mirrored right across the whole calendar, not
    /// just at the month somebody happened to test.
    /// </remarks>
    [Fact]
    public void SeasonsAreOppositeAcrossTheEquator()
    {
        for (var month = 1; month <= 12; month++)
        {
            var north = BandCharacter.SeasonAt(month, 40);
            var south = BandCharacter.SeasonAt(month, -40);

            Assert.NotEqual(Season.Unknown, north);
            Assert.NotEqual(north, south);
        }

        Assert.Equal(Season.Winter, BandCharacter.SeasonAt(1, 40));
        Assert.Equal(Season.Summer, BandCharacter.SeasonAt(7, 40));
    }

    /// <remarks>
    /// Proves every band is in exactly one element and the low bands are the
    /// night ones — the fact the whole card design rests on.
    /// </remarks>
    [Fact]
    public void EveryBandHasAnElement()
    {
        Assert.Equal(BandElement.Night, BandCharacter.ElementOf("80 m"));
        Assert.Equal(BandElement.Both, BandCharacter.ElementOf("40 m"));
        Assert.Equal(BandElement.Both, BandCharacter.ElementOf("30 m"));

        foreach (var band in AllBands.Where(b => b is not ("80 m" or "40 m" or "30 m")))
        {
            Assert.Equal(BandElement.Day, BandCharacter.ElementOf(band));
        }
    }

    /// <remarks>
    /// Proves the elapsed-time phrasing never puts a decimal or a stray digit
    /// into prose that is meant to sound like a person talking (HM-DEC-034).
    /// </remarks>
    [Theory]
    [InlineData(1, "a few minutes")]
    [InlineData(12, "about twenty minutes")]
    [InlineData(35, "about half an hour")]
    [InlineData(65, "about an hour")]
    [InlineData(95, "about an hour and a half")]
    [InlineData(190, "about three hours")]
    [InlineData(800, "most of the night")]
    public void ElapsedTimeIsSpokenNotCounted(int minutes, string expected)
        => Assert.Equal(expected, BandCharacter.Roughly(TimeSpan.FromMinutes(minutes)));

    /// <remarks>
    /// Proves no passage contains a digit at all, across the whole sweep. The
    /// band names are the one exception and are checked for explicitly.
    /// </remarks>
    [Fact]
    public void PassagesCarryNoBareNumbers()
    {
        foreach (var band in AllBands)
        {
            for (var hour = 0; hour < 24; hour += 3)
            {
                var text = BandCharacter.DescribeNow(band, SunAt(8, 13, hour));

                // Strip the band names, which legitimately carry digits.
                foreach (var name in AllBands)
                {
                    text = text.Replace(name, "BAND", StringComparison.Ordinal);
                }

                Assert.DoesNotContain(text, c => char.IsDigit(c));
            }
        }
    }

    /// <remarks>
    /// Proves determinism (§5): the same band, sun and date always produce the
    /// same words, because nothing in here reads a clock or a random source.
    /// </remarks>
    [Fact]
    public void DescriptionsAreDeterministic()
    {
        var sun = SunAt(8, 13, 18);

        foreach (var band in AllBands)
        {
            Assert.Equal(
                BandCharacter.Describe(band, sun, 8, PittsburghLat),
                BandCharacter.Describe(band, sun, 8, PittsburghLat));
        }
    }
}

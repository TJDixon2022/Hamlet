using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Distance is shown only where the app can justify the number, and it always
/// means distance to the STATION (HM-DEC-038).
/// </summary>
public sealed class SpotDistanceTests
{
    private static readonly DateTime Now = new(2026, 8, 14, 12, 0, 0, DateTimeKind.Utc);

    private static readonly LatLon Home = new(40.3782746, -79.7081649);

    private static ActivitySpot Spot(LatLon? at, string source = "POTA")
        => new("Somebody is activating a park", 7_032_000, "CW", source, Now, 15)
        {
            StationLocation = at,
        };

    /// <remarks>
    /// Proves a POTA spot with the park's coordinates reads the way somebody
    /// would say it — a rounded distance and a compass point, not a bearing in
    /// degrees (§0.7).
    /// </remarks>
    [Fact]
    public void APotaSpotReadsLikeSomebodySayingIt()
    {
        // Cleveland-ish: about 100 miles northwest of Trafford.
        var text = SpotDistance.Describe(
            Home, Spot(new LatLon(41.4993, -81.6944)), DistanceUnits.Miles);

        Assert.Contains("miles", text, StringComparison.Ordinal);
        Assert.Contains("northwest", text, StringComparison.Ordinal);
        Assert.DoesNotContain("°", text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// THE CENTRAL RULE. Proves an unknown grid produces no distance anywhere,
    /// on any spot, from any source. A number the app cannot justify is not
    /// shown, and the absence is the honest answer (§0.0).
    /// </remarks>
    [Fact]
    public void AnUnknownGridProducesNoDistanceAnywhere()
    {
        foreach (var source in new[] { "POTA", "SOTA", "RBN", "sample" })
        {
            var text = SpotDistance.Describe(
                null, Spot(new LatLon(41.4993, -81.6944), source), DistanceUnits.Miles);

            Assert.Equal("", text);
        }
    }

    /// <remarks>
    /// THE OTHER HALF OF IT. Proves a spot whose source did not state where the
    /// station is shows no distance even when the operator's grid is known.
    /// RBN states which receiver decoded a signal — where somebody who HEARD it
    /// is — and "480 miles northeast" attached to that would be a
    /// straightforward lie about the transmitter.
    /// </remarks>
    [Fact]
    public void ASpotWithNoStatedStationLocationShowsNothing()
    {
        Assert.Equal("", SpotDistance.Describe(Home, Spot(null, "RBN"), DistanceUnits.Miles));
        Assert.False(SpotDistance.CanDescribe(Spot(null, "RBN")));
        Assert.True(SpotDistance.CanDescribe(Spot(Home)));
    }

    /// <remarks>
    /// Proves the two units both work and disagree by the right factor, so a
    /// European contributor flipping the setting gets kilometers rather than
    /// mislabeled miles.
    /// </remarks>
    [Fact]
    public void BothUnitsAreOffered()
    {
        var spot = Spot(new LatLon(51.5074, -0.1278));

        var miles = SpotDistance.Describe(Home, spot, DistanceUnits.Miles);
        var km = SpotDistance.Describe(Home, spot, DistanceUnits.Kilometers);

        Assert.Contains("miles", miles, StringComparison.Ordinal);
        Assert.Contains("km", km, StringComparison.Ordinal);
        Assert.NotEqual(miles, km);
    }

    /// <remarks>
    /// Proves a station in the next town says how far and not which way. At
    /// five miles the bearing is about which end of town, and a compass point
    /// would imply the number meant something it does not.
    /// </remarks>
    [Fact]
    public void AVeryCloseStationDropsTheCompassPoint()
    {
        var text = SpotDistance.Describe(
            Home, Spot(new LatLon(40.40, -79.72)), DistanceUnits.Miles);

        Assert.Contains("away", text, StringComparison.Ordinal);
        Assert.DoesNotContain("north", text, StringComparison.Ordinal);
        Assert.DoesNotContain("south", text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the figure carries no false precision. It is a distance to a
    /// park's stated reference point, so a whole-number mile count would be
    /// claiming accuracy nothing in the chain supports.
    /// </remarks>
    [Fact]
    public void TheFigureIsRoundedToWhatItDeserves()
    {
        var text = SpotDistance.Describe(
            Home, Spot(new LatLon(51.5074, -0.1278)), DistanceUnits.Miles);

        // Four figures at this range would be a claim; the tens digit is zero.
        var digits = new string(text.TakeWhile(char.IsDigit).ToArray());

        Assert.True(digits.Length > 0, $"no distance in '{text}'");
        Assert.EndsWith("0", digits, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves the whole thing is pure (§5): two positions and a unit in, the
    /// same phrase out, with no clock and no network anywhere in it.
    /// </remarks>
    [Fact]
    public void DescriptionIsDeterministic()
    {
        var spot = Spot(new LatLon(41.4993, -81.6944));

        Assert.Equal(
            SpotDistance.Describe(Home, spot, DistanceUnits.Miles),
            SpotDistance.Describe(Home, spot, DistanceUnits.Miles));
    }

    /// <remarks>
    /// Proves POTA's own coordinates become the station location, so a park
    /// activation carries a distance and does so from the source's stated
    /// figure rather than from anything inferred.
    /// </remarks>
    [Fact]
    public void PotaSuppliesTheParkAsTheStationLocation()
    {
        var record = new PotaActivitySource.PotaSpot
        {
            Activator = "WC2A",
            Frequency = "14046.9",
            Mode = "CW",
            Reference = "US-4410",
            Name = "White River National Forest",
            LocationDesc = "US-CO",
            SpotTime = "2026-08-13T20:39:28",
            Latitude = 39.5053,
            Longitude = -106.916,
        };

        var source = new PotaActivitySource(
            new System.Net.Http.HttpClient(), () => Now);

        var spot = source.Convert(record);

        Assert.NotNull(spot);
        Assert.NotNull(spot!.StationLocation);
        Assert.Equal(39.5053, spot.StationLocation!.Value.Latitude, 4);

        var text = SpotDistance.Describe(Home, spot, DistanceUnits.Miles);
        Assert.Contains("west", text, StringComparison.Ordinal);
    }

    /// <remarks>
    /// Proves a POTA record with no usable coordinates yields no station
    /// location, so the card simply carries no distance rather than placing a
    /// park at Null Island.
    /// </remarks>
    [Theory]
    [InlineData(null, null)]
    [InlineData(39.5, null)]
    [InlineData(0.0, 0.0)]
    [InlineData(999.0, 12.0)]
    public void PotaWithoutUsableCoordinatesSaysNothing(double? lat, double? lon)
    {
        var record = new PotaActivitySource.PotaSpot
        {
            Activator = "WC2A",
            Frequency = "14046.9",
            Mode = "CW",
            SpotTime = "2026-08-13T20:39:28",
            Latitude = lat,
            Longitude = lon,
        };

        Assert.Null(PotaActivitySource.ParkLocation(record));
    }
}

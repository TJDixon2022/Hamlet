using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Maidenhead conversion, distance and bearing — the arithmetic the operator
/// is never asked to do (HM-DEC-037, HM-DEC-038).
/// </summary>
public sealed class GridSquareTests
{
    /// <remarks>
    /// <para>Proves known coordinates give known locators. Every reference
    /// below is a published grid for that place, and the set crosses both
    /// hemispheres in both axes on purpose: everything in the conversion is
    /// measured from −180° and −90°, so a sign error survives any test that
    /// stays in the northern and western quarter of the world — which is
    /// exactly where a US-focused test suite naturally sits.</para>
    /// <para>KC3QIS is the case that matters most: callook computes "FN00dj"
    /// for those coordinates independently, so this is Hamlet's arithmetic
    /// agreeing with somebody else's.</para>
    /// </remarks>
    [Theory]
    // North and west — the operator's own back yard.
    [InlineData(40.3782746, -79.7081649, "FN00")]
    // North and east — Berlin.
    [InlineData(52.5200, 13.4050, "JO62")]
    // South and east — Sydney.
    [InlineData(-33.8688, 151.2093, "QF56")]
    // South and west — Buenos Aires.
    [InlineData(-34.6037, -58.3816, "GF05")]
    // Just south of the equator, east of Greenwich — Nairobi.
    [InlineData(-1.2921, 36.8219, "KI88")]
    public void KnownCoordinatesGiveKnownGrids(double lat, double lon, string expectedSquare)
    {
        var grid = OperatorLocation.ToGrid(new LatLon(lat, lon));

        // The four-character square is the published fact for each of these
        // places. The subsquare depends on exactly which coordinate inside the
        // city was used, so asserting one here would be asserting the input
        // rather than the conversion — the six-character check that has an
        // independent source is the KC3QIS case below.
        Assert.Equal(expectedSquare, grid[..4]);
        Assert.Equal(6, grid.Length);
    }

    /// <remarks>
    /// The one six-character assertion with an outside source: callook.info
    /// computes "FN00dj" for these coordinates by its own route, so Hamlet's
    /// arithmetic is agreeing with somebody else's rather than with itself.
    /// The captured response is in <c>LookupAndGuardTests</c>.
    /// </remarks>
    [Fact]
    public void TheOperatorsOwnGridAgreesWithCallook()
        => Assert.Equal(
            "FN00DJ",
            OperatorLocation.ToGrid(new LatLon(40.3782746, -79.7081649)));

    /// <remarks>
    /// Proves the two conversions are each other's inverse to within the size
    /// of a subsquare. A locator names a box about three miles by four, so a
    /// round trip lands inside it rather than back on the exact point — and if
    /// it did not, one of the two directions would be wrong.
    /// </remarks>
    [Theory]
    [InlineData(40.3782746, -79.7081649)]
    [InlineData(52.5200, 13.4050)]
    [InlineData(-33.8688, 151.2093)]
    [InlineData(-34.6037, -58.3816)]
    [InlineData(0.0, 0.0)]
    [InlineData(64.1466, -21.9426)]
    public void GridAndCoordinatesRoundTrip(double lat, double lon)
    {
        var grid = OperatorLocation.ToGrid(new LatLon(lat, lon));
        var back = OperatorLocation.FromGrid(grid);

        Assert.NotNull(back);

        // Half a subsquare is 1/48° of latitude and 1/24° of longitude.
        Assert.True(
            Math.Abs(back!.Value.Latitude - lat) <= 1.0 / 48.0 + 0.001,
            $"{grid}: latitude came back {back.Value.Latitude} from {lat}");
        Assert.True(
            Math.Abs(back.Value.Longitude - lon) <= 1.0 / 24.0 + 0.001,
            $"{grid}: longitude came back {back.Value.Longitude} from {lon}");
    }

    /// <remarks>
    /// Proves the corners of the world convert without running off the end of
    /// the alphabet. The antimeridian and the poles are where an off-by-one in
    /// the field letter shows up, and nowhere else.
    /// </remarks>
    [Theory]
    [InlineData(-90.0, -180.0)]
    [InlineData(89.999, 179.999)]
    [InlineData(0.0, 179.999)]
    [InlineData(-89.999, 0.0)]
    [InlineData(90.0, 180.0)]
    public void TheEdgesOfTheWorldStillConvert(double lat, double lon)
    {
        var grid = OperatorLocation.ToGrid(new LatLon(lat, lon));

        Assert.Equal(6, grid.Length);
        Assert.InRange(grid[0], 'A', 'R');
        Assert.InRange(grid[1], 'A', 'R');
        Assert.True(char.IsAsciiDigit(grid[2]) && char.IsAsciiDigit(grid[3]));
        Assert.InRange(grid[4], 'A', 'X');
        Assert.InRange(grid[5], 'A', 'X');
        Assert.NotNull(OperatorLocation.FromGrid(grid));
    }

    private static readonly LatLon Pittsburgh = new(40.4406, -79.9959);
    private static readonly LatLon NewYork = new(40.7128, -74.0060);
    private static readonly LatLon London = new(51.5074, -0.1278);
    private static readonly LatLon Berlin = new(52.5200, 13.4050);
    private static readonly LatLon Sydney = new(-33.8688, 151.2093);
    private static readonly LatLon Miami = new(25.7617, -80.1918);
    private static readonly LatLon LosAngeles = new(34.0522, -118.2437);

    /// <summary>
    /// Great-circle distances between city centers as published, in
    /// kilometers.
    /// </summary>
    /// <remarks>
    /// City-to-city rather than grid-to-grid, because a locator resolves to
    /// the center of a square that the city is only somewhere inside — those
    /// few miles of slack would make it unclear whether a failure was in the
    /// distance or in the locator. Tolerance is two percent, which covers the
    /// difference between a spherical earth and the real one.
    /// </remarks>
    public static TheoryData<string, double, double, double, double, double> Journeys()
        => new()
        {
            // Pittsburgh to New York: 315 miles.
            { "Pittsburgh–New York", 40.4406, -79.9959, 40.7128, -74.0060, 507 },
            // New York to London: 3,461 miles.
            { "New York–London", 40.7128, -74.0060, 51.5074, -0.1278, 5570 },
            // London to Berlin: 579 miles.
            { "London–Berlin", 51.5074, -0.1278, 52.5200, 13.4050, 932 },
            // Sydney to London: 10,562 miles, most of the way round.
            { "Sydney–London", -33.8688, 151.2093, 51.5074, -0.1278, 16_993 },
        };

    /// <remarks>
    /// Proves distances match published figures across a continent, an ocean
    /// and nearly the whole planet.
    /// </remarks>
    [Theory]
    [MemberData(nameof(Journeys))]
    public void DistancesMatchPublishedFigures(
        string what, double lat1, double lon1, double lat2, double lon2, double expectedKm)
    {
        var km = OperatorLocation.DistanceKm(
            new LatLon(lat1, lon1), new LatLon(lat2, lon2));

        Assert.True(
            Math.Abs(km - expectedKm) / expectedKm < 0.02,
            $"{what}: computed {km:0} km against a published {expectedKm:0}");
    }

    /// <remarks>
    /// Proves the bearing points the right way from Pennsylvania. London being
    /// northeast is the one people find surprising, and it is right: the
    /// great-circle path leaves the eastern seaboard heading up over
    /// Newfoundland rather than straight across the Atlantic.
    /// </remarks>
    [Theory]
    [InlineData("New York", "east")]
    [InlineData("Miami", "south")]
    [InlineData("Los Angeles", "west")]
    [InlineData("London", "northeast")]
    // Westward across the Pacific is the short way to Sydney from here, which
    // is not the direction most people would point.
    [InlineData("Sydney", "west")]
    public void BearingPointsTheRightWay(string place, string expected)
    {
        var there = place switch
        {
            "New York" => NewYork,
            "Miami" => Miami,
            "Los Angeles" => LosAngeles,
            "London" => London,
            _ => Sydney,
        };

        var compass = OperatorLocation.DescribeCompass(
            OperatorLocation.BearingDegrees(Pittsburgh, there));

        Assert.Equal(expected, compass);
    }

    /// <remarks>
    /// Proves the bearing is the one at the near end. A great circle changes
    /// heading along its length, so the way back is not simply the reverse —
    /// and a function that returned the reverse would pass every short-hop
    /// check and fail here.
    /// </remarks>
    [Fact]
    public void BearingIsTakenAtTheNearEnd()
    {
        var out_ = OperatorLocation.BearingDegrees(Pittsburgh, London);
        var back = OperatorLocation.BearingDegrees(London, Pittsburgh);

        Assert.InRange(out_, 40, 60);

        // Pittsburgh lies west-northwest from London, not southwest.
        Assert.InRange(back, 275, 300);
    }

    /// <remarks>
    /// Proves the sixteen compass points land where they should, including the
    /// wrap at north where 350° must not become "north-northwest of nothing".
    /// </remarks>
    [Theory]
    [InlineData(0, "north")]
    [InlineData(359, "north")]
    [InlineData(45, "northeast")]
    [InlineData(90, "east")]
    [InlineData(180, "south")]
    [InlineData(270, "west")]
    [InlineData(337.5, "north-northwest")]
    [InlineData(-90, "west")]
    [InlineData(450, "east")]
    public void CompassPointsAreNamedCorrectly(double degrees, string expected)
        => Assert.Equal(expected, OperatorLocation.DescribeCompass(degrees));

    /// <remarks>
    /// Proves distances are spoken at a precision the figure deserves. This is
    /// a distance to a park's stated reference point, so "483 miles" would
    /// claim accuracy nothing in the chain supports (§0.0).
    /// </remarks>
    [Theory]
    [InlineData(800.0, true, "500 miles")]
    [InlineData(800.0, false, "800 km")]
    [InlineData(50.0, true, "30 miles")]
    [InlineData(5.0, true, "3 miles")]
    [InlineData(1.4, true, "1 mile")]
    [InlineData(0.5, true, "under a mile")]
    [InlineData(16000.0, true, "9950 miles")]
    public void RangesAreRoundedToWhatTheyDeserve(double km, bool miles, string expected)
        => Assert.Equal(expected, OperatorLocation.DescribeRange(km, miles));

    /// <remarks>
    /// Proves miles and kilometers convert by the statute-mile definition, not
    /// by a remembered approximation.
    /// </remarks>
    [Fact]
    public void MilesUseTheStatuteDefinition()
    {
        Assert.Equal(1.0, OperatorLocation.ToMiles(1.609344), 9);
        Assert.Equal(100.0, OperatorLocation.ToMiles(160.9344), 6);
    }

    /// <remarks>
    /// Proves the conversion is pure: the same point always gives the same
    /// locator, with no clock and no state involved (§5).
    /// </remarks>
    [Fact]
    public void ConversionIsDeterministic()
    {
        var point = new LatLon(40.3782746, -79.7081649);

        Assert.Equal(OperatorLocation.ToGrid(point), OperatorLocation.ToGrid(point));
        Assert.Equal(
            OperatorLocation.BearingDegrees(point, new LatLon(51.5, -0.13)),
            OperatorLocation.BearingDegrees(point, new LatLon(51.5, -0.13)));
    }
}

using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Reading a callsign's origin, and reading the operator's own location — the
/// two inputs that decide whether a spot is plausibly relevant (HM-DEC-024).
/// </summary>
public sealed class CallsignRegionTests
{
    /// <remarks>
    /// Proves US calls are placed in their district and non-US prefixes are
    /// not swept into one. "AA" through "AL" are US; "AM" onwards are not,
    /// which is the boundary a naive first-letter test gets wrong.
    /// </remarks>
    [Theory]
    [InlineData("KC3QIS", CallsignRegion.UnitedStates, 3)]
    [InlineData("W1ABC", CallsignRegion.UnitedStates, 1)]
    [InlineData("N0BAD", CallsignRegion.UnitedStates, 0)]
    [InlineData("AA9XYZ", CallsignRegion.UnitedStates, 9)]
    [InlineData("AL7ABC", CallsignRegion.UnitedStates, 7)]
    [InlineData("VE3EEE", CallsignRegion.Canada, null)]
    [InlineData("DL8LAS", CallsignRegion.Elsewhere, null)]
    [InlineData("OE3WHU", CallsignRegion.Elsewhere, null)]
    [InlineData("JA1XYZ", CallsignRegion.Elsewhere, null)]
    public void Classify_PlacesCallsigns(
        string call, CallsignRegion region, int? district)
    {
        var origin = CallsignRegions.Classify(call);

        Assert.Equal(region, origin.Region);
        Assert.Equal(district, origin.UsDistrict);
    }

    /// <remarks>
    /// Proves the decorations real feeds carry are stripped: RBN appends "-#"
    /// and "-3-#" to skimmer calls, and operators append "/P" and "/QRP".
    /// Left in place, every skimmer would be unclassifiable.
    /// </remarks>
    [Theory]
    [InlineData("WE9V-#", CallsignRegion.UnitedStates, 9)]
    [InlineData("DL8LAS-3-#", CallsignRegion.Elsewhere, null)]
    [InlineData("W3ABC/P", CallsignRegion.UnitedStates, 3)]
    [InlineData("OE3WHU/QRP", CallsignRegion.Elsewhere, null)]
    public void Classify_StripsFeedDecorations(
        string call, CallsignRegion region, int? district)
    {
        var origin = CallsignRegions.Classify(call);

        Assert.Equal(region, origin.Region);
        Assert.Equal(district, origin.UsDistrict);
    }

    /// <remarks>
    /// Proves the portable forms are read the right way round. In "EA8/DF4UE"
    /// the leading prefix is where the operator actually is, and in "W3ABC/8"
    /// the trailing digit overrides the home district.
    /// </remarks>
    [Fact]
    public void Classify_ReadsPortablePrefixesAndDistrictOverrides()
    {
        Assert.Equal(
            CallsignRegion.Elsewhere, CallsignRegions.Classify("EA8/DF4UE").Region);
        Assert.Equal(8, CallsignRegions.Classify("W3ABC/8").UsDistrict);
    }

    /// <remarks>
    /// Proves unparseable input stays Unknown instead of being bucketed
    /// somewhere convenient. A filter that quietly treats junk as "distant"
    /// is making a claim it cannot support (HM-DEC-009).
    /// </remarks>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("NOTACALL")]
    [InlineData("!!")]
    public void Classify_RefusesToGuess(string? call)
        => Assert.Equal(CallsignRegion.Unknown, CallsignRegions.Classify(call).Region);

    /// <remarks>
    /// Proves proximity from Pittsburgh: district 3's own and neighboring
    /// districts are Local, other US and Canadian skimmers are Continent, and
    /// Europe is Distant.
    /// </remarks>
    [Theory]
    [InlineData("K3GMQ-#", SpotProximity.Local)]
    [InlineData("W8XYZ-#", SpotProximity.Local)]
    [InlineData("K2AAA-#", SpotProximity.Local)]
    [InlineData("W6CAL-#", SpotProximity.Continent)]
    [InlineData("VE3EEE-#", SpotProximity.Continent)]
    [InlineData("DL8LAS-#", SpotProximity.Distant)]
    public void ProximityTo_MeasuresFromPittsburgh(string spotter, SpotProximity expected)
        => Assert.Equal(expected, CallsignRegions.ProximityTo(spotter, homeDistrict: 3));

    /// <remarks>
    /// Proves the documented fallback: with no home district, US and Canadian
    /// skimmers are still Continent, so continent-grain filtering works
    /// without knowing where the operator lives.
    /// </remarks>
    [Fact]
    public void ProximityTo_FallsBackToContinentWithNoHomeDistrict()
    {
        Assert.Equal(
            SpotProximity.Continent, CallsignRegions.ProximityTo("K3GMQ-#", null));
        Assert.Equal(
            SpotProximity.Distant, CallsignRegions.ProximityTo("DL8LAS-#", null));
    }

    /// <remarks>
    /// Proves the operator's district is read from a state, which is an exact
    /// published mapping — "Pittsburgh, PA" and "Pittsburgh, Pennsylvania"
    /// both give district 3.
    /// </remarks>
    [Theory]
    [InlineData("Pittsburgh, PA", 3)]
    [InlineData("Pittsburgh, Pennsylvania", 3)]
    [InlineData("Boston, MA", 1)]
    [InlineData("Columbus OH", 8)]
    [InlineData("Denver, CO", 0)]
    public void HomeDistrict_ReadsAState(string location, int expected)
        => Assert.Equal(expected, OperatorLocation.HomeDistrict(location));

    /// <remarks>
    /// Proves the honest degradation of HM-DEC-024: a location with no US
    /// state yields null rather than a guess, and the caller drops to
    /// continent-grain filtering.
    /// </remarks>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Berlin, Germany")]
    [InlineData("somewhere nice")]
    public void HomeDistrict_ReturnsNullRatherThanGuessing(string? location)
        => Assert.Null(OperatorLocation.HomeDistrict(location));

    /// <remarks>
    /// Proves the Maidenhead conversion is real arithmetic: EN90 and FN00 are
    /// the squares over western and central Pennsylvania, and both land in
    /// North America.
    /// </remarks>
    [Fact]
    public void Grid_ConvertsAndPlacesTheOperator()
    {
        var en90 = OperatorLocation.FromGrid("EN90");

        Assert.NotNull(en90);
        Assert.InRange(en90!.Value.Latitude, 40.0, 41.0);
        Assert.InRange(en90.Value.Longitude, -81.0, -79.0);

        Assert.True(OperatorLocation.IsNorthAmerica("EN90"));
        Assert.True(OperatorLocation.IsNorthAmerica("FN00"));
        Assert.False(OperatorLocation.IsNorthAmerica("JO62"));
        Assert.False(OperatorLocation.IsNorthAmerica("nonsense"));
    }
}

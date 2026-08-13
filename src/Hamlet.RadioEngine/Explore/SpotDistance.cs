namespace Hamlet.RadioEngine.Explore;

/// <summary>Which units a distance is spoken in.</summary>
public enum DistanceUnits
{
    /// <summary>Statute miles. The default, for a US operator.</summary>
    Miles,

    /// <summary>Kilometers.</summary>
    Kilometers,
}

/// <summary>
/// How far away a spot is, and roughly which way — where that can be said at
/// all.
/// </summary>
/// <remarks>
/// <para>WHY THIS IS WORTH BUILDING (HM-DEC-038). A newcomer has no sense of
/// what distances are plausible on which bands, and no way to acquire one:
/// they see a callsign and a frequency, work out nothing from either, and the
/// intuition every experienced operator has and none of them can explain stays
/// out of reach. After a few dozen spots that say "480 miles northeast" beside
/// "40 m", the shape of it starts to arrive on its own. That is the whole
/// feature — it is a teaching device that happens to look like a label.</para>
/// <para>WHAT IT WILL NOT DO. It answers only from two known positions: the
/// operator's coordinates and the station's, both stated rather than inferred.
/// No grid means no distance anywhere, on any card or any dot — not an
/// estimate from the location string, not a country-sized guess from a
/// callsign prefix. A number that cannot be justified is not shown, and the
/// absence is the honest answer (§0.0).</para>
/// <para>Pure: two positions and a unit in, a phrase out. No clock, no
/// network, nothing to mock.</para>
/// </remarks>
public static class SpotDistance
{
    /// <summary>
    /// Describe how far away a spot is and roughly which way, or "" when the
    /// app cannot justify a figure.
    /// </summary>
    /// <param name="here">The operator's position, or null when unknown.</param>
    /// <param name="spot">The spot.</param>
    /// <param name="units">Miles or kilometers.</param>
    /// <returns>e.g. "480 miles northeast", or "".</returns>
    public static string Describe(LatLon? here, ActivitySpot spot, DistanceUnits units)
        => Describe(here, spot.StationLocation, units);

    /// <summary>
    /// Describe the distance and bearing between two positions.
    /// </summary>
    /// <param name="here">The operator's position, or null.</param>
    /// <param name="there">The station's position, or null.</param>
    /// <param name="units">Miles or kilometers.</param>
    /// <returns>e.g. "480 miles northeast", or "".</returns>
    public static string Describe(LatLon? here, LatLon? there, DistanceUnits units)
    {
        if (here is not { } from || there is not { } to)
        {
            return "";
        }

        var km = OperatorLocation.DistanceKm(from, to);
        var range = OperatorLocation.DescribeRange(km, units == DistanceUnits.Miles);

        // Close enough that a compass point says nothing useful: at five miles
        // the bearing is about which end of town, not about propagation.
        if (km < 8)
        {
            return range + " away";
        }

        var compass = OperatorLocation.DescribeCompass(
            OperatorLocation.BearingDegrees(from, to));

        return $"{range} {compass}";
    }

    /// <summary>
    /// True when this spot could carry a distance for an operator whose
    /// position is known.
    /// </summary>
    /// <param name="spot">The spot.</param>
    /// <returns>True when the source stated where the station is.</returns>
    public static bool CanDescribe(ActivitySpot spot) => spot.StationLocation is not null;
}

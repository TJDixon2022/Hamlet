using System;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// **Where a station lands on an azimuthal equidistant map centred on the
/// operator's own station.**
/// </summary>
/// <remarks>
/// <para>**THIS IS THE PROJECTION AND NOT A PICTURE** (work instruction 301 task 3,
/// Tim's ruling of 2026-09-09: *take that and scale it and make use of it*). Hamlet
/// draws no coastline, no graticule and no landmass. It is handed a map that already
/// has all of that on it and works out where on it a dot belongs.</para>
/// <para>**WHY THIS PROJECTION AND NOT THE FLAT ONE IT REPLACES.** On an azimuthal
/// equidistant map centred where you are, **every straight line out of the centre is
/// a great circle and its length is the true distance.** That is the path a signal
/// actually takes. Unit 299's globe used a flat rectangular projection and had to
/// carry a sentence apologising that the line drawn was not the path the signal
/// took; **on this projection no such sentence is needed and none should be
/// written.**</para>
/// <para>**THE §0.0 EXPOSURE HERE IS A DOT IN THE WRONG PLACE**, which nobody would
/// ever check, because a picture is read without scepticism and there is no wording
/// to object to (HM-DEC-092). So the arithmetic below is the published one, written
/// out rather than fitted, and `AzimuthalMapTests` checks it against points whose
/// answers are known independently: the centre, the antipode, and bearings and
/// distances cross-checked against <see cref="GridPath"/>, which shares no line of
/// code with this file.</para>
/// <para>**A STATION WITH NO GRID GETS NO DOT.** There is no default centre, no
/// assumed hemisphere and no guessed longitude; <see cref="Place"/> returns null and
/// the caller says Hamlet does not know where he is.</para>
/// </remarks>
public static class AzimuthalMap
{
    /// <summary>
    /// **The three numbers that describe the shipped map image, and nothing else.**
    /// </summary>
    /// <remarks>
    /// <para>**THESE DESCRIBE ONE PARTICULAR PICTURE AND MUST CHANGE WITH IT.** They
    /// are the centre the map was drawn about and how many pixels of it equal a
    /// hundred and eighty degrees of arc. **Replace the image and all three are
    /// wrong**, silently, with every dot landing somewhere plausible and untrue.
    /// </para>
    /// <para>**THEY ARE IN ONE PLACE ON PURPOSE** (§0). A second copy of a number
    /// like this drifts, and the drift is invisible.</para>
    /// </remarks>
    /// <param name="CentreLatitude">The latitude the image is centred on, degrees.</param>
    /// <param name="CentreLongitude">The longitude the image is centred on, degrees.</param>
    /// <param name="RimRadiusPixels">
    /// How many pixels from the image's centre to its outer rim, which is the
    /// antipode and 180 degrees of arc.
    /// </param>
    public sealed record Settings(
        double CentreLatitude,
        double CentreLongitude,
        double RimRadiusPixels);

    /// <summary>Half a turn, in radians.</summary>
    private const double Half = Math.PI;

    /// <summary>
    /// Where a station belongs on the image, in pixels from its centre.
    /// </summary>
    /// <param name="settings">The numbers describing the image.</param>
    /// <param name="latitude">The station's latitude, degrees north.</param>
    /// <param name="longitude">The station's longitude, degrees east.</param>
    /// <returns>
    /// The offset from the image's centre, x to the right and y downward, in the
    /// image's own pixels.
    /// </returns>
    /// <exception cref="ArgumentNullException">There are no settings.</exception>
    /// <remarks>
    /// **THE PUBLISHED FORM, WRITTEN OUT.** With the centre at `p0`, `l0` and the
    /// station at `p`, `l`:
    /// <code>
    /// cos c = sin p0 sin p + cos p0 cos p cos(l - l0)
    /// k     = c / sin c
    /// x     = k cos p sin(l - l0)
    /// y     = k (cos p0 sin p - sin p0 cos p cos(l - l0))
    /// </code>
    /// `c` is the angular distance from the centre, so `x` and `y` come out in
    /// radians of arc and are scaled by the rim radius over pi. **`y` is negated on
    /// the way out** because the formula's `y` grows northward and a screen's grows
    /// downward.
    /// <para>**IT IS COMPUTED IN AN EQUIVALENT FORM, AND THE REASON IS A MEASURED
    /// DEFECT IN THE ONE ABOVE.** Written literally, `k = c / sin c` divides by a
    /// number that goes to zero at the antipode, and there the position comes out as
    /// a vanishing quantity times an enormous one. **Measured: the exact antipode of
    /// the centre landed off the rim by more than twelve miles' worth of pixels**,
    /// against every real station agreeing to the mile.</para>
    /// <para>**THE FORM USED INSTEAD IS THE SAME EQUATION AND NOT AN
    /// APPROXIMATION.** `x` and `y` above are `k` times two quantities whose
    /// magnitude is exactly `sin c`, so their ratio is the initial bearing and their
    /// length is exactly `c`: putting the dot at distance `c` on that bearing gives
    /// the identical answer with nothing dividing by nothing. `c` itself comes from
    /// the haversine, which is the stable way to get a small or a near-half-turn
    /// arc.</para>
    /// </remarks>
    public static (double X, double Y) Place(
        Settings settings, double latitude, double longitude)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var p0 = Radians(settings.CentreLatitude);
        var p = Radians(latitude);
        var dl = Radians(longitude - settings.CentreLongitude);

        // **HOW FAR, BY THE HAVERSINE**, which stays accurate both for a station
        // next door and for one on the far side of the world.
        var sinHalfLat = Math.Sin((p - p0) / 2.0);
        var sinHalfLon = Math.Sin(dl / 2.0);

        var a = (sinHalfLat * sinHalfLat)
            + (Math.Cos(p0) * Math.Cos(p) * sinHalfLon * sinHalfLon);

        var c = 2.0 * Math.Asin(Math.Min(1.0, Math.Sqrt(a)));

        if (c <= double.Epsilon)
        {
            return (0, 0);
        }

        // **WHICH WAY ROUND**, which is the ratio of the same two quantities the
        // published `x` and `y` are built from. At the antipode every direction is
        // equally correct and the arithmetic settles on one; **that is a real
        // property of the projection rather than a defect**, since the whole rim is
        // the same point.
        var east = Math.Cos(p) * Math.Sin(dl);
        var north = (Math.Cos(p0) * Math.Sin(p))
            - (Math.Sin(p0) * Math.Cos(p) * Math.Cos(dl));

        var radius = c * settings.RimRadiusPixels / Half;
        var bearing = Math.Atan2(east, north);

        // **Y IS FLIPPED FOR THE SCREEN**, and that is the one place a sign error
        // would put every dot in the wrong hemisphere while still looking sensible.
        return (radius * Math.Sin(bearing), -radius * Math.Cos(bearing));
    }

    /// <summary>How far from the image's centre a station lands, in pixels.</summary>
    /// <param name="settings">The numbers describing the image.</param>
    /// <param name="latitude">The station's latitude, degrees north.</param>
    /// <param name="longitude">The station's longitude, degrees east.</param>
    /// <returns>The distance from the centre, in the image's own pixels.</returns>
    public static double FromCentre(
        Settings settings, double latitude, double longitude)
    {
        var (x, y) = Place(settings, latitude, longitude);

        return Math.Sqrt((x * x) + (y * y));
    }

    private static double Radians(double degrees) => degrees * Math.PI / 180.0;
}

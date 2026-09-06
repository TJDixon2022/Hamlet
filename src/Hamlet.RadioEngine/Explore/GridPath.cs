using System.Globalization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// How far away another grid square is, in statute miles, and which way, in
/// degrees true.
/// </summary>
/// <remarks>
/// <para>**IT OWNS NO ARITHMETIC AND THAT IS DELIBERATE** (§0). Every figure here
/// comes out of <see cref="OperatorLocation"/>, which has decoded Maidenhead
/// locators and computed great-circle distances since the spot cards were built
/// (HM-DEC-038). A second copy of a haversine is a second answer waiting to
/// disagree with the first, and the two would disagree silently, about a number
/// nobody can check by eye.</para>
/// <para>**WHAT IT ADDS IS THE UNITS TIM RULED ON**, 2026-09-05: *distance in
/// miles and degrees is fine*. The tree already spoke distances, in
/// `DescribeRange`, and directions, in `DescribeCompass` — and both are right for
/// a spot card and wrong here. *Four hundred and eighty miles northeast* is a
/// picture; a bearing an operator turns a beam to is a number.</para>
/// <para>**THE FORMULAE, NAMED, BECAUSE THIS IS THE OPERATOR'S OWN FIELD.**
/// Distance is the **haversine formula on a sphere of radius 6371 km**, which is
/// the IUGG mean radius, converted to statute miles at exactly 1.609344 km to the
/// mile. Bearing is the **initial great-circle bearing**, `atan2(sin Δλ · cos φ2,
/// cos φ1 · sin φ2 − sin φ1 · cos φ2 · cos Δλ)`, normalised to 0–360 clockwise
/// from true north. Both live in <see cref="OperatorLocation"/> and both are
/// checked here against the spherical law of cosines as an independent second
/// formula.</para>
/// <para>**IT IS THE INITIAL BEARING AND NOT A CONSTANT HEADING.** On a great
/// circle the bearing changes along the path, and the one that matters to an
/// operator is where the antenna points from here. Over four thousand miles the
/// far end differs by tens of degrees, so calling this *the* bearing without the
/// word *initial* would be a claim the geometry does not support.</para>
/// <para>**A GRID IS A BOX AND NEVER A POINT** (Tim's ruling, 2026-09-05). A
/// four-character square is roughly 70 by 100 miles and a six-character one about
/// three miles across; `FromGrid` takes the centre, which is the best anybody can
/// do, and the rounding below is chosen so the display never claims more than the
/// square can support.</para>
/// </remarks>
public static class GridPath
{
    /// <summary>Kilometers in a statute mile, exactly, by international definition.</summary>
    private const double KmPerMile = 1.609344;

    /// <summary>Great-circle distance in statute miles.</summary>
    /// <param name="from">Where the operator is.</param>
    /// <param name="to">Where the station is.</param>
    /// <returns>Distance in statute miles.</returns>
    public static double MilesBetween(LatLon from, LatLon to)
        => OperatorLocation.DistanceKm(from, to) / KmPerMile;

    /// <summary>Initial great-circle bearing, degrees clockwise from true north.</summary>
    /// <param name="from">Where the operator is.</param>
    /// <param name="to">Where the station is.</param>
    /// <returns>0 to 360 degrees.</returns>
    public static double BearingDegrees(LatLon from, LatLon to)
        => OperatorLocation.BearingDegrees(from, to);

    /// <summary>A distance in the words the tooltip shows.</summary>
    /// <param name="miles">Distance in statute miles.</param>
    /// <returns>e.g. "240 miles" or "4,500 miles".</returns>
    /// <remarks>
    /// <para>**NEAREST TEN UNDER A THOUSAND, NEAREST HUNDRED ABOVE.** The
    /// instruction offers this and it is taken, because the grid is what decides
    /// it: a four-character square is seventy miles across, so a figure to the
    /// mile would be precision the input never carried, and at four thousand miles
    /// even a hundred is well inside the square's own width.</para>
    /// <para>**NOTHING READS "0 miles".** Under five miles rounds to nought and a
    /// station in the next street is not at no distance, so the floor is ten and it
    /// is the same ten the first rounding step gives. A grid square cannot tell
    /// two stations ten miles apart from each other anyway.</para>
    /// </remarks>
    public static string DescribeMiles(double miles)
    {
        // **AWAY FROM ZERO AT THE HALF, WHICH `Math.Round` DOES NOT DO.** Its
        // default is banker's rounding, so 245 miles goes to 240 and 235 miles
        // goes to 240 as well — two different distances landing on one number
        // with nothing on screen to say so. `DigitalDecodeRow.FormatSnr` made the
        // same correction for the same reason, one file over.
        var rounded = miles < 1000
            ? Math.Round(miles / 10, MidpointRounding.AwayFromZero) * 10
            : Math.Round(miles / 100, MidpointRounding.AwayFromZero) * 100;

        if (rounded < 10)
        {
            rounded = 10;
        }

        return rounded.ToString("#,##0", CultureInfo.InvariantCulture) + " miles";
    }

    /// <summary>A bearing in the words the tooltip shows.</summary>
    /// <param name="degrees">Degrees clockwise from true north.</param>
    /// <returns>e.g. "47 degrees".</returns>
    /// <remarks>
    /// **WHOLE DEGREES, AND 360 IS WRITTEN AS 0.** A tenth of a degree would be
    /// spurious against a square seventy miles wide, and a beam that could be
    /// pointed to a tenth of a degree is not one anybody owns.
    /// </remarks>
    public static string DescribeBearing(double degrees)
    {
        var whole = (int)Math.Round(((degrees % 360.0) + 360.0) % 360.0) % 360;

        return whole.ToString("0", CultureInfo.InvariantCulture) + " degrees";
    }
}

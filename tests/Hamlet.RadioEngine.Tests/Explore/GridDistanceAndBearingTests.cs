using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Work instruction 252, task 1: how far away a grid square is, in statute
/// miles, and which way, in degrees true.
/// </summary>
/// <remarks>
/// <para>**THE ARITHMETIC WAS ALREADY IN THE TREE AND IS NOT REBUILT.**
/// `OperatorLocation.FromGrid` has decoded four and six character Maidenhead
/// locators to the centre of the square since the spot cards were built
/// (HM-DEC-038), `DistanceKm` is the haversine formula at an earth radius of
/// 6371 km, and `BearingDegrees` is the initial great-circle bearing. A second
/// copy of any of those would be a second answer waiting to disagree (§0).</para>
/// <para>**WHAT IS NEW IS THE UNITS THE RULING NAMES.** Tim's ruling of
/// 2026-09-05 is *distance in miles and degrees*, and what the tree had was
/// `DescribeRange`, which speaks in words, and `DescribeCompass`, which gives
/// sixteen named points. Those are right for a spot card and wrong for this: a
/// bearing an operator turns a beam to is a number.</para>
/// <para>**THE HAND-CHECKED PAIRS BELOW ARE THE POINT OF THE TASK.** Great-circle
/// code that is wrong is wrong plausibly — it returns a number of about the right
/// size, and nothing on screen says otherwise. Each row was worked out from the
/// square centres `FromGrid` produces and checked against the spherical law of
/// cosines as an independent second formula.</para>
/// </remarks>
public sealed class GridDistanceAndBearingTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the computed pairs are printed.</param>
    public GridDistanceAndBearingTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>Known pairs, hand-checked, across three very different hops.</summary>
    /// <remarks>
    /// <para>**FN00DJ IS THE OPERATOR'S OWN SQUARE**, which is what makes these
    /// the distances he can check against his own logbook rather than against
    /// this test. It decodes to **40.40°N, 79.71°W** — western Pennsylvania.</para>
    /// <para>**THE FIRST DRAFT OF THIS TABLE WAS WRONG, AND HOW IT WAS WRONG IS
    /// WORTH KEEPING.** It carried 240 miles for FN31, 8,060 for OF88, and called
    /// OF88 Johannesburg. All three came from memory rather than arithmetic, which
    /// is the exact failure §12.5 describes: had the implementation been wrong in
    /// the same direction, the pair would have agreed and proved nothing. The rows
    /// below are re-derived from the square centres `FromGrid` actually produces,
    /// each one checked by a flat-earth approximation over the same degrees before
    /// being compared with the code:</para>
    /// <para>FN31 is 41.5°N, 73.0°W. From FN00DJ that is 6.71° of longitude at
    /// about 41° north — 6.71 × 69 × cos 41 ≈ 349 miles east — and 1.10° of
    /// latitude, ≈ 76 miles north. The hypotenuse is ≈ 357 miles on a bearing of
    /// atan(349 / 76) ≈ 78°, and the sphere gives 358.2 miles at 75.5°.</para>
    /// <para>OF88 is 31.5°S, 117.0°E, which is **Perth and not Johannesburg** —
    /// naming it wrongly is what made 8,060 look plausible. Perth is nearly
    /// antipodal to Pennsylvania, so the short great circle runs northwest across
    /// the pole rather than east, and 11,321 miles against a maximum possible
    /// 12,430 is the shape that says so. A bearing of 298° is northwest, which is
    /// the check that the path is the polar one.</para>
    /// <para>The tolerances are in miles and generous on purpose: what is pinned
    /// is that the formula and the units are right, not the fourth significant
    /// figure of a square three miles across.</para>
    /// </remarks>
    [Theory]
    // A short hop: western Pennsylvania to FN31, southern New England.
    [InlineData("FN00DJ", "FN31", 358.0, 12.0, 75.5, 4.0)]
    // The long one the instruction asks for: FN00DJ to JN86, Hungary, about four
    // and a half thousand miles and well north of due east.
    [InlineData("FN00DJ", "JN86", 4551.0, 60.0, 48.5, 4.0)]
    // Nearly antipodal: FN00DJ to OF88, Perth, where the short path goes
    // northwest over the pole rather than east.
    [InlineData("FN00DJ", "OF88", 11321.0, 120.0, 298.0, 5.0)]
    public void KnownPairsComeOutWhereTheyShould(
        string here, string there,
        double miles, double milesTolerance,
        double bearing, double bearingTolerance)
    {
        var from = OperatorLocation.FromGrid(here);
        var to = OperatorLocation.FromGrid(there);

        Assert.NotNull(from);
        Assert.NotNull(to);

        var gotMiles = GridPath.MilesBetween(from!.Value, to!.Value);
        var gotBearing = GridPath.BearingDegrees(from.Value, to.Value);

        _output.WriteLine(
            here + " -> " + there + " : "
            + gotMiles.ToString("0.0") + " miles, "
            + gotBearing.ToString("0.0") + " degrees true");

        Assert.InRange(gotMiles, miles - milesTolerance, miles + milesTolerance);
        Assert.InRange(
            gotBearing, bearing - bearingTolerance, bearing + bearingTolerance);
    }

    /// <summary>
    /// A four-character grid and a six-character grid of the same place agree to
    /// within the coarser square.
    /// </summary>
    /// <remarks>
    /// <para>**A FOUR-CHARACTER SQUARE IS 2° OF LONGITUDE BY 1° OF LATITUDE** —
    /// about 100 by 70 miles in the mid latitudes — so a point inside it can be
    /// half a diagonal from its centre, which is about 70 miles. That is the
    /// tolerance, and it is the instruction's own *within the coarser square's
    /// size*.</para>
    /// <para>**THE FINE FORMS BELOW ARE DELIBERATELY CORNERS.** `AA` is the
    /// southwest subsquare and `XX` the northeast, so each row is the worst case
    /// rather than a comfortable one — a centre-ish subsquare would pass a
    /// tolerance twice too tight and say nothing.</para>
    /// <para>**AND THIS IS WHY A GRID IS NEVER A PLACE NAME** (Tim's ruling). The
    /// disagreement below is not error, it is the width of what the operator
    /// actually told us: he is somewhere in that box and the app does not know
    /// where.</para>
    /// </remarks>
    [Theory]
    [InlineData("FN00", "FN00DJ")]
    [InlineData("JN86", "JN86AA")]
    [InlineData("OF88", "OF88XX")]
    public void TheCoarseAndFineFormsOfOnePlaceAgree(string coarse, string fine)
    {
        var here = OperatorLocation.FromGrid("FN00DJ")!.Value;

        var toCoarse = GridPath.MilesBetween(
            here, OperatorLocation.FromGrid(coarse)!.Value);
        var toFine = GridPath.MilesBetween(
            here, OperatorLocation.FromGrid(fine)!.Value);

        _output.WriteLine(
            coarse + " " + toCoarse.ToString("0.0") + " miles vs "
            + fine + " " + toFine.ToString("0.0") + " miles, apart by "
            + Math.Abs(toCoarse - toFine).ToString("0.0"));

        Assert.InRange(Math.Abs(toCoarse - toFine), 0.0, 70.0);
    }

    /// <summary>
    /// The haversine distance agrees with an independently written second
    /// formula.
    /// </summary>
    /// <remarks>
    /// **A SECOND FORMULA IS THE ONLY REAL CHECK ON THE FIRST** (§12.5). The
    /// spherical law of cosines is a different expression of the same geometry
    /// and shares none of haversine's algebra, so the two agreeing is evidence
    /// about the geometry rather than about one implementation. It is used here
    /// as the check and never in the product, because it loses precision at short
    /// range, which is exactly where haversine is good.
    /// </remarks>
    [Theory]
    [InlineData("FN00DJ", "FN31")]
    [InlineData("FN00DJ", "JN86")]
    [InlineData("FN00DJ", "OF88")]
    [InlineData("JN86", "OF88")]
    public void HaversineAgreesWithTheLawOfCosines(string here, string there)
    {
        var a = OperatorLocation.FromGrid(here)!.Value;
        var b = OperatorLocation.FromGrid(there)!.Value;

        var mine = GridPath.MilesBetween(a, b);
        var check = LawOfCosinesMiles(a, b);

        _output.WriteLine(
            here + " -> " + there + " : haversine " + mine.ToString("0.00")
            + ", law of cosines " + check.ToString("0.00"));

        Assert.InRange(Math.Abs(mine - check), 0.0, 1.0);
    }

    /// <summary>The same distance by a formula that shares no algebra.</summary>
    private static double LawOfCosinesMiles(LatLon a, LatLon b)
    {
        const double earthRadiusMiles = 6371.0 / 1.609344;

        var lat1 = a.Latitude * Math.PI / 180.0;
        var lat2 = b.Latitude * Math.PI / 180.0;
        var dLon = (b.Longitude - a.Longitude) * Math.PI / 180.0;

        var cos = (Math.Sin(lat1) * Math.Sin(lat2))
                  + (Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(dLon));

        return earthRadiusMiles * Math.Acos(Math.Clamp(cos, -1.0, 1.0));
    }

    /// <summary>The rounding the tooltip shows, which is the ruling's own.</summary>
    /// <remarks>
    /// **NEAREST TEN UNDER A THOUSAND, NEAREST HUNDRED ABOVE**, which is the
    /// instruction's own suggestion and is taken. The reason it is right is the
    /// grid: a four-character square is seventy miles across, so a mile in the
    /// display would be precision the input does not carry, and at four thousand
    /// miles even a hundred is inside the square's own width.
    /// </remarks>
    [Theory]
    [InlineData(3.0, "10 miles")]
    [InlineData(12.0, "10 miles")]
    [InlineData(244.0, "240 miles")]
    [InlineData(245.0, "250 miles")]
    [InlineData(999.0, "1,000 miles")]
    [InlineData(4531.0, "4,500 miles")]
    [InlineData(8060.0, "8,100 miles")]
    public void MilesAreRoundedToWhatTheGridCanSupport(double miles, string shown)
    {
        var got = GridPath.DescribeMiles(miles);

        _output.WriteLine(miles.ToString("0.0") + " -> " + got);

        Assert.Equal(shown, got);
    }

    /// <summary>A bearing is a whole number of degrees, and it wraps.</summary>
    [Theory]
    [InlineData(0.0, "0 degrees")]
    [InlineData(46.4, "46 degrees")]
    [InlineData(46.6, "47 degrees")]
    [InlineData(359.7, "0 degrees")]
    public void BearingsAreWholeDegrees(double degrees, string shown)
    {
        var got = GridPath.DescribeBearing(degrees);

        _output.WriteLine(degrees.ToString("0.0") + " -> " + got);

        Assert.Equal(shown, got);
    }
}

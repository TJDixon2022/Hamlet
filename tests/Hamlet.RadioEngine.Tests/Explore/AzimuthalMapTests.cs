using System;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Work instruction 301 task 3: **the projection, checked before anything trusts
/// it.**
/// </summary>
/// <remarks>
/// <para>**A DOT PLACED BY UNTESTED ARITHMETIC IS §0.0'S FAULT IN A PICTURE.** A
/// wrong dot on a map is the worst shape this application's prime directive covers:
/// nobody reads a picture sceptically, there is no wording to object to, and a
/// station two thousand miles from where it belongs looks exactly as convincing as
/// one in the right place (HM-DEC-092).</para>
/// <para>**SO IT IS CHECKED AGAINST SOMETHING THAT SHARES NO LINE OF CODE WITH IT.**
/// <see cref="GridPath"/> computes great-circle distance and initial bearing by its
/// own arithmetic, written for a different purpose years of units ago. On an
/// azimuthal equidistant map those two quantities are exactly what a dot's position
/// means: **how far from the centre is the distance, and which way round is the
/// bearing.** If this file's projection and that one's spherical trigonometry agree
/// on both for real stations, they are not agreeing by accident.</para>
/// <para>**THE THREE KNOWN POINTS THE INSTRUCTION ASKS FOR** are the operator's own
/// centre, something transatlantic, and something antipodal, and all three are
/// below.</para>
/// </remarks>
public sealed class AzimuthalMapTests
{
    /// <summary>
    /// **Stand-in numbers for a shipped image that is not in the tree yet.**
    /// </summary>
    /// <remarks>
    /// **THESE ARE THE TEST'S OWN AND NOT THE APPLICATION'S.** The map Tim supplied
    /// has not arrived, so nothing here claims to describe it; a round rim radius is
    /// chosen so that a reader can do the arithmetic in their head - **1,800 pixels
    /// to 180 degrees is exactly ten pixels per degree.**
    /// </remarks>
    private static readonly AzimuthalMap.Settings Test =
        new(CentreLatitude: 40.0, CentreLongitude: -75.0, RimRadiusPixels: 1800);

    /// <summary>Ten pixels to the degree, from the settings above.</summary>
    private const double PerDegree = 10.0;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the checked points are printed.</param>
    public AzimuthalMapTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The centre of the map is the middle of the image.**</summary>
    [Fact]
    public void TheCentreIsTheMiddle()
    {
        var (x, y) = AzimuthalMap.Place(
            Test, Test.CentreLatitude, Test.CentreLongitude);

        _output.WriteLine("the operator's own station: " + Round(x, y));

        Assert.Equal(0, x, 6);
        Assert.Equal(0, y, 6);
    }

    /// <summary>**The antipode is on the rim, whichever way it is reached.**</summary>
    /// <remarks>
    /// **THE POINT OPPOSITE THE CENTRE IS THE WHOLE RIM AT ONCE**, which is a real
    /// property of this projection rather than a defect: every direction from the
    /// centre reaches it after the same 180 degrees. What is checked here is that it
    /// lands **on** the rim, at the full radius, rather than somewhere inside it.
    /// </remarks>
    [Fact]
    public void TheAntipodeIsOnTheRim()
    {
        var antipode = AzimuthalMap.FromCentre(
            Test, -Test.CentreLatitude, Test.CentreLongitude + 180.0);

        _output.WriteLine(
            "the antipode of 40N 75W is 40S 105E, and it lands "
            + antipode.ToString("0.0") + " px from the middle of a map whose rim is "
            + Test.RimRadiusPixels + " px");

        // **A THOUSANDTH OF A PIXEL**, rather than the last decimal a double can
        // carry. Measured, the antipode lands 1799.99998 px out on an 1800 px rim:
        // that is the precision of an inverse sine at the very end of its range and
        // not a placement error, and on a map it is a fifty-thousandth of a pixel.
        Assert.Equal(Test.RimRadiusPixels, antipode, 0.001);

        // **AND NOTHING IS EVER FURTHER OUT THAN THE RIM**, because 180 degrees is
        // as far as anywhere on Earth can be.
        foreach (var (lat, lon) in new[]
        {
            (-40.0, 105.0), (-39.0, 104.0), (-41.0, 106.0), (0.0, 0.0),
            (90.0, 0.0), (-90.0, 0.0), (0.0, 180.0),
        })
        {
            var far = AzimuthalMap.FromCentre(Test, lat, lon);

            Assert.True(
                far <= Test.RimRadiusPixels + 1e-6,
                lat + "," + lon + " landed " + far.ToString("0.0")
                + " px out, past a rim at " + Test.RimRadiusPixels);
        }
    }

    /// <summary>**Distance from the middle is the great-circle distance.**</summary>
    /// <remarks>
    /// **THE CROSS-CHECK, AND IT IS THE ONE THAT MATTERS.** `GridPath.MilesBetween`
    /// shares no line of code with the projection. A degree of arc is 69.09 statute
    /// miles on a sphere of the radius that file uses, so a dot's distance from the
    /// middle in pixels, divided by ten pixels per degree and multiplied by that,
    /// must be the distance `GridPath` reports independently.
    /// </remarks>
    [Theory]
    [InlineData("his own centre", 40.0, -75.0)]
    [InlineData("transatlantic: Ireland", 53.4, -8.2)]
    [InlineData("transatlantic: Portugal", 38.7, -9.1)]
    [InlineData("across the pole: Japan", 35.7, 139.7)]
    [InlineData("south: Argentina", -34.6, -58.4)]
    [InlineData("antipodal: 40S 105E", -40.0, 105.0)]
    [InlineData("near antipodal: Perth", -31.9, 115.9)]
    public void DistanceFromTheMiddleIsTheRealDistance(
        string what, double latitude, double longitude)
    {
        var here = new LatLon(Test.CentreLatitude, Test.CentreLongitude);
        var there = new LatLon(latitude, longitude);

        var pixels = AzimuthalMap.FromCentre(Test, latitude, longitude);

        // The projection's own answer, turned back into miles.
        var degrees = pixels / PerDegree;
        var fromMap = degrees * MilesPerDegree;

        // The independent one.
        var fromTrigonometry = GridPath.MilesBetween(here, there);

        _output.WriteLine(
            what.PadRight(26)
            + pixels.ToString("0.0").PadLeft(8) + " px  ->  "
            + fromMap.ToString("0").PadLeft(6) + " miles     GridPath says "
            + fromTrigonometry.ToString("0").PadLeft(6));

        // **A MILE IN A THOUSAND.** The two use spherical radii that are not
        // identical to the last decimal, so an exact match would be the wrong thing
        // to demand; what is being checked is that the projection is measuring the
        // same thing at all.
        Assert.Equal(
            fromTrigonometry, fromMap, Math.Max(2.0, fromTrigonometry / 1000.0));
    }

    /// <summary>**The direction from the middle is the real initial bearing.**</summary>
    /// <remarks>
    /// **THE SECOND HALF OF THE CROSS-CHECK, AND THE ONE THAT CATCHES A SIGN.** A
    /// distance alone would be identical if every dot were mirrored east for west or
    /// north for south. `GridPath.BearingDegrees` is independent arithmetic, and on
    /// this projection the bearing is simply the angle of the dot from straight up.
    /// </remarks>
    [Theory]
    [InlineData("Ireland, roughly east-north-east", 53.4, -8.2)]
    [InlineData("Portugal, roughly east", 38.7, -9.1)]
    [InlineData("Japan, over the pole", 35.7, 139.7)]
    [InlineData("Argentina, roughly south", -34.6, -58.4)]
    [InlineData("Perth, roughly south-east", -31.9, 115.9)]
    [InlineData("Alaska, roughly north-west", 64.8, -147.7)]
    public void TheDirectionFromTheMiddleIsTheRealBearing(
        string what, double latitude, double longitude)
    {
        var here = new LatLon(Test.CentreLatitude, Test.CentreLongitude);
        var there = new LatLon(latitude, longitude);

        var (x, y) = AzimuthalMap.Place(Test, latitude, longitude);

        // **NORTH IS UP AND THE ANGLE RUNS CLOCKWISE**, which is what a bearing is.
        // `y` is already flipped for the screen, so up is negative.
        var fromMap = (Math.Atan2(x, -y) * 180.0 / Math.PI + 360.0) % 360.0;

        var fromTrigonometry = GridPath.BearingDegrees(here, there);

        var apart = Math.Abs(((fromMap - fromTrigonometry + 540.0) % 360.0) - 180.0);

        _output.WriteLine(
            what.PadRight(32)
            + "map " + fromMap.ToString("0.0").PadLeft(6)
            + "   GridPath " + fromTrigonometry.ToString("0.0").PadLeft(6)
            + "   apart " + apart.ToString("0.00"));

        Assert.True(
            apart < 0.5,
            what + ": the map puts it on a bearing of " + fromMap.ToString("0.0")
            + " and the independent arithmetic says " + fromTrigonometry.ToString("0.0"));
    }

    /// <summary>**East is right, north is up, and neither is mirrored.**</summary>
    /// <remarks>
    /// **THE PLAINEST CHECK IN THE FILE AND THE EASIEST FAULT TO SHIP.** One sign
    /// wrong puts every station in the opposite hemisphere on a map that still looks
    /// entirely reasonable.
    /// </remarks>
    [Fact]
    public void EastIsRightAndNorthIsUp()
    {
        var east = AzimuthalMap.Place(Test, 40.0, -65.0);
        var west = AzimuthalMap.Place(Test, 40.0, -85.0);
        var north = AzimuthalMap.Place(Test, 50.0, -75.0);
        var south = AzimuthalMap.Place(Test, 30.0, -75.0);

        _output.WriteLine("ten degrees east : " + Round(east.X, east.Y));
        _output.WriteLine("ten degrees west : " + Round(west.X, west.Y));
        _output.WriteLine("ten degrees north: " + Round(north.X, north.Y));
        _output.WriteLine("ten degrees south: " + Round(south.X, south.Y));

        Assert.True(east.X > 0, "east of the centre came out to the left");
        Assert.True(west.X < 0, "west of the centre came out to the right");
        Assert.True(north.Y < 0, "north of the centre came out downward");
        Assert.True(south.Y > 0, "south of the centre came out upward");

        // **DUE NORTH AND DUE SOUTH ARE EXACTLY TEN DEGREES**, which at ten pixels
        // to the degree is a hundred pixels, and a meridian is a great circle so
        // this one is arithmetic anybody can check by hand.
        Assert.Equal(100.0, Math.Abs(north.Y), 6);
        Assert.Equal(100.0, Math.Abs(south.Y), 6);
        Assert.Equal(0.0, north.X, 6);
        Assert.Equal(0.0, south.X, 6);
    }

    /// <summary>**The rim radius scales everything and nothing else does.**</summary>
    [Fact]
    public void TheRimRadiusIsTheOnlyScale()
    {
        var small = new AzimuthalMap.Settings(40.0, -75.0, 900);
        var large = new AzimuthalMap.Settings(40.0, -75.0, 1800);

        var a = AzimuthalMap.FromCentre(small, 53.4, -8.2);
        var b = AzimuthalMap.FromCentre(large, 53.4, -8.2);

        _output.WriteLine(
            "Ireland at a 900 px rim: " + a.ToString("0.0")
            + " px; at 1800: " + b.ToString("0.0"));

        Assert.Equal(2.0, b / a, 9);
    }

    /// <summary>Miles in one degree of arc, on the sphere `GridPath` uses.</summary>
    /// <remarks>
    /// **DERIVED FROM THAT FILE RATHER THAN TYPED**: one degree along a meridian is
    /// a great-circle arc, so asking it for the distance between two points one
    /// degree apart is the figure itself, and it cannot drift from whatever radius
    /// that file uses.
    /// </remarks>
    private static readonly double MilesPerDegree = GridPath.MilesBetween(
        new LatLon(0, 0), new LatLon(1, 0));

    private static string Round(double x, double y)
        => "x " + x.ToString("0.00") + ", y " + y.ToString("0.00");
}

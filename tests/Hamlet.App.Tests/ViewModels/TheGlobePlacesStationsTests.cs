using System;
using System.Globalization;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.ViewModels;

/// <summary>
/// Work instruction 306 task 3: **the two stations are placed from real grids, and
/// nothing is placed that cannot be.**
/// </summary>
/// <remarks>
/// <para>**THE OPERATOR'S POSITION IS READ AND NEVER ASSUMED.** The whole reason
/// this asset is centred on the pole rather than on a station is Tim's own: *"The
/// idea behind the map is not that it is specific to me, but that it will work for
/// anyone."* A hard-coded FN00 is exactly the thing that withdrew the first
/// image.</para>
/// <para>**THREE WAYS TO BE UNPLACEABLE, ALL SILENT.** No grid on the air; south of
/// this picture's rim, which is the equator; and outside the cropped bitmap. **A dot
/// is a claim** (§0.0, HM-DEC-092), so where Hamlet cannot place one it places
/// nothing - no rim marker, no arrow, no *not shown* label.</para>
/// <para>**THESE ARE COMPUTED PLACEMENTS, NOT SEEN ONES.** Nothing in this
/// repository can look at a picture; what is asserted is the arithmetic that decides
/// where a dot goes and whether there is one.</para>
/// </remarks>
public sealed class TheGlobePlacesStationsTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the placements are printed.</param>
    public TheGlobePlacesStationsTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The operator is placed from his own resolved grid.**</summary>
    [Fact]
    public void TheOperatorIsPlacedFromHisOwnResolvedGrid()
    {
        var plot = new Ft8GlobePlot("FN00DJ", "IO63", "EI4GNB", "Ireland");

        var here = OperatorLocation.FromGrid("FN00DJ")!.Value;
        var expected = FlatWorldMap.Relief.Place(here.Latitude, here.Longitude)!.Value;

        _output.WriteLine(
            "FN00DJ resolves to "
            + here.Latitude.ToString("0.###", CultureInfo.InvariantCulture) + ", "
            + here.Longitude.ToString("0.###", CultureInfo.InvariantCulture));

        _output.WriteLine("and is placed at " + At(plot.OperatorX, plot.OperatorY));

        Assert.True(plot.HasOperator);
        Assert.Equal(expected.X, plot.OperatorX, 6);
        Assert.Equal(expected.Y, plot.OperatorY, 6);
    }

    /// <summary>**The centre moves when the operator's grid does.**</summary>
    /// <remarks>
    /// **NOTHING IS PINNED TO FN00.** Two operators, two different places on the same
    /// picture, and the station they are both working does not move.
    /// </remarks>
    [Fact]
    public void TheCentreMovesWhenTheOperatorGridDoes()
    {
        var american = new Ft8GlobePlot("FN00DJ", "IO63", "EI4GNB", "Ireland");
        var european = new Ft8GlobePlot("JO65", "IO63", "EI4GNB", "Ireland");

        _output.WriteLine("FN00DJ operator at " + At(american.OperatorX, american.OperatorY));
        _output.WriteLine("JO65   operator at " + At(european.OperatorX, european.OperatorY));
        _output.WriteLine("station, both      : " + At(american.StationX, american.StationY));

        Assert.True(american.HasOperator);
        Assert.True(european.HasOperator);

        // **A DIFFERENT OPERATOR IS A DIFFERENT PLACE**, by a long way.
        var moved = Math.Sqrt(
            Math.Pow(american.OperatorX - european.OperatorX, 2)
            + Math.Pow(american.OperatorY - european.OperatorY, 2));

        _output.WriteLine(
            "the operator moved " + moved.ToString("0", CultureInfo.InvariantCulture)
            + " px between the two");

        Assert.True(moved > 100, "the operator barely moved: " + moved + " px");

        // **AND THE STATION DID NOT MOVE AT ALL.**
        Assert.Equal(american.StationX, european.StationX, 6);
        Assert.Equal(american.StationY, european.StationY, 6);
    }

    /// <summary>**A station with no grid is not placed.**</summary>
    [Fact]
    public void AStationWithNoGridIsNotPlaced()
    {
        var plot = new Ft8GlobePlot("FN00DJ", null, "K9XP", "");

        _output.WriteLine("caption: " + plot.Caption);

        Assert.False(plot.HasStation);
        Assert.False(plot.HasPath);

        // **AND NO DISTANCE EITHER** (HM-DEC-038). No grid means no distance,
        // anywhere - never an estimate from a prefix.
        Assert.Null(plot.Miles);
    }

    /// <summary>**A station south of the equator is placed now.**</summary>
    /// <remarks>
    /// <para>**THIS IS THE WHOLE POINT OF UNIT 308.** On the polar picture the rim
    /// was the equator and Sydney had nowhere to be drawn; the flat relief map
    /// reaches 63.79 degrees south and **everyone unit 306 named as having no place
    /// now has one** - Nairobi, Buenos Aires, Sydney, Cape Town, Sao Paulo, Lima,
    /// Auckland and Honolulu.</para>
    /// <para>**THE REFUSAL IS NOT REMOVED, IT IS RARER.** Antarctica and a narrow
    /// strip of ocean west of Hawaii are the whole of what is left, and the next test
    /// holds that behaviour.</para>
    /// </remarks>
    [Fact]
    public void AStationSouthOfTheEquatorIsPlacedNow()
    {
        var plot = new Ft8GlobePlot("FN00DJ", "QF56", "VK2ABC", "New South Wales");

        _output.WriteLine("caption: " + plot.Caption);
        _output.WriteLine("placed at: " + At(plot.StationX, plot.StationY));

        Assert.True(plot.HasStation);
        Assert.True(plot.StationGridResolved);
        Assert.NotNull(plot.Miles);

        // **AND THE PATH IS DRAWN**, because both ends are on the picture.
        Assert.True(plot.HasPath);

        Assert.DoesNotContain(
            "does not know where", plot.Caption, StringComparison.Ordinal);
    }

    /// <summary>**Antarctica is still not placed, and nothing is claimed.**</summary>
    /// <remarks>
    /// **THE COVERAGE LIMIT THAT IS LEFT.** McMurdo at 77.84 degrees south is below
    /// the bottom edge of the file. He put a grid on the air and the distance to him
    /// is measured; there is simply nowhere on this picture to draw him, and a dot on
    /// the border would say he is there (§0.0).
    /// </remarks>
    [Fact]
    public void AStationInAntarcticaIsNotPlacedAndNothingIsClaimed()
    {
        var plot = new Ft8GlobePlot("FN00DJ", "RB32", "KC4AAA", "Antarctica");

        _output.WriteLine("caption: " + plot.Caption);

        Assert.False(plot.HasStation);
        Assert.False(plot.HasPath);
        Assert.True(plot.StationGridResolved);
        Assert.NotNull(plot.Miles);

        Assert.DoesNotContain(
            "does not know where", plot.Caption, StringComparison.Ordinal);
    }

    /// <summary>**A station outside the bitmap is not placed.**</summary>
    /// <remarks>
    /// **INSIDE THE DISC IS NOT THE SAME AS ON THE FILE.** The picture is cropped top
    /// and bottom, so a northern-hemisphere station can be inside the projection's
    /// coverage and off the edge of the image.
    /// </remarks>
    [Fact]
    public void AStationOutsideTheBitmapIsNotPlaced()
    {
        var outside = Outside();

        _output.WriteLine(
            "grid " + outside.Grid + " is inside the rim at "
            + outside.FromCentre.ToString("0.0", CultureInfo.InvariantCulture)
            + " px of "
            + (AzimuthalMap.NorthPolar.RimDegrees
               * AzimuthalMap.NorthPolar.PixelsPerDegree)
                .ToString("0.0", CultureInfo.InvariantCulture)
            + ", and off the bitmap at " + At(outside.X, outside.Y));

        var plot = new Ft8GlobePlot("FN00DJ", outside.Grid, "TEST", "");

        Assert.False(plot.HasStation);
        Assert.True(plot.StationGridResolved);
    }

    /// <summary>A northern grid inside the disc and off the file, found by sweeping.</summary>
    private static (string Grid, double X, double Y, double FromCentre) Outside()
    {
        var rim = AzimuthalMap.NorthPolar.RimDegrees
            * AzimuthalMap.NorthPolar.PixelsPerDegree;

        for (var lat = 1.0; lat < 40.0; lat += 1.0)
        {
            for (var lon = -180.0; lon < 180.0; lon += 1.0)
            {
                var settings = new AzimuthalMap.Settings(
                    90, 0, AzimuthalMap.NorthPolar.HalfTurnPixels);

                var (u, v) = AzimuthalMap.Place(settings, lat, lon);
                var r = Math.Sqrt((u * u) + (v * v));

                if (r > rim || FlatWorldMap.Relief.Place(lat, lon) is not null)
                {
                    continue;
                }

                var turn = AzimuthalMap.NorthPolar.RotationDegrees * Math.PI / 180.0;
                var s = Math.Sin(turn);
                var c = Math.Cos(turn);

                return (
                    OperatorLocation.ToGrid(new LatLon(lat, lon)),
                    AzimuthalMap.NorthPolar.CentreX + (s * u) + (c * v),
                    AzimuthalMap.NorthPolar.CentreY - (c * u) + (s * v),
                    r);
            }
        }

        throw new InvalidOperationException(
            "no northern place is inside the rim and off the bitmap, so this "
            + "picture is not cropped and the test has nothing to assert");
    }

    private static string At(double x, double y)
        => x.ToString("0.##", CultureInfo.InvariantCulture) + ", "
           + y.ToString("0.##", CultureInfo.InvariantCulture);
}

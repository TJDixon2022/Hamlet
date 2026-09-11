using System;
using System.Globalization;
using System.Linq;
using Hamlet.RadioEngine.Explore;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Explore;

/// <summary>
/// Work instruction 308 task 3: **the path is the great circle, sampled and split.**
/// </summary>
/// <remarks>
/// <para>**THE STRAIGHT LINE WAS NEVER THE PATH.** Unit 306 corrected a caveat about
/// flat maps on the grounds that a line out of the centre of an azimuthal picture is
/// the great circle - which is true, and the operator was never at the centre of the
/// polar picture, so the line between two markers there was not the path either. **The
/// fix is not a projection that flatters straight lines. It is to stop drawing
/// them.**</para>
/// <para>**THE FIGURES ARE THE AUTHOR'S AND ARE INDICATIONS** (`SHACK_FACTS.md`
/// FACT-004). They were measured on a machine with no radio; what this asserts is
/// arithmetic, not appearance. Nothing in this tree can see whether the drawn arc goes
/// over Alaska.</para>
/// </remarks>
public sealed class TheGreatCirclePathTests
{
    private const double FromLatitude = 40.396;
    private const double FromLongitude = -79.708;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the paths are printed.</param>
    public TheGreatCirclePathTests(ITestOutputHelper output) => _output = output;

    /// <summary>**The four paths, their segments and their vertices.**</summary>
    /// <param name="place">Where the path goes, for the failure message.</param>
    /// <param name="latitude">Its latitude.</param>
    /// <param name="longitude">Its longitude.</param>
    /// <param name="segments">How many runs it draws as.</param>
    /// <param name="vertices">How many points survive, across all runs.</param>
    [Theory]
    [InlineData("Sao Paulo", -23.5505, -46.6333, 1, 181)]
    [InlineData("Cape Town", -33.9249, 18.4241, 1, 181)]
    [InlineData("Tokyo", 35.6762, 139.6503, 2, 181)]
    [InlineData("Auckland", -36.8485, 174.7633, 2, 181)]
    public void EachPathDrawsAsTheExpectedRuns(
        string place, double latitude, double longitude, int segments, int vertices)
    {
        var runs = GreatCirclePath.On(
            FlatWorldMap.Relief, FromLatitude, FromLongitude, latitude, longitude);

        var counted = runs.Sum(r => r.Count);

        _output.WriteLine(
            place.PadRight(12) + runs.Count + " run(s), "
            + string.Join(" + ", runs.Select(r => r.Count)) + " = " + counted
            + " vertices of a possible " + (GreatCirclePath.Segments + 1));

        Assert.Equal(segments, runs.Count);

        // **VERTICES DROPPED ARE SAMPLES WITH NO PLACE ON THE FILE**, which is the
        // coverage limit doing its job rather than an error.
        Assert.True(
            counted <= vertices,
            place + " drew " + counted + " vertices, more than the " + vertices
            + " the arc has");
    }

    /// <summary>**The Tokyo path goes over Alaska and the straight line does not.**</summary>
    /// <remarks>
    /// **THIS IS THE ONE THAT PROVES THE METHOD.** If this passes, the drawn path is
    /// the path; if it fails, the map is drawing a route nobody took.
    /// </remarks>
    [Fact]
    public void TheTokyoPathGoesOverAlaskaAndTheStraightLineDoesNot()
    {
        var samples = GreatCirclePath.Samples(
            FromLatitude, FromLongitude, 35.6762, 139.6503);

        var middle = samples[samples.Count / 2];

        var onTheArc = FlatWorldMap.Relief.Place(middle.Latitude, middle.Longitude)!.Value;

        var from = FlatWorldMap.Relief.Place(FromLatitude, FromLongitude)!.Value;
        var to = FlatWorldMap.Relief.Place(35.6762, 139.6503)!.Value;

        var straight = ((from.X + to.X) / 2.0, (from.Y + to.Y) / 2.0);

        var apart = Math.Sqrt(
            Square(onTheArc.X - straight.Item1) + Square(onTheArc.Y - straight.Item2));

        _output.WriteLine(
            "the arc midpoint  : "
            + middle.Latitude.ToString("0.00", CultureInfo.InvariantCulture) + " N, "
            + Math.Abs(middle.Longitude).ToString("0.00", CultureInfo.InvariantCulture)
            + " W  ->  " + Where(onTheArc));

        _output.WriteLine("the straight line : " + Where(straight));
        _output.WriteLine("the two markers   : " + Where(from) + "  and  " + Where(to));
        _output.WriteLine(
            "they differ by    : "
            + apart.ToString("0.0", CultureInfo.InvariantCulture) + " px");

        // **NORTHERN ALASKA**, which is where a signal from Pennsylvania to Tokyo
        // actually goes.
        Assert.InRange(middle.Latitude, 66.0, 67.5);
        Assert.InRange(middle.Longitude, -156.5, -154.0);

        // **AND THE STRAIGHT LINE IS NOWHERE NEAR IT.**
        Assert.True(
            apart > 200,
            "the arc and the straight line differ by only "
            + apart.ToString("0.0", CultureInfo.InvariantCulture) + " px");
    }

    /// <summary>**The split is the date line, not an arbitrary gap.**</summary>
    [Fact]
    public void ThePathSplitsAtTheAntimeridian()
    {
        var runs = GreatCirclePath.On(
            FlatWorldMap.Relief, FromLatitude, FromLongitude, 35.6762, 139.6503);

        Assert.Equal(2, runs.Count);

        var endOfFirst = runs[0][^1];
        var startOfSecond = runs[1][0];

        _output.WriteLine("first run ends at   : " + Where(endOfFirst));
        _output.WriteLine("second run starts at: " + Where(startOfSecond));

        // **THE TWO ENDS ARE ON OPPOSITE SIDES OF THE PICTURE.** Joined, they would
        // draw a horizontal line straight across it.
        Assert.True(
            Math.Abs(endOfFirst.X - startOfSecond.X) > 400,
            "the two runs are not on opposite edges, so this is not the date line");
    }

    /// <summary>**A path with both ends in one hemisphere draws as one run.**</summary>
    [Fact]
    public void APathThatCrossesNoDateLineDrawsAsOneRun()
    {
        var runs = GreatCirclePath.On(
            FlatWorldMap.Relief, FromLatitude, FromLongitude, -23.5505, -46.6333);

        _output.WriteLine(
            "Sao Paulo: " + runs.Count + " run of " + runs[0].Count + " vertices");

        var only = Assert.Single(runs);

        Assert.Equal(GreatCirclePath.Segments + 1, only.Count);
    }

    private static string Where((double X, double Y) at)
        => at.X.ToString("0.0", CultureInfo.InvariantCulture) + ", "
           + at.Y.ToString("0.0", CultureInfo.InvariantCulture);

    private static double Square(double value) => value * value;
}

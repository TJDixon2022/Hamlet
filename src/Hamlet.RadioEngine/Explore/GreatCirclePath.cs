using System;
using System.Collections.Generic;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// **The path a signal actually takes, sampled and projected.**
/// </summary>
/// <remarks>
/// <para>**A FLAT MAP ONLY LIES ABOUT DIRECTION IF YOU DRAW A STRAIGHT LINE ON IT**
/// (work instruction 308 task 3). Sample the great circle, project every sample, and
/// the drawn polyline is the true path on any projection at all. That disposes of the
/// polar case too, where the operator was never at the centre and so a straight line
/// between two markers was never the path either.</para>
/// <para>**THE ONE THAT PROVES THE METHOD.** From FN00DJ to Tokyo the true path goes
/// over northern Alaska - its midpoint is 66.63 N, 155.18 W - and the straight line
/// between the two markers runs across the Atlantic, Spain and central Asia. **They
/// differ by more than two hundred pixels at the midpoint.**</para>
/// <para>**THE SPLIT IS THE ANTIMERIDIAN.** Consecutive samples that jump more than
/// half a turn of longitude are on opposite sides of the date line; without the split
/// the path draws as a horizontal line straight across the picture, which is a
/// statement about a route nobody took (§0.0).</para>
/// <para>**A SAMPLE WITH NO PLACE ENDS THE SEGMENT.** The picture does not cover
/// everywhere, and a path that skipped over a gap would be joining two points across
/// ground the file cannot show.</para>
/// </remarks>
public static class GreatCirclePath
{
    /// <summary>How many segments the arc is cut into.</summary>
    /// <remarks>
    /// **ONE HUNDRED AND EIGHTY, SO ONE HUNDRED AND EIGHTY-ONE POINTS.** At the
    /// longest path this picture can hold that is about seventy miles a step, which
    /// is finer than a four-character grid square and far finer than a pixel.
    /// </remarks>
    public const int Segments = 180;

    /// <summary>Past this much longitude between two samples, the date line is between them.</summary>
    public const double AntimeridianJumpDegrees = 180.0;

    /// <summary>The drawn path, in one or more runs of pixels.</summary>
    /// <param name="map">The picture the path is drawn on.</param>
    /// <param name="fromLatitude">Where it starts.</param>
    /// <param name="fromLongitude">Where it starts.</param>
    /// <param name="toLatitude">Where it ends.</param>
    /// <param name="toLongitude">Where it ends.</param>
    /// <returns>Each run of consecutive on-file points, in order.</returns>
    /// <exception cref="ArgumentNullException">There is no map.</exception>
    public static IReadOnlyList<IReadOnlyList<(double X, double Y)>> On(
        FlatWorldImage map,
        double fromLatitude,
        double fromLongitude,
        double toLatitude,
        double toLongitude)
    {
        ArgumentNullException.ThrowIfNull(map);

        var runs = new List<IReadOnlyList<(double X, double Y)>>();
        var run = new List<(double X, double Y)>();
        var lastLongitude = double.NaN;

        void End()
        {
            if (run.Count > 1)
            {
                runs.Add(run);
            }

            run = new List<(double X, double Y)>();
        }

        foreach (var (latitude, longitude) in Samples(
            fromLatitude, fromLongitude, toLatitude, toLongitude))
        {
            if (!double.IsNaN(lastLongitude)
                && Math.Abs(longitude - lastLongitude) > AntimeridianJumpDegrees)
            {
                End();
            }

            lastLongitude = longitude;

            if (map.Place(latitude, longitude) is not { } at)
            {
                End();
                continue;
            }

            run.Add(at);
        }

        End();

        return runs;
    }

    /// <summary>Every sample along the arc, from one end to the other.</summary>
    /// <param name="fromLatitude">Where it starts.</param>
    /// <param name="fromLongitude">Where it starts.</param>
    /// <param name="toLatitude">Where it ends.</param>
    /// <param name="toLongitude">Where it ends.</param>
    /// <returns>181 points in degrees, the ends included.</returns>
    /// <remarks>
    /// **SPHERICAL INTERPOLATION, NOT INTERPOLATION OF THE TWO NUMBERS.** Averaging
    /// latitudes and longitudes gives a line on the flat map rather than a path on
    /// the globe, which is exactly the fault this replaces.
    /// </remarks>
    public static IReadOnlyList<(double Latitude, double Longitude)> Samples(
        double fromLatitude,
        double fromLongitude,
        double toLatitude,
        double toLongitude)
    {
        var p1 = Radians(fromLatitude);
        var l1 = Radians(fromLongitude);
        var p2 = Radians(toLatitude);
        var l2 = Radians(toLongitude);

        var d = 2.0 * Math.Asin(Math.Min(1.0, Math.Sqrt(
            Square(Math.Sin((p1 - p2) / 2.0))
            + (Math.Cos(p1) * Math.Cos(p2) * Square(Math.Sin((l1 - l2) / 2.0))))));

        var points = new List<(double, double)>(Segments + 1);

        for (var step = 0; step <= Segments; step++)
        {
            var f = (double)step / Segments;

            // **THE DEGENERATE CASE IS THE SAME POINT TWICE**, where every direction
            // is equally correct and there is no arc to walk along.
            if (d <= double.Epsilon)
            {
                points.Add((fromLatitude, fromLongitude));
                continue;
            }

            var a = Math.Sin((1 - f) * d) / Math.Sin(d);
            var b = Math.Sin(f * d) / Math.Sin(d);

            var x = (a * Math.Cos(p1) * Math.Cos(l1))
                + (b * Math.Cos(p2) * Math.Cos(l2));

            var y = (a * Math.Cos(p1) * Math.Sin(l1))
                + (b * Math.Cos(p2) * Math.Sin(l2));

            var z = (a * Math.Sin(p1)) + (b * Math.Sin(p2));

            points.Add((
                Degrees(Math.Atan2(z, Math.Sqrt((x * x) + (y * y)))),
                Degrees(Math.Atan2(y, x))));
        }

        return points;
    }

    private static double Square(double value) => value * value;

    private static double Radians(double degrees) => degrees * Math.PI / 180.0;

    private static double Degrees(double radians) => radians * 180.0 / Math.PI;
}

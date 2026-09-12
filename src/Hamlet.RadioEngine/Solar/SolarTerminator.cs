namespace Hamlet.RadioEngine.Solar;

/// <summary>The point on the earth the sun is straight above.</summary>
/// <param name="Latitude">Degrees north, which is the sun's declination.</param>
/// <param name="Longitude">Degrees east, negative west.</param>
public readonly record struct SubsolarPoint(double Latitude, double Longitude);

/// <summary>
/// **The night side of the earth, from the sun and the clock** (work instruction 332 task 2).
/// </summary>
/// <remarks>
/// <para>**THIS IS NOT PROPAGATION**, for the reason <see cref="SolarClock"/> gives: where the
/// sun is, is a fact about the solar system and may be drawn plainly. Whether a band is open
/// along a path is a fact about the ionosphere Hamlet cannot see, and nothing here says
/// it.</para>
/// <para>**THE SUBSOLAR POINT IS NOAA'S GENERAL SOLAR POSITION SERIES** - the fractional year,
/// the equation of time and the declination as Fourier terms - which is good to well under a
/// degree, and a degree of longitude is four minutes of the terminator's travel. `SolarClock`
/// answers a different question, sunrise at one place, with the almanac equation; the two are
/// kept apart rather than one bent into the other.</para>
/// <para>**THE GRAY EDGE IS A SIX-DEGREE FADE**: full daylight with the sun on the horizon,
/// full dark with it six degrees below, which is civil twilight. The width is the author's
/// shape marked for the owner (work instruction 332's ARBITER block); the arithmetic is
/// asserted against a hand-computed value.</para>
/// <para>Pure: an instant in, a point out. No clock read (§5).</para>
/// </remarks>
public static class SolarTerminator
{
    /// <summary>How far below the horizon the fade to full dark runs, in degrees.</summary>
    public const double TwilightDegrees = 6.0;

    /// <summary>Where the sun is overhead at an instant.</summary>
    /// <param name="utc">The instant, in UTC.</param>
    /// <returns>The subsolar point, longitude in [-180, 180).</returns>
    public static SubsolarPoint At(DateTime utc)
    {
        var instant = utc.Kind == DateTimeKind.Local ? utc.ToUniversalTime() : utc;
        var hours = instant.TimeOfDay.TotalHours;
        var daysInYear = DateTime.IsLeapYear(instant.Year) ? 366.0 : 365.0;

        // The fractional year, in radians.
        var g = 2 * Math.PI / daysInYear * (instant.DayOfYear - 1 + ((hours - 12) / 24));

        var equationOfTimeMinutes = 229.18 * (0.000075
            + (0.001868 * Math.Cos(g)) - (0.032077 * Math.Sin(g))
            - (0.014615 * Math.Cos(2 * g)) - (0.040849 * Math.Sin(2 * g)));

        var declination = 0.006918
            - (0.399912 * Math.Cos(g)) + (0.070257 * Math.Sin(g))
            - (0.006758 * Math.Cos(2 * g)) + (0.000907 * Math.Sin(2 * g))
            - (0.002697 * Math.Cos(3 * g)) + (0.00148 * Math.Sin(3 * g));

        // **THE SUN IS OVERHEAD WHERE IT IS LOCAL SOLAR NOON**, which is fifteen degrees
        // west for every hour past noon UTC, moved by the equation of time.
        var longitude = -15.0 * (hours - 12 + (equationOfTimeMinutes / 60.0));

        return new SubsolarPoint(ToDegrees(declination), Wrap(longitude));
    }

    /// <summary>How high the sun stands over a place, in degrees; negative below the horizon.</summary>
    /// <param name="sun">Where the sun is overhead.</param>
    /// <param name="latitude">Degrees north.</param>
    /// <param name="longitude">Degrees east.</param>
    /// <returns>The elevation.</returns>
    public static double ElevationDegrees(SubsolarPoint sun, double latitude, double longitude)
    {
        var sine = (Sin(latitude) * Sin(sun.Latitude))
            + (Cos(latitude) * Cos(sun.Latitude) * Cos(longitude - sun.Longitude));

        return ToDegrees(Math.Asin(Math.Clamp(sine, -1.0, 1.0)));
    }

    /// <summary>How dark a place is: 0 with the sun up, 1 once it is six degrees down.</summary>
    /// <param name="sun">Where the sun is overhead.</param>
    /// <param name="latitude">Degrees north.</param>
    /// <param name="longitude">Degrees east.</param>
    /// <returns>The darkness, 0 to 1.</returns>
    public static double Darkness(SubsolarPoint sun, double latitude, double longitude)
        => Math.Clamp(-ElevationDegrees(sun, latitude, longitude) / TwilightDegrees, 0.0, 1.0);

    private static double Wrap(double longitude)
    {
        var turned = (longitude + 180.0) % 360.0;

        return (turned < 0 ? turned + 360.0 : turned) - 180.0;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static double ToDegrees(double radians) => radians * 180.0 / Math.PI;

    private static double Sin(double degrees) => Math.Sin(ToRadians(degrees));

    private static double Cos(double degrees) => Math.Cos(ToRadians(degrees));
}

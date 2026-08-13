namespace Hamlet.RadioEngine.Solar;

/// <summary>Whether the sun is up where the operator is.</summary>
public enum SunState
{
    /// <summary>The operator's coordinates are not known, so nothing is claimed.</summary>
    Unknown,

    /// <summary>The sun is above the horizon.</summary>
    Day,

    /// <summary>The sun is below the horizon.</summary>
    Night,
}

/// <summary>What the sun is doing where the operator is, right now.</summary>
/// <param name="State">Day, night, or unknown.</param>
/// <param name="SunriseUtc">Today's sunrise, or null.</param>
/// <param name="SunsetUtc">Today's sunset, or null.</param>
/// <param name="SinceSunrise">How long since the sun came up, when it has.</param>
/// <param name="SinceSunset">How long since the sun went down, when it has.</param>
/// <param name="UntilSunset">How long until it goes down, when it is up.</param>
/// <param name="UntilSunrise">How long until it comes up, when it is down.</param>
public sealed record SolarSnapshot(
    SunState State,
    DateTime? SunriseUtc,
    DateTime? SunsetUtc,
    TimeSpan? SinceSunrise,
    TimeSpan? SinceSunset,
    TimeSpan? UntilSunset,
    TimeSpan? UntilSunrise)
{
    /// <summary>Nothing is known: no coordinates.</summary>
    public static SolarSnapshot Unknown { get; } =
        new(SunState.Unknown, null, null, null, null, null, null);

    /// <summary>True when the sun is up.</summary>
    public bool IsDaylight => State == SunState.Day;

    /// <summary>True when anything at all can be said.</summary>
    public bool IsKnown => State != SunState.Unknown;
}

/// <summary>
/// Where the sun is, from latitude, longitude and the date.
/// </summary>
/// <remarks>
/// <para>Arithmetic, not a service (HM-DEC-033). Sunrise and sunset are a
/// solved problem that has been in printed almanacs for a century, so Hamlet
/// computes them rather than adding a dependency, an API key and a thing that
/// can be down. The implementation is the Almanac for Computers sunrise
/// equation, accurate to about a minute for the latitudes anybody operates
/// from — far tighter than the claims built on it need.</para>
/// <para>THIS IS NOT PROPAGATION. Where the sun is, is a fact about the solar
/// system and may be stated plainly. Whether a band is open is a fact about
/// the ionosphere that Hamlet cannot see and does not claim (FG-007,
/// HM-DEC-031). Everything in this namespace is the former.</para>
/// <para>Pure: latitude, longitude and an instant in, an answer out. No clock
/// read (§5), so a test can ask what the sun was doing at any moment in
/// history.</para>
/// </remarks>
public static class SolarClock
{
    /// <summary>
    /// The official zenith for sunrise and sunset: 90°50', which allows for
    /// the sun's own width and for refraction bending its image above the
    /// true horizon.
    /// </summary>
    public const double OfficialZenithDegrees = 90.833;

    /// <summary>
    /// Where the sun is at a given instant, for a given place.
    /// </summary>
    /// <param name="latitude">Degrees north, negative south.</param>
    /// <param name="longitude">Degrees east, negative west.</param>
    /// <param name="utcNow">The instant to answer for. Passed in, never read.</param>
    /// <returns>The snapshot; day and night are decided by the computed times.</returns>
    public static SolarSnapshot At(double latitude, double longitude, DateTime utcNow)
    {
        // The local day, not the UTC day. For a station in Pennsylvania the two
        // differ for five hours out of every twenty-four, and using the UTC day
        // would ask for the sunset of a day that has not started there yet.
        var date = utcNow.AddHours(longitude / 15.0).Date;

        var sunrise = EventUtc(latitude, longitude, date, rising: true);
        var sunset = EventUtc(latitude, longitude, date, rising: false);

        if (sunrise is null || sunset is null)
        {
            // Polar day or polar night: the sun does not cross the horizon at
            // all. Rather than invent a time, decide from the sun's height and
            // leave the times null.
            var aboveHorizon = IsSunUpAllDay(latitude, date);
            return new SolarSnapshot(
                aboveHorizon ? SunState.Day : SunState.Night,
                null, null, null, null, null, null);
        }

        // The computed pair brackets the local day; the instant may sit before
        // sunrise, between, or after sunset.
        var isDay = utcNow >= sunrise.Value && utcNow < sunset.Value;

        var previousSunset = utcNow < sunrise.Value
            ? EventUtc(latitude, longitude, date.AddDays(-1), rising: false)
            : sunset;

        var nextSunrise = utcNow >= sunset.Value
            ? EventUtc(latitude, longitude, date.AddDays(1), rising: true)
            : sunrise;

        return new SolarSnapshot(
            isDay ? SunState.Day : SunState.Night,
            sunrise,
            sunset,
            isDay ? utcNow - sunrise.Value : null,
            !isDay && previousSunset is not null && utcNow >= previousSunset.Value
                ? utcNow - previousSunset.Value
                : null,
            isDay ? sunset.Value - utcNow : null,
            !isDay && nextSunrise is not null && nextSunrise.Value > utcNow
                ? nextSunrise.Value - utcNow
                : null);
    }

    /// <summary>
    /// Sunrise or sunset in UTC for a date and place, or null when the sun
    /// does not cross the horizon that day.
    /// </summary>
    /// <param name="latitude">Degrees north.</param>
    /// <param name="longitude">Degrees east.</param>
    /// <param name="date">The local date at that longitude.</param>
    /// <param name="rising">True for sunrise, false for sunset.</param>
    /// <returns>
    /// The instant in UTC, or null inside the polar circles. It may fall on the
    /// UTC day either side of <paramref name="date"/>: sunset in Pennsylvania in
    /// June is just after midnight UTC.
    /// </returns>
    public static DateTime? EventUtc(
        double latitude, double longitude, DateTime date, bool rising)
    {
        var dayOfYear = date.DayOfYear;
        var longitudeHour = longitude / 15.0;

        // Approximate time of the event, as a fraction of the year's day.
        var t = dayOfYear + (((rising ? 6.0 : 18.0) - longitudeHour) / 24.0);

        // The sun's mean anomaly, then its true longitude.
        var meanAnomaly = (0.9856 * t) - 3.289;
        var trueLongitude = meanAnomaly
            + (1.916 * Sin(meanAnomaly))
            + (0.020 * Sin(2 * meanAnomaly))
            + 282.634;
        trueLongitude = Normalize(trueLongitude, 360);

        // Right ascension, pushed into the same quadrant as the longitude.
        var rightAscension = Normalize(Atan(0.91764 * Tan(trueLongitude)), 360);
        var longitudeQuadrant = Math.Floor(trueLongitude / 90.0) * 90.0;
        var ascensionQuadrant = Math.Floor(rightAscension / 90.0) * 90.0;
        rightAscension = (rightAscension + (longitudeQuadrant - ascensionQuadrant)) / 15.0;

        var sinDeclination = 0.39782 * Sin(trueLongitude);
        var cosDeclination = Math.Cos(Math.Asin(sinDeclination));

        var cosHourAngle =
            (Math.Cos(ToRadians(OfficialZenithDegrees)) - (sinDeclination * Sin(latitude)))
            / (cosDeclination * Cos(latitude));

        // Outside [-1, 1] the sun never reaches the horizon that day.
        if (cosHourAngle is > 1 or < -1)
        {
            return null;
        }

        var hourAngle = rising
            ? 360.0 - Acos(cosHourAngle)
            : Acos(cosHourAngle);
        hourAngle /= 15.0;

        var localMeanTime = hourAngle + rightAscension - (0.06571 * t) - 6.622;
        var universalTime = Normalize(localMeanTime - longitudeHour, 24);

        return date.Date.AddDays(DayOffset(universalTime, longitudeHour, rising))
            .AddHours(universalTime);
    }

    /// <summary>
    /// Which UTC day the event falls on, relative to the local date.
    /// </summary>
    /// <remarks>
    /// <para>The equation yields a time of day in UTC with the date thrown
    /// away. Stamping it on the local date is right only where the two happen
    /// to agree: sunset in Pennsylvania in June is 00:54 UTC the FOLLOWING day,
    /// and sunrise in Japan is the evening UTC of the day BEFORE. Getting this
    /// wrong puts sunset before sunrise, and everything downstream then reports
    /// a permanent night.</para>
    /// <para>The test is the event's own local solar time, which is not free to
    /// be anything: sunrise is always in the morning half of the local day and
    /// sunset always in the afternoon half. Whichever day offset satisfies that
    /// is the right one.</para>
    /// </remarks>
    private static int DayOffset(double universalTime, double longitudeHour, bool rising)
    {
        var localSolarTime = universalTime + longitudeHour;

        if (rising)
        {
            return localSolarTime < 0 ? 1 : localSolarTime >= 12 ? -1 : 0;
        }

        return localSolarTime < 12 ? 1 : localSolarTime >= 24 ? -1 : 0;
    }

    /// <summary>
    /// Inside the polar circles, whether the sun stays up all day.
    /// </summary>
    /// <remarks>
    /// The sun's declination and the observer's latitude have the same sign in
    /// polar summer and opposite signs in polar winter. That is the whole test
    /// — no station Hamlet serves is likely to need it, but returning a
    /// confident "night" for a Norwegian June would be wrong and cheap to
    /// avoid.
    /// </remarks>
    private static bool IsSunUpAllDay(double latitude, DateTime date)
    {
        var declination = 23.44 * Sin(360.0 / 365.0 * (date.DayOfYear - 81));
        return Math.Sign(declination) == Math.Sign(latitude);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static double ToDegrees(double radians) => radians * 180.0 / Math.PI;

    private static double Sin(double degrees) => Math.Sin(ToRadians(degrees));

    private static double Cos(double degrees) => Math.Cos(ToRadians(degrees));

    private static double Tan(double degrees) => Math.Tan(ToRadians(degrees));

    private static double Atan(double x) => ToDegrees(Math.Atan(x));

    private static double Acos(double x) => ToDegrees(Math.Acos(x));

    private static double Normalize(double value, double range)
    {
        var v = value % range;
        return v < 0 ? v + range : v;
    }
}

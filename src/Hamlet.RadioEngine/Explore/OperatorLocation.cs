using System.Globalization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>A point on the earth, in degrees.</summary>
/// <param name="Latitude">Degrees north, negative south.</param>
/// <param name="Longitude">Degrees east, negative west.</param>
public readonly record struct LatLon(double Latitude, double Longitude);

/// <summary>
/// Turns what the operator typed about themselves into the two facts the spot
/// filter needs: which US call district is home, and whether they are in North
/// America at all.
/// </summary>
/// <remarks>
/// <para>Both answers are derived only where the derivation is exact. The
/// state-to-district table is the FCC's own allocation, so "Pittsburgh, PA"
/// yields district 3 with no guessing. A Maidenhead locator converts to
/// latitude and longitude by arithmetic, so it can say which continent the
/// operator is on.</para>
/// <para>What is deliberately NOT derived: a call district from a grid
/// square. District boundaries are state lines, and a grid square straddles
/// them, so that step would be an estimate dressed as a fact. When the
/// location names no state the caller gets null and filters at continent
/// grain instead, which is the honest degradation (HM-DEC-009, HM-DEC-024).</para>
/// </remarks>
public static class OperatorLocation
{
    /// <summary>US call districts by state, per the FCC allocation.</summary>
    private static readonly Dictionary<string, int> StateDistricts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CT"] = 1, ["ME"] = 1, ["MA"] = 1, ["NH"] = 1, ["RI"] = 1, ["VT"] = 1,
        ["NJ"] = 2, ["NY"] = 2,
        ["DE"] = 3, ["DC"] = 3, ["MD"] = 3, ["PA"] = 3,
        ["AL"] = 4, ["FL"] = 4, ["GA"] = 4, ["KY"] = 4, ["NC"] = 4,
        ["SC"] = 4, ["TN"] = 4, ["VA"] = 4,
        ["AR"] = 5, ["LA"] = 5, ["MS"] = 5, ["NM"] = 5, ["OK"] = 5, ["TX"] = 5,
        ["CA"] = 6,
        ["AZ"] = 7, ["ID"] = 7, ["MT"] = 7, ["NV"] = 7, ["OR"] = 7,
        ["UT"] = 7, ["WA"] = 7, ["WY"] = 7,
        ["MI"] = 8, ["OH"] = 8, ["WV"] = 8,
        ["IL"] = 9, ["IN"] = 9, ["WI"] = 9,
        ["CO"] = 0, ["IA"] = 0, ["KS"] = 0, ["MN"] = 0, ["MO"] = 0,
        ["NE"] = 0, ["ND"] = 0, ["SD"] = 0,
    };

    /// <summary>Full state names, for a location typed out in words.</summary>
    private static readonly Dictionary<string, string> StateNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CONNECTICUT"] = "CT", ["MAINE"] = "ME", ["MASSACHUSETTS"] = "MA",
        ["NEW HAMPSHIRE"] = "NH", ["RHODE ISLAND"] = "RI", ["VERMONT"] = "VT",
        ["NEW JERSEY"] = "NJ", ["NEW YORK"] = "NY", ["DELAWARE"] = "DE",
        ["MARYLAND"] = "MD", ["PENNSYLVANIA"] = "PA", ["ALABAMA"] = "AL",
        ["FLORIDA"] = "FL", ["GEORGIA"] = "GA", ["KENTUCKY"] = "KY",
        ["NORTH CAROLINA"] = "NC", ["SOUTH CAROLINA"] = "SC",
        ["TENNESSEE"] = "TN", ["VIRGINIA"] = "VA", ["ARKANSAS"] = "AR",
        ["LOUISIANA"] = "LA", ["MISSISSIPPI"] = "MS", ["NEW MEXICO"] = "NM",
        ["OKLAHOMA"] = "OK", ["TEXAS"] = "TX", ["CALIFORNIA"] = "CA",
        ["ARIZONA"] = "AZ", ["IDAHO"] = "ID", ["MONTANA"] = "MT",
        ["NEVADA"] = "NV", ["OREGON"] = "OR", ["UTAH"] = "UT",
        ["WASHINGTON"] = "WA", ["WYOMING"] = "WY", ["MICHIGAN"] = "MI",
        ["OHIO"] = "OH", ["WEST VIRGINIA"] = "WV", ["ILLINOIS"] = "IL",
        ["INDIANA"] = "IN", ["WISCONSIN"] = "WI", ["COLORADO"] = "CO",
        ["IOWA"] = "IA", ["KANSAS"] = "KS", ["MINNESOTA"] = "MN",
        ["MISSOURI"] = "MO", ["NEBRASKA"] = "NE", ["NORTH DAKOTA"] = "ND",
        ["SOUTH DAKOTA"] = "SD",
    };

    /// <summary>
    /// The operator's US call district from a free-text location, or null when
    /// no US state can be read out of it.
    /// </summary>
    /// <param name="location">Whatever the operator typed, e.g.
    /// "Pittsburgh, PA" or "Pittsburgh, Pennsylvania".</param>
    /// <returns>District 0–9, or null.</returns>
    public static int? HomeDistrict(string? location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return null;
        }

        var text = location.Trim();

        // Full state names first: "Washington" must not be shadowed by a
        // two-letter scan finding some other pair of letters.
        foreach (var pair in StateNames)
        {
            if (text.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
            {
                return StateDistricts[pair.Value];
            }
        }

        foreach (var token in text.Split(
            new[] { ' ', ',', '.', ';', '/', '\t' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Length == 2 && StateDistricts.TryGetValue(token, out var district))
            {
                return district;
            }
        }

        return null;
    }

    /// <summary>
    /// Convert a Maidenhead locator to the center of the square it names.
    /// </summary>
    /// <param name="grid">Four or six characters, e.g. "EN90" or "FN00aa".</param>
    /// <returns>The center point, or null when the locator is malformed.</returns>
    public static LatLon? FromGrid(string? grid)
    {
        if (string.IsNullOrWhiteSpace(grid))
        {
            return null;
        }

        var g = grid.Trim().ToUpperInvariant();
        if (g.Length < 4
            || g[0] is < 'A' or > 'R' || g[1] is < 'A' or > 'R'
            || !char.IsAsciiDigit(g[2]) || !char.IsAsciiDigit(g[3]))
        {
            return null;
        }

        var lon = ((g[0] - 'A') * 20.0) - 180.0 + ((g[2] - '0') * 2.0);
        var lat = ((g[1] - 'A') * 10.0) - 90.0 + (g[3] - '0');

        // Six-character locators refine to a subsquare; four-character ones
        // resolve to the center of the whole square.
        if (g.Length >= 6 && g[4] is >= 'A' and <= 'X' && g[5] is >= 'A' and <= 'X')
        {
            lon += (g[4] - 'A') * (2.0 / 24.0) + (1.0 / 24.0);
            lat += (g[5] - 'A') * (1.0 / 24.0) + (0.5 / 24.0);
        }
        else
        {
            lon += 1.0;
            lat += 0.5;
        }

        return new LatLon(lat, lon);
    }

    /// <summary>
    /// Convert coordinates to a six-character Maidenhead locator.
    /// </summary>
    /// <param name="point">The position.</param>
    /// <returns>Six characters, e.g. "FN00DJ".</returns>
    /// <remarks>
    /// <para>"Maidenhead grid locator" is exactly the kind of jargon Hamlet
    /// exists to dissolve (HM-DEC-037). The operator should never have to look
    /// theirs up: the callsign lookup already returns coordinates, and turning
    /// those into a locator is arithmetic — no service, no key, nothing to be
    /// down.</para>
    /// <para>Six characters rather than four because the extra precision is
    /// free and it takes the square from about seventy miles across to about
    /// three, which matters once distances are being drawn from it.</para>
    /// <para>The scheme: the world is divided into 18 fields of 20° longitude
    /// by 10° latitude lettered A–R from the antimeridian and the south pole,
    /// each field into 10 by 10 numbered squares, each square into 24 by 24
    /// lettered subsquares. Everything is measured from −180° and −90°, so the
    /// hemispheres need no special case — which is the part that usually gets
    /// this wrong, and the reason the tests cross both.</para>
    /// </remarks>
    public static string ToGrid(LatLon point)
    {
        // Clamp rather than throw: a coordinate a hair outside the range is a
        // rounding artifact from somebody else's data, not a reason to have no
        // locator at all.
        var lon = Math.Clamp(point.Longitude, -180.0, 179.999999) + 180.0;
        var lat = Math.Clamp(point.Latitude, -90.0, 89.999999) + 90.0;

        var lonField = (int)(lon / 20.0);
        var latField = (int)(lat / 10.0);

        lon -= lonField * 20.0;
        lat -= latField * 10.0;

        var lonSquare = (int)(lon / 2.0);
        var latSquare = (int)lat;

        lon -= lonSquare * 2.0;
        lat -= latSquare;

        var lonSub = (int)(lon / (2.0 / 24.0));
        var latSub = (int)(lat / (1.0 / 24.0));

        return string.Create(6, (lonField, latField, lonSquare, latSquare, lonSub, latSub), (span, p) =>
        {
            span[0] = (char)('A' + p.lonField);
            span[1] = (char)('A' + p.latField);
            span[2] = (char)('0' + p.lonSquare);
            span[3] = (char)('0' + p.latSquare);
            span[4] = (char)('A' + Math.Clamp(p.lonSub, 0, 23));
            span[5] = (char)('A' + Math.Clamp(p.latSub, 0, 23));
        });
    }

    /// <summary>
    /// Initial great-circle bearing from one point to another, in degrees
    /// clockwise from true north.
    /// </summary>
    /// <param name="from">Where the operator is.</param>
    /// <param name="to">Where the station is.</param>
    /// <returns>0 to 360 degrees.</returns>
    public static double BearingDegrees(LatLon from, LatLon to)
    {
        var lat1 = ToRadians(from.Latitude);
        var lat2 = ToRadians(to.Latitude);
        var dLon = ToRadians(to.Longitude - from.Longitude);

        var y = Math.Sin(dLon) * Math.Cos(lat2);
        var x = (Math.Cos(lat1) * Math.Sin(lat2))
                - (Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon));

        var degrees = ToDegrees(Math.Atan2(y, x));
        return (degrees + 360.0) % 360.0;
    }

    /// <summary>
    /// A bearing in the words somebody would actually use.
    /// </summary>
    /// <param name="bearingDegrees">Degrees clockwise from north.</param>
    /// <returns>"northeast", "south-southwest" and so on.</returns>
    /// <remarks>
    /// Sixteen points rather than a number, because "480 miles northeast"
    /// is a direction a person can picture and "480 miles at 47°" is a reading
    /// off an instrument (§0.7). The extra precision would be spurious anyway:
    /// this is the bearing to a park's reference point, not to an antenna.
    /// </remarks>
    public static string DescribeCompass(double bearingDegrees)
    {
        string[] points =
        {
            "north", "north-northeast", "northeast", "east-northeast",
            "east", "east-southeast", "southeast", "south-southeast",
            "south", "south-southwest", "southwest", "west-southwest",
            "west", "west-northwest", "northwest", "north-northwest",
        };

        var normalized = ((bearingDegrees % 360.0) + 360.0) % 360.0;
        var index = (int)Math.Round(normalized / 22.5) % 16;

        return points[index];
    }

    /// <summary>
    /// True when a locator falls inside North America.
    /// </summary>
    /// <param name="grid">Maidenhead locator.</param>
    /// <returns>True for a North American locator; false for anywhere else or
    /// for a malformed locator.</returns>
    /// <remarks>
    /// A coarse bounding box, and named as such: it is used only to decide
    /// whether continent-grain filtering should keep North American reporting
    /// stations, never to describe where the operator is.
    /// </remarks>
    public static bool IsNorthAmerica(string? grid)
    {
        var point = FromGrid(grid);
        return point is not null
            && point.Value.Longitude is >= -170 and <= -50
            && point.Value.Latitude is >= 7 and <= 85;
    }

    /// <summary>
    /// Format a locator's own text back for display, or an empty string.
    /// </summary>
    /// <param name="grid">Maidenhead locator.</param>
    /// <returns>The trimmed, upper-cased locator, or "".</returns>
    public static string Normalize(string? grid)
        => string.IsNullOrWhiteSpace(grid)
            ? string.Empty
            : grid.Trim().ToUpperInvariant();

    /// <summary>Great-circle distance in kilometers between two points.</summary>
    /// <param name="a">First point.</param>
    /// <param name="b">Second point.</param>
    /// <returns>Distance in kilometers.</returns>
    public static double DistanceKm(LatLon a, LatLon b)
    {
        const double earthRadiusKm = 6371.0;
        var dLat = ToRadians(b.Latitude - a.Latitude);
        var dLon = ToRadians(b.Longitude - a.Longitude);
        var lat1 = ToRadians(a.Latitude);
        var lat2 = ToRadians(b.Latitude);

        var h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);

        return 2 * earthRadiusKm * Math.Asin(Math.Min(1.0, Math.Sqrt(h)));
    }

    /// <summary>Render a distance for display, e.g. "480 km".</summary>
    /// <param name="km">Distance in kilometers.</param>
    /// <returns>Rounded distance with its unit.</returns>
    public static string DescribeDistance(double km)
        => Math.Round(km).ToString("0", CultureInfo.InvariantCulture) + " km";

    /// <summary>Kilometers to statute miles.</summary>
    /// <param name="km">Distance in kilometers.</param>
    /// <returns>Distance in miles.</returns>
    public static double ToMiles(double km) => km / 1.609344;

    /// <summary>
    /// Render a distance in the operator's units, rounded to the precision the
    /// figure actually deserves.
    /// </summary>
    /// <param name="km">Distance in kilometers.</param>
    /// <param name="miles">True for statute miles, false for kilometers.</param>
    /// <returns>e.g. "480 miles" or "770 km".</returns>
    /// <remarks>
    /// Rounded to two significant figures above a hundred. The underlying
    /// number is a distance to a park's stated reference point, so "483 miles"
    /// would be claiming a precision nothing in the chain supports (§0.0).
    /// </remarks>
    public static string DescribeRange(double km, bool miles)
    {
        var value = miles ? ToMiles(km) : km;
        var unit = miles ? "miles" : "km";

        var rounded = value switch
        {
            < 10 => Math.Round(value),
            < 100 => Math.Round(value / 5) * 5,
            < 1000 => Math.Round(value / 10) * 10,
            _ => Math.Round(value / 50) * 50,
        };

        if (rounded < 1)
        {
            return miles ? "under a mile" : "under a kilometer";
        }

        return rounded.ToString("0", CultureInfo.InvariantCulture)
            + " " + (miles && rounded == 1 ? "mile" : unit);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    private static double ToDegrees(double radians) => radians * 180.0 / Math.PI;
}

namespace Hamlet.RadioEngine.Explore;

/// <summary>Which authority issued a callsign, at the coarsest useful grain.</summary>
public enum CallsignRegion
{
    /// <summary>The prefix was not recognized.</summary>
    Unknown,

    /// <summary>United States, including its territories.</summary>
    UnitedStates,

    /// <summary>Canada.</summary>
    Canada,

    /// <summary>A recognized prefix outside the US and Canada.</summary>
    Elsewhere,
}

/// <summary>Where a callsign was issued.</summary>
/// <param name="Region">Issuing authority at continent grain.</param>
/// <param name="UsDistrict">US call district 0–9 when
/// <paramref name="Region"/> is <see cref="CallsignRegion.UnitedStates"/>;
/// otherwise null.</param>
public readonly record struct CallsignOrigin(CallsignRegion Region, int? UsDistrict);

/// <summary>
/// Reads a callsign's prefix to place the station on the map, coarsely.
/// </summary>
/// <remarks>
/// <para>SCOPE, and why it is drawn here. This classifier answers one
/// question — "is the station that heard this plausibly hearing what I would
/// hear from here?" — and answers it at the only grain a callsign actually
/// supports: the issuing country, and for US calls the district digit.</para>
/// <para>A callsign does not say where its holder is standing. Calls are
/// portable across the whole country, an operator keeps their call after
/// moving, and RBN skimmers are frequently remote. So nothing here returns a
/// town or a state, and no caller may print one — the honest ceiling is
/// "a 3-land station", never "a station in Pittsburgh" (HM-DEC-009).</para>
/// <para>The US and Canadian prefix sets below are complete and closed, so a
/// prefix outside them is genuinely outside North America. Everything else
/// recognizable lands in <see cref="CallsignRegion.Elsewhere"/>, and anything
/// unparseable stays <see cref="CallsignRegion.Unknown"/> rather than being
/// swept into a bucket that flatters the filter.</para>
/// </remarks>
public static class CallsignRegions
{
    /// <summary>Suffixes after a slash that describe how someone is
    /// operating rather than where, e.g. <c>W3ABC/QRP</c>.</summary>
    private static readonly HashSet<string> OperationalSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "P", "M", "MM", "AM", "QRP", "A", "R", "B", "BCN", "LH",
    };

    /// <summary>Canadian prefixes (ISED allocations).</summary>
    private static readonly string[] CanadaPrefixes =
    {
        "VA", "VB", "VC", "VD", "VE", "VF", "VG", "VO", "VX", "VY", "CY", "CZ",
        "XJ", "XK", "XL", "XM", "XN", "XO",
    };

    /// <summary>
    /// Place a callsign. Never throws; unparseable input is
    /// <see cref="CallsignRegion.Unknown"/>.
    /// </summary>
    /// <param name="callsign">A callsign, optionally carrying an RBN skimmer
    /// decoration ("-#", "-2-#") or a portable suffix ("/P", "/QRP", "/8").</param>
    /// <returns>The issuing region and, for US calls, the district digit.</returns>
    public static CallsignOrigin Classify(string? callsign)
    {
        var call = Normalize(callsign);
        if (call.Length == 0)
        {
            return new CallsignOrigin(CallsignRegion.Unknown, null);
        }

        var (governing, districtOverride) = SplitPortable(call);

        // Shape first, prefix second. A word like "NOTACALL" begins with a
        // US prefix letter and would otherwise be filed as a US station with
        // no district — a guess dressed as a classification (HM-DEC-009).
        if (governing.Length == 0 || !LooksLikeACallsign(governing))
        {
            return new CallsignOrigin(CallsignRegion.Unknown, null);
        }

        if (IsUnitedStates(governing))
        {
            return new CallsignOrigin(
                CallsignRegion.UnitedStates, districtOverride ?? DistrictDigit(governing));
        }

        foreach (var prefix in CanadaPrefixes)
        {
            if (governing.StartsWith(prefix, StringComparison.Ordinal))
            {
                return new CallsignOrigin(CallsignRegion.Canada, null);
            }
        }

        // It has the shape of a call and is not US or Canadian, so it was
        // issued elsewhere. The US and Canadian prefix sets are closed, which
        // is what makes that conclusion safe rather than a default.
        return new CallsignOrigin(CallsignRegion.Elsewhere, null);
    }

    /// <summary>
    /// How close a reporting station is to an operator's own district.
    /// </summary>
    /// <param name="spotter">The station that reported the spot.</param>
    /// <param name="homeDistrict">The operator's US call district, or null
    /// when their location is unknown — then the best available answer is
    /// continent grain, which is what the caller gets.</param>
    /// <returns>Local, Continent, Distant, or Unknown.</returns>
    public static SpotProximity ProximityTo(string? spotter, int? homeDistrict)
    {
        var origin = Classify(spotter);

        switch (origin.Region)
        {
            case CallsignRegion.Unknown:
                return SpotProximity.Unknown;

            case CallsignRegion.Elsewhere:
                return SpotProximity.Distant;

            case CallsignRegion.Canada:
                return SpotProximity.Continent;

            case CallsignRegion.UnitedStates:
                if (homeDistrict is null || origin.UsDistrict is null)
                {
                    return SpotProximity.Continent;
                }

                return IsNeighboring(origin.UsDistrict.Value, homeDistrict.Value)
                    ? SpotProximity.Local
                    : SpotProximity.Continent;

            default:
                return SpotProximity.Unknown;
        }
    }

    /// <summary>
    /// True when two US call districts are the same or share a border, so a
    /// signal audible in one is plausibly audible in the other.
    /// </summary>
    /// <param name="district">The reporting station's district.</param>
    /// <param name="home">The operator's district.</param>
    /// <returns>True when the districts are the same or adjacent.</returns>
    /// <remarks>
    /// Adjacency is geographic, from the FCC district map: 1 New England,
    /// 2 NY/NJ, 3 PA/MD/DE/DC, 4 Southeast, 5 South Central, 6 California,
    /// 7 Northwest/Mountain, 8 MI/OH/WV, 9 IL/IN/WI, 0 Plains.
    /// </remarks>
    public static bool IsNeighboring(int district, int home)
    {
        if (district == home)
        {
            return true;
        }

        return home switch
        {
            0 => district is 5 or 7 or 9,
            1 => district is 2,
            2 => district is 1 or 3,
            3 => district is 2 or 4 or 8,
            4 => district is 3 or 5 or 8 or 9,
            5 => district is 0 or 4 or 6 or 7 or 9,
            6 => district is 5 or 7,
            7 => district is 0 or 5 or 6,
            8 => district is 3 or 4 or 9,
            9 => district is 0 or 4 or 5 or 8,
            _ => false,
        };
    }

    private static string Normalize(string? callsign)
    {
        if (string.IsNullOrWhiteSpace(callsign))
        {
            return string.Empty;
        }

        var call = callsign.Trim().ToUpperInvariant();

        // RBN decorates skimmer calls: "WE9V-#", "DL8LAS-3-#".
        var dash = call.IndexOf('-');
        if (dash >= 0)
        {
            call = call[..dash];
        }

        return call;
    }

    /// <summary>
    /// Reduce a possibly-portable call to the part that governs its location,
    /// plus a district override for the <c>W3ABC/8</c> form.
    /// </summary>
    private static (string Governing, int? DistrictOverride) SplitPortable(string call)
    {
        if (!call.Contains('/', StringComparison.Ordinal))
        {
            return (call, null);
        }

        var parts = call.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return (string.Empty, null);
        }

        string? governing = null;
        int? districtOverride = null;

        foreach (var part in parts)
        {
            if (OperationalSuffixes.Contains(part))
            {
                continue;
            }

            // "W3ABC/8" — operating from another district.
            if (part.Length == 1 && char.IsAsciiDigit(part[0]))
            {
                districtOverride = part[0] - '0';
                continue;
            }

            // "EA8/DF4UE" — the location prefix leads and wins over the home
            // call that follows it.
            if (governing is null || part.Length < governing.Length)
            {
                governing = part;
            }
        }

        return (governing ?? parts[0], districtOverride);
    }

    /// <summary>
    /// US prefixes: K, N, W and AA–AL. "A" alone is not enough — AM, AN and
    /// AO through AZ belong to Spain and other administrations.
    /// </summary>
    private static bool IsUnitedStates(string call)
    {
        var first = call[0];

        if (first is 'K' or 'N' or 'W')
        {
            return true;
        }

        return first == 'A' && call.Length > 1 && call[1] is >= 'A' and <= 'L';
    }

    /// <summary>The first digit in a US call is its district.</summary>
    private static int? DistrictDigit(string call)
    {
        foreach (var c in call)
        {
            if (char.IsAsciiDigit(c))
            {
                return c - '0';
            }
        }

        return null;
    }

    private static bool LooksLikeACallsign(string call)
    {
        var sawLetter = false;
        var sawDigit = false;

        foreach (var c in call)
        {
            if (char.IsAsciiLetter(c))
            {
                sawLetter = true;
            }
            else if (char.IsAsciiDigit(c))
            {
                sawDigit = true;
            }
            else
            {
                return false;
            }
        }

        return sawLetter && sawDigit && call.Length >= 3;
    }
}

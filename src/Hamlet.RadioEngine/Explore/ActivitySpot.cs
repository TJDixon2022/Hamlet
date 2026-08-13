namespace Hamlet.RadioEngine.Explore;

/// <summary>What kind of transmission a spot reports.</summary>
/// <remarks>
/// Ranking treats these very differently (HM-DEC-025): a CQ is an open
/// invitation, a contest exchange is a closed conversation running at speed,
/// and a beacon answers nobody. A source that does not say gets
/// <see cref="Unknown"/> rather than a flattering guess (HM-DEC-009).
/// </remarks>
public enum SpotCallType
{
    /// <summary>The source did not say. Never assumed to be a CQ.</summary>
    Unknown,

    /// <summary>Calling CQ — actively looking for anyone to answer.</summary>
    Cq,

    /// <summary>An unattended beacon. Nothing to work.</summary>
    Beacon,

    /// <summary>A contest exchange: fast, formulaic, hard for a newcomer.</summary>
    Contest,

    /// <summary>Reported as DX activity without a call type.</summary>
    Dx,
}

/// <summary>
/// How close the station reporting a spot is to this operator.
/// </summary>
/// <remarks>
/// Derived from the reporting station's callsign prefix, which is coarse on
/// purpose. A callsign says which country and US call district issued it, not
/// where its owner is standing today, so nothing here claims a town or a
/// state — see <see cref="CallsignRegions"/>.
/// </remarks>
public enum SpotProximity
{
    /// <summary>Not determined — no reporting station, or an unknown prefix.</summary>
    Unknown,

    /// <summary>Same or neighboring call district: reliably audible ground.</summary>
    Local,

    /// <summary>Same continent.</summary>
    Continent,

    /// <summary>Another continent.</summary>
    Distant,
}

/// <summary>One thing happening on the air, told in plain language.</summary>
/// <param name="Story">The invitation, e.g. "Park activation in Ohio —
/// calling CQ in Morse at 15 WPM".</param>
/// <param name="FrequencyHz">Where to tune.</param>
/// <param name="Mode">Mode name, matching a field-guide entry when possible.</param>
/// <param name="Source">Which network reported it (RBN, POTA, fixture…).
/// A spot is a third party's claim; the source is shown, per the prime
/// directive.</param>
/// <param name="HeardAtUtc">When it was reported. Age is shown, never hidden —
/// a stale spot dressed as live is a lie.</param>
/// <param name="Wpm">CW speed when known; the hook for copy-speed filtering
/// (FG-002).</param>
/// <remarks>
/// The six positional fields are what every source can supply. The init-only
/// properties below are what ranking wants and only some sources know — each
/// one is nullable or has an explicit "unknown" member, so a source that
/// cannot say leaves it unsaid instead of filling in something plausible
/// (HM-DEC-009).
/// </remarks>
public sealed record ActivitySpot(
    string Story, long FrequencyHz, string Mode, string Source,
    DateTime HeardAtUtc, int? Wpm)
{
    /// <summary>What kind of call this is, when the source says.</summary>
    public SpotCallType CallType { get; init; } = SpotCallType.Unknown;

    /// <summary>Signal-to-noise in dB as reported by the receiving station,
    /// when the source measures it. RBN does; POTA and SOTA do not.</summary>
    public int? SignalDb { get; init; }

    /// <summary>The station being spotted, when known.</summary>
    public string? DxCall { get; init; }

    /// <summary>The station that reported it — an RBN skimmer, or the person
    /// who posted a POTA/SOTA spot.</summary>
    public string? SpotterCall { get; init; }

    /// <summary>How close the reporting station is (see
    /// <see cref="SpotProximity"/>).</summary>
    public SpotProximity Proximity { get; init; } = SpotProximity.Unknown;

    /// <summary>True for a POTA park or SOTA summit activation: an operator
    /// who set out wanting to be found.</summary>
    public bool IsActivation { get; init; }

    /// <summary>Park or summit reference, e.g. "US-1234" or "W3/PW-003".</summary>
    public string? Reference { get; init; }

    /// <summary>Where the activation is, as the source states it — e.g.
    /// "US-PA" or the SOTA association code. Never inferred from a
    /// callsign.</summary>
    public string? PlaceLabel { get; init; }

    /// <summary>
    /// How many receivers reported this same transmission, when the source
    /// can say. RBN can: a station heard by a dozen skimmers is being heard
    /// widely, which is the closest thing to "you will probably hear it too"
    /// that a spot network can honestly offer.
    /// </summary>
    public int? ReportCount { get; init; }
}

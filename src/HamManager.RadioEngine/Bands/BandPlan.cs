namespace HamManager.RadioEngine.Bands;

/// <summary>One amateur band with its CW segment and the app's jump-to spot.</summary>
/// <param name="Name">Display name, e.g. "40 m".</param>
/// <param name="LowHz">Band lower edge in hertz.</param>
/// <param name="HighHz">Band upper edge in hertz.</param>
/// <param name="CwLowHz">CW segment lower edge in hertz.</param>
/// <param name="CwHighHz">CW segment upper edge in hertz.</param>
/// <param name="JumpHz">Where a band button lands: the CW watering hole.</param>
public sealed record CwBand(
    string Name, long LowHz, long HighHz, long CwLowHz, long CwHighHz, long JumpHz)
{
    /// <summary>True when <paramref name="frequencyHz"/> lies inside this
    /// band's CW segment.</summary>
    public bool IsInCwSegment(long frequencyHz)
        => frequencyHz >= CwLowHz && frequencyHz <= CwHighHz;

    /// <summary>Clamp a frequency to this band's edges.</summary>
    public long Clamp(long frequencyHz)
        => Math.Min(HighHz, Math.Max(LowHz, frequencyHz));
}

/// <summary>
/// The HF CW band plan and the time-of-day band advisor.
/// </summary>
/// <remarks>
/// SOURCE MARKING (§0, §4): US amateur allocations per the ARRL band plan,
/// carried from general knowledge — [extrapolated], not yet a source-marked
/// data file. HM-OPEN-005 tracks moving this into /data with citations and
/// per-license-class privileges. CW segments below are the full-band CW/data
/// allocations; jump spots are QRP/activity conventions.
/// </remarks>
public static class BandPlan
{
    /// <summary>Phase 1 bands, lowest first.</summary>
    public static IReadOnlyList<CwBand> Bands { get; } = new[]
    {
        new CwBand("80 m", 3_500_000, 4_000_000, 3_500_000, 3_600_000, 3_530_000),
        new CwBand("40 m", 7_000_000, 7_300_000, 7_000_000, 7_125_000, 7_030_000),
        new CwBand("30 m", 10_100_000, 10_150_000, 10_100_000, 10_150_000, 10_110_000),
        new CwBand("20 m", 14_000_000, 14_350_000, 14_000_000, 14_150_000, 14_030_000),
        new CwBand("17 m", 18_068_000, 18_168_000, 18_068_000, 18_110_000, 18_080_000),
        new CwBand("15 m", 21_000_000, 21_450_000, 21_000_000, 21_200_000, 21_030_000),
        new CwBand("10 m", 28_000_000, 29_700_000, 28_000_000, 28_300_000, 28_030_000),
    };

    /// <summary>The band containing <paramref name="frequencyHz"/>, or null.</summary>
    public static CwBand? BandFor(long frequencyHz)
        => Bands.FirstOrDefault(b => frequencyHz >= b.LowHz && frequencyHz <= b.HighHz);

    /// <summary>
    /// Ranked best-bet band names for CW at the given local hour. A crude
    /// propagation heuristic — low bands at night, high bands by day — that
    /// FG-001 replaces with live spot data. Hour is a parameter, not a clock
    /// read, so this is deterministic (§5).
    /// </summary>
    /// <param name="localHour">Local hour, 0–23.</param>
    /// <exception cref="ArgumentOutOfRangeException">Hour outside 0–23.</exception>
    public static IReadOnlyList<string> BestBets(int localHour)
    {
        if (localHour is < 0 or > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(localHour), localHour, "0-23.");
        }

        return localHour switch
        {
            >= 21 or < 6 => new[] { "80 m", "40 m" },
            >= 17 => new[] { "40 m", "80 m" },
            >= 9 => new[] { "20 m", "15 m" },
            _ => new[] { "40 m", "20 m" },
        };
    }
}

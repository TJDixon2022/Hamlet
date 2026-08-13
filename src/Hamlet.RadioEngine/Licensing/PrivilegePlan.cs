using Hamlet.RadioEngine.Bands;

namespace Hamlet.RadioEngine.Licensing;

/// <summary>Why a frequency and mode are or are not available.</summary>
public enum PrivilegeStatus
{
    /// <summary>The license class is not known, so nothing is claimed.</summary>
    Unknown,

    /// <summary>The operator may transmit here in this mode.</summary>
    Allowed,

    /// <summary>The class may not transmit on this frequency at all.</summary>
    OutsideClassBand,

    /// <summary>The class may transmit here, but not in this mode.</summary>
    ModeNotAuthorised,
}

/// <summary>The answer to "may I transmit here, in this mode?".</summary>
/// <param name="Status">What the regulation says.</param>
/// <param name="Explanation">Plain language, for the operator.</param>
/// <param name="Citation">The paragraph that decided it, or "".</param>
/// <param name="LowestClassThatCould">
/// The weakest class that could transmit here in this mode, or
/// <see cref="LicenseClass.Unknown"/> when no class could.
/// </param>
public sealed record PrivilegeVerdict(
    PrivilegeStatus Status,
    string Explanation,
    string Citation,
    LicenseClass LowestClassThatCould)
{
    /// <summary>True only when transmitting is affirmatively permitted.</summary>
    /// <remarks>
    /// An unknown class is not permission. Everything that gates transmitting
    /// reads this rather than testing the status itself, so "we could not tell"
    /// can never be mistaken for "yes" (HM-DEC-009).
    /// </remarks>
    public bool MayTransmit => Status == PrivilegeStatus.Allowed;
}

/// <summary>
/// A contiguous stretch of a band and what the operator may do in it.
/// </summary>
/// <param name="LowHz">Lower edge.</param>
/// <param name="HighHz">Upper edge.</param>
/// <param name="MayTransmit">True when the class may transmit somewhere in it.</param>
/// <remarks>
/// THE ONE SET OF BOUNDARIES. The band map draws its listen-only veil from
/// these spans, and anything else that ever shows privileges — the waterfall,
/// the dial tape — takes the same list rather than computing its own. Two
/// renderings of the same law that disagreed would be worse than either alone
/// (HM-DEC-029).
/// </remarks>
public readonly record struct PrivilegeSpan(long LowHz, long HighHz, bool MayTransmit)
{
    /// <summary>True when the frequency lies inside this span.</summary>
    /// <param name="hz">Frequency in hertz.</param>
    /// <returns>True when contained.</returns>
    public bool Contains(long hz) => hz >= LowHz && hz < HighHz;
}

/// <summary>
/// Answers what a license class may transmit, from the cited Part 97 data.
/// </summary>
/// <remarks>
/// <para>This is the join the data file deliberately does not contain: 97.301
/// says which frequencies a class may use, 97.305(c) says where each emission
/// may be sent, and 97.307(f) qualifies both. Doing it here, in code with
/// tests, keeps the data file a straight transcription that can be audited
/// against the regulation line by line (§0, HM-DEC-029).</para>
/// <para>Pure and clock-free: same class, frequency and mode in, same verdict
/// out (§5).</para>
/// </remarks>
public sealed class PrivilegePlan
{
    /// <summary>
    /// Standards that restrict Novice and Technician to CW on a segment.
    /// </summary>
    private const string CwOnlyStandard = "97.307(f)(9)";

    /// <summary>Standard allowing Novice/Technician CW or phone only.</summary>
    private const string CwOrPhoneStandard = "97.307(f)(10)";

    /// <summary>
    /// Standard limiting phone to stations outside the contiguous US.
    /// </summary>
    private const string OutsideContiguousUsStandard = "97.307(f)(11)";

    private static readonly LicenseClass[] ByPrivilege =
    {
        LicenseClass.Novice, LicenseClass.Technician, LicenseClass.General,
        LicenseClass.Advanced, LicenseClass.Extra,
    };

    private readonly PrivilegeData _data;

    /// <summary>Creates a plan over the shipped data.</summary>
    public PrivilegePlan()
        : this(PrivilegeData.Current)
    {
    }

    /// <summary>Creates a plan over supplied data.</summary>
    /// <param name="data">The privileges data.</param>
    public PrivilegePlan(PrivilegeData data) => _data = data;

    /// <summary>The data behind this plan.</summary>
    public PrivilegeData Data => _data;

    /// <summary>
    /// May this class transmit here, in this mode?
    /// </summary>
    /// <param name="licenseClass">The operator's class.</param>
    /// <param name="frequencyHz">Frequency in hertz.</param>
    /// <param name="mode">What they would transmit.</param>
    /// <returns>The verdict, with its citation.</returns>
    public PrivilegeVerdict Evaluate(
        LicenseClass licenseClass, long frequencyHz, TransmitMode mode)
    {
        if (licenseClass == LicenseClass.Unknown)
        {
            return new PrivilegeVerdict(
                PrivilegeStatus.Unknown,
                "Hamlet does not know your license class, so it will not guess at "
                + "your privileges. Set it in Settings.",
                "",
                LowestClassFor(frequencyHz, mode));
        }

        if (!IsInClassBand(licenseClass, frequencyHz))
        {
            var lowest = LowestClassFor(frequencyHz, mode);
            return new PrivilegeVerdict(
                PrivilegeStatus.OutsideClassBand,
                lowest == LicenseClass.Unknown
                    ? "No US license class may transmit here."
                    : $"{Describe(licenseClass)} privileges do not reach this frequency; "
                      + $"it needs {Describe(lowest)}.",
                CiteFor(licenseClass),
                lowest);
        }

        var (allowed, citation, reason) = ModeAllowed(licenseClass, frequencyHz, mode);

        if (allowed)
        {
            return new PrivilegeVerdict(
                PrivilegeStatus.Allowed,
                $"Your {Describe(licenseClass)} license covers {Describe(mode)} here.",
                citation,
                LowestClassFor(frequencyHz, mode));
        }

        return new PrivilegeVerdict(
            PrivilegeStatus.ModeNotAuthorised,
            reason,
            citation,
            LowestClassFor(frequencyHz, mode));
    }

    /// <summary>
    /// True when the class may transmit at this frequency in some mode.
    /// </summary>
    /// <param name="licenseClass">The operator's class.</param>
    /// <param name="frequencyHz">Frequency in hertz.</param>
    /// <returns>True when any mode is permitted here.</returns>
    public bool MayTransmitAnyMode(LicenseClass licenseClass, long frequencyHz)
        => licenseClass != LicenseClass.Unknown && IsInClassBand(licenseClass, frequencyHz);

    /// <summary>
    /// Break a band into stretches the class may and may not transmit in.
    /// </summary>
    /// <param name="band">The band on screen.</param>
    /// <param name="licenseClass">The operator's class.</param>
    /// <returns>
    /// Contiguous spans covering the whole band, in ascending order. Empty
    /// when the class is unknown — the caller must then draw no overlay at
    /// all rather than a guessed one (HM-DEC-029).
    /// </returns>
    public IReadOnlyList<PrivilegeSpan> SpansFor(CwBand band, LicenseClass licenseClass)
    {
        if (licenseClass == LicenseClass.Unknown)
        {
            return Array.Empty<PrivilegeSpan>();
        }

        // Every class-band edge inside this band is a boundary; between two
        // adjacent edges the answer cannot change.
        var edges = new SortedSet<long> { band.LowHz, band.HighHz };

        if (_data.ClassBands.TryGetValue(licenseClass, out var ranges))
        {
            foreach (var range in ranges)
            {
                if (range.HighHz > band.LowHz && range.LowHz < band.HighHz)
                {
                    edges.Add(Math.Max(band.LowHz, range.LowHz));
                    edges.Add(Math.Min(band.HighHz, range.HighHz));
                }
            }
        }

        var points = edges.ToList();
        var spans = new List<PrivilegeSpan>(points.Count);

        for (var i = 0; i < points.Count - 1; i++)
        {
            var low = points[i];
            var high = points[i + 1];
            if (high <= low)
            {
                continue;
            }

            // Sample the middle: no boundary lies strictly inside a span.
            var probe = low + ((high - low) / 2);
            var may = MayTransmitAnyMode(licenseClass, probe);

            if (spans.Count > 0 && spans[^1].MayTransmit == may)
            {
                spans[^1] = spans[^1] with { HighHz = high };
            }
            else
            {
                spans.Add(new PrivilegeSpan(low, high, may));
            }
        }

        return spans;
    }

    /// <summary>
    /// The weakest class that could transmit here in this mode.
    /// </summary>
    /// <param name="frequencyHz">Frequency in hertz.</param>
    /// <param name="mode">The mode.</param>
    /// <returns>The class, or Unknown when none may.</returns>
    public LicenseClass LowestClassFor(long frequencyHz, TransmitMode mode)
    {
        foreach (var cls in ByPrivilege)
        {
            if (IsInClassBand(cls, frequencyHz) && ModeAllowed(cls, frequencyHz, mode).Allowed)
            {
                return cls;
            }
        }

        return LicenseClass.Unknown;
    }

    /// <summary>What the class above buys, for the upgrade panel.</summary>
    /// <param name="licenseClass">The operator's class.</param>
    /// <returns>The path, or null at the top or when unknown.</returns>
    public UpgradePath? UpgradeFrom(LicenseClass licenseClass)
        => _data.Upgrades.TryGetValue(licenseClass, out var path)
            && path.Next != LicenseClass.Unknown
            ? path
            : null;

    /// <summary>
    /// How much of a band a class may transmit on, as a fraction.
    /// </summary>
    /// <param name="band">The band.</param>
    /// <param name="licenseClass">The class.</param>
    /// <returns>0 to 1; 0 when the class is unknown.</returns>
    public double CoverageOf(CwBand band, LicenseClass licenseClass)
    {
        var spans = SpansFor(band, licenseClass);
        if (spans.Count == 0)
        {
            return 0;
        }

        double total = band.HighHz - band.LowHz;
        if (total <= 0)
        {
            return 0;
        }

        double allowed = spans.Where(s => s.MayTransmit).Sum(s => (double)(s.HighHz - s.LowHz));
        return allowed / total;
    }

    private bool IsInClassBand(LicenseClass licenseClass, long frequencyHz)
        => _data.ClassBands.TryGetValue(licenseClass, out var ranges)
           && ranges.Any(r => r.Contains(frequencyHz));

    /// <summary>
    /// Apply 97.305 and the 97.307(f) standards to one class and frequency.
    /// </summary>
    private (bool Allowed, string Citation, string Reason) ModeAllowed(
        LicenseClass licenseClass, long frequencyHz, TransmitMode mode)
    {
        var rows = _data.EmissionRanges.Where(r => r.Range.Contains(frequencyHz)).ToList();
        var novicish = licenseClass is LicenseClass.Novice or LicenseClass.Technician;

        if (mode == TransmitMode.Cw)
        {
            // 97.305(a): CW rides on the class frequency table alone.
            return (true, "97.305(a)", "");
        }

        foreach (var row in rows)
        {
            if (!row.Emissions.Contains(mode))
            {
                continue;
            }

            if (novicish && row.Standards.Contains(CwOnlyStandard))
            {
                return (false, CwOnlyStandard,
                    $"{Describe(licenseClass)} licensees may only send Morse here — "
                    + $"{Describe(mode)} on this segment needs General.");
            }

            if (novicish
                && row.Standards.Contains(CwOrPhoneStandard)
                && mode is not TransmitMode.Phone)
            {
                return (false, CwOrPhoneStandard,
                    $"{Describe(licenseClass)} licensees may only send Morse or voice here.");
            }

            if (row.Standards.Contains(OutsideContiguousUsStandard))
            {
                // 97.307(f)(11) limits this segment to stations west of 130 W
                // or south of 20 N. Hamlet does not assume the operator
                // qualifies, and says which rule decided it.
                return (false, OutsideContiguousUsStandard,
                    $"{Describe(mode)} on this segment is only for stations outside the "
                    + "contiguous US (west of 130° W or south of 20° N).");
            }

            return (true, row.Cite, "");
        }

        return (false, "97.305(c)",
            $"{Describe(mode)} is not authorised on this frequency for any class — "
            + "this part of the band is for other modes.");
    }

    private string CiteFor(LicenseClass licenseClass) => licenseClass switch
    {
        LicenseClass.Extra => "97.301(b)",
        LicenseClass.Advanced => "97.301(c)",
        LicenseClass.General => "97.301(d)",
        LicenseClass.Technician or LicenseClass.Novice => "97.301(e)",
        _ => "97.301",
    };

    /// <summary>The operator-facing name of a class.</summary>
    /// <param name="licenseClass">The class.</param>
    /// <returns>Its name, or "an unknown class".</returns>
    public static string Describe(LicenseClass licenseClass) => licenseClass switch
    {
        LicenseClass.Novice => "Novice",
        LicenseClass.Technician => "Technician",
        LicenseClass.General => "General",
        LicenseClass.Advanced => "Advanced",
        LicenseClass.Extra => "Extra",
        _ => "an unknown class",
    };

    /// <summary>The operator-facing name of a mode.</summary>
    /// <param name="mode">The mode.</param>
    /// <returns>Plain language, e.g. "voice".</returns>
    public static string Describe(TransmitMode mode) => mode switch
    {
        TransmitMode.Cw => "Morse",
        TransmitMode.Data => "digital modes",
        TransmitMode.Phone => "voice",
        TransmitMode.Image => "images",
        _ => "that mode",
    };
}

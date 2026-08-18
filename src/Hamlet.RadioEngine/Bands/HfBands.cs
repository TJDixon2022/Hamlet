using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;

namespace Hamlet.RadioEngine.Bands;

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
/// The HF bands Hamlet works, built from cited data (HM-DEC-110).
/// </summary>
/// <remarks>
/// <para>**THIS REPLACES `BandPlan`, WHICH CARRIED SEVEN BANDS OF LITERALS ITS
/// OWN COMMENT MARKED AS GENERAL KNOWLEDGE.** Two band plans in one tree is the
/// state §0 exists to prevent, and the uncited one had the friendlier name. It
/// mattered more than tidiness: §0.2.1 forbids frequencies asserted from a
/// model's memory, so the scanner had to be built around that class rather than
/// on it.</para>
/// <para>**THERE IS NO FREQUENCY LITERAL IN THIS FILE.** Every number is
/// derived:</para>
/// <list type="bullet">
/// <item><b>Band edges</b> from the Extra class ranges in 47 CFR 97.301(b),
/// which by definition reach every band edge. 80 m is the regulation's 80 m and
/// 75 m rows joined, which is the only join needed.</item>
/// <item><b>CW segments</b> from the union of the ranges carrying data in
/// 97.305(c). They match the old literals to the hertz, **which corrects
/// HM-OPEN-005's own claim** that they are convention rather than regulation and
/// do not align with the privilege boundaries.</item>
/// <item><b>Jump spots</b> from the first "CW main street" block in
/// <c>data/bands/us-neighborhoods.json</c> (HM-DEC-110).</item>
/// </list>
/// <para>**THE NEIGHBORHOOD FILE IS NOT THE SOURCE FOR THE SEGMENTS** and must
/// not become one. Its Morse rows fall short at the top of every band, by 10 kHz
/// on 17 m up to 230 kHz on 10 m, with a hole on 40 m between 7.040 and 7.050.
/// That is not a defect in it: those rows are conventions somebody published and
/// the space between belongs to nobody (HM-DEC-054). A CW segment is a
/// regulatory boundary, and the privileges file is where regulation lives.</para>
/// </remarks>
public static class HfBands
{
    /// <summary>The name of the block a band button lands in (HM-DEC-110).</summary>
    /// <remarks>
    /// **THE MAIN STREET RATHER THAN THE QRP WATERING HOLE.** Five of the seven
    /// spots already were this block, so it moves the fewest dials, and a
    /// watering hole is a narrower slice than a segment's main street, which is
    /// the wrong place to aim somebody who has never called anybody.
    /// </remarks>
    public const string LandingBlock = "CW main street";

    /// <summary>Which bands Hamlet offers, lowest first.</summary>
    /// <remarks>
    /// The seven HF bands with a CW segment a beginner would work. 160 m, 60 m
    /// and 12 m are deliberately absent, and that is a scope decision rather than
    /// a data one: 60 m is channelized, and the other two wait on a ruling rather
    /// than on a citation.
    /// </remarks>
    public static IReadOnlyList<string> Names { get; } = new[]
    {
        "80 m", "40 m", "30 m", "20 m", "17 m", "15 m", "10 m",
    };

    private static readonly Lazy<IReadOnlyList<CwBand>> Built = new(Build);

    /// <summary>Phase 1 bands, lowest first.</summary>
    public static IReadOnlyList<CwBand> Bands => Built.Value;

    /// <summary>The band containing <paramref name="frequencyHz"/>, or null.</summary>
    public static CwBand? BandFor(long frequencyHz)
        => Bands.FirstOrDefault(
            b => frequencyHz >= b.LowHz && frequencyHz <= b.HighHz);

    /// <summary>
    /// Ranked best-bet band names for CW at the given local hour. A crude
    /// propagation heuristic, low bands at night and high bands by day, that
    /// FG-001 replaces with live spot data. Hour is a parameter rather than a
    /// clock read, so this is deterministic (§5).
    /// </summary>
    /// <param name="localHour">Local hour, 0 to 23.</param>
    /// <exception cref="ArgumentOutOfRangeException">Hour outside 0 to 23.</exception>
    /// <remarks>
    /// **EDITORIAL, AND MARKED AS SUCH.** It is the one thing here not derived
    /// from a citation, because there is nothing to cite: it is a rule of thumb
    /// about the ionosphere, it is already demoted to a tiebreaker for when
    /// nothing has been heard anywhere (HM-DEC-046), and it names bands rather
    /// than frequencies so it asserts no number at all.
    /// </remarks>
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

    private static IReadOnlyList<CwBand> Build()
    {
        var privileges = PrivilegeData.Current;
        var conventions = NeighborhoodData.Current;

        var extra = privileges.ClassBands[LicenseClass.Extra];
        var built = new List<CwBand>();

        foreach (var name in Names)
        {
            // **THE EDGES: THE EXTRA CLASS REACHES EVERY BAND EDGE BY
            // DEFINITION**, so its own ranges are the band. 80 m is the only one
            // needing two rows joined, because the regulation calls 3.6 to 4.0
            // "75 m" and everybody on the air calls the lot of it 80 m.
            var rows = extra.Where(r => Covers(name, r.Band)).ToList();

            if (rows.Count == 0)
            {
                continue;
            }

            var lowHz = rows.Min(r => r.LowHz);
            var highHz = rows.Max(r => r.HighHz);

            // **THE CW SEGMENT IS WHERE DATA MAY BE SENT.** CW itself is absent
            // from 97.305(c) on purpose, because 97.305(a) allows it anywhere the
            // control operator is authorized at all, so the data ranges are what
            // mark the bottom of a band off from the phone segment above it.
            var data = privileges.EmissionRanges
                .Where(r => r.Emissions.Contains(TransmitMode.Data))
                .Where(r => r.Range.LowHz >= lowHz && r.Range.HighHz <= highHz)
                .ToList();

            if (data.Count == 0)
            {
                continue;
            }

            var cwLowHz = data.Min(r => r.Range.LowHz);
            var cwHighHz = data.Max(r => r.Range.HighHz);

            built.Add(new CwBand(
                name,
                lowHz,
                highHz,
                cwLowHz,
                cwHighHz,
                Landing(conventions, name, cwLowHz, cwHighHz)));
        }

        return built;
    }

    /// <summary>Whether a regulation row belongs to one of Hamlet's bands.</summary>
    /// <remarks>
    /// The one join in the whole file, and it is named rather than buried in a
    /// table: the CFR splits 3.5 to 4.0 into an 80 m row and a 75 m row, and
    /// everybody on the air calls the lot of it 80 m.
    /// </remarks>
    private static bool Covers(string bandName, string rowBand)
        => rowBand == bandName || (bandName == "80 m" && rowBand == "75 m");

    /// <summary>Where a band button lands (HM-DEC-110).</summary>
    /// <remarks>
    /// The first Morse block called <see cref="LandingBlock"/> inside the CW
    /// segment. **A band with no such block gets the bottom of its own segment
    /// rather than a number from nowhere** (§0.0). That is a real frequency the
    /// operator may use, derived from the same citation as the segment, and it is
    /// the honest answer when the conventions have nothing to say.
    /// </remarks>
    private static long Landing(
        NeighborhoodData conventions, string bandName, long cwLowHz, long cwHighHz)
    {
        var block = conventions.ForBand(bandName)
            .FirstOrDefault(h => h.Family == ModeFamily.Cw
                                 && h.Name == LandingBlock
                                 && h.JumpHz >= cwLowHz
                                 && h.JumpHz <= cwHighHz);

        return block?.JumpHz ?? cwLowHz;
    }
}

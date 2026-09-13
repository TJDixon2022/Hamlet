using System.Globalization;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>What the green zone reports once a session, when it first has a size.</summary>
/// <param name="Width">The panel's width.</param>
/// <param name="Height">The panel's height.</param>
/// <param name="Regions">Which regions drew, comma-joined: `left,map,right`.</param>
public sealed record GreenZoneLayout(double Width, double Height, string Regions);

/// <summary>
/// **The panel under the neighborhood map: where you are, the world's clock, and what is
/// worth doing now.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"The green zone is wasting a lot of real estate."* Unit 331
/// compressed it to two lines, and he answered: *"I said this was wasted real estate on the
/// right so you just put more on the left."* So the panel is three regions across its whole
/// width (work instruction 332 task 2): the band large on the left, the world with its night
/// side in the middle, and the best bet and what has been heard on the right.</para>
/// <para>**NO BAND PILLS** (Tim, 2026-09-12: *"Too redundant. We don't need the repeat of the
/// band list on the green. Maybe make the map bigger."*; work instruction 334 task 1). The
/// band strip above the panel is the pills, and the map has the room they held.</para>
/// <para>**THE BAND COMES FIRST AND LARGEST**, which is Tim's own reason: *"I really didn't
/// know 14.070 was 20 meters until recently."*</para>
/// <para>**NOTHING HERE IS INVENTED AND NOTHING HERE IS COMPUTED TWICE** (§0.0, §0 on
/// generated-from-a-source-of-truth). Every value already exists on the screen: the license
/// words are `PrivilegeStatusLine`'s, the band is the band the dial is in from
/// <see cref="HfBands.Bands"/>, the family and sub-mode are the neighborhood's and the
/// tab's, the best bet is the ranking the band strip's pills already wear, and the count and
/// the sparkline are the map's own dots.</para>
/// <para>**BAND OPENNESS IS NEVER DRAWN OR SAID** (§0.0). The night side is the sun and the
/// clock, which is fact; whether a path is open is a forecast Hamlet has no source for. The
/// one sentence about propagation is <see cref="RuleOfThumb"/>, and it says it is one.</para>
/// <para>Pure: frequencies, a neighborhood, a ranking, a count and the bands in, strings out.
/// No clock and no state (§5).</para>
/// </remarks>
public sealed record GreenZone
{
    /// <summary>The separator every line here is built with.</summary>
    public const string Between = " · ";

    /// <summary>What the best-bet band wears when it is the band he is on.</summary>
    /// <remarks>
    /// **THE CHECK IS THE NON-COLOR CARRIER** (§0.6). *You are on the best band* and *the
    /// best band is elsewhere* differ by a mark before they differ by a hue, so the line
    /// survives a grayscale print and a color vision deficiency both.
    /// </remarks>
    public const string OnIt = " ✓";

    /// <summary>
    /// **The one line about propagation, stated as a rule of thumb** (§0.0).
    /// </summary>
    /// <remarks>
    /// <para>**THE AUTHOR'S WORDING, MARKED FOR THE OWNER** (work instruction 332's ARBITER block).
    /// It says what the sun tends to do to the bands and nothing about whether any band is
    /// open now, because Hamlet does not know.</para>
    /// <para>**SHORTENED TO THE MOCKUP'S OWN WORDS IN WORK INSTRUCTION 338** (the arbiter's
    /// ruling 2, overrulable; `assets/main-screen-mockup.png` draws exactly this sentence). The
    /// `Rule of thumb:` prefix and *the gray edge is where both happen* came off: at 1400 the
    /// green block's text column is about 220 px on the test host and the long line took six
    /// lines of it, which held a licensed operator's top row at 0.300 of the height below the
    /// band pills against the mockup's 0.262. `PHASE_PLAN.md` §6: *a string will not fit -
    /// shorten and say which.* The license line was not reworded; it is the regulation's
    /// sentence.</para>
    /// </remarks>
    public const string RuleOfThumb =
        "20 m and up want daylight along the path; 40 m and down want dark.";

    /// <summary>How many bins the minute is cut into for the sparkline: five seconds each.</summary>
    public const int SparklineBins = 12;

    /// <summary>Nothing known. The panel draws its license line and nothing else.</summary>
    public static GreenZone Empty { get; } = new();

    /// <summary>**The regulation, in the words it already used.**</summary>
    /// <remarks>
    /// The headline, the detail and the citation, joined. **The words are not rewritten** -
    /// the only edit is a trailing period dropped off the detail, because a sentence stop in
    /// the middle of a joined line reads as a mistake.
    /// </remarks>
    public string License { get; private init; } = "";

    /// <summary>True where there is a license line at all.</summary>
    public bool HasLicense => License.Length > 0;

    /// <summary>
    /// **The small line on the left: the license phrase and the citation**, without the
    /// frequency the region already shows beside the band.
    /// </summary>
    public string LicensePhrase { get; private init; } = "";

    /// <summary>True where there is a license phrase.</summary>
    public bool HasLicensePhrase => LicensePhrase.Length > 0;

    /// <summary>The verdict half of the headline - `yours to use` - or "".</summary>
    public string Verdict { get; private init; } = "";

    /// <summary>**The band the dial is in, and it comes first** - `20 m`.</summary>
    /// <remarks>
    /// **FROM THE CITED ROWS AND NOT FROM THE BAND HE PRESSED** (§0.0). Empty where the dial
    /// is not in an amateur band at all, which is not the same as a band Hamlet failed to
    /// name.
    /// </remarks>
    public string Band { get; private init; } = "";

    /// <summary>True where the dial is inside a band this record could name.</summary>
    public bool HasBand => Band.Length > 0;

    /// <summary>The dial, in megahertz with its unit - `14.074 MHz`.</summary>
    public string Frequency { get; private init; } = "";

    /// <summary>The family word, from the palette the legend uses - `Digital`.</summary>
    /// <remarks>
    /// **THE PALETTE'S OWN LABEL AND NOT A NEW WORD.** The word is the primary carrier and the
    /// family ink is the second (§0.6, §0.5 - family color is text only).
    /// </remarks>
    public string Family { get; private init; } = "";

    /// <summary>True where the dial is in a block with a family.</summary>
    public bool HasFamily => Family.Length > 0;

    /// <summary>Which family, for the ink. <see cref="ModeFamily.Open"/> where unknown.</summary>
    public ModeFamily FamilyOf { get; private init; } = ModeFamily.Open;

    /// <summary>The sub-mode under the tab - `FT8`, `PSK31`, `SSB` - or "".</summary>
    /// <remarks>
    /// **ABSENT RATHER THAN GUESSED** (§0.0). Morse has no sub-mode; a digital block whose
    /// mode the tab has not picked says the family and stops.
    /// </remarks>
    public string SubMode { get; private init; } = "";

    /// <summary>True where there is a sub-mode to name.</summary>
    public bool HasSubMode => SubMode.Length > 0;

    /// <summary>**Under the band: the mode and the verdict** - `Digital · FT8 · yours to use`.</summary>
    public string ModeLine
        => string.Join(Between, new[] { Family, SubMode, Verdict }.Where(s => s.Length > 0));

    /// <summary>True where there is a mode line.</summary>
    public bool HasModeLine => ModeLine.Length > 0;

    /// <summary>The band the pills call the best bet, plus its check where it is here.</summary>
    public string BestBet { get; private init; } = "";

    /// <summary>True where the ranking named a band at all.</summary>
    public bool HasBestBet => BestBet.Length > 0;

    /// <summary>True where the best bet is the band the dial is in.</summary>
    public bool BestBetIsHere { get; private init; }

    /// <summary>How many stations the map's own dots heard in the last minute.</summary>
    public string Heard { get; private init; } = "";

    /// <summary>True where there is a count to show.</summary>
    public bool HasHeard => Heard.Length > 0;

    /// <summary>**The strayed-segment nudge, and only where it is true.**</summary>
    public string Strayed { get; private init; } = "";

    /// <summary>True where the sub-mode's segment does not contain the dial.</summary>
    public bool HasStrayed => Strayed.Length > 0;

    /// <summary>The operator's grid for the map's one marker, or "".</summary>
    public string OperatorGrid { get; private init; } = "";

    /// <summary>Build the panel's lines.</summary>
    /// <param name="status">The license verdict, already built.</param>
    /// <param name="frequencyHz">Where the dial is.</param>
    /// <param name="here">The neighborhood the dial is in, or null.</param>
    /// <param name="subMode">The sub-mode under the tab, or null.</param>
    /// <param name="segment">
    /// The neighborhood that sub-mode lives in on this band, or null where the band has no
    /// block for it and there is therefore nothing to have strayed from.
    /// </param>
    /// <param name="bestBetBand">The band the ranking put first, or "".</param>
    /// <param name="heardInTheLastMinute">
    /// How many of the map's dots were heard inside the last minute, or null where the feed
    /// has never answered - which is absent rather than nought (§0.0).
    /// </param>
    /// <param name="operatorGrid">The operator's grid from Settings, or null.</param>
    /// <returns>The lines, ready to bind.</returns>
    public static GreenZone For(
        PrivilegeStatus status,
        long frequencyHz,
        Neighborhood? here,
        string? subMode,
        Neighborhood? segment,
        string bestBetBand,
        int? heardInTheLastMinute,
        string? operatorGrid = null)
    {
        var band = HfBands.Bands.FirstOrDefault(
            b => frequencyHz >= b.LowHz && frequencyHz <= b.HighHz);

        var family = here is { Family: not ModeFamily.OutsideTheBand }
            ? Controls.ModePalette.For(here.Family)
            : null;

        var picked = subMode?.Trim() ?? "";

        // **MORSE HAS NO SUB-MODE AND THE LINE SAYS SO BY SAYING NOTHING** - the tab's
        // pick is about digital modes, and carrying it into a CW block would be the tab
        // answering a question about the dial.
        var under = here is { Family: ModeFamily.Digital } ? picked : "";

        return new GreenZone
        {
            License = JoinLicense(status),
            LicensePhrase = JoinPhrase(status),
            Verdict = VerdictOf(status.Headline),
            Band = band?.Name ?? "",
            Frequency = Megahertz(frequencyHz) + " MHz",
            Family = family?.Label ?? "",
            FamilyOf = here?.Family ?? ModeFamily.Open,
            SubMode = under,
            BestBet = BestBetWords(bestBetBand, band?.Name ?? ""),
            BestBetIsHere = bestBetBand.Length > 0
                && string.Equals(bestBetBand, band?.Name, StringComparison.Ordinal),
            Heard = Stations(heardInTheLastMinute),
            Strayed = Stray(picked, segment, frequencyHz),
            OperatorGrid = OperatorLocation.Normalize(operatorGrid),
        };
    }

    /// <summary>
    /// **The last minute of heard stations, in five-second bins, oldest first.**
    /// </summary>
    /// <param name="heardAtUtc">When each of the map's dots was heard.</param>
    /// <param name="nowUtc">The clock reading the count was taken at.</param>
    /// <returns><see cref="SparklineBins"/> counts that add up to the heard count.</returns>
    /// <remarks>
    /// **THE SAME WINDOW AS THE COUNT**: a spot is in if it was heard no more than a minute
    /// before the reading, including one stamped a moment after it, which the count also
    /// includes. So the bins always add up to *heard just now*.
    /// </remarks>
    public static IReadOnlyList<int> Sparkline(IEnumerable<DateTime> heardAtUtc, DateTime nowUtc)
    {
        var bins = new int[SparklineBins];
        var width = 60.0 / SparklineBins;

        foreach (var at in heardAtUtc)
        {
            var age = (nowUtc - at).TotalSeconds;

            if (age > 60)
            {
                continue;
            }

            var back = Math.Min(SparklineBins - 1, (int)(Math.Max(0, age) / width));

            bins[SparklineBins - 1 - back]++;
        }

        return bins;
    }

    private static string JoinLicense(PrivilegeStatus status)
    {
        var said = new List<string>();

        if (status.Headline.Length > 0)
        {
            said.Add(status.Headline);
        }

        if (status.Detail.Length > 0)
        {
            said.Add(status.Detail.TrimEnd('.'));
        }

        if (status.Citation.Length > 0)
        {
            said.Add(status.Citation);
        }

        return string.Join(Between, said);
    }

    private static string JoinPhrase(PrivilegeStatus status)
        => string.Join(
            Between,
            new[] { status.Detail.TrimEnd('.'), status.Citation }.Where(s => s.Length > 0));

    /// <summary>`14.074 MHz · yours to use` gives `yours to use`; a bare frequency gives "".</summary>
    private static string VerdictOf(string headline)
    {
        var at = headline.IndexOf(Between, StringComparison.Ordinal);

        return at < 0 ? "" : headline[(at + Between.Length)..];
    }

    private static string BestBetWords(string bestBet, string bandHere)
        => bestBet.Length == 0
            ? ""
            : bestBet + (string.Equals(bestBet, bandHere, StringComparison.Ordinal)
                ? OnIt
                : "");

    private static string Stations(int? count)
        => count is null
            ? ""
            : count.Value.ToString(CultureInfo.InvariantCulture)
                + (count.Value == 1 ? " station" : " stations");

    /// <summary>The nudge, or "" where the dial is in the segment it names.</summary>
    private static string Stray(string subMode, Neighborhood? segment, long frequencyHz)
    {
        if (subMode.Length == 0 || segment is null || segment.Contains(frequencyHz))
        {
            return "";
        }

        return subMode + " lives at " + Megahertz(segment.JumpHz)
            + "; you are at " + Megahertz(frequencyHz);
    }

    private static string Megahertz(long hz)
        => (hz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture);
}

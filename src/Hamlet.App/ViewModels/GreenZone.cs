using System.Globalization;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **The panel under the neighborhood map: one line of license, and then what is true
/// right now.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"The green zone is wasting a lot of real estate."* What he
/// was looking at was three lines and a full-width panel saying
/// `14.074 MHz · yours to use` / `Your General license covers digital modes here.` /
/// `97.305(c)(3)(ix)` - and **saying the same thing every time**, because all three are
/// facts about the regulation and the regulation does not change while he operates.</para>
/// <para>**SO THE THREE BECOME ONE, AND THE LINE THEY MAKE ROOM FOR IS LIVE** (work
/// instruction 331 task 4). The band comes first on it, which is Tim's own reason: *"I
/// really didn't know 14.070 was 20 meters until recently."* A frequency and its band name
/// joined in one place is the thing he did not have.</para>
/// <para>**NOTHING HERE IS INVENTED AND NOTHING HERE IS COMPUTED TWICE** (§0.0, §0 on
/// generated-from-a-source-of-truth). Every value already exists on the screen: the license
/// words are `PrivilegeStatusLine`'s, the band is the band the dial is in from
/// <see cref="HfBands.Bands"/>, the family and sub-mode are the neighborhood's and the
/// tab's, the best bet is the ranking the band pills already wear, and the count is the
/// map's own dots. This record joins them and measures nothing of its own.</para>
/// <para>**A THIRD LINE ONLY WHEN IT IS TRUE.** Where the sub-mode he has picked lives in a
/// segment the dial is not in, the panel says so and says both numbers; where it does not,
/// the panel is a line shorter.</para>
/// <para>Pure: frequencies, a neighborhood, a ranking and a count in, strings out. No
/// clock and no state (§5).</para>
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

    /// <summary>Nothing known. The panel draws its license line and nothing else.</summary>
    public static GreenZone Empty { get; } = new();

    /// <summary>**Line one: the regulation, in the words it already used.**</summary>
    /// <remarks>
    /// The headline, the detail and the citation, joined. **The words are not rewritten** -
    /// the instruction says *the existing three lines on one line, in the same words* - and
    /// the only edit is a trailing period dropped off the detail, because a sentence stop in
    /// the middle of a joined line reads as a mistake.
    /// </remarks>
    public string License { get; private init; } = "";

    /// <summary>True where there is a license line at all.</summary>
    public bool HasLicense => License.Length > 0;

    /// <summary>**The band the dial is in, and it comes first** - `20 m`.</summary>
    /// <remarks>
    /// **FROM THE CITED ROWS AND NOT FROM THE BAND HE PRESSED** (§0.0). The pill he last
    /// pressed is a preference; the band containing the dial frequency is a measurement, and
    /// they differ every time a tune lands outside the band he came from. Empty where the
    /// dial is not in an amateur band at all, which is not the same as a band Hamlet failed
    /// to name.
    /// </remarks>
    public string Band { get; private init; } = "";

    /// <summary>True where the dial is inside a band this record could name.</summary>
    public bool HasBand => Band.Length > 0;

    /// <summary>The dial, in megahertz with its unit - `14.074 MHz`.</summary>
    public string Frequency { get; private init; } = "";

    /// <summary>The family word, from the palette the legend uses - `Digital`.</summary>
    /// <remarks>
    /// **THE PALETTE'S OWN LABEL AND NOT A NEW WORD.** The map legend teaches him four
    /// family names and this line uses one of them, so the two surfaces cannot come to call
    /// one family two things. The word is the primary carrier and the family ink is the
    /// second (§0.6, §0.5 - family color is text only).
    /// </remarks>
    public string Family { get; private init; } = "";

    /// <summary>True where the dial is in a block with a family.</summary>
    public bool HasFamily => Family.Length > 0;

    /// <summary>Which family, for the ink. <see cref="ModeFamily.Open"/> where unknown.</summary>
    public ModeFamily FamilyOf { get; private init; } = ModeFamily.Open;

    /// <summary>The sub-mode under the tab - `FT8`, `PSK31`, `SSB` - or "".</summary>
    /// <remarks>
    /// **ABSENT RATHER THAN GUESSED** (§0.0). Morse has no sub-mode and the line says `CW`
    /// alone; a digital block whose mode the tab has not picked says the family and stops.
    /// </remarks>
    public string SubMode { get; private init; } = "";

    /// <summary>True where there is a sub-mode to name.</summary>
    public bool HasSubMode => SubMode.Length > 0;

    /// <summary>The band the pills call the best bet, plus its check where it is here.</summary>
    public string BestBet { get; private init; } = "";

    /// <summary>True where the ranking named a band at all.</summary>
    public bool HasBestBet => BestBet.Length > 0;

    /// <summary>True where the best bet is the band the dial is in.</summary>
    /// <remarks>
    /// **THE NUDGE IS THE OTHER CASE AND IT IS PRESSABLE** - clicking it tunes there the way
    /// the pill does, because a line that names a better band and cannot take him to it
    /// makes him find the pill (§0.5.1).
    /// </remarks>
    public bool BestBetIsHere { get; private init; }

    /// <summary>How many stations the map's own dots heard in the last minute.</summary>
    public string Heard { get; private init; } = "";

    /// <summary>True where there is a count to show.</summary>
    public bool HasHeard => Heard.Length > 0;

    /// <summary>**The strayed-segment nudge, and only where it is true.**</summary>
    /// <remarks>
    /// `PSK31 lives at 14.070; you are at 14.074`. **Both numbers come off the cited band
    /// rows** - the sub-mode's own neighborhood and the dial - so neither is a frequency
    /// written in this file (§0.2.1).
    /// </remarks>
    public string Strayed { get; private init; } = "";

    /// <summary>True where the sub-mode's segment does not contain the dial.</summary>
    public bool HasStrayed => Strayed.Length > 0;

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
    /// <returns>The lines, ready to bind.</returns>
    public static GreenZone For(
        PrivilegeStatus status,
        long frequencyHz,
        Neighborhood? here,
        string? subMode,
        Neighborhood? segment,
        string bestBetBand,
        int? heardInTheLastMinute)
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
        };
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

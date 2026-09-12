using System.Globalization;
using Hamlet.RadioEngine.Contacts;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **One badge on the achievements page: a kind, its next card, and its score.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"I want to rework the achievements dialog. The opening
/// dialog page should be a list of badges indicating type of achievements. Nice badges,
/// real nice."* `assets/achievements-opening-mockup.png` is the shape he approved -
/// *"much better"* - drawn at mockup speed; **the application draws it as vector paths and
/// ships no image asset** (work instruction 331 task 6, and its section 10).</para>
/// <para>**THE NEXT CARD IS §3.1 BENT BY ONE RULING AND NO FURTHER** (ruling C,
/// 2026-09-12): the nearest unearned card in each kind is visible on night one, and
/// **nothing beyond it**. So a badge shows at most one thing he has not done, which is the
/// difference between a target and the wall of empty cards §2 forbids.</para>
/// <para>**NOTHING HERE HOLDS A POINT VALUE.** The score, the level and the gap all come
/// off <see cref="AchievementScores"/>, which reads the owner's file; where the file could
/// not be read the score is absent and the page says so in a line rather than drawing
/// nought (§0.0).</para>
/// <para>**AND NOTHING HERE SAYS *confirmed*** (`ACHIEVEMENTS_PHILOSOPHY.md` §4). Every
/// count is of contacts in his own log.</para>
/// <para>**A BADGE IS PRESSED TO OPEN ITS KIND** (Tim, 2026-09-12: *"When you click on a
/// category, that category replaces it. It's a click-into system."*). The same record draws
/// the seven continent badges inside Continents, where <see cref="Kind"/> is
/// `continent-EU` and the two corner lines are said rather than scored.</para>
/// </remarks>
/// <param name="Kind">One of <see cref="AchievementKinds.All"/>, or `continent-XX`.</param>
/// <param name="Name">What the band reads: `Hall of Fame`, `Total Miles`.</param>
/// <param name="Meaning">The one line under the band: `once-only firsts`.</param>
/// <param name="Emblem">Which emblem the control draws. See `BadgeEmblemControl`.</param>
/// <param name="Band">The color band's fill, as a hex string from the page's own set.</param>
/// <param name="NextCard">The one card he could earn next, in his own words.</param>
/// <param name="NextIsDoor">
/// True where the next card is a door - a first that opens a set - and false where it is a
/// counter. **The ring and the quill are the two forms** (§R16), so the difference is a
/// shape before it is a hue (§0.6).
/// </param>
/// <param name="Standing">
/// The corner line about the count: `0 worked`, `1 of 7`, `0 mi so far`.
/// </param>
/// <param name="Score">Where the kind stands, from the owner's file.</param>
public sealed record AchievementBadge(
    string Kind,
    string Name,
    string Meaning,
    string Emblem,
    string Band,
    string NextCard,
    bool NextIsDoor,
    string Standing,
    AchievementKindScore Score)
{
    /// <summary>True where there is a next card to show.</summary>
    public bool HasNextCard => NextCard.Length > 0;

    /// <summary>
    /// **The corner's score line**: `35 pts · Bronze · 3 to Silver`, or "" when absent.
    /// </summary>
    /// <remarks>
    /// **THE KIND'S NAME IS NOT IN IT.** The instruction's example reads
    /// `Countries · 35 pts · Bronze · 3 to Silver`, and the name is already on the badge's
    /// band two inches above; saying it twice is the screen stuttering. The badge's own
    /// tooltip carries the whole phrase for somebody reading one badge out of context.
    /// </remarks>
    public string ScoreLine
    {
        get
        {
            if (PointsLine.Length == 0)
            {
                return "";
            }

            return GapLine.Length > 0 ? PointsLine + " · " + GapLine : PointsLine;
        }
    }

    /// <summary>True where there is a score to draw.</summary>
    public bool HasScore => ScoreLine.Length > 0;

    /// <summary>The whole phrase, for a hover.</summary>
    public string ScoreTip => HasScore ? Name + " · " + ScoreLine : "";

    /// <summary>Where the points line is said rather than scored - a continent badge.</summary>
    public string? PointsSaid { get; init; }

    /// <summary>Where the gap line is said rather than scored - a continent badge.</summary>
    public string? GapSaid { get; init; }

    /// <summary>
    /// **The corner's first score line**: `35 pts · Bronze`, or "" where absent.
    /// </summary>
    /// <remarks>
    /// **THE SCORE IS TWO LINES AND NOT ONE** (work instruction 332 task 1: *text fits,
    /// everywhere*). `0 pts · unranked · 10 to Bronze` is thirty-one characters and a
    /// badge four across does not hold it without clipping or wrapping a word, so the gap
    /// is its own line under the points.
    /// </remarks>
    public string PointsLine
        => PointsSaid
            ?? (Score.Points is { } points
                ? points.ToString("#,0", CultureInfo.InvariantCulture) + " pts · "
                    + Score.LevelName
                : "");

    /// <summary>True where there is a points line to draw.</summary>
    public bool HasPointsLine => PointsLine.Length > 0;

    /// <summary>The corner's second score line: `10 to Bronze`, or "".</summary>
    public string GapLine
        => GapSaid
            ?? (Score.Points is not null
                && Score.ToNextLevel is { } gap
                && Score.NextLevelName.Length > 0
                ? gap.ToString("#,0", CultureInfo.InvariantCulture) + " to "
                    + Score.NextLevelName
                : "");

    /// <summary>True where there is a gap line to draw.</summary>
    public bool HasGapLine => GapLine.Length > 0;
}

/// <summary>
/// **The achievements page: eight badges, a running total, and nothing he has not
/// opened.**
/// </summary>
/// <remarks>
/// <para>**THE COLORS ARE THE MOCKUP'S AND THEY ARE THE AUTHOR'S SHAPE MARKED FOR THE
/// OWNER** (work instruction 331's ARBITER block says so). They are a badge's own band and
/// not a family color, so §0.5's *family color is text only* is not in play: nothing here
/// claims a mode family. **Every band carries white text and its own name in words**, so
/// the hue is decoration over a label rather than the carrier of anything (§0.6).</para>
/// <para>**THE RUNNING TOTAL IS ALWAYS ON THE PAGE** - `Total 145 pts · Rank 3 · 105 to
/// Rank 4` - and it is the sum of the eight, computed once. Where the points file could
/// not be read it is absent and <see cref="Problem"/> says why.</para>
/// <para>**EVERY STRING A SLOT CAN CARRY IS SHORT ENOUGH FOR THE SLOT** (work instruction
/// 332 task 1). The next-card words are at most eighteen characters and the meaning lines
/// at most eighteen, which is what a badge a quarter of the window wide holds on the test
/// host's flat ten pixels a character - wider than any face on the glass.</para>
/// </remarks>
public sealed class AchievementBadgePage
{
    /// <summary>The one line under the title, when there is a total to say.</summary>
    private const string TotalWord = "Total ";

    /// <summary>The Continents band, which the seven continent badges wear too.</summary>
    public const string ContinentsBand = "#2A7A94";

    private const string FirstContinent = "A first continent";
    private const string MoreContinents = "One more continent";
    private const string FirstCountry = "Your first country";
    private const string MoreCountries = "One more country";
    private const string FirstState = "Your first state";
    private const string MoreStates = "One more state";
    private const string FirstGrid = "Your first grid";
    private const string MoreGrids = "One more grid";
    private const string FirstBand = "Your first band";
    private const string MoreBands = "One more band";
    private const string FirstMode = "Your first mode";
    private const string MoreModes = "One more mode";

    /// <summary>What an unworked continent's badge says is next.</summary>
    public const string FirstHere = "A first here";

    /// <summary>Build the page.</summary>
    /// <param name="log">The contacts.</param>
    /// <param name="points">The owner's points file, loaded or absent.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public AchievementBadgePage(AchievementLog log, AchievementPoints points)
    {
        ArgumentNullException.ThrowIfNull(log);
        ArgumentNullException.ThrowIfNull(points);

        Log = log;
        Scores = new AchievementScores(log, points);

        Badges = AchievementKinds.All.Select(kind => Build(kind, log, Scores)).ToList();
    }

    /// <summary>The contacts the page was built from, for a category to open over.</summary>
    public AchievementLog Log { get; }

    /// <summary>The eight badges, in the page's order.</summary>
    public IReadOnlyList<AchievementBadge> Badges { get; }

    /// <summary>Every score, and the total.</summary>
    public AchievementScores Scores { get; }

    /// <summary>The line under the title, or "" where the file could not be read.</summary>
    public string TotalLine
    {
        get
        {
            if (Scores.Total is not { } total)
            {
                return "";
            }

            var said = new List<string>
            {
                TotalWord + total.ToString("#,0", CultureInfo.InvariantCulture) + " pts",
                "Rank " + Scores.Rank.ToString(CultureInfo.InvariantCulture),
            };

            if (Scores.ToNextRank is { } gap)
            {
                said.Add(
                    gap.ToString("#,0", CultureInfo.InvariantCulture)
                    + " to Rank " + (Scores.Rank + 1).ToString(CultureInfo.InvariantCulture));
            }

            return string.Join(" · ", said);
        }
    }

    /// <summary>True where there is a total to draw.</summary>
    public bool HasTotal => TotalLine.Length > 0;

    /// <summary>What went wrong with the points file, in one line, or "".</summary>
    public string Problem => Scores.Points.Problem;

    /// <summary>True where the page has to say the file could not be read.</summary>
    public bool HasProblem => Problem.Length > 0;

    /// <summary>The subtitle: what the page is.</summary>
    public string Subtitle
        => Badges.Count.ToString(CultureInfo.InvariantCulture)
            + " kinds. In each, the next one you could earn.";

    /// <summary>
    /// **Every string a badge's next-card slot can carry**, so a test can measure the
    /// longest against the slot rather than trusting the fixture to reach it.
    /// </summary>
    public static IReadOnlyList<string> EveryNextCard
        => Firsts.Select(f => f.Said)
            .Concat(new[]
            {
                FirstContinent, MoreContinents, FirstCountry, MoreCountries, FirstState,
                MoreStates, FirstGrid, MoreGrids, FirstBand, MoreBands, FirstMode,
                MoreModes, FirstHere,
            })
            .ToList();

    /// <summary>The named firsts, in the order a first evening reaches them.</summary>
    /// <remarks>
    /// <para>**THE KEYS ARE THE POINTS FILE'S AND THE WORDS ARE THE SCREEN'S.** A key the
    /// file carries and this list does not is simply never shown as a *next*, which is the
    /// safe direction: the owner can add a key and Hamlet will score it without claiming to
    /// know what to call it.</para>
    /// <para>**SHORTENED IN WORK INSTRUCTION 332** so each fits a badge's next-card slot
    /// without clipping: *Your first contact outside your own country* is now *A DX
    /// contact*, *Your first PSK31 contact* is *A PSK31 contact*, *Your first Morse
    /// contact* is *A Morse contact*, and the two distances lost *A contact*.</para>
    /// </remarks>
    public static IReadOnlyList<(string Key, string Said)> Firsts { get; } = new[]
    {
        ("first_contact", "Your first contact"),
        ("first_dx", "A DX contact"),
        ("first_psk31", "A PSK31 contact"),
        ("first_cw_qso", "A Morse contact"),
        ("first_over_5000_miles", "Over 5,000 miles"),
        ("first_over_10000_miles", "Over 10,000 miles"),
    };

    private static AchievementBadge Build(
        string kind, AchievementLog log, AchievementScores scores)
    {
        var score = scores.For(kind);

        return kind switch
        {
            AchievementKinds.HallOfFame => new AchievementBadge(
                kind, "Hall of Fame", "once-only firsts", "trophy", "#A8811A",
                NextFirst(log), NextIsDoor: true, Worked(score.Worked), score),

            AchievementKinds.Continents => new AchievementBadge(
                kind, "Continents", "each of the 7", "americas", ContinentsBand,
                NextContinent(log), NextIsDoor: true,
                score.Worked.ToString(CultureInfo.InvariantCulture) + " of "
                    + Hamlet.RadioEngine.Explore.DxccContinents.Codes.Count
                        .ToString(CultureInfo.InvariantCulture),
                score),

            AchievementKinds.Countries => new AchievementBadge(
                kind, "Countries", "one per entity", "flags", "#A33333",
                score.Worked == 0 ? FirstCountry : MoreCountries,
                NextIsDoor: false, Worked(score.Worked), score),

            AchievementKinds.States => new AchievementBadge(
                kind, "States", "the 50, plus DC", "star", "#2C4C9B",
                score.Worked == 0 ? FirstState : MoreStates,
                NextIsDoor: false, Worked(score.Worked), score),

            AchievementKinds.Grids => new AchievementBadge(
                kind, "Grids", "4-character grids", "grid", "#2F6B3A",
                score.Worked == 0 ? FirstGrid : MoreGrids,
                NextIsDoor: false, Worked(score.Worked), score),

            AchievementKinds.TotalMiles => new AchievementBadge(
                kind, "Total Miles", "every mile, added", "globe", "#6B4C9A",
                NextTier(scores), NextIsDoor: false,
                scores.TotalMiles.ToString("#,0", CultureInfo.InvariantCulture)
                    + " mi so far",
                score),

            AchievementKinds.Bands => new AchievementBadge(
                kind, "Bands", "first on each band", "waves", "#8A5A1E",
                score.Worked == 0 ? FirstBand : MoreBands,
                NextIsDoor: false,
                score.Worked.ToString(CultureInfo.InvariantCulture) + " of "
                    + Hamlet.RadioEngine.Bands.HfBands.Bands.Count
                        .ToString(CultureInfo.InvariantCulture),
                score),

            _ => new AchievementBadge(
                kind, "Modes", "five modes to work", "modes", "#3E4650",
                NextMode(log), NextIsDoor: false,
                score.Worked.ToString(CultureInfo.InvariantCulture) + " of "
                    + AchievementScores.WorkableModes.ToString(CultureInfo.InvariantCulture),
                score),
        };
    }

    /// <summary>`0 worked`, and never `0 of 340`.</summary>
    /// <remarks>
    /// **A DENOMINATOR IS A COUNT OF WHAT HE HAS NOT DONE** (§3.7), and for countries and
    /// grids it is a large one. The kinds with a small, knowable set - continents, bands,
    /// modes - say `1 of 7`, because seven is a target rather than a reproach.
    /// </remarks>
    private static string Worked(long count)
        => count.ToString("#,0", CultureInfo.InvariantCulture) + " worked";

    /// <summary>The nearest Hall of Fame first he has not earned, in his own words.</summary>
    private static string NextFirst(AchievementLog log)
    {
        var earned = AchievementScores.FirstsEarned(log);

        foreach (var (key, said) in Firsts)
        {
            if (!earned.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                return said;
            }
        }

        return "";
    }

    /// <summary>
    /// The next continent card, **naming no continent he has not opened** (§3.1).
    /// </summary>
    /// <remarks>
    /// **IT NAMES NONE AT ALL SINCE WORK INSTRUCTION 332.** *A first outside North
    /// America* named the one he is standing in and ran to twenty-nine characters, which no
    /// badge a quarter of the window wide holds; *One more continent* says the same target
    /// in eighteen.
    /// </remarks>
    private static string NextContinent(AchievementLog log)
    {
        if (log.Continents.Count == 0)
        {
            return FirstContinent;
        }

        return log.Continents.Count >= Hamlet.RadioEngine.Explore.DxccContinents.Codes.Count
            ? ""
            : MoreContinents;
    }

    /// <summary>The lowest Total Miles tier he has not reached.</summary>
    private static string NextTier(AchievementScores scores)
    {
        var reached = (long)Math.Round(scores.TotalMiles);

        foreach (var (at, _) in scores.Points.Milestones(AchievementKinds.TotalMiles))
        {
            if (reached < at)
            {
                return at.ToString("#,0", CultureInfo.InvariantCulture) + " miles";
            }
        }

        return "";
    }

    /// <summary>The next mode card - the first one, or one more, or none at all five.</summary>
    private static string NextMode(AchievementLog log)
    {
        if (log.Modes.Count == 0)
        {
            return FirstMode;
        }

        return log.Modes.Count >= AchievementScores.WorkableModes ? "" : MoreModes;
    }

    /// <summary>What a category says is next in a kind, for the unearned card.</summary>
    /// <param name="kind">One of the counted kinds.</param>
    /// <param name="worked">How many he has.</param>
    /// <returns>The words, which are the badge's own.</returns>
    public static string NextWords(string kind, long worked) => kind switch
    {
        AchievementKinds.Countries => worked == 0 ? FirstCountry : MoreCountries,
        AchievementKinds.States => worked == 0 ? FirstState : MoreStates,
        AchievementKinds.Grids => worked == 0 ? FirstGrid : MoreGrids,
        AchievementKinds.Bands => worked == 0 ? FirstBand : MoreBands,
        AchievementKinds.Modes => worked == 0 ? FirstMode : MoreModes,
        _ => "",
    };
}

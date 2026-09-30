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

    /// <summary>
    /// **The trading card a continent badge is drawn as inside Continents** (work instruction 335
    /// task 4), or null on the page's eight.
    /// </summary>
    public AchievementCategoryCard? Card { get; init; }

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

    // ------------------------------------------------------------------------------------------
    // **THE TILE** (work instruction 506 task 2, `assets/achievements-look/opening-page.html`).
    // ------------------------------------------------------------------------------------------

    /// <summary>
    /// **Nothing earned in this kind, so the tile is drawn locked** and says what opens it (work instruction
    /// 506). §2 stands: a locked tile appears only where nothing in the kind is earned, one per kind, and
    /// never a wall of blanks.
    /// </summary>
    public bool IsLocked => Score.Worked == 0;

    /// <summary>True where the kind has something earned.</summary>
    public bool IsOpened => !IsLocked;

    /// <summary>
    /// The header's fill: the kind's color, but `#8A6A10` for the Hall of Fame, since white on its `#A8811A`
    /// does not reach 4.5 to 1 (§0.6). The author's, overrulable; the kind's color elsewhere is unchanged.
    /// </summary>
    public string TileBand => Kind == AchievementKinds.HallOfFame ? HallOfFameHeader : Band;

    /// <summary>The Hall of Fame header's fill.</summary>
    public const string HallOfFameHeader = "#8A6A10";

    /// <summary>The level in words, `Bronze`, from his file's `levels`; "" below the first level or with no file.</summary>
    public string LevelChip => Score.Points is not null && Score.Level >= 1 ? Score.LevelName : "";

    /// <summary>True where the tile draws its level.</summary>
    public bool HasLevelChip => LevelChip.Length > 0;

    /// <summary>The count, large: `8`, or `31,400` miles.</summary>
    public string CountText => Score.Worked.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>What the count counts: `countries worked`. **Never a denominator** (§3.7).</summary>
    public string CountWords => Words.TryGetValue(Kind, out var said) ? (Score.Worked == 1 ? said.One : said.Many) : "";

    /// <summary>
    /// True where a bar to the next level is drawn: the file was read and names a level for this kind, the
    /// next one or the top one.
    /// </summary>
    public bool HasBar => Score.Points is not null && (Score.NextLevelAt is not null || Score.Level >= 1);

    /// <summary>
    /// **How far to the next level of his file**, nought to one: the count over the count the next level
    /// starts at, and full at the top level. A bar is never a fraction of the world (§3.7).
    /// </summary>
    public double BarFraction => Score.NextLevelAt is { } next && next > 0
        ? Math.Clamp((double)Score.Worked / next, 0, 1)
        : 1;

    /// <summary>`2 more to Silver`, `18,600 mi to Gold`, or `the top level`; "" where no bar is drawn.</summary>
    public string BarGapLine => !HasBar ? ""
        : Score.ToNextLevel is { } gap && Score.NextLevelName.Length > 0
            ? (Kind == AchievementKinds.TotalMiles
                ? gap.ToString("#,0", CultureInfo.InvariantCulture) + " mi to "
                : gap.ToString("#,0", CultureInfo.InvariantCulture) + " more to ") + Score.NextLevelName
            : "the top level";

    /// <summary>`Next:`, or `Opens a set:` where the next card is a door (R19): the word carries it, the ring decorates it (§0.6).</summary>
    public string NextLabel => NextIsDoor ? "Opens a set:" : "Next:";

    /// <summary>What a locked tile says under its name: `No states worked yet.`</summary>
    public string NoneYetLine => NoneYet.TryGetValue(Kind, out var said) ? said : "";

    /// <summary>What the first one in a locked kind takes, after `To open it:`. The author's words.</summary>
    public string ToOpenLine => ToOpen.TryGetValue(Kind, out var said) ? said : "";

    private static readonly Dictionary<string, (string One, string Many)> Words = new(StringComparer.Ordinal)
    {
        [AchievementKinds.HallOfFame] = ("once-only first", "once-only firsts"),
        [AchievementKinds.Continents] = ("continent reached", "continents reached"),
        [AchievementKinds.Countries] = ("country worked", "countries worked"),
        [AchievementKinds.States] = ("state, from STATE", "states, from STATE"),
        [AchievementKinds.Grids] = ("grid square", "grid squares"),
        [AchievementKinds.TotalMiles] = ("mile, added up", "miles, added up"),
        [AchievementKinds.Bands] = ("band opened", "bands opened"),
        [AchievementKinds.Modes] = ("mode worked", "modes worked"),
    };

    /// <remarks>
    /// **SHORT BECAUSE THE TILE IS NARROW** (§6): a quarter of the page's right side holds about sixteen
    /// characters at the tile's size on the test host's measure, which is wider than any face on the glass.
    /// The kind's name is on the tile above it, so the line need not repeat it.
    /// </remarks>
    private static readonly Dictionary<string, string> NoneYet = AchievementKinds.All.ToDictionary(k => k, _ => "Nothing here yet.", StringComparer.Ordinal);

    /// <remarks>
    /// **WHAT THE FIRST ONE TAKES, IN SIXTEEN CHARACTERS OR FEWER** (§6), after `To open it:`. Any contact
    /// opens a continent, a country, a band and a mode; a state wants the log's `STATE` field on a US record,
    /// and a grid and a mile want the station's grid. The author's words, overrulable.
    /// </remarks>
    private static readonly Dictionary<string, string> ToOpen = new(StringComparer.Ordinal)
    {
        [AchievementKinds.HallOfFame] = "a first contact.",
        [AchievementKinds.Continents] = "any contact.",
        [AchievementKinds.Countries] = "any contact.",
        [AchievementKinds.States] = "a US state.",
        [AchievementKinds.Grids] = "a logged grid.",
        [AchievementKinds.TotalMiles] = "a logged grid.",
        [AchievementKinds.Bands] = "any contact.",
        [AchievementKinds.Modes] = "any contact.",
    };
}

/// <summary>
/// **One rank on the trail across the top of the page** (work instruction 506 task 2): a passed rank, the
/// one he holds, or the next one, locked.
/// </summary>
/// <param name="Name">The rank's name from his file, or `Rank n`.</param>
/// <param name="Under">The line under it: where it began, `you are here`, or `opens at 500 points`.</param>
/// <param name="State">`passed`, `here` or `locked`.</param>
public sealed record AchievementRankStep(string Name, string Under, string State)
{
    /// <summary>True for a rank he has passed: checked, and a solid line after it.</summary>
    public bool IsPassed => State == "passed";

    /// <summary>True for the rank he holds.</summary>
    public bool IsHere => State == "here";

    /// <summary>True for the next rank: a padlock, and a dashed line leading to it.</summary>
    public bool IsLocked => State == "locked";

    /// <summary>True for every step but the first, which has no line before it.</summary>
    public bool HasLineBefore { get; init; }

    /// <summary>True where the line before it is dashed: the one leading to the locked rank.</summary>
    public bool LineBeforeDashed => IsLocked;

    /// <summary>True where the line before it is solid.</summary>
    public bool LineBeforeSolid => HasLineBefore && !IsLocked;

    /// <summary>True where a solid half-line runs from it to a step that is not locked.</summary>
    public bool RightSolid { get; init; }

    /// <summary>True where a dashed half-line runs from it to the locked rank.</summary>
    public bool RightDashed { get; init; }
}

/// <summary>
/// **The rank trail** (work instruction 506 task 2): each rank passed, checked; the one he holds; the next one
/// locked with where it opens; and nothing after it. §2 stands: **one locked rank, never the ladder.**
/// </summary>
public sealed class AchievementRankTrail
{
    /// <summary>How many passed ranks are drawn before the older ones are counted in words instead.</summary>
    public const int PassedDrawn = 3;

    /// <summary>More passed ranks than this and only the last <see cref="PassedDrawn"/> are drawn.</summary>
    public const int PassedMost = 4;

    /// <summary>Build the trail from the scores; empty where the file could not be read.</summary>
    /// <param name="scores">The scores.</param>
    public AchievementRankTrail(AchievementScores scores)
    {
        ArgumentNullException.ThrowIfNull(scores);

        if (scores.Total is null)
        {
            return;
        }

        var ranks = scores.Points.Ranks;
        var passed = Enumerable.Range(1, scores.Rank - 1).ToList();
        var shown = passed.Count > PassedMost ? passed.Skip(passed.Count - PassedDrawn).ToList() : passed;
        var steps = new List<AchievementRankStep>();

        if (shown.Count < passed.Count)
        {
            var earlier = passed.Count - shown.Count;

            EarlierLine = Spelled(earlier) + (earlier == 1 ? " rank before" : " ranks before");
        }

        foreach (var rank in shown)
        {
            steps.Add(new AchievementRankStep(scores.Points.RankName(rank), BeganAt(rank, ranks), "passed"));
        }

        steps.Add(new AchievementRankStep(scores.RankName, "you are here", "here"));

        if (scores.NextRankAt is { } next)
        {
            steps.Add(new AchievementRankStep(
                scores.NextRankName, "opens at " + next.ToString("#,0", CultureInfo.InvariantCulture) + " points", "locked"));
        }

        Steps = steps.Select((s, i) => s with
        {
            HasLineBefore = i > 0 || EarlierLine.Length > 0,
            RightSolid = i + 1 < steps.Count && !steps[i + 1].IsLocked,
            RightDashed = i + 1 < steps.Count && steps[i + 1].IsLocked,
        }).ToList();
    }

    /// <summary>The steps, left to right.</summary>
    public IReadOnlyList<AchievementRankStep> Steps { get; } = Array.Empty<AchievementRankStep>();

    /// <summary>True where there is a trail to draw.</summary>
    public bool HasSteps => Steps.Count > 0;

    /// <summary>The passed ranks and the one he holds, drawn in equal cells.</summary>
    public IReadOnlyList<AchievementRankStep> Drawn => Steps.Where(s => !s.IsLocked).ToList();

    /// <summary>The next rank, locked, drawn in its own cell at the right; null at the top rank.</summary>
    public AchievementRankStep? Locked => Steps.FirstOrDefault(s => s.IsLocked);

    /// <summary>True where there is a locked rank to draw.</summary>
    public bool HasLocked => Locked is not null;

    /// <summary>`two ranks before`, where more passed ranks exist than are drawn; otherwise "".</summary>
    public string EarlierLine { get; } = "";

    /// <summary>True where the earlier ranks are counted in words.</summary>
    public bool HasEarlierLine => EarlierLine.Length > 0;

    private static string BeganAt(int rank, IReadOnlyList<long> ranks)
        => rank <= 1 || rank - 2 >= ranks.Count
            ? "the start"
            : ranks[rank - 2].ToString("#,0", CultureInfo.InvariantCulture) + " points";

    private static string Spelled(int count) => count switch
    {
        1 => "one",
        2 => "two",
        3 => "three",
        4 => "four",
        5 => "five",
        6 => "six",
        7 => "seven",
        8 => "eight",
        9 => "nine",
        _ => count.ToString(CultureInfo.InvariantCulture),
    };
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

    /// <summary>The Modes badge's meaning line, counted rather than typed.</summary>
    /// <remarks>
    /// <para>**THE NUMBER IS NOT IN THIS FILE** (work instruction 368 decision BY). It read
    /// *five modes to work* as a string until the Olivia phase put a sixth mode in the log,
    /// and a badge whose words and whose standing come from two different places is a badge
    /// that can say *five modes to work · 0 of 6* (§0.0).</para>
    /// <para>**THE WORD AND NOT THE DIGIT**, because the standing beside it already carries
    /// the digits and *5 modes to work · 0 of 5* reads as an error.
    /// <see cref="AchievementScores.WorkableModes"/> is a small count of a table that
    /// changes about once a year, so the words for it are spelled out here and the one
    /// nobody has written falls back to the figure rather than to silence.</para>
    /// </remarks>
    public static string ModesToWork => Spelled(AchievementScores.WorkableModes) + " modes to work";

    /// <summary>A small count in words, or the figure where nobody has spelled it.</summary>
    private static string Spelled(int count)
        => count switch
        {
            2 => "two",
            3 => "three",
            4 => "four",
            5 => "five",
            6 => "six",
            7 => "seven",
            8 => "eight",
            9 => "nine",
            10 => "ten",
            _ => count.ToString(CultureInfo.InvariantCulture),
        };

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

    /// <summary>
    /// **The operator's grid from Settings, the one every contact's miles were measured from**,
    /// or "".
    /// </summary>
    /// <remarks>
    /// **THE MAP ON A CARD AND THE DISTANCE UNDER IT ARE ONE MEASUREMENT** (work instruction 335
    /// task 2). `AchievementContact.Miles` is measured from this grid and not from the record's
    /// own `MyGrid`, so the path is drawn from here too; a map from one grid over a distance from
    /// another would be two claims that can disagree (§0.0).
    /// </remarks>
    public string OperatorGrid { get; init; } = "";

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
                // **THE RANK BY THE NAME IN HIS FILE** (work instruction 336 task 2, R23), and
                // `Rank n` where the file names none - so the gap names the next rank the same way.
                Scores.RankName,
            };

            if (Scores.ToNextRank is { } gap)
            {
                said.Add(
                    gap.ToString("#,0", CultureInfo.InvariantCulture) + " to " + Scores.NextRankName);
            }

            return string.Join(" · ", said);
        }
    }

    /// <summary>True where there is a total to draw.</summary>
    public bool HasTotal => TotalLine.Length > 0;

    /// <summary>What went wrong with the points file, in one line, or "".</summary>
    public string Problem => Scores.Points.Problem;

    private AchievementRankTrail? _trail;

    /// <summary>**The rank trail** across the top of the page (work instruction 506 task 2).</summary>
    public AchievementRankTrail Trail => _trail ??= new AchievementRankTrail(Scores);

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

        // **THE OTHER KEYBOARD MODE** (work instruction 368 decision CA), in the shortened
        // form unit 332 set, and worth what `first_psk31` is worth in the owner's own points
        // file. **The value is the arbiter's and overrulable**: it is one line of
        // `data/achievements/achievement-points.json` and nothing here reads a number.
        ("first_olivia", "An Olivia contact"),
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
                kind, "States", "the 50 states", "star", "#2C4C9B",
                score.Worked == 0 ? FirstState : MoreStates,
                NextIsDoor: false, Worked(score.Worked) + FromStateField, score),

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
                kind, "Modes", ModesToWork, "modes", "#3E4650",
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

    /// <summary>
    /// **What the States count counts, said after it**: `2 worked, from STATE` (work instruction 348
    /// ruling 35).
    /// </summary>
    /// <remarks>
    /// <para>**THE COUNT IS THE LOG'S `STATE` FIELD AND NOTHING ELSE** (R23), so on a log whose US
    /// records mostly carry none - Hamlet's own entries carry none - a bare `2 worked` beside hundreds
    /// of US contacts reads as a fault. The badge and the band say the same words, so the same
    /// number.</para>
    /// <para>**SHORTENED TO FIT** (§6): *2 states worked, read from the log's STATE field* is 48
    /// characters, and the badge's corner holds about 22 on the test host's ten pixels a character.
    /// The kind's name is on the badge already, so *states* goes, and *STATE* in capitals names the
    /// field.</para>
    /// </remarks>
    private const string FromStateField = ", from STATE";

    /// <summary>The nearest Hall of Fame first he has not earned, in his own words.</summary>
    private static string NextFirst(AchievementLog log)
        => NextFirstOf(AchievementScores.FirstsEarned(log))?.Said ?? "";

    /// <summary>
    /// **The nearest unearned first the window may name**, for the badge and for the category.
    /// </summary>
    /// <param name="earned">The keys the log has earned.</param>
    /// <returns>The key and its words, or null where every first is held.</returns>
    /// <remarks>
    /// <para>**A PSK31 FIRST IS NEVER THE NEXT CARD WHILE ANOTHER IS LEFT** (work instruction
    /// 333 task 2, step 5 criterion 3). §3.1 keeps every PSK31 card absent until the first
    /// PSK31 contact, and `first_psk31` unearned means exactly that there has been none. On
    /// any log with a DX contact it stood next in the list's order, so the page and the
    /// category both drew *A PSK31 contact* before he had worked one. It now shows the
    /// nearest first §3.1 allows.</para>
    /// <para>**WHERE IT IS THE ONLY FIRST LEFT, IT IS STILL SHOWN**, and that is not a
    /// choice made here. Ruling C says the nearest unearned card in each kind and §3.1 says
    /// no PSK31 card; with nothing else left they cannot both hold, so the screen is left as
    /// it was and the collision is raised in the report for the owner.</para>
    /// </remarks>
    public static (string Key, string Said)? NextFirstOf(IReadOnlyList<string> earned)
    {
        ArgumentNullException.ThrowIfNull(earned);

        var unearned = Firsts
            .Where(f => !earned.Contains(f.Key, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (unearned.Count == 0)
        {
            return null;
        }

        foreach (var first in unearned)
        {
            if (!AbsentUntilWorked.Contains(first.Key, StringComparer.Ordinal))
            {
                return first;
            }
        }

        return unearned[0];
    }

    /// <summary>The firsts §3.1 keeps off the window until they are earned.</summary>
    /// <remarks>
    /// **BOTH KEYBOARD MODES** (work instruction 368 decision BZ). §3.1 kept *A PSK31
    /// contact* off the window until he had made one, for the reason that naming a mode's
    /// achievement before he has worked it is the screen making him a promise about a mode
    /// it has not yet shown him. Olivia is the same case and arrives the same way.
    /// </remarks>
    private static readonly string[] AbsentUntilWorked = ["first_psk31", "first_olivia"];

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

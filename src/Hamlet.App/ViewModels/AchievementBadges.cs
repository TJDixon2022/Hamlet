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
/// </remarks>
/// <param name="Kind">One of <see cref="AchievementKinds.All"/>.</param>
/// <param name="Name">What the band reads: `Hall of Fame`, `Total Miles`.</param>
/// <param name="Meaning">The one line under the name: `firsts that happen once`.</param>
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
            if (Score.Points is not { } points)
            {
                return "";
            }

            var said = new List<string>
            {
                points.ToString(CultureInfo.InvariantCulture) + " pts",
                Score.LevelName,
            };

            if (Score.ToNextLevel is { } gap && Score.NextLevelName.Length > 0)
            {
                said.Add(
                    gap.ToString("#,0", CultureInfo.InvariantCulture)
                    + " to " + Score.NextLevelName);
            }

            return string.Join(" · ", said);
        }
    }

    /// <summary>True where there is a score to draw.</summary>
    public bool HasScore => ScoreLine.Length > 0;

    /// <summary>The whole phrase, for a hover.</summary>
    public string ScoreTip => HasScore ? Name + " · " + ScoreLine : "";
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
/// </remarks>
public sealed class AchievementBadgePage
{
    /// <summary>The one line under the title, when there is a total to say.</summary>
    private const string TotalWord = "Total ";

    /// <summary>Build the page.</summary>
    /// <param name="log">The contacts.</param>
    /// <param name="points">The owner's points file, loaded or absent.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public AchievementBadgePage(AchievementLog log, AchievementPoints points)
    {
        ArgumentNullException.ThrowIfNull(log);
        ArgumentNullException.ThrowIfNull(points);

        Scores = new AchievementScores(log, points);

        Badges = AchievementKinds.All.Select(kind => Build(kind, log, Scores)).ToList();
    }

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

    private static AchievementBadge Build(
        string kind, AchievementLog log, AchievementScores scores)
    {
        var score = scores.For(kind);

        return kind switch
        {
            AchievementKinds.HallOfFame => new AchievementBadge(
                kind, "Hall of Fame", "firsts that happen once", "trophy", "#A8811A",
                NextFirst(log), NextIsDoor: true, Worked(score.Worked), score),

            AchievementKinds.Continents => new AchievementBadge(
                kind, "Continents", "a first in each of 7", "americas", "#2A7A94",
                NextContinent(log), NextIsDoor: true,
                score.Worked.ToString(CultureInfo.InvariantCulture) + " of "
                    + Hamlet.RadioEngine.Explore.DxccContinents.Codes.Count
                        .ToString(CultureInfo.InvariantCulture),
                score),

            AchievementKinds.Countries => new AchievementBadge(
                kind, "Countries", "one card per entity", "flags", "#A33333",
                score.Worked == 0 ? "Your first country" : "One more country",
                NextIsDoor: false, Worked(score.Worked), score),

            AchievementKinds.States => new AchievementBadge(
                kind, "States", "the 50, plus DC", "star", "#2C4C9B",
                score.Worked == 0 ? "Your first state" : "One more state",
                NextIsDoor: false, Worked(score.Worked), score),

            AchievementKinds.Grids => new AchievementBadge(
                kind, "Grids", "4-character squares", "grid", "#2F6B3A",
                score.Worked == 0 ? "Your first grid square" : "One more grid square",
                NextIsDoor: false, Worked(score.Worked), score),

            AchievementKinds.TotalMiles => new AchievementBadge(
                kind, "Total Miles", "grid to grid, added up", "globe", "#6B4C9A",
                NextTier(scores), NextIsDoor: false,
                scores.TotalMiles.ToString("#,0", CultureInfo.InvariantCulture)
                    + " mi so far",
                score),

            AchievementKinds.Bands => new AchievementBadge(
                kind, "Bands", "a first on each", "waves", "#8A5A1E",
                score.Worked == 0 ? "Your first band" : "One more band",
                NextIsDoor: false,
                score.Worked.ToString(CultureInfo.InvariantCulture) + " of "
                    + Hamlet.RadioEngine.Bands.HfBands.Bands.Count
                        .ToString(CultureInfo.InvariantCulture),
                score),

            _ => new AchievementBadge(
                kind, "Modes", "FT8 · FT4 · PSK31 · CW · SSB", "modes", "#3E4650",
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

    /// <summary>The named firsts, in the order a first evening reaches them.</summary>
    /// <remarks>
    /// **THE KEYS ARE THE POINTS FILE'S AND THE WORDS ARE THE SCREEN'S.** A key the file
    /// carries and this list does not is simply never shown as a *next*, which is the safe
    /// direction: the owner can add a key and Hamlet will score it without claiming to
    /// know what to call it.
    /// </remarks>
    private static IReadOnlyList<(string Key, string Said)> Firsts { get; } = new[]
    {
        ("first_contact", "Your first contact"),
        ("first_dx", "Your first contact outside your own country"),
        ("first_psk31", "Your first PSK31 contact"),
        ("first_cw_qso", "Your first Morse contact"),
        ("first_over_5000_miles", "A contact over 5,000 miles"),
        ("first_over_10000_miles", "A contact over 10,000 miles"),
    };

    /// <summary>
    /// The next continent card, **naming no continent he has not opened** (§3.1).
    /// </summary>
    /// <remarks>
    /// **THIS IS THE ONE PLACE THE DOOR RULE BITES ON THIS PAGE.** *First outside
    /// N. America* names the one he is standing in, which he already knows, and not the one
    /// behind the door. With nothing worked at all there is no continent to be outside of,
    /// so the card is the category.
    /// </remarks>
    private static string NextContinent(AchievementLog log)
    {
        if (log.Continents.Count == 0)
        {
            return "Your first contact anywhere";
        }

        if (log.Continents.Count >= Hamlet.RadioEngine.Explore.DxccContinents.Codes.Count)
        {
            return "";
        }

        var here = log.Continents[0];

        return Hamlet.RadioEngine.Explore.DxccContinents.Codes
                .TryGetValue(here, out var name)
            ? "A first outside " + name
            : "A first on a new continent";
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

    /// <summary>The next mode card - the first one, or one more.</summary>
    private static string NextMode(AchievementLog log)
        => log.Modes.Count == 0 ? "Your first mode" : "One more mode";
}

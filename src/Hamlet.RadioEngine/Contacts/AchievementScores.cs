using System.Globalization;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>**What one kind is worth, and where it stands.**</summary>
/// <param name="Kind">One of <see cref="AchievementKinds.All"/>.</param>
/// <param name="Points">The score, or null where the points file could not be read.</param>
/// <param name="Level">Which level the count has reached: 0 is none yet.</param>
/// <param name="Worked">How many of this kind he has - or the miles, for Total Miles.</param>
/// <param name="NextLevelAt">
/// The count the next level starts at, or null where he is at the top or the file names no
/// levels.
/// </param>
public sealed record AchievementKindScore(
    string Kind,
    int? Points,
    int Level,
    long Worked,
    long? NextLevelAt)
{
    /// <summary>How far to the next level, or null where there is no next one.</summary>
    public long? ToNextLevel => NextLevelAt is { } at ? Math.Max(0, at - Worked) : null;

    /// <summary>True where there is a score to show at all.</summary>
    public bool HasPoints => Points is not null;

    /// <summary>**The level's name.** Numbered until the owner names them.</summary>
    /// <remarks>
    /// **THE FOUR NAMES ARE THE ONES THE INSTRUCTION USES** - its own example reads
    /// `Countries · 35 pts · Bronze · 3 to Silver` - and a kind whose file names more
    /// thresholds than there are names falls back to `Level n` rather than inventing a
    /// fifth word. **Nothing here shames**: level 0 is *unranked* and not *none*.
    /// </remarks>
    public string LevelName => Level switch
    {
        0 => "unranked",
        1 => "Bronze",
        2 => "Silver",
        3 => "Gold",
        4 => "Platinum",
        _ => "Level " + Level.ToString(CultureInfo.InvariantCulture),
    };

    /// <summary>What the next level up is called, or "".</summary>
    public string NextLevelName => NextLevelAt is null
        ? ""
        : new AchievementKindScore(Kind, Points, Level + 1, Worked, null).LevelName;
}

/// <summary>
/// **Every kind's score, and the running total, computed from the log and the owner's
/// file.**
/// </summary>
/// <remarks>
/// <para>**NO POINT VALUE IS IN THIS FILE** (work instruction 331 task 6: *do not
/// hard-code a point value; the file is the source*). Every number below came out of
/// <see cref="AchievementPoints"/>, and where the file does not say, the kind's score is
/// **absent** rather than nought - which is the difference between *he has earned nothing
/// in this kind* and *nobody can tell what he has earned* (§0.0).</para>
/// <para>**WORKED, NEVER CONFIRMED** (`ACHIEVEMENTS_PHILOSOPHY.md` §4). Every count here
/// is of contacts in the operator's own log. Hamlet has no path to an LoTW or a card file
/// and therefore no idea what is confirmed, and the word does not appear.</para>
/// <para>**AND A RANK NEVER GOES DOWN** (§4, and the instruction). The rank is a function
/// of the total and the total only grows, because nothing is ever taken off the log; the
/// one way a total could fall is the points file being edited downward, which is the
/// owner's own act on his own file.</para>
/// <para>Pure: a log and a points file in, numbers out. No clock, no state (§5).</para>
/// </remarks>
public sealed class AchievementScores
{
    private readonly Dictionary<string, AchievementKindScore> _byKind =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Score a log.</summary>
    /// <param name="log">The contacts.</param>
    /// <param name="points">The owner's file, loaded or absent.</param>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public AchievementScores(AchievementLog log, AchievementPoints points)
    {
        ArgumentNullException.ThrowIfNull(log);
        ArgumentNullException.ThrowIfNull(points);

        Points = points;
        TotalMiles = MilesIn(log);

        foreach (var kind in AchievementKinds.All)
        {
            _byKind[kind] = Score(kind, log, points);
        }

        Total = points.Loaded
            ? _byKind.Values.Sum(s => s.Points ?? 0)
            : null;

        Rank = RankOf(Total, points.Ranks);
        NextRankAt = NextRank(Total, points.Ranks);
    }

    /// <summary>The file the scores were computed from.</summary>
    public AchievementPoints Points { get; }

    /// <summary>The running total, or null where the file could not be read.</summary>
    public int? Total { get; }

    /// <summary>
    /// **Which rank the total reaches.** 1 before the first threshold is passed.
    /// </summary>
    /// <remarks>
    /// **RANKS ARE NUMBERED AND THE NAMES ARE THE OWNER'S LATER** (the instruction parks
    /// them). Numbering from 1 rather than 0 is deliberate: night one is rank 1 and not
    /// rank nought, because nought reads as a judgement and this screen makes none (§3.7).
    /// </remarks>
    public int Rank { get; }

    /// <summary>The total the next rank starts at, or null at the top.</summary>
    public long? NextRankAt { get; }

    /// <summary>How many points to the next rank, or null at the top.</summary>
    public long? ToNextRank
        => NextRankAt is { } at && Total is { } total ? Math.Max(0, at - total) : null;

    /// <summary>
    /// **Every logged mile, added up** (work instruction 331 task 7).
    /// </summary>
    /// <remarks>
    /// **A CONTACT WITHOUT A GRID ON BOTH ENDS CONTRIBUTES NOTHING, AND NEVER AN ESTIMATE
    /// FROM THE COUNTRY** (§0.0, and the instruction says so twice). `AchievementContact`
    /// already computes the great-circle from the operator's own grid to the station's with
    /// the same `GridPath` the conversation card draws, and it is null where either end is
    /// missing - so this is a sum over the ones that have one and nothing else.
    /// </remarks>
    public double TotalMiles { get; }

    /// <summary>One kind's score.</summary>
    /// <param name="kind">One of <see cref="AchievementKinds.All"/>.</param>
    /// <returns>The score.</returns>
    public AchievementKindScore For(string kind)
        => _byKind.TryGetValue(kind, out var score)
            ? score
            : new AchievementKindScore(kind, null, 0, 0, null);

    /// <summary>Every kind's score, in the page's own order.</summary>
    public IReadOnlyList<AchievementKindScore> All
        => AchievementKinds.All.Select(For).ToList();

    /// <summary>How many of a kind he has worked, as the badge's corner says it.</summary>
    /// <param name="kind">One of <see cref="AchievementKinds.All"/>.</param>
    /// <param name="log">The contacts.</param>
    /// <returns>The count, or the whole miles for Total Miles.</returns>
    public static long WorkedIn(string kind, AchievementLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return kind switch
        {
            AchievementKinds.Continents => log.Continents.Count,
            AchievementKinds.Countries => log.Entities.Count,
            AchievementKinds.Grids => log.Grids.Count,
            AchievementKinds.Bands => log.Bands.Count,
            AchievementKinds.Modes => log.Modes.Count,
            AchievementKinds.States => 0,
            AchievementKinds.TotalMiles => (long)Math.Round(MilesIn(log)),
            AchievementKinds.HallOfFame => FirstsIn(log),
            _ => 0,
        };
    }

    /// <summary>Every mile in a log, added up.</summary>
    /// <param name="log">The contacts.</param>
    /// <returns>The sum of the great-circle distances that exist.</returns>
    public static double MilesIn(AchievementLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return log.Contacts.Where(c => c.Miles is not null).Sum(c => c.Miles!.Value);
    }

    /// <summary>Which Hall of Fame firsts the log has earned, by the file's own keys.</summary>
    /// <param name="log">The contacts.</param>
    /// <returns>The keys, which are the points file's.</returns>
    /// <remarks>
    /// <para>**EACH ONE IS A QUESTION THE LOG CAN ANSWER AND NOTHING ELSE IS CLAIMED.**
    /// `first_dx` is a contact outside the operator's own DXCC entity, which needs his own
    /// entity to be knowable from his own callsign; `first_over_5000_miles` needs a grid on
    /// both ends. **A first Hamlet cannot decide is not earned**, rather than being earned
    /// on a guess.</para>
    /// <para>**`first_answer_to_own_cq` IS NOT DECIDABLE FROM AN ADIF RECORD** and is
    /// therefore never awarded here. The log says a contact happened and not who called
    /// first, and inventing that from the exchange would be the screen deciding something
    /// nobody recorded (§0.0). It stays in the owner's file because the file is his and a
    /// key Hamlet cannot yet award is a key it may award later.</para>
    /// </remarks>
    public static IReadOnlyList<string> FirstsEarned(AchievementLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        var earned = new List<string>();

        if (log.Count > 0)
        {
            earned.Add("first_contact");
        }

        var mine = log.Contacts
            .Select(c => c.Entity)
            .FirstOrDefault(e => e is not null);

        if (mine is not null && log.Entities.Any(
                e => !string.Equals(e, mine, StringComparison.OrdinalIgnoreCase)))
        {
            earned.Add("first_dx");
        }

        if (log.Modes.Any(m => string.Equals(m, "PSK31", StringComparison.OrdinalIgnoreCase)))
        {
            earned.Add("first_psk31");
        }

        if (log.Modes.Any(m => string.Equals(m, "CW", StringComparison.OrdinalIgnoreCase)))
        {
            earned.Add("first_cw_qso");
        }

        var furthest = log.Contacts.Where(c => c.Miles is not null)
            .Select(c => c.Miles!.Value)
            .DefaultIfEmpty(0)
            .Max();

        if (furthest >= 5000)
        {
            earned.Add("first_over_5000_miles");
        }

        if (furthest >= 10000)
        {
            earned.Add("first_over_10000_miles");
        }

        return earned;
    }

    private static int FirstsIn(AchievementLog log) => FirstsEarned(log).Count;

    private static AchievementKindScore Score(
        string kind, AchievementLog log, AchievementPoints points)
    {
        var worked = WorkedIn(kind, log);

        if (!points.Loaded)
        {
            return new AchievementKindScore(kind, null, 0, worked, null);
        }

        var earned = kind switch
        {
            AchievementKinds.Continents => Continents(log, points),
            AchievementKinds.Countries => Counted(
                AchievementKinds.Countries, log.Entities.Count, points),
            AchievementKinds.Grids => Counted(
                AchievementKinds.Grids, log.Grids.Count, points),
            AchievementKinds.Bands => Named(
                AchievementKinds.Bands, log.Bands, points, HfBandsCount),
            AchievementKinds.Modes => Named(
                AchievementKinds.Modes, log.Modes, points, WorkableModes),

            // **STATES ARE NOT IN THE LOG YET AND THE SCORE SAYS NOUGHT RATHER THAN
            // GUESSING.** An ADIF record carries `STATE`, `AchievementContact` does not
            // read it, and a state worked out from a callsign prefix would be a claim
            // about where somebody lives (§0.0). The badge draws, its next card is the
            // first state, and its score is nought until the log can answer.
            AchievementKinds.States => 0,

            AchievementKinds.TotalMiles => Tiers(
                AchievementKinds.TotalMiles, (long)Math.Round(MilesIn(log)), points),
            AchievementKinds.HallOfFame => FirstsEarned(log)
                .Sum(key => points.Special(AchievementKinds.HallOfFame, key) ?? 0),
            _ => 0,
        };

        var levels = points.Levels(kind);
        var level = levels.Count(at => worked >= at);
        var nextAt = level < levels.Count ? levels[level] : (long?)null;

        return new AchievementKindScore(kind, earned, level, worked, nextAt);
    }

    /// <summary>How many HF bands there are to work, for the *all* bonus.</summary>
    /// <remarks>
    /// **THE CITED ROWS' OWN COUNT** (§0), so an *all bands* bonus cannot be paid on a
    /// number somebody typed here.
    /// </remarks>
    private static int HfBandsCount => Bands.HfBands.Bands.Count;

    /// <summary>
    /// **How many modes there are to work, which is not how many Hamlet knows.**
    /// </summary>
    /// <remarks>
    /// **WSPR IS A BEACON AND NOBODY WORKS ANYBODY ON IT** (`ContactMode.IsContactMode`),
    /// so an *all modes* bonus that waited for it would be a bonus he can never be paid.
    /// The five are FT8, FT4, PSK31, CW and Voice.
    /// </remarks>
    public static int WorkableModes => ContactModes.Six.Count(m => m.IsContactMode);

    private static int Continents(AchievementLog log, AchievementPoints points)
    {
        var sum = log.Continents.Sum(code => points.PerContinent(code) ?? 0);

        // **THE *ALL* BONUS NEEDS THE CITED COUNT OF CONTINENTS AND NOT THE NUMBER
        // SEVEN.** `DxccContinents.Codes` is the publication's own list.
        if (log.Continents.Count >= Explore.DxccContinents.Codes.Count
            && points.AllBonus(AchievementKinds.Continents) is { } bonus)
        {
            sum += bonus;
        }

        return sum;
    }

    private static int Counted(string kind, int worked, AchievementPoints points)
    {
        var sum = worked * (points.Per(kind) ?? 0);

        sum += points.Milestones(kind).Where(m => worked >= m.At).Sum(m => m.Points);

        return sum;
    }

    private static int Tiers(string kind, long reached, AchievementPoints points)
        => points.Milestones(kind).Where(t => reached >= t.At).Sum(t => t.Points);

    private static int Named(
        string kind,
        IReadOnlyList<string> worked,
        AchievementPoints points,
        int howManyThereAre)
    {
        var per = points.Per(kind) ?? 0;
        var sum = 0;

        foreach (var one in worked)
        {
            // **A SPECIAL REPLACES THE PER RATHER THAN ADDING TO IT.** 160 m is worth 15
            // and not 20, which is what *by difficulty* means: the figure beside the name
            // is what that one is worth.
            sum += points.Special(kind, one) ?? per;
        }

        if (howManyThereAre > 0 && worked.Count >= howManyThereAre
            && points.AllBonus(kind) is { } bonus)
        {
            sum += bonus;
        }

        return sum;
    }

    private static int RankOf(int? total, IReadOnlyList<long> ranks)
        => total is { } points ? 1 + ranks.Count(at => points >= at) : 1;

    private static long? NextRank(int? total, IReadOnlyList<long> ranks)
    {
        if (total is not { } points)
        {
            return null;
        }

        foreach (var at in ranks)
        {
            if (points < at)
            {
                return at;
            }
        }

        return null;
    }
}

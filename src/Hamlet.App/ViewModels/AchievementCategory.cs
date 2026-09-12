using System.Globalization;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **One card inside a category: what it is, what it is worth, and whether he holds it.**
/// </summary>
/// <param name="Title">What it is: `Norway`, `A PSK31 contact`, `50,000 miles`.</param>
/// <param name="Figure">
/// The fact under it - `2 contacts` - or `next` on the one unearned card.
/// </param>
/// <param name="PointsLine">
/// What the owner's file says it is worth - `5 pts` - or "" where the file could not be
/// read.
/// </param>
/// <param name="Earned">Whether he holds it.</param>
public sealed record AchievementCategoryCard(
    string Title,
    string Figure,
    string PointsLine,
    bool Earned)
{
    /// <summary>What the one unearned card says under its title.</summary>
    public const string NextWord = "next";

    /// <summary>True where there is a fact to draw.</summary>
    public bool HasFigure => Figure.Length > 0;

    /// <summary>True where there is a worth to draw.</summary>
    public bool HasPoints => PointsLine.Length > 0;

    /// <summary>
    /// **An unearned card is drawn a little quieter, and the word `next` carries it too**
    /// (§0.6: opacity is a second carrier and never the only one).
    /// </summary>
    public double CardOpacity => Earned ? 1.0 : 0.78;
}

/// <summary>
/// **What replaces the page when a badge is pressed: the kind, its cards, and a way back.**
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-12**: *"When you click on a category, that category replaces it.
/// It's a click-into system."* One window and one frame; the page and a category are two
/// states of it, and the back control is how he gets from the second to the first.</para>
/// <para>**EARNED CARDS, THEN THE NEAREST UNEARNED ONE, AND NOTHING BEYOND IT**
/// (`ACHIEVEMENTS_PHILOSOPHY.md` §3.1, bent by ruling C of 2026-09-12 exactly as far as
/// the page is and no further). **Every card shows its points**, read from the owner's
/// file; where the file could not be read the worth is absent rather than nought.</para>
/// <para>**CONTINENTS OPENS TO SEVEN CONTINENT BADGES**, each pressable in turn, and a
/// continent opens to its countries with the same back control returning to the seven.
/// **Total Miles opens to its tiers** with the running sum and a bar to the next one.</para>
/// <para>**WORKED, NEVER CONFIRMED** (§4). The word does not appear here.</para>
/// <para>**THE LAYOUT IS THE AUTHOR'S SHAPE MARKED FOR THE OWNER** (work instruction 332's
/// ARBITER block).</para>
/// </remarks>
public sealed class AchievementCategory
{
    /// <summary>The key a continent badge's kind starts with.</summary>
    public const string ContinentPrefix = "continent-";

    private AchievementCategory(
        AchievementBadge badge,
        string? parentKind,
        string? parentName,
        IReadOnlyList<AchievementCategoryCard> cards,
        IReadOnlyList<AchievementBadge> subBadges)
    {
        Kind = badge.Kind;
        Name = badge.Name;
        Meaning = badge.Meaning;
        Emblem = badge.Emblem;
        Band = badge.Band;
        Standing = badge.Standing;
        PointsLine = badge.PointsLine;
        GapLine = badge.GapLine;
        ParentKind = parentKind;
        BackLabel = "‹ " + (parentName ?? "All achievements");
        Cards = cards;
        SubBadges = subBadges;
    }

    /// <summary>The kind this is: `countries`, or `continent-EU`.</summary>
    public string Kind { get; }

    /// <summary>The category's name, across the band.</summary>
    public string Name { get; }

    /// <summary>Its meaning line, under the name.</summary>
    public string Meaning { get; }

    /// <summary>Which emblem the band carries.</summary>
    public string Emblem { get; }

    /// <summary>The band's fill.</summary>
    public string Band { get; }

    /// <summary>What he has in it: `8 worked`.</summary>
    public string Standing { get; }

    /// <summary>Its score and level: `40 pts · unranked`.</summary>
    public string PointsLine { get; }

    /// <summary>True where there is a points line.</summary>
    public bool HasPointsLine => PointsLine.Length > 0;

    /// <summary>The gap to the next level: `2 to Bronze`.</summary>
    public string GapLine { get; }

    /// <summary>True where there is a gap line.</summary>
    public bool HasGapLine => GapLine.Length > 0;

    /// <summary>Where the back control goes: null for the page, or a parent kind.</summary>
    public string? ParentKind { get; }

    /// <summary>What the back control says: `‹ All achievements`, or `‹ Continents`.</summary>
    /// <remarks>
    /// **THE SAME CONTROL IN THE SAME PLACE, NAMING WHERE IT GOES.** Inside a continent it
    /// returns to the seven and not to the eight, and a control reading *All achievements*
    /// that went somewhere else would be a label saying a false thing (§0.0).
    /// </remarks>
    public string BackLabel { get; }

    /// <summary>Earned cards, then at most one unearned.</summary>
    public IReadOnlyList<AchievementCategoryCard> Cards { get; }

    /// <summary>True where there are cards to draw.</summary>
    public bool HasCards => Cards.Count > 0;

    /// <summary>The seven continent badges, inside Continents only.</summary>
    public IReadOnlyList<AchievementBadge> SubBadges { get; }

    /// <summary>True where there are badges to draw rather than cards.</summary>
    public bool HasSubBadges => SubBadges.Count > 0;

    /// <summary>How far to the next tier, 0 to 1, for Total Miles.</summary>
    public double BarFraction { get; private init; }

    /// <summary>What the bar is of: `42,041 of 50,000 miles`.</summary>
    public string BarLine { get; private init; } = "";

    /// <summary>True where there is a bar to draw.</summary>
    public bool HasBar => BarLine.Length > 0;

    /// <summary>Open a category over a page.</summary>
    /// <param name="kind">A kind from the page, or `continent-XX`.</param>
    /// <param name="page">The page it opens from.</param>
    /// <returns>The category, or null where the kind is not one Hamlet draws.</returns>
    public static AchievementCategory? For(string kind, AchievementBadgePage page)
    {
        ArgumentNullException.ThrowIfNull(page);

        if (kind.StartsWith(ContinentPrefix, StringComparison.Ordinal))
        {
            return ForContinent(kind[ContinentPrefix.Length..], page);
        }

        var badge = page.Badges.FirstOrDefault(b => b.Kind == kind);

        if (badge is null)
        {
            return null;
        }

        var log = page.Log;
        var points = page.Scores.Points;

        if (kind == AchievementKinds.Continents)
        {
            return new AchievementCategory(
                badge, null, null, Array.Empty<AchievementCategoryCard>(),
                ContinentBadges(page));
        }

        if (kind == AchievementKinds.TotalMiles)
        {
            return MilesFor(badge, page);
        }

        var cards = kind switch
        {
            AchievementKinds.HallOfFame => HallOfFame(log, points),
            AchievementKinds.Countries => Counted(
                kind,
                log.Entities.OrderBy(EntitySpoken.Of, StringComparer.OrdinalIgnoreCase)
                    .Select(e => (EntitySpoken.Of(e), log.Contacts.Count(c => Same(c.Entity, e))))
                    .ToList(),
                long.MaxValue,
                points),
            AchievementKinds.States => Counted(
                kind, new List<(string, int)>(), long.MaxValue, points),
            AchievementKinds.Grids => Counted(
                kind,
                log.Grids.OrderBy(g => g, StringComparer.Ordinal)
                    .Select(g => (g, log.Contacts.Count(
                        c => c.Grid is { Length: >= 4 } at && Same(at[..4], g))))
                    .ToList(),
                long.MaxValue,
                points),
            AchievementKinds.Bands => Counted(
                kind,
                log.Bands
                    .Select(b => (AdifLog.BandDisplayNameFor(b), log.OnBand(b).Count))
                    .ToList(),
                Hamlet.RadioEngine.Bands.HfBands.Bands.Count,
                points,
                log.Bands),
            _ => Counted(
                kind,
                log.Modes.Select(m => (m, log.InMode(m).Count)).ToList(),
                AchievementScores.WorkableModes,
                points,
                log.Modes),
        };

        return new AchievementCategory(
            badge, null, null, cards, Array.Empty<AchievementBadge>());
    }

    /// <summary>`5 pts`, `1 pt`, or "" where the file does not say.</summary>
    public static string Pts(int? points)
        => points is { } value
            ? value.ToString("#,0", CultureInfo.InvariantCulture) + (value == 1 ? " pt" : " pts")
            : "";

    /// <summary>`1 contact`, `3 contacts`.</summary>
    private static string Contacts(int count)
        => count == 1
            ? "1 contact"
            : count.ToString("#,0", CultureInfo.InvariantCulture) + " contacts";

    private static bool Same(string? a, string? b)
        => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    /// <summary>The firsts he holds, in the order an evening reaches them, and the next.</summary>
    private static List<AchievementCategoryCard> HallOfFame(
        AchievementLog log, AchievementPoints points)
    {
        var earned = AchievementScores.FirstsEarned(log);
        var cards = new List<AchievementCategoryCard>();
        AchievementCategoryCard? next = null;

        foreach (var (key, said) in AchievementBadgePage.Firsts)
        {
            var worth = Pts(points.Special(AchievementKinds.HallOfFame, key));

            if (earned.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                cards.Add(new AchievementCategoryCard(said, "", worth, true));
            }
            else
            {
                next ??= new AchievementCategoryCard(
                    said, AchievementCategoryCard.NextWord, worth, false);
            }
        }

        if (next is not null)
        {
            cards.Add(next);
        }

        return cards;
    }

    /// <summary>One card per thing he has, then the next one while there is one.</summary>
    /// <param name="kind">The kind.</param>
    /// <param name="held">What he has, as shown, with how many contacts.</param>
    /// <param name="howManyThereAre">The set's size, or long.MaxValue where it is open.</param>
    /// <param name="points">The owner's file.</param>
    /// <param name="keys">The names the file's specials are keyed by, where they differ.</param>
    private static List<AchievementCategoryCard> Counted(
        string kind,
        IReadOnlyList<(string Shown, int Contacts)> held,
        long howManyThereAre,
        AchievementPoints points,
        IReadOnlyList<string>? keys = null)
    {
        var per = points.Per(kind);
        var cards = new List<AchievementCategoryCard>();

        for (var i = 0; i < held.Count; i++)
        {
            // **A SPECIAL REPLACES THE PER** (`AchievementScores.Named`), so 160 m says 15.
            var worth = keys is not null ? points.Special(kind, keys[i]) ?? per : per;

            cards.Add(new AchievementCategoryCard(
                held[i].Shown, Contacts(held[i].Contacts), Pts(worth), true));
        }

        if (held.Count < howManyThereAre)
        {
            cards.Add(new AchievementCategoryCard(
                AchievementBadgePage.NextWords(kind, held.Count),
                AchievementCategoryCard.NextWord,
                Pts(per),
                false));
        }

        return cards;
    }

    /// <summary>Total Miles: the tiers reached, the next, and the bar to it.</summary>
    private static AchievementCategory MilesFor(AchievementBadge badge, AchievementBadgePage page)
    {
        var reached = (long)Math.Round(page.Scores.TotalMiles);
        var cards = new List<AchievementCategoryCard>();
        long? nextAt = null;

        foreach (var (at, worth) in page.Scores.Points.Milestones(AchievementKinds.TotalMiles))
        {
            var said = at.ToString("#,0", CultureInfo.InvariantCulture) + " miles";

            if (reached >= at)
            {
                cards.Add(new AchievementCategoryCard(said, "reached", Pts(worth), true));
            }
            else
            {
                cards.Add(new AchievementCategoryCard(
                    said, AchievementCategoryCard.NextWord, Pts(worth), false));
                nextAt = at;
                break;
            }
        }

        return new AchievementCategory(
            badge, null, null, cards, Array.Empty<AchievementBadge>())
        {
            // **A BAR OF MILES AND NOTHING ELSE**, with the two figures in words beside
            // it (work instruction 300 task 4's rule: say what the fraction is of).
            BarFraction = nextAt is { } of && of > 0 ? Math.Min(1.0, (double)reached / of) : 0,
            BarLine = nextAt is { } to
                ? reached.ToString("#,0", CultureInfo.InvariantCulture) + " of "
                    + to.ToString("#,0", CultureInfo.InvariantCulture) + " miles"
                : "",
        };
    }

    /// <summary>
    /// **The seven continent badges**, in the order of their names.
    /// </summary>
    /// <remarks>
    /// <para>**THE SEVEN ARE THE CITED TABLE'S** (`DxccContinents.Codes`) and the order is
    /// alphabetical by name, which is no judgement about which matters more.</para>
    /// <para>**AN UNWORKED CONTINENT'S BADGE NAMES IT, BECAUSE THE INSTRUCTION ASKS FOR
    /// SEVEN**, and its next card is *A first here* rather than a sentence naming it again.
    /// That is §3.1 bent one step further than the page bends it, and it is raised in the
    /// report rather than decided silently.</para>
    /// </remarks>
    private static IReadOnlyList<AchievementBadge> ContinentBadges(AchievementBadgePage page)
    {
        var log = page.Log;
        var points = page.Scores.Points;

        return DxccContinents.Codes
            .OrderBy(c => c.Value, StringComparer.Ordinal)
            .Select(c => ContinentBadge(c.Key, log, points))
            .ToList();
    }

    private static AchievementBadge ContinentBadge(
        string code, AchievementLog log, AchievementPoints points)
    {
        var worked = log.EntitiesOn(code);
        var opened = log.Continents.Contains(code, StringComparer.OrdinalIgnoreCase);
        var per = points.PerContinent(code);

        return new AchievementBadge(
            ContinentPrefix + code,
            DxccContinents.NameOf(code),
            "a DXCC continent",
            ContinentPrefix + code,
            AchievementBadgePage.ContinentsBand,
            opened
                ? AchievementBadgePage.NextWords(AchievementKinds.Countries, worked)
                : AchievementBadgePage.FirstHere,
            NextIsDoor: !opened,
            worked.ToString("#,0", CultureInfo.InvariantCulture) + " worked",
            new AchievementKindScore(ContinentPrefix + code, opened ? per : 0, 0, worked, null))
        {
            PointsSaid = points.Loaded ? Pts(opened ? per : 0) : "",
            GapSaid = !opened && per is { } worth
                ? worth.ToString("#,0", CultureInfo.InvariantCulture) + " for a first"
                : "",
        };
    }

    /// <summary>One continent, opened to its countries.</summary>
    private static AchievementCategory? ForContinent(string code, AchievementBadgePage page)
    {
        if (!DxccContinents.Codes.ContainsKey(code))
        {
            return null;
        }

        var log = page.Log;
        var badge = ContinentBadge(code, log, page.Scores.Points);

        var held = log.OnContinent(code)
            .Where(c => c.Entity is not null)
            .GroupBy(c => c.Entity!, StringComparer.OrdinalIgnoreCase)
            .Select(g => (EntitySpoken.Of(g.Key), g.Count()))
            .OrderBy(x => x.Item1, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var cards = Counted(
            AchievementKinds.Countries, held, DxccContinents.EntitiesOn(code),
            page.Scores.Points);

        return new AchievementCategory(
            badge,
            AchievementKinds.Continents,
            page.Badges.First(b => b.Kind == AchievementKinds.Continents).Name,
            cards,
            Array.Empty<AchievementBadge>());
    }
}

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

    /// <summary>The station that earned it, or "".</summary>
    public string Callsign { get; init; } = "";

    /// <summary>His grid as the log carries it, or "".</summary>
    public string Grid { get; init; } = "";

    /// <summary>The line under the title: `LA1ZZZ · JO28`, or the callsign alone.</summary>
    public string CallGridLine { get; init; } = "";

    /// <summary>The path map, or null where there is no grid to draw it from.</summary>
    public Ft8GlobePlot? Globe { get; init; }

    /// <summary>True where the card draws a map.</summary>
    public bool HasMap => Globe is not null;

    /// <summary>What the card says where it has no map, or "".</summary>
    public string NoMapWord { get; init; } = "";

    /// <summary>The contacts listed where the map would be, or empty.</summary>
    public IReadOnlyList<string> ContactLines { get; init; } = Array.Empty<string>();

    /// <summary>The distance in large type: `6,700 mi`, or "".</summary>
    public string DistanceLine { get; init; } = "";

    /// <summary>`20 m · FT8`, or what of it the log has.</summary>
    public string BandModeLine { get; init; } = "";

    /// <summary>`Sep 11, 2026`, or "" where the log has no date.</summary>
    public string DateLine { get; init; } = "";

    /// <summary>True where there is a callsign line.</summary>
    public bool HasCallGridLine => CallGridLine.Length > 0;

    /// <summary>True where the card has no map and says why in its place.</summary>
    public bool HasNoMap => !HasMap && NoMapWord.Length > 0;

    /// <summary>True where there is a distance.</summary>
    public bool HasDistance => DistanceLine.Length > 0;

    /// <summary>True where there is a band or a mode.</summary>
    public bool HasBandMode => BandModeLine.Length > 0;

    /// <summary>True where there is a date.</summary>
    public bool HasDate => DateLine.Length > 0;

    /// <summary>
    /// True where the old figure line still carries the card: `next`, `reached`, `2 contacts` on a
    /// card that is not yet a contact.
    /// </summary>
    public bool ShowsFigure => HasFigure && Callsign.Length == 0;

    /// <summary>What a next card wants, in words: `Any country you have not worked`, or "".</summary>
    public string WantsLine { get; init; } = "";

    /// <summary>The heading over the callers: `calling CQ at 21:41 UTC, unworked`, or "".</summary>
    public string CallersHeading { get; init; } = "";

    /// <summary>Who on the CQ list would earn this card, at most three.</summary>
    public IReadOnlyList<NextCaller> Callers { get; init; } = Array.Empty<NextCaller>();

    /// <summary>Where nobody listed would earn it, the words that say so; otherwise "".</summary>
    public string NoCallerLine { get; init; } = "";

    /// <summary>`and 2 more on the CQ list` where more would earn it than are listed, or "".</summary>
    public string MoreCallersLine { get; init; } = "";

    /// <summary>True where there is a wants line.</summary>
    public bool HasWants => WantsLine.Length > 0;

    /// <summary>
    /// What mark the callers who would earn this card carry on the CQ list, in words, or "" where
    /// the list draws no mark for them (work instruction 342, ruling 14).
    /// </summary>
    public string QuillLine { get; init; } = "";

    /// <summary>True where there is a quill line.</summary>
    public bool HasQuillLine => QuillLine.Length > 0;

    /// <summary>True where the card draws the CQ panel: a heading, callers, or the words instead.</summary>
    public bool HasCallersPanel
        => !Earned && (CallersHeading.Length > 0 || Callers.Count > 0 || NoCallerLine.Length > 0);

    /// <summary>True where there is a heading over the callers.</summary>
    public bool HasCallersHeading => CallersHeading.Length > 0;

    /// <summary>True where the card says nobody listed would earn it.</summary>
    public bool HasNoCaller => NoCallerLine.Length > 0;

    /// <summary>True where more would earn it than are listed.</summary>
    public bool HasMoreCallers => MoreCallersLine.Length > 0;

    /// <summary>A count the card carries: `3 countries worked there`, or "".</summary>
    public string CountLine { get; init; } = "";

    /// <summary>True where there is a count line.</summary>
    public bool HasCountLine => CountLine.Length > 0;

    /// <summary>How far toward the line this card is, 0 to 1, where it is a bar.</summary>
    public double TierFraction { get; init; }

    /// <summary>What the card's bar is of: `36,120 of 50,000 mi`, or "" where it has no bar.</summary>
    public string TierLine { get; init; } = "";

    /// <summary>True where the card draws a bar.</summary>
    public bool HasTierBar => TierLine.Length > 0;
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

        // **THE BAND SAYS FOUR THINGS AND DRAWS ONE** (work instruction 335 task 1, R22): the
        // count, the score, the level, and a bar to the next level. **Where there is no next
        // level it says so in words and draws no bar**, because a bar is a fraction and a
        // fraction of nothing is a claim nobody measured (§0.0).
        var score = badge.Score;
        var inContinent = Kind.StartsWith(ContinentPrefix, StringComparison.Ordinal);

        ScoreLine = inContinent ? badge.PointsSaid ?? "" : Pts(score.Points);
        LevelName = inContinent || score.Points is null ? "" : score.LevelName;

        if (!inContinent && score.Points is not null && score.NextLevelAt is { } next && next > 0)
        {
            LevelFraction = Math.Clamp((double)score.Worked / next, 0.0, 1.0);
            LevelBarLine = Amount(score.Worked, false) + " of " + Amount(next, true) + " to "
                + score.NextLevelName;
        }
        else
        {
            // **THIRTY CHARACTERS AT MOST**, which is what the bar's 300 px slot holds on the
            // test host. The first cut ran to 46 and clipped on continent-EU (§6: shortened).
            NoNextLevelLine = inContinent ? "no levels per continent"
                : score.Points is null ? "no levels: file unreadable"
                : score.Level == 0 ? "no levels in the points file"
                : score.LevelName + ", the top level";
        }

        // **THE GAP IS ON THE LINE AS THE PICTURE DRAWS IT** (work instruction 342, ruling 13,
        // overruling unit 335's choice to say it once): `· 14 to Silver` here and `11 of 25 to
        // Silver` over the bar. **Where the line would pass BandLineMost characters - the band
        // line's 608 px slot at the window's own 1040 on the test host - the meaning goes first**,
        // because the name above it already says what the kind is, **and the gap only if that is
        // still not enough**, because the words over the bar carry it (§6: shortened, and said
        // which). On the shipped points file and the twelve-contact fixture that takes the meaning
        // off Grids and Total Miles, and the gap off nothing.
        var full = Joined(Meaning, Standing, ScoreLine, LevelName, GapLine);
        var shorter = Joined(Standing, ScoreLine, LevelName, GapLine);

        BandLine = full.Length <= BandLineMost ? full
            : shorter.Length <= BandLineMost ? shorter
            : Joined(Standing, ScoreLine, LevelName, HasLevelBar ? "" : GapLine);
    }

    /// <summary>
    /// **The most characters the band line carries** (work instruction 342, ruling 13): its slot at
    /// the window's own 1040 is 608 px, and the test host draws twelve-point text ten pixels a
    /// character, which is wider than any face on the glass.
    /// </summary>
    public const int BandLineMost = 60;

    /// <summary>What the white ink on a band has to clear against its fill (§0.6).</summary>
    public const double LeastContrast = 4.5;

    /// <summary>
    /// **The ink on the band**: white where white clears 4.5:1 against the band's fill, and
    /// near-black where it does not.
    /// </summary>
    /// <remarks>
    /// **COMPUTED, NOT CHOSEN BY EYE** (§0.6: every ink clears 4.5:1 against its own fill).
    /// The Hall of Fame gold `#A8811A` holds white at about 3.6:1, so its band is inked dark;
    /// every other band holds white.
    /// </remarks>
    public string BandInk => Contrast("#FFFFFF", Band) >= LeastContrast ? "#FFFFFF" : "#1B1B1B";

    /// <summary>The WCAG contrast ratio between two `#RRGGBB` colors.</summary>
    /// <param name="first">One color.</param>
    /// <param name="second">The other.</param>
    /// <returns>From 1 to 21.</returns>
    public static double Contrast(string first, string second)
    {
        var a = Luminance(first);
        var b = Luminance(second);

        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    private static double Luminance(string hex)
    {
        var text = hex.TrimStart('#');

        double Channel(int at)
        {
            var value = int.Parse(text.Substring(at, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;

            return value <= 0.03928 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        return (0.2126 * Channel(0)) + (0.7152 * Channel(2)) + (0.0722 * Channel(4));
    }

    /// <summary>
    /// **The line under the name**: what the kind is, the count, the score, the level and the
    /// gap - `one per entity · 8 worked · 40 pts · unranked · 2 to Bronze`.
    /// </summary>
    public string BandLine { get; }

    /// <summary>The score alone: `40 pts`, or "" where the file could not be read.</summary>
    public string ScoreLine { get; }

    /// <summary>The level's name: `Bronze`, or "" where there is none to say.</summary>
    public string LevelName { get; }

    /// <summary>How far toward the next level, 0 to 1.</summary>
    public double LevelFraction { get; }

    /// <summary>What the level bar is of: `8 of 10 to Bronze`, or "" where there is no bar.</summary>
    public string LevelBarLine { get; } = "";

    /// <summary>True where there is a next level and so a bar toward it.</summary>
    public bool HasLevelBar => LevelBarLine.Length > 0;

    /// <summary>Where there is no next level, the words that say so; otherwise "".</summary>
    public string NoNextLevelLine { get; } = "";

    /// <summary>True where the band says there is no next level.</summary>
    public bool HasNoNextLevel => NoNextLevelLine.Length > 0;

    /// <summary>How many cards and badges this category draws, for the record (R13).</summary>
    public int RenderedCount => Cards.Count + SubBadges.Count;

    /// <summary>A count as the band says it: `8`, or `50,000 mi` for Total Miles' target.</summary>
    private string Amount(long value, bool withUnit)
        => value.ToString("#,0", CultureInfo.InvariantCulture)
            + (withUnit && Kind == AchievementKinds.TotalMiles ? " mi" : "");

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

    // **TOTAL MILES' CATEGORY-WIDE BAR IS GONE** (work instruction 335 task 4): each tier card
    // carries its own bar now, and the band carries the level bar.

    /// <summary>Open a category over a page.</summary>
    /// <param name="kind">A kind from the page, or `continent-XX`.</param>
    /// <param name="page">The page it opens from.</param>
    /// <returns>The category, or null where the kind is not one Hamlet draws.</returns>
    /// <param name="calling">The CQ list as it was read, for the next cards, or null.</param>
    /// <param name="bet">The green zone's best bet as it was read, for the Bands next card, or null.</param>
    public static AchievementCategory? For(
        string kind, AchievementBadgePage page, CqSnapshot? calling = null, BandBet? bet = null)
    {
        ArgumentNullException.ThrowIfNull(page);

        var heard = calling ?? CqSnapshot.None;
        var best = bet ?? BandBet.None;

        if (kind.StartsWith(ContinentPrefix, StringComparison.Ordinal))
        {
            return ForContinent(kind[ContinentPrefix.Length..], page, heard);
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
                ContinentBadges(page, heard));
        }

        if (kind == AchievementKinds.TotalMiles)
        {
            return MilesFor(badge, page);
        }

        var cards = kind switch
        {
            AchievementKinds.HallOfFame => HallOfFame(log, points, page.OperatorGrid, heard, best),
            AchievementKinds.Countries => Earned(
                kind,
                log.Entities.OrderBy(EntitySpoken.Of, StringComparer.OrdinalIgnoreCase)
                    .Select(e => (EntitySpoken.Of(e), Where(log, c => Same(c.Entity, e))))
                    .ToList(),
                long.MaxValue,
                points,
                page.OperatorGrid,
                placeUnderCall: false,
                heard,
                c => UnworkedCountry(log, c, null)),

            AchievementKinds.States => StatesFor(log, points, page.OperatorGrid),
            AchievementKinds.Grids => Earned(
                kind,
                log.Grids.OrderBy(g => g, StringComparer.Ordinal)
                    .Select(g => (g, Where(log, c => c.Grid is { Length: >= 4 } at && Same(at[..4], g))))
                    .ToList(),
                long.MaxValue,
                points,
                page.OperatorGrid,
                placeUnderCall: true,
                heard,
                c => UnworkedGrid(log, c)),
            AchievementKinds.Bands => BandsFor(log, points, page.OperatorGrid, heard, best),
            _ => ModesFor(log, points, page.OperatorGrid, heard, best),
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

    /// <summary>
    /// **The firsts he holds, each the contact that earned it, in the order an evening reaches
    /// them, and the next** (work instruction 335 task 4, R22).
    /// </summary>
    private static List<AchievementCategoryCard> HallOfFame(
        AchievementLog log, AchievementPoints points, string operatorGrid, CqSnapshot calling, BandBet bet)
    {
        var earned = AchievementScores.FirstsEarned(log);
        var cards = new List<AchievementCategoryCard>();

        foreach (var (key, said) in AchievementBadgePage.Firsts)
        {
            if (!earned.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var worth = Pts(points.Special(AchievementKinds.HallOfFame, key));

            cards.Add(EarnerOf(log, key) is { } contact
                ? EarnedBy(said, new[] { contact }, worth, operatorGrid, placeUnderCall: true)
                : new AchievementCategoryCard(said, "", worth, true));
        }

        // **THE SAME NEXT FIRST THE BADGE SHOWS**, chosen in one place so the page and the
        // category cannot come to disagree about what §3.1 keeps absent - **left exactly as unit
        // 333 chose it, collision and all.** What this unit adds is what it measures toward.
        if (AchievementBadgePage.NextFirstOf(earned) is { } next)
        {
            cards.Add(NextFirst(
                log, next, Pts(points.Special(AchievementKinds.HallOfFame, next.Key)),
                operatorGrid, calling, bet));
        }

        return cards;
    }

    /// <summary>
    /// **The contact that earned a first, by the rule `AchievementScores.FirstsEarned` decides
    /// it with**: the earliest that meets it.
    /// </summary>
    /// <remarks>
    /// **`first_dx` TAKES HIS OWN ENTITY AS `FirstsEarned` DOES** - the first entity in the file -
    /// so the card and the score cannot disagree about which contact was the DX one.
    /// </remarks>
    private static AchievementContact? EarnerOf(AchievementLog log, string key)
    {
        var mine = log.Contacts.Select(c => c.Entity).FirstOrDefault(e => e is not null);

        IEnumerable<AchievementContact> among = key switch
        {
            "first_contact" => log.Contacts,
            "first_dx" => log.Contacts.Where(c => c.Entity is not null && mine is not null && !Same(c.Entity, mine)),
            "first_psk31" => log.InMode("PSK31"),

            // **AND THE OTHER KEYBOARD MODE** (work instruction 379 task 2, criterion 8.2).
            // Without this row the key fell to the empty default below, so `first_olivia` was
            // **earned and scored with nobody's name on it**: measured at task 1 on one log
            // holding both contacts, `A PSK31 contact` carried `1 contact`, the callsign, the
            // grid, `20 m · PSK31` and the date, and `An Olivia contact` carried five empty
            // strings for the same facts. The score agreed and the card did not, which is the
            // worse half - a page saying he earned a first and unable to say who gave it to him.
            // **The name is the table's** (`ContactModes.OliviaName`), so this row and the log's
            // own `Mode.Name` cannot come to disagree about the spelling.
            "first_olivia" => log.InMode(ContactModes.OliviaName),

            "first_cw_qso" => log.InMode("CW"),
            "first_over_5000_miles" => log.Contacts.Where(c => c.Miles >= 5000),
            "first_over_10000_miles" => log.Contacts.Where(c => c.Miles >= 10000),
            _ => Array.Empty<AchievementContact>(),
        };

        return among.OrderBy(c => c.StartedUtc ?? DateTime.MaxValue).FirstOrDefault();
    }

    /// <summary>What the Morse first says, because the CQ list is the digital decoded list.</summary>
    public const string NoMorseOnTheList = "the CQ list carries no Morse";

    /// <summary>
    /// **The next first, with what it measures toward**: a bar for a distance first, the callers
    /// for a first a caller could earn, or the words where the list cannot carry one.
    /// </summary>
    private static AchievementCategoryCard NextFirst(
        AchievementLog log,
        (string Key, string Said) next,
        string pointsLine,
        string operatorGrid,
        CqSnapshot calling,
        BandBet bet)
    {
        var mine = log.Contacts.Select(c => c.Entity).FirstOrDefault(e => e is not null);

        switch (next.Key)
        {
            case "first_over_5000_miles":
            case "first_over_10000_miles":
            {
                // **A BAR OF THE FURTHEST CONTACT TOWARD THE LINE**, with both figures in words.
                var line = next.Key == "first_over_5000_miles" ? 5000L : 10000L;
                var furthest = log.Contacts.Where(c => c.Miles is not null)
                    .Select(c => c.Miles!.Value)
                    .DefaultIfEmpty(0)
                    .Max();

                return new AchievementCategoryCard(next.Said, AchievementCategoryCard.NextWord, pointsLine, false)
                {
                    WantsLine = "A contact over " + Number(line) + " miles away",
                    TierFraction = Math.Min(1.0, furthest / line),
                    TierLine = "furthest " + Number((long)Math.Round(furthest)) + " of " + Number(line) + " mi",
                };
            }

            case "first_psk31":
                return NextCard(
                    next.Said, pointsLine,
                    LivesAt("PSK31", bet.Band) is { Length: > 0 } at ? "PSK31 lives at " + at : "Any PSK31 contact",
                    calling,
                    CallersFrom(calling, operatorGrid, c => c.Mode == "PSK31" ? Earns(PlaceOf(c)) : null),
                    NoOneCalling);

            // **THE CQ LIST IS THE DIGITAL DECODED LIST AND CARRIES NO MORSE**, so saying no one is
            // calling in Morse would be a claim about a list that could never show one (§0.0).
            case "first_cw_qso":
                return new AchievementCategoryCard(next.Said, AchievementCategoryCard.NextWord, pointsLine, false)
                {
                    WantsLine = "A contact in Morse",
                    NoCallerLine = NoMorseOnTheList,
                };

            case "first_dx":
                return NextCard(
                    next.Said, pointsLine, "A station outside your own country", calling,
                    CallersFrom(
                        calling, operatorGrid,
                        c => mine is not null && DxccPrefixes.EntityOf(c.Callsign) is { } e && !Same(e, mine)
                            ? Earns(EntitySpoken.Short(e))
                            : null),
                    NoOneCalling);

            default:
                return NextCard(
                    next.Said, pointsLine, "Any station at all", calling,
                    CallersFrom(calling, operatorGrid, c => Earns(PlaceOf(c))),
                    NoOneCalling);
        }
    }

    /// <summary>
    /// **Bands: the first contact on each, and the green zone's best bet first among the bands
    /// not yet worked** (work instruction 335 task 4, R22).
    /// </summary>
    /// <remarks>
    /// **THE BEST BET IS THE GREEN ZONE'S AND NOT A SECOND OPINION**: copied off the band button
    /// the ranking badged, in the badge's own words, so a clock guess is never repeated as an
    /// observation. The rest keep the band row's order; nothing is ranked here.
    /// </remarks>
    private static List<AchievementCategoryCard> BandsFor(
        AchievementLog log, AchievementPoints points, string operatorGrid, CqSnapshot calling, BandBet bet)
    {
        var kind = AchievementKinds.Bands;
        var per = points.Per(kind);

        // **A SPECIAL REPLACES THE PER** (`AchievementScores.Named`), so 160 m says 15.
        var cards = log.Bands
            .Select(b => EarnedBy(
                AdifLog.BandDisplayNameFor(b) is { Length: > 0 } name ? name : b,
                log.OnBand(b),
                Pts(points.Special(kind, b) ?? per),
                operatorGrid,
                placeUnderCall: true))
            .ToList();

        var worked = log.Bands
            .Select(AdifLog.BandDisplayNameFor)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unworked = Hamlet.RadioEngine.Bands.HfBands.Names
            .Where(n => !worked.Contains(n))
            .OrderBy(n => string.Equals(n, bet.Band, StringComparison.Ordinal) ? 0 : 1)
            .Select(n => new NextCaller(n, string.Equals(n, bet.Band, StringComparison.Ordinal) ? bet.Label : ""))
            .ToList();

        if (unworked.Count > 0)
        {
            cards.Add(NextCard(
                    AchievementBadgePage.NextWords(kind, log.Bands.Count), Pts(per),
                    "A band you have not worked", calling, unworked, "")
                with
                {
                    CallersHeading = "bands you have not worked",
                    MoreCallersLine = unworked.Count > ListedCallers
                        ? "and " + (unworked.Count - ListedCallers).ToString(CultureInfo.InvariantCulture) + " more"
                        : "",
                    CountLine = bet.Band.Length > 0 && worked.Contains(bet.Band)
                        ? bet.Label + ": " + bet.Band + ", worked"
                        : "",
                });
        }

        return cards;
    }

    /// <summary>
    /// **Modes: the first contact in each, and where each unworked mode lives and who is there**
    /// (work instruction 335 task 4, R22).
    /// </summary>
    /// <remarks>
    /// <para>**WHERE IT LIVES IS A TABLE HAMLET ALREADY CITES**, on the green zone's best-bet band
    /// where that band has a place and the lowest band that does otherwise: a digital mode's row in
    /// `DigitalCallingFrequencies`, and **CW where a band button lands** (`CwBand.JumpHz`,
    /// HM-DEC-110; work instruction 346 ruling 25). It is read, and nothing that tunes is touched.
    /// Voice has no cited place, so its row says who is there alone.</para>
    /// <para>**WHO IS THERE SAYS ONLY WHAT THE CQ LIST CAN KNOW, AND NEVER NOTHING** (§0.0; ruling
    /// 26). The list is the digital decoded list, so it carries no Morse and no voice; an FT8-shaped
    /// row does not say whether it was FT8 or FT4 (`CqCall.Mode`); and a PSK31 row does say its mode,
    /// so PSK31 names its nearest caller or says no one was calling CQ in it when the list was read.</para>
    /// <para>**`ModeFirstRow.Why` IS NOT DRAWN** - it says false things (parked).</para>
    /// </remarks>
    private static List<AchievementCategoryCard> ModesFor(
        AchievementLog log, AchievementPoints points, string operatorGrid, CqSnapshot calling, BandBet bet)
    {
        var kind = AchievementKinds.Modes;
        var per = points.Per(kind);

        var cards = log.Modes
            .Select(m => EarnedBy(m, log.InMode(m), Pts(points.Special(kind, m) ?? per), operatorGrid, placeUnderCall: true))
            .ToList();

        // **EVERY MODE THERE IS TO WORK, WHICH SINCE THE OLIVIA PHASE IS SIX** (work
        // instruction 368 decisions BY and BZ). The badge's standing counts against
        // `AchievementScores.WorkableModes`, so an unworked list read off a shorter table
        // would leave the badge saying *one more mode* over a page with no card for it.
        // **An unworked row is an invitation and not a claim that he worked it**, which is
        // what every unworked mode gets and what 5.2 expressly allows before the first
        // Olivia contact.
        var unworked = ContactModes.Logged
            .Where(m => m.IsContactMode && !log.Modes.Contains(m.Name, StringComparer.OrdinalIgnoreCase))
            .Select(m => new NextCaller(m.Name, Joined(LivesAt(m.Name, bet.Band), WhoIsThere(calling, operatorGrid, m.Name))))
            .ToList();

        if (unworked.Count > 0)
        {
            cards.Add(NextCard(
                    AchievementBadgePage.NextWords(kind, log.Modes.Count), Pts(per),
                    "A mode you have not worked", calling, unworked, "")
                with
                {
                    CallersHeading = "where each one lives",
                    MoreCallersLine = unworked.Count > ListedCallers
                        ? "and " + (unworked.Count - ListedCallers).ToString(CultureInfo.InvariantCulture) + " more"
                        : "",
                });
        }

        return cards;
    }

    /// <summary>
    /// `14.070 on 20 m` from the cited digital table, `18.080 on 17 m` for CW where a band button lands,
    /// or "" where the mode has neither.
    /// </summary>
    private static string LivesAt(string mode, string betBand)
    {
        // **CW IS WHERE THE BAND BUTTON LANDS** (HM-DEC-110, work instruction 346 ruling 25): the same
        // `CwBand.JumpHz` a press on the band uses, read here and never written.
        if (string.Equals(mode, "CW", StringComparison.OrdinalIgnoreCase))
        {
            var cw = Hamlet.RadioEngine.Bands.HfBands.Bands;
            var band = cw.FirstOrDefault(b => string.Equals(b.Name, betBand, StringComparison.Ordinal)) ?? cw.FirstOrDefault();

            return band is null ? "" : OnBand(band.JumpHz, band.Name);
        }

        // **OLIVIA'S SPOT IS IN ITS OWN CITED TABLE** (work instruction 368; unit 358 task 4).
        // `DigitalCallingFrequencies` holds the band-plan blocks and Olivia has none - its
        // calling frequencies are `data/bands/olivia-calling.json`, which is where the tab
        // itself reads them from - so without this the one unworked mode a beginner is being
        // invited to work was the one row that could not say where to find it. **The number is
        // the table's own and is not written here** (§0).
        if (string.Equals(mode, ContactModes.OliviaName, StringComparison.OrdinalIgnoreCase))
        {
            var table = Hamlet.RadioEngine.Olivia.OliviaData.Current.Calling;
            var row = table?.CallingRowFor(betBand) ?? table?.Rows.FirstOrDefault();

            return row is not null && table!.DialHzFor(row) is { } dial
                ? OnBand(dial, row.Band)
                : "";
        }

        var bands = Hamlet.RadioEngine.Bands.DigitalCallingFrequencies.BandsWith(mode);

        if (bands.Count == 0)
        {
            return "";
        }

        var on = bands.Contains(betBand, StringComparer.Ordinal) ? betBand : bands[0];

        return Hamlet.RadioEngine.Bands.DigitalCallingFrequencies.Find(on, mode) is { } block
            ? OnBand(block.JumpHz, on)
            : "";
    }

    /// <summary>`3.580 on 80 m`.</summary>
    private static string OnBand(long hz, string band)
        => (hz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture) + " on " + band;

    /// <summary>
    /// **Who is there in a mode, as far as the CQ list can know, and never ""** (work instruction 346
    /// ruling 26).
    /// </summary>
    /// <remarks>
    /// <para>**ONLY A PSK31 ROW SAYS ITS MODE** (`CqSnapshot.From`), so only PSK31 names a caller or
    /// says no one is calling: the nearest by the miles his CQ's grid gives, with how many more. A
    /// PSK31 caller's grid is the one his certain reading holds (work instruction 347 ruling 29), so
    /// he carries his distance where his CQ sent a grid, and is his callsign alone where it sent none
    /// or the reading was uncertain.</para>
    /// <para>**NEVER *NO ONE* WHERE THE LIST COULD NOT SHOW ONE.** FT8 and FT4 rows cannot be told
    /// apart, and Morse and voice are never on it.</para>
    /// </remarks>
    private static string WhoIsThere(CqSnapshot calling, string operatorGrid, string mode)
    {
        switch (mode)
        {
            case "CW":
                return NoMorseOnTheList;
            case "FT4":
                return ListCannotTellFt4;
            case "FT8":
                return ListCannotTellFt8;
            case "PSK31":
                break;
            default:
                return string.Equals(mode, ContactModes.OliviaName, StringComparison.OrdinalIgnoreCase)
                    ? ListCannotTellOlivia
                    : NoVoiceOnTheList;
        }

        if (calling.ReadUtc is null)
        {
            return ListNotRead;
        }

        var inIt = calling.Calls
            .Where(c => string.Equals(c.Mode, mode, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => MilesBetween(operatorGrid, c.Grid) ?? double.MaxValue)
            .ToList();

        return inIt.Count == 0
            ? NoOneCallingInItAt + calling.ReadAt
            : Joined(inIt[0].Callsign, MilesTo(operatorGrid, inIt[0].Grid))
                + (inIt.Count > 1 ? " and " + (inIt.Count - 1).ToString(CultureInfo.InvariantCulture) + " more" : "");
    }

    /// <summary>The caller's country, short, or "" where the table declines.</summary>
    private static string PlaceOf(CqCall call)
        => DxccPrefixes.EntityOf(call.Callsign) is { } entity ? EntitySpoken.Short(entity) : "";

    private static (string Place, bool OpensContinent)? Earns(string place) => (place, false);

    /// <summary>`50,000`.</summary>
    private static string Number(long value) => value.ToString("#,0", CultureInfo.InvariantCulture);

    /// <summary>What a card with no grid says where its map would be.</summary>
    public const string NoGridWord = "no grid, so no map";

    /// <summary>What a card says where the operator's own grid is not set.</summary>
    public const string NoOwnGridWord = "set your grid for a map";

    /// <summary>What a card says where the station is somewhere this picture does not reach.</summary>
    public const string OffTheMapWord = "off the edge of this map";

    /// <summary>How many contacts a card with no map lists where the map would be.</summary>
    private const int ListedContacts = 3;

    private static IReadOnlyList<AchievementContact> Where(
        AchievementLog log, Func<AchievementContact, bool> wanted)
        => log.Contacts.Where(wanted).ToList();

    /// <summary>One card per place he has, each the contact that earned it, then the next.</summary>
    /// <param name="kind">Countries or Grids.</param>
    /// <param name="held">Each place as shown, with every contact in it.</param>
    /// <param name="howManyThereAre">The set's size, or long.MaxValue where it is open.</param>
    /// <param name="points">The owner's file.</param>
    /// <param name="operatorGrid">The grid the miles were measured from.</param>
    /// <param name="placeUnderCall">True where the callsign line names the country, not the grid.</param>
    /// <param name="calling">The CQ list as it was read.</param>
    /// <param name="earns">What working a caller would earn here, or null.</param>
    /// <param name="wants">What the next card wants, where it is not the kind's own words.</param>
    /// <param name="quill">The quill sentence, where the caller knows it; Countries works it out otherwise.</param>
    private static List<AchievementCategoryCard> Earned(
        string kind,
        IReadOnlyList<(string Shown, IReadOnlyList<AchievementContact> Contacts)> held,
        long howManyThereAre,
        AchievementPoints points,
        string operatorGrid,
        bool placeUnderCall,
        CqSnapshot calling,
        Func<CqCall, (string Place, bool OpensContinent)?> earns,
        string? wants = null,
        string? quill = null)
    {
        var per = points.Per(kind);
        var cards = held
            .Select(h => EarnedBy(h.Shown, h.Contacts, Pts(per), operatorGrid, placeUnderCall))
            .ToList();

        if (held.Count < howManyThereAre)
        {
            var callers = CallersFrom(calling, operatorGrid, earns);

            cards.Add(NextCard(
                    AchievementBadgePage.NextWords(kind, held.Count),
                    Pts(per),
                    wants ?? WantsFor(kind),
                    calling,
                    callers,
                    NoOneCalling)
                with
                {
                    // **COUNTRIES SAYS WHICH QUILL FROM THE CALLERS' OWN MARKS; GRIDS SAYS NONE**,
                    // because the decoded list marks a country and never a square (ruling 14).
                    QuillLine = quill ?? (kind == AchievementKinds.Countries ? QuillFor(callers) : ""),
                });
        }

        return cards;
    }

    /// <summary>
    /// **The picture's sentence**, for a next card whose callers carry the still green quill on the
    /// CQ list (work instruction 342, ruling 14).
    /// </summary>
    /// <remarks>
    /// <para>**ONLY WHERE THE LIST DRAWS IT.** A decoded row is marked by its sender's country
    /// (`MainWindowViewModel.MarkIfItOpensSomething`, `NudgeSet.WouldOpen`): the still green quill
    /// for a country not in the log on a continent that is, the ringed amber quill where the
    /// continent is new too, and nothing for a country already worked. **It never marks a square,
    /// a state, a band or a mode**, so Grids, States, Bands, Modes, Total Miles and Hall of Fame
    /// carry no sentence at all.</para>
    /// <para>**THE SAME LENGTH WHERE IT IS NOT GREEN**: <see cref="RingedQuill"/> where every
    /// caller who would earn the card opens a continent, and <see cref="AnyQuill"/> where they are
    /// mixed or nobody is listed - each caller's own mark beside him says which.</para>
    /// </remarks>
    public const string GreenQuill = "On the CQ list they carry the green quill.";

    /// <summary>What the sentence says where every caller who would earn the card opens a continent.</summary>
    public const string RingedQuill = "On the CQ list they carry the ringed quill.";

    /// <summary>What the sentence says where the callers carry either quill, or none is listed.</summary>
    public const string AnyQuill = "On the CQ list they carry a quill.";

    /// <summary>Which of the three sentences is true of a Countries next card's callers.</summary>
    private static string QuillFor(IReadOnlyList<NextCaller> callers)
        => callers.Count == 0 ? AnyQuill
            : callers.All(c => c.OpensContinent) ? RingedQuill
            : callers.All(c => !c.OpensContinent) ? GreenQuill
            : AnyQuill;

    /// <summary>
    /// **States: each state he holds is the contact that earned it, then the next** (work
    /// instruction 336 task 1, R23).
    /// </summary>
    /// <remarks>
    /// <para>**THE SAME BUILDER COUNTRIES USES**, over the contacts
    /// <see cref="AchievementLog.StateOf"/> scores, so a state's card is the earliest of them.
    /// Each card carries what that state is worth in the file: its `special` where it has one,
    /// else `per`.</para>
    /// <para>**A CQ CARRIES NO STATE** (the arbiter's proposal of unit 335, marked for Tim): the
    /// next card says what it wants and that Hamlet cannot tell a caller's state, and never that no
    /// one is calling from there, which nobody measured (§0.0). A state guessed from a prefix is
    /// rejected: a W3 can be anywhere. **That card is left exactly as unit 335 drew it.**</para>
    /// </remarks>
    private static List<AchievementCategoryCard> StatesFor(
        AchievementLog log, AchievementPoints points, string operatorGrid)
    {
        const string kind = AchievementKinds.States;
        var per = points.Per(kind);
        var held = log.States;

        var cards = held.OrderBy(s => s, StringComparer.Ordinal)
            .Select(s => EarnedBy(
                s, log.InState(s), Pts(points.Special(kind, s) ?? per), operatorGrid, placeUnderCall: false))
            .ToList();

        if (held.Count < AchievementLog.FiftyStates.Count)
        {
            cards.Add(new AchievementCategoryCard(
                AchievementBadgePage.NextWords(kind, held.Count), AchievementCategoryCard.NextWord, Pts(per), false)
            {
                WantsLine = WantsFor(kind),
                NoCallerLine = NoStateFromTheAir,
                CountLine = NoStateLine(log),
            });
        }

        return cards;
    }

    /// <summary>
    /// **How many United States, Alaska and Hawaii records carry no `STATE`**, in words - `3 US contacts
    /// carry no STATE` - or "" where none do (work instruction 348 ruling 35).
    /// </summary>
    /// <remarks>
    /// <para>**A COUNT HAMLET CAN READ FROM THE LOG, AND NOTHING ABOUT WHERE THOSE STATIONS ARE**
    /// (§0.0): it is why the States count can sit far below the US contacts, and it names no state and
    /// guesses none from a callsign.</para>
    /// <para>**A US RECORD IS ONE A `STATE` WOULD SCORE ON**: the contact with `PA` put in its place
    /// scores under <see cref="AchievementLog.StateOf"/>, which keeps the three entities in one place. A
    /// record that carries a `STATE` scoring nothing, such as `DC`, carries one and is not counted.</para>
    /// </remarks>
    private static string NoStateLine(AchievementLog log)
    {
        var none = log.Contacts.Count(c => c.State is null && AchievementLog.StateOf(c with { State = "PA" }) is not null);

        return none switch
        {
            0 => "",
            1 => "1 US contact carries no STATE",
            _ => none.ToString("#,0", CultureInfo.InvariantCulture) + " US contacts carry no STATE",
        };
    }

    /// <summary>What a next card says where nobody on the CQ list would earn it.</summary>
    public const string NoOneCalling = "no one is calling from there now";

    /// <summary>What the States next card says instead (the arbiter's proposal, marked for Tim).</summary>
    /// <remarks>
    /// **SHORTENED TO FIT** (§6): *Hamlet cannot tell a caller's state from the air* needed 480 px
    /// in a 426 px slot at the window's 1040. The claim is the same without *from the air*.
    /// </remarks>
    public const string NoStateFromTheAir = "Hamlet cannot tell a caller's state";

    /// <summary>What a next card says where no CQ list was handed to the window.</summary>
    public const string ListNotRead = "the CQ list was not read";

    /// <summary>
    /// What a Modes row says where the list's rows say that mode and nobody is calling in it, before the
    /// time the list was read: `no one calling at 21:41 UTC`.
    /// </summary>
    /// <remarks>
    /// <para>**NEVER *NOW*** (work instruction 347 ruling 31): the list is read once when the window opens
    /// (ruling 14), so the row says when nobody was on it rather than that nobody is calling at all.</para>
    /// <para>**SHORTENED TO FIT** (§6): *no one calling CQ in it at 21:41 UTC* squeezed the row's place
    /// column at 1400, so *PSK31* needed 50 px in a 48 px slot. The row already names the mode, so
    /// *CQ in it* went.</para>
    /// </remarks>
    public const string NoOneCallingInItAt = "no one calling at ";

    /// <summary>What the FT4 row says: an FT8-shaped row does not say which of the two it was.</summary>
    public const string ListCannotTellFt4 = "the CQ list cannot tell FT4 from FT8";

    /// <summary>What the FT8 row says, for the same reason.</summary>
    public const string ListCannotTellFt8 = "the CQ list cannot tell FT8 from FT4";

    /// <summary>What the Voice row says: the CQ list is the digital decoded list.</summary>
    public const string NoVoiceOnTheList = "the CQ list carries no voice";

    /// <summary>What the Olivia row says, for the reason the FT4 row has its own sentence.</summary>
    /// <remarks>
    /// <para>**THE LIST CANNOT TELL THE TWO KEYBOARD MODES APART** (work instruction 368, §0.0).
    /// `CqSnapshot` reads a text-only row as `PSK31`, and since unit 364 an Olivia row is a
    /// text-only row, so a CQ heard on Olivia is on that list under PSK31's name. Saying *no
    /// one calling in it* would be a claim the list cannot support, and saying *the CQ list
    /// carries no voice* - which is what the Voice branch answered before this unit - would be
    /// a sentence about a different mode entirely.</para>
    /// <para>**IT IS SHORTER THAN THE FT4 SENTENCE BECAUSE THE ROW IS** (work instruction 332's
    /// rule, measured here rather than guessed). The mode's name is in the row's own left
    /// column and its calling spot is in front of this, and the longer form - *the CQ list
    /// cannot tell Olivia from PSK31* - clipped the mode's name to nothing at 1400 px on the
    /// test host. The fact stated is the same one.</para>
    /// </remarks>
    public const string ListCannotTellOlivia = "reads as PSK31 on the CQ list";

    /// <summary>How many callers a next card lists.</summary>
    private const int ListedCallers = 3;

    /// <summary>What a next card wants, in words, by kind.</summary>
    private static string WantsFor(string kind) => kind switch
    {
        AchievementKinds.Countries => "Any country you have not worked",
        AchievementKinds.Grids => "Any grid you have not worked",
        AchievementKinds.States => "Any state you have not worked",
        _ => "",
    };

    /// <summary>
    /// **The next card: what it wants, and the one thing Hamlet knows that helps** (work
    /// instruction 335 task 3, R22).
    /// </summary>
    /// <remarks>
    /// <para>**THE HEADING CARRIES THE TIME THE LIST WAS READ** rather than *right now*, because
    /// the window is modal and the list is not kept live (see <see cref="CqSnapshot"/>).</para>
    /// <para>**WHERE NO LIST WAS HANDED IN IT SAYS SO**, and not that no one is calling: nobody
    /// looked (§0.0).</para>
    /// </remarks>
    private static AchievementCategoryCard NextCard(
        string title,
        string pointsLine,
        string wants,
        CqSnapshot calling,
        IReadOnlyList<NextCaller> callers,
        string noCaller)
        => new(title, AchievementCategoryCard.NextWord, pointsLine, false)
        {
            WantsLine = wants,
            CallersHeading = calling.ReadUtc is null ? "" : "calling CQ at " + calling.ReadAt + ", unworked",
            Callers = callers.Take(ListedCallers).ToList(),
            MoreCallersLine = callers.Count > ListedCallers
                ? "and " + (callers.Count - ListedCallers).ToString(CultureInfo.InvariantCulture)
                    + " more on the CQ list"
                : "",
            NoCallerLine = callers.Count > 0 ? ""
                : calling.ReadUtc is null ? ListNotRead
                : noCaller,
        };

    /// <summary>Everyone on the CQ list who would earn this card, with how far away he is.</summary>
    private static IReadOnlyList<NextCaller> CallersFrom(
        CqSnapshot calling,
        string operatorGrid,
        Func<CqCall, (string Place, bool OpensContinent)?> earns)
        => calling.Calls
            .Select(c => (Call: c, Earns: earns(c)))
            .Where(x => x.Earns is not null)
            .Select(x => new NextCaller(
                x.Earns!.Value.Place,
                Joined(x.Call.Callsign, MilesTo(operatorGrid, x.Call.Grid)),
                x.Earns.Value.OpensContinent))
            .ToList();

    /// <summary>
    /// **A caller from a country not in the log**, on one continent where one is named - and
    /// whether he would open that continent too.
    /// </summary>
    /// <remarks>
    /// **SILENT WHERE THE ENTITY TABLE DECLINES**, as the quill is (`NudgeSet.WouldOpen`): a
    /// prefix the table will not assign opens nothing as far as Hamlet knows.
    /// </remarks>
    private static (string Place, bool OpensContinent)? UnworkedCountry(
        AchievementLog log, CqCall call, string? onContinent)
    {
        var entity = DxccPrefixes.EntityOf(call.Callsign);

        if (entity is null || log.Entities.Contains(entity, StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        var continent = DxccContinents.Of(entity);

        if (onContinent is not null && !Same(continent, onContinent))
        {
            return null;
        }

        return (EntitySpoken.Short(entity),
            continent is not null && !log.Continents.Contains(continent, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>A caller whose CQ carried a four-character square not in the log.</summary>
    private static (string Place, bool OpensContinent)? UnworkedGrid(AchievementLog log, CqCall call)
    {
        if (call.Grid.Length < 4)
        {
            return null;
        }

        var square = call.Grid[..4].ToUpperInvariant();

        return log.Grids.Contains(square, StringComparer.OrdinalIgnoreCase) ? null : (square, false);
    }

    /// <summary>`4,500 mi` from the operator's grid to his, or "" where either is missing.</summary>
    private static string MilesTo(string operatorGrid, string grid)
        => MilesBetween(operatorGrid, grid) is { } miles
            ? GridPath.DescribeMiles(miles).Replace(" miles", " mi", StringComparison.Ordinal)
            : "";

    /// <summary>The miles from the operator's grid to his, or null where either is missing.</summary>
    private static double? MilesBetween(string operatorGrid, string grid)
        => OperatorLocation.FromGrid(operatorGrid) is { } here && OperatorLocation.FromGrid(grid) is { } there
            ? GridPath.MilesBetween(here, there)
            : null;

    /// <summary>
    /// **The card for one place, built from the contact that earned it** (work instruction 335
    /// task 2, R22).
    /// </summary>
    /// <param name="title">What the card is: the country, or the square.</param>
    /// <param name="contacts">Every contact in that place.</param>
    /// <param name="pointsLine">What it is worth.</param>
    /// <param name="operatorGrid">The grid the miles were measured from.</param>
    /// <param name="placeUnderCall">True where the callsign line names the country.</param>
    /// <remarks>
    /// <para>**THE EARLIEST CONTACT EARNED IT**, by `StartedUtc`. The sort is stable, so a record
    /// with no date keeps its place in the file and comes after every dated one: a first by
    /// date where there is a date, and the file's own order where there is not.</para>
    /// <para>**EVERY FACT IS THE LOG'S AND NOTHING IS RECOMPUTED.** The distance is
    /// `AchievementContact.Miles`; the map is the conversation card's `Ft8GlobePlot` over the same
    /// two grids, so the drawn path and the printed distance are one measurement.</para>
    /// <para>**A MISSING FACT IS ABSENT, NEVER A DASH** (§6). **A MAP WITH NO GRID IS NO MAP**,
    /// and the card says so in a word and lists the contacts where the map would be, so it is
    /// never a white rectangle.</para>
    /// </remarks>
    internal static AchievementCategoryCard EarnedBy(
        string title,
        IReadOnlyList<AchievementContact> contacts,
        string pointsLine,
        string operatorGrid,
        bool placeUnderCall)
    {
        var inOrder = contacts.OrderBy(c => c.StartedUtc ?? DateTime.MaxValue).ToList();
        var first = inOrder[0];
        // **THE SHORT NAME** (`EntitySpoken.Short`), the tree's own name for a place on a line
        // with a callsign beside it: `W3YNI · United States`, not `W3YNI · the United States`.
        var place = first.Entity is null ? "" : EntitySpoken.Short(first.Entity);
        var grid = first.Grid ?? "";
        var plot = grid.Length > 0 ? new Ft8GlobePlot(operatorGrid, grid, first.Callsign, place) : null;
        var globe = plot is { HasPath: true } ? plot : null;

        return new AchievementCategoryCard(title, Contacts(contacts.Count), pointsLine, true)
        {
            Callsign = first.Callsign,
            Grid = grid,
            CallGridLine = Joined(first.Callsign, placeUnderCall ? place : grid),
            Globe = globe,
            NoMapWord = globe is not null ? ""
                : grid.Length == 0 ? NoGridWord
                : plot is { OperatorGridResolved: false } ? NoOwnGridWord
                : OffTheMapWord,
            ContactLines = globe is not null
                ? Array.Empty<string>()
                : inOrder.Take(ListedContacts)
                    .Select(c => Joined(c.Callsign, AdifLog.BandDisplayNameFor(c.Band), DateOf(c)))
                    .ToList(),
            DistanceLine = first.Miles is { } miles
                ? GridPath.DescribeMiles(miles).Replace(" miles", " mi", StringComparison.Ordinal)
                : "",
            BandModeLine = Joined(AdifLog.BandDisplayNameFor(first.Band), first.Mode?.Name ?? ""),
            DateLine = DateOf(first),
        };
    }

    /// <summary>`Aug 12, 2026`, or "" where the record has no date.</summary>
    private static string DateOf(AchievementContact contact)
        => contact.StartedUtc?.ToString("MMM d, yyyy", CultureInfo.InvariantCulture) ?? "";

    /// <summary>The parts that exist, joined by a middle dot; nothing for the ones that do not.</summary>
    private static string Joined(params string[] parts)
        => string.Join(" · ", parts.Where(p => p.Length > 0));

    /// <summary>
    /// **Total Miles: each tier is a bar toward its line, and a reached tier is the contact that
    /// crossed it** (work instruction 335 task 4, R22).
    /// </summary>
    /// <remarks>
    /// <para>**THE CROSSING IS FOUND BY ADDING THE LOG'S OWN MILES IN DATE ORDER** until the sum
    /// reaches the line - the same miles `AchievementScores.TotalMiles` adds, and a stable sort,
    /// so undated contacts keep file order after the dated ones.</para>
    /// <para>**A BAR OF MILES AND NOTHING ELSE**, with the two figures in words beside it (work
    /// instruction 300 task 4's rule: say what the fraction is of).</para>
    /// </remarks>
    private static AchievementCategory MilesFor(AchievementBadge badge, AchievementBadgePage page)
    {
        var reached = (long)Math.Round(page.Scores.TotalMiles);
        var cards = new List<AchievementCategoryCard>();
        var inOrder = page.Log.Contacts
            .Where(c => c.Miles is not null)
            .OrderBy(c => c.StartedUtc ?? DateTime.MaxValue)
            .ToList();

        foreach (var (at, worth) in page.Scores.Points.Milestones(AchievementKinds.TotalMiles))
        {
            var said = Number(at) + " miles";

            if (reached >= at)
            {
                var sum = 0.0;
                var crossing = inOrder.FirstOrDefault(c => (sum += c.Miles!.Value) >= at);
                var card = crossing is null
                    ? new AchievementCategoryCard(said, "reached", Pts(worth), true)
                    : EarnedBy(said, new[] { crossing }, Pts(worth), page.OperatorGrid, placeUnderCall: true);

                cards.Add(card with { TierFraction = 1.0, TierLine = "reached " + Number(at) + " mi" });
            }
            else
            {
                cards.Add(new AchievementCategoryCard(said, AchievementCategoryCard.NextWord, Pts(worth), false)
                {
                    WantsLine = "Every contact adds its miles",
                    TierFraction = at > 0 ? Math.Min(1.0, (double)reached / at) : 0,
                    TierLine = Number(reached) + " of " + Number(at) + " mi",
                });

                break;
            }
        }

        return new AchievementCategory(
            badge, null, null, cards, Array.Empty<AchievementBadge>());
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
    private static IReadOnlyList<AchievementBadge> ContinentBadges(
        AchievementBadgePage page, CqSnapshot calling)
    {
        var log = page.Log;
        var points = page.Scores.Points;

        return DxccContinents.Codes
            .OrderBy(c => c.Value, StringComparer.Ordinal)
            .Select(c => ContinentBadge(c.Key, log, points) with
            {
                Card = ContinentCard(c.Key, log, points, page.OperatorGrid, calling),
            })
            .ToList();
    }

    /// <summary>
    /// **A continent as a trading card** (work instruction 335 task 4, R22): the first contact
    /// that opened it with the count of countries worked there, or the continent named with who
    /// is calling from it.
    /// </summary>
    private static AchievementCategoryCard ContinentCard(
        string code, AchievementLog log, AchievementPoints points, string operatorGrid, CqSnapshot calling)
    {
        var name = DxccContinents.NameOf(code);
        var worth = Pts(points.PerContinent(code));
        var here = log.OnContinent(code);

        if (here.Count > 0)
        {
            var worked = log.EntitiesOn(code);

            return EarnedBy(name, here, worth, operatorGrid, placeUnderCall: true) with
            {
                CountLine = worked.ToString(CultureInfo.InvariantCulture)
                    + (worked == 1 ? " country" : " countries") + " worked there",
            };
        }

        // **A CONTINENT NEVER REACHED: EVERY CALLER ON IT WOULD OPEN IT**, so his row carries the
        // ringed quill (work instruction 342, ruling 14).
        return NextCard(
                name, worth, "A first contact here", calling,
                CallersFrom(calling, operatorGrid, c => UnworkedCountry(log, c, code)),
                NoOneCalling)
            with
            {
                QuillLine = RingedQuill,
            };
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
    private static AchievementCategory? ForContinent(
        string code, AchievementBadgePage page, CqSnapshot calling)
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
            .Select(g => (EntitySpoken.Of(g.Key), (IReadOnlyList<AchievementContact>)g.ToList()))
            .OrderBy(x => x.Item1, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // **THE SAME CARDS COUNTRIES DRAWS** (work instruction 335 task 2): a continent opened to
        // its countries is each of them, the contact that earned it. **Its callers' quill is the
        // continent's**: green where the log has reached it, ringed where it has not (ruling 14).
        var cards = Earned(
            AchievementKinds.Countries, held, DxccContinents.EntitiesOn(code),
            page.Scores.Points, page.OperatorGrid, placeUnderCall: false,
            calling, c => UnworkedCountry(log, c, code),
            "Any unworked country in " + DxccContinents.NameOf(code),
            log.Continents.Contains(code, StringComparer.OrdinalIgnoreCase) ? GreenQuill : RingedQuill);

        return new AchievementCategory(
            badge,
            AchievementKinds.Continents,
            page.Badges.First(b => b.Kind == AchievementKinds.Continents).Name,
            cards,
            Array.Empty<AchievementBadge>());
    }
}

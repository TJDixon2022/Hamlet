using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **One tab: the whole log, or a band, or a mode he has opened.**
/// </summary>
/// <remarks>
/// **THERE IS NO TAB FOR A BAND HE HAS NEVER WORKED** (§3.1). *No empty 80 m tab* is
/// the philosophy's own phrase, and these come out of his log rather than out of a
/// list of bands with the unworked ones filtered off. The difference matters because
/// there is then no list to forget to filter.
/// </remarks>
/// <param name="Key">A stable id, for the reveal to remember it by.</param>
/// <param name="Title">What the tab reads: `Everything`, `40 m`, `FT8`.</param>
/// <param name="Kind">`all`, `band` or `mode`, for ordering and for a test.</param>
/// <param name="Groups">
/// The records inside it, grouped by what they are about. **Only the groups this
/// scope can actually fill.**
/// </param>
public sealed record AchievementScope(
    string Key,
    string Title,
    string Kind,
    IReadOnlyList<AchievementGroup> Groups)
{
    /// <summary>How many cards the tab holds.</summary>
    public int Count => Groups.Sum(g => g.Count);

    /// <summary>What the tab says about itself. Never a count of what is missing.</summary>
    public string Summary => Count == 1 ? "1 record" : Count + " records";
}

/// <summary>
/// **The achievements screen: only what he has opened, and a handful of standing
/// targets.**
/// </summary>
/// <remarks>
/// <para>**`ACHIEVEMENTS_PHILOSOPHY.md` IS THE REASON FOR EVERY RULE IN THIS TYPE**
/// and its §2 outranks completeness: *a wall of empty cards reads as failure to
/// somebody who has felt like a failure at this hobby for years.* Nothing here can
/// produce an unearned record card, and there is no member that counts what he has
/// not done.</para>
/// <para>**THE SHAPE, IN ONE PARAGRAPH.** A band or a mode has no tab until he has
/// made a contact in it, and a continent has no card until he has worked one there
/// (§3.1). Opening a tab brings the records inside it that his own log can already
/// fill (§3.2), so one contact yields several cards that are all **filled** rather
/// than several blanks. **The possibilities come from the challenges**, which stand
/// whether or not they are earned (§3.4), and from a place's own nudge, which is
/// shown only inside a group he has already opened.</para>
/// <para>**RECORDS ARE GROUPED BY WHAT THEY ARE ABOUT AND TABBED BY SCOPE** (work
/// instruction 298 task 3): how far, how faint and when, down the page; everything,
/// then each band, then each mode, across the top.</para>
/// <para>**IT READS AND IT NEVER WRITES.** There is no path from this type to the
/// contact log, to a send path or to the radio. A log record is a statement the
/// operator made and a screen about it may not revise one.</para>
/// <para>**EVERY FIGURE IS THE LOG'S** (§5 question 1). Where a fact is absent the
/// card is absent with it: no grid means no distance card, no report means no signal
/// card, no time means no busiest day.</para>
/// </remarks>
public sealed class AchievementScreen
{
    /// <summary>How many entities of a group to name in a nudge.</summary>
    /// <remarks>
    /// **FOUR IS A GIFT AND FORTY IS A CHORE LIST** (§3.3). The instruction's own
    /// example names four prefixes, and §3.2's is four countries.
    /// </remarks>
    public const int NearestFew = 4;

    private readonly AchievementLog _log;

    /// <summary>Build the screen from the log.</summary>
    /// <param name="log">What the contact log holds.</param>
    /// <param name="challenges">
    /// The standing targets, which are always visible. **They are handed in rather
    /// than built here** because they are the one kind of card that does not come out
    /// of the unlock rule: this type decides what he has opened, and what to aim him
    /// at next is a separate judgement with its own file.
    /// </param>
    /// <exception cref="ArgumentNullException">There is no log.</exception>
    public AchievementScreen(
        AchievementLog log, IReadOnlyList<AchievementCard>? challenges = null)
    {
        ArgumentNullException.ThrowIfNull(log);

        _log = log;

        Scopes = BuildScopes().ToList();
        Places = BuildPlaces().ToList();
        Challenges = challenges ?? Array.Empty<AchievementCard>();
    }

    /// <summary>The tabs he has opened: everything, then bands, then modes.</summary>
    public IReadOnlyList<AchievementScope> Scopes { get; }

    /// <summary>The continents he has worked, each with the countries in it.</summary>
    public IReadOnlyList<AchievementGroup> Places { get; }

    /// <summary>The standing targets, which are always here.</summary>
    /// <remarks>
    /// **§3.4: THESE ARE THE *GO AND TRY THIS* HALF AND HIDING THEM DEFEATS THEM.**
    /// They are the only cards on the screen that may appear unearned.
    /// </remarks>
    public IReadOnlyList<AchievementCard> Challenges { get; }

    /// <summary>Every group on the screen, flattened, for a sweep.</summary>
    public IReadOnlyList<AchievementGroup> Groups
        => Scopes.SelectMany(s => s.Groups).Concat(Places).ToList();

    /// <summary>True where anything at all has been opened.</summary>
    public bool HasGroups => Scopes.Count > 0 || Places.Count > 0;

    /// <summary>Every key that is open, for the reveal to compare with what it saw.</summary>
    /// <remarks>
    /// **THE SCOPES AND THE PLACES, NOT THE CARDS.** §3.2's reveal is about a group
    /// opening - *a new tab appearing is itself the reward* - and announcing every
    /// new record card would fire on a contact that merely beat one.
    /// </remarks>
    public IReadOnlyList<string> OpenKeys
        => Scopes.Select(s => s.Key).Concat(Places.Select(p => p.Key)).ToList();

    /// <summary>What the screen says about itself, counting only what is open.</summary>
    /// <remarks>
    /// **IT COUNTS WHAT HE HAS OPENED AND NEVER WHAT HE HAS NOT** (the instruction,
    /// and §2). Unit 287's `The first of each mode: 1 of 6` is the shape this
    /// replaces: it advertised five slots he could not see.
    /// </remarks>
    public string OpenedLine
    {
        get
        {
            if (_log.Count == 0)
            {
                return "Nothing here yet. The first contact you log opens the "
                    + "first of these, and every one after that opens more.";
            }

            var cards = Scopes.Sum(s => s.Count) + Places.Sum(p => p.Count);

            var tabs = Scopes.Count == 1 ? "1 view" : Scopes.Count + " views";
            var kept = cards == 1 ? "1 record" : cards + " records";

            return $"{tabs} open, {kept} in them.";
        }
    }

    /// <summary>The tabs, in the order they are drawn.</summary>
    private IEnumerable<AchievementScope> BuildScopes()
    {
        if (_log.Count == 0)
        {
            yield break;
        }

        var everything = Grouped("all", "so far", _log.Contacts);

        if (everything.Count > 0)
        {
            yield return new AchievementScope("all", "Everything", "all", everything);
        }

        // **THE BANDS COME OUT OF HIS LOG** (§3.1), not out of `HfBands.Names` with
        // the unworked ones filtered off, because then there is a list to forget to
        // filter.
        foreach (var band in _log.Bands)
        {
            // **THE HEADING IS WHAT A PERSON READS AND THE KEY IS WHAT THE RECORD
            // SAYS.** ADIF spells a band `20m` and every other screen in this
            // application says `20 m`; a heading taken straight off the record read
            // `20m` here and nowhere else, which is the application disagreeing with
            // itself about a word.
            var shown = AdifLog.BandDisplayNameFor(band);
            var groups = Grouped("band-" + band, "on " + shown, _log.OnBand(band));

            if (groups.Count > 0)
            {
                yield return new AchievementScope(
                    "band-" + band, shown, "band", groups);
            }
        }

        foreach (var mode in _log.Modes)
        {
            var groups = Grouped("mode-" + mode, "on " + mode, _log.InMode(mode));

            if (groups.Count > 0)
            {
                yield return new AchievementScope("mode-" + mode, mode, "mode", groups);
            }
        }
    }

    /// <summary>The records for one scope, grouped by what they are about.</summary>
    /// <param name="prefix">A stable key prefix for this scope.</param>
    /// <param name="scope">How the scope is said: `on 40 m`, `so far`.</param>
    /// <param name="among">The contacts in scope.</param>
    /// <returns>Only the groups this scope can fill.</returns>
    /// <remarks>
    /// **HOW FAR, HOW FAINT, WHEN** (work instruction 298 task 3). Three things a
    /// record can be about, and a group with nothing in it is not emitted at all -
    /// which is what stops a scope whose contacts carried no grid squares showing an
    /// empty distance heading.
    /// </remarks>
    private static List<AchievementGroup> Grouped(
        string prefix, string scope, IReadOnlyList<AchievementContact> among)
    {
        var groups = new List<AchievementGroup>();

        Add(groups, prefix + "-distance", "How far", Distance(prefix, scope, among));
        Add(groups, prefix + "-signal", "How faint", Signal(prefix, scope, among));
        Add(groups, prefix + "-days", "When", Days(prefix, scope, among));

        return groups;
    }

    private static void Add(
        List<AchievementGroup> into, string key, string title,
        List<AchievementCard> cards)
    {
        if (cards.Count > 0)
        {
            into.Add(new AchievementGroup(
                key, title, cards.Count == 1 ? "1 record" : cards.Count + " records",
                "", "", cards));
        }
    }

    /// <summary>The distance records, or none.</summary>
    private static List<AchievementCard> Distance(
        string prefix, string scope, IReadOnlyList<AchievementContact> among)
    {
        var cards = new List<AchievementCard>();

        if (AchievementLog.Furthest(among) is { Miles: { } miles } furthest)
        {
            cards.Add(new AchievementCard(
                key: prefix + "-furthest",
                kind: AchievementKind.Record,
                title: "Furthest " + scope,
                figure: GridPath.DescribeMiles(miles),
                station: AchievementCard.StationLine(furthest),
                detail: Bearing(furthest)
                    + "It is the great-circle distance from your own grid square to "
                    + "his, which is the way a radio signal actually travels rather "
                    + "than the way a flat map is drawn. A four-character grid square "
                    + "is a box about seventy miles across, so this figure is good to "
                    + "about that and no better.",
                earned: true));
        }

        return cards;
    }

    /// <summary>The signal records, or none.</summary>
    private static List<AchievementCard> Signal(
        string prefix, string scope, IReadOnlyList<AchievementContact> among)
    {
        var cards = new List<AchievementCard>();

        if (AchievementLog.WeakestReceived(among) is { ReportReceived: { } given } weakest)
        {
            cards.Add(new AchievementCard(
                key: prefix + "-weakest-received",
                kind: AchievementKind.Record,
                title: "Faintest you have been heard " + scope,
                figure: AchievementCard.Signed(given) + " dB",
                station: AchievementCard.StationLine(weakest),
                detail: "This is how far into the noise your signal was when he read "
                    + "it, in decibels, and a lower number is the better record: it "
                    + "means less of you arrived and he read you anyway. FT8 decodes "
                    + "down to about -21 dB, which is well below what an ear can hear "
                    + "at all, so a report near that is your signal at the edge of "
                    + "what any receiver can do with it.",
                earned: true));
        }

        if (AchievementLog.WeakestSent(among) is { ReportSent: { } gave } heard)
        {
            cards.Add(new AchievementCard(
                key: prefix + "-weakest-sent",
                kind: AchievementKind.Record,
                title: "Faintest you have heard " + scope,
                figure: AchievementCard.Signed(gave) + " dB",
                station: AchievementCard.StationLine(heard),
                detail: "The weakest signal you pulled out of the noise and answered. "
                    + "This one is about your receiving rather than your "
                    + "transmitting: your antenna, how electrically quiet your house "
                    + "is, and how much of the band you were listening across. It is "
                    + "the half of a contact you have the most control over.",
                earned: true));
        }

        return cards;
    }

    /// <summary>The time records, or none.</summary>
    private static List<AchievementCard> Days(
        string prefix, string scope, IReadOnlyList<AchievementContact> among)
    {
        var cards = new List<AchievementCard>();

        if (AchievementLog.First(among) is { StartedUtc: { } began } first)
        {
            cards.Add(new AchievementCard(
                key: prefix + "-first",
                kind: AchievementKind.Record,
                title: "First " + scope,
                figure: began.ToString("d MMMM yyyy", CultureInfo.InvariantCulture),
                station: AchievementCard.StationLine(first),
                detail: "The one that started it. Every log is kept in UTC, which is "
                    + "the one clock everybody on the band shares, so a late evening "
                    + "contact can carry tomorrow's date and still be the same "
                    + "evening to you.",
                earned: true));
        }

        if (AchievementLog.BusiestDay(among) is { } day)
        {
            cards.Add(new AchievementCard(
                key: prefix + "-busiest-day",
                kind: AchievementKind.Record,
                title: "Busiest day " + scope,
                figure: day.Count == 1
                    ? "1 contact"
                    : day.Count.ToString(CultureInfo.InvariantCulture) + " contacts",
                station: day.Day.ToString("d MMMM yyyy", CultureInfo.InvariantCulture),
                detail: "Counted in UTC. A day when the band is open and you happen "
                    + "to be at the radio is worth several ordinary ones, which is "
                    + "why a good day is a record worth keeping rather than just a "
                    + "busy afternoon.",
                earned: true));
        }

        if (AchievementLog.BusiestHour(among) is { } hour)
        {
            cards.Add(new AchievementCard(
                key: prefix + "-busiest-hour",
                kind: AchievementKind.Record,
                title: "Busiest hour " + scope,
                figure: hour.Hour.ToString("00", CultureInfo.InvariantCulture)
                    + ":00 UTC",
                station: hour.Count == 1
                    ? "1 contact"
                    : hour.Count.ToString(CultureInfo.InvariantCulture) + " contacts",
                detail: "The hour of the day you have worked most in, in UTC. This is "
                    + "worth watching: the bands are not the same at every hour, and "
                    + "an hour that keeps coming up is usually one where the path you "
                    + "are working happens to be open.",
                earned: true));
        }

        return cards;
    }

    /// <summary>The bearing clause for a distance hover, or "".</summary>
    /// <remarks>
    /// **NO BEARING ON THE FACE** (Tim's ruling, 2026-09-08: *what am I, some sort of
    /// submarine captain?*). It lives here, on the hover, with everything else
    /// technical, and it is not deleted.
    /// </remarks>
    private static string Bearing(AchievementContact contact)
    {
        var here = OperatorLocation.FromGrid(contact.MyGrid);
        var there = OperatorLocation.FromGrid(contact.Grid);

        if (here is not { } from || there is not { } to)
        {
            return "";
        }

        return "He was on an initial bearing of "
            + GridPath.DescribeBearing(GridPath.BearingDegrees(from, to))
            + " from you. ";
    }

    /// <summary>One continent group per continent he has worked.</summary>
    /// <remarks>
    /// <para>**THE COUNT SAYS *WORKED* AND NEVER *CONFIRMED*** (§4). DXCC is counted
    /// by confirmations and Hamlet has none of them; it has contacts.</para>
    /// <para>**THE DENOMINATOR IS WHAT HAMLET CAN RECOGNISE**, which is the entities
    /// the cited continent table carries rather than the publication's own total.
    /// That is a smaller claim than the truth and it is the honest direction to be
    /// wrong in: every entity counted is one a callsign here can resolve to.</para>
    /// </remarks>
    private IEnumerable<AchievementGroup> BuildPlaces()
    {
        foreach (var code in _log.Continents)
        {
            var name = DxccContinents.NameOf(code);
            var worked = _log.EntitiesOn(code);
            var total = DxccContinents.EntitiesOn(code);

            var cards = _log.OnContinent(code)
                .Where(c => c.Entity is not null)
                .GroupBy(c => c.Entity!, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => Entity(code, name, g.Key, g.ToList()))
                .ToList();

            yield return new AchievementGroup(
                "continent-" + code,
                name,
                $"{worked} of {total} worked",
                "",
                "",
                cards);
        }
    }

    /// <summary>One card for a country he has worked.</summary>
    private static AchievementCard Entity(
        string code, string continent, string entity,
        IReadOnlyList<AchievementContact> among)
    {
        var first = AchievementLog.First(among) ?? among[0];

        var when = first.StartedUtc is { } at
            ? at.ToString("d MMMM yyyy", CultureInfo.InvariantCulture)
            : "a date the record does not carry";

        var many = among.Count == 1
            ? "1 contact"
            : among.Count.ToString(CultureInfo.InvariantCulture) + " contacts";

        return new AchievementCard(
            key: "entity-" + code + "-" + entity,
            kind: AchievementKind.Record,
            title: EntitySpoken.Of(entity),
            figure: many,
            station: AchievementCard.StationLine(first),
            detail: $"You first worked {EntitySpoken.Of(entity)} on {when}, and it "
                + $"counts toward {continent}. The count says worked rather than "
                + "confirmed: the DXCC award is counted from confirmations, on paper "
                + "or electronic, and Hamlet only knows what passed on the air from "
                + "your own log.",
            earned: true);
    }
}

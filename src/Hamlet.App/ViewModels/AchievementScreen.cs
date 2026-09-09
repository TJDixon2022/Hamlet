using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

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
/// <para>**THE SHAPE, IN ONE PARAGRAPH.** A band, a mode or a continent has no group
/// until he has made a contact in it (§3.1). Opening one brings its group and the
/// records inside it that his own log can already fill (§3.2), so one contact yields
/// several cards that are all *filled* rather than several blanks. **The
/// possibilities come from the challenges**, which stand whether or not they are
/// earned (§3.4), and from a group's own nudge, which is shown only inside a group
/// he has already opened.</para>
/// <para>**IT READS AND IT NEVER WRITES.** There is no path from this type to the
/// contact log, to a send path or to the radio. A log record is a statement the
/// operator made and a screen about it may not revise one.</para>
/// <para>**EVERY FIGURE IS THE LOG'S** (§5 question 1). Where a fact is absent the
/// card is absent with it: no grid means no distance card, no report means no signal
/// card, no time means no busiest day.</para>
/// </remarks>
public sealed class AchievementScreen
{
    /// <summary>
    /// How many entities of a continent to name in its nudge.
    /// </summary>
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
    /// than built here** because they are the one kind of card that does not come
    /// out of the unlock rule: this type decides what he has opened, and what to
    /// aim him at next is a separate judgement with its own file.
    /// </param>
    /// <exception cref="ArgumentNullException">There is no log.</exception>
    public AchievementScreen(
        AchievementLog log, IReadOnlyList<AchievementCard>? challenges = null)
    {
        ArgumentNullException.ThrowIfNull(log);

        _log = log;

        Groups = Build().ToList();
        Challenges = challenges ?? Array.Empty<AchievementCard>();
    }

    /// <summary>The groups he has opened, in the order they are drawn.</summary>
    public IReadOnlyList<AchievementGroup> Groups { get; }

    /// <summary>The standing targets, which are always here.</summary>
    /// <remarks>
    /// **§3.4: THESE ARE THE *GO AND TRY THIS* HALF AND HIDING THEM DEFEATS THEM.**
    /// They are the only cards on the screen that may appear unearned.
    /// </remarks>
    public IReadOnlyList<AchievementCard> Challenges { get; }

    /// <summary>True where anything at all has been opened.</summary>
    public bool HasGroups => Groups.Count > 0;

    /// <summary>Every group key that is open, for the reveal to compare against.</summary>
    public IReadOnlyList<string> OpenKeys => Groups.Select(g => g.Key).ToList();

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

            var cards = Groups.Sum(g => g.Count);

            var groups = Groups.Count == 1 ? "1 group" : Groups.Count + " groups";
            var kept = cards == 1 ? "1 record" : cards + " records";

            return $"{groups} open, {kept} in them.";
        }
    }

    /// <summary>Build every group he has opened.</summary>
    private IEnumerable<AchievementGroup> Build()
    {
        if (_log.Count == 0)
        {
            yield break;
        }

        // **THE WHOLE-LOG GROUP OPENS ON THE FIRST CONTACT**, because every card in
        // it is about the log entire and one contact is a log.
        var overall = Records("all", "so far", _log.Contacts);

        if (overall.Count > 0)
        {
            yield return new AchievementGroup(
                "records", "Your records", Kept(overall.Count), "", "", overall);
        }

        // **A BAND HAS NO GROUP UNTIL HE HAS WORKED IT** (§3.1). Not dimmed, not
        // dashed - absent. There is no list of bands here to filter; the bands come
        // out of his own log.
        foreach (var band in _log.Bands)
        {
            // **THE HEADING IS WHAT A PERSON READS AND THE KEY IS WHAT THE RECORD
            // SAYS.** ADIF spells a band `20m` and every other screen in this
            // application says `20 m`; a heading taken straight off the record read
            // `20m` here and nowhere else, which is the application disagreeing with
            // itself about a word.
            var shown = AdifLog.BandDisplayNameFor(band);

            var cards = Records("band-" + band, "on " + shown, _log.OnBand(band));

            if (cards.Count > 0)
            {
                yield return new AchievementGroup(
                    "band-" + band, shown, Kept(cards.Count), "", "", cards);
            }
        }

        foreach (var mode in _log.Modes)
        {
            var cards = Records("mode-" + mode, "on " + mode, _log.InMode(mode));

            if (cards.Count > 0)
            {
                yield return new AchievementGroup(
                    "mode-" + mode, mode, Kept(cards.Count), "", "", cards);
            }
        }

        foreach (var group in Continents())
        {
            yield return group;
        }
    }

    /// <summary>One continent group per continent he has worked.</summary>
    /// <remarks>
    /// <para>**THE COUNT SAYS *WORKED* AND NEVER *CONFIRMED*** (§4). DXCC is
    /// counted by confirmations and Hamlet has none of them; it has contacts.</para>
    /// <para>**THE DENOMINATOR IS WHAT HAMLET CAN RECOGNISE**, which is the 303
    /// entities the cited continent table carries rather than the publication's 340.
    /// That is a smaller claim than the truth and it is the honest direction to be
    /// wrong in: every entity counted is one a callsign here can resolve to.</para>
    /// </remarks>
    private IEnumerable<AchievementGroup> Continents()
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
            ? "one contact"
            : among.Count.ToString(CultureInfo.InvariantCulture) + " contacts";

        return new AchievementCard(
            key: "entity-" + code + "-" + entity,
            kind: AchievementKind.Record,
            title: EntitySpoken.Of(entity),
            figure: many,
            station: AchievementCard.StationLine(first),
            detail: $"You first worked {EntitySpoken.Of(entity)} on {when}, and it "
                + $"counts toward {continent}. **The count says worked and not "
                + "confirmed**: DXCC awards are counted from cards and confirmations, "
                + "and Hamlet only knows what passed on the air from your own log.",
            earned: true);
    }

    /// <summary>The record cards one set of contacts can fill, and no others.</summary>
    /// <param name="prefix">A stable key prefix for this scope.</param>
    /// <param name="scope">How the scope is said: `on 40 m`, `so far`.</param>
    /// <param name="among">The contacts in scope.</param>
    /// <returns>Only the cards this scope can actually fill.</returns>
    /// <remarks>
    /// **A RECORD HE CANNOT YET HOLD DOES NOT APPEAR** (the instruction, §3.1).
    /// *Furthest on 80 m* does not exist until he has worked 80 m, and it does not
    /// exist on 80 m either until some 80 m contact carried a grid square.
    /// </remarks>
    private static List<AchievementCard> Records(
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
                detail: "The great-circle distance from your own grid square to his, "
                    + "which is the way a radio signal actually travels rather than "
                    + "the way a map is drawn. A four-character grid square is a box "
                    + "about seventy miles across, so this is good to about that and "
                    + "no better.",
                earned: true));
        }

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
                    + "down to about -21, so a report near that is your signal at the "
                    + "edge of what any receiver can do with it.",
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
                    + "This one is about your receiving: your antenna, how quiet your "
                    + "location is, and how much of the band you were listening "
                    + "across. It is the half of a contact you have most control "
                    + "over.",
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
                detail: "Counted in UTC, which is the day your log is kept in and "
                    + "which is why a late evening contact can land on tomorrow's "
                    + "date. Every operator's log runs on UTC for exactly that "
                    + "reason: it is the one clock everybody on the band shares.",
                earned: true));
        }

        return cards;
    }

    /// <summary>How a group says what is in it, counting only what is there.</summary>
    private static string Kept(int cards)
        => cards == 1 ? "1 record" : cards + " records";
}

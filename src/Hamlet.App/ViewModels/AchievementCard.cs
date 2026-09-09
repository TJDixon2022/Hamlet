using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>Which of the two kinds a card is.</summary>
/// <remarks>
/// **THEY DO DIFFERENT JOBS AND THE DIFFERENCE IS THE WHOLE OF §3.4.** A record is
/// *look what you have collected* and would be discouraging as a wall of blanks, so
/// it exists only once he has opened it. A challenge is *go and try this* and its
/// whole purpose is to be a target, so hiding it defeats it.
/// </remarks>
public enum AchievementKind
{
    /// <summary>Unlocks on first contact in its scope. Never shown unearned.</summary>
    Record,

    /// <summary>Always visible, earned or not. A target.</summary>
    Challenge,
}

/// <summary>
/// **One card on the achievements screen.**
/// </summary>
/// <remarks>
/// <para>**EVERY CARD IS COMPUTED FROM THE LOG ALONE** -
/// `ACHIEVEMENTS_PHILOSOPHY.md` §5 question 1 - and a card whose figure is not in
/// the log does not exist rather than showing a blank.</para>
/// <para>**NOTHING SHAMES** (§4). A record card says what he collected. A challenge
/// card says how close he is and reads as an invitation; there is no wording here
/// for a thing he failed to do, no streak he broke and no elapsed silence.</para>
/// <para>**THE `i` HOVER IS THE PRODUCT** (§3.5). *That sentence is the actual
/// product. The card is the reason he reads it.* A card whose hover teaches nothing
/// fails §5 question 4 and does not belong on the screen.</para>
/// </remarks>
public sealed class AchievementCard
{
    /// <summary>Builds a card.</summary>
    /// <param name="key">A stable id, for the reveal to remember it by.</param>
    /// <param name="kind">Record or challenge.</param>
    /// <param name="title">What it is, in words.</param>
    /// <param name="figure">The number or the answer. Empty where not yet held.</param>
    /// <param name="station">Who and where, or empty.</param>
    /// <param name="detail">The `i` hover, which teaches.</param>
    /// <param name="earned">Whether he has it.</param>
    /// <param name="progress">How close he is, for a challenge. Empty otherwise.</param>
    public AchievementCard(
        string key,
        AchievementKind kind,
        string title,
        string figure,
        string station,
        string detail,
        bool earned,
        string progress = "")
    {
        Key = key;
        Kind = kind;
        Title = title;
        Figure = figure;
        Station = station;
        Detail = detail;
        Earned = earned;
        Progress = progress;
    }

    /// <summary>A stable id. Never shown.</summary>
    public string Key { get; }

    /// <summary>Record or challenge.</summary>
    public AchievementKind Kind { get; }

    /// <summary>What it is.</summary>
    public string Title { get; }

    /// <summary>The figure, or "" where he does not hold it yet.</summary>
    public string Figure { get; }

    /// <summary>True where there is a figure to draw.</summary>
    public bool HasFigure => Figure.Length > 0;

    /// <summary>
    /// The station and the place: `EI4GNB · Ireland · 3,180 miles`.
    /// </summary>
    /// <remarks>
    /// **NO BEARING** (Tim's ruling, 2026-09-08: *what am I, some sort of submarine
    /// captain?*). It is on the hover with everything else technical.
    /// </remarks>
    public string Station { get; }

    /// <summary>True where a station is named.</summary>
    public bool HasStation => Station.Length > 0;

    /// <summary>The `i` hover.</summary>
    public string Detail { get; }

    /// <summary>True where the hover has something to say.</summary>
    public bool HasDetail => Detail.Length > 0;

    /// <summary>Whether he holds it.</summary>
    public bool Earned { get; }

    /// <summary>How close he is, for a challenge that is not yet earned.</summary>
    /// <remarks>
    /// **FROM HIS OWN LOG AND NEVER FROM A TARGET HE HAS NOT MET** (§3.4, and the
    /// instruction's *first past 5,000 miles · best so far 4,410*). Where he has no
    /// contact that could measure it at all, the line says that rather than showing
    /// a zero, because a zero is a score and this is not one.
    /// </remarks>
    public string Progress { get; }

    /// <summary>True where a progress line is drawn.</summary>
    public bool HasProgress => Progress.Length > 0;

    /// <summary>How a challenge he has not earned is drawn back.</summary>
    /// <remarks>
    /// **A TARGET IS NOT A FAILURE AND IS NOT DRAWN AS ONE** (§4). It is drawn a
    /// little quieter than a card he holds, and the word *earned* is not used
    /// anywhere near it. **Opacity is a second carrier and never the only one**
    /// (§0.6): an earned card carries its figure and an unearned one carries its
    /// progress line, which reads the same in grayscale.
    /// </remarks>
    public double CardOpacity => Earned ? 1.0 : 0.78;

    /// <summary>The station line for one contact, or "" where it names nothing.</summary>
    /// <param name="contact">The contact behind a record.</param>
    /// <returns>`EI4GNB · Ireland · 3,180 miles`, with absent parts left out.</returns>
    /// <remarks>
    /// **EACH PART IS ABSENT WHERE ITS FACT IS** (§0.0). No entity and the country
    /// is left out; no grid and the distance is. A station with neither is still a
    /// callsign, which is the one thing every record has.
    /// </remarks>
    public static string StationLine(AchievementContact? contact)
    {
        if (contact is null)
        {
            return "";
        }

        var parts = new List<string> { contact.Callsign };

        if (contact.Entity is { Length: > 0 } entity)
        {
            parts.Add(EntitySpoken.Of(entity));
        }

        if (contact.Miles is { } miles)
        {
            parts.Add(GridPath.DescribeMiles(miles));
        }

        return string.Join(" · ", parts);
    }

    /// <summary>A report with its sign, the way the air carries it.</summary>
    public static string Signed(int decibels)
        => decibels.ToString("+0;-0;0", CultureInfo.InvariantCulture);
}

/// <summary>
/// **One group of cards, which does not exist until something in it is earned.**
/// </summary>
/// <remarks>
/// <para>**§3.1: NOTHING IS VISIBLE UNTIL SOMETHING ADJACENT HAS BEEN EARNED.** Not
/// dimmed, not dashed - absent. No empty 80 m tab, no ghost FT4 records, no Africa
/// card on day one. **A new group appearing is itself the reward** (§3.1), which is
/// why <see cref="AchievementScreen"/> announces it.</para>
/// <para>**THE SUMMARY COUNTS WHAT HE HAS OPENED AND NEVER WHAT HE HAS NOT** (the
/// instruction, and §2). Unit 287's `1 of 6` advertised five slots he could not see;
/// there is nothing on this type that could say such a thing.</para>
/// <para>**WHERE A GROUPING IS HAMLET'S OWN, <see cref="Note"/> SAYS SO** (§4). DXCC
/// defines continents; anything finer is this application's idea and the screen may
/// not imply somebody else recognises it.</para>
/// </remarks>
/// <param name="Key">A stable id, for the reveal to remember it by.</param>
/// <param name="Title">What the group is.</param>
/// <param name="Summary">What he has opened in it. Never a count of what he has not.</param>
/// <param name="Note">Where the grouping is Hamlet's own, saying so. Empty otherwise.</param>
/// <param name="Nudge">
/// Inside an opened group only, what to look for next. Empty otherwise, and never
/// shown for a group he has not opened.
/// </param>
/// <param name="Cards">The cards, every one of them earned or a standing challenge.</param>
public sealed record AchievementGroup(
    string Key,
    string Title,
    string Summary,
    string Note,
    string Nudge,
    IReadOnlyList<AchievementCard> Cards)
{
    /// <summary>True where the group says its grouping is Hamlet's own.</summary>
    public bool HasNote => Note.Length > 0;

    /// <summary>True where there is something to look for next.</summary>
    public bool HasNudge => Nudge.Length > 0;

    /// <summary>How many cards it holds.</summary>
    public int Count => Cards.Count;
}

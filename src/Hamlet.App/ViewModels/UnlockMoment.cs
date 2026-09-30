using System.Globalization;
using Hamlet.RadioEngine.Contacts;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **The moment something unlocks** (work instruction 506 task 5, `assets/achievements-look/unlock-moment.html`):
/// what one logged contact earned that the log did not hold before, the points it added, and where the rank
/// stands now.
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-30**: *"We need unlocking."* The window shows what is unlocked when he goes to look;
/// this says so at the moment it happens.</para>
/// <para>**WHAT IS EARNED IS WHAT THE ACHIEVEMENTS WINDOW SAYS IS EARNED**, before the write and after it:
/// the same pages, the same cards, compared by kind and name. Nothing here decides what counts.</para>
/// <para>**A POINTS FILE THAT CANNOT BE READ: NO PANEL**, because the points and the rank are the moment,
/// and a moment without them is a claim nobody can check (§0.0). **It writes nothing** to the log, the radio
/// or the settings.</para>
/// </remarks>
public sealed class UnlockMoment
{
    private UnlockMoment(
        string kind,
        AchievementCategoryCard card,
        IReadOnlyList<AchievementCategoryCard> others,
        AchievementScores before,
        AchievementScores after)
    {
        Kind = kind;
        Card = card;

        var miles = card.Miles is { } m ? Math.Round(m).ToString("#,0", CultureInfo.InvariantCulture) + " miles" : "";
        var where = card.Place.Length > 0 ? card.Callsign + " in " + card.Place : card.Callsign;
        var bandMode = card.BandModeLine.Replace(" · ", " ", StringComparison.Ordinal);

        Sentence = string.Join(", ", new[] { where, miles, bandMode }.Where(p => p.Length > 0)) + ".";
        AlsoLine = others.Count == 0 ? "" : "Also: " + string.Join(", ", others.Select(o => o.Title)) + ".";

        var added = (after.Total ?? 0) - (before.Total ?? 0);

        PointsAdded = "+" + AchievementStanding.Points(added);
        RankCrossed = after.Rank > before.Rank ? "You reached " + after.RankName + "." : "";

        // **THE RANK BAR, WITH THE PART JUST GAINED IN THE LIGHTER GREEN**: from where the rank he now holds
        // began to where the next begins; the part he had before the contact in the darker green, and the
        // part it added in the lighter. A rank crossed starts the darker part at nought.
        var start = after.RankStartsAt ?? 0;
        var total = after.Total ?? 0;

        if (after.NextRankAt is { } next && next > start)
        {
            var span = (double)(next - start);

            AfterFraction = Math.Clamp((total - start) / span, 0, 1);
            BeforeFraction = RankCrossed.Length > 0 ? 0 : Math.Clamp(((before.Total ?? 0) - start) / span, 0, 1);
            BarLeft = after.RankName;
            BarRight = after.NextRankName + " at " + next.ToString("#,0", CultureInfo.InvariantCulture);
            BarUnder = AchievementStanding.Points(total) + ". " + (next - total).ToString("#,0", CultureInfo.InvariantCulture) + " to go.";
        }
        else
        {
            AfterFraction = 1;
            BeforeFraction = RankCrossed.Length > 0 ? 0 : 1;
            BarLeft = after.RankName;
            BarRight = "the top rank";
            BarUnder = AchievementStanding.Points(total) + ".";
        }
    }

    /// <summary>
    /// **What one logged contact unlocked**, or null where it earned nothing new or the points file could not
    /// be read.
    /// </summary>
    /// <param name="before">The log's records before the write.</param>
    /// <param name="after">The log's records after it.</param>
    /// <param name="operatorGrid">His locator, the one every mile is measured from.</param>
    /// <param name="points">His points file.</param>
    /// <returns>The moment, or null.</returns>
    public static UnlockMoment? Between(
        IReadOnlyList<AdifLogRecord> before,
        IReadOnlyList<AdifLogRecord> after,
        string? operatorGrid,
        AchievementPoints points)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);
        ArgumentNullException.ThrowIfNull(points);

        if (!points.Loaded)
        {
            return null;
        }

        var was = new AchievementBadgePage(new AchievementLog(before, operatorGrid), points) { OperatorGrid = operatorGrid ?? "" };
        var now = new AchievementBadgePage(new AchievementLog(after, operatorGrid), points) { OperatorGrid = operatorGrid ?? "" };
        var held = AchievementsViewModel.EarnedIn(was).Select(e => (e.Kind, e.Card.Title)).ToHashSet();
        var fresh = AchievementsViewModel.EarnedIn(now).Where(e => !held.Contains((e.Kind, e.Card.Title))).ToList();

        if (fresh.Count == 0)
        {
            return null;
        }

        // **ONE PANEL, THE LARGEST BY POINTS NAMED**, and the rest in a line under it; on a tie, the first in
        // the page's own order.
        var largest = fresh.Select((e, i) => (e, i)).OrderByDescending(x => Worth(x.e.Card)).ThenBy(x => x.i).First().e;

        return new UnlockMoment(
            largest.Kind, largest.Card, fresh.Where(e => e != largest).Select(e => e.Card).ToList(), was.Scores, now.Scores);
    }

    /// <summary>What a card is worth, from its `5 pts`; nought where it says none.</summary>
    private static int Worth(AchievementCategoryCard card)
        => int.TryParse(card.PointsLine.Replace(" pts", "", StringComparison.Ordinal).Replace(",", "", StringComparison.Ordinal), NumberStyles.None, CultureInfo.InvariantCulture, out var worth) ? worth : 0;

    /// <summary>The kind the named card belongs to.</summary>
    public string Kind { get; }

    /// <summary>The card named: the largest thing earned by points.</summary>
    public AchievementCategoryCard Card { get; }

    /// <summary>`Unlocked`.</summary>
    public string Heading => "Unlocked";

    /// <summary>What was unlocked: `South America`.</summary>
    public string Name => Card.Title;

    /// <summary>The station, place, distance, band and mode in one sentence: `CE3ZZZ in Chile, 5,112 miles, 20 m FT8.`</summary>
    public string Sentence { get; }

    /// <summary>The other things the same contact earned, `Also: FN20, 20 m.`, or "".</summary>
    public string AlsoLine { get; }

    /// <summary>True where the contact earned more than one thing.</summary>
    public bool HasAlso => AlsoLine.Length > 0;

    /// <summary>`+60 points`.</summary>
    public string PointsAdded { get; }

    /// <summary>`You reached Rank 4.` where the contact crossed a rank, or "".</summary>
    public string RankCrossed { get; }

    /// <summary>True where a rank was crossed.</summary>
    public bool HasRankCrossed => RankCrossed.Length > 0;

    /// <summary>How far along the rank bar the total stood before the contact, nought to one.</summary>
    public double BeforeFraction { get; }

    /// <summary>How far along it stands now, nought to one.</summary>
    public double AfterFraction { get; }

    /// <summary>The rank he holds, at the bar's left.</summary>
    public string BarLeft { get; }

    /// <summary>`Rank 5 at 500`, or `the top rank`, at the bar's right.</summary>
    public string BarRight { get; }

    /// <summary>`472 points. 28 to go.`</summary>
    public string BarUnder { get; }

    /// <summary>`See where Chile is`: handed in by the main window's view model when the panel is shown.</summary>
    public System.Windows.Input.ICommand? SeeWhere { get; set; }

    /// <summary>`Keep going`: handed in the same way.</summary>
    public System.Windows.Input.ICommand? KeepGoing { get; set; }

    /// <summary>True where the path can be shown: the card has a map.</summary>
    public bool CanShowWhere => Card.HasMap;

    /// <summary>`See where Chile is`, or `See where it is` where no place is known.</summary>
    public string SeeWhereLabel => "See where " + (Card.Place.Length > 0 ? Card.Place : "it") + " is";
}

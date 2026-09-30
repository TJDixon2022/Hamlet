using System.Globalization;
using Hamlet.RadioEngine.Contacts;

namespace Hamlet.App.ViewModels;

/// <summary>
/// **Your standing: the ring, the gap to the next rank, three facts, and what was just unlocked**
/// (work instruction 506, the left third of `assets/achievements-look/opening-page.html`).
/// </summary>
/// <remarks>
/// <para>**TIM, 2026-09-30**: *"We need to show overall progress. We need unlocking."* The ring is the
/// overall progress, the gap is the next unlock, and the button under them is the last thing unlocked.</para>
/// <para>**NO NUMBER HERE IS WRITTEN IN CODE.** The rank, its name, where it began and where the next
/// begins are all read off <see cref="AchievementScores"/>, which reads the owner's file; where the file
/// could not be read the ring is not drawn and no number is shown, and the panel says why.</para>
/// <para>**WORKED, NEVER CONFIRMED** (§4), and **no count of what he has not done** (§3.7).</para>
/// </remarks>
public sealed class AchievementStanding
{
    /// <summary>Build the panel from the page and the earned cards of every kind.</summary>
    /// <param name="page">The achievements page.</param>
    /// <param name="earned">Every earned card of every kind, with the kind it belongs to.</param>
    /// <param name="firstContactPoints">What the first contact is worth, in the file's words (`10 pts`), or "".</param>
    public AchievementStanding(
        AchievementBadgePage page,
        IReadOnlyList<(string Kind, AchievementCategoryCard Card)> earned,
        string firstContactPoints)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(earned);

        var scores = page.Scores;

        Problem = page.Problem;
        HasScores = scores.Total is not null;

        if (scores.Total is { } total)
        {
            var named = scores.RankName != "Rank " + scores.Rank.ToString(CultureInfo.InvariantCulture);

            RankLabel = named ? "" : "Rank";
            RankBig = named ? scores.RankName : scores.Rank.ToString(CultureInfo.InvariantCulture);
            PointsLine = Points(total);

            if (scores.NextRankAt is { } next && scores.RankStartsAt is { } start && next > start)
            {
                Fraction = Math.Clamp((double)(total - start) / (next - start), 0, 1);
                GapLine = Points(next - total) + " to " + scores.NextRankName + ".";
            }
            else
            {
                // **AT THE TOP RANK THE RING IS FULL AND SAYS SO.**
                Fraction = 1;
                GapLine = scores.RankName + ", the top rank.";
            }
        }

        Contacts = page.Log.Contacts.Count.ToString("#,0", CultureInfo.InvariantCulture);
        Countries = scores.For(AchievementKinds.Countries).Worked.ToString("#,0", CultureInfo.InvariantCulture);

        var farthest = page.Log.Contacts.Where(c => c.Miles is not null).Select(c => c.Miles!.Value).DefaultIfEmpty(double.NaN).Max();

        Farthest = double.IsNaN(farthest) ? "" : Math.Round(farthest).ToString("#,0", CultureInfo.InvariantCulture);

        // **THE MOST RECENT THING HE EARNED, BY THE DATE OF THE CONTACT THAT EARNED IT.** A card with
        // no date comes after every dated one; on a tie the first kind in the page's order wins, so the
        // choice does not move from one opening to the next.
        var latest = earned
            .Where(e => e.Card.Earned)
            .OrderByDescending(e => e.Card.EarnedUtc ?? DateTime.MinValue)
            .FirstOrDefault();

        if (latest.Card is { } card)
        {
            JustUnlocked = card;
            JustUnlockedLine = string.Join(" · ", new[] { card.Callsign, card.DistanceLine }.Where(p => p.Length > 0));
            JustUnlockedPoints = PointsAdded(card.PointsLine) + (card.HasMap ? " · see the map" : "");
        }
        else
        {
            // **TWO LINES, EACH WHOLE** (§6): the sentence is wider than the panel on the test host's measure,
            // so it is said in two runs rather than wrapped.
            NothingYetLine = "Your first contact earns";
            NothingYetWorth = firstContactPoints.Length > 0
                ? PointsAdded(firstContactPoints).TrimStart('+') + "."
                : "a place in the Hall of Fame.";
        }
    }

    /// <summary>What went wrong with the points file, or "".</summary>
    public string Problem { get; }

    /// <summary>True where the file was read, so the ring and the numbers are drawn.</summary>
    public bool HasScores { get; }

    /// <summary>True where the panel says the file could not be read.</summary>
    public bool HasProblem => !HasScores && Problem.Length > 0;

    /// <summary>`Rank` over the number where the file names no rank, or "" where the name is drawn.</summary>
    public string RankLabel { get; } = "";

    /// <summary>True where the small `Rank` word is drawn.</summary>
    public bool HasRankLabel => RankLabel.Length > 0;

    /// <summary>The rank's name from his file, or its number: the large figure in the ring.</summary>
    public string RankBig { get; } = "";

    /// <summary>`412 points`.</summary>
    public string PointsLine { get; } = "";

    /// <summary>
    /// How far from where his rank began to where the next begins, nought to one; one at the top rank.
    /// </summary>
    public double Fraction { get; }

    /// <summary>`88 points to Rank 4.`, or `Rank 8, the top rank.`</summary>
    public string GapLine { get; } = "";

    /// <summary>How many contacts the log holds.</summary>
    public string Contacts { get; }

    /// <summary>How many countries he has worked.</summary>
    public string Countries { get; }

    /// <summary>The farthest contact in whole miles, or "" where none has a distance.</summary>
    public string Farthest { get; }

    /// <summary>True where there is a farthest to draw.</summary>
    public bool HasFarthest => Farthest.Length > 0;

    /// <summary>The most recent card he earned, or null where he has earned nothing.</summary>
    public AchievementCategoryCard? JustUnlocked { get; }

    /// <summary>True where there is a most recent card.</summary>
    public bool HasJustUnlocked => JustUnlocked is not null;

    /// <summary>
    /// The seal's ink on the dark panel: the kind's own color carried seven tenths of the way to white, as
    /// the picture's pale red is for Countries, so it reads on `#14202B`; "" where nothing is unlocked.
    /// </summary>
    public string JustUnlockedSealInk => JustUnlocked is { SealColor.Length: 7 } card ? Tint(card.SealColor, 0.7) : "#FFFFFF";

    /// <summary>A `#RRGGBB` color carried a share of the way to white.</summary>
    internal static string Tint(string color, double share)
    {
        int Channel(int at)
        {
            var value = int.Parse(color.Substring(at, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            return (int)Math.Round(value + ((255 - value) * share));
        }

        return "#" + Channel(1).ToString("X2", CultureInfo.InvariantCulture) + Channel(3).ToString("X2", CultureInfo.InvariantCulture)
            + Channel(5).ToString("X2", CultureInfo.InvariantCulture);
    }

    /// <summary>`LA1ZZZ · 3,742 mi`.</summary>
    public string JustUnlockedLine { get; } = "";

    /// <summary>`+5 points · see the map`.</summary>
    public string JustUnlockedPoints { get; } = "";

    /// <summary>What the first contact earns, where nothing is earned yet; otherwise "".</summary>
    public string NothingYetLine { get; } = "";

    /// <summary>What the first contact is worth, the second line: `10 points.`</summary>
    public string NothingYetWorth { get; } = "";

    /// <summary>True where nothing is earned yet.</summary>
    public bool HasNothingYet => NothingYetLine.Length > 0;

    /// <summary>`1 point`, `412 points`.</summary>
    internal static string Points(long count)
        => count.ToString("#,0", CultureInfo.InvariantCulture) + (count == 1 ? " point" : " points");

    /// <summary>True where the most recent card has a path, so the panel is the button that opens it.</summary>
    public bool JustUnlockedOpensMap => JustUnlocked is { HasMap: true };

    /// <summary>True where the most recent card has no path, so the panel is not a button.</summary>
    public bool JustUnlockedHasNoMap => JustUnlocked is { HasMap: false };

    /// <summary>`+5 points` for a card's `5 pts`; "" where the card carries no worth.</summary>
    internal static string PointsAdded(string pointsLine)
        => long.TryParse(pointsLine.Replace(" pts", "", StringComparison.Ordinal).Replace(",", "", StringComparison.Ordinal), NumberStyles.None, CultureInfo.InvariantCulture, out var worth)
            ? "+" + Points(worth)
            : "";
}

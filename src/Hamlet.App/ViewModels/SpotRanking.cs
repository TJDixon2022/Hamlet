using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>A spot with its score and the reason that score is what it is.</summary>
/// <param name="Spot">The spot itself.</param>
/// <param name="Score">Higher is a better next ten minutes for a newcomer.</param>
/// <param name="Reason">The line printed on the card, e.g.
/// "park activation · calling CQ · 15 WPM".</param>
public sealed record RankedSpot(ActivitySpot Spot, int Score, string Reason);

/// <summary>
/// Ranks the happening-now list by how good a next ten minutes each spot
/// would make for somebody who has never made a contact.
/// </summary>
/// <remarks>
/// <para>THE RULE THAT SHAPES THIS FILE (HM-DEC-025). A card that is ranked
/// highly without saying why is a guess presented as a decode. So the score
/// and the reason are produced by one pass over one set of facts: every
/// component that moves a spot up the list contributes a phrase, and the
/// phrases printed on the card are the components that actually moved it.
/// The reason cannot drift away from the ranking because it is not written
/// separately from it.</para>
/// <para>What is worth points, and why, for this operator:</para>
/// <list type="bullet">
/// <item><b>Park and summit activations</b> lead. That operator carried a
/// radio somewhere on purpose to be called, needs contacts for the
/// activation to count, and will slow down for an obvious beginner. It is
/// the friendliest contact on the band.</item>
/// <item><b>CQ</b> beats a contest run beats an unlabeled spot. A CQ is an
/// open invitation; a contest exchange is a closed loop at speed; a beacon
/// answers nobody and is pushed to the bottom.</item>
/// <item><b>Slow CW</b> beats fast. Under about 18 WPM is copyable by
/// somebody still counting dits; over 24 is a wall.</item>
/// <item><b>Close and strong</b> beats marginal — including how many
/// receivers heard it, which is the best evidence available that this
/// operator's receiver will too.</item>
/// <item><b>Fresh</b> beats old. A twenty-minute-old spot is a station that
/// has probably packed up.</item>
/// <item><b>Workable today</b> beats impressive. CW is what Hamlet decodes
/// and what phase 1 delivers; FT8 is pushed down because the app cannot read
/// it until phase 3, so recommending it amounts to recommending a
/// waterfall.</item>
/// </list>
/// <para>No clock is read here. Elapsed time is passed in, so every threshold
/// is testable exactly and the same spot set always ranks the same way (§5).
/// </para>
/// </remarks>
public static class SpotRanking
{
    /// <summary>At or under this speed, CW is comfortable for a beginner.</summary>
    public const int ComfortableWpm = 13;

    /// <summary>The speed the brief names as the edge of copyable.</summary>
    public const int CopyableWpm = 18;

    /// <summary>Above this, a newcomer is not going to copy the call.</summary>
    public const int FastWpm = 24;

    /// <summary>Signal in dB at or above which RBN reports read as strong.</summary>
    public const int StrongSignalDb = 20;

    /// <summary>How many phrases a card's reason may carry.</summary>
    public const int MaxReasonParts = 3;

    /// <summary>
    /// The beacon penalty, deliberately larger than every positive component
    /// added together, so a beacon can never outrank a station that might
    /// answer.
    /// </summary>
    public const int BeaconPenalty = 200;

    /// <summary>
    /// Rank a set of spots, best first.
    /// </summary>
    /// <param name="spots">The spots to rank.</param>
    /// <param name="nowUtc">Reference time; ages are measured against it.</param>
    /// <returns>Every spot, scored and explained, highest first.</returns>
    public static IReadOnlyList<RankedSpot> Rank(
        IEnumerable<ActivitySpot> spots, DateTime nowUtc)
        => spots
            .Select(s => Evaluate(s, nowUtc - s.HeardAtUtc))
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => r.Spot.HeardAtUtc)
            .ToList();

    /// <summary>
    /// Score one spot and write its reason.
    /// </summary>
    /// <param name="spot">The spot.</param>
    /// <param name="age">How long ago it was heard.</param>
    /// <returns>The spot with its score and reason.</returns>
    public static RankedSpot Evaluate(ActivitySpot spot, TimeSpan age)
    {
        var score = 0;
        var parts = new List<(int Weight, string Text)>();

        // Activations: the friendliest contact on the band.
        if (spot.IsActivation)
        {
            var kind = string.Equals(spot.Source, "SOTA", StringComparison.OrdinalIgnoreCase)
                ? "summit activation"
                : "park activation";
            var where = string.IsNullOrWhiteSpace(spot.PlaceLabel)
                ? kind
                : $"{kind} in {spot.PlaceLabel}";

            score += 30;
            parts.Add((30, where));
        }

        // What kind of call it is.
        switch (spot.CallType)
        {
            case SpotCallType.Cq:
                score += 25;
                parts.Add((25, "calling CQ"));
                break;
            case SpotCallType.Dx:
                score += 5;
                break;
            case SpotCallType.Contest:
                score -= 10;
                parts.Add((4, "a contest exchange, fast and formulaic"));
                break;
            case SpotCallType.Beacon:
                // Decisive, not merely heavy. A beacon is strong, close,
                // steady and permanently useless for making a contact, so it
                // scores well on every other axis and would otherwise float
                // above real stations. This penalty is larger than every
                // positive component combined, which puts beacons at the
                // bottom of the list without hiding them — they are still
                // real signals, and a band's beacons are worth seeing on the
                // map.
                score -= BeaconPenalty;
                parts.Add((3, "a beacon, so nobody is listening for you"));
                break;
            case SpotCallType.Unknown:
            default:
                parts.Add((1, "no call type reported"));
                break;
        }

        // Speed.
        if (spot.Wpm is { } wpm)
        {
            if (wpm <= ComfortableWpm)
            {
                score += 20;
                parts.Add((22, $"{wpm} WPM, slow enough to copy"));
            }
            else if (wpm <= CopyableWpm)
            {
                score += 14;
                parts.Add((16, $"{wpm} WPM"));
            }
            else if (wpm <= FastWpm)
            {
                score += 4;
                parts.Add((6, $"{wpm} WPM, which is quick"));
            }
            else
            {
                score -= 6;
                parts.Add((5, $"{wpm} WPM, which is very fast"));
            }
        }

        // How close the station that heard it is.
        switch (spot.Proximity)
        {
            case SpotProximity.Local:
                score += 18;
                parts.Add((18, "heard nearby"));
                break;
            case SpotProximity.Continent:
                score += 8;
                parts.Add((8, "heard on this continent"));
                break;
            case SpotProximity.Distant:
                // Heavier than it looks, and deliberately so. Live feeds are
                // full of European park activations that score beautifully on
                // every other axis; on 40 m from Pennsylvania in daylight they
                // are not a contact a beginner is going to make, and putting
                // them at the top would send this operator to listen to noise.
                score -= 25;
                parts.Add((6, "heard far away, so a hard first contact"));
                break;
            case SpotProximity.Unknown:
            default:
                break;
        }

        // Signal strength, where somebody measured it. The figure travels with
        // its meaning rather than as a bare word, so the operator can see what
        // "strong" was derived from and start to learn the scale (HM-DEC-042).
        if (spot.SignalDb is { } db)
        {
            if (db >= StrongSignalDb)
            {
                score += 12;
                parts.Add((14, SignalReport.Describe(db)));
            }
            else if (db >= SignalReport.FairDb)
            {
                score += 7;
                parts.Add((7, SignalReport.Describe(db)));
            }
            else if (db >= SignalReport.WeakDb)
            {
                score += 2;
            }
            else
            {
                score -= 2;
                parts.Add((2, SignalReport.Describe(db)));
            }
        }

        // How many receivers agree it is there.
        if (spot.ReportCount is { } reports && reports > 1)
        {
            if (reports >= 10)
            {
                score += 10;
                parts.Add((12, $"{reports} receivers hear it"));
            }
            else if (reports >= 5)
            {
                score += 6;
                parts.Add((9, $"{reports} receivers hear it"));
            }
            else
            {
                score += 3;
                parts.Add((3, $"{reports} receivers hear it"));
            }
        }

        // Whether the operator can actually do anything about it today.
        //
        // This is the one weighting that is about Hamlet rather than about the
        // air. FT8 is the busiest thing on most bands, and against live feeds
        // it swamped the top of this list — but Hamlet cannot decode it until
        // phase 3, and a beginner cannot work it by ear at all. Recommending
        // it as somebody's next ten minutes is recommending they watch a
        // waterfall. CW is what Hamlet is for and what phase 1 delivers, so it
        // is lifted; voice is workable with no help from the app and sits in
        // between. The card says which of these applied, so the preference is
        // stated rather than smuggled (HM-DEC-025).
        switch ((spot.Mode ?? "").ToUpperInvariant())
        {
            case "CW":
                score += 12;
                parts.Add((11, "Morse, which is what Hamlet is built to decode"));
                break;
            case "FT8":
            case "FT4":
                score -= 20;
                parts.Add((15, "FT8, which Hamlet cannot decode until phase 3"));
                break;
            default:
                break;
        }

        // Freshness.
        var minutes = Math.Max(0, age.TotalMinutes);
        if (minutes <= 2)
        {
            score += 15;
            parts.Add((13, "just now"));
        }
        else if (minutes <= 5)
        {
            score += 10;
            parts.Add((10, $"{(int)minutes} min ago"));
        }
        else if (minutes <= 10)
        {
            score += 5;
            parts.Add((5, $"{(int)minutes} min ago"));
        }
        else if (minutes <= 20)
        {
            parts.Add((4, $"{(int)minutes} min ago"));
        }
        else
        {
            score -= 10;
            parts.Add((7, $"{(int)minutes} min ago, so probably gone"));
        }

        return new RankedSpot(spot, score, BuildReason(parts));
    }

    /// <summary>
    /// Assemble the card's reason from the strongest phrases.
    /// </summary>
    /// <param name="parts">Weighted phrases collected while scoring.</param>
    /// <returns>The reason line; never empty.</returns>
    /// <remarks>
    /// Never returns an empty string. A card whose spot carries no notable
    /// fact still has to account for itself, so the weakest case still prints
    /// what little is known rather than showing a bare rank.
    /// </remarks>
    private static string BuildReason(List<(int Weight, string Text)> parts)
    {
        if (parts.Count == 0)
        {
            return "on the air, with nothing else reported";
        }

        var chosen = parts
            .OrderByDescending(p => p.Weight)
            .Take(MaxReasonParts)
            .Select(p => p.Text);

        return string.Join(" · ", chosen);
    }
}

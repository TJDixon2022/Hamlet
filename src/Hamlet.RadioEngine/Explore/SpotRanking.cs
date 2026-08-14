namespace Hamlet.RadioEngine.Explore;

/// <summary>A spot with its score and the reason that score is what it is.</summary>
/// <param name="Spot">The spot itself.</param>
/// <param name="Score">Higher is a better next ten minutes for a newcomer.</param>
/// <param name="Reason">The line printed on the card, e.g.
/// "park activation in US-PA · calling CQ · 15 WPM · still going".</param>
public sealed record RankedSpot(ActivitySpot Spot, int Score, string Reason);

/// <summary>
/// Ranks the happening-now list by what a newcomer can actually work
/// (HM-DEC-058).
/// </summary>
/// <remarks>
/// <para>WORKABILITY, NOT DISTANCE. Distance does not run in a straight line
/// with workability on HF. There is a skip zone: on 20 m a station two hundred
/// miles off is often unreachable because the signal leaves at too shallow an
/// angle and comes down beyond them, while somebody two thousand miles out is
/// easy, and on 40 m at night the close-in stations come back in loud. Sorting
/// nearest-first would put the hardest contacts at the top and call them the
/// best chance. So nothing here reads a distance, and the weights live in
/// <see cref="SpotRankWeights"/> where the absence is visible.</para>
/// <para>THE REASON IS PRODUCED BY THE SAME PASS AS THE SCORE (HM-DEC-025). A
/// card ranked highly without saying why is a guess presented as a decode, so
/// every component that moves a spot contributes a phrase and the phrases
/// printed are the components that actually moved it. The reason cannot drift
/// away from the ranking because it is not written separately from it.</para>
/// <para>LIVENESS COMES FROM THE LENS MACHINERY (HM-DEC-057), so the fade on a
/// card and its position in the list are two readings of one clock rather than
/// two clocks that can disagree.</para>
/// <para>No clock is read here. The moment is passed in, so every threshold is
/// testable exactly and the same spot set always ranks the same way (§5).</para>
/// </remarks>
public static class SpotRanking
{
    /// <summary>
    /// Rank a set of spots a lens has already chosen, best first.
    /// </summary>
    /// <param name="lensed">What the lens shows, with its liveness.</param>
    /// <param name="lifetimes">The configured source lifetimes.</param>
    /// <param name="copySpeedWpm">
    /// The speed the operator has stated, or null when nothing has stated one.
    /// </param>
    /// <returns>Every spot, scored and explained, highest first.</returns>
    public static IReadOnlyList<RankedSpot> Rank(
        IEnumerable<LensedSpot> lensed,
        SpotLifetimeSettings? lifetimes = null,
        int? copySpeedWpm = null)
    {
        ArgumentNullException.ThrowIfNull(lensed);

        return lensed
            .Select(l => Evaluate(l.Spot, l.Liveness, lifetimes, copySpeedWpm))
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => r.Spot.HeardAtUtc)
            .ToList();
    }

    /// <summary>
    /// Rank a set of spots against a moment, best first.
    /// </summary>
    /// <param name="spots">The spots to rank.</param>
    /// <param name="nowUtc">Reference time; liveness is measured against it.</param>
    /// <param name="lifetimes">The configured source lifetimes.</param>
    /// <param name="copySpeedWpm">
    /// The speed the operator has stated, or null when nothing has stated one.
    /// </param>
    /// <returns>Every spot, scored and explained, highest first.</returns>
    public static IReadOnlyList<RankedSpot> Rank(
        IEnumerable<ActivitySpot> spots,
        DateTime nowUtc,
        SpotLifetimeSettings? lifetimes = null,
        int? copySpeedWpm = null)
    {
        ArgumentNullException.ThrowIfNull(spots);

        return spots
            .Select(s => Evaluate(
                s, SpotLensView.Liveness(s, nowUtc - s.HeardAtUtc, lifetimes), lifetimes,
                copySpeedWpm))
            .OrderByDescending(r => r.Score)
            .ThenByDescending(r => r.Spot.HeardAtUtc)
            .ToList();
    }

    /// <summary>
    /// Score one spot and write its reason.
    /// </summary>
    /// <param name="spot">The spot.</param>
    /// <param name="liveness">How much of its lifetime is left, 1 down to 0.</param>
    /// <param name="lifetimes">The configured source lifetimes.</param>
    /// <param name="copySpeedWpm">
    /// The speed the operator has stated, or null when nothing has stated one.
    /// </param>
    /// <returns>The spot with its score and reason.</returns>
    public static RankedSpot Evaluate(
        ActivitySpot spot,
        double liveness,
        SpotLifetimeSettings? lifetimes = null,
        int? copySpeedWpm = null)
    {
        ArgumentNullException.ThrowIfNull(spot);

        var score = 0;
        var parts = new List<(int Weight, string Text)>();

        score += Soliciting(spot, parts);
        score += StillThere(spot, liveness, parts, lifetimes);
        score += Speed(spot, parts, copySpeedWpm);
        score += Audible(spot, parts);
        score += Workable(spot, parts);

        return new RankedSpot(spot, score, BuildReason(parts));
    }

    /// <summary>Whether this station is asking to be called.</summary>
    /// <remarks>
    /// An activator calling CQ is the friendliest target on the band for
    /// somebody's first contact, and this is the half of the rank that says so.
    /// </remarks>
    private static int Soliciting(ActivitySpot spot, List<(int, string)> parts)
    {
        var score = 0;

        if (spot.IsActivation)
        {
            var kind = string.Equals(spot.Source, "SOTA", StringComparison.OrdinalIgnoreCase)
                ? "summit activation"
                : "park activation";
            var where = string.IsNullOrWhiteSpace(spot.PlaceLabel)
                ? kind
                : $"{kind} in {spot.PlaceLabel}";

            score += SpotRankWeights.Activation;
            parts.Add((SpotRankWeights.Activation, where));
        }

        switch (spot.CallType)
        {
            case SpotCallType.Cq:
                score += SpotRankWeights.CallingCq;
                parts.Add((SpotRankWeights.CallingCq, "calling CQ"));
                break;

            case SpotCallType.Dx:
                score += SpotRankWeights.Dx;
                break;

            case SpotCallType.Contest:
                score += SpotRankWeights.ContestExchange;
                parts.Add((4, "a contest exchange, fast and formulaic"));
                break;

            case SpotCallType.Beacon:
                score += SpotRankWeights.Beacon;
                parts.Add((3, "a beacon, so nobody is listening for you"));
                break;

            case SpotCallType.Unknown:
            default:
                parts.Add((1, "no call type reported"));
                break;
        }

        return score;
    }

    /// <summary>
    /// Whether that person is probably still on that frequency.
    /// </summary>
    /// <remarks>
    /// The one clock, from the lens machinery (HM-DEC-057). The phrase is the
    /// source's own language about its own kind of evidence: an activation may
    /// say somebody is probably still working callers, because activators stay
    /// put, and the same sentence about a skimmer report would be an invention
    /// (HM-DEC-045).
    /// </remarks>
    private static int StillThere(
        ActivitySpot spot,
        double liveness,
        List<(int, string)> parts,
        SpotLifetimeSettings? lifetimes)
    {
        var clamped = Math.Clamp(liveness, 0.0, 1.0);

        // From a penalty to a bonus rather than from nothing to a bonus, so a
        // spot whose lifetime is spent is actively pushed down rather than
        // merely un-helped (HM-DEC-058).
        var span = SpotRankWeights.Liveness - SpotRankWeights.Expired;
        var score = (int)Math.Round(SpotRankWeights.Expired + (span * clamped));

        var lifetime = SpotLifetime.For(spot, lifetimes);

        // Rounded to the second, because reconstructing the elapsed time from a
        // fraction lands a hair under the whole number and the phrasing steps at
        // whole minutes. Two minutes came out as "just now" instead of "they are
        // probably still there", which is a different claim.
        var elapsed = TimeSpan.FromSeconds(
            Math.Round(lifetime.TotalSeconds * (1.0 - clamped)));

        parts.Add((
            Math.Max(4, score),
            SpotLifetime.DescribeOpportunity(spot, elapsed, lifetimes)));

        return score;
    }

    /// <summary>
    /// How fast the station is sending, where the source measured it.
    /// </summary>
    /// <remarks>
    /// <para>DESCRIBES THE STATION, AND COMPARES IT ONLY TO A NUMBER THE
    /// OPERATOR STATED (HM-DEC-058, HM-DEC-066). Hamlet has never measured what
    /// speed this person can copy, so it may not claim any speed suits them:
    /// that would be a confident match against a number nobody has ever taken,
    /// which is the guess §0.0 forbids. What it may do, once there is a setting,
    /// is say a station is sending faster than what the setting says, because
    /// that is a comparison between two stated numbers and no claim about
    /// anybody. Every phrase below is one or the other and never a verdict.</para>
    /// <para>With nothing stated the scale falls back to the pace Morse itself
    /// runs at, which is where it started (HM-OPEN-006).</para>
    /// </remarks>
    private static int Speed(ActivitySpot spot, List<(int, string)> parts, int? copySpeedWpm)
    {
        if (spot.Wpm is not { } wpm)
        {
            return 0;
        }

        if (copySpeedWpm is not { } stated)
        {
            if (wpm <= SpotRankWeights.RelaxedWpm)
            {
                parts.Add((14, $"{wpm} WPM, which is a relaxed pace"));
                return SpotRankWeights.RelaxedSpeed;
            }

            if (wpm <= SpotRankWeights.ModerateWpm)
            {
                parts.Add((10, $"{wpm} WPM"));
                return SpotRankWeights.ModerateSpeed;
            }

            if (wpm <= SpotRankWeights.QuickWpm)
            {
                parts.Add((6, $"{wpm} WPM, which is quick"));
                return SpotRankWeights.QuickSpeed;
            }

            parts.Add((5, $"{wpm} WPM, which is very fast"));
            return SpotRankWeights.VeryFastSpeed;
        }

        // The bands keep the shape they always had and slide to wherever the
        // operator put their number, so a setting left at the shipped default
        // ranks exactly as this always has (HM-DEC-066).
        if (wpm <= stated)
        {
            parts.Add((14, $"{wpm} WPM, at or under the {stated} in your settings"));
            return SpotRankWeights.RelaxedSpeed;
        }

        if (wpm <= stated + SpotRankWeights.ALittleOverWpm)
        {
            parts.Add((10, $"{wpm} WPM, a little over the {stated} in your settings"));
            return SpotRankWeights.ModerateSpeed;
        }

        if (wpm <= stated + SpotRankWeights.WellOverWpm)
        {
            parts.Add((6, $"{wpm} WPM, well over the {stated} in your settings"));
            return SpotRankWeights.QuickSpeed;
        }

        parts.Add((5, $"{wpm} WPM, far over the {stated} in your settings"));
        return SpotRankWeights.VeryFastSpeed;
    }

    /// <summary>
    /// The evidence that this operator's own receiver will hear it.
    /// </summary>
    /// <remarks>
    /// <para>ONLY WHERE THE SOURCE REPORTS A RECEIVER (HM-DEC-038). A skimmer
    /// report states where a machine that decoded the signal is standing, and a
    /// nearby one is real evidence about audibility. An activation's proximity
    /// is where the STATION is, which is a distance, and distance does not vote
    /// here (HM-DEC-058). It is on the card either way.</para>
    /// <para>Signal strength and how many receivers agree are measurements and
    /// vote wherever they are reported.</para>
    /// </remarks>
    private static int Audible(ActivitySpot spot, List<(int, string)> parts)
    {
        var score = 0;

        if (SpotLifetime.IsSkimmer(spot))
        {
            switch (spot.Proximity)
            {
                case SpotProximity.Local:
                    score += SpotRankWeights.ReceiverNearby;
                    parts.Add((
                        SpotRankWeights.ReceiverNearby,
                        "a receiver near you decoded it"));
                    break;

                case SpotProximity.Continent:
                    score += SpotRankWeights.ReceiverOnContinent;
                    parts.Add((
                        SpotRankWeights.ReceiverOnContinent,
                        "a receiver on this continent decoded it"));
                    break;

                case SpotProximity.Distant:
                case SpotProximity.Unknown:
                default:
                    break;
            }
        }

        if (spot.SignalDb is { } db)
        {
            if (db >= SpotRankWeights.StrongSignalDb)
            {
                score += SpotRankWeights.StrongSignal;
                parts.Add((14, SignalReport.Describe(db)));
            }
            else if (db >= SignalReport.FairDb)
            {
                score += SpotRankWeights.FairSignal;
                parts.Add((7, SignalReport.Describe(db)));
            }
            else if (db >= SignalReport.WeakDb)
            {
                score += SpotRankWeights.WeakSignal;
            }
            else
            {
                score += SpotRankWeights.BarelyThere;
                parts.Add((2, SignalReport.Describe(db)));
            }
        }

        if (spot.ReportCount is { } reports && reports > 1)
        {
            var weight = reports >= 10
                ? SpotRankWeights.ManyReceivers
                : reports >= 5
                    ? SpotRankWeights.SeveralReceivers
                    : SpotRankWeights.AFewReceivers;

            score += weight;
            parts.Add((weight + 2, $"{reports} receivers hear it"));
        }

        return score;
    }

    /// <summary>Whether the operator can do anything about it today.</summary>
    private static int Workable(ActivitySpot spot, List<(int, string)> parts)
    {
        switch ((spot.Mode ?? "").ToUpperInvariant())
        {
            case "CW":
                parts.Add((11, "Morse, which is what Hamlet is built to decode"));
                return SpotRankWeights.Morse;

            case "FT8":
            case "FT4":
                parts.Add((15, "FT8, which Hamlet cannot decode until phase 3"));
                return SpotRankWeights.DigitalHamletCannotRead;

            default:
                return 0;
        }
    }

    /// <summary>
    /// Assemble the card's reason from the strongest phrases.
    /// </summary>
    /// <param name="parts">Weighted phrases collected while scoring.</param>
    /// <returns>The reason line; never empty.</returns>
    /// <remarks>
    /// Never returns an empty string. A card whose spot carries no notable fact
    /// still has to account for itself, so the weakest case still prints what
    /// little is known rather than showing a bare rank.
    /// </remarks>
    private static string BuildReason(List<(int Weight, string Text)> parts)
    {
        if (parts.Count == 0)
        {
            return "on the air, with nothing else reported";
        }

        var chosen = parts
            .OrderByDescending(p => p.Weight)
            .Take(SpotRankWeights.MaxReasonParts)
            .Select(p => p.Text);

        return string.Join(" · ", chosen);
    }
}

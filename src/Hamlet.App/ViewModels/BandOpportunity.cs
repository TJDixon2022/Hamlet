using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>
/// What one band is holding, so an empty band can point at a busy one.
/// </summary>
/// <param name="BandName">The band, e.g. "40 m".</param>
/// <param name="Count">How many live spots it holds.</param>
/// <param name="NewestAge">How long since the most recent of them.</param>
/// <param name="Activations">How many of them are park or summit activations.</param>
public sealed record BandOpportunity(
    string BandName, int Count, TimeSpan NewestAge, int Activations)
{
    /// <summary>
    /// The band in one sentence, with the evidence attached (HM-DEC-025).
    /// </summary>
    /// <returns>e.g. "40 m has eleven stations, the newest a few minutes ago."</returns>
    /// <remarks>
    /// Counted in words rather than digits, like everything else the operator
    /// reads (§0.7). Activations are named separately when there are any,
    /// because "three of them park activators" is the part that tells a
    /// newcomer the band is worth their nerve rather than just worth their
    /// time.
    /// </remarks>
    public string Describe()
    {
        var what = Count == 1 ? "one station" : $"{Spell(Count)} stations";
        var age = SpotLifetime.DescribeAge(NewestAge);

        var activations = Activations switch
        {
            0 => "",
            1 => ", one of them a park or summit activator",
            _ => $", {Spell(Activations)} of them park or summit activators",
        };

        return $"{BandName} has {what}{activations}, the newest {age}.";
    }

    private static string Spell(int n) => n switch
    {
        1 => "one",
        2 => "two",
        3 => "three",
        4 => "four",
        5 => "five",
        6 => "six",
        7 => "seven",
        8 => "eight",
        9 => "nine",
        10 => "ten",
        11 => "eleven",
        12 => "twelve",
        _ => n.ToString(System.Globalization.CultureInfo.InvariantCulture),
    };
}

/// <summary>
/// Summarizes what every band is holding, from history.
/// </summary>
/// <remarks>
/// <para>THE ANSWER TO AN EMPTY BAND (HM-DEC-045). The app already had this
/// data and never used it, so a quiet band produced "nothing here" while
/// eleven stations sat on the band next door. Looking across every band before
/// declaring emptiness costs one pass over a list already in memory.</para>
/// <para>Each band is judged with its own spots' lifetimes, so a park
/// activation counts for an hour and a skimmer report for twenty minutes, and
/// the count means the same thing on every row.</para>
/// <para>Pure: spots and a moment in, a summary out (§5).</para>
/// </remarks>
public static class BandOpportunities
{
    /// <summary>
    /// What each band is holding right now.
    /// </summary>
    /// <param name="bands">The bands on display.</param>
    /// <param name="spots">Every spot held, across all bands.</param>
    /// <param name="nowUtc">The moment to judge against.</param>
    /// <param name="lifetimes">The configured lifetimes.</param>
    /// <returns>One entry per band, busiest first.</returns>
    public static IReadOnlyList<BandOpportunity> Summarize(
        IReadOnlyList<CwBand> bands,
        IReadOnlyList<ActivitySpot> spots,
        DateTime nowUtc,
        SpotLifetimeSettings? lifetimes = null)
    {
        var results = new List<BandOpportunity>(bands.Count);

        foreach (var band in bands)
        {
            var live = spots
                .Where(s => s.FrequencyHz >= band.LowHz && s.FrequencyHz <= band.HighHz)
                .Where(s => SpotLifetime.IsLive(s, nowUtc, lifetimes))
                .Where(s => s.CallType != SpotCallType.Beacon)
                .ToList();

            if (live.Count == 0)
            {
                results.Add(new BandOpportunity(band.Name, 0, TimeSpan.Zero, 0));
                continue;
            }

            var newest = live.Max(s => s.HeardAtUtc);

            results.Add(new BandOpportunity(
                band.Name,
                live.Count,
                nowUtc - newest,
                live.Count(s => s.IsActivation)));
        }

        return results.OrderByDescending(r => r.Count).ToList();
    }

    /// <summary>
    /// The bands in order, best first, and whether that order rests on
    /// anything Hamlet actually heard.
    /// </summary>
    /// <param name="bands">The bands on display.</param>
    /// <param name="spots">Every spot held, across all bands.</param>
    /// <param name="nowUtc">The moment to judge against.</param>
    /// <param name="localHour">Local hour, for the tiebreaker.</param>
    /// <param name="lifetimes">The configured lifetimes.</param>
    /// <returns>The ranking.</returns>
    /// <remarks>
    /// <para>ONE ORDER, SO NOTHING CAN DISAGREE (HM-DEC-046). The best-bet
    /// badge, the lead card's alternative and the band pips all read from this.
    /// They used to answer the same question separately, and the badge answered
    /// it from a clock lookup table written in the first week, which is how it
    /// came to sit on a band with no pips while the lead card was pointing
    /// somewhere else on the same screen.</para>
    /// <para>Activations break ties ahead of raw count, because a park operator
    /// wanting contacts is worth more to a newcomer than the same number of
    /// bare skimmer reports, and recency breaks the rest.</para>
    /// </remarks>
    public static BandRanking Rank(
        IReadOnlyList<CwBand> bands,
        IReadOnlyList<ActivitySpot> spots,
        DateTime nowUtc,
        int localHour,
        SpotLifetimeSettings? lifetimes = null)
    {
        var summary = Summarize(bands, spots, nowUtc, lifetimes);

        if (summary.Any(b => b.Count > 0))
        {
            var observed = summary
                .OrderByDescending(b => b.Count)
                .ThenByDescending(b => b.Activations)
                .ThenBy(b => b.NewestAge)
                .ThenBy(b => b.BandName, StringComparer.Ordinal)
                .ToList();

            return new BandRanking(observed, FromObservation: true);
        }

        // Nothing has been heard on any band. The clock heuristic is all
        // that is left, and it is a guess: low bands at night, high bands by
        // day, from a table written before the app could hear anything. It is
        // allowed to stand in, and it has to say so (HM-DEC-009).
        var order = BandPlan.BestBets(localHour);

        var guessed = summary
            .OrderBy(b => IndexOf(order, b.BandName))
            .ThenBy(b => b.BandName, StringComparer.Ordinal)
            .ToList();

        return new BandRanking(guessed, FromObservation: false);
    }

    private static int IndexOf(IReadOnlyList<string> order, string name)
    {
        for (var i = 0; i < order.Count; i++)
        {
            if (string.Equals(order[i], name, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return int.MaxValue;
    }
}

/// <summary>The bands in order, and what the order rests on.</summary>
/// <param name="Bands">Best first.</param>
/// <param name="FromObservation">True when at least one band has live spots,
/// so the order reflects what Hamlet heard rather than the clock.</param>
public sealed record BandRanking(
    IReadOnlyList<BandOpportunity> Bands, bool FromObservation)
{
    /// <summary>The best band, or null when there are no bands at all.</summary>
    public BandOpportunity? Best => Bands.Count > 0 ? Bands[0] : null;

    /// <summary>The best band's name, or "".</summary>
    public string BestBandName => Best?.BandName ?? "";

    /// <summary>
    /// Whether the best-bet badge belongs on this band.
    /// </summary>
    /// <param name="bandName">The band a button is drawing.</param>
    /// <returns>True on exactly one band, or none when nothing is ranked.</returns>
    /// <remarks>
    /// The badge's whole decision, as one function, so it can be tested rather
    /// than trusted (HM-DEC-046). The ViewModel does nothing but copy this
    /// answer onto each button, which is what stops the badge quietly
    /// acquiring a second opinion the way it had one before.
    /// </remarks>
    public bool BadgeGoesOn(string bandName)
        => BestBandName.Length > 0
           && string.Equals(bandName, BestBandName, StringComparison.Ordinal);

    /// <summary>
    /// The best band that is not the one already on screen, or null.
    /// </summary>
    /// <param name="currentBand">The band the operator is looking at.</param>
    /// <returns>The alternative to offer, or null when there is none.</returns>
    /// <remarks>
    /// The lead card's suggestion comes from here rather than from its own
    /// pass over the data, which is what makes it impossible for the badge and
    /// the card to name different bands (HM-DEC-046). Only a band with
    /// something live on it is ever offered: sending somebody to an empty band
    /// on a clock guess would be worse than saying nothing.
    /// </remarks>
    public BandOpportunity? BestOtherThan(string currentBand)
        => Bands.FirstOrDefault(
            b => b.Count > 0
                 && !string.Equals(b.BandName, currentBand, StringComparison.Ordinal));

    /// <summary>
    /// What the badge says, so a guess can never look like an observation.
    /// </summary>
    /// <remarks>
    /// "best bet now" is a claim about what is happening. With nothing heard
    /// on any band it becomes a claim about the hour, and it says so in words
    /// rather than looking identical to the real thing (§0.0, §0.6).
    /// </remarks>
    public string BadgeLabel => FromObservation ? "best bet now" : "likely, going on the hour";

    /// <summary>The hover line behind the badge, with its evidence.</summary>
    public string BadgeTooltip
    {
        get
        {
            if (Best is not { } best)
            {
                return "";
            }

            return FromObservation
                ? $"The busiest band Hamlet can see. {best.Describe()}"
                : "Nothing has been heard on any band yet, so this is going on the "
                  + "time of day rather than on anything reported. Low bands after "
                  + "dark, high bands in daylight. Treat it as a place to start "
                  + "listening rather than as a fact.";
        }
    }
}

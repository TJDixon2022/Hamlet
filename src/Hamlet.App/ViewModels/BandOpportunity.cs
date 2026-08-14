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
}

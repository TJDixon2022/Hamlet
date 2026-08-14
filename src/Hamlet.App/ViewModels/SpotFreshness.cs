using System.Globalization;

namespace Hamlet.App.ViewModels;

/// <summary>How much the happening-now feed should be trusted right now.</summary>
public enum FreshnessLevel
{
    /// <summary>Inside one refresh interval: shown normally.</summary>
    Fresh,

    /// <summary>Past twice the interval: shown amber.</summary>
    Aging,

    /// <summary>Past four times the interval: labeled "stale".</summary>
    Stale,
}

/// <summary>
/// The freshness rule for the happening-now feed (HM-DEC-020), as pure
/// functions of an elapsed time and the configured interval.
/// </summary>
/// <remarks>
/// No clock is read here on purpose (§5, determinism): the caller supplies
/// the elapsed time, so every threshold is testable without waiting and
/// without a fake clock. This is the prime directive applied to the feed
/// itself — a panel that says "7 spots" while the data is an hour old is
/// presenting a guess as a fact.
/// </remarks>
public static class SpotFreshness
{
    /// <summary>Multiple of the interval at which the age goes amber.</summary>
    public const int AgingMultiple = 2;

    /// <summary>Multiple of the interval at which the feed reads "stale".</summary>
    public const int StaleMultiple = 4;

    /// <summary>
    /// The yardstick used when auto-refresh is switched off. The operator has
    /// turned off the refresh, not turned off the passage of time, so the
    /// panel still ages against the shipped interval.
    /// </summary>
    public const int OffYardstickMinutes = Settings.AppSettings.DefaultSpotRefreshMinutes;

    /// <summary>Classify an elapsed time against the refresh interval.</summary>
    /// <param name="sinceUpdate">Time since the feed last loaded.</param>
    /// <param name="intervalMinutes">Configured interval; 0 means off, and
    /// falls back to <see cref="OffYardstickMinutes"/>.</param>
    /// <returns>Fresh, Aging or Stale.</returns>
    public static FreshnessLevel Evaluate(TimeSpan sinceUpdate, int intervalMinutes)
    {
        var yardstick = intervalMinutes > 0 ? intervalMinutes : OffYardstickMinutes;
        var minutes = Math.Max(0, sinceUpdate.TotalMinutes);

        if (minutes > yardstick * (double)StaleMultiple)
        {
            return FreshnessLevel.Stale;
        }

        return minutes > yardstick * (double)AgingMultiple
            ? FreshnessLevel.Aging
            : FreshnessLevel.Fresh;
    }

    /// <summary>Plain-language age, e.g. "just now", "30s ago", "4 min ago".</summary>
    /// <param name="elapsed">How long ago it happened.</param>
    /// <returns>Human-readable age.</returns>
    public static string Describe(TimeSpan elapsed)
    {
        var seconds = Math.Max(0, elapsed.TotalSeconds);

        if (seconds < 10)
        {
            return "just now";
        }

        if (seconds < 60)
        {
            return ((int)seconds).ToString(CultureInfo.InvariantCulture) + "s ago";
        }

        var minutes = (int)(seconds / 60);
        if (minutes < 90)
        {
            return minutes.ToString(CultureInfo.InvariantCulture) + " min ago";
        }

        var hours = minutes / 60;
        return hours.ToString(CultureInfo.InvariantCulture)
               + (hours == 1 ? " hour ago" : " hours ago");
    }

    /// <summary>
    /// The panel header summary — the line a collapsed panel still carries
    /// (HM-DEC-021), e.g. "7 spots · updated 30s ago" or
    /// "7 spots · stale · updated 42 min ago".
    /// </summary>
    /// <param name="spotCount">How many spots are showing.</param>
    /// <param name="sinceUpdate">Time since the feed last loaded.</param>
    /// <param name="intervalMinutes">Configured interval; 0 means off.</param>
    /// <param name="everLoaded">False before the first load completes.</param>
    /// <returns>The summary line.</returns>
    /// <remarks>
    /// FEED FRESHNESS, NOT OPPORTUNITY FRESHNESS (HM-DEC-045). "Checked" is
    /// deliberate: this line says how long since Hamlet last talked to the
    /// network, which is a fact about Hamlet. How long since each spot
    /// happened, and whether that person is likely still there, is a different
    /// fact and belongs on the card. A feed checked four seconds ago can be
    /// full of hour-old spots, and the wording must not let anybody read one
    /// as the other.
    /// </remarks>
    public static string Summary(
        int spotCount, TimeSpan sinceUpdate, int intervalMinutes, bool everLoaded = true)
    {
        if (!everLoaded)
        {
            return "loading…";
        }

        var count = spotCount == 1 ? "1 spot" : $"{spotCount} spots";
        var stale = Evaluate(sinceUpdate, intervalMinutes) == FreshnessLevel.Stale
            ? " · feed is stale"
            : "";

        return $"{count}{stale} · checked {Describe(sinceUpdate)}";
    }
}

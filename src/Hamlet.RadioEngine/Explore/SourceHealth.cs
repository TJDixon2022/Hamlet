namespace Hamlet.RadioEngine.Explore;

/// <summary>How a single activity source is currently doing.</summary>
public enum SourceState
{
    /// <summary>Switched off by the operator.</summary>
    Disabled,

    /// <summary>Never asked yet this session.</summary>
    Idle,

    /// <summary>Answered on the last attempt.</summary>
    Ok,

    /// <summary>Failed, but earlier spots are still on screen and aging.</summary>
    Degraded,

    /// <summary>Failed with nothing left to show.</summary>
    Failed,
}

/// <summary>What one source contributed to the last refresh.</summary>
/// <param name="Name">The source's stable name, e.g. "POTA".</param>
/// <param name="State">Where it stands right now.</param>
/// <param name="SpotCount">Spots it is currently contributing, cached ones
/// included.</param>
/// <param name="LastOkUtc">When it last answered, or null if it never has.</param>
/// <param name="Message">Short plain-language note for the operator, e.g.
/// "no answer — retrying in 2 min". Null when there is nothing to say.</param>
/// <remarks>
/// This record is the whole reason the band-conditions line can be honest
/// (HM-DEC-025). A count of signals means nothing without knowing which
/// networks were answering when it was taken, so the count and its provenance
/// travel together.
/// </remarks>
public sealed record SourceStatus(
    string Name, SourceState State, int SpotCount, DateTime? LastOkUtc, string? Message)
{
    /// <summary>True when this source's numbers can be relied on.</summary>
    public bool IsContributing => State is SourceState.Ok or SourceState.Degraded;

    /// <summary>True when the operator asked for this source and it is not
    /// answering — the case the conditions line must confess to.</summary>
    public bool IsLetDown => State is SourceState.Degraded or SourceState.Failed;
}

/// <summary>
/// How long to wait before retrying a source that just failed.
/// </summary>
/// <remarks>
/// Pure arithmetic on a failure count, with no clock read and no randomness,
/// so the schedule is testable exactly (§5). Doubling from thirty seconds and
/// capping at fifteen minutes: long enough that a network outage costs a
/// struggling service nothing, short enough that a passing blip clears before
/// the operator notices (HM-DEC-022).
/// </remarks>
public static class SourceBackoff
{
    /// <summary>Wait after the first failure.</summary>
    public static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(30);

    /// <summary>The longest wait between retries.</summary>
    public static readonly TimeSpan MaxDelay = TimeSpan.FromMinutes(15);

    /// <summary>How long to wait after a given number of consecutive failures.</summary>
    /// <param name="consecutiveFailures">Failures since the last success;
    /// zero or less means no wait.</param>
    /// <returns>The delay before the next attempt.</returns>
    public static TimeSpan Delay(int consecutiveFailures)
    {
        if (consecutiveFailures <= 0)
        {
            return TimeSpan.Zero;
        }

        // Shift rather than Pow, and stop shifting well before it overflows.
        var doublings = Math.Min(consecutiveFailures - 1, 16);
        var ticks = BaseDelay.Ticks * (1L << doublings);

        return ticks >= MaxDelay.Ticks ? MaxDelay : TimeSpan.FromTicks(ticks);
    }

    /// <summary>Plain-language note for a source that is waiting to retry.</summary>
    /// <param name="remaining">Time left before the next attempt.</param>
    /// <returns>Text such as "no answer — retrying in 2 min".</returns>
    public static string Describe(TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero)
        {
            return "no answer — retrying now";
        }

        return remaining.TotalSeconds < 90
            ? $"no answer — retrying in {Math.Max(1, (int)remaining.TotalSeconds)}s"
            : $"no answer — retrying in {(int)Math.Round(remaining.TotalMinutes)} min";
    }
}

namespace Hamlet.RadioEngine.Explore;

/// <summary>A spot as it came back out of the store.</summary>
/// <param name="Spot">The spot itself.</param>
/// <param name="FirstSeenUtc">When Hamlet first recorded it.</param>
/// <param name="LastSeenUtc">The last time a source reported it again.</param>
/// <param name="ActedOnUtc">
/// When the operator tuned to it, or null. Kept so "what's new" never re-offers
/// somebody they have already been to see (HM-DEC-057).
/// </param>
public sealed record StoredSpot(
    ActivitySpot Spot,
    DateTime FirstSeenUtc,
    DateTime LastSeenUtc,
    DateTime? ActedOnUtc = null);

/// <summary>
/// Somewhere to keep every spot Hamlet has seen, so nothing is lost on
/// restart.
/// </summary>
/// <remarks>
/// <para>WHY THIS EXISTS (HM-DEC-045). Hamlet used to hold spots in memory and
/// throw away anything past a ten-minute window, which meant it forgot
/// everything on restart and forgot most of it while running. The Reverse
/// Beacon Network compounds it: it is a live stream, so a fresh start knows
/// nothing at all until somebody transmits. With history the display becomes a
/// VIEW over what has been seen rather than a buffer that forgets.</para>
/// <para>The interface exists so the store can be faked in tests and so a
/// store that cannot be opened can be swapped for one that only remembers this
/// session, rather than the app failing to start over a cache.</para>
/// <para>Implementations never throw for storage reasons. Losing history is a
/// nuisance; refusing to run is a bug (§8).</para>
/// </remarks>
public interface ISpotStore : IDisposable
{
    /// <summary>True when this store outlives the process.</summary>
    /// <remarks>
    /// False for the memory fallback, and the app says so rather than letting
    /// somebody believe their history is being kept when it is not.
    /// </remarks>
    bool IsPersistent { get; }

    /// <summary>
    /// Record spots, updating the last-seen time of any already held.
    /// </summary>
    /// <param name="spots">What a refresh returned.</param>
    /// <param name="nowUtc">The moment of this refresh.</param>
    /// <returns>How many rows were newly inserted.</returns>
    int Record(IReadOnlyList<ActivitySpot> spots, DateTime nowUtc);

    /// <summary>
    /// Everything seen since a cutoff, newest first.
    /// </summary>
    /// <param name="sinceUtc">The oldest report time to return.</param>
    /// <returns>The spots, newest reported first.</returns>
    IReadOnlyList<StoredSpot> Since(DateTime sinceUtc);

    /// <summary>
    /// Drop anything older than a cutoff.
    /// </summary>
    /// <param name="beforeUtc">Delete spots reported before this.</param>
    /// <returns>How many rows went.</returns>
    int Prune(DateTime beforeUtc);

    /// <summary>
    /// Record that the operator tuned to a spot.
    /// </summary>
    /// <param name="key">The spot's identity, from <see cref="SpotIdentity"/>.</param>
    /// <param name="nowUtc">When they did it.</param>
    /// <remarks>
    /// MARKED, NEVER REMOVED (HM-DEC-057). Acting on a spot takes it out of
    /// "what's new" and leaves it exactly where it was under "best chance",
    /// because it is still a live station and somebody may want to go back.
    /// Marking something the store has never heard of is harmless and does
    /// nothing, which is the honest answer rather than an invented row.
    /// </remarks>
    void MarkActedOn(string key, DateTime nowUtc);

    /// <summary>How many spots are held.</summary>
    /// <returns>The row count.</returns>
    int Count();
}

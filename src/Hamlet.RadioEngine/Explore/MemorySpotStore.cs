namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// A spot store that only remembers this session.
/// </summary>
/// <remarks>
/// <para>What Hamlet falls back to when the database cannot be opened: a disk
/// that is full, a file another copy of the app has locked, a folder somebody
/// made read-only. Losing history is a nuisance and refusing to start over a
/// cache would be a bug (§8), so the app carries on with everything the
/// session has seen and says plainly that it will not survive a restart.</para>
/// <para>It is also what the tests drive, since it applies exactly the same
/// dedupe and prune rules as the real one without touching a disk.</para>
/// </remarks>
public sealed class MemorySpotStore : ISpotStore
{
    private readonly Dictionary<string, StoredSpot> _rows =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly object _gate = new();

    /// <inheritdoc/>
    public bool IsPersistent => false;

    /// <inheritdoc/>
    public int Record(IReadOnlyList<ActivitySpot> spots, DateTime nowUtc)
    {
        var inserted = 0;

        lock (_gate)
        {
            foreach (var spot in spots)
            {
                var key = SpotIdentity.KeyFor(spot);

                if (_rows.TryGetValue(key, out var held))
                {
                    // Seen again is an update, never a second row. The report
                    // time stays the earliest one, because that is when the
                    // thing actually happened.
                    _rows[key] = held with
                    {
                        LastSeenUtc = nowUtc,
                        Spot = SpotIdentity.Better(held.Spot, spot),
                    };
                    continue;
                }

                _rows[key] = new StoredSpot(spot, nowUtc, nowUtc);
                inserted++;
            }
        }

        return inserted;
    }

    /// <inheritdoc/>
    public IReadOnlyList<StoredSpot> Since(DateTime sinceUtc)
    {
        lock (_gate)
        {
            return _rows.Values
                .Where(r => r.Spot.HeardAtUtc >= sinceUtc)
                .OrderByDescending(r => r.Spot.HeardAtUtc)
                .ToList();
        }
    }

    /// <inheritdoc/>
    public int Prune(DateTime beforeUtc)
    {
        lock (_gate)
        {
            var gone = _rows
                .Where(p => p.Value.Spot.HeardAtUtc < beforeUtc)
                .Select(p => p.Key)
                .ToList();

            foreach (var key in gone)
            {
                _rows.Remove(key);
            }

            return gone.Count;
        }
    }

    /// <inheritdoc/>
    public int Count()
    {
        lock (_gate)
        {
            return _rows.Count;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_gate)
        {
            _rows.Clear();
        }
    }
}

/// <summary>
/// How a spot is identified across refreshes and across sources.
/// </summary>
/// <remarks>
/// The same rule the aggregate already applies, in one place so the store and
/// the merge cannot drift apart: who, and roughly where. Frequencies bucket to
/// 200 Hz because two skimmers measuring the same carrier rarely agree to the
/// hertz.
/// </remarks>
public static class SpotIdentity
{
    /// <summary>The bucket width used when comparing frequencies.</summary>
    public const long FrequencyBucketHz = 200;

    /// <summary>A stable key for one spot.</summary>
    /// <param name="spot">The spot.</param>
    /// <returns>Its identity.</returns>
    public static string KeyFor(ActivitySpot spot)
    {
        var bucket = spot.FrequencyHz / FrequencyBucketHz;

        return string.IsNullOrWhiteSpace(spot.DxCall)
            ? $"@{bucket}|{spot.Story}"
            : $"{spot.DxCall.Trim().ToUpperInvariant()}|{bucket}";
    }

    /// <summary>
    /// Of two reports of the same station, the one worth keeping.
    /// </summary>
    /// <param name="held">What the store already has.</param>
    /// <param name="arriving">What just came in.</param>
    /// <returns>The better record.</returns>
    /// <remarks>
    /// An activation beats a bare skimmer report, because knowing somebody is
    /// in a park is worth more to a newcomer than knowing a receiver heard
    /// them. Otherwise the earlier report time wins, since that is when the
    /// thing actually happened and a later sighting does not move it.
    /// </remarks>
    public static ActivitySpot Better(ActivitySpot held, ActivitySpot arriving)
    {
        if (arriving.IsActivation && !held.IsActivation)
        {
            return arriving;
        }

        if (held.IsActivation && !arriving.IsActivation)
        {
            return held;
        }

        return held.HeardAtUtc <= arriving.HeardAtUtc ? held : arriving;
    }
}

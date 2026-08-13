namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// Fans one refresh out to every enabled source, and never lets one dead
/// network take the panel down with it.
/// </summary>
/// <remarks>
/// <para>The rules, which are HM-DEC-022 (HM-DEC-024 adds the sources that
/// exercise them):</para>
/// <list type="bullet">
/// <item>A source the operator switched off contributes nothing and its
/// cached spots are dropped — "off" means gone, not hidden.</item>
/// <item>A source that fails keeps its previous spots on screen, aging
/// visibly, and is marked Degraded. Losing the network is not a reason to
/// blank a panel the operator was reading.</item>
/// <item>A failing source is retried on an exponential backoff, so an outage
/// costs a struggling volunteer service nothing.</item>
/// <item>Cached spots expire at <see cref="CacheMaxAge"/>. A spot old enough
/// to be irrelevant is dropped rather than shown as "happening now".</item>
/// <item>Every refresh publishes <see cref="Statuses"/>, so the UI can say
/// which networks its numbers actually came from instead of implying they are
/// complete (HM-DEC-009).</item>
/// </list>
/// </remarks>
public sealed class AggregateActivitySource : IContextualActivitySource
{
    /// <summary>The settings key and display name for this source.</summary>
    public const string SourceName = "All sources";

    /// <summary>Beyond this age a cached spot is no longer "happening now".</summary>
    public static readonly TimeSpan CacheMaxAge = TimeSpan.FromMinutes(45);

    /// <summary>How long a single source gets before it is counted as down.</summary>
    public static readonly TimeSpan PerSourceTimeout = TimeSpan.FromSeconds(20);

    private readonly IReadOnlyList<IActivitySource> _sources;
    private readonly Func<string, bool> _isEnabled;
    private readonly Func<DateTime> _utcNow;
    private readonly Dictionary<string, SourceEntry> _entries = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    /// <summary>Creates the aggregate.</summary>
    /// <param name="sources">The sources to fan out to, in preference order —
    /// earlier sources win a duplicate.</param>
    /// <param name="isEnabled">Answers whether a source name is switched on.
    /// Called on every refresh so a toggle takes effect immediately.</param>
    /// <param name="utcNow">Clock, injected so backoff is testable without
    /// waiting (§5). Defaults to the system clock.</param>
    public AggregateActivitySource(
        IEnumerable<IActivitySource> sources,
        Func<string, bool> isEnabled,
        Func<DateTime>? utcNow = null)
    {
        _sources = sources.ToArray();
        _isEnabled = isEnabled;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);

        foreach (var source in _sources)
        {
            _entries[source.Name] = new SourceEntry();
        }
    }

    /// <inheritdoc/>
    public string Name => SourceName;

    /// <summary>What each source contributed to the last refresh.</summary>
    public IReadOnlyList<SourceStatus> Statuses
    {
        get
        {
            lock (_gate)
            {
                return _sources
                    .Select(s => _entries[s.Name].Status ?? Idle(s.Name))
                    .ToArray();
            }
        }
    }

    /// <inheritdoc/>
    public void SetContext(ActivityContext context)
    {
        foreach (var source in _sources.OfType<IContextualActivitySource>())
        {
            source.SetContext(context);
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ActivitySpot>> GetSpotsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = _utcNow();
        var work = new List<(IActivitySource Source, Task<IReadOnlyList<ActivitySpot>>? Task)>();

        foreach (var source in _sources)
        {
            if (!_isEnabled(source.Name))
            {
                SetDisabled(source.Name);
                continue;
            }

            if (IsWaitingToRetry(source.Name, now, out var remaining))
            {
                KeepCached(source.Name, now, SourceBackoff.Describe(remaining));
                continue;
            }

            work.Add((source, FetchAsync(source, cancellationToken)));
        }

        foreach (var (source, task) in work)
        {
            if (task is null)
            {
                continue;
            }

            try
            {
                var spots = await task.ConfigureAwait(false);
                Succeed(source.Name, spots, now);
            }
            catch (Exception)
            {
                // The source is the thing that failed, not the refresh. Its
                // last spots stay on screen and age where the operator can
                // see them.
                Fail(source.Name, now);
            }
        }

        return Collect(now);
    }

    private async Task<IReadOnlyList<ActivitySpot>> FetchAsync(
        IActivitySource source, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(PerSourceTimeout);
        return await source.GetSpotsAsync(timeout.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Merge every contributing source's spots, newest first, dropping
    /// duplicates.
    /// </summary>
    /// <remarks>
    /// The same station is routinely reported by RBN and by POTA within
    /// seconds. Source order decides the winner, and the aggregate is
    /// constructed activation-sources-first, so the operator keeps the
    /// version of the spot that knows it is a park activation.
    /// </remarks>
    private IReadOnlyList<ActivitySpot> Collect(DateTime now)
    {
        lock (_gate)
        {
            var merged = new List<ActivitySpot>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var source in _sources)
            {
                var entry = _entries[source.Name];
                if (entry.Status?.IsContributing != true)
                {
                    continue;
                }

                foreach (var spot in entry.Spots)
                {
                    if (now - spot.HeardAtUtc > CacheMaxAge)
                    {
                        continue;
                    }

                    if (!seen.Add(DedupeKey(spot)))
                    {
                        continue;
                    }

                    merged.Add(spot);
                }
            }

            merged.Sort((a, b) => b.HeardAtUtc.CompareTo(a.HeardAtUtc));
            return merged;
        }
    }

    /// <summary>
    /// Identity across sources: who, and roughly where. Frequencies are
    /// rounded to the nearest 200 Hz because two skimmers measuring the same
    /// carrier rarely agree to the hertz.
    /// </summary>
    private static string DedupeKey(ActivitySpot spot)
    {
        var bucket = spot.FrequencyHz / 200;
        return string.IsNullOrWhiteSpace(spot.DxCall)
            ? $"@{bucket}|{spot.Story}"
            : $"{spot.DxCall.Trim().ToUpperInvariant()}|{bucket}";
    }

    private void Succeed(string name, IReadOnlyList<ActivitySpot> spots, DateTime now)
    {
        lock (_gate)
        {
            var entry = _entries[name];
            entry.Spots = spots;
            entry.Failures = 0;
            entry.RetryAtUtc = null;
            entry.LastOkUtc = now;
            entry.Status = new SourceStatus(name, SourceState.Ok, spots.Count, now, null)
            {
                ScopedToBand = ScopeOf(name),
            };
        }
    }

    /// <summary>
    /// The band a source is limited to, or null when it sees them all.
    /// </summary>
    /// <remarks>
    /// Published on every status so callers can tell a source that heard
    /// nothing on a band from a source that was never pointed at it
    /// (HM-DEC-031).
    /// </remarks>
    private string? ScopeOf(string name)
        => _sources.FirstOrDefault(s => s.Name == name) is IBandScopedActivitySource scoped
            ? scoped.ScopedBandName
            : null;

    private void Fail(string name, DateTime now)
    {
        lock (_gate)
        {
            var entry = _entries[name];
            entry.Failures++;
            var delay = SourceBackoff.Delay(entry.Failures);
            entry.RetryAtUtc = now + delay;

            var live = entry.Spots.Count(s => now - s.HeardAtUtc <= CacheMaxAge);
            entry.Status = new SourceStatus(
                name,
                live > 0 ? SourceState.Degraded : SourceState.Failed,
                live,
                entry.LastOkUtc,
                SourceBackoff.Describe(delay))
            {
                ScopedToBand = ScopeOf(name),
            };
        }
    }

    private void SetDisabled(string name)
    {
        lock (_gate)
        {
            var entry = _entries[name];
            entry.Spots = Array.Empty<ActivitySpot>();
            entry.Failures = 0;
            entry.RetryAtUtc = null;
            entry.Status = new SourceStatus(
                name, SourceState.Disabled, 0, entry.LastOkUtc, null)
            {
                ScopedToBand = ScopeOf(name),
            };
        }
    }

    private void KeepCached(string name, DateTime now, string message)
    {
        lock (_gate)
        {
            var entry = _entries[name];
            var live = entry.Spots.Count(s => now - s.HeardAtUtc <= CacheMaxAge);
            entry.Status = new SourceStatus(
                name,
                live > 0 ? SourceState.Degraded : SourceState.Failed,
                live,
                entry.LastOkUtc,
                message)
            {
                ScopedToBand = ScopeOf(name),
            };
        }
    }

    private bool IsWaitingToRetry(string name, DateTime now, out TimeSpan remaining)
    {
        lock (_gate)
        {
            var retryAt = _entries[name].RetryAtUtc;
            if (retryAt is null || now >= retryAt.Value)
            {
                remaining = TimeSpan.Zero;
                return false;
            }

            remaining = retryAt.Value - now;
            return true;
        }
    }

    private SourceStatus Idle(string name)
        => new(name, SourceState.Idle, 0, null, null) { ScopedToBand = ScopeOf(name) };

    private sealed class SourceEntry
    {
        public IReadOnlyList<ActivitySpot> Spots { get; set; } = Array.Empty<ActivitySpot>();

        public int Failures { get; set; }

        public DateTime? RetryAtUtc { get; set; }

        public DateTime? LastOkUtc { get; set; }

        public SourceStatus? Status { get; set; }
    }
}

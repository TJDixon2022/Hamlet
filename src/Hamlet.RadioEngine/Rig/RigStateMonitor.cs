namespace Hamlet.RadioEngine.Rig;

/// <summary>Payload for <see cref="RigStateMonitor.StateChanged"/>.</summary>
public sealed class RigStateChangedEventArgs : EventArgs
{
    /// <summary>Creates the payload.</summary>
    /// <param name="state">The new snapshot.</param>
    public RigStateChangedEventArgs(RigState state) => State = state;

    /// <summary>Everything known about the radio, as of now.</summary>
    public RigState State { get; }
}

/// <summary>
/// Keeps the rig state model current, without flooding a slow bus.
/// </summary>
/// <remarks>
/// <para>THE POLITENESS RULES, all in one place (HM-DEC-050). CI-V is a slow
/// serial line shared with the radio's own transceive stream, and a decoder
/// that hammers it makes the radio feel sluggish and the app unreliable, which
/// is the hardest kind of defect to attribute because nothing actually
/// breaks.</para>
/// <para>One command at a time, which the rig already enforces with its own
/// gate, so this never issues a second read before the first has answered or
/// timed out. Fast-moving values a few times a second and only while somebody
/// is looking at them. Slow-moving ones swept on connect and then rarely.
/// Nothing polled that the radio volunteers. And everything stops the moment
/// the window goes out of sight, which is the same politeness the spot feeds
/// already observe (HM-DEC-020).</para>
/// <para>A read that times out marks its value unknown and the loop moves on to
/// the next field. It is never retried in place: a bus that is already
/// struggling is the last thing to send more commands to, and the next sweep
/// will ask again anyway.</para>
/// <para>No clock is read except through <see cref="DateTime.UtcNow"/> at the
/// moment a value is stamped, and the loop's pacing is injectable, so a test can
/// drive a hundred sweeps without waiting for a hundred real intervals.</para>
/// </remarks>
public sealed class RigStateMonitor : IDisposable
{
    private readonly IRig _rig;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;
    private readonly object _gate = new();

    private CancellationTokenSource? _cts;
    private Task? _loop;
    private RigState _state = RigState.Empty;
    private bool _watching = true;
    private bool _disposed;

    /// <summary>Creates a monitor over a rig.</summary>
    /// <param name="rig">The radio to keep track of.</param>
    /// <param name="delay">
    /// How the loop waits, injectable so a test can run it at speed. Null uses
    /// <see cref="Task.Delay(TimeSpan, CancellationToken)"/>.
    /// </param>
    public RigStateMonitor(IRig rig, Func<TimeSpan, CancellationToken, Task>? delay = null)
    {
        _rig = rig ?? throw new ArgumentNullException(nameof(rig));
        _delay = delay ?? Task.Delay;
        _rig.ValuesReported += OnValuesReported;
    }

    /// <summary>Everything known about the radio right now.</summary>
    public RigState State
    {
        get
        {
            lock (_gate)
            {
                return _state;
            }
        }
    }

    /// <summary>Raised whenever anything in the model changes.</summary>
    /// <remarks>Raised on the polling thread; the UI layer marshals.</remarks>
    public event EventHandler<RigStateChangedEventArgs>? StateChanged;

    /// <summary>How many commands the monitor has issued, for the record.</summary>
    /// <remarks>
    /// §0.0.1 wants the app's own behavior visible. "Is Hamlet flooding the
    /// bus" is a question somebody will ask, and a count is how it gets
    /// answered rather than argued about.
    /// </remarks>
    public long ReadCount { get; private set; }

    /// <summary>
    /// Whether anybody is looking. Polling stops entirely when nothing is.
    /// </summary>
    /// <remarks>
    /// The same politeness the spot feeds observe (HM-DEC-020). A minimized
    /// window has no S-meter on screen, so asking four times a second for one is
    /// pure noise on a bus somebody else is trying to use.
    /// </remarks>
    public bool IsWatching
    {
        get
        {
            lock (_gate)
            {
                return _watching;
            }
        }

        set
        {
            lock (_gate)
            {
                _watching = value;
            }
        }
    }

    /// <summary>Begin keeping the model current.</summary>
    public void Start()
    {
        lock (_gate)
        {
            if (_disposed || _loop is not null)
            {
                return;
            }

            _state = _state.WithRigName(_rig.Capabilities.Model);
            _cts = new CancellationTokenSource();
            _loop = Task.Run(() => RunAsync(_cts.Token), CancellationToken.None);
        }
    }

    /// <summary>Stop polling. Safe when not started.</summary>
    public void Stop()
    {
        CancellationTokenSource? cts;
        Task? loop;

        lock (_gate)
        {
            cts = _cts;
            loop = _loop;
            _cts = null;
            _loop = null;
        }

        cts?.Cancel();

        try
        {
            loop?.GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            // Shutdown never throws (§8).
        }

        cts?.Dispose();
    }

    /// <summary>
    /// Read one field now, whatever the plan says about it.
    /// </summary>
    /// <param name="field">What to read.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>A task that completes when the model has been updated.</returns>
    /// <remarks>
    /// The on-demand path: opening the diagnostics screen asks for the things
    /// that are not worth polling for.
    /// </remarks>
    public async Task RefreshAsync(RigField field, CancellationToken cancellationToken = default)
        => await ReadIntoStateAsync(field, cancellationToken).ConfigureAwait(false);

    /// <summary>Read every field the plan is willing to ask for.</summary>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>A task that completes when the sweep is done.</returns>
    public async Task RefreshAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var field in Enum.GetValues<RigField>())
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (RigPollPlan.RateFor(field) == RigPollRate.Never)
            {
                continue;
            }

            await ReadIntoStateAsync(field, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_gate)
        {
            _disposed = true;
        }

        _rig.ValuesReported -= OnValuesReported;
        Stop();
    }

    /// <summary>
    /// The loop: a session sweep on connect, then live values at their rate and
    /// a session sweep occasionally.
    /// </summary>
    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            var live = RigPollPlan.At(RigPollRate.Live);
            var session = RigPollPlan.At(RigPollRate.Session);
            var sinceSweep = RigPollPlan.SessionInterval;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!IsWatching)
                {
                    // Nothing on screen, so nothing on the bus. The sweep timer
                    // is left where it is, so coming back to the window brings
                    // the settings up to date rather than waiting out the rest
                    // of an interval that elapsed while nobody was looking.
                    await _delay(RigPollPlan.LiveInterval, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (sinceSweep >= RigPollPlan.SessionInterval)
                {
                    sinceSweep = TimeSpan.Zero;

                    foreach (var field in session)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        await ReadIntoStateAsync(field, cancellationToken).ConfigureAwait(false);
                    }
                }

                foreach (var field in live)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await ReadIntoStateAsync(field, cancellationToken).ConfigureAwait(false);
                }

                await _delay(RigPollPlan.LiveInterval, cancellationToken).ConfigureAwait(false);
                sinceSweep += RigPollPlan.LiveInterval;
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        catch (Exception)
        {
            // A polling loop that takes the app down with it would be worse
            // than one that stops (§8). The values simply stop being refreshed,
            // go stale, and the UI says so.
        }
    }

    /// <summary>Read one field and fold the answer into the model.</summary>
    private async Task ReadIntoStateAsync(RigField field, CancellationToken cancellationToken)
    {
        IReadOnlyList<RigValue> values;

        try
        {
            // The state is passed in because a couple of readings cannot be
            // decoded without it: a filter index means nothing without the mode.
            values = await _rig.ReadAsync(field, State, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // The contract says reads do not throw, and this is the belt to that
            // brace: whatever went wrong, the field is unknown and the loop
            // carries on rather than dying (§8).
            values = new[] { RigValue.Unknown(field, $"the read failed: {ex.GetType().Name}") };
        }

        ReadCount++;
        Publish(values);
    }

    private void OnValuesReported(object? sender, RigValuesReportedEventArgs e)
        => Publish(e.Values);

    /// <summary>Fold values into the model and tell anybody watching.</summary>
    private void Publish(IReadOnlyList<RigValue> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        RigState next;

        lock (_gate)
        {
            _state = _state.With(values);
            next = _state;
        }

        StateChanged?.Invoke(this, new RigStateChangedEventArgs(next));
    }
}

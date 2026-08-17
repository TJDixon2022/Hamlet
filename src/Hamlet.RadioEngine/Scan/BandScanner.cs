using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Scan;

/// <summary>
/// Why a scan is not running (HM-DEC-107, §0.2.1).
/// </summary>
/// <remarks>
/// **EVERY ONE OF THESE IS A DIFFERENT THING TO TELL THE OPERATOR** and they are
/// kept apart for the same reason unknown, off, unsupported and stale are kept
/// apart in the record (§8.1). "The radio stopped answering" and "you turned the
/// dial" both end a scan and one of them is something he did on purpose.
/// </remarks>
public enum ScanStopCause
{
    /// <summary>It has not been started.</summary>
    NotStarted,

    /// <summary>It is running.</summary>
    Running,

    /// <summary>Every enabled segment was covered.</summary>
    Finished,

    /// <summary>The operator pressed stop.</summary>
    OperatorStopped,

    /// <summary>It found somebody and stayed there.</summary>
    StoppedOnSomebody,

    /// <summary>
    /// Hamlet does not know enough about the radio yet.
    /// </summary>
    /// <remarks>
    /// The <see cref="RigStateMonitor.Populated"/> gate, added after three
    /// separate faults raced the same poll sweep (HM-DEC-094). A scan that
    /// starts before the first sweep has answered is deciding from defaults.
    /// </remarks>
    RigStateNotPopulated,

    /// <summary>The radio is transmitting, or a transmit path is armed.</summary>
    Transmitting,

    /// <summary>The operator turned the dial.</summary>
    /// <remarks>
    /// **HE WINS, INSTANTLY AND WITHOUT ARGUMENT.** Someone reaching for the VFO
    /// while a scan is running is telling Hamlet to get out of the way, and the
    /// scan does not finish its dwell first (§0.2.1).
    /// </remarks>
    DialTouched,

    /// <summary>What Hamlet knows about the radio has gone stale or unknown.</summary>
    RigStateUnusable,

    /// <summary>The link stopped answering.</summary>
    /// <remarks>
    /// **SILENCE IS A STOP** (§0.2.1). A read that does not come back leaves
    /// Hamlet unable to say where the dial is, and a scanner that goes on tuning
    /// in that state is moving a dial it cannot see.
    /// </remarks>
    LinkSilent,

    /// <summary>Something asked for a frequency outside the configured segments.</summary>
    OutsideSegments,

    /// <summary>There was nowhere configured to scan.</summary>
    NothingConfigured,
}

/// <summary>
/// Where the dial was before a scan moved it (HM-DEC-107, §0.2.1).
/// </summary>
/// <remarks>
/// **THE CRASH-SAFE HALF IS THE REASON THIS IS AN INTERFACE AND NOT A FIELD.**
/// Restoring on a clean stop is easy and covers every case except the one that
/// matters: the app going away mid-scan leaves the operator's radio parked
/// somewhere he never chose, with nothing on screen to say why. So the frequency
/// is written down before the first tune and read back on the next connect.
/// </remarks>
public interface IScanHome
{
    /// <summary>Write down where the dial was, before moving it.</summary>
    void Remember(long frequencyHz);

    /// <summary>Forget it, once the dial is safely back.</summary>
    void Clear();

    /// <summary>
    /// A frequency left over from a scan that never finished, or null.
    /// </summary>
    long? Pending { get; }
}

/// <summary>What a scan came to.</summary>
/// <param name="Cause">Why it is not running.</param>
/// <param name="Dwells">Everywhere it listened, in order.</param>
/// <param name="Stopped">Where it stopped, if it found somebody.</param>
/// <param name="HomeHz">Where the dial was before it started, if known.</param>
/// <param name="HomeRestored">Whether the dial got back there.</param>
public sealed record ScanOutcome(
    ScanStopCause Cause,
    IReadOnlyList<ScanDwell> Dwells,
    ScanDwell? Stopped,
    long? HomeHz,
    bool HomeRestored)
{
    /// <summary>What happened, in the app's voice (§0.7).</summary>
    public string Sentence => Cause switch
    {
        ScanStopCause.StoppedOnSomebody when Stopped is not null
            => $"the scan stopped at {Stopped.FrequencyHz / 1_000_000.0:0.000} MHz "
               + $"because {Stopped.Verdict.Sentence}",
        ScanStopCause.Finished
            => $"the scan listened at {Dwells.Count} places and found nobody, so "
               + "the dial is back where you left it",
        ScanStopCause.OperatorStopped
            => "you stopped the scan, and the dial is back where you left it",
        ScanStopCause.DialTouched
            => "you turned the dial, so the scan got out of the way and left it "
               + "where you put it",
        ScanStopCause.Transmitting
            => "the radio was transmitting, and a scan never moves the dial while "
               + "anything is going out",
        ScanStopCause.RigStateNotPopulated
            => "Hamlet has not heard back from the radio about enough of its "
               + "state yet, and a scan that starts before then is working from "
               + "guesses rather than from the radio",
        ScanStopCause.RigStateUnusable
            => "what Hamlet knows about the radio went stale, so it stopped "
               + "moving a dial it could no longer see",
        ScanStopCause.LinkSilent
            => "the radio stopped answering, and the scan stops the moment it "
               + "does, because silence is a stop",
        ScanStopCause.OutsideSegments
            => "something asked the scan to go outside the stretch you "
               + "configured, so it did not go",
        ScanStopCause.NothingConfigured
            => "there is nowhere for the scan to go: every segment in your scan "
               + "file is switched off",
        ScanStopCause.Running => "the scan is running",
        _ => "the scan has not been started",
    };
}

/// <summary>
/// Points the radio at each candidate in turn, inside a fence the operator set
/// (HM-DEC-107 phase 8, §0.2.1).
/// </summary>
/// <remarks>
/// <para>**THIS CLASS MOVES SOMEBODY ELSE'S DIAL, WHICH IS A DIFFERENT CATEGORY
/// OF ACT FROM READING THE RADIO.** It does it repeatedly and unasked, so every
/// rule in §0.2.1 is checked here rather than at the call sites: before the
/// scan starts, before every single tune, and after every dwell. A guard that
/// runs once at the start is a guard against the state at the start.</para>
/// <para>**IT NEVER TRANSMITS AND HAS NO WAY TO** (§0.2). Nothing in it reaches
/// <see cref="IRig.SendCwAsync"/>, and the only rig calls it makes are reading
/// the frequency and setting it.</para>
/// <para>Delay is injected, so a test runs a twenty second dwell in no time and
/// the same code runs both (§5).</para>
/// </remarks>
public sealed class BandScanner
{
    /// <summary>How old a frequency reading may be and still be acted on.</summary>
    /// <remarks>
    /// Four seconds. The dial is polled several times a second, so a reading
    /// this old means the poll loop has stalled, and tuning on it would be
    /// moving a dial from a picture rather than from the radio.
    /// </remarks>
    public static readonly TimeSpan FreshEnough = TimeSpan.FromSeconds(4);

    /// <summary>
    /// How far the dial may sit from where the scan put it before that counts as
    /// the operator having touched it.
    /// </summary>
    /// <remarks>
    /// A hundred hertz. The radio reports what it rounded to, and a CW signal is
    /// tuned to within a few tens of hertz, so anything wider than this is a
    /// hand on the knob rather than arithmetic.
    /// </remarks>
    public const long DialTouchedHz = 100;

    private readonly IRig _rig;
    private readonly RigStateMonitor _monitor;
    private readonly IScanHome _home;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;
    private readonly Func<DateTime> _utcNow;
    private readonly object _gate = new();

    private long _placedAtHz;
    private bool _placed;
    private volatile bool _dialTouched;

    /// <summary>Creates a scanner.</summary>
    /// <param name="rig">The radio.</param>
    /// <param name="monitor">What Hamlet knows about it.</param>
    /// <param name="home">Where the dial was, including across a crash.</param>
    /// <param name="delay">How to wait. Injected so tests are instant (§5).</param>
    /// <param name="utcNow">The clock, for staleness only.</param>
    public BandScanner(
        IRig rig,
        RigStateMonitor monitor,
        IScanHome home,
        Func<TimeSpan, CancellationToken, Task>? delay = null,
        Func<DateTime>? utcNow = null)
    {
        _rig = rig;
        _monitor = monitor;
        _home = home;
        _delay = delay ?? Task.Delay;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>True while a scan is moving the dial.</summary>
    public bool IsScanning { get; private set; }

    /// <summary>Why it is not running.</summary>
    public ScanStopCause Cause { get; private set; } = ScanStopCause.NotStarted;

    /// <summary>Raised as each dwell finishes, so a surface can show progress.</summary>
    public event EventHandler<ScanDwell>? DwellFinished;

    /// <summary>
    /// Stop, now.
    /// </summary>
    /// <remarks>
    /// **THE ALWAYS-VISIBLE STOP CONTROL CALLS THIS AND NOTHING ELSE** (§0.2.1).
    /// It sets a flag and returns; it awaits nothing, so it cannot queue behind
    /// the tune it is stopping. The dial goes home on the way out of the loop.
    /// </remarks>
    public void Stop()
    {
        lock (_gate)
        {
            _stopAsked = true;
        }

        _cancel?.Cancel();
    }

    private bool _stopAsked;
    private CancellationTokenSource? _cancel;

    /// <summary>
    /// Put the dial back after a scan that never finished.
    /// </summary>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The frequency restored, or null if there was nothing to restore.</returns>
    /// <remarks>
    /// **THE CRASH-SAFE PATH, CALLED ON CONNECT** (§0.2.1). If Hamlet went away
    /// mid-scan the operator's radio is parked somewhere he never chose, and the
    /// only moment Hamlet can put it right is the next time it can reach the
    /// radio at all.
    /// </remarks>
    public async Task<long?> RestoreHomeAsync(CancellationToken cancellationToken = default)
    {
        if (_home.Pending is not { } hz)
        {
            return null;
        }

        try
        {
            await _rig.SetFrequencyHzAsync(hz, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            // The radio is not answering. The note stays on disk, so the next
            // connect tries again rather than the frequency being lost.
            return null;
        }

        _home.Clear();

        return hz;
    }

    /// <summary>
    /// Run a scan over the given candidates.
    /// </summary>
    /// <param name="candidates">Where to listen, best first.</param>
    /// <param name="segments">The fence. Nothing outside it is visited.</param>
    /// <param name="listen">
    /// How to listen at one place. Given the frequency and how long, it returns
    /// what the decoder made of it.
    /// </param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>What the scan came to. Never throws.</returns>
    public async Task<ScanOutcome> RunAsync(
        IReadOnlyList<long> candidates,
        ScanSegments segments,
        Func<long, double, CancellationToken, Task<ScanDwell>> listen,
        CancellationToken cancellationToken = default)
    {
        var dwells = new List<ScanDwell>();

        lock (_gate)
        {
            _stopAsked = false;
        }

        _dialTouched = false;
        _placed = false;

        // THE START GATE. Everything here is checked before the dial has moved
        // at all, so a refusal costs the operator nothing (§0.2.1).
        if (!_monitor.IsPopulated)
        {
            return Refuse(ScanStopCause.RigStateNotPopulated, dwells);
        }

        var fence = segments.Enabled;

        if (fence.Count == 0)
        {
            return Refuse(ScanStopCause.NothingConfigured, dwells);
        }

        if (Blocked() is { } blocked)
        {
            return Refuse(blocked, dwells);
        }

        long home;

        try
        {
            home = await _rig.GetFrequencyHzAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return Refuse(ScanStopCause.LinkSilent, dwells);
        }

        // WRITTEN DOWN BEFORE THE FIRST TUNE, not after it. The window this
        // closes is small and it is exactly the window a crash lands in.
        _home.Remember(home);

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _cancel = linked;
        IsScanning = true;
        Cause = ScanStopCause.Running;

        _rig.FrequencyChanged += OnFrequencyChanged;

        try
        {
            foreach (var candidate in candidates)
            {
                if (StopRequested())
                {
                    return await GoHome(ScanStopCause.OperatorStopped, dwells, home, null)
                        .ConfigureAwait(false);
                }

                if (_dialTouched)
                {
                    // HE MOVED IT, SO IT IS WHERE HE WANTS IT. Putting the dial
                    // back would take the radio away from him a second time.
                    return Leave(ScanStopCause.DialTouched, dwells, home);
                }

                if (Blocked() is { } during)
                {
                    return await GoHome(during, dwells, home, null).ConfigureAwait(false);
                }

                if (!fence.Any(s => s.Contains(candidate)))
                {
                    // Not fatal: this candidate is simply not somewhere the
                    // operator said the scan may go, so it is skipped and the
                    // scan carries on with the ones that are.
                    continue;
                }

                ScanDwell dwell;

                try
                {
                    await _rig.SetFrequencyHzAsync(candidate, linked.Token).ConfigureAwait(false);

                    _placedAtHz = candidate;
                    _placed = true;

                    dwell = await listen(candidate, ScanDwell.LongestSeconds, linked.Token)
                        .ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return await GoHome(
                        _dialTouched ? ScanStopCause.DialTouched : ScanStopCause.OperatorStopped,
                        dwells, home, null).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    return await GoHome(ScanStopCause.LinkSilent, dwells, home, null)
                        .ConfigureAwait(false);
                }

                dwells.Add(dwell);
                DwellFinished?.Invoke(this, dwell);

                if (dwell.Decide(dwell.Seconds) == DwellAction.Stay)
                {
                    // FOUND SOMEBODY, SO THE DIAL STAYS. The note is cleared,
                    // because the operator is now where he wants to be and a
                    // later connect must not drag him away from it.
                    _home.Clear();
                    IsScanning = false;
                    Cause = ScanStopCause.StoppedOnSomebody;

                    return new ScanOutcome(
                        ScanStopCause.StoppedOnSomebody, dwells, dwell, home, false);
                }
            }

            return await GoHome(ScanStopCause.Finished, dwells, home, null)
                .ConfigureAwait(false);
        }
        finally
        {
            _rig.FrequencyChanged -= OnFrequencyChanged;
            _cancel = null;
            IsScanning = false;
        }
    }

    private bool StopRequested()
    {
        lock (_gate)
        {
            return _stopAsked;
        }
    }

    /// <summary>
    /// Everything that forbids moving the dial right now, or null.
    /// </summary>
    /// <remarks>
    /// **RUN BEFORE EVERY TUNE AND NOT ONCE AT THE START.** The radio can start
    /// transmitting, and the poll loop can stall, at any point during a scan
    /// that runs for minutes.
    /// </remarks>
    private ScanStopCause? Blocked()
    {
        var state = _monitor.State;

        if (state.IsTransmitting)
        {
            return ScanStopCause.Transmitting;
        }

        var frequency = state[RigField.Frequency];

        if (!frequency.IsKnown)
        {
            return ScanStopCause.RigStateUnusable;
        }

        return frequency.IsStale(_utcNow(), FreshEnough)
            ? ScanStopCause.RigStateUnusable
            : null;
    }

    private void OnFrequencyChanged(object? sender, FrequencyChangedEventArgs e)
    {
        if (!_placed)
        {
            return;
        }

        if (Math.Abs(e.FrequencyHz - _placedAtHz) <= DialTouchedHz)
        {
            return;
        }

        _dialTouched = true;
        _cancel?.Cancel();
    }

    private ScanOutcome Refuse(ScanStopCause cause, List<ScanDwell> dwells)
    {
        IsScanning = false;
        Cause = cause;

        return new ScanOutcome(cause, dwells, null, null, false);
    }

    /// <summary>The dial is already where the operator wants it, so leave it.</summary>
    private ScanOutcome Leave(ScanStopCause cause, List<ScanDwell> dwells, long home)
    {
        _home.Clear();
        IsScanning = false;
        Cause = cause;

        return new ScanOutcome(cause, dwells, null, home, false);
    }

    private async Task<ScanOutcome> GoHome(
        ScanStopCause cause, List<ScanDwell> dwells, long home, ScanDwell? stopped)
    {
        var restored = false;

        try
        {
            // **NOT ON THE LINKED TOKEN.** That token is very often the reason
            // this is being called, and a restore cancelled by the thing that
            // stopped the scan leaves the dial exactly where §0.2.1 says it
            // must not be left.
            await _rig.SetFrequencyHzAsync(home, CancellationToken.None)
                .ConfigureAwait(false);

            restored = true;
            _home.Clear();
        }
        catch (Exception)
        {
            // The note stays on disk. The next connect puts the dial back.
        }

        IsScanning = false;
        Cause = cause;

        return new ScanOutcome(cause, dwells, stopped, home, restored);
    }
}

/// <summary>
/// Remembers the dial's home in a small file beside the settings
/// (HM-DEC-107, §0.2.1).
/// </summary>
/// <remarks>
/// **A FILE RATHER THAN A SETTING, BECAUSE OF WHEN IT IS WRITTEN.** It is
/// written in the moment before the first tune and deleted the moment the dial
/// is back, so it exists only while a scan is in flight. Settings are saved on a
/// clean exit, which is the one exit this has to survive.
/// </remarks>
public sealed class FileScanHome : IScanHome
{
    private readonly string _path;

    /// <summary>Creates the store.</summary>
    /// <param name="path">Where to keep the note.</param>
    public FileScanHome(string path) => _path = path;

    /// <inheritdoc />
    public long? Pending
    {
        get
        {
            try
            {
                if (!File.Exists(_path))
                {
                    return null;
                }

                var text = File.ReadAllText(_path).Trim();

                return long.TryParse(text, out var hz) && hz > 0 ? hz : null;
            }
            catch (Exception)
            {
                // Never-throw discipline (§8): a note that cannot be read is a
                // note there is not, and it must not take the app down with it.
                return null;
            }
        }
    }

    /// <inheritdoc />
    public void Remember(long frequencyHz)
    {
        try
        {
            var folder = Path.GetDirectoryName(_path);

            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(_path, frequencyHz.ToString(
                System.Globalization.CultureInfo.InvariantCulture));
        }
        catch (Exception)
        {
            // Dropped and not propagated (§8).
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        try
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
        catch (Exception)
        {
            // Dropped and not propagated (§8).
        }
    }
}

using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// The envelope around a scanner that moves somebody else's dial
/// (HM-DEC-107 phase 8, §0.2.1).
/// </summary>
/// <remarks>
/// <para>**EVERY ABORT IS SIMULATED HERE RATHER THAN REASONED ABOUT.** The brief
/// is explicit about that, and it is the right instinct: a safety rule that has
/// only ever been argued for is a safety rule nobody has run.</para>
/// <para>**AND NOTHING IN THIS FILE TRANSMITS.** The scanner has no path to the
/// transmitter at all, which is proved below rather than asserted (§0.2).</para>
/// </remarks>
public sealed class BandScannerSafetyTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the outcomes are printed.</param>
    public BandScannerSafetyTests(ITestOutputHelper output) => _output = output;

    private const long Home = 7_040_000;

    private static readonly ScanSegments Fence = ScanSegments.Parse(
        """
        {
          "schema": "hamlet.scan-segments/1",
          "segments": [
            { "band": "40 m", "name": "the Morse end", "lowHz": 7000000,
              "highHz": 7050000, "cite": "test fixture, not a citation" }
          ]
        }
        """,
        "a test fixture");

    private static readonly long[] Candidates =
    {
        7_010_000, 7_020_000, 7_030_000,
    };

    /// <summary>A dwell that heard nothing, so the scan keeps going.</summary>
    private static Task<ScanDwell> HeardNothing(
        long hz, double seconds, CancellationToken token)
        => Task.FromResult(new ScanDwell(hz, seconds));

    /// <summary>A dwell that heard somebody calling.</summary>
    private static Task<ScanDwell> HeardACall(
        long hz, double seconds, CancellationToken token)
    {
        var dwell = new ScanDwell(hz, seconds);

        foreach (var c in "CQ DE W1AW")
        {
            dwell.Take(c == ' '
                ? new CwCharacter(
                    MorseAlphabet.WordGap, CwConfidence.High, 1, "", 20, 18,
                    TimeSpan.Zero)
                : new CwCharacter(
                    c.ToString(), CwConfidence.High, 0.95, ".-", 20, 18,
                    TimeSpan.Zero));
        }

        return Task.FromResult(dwell);
    }

    private static async Task<(ScanTestRig Rig, RigStateMonitor Monitor)> Ready()
    {
        var rig = new ScanTestRig(Home);
        var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        monitor.Start();

        await monitor.Populated.WaitAsync(TimeSpan.FromSeconds(5));

        monitor.Stop();

        return (rig, monitor);
    }

    /// <remarks>
    /// <para>Proves §0.2.1: **the dial goes back when the scan finds nobody.**
    /// A scanner that leaves the radio parked wherever it gave up has taken the
    /// operator's frequency away from him for nothing.</para>
    /// </remarks>
    [Fact]
    public async Task AScanThatFoundNobodyPutsTheDialBack()
    {
        var (rig, monitor) = await Ready();
        using var _ = monitor;

        var home = new MemoryScanHome();
        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        var outcome = await scanner.RunAsync(Candidates, Fence, HeardNothing);

        _output.WriteLine(outcome.Sentence);
        _output.WriteLine($"tuned to: {string.Join(", ", rig.Tuned)}");

        Assert.Equal(ScanStopCause.Finished, outcome.Cause);
        Assert.True(outcome.HomeRestored);
        Assert.Equal(Home, rig.FrequencyHz);

        // And the crash note is gone, so the next connect does not drag him back.
        Assert.Null(home.Pending);
    }

    /// <remarks>
    /// Proves HM-DEC-107 phase 7 and 8 together: **finding somebody is the one
    /// case where the dial stays**, and the note is cleared so a later connect
    /// cannot pull the operator off the station he just found.
    /// </remarks>
    [Fact]
    public async Task AScanThatFoundSomebodyStaysThere()
    {
        var (rig, monitor) = await Ready();
        using var _ = monitor;

        var home = new MemoryScanHome();
        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        var outcome = await scanner.RunAsync(Candidates, Fence, HeardACall);

        _output.WriteLine(outcome.Sentence);

        Assert.Equal(ScanStopCause.StoppedOnSomebody, outcome.Cause);
        Assert.Equal(Candidates[0], rig.FrequencyHz);
        Assert.Null(home.Pending);
    }

    /// <remarks>
    /// <para>Proves §0.2.1: **the operator touching the dial ends the scan
    /// instantly, and the dial is left where he put it.** Putting it back would
    /// take the radio away from him a second time, which is the opposite of
    /// what the abort is for.</para>
    /// </remarks>
    [Fact]
    public async Task TouchingTheDialEndsTheScanAndLeavesItWhereHePutIt()
    {
        var (rig, monitor) = await Ready();
        using var _ = monitor;

        var home = new MemoryScanHome();
        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        // He reaches for the knob during the first dwell.
        var outcome = await scanner.RunAsync(
            Candidates, Fence,
            (hz, seconds, token) =>
            {
                rig.OperatorTunesTo(7_012_345);
                return HeardNothing(hz, seconds, token);
            });

        _output.WriteLine(outcome.Sentence);

        Assert.Equal(ScanStopCause.DialTouched, outcome.Cause);
        Assert.False(outcome.HomeRestored);
        Assert.Equal(7_012_345, rig.FrequencyHz);
        Assert.Null(home.Pending);
    }

    /// <remarks>
    /// Proves §0.2 and §0.2.1: **a scan never moves the dial while anything is
    /// going out.** The check runs before every tune and not once at the start,
    /// because a radio can start transmitting at any point in a scan that runs
    /// for minutes.
    /// </remarks>
    [Fact]
    public async Task AScanRefusesToStartWhileTheRadioIsTransmitting()
    {
        var (rig, monitor) = await Ready();
        using var _ = monitor;

        rig.Transmitting = true;

        await monitor.RefreshAsync(RigField.TransmitStatus);

        var home = new MemoryScanHome();
        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        var outcome = await scanner.RunAsync(Candidates, Fence, HeardNothing);

        _output.WriteLine(outcome.Sentence);

        Assert.Equal(ScanStopCause.Transmitting, outcome.Cause);

        // AND THE DIAL NEVER MOVED AT ALL. A refusal before the first tune costs
        // the operator nothing, which is why the gate is where it is.
        Assert.Empty(rig.Tuned);
        Assert.Null(home.Pending);
    }

    /// <remarks>
    /// <para>Proves §0.2.1's <c>Populated</c> gate (HM-DEC-094): **a scan will
    /// not start before the radio has answered anything.** One that did would be
    /// deciding from defaults, which is the fault three separate races produced
    /// before the gate existed.</para>
    /// </remarks>
    [Fact]
    public async Task AScanRefusesBeforeTheRadioHasAnsweredAnything()
    {
        var rig = new ScanTestRig(Home);
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        // Deliberately never started, so nothing has been read.
        Assert.False(monitor.IsPopulated);

        var home = new MemoryScanHome();
        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        var outcome = await scanner.RunAsync(Candidates, Fence, HeardNothing);

        _output.WriteLine(outcome.Sentence);

        Assert.Equal(ScanStopCause.RigStateNotPopulated, outcome.Cause);
        Assert.Empty(rig.Tuned);
    }

    /// <remarks>
    /// <para>Proves §0.2.1: **silence is a stop.** A read that does not come
    /// back leaves Hamlet unable to say where the dial is, and a scanner that
    /// goes on tuning then is moving a dial it cannot see.</para>
    /// </remarks>
    [Fact]
    public async Task AScanStopsWhenTheLinkGoesSilent()
    {
        var (rig, monitor) = await Ready();
        using var _ = monitor;

        var home = new MemoryScanHome();
        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        var outcome = await scanner.RunAsync(
            Candidates, Fence,
            (hz, seconds, token) =>
            {
                rig.LinkIsSilent = true;
                return HeardNothing(hz, seconds, token);
            });

        _output.WriteLine(outcome.Sentence);

        Assert.Equal(ScanStopCause.LinkSilent, outcome.Cause);

        // **AND THE NOTE SURVIVES.** The restore could not be made either, so
        // the frequency has to still be on disk for the next connect to use.
        Assert.False(outcome.HomeRestored);
        Assert.Equal(Home, home.Pending);
    }

    /// <remarks>
    /// Proves §0.2.1: **a reading Hamlet cannot trust stops the scan.** The dial
    /// is polled several times a second, so a stale reading means the poll loop
    /// has stalled, and tuning on it is moving a dial from a picture rather than
    /// from the radio.
    /// </remarks>
    [Fact]
    public async Task AScanStopsWhenWhatHamletKnowsHasGoneStale()
    {
        var (rig, monitor) = await Ready();
        using var _ = monitor;

        var home = new MemoryScanHome();

        // The clock jumps well past the freshness window while nothing new has
        // been read, which is exactly what a stalled poll loop looks like.
        var scanner = new BandScanner(
            rig, monitor, home,
            utcNow: () => rig.Now + BandScanner.FreshEnough + TimeSpan.FromSeconds(1));

        var outcome = await scanner.RunAsync(Candidates, Fence, HeardNothing);

        _output.WriteLine(outcome.Sentence);

        Assert.Equal(ScanStopCause.RigStateUnusable, outcome.Cause);
        Assert.Empty(rig.Tuned);
    }

    /// <remarks>
    /// <para>Proves §0.2.1: **the fence is checked every time the dial moves.**
    /// A candidate outside the operator's configured stretch is skipped and the
    /// scan carries on with the ones inside it, because one bad suggestion is
    /// not a reason to abandon the band.</para>
    /// </remarks>
    [Fact]
    public async Task NothingOutsideTheOperatorsOwnFileIsEverVisited()
    {
        var (rig, monitor) = await Ready();
        using var _ = monitor;

        var home = new MemoryScanHome();
        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        var outside = new[] { 14_030_000L, 7_020_000L, 3_530_000L };

        var outcome = await scanner.RunAsync(outside, Fence, HeardNothing);

        _output.WriteLine($"tuned to: {string.Join(", ", rig.Tuned)}");

        // Only the one inside the fence, and the trip home.
        Assert.Equal(new[] { 7_020_000L, Home }, rig.Tuned);
        Assert.Equal(ScanStopCause.Finished, outcome.Cause);
    }

    /// <remarks>
    /// Proves §0.2.1: **one obvious stop, and it awaits nothing.** A stop that
    /// queued behind the tune it is stopping is not a stop.
    /// </remarks>
    [Fact]
    public async Task TheStopControlEndsItAndPutsTheDialBack()
    {
        var (rig, monitor) = await Ready();
        using var _ = monitor;

        var home = new MemoryScanHome();
        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        var outcome = await scanner.RunAsync(
            Candidates, Fence,
            (hz, seconds, token) =>
            {
                scanner.Stop();
                return HeardNothing(hz, seconds, token);
            });

        _output.WriteLine(outcome.Sentence);

        Assert.Equal(ScanStopCause.OperatorStopped, outcome.Cause);
        Assert.True(outcome.HomeRestored);
        Assert.Equal(Home, rig.FrequencyHz);
        Assert.Null(home.Pending);
    }

    /// <remarks>
    /// <para>Proves §0.2.1's crash-safe half, which is the reason the note is a
    /// file rather than a field: **the app going away mid-scan leaves the
    /// operator's radio parked somewhere he never chose**, and the only moment
    /// Hamlet can put it right is the next time it can reach the radio at
    /// all.</para>
    /// </remarks>
    [Fact]
    public async Task AScanThatNeverFinishedPutsTheDialBackOnTheNextConnect()
    {
        var rig = new ScanTestRig(7_019_000);
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        // Written by the session that went away, and still on disk.
        var home = new MemoryScanHome();
        home.Remember(Home);

        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        var restored = await scanner.RestoreHomeAsync();

        _output.WriteLine($"restored to {restored}");

        Assert.Equal(Home, restored);
        Assert.Equal(Home, rig.FrequencyHz);
        Assert.Null(home.Pending);
    }

    /// <remarks>
    /// Proves §0.2.1: **a restore that could not be made keeps the note**, so
    /// the connect after this one tries again rather than the frequency being
    /// quietly lost.
    /// </remarks>
    [Fact]
    public async Task ARestoreTheRadioRefusedKeepsTheNote()
    {
        var rig = new ScanTestRig(7_019_000) { LinkIsSilent = true };
        using var monitor = new RigStateMonitor(rig, (_, _) => Task.CompletedTask);

        var home = new MemoryScanHome();
        home.Remember(Home);

        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        Assert.Null(await scanner.RestoreHomeAsync());
        Assert.Equal(Home, home.Pending);
    }

    /// <remarks>
    /// <para>Proves §0.2: **a scan never transmits, and there is no path by
    /// which it could.** The test rig fails loudly on any keying call, so this
    /// holds against every route through the scanner rather than against the
    /// one an argument happened to consider.</para>
    /// </remarks>
    [Fact]
    public async Task AScanNeverKeysTheTransmitter()
    {
        var (rig, monitor) = await Ready();
        using var _ = monitor;

        var home = new MemoryScanHome();
        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        await scanner.RunAsync(Candidates, Fence, HeardNothing);
        await scanner.RunAsync(Candidates, Fence, HeardACall);

        Assert.Equal(0, rig.KeyingAttempts);
    }

    /// <remarks>
    /// Proves §0.2.1: a scan with nowhere configured to go says so, rather than
    /// falling back on somewhere the operator did not choose.
    /// </remarks>
    [Fact]
    public async Task AScanWithEveryStretchSwitchedOffSaysSoAndGoesNowhere()
    {
        var (rig, monitor) = await Ready();
        using var _ = monitor;

        var off = ScanSegments.Parse(
            """
            {
              "segments": [
                { "band": "40 m", "name": "off", "lowHz": 7000000,
                  "highHz": 7050000, "cite": "test", "enabled": false }
              ]
            }
            """);

        var home = new MemoryScanHome();
        var scanner = new BandScanner(rig, monitor, home, utcNow: () => rig.Now);

        var outcome = await scanner.RunAsync(Candidates, off, HeardNothing);

        _output.WriteLine(outcome.Sentence);

        Assert.Equal(ScanStopCause.NothingConfigured, outcome.Cause);
        Assert.Empty(rig.Tuned);
    }

    /// <summary>The note, in memory, so a test does not touch the disk.</summary>
    private sealed class MemoryScanHome : IScanHome
    {
        public long? Pending { get; private set; }

        public void Remember(long frequencyHz) => Pending = frequencyHz;

        public void Clear() => Pending = null;
    }

    /// <summary>
    /// A radio that answers, remembers where it was told to go, and screams if
    /// anything tries to key it.
    /// </summary>
    /// <remarks>
    /// Hand-rolled rather than a mocking framework (§6).
    /// </remarks>
    private sealed class ScanTestRig : IRig
    {
        private readonly List<long> _tuned = new();

        public ScanTestRig(long frequencyHz) => FrequencyHz = frequencyHz;

        public long FrequencyHz { get; private set; }

        public IReadOnlyList<long> Tuned => _tuned;

        public bool Transmitting { get; set; }

        public bool LinkIsSilent { get; set; }

        public int KeyingAttempts { get; private set; }

        public DateTime Now { get; } = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

        public bool IsConnected => true;

        public bool IsSimulated => true;

        public RigCapabilities Capabilities { get; } = new(
            "Scan test radio", false, false, false, false, Array.Empty<string>());

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        /// <summary>A hand on the knob.</summary>
        public void OperatorTunesTo(long hz)
        {
            FrequencyHz = hz;
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(hz));
        }

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
            => LinkIsSilent
                ? Task.FromException<long>(new IOException("the radio stopped answering"))
                : Task.FromResult(FrequencyHz);

        public Task SetFrequencyHzAsync(
            long frequencyHz, CancellationToken cancellationToken = default)
        {
            if (LinkIsSilent)
            {
                return Task.FromException(new IOException("the radio stopped answering"));
            }

            _tuned.Add(frequencyHz);
            FrequencyHz = frequencyHz;

            return Task.CompletedTask;
        }

        /// <summary>
        /// **THE TRIPWIRE.** Nothing in a scan may reach this, ever (§0.2).
        /// </summary>
        public Task<bool> SendCwAsync(
            string message, CancellationToken cancellationToken = default)
        {
            KeyingAttempts++;
            return Task.FromResult(false);
        }

        /// <summary>Counted for the same reason as the send.</summary>
        public void AbortCw() => KeyingAttempts++;

        public Task<RigWriteResult> SetSettingAsync(
            CivWrite write, int value, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("scan test radio"));

        public Task<RigWriteResult> SetModeAsync(
            CivMode mode, bool dataMode, CancellationToken cancellationToken = default)
            => Task.FromResult(RigWriteResult.NotSupported("scan test radio"));

        public Task<IReadOnlyList<RigValue>> ReadAsync(
            RigField field, RigState context, CancellationToken cancellationToken = default)
        {
            if (LinkIsSilent)
            {
                return Task.FromResult<IReadOnlyList<RigValue>>(new[]
                {
                    RigValue.Unknown(field, "the radio stopped answering"),
                });
            }

            RigValue value = field switch
            {
                RigField.Frequency => RigValue.Known(
                    field, FrequencyHz, $"{FrequencyHz / 1_000_000.0:0.000} MHz",
                    Now, "scan test radio"),
                RigField.TransmitStatus => RigValue.Known(
                    field, Transmitting ? 1 : 0, Transmitting ? "transmitting" : "receiving",
                    Now, "scan test radio"),
                _ => RigValue.Known(field, 0, "0", Now, "scan test radio"),
            };

            return Task.FromResult<IReadOnlyList<RigValue>>(new[] { value });
        }

        /// <summary>Unused here, and present because the seam requires it.</summary>
        public void Volunteer(params RigValue[] values)
            => ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(values));
    }
}

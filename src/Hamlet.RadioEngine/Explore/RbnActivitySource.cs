namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// The Reverse Beacon Network: a worldwide net of automated receivers that
/// report every CW signal they decode. Roughly six spots a second, worldwide,
/// forever.
/// </summary>
/// <remarks>
/// <para>Connection: telnet to <c>telnet.reversebeacon.net</c> port 7000 for
/// the CW and RTTY feed, verified live on 2026-08-13. The server prompts
/// "Please enter your call:" and the operator's callsign is the whole login —
/// there is no password. That callsign goes over the wire because the service
/// requires it, and it still never enters Hamlet's telemetry (HM-DEC-024).</para>
/// <para>THE FIREHOSE. Twenty thousand spots an hour is not a feed, it is a
/// denial of service against a newcomer's attention, so what reaches the list
/// is filtered twice:</para>
/// <list type="bullet">
/// <item><b>Band.</b> Only the band the operator is looking at.</item>
/// <item><b>Continent.</b> Only skimmers on the operator's own continent. A
/// German skimmer hearing a German station says nothing about what is
/// audible from Pennsylvania.</item>
/// </list>
/// <para>Continent, and not the operator's own call district, is deliberate.
/// On HF a skimmer eight hundred kilometers away hears very nearly what you
/// hear, so filtering to adjacent districts would throw away good spots to
/// no purpose. District closeness is not discarded — it rides along on
/// <see cref="ActivitySpot.Proximity"/> and lifts a spot up the ranking
/// instead (HM-DEC-025). Filtering decides what is plausible; ranking decides
/// what is best.</para>
/// <para>Many skimmers hear the same station at once, so reports are collapsed
/// per station and frequency, keeping the strongest report and counting the
/// rest. That count is the honest signal a newcomer actually wants: twelve
/// receivers hearing something is the best available evidence that a
/// thirteenth — theirs — will too.</para>
/// <para>What reaches the map is not filtered by continent: the map shows
/// every in-band dot the source holds, because a band's shape is the point of
/// it. The filter is for the list.</para>
/// </remarks>
public sealed class RbnActivitySource
    : IContextualActivitySource, IBandScopedActivitySource, IDisposable
{
    /// <summary>The settings key and display name for this source.</summary>
    public const string SourceName = "RBN";

    /// <summary>Default cluster host.</summary>
    public const string DefaultHost = "telnet.reversebeacon.net";

    /// <summary>Default port: the CW and RTTY feed.</summary>
    public const int DefaultPort = 7000;

    /// <summary>How long a spot stays in the window. RBN is a live stream;
    /// beyond this a report is history, not "happening now".</summary>
    public static readonly TimeSpan RetentionWindow = TimeSpan.FromMinutes(20);

    /// <summary>Hard ceiling on retained reports, so memory and render cost
    /// stay bounded whatever the band is doing.</summary>
    public const int MaxRetainedSpots = 400;

    /// <summary>What the server prints when it wants the callsign.</summary>
    private const string LoginPrompt = "call";

    private readonly Func<ITextConnection> _connect;
    private readonly string _callsign;
    private readonly Func<DateTime> _utcNow;
    private readonly object _gate = new();
    private readonly List<RbnSpot> _window = new();

    private CancellationTokenSource? _life;
    private Task? _reader;
    private ActivityContext _context = new();
    private volatile bool _loggedIn;
    private int _consecutiveFailures;

    /// <summary>Creates the source against the real cluster.</summary>
    /// <param name="callsign">The operator's callsign — the telnet login.</param>
    /// <param name="host">Cluster host.</param>
    /// <param name="port">Cluster port.</param>
    /// <param name="utcNow">Clock, injected for testability.</param>
    public RbnActivitySource(
        string callsign,
        string host = DefaultHost,
        int port = DefaultPort,
        Func<DateTime>? utcNow = null)
        : this(() => new TcpTextConnection(host, port), callsign, utcNow)
    {
    }

    /// <summary>Creates the source over an injected connection.</summary>
    /// <param name="connect">Makes a fresh connection; called again on every
    /// reconnect.</param>
    /// <param name="callsign">The operator's callsign — the telnet login.</param>
    /// <param name="utcNow">Clock, injected for testability.</param>
    public RbnActivitySource(
        Func<ITextConnection> connect, string callsign, Func<DateTime>? utcNow = null)
    {
        _connect = connect;
        _callsign = (callsign ?? "").Trim().ToUpperInvariant();
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <inheritdoc/>
    public string Name => SourceName;

    /// <inheritdoc/>
    /// <remarks>
    /// Always the band on screen: the list this source returns is filtered to
    /// it, so RBN has nothing to say about any other band.
    /// </remarks>
    public string? ScopedBandName
        => string.IsNullOrWhiteSpace(_context.BandName) ? null : _context.BandName;

    /// <summary>True once the cluster has accepted the callsign.</summary>
    public bool IsLoggedIn => _loggedIn;

    /// <summary>Reports currently held, before band and continent filtering.</summary>
    public int RetainedCount
    {
        get
        {
            lock (_gate)
            {
                return _window.Count;
            }
        }
    }

    /// <summary>
    /// How many distinct skimmers have reported anybody in a frequency range,
    /// or null when the feed is not answering (HM-DEC-082).
    /// </summary>
    /// <param name="lowHz">Band lower edge.</param>
    /// <param name="highHz">Band upper edge.</param>
    /// <returns>A count, or null when it cannot be obtained.</returns>
    /// <remarks>
    /// <para>"NONE OF THEM COPIED YOU" IS WORTH NOTHING WITHOUT KNOWING HOW MANY
    /// "THEM" THERE WERE. Zero skimmers watching a band is not the same event as
    /// forty, and today both produced the same silence.</para>
    /// <para>IT IS A LOWER BOUND AND IT IS DESCRIBED AS ONE. A skimmer that
    /// heard nothing publishes nothing, so it cannot be counted, and this
    /// measures machines that reported somebody rather than machines that were
    /// listening. Calling it a count of who was listening would claim more than
    /// the wire supports.</para>
    /// <para>NULL WHEN THE FEED IS NOT ANSWERING, never zero. An absent number
    /// reads as zero to somebody who has been disappointed before, and those are
    /// opposite facts about the evening.</para>
    /// </remarks>
    public int? SkimmersReporting(long lowHz, long highHz)
    {
        if (!_loggedIn)
        {
            return null;
        }

        lock (_gate)
        {
            Prune();

            return _window
                .Where(s => s.FrequencyHz >= lowHz && s.FrequencyHz <= highHz)
                .Select(s => s.Spotter)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();
        }
    }

    /// <inheritdoc/>
    public void SetContext(ActivityContext context) => _context = context;

    /// <summary>
    /// Start the long-lived connection. Safe to call repeatedly; the second
    /// call does nothing.
    /// </summary>
    public void Start()
    {
        if (_life is not null)
        {
            return;
        }

        if (_callsign.Length == 0)
        {
            // RBN will not accept an anonymous login, and inventing a callsign
            // to get in would be lying to the service on the operator's
            // behalf.
            return;
        }

        _life = new CancellationTokenSource();
        _reader = Task.Run(() => RunAsync(_life.Token));
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ActivitySpot>> GetSpotsAsync(
        CancellationToken cancellationToken = default)
    {
        Start();
        return Task.FromResult(Snapshot(forMap: false));
    }

    /// <summary>
    /// Every in-band report the source holds, continent filtering skipped —
    /// what the neighborhood map draws.
    /// </summary>
    /// <returns>In-band spots, newest first.</returns>
    public IReadOnlyList<ActivitySpot> GetMapSpots() => Snapshot(forMap: true);

    /// <summary>
    /// Every spot line the feed produced, before any filtering (HM-DEC-075).
    /// </summary>
    /// <remarks>
    /// RAW ON PURPOSE. <see cref="GetSpotsAsync"/> keeps what is worth putting
    /// in front of somebody looking for a contact, which means it drops
    /// everything out of band and everything a skimmer too far away decoded.
    /// A report of the operator's own callsign is neither of those things: it
    /// is the answer to "did anybody hear me", and the further away the
    /// receiver the more it means. So it is taken here, before the list's own
    /// judgment is applied to it.
    /// </remarks>
    public event Action<RbnSpot>? SpotParsed;

    /// <summary>Feed one raw line in, as the reader loop does.</summary>
    /// <param name="line">A line from the cluster.</param>
    /// <returns>True when the line was a spot and was retained.</returns>
    /// <remarks>Exposed so tests can drive the window without a socket.</remarks>
    internal bool Accept(string? line)
    {
        var spot = RbnSpotLine.Parse(line, _utcNow());
        if (spot is null)
        {
            return false;
        }

        lock (_gate)
        {
            _window.Add(spot);
            Prune();
        }

        // Outside the lock: a handler that took a moment must not hold the
        // reader loop, and a handler that threw must not take the feed down
        // with it (§8).
        try
        {
            SpotParsed?.Invoke(spot);
        }
        catch (Exception)
        {
            // A listener's problem is never the feed's problem.
        }

        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            _life?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Already torn down.
        }

        _life?.Dispose();
        _life = null;
        _reader = null;
        _loggedIn = false;
    }

    private IReadOnlyList<ActivitySpot> Snapshot(bool forMap)
    {
        var now = _utcNow();
        List<RbnSpot> live;

        lock (_gate)
        {
            Prune();
            live = _window.ToList();
        }

        // Collapse the many skimmers that heard one station into one spot,
        // keeping the strongest report and counting the rest.
        var best = new Dictionary<string, (RbnSpot Spot, int Count, SpotProximity Near)>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var s in live)
        {
            if (!_context.IsInBand(s.FrequencyHz))
            {
                continue;
            }

            var proximity = CallsignRegions.ProximityTo(s.Spotter, _context.HomeDistrict);

            if (!forMap && !IsPlausiblyAudible(proximity))
            {
                continue;
            }

            var key = $"{s.DxCall.ToUpperInvariant()}|{s.FrequencyHz / 200}";

            if (best.TryGetValue(key, out var held))
            {
                var better = Stronger(s, held.Spot);
                best[key] = (better, held.Count + 1, Closest(held.Near, proximity));
            }
            else
            {
                best[key] = (s, 1, proximity);
            }
        }

        var spots = best.Values
            .Select(e => ToSpot(e.Spot, e.Count, e.Near, now))
            .OrderByDescending(s => s.HeardAtUtc)
            .ToList();

        return spots;
    }

    /// <summary>
    /// Whether a skimmer is close enough for its report to mean anything
    /// here. Continent grain — see the type remarks for why not district.
    /// </summary>
    private bool IsPlausiblyAudible(SpotProximity proximity)
    {
        if (proximity is SpotProximity.Local or SpotProximity.Continent)
        {
            return true;
        }

        // With no idea where the operator is, continent filtering has no
        // anchor, so nothing is filtered out on that basis and the ranking
        // sorts it out instead.
        return !_context.HomeInNorthAmerica && _context.HomeDistrict is null;
    }

    private ActivitySpot ToSpot(
        RbnSpot s, int reports, SpotProximity proximity, DateTime now)
    {
        return new ActivitySpot(
            BuildStory(s, reports),
            s.FrequencyHz,
            s.Mode,
            SourceName,
            s.HeardAtUtc > now ? now : s.HeardAtUtc,
            s.Wpm)
        {
            CallType = s.CallType,
            SignalDb = s.SignalDb,
            DxCall = s.DxCall,
            SpotterCall = s.Spotter,
            Proximity = proximity,
            ReportCount = reports,
        };
    }

    private static string BuildStory(RbnSpot s, int reports)
    {
        var what = s.CallType switch
        {
            SpotCallType.Cq => $"{s.DxCall} is calling CQ",
            SpotCallType.Beacon => $"{s.DxCall} is a beacon, transmitting to nobody",
            SpotCallType.Contest => $"{s.DxCall} is working a contest run",
            _ => $"{s.DxCall} is on the air",
        };

        var speed = s.Wpm is not null ? $" at {s.Wpm} WPM" : "";
        var heard = reports > 1
            ? $", and {reports} receivers hear it"
            : $", heard by {s.Spotter}";

        return what + speed + heard;
    }

    private static RbnSpot Stronger(RbnSpot a, RbnSpot b)
        => (a.SignalDb ?? int.MinValue) >= (b.SignalDb ?? int.MinValue) ? a : b;

    private static SpotProximity Closest(SpotProximity a, SpotProximity b)
    {
        static int Rank(SpotProximity p) => p switch
        {
            SpotProximity.Local => 3,
            SpotProximity.Continent => 2,
            SpotProximity.Distant => 1,
            _ => 0,
        };

        return Rank(a) >= Rank(b) ? a : b;
    }

    /// <summary>Drop what has aged out, then enforce the hard cap.</summary>
    private void Prune()
    {
        var cutoff = _utcNow() - RetentionWindow;
        _window.RemoveAll(s => s.HeardAtUtc < cutoff);

        if (_window.Count <= MaxRetainedSpots)
        {
            return;
        }

        _window.Sort((a, b) => a.HeardAtUtc.CompareTo(b.HeardAtUtc));
        _window.RemoveRange(0, _window.Count - MaxRetainedSpots);
    }

    /// <summary>
    /// Stay connected: log in, read until the stream ends or faults, then
    /// wait out the shared backoff and try again.
    /// </summary>
    private async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            ITextConnection? connection = null;

            try
            {
                connection = _connect();
                await connection.ConnectAsync(token).ConfigureAwait(false);
                await LogInAsync(connection, token).ConfigureAwait(false);
                _consecutiveFailures = 0;

                while (!token.IsCancellationRequested)
                {
                    var line = await connection.ReadLineAsync(token).ConfigureAwait(false);
                    if (line is null)
                    {
                        break;
                    }

                    Accept(line);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception)
            {
                // Any network fault is a reconnect, never a crash: this runs
                // on a background task with nobody to catch it (§8).
                _consecutiveFailures++;
            }
            finally
            {
                _loggedIn = false;
                connection?.Dispose();
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            _consecutiveFailures = Math.Max(1, _consecutiveFailures);
            try
            {
                await Task.Delay(SourceBackoff.Delay(_consecutiveFailures), token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task LogInAsync(ITextConnection connection, CancellationToken token)
    {
        // The prompt arrives without a newline; the connection hands back a
        // partial line when the stream goes quiet, which is what lets this
        // spot it at all.
        for (var attempt = 0; attempt < 5 && !token.IsCancellationRequested; attempt++)
        {
            var line = await connection.ReadLineAsync(token).ConfigureAwait(false);
            if (line is null)
            {
                return;
            }

            if (line.Contains(LoginPrompt, StringComparison.OrdinalIgnoreCase))
            {
                await connection.WriteLineAsync(_callsign, token).ConfigureAwait(false);
                _loggedIn = true;
                return;
            }

            // Some lines before the prompt are banner text; a spot this early
            // is still a spot.
            Accept(line);
        }
    }
}

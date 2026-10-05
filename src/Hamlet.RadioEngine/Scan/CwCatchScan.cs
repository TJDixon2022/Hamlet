using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Scan;

/// <summary>Whether a catch found a shape.</summary>
public enum CatchKind
{
    /// <summary>Dits and dahs were made out: the light showed a shape forming, or better.</summary>
    Positive,

    /// <summary>The scope showed energy and no shape formed.</summary>
    Negative,
}

/// <summary>Why the scan left a catch.</summary>
public enum CatchLeft
{
    /// <summary>A positive that printed letters and then went silent: the station finished.</summary>
    ReadOut,

    /// <summary>A positive that went silent without printing a letter.</summary>
    WentSilent,

    /// <summary>The stay ran out: the positive stay, or the negative stay.</summary>
    StayRanOut,

    /// <summary>The operator pressed Stop.</summary>
    Stopped,

    /// <summary>The link to the radio dropped.</summary>
    LinkDropped,

    /// <summary>The operator moved the dial; the scan carries on from there.</summary>
    DialMoved,

    /// <summary>The band changed under the scan.</summary>
    BandChanged,

    /// <summary>The scan's length ran out.</summary>
    ScanEnded,

    /// <summary>The CW tab was left, or the radio transmitted, and the scan stopped.</summary>
    ScanStopped,
}

/// <summary>How a scan ended, or why it did not start.</summary>
public enum CwScanEnd
{
    /// <summary>It is running.</summary>
    Running,

    /// <summary>Its length ran out.</summary>
    LengthReached,

    /// <summary>The operator pressed Stop.</summary>
    Stopped,

    /// <summary>The CW tab was left: the tab writes the mode, so a scan off it would be scanning in another mode.</summary>
    TabChanged,

    /// <summary>The link to the radio dropped.</summary>
    LinkDropped,

    /// <summary>The band changed under it.</summary>
    BandChanged,

    /// <summary>The radio transmitted: a scan never moves the dial while anything is going out (§0.2.1).</summary>
    Transmitting,

    /// <summary>Refused: Hamlet has not heard enough of the radio's state yet (§0.2.1).</summary>
    NotPopulated,

    /// <summary>Refused: the radio is not on a band with a CW segment Hamlet knows.</summary>
    NoCwSegment,

    /// <summary>Refused, or ended: the radio's scope sent no sweep.</summary>
    NoScope,
}

/// <summary>A scan's three settings: its length, and how long it stays on a positive and on a negative.</summary>
/// <param name="Length">How long the scan runs before it stops on its own; thirty minutes by default.</param>
/// <param name="PositiveStay">The longest it stays where a shape formed; ninety seconds by default.</param>
/// <param name="NegativeStay">How long it listens where none did; thirty seconds by default.</param>
public sealed record CwScanSettings(TimeSpan Length, TimeSpan PositiveStay, TimeSpan NegativeStay)
{
    /// <summary>The owner's defaults, 2026-10-04.</summary>
    public static CwScanSettings Defaults { get; } = new(TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(30));
}

/// <summary>One field of the radio's state at landing, as the capture sheet has it.</summary>
/// <param name="Field">Which.</param>
/// <param name="Value">What it read, or why it is unknown.</param>
/// <param name="Known">Whether there is a reading behind it.</param>
public sealed record CatchRigField(string Field, string Value, bool Known);

/// <summary>
/// One catch, as its JSON file holds it (work instruction 540, task 2).
/// </summary>
/// <param name="StartUtc">When the scan landed.</param>
/// <param name="EndUtc">When it left.</param>
/// <param name="DialHz">Where the dial was.</param>
/// <param name="SignalHz">The signal's frequency from the scope.</param>
/// <param name="SignalLevel">The signal's height on the scope's own scale.</param>
/// <param name="ScopeFloor">The scope's floor on the same scale.</param>
/// <param name="Kind">Positive or negative.</param>
/// <param name="Left">Why it left.</param>
/// <param name="Stations">Every sender the gate held.</param>
/// <param name="Text">The text printed.</param>
/// <param name="Letters">Each letter and its time.</param>
/// <param name="Lights">The light's states over the stay.</param>
/// <param name="Radio">The radio's state at landing.</param>
/// <param name="Wav">The WAV's file name.</param>
/// <param name="SampleRate">The WAV's rate.</param>
/// <param name="Seconds">How long it heard.</param>
public sealed record CwCatch(
    DateTime StartUtc,
    DateTime EndUtc,
    long DialHz,
    long SignalHz,
    double SignalLevel,
    double ScopeFloor,
    CatchKind Kind,
    CatchLeft Left,
    IReadOnlyList<CatchStation> Stations,
    string Text,
    IReadOnlyList<CatchLetter> Letters,
    IReadOnlyList<CatchLight> Lights,
    IReadOnlyList<CatchRigField> Radio,
    string Wav,
    int SampleRate,
    double Seconds)
{
    /// <summary>The catch's own JSON file name.</summary>
    [JsonIgnore]
    public string Json => Path.ChangeExtension(Wav, ".json");
}

/// <summary>A catch's line in scan.json.</summary>
/// <param name="StartUtc">When.</param>
/// <param name="SignalHz">Where.</param>
/// <param name="Kind">Positive or negative.</param>
/// <param name="Left">Why it left.</param>
/// <param name="Text">What printed.</param>
/// <param name="Wav">Its WAV.</param>
/// <param name="Json">Its JSON.</param>
public sealed record CwScanEntry(DateTime StartUtc, long SignalHz, CatchKind Kind, CatchLeft Left, string Text, string Wav, string Json);

/// <summary>A scan, as scan.json holds it.</summary>
/// <param name="StartUtc">When it started.</param>
/// <param name="EndUtc">When it ended, or null while it runs.</param>
/// <param name="Band">The band.</param>
/// <param name="SegmentLowHz">The CW segment's lower edge.</param>
/// <param name="SegmentHighHz">Its upper edge.</param>
/// <param name="HomeHz">Where the dial was when it started.</param>
/// <param name="LengthMinutes">The scan's length.</param>
/// <param name="PositiveStaySeconds">The positive stay.</param>
/// <param name="NegativeStaySeconds">The negative stay.</param>
/// <param name="SilentSeconds">How long a positive's station must be silent before the scan leaves it.</param>
/// <param name="MarginSpreads">The scope margin, in the floor's own spreads.</param>
/// <param name="Catches">Each catch, in order.</param>
/// <param name="Ended">How it ended.</param>
/// <param name="Sentence">What the line under the header said at the end.</param>
/// <param name="ScopeFollowsDial">Whether the scope moved with the dial when the segment was wider than its span; null where it was not wider.</param>
public sealed record CwScanSummary(
    DateTime StartUtc,
    DateTime? EndUtc,
    string Band,
    long SegmentLowHz,
    long SegmentHighHz,
    long HomeHz,
    double LengthMinutes,
    double PositiveStaySeconds,
    double NegativeStaySeconds,
    double SilentSeconds,
    double MarginSpreads,
    IReadOnlyList<CwScanEntry> Catches,
    CwScanEnd Ended,
    string Sentence,
    bool? ScopeFollowsDial);

/// <summary>
/// **THE SCAN: CATCH CW UNATTENDED, POSITIVES AND NEGATIVES, LISTEN ONLY** (work instruction 540, HM-DEC-244).
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-04:** *"I want a scan, like a radio scan where the radio used to scan when it sensed
/// something, it would stop. I want this to run unattended. It will scan, it will look for shape, it will sit there while
/// it's able to decode something. It will record what it's decoding as well as the waveform."* And: *"It's just as good to
/// find areas where the waterfall looks like it should have something and you can't hear it as when you can hear it."*</para>
/// <para>**WHAT IT WRITES TO THE RADIO: THE FREQUENCY, AND NOTHING ELSE.** It tunes to survey the segment where the scope
/// is narrower than it, to land on each peak, and back home at the end. It writes no mode (the CW tab is the mode), no
/// scope setting and no filter, and it never transmits: it holds the <see cref="ListenOnlyLock"/> from start to end, and
/// nothing in it reaches a keying call.</para>
/// <para>**§0.2.1 HOLDS, AS THE OWNER ANSWERED IT.** It refuses to start before the rig state is populated, never tunes
/// while the radio is transmitting, writes down where the dial was before it moves it and puts it back when it ends, and
/// stops when the link fails to answer. Two of that section's rules the owner answered otherwise for this scan, and
/// HM-DEC-244 records it: **a dial moved by hand is carried on from**, not a stop, and **the fence is the CW segment of the
/// band the radio is on**, from the band data the map uses, not a file he edits.</para>
/// <para>Delay and clock are injected, so a test runs a thirty-minute scan in seconds and the same code runs both (§5).</para>
/// </remarks>
public sealed class CwCatchScan
{
    /// <summary>
    /// **HOW LONG A POSITIVE'S STATION MUST BE SILENT BEFORE THE SCAN LEAVES IT: TEN SECONDS.** The author's: longer than
    /// any pause inside one operator's sending - two word gaps at 5 WPM Farnsworth are under four seconds - and long enough
    /// for the other operator to begin a reply on the same frequency, so a contact in progress keeps the scan there up to
    /// the positive stay, and one that has ended does not.
    /// </summary>
    public const double SilentSeconds = 10;

    /// <summary>How often the scan looks at what it hears: a quarter of a second.</summary>
    public static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(250);

    /// <summary>How long the scope is held over at each place surveyed: two seconds, a few of a keyed station's marks.</summary>
    public static readonly TimeSpan SurveyHold = TimeSpan.FromSeconds(2);

    /// <summary>How long after tuning the scan waits before it listens or reads the scope: half a second.</summary>
    public static readonly TimeSpan Settle = TimeSpan.FromMilliseconds(500);

    /// <summary>How long it waits for the scope's first sweep before saying it sends none: five seconds.</summary>
    public static readonly TimeSpan ScopeWait = TimeSpan.FromSeconds(5);

    private readonly IRig _rig;
    private readonly RigStateMonitor _monitor;
    private readonly ISpectrumSource _scope;
    private readonly CwCatchEar _ear;
    private readonly ListenOnlyLock _listenOnly;
    private readonly IScanHome _home;
    private readonly string _folder;
    private readonly CwScanSettings _settings;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;
    private readonly Func<DateTime> _utcNow;
    private readonly object _gate = new();
    private readonly ScopeWatch _watch = new();

    private long _frameLowHz;
    private long _frameHighHz;
    private long _frames;
    private long _placedHz;
    private bool _placed;
    private long? _dialMovedTo;
    private CwScanEnd? _stopFor;
    private string? _bandName;
    private CancellationTokenSource? _cancel;

    /// <summary>Creates a scan.</summary>
    /// <param name="rig">The radio.</param>
    /// <param name="monitor">What Hamlet knows about it.</param>
    /// <param name="scope">The radio's scope sweeps.</param>
    /// <param name="ear">The scan's ear on the audio.</param>
    /// <param name="listenOnly">The lock that keeps every keying path shut while it runs.</param>
    /// <param name="home">Where the dial was, including across a crash.</param>
    /// <param name="scansFolder">Hamlet's data folder for scans, beside the telemetry.</param>
    /// <param name="settings">Length and stays.</param>
    /// <param name="delay">How to wait; injected so tests are instant (§5).</param>
    /// <param name="utcNow">The clock.</param>
    public CwCatchScan(
        IRig rig,
        RigStateMonitor monitor,
        ISpectrumSource scope,
        CwCatchEar ear,
        ListenOnlyLock listenOnly,
        IScanHome home,
        string scansFolder,
        CwScanSettings? settings = null,
        Func<TimeSpan, CancellationToken, Task>? delay = null,
        Func<DateTime>? utcNow = null)
    {
        _rig = rig ?? throw new ArgumentNullException(nameof(rig));
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        _ear = ear ?? throw new ArgumentNullException(nameof(ear));
        _listenOnly = listenOnly ?? throw new ArgumentNullException(nameof(listenOnly));
        _home = home ?? throw new ArgumentNullException(nameof(home));
        _folder = scansFolder ?? throw new ArgumentNullException(nameof(scansFolder));
        _settings = settings ?? CwScanSettings.Defaults;
        _delay = delay ?? Task.Delay;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>True while it runs.</summary>
    public bool IsRunning { get; private set; }

    /// <summary>The folder this scan writes to, once it has started.</summary>
    public string? ScanFolder { get; private set; }

    /// <summary>What the line under the header says, each time it changes.</summary>
    public event Action<string>? Said;

    /// <summary>Raised as each catch is saved.</summary>
    public event Action<CwCatch>? Caught;

    /// <summary>Stop at once; the catch under way is saved, marked stopped.</summary>
    public void Stop() => StopFor(CwScanEnd.Stopped);

    /// <summary>Stop at once, for a reason: the tab was left, say.</summary>
    /// <param name="reason">Why.</param>
    public void StopFor(CwScanEnd reason)
    {
        lock (_gate)
        {
            _stopFor ??= reason;
        }

        try
        {
            _cancel?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    /// <summary>The line under the header, in the app's voice (§0.7).</summary>
    internal static string Line(CwScanEnd end, int catches) => end switch
    {
        CwScanEnd.LengthReached => $"scan done · {Catches(catches)}",
        CwScanEnd.Stopped => $"scan stopped · {Catches(catches)}",
        CwScanEnd.TabChanged => $"scan stopped · the CW tab was left · {Catches(catches)}",
        CwScanEnd.LinkDropped => $"scan aborted · the radio's link dropped · {Catches(catches)}",
        CwScanEnd.BandChanged => $"scan stopped · the band changed · {Catches(catches)}",
        CwScanEnd.Transmitting => $"scan stopped · the radio transmitted · {Catches(catches)}",
        CwScanEnd.NotPopulated => "scan not started · Hamlet has not heard enough from the radio yet",
        CwScanEnd.NoCwSegment => "scan not started · the radio is not on a band with a CW segment Hamlet knows",
        CwScanEnd.NoScope => $"scan stopped · the radio's scope sent no sweep · {Catches(catches)}",
        _ => "scanning",
    };

    private static string Catches(int n) => n == 1 ? "1 catch" : $"{n} catches";

    /// <summary>
    /// Run the scan until its length runs out, Stop is pressed, or something ends it.
    /// </summary>
    /// <param name="cancellationToken">Cancellation, as Stop.</param>
    /// <returns>The summary, as scan.json holds it. Never throws.</returns>
    public async Task<CwScanSummary> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = _utcNow();
        var catches = new List<CwScanEntry>();

        // THE START GATE: everything is checked before the dial moves at all (§0.2.1).
        if (!_monitor.IsPopulated)
        {
            return Refused(CwScanEnd.NotPopulated, started);
        }

        if (_monitor.State.IsTransmitting)
        {
            return Refused(CwScanEnd.Transmitting, started);
        }

        long home;

        try
        {
            home = await _rig.GetFrequencyHzAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return Refused(CwScanEnd.LinkDropped, started);
        }

        if (HfBands.BandFor(home) is not { } band)
        {
            return Refused(CwScanEnd.NoCwSegment, started);
        }

        lock (_gate)
        {
            _stopFor = null;
            _dialMovedTo = null;
            _placed = false;
            _bandName = band.Name;
        }

        ScanFolder = Path.Combine(_folder, "scan-" + started.ToString("yyyy-MM-dd-HHmmss", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(ScanFolder);

        using var listenOnly = _listenOnly.Hold();
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _cancel = linked;
        IsRunning = true;
        _home.Remember(home);
        _rig.FrequencyChanged += OnFrequencyChanged;
        _scope.FrameReady += OnFrame;

        var deadline = started + _settings.Length;
        var cursor = band.CwLowHz;
        bool? follows = null;
        CwScanEnd end;

        CwScanSummary Summary(CwScanEnd ended, DateTime? at) => new(
            started, at, band.Name, band.CwLowHz, band.CwHighHz, home,
            _settings.Length.TotalMinutes, _settings.PositiveStay.TotalSeconds, _settings.NegativeStay.TotalSeconds,
            SilentSeconds, ScopeWatch.MarginSpreads, catches.ToList(), ended,
            ended == CwScanEnd.Running ? "scanning" : Line(ended, catches.Count), follows);

        Write("scan.json", Summary(CwScanEnd.Running, null));

        try
        {
            while (true)
            {
                if (Ended(band, deadline) is { } over)
                {
                    end = over;
                    break;
                }

                Say($"scanning {Mhz(cursor)}");

                var (peaks, scopeFollows) = await Survey(band, linked.Token).ConfigureAwait(false);

                follows ??= scopeFollows;

                if (peaks is null)
                {
                    end = Ended(band, deadline) ?? CwScanEnd.NoScope;
                    break;
                }

                // Round and round, in frequency order, from where the scan is now.
                var round = peaks.Where(p => p.FrequencyHz >= cursor).Concat(peaks.Where(p => p.FrequencyHz < cursor)).ToList();

                if (round.Count == 0)
                {
                    await Wait(SurveyHold, linked.Token).ConfigureAwait(false);
                    cursor = band.CwLowHz;
                    continue;
                }

                CwScanEnd? stop = null;

                foreach (var peak in round)
                {
                    if (Ended(band, deadline) is { } now)
                    {
                        stop = now;
                        break;
                    }

                    var (caught, after) = await CatchAt(peak, band, deadline, linked.Token).ConfigureAwait(false);

                    if (caught is not null)
                    {
                        catches.Add(new CwScanEntry(caught.StartUtc, caught.SignalHz, caught.Kind, caught.Left, caught.Text, caught.Wav, caught.Json));
                        Write("scan.json", Summary(CwScanEnd.Running, null));
                        Caught?.Invoke(caught);
                    }

                    if (after is { } ended)
                    {
                        stop = ended;
                        break;
                    }

                    long? moved;

                    lock (_gate)
                    {
                        moved = _dialMovedTo;
                        _dialMovedTo = null;
                    }

                    if (moved is { } there)
                    {
                        // **THE DIAL MOVED BY HAND: CARRY ON FROM THERE** (the owner, 2026-10-04).
                        cursor = there;
                        break;
                    }

                    cursor = peak.FrequencyHz + ScopeWatch.MergeHz;
                }

                if (stop is { } s)
                {
                    end = s;
                    break;
                }

                if (cursor > band.CwHighHz)
                {
                    cursor = band.CwLowHz;
                }
            }
        }
        catch (OperationCanceledException)
        {
            end = Ended(band, deadline) ?? CwScanEnd.Stopped;
        }
        catch (Exception)
        {
            end = CwScanEnd.LinkDropped;
        }
        finally
        {
            _rig.FrequencyChanged -= OnFrequencyChanged;
            _scope.FrameReady -= OnFrame;
            _cancel = null;
        }

        // **THE DIAL GOES HOME** (§0.2.1), unless the link is down - the note stays on disk and the next connect puts it
        // back - or the band changed under the scan, which the operator did and which putting back would undo.
        if (end is not (CwScanEnd.LinkDropped or CwScanEnd.BandChanged))
        {
            try
            {
                await _rig.SetFrequencyHzAsync(home, CancellationToken.None).ConfigureAwait(false);
                _home.Clear();
            }
            catch (Exception)
            {
                // The note stays on disk; the next connect puts the dial back.
            }
        }
        else if (end == CwScanEnd.BandChanged)
        {
            _home.Clear();
        }

        IsRunning = false;

        var summary = Summary(end, _utcNow());

        Write("scan.json", summary);
        Say(summary.Sentence);

        return summary;
    }

    /// <summary>
    /// Whatever ends the scan now, or null: Stop or the tab, its length, the link, the band, or a transmission.
    /// </summary>
    private CwScanEnd? Ended(CwBand band, DateTime deadline)
    {
        lock (_gate)
        {
            if (_stopFor is { } reason)
            {
                return reason;
            }
        }

        if (!_rig.IsConnected)
        {
            return CwScanEnd.LinkDropped;
        }

        var state = _monitor.State;

        if (state.IsTransmitting)
        {
            return CwScanEnd.Transmitting;
        }

        var dial = state[RigField.Frequency];

        if (!dial.IsKnown || dial.IsStale(_utcNow(), BandScanner.FreshEnough))
        {
            return CwScanEnd.LinkDropped;
        }

        if (dial.Number is { } hz && HfBands.BandFor((long)hz)?.Name != band.Name)
        {
            return CwScanEnd.BandChanged;
        }

        return _utcNow() >= deadline ? CwScanEnd.LengthReached : null;
    }

    /// <summary>
    /// The peaks of the CW segment: the scope as it is where the segment fits its span, or else the scope moved across
    /// the segment by tuning the dial, a span at a time. Null where the scope sends nothing.
    /// </summary>
    private async Task<(List<ScopePeak>? Peaks, bool? Follows)> Survey(CwBand band, CancellationToken token)
    {
        var waited = TimeSpan.Zero;

        while (Interlocked.Read(ref _frames) == 0)
        {
            if (waited >= ScopeWait)
            {
                return (null, null);
            }

            await Wait(Tick, token).ConfigureAwait(false);
            waited += Tick;
        }

        var (low, high) = Edges();

        if (low <= band.CwLowHz && high >= band.CwHighHz)
        {
            return (await HeldPeaks(band.CwLowHz, band.CwHighHz, token).ConfigureAwait(false), null);
        }

        // **WIDER THAN THE SCOPE'S SPAN: THE SCOPE IS MOVED ACROSS IT** by tuning the dial, which in the scope's centre mode
        // carries the span with it. Where it does not - the scope in its fixed mode - only what the scope shows is scanned,
        // and the summary says so. No scope setting is written.
        var span = high - low;
        var peaks = new List<ScopePeak>();
        bool? follows = null;

        for (var centre = band.CwLowHz + (span / 2); ; centre += span * 9 / 10)
        {
            await Tune(Math.Min(centre, band.CwHighHz), token).ConfigureAwait(false);
            await Wait(Settle, token).ConfigureAwait(false);

            var (l, h) = Edges();

            if (centre < l || centre > h)
            {
                follows = false;
                peaks.AddRange(await HeldPeaks(Math.Max(band.CwLowHz, l), Math.Min(band.CwHighHz, h), token).ConfigureAwait(false));
                break;
            }

            follows = true;
            peaks.AddRange(await HeldPeaks(Math.Max(band.CwLowHz, l), Math.Min(band.CwHighHz, h), token).ConfigureAwait(false));

            if (h >= band.CwHighHz)
            {
                break;
            }
        }

        var merged = new List<ScopePeak>();

        foreach (var p in peaks.OrderByDescending(p => p.Level))
        {
            if (merged.All(m => Math.Abs(m.FrequencyHz - p.FrequencyHz) > ScopeWatch.MergeHz))
            {
                merged.Add(p);
            }
        }

        return (merged.OrderBy(p => p.FrequencyHz).ToList(), follows);
    }

    private async Task<List<ScopePeak>> HeldPeaks(long fromHz, long toHz, CancellationToken token)
    {
        lock (_gate)
        {
            _watch.Clear();
        }

        await Wait(SurveyHold, token).ConfigureAwait(false);

        lock (_gate)
        {
            return _watch.Peaks(fromHz, toHz).ToList();
        }
    }

    private (long Low, long High) Edges()
    {
        lock (_gate)
        {
            return (_frameLowHz, _frameHighHz);
        }
    }

    /// <summary>Land on a peak, listen, decide, and save the catch. Returns the catch, and what ended the scan, if anything did.</summary>
    private async Task<(CwCatch? Catch, CwScanEnd? Ended)> CatchAt(ScopePeak peak, CwBand band, DateTime deadline, CancellationToken token)
    {
        try
        {
            await Tune(peak.FrequencyHz, token).ConfigureAwait(false);
            await Wait(Settle, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return (null, Ended(band, deadline) ?? CwScanEnd.Stopped);
        }
        catch (Exception)
        {
            return (null, CwScanEnd.LinkDropped);
        }

        var start = _utcNow();
        var radio = _monitor.State.All()
            .Select(v => new CatchRigField(v.Field.ToString(), v.Text, v.IsKnown))
            .ToList();
        var name = "catch-" + start.ToString("HHmmss", CultureInfo.InvariantCulture) + "-" + peak.FrequencyHz.ToString(CultureInfo.InvariantCulture);

        _ear.Begin();

        CatchLeft left;
        CwScanEnd? ended = null;
        var sense = _ear.Sense();

        while (true)
        {
            try
            {
                await Wait(Tick, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                ended = Ended(band, deadline) ?? CwScanEnd.Stopped;
                left = Leaving(ended.Value);
                sense = _ear.Sense();
                break;
            }

            sense = _ear.Sense();

            var stayed = _utcNow() - start;
            var positive = sense.ShapeSeen;

            Say(positive
                ? $"listening on {Mhz(peak.FrequencyHz)} · {LightWords(sense.Light)} · {Clock(stayed)}"
                : $"negative on {Mhz(peak.FrequencyHz)} · {Clock(stayed)}");

            if (Ended(band, deadline) is { } over)
            {
                ended = over;
                left = Leaving(over);
                break;
            }

            bool moved;

            lock (_gate)
            {
                moved = _dialMovedTo is not null;
            }

            if (moved)
            {
                left = CatchLeft.DialMoved;
                break;
            }

            if (positive)
            {
                if (sense.SilentSeconds >= SilentSeconds)
                {
                    left = sense.Letters > 0 ? CatchLeft.ReadOut : CatchLeft.WentSilent;
                    break;
                }

                if (stayed >= _settings.PositiveStay)
                {
                    left = CatchLeft.StayRanOut;
                    break;
                }
            }
            else if (stayed >= _settings.NegativeStay)
            {
                left = CatchLeft.StayRanOut;
                break;
            }
        }

        var wav = name + ".wav";
        CatchHeard heard;

        try
        {
            heard = _ear.End(Path.Combine(ScanFolder!, wav));
        }
        catch (Exception)
        {
            heard = new CatchHeard([], string.Empty, [], [], 0, 0);
        }

        var caught = new CwCatch(
            start, _utcNow(), peak.FrequencyHz, peak.FrequencyHz, peak.Level, peak.Floor,
            sense.ShapeSeen ? CatchKind.Positive : CatchKind.Negative, left,
            heard.Stations, heard.Text, heard.Letters, heard.Lights, radio, wav, heard.SampleRate, heard.Seconds);

        Write(caught.Json, caught);

        return (caught, ended);
    }

    private static CatchLeft Leaving(CwScanEnd end) => end switch
    {
        CwScanEnd.Stopped => CatchLeft.Stopped,
        CwScanEnd.LinkDropped => CatchLeft.LinkDropped,
        CwScanEnd.BandChanged => CatchLeft.BandChanged,
        CwScanEnd.LengthReached => CatchLeft.ScanEnded,
        _ => CatchLeft.ScanStopped,
    };

    /// <summary>Tune, writing down where before the radio is told, so its echo is not read as a hand on the dial.</summary>
    private async Task Tune(long hz, CancellationToken token)
    {
        lock (_gate)
        {
            _placedHz = hz;
            _placed = true;
        }

        await _rig.SetFrequencyHzAsync(hz, token).ConfigureAwait(false);
    }

    private Task Wait(TimeSpan span, CancellationToken token) => _delay(span, token);

    private void OnFrequencyChanged(object? sender, FrequencyChangedEventArgs e)
    {
        lock (_gate)
        {
            if (_placed && Math.Abs(e.FrequencyHz - _placedHz) > BandScanner.DialTouchedHz)
            {
                _dialMovedTo = e.FrequencyHz;
                _placedHz = e.FrequencyHz;

                // **A HAND TUNE INTO ANOTHER BAND IS A BAND CHANGE**, and the scan stops at once rather than waiting for the
                // next report of the dial: it never changes band, and it does not scan one it was not started on.
                if (_bandName is not null && HfBands.BandFor(e.FrequencyHz)?.Name != _bandName)
                {
                    _stopFor ??= CwScanEnd.BandChanged;
                }
            }
        }
    }

    private void OnFrame(in SpectrumFrame frame)
    {
        lock (_gate)
        {
            if (frame.LowHz != _frameLowHz || frame.HighHz != _frameHighHz)
            {
                _watch.Clear();
            }

            _frameLowHz = frame.LowHz;
            _frameHighHz = frame.HighHz;
            _watch.Add(frame.LowHz, frame.HighHz, frame.Bins);
        }

        Interlocked.Increment(ref _frames);
    }

    private CwScanSummary Refused(CwScanEnd why, DateTime at)
    {
        var sentence = Line(why, 0);

        Say(sentence);

        return new CwScanSummary(at, at, string.Empty, 0, 0, 0, _settings.Length.TotalMinutes, _settings.PositiveStay.TotalSeconds,
            _settings.NegativeStay.TotalSeconds, SilentSeconds, ScopeWatch.MarginSpreads, [], why, sentence, null);
    }

    private void Say(string line) => Said?.Invoke(line);

    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) },
    };

    private void Write<T>(string file, T value)
    {
        try
        {
            File.WriteAllText(Path.Combine(ScanFolder!, file), JsonSerializer.Serialize(value, Json));
        }
        catch (Exception)
        {
            // Never-throw discipline (§8): a file that cannot be written is dropped, not a scan that dies.
        }
    }

    private static string Mhz(long hz) => (hz / 1_000_000.0).ToString("0.000", CultureInfo.InvariantCulture);

    private static string Clock(TimeSpan t) => $"{(int)t.TotalMinutes}:{t.Seconds:00}";

    private static string LightWords(Cw.CwShapeLight light) => light switch
    {
        Cw.CwShapeLight.Listening => "shape found",
        _ => CwCatchEar.Words(light),
    };
}

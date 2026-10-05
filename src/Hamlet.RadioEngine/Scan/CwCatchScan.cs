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

    /// <summary>
    /// **EMPTY** (work instruction 542, task 3): no tone in the passband at the scope's peak, nor half a filter either side. Left
    /// at once; the owner's negatives - a tone Hamlet cannot read - are kept apart from it.
    /// </summary>
    Empty,

    /// <summary>
    /// **A CARRIER** (work instruction 544, task 3, HM-DEC-248): a tone that stands clear of the noise and does not key. Left
    /// early, and its true frequency remembered for the rest of the scan.
    /// </summary>
    Carrier,
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

    /// <summary>Nothing was heard: an empty stop, left at once (work instruction 542).</summary>
    NothingHeard,

    /// <summary>A steady carrier: its tone held, never keyed, and the scan left it (work instruction 544).</summary>
    SteadyCarrier,
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

/// <summary>A scan's settings: its length, how long it stays on a positive and on a negative, and how long it watches a span.</summary>
/// <param name="Length">How long the scan runs before it stops on its own; thirty minutes by default.</param>
/// <param name="PositiveStay">The longest it stays where a shape formed; ninety seconds by default.</param>
/// <param name="NegativeStay">How long it listens where none did; thirty seconds by default.</param>
public sealed record CwScanSettings(TimeSpan Length, TimeSpan PositiveStay, TimeSpan NegativeStay)
{
    /// <summary>The survey time's default: three seconds, the owner's *"two, three seconds"*, about thirteen of the radio's sweeps.</summary>
    public static readonly TimeSpan DefaultSurveyTime = TimeSpan.FromSeconds(3);

    /// <summary>The owner's defaults, 2026-10-04, and the survey time of 2026-10-05. Declared after the survey time, which it reads.</summary>
    public static CwScanSettings Defaults { get; } = new(TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(30));

    /// <summary>
    /// **HOW LONG THE SCAN WATCHES EACH SPAN** before it visits what it saw (work instruction 543, HM-DEC-247); three seconds
    /// by default.
    /// </summary>
    public TimeSpan SurveyTime { get; init; } = DefaultSurveyTime;
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
/// <param name="ScopeHz">Where the scope showed the peak, before the scan landed by ear (work instruction 542).</param>
/// <param name="ToneHz">The tone the ear found there, before the dial was retuned to put it at the CW pitch; null where none.</param>
/// <param name="Survey">The survey the station was listed in, counting from one (work instruction 543); nought where none.</param>
/// <param name="ToneAfterHz">The tone heard after the scan centred it, the last measurement before the stay (work instruction 544); null where none was heard.</param>
/// <param name="ToneFollowsDial">Which way the tone moved for a move of the dial, as the scan knew it then: 1 where it rises with the dial, -1 where it falls.</param>
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
    double Seconds,
    long ScopeHz = 0,
    double? ToneHz = null,
    int Survey = 0,
    double? ToneAfterHz = null,
    int ToneFollowsDial = 0)
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
/// <param name="Survey">The survey the station was listed in, counting from one (work instruction 543).</param>
public sealed record CwScanEntry(DateTime StartUtc, long SignalHz, CatchKind Kind, CatchLeft Left, string Text, string Wav, string Json, int Survey = 0);

/// <summary>
/// **ONE SURVEY OF ONE SPAN, AS SCAN.JSON KEEPS IT** (work instruction 543, task 3, HM-DEC-247): the span watched, the sweeps
/// that came in, and every peak considered, listed or skipped and why. A station the owner sees on the waterfall and the scan
/// skips has its reason here.
/// </summary>
/// <param name="Index">Which survey, counting from one; each catch names the survey it came from.</param>
/// <param name="StartUtc">When the watching began.</param>
/// <param name="LowHz">The span's lower edge, inside the CW segment.</param>
/// <param name="HighHz">Its upper edge.</param>
/// <param name="Sweeps">How many sweeps came in while the span was watched.</param>
/// <param name="Floor">The scope's floor on its own scale.</param>
/// <param name="Line">The line a bin must stand at or over to count.</param>
/// <param name="BinHz">How wide a bin was.</param>
/// <param name="Considered">Every peak considered, with its level, width at half its height, sweeps stood and verdict.</param>
/// <param name="Listed">How many stations were listed to visit.</param>
public sealed record CwScanSurvey(
    int Index,
    DateTime StartUtc,
    long LowHz,
    long HighHz,
    int Sweeps,
    double Floor,
    double Line,
    double BinHz,
    IReadOnlyList<ScopeCandidate> Considered,
    int Listed);

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
/// <param name="SurveySeconds">How long each span was watched (work instruction 543).</param>
/// <param name="Surveys">Every survey, in order, with every peak it considered.</param>
/// <param name="Carriers">Every carrier the scan found, at its true frequency (work instruction 544).</param>
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
    bool? ScopeFollowsDial,
    double SurveySeconds = 0,
    IReadOnlyList<CwScanSurvey>? Surveys = null,
    IReadOnlyList<long>? Carriers = null);

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
            _bandName = band.Name;
            _carriers.Clear();
            _placed = false;
            _bandName = band.Name;

            // CW starts as the owner's radio does, the tone rising with the dial; CW-R the other way (work instruction 544).
            _toneFollowsDial = _monitor.State[RigField.Mode] is { IsKnown: true, Number: { } mode } && (Civ.CivMode)(int)mode == Civ.CivMode.CwReverse ? -1 : 1;
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
        var surveys = new List<CwScanSurvey>();
        long? nextCentre = null;
        long? carryFrom = null;
        bool? follows = null;
        CwScanEnd end;

        CwScanSummary Summary(CwScanEnd ended, DateTime? at) => new(
            started, at, band.Name, band.CwLowHz, band.CwHighHz, home,
            _settings.Length.TotalMinutes, _settings.PositiveStay.TotalSeconds, _settings.NegativeStay.TotalSeconds,
            SilentSeconds, ScopeWatch.MarginSpreads, catches.ToList(), ended,
            ended == CwScanEnd.Running ? "scanning" : Line(ended, catches.Count), follows,
            _settings.SurveyTime.TotalSeconds, surveys.ToList(), Carriers());

        Write("scan.json", Summary(CwScanEnd.Running, null));

        // **WATCH A SPAN, VISIT WHAT IT SAW, THEN MOVE ON** (work instruction 543, HM-DEC-247). The owner, watching a scan:
        // *"Advance, scan the waterfall for maybe two, three seconds, see if there's any station, then lock into that station
        // and try it."*
        try
        {
            while (true)
            {
                if (Ended(band, deadline) is { } over)
                {
                    end = over;
                    break;
                }

                if (!await FirstSweep(linked.Token).ConfigureAwait(false))
                {
                    end = Ended(band, deadline) ?? CwScanEnd.NoScope;
                    break;
                }

                // 1. **ADVANCE ONE SPAN** across the CW segment, wrapping at its end. Where the scope already shows the whole
                // segment, or stays put when the dial moves (its fixed mode), there is one span and it is what the scope shows.
                var (low, high) = Edges();
                long watchLow, watchHigh;

                if ((low <= band.CwLowHz && high >= band.CwHighHz) || follows == false)
                {
                    watchLow = Math.Max(band.CwLowHz, low);
                    watchHigh = Math.Min(band.CwHighHz, high);
                }
                else
                {
                    var span = high - low;
                    var centre = nextCentre ?? band.CwLowHz + (span / 2);

                    if (centre - (span / 2) >= band.CwHighHz)
                    {
                        centre = band.CwLowHz + (span / 2);
                    }

                    if (surveys.Count > 0)
                    {
                        Say($"advancing to {Mhz(Math.Max(band.CwLowHz, centre - (span / 2)))}–{Mhz(Math.Min(band.CwHighHz, centre + (span / 2)))}");
                    }

                    await Tune(centre, linked.Token).ConfigureAwait(false);
                    await Wait(Settle, linked.Token).ConfigureAwait(false);

                    var (l, h) = Edges();

                    follows = centre >= l && centre <= h;
                    watchLow = Math.Max(band.CwLowHz, l);
                    watchHigh = Math.Min(band.CwHighHz, h);
                    nextCentre = centre + span;
                }

                // 2. **WATCH THE WATERFALL** for the survey time and list every station that keeps showing up.
                var (survey, stopped) = await Watch(watchLow, watchHigh, band, deadline, linked.Token).ConfigureAwait(false);

                surveys.Add(new CwScanSurvey(
                    surveys.Count + 1, survey.StartUtc, watchLow, watchHigh, survey.Result.Sweeps, survey.Result.Floor, survey.Result.Line,
                    survey.Result.BinHz, survey.Result.Considered, survey.Result.Listed.Count));
                Write("scan.json", Summary(CwScanEnd.Running, null));

                if (stopped is { } early)
                {
                    end = early;
                    break;
                }

                // 3. **VISIT EACH STATION ON THE LIST, IN FREQUENCY ORDER**, landing by ear as before. 4. **When the list is
                // done, advance**; a span with no station advances at once.
                // After a hand on the dial, the visits begin at the station nearest where it was left, then wrap.
                var listed = carryFrom is { } hand
                    ? survey.Result.Listed.Where(p => p.FrequencyHz >= hand - ScopeWatch.MergeHz).Concat(survey.Result.Listed.Where(p => p.FrequencyHz < hand - ScopeWatch.MergeHz)).ToList()
                    : survey.Result.Listed;

                carryFrom = null;
                CwScanEnd? stop = null;

                for (var i = 0; i < listed.Count; i++)
                {
                    if (Ended(band, deadline) is { } now)
                    {
                        stop = now;
                        break;
                    }

                    long? nearCarrier;

                    lock (_gate)
                    {
                        nearCarrier = _carriers.Cast<long?>().FirstOrDefault(c => Math.Abs(c!.Value - listed[i].FrequencyHz) <= CarrierSkipHz);
                    }

                    // **A KNOWN CARRIER IS NOT VISITED AGAIN** (work instruction 544, task 3), from either side.
                    if (nearCarrier is { } known)
                    {
                        Say($"visiting {i + 1} of {listed.Count} · {Mhz4(listed[i].FrequencyHz)} · the carrier at {Mhz4(known)}, passed by");
                        continue;
                    }

                    var (caught, after) = await CatchAt(listed[i], band, deadline, i + 1, listed.Count, surveys.Count, linked.Token).ConfigureAwait(false);

                    if (caught is not null)
                    {
                        catches.Add(new CwScanEntry(caught.StartUtc, caught.SignalHz, caught.Kind, caught.Left, caught.Text, caught.Wav, caught.Json, caught.Survey));
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
                        // **THE DIAL MOVED BY HAND: CARRY ON FROM THERE** (the owner, 2026-10-04): the next span is the one
                        // centred where the hand left it.
                        nextCentre = there;
                        carryFrom = there;
                        break;
                    }
                }

                if (stop is { } s)
                {
                    end = s;
                    break;
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

    /// <summary>Wait for the scope's first sweep; false where none comes within <see cref="ScopeWait"/>.</summary>
    private async Task<bool> FirstSweep(CancellationToken token)
    {
        var waited = TimeSpan.Zero;

        while (Interlocked.Read(ref _frames) == 0)
        {
            if (waited >= ScopeWait)
            {
                return false;
            }

            await Wait(Tick, token).ConfigureAwait(false);
            waited += Tick;
        }

        return true;
    }

    /// <summary>
    /// **WATCH THE WATERFALL** (work instruction 543): forget what was watched, take the sweeps that come in for the survey
    /// time, saying what it is doing as it goes, and list what they show. Returns early with what ended the scan, if anything did.
    /// </summary>
    private async Task<((DateTime StartUtc, ScopeSurvey Result) Survey, CwScanEnd? Ended)> Watch(long lowHz, long highHz, CwBand band, DateTime deadline, CancellationToken token)
    {
        var start = _utcNow();

        lock (_gate)
        {
            _watch.Clear();
        }

        ScopeSurvey Now()
        {
            lock (_gate)
            {
                return _watch.Survey(lowHz, highHz);
            }
        }

        var watched = TimeSpan.Zero;

        while (watched < _settings.SurveyTime)
        {
            await Wait(Tick, token).ConfigureAwait(false);
            watched += Tick;

            Say($"watching {Mhz(lowHz)}–{Mhz(highHz)} · {Clock(watched)} · {Stations(Now().Listed.Count)}");

            if (Ended(band, deadline) is { } over)
            {
                return ((start, Now()), over);
            }
        }

        return ((start, Now()), null);
    }

    private static string Stations(int n) => n == 1 ? "1 station" : $"{n} stations";

    private (long Low, long High) Edges()
    {
        lock (_gate)
        {
            return (_frameLowHz, _frameHighHz);
        }
    }

    /// <summary>Land on a peak, listen, decide, and save the catch. Returns the catch, and what ended the scan, if anything did.</summary>
    private async Task<(CwCatch? Catch, CwScanEnd? Ended)> CatchAt(ScopePeak peak, CwBand band, DateTime deadline, int index, int total, int survey, CancellationToken token)
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

        var name = "catch-" + _utcNow().ToString("HHmmss", CultureInfo.InvariantCulture) + "-" + peak.FrequencyHz.ToString(CultureInfo.InvariantCulture);

        Say($"visiting {index} of {total} · {Mhz4(peak.FrequencyHz)} · landing");

        // **LANDING BY EAR** (work instruction 542, task 3, HM-DEC-246): at a wide span a scope bin is a few hundred hertz, so the
        // scope's peak alone landed signals 6 to 241 Hz off the pitch, some outside the filter. So the scan listens, finds the
        // strongest narrow tone in the passband, and retunes so that tone sits at the CW pitch; with none, it tries half a
        // filter either side before it calls the stop empty.
        var (pitch, width) = Hearing();
        var landedAt = peak.FrequencyHz;
        (double Hz, double OverDb)? tone = null;
        var probeStart = _utcNow();
        double? toneAfter = null;
        var heardFrom = probeStart;

        try
        {
            foreach (var side in new[] { 0L, -(long)(width / 2), (long)(width / 2) })
            {
                if (side != 0)
                {
                    await Tune(peak.FrequencyHz + side, token).ConfigureAwait(false);
                    await Wait(Settle, token).ConfigureAwait(false);
                }

                heardFrom = _utcNow();
                _ear.Begin();
                await Wait(Probe, token).ConfigureAwait(false);
                tone = _ear.Tone(Probe.TotalSeconds, pitch - (width / 2), pitch + (width / 2));

                if (tone is not null)
                {
                    landedAt = peak.FrequencyHz + side;
                    break;
                }
            }

            // **A HIGH PEAK EARNS A LONGER LISTEN** (work instruction 544, task 4, HM-DEC-248): operators pause longer than a
            // check between overs, so where the scope drew the peak well over anything noise draws and no tone was heard in the
            // three checks, the scan listens at the peak once more, for longer, before it calls the stop empty.
            if (tone is null && peak.Level >= HighPeakLevel)
            {
                await Tune(peak.FrequencyHz, token).ConfigureAwait(false);
                await Wait(Settle, token).ConfigureAwait(false);
                heardFrom = _utcNow();
                _ear.Begin();
                await Wait(LongListen, token).ConfigureAwait(false);
                tone = _ear.Tone(LongListen.TotalSeconds, pitch - (width / 2), pitch + (width / 2));
                landedAt = peak.FrequencyHz;
            }

            // **THE TONE SITS AT THE PITCH BEFORE THE STAY IS JUDGED** (work instruction 544, task 2, HM-DEC-248). A tone already
            // within a few hertz of the pitch is not retuned, and the catch keeps what the probe heard. Otherwise the dial is
            // moved, the tone measured again, and moved again until it sits there; the catch keeps the last probe's audio.
            if (tone is { } heardTone && Math.Abs(heardTone.Hz - pitch) > LandedHz)
            {
                (toneAfter, heardFrom) = await Centre(landedAt, heardTone.Hz, pitch, width, token).ConfigureAwait(false);
            }
            else
            {
                toneAfter = tone?.Hz;
            }
        }
        catch (OperationCanceledException)
        {
            var stopped = Ended(band, deadline) ?? CwScanEnd.Stopped;

            return (Save(name, peak, survey, probeStart, CatchKind.Empty, Leaving(stopped), tone, _ear.Sense(), []), stopped);
        }
        catch (Exception)
        {
            return (Save(name, peak, survey, probeStart, CatchKind.Empty, CatchLeft.LinkDropped, tone, _ear.Sense(), []), CwScanEnd.LinkDropped);
        }

        if (tone is null)
        {
            // **EMPTY: LEFT AT ONCE.** No tone at the peak nor half a filter either side: the scope's peak held nothing to hear.
            Say($"visiting {index} of {total} · {Mhz4(peak.FrequencyHz)} · empty");

            return (Save(name, peak, survey, probeStart, CatchKind.Empty, CatchLeft.NothingHeard, null, _ear.Sense(), []), null);
        }

        var start = heardFrom;
        var radio = _monitor.State.All()
            .Select(v => new CatchRigField(v.Field.ToString(), v.Text, v.IsKnown))
            .ToList();

        CatchLeft left;
        CwScanEnd? ended = null;
        var carrierJudged = false;
        var isCarrier = false;
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
                ? $"visiting {index} of {total} · {Mhz4(Placed())} · {LightWords(sense.Light)} · {Clock(stayed)}"
                : $"visiting {index} of {total} · {Mhz4(Placed())} · negative · {Clock(stayed)}");

            if (Ended(band, deadline) is { } over)
            {
                ended = over;
                left = Leaving(over);
                break;
            }

            // **A CARRIER IS A CARRIER** (work instruction 544, task 3): once the stay has heard enough, a tone that has not
            // keyed is a carrier, whatever shape its marks made, and it is left at once.
            if (!carrierJudged && stayed >= CarrierListen && (toneAfter ?? tone?.Hz) is { } held)
            {
                carrierJudged = true;

                if (_ear.KeyUpShare(held) is { } share && share < CarrierKeyUpShare)
                {
                    isCarrier = true;
                    left = CatchLeft.SteadyCarrier;
                    Say($"visiting {index} of {total} · {Mhz4(Placed())} · a carrier, not keyed · {Clock(stayed)}");
                    break;
                }
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

        long dial;

        lock (_gate)
        {
            dial = _placedHz;
        }

        var caught = new CwCatch(
            start, _utcNow(), dial, dial, peak.Level, peak.Floor,
            isCarrier ? CatchKind.Carrier : sense.ShapeSeen ? CatchKind.Positive : CatchKind.Negative, left,
            heard.Stations, heard.Text, heard.Letters, heard.Lights, radio, wav, heard.SampleRate, heard.Seconds,
            peak.FrequencyHz, tone?.Hz, survey, toneAfter, _toneFollowsDial);

        Write(caught.Json, caught);

        // **A CARRIER'S TRUE FREQUENCY IS REMEMBERED**: the dial, less the tone's offset from the pitch the way the tone moves.
        if (isCarrier)
        {
            var heldTone = toneAfter ?? tone?.Hz ?? pitch;

            lock (_gate)
            {
                _carriers.Add(dial - (long)Math.Round(_toneFollowsDial * (heldTone - pitch)));
            }
        }

        return (caught, ended);
    }

    /// <summary>
    /// How high on the scope's own scale a peak must stand to earn <see cref="LongListen"/> before it is called empty: 20
    /// (work instruction 544, task 4).
    /// </summary>
    /// <remarks>
    /// The author's: on the clipped scope the noise blips of the first scans stood at 6 and 7, and three times the highest
    /// is energy noise alone does not draw. Stops at 79, 38 and 31 were called empty after their two-second checks.
    /// </remarks>
    public const double HighPeakLevel = 20;

    /// <summary>
    /// How long the scan listens once more at a high peak where its three checks heard no tone: six seconds, longer than the
    /// four an operator pauses between overs (work instruction 544, task 4). The author's.
    /// </summary>
    public static readonly TimeSpan LongListen = TimeSpan.FromSeconds(6);

    /// <summary>How long the stay listens before it judges whether a tone keys at all: eight seconds.</summary>
    /// <remarks>The author's: a few words at any speed, so the share of time key-up is a share of many gaps.</remarks>
    public static readonly TimeSpan CarrierListen = TimeSpan.FromSeconds(8);

    /// <summary>
    /// The share of the time key-up under which a tone is a carrier: 0.15 (work instruction 544, task 3, HM-DEC-248).
    /// </summary>
    /// <remarks>
    /// The author's, from what CW is: a keyed signal is key-up a quarter of its time or more, a gap after every element and
    /// three after every letter, less what frames straddling an edge and the fastest keying take from it. A steady tone in
    /// noise falls 6 dB under its own second only now and then. Measured, not chosen from: the scan's two stations read
    /// 0.23 to 0.39 over every eight seconds, and the carrier in the tree 0.085 to 0.105.
    /// </remarks>
    public const double CarrierKeyUpShare = 0.15;

    /// <summary>How near a known carrier a peak may be and not be visited again, from either side: 400 Hz.</summary>
    /// <remarks>
    /// The author's: the scope's peak sat 315 and 380 Hz from the stations it led to in the scan's two catches here, so a
    /// peak that near a carrier may be that carrier seen again. A station that close to a carrier is passed by as well.
    /// </remarks>
    public const long CarrierSkipHz = 400;

    private readonly List<long> _carriers = new();

    private List<long> Carriers()
    {
        lock (_gate)
        {
            return _carriers.ToList();
        }
    }

    /// <summary>How long the scan listens at each try for a tone before it judges: two seconds, a few of a station's marks.</summary>
    public static readonly TimeSpan Probe = TimeSpan.FromSeconds(2);

    /// <summary>How near the CW pitch a tone may sit and be landed on already, with no retune: ten hertz, two of the ear's steps.</summary>
    public const double LandedHz = 10;

    /// <summary>
    /// **WHICH WAY THE TONE MOVES WHEN THE DIAL MOVES** (work instruction 544, task 2, HM-DEC-248): 1 where it rises with the
    /// dial, -1 where it falls. Started from the mode and corrected from the audio, and kept for the rest of the scan.
    /// </summary>
    /// <remarks>
    /// <para>**FOUND ON THE SCAN'S TWO STATIONS**: landing by ear moved the dial by the tone less the pitch, as though the
    /// tone fell as the dial rose. On the owner's radio in CW it rises with it. A retune of +130 Hz took 730 Hz to 860,
    /// and one of -65 Hz took 535 to 470, so each landing doubled the error it set out to take away and left the station
    /// on the filter's skirt.</para>
    /// <para>**THE RADIO'S SIDE IS A SETTING HAMLET DOES NOT READ**, so it is measured: CW starts as the owner's radio
    /// does, the tone rising with the dial, CW-R the other way, and a retune that moves the tone away from the pitch, or
    /// out of the filter, turns it round.</para>
    /// </remarks>
    private int _toneFollowsDial = 1;

    /// <summary>
    /// **CENTRE THE TONE** (work instruction 544, task 2): move the dial so the tone heard at <paramref name="dial"/> sits at
    /// the pitch, measure it again, and go on until it is within <see cref="LandedHz"/>, three moves at most. Where a move
    /// takes the tone further from the pitch or out of the filter, the tone goes the other way on this radio: the scan
    /// turns round from where it last heard the tone well. Ends at the best dial with a measurement there, whose audio the
    /// catch keeps.
    /// </summary>
    /// <returns>The tone at the dial left, or null where none could be heard there; and when its probe began.</returns>
    private async Task<(double? Tone, DateTime HeardFrom)> Centre(long dial, double tone, double pitch, double width, CancellationToken token)
    {
        var at = dial;
        double? last = null;
        var heardFrom = _utcNow();

        async Task<double?> Probe(long hz)
        {
            await Tune(hz, token).ConfigureAwait(false);
            await Wait(Settle, token).ConfigureAwait(false);
            at = hz;
            heardFrom = _utcNow();
            _ear.Begin();
            await Wait(CwCatchScan.Probe, token).ConfigureAwait(false);

            return _ear.Tone(CwCatchScan.Probe.TotalSeconds, pitch - (width / 2), pitch + (width / 2))?.Hz;
        }

        for (var move = 0; move < 3 && Math.Abs(tone - pitch) > LandedHz; move++)
        {
            var next = dial + (long)Math.Round(_toneFollowsDial * (pitch - tone));
            var heard = await Probe(next).ConfigureAwait(false);

            last = heard;

            if (heard is { } h && Math.Abs(h - pitch) < Math.Abs(tone - pitch))
            {
                dial = next;
                tone = h;
                continue;
            }

            // Further from the pitch, or out of the filter: the tone goes the other way. From where it was heard well.
            _toneFollowsDial = -_toneFollowsDial;
        }

        if (at != dial)
        {
            last = await Probe(dial).ConfigureAwait(false);
        }

        return (last, heardFrom);
    }

    /// <summary>The CW pitch and filter width the scan hears through: the radio's own in CW, or the ear's where unread.</summary>
    private (double PitchHz, double WidthHz) Hearing()
    {
        var (pitch, width) = Cw.CwChain.Passband(_monitor.State);

        // A pitch or width read as nought is not a reading anybody could hear through.
        return (pitch is > 0 and var p ? p : _ear.PitchHz, width is > 0 and var w ? w : _ear.WidthHz);
    }

    /// <summary>Save a catch the landing ended: an empty stop, or one stopped while the scan was still listening for a tone.</summary>
    private CwCatch Save(string name, ScopePeak peak, int survey, DateTime start, CatchKind kind, CatchLeft left, (double Hz, double OverDb)? tone, CatchSense sense, IReadOnlyList<CatchRigField> radio)
    {
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

        long dial;

        lock (_gate)
        {
            dial = _placedHz;
        }

        var caught = new CwCatch(
            start, _utcNow(), dial, kind == CatchKind.Empty ? peak.FrequencyHz : dial, peak.Level, peak.Floor, sense.ShapeSeen ? CatchKind.Positive : kind, left,
            heard.Stations, heard.Text, heard.Letters, heard.Lights, radio, wav, heard.SampleRate, heard.Seconds,
            peak.FrequencyHz, tone?.Hz, survey, null, _toneFollowsDial);

        Write(caught.Json, caught);

        return caught;
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

    private static string Mhz4(long hz) => (hz / 1_000_000.0).ToString("0.0000", CultureInfo.InvariantCulture);

    private long Placed()
    {
        lock (_gate)
        {
            return _placedHz;
        }
    }

    private static string Clock(TimeSpan t) => $"{(int)t.TotalMinutes}:{t.Seconds:00}";

    private static string LightWords(Cw.CwShapeLight light) => light switch
    {
        Cw.CwShapeLight.Listening => "shape found",
        _ => CwCatchEar.Words(light),
    };
}

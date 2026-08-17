using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls.ApplicationLifetimes;
using Hamlet.App.Licensing;
using Hamlet.App.Layout;
using Hamlet.App.Settings;
using Hamlet.App.Startup;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Solar;
using Hamlet.RadioEngine.Training;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Scan;
using Hamlet.RadioEngine.Transport;

namespace Hamlet.App.ViewModels;

/// <summary>
/// Shell ViewModel. One source of truth — <see cref="FrequencyHz"/> — that
/// the digits, ribbon and tape all bind to two-way. UI-origin changes are
/// throttled out to the rig; rig-origin changes (the physical knob) flow in
/// without echoing back out.
/// </summary>
/// <remarks>
/// It also owns the happening-now feed's clock (HM-DEC-020): a refresh timer
/// on the operator's interval, a one-second timer that keeps every displayed
/// age honest, and a pause when the window is not on screen. The freshness
/// rule itself lives in <see cref="SpotFreshness"/> as pure functions, so the
/// thresholds are testable without waiting for real minutes to pass.
/// </remarks>
public partial class MainWindowViewModel : ObservableObject
{
    /// <summary>
    /// The no-hardware entry in the port list — something to choose on
    /// purpose rather than a fallback (HM-DEC-026).
    /// </summary>
    public const string TrainingRadio = "Training radio (no hardware)";

    /// <summary>How long a newly-arrived spot wears its "new" tag.</summary>
    public static readonly TimeSpan NewSpotTagLifetime = TimeSpan.FromSeconds(30);

    private static readonly TimeSpan RigSendThrottle = TimeSpan.FromMilliseconds(150);
    private static readonly TimeSpan AgeTick = TimeSpan.FromSeconds(1);

    /// <summary>
    /// How long the dial has to sit still before the mode follows it.
    /// </summary>
    /// <remarks>
    /// Long enough that a drag across three neighborhoods is one change rather
    /// than three (HM-DEC-056). Changing mode is not like nudging a frequency:
    /// the radio mutes for an instant each time, so a flurry of them through one
    /// gesture would sound broken.
    /// </remarks>
    private static readonly TimeSpan ModeSettleDelay = TimeSpan.FromMilliseconds(600);

    private readonly DispatcherTimer _rigSendTimer;
    private readonly DispatcherTimer _modeSettleTimer;
    private readonly DispatcherTimer _spotRefreshTimer;
    private readonly DispatcherTimer _ageTimer;
    private readonly AppSettings _settings;
    private readonly JsonlTelemetry? _telemetry;
    private readonly List<ActivitySpot> _allBandSpots = new();

    /// <summary>
    /// Spots the operator tuned to in this session.
    /// </summary>
    /// <remarks>
    /// The store keeps the durable record; this covers the moments between an
    /// operator clicking Tune and the next write reaching the disk, so a card
    /// they have just been to does not reappear under "what's new" one refresh
    /// later (HM-DEC-057).
    /// </remarks>
    /// <summary>
    /// Whether the dial has sat still long enough to be worth remembering
    /// (HM-DEC-072).
    /// </summary>
    private readonly DwellTracker _dwell = new();

    /// <summary>
    /// A callsign the operator arrived on, and the frequency it belongs to.
    /// </summary>
    /// <remarks>
    /// TIED TO ITS FREQUENCY ON PURPOSE (§0.0). A call held loose would still be
    /// attached after the dial had moved somewhere else, and the recent list
    /// would name a station on a frequency nobody ever heard one on. Tying the
    /// two together means the pairing expires by itself the moment it stops
    /// being true.
    /// </remarks>
    private string _arrivedOnStation = "";
    private long _arrivedOnHz = -1;

    private readonly HashSet<string> _actedOnSpots =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Which of the two questions the happening-now list is answering
    /// (HM-DEC-057).
    /// </summary>
    /// <remarks>
    /// Not observable on its own: the two booleans below are what the segmented
    /// control binds to, because a toggle needs to know whether it is the one
    /// that is down.
    /// </remarks>
    private SpotLens _lens = SpotLens.BestChance;

    /// <summary>True once the operator has chosen a lens themselves.</summary>
    /// <remarks>
    /// The whole of "may never override the operator afterward". Inference runs
    /// while this is false and never once it is true.
    /// </remarks>
    private bool _lensChosenByOperator;
    private AggregateActivitySource _activitySource;
    private RbnActivitySource? _rbn;
    private IDisposable[] _ownedSources = Array.Empty<IDisposable>();
    private TrainingSpectrumSource? _trainingSpectrum;
    private readonly DispatcherTimer _decodeTimer;
    private RigStateMonitor? _rigMonitor;
    private IAudioSource? _audioInput;
    private CwDecoder? _decoder;
    private readonly Audio.ModeAudioPlayer _audio = new();
    private readonly PrivilegePlan _privileges = new();
    private CancellationTokenSource? _licenseLookup;
    private IRig? _rig;
    private bool _updatingFromRig;
    private bool _rigSendPending;
    private ModeFollowState _modeFollow = ModeFollowState.Armed(false);
    private bool _settingModeOurselves;
    private CivMode? _lastKnownMode;
    private bool _windowVisible = true;
    private bool _spotsEverLoaded;
    private IReadOnlyList<StoredSpot> _bandHistory = Array.Empty<StoredSpot>();
    private RigSpectrumSource? _rigSpectrum;
    private int _lastNewSpotCount;

    /// <summary>True when "Best chance" is the lens in use.</summary>
    [ObservableProperty]
    private bool _isBestChance = true;

    /// <summary>True when "What's new" is the lens in use.</summary>
    [ObservableProperty]
    private bool _isWhatsNew;

    /// <summary>What the active lens is for, on hover.</summary>
    [ObservableProperty]
    private string _lensQuestion = "";

    /// <summary>
    /// What is stopping the radio's scope from reaching the waterfall, or "".
    /// </summary>
    [ObservableProperty]
    private string _scopeNote = "";

    /// <summary>The three family chips, with their counts (HM-DEC-061).</summary>
    public ObservableCollection<FamilyChipViewModel> FamilyChips { get; } = new();

    /// <summary>Which families are switched on.</summary>
    private HashSet<ModeFamily> _families = new(FamilyFilter.All);
    private DateTime _lastSpotLoadUtc = DateTime.UtcNow;

    [ObservableProperty]
    private long _frequencyHz;

    [ObservableProperty]
    private string _statusText = "Pick a port and connect";

    [ObservableProperty]
    private string _modeLineText = "";

    [ObservableProperty]
    private bool _isInsideCwSegment = true;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _connectButtonText = "Connect";

    [ObservableProperty]
    private string _selectedPort = TrainingRadio;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MapSummary))]
    [NotifyPropertyChangedFor(nameof(MapLowHz))]
    [NotifyPropertyChangedFor(nameof(MapHighHz))]
    private BandButtonViewModel _selectedBand;

    [ObservableProperty]
    private IReadOnlyList<Neighborhood> _neighborhoods = Array.Empty<Neighborhood>();

    /// <summary>
    /// The lowest frequency the neighborhood map draws.
    /// </summary>
    /// <remarks>
    /// Wider than the band on purpose (HM-DEC-055). The map that stopped exactly
    /// at the band edge showed a wall as the end of the picture, so the operator
    /// who tuned to the very top of 20 m had no way to see that a little further
    /// up there is no amateur spectrum at all. The dial tape and the waterfall
    /// keep the band's own edges, because those are zoomed views of where the
    /// radio is rather than pictures of the whole band.
    /// </remarks>
    public long MapLowHz => SelectedBand.Band.LowHz - NeighborhoodPlan.MarginHz(SelectedBand.Band);

    /// <summary>The highest frequency the neighborhood map draws.</summary>
    public long MapHighHz => SelectedBand.Band.HighHz + NeighborhoodPlan.MarginHz(SelectedBand.Band);

    [ObservableProperty]
    private IReadOnlyList<Controls.ActivityDot> _activityDots =
        Array.Empty<Controls.ActivityDot>();

    /// <summary>
    /// The mode the radio is actually in, or empty when it has not been read.
    /// </summary>
    /// <remarks>
    /// THIS WAS THE LITERAL "CW" IN THE WINDOW until HM-DEC-050. The rig display
    /// showed CW whatever the radio was set to, which meant the screen lied the
    /// moment somebody switched to sideband. Empty is the honest starting point:
    /// nobody has asked yet.
    /// </remarks>
    [ObservableProperty]
    private string _rigModeText = "";

    /// <summary>The filter designator, or empty. It always read FIL2 before.</summary>
    [ObservableProperty]
    private string _rigFilterText = "";

    /// <summary>
    /// Where the S-meter sits, 0 to 1, or null when there is no reading.
    /// </summary>
    /// <remarks>
    /// Null rather than zero, all the way to the control. A needle at rest looks
    /// exactly like a measurement of a quiet band (§0.0).
    /// </remarks>
    [ObservableProperty]
    private double? _sMeterLevel;

    /// <summary>
    /// The filter's width in words, or empty when it is not known.
    /// </summary>
    /// <remarks>
    /// Shown beside the decoder's speed readout, because this is the number that
    /// explains a bad decode: a passband wide open at 3 kHz puts several signals
    /// into the decoder at once.
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFilterBandwidth))]
    private string _filterBandwidthText = "";

    /// <summary>The sending speed the decoder is tracking, or 0.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TerminalSummary))]
    [NotifyPropertyChangedFor(nameof(TerminalSpeedText))]
    [NotifyPropertyChangedFor(nameof(HasDetectedSpeed))]
    private int _detectedWpm;

    /// <summary>
    /// What the decoder has noticed about the signal, in plain words, or empty.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDecodeNote))]
    [NotifyPropertyChangedFor(nameof(TerminalSummary))]
    private string _decodeNote = "";

    /// <summary>Whether the decoder is listening to anything.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TerminalSummary))]
    [NotifyPropertyChangedFor(nameof(TerminalIdleText))]
    private bool _isDecoding;

    /// <summary>What the decoder is listening to, in words.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TerminalIdleText))]
    private string _audioInputName = "";

    [ObservableProperty]
    private string _storyTitle = "";

    [ObservableProperty]
    private string _storyBadge = "";

    [ObservableProperty]
    private string _storyBody = "";

    [ObservableProperty]
    private long _storyTuneHz;

    [ObservableProperty]
    private string _spotsSummary = "loading…";

    [ObservableProperty]
    private FreshnessLevel _spotsFreshness = FreshnessLevel.Fresh;

    /// <summary>The one written suggestion above the list (HM-DEC-025).</summary>
    [ObservableProperty]
    private LeadSuggestion _lead = new(
        false, "Looking…", "Hamlet is asking the spot networks what is on the air.",
        "", 0, "");

    /// <summary>The band-conditions claim and its evidence (HM-DEC-025).</summary>
    [ObservableProperty]
    private ConditionsLine _conditions = new(
        "Checking the bands…", "", ConditionsConfidence.Thin, null);

    /// <summary>
    /// The spectrum the waterfall draws, or null when nothing is receiving.
    /// The control subscribes to it directly; pixels never travel through
    /// binding (HM-DEC-006).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SignalsAreSimulated))]
    [NotifyPropertyChangedFor(nameof(WaterfallSummary))]
    [NotifyPropertyChangedFor(nameof(SpectrumNotice))]
    private ISpectrumSource? _spectrumSource;

    /// <summary>Waterfall display gain — a setting, not per-frame data.</summary>
    [ObservableProperty]
    private double _waterfallGain = 1.35;

    /// <summary>
    /// Where this license class may transmit across the band on screen.
    /// </summary>
    /// <remarks>
    /// Computed once, here, from the cited Part 97 data, and handed to every
    /// surface that shows privileges. The band map binds to it today; if the
    /// waterfall or the dial tape ever shows privileges they take this same
    /// list rather than computing their own, so two pictures of one law
    /// cannot disagree (HM-DEC-029).
    /// Empty means the class is unknown and nothing is drawn.
    /// </remarks>
    [ObservableProperty]
    private IReadOnlyList<PrivilegeSpan> _privilegeSpans = Array.Empty<PrivilegeSpan>();

    /// <summary>The line under the band map (HM-DEC-029).</summary>
    [ObservableProperty]
    private PrivilegeStatus _privilegeStatus = new(
        PrivilegeTone.Unknown, "", "", "", "", "");

    /// <summary>The upgrade ladder, shown only while the operator asks for it.</summary>
    [ObservableProperty]
    private IReadOnlyList<string> _upgradeLadder = Array.Empty<string>();

    /// <summary>True while the upgrade panel is open.</summary>
    [ObservableProperty]
    private bool _upgradeLadderVisible;

    /// <summary>
    /// A lookup disagreeing with a hand-set class, or null.
    /// </summary>
    /// <remarks>
    /// Non-null puts the choice on screen. Nothing is written to the profile
    /// while this is set — the operator decides (HM-DEC-028).
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LicenseMismatchNarration))]
    private LicenseResolution? _licenseMismatch;

    /// <summary>
    /// What the disagreement says, or "" when there is none (HM-DEC-089).
    /// </summary>
    /// <remarks>
    /// **FLATTENED RATHER THAN REACHED THROUGH.** A binding that walks into a
    /// null object logs a binding error on every evaluation, and a binding error
    /// is a defect rather than a diagnostic (§0.5.1). The panel is hidden when
    /// there is nothing to ask, and hidden is not the same as not evaluated.
    /// </remarks>
    public string LicenseMismatchNarration => LicenseMismatch?.Narration ?? "";

    /// <summary>One line naming which sources answered, for the panel header.</summary>
    [ObservableProperty]
    private string _sourcesSummary = "";

    [ObservableProperty]
    private bool _mapExpanded = true;

    [ObservableProperty]
    private bool _tapeExpanded = true;

    [ObservableProperty]
    private bool _waterfallExpanded = true;

    [ObservableProperty]
    private bool _scanExpanded = true;

    [ObservableProperty]
    private bool _terminalExpanded = true;

    [ObservableProperty]
    private bool _storyExpanded = true;

    [ObservableProperty]
    private bool _guideExpanded = true;

    [ObservableProperty]
    private bool _spotsExpanded = true;

    [ObservableProperty]
    private bool _leadExpanded = true;

    [ObservableProperty]
    private bool _contactExpanded = true;

    [ObservableProperty]
    private bool _transmitExpanded = true;

    [ObservableProperty]
    private bool _phrasebookExpanded = true;

    [ObservableProperty]
    private bool _heardExpanded = true;

    /// <summary>Who heard the operator, newest first (HM-DEC-075).</summary>
    public ObservableCollection<HeardReport> HeardReports { get; } = new();

    /// <summary>What Hamlet says about whether anybody heard him.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HeardSummary))]
    [NotifyPropertyChangedFor(nameof(HeardDetail))]
    [NotifyPropertyChangedFor(nameof(HasHeardReports))]
    private HeardSummary _heard = HeardWatch.Describe(
        null, Array.Empty<HeardReport>(), DateTime.UtcNow);

    /// <summary>The panel's collapsed summary (§0.5).</summary>
    public string HeardSummary => Heard.Headline;

    /// <summary>The paragraph under it.</summary>
    public string HeardDetail => Heard.Detail;

    /// <summary>True when there is a list to draw.</summary>
    public bool HasHeardReports => HeardReports.Count > 0;

    /// <summary>When the operator last called, or null.</summary>
    private DateTime? _calledAtUtc;

    /// <summary>
    /// A skimmer line arrived. Is it about him?
    /// </summary>
    /// <remarks>
    /// Runs on the feed's reader thread, so everything that touches the UI is
    /// posted. The match is exact: telling this operator he was heard when the
    /// machine heard a different station would be the cruelest bug in the
    /// application (HM-DEC-075).
    /// </remarks>
    private void OnRbnSpotParsed(RbnSpot spot)
    {
        if (!HeardWatch.IsMine(spot, _settings.Operator.Callsign))
        {
            return;
        }

        var report = HeardWatch.From(spot);

        // KEPT BEFORE IT IS SHOWN. The screen for a history of these comes
        // later, and a record that only started when somebody built that screen
        // would have missed the first one (HM-DEC-075).
        if (_spotStore is SqliteSpotStore store && !store.RecordHeard(report))
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            HeardReports.Insert(0, report);
            OnPropertyChanged(nameof(HasHeardReports));
            RefreshHeard(DateTime.UtcNow);
        });
    }

    /// <summary>
    /// The transmit precondition verdict changed, so the record says so
    /// (HM-DEC-077).
    /// </summary>
    /// <param name="readiness">The verdict, carrying what decided it.</param>
    /// <param name="context">The state it was decided from.</param>
    /// <param name="trigger">What caused the evaluation.</param>
    /// <remarks>
    /// AND IT REACHES THE OPERATOR TOO, not only the file. A file somebody has
    /// to upload is the second line of defense; the first is the screen, and the
    /// evening this was written the screen said nothing at all.
    /// </remarks>
    private void OnReadinessChanged(
        CwReadiness readiness, TransmitContext context, string trigger)
    {
        AppEvents.TransmitReadinessEvaluated(
            _telemetry, readiness, context.State, trigger);

        Decisions.Note(
            "Can I send",
            readiness.Reason,
            readiness.MaySend ? Outcome.Proceeded : Outcome.Refused,
            readiness.Detail,
            DateTime.UtcNow);
    }

    /// <summary>
    /// The send buttons changed state, so the record says what the operator saw
    /// (HM-DEC-078).
    /// </summary>
    /// <param name="enabled">Whether they can be pressed now.</param>
    /// <param name="readiness">The verdict behind it, or null.</param>
    private void OnSendEnabledChanged(bool enabled, CwReadiness? readiness)
    {
        AppEvents.SendButtonsEnabledChanged(_telemetry, enabled, readiness);

        Decisions.Note(
            "Send buttons",
            enabled ? "usable" : readiness?.Reason ?? "no_verdict",
            enabled ? Outcome.Proceeded : Outcome.Refused,
            enabled
                ? "The send buttons are live."
                : "The send buttons are off, and the reason is beside them.",
            DateTime.UtcNow);
    }

    /// <summary>
    /// A send produced a real SWR reading for the first time (HM-DEC-081).
    /// </summary>
    /// <remarks>
    /// Persisted so the note about the back of the radio does not come back on
    /// restart. It has earned its place by then: Hamlet has measured something
    /// about the socket and the operator has read the number.
    /// </remarks>
    private void OnSwrMeasured()
    {
        if (_settings.HasMeasuredSwr)
        {
            return;
        }

        _settings.HasMeasuredSwr = true;
        SettingsStore.Save(_settings);
    }

    /// <summary>
    /// How many skimmers were reporting on this band, or null (HM-DEC-082).
    /// </summary>
    /// <remarks>
    /// Null rather than zero when the feed is not answering, because an absent
    /// number reads as zero to somebody who has been disappointed before, and
    /// those are opposite facts about the evening.
    /// </remarks>
    private int? SkimmersOnThisBand()
        => _rbn?.SkimmersReporting(
            SelectedBand.Band.LowHz, SelectedBand.Band.HighHz);

    /// <summary>
    /// Everything measured about a send, kept with its record (HM-DEC-082).
    /// </summary>
    /// <param name="evidence">The chain.</param>
    private void OnChainReported(TransmitEvidence evidence)
    {
        AppEvents.TransmitChain(
            _telemetry,
            TransmitChain.BrokeAt(evidence)?.ToString() ?? "none",
            evidence.KeyedSeconds,
            evidence.PowerReading,
            evidence.SwrReading,
            evidence.SkimmersListening,
            evidence.Reports);

        Decisions.Note(
            "Transmit chain",
            TransmitChain.BrokeAt(evidence)?.ToString().ToLowerInvariant() ?? "whole",
            TransmitChain.BrokeAt(evidence) is null
                ? Outcome.Proceeded
                : Outcome.Degraded,
            TransmitChain.Describe(evidence),
            DateTime.UtcNow);
    }

    /// <summary>
    /// A message went to the radio (HM-DEC-079).
    /// </summary>
    /// <param name="message">What is going out. Recorded by length, never by text.</param>
    /// <param name="context">Where and in what mode.</param>
    private void OnSendStarted(string message, TransmitContext context)
    {
        AppEvents.SendStarted(
            _telemetry, message.Length, CwMessage.PieceCount(message),
            context.FrequencyHz,
            context.State[RigField.Mode] is { IsKnown: true } mode ? mode.Text : "");

        Decisions.Note(
            "Sending", "started", Outcome.Proceeded,
            $"{message.Length} characters going out.", DateTime.UtcNow);
    }

    /// <summary>
    /// The radio finished sending, one way or another (HM-DEC-079, HM-DEC-085).
    /// </summary>
    /// <param name="message">What went. Recorded by length, never by text.</param>
    /// <param name="context">Where.</param>
    /// <param name="outcome">What became of it, or null.</param>
    /// <param name="elapsed">How long the radio really keyed.</param>
    /// <param name="end">How the end of it was established.</param>
    /// <remarks>
    /// **COMPLETION MEANS THE RADIO FINISHED SENDING, NOT THAT THE BYTES WERE
    /// ACCEPTED** (HM-DEC-085). This used to be measured from the send call, so
    /// an eighteen-second transmission was recorded as a hundredth of a second,
    /// and that figure was not only wrong in the file: it reached the operator as
    /// "the radio keyed for 0 seconds" in the account of what happened. How the
    /// end was established is recorded beside it, because a duration Hamlet
    /// watched and one it worked out are different kinds of fact (§0.0).
    /// </remarks>
    private void OnSendFinished(
        string message,
        TransmitContext context,
        TransmitOutcome? outcome,
        TimeSpan elapsed,
        TransmissionEnd end)
    {
        var seconds = elapsed.TotalSeconds;
        var result = outcome?.Result;
        var what = result?.Outcome.ToString() ?? "Unknown";

        AppEvents.SendFinished(
            _telemetry, message.Length, what,
            result?.PiecesSent ?? 0, result?.PiecesTotal ?? 0,
            seconds, context.FrequencyHz, end.ToString());

        Decisions.Note(
            "Sending", what.ToLowerInvariant(),
            outcome?.Sent == true ? Outcome.Proceeded : Outcome.Failed,
            end == TransmissionEnd.Expected
                ? $"Ran about {seconds:0.0} seconds by the arithmetic. This radio "
                  + "does not report whether it is keying, so that is a calculation "
                  + "and not something Hamlet watched."
                : $"The radio keyed for {seconds:0.0} seconds.",
            DateTime.UtcNow);
    }

    /// <summary>
    /// "I can hear it and Hamlet can't" (HM-DEC-084).
    /// </summary>
    /// <remarks>
    /// SETTINGS ARE CONSEQUENCES OF INTENT AND NEVER CONTROLS. There is no noise
    /// blanker toggle anywhere in this application and there never will be:
    /// there is a button that names a problem the operator has, and behind it
    /// the handful of changes that usually cause it.
    /// </remarks>
    public ReceiveHelpViewModel ReceiveHelp { get; }

    /// <summary>Whether the panel is open (§0.5).</summary>
    [ObservableProperty]
    private bool _receiveHelpExpanded = true;

    /// <summary>
    /// What the radio is telling Hamlet right now, in one sentence.
    /// </summary>
    /// <remarks>
    /// The header of the panel: three figures and then a reading of them
    /// together. Every one of them measured or absent (§0.0).
    /// </remarks>
    public string ReceiveHeadline
    {
        get
        {
            var state = RigState;

            if (!IsConnected)
            {
                return "Nothing is connected, so there is nothing to look at yet.";
            }

            var heard = DetectedWpm > 0 || !Transcript.IsEmpty;
            var noise = state[RigField.NoiseBlanker].IsKnown
                        || state[RigField.RfGain].IsKnown;

            if (!noise)
            {
                return "Hamlet has not read enough from the radio yet to say how "
                    + "it is set up. It asks on connect, so give it a moment.";
            }

            return heard
                ? "The radio is hearing something and letters are coming through, "
                  + "so the path from the antenna to the decoder is working."
                : "The antenna is hearing something, so the radio is connected to "
                  + "the world. Nothing has resolved into letters yet.";
        }
    }

    /// <summary>How long a quiet decoder waits before offering to look.</summary>
    /// <remarks>
    /// Two minutes. Long enough that an ordinary gap between stations does not
    /// trigger it, short enough that somebody staring at an empty terminal gets
    /// the offer while they are still staring (HM-DEC-084).
    /// </remarks>
    private static readonly TimeSpan QuietBeforeOffering = TimeSpan.FromMinutes(2);

    /// <summary>When the decoder last produced a character.</summary>
    private DateTime _lastDecodeUtc = DateTime.MinValue;

    /// <summary>Whether the operator has waved the offer away.</summary>
    private bool _receiveOfferDismissed;

    /// <summary>
    /// A quiet line in the terminal offering to have a look (HM-DEC-084).
    /// </summary>
    /// <remarks>
    /// <para>**A POPUP SOMEBODY HAS TO KNOW TO OPEN IS A POPUP THEY WILL NOT
    /// OPEN WHEN THEY ARE FRUSTRATED**, which is exactly when it is needed. So
    /// the offer also appears where the problem shows: an empty terminal on a
    /// frequency where the app expected something.</para>
    /// <para>One line, not a banner, and dismissible. It says nothing at all
    /// when there is nothing to change, because an offer to fix a radio that is
    /// already right teaches somebody to ignore the next one.</para>
    /// </remarks>
    public string ReceiveOffer
    {
        get
        {
            if (_receiveOfferDismissed || !IsConnected || !IsDecoding)
            {
                return "";
            }

            if (DateTime.UtcNow - _lastDecodeUtc < QuietBeforeOffering)
            {
                return "";
            }

            return ReceiveHelp.Rows.Any(r => r.WouldChange)
                ? "Nothing has come through for a while. Hamlet can see a few "
                  + "things about the radio that usually cause that, and it can "
                  + "put them back afterward."
                : "";
        }
    }

    /// <summary>True when the offer has something to say.</summary>
    public bool HasReceiveOffer => ReceiveOffer.Length > 0;

    /// <summary>Wave the offer away for this session.</summary>
    [RelayCommand]
    private void DismissReceiveOffer()
    {
        _receiveOfferDismissed = true;
        OnPropertyChanged(nameof(ReceiveOffer));
        OnPropertyChanged(nameof(HasReceiveOffer));
    }

    /// <summary>Open the panel from the offer.</summary>
    [RelayCommand]
    private void OpenReceiveHelp()
    {
        ReceiveHelpExpanded = true;
        _receiveOfferDismissed = true;
        OnPropertyChanged(nameof(ReceiveOffer));
        OnPropertyChanged(nameof(HasReceiveOffer));
    }

    /// <summary>Write one documented setting to the radio (HM-DEC-084).</summary>
    private Task<RigWriteResult> WriteSettingAsync(CivWrite write, int value)
        => _rig is null
            ? Task.FromResult(RigWriteResult.NotSupported("nothing is connected"))
            : _rig.SetSettingAsync(write, value);

    /// <summary>Every change Hamlet made is announced (HM-DEC-084).</summary>
    /// <param name="change">What changed.</param>
    private void OnSettingChanged(SettingChange change)
    {
        StatusText = change.Says;

        AppEvents.SettingChanged(
            _telemetry, change.Write.Field.ToString(), change.Write.Label,
            change.Was, change.Now, change.Outcome.ToString());

        Decisions.Note(
            "Changed a setting",
            change.Confirmed ? change.Write.Field.ToString() : "unconfirmed",
            change.Confirmed ? Outcome.Proceeded : Outcome.Failed,
            change.Says,
            change.AtUtc);
    }

    /// <summary>What Hamlet has recently decided (HM-DEC-077).</summary>
    public DecisionLogViewModel Decisions { get; } = new();

    /// <summary>Open the record of what Hamlet decided.</summary>
    [RelayCommand]
    private void OpenDecisionLog()
    {
        var owner = (Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (owner is null)
        {
            return;
        }

        new Views.DecisionLogWindow { DataContext = Decisions }.ShowDialog(owner);
    }

    /// <summary>
    /// Something reached the air, so start watching for whoever heard it.
    /// </summary>
    /// <remarks>
    /// A call starts a fresh watch, because "did anybody hear me" is a question
    /// about this call. An answer or an exchange does not: the reports from the
    /// call are still the answer, and clearing them mid-contact would throw away
    /// the thing he came for (HM-DEC-075).
    /// </remarks>
    private void OnSomethingWentOut(SendOption option)
    {
        if (option.Stage == ContactStage.Calling)
        {
            NoteCallWentOut(DateTime.UtcNow);
        }

        // SOME WIDGETS ARRIVE ON THEIR OWN (HM-DEC-086), and the phrasebook is
        // the first of them. A contact beginning is exactly when somebody needs
        // to know what people say, and a contact ending is exactly when they do
        // not, so it comes out on the first thing that reaches the air and goes
        // away again after the sign-off.
        //
        // If the operator has moved it in the meantime it is theirs, and Hamlet
        // stops taking it away.
        if (option.Stage == ContactStage.SigningOff)
        {
            Canvas.Dismiss(Layout.Widgets.Phrasebook);
        }
        else
        {
            Canvas.Summon(Layout.Widgets.Phrasebook);
        }
    }

    /// <summary>When the last rig heartbeat went into the record.</summary>
    private DateTime _lastHeartbeatUtc = DateTime.MinValue;

    /// <summary>What the last heartbeat reported, so the next one is a delta.</summary>
    private RigState? _lastHeartbeatState;

    /// <summary>How often a quiet session writes its spine (HM-DEC-077).</summary>
    /// <remarks>
    /// A minute. The session that prompted this ran nearly two hours with its
    /// last human action in the first five minutes, and nothing in between said
    /// what the radio was doing.
    /// </remarks>
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromMinutes(1);

    /// <summary>Put the rig state in the record on a slow interval.</summary>
    /// <param name="nowUtc">The moment.</param>
    private void Heartbeat(DateTime nowUtc)
    {
        if (nowUtc - _lastHeartbeatUtc < HeartbeatInterval)
        {
            return;
        }

        _lastHeartbeatUtc = nowUtc;

        var state = RigState;

        AppEvents.RigHeartbeat(_telemetry, _lastHeartbeatState, state);

        _lastHeartbeatState = state;
    }

    /// <summary>Recompute what the heard panel says.</summary>
    /// <param name="nowUtc">The moment, passed in so the states are testable.</param>
    internal void RefreshHeard(DateTime nowUtc)
        => Heard = HeardWatch.Describe(_calledAtUtc, HeardReports.ToList(), nowUtc);

    /// <summary>
    /// A call went out, so start watching (HM-DEC-075).
    /// </summary>
    /// <remarks>
    /// The reports from before this moment are cleared, because the question is
    /// whether anybody heard THIS call. Leaving the last one's answers up would
    /// tell him he had been heard when nothing had come back yet, which is the
    /// feature inflating a silence and the one thing it may never do (§0.0).
    /// </remarks>
    internal void NoteCallWentOut(DateTime nowUtc)
    {
        _calledAtUtc = nowUtc;
        HeardReports.Clear();
        OnPropertyChanged(nameof(HasHeardReports));
        RefreshHeard(nowUtc);
    }

    /// <summary>The frequencies the operator saved (HM-DEC-060).</summary>
    public ObservableCollection<Favorite> Favorites { get; } = new();

    /// <summary>Where the operator has been, most recent first (HM-DEC-072).</summary>
    public ObservableCollection<RecentStation> Recent { get; } = new();

    /// <summary>The Radio menu's Favorites submenu (HM-DEC-060, HM-DEC-072).</summary>
    /// <remarks>
    /// EACH ITEM CARRIES ITS OWN COMMAND, which is not a style choice. A menu
    /// opens in its own popup, and a popup is a separate visual tree, so a
    /// binding that walks up to the window for the command resolves to nothing
    /// and the item silently does nothing when clicked. Nothing about that fails
    /// to compile and nothing about it looks wrong on screen, which is the worst
    /// combination there is.
    /// </remarks>
    public ObservableCollection<TuneMenuItem> FavoriteMenu { get; } = new();

    /// <summary>The Radio menu's Recent submenu (HM-DEC-072).</summary>
    public ObservableCollection<TuneMenuItem> RecentMenu { get; } = new();

    /// <summary>Rebuild the two submenus from the two lists.</summary>
    private void RebuildMenus()
    {
        FavoriteMenu.Clear();
        foreach (var favorite in Favorites)
        {
            var target = favorite;
            FavoriteMenu.Add(new TuneMenuItem(target.Name, () => TuneToFavorite(target)));
        }

        RecentMenu.Clear();
        foreach (var entry in Recent)
        {
            var target = entry;
            RecentMenu.Add(new TuneMenuItem(target.Label, () => TuneToRecent(target)));
        }

        OnPropertyChanged(nameof(HasFavorites));
        OnPropertyChanged(nameof(HasRecent));
    }

    /// <summary>
    /// The recent place picked from the dropdown, which tunes there.
    /// </summary>
    /// <remarks>
    /// Cleared straight after, exactly as the favorites box is, so it reads
    /// "recent" again rather than showing a stale selection once the dial has
    /// moved on.
    /// </remarks>
    [ObservableProperty]
    private RecentStation? _selectedRecent;

    /// <summary>True when there is anywhere to go back to.</summary>
    public bool HasRecent => Recent.Count > 0;

    /// <summary>
    /// What the star says, inside the display: one short word (HM-DEC-070).
    /// </summary>
    [ObservableProperty]
    private string _favoriteLabel = "save";

    /// <summary>
    /// The name of the favorite the dial is sitting on, or "".
    /// </summary>
    /// <remarks>
    /// Shown on the strip under the display rather than in the black, because a
    /// name is as long as somebody made it and the LCD has a mode badge at one
    /// end and a clock at the other (HM-DEC-070).
    /// </remarks>
    [ObservableProperty]
    private string _favoriteHere = "";

    /// <summary>
    /// The favorite picked from the dropdown, which tunes there.
    /// </summary>
    /// <remarks>
    /// Cleared straight after, so the box reads "favorites" again rather than
    /// showing a stale selection once the dial has moved on.
    /// </remarks>
    [ObservableProperty]
    private Favorite? _selectedFavorite;

    /// <summary>True when there is anything in the list to show.</summary>
    /// <remarks>
    /// A dropdown with nothing in it is a control that looks broken, so it is
    /// absent until there is something to pick.
    /// </remarks>
    public bool HasFavorites => Favorites.Count > 0;

    /// <summary>True when the dial is sitting on a saved frequency.</summary>
    /// <remarks>
    /// The star is filled here and hollow elsewhere, and pressing it on a
    /// favorite un-saves, so it is a toggle rather than two controls.
    /// </remarks>
    [ObservableProperty]
    private bool _isFavorite;

    /// <summary>
    /// The worked contact, both sides, in the operator's own callsign
    /// (HM-DEC-043).
    /// </summary>
    public ContactShapeViewModel ContactShape { get; }

    /// <summary>Sending Morse (HM-DEC-059).</summary>
    public CwTransmitViewModel Transmit { get; }

    /// <summary>The phrases people actually send (HM-DEC-059).</summary>
    public PhrasebookViewModel Phrasebook { get; }

    /// <summary>
    /// Everything the transmit guard and the break-in precondition need.
    /// </summary>
    /// <remarks>
    /// Read at the moment somebody presses rather than captured earlier, because
    /// the frequency, the mode and the break-in setting can all have moved since
    /// the panel was drawn and the guard has to answer about now (§0.2).
    /// </remarks>
    private TransmitContext BuildTransmitContext()
        => new(
            _settings.Operator.LicenseClass,
            FrequencyHz,
            _settings.RestrictTransmitToPrivileges,
            IsConnected,
            _rig?.Capabilities,
            RigState);

    /// <summary>How long spot history is kept before it is pruned.</summary>
    /// <remarks>
    /// A few days is plenty for anything the app does today, and the store
    /// must never grow without bound (HM-DEC-045).
    /// </remarks>
    public static readonly TimeSpan HistoryRetention = TimeSpan.FromDays(3);

    /// <summary>How often pruning runs.</summary>
    private static readonly TimeSpan PruneInterval = TimeSpan.FromHours(6);

    private readonly ISpotStore _spotStore;
    private DateTime _lastPruneUtc = DateTime.MinValue;

    /// <summary>The field guide entries, each with its samples.</summary>
    public IReadOnlyList<ModeCardViewModel> ModeCards { get; } =
        ModeGuide.Modes.Select(m => new ModeCardViewModel(m)).ToList();

    /// <summary>Happening-now spots, plain language, source-labeled.</summary>
    public ObservableCollection<SpotViewModel> Spots { get; } = new();

    /// <summary>Phase 1 bands with best-bet ranking for the current hour.</summary>
    public ObservableCollection<BandButtonViewModel> Bands { get; }

    /// <summary>The training radio plus every serial port on this machine.</summary>
    public ObservableCollection<string> AvailablePorts { get; }

    /// <summary>The operator's license class, or Unknown.</summary>
    public LicenseClass LicenseClass => _settings.Operator.LicenseClass;

    /// <summary>Provenance for the class, shown in Settings and the About box.</summary>
    public string LicenseProvenance => LicenseResolver.DescribeProvenance(_settings.Operator);

    /// <summary>
    /// The Shakespeare line under the wordmark, or "" when there is none
    /// (HM-DEC-039).
    /// </summary>
    [ObservableProperty]
    private string _byline = "";

    /// <summary>Which play the line was bent out of; shown on hover.</summary>
    [ObservableProperty]
    private string _bylineSource = "";

    /// <summary>True when there is a line to show at all.</summary>
    public bool HasByline => Byline.Length > 0;

    partial void OnBylineChanged(string value) => OnPropertyChanged(nameof(HasByline));

    /// <summary>Collapsed-header line for the neighborhood map (HM-DEC-021).</summary>
    public string MapSummary => string.Create(CultureInfo.InvariantCulture,
        $"CW main street · {SelectedBand.Band.CwLowHz / 1e6:0.000}"
        + $"–{SelectedBand.Band.CwHighHz / 1e6:0.000}");

    /// <summary>
    /// True when what the waterfall is drawing was synthesised rather than
    /// received off the air.
    /// </summary>
    /// <remarks>
    /// Derived on every read from the source itself, which has no setter and
    /// neither does this. That is the whole of HM-DEC-026: connection state
    /// IS the mode, so the label cannot drift out of step with what is on
    /// screen, and there is no setting anywhere that could put synthetic
    /// signals up unlabeled.
    /// </remarks>
    public bool SignalsAreSimulated => SpectrumSource?.IsSimulated == true;

    /// <summary>The persistent label the waterfall panel carries.</summary>
    public string SpectrumNotice
        => SignalsAreSimulated
            ? "Simulated signals, from the training radio rather than the air"
            : "";

    /// <summary>Collapsed-header line for the waterfall (HM-DEC-021).</summary>
    public string WaterfallSummary
    {
        get
        {
            if (SpectrumSource is null)
            {
                return "not yet receiving";
            }

            if (SignalsAreSimulated)
            {
                return $"simulated signals · {SelectedBand.Band.Name}";
            }

            // A shut panel may not claim to be receiving while nothing has
            // arrived, which is the collapsed-summary half of HM-DEC-067: a
            // panel that goes quiet about a problem is §0.5 broken by
            // omission.
            if (_rigSpectrum is { PartsReceived: 0 })
            {
                return $"no data has ever arrived · {SelectedBand.Band.Name}";
            }

            return _rigSpectrum is { SweepCount: 0 }
                ? $"parts arriving, no complete sweep · {SelectedBand.Band.Name}"
                : $"receiving · {SelectedBand.Band.Name}";
        }
    }

    /// <summary>
    /// What the scope path has actually done, stage by stage (HM-DEC-093).
    /// </summary>
    /// <remarks>
    /// <para>**AN EMPTY WATERFALL IS A CLAIM AND BLACK IS NOT A STATE**
    /// (HM-DEC-092). "Receiving frames and the band is quiet" and "no frame has
    /// ever arrived" paint exactly the same picture, and they are completely
    /// different facts. Three sessions reported this feature working while the
    /// second was true, and nothing on screen or in the log could have told them
    /// apart.</para>
    /// <para>So the counters are on the display itself: what came off the wire,
    /// what parsed, what was thrown away and why, and what was handed to the
    /// drawing. The first zero is the address of the fault.</para>
    /// </remarks>
    public string ScopeStages
    {
        get
        {
            if (_rigSpectrum is not { } stream)
            {
                return "";
            }

            if (stream.PartsReceived == 0)
            {
                return "No spectrum data has ever arrived from the radio. This is "
                    + "not a quiet band: nothing at all has come down the cable "
                    + "since Hamlet connected.";
            }

            var since = stream.LastPartUtc is { } last
                ? (int)(DateTime.UtcNow - last).TotalSeconds
                : -1;

            var rejected = stream.PartsRejected == 0
                ? ""
                : $", {stream.PartsRejected} thrown away";

            var quiet = since > 3 ? $", nothing for {since} seconds" : "";

            return $"{stream.PartsReceived} parts in, {stream.PartsParsed} read"
                + $"{rejected}, {stream.SweepsDelivered} sweeps drawn{quiet}.";
        }
    }

    /// <summary>True when there is anything to say about the stages.</summary>
    public bool HasScopeStages => ScopeStages.Length > 0;

    /// <summary>
    /// Why the first part Hamlet could not read was rejected, or "".
    /// </summary>
    public string ScopeRejection => _rigSpectrum?.FirstRejection ?? "";

    /// <summary>True when at least one part could not be read.</summary>
    public bool HasScopeRejection => ScopeRejection.Length > 0;

    /// <summary>The decoded Morse, on its way to the terminal.</summary>
    /// <remarks>
    /// Held here and read by the control directly rather than bound as a
    /// string, which is the arrangement HM-DEC-006 settled for the waterfall and
    /// applies for the same reason: at speed this fills at about forty
    /// characters a second.
    /// </remarks>
    public CwTranscript Transcript { get; } = new();

    /// <summary>True when there is something worth saying about the signal.</summary>
    public bool HasDecodeNote => DecodeNote.Length > 0;

    /// <summary>True once the filter width has been read from the radio.</summary>
    public bool HasFilterBandwidth => FilterBandwidthText.Length > 0;

    /// <summary>Everything Hamlet currently knows about the radio.</summary>
    public RigState RigState => _rigMonitor?.State ?? RigState.Empty;

    /// <summary>True once the decoder is tracking a speed worth showing.</summary>
    public bool HasDetectedSpeed => DetectedWpm > 0;

    /// <summary>The live speed readout on the terminal's header.</summary>
    public string TerminalSpeedText
        => DetectedWpm > 0 ? $"{DetectedWpm} WPM" : "";

    /// <summary>What the terminal shows before anything has been decoded.</summary>
    public string TerminalIdleText
        => !IsDecoding
            ? "not listening yet. Connect a radio, or pick the training radio, and this fills in."
            : $"listening to {AudioInputName}. Nothing decoded yet.";

    /// <summary>
    /// Collapsed-header line for the CW terminal (HM-DEC-021).
    /// </summary>
    /// <remarks>
    /// A SHUT PANEL STILL HAS TO TELL THE TRUTH (§0.5). If the decoder is
    /// struggling, that is exactly the moment somebody would shut the panel and
    /// conclude the app does not work, so the note travels into the summary
    /// rather than being hidden with the detail.
    /// </remarks>
    public string TerminalSummary
    {
        get
        {
            if (!IsDecoding)
            {
                return "not listening";
            }

            if (Transcript.IsEmpty)
            {
                return HasDecodeNote ? "nothing decoded yet" : "listening";
            }

            var speed = DetectedWpm > 0 ? $"{DetectedWpm} WPM · " : "";
            var tail = Transcript.Tail(28);

            return HasDecodeNote
                ? $"{speed}{tail} · signal is hard going"
                : $"{speed}{tail}";
        }
    }

    /// <summary>Collapsed-header line for the field guide.</summary>
    public string GuideSummary => $"{ModeCards.Count} modes · hear each one";

    /// <summary>Designer constructor.</summary>
    public MainWindowViewModel() : this(new AppSettings(), null)
    {
    }

    /// <summary>Runtime constructor.</summary>
    /// <param name="settings">Live settings; panel and feed state persist here.</param>
    /// <param name="telemetry">The writer, or null.</param>
    public MainWindowViewModel(AppSettings settings, JsonlTelemetry? telemetry)
    {
        _settings = settings;
        _telemetry = telemetry;

        Bands = new ObservableCollection<BandButtonViewModel>(
            BandPlan.Bands.Select(b => new BandButtonViewModel(b)));

        _selectedBand = Bands.FirstOrDefault(b => b.Band.Name == settings.LastBand)
                        ?? Bands.First(b => b.Band.Name == "40 m");
        _frequencyHz = _selectedBand.Band.JumpHz;

        _mapExpanded = settings.IsPanelExpanded(PanelKeys.Map);
        _tapeExpanded = settings.IsPanelExpanded(PanelKeys.Tape);
        _waterfallExpanded = settings.IsPanelExpanded(PanelKeys.Waterfall);
        _scanExpanded = settings.IsPanelExpanded(PanelKeys.Scan);
        _terminalExpanded = settings.IsPanelExpanded(PanelKeys.Terminal);
        _storyExpanded = settings.IsPanelExpanded(PanelKeys.Story);
        _guideExpanded = settings.IsPanelExpanded(PanelKeys.Guide);
        _spotsExpanded = settings.IsPanelExpanded(PanelKeys.Spots);
        _leadExpanded = settings.IsPanelExpanded(PanelKeys.Lead);
        _contactExpanded = settings.IsPanelExpanded(PanelKeys.Contact);
        _transmitExpanded = settings.IsPanelExpanded(PanelKeys.Transmit);
        _phrasebookExpanded = settings.IsPanelExpanded(PanelKeys.Phrasebook);
        _heardExpanded = settings.IsPanelExpanded(PanelKeys.Heard);

        ContactShape = new ContactShapeViewModel(settings.Operator.Callsign);

        // Everything the guard and the break-in precondition need, read at the
        // moment somebody presses rather than captured when the panel was built
        // (HM-DEC-059).
        Transmit = new CwTransmitViewModel(
            BuildTransmitContext, OnSomethingWentOut, OnReadinessChanged,
            OnSendEnabledChanged, OnSendStarted, OnSendFinished, OnSwrMeasured,
            SkimmersOnThisBand,
            () => SelectedBand.Band.Name,
            OnChainReported)
        {
            YourCall = settings.Operator.Callsign,
            Qth = settings.Operator.Location,
        };

        Phrasebook = new PhrasebookViewModel();

        foreach (var saved in settings.Favorites)
        {
            Favorites.Add(new Favorite(
                saved.FrequencyHz, saved.Name, saved.Mode, saved.BandName,
                saved.Neighborhood, saved.SavedUtc));
        }

        foreach (var saved in settings.Recent)
        {
            // A NAME WITH NO RECORDED SOURCE CAME FROM A SPOT FEED, because
            // that was the only way one could get in before provenance existed
            // (HM-DEC-073). That is a fact about the file rather than a guess
            // about the entry, so nothing is invented by reading it that way.
            var source = Enum.TryParse<StationSource>(saved.StationSource, out var parsed)
                ? parsed
                : saved.Station.Length > 0
                    ? StationSource.SpotFeed
                    : StationSource.None;

            Recent.Add(new RecentStation(
                saved.FrequencyHz, saved.Station, saved.Mode, saved.BandName,
                saved.Neighborhood, saved.VisitedUtc, source));
        }

        ReceiveHelp = new ReceiveHelpViewModel(
            () => RigState, WriteSettingAsync, OnSettingChanged);

        _receiveHelpExpanded = settings.IsPanelExpanded(PanelKeys.ReceiveHelp);

        RebuildMenus();


        // A stored lens is the operator's own last answer rather than a guess,
        // so it is restored and inference never runs against it (HM-DEC-057).
        if (Enum.TryParse<SpotLens>(settings.SpotLens, out var storedLens))
        {
            _lens = storedLens;
            _lensChosenByOperator = true;
        }

        _families = new HashSet<ModeFamily>(FamilyFilter.Parse(settings.SpotFamilies));

        _isBestChance = _lens == SpotLens.BestChance;
        _isWhatsNew = _lens == SpotLens.WhatsNew;
        _lensQuestion = SpotLensView.Question(_lens);

        // History, or an honest substitute for it. A store that cannot be
        // opened is a nuisance, never a reason not to start (§8, HM-DEC-045).
        _spotStore = SqliteSpotStore.TryOpen(
            System.IO.Path.Combine(SettingsStore.DataFolder, SqliteSpotStore.FileName))
            ?? (ISpotStore)new MemorySpotStore();

        if (!_spotStore.IsPersistent)
        {
            AppEvents.SpotHistoryUnavailable(_telemetry);
        }

        AvailablePorts = new ObservableCollection<string> { TrainingRadio };
        foreach (var name in SafePortNames())
        {
            AvailablePorts.Add(name);
        }

        if (settings.LastPort is not null && AvailablePorts.Contains(settings.LastPort))
        {
            _selectedPort = settings.LastPort;
        }

        _rigSendTimer = new DispatcherTimer(
            RigSendThrottle, DispatcherPriority.Background, OnRigSendTick);
        _rigSendTimer.Stop();

        _modeFollow = ModeFollowState.Armed(settings.ModeFollowsTheMap);
        _modeSettleTimer = new DispatcherTimer(
            ModeSettleDelay, DispatcherPriority.Background, OnModeSettleTick);
        _modeSettleTimer.Stop();

        _spotRefreshTimer = new DispatcherTimer(
            TimeSpan.FromMinutes(AppSettings.DefaultSpotRefreshMinutes),
            DispatcherPriority.Background, OnSpotRefreshTick);
        _spotRefreshTimer.Stop();

        _ageTimer = new DispatcherTimer(AgeTick, DispatcherPriority.Background, OnAgeTick);

        // The decoder runs on whichever thread the audio arrives on, so the
        // readouts it feeds are refreshed here rather than raised from there.
        // Four times a second is faster than a speed estimate moves and slower
        // than anything it would be worth interrupting the UI for.
        _decodeTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, OnDecodeTick);
        _ageTimer.Stop();

        _activitySource = BuildSources();

        Neighborhoods = NeighborhoodPlan.WithEdges(_selectedBand.Band);
        ShowNeighborhood(Neighborhoods.First(n => n.Contains(FrequencyHz)));
        UpdateModeLine();
        UpdateSpotFreshness();

        _hasTunedByWheel = settings.HasTunedByWheel;

        PickByline();

        // THE SCANNER'S FACE (HM-DEC-107, §0.2.1). It is built before the
        // canvas because the canvas places it, and it stays detached from any
        // radio until one connects: a scanner that could move the dial with no
        // rig behind it would be a scanner with nothing to abort against.
        Scan = new ScanViewModel(_settings, line => StatusText = line);

        // THE OPERATOR'S OWN SCAN FILE, WRITTEN ONCE (§0.2.1). It cannot be
        // edited until it exists, and nothing else in the app was going to
        // create it. Never overwritten afterwards: what he wrote is his.
        ScanSegments.WriteDefaultIfMissing(SettingsStore.ScanSegmentsPath);

        // THE CANVAS, LAST, because everything it places has to exist first
        // (HM-DEC-086). Nobody ever starts on an empty one: a first run, or a
        // layouts file that could not be read, lands on Getting started with
        // widgets already on it.
        Canvas = new CanvasViewModel(
            this, LayoutStore.Load(), () => LayoutStore.Save(Canvas!.Book()),
            OpenPanel);

        _ = ReloadSpotsAsync("startup");
        _ = ResolveProfileAsync();
        ApplyFeedTimers();
    }

    /// <summary>
    /// Write the canvas out, whatever else is happening (HM-DEC-089).
    /// </summary>
    /// <remarks>
    /// The arrangement already saves on every change, so this is the belt beside
    /// the braces: called as the window closes, because rebuilding a workspace by
    /// hand is the one loss the operator would actually feel.
    /// </remarks>
    public void KeepTheCanvas() => LayoutStore.Save(Canvas.Book());

    /// <summary>
    /// Open a panel that has just arrived on the canvas (HM-DEC-087).
    /// </summary>
    /// <param name="widgetId">Which widget.</param>
    /// <remarks>
    /// <para>**A WIDGET SOMEBODY REACHED FOR ARRIVES SHOWING ITS CONTENTS.**
    /// They all used to arrive shut, so pulling three things out of the tray
    /// gave three title bars and an empty canvas.</para>
    /// <para>The panel still owns whether it is open and goes on persisting that
    /// (HM-DEC-021), so this sets the same property the header would. The widget
    /// ids and the panel keys are the same words on purpose, and the two that
    /// differ are named here rather than assumed.</para>
    /// </remarks>
    private void OpenPanel(string widgetId)
    {
        switch (widgetId)
        {
            case Layout.Widgets.Map: MapExpanded = true; break;
            case Layout.Widgets.Tape: TapeExpanded = true; break;
            case Layout.Widgets.Waterfall: WaterfallExpanded = true; break;
            case Layout.Widgets.Scan: ScanExpanded = true; break;
            case Layout.Widgets.Terminal: TerminalExpanded = true; break;
            case Layout.Widgets.Story: StoryExpanded = true; break;
            case Layout.Widgets.Guide: GuideExpanded = true; break;
            case Layout.Widgets.Spots: SpotsExpanded = true; break;
            case Layout.Widgets.Lead: LeadExpanded = true; break;
            case Layout.Widgets.Contact: ContactExpanded = true; break;
            case Layout.Widgets.Heard: HeardExpanded = true; break;
            case Layout.Widgets.ReceiveHelp: ReceiveHelpExpanded = true; break;
            case Layout.Widgets.Phrasebook: PhrasebookExpanded = true; break;

            // The send panel's widget is called "send" and its stored key is
            // "transmit", which is the one place the two lists disagree. Renaming
            // either would change a settings key, so it is written down instead
            // (§6.1).
            case Layout.Widgets.Send: TransmitExpanded = true; break;
        }
    }

    /// <summary>
    /// What is on the canvas, and what could be (HM-DEC-086).
    /// </summary>
    /// <remarks>
    /// The widgets all bind against this view model, exactly as the panels they
    /// used to be did, so the canvas holds their arrangement and nothing about
    /// their contents.
    /// </remarks>
    public CanvasViewModel Canvas { get; }

    /// <summary>The scanner, and the stop control §0.2.1 requires.</summary>
    public ScanViewModel Scan { get; }

    /// <summary>
    /// Choose the line under the wordmark, avoiding last launch's
    /// (HM-DEC-039).
    /// </summary>
    /// <remarks>
    /// The chosen index is saved immediately rather than at shutdown, because
    /// an app that is killed rather than closed would otherwise show the same
    /// line forever — and this is meant to be a small surprise, not a fixture.
    /// </remarks>
    private void PickByline()
    {
        var picked = Bylines.Pick(_settings.LastBylineIndex);

        if (picked is not { } choice)
        {
            // No file, or nothing in it. There is simply no byline; a
            // placeholder would be worse than the silence.
            return;
        }

        Byline = choice.Line.Text;
        BylineSource = choice.Line.Source;

        _settings.LastBylineIndex = choice.Index;
        SettingsStore.Save(_settings);
    }

    /// <summary>
    /// Assemble the live sources the operator has switched on (HM-DEC-024).
    /// </summary>
    /// <returns>The aggregate to poll.</returns>
    /// <remarks>
    /// <para>Order is preference order: the aggregate keeps the first version
    /// of a duplicate spot, and an activation carries far more meaning for a
    /// newcomer than the same station seen bare by a skimmer, so POTA and
    /// SOTA lead.</para>
    /// <para>RBN is left out entirely when the operator has not set a
    /// callsign. Its telnet login is the callsign — there is no anonymous
    /// access — and inventing one would be lying to the service on the
    /// operator's behalf. Settings says so next to the switch.</para>
    /// </remarks>
    private AggregateActivitySource BuildSources()
    {
        DisposeSources();

        var version = AboutViewModel.AppVersion;
        var callsign = _settings.Operator.Callsign?.Trim() ?? "";
        var owned = new List<IDisposable>();
        var sources = new List<IActivitySource>();

        var pota = new PotaActivitySource(version, callsign);
        sources.Add(pota);
        owned.Add(pota);

        var sota = new SotaActivitySource(version, callsign);
        sources.Add(sota);
        owned.Add(sota);

        if (callsign.Length > 0)
        {
            _rbn = new RbnActivitySource(callsign);
            _rbn.SpotParsed += OnRbnSpotParsed;
            sources.Add(_rbn);
            owned.Add(_rbn);
        }
        else
        {
            _rbn = null;
        }

        sources.Add(new FakeActivitySource());

        _ownedSources = owned.ToArray();

        var aggregate = new AggregateActivitySource(sources, _settings.IsSourceEnabled);
        aggregate.SetContext(BuildContext());
        return aggregate;
    }

    /// <summary>
    /// What the sources need to know about the operator and the band on
    /// screen.
    /// </summary>
    private ActivityContext BuildContext() => new()
    {
        BandName = SelectedBand.Band.Name,
        BandLowHz = SelectedBand.Band.LowHz,
        BandHighHz = SelectedBand.Band.HighHz,
        HomeDistrict = OperatorLocation.HomeDistrict(_settings.Operator.Location),
        HomeInNorthAmerica =
            OperatorLocation.IsNorthAmerica(_settings.Operator.GridSquare)
            || OperatorLocation.HomeDistrict(_settings.Operator.Location) is not null
            || CallsignRegions.Classify(_settings.Operator.Callsign).Region
                is CallsignRegion.UnitedStates or CallsignRegion.Canada,
    };

    private void DisposeSources()
    {
        foreach (var source in _ownedSources)
        {
            try
            {
                source.Dispose();
            }
            catch (Exception)
            {
                // Tearing down a feed is best-effort; never fatal (§8).
            }
        }

        _ownedSources = Array.Empty<IDisposable>();
    }

    /// <summary>
    /// Window visibility, pushed in by the view. The feed stops polling when
    /// nobody is looking and refreshes the moment the window comes back
    /// (HM-DEC-020) — free today against a fixture, and ordinary politeness
    /// once RBN and POTA are behind the same seam.
    /// </summary>
    /// <param name="visible">True when the window is on screen.</param>
    public void SetWindowVisible(bool visible)
    {
        if (visible == _windowVisible)
        {
            return;
        }

        _windowVisible = visible;
        ApplyFeedTimers();

        // CI-V is a slow bus shared with the radio's own transceive stream, and
        // a minimized window has no S-meter on screen to justify asking for one
        // four times a second. Same politeness the spot feeds observe
        // (HM-DEC-020, HM-DEC-050).
        if (_rigMonitor is not null)
        {
            _rigMonitor.IsWatching = visible;
        }

        // Nothing is watching a hidden window, and twenty-five frames a
        // second of synthesis for nobody is the same rudeness HM-DEC-020
        // named — here it is only the operator's own CPU being spent.
        if (_trainingSpectrum is not null)
        {
            if (visible)
            {
                _trainingSpectrum.Start();
            }
            else
            {
                _trainingSpectrum.Stop();
            }
        }

        if (visible)
        {
            _ = ReloadSpotsAsync("resume");
        }
    }

    /// <summary>Re-read feed settings after the Settings dialog closes.</summary>
    public void ApplyFeedTimers()
    {
        var minutes = _settings.SpotRefreshMinutes;
        var shouldRun = minutes > 0 && _windowVisible;

        _spotRefreshTimer.Stop();
        if (shouldRun)
        {
            _spotRefreshTimer.Interval = TimeSpan.FromMinutes(minutes);
            _spotRefreshTimer.Start();
        }

        // The age line keeps ticking whenever the window is up, even with
        // auto-refresh off: an operator who switched refreshing off still
        // needs to see the data getting old.
        if (_windowVisible)
        {
            if (!_ageTimer.IsEnabled)
            {
                _ageTimer.Start();
            }
        }
        else
        {
            _ageTimer.Stop();
        }

        AppEvents.SpotTimerChanged(_telemetry, shouldRun, minutes);
        UpdateSpotFreshness();
    }

    /// <summary>Show a neighborhood's story in the Explorer card.</summary>
    [RelayCommand]
    private void ShowNeighborhood(Neighborhood hood)
    {
        AppEvents.NeighborhoodClicked(_telemetry, hood.Name);
        StoryTitle = hood.Name;
        StoryBadge = hood.Vibe;
        StoryBody = hood.Blurb;
        StoryTuneHz = hood.JumpHz;
    }

    /// <summary>Show a mode's field-guide story in the Explorer card.</summary>
    [RelayCommand]
    private void ShowMode(ModeInfo mode)
    {
        AppEvents.ModeCardOpened(_telemetry, mode.Name);
        StoryTitle = $"{mode.Name} · {mode.Tagline}";
        StoryBadge = mode.Difficulty;
        StoryBody = $"{mode.Why} Sounds like: {mode.Sound}. Learn its waterfall "
            + "fingerprint and the void stops being static.";
        StoryTuneHz = mode.LivesAt40mHz ?? SelectedBand.Band.JumpHz;
    }

    /// <summary>
    /// Switch the happening-now list between its two questions (HM-DEC-057).
    /// </summary>
    /// <param name="lensName">The lens, as its enum name.</param>
    /// <remarks>
    /// LEAVING "WHAT'S NEW" IS WHAT MOVES THE WATERMARK, not arriving at it.
    /// Moving it on arrival would empty the list the instant somebody opened it,
    /// which is the one thing a delta must not do. So the list stays still while
    /// they are reading it, and is a fresh delta the next time they come back.
    /// </remarks>
    [RelayCommand]
    private void SelectLens(string lensName)
    {
        if (!Enum.TryParse<SpotLens>(lensName, out var lens) || lens == _lens)
        {
            return;
        }

        if (_lens == SpotLens.WhatsNew)
        {
            MarkLookedAtWhatsNew();
        }

        SetLens(lens);
        _lensChosenByOperator = true;

        _settings.SpotLens = lens.ToString();
        SettingsStore.Save(_settings);

        AppEvents.SpotLensChosen(_telemetry, lens.ToString());
        ApplyLens(DateTime.UtcNow);
    }

    /// <summary>
    /// Switch a mode family on or off in the happening-now list (HM-DEC-061).
    /// </summary>
    /// <param name="familyName">The family, as its enum name.</param>
    /// <remarks>
    /// THE CHIPS FILTER AND THEY NEVER DELETE. This is one more view over the
    /// same store the lenses read, so a chip changes what is drawn and changes
    /// nothing about what Hamlet holds.
    /// </remarks>
    [RelayCommand]
    private void ToggleFamily(string familyName)
    {
        if (!Enum.TryParse<ModeFamily>(familyName, out var family)
            || !FamilyFilter.Offered.Contains(family))
        {
            return;
        }

        if (!_families.Remove(family))
        {
            _families.Add(family);
        }

        _settings.SpotFamilies = _families.Select(f => f.ToString()).ToList();
        SettingsStore.Save(_settings);

        AppEvents.SpotFamilyToggled(
            _telemetry, family.ToString(), _families.Contains(family));

        ApplyLens(DateTime.UtcNow);
    }

    /// <summary>Redraw the chips with their counts.</summary>
    /// <remarks>
    /// The count is over everything the lens has rather than over what survives
    /// the filter, which is the whole teaching: somebody who filters to Morse
    /// and still sees forty-one voice stations learns the band is full of people
    /// they could talk to.
    /// </remarks>
    private void RebuildFamilyChips(IEnumerable<ActivitySpot> beforeFiltering)
    {
        FamilyChips.Clear();

        foreach (var chip in FamilyFilter.Chips(beforeFiltering, _families))
        {
            FamilyChips.Add(new FamilyChipViewModel(chip));
        }
    }

    /// <summary>Record that the operator has now seen what was new.</summary>
    private void MarkLookedAtWhatsNew()
    {
        _settings.SpotsLastLookedUtc = DateTime.UtcNow;
        SettingsStore.Save(_settings);
    }

    /// <summary>
    /// Save where the dial is, or un-save it (HM-DEC-060).
    /// </summary>
    /// <remarks>
    /// SAVING CAPTURES CONTEXT AUTOMATICALLY. The frequency, the mode, the band
    /// and what the map says lives there, so a favorite reads "14.074, where the
    /// digital modes gather" with nothing typed. The radio's own memory channels
    /// are numbered slots whose meaning you have to remember, which is the
    /// problem rather than the answer.
    /// </remarks>
    [RelayCommand]
    private void ToggleFavorite()
    {
        var existing = RadioEngine.Explore.Favorites.At(Favorites, FrequencyHz);

        if (existing is not null)
        {
            Favorites.Remove(existing);
            AppEvents.FavoriteRemoved(_telemetry, existing.BandName);
        }
        else
        {
            if (Favorites.Count >= RadioEngine.Explore.Favorites.Maximum)
            {
                StatusText = "That is as many favorites as Hamlet keeps. Remove one "
                           + "from Radio, Manage favorites, and this will save.";
                return;
            }

            var here = Neighborhoods.FirstOrDefault(n => n.Contains(FrequencyHz));

            var favorite = RadioEngine.Explore.Favorites.From(
                FrequencyHz, RigModeText, here, DateTime.UtcNow);

            Favorites.Add(favorite);
            AppEvents.FavoriteSaved(_telemetry, favorite.BandName);
            StatusText = $"Saved as \"{favorite.Name}\".";
        }

        PersistFavorites();
        UpdateFavoriteState();
    }

    /// <summary>Tune to a saved frequency.</summary>
    [RelayCommand]
    private void TuneToFavorite(Favorite? favorite)
    {
        if (favorite is null)
        {
            return;
        }

        AppEvents.FavoriteTuned(_telemetry, favorite.BandName);
        TuneTo(favorite.FrequencyHz);
    }

    /// <summary>Rename, reorder and delete favorites.</summary>
    [RelayCommand]
    private void ManageFavorites()
    {
        var window = new Views.FavoritesWindow
        {
            DataContext = new FavoritesViewModel(
                Favorites, PersistFavorites, Recent, StarRecent),
        };

        if (Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow is { } owner)
        {
            window.ShowDialog(owner);
        }
        else
        {
            window.Show();
        }
    }

    /// <summary>Write the list back to settings.json.</summary>
    private void PersistFavorites()
    {
        _settings.Favorites = Favorites
            .Select(f => new SavedFavorite
            {
                FrequencyHz = f.FrequencyHz,
                Name = f.Name,
                Mode = f.Mode,
                BandName = f.BandName,
                Neighborhood = f.Neighborhood,
                SavedUtc = f.SavedUtc,
            })
            .ToList();

        SettingsStore.Save(_settings);
        RebuildMenus();
        UpdateFavoriteState();
    }

    partial void OnSelectedFavoriteChanged(Favorite? value)
    {
        if (value is null)
        {
            return;
        }

        var picked = value;
        SelectedFavorite = null;
        TuneToFavorite(picked);
    }

    /// <summary>Light or unlight the star for where the dial is now.</summary>
    private void UpdateFavoriteState()
    {
        var here = RadioEngine.Explore.Favorites.At(Favorites, FrequencyHz);

        IsFavorite = here is not null;
        FavoriteLabel = RadioEngine.Explore.Favorites.StarLabel(here);
        FavoriteHere = RadioEngine.Explore.Favorites.NameHere(here);
    }

    /// <summary>Tune the rig (and the whole UI) to a target — the payoff
    /// click on every story and spot.</summary>
    [RelayCommand]
    private void TuneTo(long hz)
    {
        AppEvents.TuneRequested(_telemetry, hz, "story_or_spot");
        var arrivedOn = MarkActedOn(hz);
        var band = BandPlan.BandFor(hz);
        if (band is not null && band.Name != SelectedBand.Band.Name)
        {
            SelectedBand = Bands.First(b => b.Band.Name == band.Name);
        }

        FrequencyHz = hz;

        // AFTER THE MOVE, NEVER BEFORE IT. Switching bands can land the dial on
        // that band's own default on the way past, and the frequency handler
        // expires an arrival that no longer matches where the dial is. Setting
        // this first meant the callsign was thrown away in transit
        // (HM-DEC-072).
        _arrivedOnStation = arrivedOn;
        _arrivedOnHz = arrivedOn.Length > 0 ? hz : -1;
    }

    partial void OnSelectedBandChanged(BandButtonViewModel value)
    {
        Neighborhoods = NeighborhoodPlan.WithEdges(value.Band);

        // A band change is a fresh start rather than a continuation, and
        // somebody who took the wheel on 40 m did not mean to keep it forever
        // (HM-DEC-056).
        _modeFollow = _modeFollow.Rearmed();
        ModeFollowSuspended = false;
        ScheduleModeFollow();

        _settings.LastBand = value.Band.Name;
        SettingsStore.Save(_settings);
        AppEvents.BandChanged(_telemetry, value.Band.Name);

        // The list, the dots, the lead card and the conditions line are all
        // about the band on screen, so changing band re-asks rather than
        // leaving the previous band's answers up.
        _ = ReloadSpotsAsync("band_changed");

        // The training radio synthesises one band at a time, and its signals
        // are placed against that band's neighborhood map.
        if (_trainingSpectrum is not null)
        {
            StartTrainingSpectrum();
        }
    }

    /// <summary>
    /// Tune to a dot on the neighborhood map (HM-DEC-023). Separate from
    /// <see cref="TuneToCommand"/> only so the telemetry can tell a map click
    /// from a card click.
    /// </summary>
    [RelayCommand]
    private void TuneToDot(long hz)
    {
        AppEvents.MapDotTuned(_telemetry, hz);
        TuneTo(hz);
    }

    /// <summary>
    /// Tune to a spot marker on the dial tape. The same spots the map draws,
    /// separated here only so the telemetry can tell the two gestures apart.
    /// </summary>
    [RelayCommand]
    private void TuneToTapeMarker(long hz)
    {
        AppEvents.TapeMarkerTuned(_telemetry, hz);
        TuneTo(hz);
    }

    partial void OnSelectedPortChanged(string value)
    {
        _settings.LastPort = value;
        SettingsStore.Save(_settings);
    }

    partial void OnTransmitExpandedChanged(bool value)
        => PersistPanel(PanelKeys.Transmit, value);

    partial void OnPhrasebookExpandedChanged(bool value)
        => PersistPanel(PanelKeys.Phrasebook, value);

    partial void OnMapExpandedChanged(bool value) => PersistPanel(PanelKeys.Map, value);

    partial void OnTapeExpandedChanged(bool value) => PersistPanel(PanelKeys.Tape, value);

    partial void OnWaterfallExpandedChanged(bool value)
        => PersistPanel(PanelKeys.Waterfall, value);

    partial void OnScanExpandedChanged(bool value) => PersistPanel(PanelKeys.Scan, value);

    partial void OnTerminalExpandedChanged(bool value)
        => PersistPanel(PanelKeys.Terminal, value);

    partial void OnStoryExpandedChanged(bool value) => PersistPanel(PanelKeys.Story, value);

    partial void OnGuideExpandedChanged(bool value) => PersistPanel(PanelKeys.Guide, value);

    partial void OnReceiveHelpExpandedChanged(bool value)
        => PersistPanel(PanelKeys.ReceiveHelp, value);

    partial void OnSpotsExpandedChanged(bool value) => PersistPanel(PanelKeys.Spots, value);

    partial void OnLeadExpandedChanged(bool value) => PersistPanel(PanelKeys.Lead, value);

    partial void OnContactExpandedChanged(bool value) => PersistPanel(PanelKeys.Contact, value);

    private void PersistPanel(string key, bool expanded)
    {
        _settings.SetPanelExpanded(key, expanded);
        SettingsStore.Save(_settings);
        AppEvents.PanelToggled(_telemetry, key, expanded);
    }

    /// <summary>Reload the happening-now feed by hand. Always works, whatever
    /// the interval setting says, and resets the timer (HM-DEC-020).</summary>
    [RelayCommand]
    private async Task RefreshSpotsAsync()
    {
        await ReloadSpotsAsync("manual");
        ApplyFeedTimers();
    }

    /// <summary>Open the settings dialog.</summary>
    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        if (Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow is null)
        {
            return;
        }

        AppEvents.SettingsOpened(_telemetry);
        var window = new Views.SettingsWindow
        {
            DataContext = new SettingsViewModel(_settings, _telemetry),
        };
        await window.ShowDialog(desktop.MainWindow);

        // Source switches and the callsign may both have changed while the
        // dialog was open, and the callsign is RBN's login, so the sources are
        // rebuilt rather than reconfigured.
        _activitySource = BuildSources();
        _ = ReloadSpotsAsync("settings");

        // The callsign, the class or the grid may have changed while the
        // dialog was open, so the lazy resolve gets another chance
        // (HM-DEC-028, HM-DEC-037).
        OnPropertyChanged(nameof(GridProvenance));
        UpdateBandCharacter(DateTime.UtcNow);
        UpdateSpotDistances();
        _ = ResolveProfileAsync();
        UpdatePrivileges();

        // The interval may have changed while the dialog was open.
        ApplyFeedTimers();
    }

    /// <summary>Open %AppData%\Hamlet in the file browser.</summary>
    [RelayCommand]
    private void OpenDataFolder() => SettingsStore.OpenDataFolder();

    /// <summary>Show the About window.</summary>
    [RelayCommand]
    private async Task OpenAboutAsync()
    {
        if (Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow is null)
        {
            return;
        }

        AppEvents.AboutOpened(_telemetry);
        var window = new Views.AboutWindow
        {
            DataContext = new AboutViewModel(_settings, _telemetry),
        };
        await window.ShowDialog(desktop.MainWindow);
    }

    /// <summary>
    /// Point the waterfall at a freshly synthesised band.
    /// </summary>
    /// <remarks>
    /// Rebuilt per band rather than retuned, because the signals are placed
    /// against that band's own neighborhood map — practicing on 20 m has to
    /// teach 20 m (HM-DEC-026).
    /// </remarks>
    /// <summary>
    /// Listen to the radio's own spectrum scope (HM-DEC-062, HM-DEC-005).
    /// </summary>
    /// <remarks>
    /// <para>THE RADIO COMPUTES THIS AND HAMLET DOES NOT. The panadapter is
    /// already running and already band-wide, so the app never computes a
    /// wideband transform the radio has finished.</para>
    /// <para>Listening costs the bus nothing: the radio pushes these once its
    /// own output is switched on, so nothing is polled for and the existing loop
    /// cannot be starved (HM-DEC-050).</para>
    /// <para>Nothing here turns the scope on. That is a write, and Hamlet reads
    /// the two settings and says what is missing instead, because the stream
    /// also needs two radio menu settings there is no command for at all.</para>
    /// </remarks>
    private void StartRigSpectrum(IRig rig)
    {
        StopTrainingSpectrum();

        if (rig is not Ic7300Rig radio || !rig.Capabilities.HasSpectrumScope)
        {
            return;
        }

        _rigSpectrum = new RigSpectrumSource(radio);
        _rigSpectrum.Start();

        SpectrumSource = _rigSpectrum;
        AppEvents.SpectrumSourceChanged(
            _telemetry, "rig", SelectedBand.Band.Name, simulated: false);

        _ = AskForTheSpectrumAsync(radio);
    }

    /// <summary>
    /// Ask the radio to send its spectrum down the cable (HM-DEC-092).
    /// </summary>
    /// <param name="radio">The connected radio.</param>
    /// <returns>A task.</returns>
    /// <remarks>
    /// <para>**THE APPLICATION READ THIS SETTING FOR MONTHS AND NEVER ONCE TRIED
    /// TO SET IT.** It found the output off, printed a paragraph naming two menu
    /// settings it had never read, and stopped. Both were already correct and the
    /// operator walked to the radio for nothing.</para>
    /// <para>`27 11` is send/read and tier one: it decides whether the picture
    /// the radio is already drawing on its own screen is also sent to the
    /// computer, and nothing about it can key anything (HM-DEC-084). The
    /// preconditions in footnote 4 are real and are not grounds to decline in
    /// advance, because one of them Hamlet cannot read at all: attempting it and
    /// reporting the answer replaces a guess with a measurement (§0.0).</para>
    /// <para>Not awaited, for the same reason the startup reconnect is not: a
    /// window that will not paint until the radio answers looks broken to
    /// anybody whose radio is busy.</para>
    /// </remarks>
    private async Task AskForTheSpectrumAsync(Ic7300Rig radio)
    {
        // **READ BEFORE WRITE, WHICH MEANS WAITING FOR THE FIRST READ**
        // (HM-DEC-094, HM-DEC-084). This fired eight tenths of a second after
        // connect with all forty fields still unknown, so it had nothing to read
        // before, could not know whether the setting already had the value it was
        // about to write, and reported "refused" to the operator for a command the
        // radio had not had time to answer.
        if (_rigMonitor is { } monitor)
        {
            var ready = await Task.WhenAny(
                monitor.Populated, Task.Delay(WaitForFirstRead)).ConfigureAwait(true);

            if (ready != monitor.Populated)
            {
                // The radio has not answered anything at all. Writing into that
                // silence would be the same fault one layer along.
                _scopeWriteRefused = false;
                return;
            }
        }

        var already = RigState[RigField.ScopeOutput];

        if (already is { IsKnown: true, Number: > 0 })
        {
            return;
        }

        // WHAT IT WAS BEFORE, SO IT CAN BE PUT BACK. Unknown stays unknown and
        // the restore is simply not offered, rather than a plausible value being
        // written back under the name of restoring (HM-DEC-084).
        _scopeOutputWas = already.IsKnown ? (int?)already.Number : null;

        try
        {
            var result = await radio
                .SetSettingAsync(CivWrites.ScopeOutput, 1)
                .ConfigureAwait(true);

            _scopeWriteRefused = !result.Worked;

            AppEvents.ScopeOutputRequested(
                _telemetry, result.Outcome.ToString(), radio.Link.BaudRate,
                radio.Link.Unanswered);

            Decisions.Note(
                "Scope output",
                result.Worked ? "asked and granted" : "asked and refused",
                result.Worked ? Outcome.Proceeded : Outcome.Failed,
                result.Worked
                    ? "The radio is sending its spectrum to the computer now."
                    : "The radio would not send its spectrum. Hamlet asked rather "
                      + "than assuming which setting was in the way.",
                DateTime.UtcNow);
        }
        catch (Exception)
        {
            // A radio that will not answer is a condition, never a crash (§8).
            _scopeWriteRefused = true;
        }
    }

    /// <summary>
    /// How long to wait for the radio to answer anything before giving up.
    /// </summary>
    /// <remarks>
    /// Five seconds, which is many times a poll sweep and far short of anybody's
    /// patience. A radio that has said nothing in five seconds is not going to
    /// accept a setting either (HM-DEC-094).
    /// </remarks>
    private static readonly TimeSpan WaitForFirstRead = TimeSpan.FromSeconds(5);

    /// <summary>What the scope output was before Hamlet asked, or null.</summary>
    private int? _scopeOutputWas;

    /// <summary>True once the radio has refused to send its spectrum.</summary>
    private bool _scopeWriteRefused;

    /// <summary>
    /// Put the scope output back if Hamlet turned it on (HM-DEC-092).
    /// </summary>
    /// <returns>A task.</returns>
    /// <remarks>
    /// Every write is undoable (HM-DEC-084), and an operator who had this off had
    /// it off for a reason even if the reason was only that they never turned it
    /// on. Where the previous value was never read, nothing is restored, because
    /// writing a plausible number back would be a guess wearing the most
    /// reassuring word in the application.
    /// </remarks>
    private async Task ReleaseTheSpectrumAsync()
    {
        if (_rig is not Ic7300Rig radio || _scopeOutputWas is not { } was)
        {
            return;
        }

        _scopeOutputWas = null;

        try
        {
            await radio.SetSettingAsync(CivWrites.ScopeOutput, was)
                .ConfigureAwait(true);
        }
        catch (Exception)
        {
            // Leaving a receive-side setting on is not worth a crash (§8).
        }
    }

    private void StartTrainingSpectrum()
    {
        StopTrainingSpectrum();

        _trainingSpectrum = new TrainingSpectrumSource(
            SelectedBand.Band, callsign: _settings.Operator.Callsign);
        _trainingSpectrum.Start();

        SpectrumSource = _trainingSpectrum;
        AppEvents.SpectrumSourceChanged(
            _telemetry, "training", SelectedBand.Band.Name, simulated: true);
    }

    /// <summary>
    /// Wipe the terminal, and nothing else.
    /// </summary>
    /// <remarks>
    /// <para>WHAT THIS DOES NOT TOUCH IS THE POINT (HM-DEC-051). Tuning around
    /// leaves a pile of half-decoded garbage above whatever is arriving now, and
    /// there was no way to start fresh. So this clears what is displayed, and
    /// stops there.</para>
    /// <para>The decoder keeps running. It keeps its speed estimate, its
    /// adapted noise floor and its tone tracking, because those took real
    /// seconds of signal to arrive at and throwing them away mid-decode is
    /// exactly what nobody wants while chasing a marginal one. A clear that
    /// quietly reset the decoder would look like the app losing the signal at
    /// the moment the operator asked for a tidy screen.</para>
    /// </remarks>
    [RelayCommand]
    private void ClearTerminal()
    {
        Transcript.Clear();
        OnPropertyChanged(nameof(TerminalSummary));
    }

    /// <summary>
    /// Open the screen that says what the radio is doing.
    /// </summary>
    /// <remarks>
    /// Under Tools rather than buried, because the moment somebody needs it is
    /// the moment something is wrong and they are already frustrated
    /// (HM-DEC-050).
    /// </remarks>
    [RelayCommand]
    private void OpenRigDiagnostics()
    {
        var owner = (Application.Current?.ApplicationLifetime
            as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (owner is null)
        {
            return;
        }

        var window = new Views.RigDiagnosticsWindow
        {
            DataContext = new RigDiagnosticsViewModel(_rigMonitor, RigState),
        };

        AppEvents.RigDiagnosticsOpened(_telemetry, RigState.KnownCount);
        window.ShowDialog(owner);
    }

    /// <summary>
    /// Begin keeping track of what the radio is doing.
    /// </summary>
    /// <remarks>
    /// The monitor polls on its own thread and raises state changes from it, so
    /// everything it hands over is marshalled onto the UI thread here rather
    /// than each surface remembering to.
    /// </remarks>
    private void StartRigMonitor(IRig rig)
    {
        StopRigMonitor();

        _rigMonitor = new RigStateMonitor(rig);
        _rigMonitor.StateChanged += OnRigStateChanged;
        _rigMonitor.IsWatching = _windowVisible;
        _rigMonitor.Start();
    }

    private void StopRigMonitor()
    {
        if (_rigMonitor is null)
        {
            return;
        }

        _rigMonitor.StateChanged -= OnRigStateChanged;
        _rigMonitor.Dispose();
        _rigMonitor = null;

        // Back to knowing nothing, rather than leaving the last radio's
        // readings on screen as though they were this one's (§0.0).
        ApplyRigState(RigState.Empty);
    }

    /// <summary>
    /// THE SEAM WHERE RIG STATE ENTERS THE UI, AND THE ONLY ONE (HM-DEC-078).
    /// </summary>
    /// <remarks>
    /// <para>The monitor raises this from the serial read loop, so everything
    /// downstream would otherwise be touching bindable properties from a
    /// background thread. Marshalling here rather than at each consumer means a
    /// property added next month is safe without anybody remembering, and there
    /// is one line to read to know that it is.</para>
    /// <para>It fires every poll cycle whether anything changed or not, four
    /// times a second, which is why everything it reaches has to be cheap and
    /// idempotent. A consumer that rebuilds controls on this cadence destroys
    /// them out from under the operator's finger, and that is exactly what
    /// killed two live attempts (HM-DEC-078).</para>
    /// </remarks>
    private void OnRigStateChanged(object? sender, RigStateChangedEventArgs e)
        => Dispatcher.UIThread.Post(() => ApplyRigState(e.State));

    /// <summary>
    /// Push what the radio said onto the surfaces that show it.
    /// </summary>
    /// <remarks>
    /// Every one of these is empty or null when the value is not known, and
    /// none of them substitutes a default. That is the whole point of the model
    /// underneath (HM-DEC-050).
    /// </remarks>
    private void ApplyRigState(RigState state)
    {
        // EVERYTHING BELOW TOUCHES BINDABLE STATE, so arriving here off the UI
        // thread would mean notifications the framework may silently drop
        // (HM-DEC-078). The one caller posts; this is what keeps a second caller
        // from quietly not doing so.
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => ApplyRigState(state));
            return;
        }

        NoticeOperatorModeChange(state);

        RigModeText = state[RigField.Mode] is { IsKnown: true } mode ? mode.Text : "";
        RigFilterText = state[RigField.FilterSelection] is { IsKnown: true } filter
            ? filter.Text
            : "";

        SMeterLevel = state.SMeterFraction;

        FilterBandwidthText = state[RigField.FilterBandwidth] is { IsKnown: true } width
            ? width.Text
            : "";

        OnPropertyChanged(nameof(RigState));
        OnPropertyChanged(nameof(TerminalSummary));

        // A waterfall that sat empty without saying why would be the app looking
        // broken while the answer was four menu screens away (HM-DEC-062). The
        // sweep count goes in because settings that read as on and a waterfall
        // that stays blank is the case somebody actually sits and stares at
        // (HM-DEC-067).
        var scope = ScopeReadiness.Check(
            _rig?.Capabilities, state, _rigSpectrum?.SweepCount ?? -1,
            (_rig as Ic7300Rig)?.Link, _scopeWriteRefused);

        ScopeNote = _rig is null || _rig.IsSimulated || scope.IsReady
            ? ""
            : string.IsNullOrEmpty(scope.WhereToLook)
                ? scope.Detail
                : $"{scope.Detail} {scope.WhereToLook}";

        OnPropertyChanged(nameof(WaterfallSummary));
        OnPropertyChanged(nameof(ScopeStages));
        OnPropertyChanged(nameof(HasScopeStages));
        OnPropertyChanged(nameof(ScopeRejection));
        OnPropertyChanged(nameof(HasScopeRejection));

        // The break-in setting and the mode both live in here, and both decide
        // whether a send would reach the air. So the panel re-asks whenever the
        // radio says anything, and somebody reads "break-in is off" before they
        // press rather than after (HM-DEC-059).
        Transmit.Refresh();
        ReceiveHelp.Refresh();
    }

    /// <summary>
    /// A mode Hamlet did not ask for is the operator's own hand, so it stands
    /// down until the next band change.
    /// </summary>
    /// <remarks>
    /// SUSPENDED IS A VISIBLE STATE AND NEVER A SILENT ONE (HM-DEC-056). An app
    /// that quietly stopped doing a thing it had been doing is worse than one
    /// that never did it, because the operator has no way to tell whether it is
    /// standing down or broken. So it says so, once, in the status line.
    /// </remarks>
    private void NoticeOperatorModeChange(RigState state)
    {
        if (_settingModeOurselves
            || _modeFollow.Suspended
            || !_modeFollow.Enabled
            || state[RigField.Mode] is not { IsKnown: true } mode)
        {
            return;
        }

        var was = _lastKnownMode;
        _lastKnownMode = (CivMode)(int)mode.Number!.Value;

        // The first reading of a session is not a change; it is the answer to
        // Hamlet asking what mode the radio was already in.
        if (was is null || was == _lastKnownMode)
        {
            return;
        }

        _modeFollow = _modeFollow.SuspendedByOperator();
        ModeFollowSuspended = true;
        StatusText = $"You set the radio to {mode.Text}, so Hamlet will leave the "
                   + "mode alone until you next change band.";
    }

    /// <summary>
    /// True while the operator is driving the mode and Hamlet is not.
    /// </summary>
    /// <remarks>
    /// Shown on screen rather than kept in a field, because the operator always
    /// has to know who is driving (HM-DEC-056).
    /// </remarks>
    [ObservableProperty]
    private bool _modeFollowSuspended;

    /// <summary>
    /// Start listening, and decoding what is heard.
    /// </summary>
    /// <remarks>
    /// <para>The training radio makes its own Morse, so somebody with no
    /// hardware at all still gets a working terminal (HM-DEC-026). A real radio
    /// gets whichever capture device the operator chose, which on a connected
    /// IC-7300 is its own USB codec.</para>
    /// <para>A machine with no sound device, or one that refuses to open, leaves
    /// the terminal saying it is not listening. Nothing here throws: the
    /// Explorer, the map and the training radio all work perfectly well without
    /// audio, and refusing to start would be a spectacular punishment for an
    /// unplugged cable (§8).</para>
    /// </remarks>
    private void StartDecoding()
    {
        StopDecoding();

        try
        {
            _audioInput = OpenAudioInput();
        }
        catch (Exception)
        {
            _audioInput = null;
        }

        if (_audioInput is null)
        {
            AudioInputName = "";
            IsDecoding = false;
            return;
        }

        // WHAT WINDOWS IS DOING TO THE INPUT, read once when listening starts
        // (HM-DEC-088). It is a third gain nobody can see, after the radio's
        // speaker level and its quite separate USB output level.
        _capture = WasapiAudioDevices.Health(_settings.AudioInputDeviceId);

        _decoder = new CwDecoder(_audioInput.SampleRate, _settings.CwPitchHz);
        _decoder.CharacterDecoded += Transcript.Append;

        // WHEN SOMETHING LAST CAME THROUGH, which is what the quiet offer waits
        // on (HM-DEC-084). Set here rather than polled, so an empty terminal is
        // measured from the last real character rather than from a timer.
        _decoder.CharacterDecoded += _ => _lastDecodeUtc = DateTime.UtcNow;
        _lastDecodeUtc = DateTime.UtcNow;
        _decoder.Listen(_audioInput);
        _audioInput.Start();

        AudioInputName = _audioInput.DeviceName;
        IsDecoding = true;
        _decodeTimer.Start();

        AppEvents.DecoderStarted(
            _telemetry, _audioInput.IsSimulated, _audioInput.SampleRate, _settings.CwPitchHz);
    }

    /// <summary>
    /// The source to listen to, or null when there is nothing to listen with.
    /// </summary>
    private IAudioSource? OpenAudioInput()
    {
        if (_rig?.IsSimulated == true)
        {
            // Real Morse at a known speed, with nothing plugged in. Twelve words
            // a minute is a patient operator, which is where somebody learning
            // to copy should start (HM-DEC-026).
            return new TrainingAudioSource(
                MorseCode.CqCall(_settings.Operator.Callsign),
                wordsPerMinute: 12,
                toneHz: _settings.CwPitchHz);
        }

        var device = AudioDeviceChoice.Choose(
            new WasapiAudioDevices().List(), _settings.AudioInputDeviceId);

        return device is null ? null : new WasapiAudioSource(device);
    }

    /// <summary>Stop listening and put the decoder away.</summary>
    private void StopDecoding()
    {
        _decodeTimer.Stop();

        if (_decoder is not null)
        {
            _decoder.CharacterDecoded -= Transcript.Append;
            _decoder.Listen(null);
            _decoder = null;
        }

        _audioInput?.Stop();
        _audioInput?.Dispose();
        _audioInput = null;

        IsDecoding = false;
        DetectedWpm = 0;
        DecodeNote = "";
        AudioInputName = "";
        Transcript.Clear();
        OnPropertyChanged(nameof(TerminalSummary));
    }

    /// <summary>
    /// Bring the readouts up to date with what the decoder is doing.
    /// </summary>
    private void OnDecodeTick(object? sender, EventArgs e)
    {
        if (_decoder is null)
        {
            return;
        }

        // ONE GUARDED ANSWER (HM-DEC-090). Zero means nothing has earned the
        // right to name a speed, and every surface that shows one reads this.
        DetectedWpm = _decoder.WordsPerMinute ?? 0;
        DecodeNote = _decoder.Watch.NoteText;

        // WHAT IS ARRIVING, WHETHER OR NOT ANYTHING DECODES (HM-DEC-088). A
        // strong signal that will not resolve and an empty band used to produce
        // the same screen, and they are different problems.
        DecodeReport = _decoder.Report;

        // **A STALLED AUDIO PIPELINE USED TO LOOK EXACTLY LIKE A QUIET BAND**
        // (HM-DEC-090). Nothing anywhere said the samples had stopped arriving,
        // so the capture went on handing over the same thirty seconds and the
        // decoder went on reporting what it made of them. Whatever stops the
        // stream, this notices within a couple of seconds and says so (§0.0.1).
        var seen = _decoder.Tap.SamplesSeen;

        if (seen != _lastSamplesSeen)
        {
            _lastSamplesSeen = seen;
            _audioLastMovedUtc = DateTime.UtcNow;
        }

        AudioHasStalled = seen > 0
            && DateTime.UtcNow - _audioLastMovedUtc > AudioStallAfter;
        OnPropertyChanged(nameof(TerminalSummary));
        OnPropertyChanged(nameof(InputLevelText));
        OnPropertyChanged(nameof(InputLevelFraction));
        OnPropertyChanged(nameof(DecoderStory));
        OnPropertyChanged(nameof(HasDecoderStory));
        OnPropertyChanged(nameof(CaptureNote));
        OnPropertyChanged(nameof(HasCaptureNote));

        NoteDecodeQuality();

        // OFFERED, NEVER ASSERTED (HM-DEC-059, HM-OPEN-006). The decoder
        // measured what the other station is sending at, so Hamlet may say so.
        // It has never asked what speed this operator can copy, so it may not
        // claim that this one suits them.
        // NO DECODE MEANS NO STATION, SO THE LINE IS ABSENT (HM-DEC-090). It
        // read "they are sending at about 62 words a minute" with nobody
        // sending, which is the phantom speed reaching a third surface.
        Transmit.HeardWpm = _decoder.WordsPerMinute;
    }

    /// <summary>
    /// True once the operator has tuned with the wheel (HM-DEC-088).
    /// </summary>
    /// <remarks>
    /// Persisted, because a hint that came back every launch would not have
    /// retired at all.
    /// </remarks>
    [ObservableProperty]
    private bool _hasTunedByWheel;

    partial void OnHasTunedByWheelChanged(bool value)
    {
        if (value && !_settings.HasTunedByWheel)
        {
            _settings.HasTunedByWheel = true;
            SettingsStore.Save(_settings);
        }
    }

    /// <summary>
    /// True when audio has stopped arriving while the decoder is listening
    /// (HM-DEC-090).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InputLevelText))]
    private bool _audioHasStalled;

    /// <summary>How long silence from the sound card counts as a stall.</summary>
    /// <remarks>
    /// Two seconds. Audio arrives in chunks many times a second, so nothing
    /// legitimate is quiet for that long, and a shorter window would cry wolf
    /// over ordinary scheduling.
    /// </remarks>
    private static readonly TimeSpan AudioStallAfter = TimeSpan.FromSeconds(2);

    private long _lastSamplesSeen = -1;
    private DateTime _audioLastMovedUtc = DateTime.UtcNow;

    /// <summary>What the decoder is hearing and making of it (HM-DEC-088).</summary>
    [ObservableProperty]
    private CwDecodeReport _decodeReport = CwDecodeReport.None;

    /// <summary>
    /// The input level in words, always, even when nothing is decoding.
    /// </summary>
    /// <remarks>
    /// **THIS IS THE ONE-GLANCE ANSWER TO THE WHOLE COMPLAINT.** If the level is
    /// down at the bottom while the operator is listening to a perfectly good
    /// signal, the two audio paths have come apart and no amount of decoder work
    /// will fix it.
    /// </remarks>
    public string InputLevelText
    {
        get
        {
            if (!IsDecoding)
            {
                return "";
            }

            if (AudioHasStalled)
            {
                return "audio has stopped arriving";
            }

            var level = DecodeReport.Level;

            if (level.Clipping)
            {
                return "input overloading";
            }

            if (level.NearlySilent)
            {
                return "almost nothing arriving";
            }

            return $"input peaking at {level.PeakDb:0} dB, noise around "
                + $"{level.FloorDb:0} dB";
        }
    }

    /// <summary>The input level as a bar, zero to one.</summary>
    public double InputLevelFraction
    {
        get
        {
            var peak = DecodeReport.Level.PeakDb;

            return Math.Clamp(
                (peak - AudioLevel.SilenceDb)
                    / (AudioLevel.FullScaleDb - AudioLevel.SilenceDb),
                0, 1);
        }
    }

    /// <summary>What the decoder can see, when it is producing nothing.</summary>
    public string DecoderStory => CwDecodeStory.Describe(DecodeReport, IsDecoding);

    /// <summary>True when there is something to say about it.</summary>
    public bool HasDecoderStory => DecoderStory.Length > 0;

    /// <summary>What Windows is doing to the input, where it could be read.</summary>
    public string CaptureNote => CaptureAdvice.Describe(_capture);

    /// <summary>True when Windows is doing something worth saying.</summary>
    public bool HasCaptureNote => CaptureNote.Length > 0;

    /// <summary>The standing note about enhancements, which Hamlet cannot read.</summary>
    public static string EnhancementsNote => CaptureAdvice.EnhancementsNote;

    private CaptureHealth _capture = CaptureHealth.Unknown;

    /// <summary>The last decode-quality figures written, so an unchanged one is not.</summary>
    private CwDecodeReport _lastQuality = CwDecodeReport.None;

    /// <summary>When the last one was written.</summary>
    private DateTime _lastQualityUtc = DateTime.MinValue;

    /// <summary>
    /// How rarely the decode-quality figures may be written.
    /// </summary>
    /// <remarks>
    /// **THE LAST TELEMETRY FILE WROTE THE SAME UNCHANGED STATE TWICE PER MORSE
    /// ELEMENT** and buried everything that mattered under it (HM-DEC-077). Ten
    /// seconds, and only when something actually moved.
    /// </remarks>
    private static readonly TimeSpan QualityInterval = TimeSpan.FromSeconds(10);

    /// <summary>Put the decode-quality figures in the record, rarely.</summary>
    private void NoteDecodeQuality()
    {
        var now = DateTime.UtcNow;

        if (now - _lastQualityUtc < QualityInterval)
        {
            return;
        }

        var report = DecodeReport;

        // WHAT DECIDES IT IS WHICH NUMBERS MOVED, which is why the rate limit
        // lives here and not inside the event (§8.1).
        var moved = report.CharactersEmitted != _lastQuality.CharactersEmitted
            || report.HasTone != _lastQuality.HasTone
            || report.Clipping != _lastQuality.Clipping
            || report.NearlySilent != _lastQuality.NearlySilent
            || Math.Abs(report.Level.PeakDb - _lastQuality.Level.PeakDb) >= 3;

        if (!moved)
        {
            return;
        }

        _lastQualityUtc = now;
        _lastQuality = report;

        AppEvents.DecodeQuality(_telemetry, report, "sampled");
    }

    /// <summary>
    /// Keep the last half minute the decoder heard, as a file (HM-DEC-088).
    /// </summary>
    /// <remarks>
    /// <para>**EVERYTHING ELSE ABOUT THE FAINT-SIGNAL PROBLEM IS A HYPOTHESIS
    /// UNTIL ONE OF THESE EXISTS.** A wrong decode with its input attached is a
    /// regression test; a wrong decode without one is an argument that runs for
    /// three sessions (§0.0.1, HM-DEC-007).</para>
    /// <para>The state at the moment of capture is written beside it, because a
    /// recording whose filter width and keyer speed are unknown can be listened
    /// to and cannot be reasoned about.</para>
    /// </remarks>
    [RelayCommand]
    private void CaptureAudio()
    {
        var tap = _decoder?.Tap;
        var audio = tap?.Snapshot();

        if (tap is null || audio is null)
        {
            StatusText = "There is no audio to keep just now.";
            AppEvents.AudioCaptured(_telemetry, 0, FrequencyHz, worked: false);
            return;
        }

        // **A CAPTURE THAT CANNOT PROVE IT IS FRESH IS NOT WRITTEN**
        // (HM-DEC-090). Three presses inside seventy seconds produced
        // byte-identical files with identical analysis, beside rig state that
        // differed on every one, and the operator reasoned from one recording
        // presented as three. Evidence that looks specific and is not is worse
        // than no evidence at all (§0.0.1).
        var seen = tap.SamplesSeen;

        if (seen == _lastCaptureSamples)
        {
            StatusText =
                "No new audio has arrived since the last time you kept some, so "
                + "there is nothing fresh to write. The recording would have been "
                + "the same file over again.";

            AppEvents.AudioCaptured(_telemetry, 0, FrequencyHz, worked: false);
            return;
        }

        try
        {
            var folder = Path.Combine(SettingsStore.DataFolder, "captures");
            Directory.CreateDirectory(folder);

            var stamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HHmmss");
            var wav = Path.Combine(folder, $"cw-{stamp}.wav");

            WavAudio.Write(wav, audio);
            File.WriteAllText(
                Path.Combine(folder, $"cw-{stamp}.txt"),
                CaptureNotes(audio, seen));

            _lastCaptureSamples = seen;

            StatusText =
                $"Kept the last {audio.Duration.TotalSeconds:0} seconds of what the "
                + "decoder heard, with what the radio was doing beside it.";

            AppEvents.AudioCaptured(
                _telemetry, audio.Duration.TotalSeconds, FrequencyHz, worked: true);
        }
        catch (Exception)
        {
            // A capture that cannot be written loses a recording and nothing
            // else (§8).
            StatusText = "Hamlet could not write the recording.";
            AppEvents.AudioCaptured(_telemetry, 0, FrequencyHz, worked: false);
        }
    }

    /// <summary>How much audio had arrived when the last capture was written.</summary>
    private long _lastCaptureSamples = -1;

    /// <summary>Everything worth knowing about a capture, beside it.</summary>
    /// <param name="audio">What was written.</param>
    /// <param name="samplesSeen">How much audio has ever arrived.</param>
    private string CaptureNotes(MonoAudio audio, long samplesSeen)
    {
        var state = RigState;
        var report = DecodeReport;

        var lines = new List<string>
        {
            $"captured   {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",

            // TWO FIGURES THAT MAKE A FROZEN CAPTURE OBVIOUS (HM-DEC-090). The
            // running total says whether any audio arrived since last time, and
            // the fingerprint says whether this is the same recording, without
            // anybody having to compare files by hand.
            $"audioSeen  {samplesSeen} samples",
            $"fingerprint {Fingerprint(audio)}",
            $"seconds    {audio.Duration.TotalSeconds:0.0}",
            $"sampleRate {audio.SampleRate}",
            // **ONE SOURCE, AND IT SAYS WHICH** (HM-DEC-091). This line read
            // 7.030 MHz in a file whose own rig block, four lines further down,
            // read 14.055: the header took the app's idea of where it was tuned
            // and the block took the radio's. Where the radio has been read, the
            // radio is the answer, and where it has not, the header says so
            // rather than presenting a guess in the same shape as a measurement.
            $"frequency  {CapturedFrequency()}",

            // **AND THE BAND COMES FROM THE SAME READING AS THE FREQUENCY**
            // (HM-DEC-096, phase 6). This line took the band button the operator
            // last pressed, which is the app's idea of where it is rather than
            // the radio's, and it is how three captures came to say 40 m in a
            // header whose own rig block read 14.055 MHz. Two fields describing
            // one fact from two sources is the fault, not the wrong value.
            $"band       {CapturedBand()}",
            "",
            // THE RECORDING'S OWN PEAK, not the meter's last fifth of a second
            // (HM-DEC-094). Those differed by eight decibels on a file that was
            // nearly clipping while the sidecar said there was headroom.
            $"inputPeak  {AudioTap.PeakOf(audio):0.0} dBFS  (over the whole recording)",
            $"meterPeak  {report.Level.PeakDb:0.0} dBFS  (the moment it was kept)",
            $"inputFloor {report.Level.FloorDb:0.0} dBFS",
            $"clipping   {report.Clipping}",
            $"toneHz     {(report.HasTone ? report.ToneHz.ToString("0") : "none")}",
            $"snrDb      {(double.IsNaN(report.SnrDb) ? "unread" : report.SnrDb.ToString("0.0"))}",
            $"elements   {report.ElementsSeen} seen, {report.ElementsResolved} resolved",
            $"characters {report.CharactersEmitted} emitted, {report.CharactersUnsure} unsure",
            "",
        };

        // WHAT THE DECODER HAS DONE SINCE THE LAST CAPTURE, beside the totals.
        // The totals are cumulative over the whole session, so two captures
        // showing the same ones mean nothing was decoded in between, and a reader
        // should not have to work that out by subtraction.
        lines.Add(
            $"sinceLast  {report.CharactersEmitted - _lastCaptureCharacters} characters, "
            + $"{report.ElementsSeen - _lastCaptureElements} elements");

        lines.Add("");

        _lastCaptureCharacters = report.CharactersEmitted;
        _lastCaptureElements = report.ElementsSeen;

        // EVERY FIELD WITH ITS PROVENANCE, and unread stays unread rather than
        // becoming a zero somebody later reasons from (HM-DEC-050).
        foreach (var value in state.All())
        {
            lines.Add(
                $"{value.Field,-20} "
                + (value.IsKnown
                    ? value.Text
                    : value.State.ToString().ToLowerInvariant()));
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Where the radio actually is, for the capture header (HM-DEC-091).
    /// </summary>
    /// <returns>The frequency and where it came from.</returns>
    /// <remarks>
    /// The stale-frequency fault has now appeared four times in this project, and
    /// every one of them was two sources for one fact. The radio's own reading
    /// wins whenever there is one; Hamlet's own is labeled as Hamlet's.
    /// </remarks>
    private string CapturedFrequency()
    {
        var read = RigState[RigField.Frequency];

        return read is { IsKnown: true, Number: { } hz }
            ? $"{(long)hz} Hz  (read from the radio)"
            : $"{FrequencyHz} Hz  (Hamlet's own, the radio was not read)";
    }

    /// <summary>
    /// Which band the capture was made on, derived from the frequency that was
    /// actually read (HM-DEC-096, phase 6).
    /// </summary>
    /// <returns>The band's name, with where it came from.</returns>
    /// <remarks>
    /// **DERIVED, NOT SELECTED.** The band a capture was made on is a fact about
    /// the frequency, and the frequency is a fact about the radio. Taking it from
    /// the button the operator last pressed makes it a fact about the app, which
    /// is how a header came to disagree with the rig block four lines under it.
    /// Where the radio has not been read, this says so rather than presenting a
    /// guess in the same shape as a measurement (§0.0).
    /// </remarks>
    private string CapturedBand()
    {
        var read = RigState[RigField.Frequency];

        if (read is { IsKnown: true, Number: { } hz })
        {
            return BandPlan.BandFor((long)hz) is { } band
                ? $"{band.Name}  (from the frequency the radio reported)"
                : "outside every band Hamlet knows";
        }

        return $"{SelectedBand.Band.Name}  (Hamlet's own, the radio was not read)";
    }

    /// <summary>What the decoder had emitted at the last capture.</summary>
    private int _lastCaptureCharacters;

    /// <summary>What the decoder had measured at the last capture.</summary>
    private int _lastCaptureElements;

    /// <summary>
    /// A short fingerprint of the audio, so two identical captures are visibly
    /// identical (HM-DEC-090).
    /// </summary>
    /// <param name="audio">The recording.</param>
    /// <returns>Twelve hexadecimal characters.</returns>
    private static string Fingerprint(MonoAudio audio)
    {
        var bytes = new byte[audio.Samples.Length * sizeof(float)];
        Buffer.BlockCopy(audio.Samples, 0, bytes, 0, bytes.Length);

        return Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(bytes))[..12].ToLowerInvariant();
    }

    /// <summary>Stop listening to the radio's scope.</summary>
    private void StopRigSpectrum()
    {
        if (_rigSpectrum is null)
        {
            return;
        }

        _rigSpectrum.Stop();
        _rigSpectrum.Dispose();
        _rigSpectrum = null;

        if (SpectrumSource is RigSpectrumSource)
        {
            SpectrumSource = null;
        }
    }

    private void StopTrainingSpectrum()
    {
        if (_trainingSpectrum is null)
        {
            return;
        }

        SpectrumSource = null;
        _trainingSpectrum.Dispose();
        _trainingSpectrum = null;
    }

    /// <summary>
    /// Play one of the field guide's generated samples.
    /// </summary>
    /// <param name="sample">Which sample the operator asked for.</param>
    /// <remarks>
    /// Fire-and-forget on purpose: generation runs off the UI thread inside
    /// the player, and a field guide that froze while it built six seconds of
    /// SSB would be teaching patience rather than radio (HM-DEC-027).
    /// </remarks>
    [RelayCommand]
    private async Task PlaySampleAsync(ModeSampleButton? sample)
    {
        if (sample is null)
        {
            return;
        }

        AppEvents.ModeSamplePlayed(
            _telemetry, sample.Request.Mode.ToString(), sample.Request.WordsPerMinute);

        await _audio.PlayAsync(sample.Request);
    }

    /// <summary>Stop any sample that is playing.</summary>
    [RelayCommand]
    private void StopSample() => _audio.Stop();

    /// <summary>
    /// Release the training radio and the audio device on the way out.
    /// </summary>
    /// <remarks>
    /// Called from the shell's shutdown path. Leaving a sample playing after
    /// the window closes would be a small thing that feels broken.
    /// </remarks>
    public void ShutDownTraining()
    {
        StopRigMonitor();
        StopDecoding();
        StopTrainingSpectrum();
        _audio.Dispose();

        // The history store holds a file handle, so it closes with the window
        // rather than waiting for the process to end (HM-DEC-045).
        _spotStore.Dispose();
    }

    /// <summary>Close the app.</summary>
    [RelayCommand]
    private void Exit()
    {
        if (Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    /// <summary>Jump to a band's CW watering hole.</summary>
    [RelayCommand]
    private void SelectBand(BandButtonViewModel band)
    {
        SelectedBand = band;
        FrequencyHz = band.Band.JumpHz;
    }

    [RelayCommand]
    private async Task ToggleConnectAsync()
    {
        if (IsConnected)
        {
            await TearDownRigAsync();
            StatusText = "Disconnected";
            return;
        }

        await ConnectToAsync(SelectedPort);
    }

    /// <summary>
    /// Connect to one port, reporting what happened in the status line.
    /// </summary>
    /// <param name="port">A COM port name, or the training radio entry.</param>
    /// <param name="remember">
    /// Whether this port becomes the one to reconnect to next time. False for
    /// the startup fallback: landing on the training radio because a COM port
    /// was missing must not erase the radio the operator actually owns, or one
    /// evening with the rig switched off would quietly cost them the setting.
    /// </param>
    /// <returns>True when the radio answered.</returns>
    /// <remarks>
    /// Shared by the Connect button and the startup reconnect, so the two
    /// cannot drift into connecting differently (HM-DEC-052).
    /// </remarks>
    private async Task<bool> ConnectToAsync(string port, bool remember = true)
    {
        var rig = CreateRig(port);
        var rigType = port == TrainingRadio ? "simulated" : "IC-7300";
        StatusText = $"Connecting to {port}…";

        if (!await rig.ConnectAsync())
        {
            (rig as IDisposable)?.Dispose();
            AppEvents.ConnectFailed(_telemetry, port, rigType, "no_response");
            // THE THIRD CAUSE IS THE ONE NOBODY GUESSES (HM-DEC-069). The
            // radio has a single setting for what leaves its USB port, and set
            // to RTTY Decode that port carries decoded text instead of control
            // messages. Every frame Hamlet sends is then correct and answered
            // by nothing, which looks exactly like a bad cable (§0.0.1).
            StatusText = $"No answer on {port}. It is usually the cable, the baud "
                       + "rate or the CI-V address, and there is one more that "
                       + "catches people out: with USB Serial Function set to RTTY "
                       + "Decode, that port carries decoded text rather than "
                       + "control messages and nothing Hamlet asks will be answered";
            return false;
        }

        var keep = _settings.LastPort;
        SelectedPort = port;

        if (!remember && keep != port)
        {
            // The dropdown shows where Hamlet ended up, and the file still holds
            // where it was trying to go.
            _settings.LastPort = keep;
            SettingsStore.Save(_settings);
        }

        _rig = rig;
        AppEvents.ConnectOk(_telemetry, SelectedPort, rigType);
        rig.FrequencyChanged += OnRigFrequencyChanged;
        IsConnected = true;
        ConnectButtonText = "Disconnect";

        // Connection state IS the mode (HM-DEC-026). A simulated rig gets the
        // synthesiser and a real one gets the radio's own scope, and neither
        // source has a setter for whether it is simulated: the waterfall's label
        // is read off whichever one is attached, so real data arriving cannot
        // weaken it and synthetic data cannot arrive unlabeled (HM-DEC-062).
        if (rig.IsSimulated)
        {
            StartTrainingSpectrum();
        }
        else
        {
            StartRigSpectrum(rig);
        }

        // Audio, on the other hand, both radios can supply: the training radio
        // makes its own Morse and a real one arrives through the capture device
        // the operator chose. So the terminal fills in either way.
        StartDecoding();
        StartRigMonitor(rig);

        // The one door to the transmitter, and it only exists while a radio is
        // connected (§0.2, HM-DEC-059).
        Transmit.Attach(new CwTransmitter(new KeyerCwSender(rig)));

        // THE SCANNER GETS ITS RADIO, AND THE DIAL GOES BACK IF A SCAN DIED
        // MID-RUN (§0.2.1). The note is written before the first tune for
        // exactly this case, and connecting is the only moment Hamlet can act
        // on it: until there is a radio there is nothing to put back.
        Scan.Attach(rig, _rigMonitor, _decoder, SpectrumSource);
        await Scan.RestoreHomeAsync();

        var hz = await rig.GetFrequencyHzAsync();
        ApplyRigFrequency(hz);
        StatusText = port == TrainingRadio
            ? "On the training radio, with synthesised signals and nothing on the air"
            : $"Connected to the IC-7300 on {port}";

        return true;
    }

    /// <summary>
    /// Reconnect to the last radio when the app opens, if that is wanted.
    /// </summary>
    /// <remarks>
    /// <para>FAILS QUIETLY AND LEGIBLY, because failing is the normal case
    /// (HM-DEC-052). A radio switched off, unplugged, or on a COM port Windows
    /// has renumbered is what happens every time somebody opens Hamlet at their
    /// desk rather than in the shack. None of that is an error and none of it
    /// gets a dialog: the status line says what happened, in a sentence, and
    /// the app carries on with the training radio so it is still worth having
    /// open.</para>
    /// <para>A MISSING PORT IS NAMED SPECIFICALLY, because renumbering is the
    /// single most common cause and saying "COM3 isn't on this computer any
    /// more" saves somebody twenty minutes of checking a cable that was fine.
    /// </para>
    /// <para>Once, and never in a loop. If the radio arrives later the operator
    /// can click Connect, and a background loop reopening a COM port is exactly
    /// the kind of thing that upsets other software sharing it.</para>
    /// </remarks>
    public async Task ReconnectOnStartupAsync()
    {
        // Nothing here is allowed to take the app down with it (§8). This runs
        // unawaited off the window's Opened event, so an exception escaping it
        // would surface as a crash with no stack anybody could connect to the
        // radio being unplugged.
        try
        {
            await ReconnectCoreAsync().ConfigureAwait(true);
        }
        catch (Exception)
        {
            StatusText = ReconnectPlan.CouldNotOpen();
        }
    }

    private async Task ReconnectCoreAsync()
    {
        var plan = ReconnectPlan.Decide(
            _settings.ReconnectOnStartup,
            IsConnected,
            _settings.LastPort,
            AvailablePorts,
            TrainingRadio);

        switch (plan.Step)
        {
            case ReconnectStep.Nothing:
                return;

            case ReconnectStep.TrainingRadio:
                if (plan.Explanation is not null)
                {
                    AppEvents.ConnectFailed(
                        _telemetry, _settings.LastPort ?? string.Empty,
                        "IC-7300", "port_absent");
                }

                await ConnectToAsync(plan.Port, remember: plan.Explanation is null);

                if (plan.Explanation is not null)
                {
                    StatusText = plan.Explanation;
                }

                return;

            case ReconnectStep.RememberedPort:
                if (await ConnectToAsync(plan.Port))
                {
                    return;
                }

                // Once, and never in a loop. The radio is off, and a background
                // retry reopening a COM port is exactly what upsets the other
                // software sharing it.
                await ConnectToAsync(TrainingRadio, remember: false);
                StatusText = ReconnectPlan.NoAnswer(plan.Port);
                return;
        }
    }

    /// <summary>UI-origin frequency changes: clamp to band, refresh the mode
    /// line, and schedule a throttled rig send so tape drags don't flood the
    /// CI-V bus.</summary>
    partial void OnFrequencyHzChanged(long value)
    {
        // THE DIAL REACHES PAST THE BAND EDGE, on purpose (HM-DEC-055). It used
        // to stop dead at the edge, which is a locked control standing in for an
        // explanation and is what HM-DEC-029 says not to do. The stop is now the
        // end of the picture rather than the end of the band, so somebody who
        // tunes off the top of 20 m sees what is out there and reads why it is
        // not theirs, instead of finding the knob refusing to turn. Nothing here
        // is about privileges: it is about what the map on screen can show.
        //
        // And it binds to the operator's own tuning only. A frequency the radio
        // reported is a measurement, and a measurement that has been clamped to
        // fit a picture is a wrong number about the one thing every other
        // surface trusts (§0.0).
        var clamped = _updatingFromRig ? value : Math.Clamp(value, MapLowHz, MapHighHz);
        if (clamped != value)
        {
            FrequencyHz = clamped;
            return;
        }

        UpdateModeLine();
        UpdateFavoriteState();
        ScheduleModeFollow();

        // The dwell clock restarts wherever the dial lands, from any source
        // (HM-DEC-072). A callsign the operator arrived on stops applying the
        // moment he is somewhere else, which is what keeps the recent list from
        // naming a station on a frequency nobody heard one on.
        _dwell.Moved(clamped, DateTime.UtcNow);

        if (_arrivedOnHz != clamped)
        {
            _arrivedOnStation = "";
            _arrivedOnHz = -1;
        }

        if (_updatingFromRig || _rig is null || !IsConnected)
        {
            return;
        }

        _rigSendPending = true;
        if (!_rigSendTimer.IsEnabled)
        {
            _rigSendTimer.Start();
        }
    }

    private async void OnRigSendTick(object? sender, EventArgs e)
    {
        if (!_rigSendPending || _rig is null || !IsConnected)
        {
            _rigSendTimer.Stop();
            return;
        }

        _rigSendPending = false;
        try
        {
            await _rig.SetFrequencyHzAsync(FrequencyHz);
        }
        catch (Exception ex)
        {
            StatusText = $"Set frequency failed: {ex.Message}";
        }

        if (!_rigSendPending)
        {
            _rigSendTimer.Stop();
        }
    }

    /// <summary>
    /// Let the dial settle, then think about the mode.
    /// </summary>
    /// <remarks>
    /// Restarted on every move, so a drag across three neighborhoods produces
    /// one change and not three (HM-DEC-056).
    /// </remarks>
    private void ScheduleModeFollow()
    {
        if (!_modeFollow.Enabled || _modeFollow.Suspended || _rig is null || !IsConnected)
        {
            return;
        }

        _modeSettleTimer.Stop();
        _modeSettleTimer.Start();
    }

    private async void OnModeSettleTick(object? sender, EventArgs e)
    {
        _modeSettleTimer.Stop();
        await FollowTheMapAsync();
    }

    /// <summary>
    /// Set the radio to the mode this stretch of band is worked in.
    /// </summary>
    /// <remarks>
    /// <para>NARRATED, ALWAYS (HM-DEC-056). A radio that changes itself silently
    /// is the "is it broken" confusion relocated rather than removed, and this
    /// operator has had enough of machines doing things without saying so.</para>
    /// <para>A write that is not confirmed leaves the mode unknown rather than
    /// assumed. The rig reports that itself, so the badge empties and the screen
    /// stops claiming to know something it does not (§0.0).</para>
    /// </remarks>
    private async Task FollowTheMapAsync()
    {
        var rig = _rig;
        if (rig is null || !IsConnected)
        {
            return;
        }

        var here = Neighborhoods.FirstOrDefault(n => n.Contains(FrequencyHz));
        var decision = ModeFollowPlan.Decide(
            _modeFollow, RigState.Mode, RigState.IsDataMode,
            ModeFollowPlan.TargetFor(here));

        if (!decision.Write)
        {
            return;
        }

        _settingModeOurselves = true;
        try
        {
            var result = await rig.SetModeAsync(decision.Mode, decision.DataMode);

            _lastKnownMode = result.Worked ? decision.Mode : null;

            // A radio that has no such mode says so by having nothing to say,
            // and blanking the status line over it would wipe whatever the
            // operator was reading.
            var say = result.Worked ? decision.Narration : result.Detail;
            if (say.Length > 0)
            {
                StatusText = say;
            }

            AppEvents.ModeFollowed(
                _telemetry, decision.Mode.ToString(), decision.DataMode,
                result.Outcome.ToString());
        }
        catch (Exception ex)
        {
            // Never-throw discipline (§8). A mode change that failed is a
            // sentence, not a crash.
            StatusText = $"Hamlet could not set the mode: {ex.Message}";
        }
        finally
        {
            _settingModeOurselves = false;
        }
    }

    private async void OnSpotRefreshTick(object? sender, EventArgs e)
        => await ReloadSpotsAsync("timer");

    private void OnAgeTick(object? sender, EventArgs e)
    {
        UpdateSpotFreshness();

        var now = DateTime.UtcNow;

        NoteDwell(now);
        RefreshHeard(now);
        Heartbeat(now);

        // A SECOND WAY FOR THE TRANSMISSION TO END (HM-DEC-085). The latch is
        // normally released by the rig poll, which runs four times a second while
        // the window is up. If that stalls, on a disconnect or a window that goes
        // away mid-send, nothing else would ever look at the clock and the send
        // controls would stay unavailable until something happened to refresh
        // them. This ticks once a second regardless, so the arithmetic still runs
        // out on its own.
        if (Transmit.IsSending)
        {
            Transmit.Refresh();
        }
        OnPropertyChanged(nameof(ReceiveHeadline));
        OnPropertyChanged(nameof(ReceiveOffer));
        OnPropertyChanged(nameof(HasReceiveOffer));

        TellTheCanvasWhatItIsMissing();
    }

    /// <summary>
    /// What is happening on widgets that are not out (HM-DEC-086).
    /// </summary>
    /// <remarks>
    /// <para>**NOTHING IS SWALLOWED AND NOTHING IS FLUNG ONTO THE CANVAS.** Morse
    /// arriving while the terminal is in the tray is a thing the operator would
    /// want to know, and it is not a reason for Hamlet to rearrange their
    /// screen.</para>
    /// <para>So it says so, once, in a line with the widget's name on a button
    /// beside it. It is §0.5 one level up: a collapsed panel still carries its
    /// summary, and a widget that is not out still carries its news. **The work
    /// never stopped** — the decoder ran, the spots came in, the reports were
    /// counted, all of it into the same view model. Bringing the widget out shows
    /// the history rather than starting from the moment it appeared.</para>
    /// </remarks>
    private void TellTheCanvasWhatItIsMissing()
    {
        // LIVE: it is going on right now, and while the terminal is away the
        // operator is missing a conversation as it happens (HM-DEC-087).
        Canvas.News(
            Layout.Widgets.Terminal,
            IsDecoding && !Transcript.IsEmpty
                ? "Morse is arriving right now and the terminal is not out. Nothing "
                  + "is being lost, so bringing it back shows all of it."
                : "",
            AbsentUrgency.Live);

        // QUIET: the reports are counted and kept, and they will read the same
        // whenever the panel comes back, so there is nothing to hurry for.
        Canvas.News(
            Layout.Widgets.Heard,
            HasHeardReports
                ? "Somebody heard your call, and the panel that says who is away."
                : "",
            AbsentUrgency.Quiet);
    }

    /// <summary>
    /// Remember where the operator has been, once he has actually stopped there
    /// (HM-DEC-072).
    /// </summary>
    /// <param name="nowUtc">The moment, passed in so the rule is testable.</param>
    /// <remarks>
    /// <para>DWELL RATHER THAN LANDING. The decision lives in
    /// <see cref="DwellTracker"/> and this only supplies the clock and the
    /// context, so the rule can be proved to the second without waiting twenty
    /// of them.</para>
    /// <para>The callsign goes on only where something identified one. Arriving
    /// by clicking a spot card counts, because the operator acted on a report of
    /// that station. Scroll-wheeling onto a frequency a spot happens to sit near
    /// does not, because nothing was checked and an entry that named a station
    /// then would be asserting a presence out of proximity (§0.0).</para>
    /// </remarks>
    internal void NoteDwell(DateTime nowUtc)
    {
        // SEEDED HERE RATHER THAN ONLY WHERE THE DIAL MOVES. The frequency the
        // app opens on was never announced by a change, so nothing started its
        // clock and the place somebody was already sitting on could never be
        // remembered. A move to where the tracker already is costs nothing, so
        // asking every tick is both cheap and the only version that cannot be
        // defeated by a path that sets the frequency quietly.
        _dwell.Moved(FrequencyHz, nowUtc);

        if (!_dwell.Settled(nowUtc))
        {
            return;
        }

        var here = Neighborhoods.FirstOrDefault(n => n.Contains(FrequencyHz));

        // HAMLET'S OWN EARS FIRST (HM-DEC-073). A callsign the decoder read off
        // the air, here, with every character solid, is a stronger fact than a
        // report from somebody else's receiver minutes ago about a frequency
        // that may since have changed hands. Where both exist the decoder wins,
        // and either way the surface says which one it was.
        var station = CallsignResolver.StationHeard(Transcript.Recent());
        var source = StationSource.Decoder;

        if (station is null)
        {
            station = _arrivedOnHz == FrequencyHz ? _arrivedOnStation : "";
            source = station.Length > 0 ? StationSource.SpotFeed : StationSource.None;
        }

        var visit = RecentStations.From(
            FrequencyHz, station, RigModeText, here, nowUtc, source);

        var kept = RecentStations.Remember(Recent, visit);

        Recent.Clear();
        foreach (var entry in kept)
        {
            Recent.Add(entry);
        }

        PersistRecent();
    }

    /// <summary>Write the recent list back to settings.json.</summary>
    private void PersistRecent()
    {
        _settings.Recent = Recent
            .Select(r => new SavedRecentStation
            {
                FrequencyHz = r.FrequencyHz,
                Station = r.Station,
                Mode = r.Mode,
                BandName = r.BandName,
                Neighborhood = r.Neighborhood,
                VisitedUtc = r.VisitedUtc,
                StationSource = r.Source.ToString(),
            })
            .ToList();

        SettingsStore.Save(_settings);
        RebuildMenus();
    }

    /// <summary>Tune back to somewhere the operator has been.</summary>
    [RelayCommand]
    private void TuneToRecent(RecentStation? entry)
    {
        if (entry is null)
        {
            return;
        }

        AppEvents.TuneRequested(_telemetry, entry.FrequencyHz, "recent");
        TuneTo(entry.FrequencyHz);
    }

    /// <summary>
    /// Star a place he has been into a favorite (HM-DEC-072).
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <remarks>
    /// HOW MOST FAVORITES WILL ACTUALLY BE BORN. Somebody was somewhere good,
    /// did not think to save it, and wants it the following evening. What it
    /// captures is what a direct save captures, from the same function, so a
    /// favorite made this way is indistinguishable from one made at the star.
    /// </remarks>
    [RelayCommand]
    private void StarRecent(RecentStation? entry)
    {
        if (entry is null)
        {
            return;
        }

        if (RadioEngine.Explore.Favorites.At(Favorites, entry.FrequencyHz) is not null)
        {
            StatusText = $"{entry.Label} is already saved.";
            return;
        }

        if (Favorites.Count >= RadioEngine.Explore.Favorites.Maximum)
        {
            StatusText = "That is as many favorites as Hamlet keeps. Remove one "
                       + "from Radio, Manage favorites, and this will save.";
            return;
        }

        var here = Neighborhoods.FirstOrDefault(n => n.Contains(entry.FrequencyHz));
        var favorite = RecentStations.ToFavorite(entry, here, DateTime.UtcNow);

        Favorites.Add(favorite);
        AppEvents.FavoriteSaved(_telemetry, favorite.BandName);
        StatusText = $"Saved as \"{favorite.Name}\".";

        PersistFavorites();
    }

    partial void OnSelectedRecentChanged(RecentStation? value)
    {
        if (value is null)
        {
            return;
        }

        var picked = value;
        SelectedRecent = null;
        TuneToRecent(picked);
    }

    /// <summary>
    /// Reload the feed, preserving the reading position: spots that are still
    /// there keep their place in the list, departures drop out, and arrivals
    /// append with a "new" tag. Sorting the list afresh on every tick would
    /// move a card out from under the operator's cursor mid-read
    /// (HM-DEC-020).
    /// </summary>
    /// <summary>The lifetimes the operator has configured.</summary>
    private SpotLifetimeSettings Lifetimes => _settings.Lifetimes;

    /// <summary>
    /// Rank the bands from what Hamlet has actually heard (HM-DEC-046).
    /// </summary>
    /// <param name="nowUtc">The moment to judge against.</param>
    /// <returns>The ranking, best first.</returns>
    /// <remarks>
    /// The local hour is passed in rather than read inside, so the ranking
    /// stays a pure function and the tiebreaker is testable without waiting
    /// for a particular time of day (§5).
    /// </remarks>
    private BandRanking RankBands(DateTime nowUtc)
        => BandOpportunities.Rank(
            Bands.Select(b => b.Band).ToList(),
            _allBandSpots,
            nowUtc,
            DateTime.Now.Hour,
            Lifetimes);

    /// <summary>
    /// Move the best-bet badge to whichever band the ranking puts first.
    /// </summary>
    /// <param name="ranking">The shared ranking.</param>
    /// <remarks>
    /// The label travels with it, so a clock guess never wears the same words
    /// as an observation (§0.0). Nothing is marked at all when no band has a
    /// name in the ranking, which cannot happen today but would otherwise be
    /// a silent badge on the first band in the list.
    /// </remarks>
    private void ApplyBestBet(BandRanking ranking)
    {
        foreach (var button in Bands)
        {
            // The decision lives in the ranking, not here. This loop copies an
            // answer and is not allowed to form one of its own.
            var isBest = ranking.BadgeGoesOn(button.Band.Name);

            button.IsBestBet = isBest;

            if (isBest)
            {
                button.BestBetLabel = ranking.BadgeLabel;
                button.BestBetTooltip = ranking.BadgeTooltip;
            }
        }
    }

    /// <summary>
    /// Write what a refresh returned to history, and keep the file bounded.
    /// </summary>
    /// <remarks>
    /// Runs off the UI thread. The store never throws for storage reasons, so
    /// this needs no guard of its own; the worst case is that history stops
    /// growing and the app carries on with what it has (§8).
    /// </remarks>
    private void RecordAndPrune(IReadOnlyList<ActivitySpot> spots, DateTime now)
    {
        _spotStore.Record(spots, now);

        // Pruning on a schedule rather than every refresh, because deleting
        // nothing a hundred times an hour is just disk noise.
        if (now - _lastPruneUtc < PruneInterval)
        {
            return;
        }

        _lastPruneUtc = now;
        var gone = _spotStore.Prune(now - HistoryRetention);

        if (gone > 0)
        {
            AppEvents.SpotHistoryPruned(_telemetry, gone, _spotStore.Count());
        }
    }

    /// <summary>
    /// Everything still worth showing: what the feed just returned, plus
    /// anything in history still inside its own source's lifetime.
    /// </summary>
    /// <remarks>
    /// <para>THE FIX FOR THE ACTUAL COMPLAINT (HM-DEC-045). The feed only
    /// returns what its sources hold right now, and RBN in particular holds
    /// nothing at all on a fresh start. History fills that in.</para>
    /// <para>Live spots win over stored copies of themselves, because the live
    /// one may carry a newer report count or a better story. Identity is the
    /// same rule the store and the aggregate use.</para>
    /// </remarks>
    private IReadOnlyList<StoredSpot> LiveFromHistory(
        IReadOnlyList<ActivitySpot> live, DateTime now)
    {
        var merged = new List<StoredSpot>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // The store first, because its rows carry the two facts the lenses need
        // and a live spot cannot: when Hamlet first saw this, and whether the
        // operator has already been to it (HM-DEC-057).
        foreach (var stored in _spotStore.Since(now - Lifetimes.Longest))
        {
            if (!SpotLifetime.IsLive(stored.Spot, now, Lifetimes))
            {
                continue;
            }

            if (seen.Add(SpotIdentity.KeyFor(stored.Spot)))
            {
                merged.Add(stored);
            }
        }

        // Anything the feed just returned that the store did not give back. It
        // was recorded a moment ago, so this is a store that could not be
        // written rather than a spot that is new, and the list still shows it
        // rather than losing it to a disk problem (§8).
        foreach (var spot in live)
        {
            if (SpotLifetime.IsLive(spot, now, Lifetimes)
                && seen.Add(SpotIdentity.KeyFor(spot)))
            {
                merged.Add(new StoredSpot(spot, now, now));
            }
        }

        merged.Sort((a, b) => b.Spot.HeardAtUtc.CompareTo(a.Spot.HeardAtUtc));
        return merged;
    }

    private async Task ReloadSpotsAsync(string trigger)
    {
        _activitySource.SetContext(BuildContext());

        IReadOnlyList<ActivitySpot> spots;
        try
        {
            spots = await _activitySource.GetSpotsAsync();
        }
        catch (Exception)
        {
            // An unreachable feed leaves the last set on screen, aging
            // visibly. Silence beats an invented refresh.
            return;
        }

        var now = DateTime.UtcNow;

        // Everything seen goes to history before anything is drawn, off the UI
        // thread, so a slow disk cannot stutter the window (HM-DEC-045).
        await Task.Run(() => RecordAndPrune(spots, now));

        // The display is a VIEW OVER HISTORY rather than a buffer that
        // forgets. What the feed just returned is merged with everything still
        // inside its source's lifetime, so a park activator spotted twenty
        // minutes ago is still an invitation instead of being discarded.
        var history = LiveFromHistory(spots, now);

        _allBandSpots.Clear();
        _allBandSpots.AddRange(history.Select(h => h.Spot));

        // The list shows the band on screen; the conditions line keeps the
        // whole spectrum, which is what lets it say "try 40 m" with a count.
        _bandHistory = history
            .Where(h => SelectedBand.Band.LowHz <= h.Spot.FrequencyHz
                        && h.Spot.FrequencyHz <= SelectedBand.Band.HighHz)
            .ToList();

        var onBand = _bandHistory.Select(h => h.Spot).ToList();
        var ranked = ApplyLens(now);
        var newCount = _lastNewSpotCount;

        UpdateBandActivity(now);
        ActivityDots = BuildDots(ranked, now);

        // ONE RANKING, READ BY BOTH (HM-DEC-046). The badge and the lead card
        // used to answer "which band is best" separately, and the badge
        // answered it from a clock table, so they could and did contradict
        // each other on the same screen.
        var ranking = RankBands(now);
        ApplyBestBet(ranking);

        Lead = LeadCard.Choose(
            ranked,
            SelectedBand.Band.Name,
            AnySourceAnswering(),
            ranking,
            _settings.Lifetimes.Longest);

        Conditions = BandConditions.Describe(
            SelectedBand.Band.Name, onBand, _allBandSpots, _activitySource.Statuses, now);

        _lastSpotLoadUtc = now;
        _spotsEverLoaded = true;
        UpdateSourceSummary();
        UpdateSpotFreshness();

        AppEvents.SpotsRefreshed(_telemetry, trigger, Spots.Count, newCount);
        AppEvents.LeadCardBuilt(
            _telemetry, Lead.HasSuggestion,
            ranked.Count > 0 && Lead.HasSuggestion ? ranked[0].Score : 0);
    }

    /// <summary>
    /// Put the current lens over the band's history and redraw the list.
    /// </summary>
    /// <param name="now">Reference time.</param>
    /// <returns>The ranked spots the lens shows.</returns>
    /// <remarks>
    /// <para>NOTHING IS DELETED HERE OR ANYWHERE BELOW IT (HM-DEC-057). This
    /// filters the history the store handed back and the store keeps every row
    /// either way, so switching lenses changes what is on screen and changes
    /// nothing about what Hamlet holds.</para>
    /// <para>Inference chooses the opening lens once, and only while the
    /// operator has not chosen one themselves. After that it is theirs.</para>
    /// </remarks>
    private IReadOnlyList<RankedSpot> ApplyLens(DateTime now)
    {
        var attention = new SpotAttention(_settings.SpotsLastLookedUtc, _actedOnSpots);

        if (!_lensChosenByOperator)
        {
            var unseen = _bandHistory.Count(h => SpotLensView.IsUnseen(h, attention));
            SetLens(SpotLensView.OpeningLens(attention.LastLookedUtc, now, unseen));
        }

        var lensed = SpotLensView.Apply(_lens, _bandHistory, attention, now, Lifetimes);

        // The chips count everything the lens has and draw only the families
        // that are on, which composes with the lens rather than fighting it
        // (HM-DEC-061).
        RebuildFamilyChips(lensed.Select(l => l.Spot));

        var shown = FamilyFilter.Apply(lensed.Select(l => l.Spot), _families).ToHashSet();
        var kept = lensed.Where(l => shown.Contains(l.Spot)).ToList();

        var prominence = kept.ToDictionary(
            l => SpotViewModel.KeyFor(l.Spot), l => l.Prominence, StringComparer.Ordinal);

        // The rank reads the lens's own liveness rather than measuring the
        // clock again, so the fade on a card and its place in the list are two
        // readings of one number (HM-DEC-058).
        var ranked = SpotRanking.Rank(kept, Lifetimes, _settings.CopySpeedWpm);

        _lastNewSpotCount = RebuildSpotList(ranked, now, prominence);
        UpdateSpotFreshness();

        return ranked;
    }

    /// <summary>
    /// Move the lens and tell the two toggles which of them is down.
    /// </summary>
    /// <param name="lens">The lens now in use.</param>
    /// <remarks>
    /// EVERY PATH THAT MOVES THE LENS COMES THROUGH HERE, which it did not at
    /// first: the operator's own click set the field and left the buttons
    /// bound to stale booleans, so the control showed the wrong one down until
    /// something else happened to refresh it. The toggles bind one way on
    /// purpose, since which lens is in use is the ViewModel's answer rather than
    /// the button's.
    /// </remarks>
    private void SetLens(SpotLens lens)
    {
        _lens = lens;
        IsBestChance = lens == SpotLens.BestChance;
        IsWhatsNew = lens == SpotLens.WhatsNew;
        LensQuestion = SpotLensView.Question(lens);
    }

    /// <summary>
    /// Mark every spot at this frequency as one the operator has been to.
    /// </summary>
    /// <param name="hz">Where they tuned.</param>
    /// <returns>
    /// The callsign of a station reported there, or "" when none of the spots
    /// named one (HM-DEC-072).
    /// </returns>
    /// <remarks>
    /// By frequency bucket rather than by card, because the click that tunes
    /// carries a frequency and two skimmers measuring the same carrier rarely
    /// agree to the hertz. It is the same bucket the store identifies a spot by
    /// (<see cref="SpotIdentity.FrequencyBucketHz"/>), so the two cannot drift.
    /// </remarks>
    private string MarkActedOn(long hz)
    {
        var bucket = hz / SpotIdentity.FrequencyBucketHz;
        var now = DateTime.UtcNow;
        var arrivedOn = "";

        foreach (var stored in _bandHistory)
        {
            if (stored.Spot.FrequencyHz / SpotIdentity.FrequencyBucketHz != bucket)
            {
                continue;
            }

            var key = SpotIdentity.KeyFor(stored.Spot);
            if (_actedOnSpots.Add(key))
            {
                _spotStore.MarkActedOn(key, now);
            }

            // WHO HE WENT TO SEE (HM-DEC-072). Only reached from a click on a
            // card, a dot or a story, so this is the operator acting on a report
            // of that station rather than the dial happening to be near one.
            if (arrivedOn.Length == 0 && !string.IsNullOrWhiteSpace(stored.Spot.DxCall))
            {
                arrivedOn = stored.Spot.DxCall.Trim();
            }
        }

        return arrivedOn;
    }

    /// <summary>
    /// Rebuild the card list in ranked order, reusing the cards already on
    /// screen so a surviving spot keeps its identity.
    /// </summary>
    /// <param name="ranked">Ranked spots, best first.</param>
    /// <param name="now">Reference time.</param>
    /// <param name="prominence">How strongly to draw each card, by key.</param>
    /// <returns>How many spots were not in the previous set.</returns>
    /// <remarks>
    /// HM-DEC-020 said the list is not re-sorted on every tick, because moving
    /// a card out from under a reading operator's cursor costs more than a
    /// perfect order. That still holds and is why the one-second age tick only
    /// re-ages text. Ranking reorders on a data refresh only — a deliberate,
    /// five-minutes-apart event where the content genuinely changed
    /// (HM-DEC-025 amends HM-DEC-020 to exactly this extent).
    /// </remarks>
    private int RebuildSpotList(
        IReadOnlyList<RankedSpot> ranked,
        DateTime now,
        IReadOnlyDictionary<string, double> prominence)
    {
        var existing = new Dictionary<string, SpotViewModel>(StringComparer.Ordinal);
        foreach (var vm in Spots)
        {
            existing[vm.Key] = vm;
        }

        var rebuilt = new List<SpotViewModel>(ranked.Count);
        var newCount = 0;

        foreach (var entry in ranked)
        {
            var key = SpotViewModel.KeyFor(entry.Spot);

            var distance = DescribeDistance(entry.Spot);

            // How strongly to draw it: the fade that lets the eye find what is
            // current without reading a timestamp (HM-DEC-057). A card the lens
            // did not measure is drawn plainly rather than dimmed on a guess.
            var drawAt = prominence.TryGetValue(key, out var p) ? p : 1.0;

            if (existing.TryGetValue(key, out var vm))
            {
                vm.Update(entry.Spot, now, entry.Reason, distance, Lifetimes, drawAt);
                rebuilt.Add(vm);
                continue;
            }

            // Nothing is "new" on the first load — everything would be.
            rebuilt.Add(new SpotViewModel(
                entry.Spot, now, isNew: _spotsEverLoaded, entry.Reason, distance,
                Lifetimes, drawAt));

            if (_spotsEverLoaded)
            {
                newCount++;
            }
        }

        if (!SameOrder(Spots, rebuilt))
        {
            Spots.Clear();
            foreach (var vm in rebuilt)
            {
                Spots.Add(vm);
            }
        }

        return newCount;
    }

    private static bool SameOrder(
        IReadOnlyList<SpotViewModel> current, IReadOnlyList<SpotViewModel> rebuilt)
    {
        if (current.Count != rebuilt.Count)
        {
            return false;
        }

        for (var i = 0; i < current.Count; i++)
        {
            if (!ReferenceEquals(current[i], rebuilt[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Turn ranked spots into map dots, with prominence following the rank so
    /// the map and the list agree about what matters (HM-DEC-023).
    /// </summary>
    private IReadOnlyList<Controls.ActivityDot> BuildDots(
        IReadOnlyList<RankedSpot> ranked, DateTime now)
    {
        if (ranked.Count == 0)
        {
            return Array.Empty<Controls.ActivityDot>();
        }

        var dots = new List<Controls.ActivityDot>(ranked.Count);

        for (var i = 0; i < ranked.Count; i++)
        {
            var entry = ranked[i];

            // Prominence falls away over the first ten places; past that every
            // dot is drawn at the floor rather than vanishing, because it is
            // still a real signal on a real frequency.
            var prominence = Math.Max(0.0, 1.0 - (i / 10.0));

            dots.Add(new Controls.ActivityDot(
                entry.Spot.FrequencyHz,
                entry.Spot.Story,
                entry.Spot.Mode,
                entry.Spot.Source,
                SpotFreshness.Describe(now - entry.Spot.HeardAtUtc),
                entry.Reason,
                prominence)
            {
                Distance = DescribeDistance(entry.Spot),
            });
        }

        return dots;
    }

    /// <summary>
    /// How far away a spot is, or "" when the app cannot justify a figure
    /// (HM-DEC-038).
    /// </summary>
    /// <param name="spot">The spot.</param>
    /// <returns>e.g. "480 miles northeast", or "".</returns>
    private string DescribeDistance(ActivitySpot spot)
        => SpotDistance.Describe(
            _settings.Operator.Position, spot, _settings.DistanceUnits);

    /// <summary>
    /// Recompute every visible distance.
    /// </summary>
    /// <remarks>
    /// Called when the grid arrives, when it changes, and when the units
    /// change — the moment a position becomes known, every card and dot on
    /// screen can say something it could not say a second ago.
    /// </remarks>
    private void UpdateSpotDistances()
    {
        foreach (var spot in Spots)
        {
            spot.Distance = DescribeDistance(spot.Spot);
        }

        // The dots are a record type rebuilt wholesale, so the cheapest
        // correct thing is to rebuild them from the spots that are already on
        // the list rather than hold a second copy of the ranking.
        if (ActivityDots.Count > 0)
        {
            ActivityDots = ActivityDots
                .Select(d => d with
                {
                    Distance = DistanceForFrequency(d.FrequencyHz),
                })
                .ToList();
        }
    }

    /// <summary>
    /// The distance for whatever spot sits on this frequency, or "".
    /// </summary>
    private string DistanceForFrequency(long frequencyHz)
    {
        var match = Spots.FirstOrDefault(s => s.FrequencyHz == frequencyHz);
        return match is null ? "" : DescribeDistance(match.Spot);
    }

    /// <summary>
    /// Refresh every band button's activity indicator (HM-DEC-031).
    /// </summary>
    /// <remarks>
    /// Computed from the whole-spectrum spot set and the source statuses, so
    /// a band button and the conditions line under the map are always
    /// counting the same minutes from the same evidence.
    /// </remarks>
    private void UpdateBandActivity(DateTime now)
    {
        var readings = BandActivity.Summarize(
            Bands.Select(b => b.Band).ToList(),
            _allBandSpots,
            _activitySource.Statuses,
            now);

        foreach (var reading in readings)
        {
            var button = Bands.FirstOrDefault(b => b.Band.Name == reading.BandName);
            if (button is not null)
            {
                button.Activity = reading;
            }
        }

        UpdateBandCharacter(now);
    }

    /// <summary>
    /// Refresh each card's look and its character text for the current hour
    /// (HM-DEC-033).
    /// </summary>
    /// <remarks>
    /// Sunrise and sunset are computed from the operator's grid square. With
    /// no grid there are no coordinates, so nothing is dimmed, the icons stay
    /// neutral and the text says how to fix that — Hamlet never guesses where
    /// somebody is.
    /// </remarks>
    private void UpdateBandCharacter(DateTime nowUtc)
    {
        var here = OperatorLocation.FromGrid(_settings.Operator.GridSquare);

        var sun = here is null
            ? SolarSnapshot.Unknown
            : SolarClock.At(here.Value.Latitude, here.Value.Longitude, nowUtc);

        var names = Bands.Select(b => b.Band.Name).ToList();

        foreach (var button in Bands)
        {
            button.Card = BandCardStyles.For(button.Band.Name, names, sun);
            button.Character = BandCharacter.Describe(
                button.Band.Name, sun, nowUtc.Month, here?.Latitude);
        }
    }

    private bool AnySourceAnswering()
        => _activitySource.Statuses.Any(s => s.State == SourceState.Ok);

    /// <summary>
    /// Name which sources answered, and confess the ones that did not.
    /// </summary>
    private void UpdateSourceSummary()
    {
        var statuses = _activitySource.Statuses;
        var ok = statuses.Where(s => s.State == SourceState.Ok).Select(s => s.Name).ToList();
        var down = statuses.Where(s => s.IsLetDown).ToList();

        foreach (var status in down)
        {
            AppEvents.SourceUnhealthy(_telemetry, status.Name, status.State.ToString());
        }

        SourcesSummary = ok.Count == 0
            ? "no sources answering"
            : string.Join(", ", ok)
              + (down.Count > 0 ? $" · {string.Join(", ", down.Select(d => d.Name))} down" : "");
    }

    /// <summary>Re-age everything on screen: the header line, its color, each
    /// card's own age, and the expiry of the "new" tags.</summary>
    private void UpdateSpotFreshness()
    {
        var now = DateTime.UtcNow;
        var since = now - _lastSpotLoadUtc;
        var interval = _settings.SpotRefreshMinutes;

        // THE LENS IS NAMED FIRST (§0.5, HM-DEC-057). A shut panel that has
        // silently changed which question it is answering is the prime
        // directive broken by omission: the operator reads a count and takes it
        // for a count of everything.
        // A shut panel never hides that it is filtering (§0.5, HM-DEC-061).
        var filtered = FamilyFilter.Summary(_families);

        SpotsSummary = SpotLensView.Summary(_lens, Spots.Count)
            + (filtered.Length > 0 ? $" · {filtered}" : "")
            + " · " + SpotFreshness.Tail(since, interval, _spotsEverLoaded)
            + (SourcesSummary.Length > 0 ? $" · {SourcesSummary}" : "");
        SpotsFreshness = _spotsEverLoaded
            ? SpotFreshness.Evaluate(since, interval)
            : FreshnessLevel.Fresh;

        foreach (var spot in Spots)
        {
            spot.Reage(now);
        }
    }

    private void OnRigFrequencyChanged(object? sender, FrequencyChangedEventArgs e)
        => Dispatcher.UIThread.Post(() => ApplyRigFrequency(e.FrequencyHz));

    /// <summary>Rig-origin frequency (the physical knob): follow it, switch
    /// the selected band if the operator crossed one, and never echo it back
    /// out to the rig.</summary>
    /// <remarks>
    /// THE RADIO IS NEVER ARGUED WITH (§0.0, HM-DEC-055). A 7300 tunes right
    /// across the shortwave broadcast bands and somebody will do it, so when the
    /// reading is off every ham band the nearest one is put on screen and the
    /// display follows the radio out into it. Clamping here would show a
    /// frequency the radio is not on, which is a confident wrong answer about
    /// the one number every other surface trusts.
    /// </remarks>
    private void ApplyRigFrequency(long hz)
    {
        _updatingFromRig = true;
        try
        {
            var band = BandPlan.BandFor(hz) ?? AmateurSpectrum.Nearest(hz);
            if (band is not null && band.Name != SelectedBand.Band.Name)
            {
                SelectedBand = Bands.First(b => b.Band.Name == band.Name);
            }

            FrequencyHz = hz;
        }
        finally
        {
            _updatingFromRig = false;
        }
    }

    private void UpdateModeLine()
    {
        IsInsideCwSegment = SelectedBand.Band.IsInCwSegment(FrequencyHz);

        // THE SAME FACT, FROM THE SAME PLACE (HM-DEC-055). This line used to say
        // "OUTSIDE the CW segment" above the top of 20 m, which is true and
        // wildly understates matters, because up there it is not a ham band at
        // all. The map, the card and this line now all read one derivation, so
        // no two of them can disagree about it.
        var standing = AmateurSpectrum.Describe(FrequencyHz);

        ModeLineText = standing.IsAmateur
            ? $"CW · {SelectedBand.Band.Name} · "
              + (IsInsideCwSegment ? "inside the CW segment" : "OUTSIDE the CW segment")
            : $"CW · past the edge of {SelectedBand.Band.Name} · not a ham band";

        UpdatePrivileges();
    }

    /// <summary>
    /// Recompute the privilege spans and the status line for where the
    /// operator is tuned.
    /// </summary>
    /// <remarks>
    /// One computation feeding both the map's veil and the line beneath it,
    /// so the hatching and the words can never contradict each other
    /// (HM-DEC-029). Phase 1 is a CW app, so the line answers for Morse; when
    /// other modes can be sent, the mode being transmitted becomes the
    /// argument rather than a constant.
    /// </remarks>
    private void UpdatePrivileges()
    {
        var cls = _settings.Operator.LicenseClass;

        PrivilegeSpans = _privileges.SpansFor(SelectedBand.Band, cls);
        // The card answers two questions at once: what the license allows, and
        // what is actually going on where the dial is pointing (HM-DEC-054).
        PrivilegeStatus = PrivilegeStatusLine.Build(
            _privileges, cls, FrequencyHz, TransmitMode.Cw,
            Neighborhoods.FirstOrDefault(n => n.Contains(FrequencyHz)));

        // A pending mismatch is a question about a class the operator has
        // since changed. Answering it by other means makes it moot, and a
        // panel still offering "keep Technician" after they picked General
        // would be asking about a world that no longer exists.
        if (LicenseMismatch is { } pending && pending.Existing != cls)
        {
            LicenseMismatch = null;
        }

        // The ladder was opened from the listen-only line, and the button
        // that opens it disappears once the frequency is theirs. Leaving the
        // panel behind would turn an invitation into permanent chrome — and
        // an upgrade pitch inside a green "yours to use" box reads as a nag
        // (HM-DEC-029).
        if (UpgradeLadderVisible && PrivilegeStatus.Tone != PrivilegeTone.ListenOnly)
        {
            UpgradeLadderVisible = false;
            UpgradeLadder = Array.Empty<string>();
        }
        else if (UpgradeLadderVisible)
        {
            UpgradeLadder = PrivilegeStatusLine.UpgradeLadder(
                _privileges, cls, SelectedBand.Band);
        }

        OnPropertyChanged(nameof(LicenseClass));
        OnPropertyChanged(nameof(LicenseProvenance));
    }

    /// <summary>
    /// Show or hide the upgrade ladder.
    /// </summary>
    /// <remarks>
    /// On click only, never permanent chrome: a restriction the operator
    /// asked about is motivation, and the same words shown unbidden are a nag
    /// (HM-DEC-029).
    /// </remarks>
    [RelayCommand]
    private void ToggleUpgradeLadder()
    {
        UpgradeLadderVisible = !UpgradeLadderVisible;

        UpgradeLadder = UpgradeLadderVisible
            ? PrivilegeStatusLine.UpgradeLadder(
                _privileges, _settings.Operator.LicenseClass, SelectedBand.Band)
            : Array.Empty<string>();

        if (UpgradeLadderVisible)
        {
            AppEvents.UpgradeLadderOpened(
                _telemetry, PrivilegePlan.Describe(_settings.Operator.LicenseClass));
        }
    }

    /// <summary>
    /// Look up the operator's license class when it is missing (HM-DEC-028).
    /// </summary>
    /// <remarks>
    /// <para>Lazy and automatic: attached to the fact rather than to a wizard
    /// screen, because people skip wizards and a callsign can arrive from
    /// Settings or a hand-edited file. Runs on startup and whenever the
    /// profile changes.</para>
    /// <para>Never blocks and never opens a dialog. The status bar narrates
    /// and the operator carries on; if the service is down the class stays
    /// unknown, the map draws no overlay, and Settings still takes a
    /// hand-picked answer.</para>
    /// </remarks>
    public async Task ResolveProfileAsync()
    {
        if (!ProfileResolver.NeedsLookup(_settings.Operator))
        {
            return;
        }

        var callsign = _settings.Operator.Callsign?.Trim() ?? "";
        if (callsign.Length == 0)
        {
            return;
        }

        _licenseLookup?.Cancel();
        _licenseLookup?.Dispose();
        var cts = new CancellationTokenSource();
        _licenseLookup = cts;

        // Narrate only when something is actually missing. A profile that is
        // complete still gets checked for a class disagreement, and announcing
        // that check on every startup would be the app talking about itself.
        var showNarration = LicenseResolver.NeedsResolution(_settings.Operator)
                            || GridResolver.NeedsResolution(_settings.Operator);

        if (showNarration)
        {
            StatusText = ProfileResolver.LookingUpNarration(callsign);
        }

        ProfileResolution resolution;
        try
        {
            using var lookup = new CallookCallsignLookup(
                AboutViewModel.AppVersion, callsign);
            var resolver = new ProfileResolver(lookup);
            resolution = await resolver.ResolveAsync(_settings.Operator, cts.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception)
        {
            // Never fatal: an unresolved profile is a supported state (§8).
            return;
        }

        // A lookup that answered at all wrote its receipt onto the profile
        // (HM-DEC-044), whether or not either fact was adopted. That is a
        // change worth saving: without it the badges would be recomputed from
        // an empty record on the next launch, and the profile would ask again
        // forever.
        var changed = resolution.RecordedALookup;

        switch (resolution.License.Outcome)
        {
            case LicenseResolutionOutcome.Resolved:
                changed = true;
                AppEvents.LicenseClassResolved(
                    _telemetry, PrivilegePlan.Describe(resolution.License.Found),
                    resolution.License.SourceName);
                break;

            case LicenseResolutionOutcome.Mismatch:
                // Shown, never applied. Their license, their call.
                LicenseMismatch = resolution.License;
                AppEvents.LicenseClassMismatch(
                    _telemetry,
                    PrivilegePlan.Describe(resolution.License.Found),
                    PrivilegePlan.Describe(resolution.License.Existing));
                break;

            case LicenseResolutionOutcome.NotFound:
                AppEvents.LicenseClassLookupFailed(
                    _telemetry, resolution.License.Outcome.ToString());
                break;

            default:
                break;
        }

        if (resolution.Unavailable)
        {
            AppEvents.LicenseClassLookupFailed(
                _telemetry, LicenseResolutionOutcome.Unavailable.ToString());
        }

        switch (resolution.Grid.Outcome)
        {
            case GridResolutionOutcome.Resolved:
                changed = true;

                // The grid is what the band cards and every distance rest on,
                // so the moment it arrives the screen has to catch up: nothing
                // dimmed a second ago and the sun is known now (HM-DEC-033).
                OnPropertyChanged(nameof(GridProvenance));
                UpdateBandCharacter(DateTime.UtcNow);
                UpdateSpotDistances();
                break;

            case GridResolutionOutcome.Mismatch:
                GridMismatch = resolution.Grid;
                break;

            default:
                break;
        }

        if (changed)
        {
            SettingsStore.Save(_settings);
        }

        if (showNarration || resolution.License.NeedsOperatorDecision)
        {
            var line = resolution.Narration;
            if (line.Length > 0)
            {
                StatusText = line;
            }
        }

        UpdatePrivileges();
    }

    /// <summary>Provenance for the grid square, shown in Settings.</summary>
    public string GridProvenance => GridResolver.DescribeProvenance(_settings.Operator);

    /// <summary>
    /// A lookup that disagrees with a hand-entered grid, awaiting the
    /// operator's answer (HM-DEC-037). Null when there is nothing to ask.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GridMismatchNarration))]
    private GridResolution? _gridMismatch;

    /// <summary>What the grid disagreement says, or "" (HM-DEC-089).</summary>
    public string GridMismatchNarration => GridMismatch?.Narration ?? "";

    /// <summary>Take the looked-up grid in place of the hand-entered one.</summary>
    [RelayCommand]
    private void AcceptLookedUpGrid()
    {
        if (GridMismatch is not { } mismatch)
        {
            return;
        }

        var point = OperatorLocation.FromGrid(mismatch.Found);
        if (point is not null)
        {
            _settings.Operator.SetPositionFromLookup(
                point.Value, mismatch.SourceName, DateTime.UtcNow);
            SettingsStore.Save(_settings);
        }

        GridMismatch = null;
        OnPropertyChanged(nameof(GridProvenance));
        UpdateBandCharacter(DateTime.UtcNow);
        UpdateSpotDistances();
        StatusText = GridResolver.DescribeProvenance(_settings.Operator);
    }

    /// <summary>Keep the grid the operator typed, and stop asking.</summary>
    /// <remarks>
    /// Re-stamps it as hand-entered today, so the same disagreement does not
    /// reappear on every startup. Declining is an answer, and an app that asked
    /// again tomorrow would not have heard it.
    /// </remarks>
    [RelayCommand]
    private void KeepMyGrid()
    {
        if (GridMismatch is not { } mismatch)
        {
            return;
        }

        _settings.Operator.SetGridByHand(mismatch.Existing, DateTime.UtcNow);
        SettingsStore.Save(_settings);

        GridMismatch = null;
        OnPropertyChanged(nameof(GridProvenance));
        StatusText = GridResolver.DescribeProvenance(_settings.Operator);
    }

    /// <summary>Take the looked-up class in place of the hand-set one.</summary>
    [RelayCommand]
    private void AcceptLookedUpClass()
    {
        if (LicenseMismatch is not { } mismatch)
        {
            return;
        }

        _settings.Operator.SetLicenseClass(
            mismatch.Found, LicenseClassSource.LookedUp, mismatch.SourceName, DateTime.UtcNow);
        SettingsStore.Save(_settings);

        AppEvents.LicenseClassResolved(
            _telemetry, PrivilegePlan.Describe(mismatch.Found), mismatch.SourceName);

        LicenseMismatch = null;
        StatusText = LicenseResolver.DescribeProvenance(_settings.Operator);
        UpdatePrivileges();
    }

    /// <summary>Keep the class the operator set, and stop asking.</summary>
    /// <remarks>
    /// Re-stamps the profile as hand-set today, so the same disagreement does
    /// not reappear on every startup. Declining is an answer, and an app that
    /// asked again tomorrow would not have heard it.
    /// </remarks>
    [RelayCommand]
    private void KeepMyLicenseClass()
    {
        if (LicenseMismatch is not { } mismatch)
        {
            return;
        }

        _settings.Operator.SetLicenseClass(
            mismatch.Existing, LicenseClassSource.EnteredByOperator, "", DateTime.UtcNow);
        SettingsStore.Save(_settings);

        LicenseMismatch = null;
        StatusText = LicenseResolver.DescribeProvenance(_settings.Operator);
        UpdatePrivileges();
    }

    /// <summary>
    /// Put the radio down and give the operator their controls back.
    /// </summary>
    /// <remarks>
    /// THE UI COMES BACK WHATEVER THE RADIO DOES. The rig's own teardown is
    /// bounded and never throws, and this belt goes with that brace: the state
    /// that re-enables Disconnect and the port list is set in a finally, so no
    /// failure anywhere above it can leave somebody stuck with a dead button
    /// and an app that thinks it is still connected (HM-DEC-051).
    /// </remarks>
    private async Task TearDownRigAsync()
    {
        try
        {
            StopRigMonitor();
            StopDecoding();
            StopRigSpectrum();
            Transmit.Attach(null);
            StopTrainingSpectrum();
            _rigSendTimer.Stop();
            _rigSendPending = false;

            // The scanner loses its radio first, so nothing can be mid-tune
            // while the port is closing (§0.2.1).
            Scan.StopNow();
            Scan.Attach(null, null, null, null);

            if (_rig is not null)
            {
                _rig.FrequencyChanged -= OnRigFrequencyChanged;
                await _rig.DisconnectAsync();
                (_rig as IDisposable)?.Dispose();
            }
        }
        catch (Exception)
        {
            // Nothing here is worth trapping the operator for (§8).
        }
        finally
        {
            _rig = null;
            IsConnected = false;
            ConnectButtonText = "Connect";
        }
    }

    private static IRig CreateRig(string selection)
        => selection == TrainingRadio
            ? new TrainingRig()
            : new Ic7300Rig(new SystemSerialPort(selection));

    private static IReadOnlyList<string> SafePortNames()
    {
        try
        {
            return SystemSerialPort.GetPortNames();
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }
}

/// <summary>The persisted panel ids (HM-DEC-021). Strings typed once here,
/// never at a call site, so a typo cannot silently lose a saved state.</summary>
public static class PanelKeys
{
    /// <summary>The neighborhood map.</summary>
    public const string Map = "map";

    /// <summary>The dial tape.</summary>
    public const string Tape = "tape";

    /// <summary>The waterfall.</summary>
    public const string Waterfall = "waterfall";

    /// <summary>The band scanner (HM-DEC-107).</summary>
    public const string Scan = "scan";

    /// <summary>The CW terminal.</summary>
    public const string Terminal = "terminal";

    /// <summary>The Explorer's story card.</summary>
    public const string Story = "story";

    /// <summary>The mode field guide.</summary>
    public const string Guide = "guide";

    /// <summary>The happening-now feed.</summary>
    public const string Spots = "spots";

    /// <summary>The lead card and the band-conditions line (HM-DEC-025).</summary>
    public const string Lead = "lead";

    /// <summary>The worked contact, both sides (HM-DEC-043).</summary>
    public const string Contact = "contact";

    /// <summary>Sending Morse (HM-DEC-059).</summary>
    public const string Transmit = "transmit";

    /// <summary>Did anybody hear me (HM-DEC-075).</summary>
    public const string Heard = "heard";

    /// <summary>I can hear it and Hamlet can't (HM-DEC-084).</summary>
    public const string ReceiveHelp = "receiveHelp";

    /// <summary>The phrasebook (HM-DEC-059).</summary>
    public const string Phrasebook = "phrasebook";
}

/// <summary>One band button: the band plus its best-bet ranking for the hour
/// the app started. FG-001 replaces the ranking with live spot data.</summary>
public partial class BandButtonViewModel : ObservableObject
{
    /// <summary>
    /// What is currently known about this band's activity (HM-DEC-031).
    /// </summary>
    /// <remarks>
    /// Starts as "no data" rather than as an empty band, so a button says
    /// nothing until there is something to say. Refreshed on every spot
    /// reload.
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActivityPips))]
    [NotifyPropertyChangedFor(nameof(ActivityUnknown))]
    [NotifyPropertyChangedFor(nameof(ActivityTooltip))]
    private BandActivityReading _activity;

    /// <summary>How this card looks with the sun where it is (HM-DEC-033).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CardWidth))]
    [NotifyPropertyChangedFor(nameof(CardIcon))]
    [NotifyPropertyChangedFor(nameof(CardIconTint))]
    [NotifyPropertyChangedFor(nameof(CardBar))]
    [NotifyPropertyChangedFor(nameof(CardOpacity))]
    private BandCardStyle _card;

    /// <summary>
    /// What the sun and the season are doing to this band, in plain words.
    /// </summary>
    /// <remarks>
    /// Editorial text from the engine. It says what the sun is doing and what
    /// the band tends to do; it never says the band is open, which is
    /// propagation and not something Hamlet can see (FG-007, HM-DEC-033).
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActivityTooltip))]
    private string _character = "";

    /// <summary>Creates the button model.</summary>
    /// <param name="band">The band this button selects.</param>
    public BandButtonViewModel(CwBand band)
    {
        Band = band;
        _activity = new BandActivityReading(
            band.Name, BandActivityState.NoData, 0, 0, 0,
            "no data.", "Hamlet has not asked the spot sources yet.",
            ConditionsConfidence.Blind);

        _card = BandCardStyles.For(
            band.Name, new[] { band.Name }, SolarSnapshot.Unknown);
    }

    /// <summary>The band this button selects.</summary>
    public CwBand Band { get; }

    /// <summary>
    /// True on the band the shared ranking puts first (HM-DEC-046).
    /// </summary>
    /// <remarks>
    /// Observable and recomputed on every refresh. It used to be fixed at
    /// construction from a clock lookup table, which is how the badge came to
    /// sit on a band with no pips while the lead card was pointing somewhere
    /// else on the same screen.
    /// </remarks>
    [ObservableProperty]
    private bool _isBestBet;

    /// <summary>
    /// What the badge says: an observation, or an admitted guess.
    /// </summary>
    [ObservableProperty]
    private string _bestBetLabel = "best bet now";

    /// <summary>The evidence behind the badge, on hover.</summary>
    [ObservableProperty]
    private string _bestBetTooltip = "";

    /// <summary>Filled pips on the indicator.</summary>
    public int ActivityPips => Activity.Pips;

    /// <summary>True when the indicator should draw as unknown.</summary>
    public bool ActivityUnknown => Activity.IsUnknown;

    /// <summary>
    /// The hover text: what the sun is doing, what the season tends to do, and
    /// what was actually heard.
    /// </summary>
    /// <remarks>
    /// The character comes first because it is the part nobody ever told this
    /// operator, and the evidence sentence closes it because that is the part
    /// Hamlet can actually vouch for (HM-DEC-031, HM-DEC-033).
    /// </remarks>
    public string ActivityTooltip
        => Character.Length == 0
            ? Activity.Tooltip
            : Character + TooltipParagraphBreak + Activity.Tooltip;

    /// <summary>Blank line between the character passage and the evidence.</summary>
    private const string TooltipParagraphBreak = "\n\n";

    /// <summary>Card width, following wavelength.</summary>
    public double CardWidth => Card.Width;

    /// <summary>Sun, moon, both, or neutral.</summary>
    public Controls.DayNightIcon CardIcon => Card.Icon;

    /// <summary>The icon's color.</summary>
    public Avalonia.Media.IBrush CardIconTint => Card.IconTint;

    /// <summary>The colored bar under the label.</summary>
    public Avalonia.Media.IBrush CardBar => Card.BarBrush;

    /// <summary>Dimmed when the band is out of its element.</summary>
    public double CardOpacity => Card.Opacity;

    /// <summary>Pips in a full indicator, for the control to size itself.</summary>
    public static int ActivityPipCount => BandActivity.MaxPips;
}

/// <summary>One happening-now card: the plain-language invitation plus the
/// honesty fields — source and age — the prime directive requires.</summary>
public partial class SpotViewModel : ObservableObject
{
    [ObservableProperty]
    private string _provenance = "";

    [ObservableProperty]
    private bool _isNew;

    /// <summary>
    /// Why the ranking put this card where it is (HM-DEC-025) — shown on the
    /// card's face, never in a tooltip. A card ranked highly without a stated
    /// reason is a guess presented as a decode.
    /// </summary>
    [ObservableProperty]
    private string _reason = "";

    /// <summary>
    /// How far away and roughly which way, e.g. "480 miles northeast", or ""
    /// (HM-DEC-038).
    /// </summary>
    /// <remarks>
    /// Blank whenever the grid is unknown or the source did not say where the
    /// station is — an RBN spot never carries one, because what RBN states is
    /// where a receiver is, not where the transmitter is.
    /// </remarks>
    [ObservableProperty]
    private string _distance = "";

    /// <summary>
    /// The exact age, for anybody who wants the number (HM-DEC-045).
    /// </summary>
    /// <remarks>
    /// The card speaks in words because nobody says "17 min ago" out loud, and
    /// this is the trade that makes that safe: the figure is one hover away
    /// rather than gone.
    /// </remarks>
    [ObservableProperty]
    private string _ageTooltip = "";

    /// <summary>
    /// The mode family this spot belongs to (§0.6, HM-DEC-032).
    /// </summary>
    /// <remarks>
    /// READ OFF THE DATA, LIKE EVERY OTHER SURFACE. The card was the last thing
    /// in the app still speaking in one color while the map, the field guide and
    /// the waterfall all used the mode language. The family comes from the
    /// guide's own table rather than from a list this control carries, and there
    /// is no color literal anywhere near the card.
    /// </remarks>
    [ObservableProperty]
    private ModeFamily _family = ModeFamily.Open;

    /// <summary>
    /// How strongly to draw this card, 1 down to a floor (HM-DEC-057).
    /// </summary>
    /// <remarks>
    /// AGE FADES THE DISPLAY across each source's ruled lifetime, so the eye
    /// finds what is current without anybody reading a timestamp. Never zero: a
    /// card faded to nothing is a card removed, and removing one is what this
    /// whole design exists not to do.
    /// </remarks>
    [ObservableProperty]
    private double _prominence = 1.0;

    private ActivitySpot _spot;
    private DateTime _newUntilUtc;
    private SpotLifetimeSettings _lifetimes = SpotLifetimeSettings.Defaults;

    /// <summary>Wraps an engine spot for display.</summary>
    /// <param name="spot">The engine's spot.</param>
    /// <param name="nowUtc">The reference time for the age line.</param>
    /// <param name="isNew">True when this spot was not in the previous set.</param>
    /// <param name="reason">The ranking's stated reason for this card.</param>
    /// <param name="distance">How far away, or "" when it cannot be said.</param>
    /// <param name="lifetimes">The configured source lifetimes, which decide
    /// how this card talks about its own age (HM-DEC-045).</param>
    /// <param name="prominence">How strongly to draw it (HM-DEC-057).</param>
    public SpotViewModel(
        ActivitySpot spot, DateTime nowUtc, bool isNew,
        string reason = "", string distance = "",
        SpotLifetimeSettings? lifetimes = null, double prominence = 1.0)
    {
        _spot = spot;
        _distance = distance;
        _prominence = prominence;
        _family = ModeGuide.FamilyFor(spot.Mode);
        _lifetimes = lifetimes ?? SpotLifetimeSettings.Defaults;
        Key = KeyFor(spot);
        Story = spot.Story;
        FrequencyHz = spot.FrequencyHz;
        TuneLabel = "Tune " + (spot.FrequencyHz / 1_000_000.0)
            .ToString("0.000", CultureInfo.InvariantCulture);
        _isNew = isNew;
        _reason = reason;
        _newUntilUtc = nowUtc + MainWindowViewModel.NewSpotTagLifetime;
        Reage(nowUtc);
    }

    /// <summary>The spot behind this card.</summary>
    public ActivitySpot Spot => _spot;

    /// <summary>Identity across refreshes: what was said, and where.</summary>
    /// <param name="spot">The spot to key.</param>
    /// <returns>A stable key.</returns>
    public static string KeyFor(ActivitySpot spot)
        => spot.FrequencyHz.ToString(CultureInfo.InvariantCulture) + "|" + spot.Story;

    /// <summary>Identity across refreshes.</summary>
    public string Key { get; }

    /// <summary>The invitation.</summary>
    public string Story { get; }

    /// <summary>Where the Tune button goes.</summary>
    public long FrequencyHz { get; }

    /// <summary>Button text, e.g. "Tune 7.032".</summary>
    public string TuneLabel { get; }

    /// <summary>Take the refreshed spot; a surviving card stops being new.</summary>
    /// <param name="spot">The same spot, freshly reported.</param>
    /// <param name="nowUtc">Reference time.</param>
    /// <param name="reason">The ranking's reason, recomputed with the spot.</param>
    /// <param name="distance">How far away, recomputed with the spot.</param>
    /// <param name="lifetimes">The configured source lifetimes.</param>
    /// <param name="prominence">How strongly to draw it (HM-DEC-057).</param>
    public void Update(
        ActivitySpot spot, DateTime nowUtc, string reason = "", string distance = "",
        SpotLifetimeSettings? lifetimes = null, double prominence = 1.0)
    {
        _spot = spot;
        _lifetimes = lifetimes ?? _lifetimes;
        Prominence = prominence;
        Family = ModeGuide.FamilyFor(spot.Mode);
        IsNew = false;
        if (reason.Length > 0)
        {
            Reason = reason;
        }

        Distance = distance;
        Reage(nowUtc);
    }

    /// <summary>Recompute the age line, and expire the "new" tag once it has
    /// had its thirty seconds.</summary>
    /// <param name="nowUtc">Reference time.</param>
    /// <remarks>
    /// OPPORTUNITY FRESHNESS, NOT FEED FRESHNESS (HM-DEC-045). This line says
    /// how long since the spot happened and whether that person is likely
    /// still there. How long since Hamlet last talked to the network is a
    /// different fact and belongs in the panel header. A feed that reloaded
    /// four seconds ago can be full of hour-old spots, and the wording must
    /// not let those two be confused.
    /// </remarks>
    public void Reage(DateTime nowUtc)
    {
        var elapsed = nowUtc - _spot.HeardAtUtc;

        var provenance = $"{_spot.Mode} · {_spot.Source} · "
            + SpotLifetime.DescribeOpportunity(_spot, elapsed, _lifetimes)
            + (Distance.Length > 0 ? $" · {Distance}" : "");

        // COMPOSED TOGETHER, SO THE CARD CANNOT SAY A THING TWICE (HM-DEC-068).
        // Both lines ask the same function whether that person is probably still
        // there, and neither of them is wrong to. Read one after the other they
        // said it twice, which reads as two pieces of evidence when it is one.
        var lines = CardText.Compose(Reason, provenance);

        Provenance = lines[1];

        // The exact figure stays available for anybody who wants it, which is
        // the trade that lets the card speak in words (HM-DEC-045).
        AgeTooltip = $"Reported {SpotFreshness.Describe(elapsed)} by {_spot.Source}.";

        if (IsNew && nowUtc >= _newUntilUtc)
        {
            IsNew = false;
        }
    }
}

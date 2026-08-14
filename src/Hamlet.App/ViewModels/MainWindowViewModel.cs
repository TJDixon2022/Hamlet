using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls.ApplicationLifetimes;
using Hamlet.App.Licensing;
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
    private LicenseResolution? _licenseMismatch;

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

    /// <summary>The frequencies the operator saved (HM-DEC-060).</summary>
    public ObservableCollection<Favorite> Favorites { get; } = new();

    /// <summary>
    /// What the star says: the favorite's name here, or the invitation to save.
    /// </summary>
    [ObservableProperty]
    private string _favoriteLabel = "save this spot";

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

            return SignalsAreSimulated
                ? $"simulated signals · {SelectedBand.Band.Name}"
                : $"receiving · {SelectedBand.Band.Name}";
        }
    }

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
        _terminalExpanded = settings.IsPanelExpanded(PanelKeys.Terminal);
        _storyExpanded = settings.IsPanelExpanded(PanelKeys.Story);
        _guideExpanded = settings.IsPanelExpanded(PanelKeys.Guide);
        _spotsExpanded = settings.IsPanelExpanded(PanelKeys.Spots);
        _leadExpanded = settings.IsPanelExpanded(PanelKeys.Lead);
        _contactExpanded = settings.IsPanelExpanded(PanelKeys.Contact);
        _transmitExpanded = settings.IsPanelExpanded(PanelKeys.Transmit);
        _phrasebookExpanded = settings.IsPanelExpanded(PanelKeys.Phrasebook);

        ContactShape = new ContactShapeViewModel(settings.Operator.Callsign);

        // Everything the guard and the break-in precondition need, read at the
        // moment somebody presses rather than captured when the panel was built
        // (HM-DEC-059).
        Transmit = new CwTransmitViewModel(BuildTransmitContext)
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

        PickByline();

        _ = ReloadSpotsAsync("startup");
        _ = ResolveProfileAsync();
        ApplyFeedTimers();
    }

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
            DataContext = new FavoritesViewModel(Favorites, PersistFavorites),
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
        OnPropertyChanged(nameof(HasFavorites));
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
    }

    /// <summary>Tune the rig (and the whole UI) to a target — the payoff
    /// click on every story and spot.</summary>
    [RelayCommand]
    private void TuneTo(long hz)
    {
        AppEvents.TuneRequested(_telemetry, hz, "story_or_spot");
        MarkActedOn(hz);
        var band = BandPlan.BandFor(hz);
        if (band is not null && band.Name != SelectedBand.Band.Name)
        {
            SelectedBand = Bands.First(b => b.Band.Name == band.Name);
        }

        FrequencyHz = hz;
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

    partial void OnTerminalExpandedChanged(bool value)
        => PersistPanel(PanelKeys.Terminal, value);

    partial void OnStoryExpandedChanged(bool value) => PersistPanel(PanelKeys.Story, value);

    partial void OnGuideExpandedChanged(bool value) => PersistPanel(PanelKeys.Guide, value);

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

        // The break-in setting and the mode both live in here, and both decide
        // whether a send would reach the air. So the panel re-asks whenever the
        // radio says anything, and somebody reads "break-in is off" before they
        // press rather than after (HM-DEC-059).
        Transmit.Refresh();
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

        _decoder = new CwDecoder(_audioInput.SampleRate, _settings.CwPitchHz);
        _decoder.CharacterDecoded += Transcript.Append;
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

        DetectedWpm = _decoder.State.WordsPerMinute;
        DecodeNote = _decoder.Watch.NoteText;
        OnPropertyChanged(nameof(TerminalSummary));

        // OFFERED, NEVER ASSERTED (HM-DEC-059, HM-OPEN-006). The decoder
        // measured what the other station is sending at, so Hamlet may say so.
        // It has never asked what speed this operator can copy, so it may not
        // claim that this one suits them.
        Transmit.HeardWpm = _decoder.State.HasSignal ? _decoder.State.WordsPerMinute : null;
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
            StatusText = $"No answer on {port}. Check the cable, the baud rate and "
                       + "the CI-V address on the radio";
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
        // synthesiser; a real one will get CI-V 0x27 in phase 2 and, until
        // then, honestly gets nothing rather than synthetic signals dressed
        // as its own.
        if (rig.IsSimulated)
        {
            StartTrainingSpectrum();
        }
        else
        {
            StopTrainingSpectrum();
        }

        // Audio, on the other hand, both radios can supply: the training radio
        // makes its own Morse and a real one arrives through the capture device
        // the operator chose. So the terminal fills in either way.
        StartDecoding();
        StartRigMonitor(rig);

        // The one door to the transmitter, and it only exists while a radio is
        // connected (§0.2, HM-DEC-059).
        Transmit.Attach(new CwTransmitter(new KeyerCwSender(rig)));

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

    private void OnAgeTick(object? sender, EventArgs e) => UpdateSpotFreshness();

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
        var ranked = SpotRanking.Rank(kept, Lifetimes);

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
    /// <remarks>
    /// By frequency bucket rather than by card, because the click that tunes
    /// carries a frequency and two skimmers measuring the same carrier rarely
    /// agree to the hertz. It is the same bucket the store identifies a spot by
    /// (<see cref="SpotIdentity.FrequencyBucketHz"/>), so the two cannot drift.
    /// </remarks>
    private void MarkActedOn(long hz)
    {
        var bucket = hz / SpotIdentity.FrequencyBucketHz;
        var now = DateTime.UtcNow;

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
        }
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
    private GridResolution? _gridMismatch;

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
            Transmit.Attach(null);
            StopTrainingSpectrum();
            _rigSendTimer.Stop();
            _rigSendPending = false;

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

        Provenance = $"{_spot.Mode} · {_spot.Source} · "
            + SpotLifetime.DescribeOpportunity(_spot, elapsed, _lifetimes)
            + (Distance.Length > 0 ? $" · {Distance}" : "");

        // The exact figure stays available for anybody who wants it, which is
        // the trade that lets the card speak in words (HM-DEC-045).
        AgeTooltip = $"Reported {SpotFreshness.Describe(elapsed)} by {_spot.Source}.";

        if (IsNew && nowUtc >= _newUntilUtc)
        {
            IsNew = false;
        }
    }
}

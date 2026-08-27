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

    /// <summary>Where the decoder's counters stood, recently, on the audio clock.</summary>
    private CwCounterTrail? _counters;

    /// <summary>
    /// Whether Hamlet can hear keying at all, said independently of the decoder.
    /// </summary>
    private CwKeyingMeter? _keyingMeter;

    /// <summary>The meter's work, off the interface thread.</summary>
    private Task<KeyingReading>? _meterWork;

    /// <summary>When the meter last looked.</summary>
    private DateTime _meterLastUtc = DateTime.MinValue;

    /// <summary>When the current decoder began listening, or null when none is.</summary>
    private DateTime? _decoderStartedUtc;
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

    /// <summary>
    /// True while the radio is transmitting and the decoder is standing down.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SuspendedNote))]
    [NotifyPropertyChangedFor(nameof(AdvisoryNote))]
    [NotifyPropertyChangedFor(nameof(ShowKeyingMeter))]
    private bool _decodingIsSuspended;

    /// <summary>
    /// True while the decoder is refilling a window it emptied to follow
    /// somebody else.
    /// </summary>
    /// <remarks>
    /// Carried as a property of its own, the way the suspended state is, so the
    /// sentence below is reachable from the screen rather than only from the
    /// decoder: the region that shows it is the thing that has gone missing
    /// before, and a test can only prove it by driving this.
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FollowedNote))]
    [NotifyPropertyChangedFor(nameof(AdvisoryNote))]
    [NotifyPropertyChangedFor(nameof(ShowKeyingMeter))]
    private bool _listeningAfresh;

    /// <summary>
    /// The one advisory the terminal is showing, by priority.
    /// </summary>
    /// <remarks>
    /// <para>**THE TRANSCRIPT MUST NOT MOVE UNDER HIM.** Below it sat a stack of
    /// boxes that each appeared and vanished on their own, and every one of them
    /// reflowed everything around it. He watches that screen for half an hour at
    /// a time while tuning across a band, and the thing he is reading moved.</para>
    /// <para>**SO THERE IS ONE REGION OF FIXED HEIGHT AND ITS CONTENT SWAPS.**
    /// The messages are genuinely alternatives: several of them were saying
    /// versions of the same thing at the same time, that nothing is being read.
    /// Nothing was removed; what changed is that only the most useful one is on
    /// screen, and the region occupies its space whether or not it has anything
    /// to say.</para>
    /// <para>**THE ORDER IS BY HOW WRONG THE OPERATOR WOULD BE WITHOUT IT.**
    /// Suspension first, because a terminal that has stopped without saying why
    /// reads as a quiet band. Then the two that mean somebody else started
    /// sending, then what Hamlet can see when it is producing nothing, then the
    /// notes about its own limits, and last the ones about the settings.</para>
    /// <para>**TO ADD A MESSAGE**, put it in this list at the place its urgency
    /// earns and give it a tone. Do not add a panel below the transcript, and do
    /// not make an existing one conditional: either of those brings the jump
    /// back.</para>
    /// </remarks>
    public string AdvisoryNote
    {
        get
        {
            foreach (var text in Advisories())
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return "";
        }
    }

    /// <summary>
    /// Whether the keying meter's own block is shown beneath the advisory.
    /// </summary>
    /// <remarks>
    /// <para>**TWO VOICES SAYING DIFFERENT THINGS AT THE SAME TIME IS WORSE THAN
    /// EITHER OF THEM.** On the evening of 2026-08-25 the advisory said a clear
    /// tone was present and the meter's block underneath it said there was no
    /// keying, the two disagreeing about the pitch by fifty hertz — and the
    /// block's advice sends the operator across the room to change a setting on
    /// the radio for what is a decoder problem.</para>
    /// <para>**THE METER IS NOT RETIRED AND MUST NOT BE.** It is the one
    /// instrument that shares nothing with the decoder, and its whole value is
    /// that it can contradict it (HM-DEC-091). On `cw-2026-08-22-012823` it found
    /// the right frequency while the decoder took the wrong one. What is
    /// suppressed is only its block *while the advisory has something to say*,
    /// which is the case where a second, quieter, less reliable voice can only
    /// confuse: agreed with independent measurement six times and contradicted it
    /// eleven, across everything analysed from 2026-08-22 to 2026-08-25.</para>
    /// <para>When the advisory is silent the meter speaks, exactly as before.</para>
    /// </remarks>
    public bool ShowKeyingMeter
        => _settings.ShowKeyingSweep
            && IsDecoding
            && string.IsNullOrWhiteSpace(AdvisoryNote);

    /// <summary>Every advisory the terminal can show, most urgent first.</summary>
    private IEnumerable<string> Advisories()
    {
        yield return SuspendedNote;

        // **SECOND ONLY TO HIM SENDING**, because it is the one condition that
        // stops the band being readable at all and it is one press away from
        // being fixed.
        yield return OverflowAdvice;

        // **AND WHAT ELSE ON THE RECEIVE SIDE IS IN THE WAY**, which sits below
        // an overloading front end because that one stops the band being
        // readable at all while these degrade it. Read-only, and each names the
        // control on the front of the radio rather than stopping at the
        // diagnosis (HM-DEC-148).
        yield return ReceiveObstructionText;

        // **WHAT THE DECODER IS LISTENING TO, WHEN IT IS BEING HELD THERE.** It
        // sits below what is in the way because it is a state the operator chose
        // rather than a fault he needs to fix.
        yield return PitchLockText;

        // **WHY THE SCREEN JUST WENT QUIET.** Following somebody empties the
        // decoder's window, and twelve seconds of nothing with no explanation
        // reads as a dead band at the one moment it certainly is not one.
        yield return FollowedNote;

        yield return CaptureNote;
        yield return DecoderStory;
    }

    /// <summary>What the terminal says while it refills after following somebody.</summary>
    /// <remarks>
    /// The window holds twelve seconds and all of it was listened to at the other
    /// station's pitch, so it is thrown away rather than decoded as a mixture
    /// (HM-DEC-009). The cost is real and is stated rather than hidden.
    /// </remarks>
    public string FollowedNote
        => ListeningAfresh
            ? "somebody else has started sending and Hamlet has moved across to "
              + "them, so it has let go of what it was holding, because those "
              + "twelve seconds were listened to at the other station's pitch and "
              + "reading them now would put one operator's letters in the other's "
              + "mouth. Give it a few seconds to fill up again and the text picks "
              + "up where the new station is."
            : "";

    /// <summary>What the terminal says while the operator is sending.</summary>
    /// <remarks>
    /// **A TERMINAL THAT HAS STOPPED WITHOUT SAYING WHY IS ITS OWN CONFIDENT
    /// WRONG ANSWER.** An empty screen reads as a quiet band, and the one moment
    /// it is guaranteed not to be quiet is while the operator has his hand on the
    /// key.
    /// </remarks>
    public string SuspendedNote
        => DecodingIsSuspended
            ? "You are sending, so Hamlet is listening to you rather than to the "
              + "band. Whatever you key is yours and never appears here as "
              + "somebody else's. It picks the band up again a moment after you "
              + "stop."
            : "";

    /// <summary>The sending speed the decoder is tracking, or 0.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TerminalSummary))]
    [NotifyPropertyChangedFor(nameof(TerminalSpeedText))]
    [NotifyPropertyChangedFor(nameof(HasDetectedSpeed))]
    private int _detectedWpm;

    /// <summary>Whether the decoder is listening to anything.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TerminalSummary))]
    [NotifyPropertyChangedFor(nameof(TerminalIdleText))]
    [NotifyPropertyChangedFor(nameof(ShowKeyingMeter))]
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

    /// <summary>Whether the calling panel is open (§0.5).</summary>
    [ObservableProperty]
    private bool _autoCallExpanded = true;

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

    /// <summary>
    /// When a character was last actually read, for the mode-follow guard.
    /// </summary>
    /// <remarks>
    /// **SEPARATE FROM `_lastDecodeUtc` BECAUSE THAT ONE IS SEEDED AT THE START
    /// OF LISTENING** and this one must not be: a decoder that has just been
    /// switched on has read nothing, and treating that as somebody working Morse
    /// is the whole defect being fixed here.
    /// </remarks>
    private DateTime _lastCharacterUtc = DateTime.MinValue;

    /// <summary>
    /// How long after a character the operator still counts as working Morse.
    /// </summary>
    /// <remarks>
    /// Half a minute. An exchange has gaps of several seconds between overs and a
    /// slow sender leaves long ones inside a message, so anything much shorter
    /// would call him idle in the middle of a contact. Much longer and a station
    /// that finished five minutes ago would still be pinning the mode.
    /// </remarks>
    private static readonly TimeSpan CopyingMorseFor = TimeSpan.FromSeconds(30);

    /// <summary>
    /// True while somebody's Morse is actually coming through.
    /// </summary>
    /// <remarks>
    /// <para>**THIS IS WHAT `IsDecoding` WAS BEING ASKED TO MEAN AND DOES NOT.**
    /// That property is true from the moment the decoder starts listening until
    /// it stops, which is the whole session: it says the decoder is switched on
    /// and nothing about whether anybody is sending. Mode-follow read it as
    /// evidence the operator was working Morse, so **every target that was not CW
    /// was refused, permanently**, and the radio stayed in CW at 14.243 MHz where
    /// the map says upper sideband.</para>
    /// <para>The guard it feeds is right and stays. On 2026-08-18 mode-follow
    /// wrote USB with the data variant on, over and over, while the operator sat
    /// on CW main street with a signal decoding, and the send controls refused
    /// `not_in_morse` for sixty-six seconds: **he could not answer a station
    /// because the app had moved his radio out from under him.** What was wrong
    /// was the evidence, not the rule.</para>
    /// </remarks>
    private bool IsCopyingMorse
        => _lastCharacterUtc != DateTime.MinValue
           && DateTime.UtcNow - _lastCharacterUtc < CopyingMorseFor;

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

        // **THIS EVENT WAS BUILT BY HM-DEC-092 AND NOTHING EVER CALLED IT.**
        // The link's own health — sent, answered, unanswered, and now what
        // arrived unasked — has never once reached the record, which is why two
        // sessions argued about whether the radio broadcasts from a telemetry
        // field that cannot express the answer. Ruled, built, never invoked, for
        // the third time in this repository.
        if (_rig is Ic7300Rig radio)
        {
            AppEvents.CivLink(
                _telemetry, radio.Link,
                _rigSpectrum?.SweepCount ?? 0,
                _rigSpectrum?.DroppedCount ?? 0);
        }

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

            if (_rigSpectrum is { SweepCount: 0 })
            {
                return $"parts arriving, no complete sweep · {SelectedBand.Band.Name}";
            }

            // **RECEIVING MEANS FRAMES ARE ARRIVING NOW, NOT THAT ONE ONCE DID.**
            // This used to be the cumulative sweep count, so the first sweep of
            // an evening bought the word for the rest of it: the cable could
            // come out and the summary would go on saying "receiving" until the
            // app was restarted. §0.0 is broken by a single word there, and it
            // is the same word HM-DEC-093 was raised about.
            return ScopeIsFlowing
                ? $"receiving · {SelectedBand.Band.Name}"
                : $"nothing arriving now · {SelectedBand.Band.Name}";
        }
    }

    /// <summary>True when spectrum data is arriving right now.</summary>
    /// <remarks>
    /// A measurement of the last part's age rather than a count of parts ever
    /// seen, because those two answer different questions and only one of them
    /// is "is this working" (§0.0).
    /// </remarks>
    public bool ScopeIsFlowing => WhichScopeStage() == ScopeStage.Flowing;

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

            var quiet = stream.LastPartUtc is { } last
                ? (int)(DateTime.UtcNow - last).TotalSeconds
                : 0;

            // **THE DECIDING IS THE ENGINE'S AND THE DRAWING IS THIS FILE'S**
            // (§0.1). A stage count is a radio fact, and the same fault this
            // exists to catch was invisible for weeks because nothing measured
            // it, so the measurement lives where a test can reach it.
            return ScopeFlow.Say(WhichScopeStage(), stream.PartsReceived,
                stream.PartsParsed, quiet);
        }
    }

    /// <summary>Which stage of the scope path is wrong, or none.</summary>
    private ScopeStage WhichScopeStage()
        => _rigSpectrum is not { } stream
            ? ScopeStage.NotAttached
            : ScopeFlow.Check(
                attached: true,
                stream.PartsReceived,
                stream.PartsParsed,
                stream.SweepsDelivered,
                stream.LastPartUtc,
                DateTime.UtcNow);

    /// <summary>
    /// The stage counts, whether or not anything is wrong (§0.0.1).
    /// </summary>
    /// <remarks>
    /// **HIDING THE ROW MAY NOT MEAN LOSING THE NUMBERS.** The counters are what
    /// proved the scope path was discarding 2,740 parts, so they stay one hover
    /// away rather than being deleted along with the row that shouted them.
    /// </remarks>
    public string ScopeCounts
    {
        get
        {
            if (_rigSpectrum is not { } stream)
            {
                return "Nothing is attached to the waterfall yet.";
            }

            var rejected = stream.PartsRejected == 0
                ? ""
                : $", {stream.PartsRejected} thrown away";

            var since = stream.LastPartUtc is { } last
                ? $", last one {(int)(DateTime.UtcNow - last).TotalSeconds} seconds ago"
                : ", and none has ever arrived";

            return $"{stream.PartsReceived} parts in, {stream.PartsParsed} read"
                + $"{rejected}, {stream.SweepsDelivered} sweeps drawn{since}.";
        }
    }

    /// <summary>True when a stage is wrong and the row has to be on screen.</summary>
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

    /// <summary>
    /// The receiver's front end, in one chip beside the filter width.
    /// </summary>
    /// <remarks>
    /// <para>**HE WAS LOOKING AT THIS SCREEN WHILE IT HAPPENED.** On 20 metres in
    /// daylight at S9 with nothing readable, the radio was reporting
    /// `Overflow: overloading` with preamp 1 on, and Hamlet was reading it every
    /// quarter of a second and saying nothing. Overload compresses the whole
    /// passband together, so there is no tone standing above anything and the
    /// decoder measures amplitude: the ear can still take rhythm out of a
    /// compressed mess and the decoder cannot.</para>
    /// <para>**A VALUE NEVER READ SAYS SO** (HM-DEC-009). Not a blank and not a
    /// default: a panel asserting the preamp is off when the read failed is worse
    /// than one saying it does not know.</para>
    /// <para>**AND `RfGain` IS NOT HERE.** The operator has seen it report
    /// 100 per cent with the knob at noon, and a figure he has already watched
    /// contradict his own radio does not go on a panel (§0.0).</para>
    /// </remarks>
    [ObservableProperty]
    private string _frontEndText = "preamp unknown · attenuator unknown";

    /// <summary>The preamplifier setting in the radio's own words, or unknown.</summary>
    [ObservableProperty]
    private string _preampText = "preamp unknown";

    /// <summary>The attenuator setting in the radio's own words, or unknown.</summary>
    [ObservableProperty]
    private string _attenuatorText = "attenuator unknown";

    /// <summary>True while the preamplifier is switched on.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverflowAdvice))]
    private bool _preampIsOn;

    /// <summary>True while the radio reports its front end overloading.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OverflowAdvice))]
    [NotifyPropertyChangedFor(nameof(AdvisoryNote))]
    [NotifyPropertyChangedFor(nameof(ShowKeyingMeter))]
    private bool _frontEndIsOverloading;

    /// <summary>What to do about an overloading front end, in terms of a knob.</summary>
    /// <remarks>
    /// <para>**"YOUR RECEIVER IS OVERLOADING" IS A DIAGNOSIS AND NOT HELP.** The
    /// operator this application is for has never thought about front-end
    /// overload, and what he needs is the name of the button. On the IC-7300 the
    /// preamp and the attenuator share **P.AMP/ATT**, and each press cycles
    /// preamp 1, preamp 2 and off (§4, Full Manual `A7292-4EX-6`).</para>
    /// <para>**THE ATTENUATOR IS MENTIONED ONLY ONCE THE PREAMP IS ALREADY
    /// OFF**, because advice about a knob already in the right position is noise.
    /// And nothing here mentions RF gain: Hamlet's read of it is not trusted, so
    /// it advises on nothing it cannot see.</para>
    /// <para>**IT SAYS IT ONCE AND LETS IT STAND.** The sentence does not change
    /// while the condition holds, so nothing blinks or re-announces itself at him
    /// four times a second.</para>
    /// </remarks>
    public string OverflowAdvice => OverflowAdviceFor(FrontEndIsOverloading, PreampIsOn);

    /// <summary>
    /// What else on the receive side is standing in the way, in one line.
    /// </summary>
    /// <remarks>
    /// <para>**HM-DEC-148 DID THIS FOR THE PREAMP AND STOPPED THERE**, and the
    /// noise blanker, the noise reduction and the filter width are the same class
    /// of fault: Hamlet reads all three from the radio, writes them into the
    /// capture sidecar, and has never mentioned one of them on the screen the
    /// operator is looking at. A thing Hamlet knows and does not say is the same
    /// defect as a decode with no signal behind it.</para>
    /// <para>**READ-ONLY.** Nothing here writes to the radio; the rule it comes
    /// from has nowhere to put a command. A later unit may offer a button he
    /// presses.</para>
    /// <para>The rule itself is in the engine, because it is radio knowledge
    /// (§0.1), and this is the seam that shows it.</para>
    /// </remarks>
    [ObservableProperty]
    private string _receiveObstructionText = "";

    /// <summary>
    /// Whether the decoder's pitch is held, and what it is held at.
    /// </summary>
    /// <remarks>
    /// <para>**A LOCK THE OPERATOR CANNOT SEE IS A LOCK HE CANNOT TRUST.** A
    /// wandering decode and a held one look identical on screen, so without this
    /// the operator has no way to tell whether the thing he pressed did
    /// anything, and no way to tell later that it is still holding.</para>
    /// <para>It says the pitch to a tenth of a hertz because that is what the
    /// lock actually holds — an interpolated peak, not a bin — and rounding it to
    /// a whole number on the panel would make two different locks look like the
    /// same one.</para>
    /// <para>Empty while the tracker is steering, so nothing is said when there
    /// is nothing to say (HM-DEC-148's precedent for the advisory area).</para>
    /// </remarks>
    [ObservableProperty]
    private string _pitchLockText = "";

    /// <summary>What the lock control reads right now.</summary>
    [ObservableProperty]
    private string _pitchLockLabel = "Hold this pitch";

    /// <summary>
    /// Hold the decoder's pitch where the station is, or let it follow again.
    /// </summary>
    /// <remarks>
    /// <para>**THE TRACKER IS MEASURABLY THE LARGEST SOURCE OF SOUP IN THIS
    /// DECODER**, and until now the operator had no way to take it out of the
    /// path. Unit 002 put a clean generated station through the production path
    /// and got twenty-two characters that were never sent, and through the same
    /// window with the pitch nailed it got none.</para>
    /// <para>**IT LOCKS TO THE MEASURED PEAK AND NOT TO THE RADIO'S CW PITCH.**
    /// A capture from 2026-08-24 carries `CwPitch 600 Hz` while the station in it
    /// sat at 439.81, so a lock to the radio's setting would have pointed the
    /// filter at empty spectrum and held it there.</para>
    /// <para>Where nothing can be measured it refuses and says so, rather than
    /// holding a pitch nobody found (§0.0).</para>
    /// </remarks>
    [RelayCommand]
    private void TogglePitchLock()
    {
        if (_decoder is not { } decoder)
        {
            return;
        }

        if (decoder.IsLocked)
        {
            decoder.Unlock();
            PitchLockLabel = "Hold this pitch";
            PitchLockText = "";

            return;
        }

        var locked = decoder.Lock();

        if (double.IsNaN(locked))
        {
            PitchLockText =
                "There is not enough measured yet to hold a pitch, so nothing "
                + "was locked and the decoder is still following. Give it a few "
                + "seconds of a station and press again.";

            return;
        }

        PitchLockLabel = "Follow again";
    }

    /// <summary>The rule itself, so the test reads it rather than a copy (§0).</summary>
    /// <param name="overloading">Whether the radio says its front end is overloading.</param>
    /// <param name="preampIsOn">Whether the preamplifier is switched on.</param>
    /// <returns>What to say, or "" when there is nothing to say.</returns>
    internal static string OverflowAdviceFor(bool overloading, bool preampIsOn)
    {
        if (!overloading)
        {
            return "";
        }

        return preampIsOn
            ? "The radio says its front end is overloading, which means the signal "
              + "coming in is stronger than the receiver can handle and everything "
              + "in the passband is being squashed together. Nothing will decode "
              + "until that stops. Press P.AMP/ATT on the front of the radio until "
              + "the preamp reads off."
            : "The radio says its front end is overloading, and the preamp is "
              + "already off, so the next thing to try is the attenuator. Hold "
              + "P.AMP/ATT for a moment to bring it in. A strong band in daylight "
              + "can do this on its own.";
    }

    /// <summary>What the chip beside the filter width reads.</summary>
    /// <param name="overloading">Whether the radio says its front end is overloading.</param>
    /// <param name="preamp">The preamplifier in the radio's words, or unknown.</param>
    /// <param name="attenuator">The attenuator in the radio's words, or unknown.</param>
    /// <returns>The chip's text.</returns>
    /// <remarks>
    /// While it is overloading the chip leads with that and keeps the preamp
    /// beside it, because the preamp is the thing he is about to change and the
    /// attenuator is one step further on.
    /// </remarks>
    internal static string FrontEndTextFor(
        bool overloading, string preamp, string attenuator)
        => overloading ? $"overloading · {preamp}" : $"{preamp} · {attenuator}";

    /// <summary>The preamplifier, named rather than just valued.</summary>
    /// <param name="setting">0, 1 or 2, or null when it has never been read.</param>
    /// <returns>The label.</returns>
    /// <remarks>
    /// **"on" WOULD NOT DO.** Preamp 1 and preamp 2 are different settings on this
    /// radio and an operator deciding whether his front end is overloading needs
    /// to know which one is in. And a setting never read says so rather than
    /// defaulting to off (HM-DEC-009).
    /// </remarks>
    internal static string PreampLabel(int? setting) => setting switch
    {
        0 => "preamp off",
        1 => "preamp 1",
        2 => "preamp 2",
        null => "preamp unknown",
        _ => $"preamp {setting}",
    };

    /// <summary>The attenuator, named rather than just valued.</summary>
    /// <param name="decibels">The attenuation, or null when never read.</param>
    /// <returns>The label.</returns>
    internal static string AttenuatorLabel(int? decibels) => decibels switch
    {
        null => "att unknown",
        0 => "att off",
        _ => $"att {decibels} dB",
    };

    /// <summary>True once the filter width has been read from the radio.</summary>
    public bool HasFilterBandwidth => FilterBandwidthText.Length > 0;

    /// <summary>Everything Hamlet currently knows about the radio.</summary>
    public RigState RigState => _rigMonitor?.State ?? RigState.Empty;

    /// <summary>True once the decoder is tracking a speed worth showing.</summary>
    public bool HasDetectedSpeed => DetectedWpm > 0;

    /// <summary>The live speed readout on the terminal's header.</summary>
    public string TerminalSpeedText
        => DetectedWpm > 0 ? $"{DetectedWpm} WPM" : "";

    /// <summary>
    /// Why the speed field is empty, or empty itself (HM-OPEN-022).
    /// </summary>
    /// <remarks>
    /// <para>**A BLANK BOX AND A BLANK BOX THAT SAYS WHY ARE DIFFERENT THINGS**
    /// (§0.0.1). No speed is named until a clock has been proved, because a
    /// number between two stations describes neither of them, and the field goes
    /// quiet across a handover. Left as a bare gap it reads as something broken;
    /// this is the sentence that makes it read as Hamlet working.</para>
    /// </remarks>
    public string SpeedReacquiringText
        => SpeedIsReacquiring ? "working out the speed" : "";

    /// <summary>True while no speed has been proved.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpeedReacquiringText))]
    private bool _speedIsReacquiring;

    /// <summary>
    /// What just changed about who is sending, or empty (HM-DEC-096).
    /// </summary>
    /// <remarks>
    /// **A SPEED CHANGE AND A TRACKER SWITCH BOTH MEAN SOMEBODY ELSE STARTED
    /// TRANSMITTING**, which is the single most useful thing the decoder knows
    /// and the one it never said. Annotated rather than silently absorbed,
    /// because the alternative is a transcript in which two stations run into
    /// one another with nothing marking the seam.
    /// </remarks>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasHandover))]
    private string _handoverNote = "";

    /// <summary>True when there is a handover worth marking.</summary>
    public bool HasHandover => HandoverNote.Length > 0;

    /// <summary>
    /// What the leading edge is reading ahead of the settled pass.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTip))]
    private string _tipText = "";

    /// <summary>True when the leading edge is ahead of the settled pass.</summary>
    public bool HasTip => TipText.Length > 0;

    // **THE SETTLED PASS'S OWN READOUTS WENT WITH IT.** The tip mark, the
    // ceiling note and the revision count were all one pass reporting on
    // another: whether the second was refusing behind the leading edge, whether
    // its window had hit the ceiling thirty elements wanted, and how often it
    // changed the first one's mind. There is one pass now, and a line describing
    // a decoder that is gone is the fault this removal exists to end
    // (HM-DEC-091).

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
                return "listening";
            }

            var speed = DetectedWpm > 0 ? $"{DetectedWpm} WPM · " : "";
            var tail = Transcript.Tail(28);

            return $"{speed}{tail}";
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
            HfBands.Bands.Select(b => new BandButtonViewModel(b)));

        _selectedBand = Bands.FirstOrDefault(b => b.Band.Name == settings.LastBand)
                        ?? Bands.First(b => b.Band.Name == "40 m");
        _frequencyHz = _selectedBand.Band.JumpHz;

        _mapExpanded = settings.IsPanelExpanded(PanelKeys.Map);
        _tapeExpanded = settings.IsPanelExpanded(PanelKeys.Tape);
        _waterfallExpanded = settings.IsPanelExpanded(PanelKeys.Waterfall);
        _scanExpanded = settings.IsPanelExpanded(PanelKeys.Scan);
        _autoCallExpanded = settings.IsPanelExpanded(PanelKeys.AutoCall);
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
                saved.Neighborhood, saved.SavedUtc, saved.Note));
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
                saved.Neighborhood, saved.VisitedUtc, source,
                // A profile written before HM-DEC-134 has no count and the
                // property defaults to one, which is what an entry in this list
                // has always meant. Nothing is migrated because nothing is lost.
                Math.Max(1, saved.Visits)));
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
        // **THE PREDICATE OUTLIVES THE ORDER THEY ARE BUILT IN**, and the null
        // is real rather than papered over: between these two statements there is
        // no calling cycle to be running, so "not transmitting" is the true
        // answer in that window and not a convenient one.
        Scan = new ScanViewModel(
            line => StatusText = line,
            tune: TuneTo,
            transmitting: () => _autoCall?.IsCalling ?? false);

        // **THE TWO ASK EACH OTHER RATHER THAN TRACKING EACH OTHER**
        // (HM-DEC-098). The scanner moves the dial and this transmits on
        // it, so a stale copy of "is the other one running" is exactly the
        // state that would let Hamlet transmit mid-tune on a frequency
        // neither component believes it is on.
        AutoCall = _autoCall = new AutoCallViewModel(
            line => StatusText = line, () => Scan.IsScanning);

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

        // **THE FIRST SPOT LOAD WAITS FOR THE RADIO** (HM-DEC-118). It used to
        // run from here, before anything was connected, so RBN was filtered and
        // the skimmer watch scoped to whatever band was last remembered
        // (HM-DEC-024, HM-DEC-075). An empty panel asserts nothing and a
        // wrong-band panel asserts something false, which is the distinction
        // §0.0 exists to draw, and the cost of waiting is a second or two of
        // empty on a screen just opened.
        //
        // It is kicked off by `ReconnectOnStartupAsync` instead, which runs from
        // the window's Opened event and knows where the radio is. Asking the
        // radio from here stays rejected: it would put a serial read on the path
        // that builds the window.
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
            case Layout.Widgets.AutoCall: AutoCallExpanded = true; break;
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

    /// <summary>Calling CQ on a cycle, into a dummy load (HM-DEC-098).</summary>
    public AutoCallViewModel AutoCall { get; }

    /// <summary>The same object, for the scanner's predicate to read safely.</summary>
    private readonly AutoCallViewModel? _autoCall;

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
                Favorites, PersistFavorites, Recent, StarRecent, ForgetRecent),
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
                Note = f.Note,
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

        // **WRITTEN WITHOUT GOING BACK THROUGH THE SETTER**, which would save
        // the file on every tune and, worse, write the previous frequency's note
        // onto this one on the way past.
        _favoriteNote = here?.Note ?? "";
        OnPropertyChanged(nameof(FavoriteNote));
    }

    /// <summary>
    /// Why this favorite, in the operator's own words (HM-DEC-060).
    /// </summary>
    /// <remarks>
    /// <para>**THE NAME SAYS WHERE AND THIS SAYS WHY.** Hamlet can name the block
    /// from the map and it cannot know that this is the net that meets on
    /// Tuesdays. Nothing derives this, suggests it or fills it in: it is the one
    /// part of a favorite that is entirely the operator's (§0.0).</para>
    /// <para>It sits in the strip beside the name, which is where a favorite
    /// already speaks (HM-DEC-070), and it is there from the moment the star
    /// lights rather than behind a management window — a box somebody has to go
    /// and find is a box that never gets written. Empty is the ordinary state and
    /// stays empty.</para>
    /// </remarks>
    public string FavoriteNote
    {
        get => _favoriteNote;

        set
        {
            var text = (value ?? "").Trim();

            if (text == _favoriteNote)
            {
                return;
            }

            _favoriteNote = text;
            OnPropertyChanged();

            if (RadioEngine.Explore.Favorites.At(Favorites, FrequencyHz)
                is not { } here)
            {
                // No favorite here to carry it. Nothing is saved and nothing is
                // lost, because there is nothing yet for a note to belong to.
                return;
            }

            var index = Favorites.IndexOf(here);
            Favorites[index] = here with { Note = text };

            PersistFavorites();
        }
    }

    private string _favoriteNote = "";

    /// <summary>Tune the rig (and the whole UI) to a target — the payoff
    /// click on every story and spot.</summary>
    [RelayCommand]
    private void TuneTo(long hz)
    {
        AppEvents.TuneRequested(_telemetry, hz, "story_or_spot");
        var arrivedOn = MarkActedOn(hz);
        var band = HfBands.BandFor(hz);
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

    partial void OnAutoCallExpandedChanged(bool value)
        => PersistPanel(PanelKeys.AutoCall, value);

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

        // **NOTHING HERE TURNS THE SCOPE ON, WHICH IS WHAT HM-DEC-062 ALREADY
        // SAID.** `_ = AskForTheSpectrumAsync(radio)` stood on this line from
        // 8c2abf3, version 1.8.0, and wrote `27 11` at every connect against a
        // standing ruling that this path is reads only. Taking it out restores
        // that ruling rather than departing from one.
        //
        // The arithmetic is why it matters rather than being tidy. A waveform
        // sweep is 475 points in 11 parts, on the order of six hundred bytes,
        // and 115200 8N1 carries about eleven and a half thousand bytes a
        // second. Nineteen sweeps a second is the whole cable, and the dial's own
        // announcements share it. HM-OPEN-042 then found the readback could not
        // confirm this write, so Hamlet has been reporting it refused without
        // knowing, and it may have been succeeding at every connect since 1.8.0.
        //
        // Reading `27 10` and `27 11` to say what is on stays. That is the read
        // HM-DEC-062 allows, and it is what the panel needs to explain itself.
    }

    /// <summary>
    /// How the conversation with the radio is going, in one sentence (§0.0.1).
    /// </summary>
    /// <remarks>
    /// **THIS IS THE LINE THAT WOULD HAVE SAVED TWO BUILDS.** The operator turned
    /// his dial and watched Hamlet follow thirty seconds later, and the
    /// application said nothing about it: the frequency was drawn confidently
    /// four times a second while being a minute old. Everything needed to say so
    /// was already in the app and nothing assembled it.
    /// </remarks>
    public string LinkCheckLine => LinkSelfCheck
        .Describe((_rig as Ic7300Rig)?.Link, RigState, DateTime.UtcNow, IsConnected)
        .Headline;

    /// <summary>The numbers behind that sentence, for the diagnostics screen.</summary>
    public string LinkCheckDetail => LinkSelfCheck
        .Describe((_rig as Ic7300Rig)?.Link, RigState, DateTime.UtcNow, IsConnected)
        .Detail;

    /// <summary>True while there is a link check worth showing.</summary>
    public bool HasLinkCheck => LinkCheckLine.Length > 0;

    /// <summary>
    /// How old the frequency on screen is, said rather than counted, or "".
    /// </summary>
    /// <remarks>
    /// **A PROVENANCE LABEL CARRIES ITS AGE** (HM-DEC-111). That ruling came from
    /// a capture sidecar asserting a freshness it did not have, and the rig
    /// display has been doing the same thing to the one number every other
    /// surface trusts. Empty while the reading is current, because a fresh value
    /// needs no apology and a caption on every screen is a caption nobody reads.
    /// </remarks>
    public string FrequencyAgeNote
    {
        get
        {
            if (!IsConnected)
            {
                return "";
            }

            var value = RigState[RigField.Frequency];

            if (!value.IsKnown)
            {
                return "";
            }

            return value.Age(DateTime.UtcNow) is { } age
                   && age >= LinkSelfCheck.FrequencyIsOldAfter
                ? "where the radio was a moment ago, not where it is now"
                : "";
        }
    }

    /// <summary>True while that note has something to say.</summary>
    public bool HasFrequencyAgeNote => FrequencyAgeNote.Length > 0;

    /// <summary>
    /// What became of the scope's data output, as far as Hamlet knows.
    /// </summary>
    /// <remarks>
    /// **FALSE FOREVER NOW, AND KEPT RATHER THAN THREADED OUT.** Hamlet does not
    /// ask the radio to send its spectrum any more, so it cannot be refused one:
    /// `AskForTheSpectrumAsync`, the five second wait it needed before writing,
    /// and the undo that put the setting back all went with the write. The
    /// readiness check still takes the flag, and passing a false it can rely on
    /// is cheaper to read than a signature change that would have to be undone
    /// the day the write is ruled back in (HM-DEC-062, HM-DEC-092).
    /// </remarks>
    private const bool ScopeWriteRefused = false;

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
            DataContext = new RigDiagnosticsViewModel(
                _rigMonitor, RigState, (_rig as Ic7300Rig)?.Link),
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
    internal void ApplyRigState(RigState state)
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

        // **THE SWEPT FREQUENCY CORRECTS THE DISPLAY** (HM-DEC-109). A broadcast
        // missed at startup used to leave the band on screen wrong until
        // somebody next turned the dial, and the band scopes what RBN is
        // filtered to and what the skimmer watch listens for. Sweeping the model
        // and leaving the screen alone would fix the half nobody looks at.
        //
        // **THE GUARD IS THE POINT, AND IT HAD A HOLE THE WIDTH OF THE WRITE.**
        // A reading that arrived while the operator's own tune was still queued
        // would drag the dial back to where the radio has not got to yet, which
        // is the app taking the knob out of his hand (§0.2.1).
        //
        // `_rigSendPending` covered the queue and stopped at the send: it is
        // cleared before the write is awaited, so from that instant until a
        // reading taken *after* the write comes back, the model still holds the
        // frequency the radio was on before. This block then applied it, and the
        // display snapped back to where the operator had just left. He watched it
        // do that on every tune from the app, and hold for about thirty seconds,
        // which is the session sweep that used to be the frequency's only
        // refresh (ad93fb4).
        //
        // **SO THE TEST IS THE READING'S OWN AGE AGAINST THE WRITE, AND IT IS
        // BOUNDED BY THE WRITE RATHER THAN BY A TIMER.** A reading taken before
        // Hamlet moved the dial cannot say where the dial is now, whatever else
        // is true. One taken after it may, including when it disagrees, because
        // the radio is always right about its own frequency (§0.0).
        if (!_rigSendPending
            && !_writeInFlight
            && !_updatingFromRig
            && state[RigField.Frequency] is { IsKnown: true, Number: { } swept }
            && (long)swept != FrequencyHz
            && IsAfterOurOwnTune(state[RigField.Frequency]))
        {
            // **A FREQUENCY THAT GOES BACKWARDS IS THE SIGNATURE OF THIS BUG.**
            // Returning to a value the display held moments ago, right after a
            // tune, is not an ordinary observation, and nothing anywhere said so
            // while it was happening on every click for two builds.
            if (DialGuard.WouldGoBackwards((long)swept, _tunedFromHz, _tunedToHz))
            {
                AppEvents.FrequencyWentBackwards(
                    _telemetry, (long)swept, _tunedToHz ?? 0,
                    state[RigField.Frequency].Source,
                    _tunedAtUtc is { } when
                        ? (DateTime.UtcNow - when).TotalSeconds
                        : null);
            }

            ApplyRigFrequency((long)swept);
        }
        RigModeText = state[RigField.Mode] is { IsKnown: true } mode ? mode.Text : "";
        RigFilterText = state[RigField.FilterSelection] is { IsKnown: true } filter
            ? filter.Text
            : "";

        SMeterLevel = state.SMeterFraction;

        FilterBandwidthText = state[RigField.FilterBandwidth] is { IsKnown: true } width
            ? width.Text
            : "";

        // **THE RECEIVER'S FRONT END, WHERE HE IS ALREADY LOOKING** (HM-DEC-091).
        // Hamlet read all three of these from the radio on the evening it could
        // not hear anything and showed him none of them: he found `Overflow:
        // overloading` in a text file the next day. A setting standing between
        // the operator and a contact, which the app already knows about and does
        // not mention, is squarely in the way of what this application is for.
        var overflow = state[RigField.Overflow];
        var preamp = state[RigField.Preamp];
        var attenuator = state[RigField.Attenuator];

        FrontEndIsOverloading = overflow is { IsKnown: true, Number: 1 };

        // **THE RADIO'S OWN WORD FOR BOTH OF THESE IS "off"**, so the chip read
        // `off · off` and said that two things were off without saying which two.
        // A reading nobody can interpret is the same failure as a reading nobody
        // can find, so the label is composed here from the number rather than
        // taken from the radio's text.
        PreampText = PreampLabel(preamp.IsKnown ? (int?)preamp.Number : null);
        AttenuatorText = AttenuatorLabel(
            attenuator.IsKnown ? (int?)attenuator.Number : null);

        FrontEndText = FrontEndTextFor(
            FrontEndIsOverloading, PreampText, AttenuatorText);

        // The preamp is the control to reach for first, and the attenuator only
        // once it is already off: advice about a knob that is already in the
        // right position is noise.
        PreampIsOn = preamp is { IsKnown: true } && preamp.Number is 1 or 2;

        // **THE THREE THE RULING NAMED, AND ONLY WHEN THEY ARE IN THE WAY.** The
        // filter is mentioned on a measurement rather than on a width: a
        // competing station the survey actually found is a fact, and asserting
        // that some width is too wide for a signal Hamlet has not measured would
        // be a judgement nobody has ruled (§0.0).
        ReceiveObstructionText = string.Join(
            " ",
            ReceiveObstructions.For(
                state,
                state.Mode is { } inMode && CivValues.IsCw(inMode),
                _decoder?.Report.Competitor is not null)
                .Select(one => one.Says));

        OnPropertyChanged(nameof(RigState));
        OnPropertyChanged(nameof(TerminalSummary));

        // The link check and the age caption both read the clock, so they are
        // re-asked on the same beat as everything else here (HM-DEC-078).
        OnPropertyChanged(nameof(LinkCheckLine));
        OnPropertyChanged(nameof(LinkCheckDetail));
        OnPropertyChanged(nameof(HasLinkCheck));
        OnPropertyChanged(nameof(FrequencyAgeNote));
        OnPropertyChanged(nameof(HasFrequencyAgeNote));

        // A waterfall that sat empty without saying why would be the app looking
        // broken while the answer was four menu screens away (HM-DEC-062). The
        // sweep count goes in because settings that read as on and a waterfall
        // that stays blank is the case somebody actually sits and stares at
        // (HM-DEC-067).
        var scope = ScopeReadiness.Check(
            _rig?.Capabilities, state, _rigSpectrum?.SweepCount ?? -1,
            (_rig as Ic7300Rig)?.Link, ScopeWriteRefused);

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
        OnPropertyChanged(nameof(ScopeCounts));
        OnPropertyChanged(nameof(ScopeIsFlowing));

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

        // **A COUNT WRITTEN BESIDE A RECORDING IS READ AS BEING ABOUT THE
        // RECORDING** (HM-DEC-091). The decoder's counters run from here until
        // listening stops, so a capture taken seven hours in carried a character
        // count earned hours earlier on another band. This keeps a short history
        // of them against the audio clock, so a figure can be quoted for the
        // thirty seconds in the file rather than for the evening.
        _decoderStartedUtc = DateTime.UtcNow;
        _counters = new CwCounterTrail(
            (long)_audioInput.SampleRate * AudioTap.SecondsKept * 2);

        // **THE INSTRUMENT FOR THE FAULT NOBODY HAS FOUND YET** (HM-DEC-091).
        // The operator hears stations Hamlet does not, and finds out the next
        // morning from a roster. This says so while he is sitting at the radio,
        // so he can turn the gain, change the filter or retune and watch the
        // number answer. It reads the same tap the decoder reads and shares
        // nothing else with it.
        _keyingMeter = new CwKeyingMeter();
        _meterWork = null;
        _meterLastUtc = DateTime.MinValue;
        PublishKeying(KeyingReading.None);
        // **THE TWO PASSES BOTH REACH THE SCREEN NOW, AND THEY ARE NOT
        // RIVALS** (HM-DEC-096). The leading edge answers while somebody is
        // still sending and is never final; the settled pass runs a few seconds
        // behind with the whole stretch in hand and is what the transcript
        // keeps. Wiring only the first is the entire two-stage design being
        // invisible, and showing a provisional reading as though it were final
        // is §0.0 broken by omission.
        // **THE LEADING EDGE IS REPLACED, NOT APPENDED TO.** The decoder decides
        // late on purpose, so the tail of what it has read can change when the
        // next character arrives, and the terminal shows that rather than
        // stacking up every version of it.
        _decoder.LeadingEdge += Transcript.OfferEdge;
        _decoder.CharacterSettled += Transcript.Settle;

        // WHEN SOMETHING LAST CAME THROUGH, which is what the quiet offer waits
        // on (HM-DEC-084). Set here rather than polled, so an empty terminal is
        // measured from the last real character rather than from a timer.
        _decoder.CharacterDecoded += _ =>
        {
            _lastDecodeUtc = DateTime.UtcNow;
            _lastCharacterUtc = DateTime.UtcNow;
        };

        // **NOT SEEDED.** A decoder that has just started listening has read
        // nothing, and the mode-follow guard must not read that as somebody
        // working Morse.
        _lastCharacterUtc = DateTime.MinValue;
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
            _decoder.LeadingEdge -= Transcript.OfferEdge;
            _decoder.CharacterSettled -= Transcript.Settle;
            _decoder.Listen(null);
            _decoder = null;
        }

        // The trail belongs to one decoder. A new one starts at nought samples
        // with nought counted, and a history carried across the seam would make
        // a window straddle two of them.
        _counters = null;
        _decoderStartedUtc = null;
        _keyingMeter = null;
        _meterWork = null;
        PublishKeying(KeyingReading.None);

        _audioInput?.Stop();
        _audioInput?.Dispose();
        _audioInput = null;

        IsDecoding = false;
        DetectedWpm = 0;
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

        // **THE RADIO SAYS WHETHER IT IS TRANSMITTING, AND THE DECODER IS TOLD**
        // (HM-DEC-091). Hamlet has read `1C 00` for months, the diagnostics
        // screen has shown it correctly for months, and nothing consumed it: the
        // terminal decoded the operator's own sending and showed it as somebody
        // else's, which is HM-DEC-009 in the one place nobody guarded.
        //
        // Unknown leaves decoding running. A link that has gone quiet must not
        // silence the band, because a screen that stops without a reason reads as
        // an empty band (§0.0).
        var keyed = RigState[RigField.TransmitStatus];

        _decoder.RadioIsTransmitting(
            keyed.IsKnown ? keyed.Number == 1 : null, DateTime.UtcNow);

        DecodingIsSuspended = _decoder.DecodingSuspended;
        ListeningAfresh = _decoder.ListeningAfresh;

        // **THE SETTLED-PASS READOUTS WENT WITH THE SETTLED PASS.** The tip
        // mark, the ceiling note, the handover note and the revisions count all
        // described a second pass overtaking a first, and there is one pass now.
        // A readout describing a decoder that no longer exists is the defect
        // removing it was meant to end (HM-DEC-091).
        //
        // What survives of them is the one fact that is still true and still
        // useful: the decoder will not name a speed while its window straddles
        // two stations, and the panel says so rather than going quietly blank.
        SpeedIsReacquiring = _decoder.SpeedIsReacquiring;
        TipText = Transcript.TipText;

        // WHAT IS ARRIVING, WHETHER OR NOT ANYTHING DECODES (HM-DEC-088). A
        // strong signal that will not resolve and an empty band used to produce
        // the same screen, and they are different problems.
        DecodeReport = _decoder.Report;

        // **THE LOCK'S STATE, ON THE SAME TICK AS EVERYTHING ELSE.** It reads
        // the decoder rather than remembering what was pressed, so a lock that
        // refused to engage cannot leave the panel claiming one is held.
        // The advisory is recomputed from several of these on every tick, and
        // the keying meter's block follows it (task 5): one voice at a time.
        OnPropertyChanged(nameof(AdvisoryNote));
        OnPropertyChanged(nameof(ShowKeyingMeter));

        PitchLockText = _decoder.IsLocked
            ? $"The decoder is holding {_decoder.LockedToneHz:0.0} hertz and is "
              + "not following the tracker. Press the lock again to let it follow."
            : "";

        RunKeyingMeter();

        // Sampled here, on the same tick as the readouts, so the two ends of any
        // window a capture asks about are each accurate to one tick.
        _counters?.Note(new CwCounterSample(
            _decoder.Tap.SamplesSeen,
            DecodeReport.ElementsSeen,
            DecodeReport.ElementsResolved,
            DecodeReport.CharactersEmitted,
            DecodeReport.CharactersUnsure));

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
        OnPropertyChanged(nameof(KeyingAdviceIsUseful));
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
    /// True once the operator has tuned with the wheel (HM-DEC-141).
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

    /// <summary>
    /// Whether the keying sweep's advice about the antenna is worth showing.
    /// </summary>
    /// <remarks>
    /// <para>**TWO PANELS ASSERTED OPPOSITE THINGS ABOUT THE SAME BAND AND THE
    /// ADVICE SENT HIM TO THE RADIO FOR A DECODER CONDITION.** The line above
    /// said a clear tone was present; this block said no keying here, fifty hertz
    /// away; and its paragraph told him the signal was being lost between the
    /// antenna and Hamlet and to try the gain, the filter and the tuning. On the
    /// evening of 2026-08-25 he went and did that, and nothing was wrong with the
    /// radio.</para>
    /// <para>**THE ADVICE IS ONLY EVER TRUE WHERE NOTHING FOUND A TONE.** Where
    /// the decoder has one, the sweep disagreeing with it is a fault in the
    /// sweep — measured on this tree's own corpus, its calibration sits inside an
    /// overlap rather than in a gap — and telling him to go and turn knobs is
    /// acting on the wrong one of two instruments (§0.0).</para>
    /// <para>The word and the numbers stay where the sweep is shown at all; what
    /// retires is the instruction to go to the radio.</para>
    /// </remarks>
    public bool KeyingAdviceIsUseful => !DecodeReport.HasTone;

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
    private async Task CaptureAudioAsync()
    {
        // **ONE SNAPSHOT, TAKEN AT THE PRESS** (HM-DEC-091, HM-DEC-111). The
        // sidecar used to read the live report again after awaiting the radio,
        // and the decode poll runs four times a second in between: the terminal
        // said a tone at 500 hertz and the file written moments later said 400,
        // from **the same property at two instants**, with nothing on the sheet
        // saying which instant either belonged to.
        var pressed = DecodeReport;

        // **THE PRESS NOW ASSERTS A STATION, NOT ONLY A CASE** (Tim's ruling of
        // 2026-08-26). The operator saying he can hear one is evidence that one
        // is there, and it is the only evidence in this system that has never
        // been wrong. Six families of admission statistic have now been measured
        // and none of them can find a station he can hear, so waiting for the
        // survey to agree with him is waiting for something that does not
        // happen.
        //
        // He supplies the keying and Hamlet supplies the frequency: the loudest
        // bin in the band at this instant, held until he clears it or the dial
        // moves. HM-DEC-095 is untouched — it forbids Hamlet choosing a note by
        // loudness on its own judgement, and this is his judgement.
        //
        // **THE AUTOMATIC PATH IS NOT CHANGED BY THIS.** An empty band still
        // produces nothing when nobody has pressed anything.
        // **OFF THE UI THREAD, BECAUSE IT SWEEPS THE BAND.** The keying sweep
        // reads twenty-five pitches across half a minute of audio, which is the
        // same work the keying meter already does on a background task rather
        // than in front of the operator.
        var decoding = _decoder;

        var asserted = decoding is null
            ? double.NaN
            : await Task.Run(decoding.AssertStation);

        // **ASK THE RADIO WHERE IT IS BEFORE WRITING DOWN WHERE IT WAS**
        // (HM-DEC-107 phase 6 of the UI order). The frequency is never polled,
        // because the radio broadcasts a change and asking as well would spend
        // bus traffic on a fact already in hand (HM-DEC-050). What that ruling
        // provides for instead is the on-demand read, and this is exactly the
        // moment for one: a sidecar is evidence somebody will reason from
        // months later, and a broadcast missed at startup would otherwise put a
        // frequency in it that the radio was never on.
        if (_rigMonitor is not null)
        {
            await _rigMonitor.RefreshAsync(RigField.Frequency);
        }

        var tap = _decoder?.Tap;
        var audio = tap?.Snapshot();

        if (tap is null || audio is null)
        {
            StatusText =
                "There is no audio to keep just now, so the case is on the roster "
                + "with nothing behind it.";

            MarkCase(wav: "", refusal: "no audio was arriving");
            AppEvents.AudioCaptured(_telemetry, 0, CapturedHz, worked: false);
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
                + "the same file over again, and the case is on the roster saying "
                + "so.";

            // **THE GUARD IS NOT WEAKENED AND ITS REFUSAL STOPS BEING INVISIBLE**
            // (HM-DEC-090). It exists because three presses inside seventy seconds
            // once produced byte-identical files that were reasoned about as three
            // pieces of evidence. What changes here is only that the refusal
            // becomes a row: the case happened, the operator heard something, and
            // a denominator that quietly dropped it would flatter the score.
            MarkCase(wav: "", refusal: "no new audio since the last one");
            AppEvents.AudioCaptured(_telemetry, 0, CapturedHz, worked: false);
            return;
        }

        try
        {
            var folder = CaptureFolder;
            Directory.CreateDirectory(folder);

            var stamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HHmmss");
            var wav = Path.Combine(folder, $"cw-{stamp}.wav");

            WavAudio.Write(wav, audio);
            File.WriteAllText(
                Path.Combine(folder, $"cw-{stamp}.txt"),
                CaptureNotes(audio, seen, pressed));

            _lastCaptureSamples = seen;

            // **AND SAY WHAT THE PRESS DID TO THE DECODER**, because it now does
            // two things and the second one is the one he pressed it for. The
            // sentence says the pitch is the loudest bin rather than a station
            // Hamlet found, so nothing here implies more than happened (§0.0).
            StatusText = double.IsNaN(asserted)
                ? $"Kept the last {audio.Duration.TotalSeconds:0} seconds of what "
                  + "the decoder heard. Nothing has been surveyed yet, so there "
                  + "was no loudest bin to point at; give it a few seconds and "
                  + "press again."
                : $"Kept the last {audio.Duration.TotalSeconds:0} seconds, and "
                  + $"took your word for it: reading at {asserted:0} Hz, the "
                  + "loudest thing in the band just now. Hamlet did not find "
                  + "keying there, you did. Press Hold this pitch to let go "
                  + "again, or move the dial.";

            MarkCase(
                wav: Path.GetFileName(wav),
                refusal: "",
                inRecording: _counters?.Over(seen, audio.Samples.Length));

            AppEvents.AudioCaptured(
                _telemetry, audio.Duration.TotalSeconds, CapturedHz, worked: true);
        }
        catch (Exception)
        {
            // A capture that cannot be written loses a recording and nothing
            // else (§8).
            StatusText = "Hamlet could not write the recording.";
            AppEvents.AudioCaptured(_telemetry, 0, CapturedHz, worked: false);
        }
    }
    /// <summary>How much audio had arrived when the last capture was written.</summary>
    private long _lastCaptureSamples = -1;

    /// <summary>Everything worth knowing about a capture, beside it.</summary>
    /// <param name="audio">What was written.</param>
    /// <param name="samplesSeen">How much audio has ever arrived.</param>
    /// <param name="report">
    /// What the decoder was reporting at the moment of the press, taken once so
    /// every figure on the sheet belongs to one instant (HM-DEC-091).
    /// </param>
    private string CaptureNotes(
        MonoAudio audio, long samplesSeen, CwDecodeReport report)
    {
        var state = RigState;

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
            // That fix reached only the branch where the radio had been read;
            // the other one still fell back to the button, which is how
            // 14.028 MHz came to be labelled 40 m afterwards (HM-DEC-091).
            $"band       {CapturedBand()}",

            // **THE SETTING SAYS ONE THING AND THE CABLE SAYS ANOTHER**
            // (HM-DEC-091). `SHACK_FACTS.md` records CI-V Transceive measured
            // off, five and a half thousand frames in a minute with none of them
            // the radio volunteering anything, and captures the same week carried
            // the setting read back as on. A setting's name and a link's observed
            // behaviour are different facts and only the second is evidence, so a
            // capture now carries both and nobody has to reason from one alone.
            // Nothing here writes to the radio or advises anybody to change it.
            $"broadcast  {BroadcastDuringCapture(audio)}",
            "",
            // THE RECORDING'S OWN PEAK, not the meter's last fifth of a second
            // (HM-DEC-094). Those differed by eight decibels on a file that was
            // nearly clipping while the sidecar said there was headroom.
            $"inputPeak  {AudioTap.PeakOf(audio):0.0} dBFS  (over the whole recording)",
            $"meterPeak  {report.Level.PeakDb:0.0} dBFS  (the moment it was kept)",
            $"inputFloor {report.Level.FloorDb:0.0} dBFS",
            $"clipping   {report.Clipping}",
            // **TWO PITCHES ON ONE SHEET, AND THEY ARE NOT THE SAME
            // MEASUREMENT** (HM-DEC-091). This one and the `keying` line below
            // differ by up to 250 Hz on the same file, which reads as two
            // instruments contradicting each other and is not: this is the bin
            // the decoder is following right now, moved to continuously from
            // wherever it started and confirmed by two agreeing surveys
            // (HM-DEC-095), and the other is a fresh sweep of the whole range in
            // 25 Hz steps over the last six seconds that shares nothing with the
            // decoder. Where they disagree, the decoder is reading one pitch
            // while something louder or better keyed sits at another, and that is
            // worth knowing rather than worth hiding.
            // **A BANK CENTRE IS NOT A MEASUREMENT AND THIS SHEET USED TO PRINT
            // IT AS ONE** (§0.0, HM-DEC-009). Until the survey admits a keying
            // candidate the tracker answers with the middle of whatever bank it
            // is pointed at, and that number went out here to a tenth of nothing:
            // measured across the corpus it read 300 Hz on a station at 499.8 and
            // 825 Hz on a recording holding nothing at all. The pitch is now
            // written to a tenth of a hertz because that is what it is measured
            // to, and an unmeasured one says which bank it came from instead.
            $"toneHz     {ToneForTheRecord(report)}",
            // **THIS FIELD WAS CALLED `snrDb` AND IT IS NOT ONE** (HM-DEC-091:
            // one source, and it says which). It is a held peak of how far the
            // tracked bin stood above the noise beside it, rising at once and
            // falling about a decibel a second, which is what HM-DEC-090 built it
            // to be so that a station keying for a second and a half inside
            // thirty would not average away to nothing. What it is not is a
            // figure about this recording, and read as one it is badly wrong:
            // measured across this repository's captures it rates
            // `cw-2026-08-20-014854` at 41.7 and `cw-2026-08-20-014935` at 38.4,
            // neither of which holds keying at any pitch, above
            // `cw-2026-08-17-013347` at 34.7, which is the one this decoder reads
            // a callsign out of. **A work order was written from that reading.**
            //
            // The number is not deleted and not changed, because it measures
            // something real and something else was built on it. It says what it
            // measures instead, and the two figures on this sheet that do
            // separate a station from an empty band sit beside it: the
            // `inputPeak` and `inputFloor` pair the terminal shows, and the swing
            // on the `keying` line.
            $"tonePeak   {(double.IsNaN(report.SnrDb) ? "unread" : report.SnrDb.ToString("0.0"))}"
                + "  (the highest the tracked tone ever stood above the noise "
                + "beside it, held and decaying; not a figure about this "
                + "recording)",

            // **THE FIGURE FOR THIS RECORDING, WHICH IS WHAT EVERY NUMBER ON THIS
            // SHEET IS READ AS BEING** (HM-DEC-091). Derived, by taking the
            // decoder's own counters at the two ends of the audio in this file.
            // A count that cannot be derived says so and does not print a number.
            $"inThis     {InThisRecording(audio, samplesSeen)}",

            // **AND THE RUNNING TOTALS, WHICH NOW SAY WHAT THEY COVER.** They
            // were always cumulative from the moment listening started; what they
            // never did was admit it. A capture written seven hours into an
            // evening carried a character count earned hours earlier on another
            // band, and nothing beside it said the number was not about the
            // thirty seconds it sat next to.
            $"elements   {report.ElementsSeen} seen, {report.ElementsResolved} resolved"
                + $"  ({CountsCover()})",
            $"characters {report.CharactersEmitted} emitted, "
                + $"{report.CharactersUnsure} unsure  ({CountsCover()})",

            // **THE SPEED THE DECODER WAS TRACKING**, which is the first thing
            // anybody asks of a recording Hamlet could not read. Unread stays
            // unread: a fixture labelled with a speed nobody measured is worse
            // than one labelled with nothing (§0.0, HM-DEC-090).
            // **`not tracking` AND `the number was withdrawn` ARE DIFFERENT
            // FACTS AND THIS FIELD SAID THE FIRST FOR BOTH** (HM-DEC-091). The
            // panel showed 29 words a minute and the file written moments later
            // said the decoder was not tracking, which reads as two instruments
            // disagreeing and is not: the guard on the speed had withdrawn the
            // number between the two, and nothing on the sheet could say so.
            //
            // The reading is taken here, at the press, from the same decoder the
            // rest of this sheet comes from, rather than from the polled snapshot
            // the header happens to be holding. And where there is no number it
            // says which of the guard's conditions was not met, because that is
            // the difference between nothing being on the air and a station being
            // heard whose speed had not yet been proved.
            $"decoderWpm {SpeedForTheRecord()}",

            // **THE SIDECAR RECORDED COUNTS AND NEVER A CHARACTER OF TEXT**, so
            // nothing beside a kept recording said what Hamlet had made of it.
            // The whole transcript goes here rather than the roster's tail,
            // because a file read by a person has no one-line constraint.
            //
            // It is called `text` and not `read` deliberately: `read` is the name
            // of the roster's own column, which is the operator's verdict and is
            // never written by Hamlet. Two fields one letter apart, one a machine's
            // output and one a person's judgement, is a confusion waiting for the
            // evening somebody scores thirty of them.
            $"text       {CwCaseRoster.Readable(Transcript.PlainText)}",

            // **AND THE TRANSCRIPT HAS THE SAME SHAPE OF PROBLEM AS THE COUNTS**,
            // so it gets the same treatment. It is everything read since
            // listening started, not what was read from this recording, and a
            // reader who takes it for the second has been misled by the layout.
            $"textCovers everything read {CountsCover()}",

            // **AND EVERY CHARACTER'S OWN EVIDENCE BESIDE IT, WHICH NOTHING ON
            // THIS SHEET HAS EVER CARRIED.** `reading` gives the window's
            // likelihood ratio, which is one number for everything read out of
            // that window, so a letter lifted out of a clean fade and a letter
            // assembled from the gaps between two other stations arrive here
            // looking identical. The per-character figure is measured over that
            // character's own marks against the key having been up throughout
            // them, so a wrong decode now comes with the evidence that produced
            // it and can be argued about with numbers (§0.0.1, HM-DEC-007).
            //
            // Large and positive is a character with a signal behind it. Near
            // zero is one that all-key-up explains just as well. `unmeasured` is
            // a pass that does not compute it, which is not the same as nought.
            $"spanLlr    {SpanRatiosForTheRecord()}",

            // **WHETHER SOMEBODY ELSE WAS KEYING IN THE SAME PASSBAND**, which
            // the survey has always known and no sheet has ever carried. Two
            // stations inside one filter arrive in one envelope, and amplitude is
            // what the decoder measures, so a recording that reads badly with a
            // competitor in it and a recording that reads badly on its own are
            // different faults that have looked identical on every sheet written
            // so far.
            //
            // **`none found` IS NOT `THE FREQUENCY WAS CLEAR`** (HM-DEC-009). The
            // survey wants three seconds and eight clean marks before it admits
            // anything, so a station that had just started is absent here and was
            // present on the air.
            $"competing  {CompetitorForTheRecord(report)}",

            // **WHETHER HAMLET COULD HEAR KEYING AT ALL, BESIDE WHAT IT READ**
            // (HM-DEC-091). The two answer different questions and only one of
            // them has ever been on a sheet. A capture where the operator heard a
            // station and this line says no keying is the signal going missing
            // before the decoder saw it, which is a fault nothing else here can
            // point at. Measured by sweeping this recording's own pitches and
            // sharing nothing with the decoder.
            // **HOW GOOD THE CLOCK FIT WAS, WHICH HAS NEVER BEEN ON A SHEET.**
            // A speed is one number out of a fit, and a fit that is not a fist
            // produces one just as readily as a fit that is. These are the three
            // figures that tell them apart, and every one of them is measured
            // rather than judged: nothing in the decoder reads them (§0.0.1).
            // **WHAT THE WORKING DECODER DID**, and it says so rather than
            // carrying a fitted dah-to-dit ratio that belongs to a decoder whose
            // output nobody sees (HM-DEC-091).
            $"reading    {FitLine()}",

            $"keying     {KeyingLine(_keyingReading)}"
                + "  (an independent sweep of 400 to 1200 Hz in 25 Hz steps over "
                + "the last six seconds, sharing nothing with the decoder)",

            // **THE ONE NUMBER THAT SORTED THE EVENING OF 2026-08-25 AND WAS
            // NOWHERE ON THIS SHEET.** Thirteen captures, one band, one input
            // level, the tone locked within a few hertz on twelve of them; sorted
            // by how much of the recording had the key down, the outcomes sort
            // themselves. Ten between 38 and 47 per cent read back with nought to
            // eight characters unsure. One at 24 per cent buried its real content
            // in forty-eight characters of noise. One at 18 per cent gave eight
            // seconds of station and twenty-two of invented text.
            //
            // **IT IS MEASURED OVER THE AUDIO IN THIS FILE**, at the pitch the
            // decoder was following, which is what every other figure on this
            // sheet is read as being and what the `keying` line above is not.
            // **And it is not written at all where the pitch was never measured**
            // (§0.0): the duty at the middle of whatever bank the decoder happens
            // to be pointed at is a fact about a bank rather than about a station,
            // and this sheet has printed one of those before.
            $"duty       {DutyForTheRecord(audio, report)}",
            "",
        };

        // WHAT THE DECODER HAS DONE SINCE THE LAST CAPTURE, beside the totals.
        // The totals are cumulative over the whole session, so two captures
        // showing the same ones mean nothing was decoded in between, and a reader
        // should not have to work that out by subtraction.
        // **THE ONE FIELD ON THE OLD SHEET THAT WAS ABOUT THE CAPTURE**, and
        // it is why it stays: it read `0 characters` on the press that mattered,
        // which was the truth nobody read because three bare totals beside it
        // looked more like measurements. It now says which interval it covers
        // too, because on the first capture of a session there is no previous one
        // and the difference is the whole session.
        lines.Add(
            $"sinceLast  {report.CharactersEmitted - _lastCaptureCharacters} characters, "
            + $"{report.ElementsSeen - _lastCaptureElements} elements  "
            + (_hasPreviousCapture
                ? "(since the previous capture)"
                : $"({CountsCover()}; this is the first capture of the session)"));

        lines.Add("");

        _lastCaptureCharacters = report.CharactersEmitted;
        _lastCaptureElements = report.ElementsSeen;
        _hasPreviousCapture = true;

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
    /// <summary>
    /// The frequency a capture is labelled with, from one source.
    /// </summary>
    /// <remarks>
    /// **ONE FILE HAD TWO PATHS TO ONE FACT AND ONLY ONE WAS RIGHT** (HM-DEC-111).
    /// The sidecar read the radio and the telemetry event beside it was handed
    /// `FrequencyHz`, which is Hamlet's own idea of where the dial is, so a
    /// capture could carry `7025400` in one and `14028000` in the other. Fixing
    /// the sidecar's wording without fixing this would have left the same defect
    /// with better prose on one of its two halves.
    /// </remarks>
    private long CapturedHz
        => RigState[RigField.Frequency] is { IsKnown: true, Number: { } hz }
            ? (long)hz
            : FrequencyHz;

    private string CapturedFrequency()
    {
        var read = RigState[RigField.Frequency];

        if (read is not { IsKnown: true, Number: not null })
        {
            return $"{CapturedHz} Hz  (Hamlet's own, the radio was not read)";
        }

        // **A PROVENANCE LABEL CARRIES ITS AGE** (HM-DEC-111). That ruling came
        // from this very line: it wrote "read from the radio" beside a value that
        // had been read sixty seconds and two tunings earlier, and the label
        // asserted a freshness it did not have. The frequency is polled at the
        // live rate now (HM-DEC-138) and this command asks for it again before
        // writing, so the number should be a fraction of a second old — **and
        // saying so is what makes that checkable months later** rather than
        // something a reader has to take on trust.
        var age = read.Age(DateTime.UtcNow);

        var when = age is not { } old
            ? "read from the radio, age unknown"
            : old < TimeSpan.FromSeconds(2)
                ? "read from the radio a moment ago"
                : $"read from the radio {old.TotalSeconds:0} seconds before this capture";

        return $"{CapturedHz} Hz  ({when})";
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
        // **BOTH BRANCHES DERIVE FROM THE FREQUENCY THIS FILE ALSO PRINTS**
        // (HM-DEC-091). The first version of this fixed the read case and left
        // the unread one falling back to the band button, so a header could still
        // carry a frequency from one source and a band from another: 14.028 MHz
        // labelled 40 m, which is the original defect surviving in the branch
        // nobody looked at. The band is a fact about the frequency, and there is
        // exactly one frequency on this sheet.
        var read = RigState[RigField.Frequency];
        var name = HfBands.BandFor(CapturedHz)?.Name
                   ?? "outside every band Hamlet knows";

        return read is { IsKnown: true, Number: not null }
            ? $"{name}  (from the frequency the radio reported)"
            : $"{name}  (from Hamlet's own frequency, the radio was not read)";
    }

    /// <summary>Whether any capture has already been written this session.</summary>
    private bool _hasPreviousCapture;

    /// <summary>How often the keying meter looks.</summary>
    /// <remarks>
    /// Once a second, which is fast enough to feel like an answer when the
    /// operator turns a knob and slow enough that one update's work, about
    /// seventy milliseconds of it, is a small share of a core.
    /// </remarks>
    private static readonly TimeSpan KeyingMeterEvery = TimeSpan.FromSeconds(1);

    /// <summary>What the keying meter is willing to say, in one word.</summary>
    [ObservableProperty]
    private string _keyingWord = "";

    /// <summary>The measurements behind that word.</summary>
    [ObservableProperty]
    private string _keyingDetail = "";

    /// <summary>Whether the meter is holding a verdict through a quiet stretch.</summary>
    [ObservableProperty]
    private bool _keyingIsHeld;

    /// <summary>Whether the meter can hear somebody keying.</summary>
    [ObservableProperty]
    private bool _keyingIsPresent;

    /// <summary>Whether the meter has settled on nothing being keyed.</summary>
    [ObservableProperty]
    private bool _keyingIsAbsent;

    /// <summary>Whether the meter has not seen enough to say (HM-DEC-091).</summary>
    [ObservableProperty]
    private bool _keyingIsUndecided;

    /// <summary>What the meter said, for the sidecar and the roster.</summary>
    private KeyingReading _keyingReading = KeyingReading.None;

    /// <summary>
    /// Let the keying meter look, off the interface thread (HM-DEC-091).
    /// </summary>
    /// <remarks>
    /// <para>**SEVENTY MILLISECONDS IS A VISIBLE HITCH ON THE INTERFACE THREAD**,
    /// and this runs every second for as long as the terminal is open, so it runs
    /// on a worker. **THE SEAM IS HERE AND NOWHERE ELSE**: the meter's own state
    /// is touched only by that worker, one at a time, and is read back here only
    /// after the task has completed, so the completion is what orders the two.
    /// </para>
    /// <para>A window is taken on this thread rather than inside the worker,
    /// because the tap's lock is held by the audio thread and a worker queueing
    /// behind it would drift out of step with the second it is meant to be
    /// keeping.</para>
    /// </remarks>
    private void RunKeyingMeter()
    {
        if (_meterWork is { IsCompleted: true })
        {
            if (_meterWork.IsCompletedSuccessfully)
            {
                PublishKeying(_meterWork.Result);
            }

            _meterWork = null;
        }

        if (_keyingMeter is null || _decoder is null || _meterWork is not null)
        {
            return;
        }

        if (DateTime.UtcNow - _meterLastUtc < KeyingMeterEvery)
        {
            return;
        }

        _meterLastUtc = DateTime.UtcNow;

        var meter = _keyingMeter;
        var window = _decoder.Tap.Tail(CwKeyingThresholds.Window);

        _meterWork = Task.Run(() => meter.Update(window));
    }

    /// <summary>Put a reading on the screen.</summary>
    /// <param name="reading">What the meter said.</param>
    /// <remarks>
    /// **THE NUMBERS ARE THE POINT AND THE WORD IS THE SUMMARY** (§0.0). He is
    /// going to chase a fault by turning a knob, and a figure that moves is worth
    /// more to him than a word that changes.
    /// </remarks>
    private void PublishKeying(KeyingReading reading)
    {
        _keyingReading = reading;

        KeyingWord = reading.Verdict switch
        {
            KeyingVerdict.Keying => "somebody is keying",
            KeyingVerdict.NoKeying => "no keying here",
            _ => "listening",
        };

        KeyingIsPresent = reading.Verdict == KeyingVerdict.Keying;
        KeyingIsAbsent = reading.Verdict == KeyingVerdict.NoKeying;
        KeyingIsUndecided = reading.Verdict == KeyingVerdict.Listening;
        KeyingIsHeld = reading.Held;

        KeyingDetail = KeyingDetailFor(reading);
    }

    /// <summary>What the meter measured, or why there is nothing to show.</summary>
    /// <param name="reading">The reading.</param>
    /// <returns>The detail line.</returns>
    /// <remarks>
    /// **A HELD VERDICT PRINTS THE VERDICT AND NO MEASUREMENTS.** While the meter
    /// is coasting through a gap between overs, the newest window it has is the
    /// gap, so the figures beside the word are measurements of silence wearing
    /// the station's label. On the evening of 2026-08-20 that put `9 ms key down`
    /// on screen and in a capture sidecar for a station the other recordings of
    /// the same operator measure at about ninety, and a work order was written
    /// from it. The verdict is the thing being held and it is still worth
    /// printing; the numbers are not, because they are not about what the word
    /// says.
    /// </remarks>
    private static string KeyingDetailFor(KeyingReading reading)
    {
        if (reading.Held)
        {
            return "holding through a quiet stretch, so there is nothing fresh "
                   + "to measure";
        }

        return reading.ToneHz <= 0
            ? "nothing measured yet"
            : $"{reading.ToneHz:0} Hz, key down {reading.MedianMs:0} ms, "
              + $"{reading.SwingDb:0} dB between quiet and loud, "
              + $"{reading.Runs} key-downs";
    }

    /// <summary>
    /// Each recent character with the evidence for its own span, for the sidecar.
    /// </summary>
    /// <remarks>
    /// <para>**A WRONG DECODE WITH ITS EVIDENCE ATTACHED IS A REGRESSION TEST**
    /// (HM-DEC-007). Until this line the sheet said what was read and how loud
    /// the window was, and those two together cannot tell a character read out
    /// of a signal from one the path assembled out of noise. This can, because it
    /// is measured over that character's marks and nothing else.</para>
    /// <para>**IT COVERS WHAT THE TRANSCRIPT'S RECENT TAIL COVERS AND SAYS SO.**
    /// The same caveat `textCovers` carries applies here, and the count is
    /// printed so a reader can see when the tail is shorter than the recording
    /// rather than inferring it.</para>
    /// <para>A word gap is not a character and carries no marks, so it is left
    /// out rather than printed as a nought somebody later reasons from.</para>
    /// </remarks>
    /// <returns>The characters and their span ratios, or why there are none.</returns>
    /// <summary>One likelihood figure, inside the range the record can carry.</summary>
    /// <param name="value">The figure.</param>
    /// <returns>The figure, or a marked bound where it ran past one.</returns>
    /// <remarks>
    /// **THE SHEET HAS PRINTED QUADRILLIONS AND NOBODY READS THE REST OF SUCH A
    /// SHEET.** The `6:27306879.3` family is a per-hop log-likelihood on a
    /// recording whose noise estimate went to nothing. A clamp is a statement
    /// about what the record can carry rather than about the measurement, so it
    /// says it clamped rather than quietly writing a smaller number.
    /// </remarks>
    internal static string Clamped(double value)
    {
        if (double.IsNaN(value))
        {
            return "unmeasured";
        }

        var widest = CwCharacter.WidestRecordedLlr;

        if (value > widest)
        {
            return $">{widest:0}";
        }

        return value < -widest ? $"<-{widest:0}" : $"{value:0.0}";
    }

    /// <summary>The margin's share of the span, for the sheet.</summary>
    /// <param name="value">The quotient, or NaN where there was none.</param>
    /// <returns>Three decimals, or why there is no figure.</returns>
    /// <remarks>
    /// Three decimals rather than one, because the whole distribution measured
    /// across this repository's captures sits between −0.05 and +0.12 at the
    /// tenth and ninetieth percentiles; at one decimal almost every character
    /// would print `0.0`.
    /// </remarks>
    internal static string Share(double value)
        => double.IsNaN(value) ? "unmeasured" : $"{value:0.000}";

    private string SpanRatiosForTheRecord()
        => SpanRatioLine(Transcript.Recent(), CountsCover());

    /// <summary>The span-ratio line itself, from the characters it describes.</summary>
    /// <param name="recent">The transcript's recent tail, word gaps included.</param>
    /// <param name="covers">What the tail covers, in the sheet's own words.</param>
    /// <returns>The characters and their span ratios, or why there are none.</returns>
    /// <remarks>
    /// Static and separate from the view model for the reason
    /// <see cref="KeyingLine"/> is: what a record a person reads months later
    /// says is worth a test of its own, and a test that has to build a window to
    /// read one line will not be written.
    /// </remarks>
    public static string SpanRatioLine(
        IReadOnlyList<CwCharacter> recent, string covers)
    {
        var measured = recent
            .Where(character => !character.IsWordGap
                && !double.IsNaN(character.SpanLogLikelihoodRatio))
            .ToArray();

        if (measured.Length == 0)
        {
            return recent.Count == 0
                ? "nothing read yet"
                : "unmeasured (no character carried a span ratio)";
        }

        var body = string.Join(
            " ",
            measured.Select(character =>
                $"{CwCaseRoster.Readable(character.Text)}"
                + $":{Clamped(character.SpanLogLikelihoodRatio)}"
                + $"/{Clamped(character.MarginLlr)}"
                // **AND THE QUOTIENT, BECAUSE THE CLAMP DESTROYS IT.** Both
                // figures above are clamped at a million before they are
                // printed, and on the captures where the raw margin runs to
                // hundreds of millions that is exactly what happens — so the
                // one form of this quantity that means the same thing on two
                // recordings cannot be recovered from the two beside it.
                + $"/{Share(character.MarginShareForRecord)}"));

        return $"{measured.Length} of the last {recent.Count} characters read, "
               + "each against the key having been up throughout its own span "
               + $"({covers})"
               + Environment.NewLine
               + "           " + body;
    }

    /// <summary>
    /// Somebody else keying in the same passband, for the sheet.
    /// </summary>
    /// <param name="report">The decoder's reading at the moment of the press.</param>
    /// <returns>What was found, or that nothing was.</returns>
    /// <remarks>
    /// **THE FACT AND ITS CONSEQUENCE, NOT THE ADVICE.** The sentence naming the
    /// filter and the passband controls belongs on the screen, where the operator
    /// is sitting in front of the radio; a file read the next morning wants the
    /// measurement (HM-DEC-148 is the ruling that a diagnosis in a text file is
    /// not help).
    /// </remarks>
    private static string CompetitorForTheRecord(CwDecodeReport report)
        => report.Competitor is { } other
            ? $"{Math.Abs(other.OffsetHz):0} Hz {other.Side} at "
              + $"{other.RelativeDb:+0.0;-0.0} dB relative "
              + $"({other.ToneHz:0} Hz)"
            : "none found (which is not the same as the frequency being clear)";

    /// <summary>
    /// How much of this recording had the key down, at the pitch the decoder was
    /// following, or why there is no figure.
    /// </summary>
    /// <param name="audio">The audio in this file, and nothing else.</param>
    /// <param name="report">What the decoder believed at the press.</param>
    /// <returns>The line for the sheet.</returns>
    /// <remarks>
    /// **A TENTH OF A PER CENT, BECAUSE THAT IS WHAT SEPARATES THE OUTCOMES.**
    /// The evening this was written for spread from 18 to 47 per cent across
    /// thirteen recordings and the boundary between readable and mostly invented
    /// sat around a quarter.
    /// </remarks>
    private static string DutyForTheRecord(MonoAudio audio, CwDecodeReport report)
    {
        if (!report.HasTone || !report.PitchWasMeasured)
        {
            return "not measured  (no pitch was measured, so there is no station "
                + "to measure the keying of)";
        }

        var profile = KeyingEnvelope.Measure(audio, report.ToneHz);

        return $"{profile.Duty * 100:0.0}%  (of the {audio.Duration.TotalSeconds:0.0} seconds in "
            + $"this file, the key was down at {report.ToneHz:0.0} Hz)";
    }

    /// <summary>The pitch the decoder was following, and whether it measured it.</summary>
    /// <param name="report">The decoder's reading at the moment of the press.</param>
    /// <returns>The pitch, or where the unmeasured number came from.</returns>
    /// <remarks>
    /// **UNREAD IS NOT NOUGHT AND A STARTING POINT IS NOT A READING.** The three
    /// states a reader has to be able to tell apart are a measured pitch, a bank
    /// centre nobody keyed at, and no tone at all.
    /// </remarks>
    private static string ToneForTheRecord(CwDecodeReport report)
    {
        if (!report.HasTone)
        {
            return "none";
        }

        // **THE OPERATOR'S OWN ASSERTION IS SAID AS ONE** (Tim's ruling of
        // 2026-08-26). Nothing here may read as Hamlet having found what a human
        // found: it says the pitch is the loudest bin, that a person supplied the
        // evidence there was a station on it, and that Hamlet measured no keying.
        if (report.PitchWasAsserted)
        {
            return $"{report.ToneHz:0.0} Hz  (NOT MEASURED: you said you could "
                + "hear a station, so this is the loudest bin in the band at that "
                + "moment. Hamlet did not find keying here)";
        }

        return report.PitchWasMeasured
            ? $"{report.ToneHz:0.0} Hz  (measured from the keying the survey "
              + "admitted, interpolated between bins)"
            : $"{report.ToneHz:0.0} Hz  (NOT MEASURED: the survey has admitted "
              + "no keying, so this is the middle of the bank the decoder is "
              + "pointed at rather than a station)";
    }

    /// <summary>
    /// The speed at the moment of the press, or why there is not one.
    /// </summary>
    /// <remarks>
    /// The guard on <see cref="CwDecoder.WordsPerMinute"/> withholds a number
    /// until a tone has been located, a character has resolved, the clock is not
    /// being re-acquired, and the settled pass has proved a dit. All four
    /// failures used to print the same three words (HM-DEC-091).
    /// </remarks>
    private string SpeedForTheRecord()
    {
        if (_decoder is not { } decoder)
        {
            return "not tracking (nothing is listening)";
        }

        if (decoder.WordsPerMinute is { } wpm)
        {
            return $"{wpm}";
        }

        var report = decoder.Report;
        var rolling = decoder.Reading.WordsPerMinute > 0
            ? $"the decoder's own best hypothesis was "
              + $"{decoder.Reading.WordsPerMinute:0} WPM"
            : "the decoder had no hypothesis worth naming";

        if (!report.HasTone)
        {
            return $"not tracking (no tone was located; {rolling})";
        }

        if (report.CharactersEmitted == 0)
        {
            return $"not proved (a tone but no resolved character; {rolling})";
        }

        return decoder.SpeedIsReacquiring
            ? $"withdrawn (the clock is being re-acquired; {rolling})"
            : $"not proved (the settled pass has no clock; {rolling})";
    }

    /// <summary>What the clock fit looked like, as one line.</summary>
    /// <remarks>
    /// **THE RATIO IS NOT A VERDICT.** A dah of four and a quarter dits is a real
    /// fist somebody sent on the air and this project read by hand (HM-DEC-144),
    /// so a number far from three is a thing to look at rather than a fault. What
    /// it sits beside is the separation, which is what HM-DEC-095 measured as the
    /// statistic that tells a fist from a smear.
    /// </remarks>
    private string FitLine()
    {
        if (_decoder is not { } decoder)
        {
            return "not fitted";
        }

        var reading = decoder.Reading;

        if (reading.WordsPerMinute <= 0)
        {
            return "nothing fitted yet";
        }

        // **THIS LINE USED TO QUOTE A DECODER NOBODY CAN SEE THE OUTPUT OF.** It
        // read `CwSpeedEstimator`, which fits a clock by clustering run lengths
        // and has decoded nothing since the decoder was replaced, so a sheet
        // reported a dah of 15.7 dits beside text produced by something else
        // entirely — and four evenings of captures were read as evidence about
        // the clock behind the words. They were not.
        //
        // **THE WORKING DECODER HAS NO FITTED RATIO TO REPORT**, and that is not
        // a gap: it never measures one. A dah is three dits in its model, the
        // speed is whichever hypothesis explained the audio best, and how well
        // that explanation did is the likelihood ratio against silence. Those are
        // the numbers behind the text and they are what this line carries now.
        // **A WINNER AT EITHER END OF THE SEARCH SAYS SO** (§0.0). A hypothesis
        // at the edge of a range wins by default rather than on evidence,
        // because there is nothing beyond it to lose to. On 2026-08-25 two
        // operators measured 30.9 and 30.8 words a minute and this line said 32
        // for both, which was the top of the grid, and nothing on the sheet
        // could tell a ceiling from a measurement.
        var atEdge =
            reading.WordsPerMinute >= CwProbabilisticDecoder.FastestWpm - 1e-9
                ? "  (AT THE TOP OF THE SEARCH: the sender may be faster than "
                  + "Hamlet can look)"
                : reading.WordsPerMinute <= CwProbabilisticDecoder.SlowestWpm + 1e-9
                    ? "  (AT THE BOTTOM OF THE SEARCH: the sender may be slower "
                      + "than Hamlet can look)"
                    : "";

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0:0} WPM won out of {1} to {2}, {3:0.0} better than silence per hop "
            + "against a gate of {4:0}{5}",
            reading.WordsPerMinute,
            CwProbabilisticDecoder.SlowestWpm,
            CwProbabilisticDecoder.FastestWpm,
            reading.LikelihoodRatio,
            CwProbabilisticDecoder.Gate,
            atEdge);
    }

    /// <summary>What the meter said, as one line for a record.</summary>
    /// <param name="reading">The reading.</param>
    /// <returns>The line.</returns>
    internal static string KeyingLine(KeyingReading reading)
    {
        var word = reading.Verdict switch
        {
            KeyingVerdict.Keying => "keying",
            KeyingVerdict.NoKeying => "no keying",
            _ => "listening",
        };

        if (reading.ToneHz <= 0)
        {
            return "not measured";
        }

        // **A HELD VERDICT CARRIES NO MEASUREMENTS INTO THE RECORD EITHER.** The
        // sidecar is the more dangerous of the two places, because a figure
        // written beside a recording is read months later as a fact about it.
        // **THE KEY-DOWN LENGTH PRINTED HERE USED TO BE ONE NOBODY COULD SEND**
        // (§0.0). It was the middle of every threshold crossing, and a threshold
        // is crossed by noise hundreds of times, so on a recording holding a real
        // station the chatter outnumbered the elements several to one and the
        // number landed among the chatter: four milliseconds beside an
        // adjudicated `VA3VRR`, three beside an adjudicated `N4L`. A dit at sixty
        // words a minute is twenty and sixty is faster than a hand sends, so
        // those were not measurements that had gone wrong. They were
        // measurements of something that is not Morse, printed where a reader
        // takes them for a fist.
        //
        // **THE VERDICT IS STILL CALIBRATED ON THE OLD FIGURE AND IS UNCHANGED
        // HERE.** Moving the verdict onto this one was built and measured: it
        // takes the meter from ten recordings right of twenty-three to seventeen,
        // and it costs the silence property, because in single six-second windows
        // `cw-2026-08-20-014854` scores above the bar and would then read as
        // keying. That trade is not this session's to make.
        if (reading.Held)
        {
            return word + " (held through a quiet stretch, so nothing was measured)";
        }

        var length = reading.ElementMedianMs > 0
            ? string.Format(
                CultureInfo.InvariantCulture,
                "{0:0} ms key down",
                reading.ElementMedianMs)
            : "no key-down was element length";

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} at {1:0} Hz, {2}, {3:0} dB swing, {4} key-downs",
            word,
            reading.ToneHz,
            length,
            reading.SwingDb,
            reading.Runs);
    }

    /// <summary>
    /// What the decoder made of the audio in this file, and nothing else
    /// (HM-DEC-091).
    /// </summary>
    /// <param name="audio">The recording being written.</param>
    /// <param name="samplesSeen">Where the audio clock stood when it was taken.</param>
    /// <returns>The figures, or why they could not be derived.</returns>
    private string InThisRecording(MonoAudio audio, long samplesSeen)
    {
        var window = _counters?.Over(samplesSeen, audio.Samples.Length);

        if (window is not { } inIt)
        {
            return "not derived (the decoder's own history does not reach back "
                   + $"over these {audio.Duration.TotalSeconds:0.0} seconds)";
        }

        return $"{inIt.CharactersEmitted} characters emitted, "
               + $"{inIt.CharactersUnsure} unsure, "
               + $"{inIt.ElementsSeen} elements seen, "
               + $"{inIt.ElementsResolved} resolved  "
               + $"(in the {audio.Duration.TotalSeconds:0.0} seconds of audio in this file)";
    }

    /// <summary>What the decoder's running totals cover, in words.</summary>
    /// <returns>The interval, as a clause.</returns>
    /// <remarks>
    /// **A BARE NUMBER BESIDE A RECORDING CLAIMS TO BE ABOUT THE RECORDING**
    /// (HM-DEC-091). These are honest fields once they say what they count, and
    /// the age is spoken rather than counted out, because a reader wants to know
    /// whether this is a fresh figure or one from hours ago (§0.7).
    /// </remarks>
    private string CountsCover()
    {
        if (_decoderStartedUtc is not { } started)
        {
            return "since the decoder started listening";
        }

        return "since the decoder started listening, "
               + SpokenAge(DateTime.UtcNow - started);
    }

    /// <summary>How long ago something was, said rather than counted.</summary>
    /// <param name="age">How long.</param>
    /// <returns>The phrase.</returns>
    private static string SpokenAge(TimeSpan age)
    {
        if (age < TimeSpan.FromMinutes(1))
        {
            return "less than a minute ago";
        }

        if (age < TimeSpan.FromMinutes(2))
        {
            return "about a minute ago";
        }

        if (age < TimeSpan.FromMinutes(90))
        {
            return $"about {age.TotalMinutes:0} minutes ago";
        }

        var hours = age.TotalHours;

        return hours < 2.25
            ? "about two hours ago"
            : $"about {hours:0} hours ago";
    }

    /// <summary>
    /// Whether the radio volunteered anything while this recording was made.
    /// </summary>
    /// <param name="audio">The recording, whose length is the window asked about.</param>
    /// <returns>The observation, beside the setting as it was read.</returns>
    /// <remarks>
    /// <para>**THE SETTING AND THE BEHAVIOUR ARE DIFFERENT FACTS** (HM-DEC-091).
    /// `SHACK_FACTS.md` records CI-V Transceive measured off, and captures the
    /// same week carried it read back as on. Which of those is true is the
    /// operator's to rule; what a capture can do is carry the measurement beside
    /// the name, so the next argument about it starts from evidence.</para>
    /// <para>**THIS OBSERVES AND NOTHING MORE.** It writes nothing to the radio
    /// and advises nobody to change anything (§0.2).</para>
    /// </remarks>
    private string BroadcastDuringCapture(MonoAudio audio)
    {
        var setting = RigState[RigField.CivTransceive];
        var said = setting.IsKnown
            ? setting.Text
            : setting.State.ToString().ToLowerInvariant();

        if ((_rig as Ic7300Rig)?.Link is not { } health || health.Inbound == 0)
        {
            return $"the setting reads {said}, and no frame has arrived on the "
                   + "link to check it against";
        }

        var opened = DateTime.UtcNow - audio.Duration;
        var during = health.LastTransceiveUtc is { } last && last >= opened;

        return during
            ? $"the radio volunteered a change of its own while this recording "
              + $"was being made, and the setting reads {said}"
            : $"the radio volunteered nothing while this recording was being "
              + $"made, and the setting reads {said}; {health.InboundTransceive} "
              + $"of {health.Inbound} frames since the link came up were the "
              + "radio announcing something";
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
        AutoCall.Attach(rig, _rigMonitor, _decoder);
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
        finally
        {
            // **WHATEVER HAPPENED, THE BAND ON SCREEN IS NOW THE BEST ANSWER
            // THERE IS** (HM-DEC-118). A radio that answered has set it from
            // the dial; one that did not leaves the remembered band, which is
            // the same guess as before and is now the only guess available
            // rather than a guess made in preference to asking.
            await ReloadSpotsAsync("startup").ConfigureAwait(true);
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
        // The near miss is recorded here as well as on the tick, because this is
        // the path the operator's own hand takes and the tick only sees what is
        // left over (HM-OPEN-039). Whichever notices the move first reports it;
        // the other gets null, because a place is only abandoned once.
        var left = _dwell.Moved(clamped, DateTime.UtcNow);

        if (left is not null)
        {
            AppEvents.RecentDwellShort(
                _telemetry, left.FrequencyHz, left.ShortBySeconds);
        }

        // The forget button belongs to where the dial is, so it comes and goes
        // with the dial (HM-DEC-134).
        OnPropertyChanged(nameof(IsSomewhereRemembered));

        // **AND THE DECODER LETS GO OF THE PITCH IT MEASURED SOMEWHERE ELSE.**
        // The tracker holds its last measured pitch through the gaps in a slow
        // sender's keying, which is what makes a slow fist readable at all and
        // is untouched while the dial stays put. What it could not do was let
        // go: on 2026-08-26 the operator tuned here from twenty-four minutes and
        // one QSY away, and the decoder went on mixing at the 300 Hz it had
        // measured there while the station in front of him keyed above 400. It
        // refused everything, correctly, because nothing was keyed at 300.
        //
        // It hangs on the frequency rather than on a clock because that is when
        // the evidence stops existing. A station is entitled to pause for as
        // long as it likes; it is not entitled to be heard on a frequency the
        // receiver has left.
        _decoder?.Retuned();

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

    /// <summary>When Hamlet last moved the dial itself, or null.</summary>
    /// <remarks>
    /// **THE MOMENT, NOT A WINDOW.** What makes a reading unusable is that it was
    /// taken before the radio was told to move, and that is a comparison rather
    /// than a duration. A timer here would either be too short on a busy link or
    /// leave the display frozen after it had already caught up, and both of those
    /// are the app deciding it knows better than the radio (§0.0).
    /// </remarks>
    private DateTime? _tunedAtUtc;

    /// <summary>Where the dial was before that tune, or null.</summary>
    private long _tunedFromHz;

    /// <summary>Where that tune was aiming, or null.</summary>
    private long? _tunedToHz;

    /// <summary>True while the frequency write is actually on the wire.</summary>
    /// <remarks>
    /// `_rigSendPending` covers the queue and is cleared the moment the send
    /// starts, which left the whole round trip unguarded. They are two different
    /// states and the display needs both.
    /// </remarks>
    private bool _writeInFlight;

    /// <summary>
    /// Whether a reading can speak about where the dial is now.
    /// </summary>
    /// <param name="value">The reading.</param>
    /// <returns>True when it was taken after Hamlet's own last tune.</returns>
    /// <remarks>
    /// True when Hamlet has not tuned at all, which is every case of the operator
    /// using the radio's own knob: nothing here slows down the path that was
    /// always right.
    /// </remarks>
    private bool IsAfterOurOwnTune(RigValue value)
        => DialGuard.MayFollow(value, _tunedAtUtc);

    /// <summary>Record a tune the way the send tick does, for tests.</summary>
    /// <param name="toHz">Where it was aimed.</param>
    /// <param name="fromHz">Where the dial was.</param>
    /// <param name="atUtc">When the command went out.</param>
    /// <remarks>
    /// The send tick needs a radio and a dispatcher timer; the rule it sets up is
    /// three fields and a comparison, and that is what the display depends on. So
    /// the fields are settable from the test project and the rule is exercised
    /// exactly as the tick leaves it (§5: determinism below the UI).
    /// </remarks>
    internal void NoteTuneWritten(long toHz, long fromHz, DateTime atUtc)
    {
        _tunedAtUtc = atUtc;
        _tunedFromHz = fromHz;
        _tunedToHz = toHz;
    }

    private async void OnRigSendTick(object? sender, EventArgs e)
    {
        if (!_rigSendPending || _rig is null || !IsConnected)
        {
            _rigSendTimer.Stop();
            return;
        }

        _rigSendPending = false;

        // **STAMPED BEFORE THE COMMAND GOES, NOT AFTER IT COMES BACK.** A reading
        // that crossed the write on the wire is exactly the one that must not be
        // believed, and it is in flight from this line onward.
        var from = _tunedFromHz;
        _tunedAtUtc = DateTime.UtcNow;
        _tunedFromHz = RigState[RigField.Frequency] is { IsKnown: true, Number: { } was }
            ? (long)was
            : from;
        _tunedToHz = FrequencyHz;
        _writeInFlight = true;

        var outcome = "proceeded";
        var target = FrequencyHz;

        try
        {
            await _rig.SetFrequencyHzAsync(target);
        }
        catch (Exception ex)
        {
            outcome = "failed";
            StatusText = $"Set frequency failed: {ex.Message}";
        }
        finally
        {
            _writeInFlight = false;
        }

        // **THERE WAS NO EVENT FOR THIS AT ALL** (§0.0.1). The record carried the
        // request to tune and nothing for the write that carried it out, so a
        // display disagreeing with the radio could not be placed on either side
        // of the one command in the middle.
        AppEvents.TuneWritten(
            _telemetry, target, outcome,
            _tunedAtUtc is { } when ? (DateTime.UtcNow - when).TotalSeconds : null);

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
    /// <summary>
    /// How many times the mode decision has been rescheduled this session.
    /// </summary>
    /// <remarks>
    /// **A GUARD IN FRONT OF AN UNEXPLAINED LOOP IS A SYMPTOM TREATED**
    /// (HM-OPEN-041). The plan remembering its last confirmed write stops the
    /// radio being written to over and over; it does not explain why the decision
    /// was being recomputed eighteen times in an evening with the dial standing
    /// still. This counts the reschedules so a test can assert that nothing
    /// changing produces nothing recomputing, which is the thing that was never
    /// checked.
    /// </remarks>
    internal int ModeFollowReschedules { get; private set; }

    private void ScheduleModeFollow()
    {
        // **COUNTED BEFORE THE GUARD, BECAUSE THE QUESTION IS HOW OFTEN THIS IS
        // ASKED.** Counting after it would measure how often a radio was attached,
        // and the loop being chased happens whether or not one is.
        ModeFollowReschedules++;

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

        // Read once and carried, because the dial can move while the write is
        // in flight and the memory has to name the frequency it was made at.
        var atHz = FrequencyHz;

        var here = Neighborhoods.FirstOrDefault(n => n.Contains(atHz));

        // **WHAT HE IS VISIBLY DOING BEATS WHAT THE MAP SAYS LIVES HERE.** The
        // terminal decoding and the dial inside a CW segment are both the
        // operator's own hand (HM-DEC-056), and on 2026-08-18 ignoring them cost
        // him sixty-six seconds of not being able to answer a station.
        var workingCw = IsInsideCwSegment || IsCopyingMorse;

        var decision = ModeFollowPlan.Decide(
            _modeFollow, RigState.Mode, RigState.IsDataMode,
            ModeFollowPlan.TargetFor(here), atHz, workingCw);

        if (!decision.Write)
        {
            return;
        }

        _settingModeOurselves = true;
        try
        {
            var result = await rig.SetModeAsync(decision.Mode, decision.DataMode);

            _lastKnownMode = result.Worked ? decision.Mode : null;

            // The write is remembered only where the radio confirmed it, so a
            // failed write is retried and a successful one is not repeated
            // (HM-OPEN-041).
            if (result.Worked)
            {
                _modeFollow = _modeFollow.Done(
                    atHz, decision.Mode, decision.DataMode);
            }

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
        var left = _dwell.Moved(FrequencyHz, nowUtc);

        if (left is not null)
        {
            AppEvents.RecentDwellShort(
                _telemetry, left.FrequencyHz, left.ShortBySeconds);
        }

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

        // Read before the list is rebuilt, because afterwards there is no way
        // to tell a fold from a fresh entry: both leave one row (HM-OPEN-039).
        var alreadyHere = Recent.FirstOrDefault(
            e => RecentStations.IsSamePlace(e.FrequencyHz, visit.FrequencyHz));

        var wasThere = Recent.Select(e => e.FrequencyHz).ToList();

        var kept = RecentStations.Remember(Recent, visit);

        Recent.Clear();
        foreach (var entry in kept)
        {
            Recent.Add(entry);
        }

        if (alreadyHere is not null)
        {
            AppEvents.RecentFolded(
                _telemetry,
                visit.FrequencyHz,
                alreadyHere.FrequencyHz,
                Math.Abs(visit.FrequencyHz - alreadyHere.FrequencyHz),
                alreadyHere.Visits + 1);
        }
        else
        {
            AppEvents.RecentRemembered(
                _telemetry, visit.FrequencyHz, visit.IsIdentified);
        }

        // The one that fell off the end, where one did. Named by where it was
        // rather than counted, so the record says which place was lost.
        foreach (var gone in wasThere.Where(hz => kept.All(k => k.FrequencyHz != hz)))
        {
            if (alreadyHere is null || gone != alreadyHere.FrequencyHz)
            {
                AppEvents.RecentDropped(_telemetry, gone);
            }
        }

        PersistRecent();
    }

    /// <summary>How much of the transcript goes in a roster row.</summary>
    /// <remarks>
    /// A hundred and twenty characters, which is `CwTranscript.LongestTip`'s own
    /// figure and carries several overs at any speed. The whole transcript goes in
    /// the sidecar, which is not constrained to one line.
    /// </remarks>
    private const int RosterTextLength = 120;

    /// <summary>Where captures and the roster are written.</summary>
    /// <remarks>
    /// Settable so a test can point the whole path at a temporary folder, in the
    /// manner `LayoutStore.Path` already is. **The operator's own captures are not
    /// a test's to write into**, and the adjudicated fixture folder is not either.
    /// </remarks>
    internal static string CaptureFolder { get; set; }
        = Path.Combine(SettingsStore.DataFolder, "captures");

    /// <summary>
    /// Record that the operator heard a station here (the roster).
    /// </summary>
    /// <param name="wav">The recording written, or "" when none was.</param>
    /// <param name="refusal">Why none was, or "" when one was.</param>
    /// <param name="inRecording">
    /// What the decoder did over the audio that was kept, or null when there is
    /// no recording or its figures could not be derived.
    /// </param>
    /// <remarks>
    /// <para>**THE PRESS ASSERTS SOMETHING THE APPLICATION CANNOT KNOW**: that
    /// there was a station there to hear. Every other number Hamlet holds is
    /// downstream of its own decoder, so a station it misses is a case it never
    /// counts, and a score built from them would come out at a hundred per cent
    /// while the operator sat listening to somebody it could not read.</para>
    /// <para>Called on every exit from the capture command, including both
    /// refusals. **A case with no evidence is still a case** and belongs in the
    /// denominator.</para>
    /// </remarks>
    private void MarkCase(
        string wav, string refusal, CwCounterDelta? inRecording = null)
    {
        var report = _decoder?.Report;

        // **THE ROW IS SCORED, SO ITS COUNT HAS TO BE ABOUT THE CASE**
        // (HM-DEC-091). Where the recording's own figures could be derived they
        // go in; where there is no recording, or the decoder's history does not
        // reach back over it, the session totals go in **and the cell says so**
        // rather than passing for an answer about this station.
        var emitted = inRecording?.CharactersEmitted ?? report?.CharactersEmitted ?? 0;
        var unsure = inRecording?.CharactersUnsure ?? report?.CharactersUnsure ?? 0;

        var covers = inRecording is not null
            ? CwCountsCover.Recording
            : wav.Length == 0
                ? CwCountsCover.NoRecording
                : CwCountsCover.Session;

        CwCaseRoster.Append(
            CaptureFolder,
            new CwCase(
                DateTime.UtcNow,
                CapturedHz,
                CapturedBandName(),
                wav,
                refusal,
                report is { HasTone: true } tone ? tone.ToneHz : null,
                report is { } r && !double.IsNaN(r.SnrDb) ? r.SnrDb : null,
                // **ONE SOURCE, TAKEN HERE** (HM-DEC-091). This was the polled
                // snapshot the header happens to be holding, which is a different
                // instant from every other figure on the row.
                _decoder?.WordsPerMinute,
                emitted,
                unsure,

                // **THE TAIL AT THE MOMENT OF THE PRESS**, which is what he was
                // looking at when he decided there was a station there. A hundred
                // and twenty characters carries several overs at any speed and
                // still leaves the row one line in a text editor.
                Transcript.Tail(RosterTextLength),
                covers,
                KeyingLine(_keyingReading),

                // **THE SEED COLUMN IS ALWAYS EMPTY NOW.** The control that
                // filled it was inert and came out; the column stays so a roster
                // started before this build and one started after it are the same
                // shape, and a later ruling can retire it.
                null,

                // And what the fit behind the speed looked like, so a row with no
                // speed on it can be told from a row whose speed came out of a
                // fit that was not a fist.
                FitLine()));
    }

    /// <summary>
    /// The band the capture was made on, from the frequency that was read.
    /// </summary>
    /// <remarks>
    /// The same reading the sidecar's own band line uses, so a roster row and the
    /// sidecar beside it cannot disagree about where the radio was (HM-DEC-091).
    /// </remarks>
    private string CapturedBandName()
        => HfBands.BandFor(CapturedHz)?.Name ?? "outside every band Hamlet knows";

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
                Visits = r.Visits,
            })
            .ToList();

        SettingsStore.Save(_settings);
        RebuildMenus();

        // Every route that changes the list comes through here, which is the
        // only reason the offer to forget where you are cannot go stale
        // (HM-DEC-134).
        OnPropertyChanged(nameof(IsSomewhereRemembered));
    }

    /// <summary>Take one place out of the recent list (HM-DEC-134).</summary>
    /// <param name="entry">The entry, or null.</param>
    /// <remarks>
    /// **THE LIST IS A RECORD OF WHERE HE WAS, AND HE IS ALLOWED TO SAY SOME OF
    /// IT WAS NOT WORTH KEEPING.** Nothing here asks him to confirm: a removed
    /// entry costs a visit to get back and the dwell that produced it was
    /// twenty seconds, so a dialog would be guarding something cheaper than the
    /// dialog.
    /// </remarks>
    [RelayCommand]
    private void ForgetRecent(RecentStation? entry)
    {
        if (entry is null)
        {
            return;
        }

        var kept = RecentStations.Remove(Recent, entry.FrequencyHz);

        if (kept.Count == Recent.Count)
        {
            return;
        }

        AppEvents.RecentRemoved(_telemetry, all: false, removed: 1);

        Recent.Clear();
        foreach (var row in kept)
        {
            Recent.Add(row);
        }

        PersistRecent();
    }

    /// <summary>True when where the dial is now is in the recent list.</summary>
    /// <remarks>
    /// **THE BUTTON IS ABSENT RATHER THAN GREY WHERE THERE IS NOTHING TO
    /// FORGET** (§0.5.1). Grey is reserved for what genuinely cannot be used,
    /// and a control offering to remove somewhere the operator has never been
    /// is not disabled, it is meaningless.
    /// </remarks>
    public bool IsSomewhereRemembered
        => Recent.Any(e => RecentStations.IsSamePlace(e.FrequencyHz, FrequencyHz));

    /// <summary>Forget the place the dial is on now (HM-DEC-134).</summary>
    [RelayCommand]
    private void ForgetHere()
        => ForgetRecent(
            Recent.FirstOrDefault(
                e => RecentStations.IsSamePlace(e.FrequencyHz, FrequencyHz)));

    /// <summary>Empty the recent list (HM-DEC-134).</summary>
    [RelayCommand]
    private void ForgetAllRecent()
    {
        if (Recent.Count == 0)
        {
            return;
        }

        AppEvents.RecentRemoved(_telemetry, all: true, removed: Recent.Count);

        Recent.Clear();
        PersistRecent();
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
        // **THE ON-DEMAND READ THAT USED TO BE HERE IS GONE** (HM-DEC-109).
        // It was closing a consequence rather than the cause: the band these
        // sources are scoped to derives from the frequency, and the frequency
        // could be wrong because a broadcast was missed at startup. The sweep
        // fixes that at the source, every thirty seconds, for everything that
        // reads the frequency rather than only for the two callers somebody
        // remembered. A spot refresh runs every one to fifteen minutes, so the
        // sweep is always the fresher of the two anyway.
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
            var band = HfBands.BandFor(hz) ?? AmateurSpectrum.Nearest(hz);
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
            // **THE TRANSMITTER FIRST.** A disconnect while a cycle is running
            // is the case with the most at stake, and the stop code goes out
            // while there is still a port to send it on (§0.2).
            AutoCall.StopNow();
            AutoCall.Attach(null, null, null);
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

    /// <summary>The calling cycle (HM-DEC-098).</summary>
    public const string AutoCall = "autocall";

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
    {
        get
        {
            var text = Character.Length == 0
                ? Activity.Tooltip
                : Character + TooltipParagraphBreak + Activity.Tooltip;

            // **THE BADGE'S OWN REASON MOVED HERE WHEN THE BADGE STOPPED TAKING
            // CLICKS.** It used to carry this tooltip itself; it is now drawn
            // over the card and hit-tested out of the way, because in the
            // layout flow it was pushing badged cards down and overhanging its
            // neighbours, and on 2026-08-25 that cost the operator the ability
            // to click `40 m` at all. A thing that cannot be hovered cannot
            // explain itself, and dropping the explanation would be hiding
            // information rather than a control (§0.5).
            return IsBestBet && BestBetTooltip.Length > 0
                ? text + TooltipParagraphBreak + BestBetTooltip
                : text;
        }
    }

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

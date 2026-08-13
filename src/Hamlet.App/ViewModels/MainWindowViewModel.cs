using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls.ApplicationLifetimes;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Telemetry;
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

    private readonly DispatcherTimer _rigSendTimer;
    private readonly DispatcherTimer _spotRefreshTimer;
    private readonly DispatcherTimer _ageTimer;
    private readonly AppSettings _settings;
    private readonly JsonlTelemetry? _telemetry;
    private readonly List<ActivitySpot> _allBandSpots = new();
    private AggregateActivitySource _activitySource;
    private RbnActivitySource? _rbn;
    private IDisposable[] _ownedSources = Array.Empty<IDisposable>();
    private TrainingSpectrumSource? _trainingSpectrum;
    private readonly Audio.ModeAudioPlayer _audio = new();
    private IRig? _rig;
    private bool _updatingFromRig;
    private bool _rigSendPending;
    private bool _windowVisible = true;
    private bool _spotsEverLoaded;
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
    private BandButtonViewModel _selectedBand;

    [ObservableProperty]
    private IReadOnlyList<Neighborhood> _neighborhoods = Array.Empty<Neighborhood>();

    [ObservableProperty]
    private IReadOnlyList<Controls.ActivityDot> _activityDots =
        Array.Empty<Controls.ActivityDot>();

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

    /// <summary>The field guide entries, each with its samples.</summary>
    public IReadOnlyList<ModeCardViewModel> ModeCards { get; } =
        ModeGuide.Modes.Select(m => new ModeCardViewModel(m)).ToList();

    /// <summary>Happening-now spots, plain language, source-labeled.</summary>
    public ObservableCollection<SpotViewModel> Spots { get; } = new();

    /// <summary>Phase 1 bands with best-bet ranking for the current hour.</summary>
    public ObservableCollection<BandButtonViewModel> Bands { get; }

    /// <summary>The training radio plus every serial port on this machine.</summary>
    public ObservableCollection<string> AvailablePorts { get; }

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
    /// signals up unlabelled.
    /// </remarks>
    public bool SignalsAreSimulated => SpectrumSource?.IsSimulated == true;

    /// <summary>The persistent label the waterfall panel carries.</summary>
    public string SpectrumNotice
        => SignalsAreSimulated
            ? "Simulated signals — the training radio, not the air"
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

    /// <summary>Collapsed-header line for the CW terminal.</summary>
    public string TerminalSummary => "no decode yet";

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

        var bets = BandPlan.BestBets(DateTime.Now.Hour);
        Bands = new ObservableCollection<BandButtonViewModel>(
            BandPlan.Bands.Select(b => new BandButtonViewModel(
                b,
                isBestBet: bets.Count > 0 && bets[0] == b.Name,
                isSecondBet: bets.Count > 1 && bets[1] == b.Name)));

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

        _spotRefreshTimer = new DispatcherTimer(
            TimeSpan.FromMinutes(AppSettings.DefaultSpotRefreshMinutes),
            DispatcherPriority.Background, OnSpotRefreshTick);
        _spotRefreshTimer.Stop();

        _ageTimer = new DispatcherTimer(AgeTick, DispatcherPriority.Background, OnAgeTick);
        _ageTimer.Stop();

        _activitySource = BuildSources();

        Neighborhoods = NeighborhoodPlan.ForBand(_selectedBand.Band);
        ShowNeighborhood(Neighborhoods.First(n => n.Contains(FrequencyHz)));
        UpdateModeLine();
        UpdateSpotFreshness();

        _ = ReloadSpotsAsync("startup");
        ApplyFeedTimers();
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
        StoryTitle = $"{mode.Name} — {mode.Tagline}";
        StoryBadge = mode.Difficulty;
        StoryBody = $"{mode.Why} Sounds like: {mode.Sound}. Learn its waterfall "
            + "fingerprint and the void stops being static.";
        StoryTuneHz = mode.LivesAt40mHz ?? SelectedBand.Band.JumpHz;
    }

    /// <summary>Tune the rig (and the whole UI) to a target — the payoff
    /// click on every story and spot.</summary>
    [RelayCommand]
    private void TuneTo(long hz)
    {
        AppEvents.TuneRequested(_telemetry, hz, "story_or_spot");
        var band = BandPlan.BandFor(hz);
        if (band is not null && band.Name != SelectedBand.Band.Name)
        {
            SelectedBand = Bands.First(b => b.Band.Name == band.Name);
        }

        FrequencyHz = hz;
    }

    partial void OnSelectedBandChanged(BandButtonViewModel value)
    {
        Neighborhoods = NeighborhoodPlan.ForBand(value.Band);
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

    partial void OnSelectedPortChanged(string value)
    {
        _settings.LastPort = value;
        SettingsStore.Save(_settings);
    }

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
    /// against that band's own neighborhood map — practising on 20 m has to
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
        StopTrainingSpectrum();
        _audio.Dispose();
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

        var rig = CreateRig(SelectedPort);
        var rigType = SelectedPort == TrainingRadio ? "simulated" : "IC-7300";
        StatusText = $"Connecting to {SelectedPort}…";

        if (!await rig.ConnectAsync())
        {
            (rig as IDisposable)?.Dispose();
            AppEvents.ConnectFailed(_telemetry, SelectedPort, rigType, "no_response");
            StatusText = $"No answer on {SelectedPort} — check cable, baud and "
                       + "CI-V address (HM-OPEN-003)";
            return;
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

        var hz = await rig.GetFrequencyHzAsync();
        ApplyRigFrequency(hz);
        StatusText = SelectedPort == TrainingRadio
            ? "On the training radio — synthesised signals, nothing on the air"
            : $"Connected — IC-7300 on {SelectedPort} · CI-V bytes unverified until "
              + "HM-OPEN-002 closes";
    }

    /// <summary>UI-origin frequency changes: clamp to band, refresh the mode
    /// line, and schedule a throttled rig send so tape drags don't flood the
    /// CI-V bus.</summary>
    partial void OnFrequencyHzChanged(long value)
    {
        var clamped = SelectedBand.Band.Clamp(value);
        if (clamped != value)
        {
            FrequencyHz = clamped;
            return;
        }

        UpdateModeLine();

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

        _allBandSpots.Clear();
        _allBandSpots.AddRange(spots);

        // The list shows the band on screen; the conditions line keeps the
        // whole spectrum, which is what lets it say "try 40 m" with a count.
        var onBand = spots
            .Where(s => SelectedBand.Band.LowHz <= s.FrequencyHz
                        && s.FrequencyHz <= SelectedBand.Band.HighHz)
            .ToList();

        var ranked = SpotRanking.Rank(onBand, now);
        var newCount = RebuildSpotList(ranked, now);

        ActivityDots = BuildDots(ranked, now);
        Lead = LeadCard.Choose(ranked, SelectedBand.Band.Name, AnySourceAnswering());
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
    /// Rebuild the card list in ranked order, reusing the cards already on
    /// screen so a surviving spot keeps its identity.
    /// </summary>
    /// <param name="ranked">Ranked spots, best first.</param>
    /// <param name="now">Reference time.</param>
    /// <returns>How many spots were not in the previous set.</returns>
    /// <remarks>
    /// HM-DEC-020 said the list is not re-sorted on every tick, because moving
    /// a card out from under a reading operator's cursor costs more than a
    /// perfect order. That still holds and is why the one-second age tick only
    /// re-ages text. Ranking reorders on a data refresh only — a deliberate,
    /// five-minutes-apart event where the content genuinely changed
    /// (HM-DEC-025 amends HM-DEC-020 to exactly this extent).
    /// </remarks>
    private int RebuildSpotList(IReadOnlyList<RankedSpot> ranked, DateTime now)
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

            if (existing.TryGetValue(key, out var vm))
            {
                vm.Update(entry.Spot, now, entry.Reason);
                rebuilt.Add(vm);
                continue;
            }

            // Nothing is "new" on the first load — everything would be.
            rebuilt.Add(new SpotViewModel(
                entry.Spot, now, isNew: _spotsEverLoaded, entry.Reason));

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
    private static IReadOnlyList<Controls.ActivityDot> BuildDots(
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
                prominence));
        }

        return dots;
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

    /// <summary>Re-age everything on screen: the header line, its colour, each
    /// card's own age, and the expiry of the "new" tags.</summary>
    private void UpdateSpotFreshness()
    {
        var now = DateTime.UtcNow;
        var since = now - _lastSpotLoadUtc;
        var interval = _settings.SpotRefreshMinutes;

        SpotsSummary = SpotFreshness.Summary(
            Spots.Count, since, interval, _spotsEverLoaded)
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
    private void ApplyRigFrequency(long hz)
    {
        _updatingFromRig = true;
        try
        {
            var band = BandPlan.BandFor(hz);
            if (band is not null && band.Name != SelectedBand.Band.Name)
            {
                SelectedBand = Bands.First(b => b.Band.Name == band.Name);
            }

            FrequencyHz = SelectedBand.Band.Clamp(hz);
        }
        finally
        {
            _updatingFromRig = false;
        }
    }

    private void UpdateModeLine()
    {
        IsInsideCwSegment = SelectedBand.Band.IsInCwSegment(FrequencyHz);
        ModeLineText = $"CW · {SelectedBand.Band.Name} · "
            + (IsInsideCwSegment ? "inside the CW segment" : "OUTSIDE the CW segment");
    }

    private async Task TearDownRigAsync()
    {
        StopTrainingSpectrum();
        _rigSendTimer.Stop();
        _rigSendPending = false;
        if (_rig is not null)
        {
            _rig.FrequencyChanged -= OnRigFrequencyChanged;
            await _rig.DisconnectAsync();
            (_rig as IDisposable)?.Dispose();
            _rig = null;
        }

        IsConnected = false;
        ConnectButtonText = "Connect";
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
}

/// <summary>One band button: the band plus its best-bet ranking for the hour
/// the app started. FG-001 replaces the ranking with live spot data.</summary>
public sealed class BandButtonViewModel
{
    /// <summary>Creates the button model.</summary>
    /// <param name="band">The band this button selects.</param>
    /// <param name="isBestBet">True on the top-ranked band for the hour.</param>
    /// <param name="isSecondBet">True on the runner-up band.</param>
    public BandButtonViewModel(CwBand band, bool isBestBet, bool isSecondBet)
    {
        Band = band;
        IsBestBet = isBestBet;
        IsSecondBet = isSecondBet;
    }

    /// <summary>The band this button selects.</summary>
    public CwBand Band { get; }

    /// <summary>True on the top-ranked band for the current hour.</summary>
    public bool IsBestBet { get; }

    /// <summary>True on the runner-up band.</summary>
    public bool IsSecondBet { get; }
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

    private ActivitySpot _spot;
    private DateTime _newUntilUtc;

    /// <summary>Wraps an engine spot for display.</summary>
    /// <param name="spot">The engine's spot.</param>
    /// <param name="nowUtc">The reference time for the age line.</param>
    /// <param name="isNew">True when this spot was not in the previous set.</param>
    /// <param name="reason">The ranking's stated reason for this card.</param>
    public SpotViewModel(ActivitySpot spot, DateTime nowUtc, bool isNew, string reason = "")
    {
        _spot = spot;
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
    public void Update(ActivitySpot spot, DateTime nowUtc, string reason = "")
    {
        _spot = spot;
        IsNew = false;
        if (reason.Length > 0)
        {
            Reason = reason;
        }

        Reage(nowUtc);
    }

    /// <summary>Recompute the age line, and expire the "new" tag once it has
    /// had its thirty seconds.</summary>
    /// <param name="nowUtc">Reference time.</param>
    public void Reage(DateTime nowUtc)
    {
        Provenance = $"{_spot.Mode} · {_spot.Source} · "
            + SpotFreshness.Describe(nowUtc - _spot.HeardAtUtc);

        if (IsNew && nowUtc >= _newUntilUtc)
        {
            IsNew = false;
        }
    }
}

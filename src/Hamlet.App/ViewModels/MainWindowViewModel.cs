using System.Collections.ObjectModel;
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
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Transport;

namespace Hamlet.App.ViewModels;

/// <summary>
/// Shell ViewModel. One source of truth — <see cref="FrequencyHz"/> — that
/// the digits, ribbon and tape all bind to two-way. UI-origin changes are
/// throttled out to the rig; rig-origin changes (the physical knob) flow in
/// without echoing back out.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    /// <summary>The no-hardware entry in the port list.</summary>
    public const string SimulatedRig = "Simulated rig (no hardware)";

    private static readonly TimeSpan RigSendThrottle = TimeSpan.FromMilliseconds(150);

    private readonly DispatcherTimer _rigSendTimer;
    private readonly AppSettings _settings;
    private readonly JsonlTelemetry? _telemetry;
    private IRig? _rig;
    private bool _updatingFromRig;
    private bool _rigSendPending;

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
    private string _selectedPort = SimulatedRig;

    [ObservableProperty]
    private BandButtonViewModel _selectedBand;

    [ObservableProperty]
    private IReadOnlyList<Neighborhood> _neighborhoods = Array.Empty<Neighborhood>();

    [ObservableProperty]
    private IReadOnlyList<long> _activityFrequencies = Array.Empty<long>();

    [ObservableProperty]
    private string _storyTitle = "";

    [ObservableProperty]
    private string _storyBadge = "";

    [ObservableProperty]
    private string _storyBody = "";

    [ObservableProperty]
    private long _storyTuneHz;

    /// <summary>The field guide entries.</summary>
    public IReadOnlyList<ModeInfo> ModeCards { get; } = ModeGuide.Modes;

    /// <summary>Happening-now spots, plain language, source-labeled.</summary>
    public ObservableCollection<SpotViewModel> Spots { get; } = new();

    /// <summary>Phase 1 bands with best-bet ranking for the current hour.</summary>
    public ObservableCollection<BandButtonViewModel> Bands { get; }

    /// <summary>The simulated rig plus every serial port on this machine.</summary>
    public ObservableCollection<string> AvailablePorts { get; }

    /// <summary>Designer constructor.</summary>
    public MainWindowViewModel() : this(new AppSettings(), null)
    {
    }

    /// <summary>Runtime constructor.</summary>
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

        AvailablePorts = new ObservableCollection<string> { SimulatedRig };
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

        Neighborhoods = NeighborhoodPlan.ForBand(_selectedBand.Band);
        ShowNeighborhood(Neighborhoods.First(n => n.Contains(FrequencyHz)));
        UpdateModeLine();
        _ = LoadSpotsAsync(new FakeActivitySource());
    }

    private async Task LoadSpotsAsync(IActivitySource source)
    {
        var spots = await source.GetSpotsAsync();
        Spots.Clear();
        foreach (var s in spots)
        {
            Spots.Add(new SpotViewModel(s));
        }

        ActivityFrequencies = spots.Select(s => s.FrequencyHz).ToArray();
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
    }

    partial void OnSelectedPortChanged(string value)
    {
        _settings.LastPort = value;
        SettingsStore.Save(_settings);
    }

    /// <summary>Reload the happening-now feed.</summary>
    [RelayCommand]
    private async Task RefreshSpotsAsync()
    {
        await LoadSpotsAsync(new FakeActivitySource());
        _telemetry?.Write(TelemetryCategory.Explore, "spots_refreshed");
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
    }

    /// <summary>Open %AppData%\Hamlet in the file browser.</summary>
    [RelayCommand]
    private void OpenDataFolder() => SettingsStore.OpenDataFolder();

    /// <summary>Show the about box.</summary>
    [RelayCommand]
    private void About()
        => StatusText = "Hamlet — let me ham. Open source, GPL-3.0. "
                      + "Telemetry stays on this computer.";

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
        var rigType = SelectedPort == SimulatedRig ? "simulated" : "IC-7300";
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

        var hz = await rig.GetFrequencyHzAsync();
        ApplyRigFrequency(hz);
        StatusText = SelectedPort == SimulatedRig
            ? "Connected — simulated rig (no hardware attached)"
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
        => selection == SimulatedRig
            ? new FakeRig()
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

/// <summary>One band button: the band plus its best-bet ranking for the hour
/// the app started. FG-001 replaces the ranking with live spot data.</summary>
public sealed class BandButtonViewModel
{
    /// <summary>Creates the button model.</summary>
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
public sealed class SpotViewModel
{
    /// <summary>Wraps an engine spot for display.</summary>
    public SpotViewModel(ActivitySpot spot)
    {
        Story = spot.Story;
        FrequencyHz = spot.FrequencyHz;
        TuneLabel = "Tune " + (spot.FrequencyHz / 1_000_000.0)
            .ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
        var age = (int)Math.Max(0, (DateTime.UtcNow - spot.HeardAtUtc).TotalMinutes);
        Provenance = $"{spot.Mode} · {spot.Source} · {age} min ago";
    }

    /// <summary>The invitation.</summary>
    public string Story { get; }

    /// <summary>Where the Tune button goes.</summary>
    public long FrequencyHz { get; }

    /// <summary>Button text, e.g. "Tune 7.032".</summary>
    public string TuneLabel { get; }

    /// <summary>Mode, source, and age — shown, never hidden.</summary>
    public string Provenance { get; }
}

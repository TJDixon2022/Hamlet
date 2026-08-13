using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.App.Settings;
using Hamlet.App.Licensing;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App.ViewModels;

/// <summary>
/// Settings dialog. Category switches write straight through to the settings
/// file, so a toggle takes effect on the next event with no Apply button
/// (HM-DEC-018).
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _settings;
    private readonly JsonlTelemetry? _telemetry;

    [ObservableProperty]
    private int _maxMegabytes;

    [ObservableProperty]
    private string _usageText = "";

    [ObservableProperty]
    private string _callsign = "";

    [ObservableProperty]
    private string _operatorName = "";

    [ObservableProperty]
    private string _location = "";

    [ObservableProperty]
    private string _gridSquare = "";

    [ObservableProperty]
    private RefreshChoice _spotRefresh;

    [ObservableProperty]
    private LicenceClass _licenceClass;

    [ObservableProperty]
    private bool _restrictTransmitToPrivileges;

    [ObservableProperty]
    private string _licenceProvenance = "";

    /// <summary>Designer constructor.</summary>
    public SettingsViewModel() : this(new AppSettings(), null)
    {
    }

    /// <summary>Runtime constructor.</summary>
    /// <param name="settings">Live settings, written through on every edit.</param>
    /// <param name="telemetry">The writer, or null.</param>
    public SettingsViewModel(AppSettings settings, JsonlTelemetry? telemetry)
    {
        _settings = settings;
        _telemetry = telemetry;
        _maxMegabytes = settings.TelemetryMaxMegabytes;

        _callsign = settings.Operator.Callsign;
        _operatorName = settings.Operator.OperatorName;
        _location = settings.Operator.Location;
        _gridSquare = settings.Operator.GridSquare;
        _licenceClass = settings.Operator.LicenceClass;
        _licenceProvenance = LicenceResolver.DescribeProvenance(settings.Operator);
        _restrictTransmitToPrivileges = settings.RestrictTransmitToPrivileges;

        SpotRefreshChoices = AppSettings.SpotRefreshChoices
            .Select(m => new RefreshChoice(m))
            .ToList();
        _spotRefresh = SpotRefreshChoices
            .FirstOrDefault(c => c.Minutes == settings.SpotRefreshMinutes)
            ?? SpotRefreshChoices.First(
                c => c.Minutes == AppSettings.DefaultSpotRefreshMinutes);

        Categories = new ObservableCollection<TelemetryCategoryViewModel>(
            Describe().Select(d =>
            {
                var vm = new TelemetryCategoryViewModel(
                    d.Category, d.Name, d.Description,
                    settings.IsTelemetryEnabled(d.Category));
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(TelemetryCategoryViewModel.IsEnabled))
                    {
                        _settings.SetTelemetryEnabled(vm.Category, vm.IsEnabled);
                        SettingsStore.Save(_settings);
                    }
                };
                return vm;
            }));

        Sources = new ObservableCollection<SourceToggleViewModel>(
            DescribeSources(settings).Select(d =>
            {
                var vm = new SourceToggleViewModel(
                    d.Name, d.Description, d.Note, settings.IsSourceEnabled(d.Name));
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(SourceToggleViewModel.IsEnabled))
                    {
                        _settings.SetSourceEnabled(vm.SourceName, vm.IsEnabled);
                        SettingsStore.Save(_settings);
                        Telemetry.AppEvents.SourceToggled(
                            _telemetry, vm.SourceName, vm.IsEnabled);
                    }
                };
                return vm;
            }));

        RefreshUsage();
    }

    /// <summary>The switchable categories.</summary>
    public ObservableCollection<TelemetryCategoryViewModel> Categories { get; }

    /// <summary>The switchable activity sources (HM-DEC-022, HM-DEC-024).</summary>
    public ObservableCollection<SourceToggleViewModel> Sources { get; }

    /// <summary>The licence classes the operator can pick from.</summary>
    /// <remarks>
    /// Unknown is offered on purpose. Clearing the class is a legitimate
    /// thing to do, and it makes the band map stop claiming anything rather
    /// than leaving a stale guess on screen (HM-DEC-029).
    /// </remarks>
    public IReadOnlyList<LicenceClass> LicenceClasses { get; } = new[]
    {
        LicenceClass.Unknown, LicenceClass.Technician, LicenceClass.General,
        LicenceClass.Advanced, LicenceClass.Extra, LicenceClass.Novice,
    };

    /// <summary>The offered happening-now refresh intervals (HM-DEC-020).</summary>
    public IReadOnlyList<RefreshChoice> SpotRefreshChoices { get; }

    /// <summary>Where everything is stored.</summary>
    public string DataFolderPath => SettingsStore.DataFolder;

    partial void OnCallsignChanged(string value)
    {
        _settings.Operator.Callsign = value;
        SaveProfile("callsign");
    }

    partial void OnOperatorNameChanged(string value)
    {
        _settings.Operator.OperatorName = value;
        SaveProfile("name");
    }

    partial void OnLocationChanged(string value)
    {
        _settings.Operator.Location = value;
        SaveProfile("location");
    }

    partial void OnGridSquareChanged(string value)
    {
        _settings.Operator.GridSquare = value;
        SaveProfile("grid");
    }

    partial void OnLicenceClassChanged(LicenceClass value)
    {
        // Chosen by hand, and stamped as such: a later lookup will show a
        // disagreement rather than quietly overwriting this (HM-DEC-028).
        if (value == _settings.Operator.LicenceClass)
        {
            return;
        }

        _settings.Operator.SetLicenceClass(
            value,
            value == LicenceClass.Unknown
                ? LicenceClassSource.Unset
                : LicenceClassSource.EnteredByOperator,
            "",
            DateTime.UtcNow);

        SettingsStore.Save(_settings);
        LicenceProvenance = LicenceResolver.DescribeProvenance(_settings.Operator);
        Telemetry.AppEvents.ProfileEdited(_telemetry, "licenceClass");
    }

    partial void OnRestrictTransmitToPrivilegesChanged(bool value)
    {
        _settings.RestrictTransmitToPrivileges = value;
        SettingsStore.Save(_settings);
        Telemetry.AppEvents.TransmitGuardToggled(_telemetry, value);
    }

    partial void OnSpotRefreshChanged(RefreshChoice value)
    {
        _settings.SpotRefreshMinutes = value.Minutes;
        SettingsStore.Save(_settings);
    }

    /// <summary>Persist the profile, and record only WHICH field moved — the
    /// value is the identifying part and never goes to telemetry
    /// (HM-DEC-019).</summary>
    private void SaveProfile(string field)
    {
        SettingsStore.Save(_settings);
        Telemetry.AppEvents.ProfileEdited(_telemetry, field);
    }

    [RelayCommand]
    private void OpenFolder() => SettingsStore.OpenDataFolder();

    [RelayCommand]
    private void ClearTelemetry()
    {
        var removed = _telemetry?.ClearAll() ?? 0;
        RefreshUsage();
        UsageText = removed == 0
            ? "Nothing to clear."
            : $"Cleared {removed} file{(removed == 1 ? "" : "s")}.";
    }

    partial void OnMaxMegabytesChanged(int value)
    {
        _settings.TelemetryMaxMegabytes = value;
        SettingsStore.Save(_settings);
    }

    private void RefreshUsage()
    {
        var bytes = _telemetry?.TotalBytes() ?? 0;
        UsageText = $"Currently using {bytes / 1024.0 / 1024.0:0.00} MB.";
    }

    private static IEnumerable<(TelemetryCategory Category, string Name, string Description)> Describe()
    {
        yield return (TelemetryCategory.Diagnostics, "Diagnostics",
            "App start and stop, version, and unexpected errors.");
        yield return (TelemetryCategory.Rig, "Radio",
            "Connect attempts, timeouts and CI-V errors, with the port and radio type.");
        yield return (TelemetryCategory.Tuning, "Tuning",
            "Band changes and where a tune came from — map, dial tape, digits or a spot.");
        yield return (TelemetryCategory.Explore, "Explore",
            "Which neighborhoods and field-guide cards get opened, and which spots get tuned.");
        yield return (TelemetryCategory.Decode, "Decoding",
            "That a decode ran and how confident it was. Never what was said.");
        yield return (TelemetryCategory.Performance, "Performance",
            "Frame rates and render timings, for finding slowness.");
    }

    /// <summary>
    /// The activity sources, what each one is, and any caveat the operator
    /// needs before switching it on (HM-DEC-024).
    /// </summary>
    private static IEnumerable<(string Name, string Description, string Note)>
        DescribeSources(AppSettings settings)
    {
        yield return (
            PotaActivitySource.SourceName,
            "Parks on the Air — operators calling from parks. The friendliest "
            + "contacts on the band: they want to be found and they will slow down "
            + "for you.",
            "");

        yield return (
            SotaActivitySource.SourceName,
            "Summits on the Air — operators calling from mountain tops. Often slow "
            + "CW, often short of contacts.",
            SotaActivitySource.DisabledReason);

        yield return (
            RbnActivitySource.SourceName,
            "Reverse Beacon Network — automated receivers reporting every CW signal "
            + "they decode. Filtered to your band and to receivers on your continent, "
            + "or it would be thousands a minute.",
            string.IsNullOrWhiteSpace(settings.Operator.Callsign)
                ? "Needs your callsign — RBN has no anonymous login, and Hamlet will "
                  + "not invent one. Set it under Operator above."
                : "Your callsign is sent to RBN as the login, and to POTA and SOTA in "
                  + "the User-Agent, because those services are owed knowing who is "
                  + "calling them. It is still never written to telemetry.");

        yield return (
            FakeActivitySource.SourceName,
            "Built-in sample spots, labelled \"sample\" on every card. For seeing how "
            + "the Explorer behaves with no network.",
            "Off by default now that the live feeds work — leaving it on mixes made-up "
            + "spots into a real list.");
    }
}

/// <summary>One switchable activity source.</summary>
public partial class SourceToggleViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isEnabled;

    /// <summary>Creates the row.</summary>
    /// <param name="sourceName">Stable source name and settings key.</param>
    /// <param name="description">What the source is, in plain language.</param>
    /// <param name="note">A caveat to show under it, or "".</param>
    /// <param name="isEnabled">Current switch position.</param>
    public SourceToggleViewModel(
        string sourceName, string description, string note, bool isEnabled)
    {
        SourceName = sourceName;
        Description = description;
        Note = note;
        _isEnabled = isEnabled;
    }

    /// <summary>Stable source name, e.g. "POTA".</summary>
    public string SourceName { get; }

    /// <summary>What the source is.</summary>
    public string Description { get; }

    /// <summary>A caveat, or "".</summary>
    public string Note { get; }

    /// <summary>True when there is a caveat to show.</summary>
    public bool HasNote => Note.Length > 0;
}

/// <summary>One happening-now refresh interval offered in Settings.</summary>
public sealed class RefreshChoice
{
    /// <summary>Creates the choice.</summary>
    /// <param name="minutes">Interval in minutes; 0 is off.</param>
    public RefreshChoice(int minutes)
    {
        Minutes = minutes;
        Label = minutes switch
        {
            0 => "Off",
            1 => "Every minute",
            _ => $"Every {minutes} minutes",
        };
    }

    /// <summary>Interval in minutes; 0 is off.</summary>
    public int Minutes { get; }

    /// <summary>What the combo box shows.</summary>
    public string Label { get; }
}

/// <summary>One switchable telemetry category.</summary>
public partial class TelemetryCategoryViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isEnabled;

    /// <summary>Creates the row.</summary>
    public TelemetryCategoryViewModel(
        TelemetryCategory category, string name, string description, bool isEnabled)
    {
        Category = category;
        Name = name;
        Description = description;
        _isEnabled = isEnabled;
    }

    /// <summary>Which category this row governs.</summary>
    public TelemetryCategory Category { get; }

    /// <summary>Display name.</summary>
    public string Name { get; }

    /// <summary>Plain-language description of what gets recorded.</summary>
    public string Description { get; }
}

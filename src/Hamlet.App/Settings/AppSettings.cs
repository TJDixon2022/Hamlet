using System.Text.Json;
using System.Text.Json.Serialization;
using Hamlet.RadioEngine.Explore;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App.Settings;

/// <summary>
/// Everything Hamlet remembers between runs, in one file:
/// <c>%AppData%\Hamlet\settings.json</c> (HM-DEC-018). A corrupt or
/// unreadable file yields defaults — losing preferences is a nuisance,
/// refusing to start is a bug.
/// </summary>
public sealed class AppSettings
{
    /// <summary>Window left in device-independent pixels; null until saved.</summary>
    public double? WindowX { get; set; }

    /// <summary>Window top; null until saved.</summary>
    public double? WindowY { get; set; }

    /// <summary>Window width; null until saved.</summary>
    public double? WindowWidth { get; set; }

    /// <summary>Window height; null until saved.</summary>
    public double? WindowHeight { get; set; }

    /// <summary>Whether the window was maximized at exit.</summary>
    public bool WindowMaximized { get; set; }

    /// <summary>Last selected port or the simulated-rig entry.</summary>
    public string? LastPort { get; set; }

    /// <summary>Last selected band name, e.g. "40 m".</summary>
    public string? LastBand { get; set; }

    /// <summary>Who is operating (HM-DEC-019). Displayed in the app, written
    /// here, and never written to telemetry.</summary>
    public OperatorProfile Operator { get; set; } = new();

    /// <summary>Minutes between happening-now refreshes; 0 is off
    /// (HM-DEC-020). Allowed values are in
    /// <see cref="SpotRefreshChoices"/>.</summary>
    public int SpotRefreshMinutes { get; set; } = DefaultSpotRefreshMinutes;

    /// <summary>Per-panel expand/collapse state, keyed by panel id. An absent
    /// key means expanded — a new panel arrives open (HM-DEC-021).</summary>
    public Dictionary<string, bool> PanelExpanded { get; set; } = new();

    /// <summary>
    /// Per-source on/off switches, keyed by source name (HM-DEC-022,
    /// HM-DEC-024). An absent key falls back to
    /// <see cref="DefaultSourceEnabled"/>.
    /// </summary>
    public Dictionary<string, bool> SourceEnabled { get; set; } = new();

    /// <summary>Telemetry category switches. Absent category means enabled —
    /// all categories default on (HM-DEC-018).</summary>
    public Dictionary<string, bool> TelemetryCategories { get; set; } = new();

    /// <summary>Telemetry folder size cap in megabytes.</summary>
    public int TelemetryMaxMegabytes { get; set; } = 50;

    /// <summary>
    /// "Only let me transmit where my license allows" (HM-DEC-029). On by
    /// default.
    /// </summary>
    /// <remarks>
    /// TRANSMIT ONLY. This never restricts tuning, receiving or anything the
    /// band map draws — listening is not regulated and a setting that implied
    /// otherwise would teach a beginner something false about their own
    /// license. It is read at one moment: before Hamlet keys a transmitter.
    /// </remarks>
    public bool RestrictTransmitToPrivileges { get; set; } = true;

    /// <summary>
    /// Whether distances are spoken in miles or kilometers (HM-DEC-038).
    /// </summary>
    /// <remarks>
    /// Miles, because the operator is American, the licence is American and the
    /// regulations are American — the same reasoning as the spelling standard
    /// (HM-DEC-035). It is a setting rather than a constant because the app is
    /// headed for a public release where most of the world counts the other
    /// way, and the default is picked rather than asked (§0.4).
    /// </remarks>
    public DistanceUnits DistanceUnits { get; set; } = DistanceUnits.Miles;

    /// <summary>
    /// Which byline was shown last launch, so the next one differs
    /// (HM-DEC-039). −1 when none has been shown.
    /// </summary>
    public int LastBylineIndex { get; set; } = -1;

    /// <summary>
    /// How long a park or summit activation stays a live invitation, in
    /// minutes (HM-DEC-045).
    /// </summary>
    /// <remarks>
    /// An activator hauled gear somewhere on purpose and stays put working
    /// whoever calls, often for well over an hour, so an hour is generous
    /// rather than optimistic.
    /// </remarks>
    public int ActivationLifetimeMinutes { get; set; } = 60;

    /// <summary>How long a skimmer report stays a live invitation, in minutes.</summary>
    /// <remarks>
    /// Much shorter, because a skimmer report says somebody called CQ at that
    /// moment and nothing at all about whether they are still calling.
    /// </remarks>
    public int SkimmerLifetimeMinutes { get; set; } = 20;

    /// <summary>How long contest activity stays a live invitation, in minutes.</summary>
    /// <remarks>
    /// Longest of the three: contest stations sit on one frequency for the
    /// whole event. Only applied where the source actually said it was a
    /// contest exchange, never guessed from a busy band.
    /// </remarks>
    public int ContestLifetimeMinutes { get; set; } = 180;

    /// <summary>The configured lifetimes, with absurd values refused.</summary>
    [JsonIgnore]
    public SpotLifetimeSettings Lifetimes => SpotLifetimeSettings.FromMinutes(
        ActivationLifetimeMinutes, SkimmerLifetimeMinutes, ContestLifetimeMinutes);

    /// <summary>The refresh interval the app ships with, in minutes.</summary>
    public const int DefaultSpotRefreshMinutes = 5;

    /// <summary>The offered refresh intervals in minutes; 0 is off
    /// (HM-DEC-020).</summary>
    public static IReadOnlyList<int> SpotRefreshChoices { get; } =
        new[] { 0, 1, 2, 5, 10, 15 };

    /// <summary>True when the category is on. Unknown categories are on.</summary>
    public bool IsTelemetryEnabled(TelemetryCategory category)
        => !TelemetryCategories.TryGetValue(category.ToString(), out var on) || on;

    /// <summary>Turn a category on or off.</summary>
    public void SetTelemetryEnabled(TelemetryCategory category, bool enabled)
        => TelemetryCategories[category.ToString()] = enabled;

    /// <summary>How many telemetry categories are currently on. Derived from
    /// the switches, never stored alongside them.</summary>
    [JsonIgnore]
    public int EnabledTelemetryCategoryCount
        => Enum.GetValues<TelemetryCategory>().Count(IsTelemetryEnabled);

    /// <summary>True when the panel is expanded. Unknown panels are expanded.</summary>
    /// <param name="panelKey">Stable panel id, e.g. "spots".</param>
    public bool IsPanelExpanded(string panelKey)
        => !PanelExpanded.TryGetValue(panelKey, out var open) || open;

    /// <summary>
    /// Whether an activity source ships switched on.
    /// </summary>
    /// <param name="sourceName">Source name, e.g. "POTA".</param>
    /// <returns>True when the source is on by default.</returns>
    /// <remarks>
    /// <para>Two ship off. SOTA is off for a reason that is not technical: its
    /// API's terms of service require the developer to have registered with
    /// the SOTA Reflector's API-consumers group and to have had AI-written
    /// software approved before it connects. Hamlet will not enter into that
    /// on the operator's behalf, so the switch starts off and the reason is
    /// printed next to it (HM-DEC-024).</para>
    /// <para>The sample feed is off because live feeds now work, and mixing
    /// invented spots into a real list is the prime directive broken for the
    /// sake of a fuller-looking panel. It stays one click away, because it is
    /// how the Explorer gets built with no network.</para>
    /// </remarks>
    public static bool DefaultSourceEnabled(string sourceName)
        => !string.Equals(
               sourceName,
               RadioEngine.Explore.SotaActivitySource.SourceName,
               StringComparison.OrdinalIgnoreCase)
           && !string.Equals(
               sourceName,
               RadioEngine.Explore.FakeActivitySource.SourceName,
               StringComparison.OrdinalIgnoreCase);

    /// <summary>True when an activity source is switched on.</summary>
    /// <param name="sourceName">Source name, e.g. "POTA".</param>
    /// <returns>True when the source should be polled.</returns>
    public bool IsSourceEnabled(string sourceName)
        => SourceEnabled.TryGetValue(sourceName, out var on)
            ? on
            : DefaultSourceEnabled(sourceName);

    /// <summary>Switch an activity source on or off.</summary>
    /// <param name="sourceName">Source name, e.g. "POTA".</param>
    /// <param name="enabled">True to poll it.</param>
    public void SetSourceEnabled(string sourceName, bool enabled)
        => SourceEnabled[sourceName] = enabled;

    /// <summary>Record a panel's expand/collapse state.</summary>
    /// <param name="panelKey">Stable panel id, e.g. "spots".</param>
    /// <param name="expanded">True when the panel is open.</param>
    public void SetPanelExpanded(string panelKey, bool expanded)
        => PanelExpanded[panelKey] = expanded;
}

/// <summary>Loads and saves <see cref="AppSettings"/>, and owns the paths
/// every other component asks for.</summary>
public static class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

        // Enums as names, not numbers. Two reasons, and the second is the
        // serious one. "LicenseClass": "General" is legible to somebody
        // reading their own settings file, where "LicenseClass": 3 is not.
        // And a person who hand-edits it to "General" gets what they meant —
        // without this converter that write throws, LoadFrom catches, and
        // EVERY setting silently reverts to defaults, which is a spectacular
        // punishment for a reasonable guess (HM-DEC-028 expects the callsign
        // and class to arrive from hand-edited files).
        // Reading still accepts the numeric form, so files written by earlier
        // builds load unchanged.
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>%AppData%\Hamlet — the one folder Hamlet writes to.</summary>
    public static string DataFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Hamlet");

    /// <summary>%AppData%\Hamlet\telemetry.</summary>
    public static string TelemetryFolder { get; } = Path.Combine(DataFolder, "telemetry");

    /// <summary>%AppData%\Hamlet\settings.json.</summary>
    public static string SettingsPath { get; } = Path.Combine(DataFolder, "settings.json");

    /// <summary>Load settings, or defaults if the file is missing, corrupt or
    /// unreadable. Never throws.</summary>
    public static AppSettings Load() => LoadFrom(SettingsPath);

    /// <summary>Save settings. Never throws; a failed save loses preferences,
    /// nothing more.</summary>
    public static void Save(AppSettings settings) => SaveTo(settings, SettingsPath);

    /// <summary>Load settings from an explicit path. The real load and the
    /// tested load are the same code (§5).</summary>
    /// <param name="path">Settings file path.</param>
    public static AppSettings LoadFrom(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, Options)
                           ?? new AppSettings();

            // Keys that have been renamed since the file was written are
            // carried forward here, so an upgrade never looks like the app
            // forgetting who the operator is (HM-DEC-035).
            SettingsMigrations.Apply(settings, json);

            return settings;
        }
        catch (Exception)
        {
            return new AppSettings();
        }
    }

    /// <summary>Save settings to an explicit path. Never throws.</summary>
    /// <param name="settings">Settings to write.</param>
    /// <param name="path">Destination file path.</param>
    public static void SaveTo(AppSettings settings, string path)
    {
        try
        {
            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, JsonSerializer.Serialize(settings, Options));
        }
        catch (Exception)
        {
            // Preferences are best-effort.
        }
    }

    /// <summary>Open the data folder in the OS file browser.</summary>
    public static void OpenDataFolder()
    {
        try
        {
            Directory.CreateDirectory(DataFolder);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = DataFolder,
                UseShellExecute = true,
            });
        }
        catch (Exception)
        {
            // Nothing to do if the shell refuses.
        }
    }
}

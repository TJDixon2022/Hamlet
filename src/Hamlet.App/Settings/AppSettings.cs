using System.Text.Json;
using System.Text.Json.Serialization;
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

    /// <summary>Telemetry category switches. Absent category means enabled —
    /// all categories default on (HM-DEC-018).</summary>
    public Dictionary<string, bool> TelemetryCategories { get; set; } = new();

    /// <summary>Telemetry folder size cap in megabytes.</summary>
    public int TelemetryMaxMegabytes { get; set; } = 50;

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

}

/// <summary>Loads and saves <see cref="AppSettings"/>, and owns the paths
/// every other component asks for.</summary>
public static class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
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
    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json, Options)
                   ?? new AppSettings();
        }
        catch (Exception)
        {
            return new AppSettings();
        }
    }

    /// <summary>Save settings. Never throws; a failed save loses preferences,
    /// nothing more.</summary>
    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(DataFolder);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, Options));
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

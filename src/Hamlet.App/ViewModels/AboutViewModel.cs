using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hamlet.App.Settings;
using Hamlet.RadioEngine.Telemetry;

namespace Hamlet.App.ViewModels;

/// <summary>
/// The About box (HM-DEC-019): what this app is, and — the part that earns
/// its keep — what build the operator is running and where its record lives.
/// </summary>
/// <remarks>
/// This is §0.0.1 meeting the user. "The app must be diagnosable" is only
/// half true if the diagnosis needs Tim at the keyboard; a stranger filing a
/// bug needs version, runtime, session id and telemetry state in one click.
/// The copied block deliberately omits the callsign — the operator profile is
/// displayed here, but a diagnostics paste ends up in a public issue tracker
/// (HM-DEC-018).
/// </remarks>
public partial class AboutViewModel : ObservableObject
{
    /// <summary>Where the source lives.</summary>
    public const string GitHubUrl = "https://github.com/TJDixon2022/Hamlet";

    private readonly JsonlTelemetry? _telemetry;

    [ObservableProperty]
    private string _copyStatus = "";

    /// <summary>Designer constructor.</summary>
    public AboutViewModel() : this(new AppSettings(), null)
    {
    }

    /// <summary>Runtime constructor.</summary>
    /// <param name="settings">Live settings, for the profile and the switches.</param>
    /// <param name="telemetry">The running writer, or null.</param>
    public AboutViewModel(AppSettings settings, JsonlTelemetry? telemetry)
    {
        _telemetry = telemetry;

        Version = ReadVersion();
        BuildDate = ReadBuildDate();
        RuntimeVersion = RuntimeInformation.FrameworkDescription;
        AvaloniaVersion = ReadAvaloniaVersion();
        OperatingSystem = RuntimeInformation.OSDescription;
        Byline = settings.Operator.Byline;
        SessionId = telemetry?.SessionId ?? "no telemetry this session";

        var enabled = settings.EnabledTelemetryCategoryCount;
        var total = Enum.GetValues<TelemetryCategory>().Length;
        var megabytes = (telemetry?.TotalBytes() ?? 0) / 1024.0 / 1024.0;
        TelemetryStatus = string.Create(CultureInfo.InvariantCulture,
            $"{enabled} of {total} categories on · {megabytes:0.00} MB on disk");

        DiagnosticsText = BuildDiagnostics(enabled, total, megabytes);
    }

    /// <summary>Assembly version, e.g. "0.1.0".</summary>
    public string Version { get; }

    /// <summary>When this build was produced, or "unknown" when the file
    /// timestamp cannot be read — never a plausible-looking guess.</summary>
    public string BuildDate { get; }

    /// <summary>The .NET runtime actually executing, read at run time.</summary>
    public string RuntimeVersion { get; }

    /// <summary>The Avalonia build actually loaded, read at run time.</summary>
    public string AvaloniaVersion { get; }

    /// <summary>The operating system, as the runtime describes it.</summary>
    public string OperatingSystem { get; }

    /// <summary>"by Tim, KC3QIS", or empty when the profile is empty.</summary>
    public string Byline { get; }

    /// <summary>True when there is a byline to show.</summary>
    public bool HasByline => Byline.Length > 0;

    /// <summary>The session id every telemetry line from this run carries.</summary>
    public string SessionId { get; }

    /// <summary>Categories on, and how much disk the record uses.</summary>
    public string TelemetryStatus { get; }

    /// <summary>Where everything Hamlet writes lives.</summary>
    public string DataFolderPath => SettingsStore.DataFolder;

    /// <summary>The plain-text block the copy button puts on the clipboard.
    /// Contains no callsign, name, location or grid (HM-DEC-019).</summary>
    public string DiagnosticsText { get; }

    /// <summary>Open the project on GitHub in the default browser.</summary>
    [RelayCommand]
    private void OpenGitHub()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = GitHubUrl,
                UseShellExecute = true,
            });
        }
        catch (Exception)
        {
            CopyStatus = "Could not open a browser — the address is " + GitHubUrl;
        }
    }

    /// <summary>Open %AppData%\Hamlet in the file browser.</summary>
    [RelayCommand]
    private void OpenDataFolder() => SettingsStore.OpenDataFolder();

    /// <summary>Called by the view once the clipboard write succeeded.</summary>
    public void ReportCopied()
    {
        CopyStatus = "Diagnostics copied — paste it into the bug report.";
        Telemetry.AppEvents.DiagnosticsCopied(_telemetry);
    }

    /// <summary>Called by the view when the clipboard is unavailable.</summary>
    public void ReportCopyFailed()
        => CopyStatus = "Clipboard unavailable — the block is shown above.";

    private string BuildDiagnostics(int enabled, int total, double megabytes)
    {
        var sb = new StringBuilder(320);
        sb.Append("Hamlet ").Append(Version)
          .Append(" (built ").Append(BuildDate).AppendLine(")");
        sb.Append("OS: ").AppendLine(OperatingSystem);
        sb.Append(".NET: ").AppendLine(RuntimeVersion);
        sb.Append("Avalonia: ").AppendLine(AvaloniaVersion);
        sb.Append("Session: ").AppendLine(SessionId);
        sb.Append("Data folder: ").AppendLine(DataFolderPath);
        sb.Append(string.Create(CultureInfo.InvariantCulture,
            $"Telemetry: {enabled} of {total} categories on, {megabytes:0.00} MB, "))
          .Append(_telemetry?.DroppedEventCount ?? 0).AppendLine(" events dropped");
        return sb.ToString();
    }

    private static string ReadVersion()
        => typeof(AboutViewModel).Assembly.GetName().Version?.ToString(3) ?? "unknown";

    private static string ReadBuildDate()
    {
        try
        {
            var location = typeof(AboutViewModel).Assembly.Location;
            if (string.IsNullOrEmpty(location) || !File.Exists(location))
            {
                return "unknown";
            }

            return File.GetLastWriteTime(location)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return "unknown";
        }
    }

    private static string ReadAvaloniaVersion()
    {
        var assembly = typeof(Avalonia.Application).Assembly;
        var informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
        {
            // Strip the "+<commit>" build metadata; the number is the useful part.
            var plus = informational.IndexOf('+');
            return plus > 0 ? informational[..plus] : informational;
        }

        return assembly.GetName().Version?.ToString(3) ?? "unknown";
    }
}

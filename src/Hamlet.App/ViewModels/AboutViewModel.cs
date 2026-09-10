using System.Globalization;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Hamlet.App.Telemetry;
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

        DiagnosticsText = BuildDiagnostics(enabled, total, megabytes, settings);
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
            CopyStatus = "Could not open a browser. The address is " + GitHubUrl;
        }
    }

    /// <summary>Open %AppData%\Hamlet in the file browser.</summary>
    [RelayCommand]
    private void OpenDataFolder() => SettingsStore.OpenDataFolder();

    /// <summary>Called by the view once the clipboard write succeeded.</summary>
    public void ReportCopied()
    {
        CopyStatus = "Diagnostics copied. Paste it into the bug report.";
        Telemetry.AppEvents.DiagnosticsCopied(_telemetry);
    }

    /// <summary>Called by the view when the clipboard is unavailable.</summary>
    public void ReportCopyFailed()
        => CopyStatus = "Clipboard unavailable, so the block is shown above.";

    /// <summary>How many recent lines of the stream the bundle carries.</summary>
    /// <remarks>
    /// <para>**FORTY, AND THE FIGURE IS A JUDGEMENT WRITTEN DOWN RATHER THAN BURIED**
    /// (work instruction 304 task 4, which asks how much was chosen and why).</para>
    /// <para>**IT IS THE RUN-UP TO A FAULT AND NOT A DAY'S HISTORY.** An FT8 slot
    /// produces on the order of a dozen events, so forty lines is roughly the last
    /// three or four slots - the minute before he noticed something was wrong, which
    /// is the window in which the cause is. **The whole day is already in the folder**
    /// and the bundle names the path to it; what the bundle exists for is the paste
    /// that needs no folder.</para>
    /// <para>**AND IT IS ONE PASTE** (the instruction). Forty lines of JSON is a few
    /// kilobytes, which a chat window takes without complaint; four hundred would be
    /// a file he has to attach, which is the thing this replaces.</para>
    /// </remarks>
    public const int RecentLines = 40;

    private string BuildDiagnostics(
        int enabled, int total, double megabytes, AppSettings settings)
    {
        var sb = new StringBuilder(4096);

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

        // **THE STATE, NOT ONLY THE VERSIONS** (work instruction 304 task 4). What
        // the button copied before this was enough to say which build he was on and
        // nothing whatever about what the machine was doing - and every one of the
        // three failures of 2026-09-10 was a state nobody could see.
        //
        // **IT IS THE SAME FACTS AND THE SAME WORDS AS THE STARTUP SNAPSHOT**, read
        // fresh here, so a reader comparing the two is comparing like with like and
        // an `unknown` means the same thing in both.
        sb.AppendLine();
        sb.AppendLine("--- state now ---");

        AppendState(sb, settings);

        // **AND THE RUN-UP TO THE FAULT.** A picture of now cannot say what led to
        // it, and the operator opens About *after* something has gone wrong.
        sb.AppendLine();
        sb.Append("--- last ").Append(RecentLines)
          .AppendLine(" telemetry lines ---");

        AppendRecent(sb);

        return sb.ToString();
    }

    /// <summary>The startup snapshot's own facts, read again right now.</summary>
    private void AppendState(StringBuilder sb, AppSettings settings)
    {
        try
        {
            var parts = StartupFacts.Gather(
                settings,
                categoriesOn: settings.IsTelemetryEnabled,
                telemetry: _telemetry);

            foreach (var (key, value) in StartupSnapshot.Compose(parts)
                .OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                sb.Append(key).Append(": ").AppendLine(value?.ToString() ?? "");
            }
        }
        catch (Exception error)
        {
            // **THE BUNDLE IS FOR A BROKEN MACHINE, SO IT SAYS WHEN IT BROKE.**
            sb.Append("state could not be gathered: ")
              .Append(error.GetType().Name).Append(": ")
              .AppendLine(error.Message);
        }
    }

    /// <summary>The tail of today's telemetry, or why there is none.</summary>
    private void AppendRecent(StringBuilder sb)
    {
        try
        {
            var newest = Directory
                .GetFiles(SettingsStore.TelemetryFolder, "*.jsonl")
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();

            if (newest is null)
            {
                sb.AppendLine("(no telemetry file yet)");

                return;
            }

            // **READ SHARED, BECAUSE THE WRITER STILL HAS IT OPEN.** A bundle that
            // threw while the application was running would be worthless exactly
            // when it is wanted.
            using var stream = new FileStream(
                newest, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using var reader = new StreamReader(stream);

            var tail = new Queue<string>(RecentLines);

            while (reader.ReadLine() is { } line)
            {
                if (tail.Count == RecentLines)
                {
                    tail.Dequeue();
                }

                tail.Enqueue(line);
            }

            foreach (var line in tail)
            {
                sb.AppendLine(line);
            }
        }
        catch (Exception error)
        {
            sb.Append("(the telemetry could not be read: ")
              .Append(error.GetType().Name).Append(": ")
              .Append(error.Message).AppendLine(")");
        }
    }

    /// <summary>
    /// The app version, for anything that needs it without building an About
    /// box — chiefly the User-Agent Hamlet introduces itself with to POTA,
    /// SOTA and RBN (HM-DEC-024).
    /// </summary>
    public static string AppVersion => ReadVersion();

    private static string ReadVersion()
        => typeof(AboutViewModel).Assembly.GetName().Version?.ToString(3) ?? "unknown";

    /// <summary>
    /// When this assembly was compiled, stamped at compile time (HM-DEC-079).
    /// </summary>
    /// <remarks>
    /// <para>THIS USED TO READ THE ASSEMBLY FILE'S LAST-WRITE TIME, which is a
    /// property of a file copy rather than of a build. An incremental build that
    /// did not recompile the shell, or a copy that preserved timestamps, made it
    /// report a day with nothing to do with the code running: it showed
    /// 2026-08-14 while running code built the next day.</para>
    /// <para>A build date that can be stale is worse than none, because it is
    /// the row somebody reads to check that two machines are running the same
    /// code. This one is written into the assembly by the compilation itself.
    /// When it is missing it says so, rather than falling back to a timestamp
    /// that lies (§0.0).</para>
    /// </remarks>
    private static string ReadBuildDate()
    {
        try
        {
            var stamp = typeof(AboutViewModel).Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => string.Equals(
                    a.Key, "BuildStampUtc", StringComparison.Ordinal))?
                .Value;

            return string.IsNullOrWhiteSpace(stamp) ? "unknown" : $"{stamp} UTC";
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

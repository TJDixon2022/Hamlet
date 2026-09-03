using System.Runtime.CompilerServices;
using Hamlet.App.Settings;
using Hamlet.App.ViewModels;
using Hamlet.RadioEngine.Explore;
using Xunit;

namespace Hamlet.App.Tests;

/// <summary>
/// Points every path Hamlet writes to at a temporary folder, once, before any
/// test in this assembly runs.
/// </summary>
/// <remarks>
/// <para>**UNIT 235 MEASURED WHY THIS EXISTS.** Nine tests of one class, run
/// alone with a hash-level snapshot of the operator's own
/// <c>%AppData%\Hamlet</c> either side, rewrote his <c>settings.json</c>
/// (1200 to 1352 bytes, a different SHA-256) and touched his <c>spots.db</c>.
/// Nothing in the test bodies asked for that. It is the constructor:
/// <c>MainWindowViewModel</c> opens the spot store, saves a byline index and
/// starts a callsign lookup whose answer it also saves, all before a test body
/// executes. Twenty files in this project construct one, thirty-nine times.</para>
/// <para>**IT MATTERS BECAUSE OF WHEN THE PLAN SAYS TO RUN THIS PROJECT.** The
/// full suite is run by hand, once, immediately before the operator sits down at
/// the radio. A suite that rewrites the settings that session depends on makes
/// the session unrepeatable.</para>
/// <para>A module initializer rather than a fixture, because a fixture only
/// binds the classes that ask for it and the reach into his folder is through a
/// constructor that every one of those twenty files calls.</para>
/// </remarks>
internal static class TheOperatorsFolderGuard
{
    /// <summary>The temporary folder this run writes to instead.</summary>
    internal static string Folder { get; private set; } = "";

    /// <summary>The operator's real folder, kept only so a test can assert it
    /// is not the one in use.</summary>
    internal static string TheOperatorsRealFolder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Hamlet");

    /// <summary>Redirect before the first test constructs anything.</summary>
    [ModuleInitializer]
    internal static void PointEverythingSomewhereElse()
    {
        Folder = Path.Combine(
            Path.GetTempPath(),
            "hamlet-app-tests-" + Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture));

        Directory.CreateDirectory(Folder);
        SettingsStore.DataFolder = Folder;

        // CaptureFolder captures DataFolder at its own static initialisation,
        // which may already have happened. Set it explicitly rather than relying
        // on the order (HM-DEC-009: a guard that depends on luck is not a guard).
        MainWindowViewModel.CaptureFolder = Path.Combine(Folder, "captures");
    }
}

/// <summary>
/// The operator's own data folder is unreachable from this project.
/// </summary>
/// <remarks>
/// **BOTH OF THESE WOULD HAVE FAILED ON THE MORNING OF 2026-09-03**, which is
/// the only reason they are worth committing.
/// </remarks>
public class TheOperatorsFolderIsNotOursTests
{
    /// <summary>Every path composed from the data folder points at the
    /// temporary one, and none of them at his.</summary>
    [Fact]
    public void NoPathHamletWritesToIsTheOperatorsDuringATestRun()
    {
        var his = TheOperatorsFolderGuard.TheOperatorsRealFolder;
        var ours = TheOperatorsFolderGuard.Folder;

        Assert.NotEqual("", ours);
        Assert.NotEqual(his, SettingsStore.DataFolder);

        foreach (var path in new[]
                 {
                     SettingsStore.DataFolder,
                     SettingsStore.SettingsPath,
                     SettingsStore.TelemetryFolder,
                     SettingsStore.ScanSegmentsPath,
                     SettingsStore.ScanHomePath,
                     MainWindowViewModel.CaptureFolder,
                 })
        {
            Assert.StartsWith(ours, path, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(his, path, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>Constructing the view model writes its settings and its spot
    /// store into whatever folder the seam names, and the seam goes back.</summary>
    [Fact]
    public void ConstructingTheViewModelWritesIntoTheFolderTheSeamNames()
    {
        var restoreTo = SettingsStore.DataFolder;
        var scratch = Path.Combine(
            Path.GetTempPath(), "hamlet-seam-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratch);

        try
        {
            // An empty folder before, so a file found afterwards was put there
            // by the construction and not by something earlier.
            Assert.Empty(Directory.GetFiles(scratch));

            SettingsStore.DataFolder = scratch;
            _ = new MainWindowViewModel(new AppSettings(), null);

            Assert.True(
                File.Exists(Path.Combine(scratch, "settings.json")),
                "constructing the view model should have written settings.json into the folder the seam names");

            Assert.True(
                File.Exists(Path.Combine(scratch, SqliteSpotStore.FileName)),
                "constructing the view model should have opened the spot store in the folder the seam names");
        }
        finally
        {
            SettingsStore.DataFolder = restoreTo;
        }

        Assert.Equal(restoreTo, SettingsStore.DataFolder);
        Assert.NotEqual(TheOperatorsFolderGuard.TheOperatorsRealFolder, SettingsStore.DataFolder);
    }
}

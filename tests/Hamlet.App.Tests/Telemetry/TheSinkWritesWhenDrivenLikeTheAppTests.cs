using System.Diagnostics;
using System.Text.Json;
using Hamlet.App.Settings;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Telemetry;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// Drives <see cref="JsonlTelemetry"/> with the four arguments
/// <c>App.axaml.cs:39-43</c> passes, pointed at a temporary folder, and watches
/// it write.
/// </summary>
/// <remarks>
/// <para>**A `Write` CALL RETURNING IS NOT EVIDENCE A LINE WAS WRITTEN**
/// (`CLAUDE.md` §0.0). `JsonlTelemetry.Write` swallows every exception and
/// returns void, and the append happens on a background thread. So every
/// assertion here is a file on disk with a line in it, and nothing less.</para>
/// <para>**WHY THIS EXISTS AT ALL.** On 2026-09-03 Hamlet ran on the owner's
/// machine — `settings.json` was rewritten at 16:35:55Z — and left no
/// `2026-09-03.jsonl` behind, although `App.axaml.cs` writes `app_start`
/// unconditionally at startup and an absent category key reads as enabled. Unit
/// 234 was sent to find out whether the sink writes when driven this way.</para>
/// <para>**IT IS NOT A SECOND COPY OF `TelemetryTests`.** That class proves the
/// daily-file naming, the eviction order and the never-throw discipline against a
/// hand-written predicate. Nothing there constructs the sink from an
/// <see cref="AppSettings"/>, goes through <see cref="AppEvents"/>, reads a line
/// back as JSON, or measures anything without `Dispose` — and the bench case is
/// precisely a process that never disposes.</para>
/// </remarks>
public sealed class TheSinkWritesWhenDrivenLikeTheAppTests : IDisposable
{
    private readonly string _folder = Path.Combine(
        Path.GetTempPath(), "hamlet-unit234-" + Guid.NewGuid().ToString("N")[..8]);

    private readonly ITestOutputHelper _output;

    public TheSinkWritesWhenDrivenLikeTheAppTests(ITestOutputHelper output)
        => _output = output;

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (Exception)
        {
            // Test cleanup only.
        }
    }

    /// <summary>The byte cap the shell computes, from the same setting.</summary>
    private static long CapBytes(AppSettings settings)
        => settings.TelemetryMaxMegabytes * 1024L * 1024L;

    /// <remarks>Proves: the branch `settings.json`'s empty <c>{}</c> takes reads
    /// as ON. This is the assumption the whole silent-morning question rests on,
    /// so it is asserted directly rather than inferred from a written line.</remarks>
    [Fact]
    public void ADefaultAppSettingsEnablesDiagnostics()
    {
        var settings = new AppSettings();

        Assert.Empty(settings.TelemetryCategories);
        Assert.True(settings.IsTelemetryEnabled(TelemetryCategory.Diagnostics));
        Assert.Equal(50, settings.TelemetryMaxMegabytes);
    }

    /// <remarks>Proves: constructed exactly as `App.axaml.cs:39-43` constructs it
    /// and driven through `AppEvents.AppStart`, the sink puts today's dated file
    /// on disk holding one `app_start` line with the version that was passed in,
    /// and drops nothing.</remarks>
    [Fact]
    public void DrivenLikeTheApp_AppStartLandsOnDiskAsOneLine()
    {
        var settings = new AppSettings();

        using var telemetry = new JsonlTelemetry(
            _folder,
            "1.12.38",
            category => settings.IsTelemetryEnabled(category),
            CapBytes(settings));

        AppEvents.AppStart(telemetry);

        var expected = Path.Combine(
            _folder, DateTime.UtcNow.ToString("yyyy-MM-dd") + ".jsonl");
        Assert.True(WaitForFile(expected, TimeSpan.FromSeconds(5)),
            "app_start never reached disk.");

        var lines = File.ReadAllLines(expected).Where(l => l.Length > 0).ToArray();
        Assert.Single(lines);

        using var doc = JsonDocument.Parse(lines[0]);
        var root = doc.RootElement;
        Assert.Equal("app_start", root.GetProperty("event").GetString());
        Assert.Equal("diagnostics", root.GetProperty("category").GetString());
        Assert.Equal("1.12.38", root.GetProperty("appVersion").GetString());

        Assert.Equal(0, telemetry.DroppedEventCount);

        _output.WriteLine("MEASURED file: " + Path.GetFileName(expected));
        _output.WriteLine("MEASURED line: " + lines[0]);
        _output.WriteLine("MEASURED dropped: " + telemetry.DroppedEventCount);
    }

    /// <remarks>
    /// <para>Proves: the line reaches disk WITHOUT `Dispose` ever being called,
    /// and measures how long that takes.</para>
    /// <para>**THIS IS THE BENCH CASE.** An application killed from the task
    /// manager, or closed in a way that never reaches `ShutdownRequested`, never
    /// disposes the sink. If the only thing that flushes the queue were
    /// `Dispose`, a morning at the radio could produce a completely empty record
    /// with nothing wrong anywhere else — and this test would be the place that
    /// said so.</para>
    /// </remarks>
    [Fact]
    public void WithoutDispose_TheLineStillReachesDisk()
    {
        var settings = new AppSettings();

        // Deliberately NOT in a `using`: the point is that nothing tidies up.
        var telemetry = new JsonlTelemetry(
            _folder,
            "1.12.38",
            category => settings.IsTelemetryEnabled(category),
            CapBytes(settings));

        var expected = Path.Combine(
            _folder, DateTime.UtcNow.ToString("yyyy-MM-dd") + ".jsonl");

        var clock = Stopwatch.StartNew();
        AppEvents.AppStart(telemetry);
        var landed = WaitForFile(expected, TimeSpan.FromSeconds(5));
        clock.Stop();

        Assert.True(landed,
            $"app_start did not reach disk within {clock.ElapsedMilliseconds} ms "
            + "with no Dispose called.");

        // The writer thread's own latency, which is what a killed process has
        // to outlive for its record to exist.
        _output.WriteLine(
            "MEASURED elapsed to disk with no Dispose: "
            + clock.ElapsedMilliseconds + " ms");

        Assert.Contains("app_start", File.ReadAllText(expected));
        Assert.Equal(0, telemetry.DroppedEventCount);
    }

    /// <remarks>Proves: the category guard refuses. A guard that has never been
    /// watched refusing is not a guard — step 1's boundary reasoning. With
    /// Diagnostics switched off through the same `AppSettings` the shell uses,
    /// there is no line AND no file at all.</remarks>
    [Fact]
    public void WithTheCategoryOff_NothingIsWrittenAndNoFileAppears()
    {
        var settings = new AppSettings();
        settings.SetTelemetryEnabled(TelemetryCategory.Diagnostics, false);

        using (var telemetry = new JsonlTelemetry(
            _folder,
            "1.12.38",
            category => settings.IsTelemetryEnabled(category),
            CapBytes(settings)))
        {
            AppEvents.AppStart(telemetry);
            Thread.Sleep(300);

            Assert.Equal(0, telemetry.DroppedEventCount);
        }

        Assert.Empty(Directory.GetFiles(_folder, "*.jsonl"));
    }

    /// <summary>Poll for a non-empty file. Bounded, and never a bare sleep.</summary>
    private static bool WaitForFile(string path, TimeSpan limit)
    {
        var deadline = DateTime.UtcNow + limit;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                if (File.Exists(path) && new FileInfo(path).Length > 0)
                {
                    return true;
                }
            }
            catch (IOException)
            {
                // Mid-append; try again.
            }

            Thread.Sleep(5);
        }

        return false;
    }
}

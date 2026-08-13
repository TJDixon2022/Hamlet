using Hamlet.RadioEngine.Telemetry;
using Xunit;

namespace Hamlet.RadioEngine.Tests.Telemetry;

public sealed class TelemetryTests : IDisposable
{
    private readonly string _folder =
        Path.Combine(Path.GetTempPath(), "hamlet-telemetry-" + Guid.NewGuid().ToString("N")[..8]);

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

    /// <remarks>Proves: a line carries exactly the agreed schema B fields and
    /// no machine identifier — the privacy commitment in HM-DEC-018 is
    /// enforced by a test, not by good intentions.</remarks>
    [Fact]
    public void Serialize_MatchesSchemaAndCarriesNoMachineId()
    {
        var line = JsonlTelemetry.Serialize(new TelemetryEvent(
            new DateTime(2026, 8, 13, 12, 26, 4, 118, DateTimeKind.Utc),
            "a3f2", TelemetryLevel.Info, "0.1.0",
            TelemetryCategory.Rig, "connect_failed",
            new Dictionary<string, object?>
            {
                ["port"] = "COM4",
                ["rigType"] = "IC-7300",
                ["reason"] = "timeout",
            }));

        Assert.Contains("\"ts\":\"2026-08-13T12:26:04.118Z\"", line);
        Assert.Contains("\"sessionId\":\"a3f2\"", line);
        Assert.Contains("\"level\":\"info\"", line);
        Assert.Contains("\"appVersion\":\"0.1.0\"", line);
        Assert.Contains("\"category\":\"rig\"", line);
        Assert.Contains("\"event\":\"connect_failed\"", line);
        Assert.Contains("\"rigType\":\"IC-7300\"", line);

        Assert.DoesNotContain("machineId", line);
        Assert.DoesNotContain("callsign", line);
    }

    /// <remarks>Proves: a disabled category writes nothing while an enabled
    /// one writes — the Settings switches actually gate the disk.</remarks>
    [Fact]
    public void DisabledCategory_WritesNothing()
    {
        using (var telemetry = new JsonlTelemetry(_folder, "0.1.0",
            c => c != TelemetryCategory.Explore))
        {
            telemetry.Write(TelemetryCategory.Explore, "neighborhood_clicked");
            telemetry.Write(TelemetryCategory.Rig, "connect_ok");
            Thread.Sleep(250);
        }

        var text = string.Concat(Directory.GetFiles(_folder, "*.jsonl").Select(File.ReadAllText));
        Assert.DoesNotContain("neighborhood_clicked", text);
        Assert.Contains("connect_ok", text);
    }

    /// <remarks>Proves: events land in a daily file named for the UTC date,
    /// one JSON object per line.</remarks>
    [Fact]
    public void Events_LandInDailyFileOnePerLine()
    {
        using (var telemetry = new JsonlTelemetry(_folder, "0.1.0", _ => true))
        {
            telemetry.Write(TelemetryCategory.Diagnostics, "app_start");
            telemetry.Write(TelemetryCategory.Tuning, "band_changed",
                new Dictionary<string, object?> { ["band"] = "40 m" });
            Thread.Sleep(250);
        }

        var expected = Path.Combine(_folder,
            DateTime.UtcNow.ToString("yyyy-MM-dd") + ".jsonl");
        Assert.True(File.Exists(expected));

        var lines = File.ReadAllLines(expected).Where(l => l.Length > 0).ToArray();
        Assert.Equal(2, lines.Length);
        Assert.All(lines, l => Assert.StartsWith("{", l));
        Assert.All(lines, l => Assert.EndsWith("}", l));
    }

    /// <remarks>Proves: the size cap evicts oldest daily files first and
    /// never deletes the file currently being written.</remarks>
    [Fact]
    public void SizeCap_EvictsOldestFirst()
    {
        Directory.CreateDirectory(_folder);
        var old1 = Path.Combine(_folder, "2020-01-01.jsonl");
        var old2 = Path.Combine(_folder, "2020-01-02.jsonl");
        File.WriteAllText(old1, new string('x', 4000));
        File.WriteAllText(old2, new string('x', 4000));

        using (var telemetry = new JsonlTelemetry(_folder, "0.1.0", _ => true,
            maxTotalBytes: 5000))
        {
            telemetry.Write(TelemetryCategory.Diagnostics, "app_start");
            Thread.Sleep(300);
        }

        Assert.False(File.Exists(old1));
        Assert.True(File.Exists(Path.Combine(_folder,
            DateTime.UtcNow.ToString("yyyy-MM-dd") + ".jsonl")));
    }

    /// <remarks>Proves: an unwritable folder degrades to counting drops
    /// instead of throwing — never-throw discipline (§8).</remarks>
    [Fact]
    public void UnwritableFolder_DropsInsteadOfThrowing()
    {
        var badPath = Path.Combine(_folder, "file-not-folder");
        Directory.CreateDirectory(_folder);
        File.WriteAllText(badPath, "occupied");

        using var telemetry = new JsonlTelemetry(badPath, "0.1.0", _ => true);
        telemetry.Write(TelemetryCategory.Diagnostics, "app_start");
        Thread.Sleep(250);

        Assert.True(telemetry.DroppedEventCount > 0);
    }

    /// <remarks>Proves: ClearAll removes the files Settings promises to
    /// clear.</remarks>
    [Fact]
    public void ClearAll_RemovesEveryFile()
    {
        using var telemetry = new JsonlTelemetry(_folder, "0.1.0", _ => true);
        telemetry.Write(TelemetryCategory.Diagnostics, "app_start");
        Thread.Sleep(250);

        Assert.True(telemetry.ClearAll() >= 1);
        Assert.Empty(Directory.GetFiles(_folder, "*.jsonl"));
    }
}

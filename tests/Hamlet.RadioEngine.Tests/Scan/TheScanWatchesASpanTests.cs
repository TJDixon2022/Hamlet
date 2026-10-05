using Hamlet.RadioEngine.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **THE SCAN WATCHES A SPAN, VISITS WHAT IT SAW, THEN MOVES ON** (work instruction 543, task 2, HM-DEC-247), on the fake
/// centre-mode scope at the radio's own four and a half sweeps a second, with band noise for audio.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-05:** *"It seems like we should advance, scan the waterfall for maybe two, three seconds, see if
/// there's any station, then lock into that station and try it."*</para>
/// <para>These tests read only the catches and the tunes, so the same tests compile against the scan before this unit, where
/// they fail.</para>
/// </remarks>
public sealed class TheScanWatchesASpanTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-scan-tests", Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the scan's path is printed.</param>
    public TheScanWatchesASpanTests(ITestOutputHelper output) => _output = output;

    /// <inheritdoc/>
    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private static CwScanSettings Seconds(double length) => new(TimeSpan.FromSeconds(length), TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(30));

    private void Print(SpanWorld world, CwScanSummary summary)
    {
        _output.WriteLine($"tunes: {string.Join(", ", world.Tunes.Select(t => $"{t.Hz / 1e6:0.0000}@{t.At:0.0}"))}");

        foreach (var c in summary.Catches)
        {
            _output.WriteLine($"catch {c.StartUtc:HH:mm:ss.f} at {c.SignalHz / 1e6:0.0000} MHz, {c.Kind}, left {c.Left}");
        }

        foreach (var s in summary.Surveys ?? [])
        {
            _output.WriteLine($"survey {s.Index}: {s.LowHz / 1e6:0.000}-{s.HighHz / 1e6:0.000}, {s.Sweeps} sweeps, {s.Listed} listed; "
                + string.Join("; ", s.Considered.Where(c => c.Seen > 1).Select(c => $"{c.FrequencyHz / 1e6:0.0000} {c.Verdict} ({c.Why})")));
        }
    }

    /// <remarks>
    /// The span of task 1: the strong station and both moderate ones are each visited, in frequency order, from the first
    /// survey of the span, before the scan moves on.
    /// </remarks>
    [Fact]
    public async Task AllThreeClearStationsAreVisited()
    {
        using var world = await SpanWorld.Ready(WhyClearStationsAreSkippedTests.Home, WhyClearStationsAreSkippedTests.ThreeClear);
        var (_, summary) = await world.Run(_folder, Seconds(40));

        Print(world, summary);

        var visited = summary.Catches.Where(c => c.SignalHz is > 7_000_000 and < 7_020_000).Take(3).ToList();

        Assert.Equal(3, visited.Count);

        for (var i = 0; i < 3; i++)
        {
            Assert.InRange(visited[i].SignalHz, WhyClearStationsAreSkippedTests.ThreeClear[i].Hz - 100, WhyClearStationsAreSkippedTests.ThreeClear[i].Hz + 100);
        }
    }

    /// <remarks>
    /// A span of noise blips: nothing is visited, and the scan advances to the next span once the survey time is up, the
    /// settle and three seconds after it tuned there.
    /// </remarks>
    [Fact]
    public async Task ASpanOfBlipsIsLeftAfterItsSurvey()
    {
        using var world = await SpanWorld.Ready(WhyClearStationsAreSkippedTests.Home, []);
        var (_, summary) = await world.Run(_folder, Seconds(20));

        Print(world, summary);

        Assert.Empty(summary.Catches);

        var first = world.Tunes[0];
        var second = world.Tunes[1];

        Assert.Equal(7_010_000, first.Hz);
        Assert.Equal(7_030_000, second.Hz);
        Assert.InRange(second.At - first.At, 3.4, 4.0);
    }

    /// <remarks>
    /// A station keyed down in half the sweeps, as CW is, is visited; one keyed down in one sweep of fifteen, which shows in a sweep
    /// now and then like a blip, is not. At two sweeps in five and this seed the first showed in 3 of 13 and was skipped as
    /// not repeating, which is the rule as written: it needs a quarter of the sweeps watched.
    /// </remarks>
    [Fact]
    public async Task AStationKeyedPartOfTheTimeIsVisitedIfItRepeatsEnough()
    {
        ScopeStation[] stations =
        [
            new("part-time", 7_006_000, [30, 18, 6], 0.5),
            new("rare", 7_014_000, [30, 18, 6], 0.07),
        ];

        using var world = await SpanWorld.Ready(WhyClearStationsAreSkippedTests.Home, stations, seed: 5432);
        var (_, summary) = await world.Run(_folder, Seconds(20));

        Print(world, summary);

        Assert.Contains(summary.Catches, c => Math.Abs(c.SignalHz - 7_006_000) < 100);
        Assert.DoesNotContain(summary.Catches, c => Math.Abs(c.SignalHz - 7_014_000) < 300);
    }
}

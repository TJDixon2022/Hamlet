using Hamlet.RadioEngine.Scan;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **WHY CLEAR STATIONS ARE SKIPPED** (work instruction 543, task 1, HM-DEC-247): the scan driven on a fake scope at the
/// radio's own sweep rate, about four and a half a second, with one span of ±10 kHz holding three clear stations, and
/// every peak the rule considered traced sweep by sweep with why it was kept or refused.
/// </summary>
/// <remarks>
/// <para>**THE OWNER, 2026-10-05:** *"I'm seeing very clear stations within a waterfall that never get tuned to."*</para>
/// <para>**THE THREE STATIONS**, drawn as the scope draws a keyed CW signal: a core with skirts either side, up in a little
/// over half the sweeps. The skirts are the author's, from the scope's own resolution: at ±10 kHz a bin is about 42 Hz,
/// narrower than what the scope resolves, so a signal spreads over neighbouring bins, and the stronger it is the further
/// its skirts stand over a floor the radio clips to nought.</para>
/// <list type="bullet">
/// <item>strong, 7.0043 MHz: 120 at its core, then 90, 50, 25, 10 and 4, so eleven bins over nought at its base;</item>
/// <item>moderate, 7.0091 MHz: 30, then 18 and 6, five bins at its base;</item>
/// <item>moderate, 7.0150 MHz: 24, then 14 and 5, five bins at its base.</item>
/// </list>
/// <para>No recording and no scan catch is read; the audio is band noise only.</para>
/// </remarks>
public sealed class WhyClearStationsAreSkippedTests : IDisposable
{
    internal const long Home = 7_031_000;

    internal static readonly ScopeStation[] ThreeClear =
    [
        new("strong", 7_004_300, [120, 90, 50, 25, 10, 4], 0.55),
        new("moderate", 7_009_100, [30, 18, 6], 0.55),
        new("moderate", 7_015_000, [24, 14, 5], 0.55),
    ];

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-scan-tests", Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhyClearStationsAreSkippedTests(ITestOutputHelper output) => _output = output;

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

    /// <remarks>
    /// Task 1: the scan, as it stands, over the span holding the three stations. The trace prints the sweeps it held the
    /// span for, each station's level and width over the rule's line in every sweep it was up, the sweeps it stood in
    /// narrow, and the rule's verdict; the rule's own peaks over the same sweeps are printed beside it to show the trace
    /// reads the rule as it is.
    /// </remarks>
    [Fact]
    public async Task TheTraceOfOneSpan()
    {
        using var world = await SpanWorld.Ready(Home, ThreeClear);
        var (_, summary) = await world.Run(_folder, new CwScanSettings(TimeSpan.FromSeconds(25), TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(30)));

        // The first survey hold: the tune to the first span, the settle, and the hold.
        var tune = world.Tunes.First(t => Math.Abs(t.Hz - 7_010_000) < 1_000);
        var from = tune.At + CwCatchScan.Settle.TotalSeconds;
        var until = from + CwCatchScan.SurveyHold.TotalSeconds;
        var held = world.Sweeps.Where(s => s.At > from && s.At <= until && s.Low == tune.Hz - SpanWorld.HalfSpanHz).ToList();
        var next = world.Tunes.FirstOrDefault(t => t.At > tune.At);

        _output.WriteLine($"span {held[0].Low / 1e6:0.000}-{held[0].High / 1e6:0.000} MHz, tuned at {tune.At:0.00} s, held {from:0.00} to {until:0.00} s; {held.Count} sweeps watched; next tune at {next.At:0.00} s to {next.Hz / 1e6:0.0000} MHz");

        var trace = Trace(held.Select(h => h.Bins).ToList(), held[0].Low, held[0].High, ThreeClear);

        foreach (var line in trace.Lines)
        {
            _output.WriteLine(line);
        }

        var watch = new ScopeWatch();

        foreach (var h in held)
        {
            watch.Add(h.Low, h.High, h.Bins);
        }

        var rule = watch.Peaks(held[0].Low, held[0].High);

        _output.WriteLine($"the rule's own peaks over these sweeps: {(rule.Count == 0 ? "none" : string.Join(", ", rule.Select(p => $"{p.FrequencyHz / 1e6:0.0000} MHz")))}");
        _output.WriteLine($"catches in {summary.LengthMinutes * 60:0} s of scanning: {summary.Catches.Count}; tunes: {string.Join(", ", world.Tunes.Select(t => $"{t.Hz / 1e6:0.000}@{t.At:0.0}"))}");

        Assert.Equal(trace.Kept, rule.Count);
        Assert.InRange(held.Count, 8, 10);
        Assert.DoesNotContain(summary.Catches, c => c.SignalHz is > 7_000_000 and < 7_020_000);
    }

    /// <summary>The trace: each station, sweep by sweep, against the rule as it stands.</summary>
    internal static (List<string> Lines, int Kept) Trace(List<byte[]> sweeps, long low, long high, IReadOnlyList<ScopeStation> stations)
    {
        var lines = new List<string>();
        var count = sweeps[0].Length;
        var all = sweeps.SelectMany(s => s).Select(b => (double)b).OrderBy(b => b).ToArray();
        var floor = all[all.Length / 2];
        var line = floor > 0 ? floor + 6 : floor + 1;
        var least = Math.Max(ScopeWatch.LeastRepeats, (int)Math.Ceiling(ScopeWatch.RepeatShare * sweeps.Count));
        var kept = 0;
        var binHz = (high - low) / (double)count;

        lines.Add($"floor {floor} (median of every bin), line {line}; a peak must stand in {least} of {sweeps.Count} sweeps and be {ScopeWatch.WidestBins} bins wide or less");

        foreach (var station in stations)
        {
            var centre = (int)Math.Floor((station.Hz - low) / binHz);
            var widths = new List<string>();
            var narrow = 0;
            var up = 0;

            for (var s = 0; s < sweeps.Count; s++)
            {
                var bins = sweeps[s];

                if (bins[centre] < line)
                {
                    widths.Add("-");
                    continue;
                }

                up++;

                var a = centre;
                var b = centre;

                while (a > 0 && bins[a - 1] >= line)
                {
                    a--;
                }

                while (b < count - 1 && bins[b + 1] >= line)
                {
                    b++;
                }

                var width = b - a + 1;

                widths.Add($"{bins[centre]}/{width}");
                narrow += width <= ScopeWatch.WidestBins ? 1 : 0;
            }

            var verdict = narrow >= least
                ? "kept"
                : up >= least
                    ? $"refused: too wide - {up} sweeps up, {narrow} of them {ScopeWatch.WidestBins} bins or narrower at the line"
                    : $"refused: not repeating - up in {up} of {sweeps.Count}";

            kept += narrow >= least ? 1 : 0;
            lines.Add($"{station.Name} {station.Hz / 1e6:0.0000} MHz, sweep by sweep (level/width in bins, - = down): {string.Join(" ", widths)} -> {verdict}");
        }

        // The blips: every narrow run that is no station, and how often its place came back.
        var blips = new Dictionary<int, int>();

        foreach (var bins in sweeps)
        {
            for (var i = 0; i < count; i++)
            {
                if (bins[i] >= line && stations.All(st => Math.Abs(i - (int)Math.Floor((st.Hz - low) / binHz)) > 8))
                {
                    blips[i] = blips.GetValueOrDefault(i) + 1;
                }
            }
        }

        lines.Add($"noise blips: {blips.Values.Sum()} over {blips.Count} places, the most any place came back {(blips.Count == 0 ? 0 : blips.Values.Max())} -> refused: not repeating");

        return (lines, kept);
    }
}

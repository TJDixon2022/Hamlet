using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

public sealed class ZzAgree
{
    private readonly ITestOutputHelper _o;
    public ZzAgree(ITestOutputHelper o) => _o = o;

    internal sealed record Fit(double UnitMs, double Residual);

    /// Unit 1.11.19's statistic, unchanged.
    internal static Fit Best(IReadOnlyList<double> runs)
    {
        if (runs.Count < 6)
        {
            return new Fit(double.NaN, double.NaN);
        }

        var best = new Fit(double.NaN, double.PositiveInfinity);

        for (var unit = CwToneSurvey.ShortestDitMs; unit <= 150; unit += 0.5)
        {
            var total = 0.0;

            foreach (var r in runs)
            {
                var k = Math.Min(9, Math.Max(1, (int)Math.Round(r / unit)));

                total += Math.Abs(r - (k * unit)) / unit;
            }

            var residual = total / runs.Count;

            if (residual < best.Residual)
            {
                best = new Fit(unit, residual);
            }
        }

        return best;
    }

    private static readonly (string Name, double Pitch, string Holds)[] Files =
    {
        ("unadjudicated/cw-2026-08-25-012823", 500, "a station"),
        ("unadjudicated/cw-2026-08-22-014113", 600, "a station"),
        ("unadjudicated/cw-2026-08-22-014308", 625, "a station"),
        ("unadjudicated/cw-2026-08-26-125941", 400, "a station"),
        ("cw-2026-08-17-013347", 600, "VA3VRR"),
        ("cw-2026-08-17-134712", 500, "N4L"),
        ("unadjudicated/cw-2026-08-24-012403", 600, "DE KD0UN"),
        ("cw-2026-08-18-004507", 501, "the bulletin"),
        ("unadjudicated/cw-2026-08-20-014854", 600, "**NOTHING**"),
        ("unadjudicated/cw-2026-08-20-014935", 825, "**NOTHING**"),
    };

    [Fact]
    public void DoTheGoodPassesAgreeOnAUnit()
    {
        _o.WriteLine("| capture | holds | | good passes | units fitted (ms) "
            + "| median | **spread (CV)** |");
        _o.WriteLine("|---|---|---|---|---|---|---|");

        foreach (var (name, pitch, holds) in Files)
        {
            var audio = WavAudio.Read(
                Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

            var decoder = new CwDecoder(audio.SampleRate, 600);
            var runs = new List<BinRuns>();

            decoder.Tracker.SurveyRunStreams = runs;

            var hop = decoder.Tracker.HopSamples;

            for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
            {
                decoder.Process(new AudioChunk(
                    at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            }

            decoder.Flush();

            void Row(string what, List<BinRuns> which)
            {
                var good = which
                    .Select(r => Best(r.MarksMs.Concat(r.GapsMs).ToList()))
                    .Where(f => !double.IsNaN(f.Residual) && f.Residual < 0.20)
                    .Select(f => f.UnitMs)
                    .ToList();

                if (good.Count < 2)
                {
                    _o.WriteLine($"| `{Path.GetFileName(name)}` | {holds} | {what} "
                        + $"| {good.Count} | too few to compare | | |");
                    return;
                }

                var mean = good.Average();
                var sd = Math.Sqrt(good.Sum(u => (u - mean) * (u - mean)) / good.Count);
                var sorted = good.OrderBy(x => x).ToList();

                _o.WriteLine(
                    $"| `{Path.GetFileName(name)}` | {holds} | {what} "
                    + $"| {good.Count} "
                    + $"| {string.Join(", ", sorted.Select(u => $"{u:0}"))} "
                    + $"| {sorted[sorted.Count / 2]:0.0} "
                    + $"| **{sd / Math.Max(1e-9, mean):0.000}** |");
            }

            Row("station bin", runs.Where(r => Math.Abs(r.ToneHz - pitch) <= 12.5).ToList());
            Row("**whole band**", runs);
        }
    }
}

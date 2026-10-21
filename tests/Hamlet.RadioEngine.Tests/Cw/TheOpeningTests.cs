using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **ONE FINDER, THE OPENING** (work instruction 586, HM-DEC-290): a station's opening read with the bank as the finder, its own
/// lanes beside the grid's bin at its pitch, from a second before its first keyed mark to its first printed letter. Asserts nothing.
/// </summary>
public sealed class TheOpeningTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <remarks>
    /// Work instruction 586, section 2: each keyed mark of the opening (from the board's keying map) with each of the station's three
    /// lanes at that moment, its level, key-up, key-down and contrast, and whether the lane called it; the grid's candidate over it
    /// and the floor under it; then every 50 ms row of the lane at the station's dit.
    /// </remarks>
    /// <param name="recording">The recording.</param>
    /// <param name="pitchHz">The station's pitch.</param>
    /// <param name="after">The opening is the first keyed mark at or after this second that follows a second or more of quiet at the pitch.</param>
    [Theory]
    [InlineData("cw-2026-10-02-200157", 662.8, 0)]
    [InlineData("cw-2026-10-03-144020", 599.9, 9)]
    [InlineData("cw-2026-10-03-144045", 599.9, 0)]
    [InlineData("cw-2026-10-03-221828", 601.3, 0)]
    [InlineData("cw-2026-10-08-121357", 600, 0)]
    [InlineData("cw-2026-10-03-221530", 598.4, 8.5)]
    [InlineData("w1aw-2026-10-07/piece-02", 600, 180)]
    public void TheOpeningTraced(string recording, double pitchHz, double after)
    {
        var map = TheKeyingMapTests.Of(recording);
        var station = map.Stations.OrderBy(s => Math.Abs(s.PitchHz - pitchHz)).First();
        var keyed = station.Marks.OrderBy(m => m.From).ToList();
        var first = keyed.Select((m, i) => (m, i)).First(x => x.m.From >= after && (x.i == 0 || x.m.From - keyed[x.i - 1].To >= 1.0)).m;
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);
        var lanePitch = Math.Round(station.PitchHz / CwLaneBank.SpacingHz) * CwLaneBank.SpacingHz;
        var probes = new List<(double At, double PitchHz, bool Mark, double From, double To, double LevelDb, double UpDb, double DownDb, double ContrastDb)>[3];
        var letters = new List<(double At, string Text)>();
        IReadOnlyList<CwMark> grid = [];

        for (var d = 0; d < 3; d++)
        {
            using var on = CwRules.Choose(CwRules.BankFinds);
            var probe = probes[d] = [];
            var dit = d;

            TheOneFinderTests.Read(audio.Samples[..Math.Min(audio.Samples.Length, (int)((first.From + 8) * audio.SampleRate))], audio.SampleRate, pitch, width, _ => { }, (det, now) =>
            {
                if (det.Bank is { } bank && bank.Probe is null)
                {
                    bank.Probe = (dit, lanePitch);
                    bank.ProbeTrace = probe;
                }

                if (dit == 0 && now >= first.From && now <= first.From + 6)
                {
                    grid = det.CandidatesKept;
                }
            }, dit == 0 ? (c, run) => letters.Add((run[^1].ToSeconds, c.Text + "@" + run.Average(m => m.PitchHz).ToString("0", Inv))) : null);
        }

        var printed = letters.Where(l => l.At >= first.From && l.Text.EndsWith("@" + ((int)Math.Round(lanePitch)).ToString(Inv), StringComparison.Ordinal) || (l.At >= first.From && Math.Abs(double.Parse(l.Text.Split('@')[1], Inv) - station.PitchHz) <= 50)).ToList();
        var firstLetter = printed.Count > 0 ? printed[0].At : first.From + 5;
        var gridNear = grid.Where(g => Math.Abs(g.PitchHz - station.PitchHz) <= 25 && g.ToSeconds >= first.From - 0.05).OrderBy(g => g.FromSeconds).ToList();

        output.WriteLine(string.Create(Inv, $"## {recording}, station {station.PitchHz:0} Hz, lanes at {lanePitch:0} Hz; first keyed mark {first.From:0.000} s; first letter printed near it {(printed.Count > 0 ? $"`{printed[0].Text}` at {printed[0].At:0.00} s" : "none within 5 s")}; the grid's first candidate there {(gridNear.Count > 0 ? $"{gridNear[0].FromSeconds:0.000} s, floor {gridNear[0].LevelDb - gridNear[0].ContrastDb:0.0} dB, contrast {gridNear[0].ContrastDb:0.0}" : "none kept")}"));
        output.WriteLine("| keyed mark | ms | lane 100 ms: level up down contrast called | lane 60 ms | lane 40 ms | grid candidate: from, floor, contrast |");
        output.WriteLine("|---|---|---|---|---|---|");

        foreach (var m in keyed.Where(m => m.From >= first.From - 0.01 && m.From <= firstLetter))
        {
            string Lane(int d)
            {
                var mid = (m.From + m.To) / 2;
                var hop = probes[d].Where(p => p.At >= mid).OrderBy(p => p.At).FirstOrDefault();
                var called = probes[d].Any(p => p.Mark && p.From < m.To && p.To > m.From);

                return string.Create(Inv, $"{hop.LevelDb:0.0} {hop.UpDb:0.0} {hop.DownDb:0.0} {hop.DownDb - hop.UpDb:0.0} {(called ? "yes" : "no")}");
            }

            var g = gridNear.FirstOrDefault(c => c.FromSeconds < m.To && c.ToSeconds > m.From);

            output.WriteLine(string.Create(Inv, $"| {m.From:0.000} | {(m.To - m.From) * 1000:0} | {Lane(0)} | {Lane(1)} | {Lane(2)} | {(g is null ? "-" : $"{g.FromSeconds:0.000}, {g.LevelDb - g.ContrastDb:0.0}, {g.ContrastDb:0.0}")} |"));
        }

        output.WriteLine("letters: " + string.Join(" ", printed.Take(12).Select(l => string.Create(Inv, $"{l.At:0.00}:{l.Text}"))));

        var shortest = keyed.Where(k => k.From >= first.From).Take(20).Select(k => k.To - k.From).DefaultIfEmpty(0.06).Min();
        var nearest = Enumerable.Range(0, 3).OrderBy(d => Math.Abs(Math.Log(CwLaneBank.DitSeconds[d] / Math.Max(shortest, 0.02)))).First();

        output.WriteLine(string.Create(Inv, $"every 50 ms, the lane at {lanePitch:0} Hz and {CwLaneBank.DitSeconds[nearest] * 1000:0} ms: at, level, up, down, contrast, keyed now, mark called ending"));

        var next = first.From - 1;

        foreach (var p in probes[nearest].Where(p => p.At >= first.From - 1 && p.At <= firstLetter))
        {
            if (p.At + 1e-9 < next && !p.Mark)
            {
                continue;
            }

            next = p.At + 0.05;
            var now = keyed.Any(k => k.From <= p.At && k.To >= p.At);

            output.WriteLine(string.Create(Inv, $"  {p.At:0.000} {p.LevelDb:0.0} {p.UpDb:0.0} {p.DownDb:0.0} {p.DownDb - p.UpDb:0.0} {(now ? "keyed" : "-")} {(p.Mark ? $"mark {p.From:0.000}-{p.To:0.000}" : string.Empty)}"));
        }
    }
}

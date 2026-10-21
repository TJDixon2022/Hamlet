using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **ONE FINDER: THE BANK FINDS EVERY STATION** (work instruction 584, HM-DEC-288, <see cref="CwRules.BankFinds"/>): what it costs,
/// and the traces each round of the rebuild named. Asserts nothing.
/// </summary>
public sealed class TheOneFinderTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <remarks>
    /// Work instruction 584, section 2, step 6: W1AW's piece-04 read through the chain with the bank as the finder and without it, the
    /// process's CPU time and the wall clock for each, the first read thrown away to warm the process.
    /// </remarks>
    [Fact]
    public void TheOneFinderCost()
    {
        const string Piece = "w1aw-2026-10-07/piece-04";
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Piece));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Piece);

        (double Cpu, double Wall, string Text) Time(bool bankFinds)
        {
            using var on = bankFinds ? CwRules.Choose(CwRules.BankFinds) : null;
            using var off = bankFinds ? null : CwRules.Off(CwRules.BankFinds);
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var cpu = process.TotalProcessorTime;
            var wall = System.Diagnostics.Stopwatch.StartNew();
            var (_, text) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, pitch, width);

            process.Refresh();

            return ((process.TotalProcessorTime - cpu).TotalSeconds, wall.Elapsed.TotalSeconds, text);
        }

        Time(false);

        var two = Time(false);
        var one = Time(true);

        output.WriteLine(string.Create(Inv, $"{Piece}: {audio.Samples.Length / (double)audio.SampleRate:0} s at {audio.SampleRate} Hz, filter {width:0} Hz"));
        output.WriteLine(string.Create(Inv, $"two finders: {two.Cpu:0.00} s CPU, {two.Wall:0.00} s wall"));
        output.WriteLine(string.Create(Inv, $"one finder:  {one.Cpu:0.00} s CPU, {one.Wall:0.00} s wall; {(one.Cpu - two.Cpu) / two.Cpu:+0%;-0%} of the chain with two"));
        output.WriteLine("two: " + two.Text);
        output.WriteLine("one: " + one.Text);
    }

    /// <remarks>
    /// Work instruction 584, section 3: the recordings the round's largest loss named, read with one finder and with two. Every letter
    /// with the second it printed, its pitch and contrast; every hold and its end; and with one finder, each second, the bank's lanes
    /// standing within 50 Hz of each held pitch with their shapes.
    /// </remarks>
    [Theory]
    [InlineData("w1aw-2026-10-07/piece-01")]
    [InlineData("w1aw-2026-10-07/piece-02")]
    [InlineData("cw-2026-10-02-200157")]
    [InlineData("cw-2026-10-03-221530")]
    [InlineData("cw-2026-10-03-221502")]
    public void TheLargestLossTraced(string recording)
    {
        foreach (var one in new[] { false, true })
        {
            using var on = one ? CwRules.Choose(CwRules.BankFinds) : null;
            using var off = one ? null : CwRules.Off(CwRules.BankFinds);
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
            var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);
            using var chain = new CwChain(audio.SampleRate);
            var letters = new List<string>();
            var trace = new List<string>();
            var lanes = new List<string>();
            var chunk = audio.SampleRate / 100;
            var now = 0.0;
            var lastSecond = -1;

            chain.Detector.SetPassband(pitch, width);
            chain.Detector.HandoverTrace = trace;
            chain.Decoder.Runs.RunRead += (c, run) => letters.Add(string.Create(Inv, $"{run[^1].ToSeconds:0.00} {c.Text}@{run.Average(m => m.PitchHz):0}/{run.Average(m => m.ContrastDb):0}"));

            for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
            {
                chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));
                now = (at + chunk) / (double)audio.SampleRate;

                if (one && (int)(now * 4) != lastSecond && chain.Detector.Bank is { } bank)
                {
                    lastSecond = (int)(now * 4);

                    var standing = bank.Standing(now);
                    var held = chain.Detector.HeldStations;

                    lanes.Add(string.Create(Inv, $"{lastSecond / 4.0,5:0.00} s held [{string.Join(", ", held.Select(h => $"{h.PitchHz:0}/{CwLaneBank.DitSeconds[h.Dit] * 1000:0}"))}] standing [{string.Join(", ", standing.Where(s => s.Sequence.Shape.Score >= 0.1).OrderBy(s => s.Sequence.PitchHz).Select(s => $"{s.Sequence.PitchHz:0}/{CwLaneBank.DitSeconds[s.Dit] * 1000:0} {s.Sequence.Shape.Score:0.00}"))}]"));
                }
            }

            chain.Decoder.Flush();

            output.WriteLine(string.Create(Inv, $"{recording}, {pitch:0} Hz, filter {width:0} Hz, {(one ? "one finder" : "two finders")}:"));
            output.WriteLine("  text: " + string.Concat(letters.Select(l => l.Split(' ')[1].Split('@')[0])));
            output.WriteLine("  letters: " + string.Join("  ", letters));
            output.WriteLine("  holds: " + string.Join("; ", trace));
            output.WriteLine("  lane shadows held: " + string.Join("; ", chain.Detector.ShadowsHeld.Where(h => h.Finder == "lane" && h.AtSeconds < 40).Select(h => string.Create(Inv, $"{h.AtSeconds:0.00} s {h.PitchHz:0} of {h.OfPitchHz:0} share {h.Share:0.00}"))));

            foreach (var line in lanes)
            {
                output.WriteLine("  " + line);
            }
        }
    }

    /// <remarks>
    /// Work instruction 585, section 2, step 2: the random carrier at its twenty seeds with one finder, read with the lanes' marks
    /// meeting none of the grid's tests, all three, and each alone, and the tone test the sender's window applies: which test stops
    /// each seed.
    /// </remarks>
    [Fact]
    public void TheCarrierSeedsAndTheGridsTests()
    {
        var ways = new (string Name, bool Grid, string[] Tests)[]
        {
            ("none", false, []), ("all three", true, ["edges", "narrow", "shape"]), ("edges", true, ["edges"]), ("narrow", true, ["narrow"]),
            ("shape", true, ["shape"]), ("tone", true, ["tone"]),
        };

        output.WriteLine("| seed | " + string.Join(" | ", ways.Select(w => w.Name)) + " | marks failing edges / narrow / shape / tone of called, with none |");
        output.WriteLine("|---|" + string.Concat(ways.Select(_ => "---|")) + "---|");

        foreach (var seed in Enumerable.Range(5193, 20))
        {
            var carrier = TheShapePicksTheSenderTests.Noise(25, 5192);

            TheShapePicksTheSenderTests.Key(carrier, TheShapePicksTheSenderTests.RandomKeying(2.5, 23, seed), 625, 24);

            var reads = new List<string>();
            var fails = string.Empty;

            foreach (var way in ways)
            {
                using var on = CwRules.Choose(CwRules.BankFinds);
                using var off = way.Grid ? null : CwRules.Off(CwRules.LaneMeetsTheGrid);
                var verdicts = new List<(bool Edges, bool Narrow, bool Tone, bool Shape, bool Passes)>();
                var text = Read(carrier, 8000, 600, 500, d =>
                {
                    d.LaneTests = [.. way.Tests];

                    if (!way.Grid)
                    {
                        d.LaneTestsSeen = verdicts;
                    }
                });

                reads.Add(text.Length == 0 ? "-" : $"`{text}`");

                if (!way.Grid)
                {
                    fails = string.Create(Inv, $"{verdicts.Count(v => !v.Edges)} / {verdicts.Count(v => !v.Narrow)} / {verdicts.Count(v => !v.Shape)} / {verdicts.Count(v => !v.Tone)} of {verdicts.Count}");
                }
            }

            output.WriteLine($"| {seed} | {string.Join(" | ", reads)} | {fails} |");
        }
    }

    /// <remarks>
    /// Work instruction 585, section 2, step 3: `tightfist-easy`, which one finder read 0 of 14 where two read 13, traced with two
    /// finders and one, and with one finder before this unit's rules: the text, the holds, and the lanes near its pitch each 0.25 s.
    /// </remarks>
    [Fact]
    public void TheTightFistTraced()
    {
        var audio = WavAudio.Read(TheGridStepsAsideTests.Fixture("tightfist-easy"));

        output.WriteLine("truth: " + TheGridStepsAsideTests.Truth("tightfist-easy"));

        foreach (var way in new[] { "two finders", "one finder as before", "one finder" })
        {
            using var on = way == "two finders" ? CwRules.Off(CwRules.BankFinds) : CwRules.Choose(CwRules.BankFinds);
            using var off = way == "one finder as before" ? CwRules.Off(CwRules.LoudestFirst, CwRules.LaneMeetsTheGrid) : null;
            var trace = new List<string>();
            var lanes = new List<string>();
            var verdicts = new List<(bool Edges, bool Narrow, bool Tone, bool Shape, bool Passes)>();
            var text = Read(audio.Samples, audio.SampleRate, 600, 500, d =>
            {
                d.HandoverTrace = trace;
                d.LaneTestsSeen = verdicts;
            }, (d, now) =>
            {
                if (d.Bank is { } bank && way != "two finders")
                {
                    var standing = bank.Gates.SelectMany((g, k) => g.Formed(now, CwEnvelopeDetector.HoldSeconds, 0.1).Select(f => (k, f, g)))
                        .Where(x => Math.Abs(x.f.PitchHz - 615) <= 100)
                        .OrderBy(x => x.f.PitchHz)
                        .Select(x => string.Create(Inv, $"{x.f.PitchHz:0}/{CwLaneBank.DitSeconds[x.k] * 1000:0}{(x.g.Sender(x.f.Id) is not null ? "*" : string.Empty)} {CwShadow.LevelOf(x.f.Recent):0}dB"));

                    lanes.Add(string.Create(Inv, $"{now,5:0.00} s held [{string.Join(", ", d.HeldStations.Select(h => $"{h.PitchHz:0}/{CwLaneBank.DitSeconds[h.Dit] * 1000:0}"))}] forming at 0.1 [{string.Join(", ", standing)}]"));
                }
            });

            output.WriteLine($"{way}: `{text}`");
            output.WriteLine("  holds: " + string.Join("; ", trace));
            output.WriteLine(string.Create(Inv, $"  lane marks judged: {verdicts.Count}, failing edges {verdicts.Count(v => !v.Edges)}, narrow {verdicts.Count(v => !v.Narrow)}, shape {verdicts.Count(v => !v.Shape)}, tone {verdicts.Count(v => !v.Tone)}"));

            foreach (var line in lanes)
            {
                output.WriteLine("  " + line);
            }
        }
    }


    /// <remarks>
    /// Work instruction 585, section 3: one recording read with one finder as it stood before this unit, with each of this unit's two
    /// rules alone, and with both: the text, every letter with its second, pitch and contrast, the holds, and between two seconds,
    /// each 0.25 s, every lane forming at shape 0.1 with its level and whether it stands.
    /// </remarks>
    [Theory]
    [InlineData("cw-2026-10-03-221530", 7.5, 12.5)]
    [InlineData("cw-2026-10-03-221548", 0, 6)]
    [InlineData("cw-2026-10-03-144020", 0, 5)]
    public void TheLossTracedByRule(string recording, double from, double to)
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);

        foreach (var way in new[] { "as before", "loudest first", "the grid's tests", "both" })
        {
            using var on = CwRules.Choose(CwRules.BankFinds);
            using var off = way switch
            {
                "as before" => CwRules.Off(CwRules.LoudestFirst, CwRules.LaneMeetsTheGrid),
                "loudest first" => CwRules.Off(CwRules.LaneMeetsTheGrid),
                "the grid's tests" => CwRules.Off(CwRules.LoudestFirst),
                _ => null,
            };
            var trace = new List<string>();
            var lanes = new List<string>();
            var letters = new List<string>();
            var text = Read(audio.Samples, audio.SampleRate, pitch, width, d =>
            {
                d.HandoverTrace = trace;
            }, (d, now) =>
            {
                if (d.Bank is { } bank && now >= from && now <= to)
                {
                    var formed = bank.Gates.SelectMany((g, k) => g.Formed(now, CwEnvelopeDetector.HoldSeconds, 0.1).Select(f => (k, f, g)))
                        .OrderBy(x => x.f.PitchHz)
                        .Select(x => string.Create(Inv, $"{x.f.PitchHz:0}/{CwLaneBank.DitSeconds[x.k] * 1000:0}{(x.g.Sender(x.f.Id) is not null ? "*" : string.Empty)} {CwSequenceShape.Of(x.f.Recent, x.f.Recent.Count).Score:0.00} {CwShadow.LevelOf(x.f.Recent):0}dB"));

                    lanes.Add(string.Create(Inv, $"{now,5:0.00} s held [{string.Join(", ", d.HeldStations.Select(h => $"{h.PitchHz:0}/{CwLaneBank.DitSeconds[h.Dit] * 1000:0}"))}] forming [{string.Join(", ", formed)}]"));
                }
            }, (c, run) => letters.Add(string.Create(Inv, $"{run[^1].ToSeconds:0.00}:{c.Text}@{run.Average(m => m.PitchHz):0}/{run.Average(m => m.ContrastDb):0}")));

            output.WriteLine($"{recording}, {way}: `{text}`");
            output.WriteLine("  letters: " + string.Join(" ", letters));
            output.WriteLine("  holds: " + string.Join("; ", trace));

            foreach (var line in lanes)
            {
                output.WriteLine("  " + line);
            }
        }
    }


    /// <remarks>Work instruction 585, section 3: every mark two lanes call between two seconds, with one finder and both of this unit's rules, before any gate: when, how long, level and contrast.</remarks>
    [Theory]
    [InlineData("cw-2026-10-03-221530", 1, 600, 2, 800, 8, 11.5)]
    [InlineData("cw-2026-10-03-221548", 2, 450, 1, 500, 11, 20)]
    public void TheLanesProbed(string recording, int dit, double pitchHz, int otherDit, double otherHz, double from, double to)
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);

        foreach (var (d, hz) in new[] { (dit, pitchHz), (otherDit, otherHz) })
        {
            using var on = CwRules.Choose(CwRules.BankFinds);
            var probe = new List<(double At, double PitchHz, bool Mark, double From, double To, double LevelDb, double UpDb, double DownDb, double ContrastDb)>();
            var verdicts = new List<string>();

            Read(audio.Samples, audio.SampleRate, pitch, width, det => { }, (det, now) =>
            {
                if (det.Bank is { } bank && bank.Probe is null)
                {
                    bank.Probe = (d, hz);
                    bank.ProbeTrace = probe;
                }
            });

            output.WriteLine(string.Create(Inv, $"{recording} lane {hz:0}/{CwLaneBank.DitSeconds[d] * 1000:0}, {from:0.0} to {to:0.0} s:"));

            foreach (var p in probe.Where(p => p.At >= from && p.At <= to && p.Mark))
            {
                output.WriteLine(string.Create(Inv, $"  mark {p.From:0.000}-{p.To:0.000} ({(p.To - p.From) * 1000:0} ms) level {p.LevelDb:0.0} up {p.UpDb:0.0} down {p.DownDb:0.0} contrast {p.ContrastDb:0.0}"));
            }

            var hops = probe.Where(p => p.At >= from && p.At <= to).ToList();

            if (hops.Count > 0)
            {
                output.WriteLine(string.Create(Inv, $"  contrast over the stretch: {hops.Min(p => p.ContrastDb):0.0} to {hops.Max(p => p.ContrastDb):0.0} dB; level {hops.Min(p => p.LevelDb):0.0} to {hops.Max(p => p.LevelDb):0.0}"));
            }
        }
    }

    // Reads audio through the app's chain at a passband, as the scoreboard does, with the detector set up first and looked at each 0.25 s.
    private static string Read(float[] samples, int rate, double pitch, double width, Action<CwEnvelopeDetector> setUp, Action<CwEnvelopeDetector, double>? each = null, Action<CwCharacter, IReadOnlyList<CwMark>>? run = null)
    {
        using var chain = new CwChain(rate);

        if (run is not null)
        {
            chain.Decoder.Runs.RunRead += (c, marks) => run(c, marks);
        }
        var characters = new List<CwCharacter>();
        var chunk = rate / 100;
        var last = -1;

        chain.Detector.SetPassband(pitch, width);
        setUp(chain.Detector);
        chain.Decoder.Runs.CharacterRead += characters.Add;

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));

            var now = (at + chunk) / (double)rate;

            if (each is not null && (int)(now * 4) != last)
            {
                last = (int)(now * 4);
                each(chain.Detector, now);
            }
        }

        chain.Decoder.Flush();

        return string.Join(' ', string.Concat(characters.Select(c => c.Text)).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

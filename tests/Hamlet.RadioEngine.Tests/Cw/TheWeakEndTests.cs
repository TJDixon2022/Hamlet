using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE WEAK END** (work instruction 568, HM-DEC-272): the survey of 2026-10-09 found Hamlet reading 0 of 84 letters on the
/// eight synthetic CQs at 5 dB and 0 dB, 0 of 81 on the five edge receiver fixtures and 5 of 81 on the five working ones. It
/// prints nothing rather than reading badly. These trace each of the eighteen through the chain, stage by stage, and open the
/// sender's own window at the true pitch by hand to see what it would read.
/// </summary>
public sealed class TheWeakEndTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>The eighteen: the CQs at 5 and 0 dB, the working and edge receiver fixtures, with their recipes.</summary>
    internal static IEnumerable<(string Name, string Wav, CwFixtureRecipe Recipe)> Files()
    {
        var root = CwFixtures.Folder;

        foreach (var r in SyntheticCq.All.Where(r => r.Name.Contains("-5db", StringComparison.Ordinal) || r.Name.Contains("-0db", StringComparison.Ordinal)).OrderBy(r => r.Name, StringComparer.Ordinal))
        {
            yield return (r.Name, Path.Combine(root, "synthetic-cq", r.Name + ".wav"), r);
        }

        foreach (var r in CwFixtureCatalogue.All.Where(r => r.Name.EndsWith("-working", StringComparison.Ordinal) || r.Name.EndsWith("-edge", StringComparison.Ordinal)).OrderBy(r => r.Name, StringComparer.Ordinal))
        {
            yield return (r.Name, Path.Combine(root, "receiver", r.Name + ".wav"), r);
        }
    }

    /// <summary>The marks as keyed: from the recipe's own key edges.</summary>
    internal static List<(double From, double To)> Sent(CwFixtureRecipe recipe)
    {
        var edges = CwFixtureGenerator.KeyEdges(recipe, out _);
        var marks = new List<(double, double)>();

        for (var i = 0; i + 1 < edges.Length; i += 2)
        {
            marks.Add((edges[i], edges[i + 1]));
        }

        return marks;
    }

    /// <summary>One grid bin, mirrored offline: a 10 ms Hann window every 5 ms at the pitch, the level in dB.</summary>
    internal static List<(double At, double Db)> WideBin(float[] x, int rate, double pitchHz)
    {
        var n = 2 * Math.Max(4, rate / 200);
        var hop = n / 2;
        var levels = new List<(double, double)>();

        for (var start = 0; start + n <= x.Length; start += hop)
        {
            double re = 0, im = 0;

            for (var i = 0; i < n; i++)
            {
                var w = 0.5 - (0.5 * Math.Cos(2 * Math.PI * (i + 0.5) / n));
                var phase = 2 * Math.PI * pitchHz * (start + i) / rate;

                re += x[start + i] * w * Math.Cos(phase);
                im += x[start + i] * w * Math.Sin(phase);
            }

            levels.Add(((start + (n / 2.0)) / rate, 10 * Math.Log10((re * re) + (im * im) + 1e-20)));
        }

        return levels;
    }

    /// <summary>The sender's own window, opened by hand at the pitch and the dit: the level every 5 ms, delay taken off.</summary>
    internal static List<(double At, double Db)> OwnWindow(float[] x, int rate, double pitchHz, double ditSeconds)
    {
        var lane = new CwSenderLane(rate);
        var hop = rate / 200;
        var levels = new List<(double, double)>();

        lane.Tune(pitchHz, ditSeconds);

        for (var n = 0; n < x.Length; n++)
        {
            lane.Push(x[n]);

            if ((n + 1) % hop == 0)
            {
                levels.Add((((n + 1) / (double)rate) - lane.DelaySeconds, lane.LevelDb));
            }
        }

        return levels;
    }

    private static double Median(IEnumerable<double> values)
    {
        var s = values.Where(double.IsFinite).Order().ToList();

        return s.Count == 0 ? double.NaN : s[s.Count / 2];
    }

    private static double P(IEnumerable<double> values, double p)
    {
        var s = values.Where(double.IsFinite).Order().ToList();

        return s.Count == 0 ? double.NaN : s[(int)Math.Min(s.Count - 1, Math.Floor(p * s.Count))];
    }

    /// <summary>Key-down and key-up levels on a trace against the marks as sent, a hop clear of every edge.</summary>
    internal static (double Down, double Up, double UpSpread, List<double> PerMark) Contrast(List<(double At, double Db)> trace, List<(double From, double To)> sent)
    {
        bool InMark(double t, double margin) => sent.Any(m => t >= m.From + margin && t <= m.To - margin);
        bool NearMark(double t, double margin) => sent.Any(m => t >= m.From - margin && t <= m.To + margin);

        var first = sent[0].From;
        var last = sent[^1].To;
        var down = trace.Where(h => InMark(h.At, 0.01)).Select(h => h.Db).ToList();
        var up = trace.Where(h => h.At > first - 1 && h.At < last + 1 && !NearMark(h.At, 0.015)).Select(h => h.Db).ToList();
        var upMedian = Median(up);
        var mean = up.Count > 0 ? up.Average() : double.NaN;
        var spread = up.Count > 1 ? Math.Sqrt(up.Sum(u => (u - mean) * (u - mean)) / up.Count) : double.NaN;
        var perMark = sent.Select(m => Median(trace.Where(h => h.At >= m.From + 0.01 && h.At <= m.To - 0.01).Select(h => h.Db)) - upMedian).ToList();

        return (Median(down), upMedian, spread, perMark);
    }

    /// <summary>Marks on a trace by the level rule: up at 0.6 of the contrast over key-up, down under 0.4, at least half a dit.</summary>
    internal static List<(double From, double To)> LevelMarks(List<(double At, double Db)> trace, double up, double down, double ditSeconds)
    {
        var marks = new List<(double, double)>();
        var c = down - up;
        double? from = null;

        foreach (var (at, db) in trace)
        {
            if (from is null && db >= up + (0.6 * c))
            {
                from = at;
            }
            else if (from is { } f && db < up + (0.4 * c))
            {
                if (at - f >= ditSeconds / 2)
                {
                    marks.Add((f, at));
                }

                from = null;
            }
        }

        return marks;
    }

    /// <summary>The ceiling: marks read by the recipe's own timing, dit or dah at their midpoint, gaps at theirs.</summary>
    internal static string Read(List<(double From, double To)> marks, CwFixtureRecipe r)
    {
        var dahLine = Math.Sqrt(r.DitMilliseconds * r.DahMilliseconds) / 1000;
        var letterLine = Math.Sqrt(r.ElementGapMilliseconds * r.CharacterGapMilliseconds) / 1000;
        var wordLine = Math.Sqrt(r.CharacterGapMilliseconds * r.WordGapMilliseconds) / 1000;
        var text = new StringBuilder();
        var code = new StringBuilder();

        for (var i = 0; i < marks.Count; i++)
        {
            if (i > 0)
            {
                var gap = marks[i].From - marks[i - 1].To;

                if (gap >= letterLine)
                {
                    text.Append(MorseAlphabet.Lookup(code.ToString()) ?? "■");
                    code.Clear();

                    if (gap >= wordLine)
                    {
                        text.Append(' ');
                    }
                }
            }

            code.Append(marks[i].To - marks[i].From >= dahLine ? '-' : '.');
        }

        if (code.Length > 0)
        {
            text.Append(MorseAlphabet.Lookup(code.ToString()) ?? "■");
        }

        return text.ToString();
    }

    /// <summary>What the chain did at the station's pitch, sampled every quarter second.</summary>
    internal sealed record ChainTrace(
        int Offered, double OfferedContrastDb, int Stood, int SequenceMarks, double SequenceShape, double FirstWindowSeconds,
        double FirstSenderSeconds, double SenderShape, bool Qualified, bool Printed, string Text, int LettersPrinted);

    /// <summary>Reads a file through the app's chain as the survey does, and samples the stages at the pitch.</summary>
    internal static ChainTrace Chain(float[] samples, int rate, double pitchHz, bool handover = false)
    {
        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var offered = new Dictionary<long, CwMark>();
        var text = new StringBuilder();
        var letters = 0;
        var stood = 0;
        var seqMarks = 0;
        var seqShape = 0.0;
        var firstWindow = double.NaN;
        var firstSender = double.NaN;
        var senderShape = 0.0;
        var qualified = false;
        var printed = false;
        long sequence = 0;
        var near = (double hz) => Math.Abs(hz - pitchHz) <= 40;

        chain.Detector.SetPassband(600, 500);
        chain.Detector.LaneHandover = handover;
        gate.CharacterRead += c => text.Append(c.Text);
        gate.RunRead += (_, _) => letters++;

        var chunk = rate / 100;

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));

            var now = (at + chunk) / (double)rate;

            foreach (var m in chain.Detector.OfferedNow.Where(m => near(m.PitchHz)))
            {
                offered[m.Sequence] = m;
            }

            var batch = chain.Detector.MarksSince(sequence);

            if (batch.Marks.Count > 0)
            {
                stood += batch.Marks.Count(m => near(m.PitchHz));
                sequence = batch.Marks.Max(m => m.Sequence);
            }

            if ((at + chunk) % (rate / 4) != 0)
            {
                continue;
            }

            foreach (var line in chain.Detector.SequencesNow(now).Split(" | ", StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split(' ');

                if (double.TryParse(parts[0], NumberStyles.Float, Inv, out var hz) && near(hz))
                {
                    seqMarks = Math.Max(seqMarks, int.Parse(parts[2], Inv));

                    var s = line.LastIndexOf("shape ", StringComparison.Ordinal);

                    if (s >= 0 && double.TryParse(line[(s + 6)..], NumberStyles.Float, Inv, out var shape))
                    {
                        seqShape = Math.Max(seqShape, shape);
                    }
                }
            }

            if (double.IsNaN(firstWindow) && chain.Detector.CandidateWindowsNow.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Any(w => double.TryParse(w.Split(':')[0], NumberStyles.Float, Inv, out var hz) && near(hz)))
            {
                firstWindow = now;
            }

            foreach (var s in gate.SenderShapes.Where(s => near(s.PitchHz)))
            {
                firstSender = double.IsNaN(firstSender) ? now : firstSender;
                senderShape = Math.Max(senderShape, s.Shape.Score);
                qualified |= s.Qualified;
                printed |= s.Printed;
            }
        }

        chain.Decoder.Flush();

        return new ChainTrace(
            offered.Count, Median(offered.Values.Select(m => m.OwnContrastDb)), stood, seqMarks, seqShape, firstWindow, firstSender,
            senderShape, qualified, printed, string.Join(' ', text.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)), letters);
    }

    /// <remarks>
    /// Task 1: the eighteen, a row each. The signal as sent; at the wide detector, one grid bin mirrored offline against the
    /// marks as sent and what the chain offered and stood at the pitch; at the gate, the sequences, the candidate window, the
    /// sender and its shape; in the sender's own window opened by hand at the true pitch, its contrast, the marks the level rule
    /// finds, their shape and what they would read. Asserts nothing.
    /// </remarks>
    [Fact]
    public void WhereEachWeakFileDies()
    {
        output.WriteLine("| file | sent: SNR in 520 Hz, pitch, WPM, marks | wide bin: contrast median, key-up spread | chain: offered (their contrast), stood, sequence marks, shape | window opened, sender (shape, qualified, printed) | own window: contrast median / p10 | level marks found of sent, whole | their shape | dies at | chain read | window read, right of sent | lane read, right of sent |");
        output.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|---|");

        foreach (var (name, wav, r) in Files())
        {
            output.WriteLine(Row(name, wav, r));
        }
    }

    /// <summary>
    /// The bank's sequences over a run, sampled every quarter second: for each pitch and dit, the most marks, the best shape, and the
    /// first second its recent marks split into two lengths at a ratio of two or more (work instruction 569).
    /// </summary>
    internal static List<(double PitchHz, double DitMs, int Marks, double Shape, double TwoLengthsAt, string Lengths)> BankTrace(float[] samples, int rate)
    {
        using var chain = new CwChain(rate);
        var chunk = rate / 100;
        var seen = new Dictionary<(double, int), (double PitchHz, double DitMs, int Marks, double Shape, double TwoLengthsAt, string Lengths)>();

        chain.Detector.SetPassband(600, 500);

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));

            if ((at + chunk) % (rate / 4) != 0 || chain.Detector.Bank is not { } bank)
            {
                continue;
            }

            var now = (at + chunk) / (double)rate;

            for (var d = 0; d < bank.Gates.Count; d++)
            {
                foreach (var line in bank.Gates[d].Describe(now).Split(" | ", StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = line.Split(' ');
                    var hz = double.Parse(parts[0], Inv);
                    var marks = int.Parse(parts[2], Inv);
                    var s = line.LastIndexOf("shape ", StringComparison.Ordinal);
                    var shape = double.Parse(line[(s + 6)..], Inv);
                    var lengths = line[(line.IndexOf('[', StringComparison.Ordinal) + 1)..line.IndexOf(']', StringComparison.Ordinal)];
                    var two = !line.Contains("one length", StringComparison.Ordinal);
                    var key = (Math.Round(hz / 25) * 25, d);
                    (double PitchHz, double DitMs, int Marks, double Shape, double TwoLengthsAt, string Lengths) was = seen.TryGetValue(key, out var w) ? w : (key.Item1, CwLaneBank.DitSeconds[d] * 1000, 0, 0.0, double.NaN, string.Empty);

                    seen[key] = (was.PitchHz, was.DitMs, Math.Max(was.Marks, marks), Math.Max(was.Shape, shape), double.IsNaN(was.TwoLengthsAt) && two ? now : was.TwoLengthsAt, shape >= was.Shape ? lengths : was.Lengths);
                }
            }
        }

        return seen.Values.OrderByDescending(v => v.Shape).ToList();
    }

    /// <remarks>
    /// Work instruction 569, task 1: on `cq-18wpm-5db` a bank lane at the station's pitch shows two lengths by ten seconds and
    /// reaches a shape of 0.6, where the grid's pieces never stand.
    /// </remarks>
    [Fact]
    public void TheBankFindsTheWeakCq()
    {
        var (_, wav, r) = Files().Single(f => f.Name == "cq-18wpm-5db");
        var audio = WavAudio.Read(wav);
        var trace = BankTrace(audio.Samples, audio.SampleRate);

        foreach (var t in trace.Take(8))
        {
            output.WriteLine(string.Create(Inv, $"{t.PitchHz:0} Hz at a {t.DitMs:0} ms dit: {t.Marks} marks, best shape {t.Shape:0.00}, two lengths from {t.TwoLengthsAt:0.0} s, [{t.Lengths}]"));
        }

        var station = trace.Where(t => Math.Abs(t.PitchHz - r.ToneHz) <= 25).OrderByDescending(t => t.Shape).First();

        Assert.True(station.TwoLengthsAt <= 10, $"two lengths only from {station.TwoLengthsAt:0.0} s");
        Assert.True(station.Shape >= 0.6, $"best shape {station.Shape:0.00}");
    }

    /// <remarks>
    /// Work instruction 569, task 1: on the scoreboard's loud-noise runs and the random carriers, no bank sequence reaches a shape of
    /// 0.4. Prints every run's best; asserts the noise runs.
    /// </remarks>
    [Fact]
    public void TheBankFindsNothingInNoise()
    {
        var worst = 0.0;

        foreach (var (seconds, seed) in new[] { (30, 5190 + 30), (180, 5190 + 180), (30, 5100 + 30), (180, 5100 + 180) })
        {
            var noise = Hamlet.RadioEngine.Training.CwSignal.Generate(new Hamlet.RadioEngine.Training.CwSignalRequest(
                " ", SampleRate: 8000, Amplitude: 0, NoiseAmplitude: 0.3, LeadInSeconds: seconds / 2.0, TailSeconds: seconds / 2.0, Seed: seed)).Samples;
            var best = BankTrace(noise, 8000).FirstOrDefault();

            worst = Math.Max(worst, best.Shape);
            output.WriteLine(string.Create(Inv, $"{seconds} s of loud noise, seed {seed}: best {best.PitchHz:0} Hz, {best.Marks} marks, shape {best.Shape:0.00}"));
        }

        foreach (var seed in Enumerable.Range(5193, 20))
        {
            var carrier = TheShapePicksTheSenderTests.Noise(25, 5192);

            TheShapePicksTheSenderTests.Key(carrier, TheShapePicksTheSenderTests.RandomKeying(2.5, 23, seed), 625, 24);

            var best = BankTrace(carrier, 8000).FirstOrDefault();

            output.WriteLine(string.Create(Inv, $"a carrier keyed at random, seed {seed}: best {best.PitchHz:0} Hz, {best.Marks} marks, shape {best.Shape:0.00}"));
        }

        Assert.True(worst < 0.4, $"a bank sequence on noise reached {worst:0.00}");
    }

    /// <remarks>
    /// Work instruction 569, task 1: what the bank costs. W1AW's piece-04 read through the chain with the bank and without it, the
    /// process's CPU time and the wall clock for each. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheBankCost()
    {
        const string Piece = "w1aw-2026-10-07/piece-04";
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Piece));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Piece);

        (double Cpu, double Wall, string Text) Time(bool bank)
        {
            using var rule = bank ? null : CwRules.Off(CwRules.LaneBank);
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var cpu = process.TotalProcessorTime;
            var wall = System.Diagnostics.Stopwatch.StartNew();
            var (_, text) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, pitch, width);

            process.Refresh();

            return ((process.TotalProcessorTime - cpu).TotalSeconds, wall.Elapsed.TotalSeconds, text);
        }

        Time(false);

        var without = Time(false);
        var with = Time(true);

        output.WriteLine(string.Create(Inv, $"{Piece}: {audio.Samples.Length / (double)audio.SampleRate:0} s at {audio.SampleRate} Hz, filter {width:0} Hz"));
        output.WriteLine(string.Create(Inv, $"without the bank: {without.Cpu:0.00} s CPU, {without.Wall:0.00} s wall"));
        output.WriteLine(string.Create(Inv, $"with the bank:    {with.Cpu:0.00} s CPU, {with.Wall:0.00} s wall; the bank {with.Cpu - without.Cpu:0.00} s CPU, {(with.Cpu - without.Cpu) / without.Cpu:P0} of the chain without it"));
        output.WriteLine($"the same text both ways: {with.Text == without.Text}");
    }

    /// <remarks>
    /// Work instruction 569, task 2: on `cq-18wpm-5db`, every mark the chain handed the sender gate, its pitch and length and the gap
    /// before it, beside the marks as keyed. Asserts nothing.
    /// </remarks>
    [Fact]
    public void WhatTheHandoverHandsOn()
    {
        var (_, wav, r) = Files().Single(f => f.Name == "cq-18wpm-5db");
        var audio = WavAudio.Read(wav);
        using var chain = new CwChain(audio.SampleRate);
        var marks = new List<CwMark>();
        var text = new StringBuilder();
        long seq = 0;
        var chunk = audio.SampleRate / 100;

        chain.Detector.SetPassband(600, 500);
        chain.Decoder.Runs.CharacterRead += c => text.Append(c.Text);
        chain.Detector.LaneHandover = true;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));

            var batch = chain.Detector.MarksSince(seq);

            if (batch.Marks.Count > 0)
            {
                marks.AddRange(batch.Marks);
                seq = batch.Marks.Max(m => m.Sequence);
            }
        }

        chain.Decoder.Flush();

        var sent = Sent(r);

        output.WriteLine($"read `{text}`");
        output.WriteLine("sent:   " + string.Join(" ", sent.Take(30).Select((m, i) => string.Create(Inv, $"{(i > 0 ? $"[{(m.From - sent[i - 1].To) * 1000:0}] " : string.Empty)}{(m.To - m.From) * 1000:0}"))));
        output.WriteLine("handed: " + string.Join(" ", marks.OrderBy(m => m.FromSeconds).Take(40).Select((m, i) => string.Create(Inv, $"{m.FromSeconds:0.00}:{m.LengthMs:0}@{m.PitchHz:0}"))));
        output.WriteLine(string.Create(Inv, $"first sent at {sent[0].From:0.00} s; {marks.Count} marks handed on"));
    }

    /// <remarks>
    /// Work instruction 570, task 2: the carrier keyed at random at the scoreboard's seeds, read with the handover on: what it
    /// prints, the marks the lane handed on, and the share of its time key-up by the scan's own measure. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheCarrierAtTheLane()
    {
        foreach (var seed in new[] { 5195, 5209 })
        {
            var samples = TheShapePicksTheSenderTests.Noise(25, 5192);

            TheShapePicksTheSenderTests.Key(samples, TheShapePicksTheSenderTests.RandomKeying(2.5, 23, seed), 625, 24);

            using var chain = new CwChain(8000);
            var marks = new List<CwMark>();
            var text = new StringBuilder();
            long seq = 0;

            chain.Detector.SetPassband(600, 500);
            chain.Detector.LaneHandover = true;
            chain.Decoder.Runs.CharacterRead += c => text.Append(c.Text);

            for (var at = 0; at + 80 <= samples.Length; at += 80)
            {
                chain.Process(new AudioChunk(at, 8000, samples.AsSpan(at, 80)));

                var batch = chain.Detector.MarksSince(seq);

                if (batch.Marks.Count > 0)
                {
                    marks.AddRange(batch.Marks);
                    seq = batch.Marks.Max(m => m.Sequence);
                }
            }

            chain.Decoder.Flush();

            var share = Hamlet.RadioEngine.Scan.CwCatchEar.KeyUpShareOf(samples, 8000, 625);

            output.WriteLine(string.Create(Inv, $"seed {seed}: reads `{text}`; key-up share over the whole {share:0.00}; {marks.Count} marks handed on"));
            output.WriteLine("handed: " + string.Join(" ", marks.OrderBy(m => m.FromSeconds).Take(60).Select(m => string.Create(Inv, $"{m.FromSeconds:0.00}:{m.LengthMs:0}@{m.PitchHz:0}"))));
            output.WriteLine("shadows held: " + string.Join("; ", chain.Detector.ShadowsHeld.Select(h => string.Create(Inv, $"{h.AtSeconds:0.0} s lane {h.PitchHz:0} Hz of {h.OfPitchHz:0} Hz share {h.Share:0.00}"))));
        }
    }

    /// <remarks>
    /// Work instruction 569: `two-station`, which prints nothing today, read with the handover off and on. Reported, not required.
    /// Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheTwoStationFixture()
    {
        var wav = Path.Combine(CwFixtures.Folder, "receiver", "two-station.wav");
        var audio = WavAudio.Read(wav);

        foreach (var handover in new[] { false, true })
        {
            var read = Chain(audio.Samples, audio.SampleRate, 615, handover);

            output.WriteLine($"handover {(handover ? "on" : "off")}: `{read.Text}` against `CQ CQ DE N0CALL K N0CALL DE W1XYZ K`, {TheRecordingsScoreboardTests.Right(read.Text, "CQ CQ DE N0CALL K N0CALL DE W1XYZ K")} of 27");
        }
    }

    /// <remarks>The pattern gate's sequences at the pitch, every two seconds, on two 5 dB CQs. Asserts nothing.</remarks>
    [Fact]
    public void TheSequencesAtThePitch()
    {
        foreach (var (name, wav, r) in Files().Where(f => f.Name is "cq-18wpm-5db" or "cq-12wpm-5db"))
        {
            var audio = WavAudio.Read(wav);
            using var chain = new CwChain(audio.SampleRate);
            var chunk = audio.SampleRate / 100;

            chain.Detector.SetPassband(600, 500);
            output.WriteLine($"{name}:");

            for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
            {
                chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));

                if ((at + chunk) % (2 * audio.SampleRate) == 0)
                {
                    var now = (at + chunk) / (double)audio.SampleRate;
                    var lines = chain.Detector.SequencesNow(now).Split(" | ", StringSplitOptions.RemoveEmptyEntries)
                        .Where(l => double.TryParse(l.Split(' ')[0], NumberStyles.Float, Inv, out var hz) && Math.Abs(hz - r.ToneHz) <= 40);

                    output.WriteLine(string.Create(Inv, $"  {now:0} s: {string.Join(" || ", lines)} || windows {chain.Detector.CandidateWindowsNow}"));
                }
            }
        }
    }

    /// <remarks>
    /// Work instruction 571, the guard (HM-DEC-275): the handover ships on, and through the chain as shipped each of the four
    /// 5 dB CQs reads half its letters or better. At the ship they read 13, 14, 16 and 19 of 21. Work instruction 576 (HM-DEC-280):
    /// a held lane keeps reading and its hold ends in a pause, and the four read 71 of 84 or better, 14, 17, 20 and 20 at the ship.
    /// </remarks>
    [Fact]
    public void TheWeakCqsReadAtHalfOrBetter()
    {
        using (var shipped = new CwChain(8000))
        {
            Assert.True(shipped.Detector.LaneHandover, "the handover is not on by default");
        }

        var cqs = Files().Where(f => f.Name.Contains("-5db", StringComparison.Ordinal)).ToList();

        Assert.Equal(4, cqs.Count);

        var total = 0;

        foreach (var (name, wav, r) in cqs)
        {
            var audio = WavAudio.Read(wav);
            var reference = System.Text.RegularExpressions.Regex.Replace(r.Text, @"\^([A-Z]{2})", "<$1>");
            var outOf = TheRecordingsScoreboardTests.Letters(reference).Length;
            var read = Chain(audio.Samples, audio.SampleRate, r.ToneHz, handover: true);
            var right = TheRecordingsScoreboardTests.Right(read.Text, reference);

            total += right;
            output.WriteLine($"{name}: `{read.Text}`, {right} of {outOf}");
            Assert.True(right * 2 >= outOf, $"{name} read `{read.Text}`, {right} of {outOf}");
        }

        output.WriteLine($"total {total} of 84");
        Assert.True(total >= 71, $"the four read {total} of 84");
    }

    /// <remarks>
    /// Work instruction 575, task 2: the four 5 dB CQs with a held lane reading under its own contrast floor and without (HM-DEC-279):
    /// on, they read 14, 17, 20 and 20 of 21. It ships on, with the end of its hold, since work instruction 576 (HM-DEC-280). Asserts
    /// nothing.
    /// </remarks>
    [Fact]
    public void TheWeakCqsWithAHeldLaneReading()
    {
        foreach (var held in new[] { false, true })
        {
            using var _ = held ? null : CwRules.Off(CwRules.HeldLaneReads);
            var total = 0;

            foreach (var (name, wav, r) in Files().Where(f => f.Name.Contains("-5db", StringComparison.Ordinal)))
            {
                var audio = WavAudio.Read(wav);
                var reference = System.Text.RegularExpressions.Regex.Replace(r.Text, @"\^([A-Z]{2})", "<$1>");
                var read = Chain(audio.Samples, audio.SampleRate, r.ToneHz, handover: true);
                var right = TheRecordingsScoreboardTests.Right(read.Text, reference);

                total += right;
                output.WriteLine($"{(held ? "a held lane reads, as it ships" : "the held lane off")}: {name}: `{read.Text}`, {right} of {TheRecordingsScoreboardTests.Letters(reference).Length}");
            }

            output.WriteLine($"{(held ? "a held lane reads, as it ships" : "the held lane off")}: total {total} of 84");
        }
    }

    /// <remarks>
    /// Work instruction 576, the silence after a weak station (HM-DEC-280): each 5 dB CQ followed by 20 s of the same band noise,
    /// its own shaping at its own level, read through the chain as it ships. Nothing prints in the 20 s: no letter whose last mark
    /// ends after the CQ's last key-up.
    /// </remarks>
    [Fact]
    public void NothingPrintsInTheSilenceAfterAWeakStation()
    {
        foreach (var (name, wav, r) in Files().Where(f => f.Name.Contains("-5db", StringComparison.Ordinal)))
        {
            var audio = WavAudio.Read(wav);
            var lastKeyUp = Sent(r)[^1].To;
            var tail = audio.Samples.AsSpan(audio.Samples.Length - audio.SampleRate).ToArray();
            var tailRms = Math.Sqrt(tail.Average(x => (double)x * x));
            var silence = new float[20 * audio.SampleRate];
            var silenceRms = CwFixtureGenerator.ShapedNoise(silence, r.Seed + 1);

            for (var i = 0; i < silence.Length; i++)
            {
                silence[i] = (float)Math.Clamp(silence[i] * tailRms / silenceRms, -1, 1);
            }

            var samples = audio.Samples.Concat(silence).ToArray();
            using var chain = new CwChain(audio.SampleRate);
            var letters = new List<(double At, string Text)>();
            var chunk = audio.SampleRate / 100;

            chain.Detector.SetPassband(600, 500);
            chain.Decoder.Runs.RunRead += (c, run) => letters.Add((run[^1].ToSeconds, c.Text));

            for (var at = 0; at + chunk <= samples.Length; at += chunk)
            {
                chain.Process(new AudioChunk(at, audio.SampleRate, samples.AsSpan(at, chunk)));
            }

            chain.Decoder.Flush();

            var after = letters.Where(l => l.At > lastKeyUp + 0.05).ToList();

            output.WriteLine(string.Create(Inv, $"{name}: the CQ `{string.Concat(letters.Where(l => l.At <= lastKeyUp + 0.05).Select(l => l.Text))}`, then in the 20 s: {after.Count} letters{(after.Count > 0 ? $", `{string.Join(" ", after.Select(a => $"{a.Text}@{a.At:0.0}"))}`" : string.Empty)}"));
            Assert.Empty(after);
        }
    }

    /// <summary>One file's row.</summary>
    internal static string Row(string name, string wav, CwFixtureRecipe r)
    {
        var audio = WavAudio.Read(wav);
        var sent = Sent(r);
        var dit = r.DitMilliseconds / 1000;
        var wide = Contrast(WideBin(audio.Samples, audio.SampleRate, r.ToneHz), sent);
        var ownTrace = OwnWindow(audio.Samples, audio.SampleRate, r.ToneHz, dit);
        var own = Contrast(ownTrace, sent);
        var found = LevelMarks(ownTrace, own.Up, own.Down, dit);
        var whole = sent.Count(s => found.Count(f => f.To > s.From && f.From < s.To) == 1 && found.Any(f => Math.Abs(f.From - s.From) < dit && Math.Abs(f.To - s.To) < dit));
        var shape = CwSequenceShape.Of(found.Select((f, i) => new CwMark(i, f.From, f.To, r.ToneHz, own.Down, own.Down - own.Up)).ToList(), found.Count).Score;
        var reference = System.Text.RegularExpressions.Regex.Replace(r.Text, @"\^([A-Z]{2})", "<$1>");
        var windowRead = Read(found, r);
        var windowRight = TheRecordingsScoreboardTests.Right(windowRead, reference);
        var outOf = TheRecordingsScoreboardTests.Letters(reference).Length;
        var chain = Chain(audio.Samples, audio.SampleRate, r.ToneHz);
        var lane = Chain(audio.Samples, audio.SampleRate, r.ToneHz, handover: true);
        var stage =
            chain.Offered < sent.Count / 10 ? "detector"
            : chain.SequenceMarks < 3 ? "detector"
            : double.IsNaN(chain.FirstWindowSeconds) && double.IsNaN(chain.FirstSenderSeconds) ? "candidate"
            : double.IsNaN(chain.FirstSenderSeconds) ? "window"
            : chain.SenderShape < CwSenderGate.PrintScore ? "sure"
            : !chain.Printed ? "marks"
            : TheRecordingsScoreboardTests.Right(chain.Text, reference) * 2 < outOf ? "reader"
            : "reads";

        return string.Create(Inv,
            $"| `{name}` | {r.SignalToNoiseDb:0} dB, {r.ToneHz:0} Hz, {1200 / r.DitMilliseconds:0} WPM, {sent.Count} | {wide.Down - wide.Up:0.0} dB, {wide.UpSpread:0.0} dB | {chain.Offered} ({chain.OfferedContrastDb:0.0} dB), {chain.Stood}, {chain.SequenceMarks}, {chain.SequenceShape:0.00} | {(double.IsNaN(chain.FirstWindowSeconds) ? "no window" : $"at {chain.FirstWindowSeconds:0.0} s")}, {(double.IsNaN(chain.FirstSenderSeconds) ? "no sender" : $"sender {chain.SenderShape:0.00}{(chain.Qualified ? " qualified" : string.Empty)}{(chain.Printed ? " printed" : string.Empty)}")} | {own.Down - own.Up:0.0} / {P(own.PerMark, 0.1):0.0} dB | {found.Count} of {sent.Count}, {whole} whole | {shape:0.00} | **{stage}** | `{chain.Text}` | `{windowRead}`, {windowRight} of {outOf} | `{lane.Text}`, {TheRecordingsScoreboardTests.Right(lane.Text, reference)} of {outOf} |");
    }

    /// <remarks>
    /// Work instruction 571, task 1: with the handover on, every lane the shadow rule holds back, its pitch, the stronger station's
    /// and the share, and each lane the handover holds and lets go; on W1AW's piece 4, two recordings of the owner's and the
    /// carrier at the scoreboard's first seed. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheShadowsHeld()
    {
        var jobs = new List<(string Name, Func<(float[] Samples, int Rate, double Pitch, double Width)> Audio)>
        {
            ("w1aw piece-04", () => Recording("w1aw-2026-10-07/piece-04")),
            ("cw-2026-10-03-221745", () => Recording("cw-2026-10-03-221745")),
            ("cw-2026-10-03-221530", () => Recording("cw-2026-10-03-221530")),
            ("two-station", () =>
            {
                var audio = WavAudio.Read(Path.Combine(CwFixtures.Folder, "receiver", "two-station.wav"));
                return (audio.Samples, audio.SampleRate, 600, 500);
            }),
        };

        foreach (var seed in new[] { 5193, 5209 })
        {
            jobs.Add(($"carrier seed {seed}", () =>
            {
                var samples = TheShapePicksTheSenderTests.Noise(25, 5192);

                TheShapePicksTheSenderTests.Key(samples, TheShapePicksTheSenderTests.RandomKeying(2.5, 23, seed), 625, 24);
                return (samples, 8000, 600, 500);
            }));
        }

        foreach (var (name, make) in jobs)
        {
            var (samples, rate, pitch, width) = make();
            using var chain = new CwChain(rate);
            var text = new StringBuilder();
            var trace = new List<string>();
            var shares = new Dictionary<(long LaneBin, long OfBin), double>();
            var chunk = rate / 100;

            chain.Detector.SetPassband(pitch, width);
            chain.Detector.LaneHandover = true;
            chain.Detector.HandoverTrace = trace;
            chain.Detector.ShadowShares = shares;
            chain.Decoder.Runs.CharacterRead += c => text.Append(c.Text);

            for (var at = 0; at + chunk <= samples.Length; at += chunk)
            {
                chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));
            }

            chain.Decoder.Flush();

            output.WriteLine($"{name}: reads `{text}`");
            output.WriteLine("  handover: " + string.Join("; ", trace));
            output.WriteLine("  shadows held: " + string.Join("; ", chain.Detector.ShadowsHeld.Select(h => string.Create(Inv, $"{h.AtSeconds:0.0} s lane {h.PitchHz:0} Hz of {h.OfPitchHz:0} Hz share {h.Share:0.00}"))));
            output.WriteLine("  largest shares: " + string.Join("; ", shares.OrderByDescending(s => s.Value).Take(12).Select(s => string.Create(Inv, $"lane {s.Key.LaneBin * CwEnvelopeDetector.BinSpacingHz:0} Hz of {s.Key.OfBin * CwEnvelopeDetector.BinSpacingHz:0} Hz {s.Value:0.00}"))));
        }

        static (float[], int, double, double) Recording(string recording)
        {
            var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
            var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);

            return (audio.Samples, audio.SampleRate, pitch, width);
        }
    }
}

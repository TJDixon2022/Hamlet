using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **A FOUND WEAK STATION PRINTS DITS** (work instruction 574, HM-DEC-278). On the owner's capture of 2026-10-19,
/// `cw-2026-10-19-141708`, Ken answers KB2UZK from 75 Hz below him, about 425 Hz, from about 272 s to the end. The bank stood on him
/// and handed him on, and he printed as `EIE E EIET EI I I E SE ...`. These trace his over through the chain as shipped, and the
/// 5 dB CQ at 18 WPM beside it, which has a truth: the lane that stood, its marks, how the sender gate kinded them, what the reader
/// printed from which marks, and the same audio through a lane opened by hand at the true pitch.
/// </summary>
public sealed class TheFoundWeakStationTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>The owner's capture: a trace recording with no truth.</summary>
    internal const string Ken = "cw-2026-10-19-141708";

    /// <summary>Where Ken's over starts, in seconds of the recording.</summary>
    internal const double KenFrom = 272;

    /// <summary>Lone one- and two-element words, the survey's strays.</summary>
    private static readonly HashSet<string> Strays = new(StringComparer.Ordinal) { "E", "T", "I", "A", "N", "M" };

    /// <summary>One letter as printed: when, its pitch, its text, a word gap before it, and the marks it was read from with their kinds.</summary>
    internal sealed record Letter(double Seconds, double PitchHz, string Text, bool SpaceBefore, string Pattern, IReadOnlyList<CwMark> Marks, double DitSeconds, double SplitSeconds, CwGapLines? Lines);

    /// <summary>What the chain as shipped did over a stretch.</summary>
    internal sealed record Traced(IReadOnlyList<Letter> Letters, IReadOnlyList<string> Handover, IReadOnlyList<CwMark> Handed, IReadOnlyList<(double At, double PitchHz, int Dit, double Shape)> Held);

    /// <summary>Reads audio through the chain as shipped, recording the handover, the marks handed on and each letter with its marks.</summary>
    internal static Traced Run(float[] samples, int rate, double pitchHz, double widthHz)
    {
        using var chain = new CwChain(rate);
        var gate = chain.Decoder.Runs;
        var letters = new List<Letter>();
        var handover = new List<string>();
        var marks = new List<CwMark>();
        var held = new List<(double, double, int, double)>();
        var space = false;
        var chunk = rate / 100;
        long sequence = 0;

        chain.Detector.SetPassband(pitchHz, widthHz);
        chain.Detector.HandoverTrace = handover;
        gate.CharacterRead += c => space |= c.Text == MorseAlphabet.WordGap;
        gate.RunRead += (c, run) =>
        {
            letters.Add(new Letter(run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text, space, c.Pattern, run.ToList(), gate.StationDitSeconds, gate.StationSplitSeconds, gate.StationLines));
            space = false;
        };

        for (var at = 0; at + chunk <= samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, samples.AsSpan(at, chunk)));

            var batch = chain.Detector.MarksSince(sequence);

            if (batch.Marks.Count > 0)
            {
                marks.AddRange(batch.Marks);
                sequence = batch.Marks.Max(m => m.Sequence);
            }

            if ((at / chunk) % 25 == 0 && double.IsFinite(chain.Detector.WeakStationPitchHz) && chain.Detector.Bank is { } bank)
            {
                var now = (at + chunk) / (double)rate;
                var weak = chain.Detector.WeakStationPitchHz;

                foreach (var (dit, s) in bank.Standing(now).Where(x => Math.Abs(x.Sequence.PitchHz - weak) <= CwLaneBank.SpacingHz / 2).Select(x => (x.Dit, x.Sequence)))
                {
                    held.Add((now, s.PitchHz, dit, s.Shape.Score));
                }
            }
        }

        chain.Decoder.Flush();

        return new Traced(letters, handover, marks, held);
    }

    /// <summary>The strays per 10 s among the letters printed in a stretch: lone E, T, I, A, N or M words.</summary>
    internal static double StraysPer10s(IReadOnlyList<Letter> letters, double from, double to)
    {
        var words = new List<string>();
        var word = new StringBuilder();

        foreach (var l in letters.Where(l => l.Seconds >= from && l.Seconds <= to))
        {
            if (l.SpaceBefore && word.Length > 0)
            {
                words.Add(word.ToString());
                word.Clear();
            }

            word.Append(l.Text);
        }

        if (word.Length > 0)
        {
            words.Add(word.ToString());
        }

        return words.Count(Strays.Contains) * 10 / (to - from);
    }

    /// <summary>The text printed in a stretch, words spaced.</summary>
    internal static string TextOf(IEnumerable<Letter> letters)
        => string.Concat(letters.Select((l, i) => (i > 0 && l.SpaceBefore ? " " : string.Empty) + l.Text));

    /// <summary>The marks a lane opened by hand finds: the level every 5 ms at the pitch and dit, and marks by the level rule.</summary>
    internal static (List<(double From, double To)> Marks, double Contrast, double PitchHz) ByHand(float[] samples, int rate, double from, double to, double nearHz, double ditSeconds)
    {
        var start = (int)Math.Max(0, (from - 3) * rate);
        var end = (int)Math.Min(samples.Length, to * rate);
        var slice = samples.AsSpan(start, end - start).ToArray();
        var offset = start / (double)rate;
        (List<(double At, double Db)> Trace, double Up, double Down, double Pitch) best = default;

        for (var hz = nearHz - 25; hz <= nearHz + 25; hz += 5)
        {
            var trace = TheWeakEndTests.OwnWindow(slice, rate, hz, ditSeconds).Select(h => (At: h.At + offset, h.Db)).Where(h => h.At >= from && h.At <= to).ToList();
            var sorted = trace.Select(h => h.Db).Order().ToList();
            var up = sorted[(int)(0.3 * sorted.Count)];
            var down = sorted[(int)(0.9 * sorted.Count)];

            if (best.Trace is null || down - up > best.Down - best.Up)
            {
                best = (trace, up, down, hz);
            }
        }

        return (TheWeakEndTests.LevelMarks(best.Trace!, best.Up, best.Down, ditSeconds), best.Down - best.Up, best.Pitch);
    }

    private static double Pct(IEnumerable<double> values, double p)
    {
        var s = values.Where(double.IsFinite).Order().ToList();

        return s.Count == 0 ? double.NaN : s[(int)Math.Min(s.Count - 1, Math.Floor(p * s.Count))];
    }

    private static string Ms(double seconds) => (seconds * 1000).ToString("0", Inv);

    /// <summary>The largest ratio between neighbouring sorted lengths, and where it falls.</summary>
    private static (double Ratio, double Below, double Above) Jump(IEnumerable<double> lengths)
    {
        var s = lengths.Order().ToList();
        var best = (Ratio: 0.0, Below: double.NaN, Above: double.NaN);

        for (var i = 1; i < s.Count; i++)
        {
            if (s[i - 1] > 0 && s[i] / s[i - 1] > best.Ratio)
            {
                best = (s[i] / s[i - 1], s[i - 1], s[i]);
            }
        }

        return best;
    }

    /// <summary>The four rows for one stretch, written out.</summary>
    private void Report(string name, float[] samples, int rate, double pitchHz, double widthHz, double from, double to, double nearHz)
    {
        var t = Run(samples, rate, pitchHz, widthHz);
        var mine = t.Letters.Where(l => l.Seconds >= from && l.Seconds <= to + 1).ToList();
        var near = mine.Where(l => Math.Abs(l.PitchHz - nearHz) <= 40).ToList();
        var others = mine.Except(near).ToList();

        output.WriteLine($"## {name}, {from:0} to {to:0} s");
        output.WriteLine(string.Create(Inv, $"printed, all: `{TextOf(mine)}`; strays {StraysPer10s(mine, from, to):0.0} per 10 s"));
        output.WriteLine(string.Create(Inv, $"printed near {nearHz:0} Hz: `{TextOf(near)}`; elsewhere: `{TextOf(others)}` at {string.Join(", ", others.Select(l => l.PitchHz.ToString("0", Inv)).Distinct())} Hz"));

        // 1. The lane.
        var held = t.Held.Where(h => h.At >= from - 5 && h.At <= to).ToList();
        output.WriteLine("1. the lane: " + string.Join("; ", t.Handover.Where(h => double.TryParse(h.Split(' ')[0], NumberStyles.Float, Inv, out var s) && s >= from - 10 && s <= to)));
        output.WriteLine("   held, by dit, shape first/last/max: " + string.Join("; ", held.GroupBy(h => (h.Dit, Math.Round(h.PitchHz))).Select(g => string.Create(Inv, $"{g.Key.Item2:0} Hz {CwLaneBank.DitSeconds[g.Key.Dit] * 1000:0} ms dit from {g.First().At:0.0} s, shape {g.First().Shape:0.00}/{g.Last().Shape:0.00}/{g.Max(h => h.Shape):0.00}"))));

        var handed = t.Handed.Where(m => m.FromSeconds >= from && m.FromSeconds <= to && Math.Abs(m.PitchHz - nearHz) <= 40).OrderBy(m => m.FromSeconds).ToList();
        output.WriteLine(string.Create(Inv, $"   handed on near the pitch: {handed.Count} marks at {string.Join(", ", handed.Select(m => m.PitchHz.ToString("0", Inv)).Distinct())} Hz, contrast median {Pct(handed.Select(m => m.ContrastDb), 0.5):0.0} dB, p10 {Pct(handed.Select(m => m.ContrastDb), 0.1):0.0} dB"));

        // 2. The marks as handed on, and their gaps.
        output.WriteLine("2. marks (ms) and [gaps]: " + string.Join(" ", handed.Select((m, i) => (i > 0 ? $"[{Ms(m.FromSeconds - handed[i - 1].ToSeconds)}] " : string.Empty) + Ms(m.ToSeconds - m.FromSeconds))));
        var markJump = Jump(handed.Select(m => m.ToSeconds - m.FromSeconds));
        output.WriteLine(string.Create(Inv, $"   sorted: {string.Join(" ", handed.Select(m => Ms(m.ToSeconds - m.FromSeconds)).Select(double.Parse).Order())}; widest jump {markJump.Ratio:0.00} between {Ms(markJump.Below)} and {Ms(markJump.Above)} ms"));

        // 2b and 3. The kinds the gate gave, letter by letter, and what each letter was made of.
        var kinded = near.Where(l => l.Seconds <= to + 1).ToList();
        var sb = new StringBuilder();
        var dits = new List<double>();
        var dahs = new List<double>();
        var inside = new List<double>();
        var letterGaps = new List<double>();
        var wordGaps = new List<double>();

        for (var i = 0; i < kinded.Count; i++)
        {
            var l = kinded[i];

            if (i > 0)
            {
                var gap = l.Marks[0].FromSeconds - kinded[i - 1].Marks[^1].ToSeconds;

                (l.SpaceBefore ? wordGaps : letterGaps).Add(gap);
                sb.Append(l.SpaceBefore ? $" /w{Ms(gap)}/ " : $" /l{Ms(gap)}/ ");
            }

            sb.Append(l.Text).Append('=');

            for (var k = 0; k < l.Marks.Count; k++)
            {
                var len = l.Marks[k].ToSeconds - l.Marks[k].FromSeconds;
                var kind = k < l.Pattern.Length ? l.Pattern[k] : '?';

                (kind == '-' ? dahs : dits).Add(len);

                if (k > 0)
                {
                    var g = l.Marks[k].FromSeconds - l.Marks[k - 1].ToSeconds;

                    inside.Add(g);
                    sb.Append($"({Ms(g)})");
                }

                sb.Append(kind == '-' ? "dah" : "dit").Append(Ms(len));
            }
        }

        output.WriteLine("3. printed, letter = marks with kinds, (inside gap), /l letter gap/, /w word gap/: " + sb);

        var last = kinded.LastOrDefault();
        output.WriteLine(string.Create(Inv, $"   the gate's sender at its last letter: dit {Ms(last?.DitSeconds ?? double.NaN)} ms, split {Ms(last?.SplitSeconds ?? double.NaN)} ms, letter line {Ms(last?.Lines?.CharacterSeconds ?? double.NaN)} ms, word line {Ms(last?.Lines?.WordSeconds ?? double.NaN)} ms"));
        output.WriteLine(string.Create(Inv, $"   clusters as kinded: dits {dits.Count} centre {Ms(dits.DefaultIfEmpty(double.NaN).Average())} ms, dahs {dahs.Count} centre {Ms(dahs.DefaultIfEmpty(double.NaN).Average())} ms, ratio {dahs.DefaultIfEmpty(double.NaN).Average() / dits.DefaultIfEmpty(double.NaN).Average():0.00}; gaps inside {inside.Count} centre {Ms(inside.DefaultIfEmpty(double.NaN).Average())} ms, letter {letterGaps.Count} centre {Ms(letterGaps.DefaultIfEmpty(double.NaN).Average())} ms, word {wordGaps.Count}; letter over inside {letterGaps.DefaultIfEmpty(double.NaN).Average() / inside.DefaultIfEmpty(double.NaN).Average():0.00} against √3 = 1.73"));
        var gapJump = Jump(handed.Skip(1).Select((m, i) => m.FromSeconds - handed[i].ToSeconds));
        output.WriteLine(string.Create(Inv, $"   among the handed marks' gaps, widest jump {gapJump.Ratio:0.00} between {Ms(gapJump.Below)} and {Ms(gapJump.Above)} ms"));

        // 4. By hand.
        var (hand, contrast, hz) = ByHand(samples, rate, from, to, nearHz, CwLaneBank.DitSeconds[0]);
        var handJump = Jump(hand.Select(m => m.To - m.From));
        output.WriteLine(string.Create(Inv, $"4. by hand at {hz:0} Hz with a 100 ms dit, contrast {contrast:0.0} dB: {hand.Count} marks: {string.Join(" ", hand.Select((m, i) => (i > 0 ? $"[{Ms(m.From - hand[i - 1].To)}] " : string.Empty) + Ms(m.To - m.From)))}"));
        output.WriteLine(string.Create(Inv, $"   sorted: {string.Join(" ", hand.Select(m => (m.To - m.From) * 1000).Order().Select(l => l.ToString("0", Inv)))}; widest jump {handJump.Ratio:0.00} between {Ms(handJump.Below)} and {Ms(handJump.Above)} ms"));
        output.WriteLine(string.Empty);
    }

    /// <remarks>Work instruction 574, task 1: Ken's over, 272 to 300 s, through the chain as shipped and by hand. Asserts nothing.</remarks>
    [Fact]
    public void KensOverTraced()
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Ken));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Ken);

        Report($"`{Ken}`, Ken", audio.Samples, audio.SampleRate, pitch, width, KenFrom, audio.Samples.Length / (double)audio.SampleRate, 425);
    }

    /// <remarks>Work instruction 574, task 1: the 5 dB CQ at 18 WPM, which has a truth, traced the same way. Asserts nothing.</remarks>
    [Fact]
    public void TheCqTraced()
    {
        var r = SyntheticCq.All.Single(c => c.Name == "cq-18wpm-5db");
        var audio = WavAudio.Read(Path.Combine(CwFixtures.Folder, "synthetic-cq", r.Name + ".wav"));
        var sent = TheWeakEndTests.Sent(r);

        output.WriteLine(string.Create(Inv, $"sent: {r.Text} at {r.ToneHz:0} Hz; {sent.Count} marks: {string.Join(" ", sent.Select(m => Ms(m.To - m.From)))}"));
        Report($"`{r.Name}`", audio.Samples, audio.SampleRate, 600, 500, 0, audio.Samples.Length / (double)audio.SampleRate, r.ToneHz);
    }

    /// <summary>One bank lane, the bank's own, hop by hop: its level, key-up and key-down lines, and the marks it ends.</summary>
    internal static (List<(double At, double Level, double Up, double Down)> Hops, List<(double From, double To)> Marks) Lane(float[] samples, int rate, double pitchHz, int dit, double from, double to)
    {
        var hop = Math.Max(4, rate / 200);
        var bank = new CwLaneBank(rate, hop, pitchHz, pitchHz);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var pitch = ((Array)typeof(CwLaneBank).GetField("_pitches", flags)!.GetValue(bank)!).GetValue(0)!;
        var lane = ((Array)pitch.GetType().GetProperty("Lanes")!.GetValue(pitch)!).GetValue(dit)!;
        var type = lane.GetType();
        var step = type.GetMethod("Hop")!;
        var (lastI, lastQ, up, down) = (type.GetField("_lastI", flags)!, type.GetField("_lastQ", flags)!, type.GetField("_up", flags)!, type.GetField("_down", flags)!);
        var hops = new List<(double, double, double, double)>();
        var marks = new List<(double, double)>();
        var start = (int)Math.Max(0, (from - 6) * rate);
        var end = (int)Math.Min(samples.Length, (to + 1) * rate);

        for (var n = start; n < end; n++)
        {
            bank.Push(samples[n]);

            if ((n + 1) % hop != 0)
            {
                continue;
            }

            var now = (n + 1) / (double)rate;
            var span = step.Invoke(lane, [now, hop / (double)rate]);
            var i = (double)lastI.GetValue(lane)!;
            var q = (double)lastQ.GetValue(lane)!;

            hops.Add((now, 10 * Math.Log10((2 * ((i * i) + (q * q))) + 1e-20), (double)up.GetValue(lane)!, (double)down.GetValue(lane)!));

            if (span is ValueTuple<double, double, double, double> s)
            {
                marks.Add((s.Item1, s.Item2));
            }
        }

        return (hops, marks);
    }

    /// <summary>Each reference mark against one lane: found whole, found in pieces, or missed and why.</summary>
    private void Against(string name, float[] samples, int rate, double pitchHz, int dit, double from, double to, List<(double From, double To)> reference, string what)
    {
        var (hops, marks) = Lane(samples, rate, pitchHz, dit, from, to);
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        var lines = new List<string>();

        foreach (var r in reference.Where(r => r.From >= from && r.To <= to))
        {
            var over = marks.Where(m => m.To > r.From && m.From < r.To).ToList();
            var during = hops.Where(h => h.At >= r.From + 0.01 && h.At <= r.To + 0.01).ToList();
            var contrast = during.Count == 0 ? double.NaN : during.Min(h => h.Down - h.Up);
            var peak = during.Count == 0 ? double.NaN : during.Max(h => h.Level - (h.Down - (0.4 * (h.Down - h.Up))));
            var verdict = over.Count == 1 ? "whole"
                : over.Count > 1 ? "pieces"
                : !(contrast >= CwLaneBank.MinContrastDb) ? "missed, lane contrast under 12 dB"
                : peak < 0 ? "missed, never over the start line"
                : "missed, under half a dit or merged";

            counts[verdict] = counts.GetValueOrDefault(verdict) + 1;

            if (verdict != "whole")
            {
                lines.Add(string.Create(Inv, $"{r.From:0.00} s {Ms(r.To - r.From)} ms: {verdict} (least contrast {contrast:0.0} dB, peak {peak:+0.0;-0.0} dB over the start line{(over.Count > 0 ? ", as " + string.Join("+", over.Select(o => Ms(o.To - o.From))) : string.Empty)})"));
            }
        }

        var c = hops.Where(h => h.At >= from && h.At <= to).Select(h => h.Down - h.Up).ToList();
        output.WriteLine(string.Create(Inv, $"{name}: the bank lane at {pitchHz:0} Hz, {CwLaneBank.DitSeconds[dit] * 1000:0} ms dit, against {what}: {string.Join(", ", counts.Select(k => $"{k.Value} {k.Key}"))}; lane contrast median {Pct(c, 0.5):0.0} dB, p10 {Pct(c, 0.1):0.0} dB, under 12 dB {c.Count(x => x < CwLaneBank.MinContrastDb) * 100.0 / Math.Max(1, c.Count):0}% of hops"));

        foreach (var l in lines)
        {
            output.WriteLine("   " + l);
        }
    }

    /// <remarks>Work instruction 574, task 1: the CQ's sent marks and Ken's marks by hand against the bank's own lanes. Asserts nothing.</remarks>
    [Fact]
    public void TheLanesAgainstTheMarks()
    {
        var r = SyntheticCq.All.Single(c => c.Name == "cq-18wpm-5db");
        var cq = WavAudio.Read(Path.Combine(CwFixtures.Folder, "synthetic-cq", r.Name + ".wav"));
        var sent = TheWeakEndTests.Sent(r);
        var cqEnd = cq.Samples.Length / (double)cq.SampleRate;

        for (var d = 0; d < CwLaneBank.DitSeconds.Length; d++)
        {
            Against(r.Name, cq.Samples, cq.SampleRate, 625, d, 0, cqEnd, sent, "the marks sent");
        }

        var ken = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Ken));
        var kenEnd = ken.Samples.Length / (double)ken.SampleRate;
        var (hand, _, hz) = ByHand(ken.Samples, ken.SampleRate, KenFrom, kenEnd, 425, CwLaneBank.DitSeconds[0]);

        for (var d = 0; d < CwLaneBank.DitSeconds.Length; d++)
        {
            Against(Ken, ken.Samples, ken.SampleRate, 425, d, KenFrom, kenEnd, hand, string.Create(Inv, $"the marks by hand at {hz:0} Hz"));
        }
    }

    /// <remarks>
    /// Work instruction 574, task 1: through Ken's over, every second, what the bank stands within a lane of 425 Hz, by dit and
    /// shape, and what the grid holds from 375 to 475 Hz, which keeps a lane from being handed on. Asserts nothing.
    /// </remarks>
    [Fact]
    public void WhyKenIsNotHandedOn()
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Ken));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Ken);
        using var chain = new CwChain(audio.SampleRate);
        var rate = audio.SampleRate;
        var chunk = rate / 100;

        chain.Detector.SetPassband(pitch, width);

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, rate, audio.Samples.AsSpan(at, chunk)));

            var now = (at + chunk) / (double)rate;

            if (now < KenFrom || (at / chunk) % 100 != 0 || chain.Detector.Bank is not { } bank)
            {
                continue;
            }

            var lanes = bank.Standing(now).Where(s => Math.Abs(s.Sequence.PitchHz - 425) <= 30).Select(s => string.Create(Inv, $"{s.Sequence.PitchHz:0}/{CwLaneBank.DitSeconds[s.Dit] * 1000:0} {s.Sequence.Shape.Score:0.00}"));
            var grid = chain.Detector.SequencesNow(now).Split(" | ", StringSplitOptions.RemoveEmptyEntries)
                .Where(d => double.TryParse(d.Split(' ')[0], NumberStyles.Float, Inv, out var p) && p >= 375 && p <= 475);

            output.WriteLine(string.Create(Inv, $"{now:0} s: bank {string.Join(", ", lanes)}; grid {string.Join(" | ", grid)}"));
        }
    }
}

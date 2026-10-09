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
    internal static ChainTrace Chain(float[] samples, int rate, double pitchHz)
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
        output.WriteLine("| file | sent: SNR in 520 Hz, pitch, WPM, marks | wide bin: contrast median, key-up spread | chain: offered (their contrast), stood, sequence marks, shape | window opened, sender (shape, qualified, printed) | own window: contrast median / p10 | level marks found of sent, whole | their shape | dies at | chain read | window read, right of sent |");
        output.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|");

        foreach (var (name, wav, r) in Files())
        {
            output.WriteLine(Row(name, wav, r));
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
            $"| `{name}` | {r.SignalToNoiseDb:0} dB, {r.ToneHz:0} Hz, {1200 / r.DitMilliseconds:0} WPM, {sent.Count} | {wide.Down - wide.Up:0.0} dB, {wide.UpSpread:0.0} dB | {chain.Offered} ({chain.OfferedContrastDb:0.0} dB), {chain.Stood}, {chain.SequenceMarks}, {chain.SequenceShape:0.00} | {(double.IsNaN(chain.FirstWindowSeconds) ? "no window" : $"at {chain.FirstWindowSeconds:0.0} s")}, {(double.IsNaN(chain.FirstSenderSeconds) ? "no sender" : $"sender {chain.SenderShape:0.00}{(chain.Qualified ? " qualified" : string.Empty)}{(chain.Printed ? " printed" : string.Empty)}")} | {own.Down - own.Up:0.0} / {P(own.PerMark, 0.1):0.0} dB | {found.Count} of {sent.Count}, {whole} whole | {shape:0.00} | **{stage}** | `{chain.Text}` | `{windowRead}`, {windowRight} of {outOf} |");
    }
}

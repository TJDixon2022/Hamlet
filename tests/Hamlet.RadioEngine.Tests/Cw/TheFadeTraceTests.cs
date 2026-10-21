using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE FADE, TRACED** (work instruction 582, HM-DEC-286): on two fading fists of the bank and their flat twins, every hop of the
/// bank lane nearest the note at the fist's dit, its key-up and key-down lines and contrast; the station's own window, its key-down
/// (the gate's mean of the sender's last marks) and key-up (the median of its gaps); the fade's true level from the recipe; and for
/// every mark the recipe keyed, whether the lane called it, called it short, or missed it, and whether a mark reached the sender
/// gate over it. One row per fade cycle; the full rows in <c>.run-unit\reports\</c>. Asserts nothing.
/// </summary>
public sealed class TheFadeTraceTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>The fade's period: 1 over 0.7 Hz.</summary>
    private static readonly double Cycle = 1 / CwFixtureCatalogue.QsbHz;

    [Fact]
    public void TheFadingFists()
    {
        foreach (var db in new[] { 5.0, 10.0, 20.0 })
        {
            foreach (var fades in new[] { true, false })
            {
                Trace(new CwFist(2.5, 20, db, fades));
            }
        }
    }

    /// <summary>
    /// **THE HELD LANE'S MARKS, TRACED** (work instruction 583, HM-DEC-287): on the two fading fists, as shipped and with the held
    /// lane's marks its station's and the grid stepping aside, per fade cycle the marks the held lane handed on, how many of them
    /// printed as the station's, how many new senders began within 50 Hz of the note, and the letters printed.
    /// </summary>
    [Fact]
    public void TheHeldLanesMarks()
    {
        foreach (var db in new[] { 10.0, 20.0 })
        {
            foreach (var rule in new[] { false, true })
            {
                Held(new CwFist(2.5, 20, db, true), rule);
            }
        }
    }

    private void Held(CwFist fist, bool rule)
    {
        using var off = rule ? null : CwRules.Off(CwRules.HeldLaneMarksAreItsStations, CwRules.GridStepsAside);
        using var on = rule ? CwRules.Choose(CwRules.HeldLaneMarksAreItsStations, CwRules.GridStepsAside) : null;

        var audio = CwFistBank.Audio(fist);
        var edges = CwFistBank.Edges(fist);
        using var chain = new CwChain(audio.SampleRate);
        var gate = chain.Decoder.Runs;
        var handover = new List<string>();
        var handed = new List<CwMark>();
        var begun = new List<(double Seconds, double PitchHz)>();
        var printed = new List<(double At, string Text, int Held, int Marks)>();
        long sequence = 0;

        chain.Detector.SetPassband(600, 500);
        chain.Detector.HandoverTrace = handover;
        gate.SenderBegunTrace = begun;
        gate.RunRead += (c, run) => printed.Add((run[^1].ToSeconds, c.Text, run.Count(m => m.HeldLane), run.Count));

        var chunk = audio.SampleRate / 100;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));

            var batch = chain.Detector.MarksSince(sequence);

            if (batch.Marks.Count > 0)
            {
                handed.AddRange(batch.Marks);
                sequence = batch.Marks.Max(m => m.Sequence);
            }
        }

        chain.Decoder.Flush();

        var text = string.Concat(printed.Select(p => p.Text));
        bool Near(double hz) => Math.Abs(hz - CwFistBank.ToneHz) <= CwEnvelopeDetector.HandoverHz;

        output.WriteLine($"## {fist.Name}, {(rule ? "the held lane's marks its station's, the grid stepping aside" : "as shipped")}");
        output.WriteLine($"printed: `{text}`");
        output.WriteLine($"handover: {string.Join("; ", handover.Take(10))}{(handover.Count > 10 ? $" … ({handover.Count})" : string.Empty)}");
        output.WriteLine("| cycle | from (s) | keyed | held lane handed on | grid handed on | held marks printed | new senders | letters |");
        output.WriteLine("|---|---|---|---|---|---|---|---|");

        var totals = new int[6];

        for (var c = 0; c * Cycle < edges[^1] + 1; c++)
        {
            var from = c * Cycle;
            var to = from + Cycle;
            var keyed = Enumerable.Range(0, edges.Length / 2).Count(i => edges[2 * i] >= from && edges[2 * i] < to);

            if (keyed == 0)
            {
                continue;
            }

            var lane = handed.Count(m => m.HeldLane && Near(m.PitchHz) && m.FromSeconds >= from && m.FromSeconds < to);
            var grid = handed.Count(m => !m.HeldLane && Near(m.PitchHz) && m.FromSeconds >= from && m.FromSeconds < to);
            var letters = printed.Where(p => p.At >= from && p.At < to).ToList();
            var kept = letters.Sum(p => p.Held);
            var senders = begun.Count(b => Near(b.PitchHz) && b.Seconds >= from && b.Seconds < to);

            totals[0] += keyed;
            totals[1] += lane;
            totals[2] += grid;
            totals[3] += kept;
            totals[4] += senders;
            totals[5] += letters.Count;

            output.WriteLine(string.Create(Inv, $"| {c} | {from:0.0} | {keyed} | {lane} | {grid} | {kept} | {senders} | `{string.Concat(letters.Select(l => l.Text))}` |"));
        }

        output.WriteLine(string.Create(Inv, $"totals: keyed {totals[0]}; the held lane handed on {totals[1]}, the grid {totals[2]}; held marks printed {totals[3]}; new senders {totals[4]}; letters {totals[5]}"));
        output.WriteLine(string.Empty);
    }

    /// <summary>The bank's flat and fading halves, the signal-by-fade matrix at every ratio, with the rule on and off.</summary>
    [Fact]
    public void TheBankByFade()
    {
        foreach (var on in new[] { false, true })
        {
            var rows = new System.Collections.Concurrent.ConcurrentDictionary<string, TheCwSurveyTests.Row>();

            Parallel.ForEach(CwFistBank.All, new ParallelOptions { MaxDegreeOfParallelism = 3 }, f =>
            {
                using var rule = on ? CwRules.Choose(CwRules.FollowsTheFade) : null;
                rows[f.Name] = TheCwSurveyTests.Fist(f);
            });

            int Sum(Func<CwFist, bool> which) => CwFistBank.All.Where(which).Sum(f => rows[f.Name].Right ?? 0);

            output.WriteLine(string.Create(Inv, $"## the rule {(on ? "on" : "off")}: the bank {Sum(_ => true)} of 5760, score {CwFistBank.All.Sum(f => rows[f.Name].Score ?? 0)}, invented {CwFistBank.All.Sum(f => rows[f.Name].Invented ?? 0)}; flat {Sum(f => !f.Fades)}, fading {Sum(f => f.Fades)}"));

            foreach (var ratio in CwFistBank.Ratios)
            {
                output.WriteLine(string.Create(Inv, $"dah {ratio:0.0}: ") + string.Join(" | ", CwFistBank.Signals.Select(db =>
                    string.Create(Inv, $"{db:0} dB flat {Sum(f => f.Ratio == ratio && f.Db == db && !f.Fades)} fade {Sum(f => f.Ratio == ratio && f.Db == db && f.Fades)}"))));
            }
        }
    }

    // The fade's true level at a moment, in dB about the stated signal: half the depth each way.
    private static double FadeDb(double t, bool fades)
        => fades ? CwFixtureCatalogue.QsbDepthDb / 2 * Math.Cos(2 * Math.PI * CwFixtureCatalogue.QsbHz * t) : 0;

    private void Trace(CwFist fist)
    {
        var audio = CwFistBank.Audio(fist);
        var edges = CwFistBank.Edges(fist);
        var downs = Enumerable.Range(0, edges.Length / 2).Select(i => (From: edges[2 * i], To: edges[(2 * i) + 1])).ToList();
        var dit = Array.IndexOf(CwLaneBank.DitSeconds, CwLaneBank.DitSeconds.OrderBy(d => Math.Abs(d - (1.2 / fist.Wpm))).First());
        var lanePitch = Math.Round(CwFistBank.ToneHz / CwLaneBank.SpacingHz) * CwLaneBank.SpacingHz;

        using var chain = new CwChain(audio.SampleRate);
        var gate = chain.Decoder.Runs;
        var probe = new List<(double At, double PitchHz, bool Mark, double From, double To, double LevelDb, double UpDb, double DownDb, double ContrastDb)>();
        var window = new List<(double Seconds, double LaneDb, double SenderDb, double DitSeconds, double DelaySeconds)>();
        var keyUp = new List<(double Seconds, double? KeyUpDb)>();
        var handover = new List<string>();
        var handed = new List<CwMark>();
        var printed = new List<(double At, double PitchHz, string Text)>();
        long sequence = 0;

        chain.Detector.SetPassband(600, 500);
        chain.Detector.HandoverTrace = handover;
        chain.Detector.LaneTrace = window;
        chain.Detector.LaneKeyUpTrace = keyUp;

        if (chain.Detector.Bank is { } bank)
        {
            bank.Probe = (dit, lanePitch);
            bank.ProbeTrace = probe;
        }

        gate.RunRead += (c, run) => printed.Add((run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text));

        var chunk = audio.SampleRate / 100;

        for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
        {
            chain.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan(at, chunk)));

            var batch = chain.Detector.MarksSince(sequence);

            if (batch.Marks.Count > 0)
            {
                handed.AddRange(batch.Marks);
                sequence = batch.Marks.Max(m => m.Sequence);
            }
        }

        chain.Decoder.Flush();

        var laneMarks = probe.Where(p => p.Mark).Select(p => (p.From, p.To)).ToList();
        var near = handed.Where(m => Math.Abs(m.PitchHz - CwFistBank.ToneHz) <= CwEnvelopeDetector.HandoverHz).Select(m => (From: m.FromSeconds, To: m.ToSeconds)).ToList();

        // A key-down is called where a mark overlaps it at 0.7 of its length or more, short where one overlaps it less, missed where none does.
        static string Judge((double From, double To) down, List<(double From, double To)> marks)
        {
            var over = marks.Where(m => m.To > down.From && m.From < down.To).ToList();

            if (over.Count == 0)
            {
                return "missed";
            }

            return over.Max(m => m.To - m.From) >= 0.7 * (down.To - down.From) ? "called" : "short";
        }

        static bool Extra((double From, double To) mark, List<(double From, double To)> downs)
            => !downs.Any(d => d.To > mark.From - 0.02 && d.From < mark.To + 0.02);

        var full = new StringBuilder();
        var text = string.Concat(printed.Select(p => p.Text));

        output.WriteLine($"## {fist.Name}: lane {lanePitch:0} Hz at a {CwLaneBank.DitSeconds[dit] * 1000:0} ms dit");
        output.WriteLine($"printed: `{text}`");
        output.WriteLine("printed at: " + string.Join(" ", printed.Select(p => string.Create(Inv, $"{p.Text}@{p.At:0.00}/{p.PitchHz:0}"))));
        output.WriteLine($"handover: {string.Join("; ", handover.Take(12))}{(handover.Count > 12 ? $" … ({handover.Count})" : string.Empty)}");
        output.WriteLine("| cycle | from (s) | fade at peak / trough (dB) | lane key-down (dB) | lane key-up (dB) | lane contrast (dB) | window key-down (dB) | window key-up (dB) | keyed | lane called / short / missed | gate called / short / missed | lane extra | gate extra (ms) |");
        output.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|");

        full.AppendLine($"## {fist.Name}");
        full.AppendLine("at\tfade dB\tlevel\tup\tdown\tcontrast\twindow level\twindow key-down\twindow key-up");

        foreach (var p in probe.Where((_, i) => i % 50 == 0))
        {
            var w = window.LastOrDefault(x => x.Seconds <= p.At);
            var u = keyUp.LastOrDefault(x => x.Seconds <= p.At);

            full.AppendLine(string.Create(Inv, $"{p.At:0.000}\t{FadeDb(p.At, fist.Fades):0.0}\t{p.LevelDb:0.0}\t{p.UpDb:0.0}\t{p.DownDb:0.0}\t{p.ContrastDb:0.0}\t{w.LaneDb:0.0}\t{w.SenderDb:0.0}\t{u.KeyUpDb:0.0}"));
        }

        var totals = new int[8];
        var troughMissed = 0;
        var troughMarks = 0;
        var peakExtra = 0;
        var extraAll = 0;

        for (var c = 0; c * Cycle < edges[^1] + 1; c++)
        {
            var from = c * Cycle;
            var to = from + Cycle;
            var hops = probe.Where(p => p.At >= from && p.At < to).ToList();
            var win = window.Where(x => x.Seconds >= from && x.Seconds < to).ToList();
            var ups = keyUp.Where(x => x.Seconds >= from && x.Seconds < to && x.KeyUpDb.HasValue).ToList();
            var keyed = downs.Where(d => d.From >= from && d.From < to).ToList();

            if (keyed.Count == 0)
            {
                continue;
            }

            var lane = keyed.Select(d => Judge(d, laneMarks)).ToList();
            var gated = keyed.Select(d => Judge(d, near)).ToList();
            var laneExtra = laneMarks.Where(m => m.From >= from && m.From < to && Extra(m, downs)).ToList();
            var gateExtra = near.Where(m => m.From >= from && m.From < to && Extra(m, downs)).ToList();

            for (var k = 0; k < keyed.Count; k++)
            {
                var trough = FadeDb((keyed[k].From + keyed[k].To) / 2, fist.Fades) < 0;

                troughMarks += trough ? 1 : 0;
                troughMissed += trough && lane[k] != "called" ? 1 : 0;
            }

            peakExtra += laneExtra.Count(m => FadeDb(m.From, fist.Fades) >= 0);
            extraAll += laneExtra.Count;

            string Range(IEnumerable<double> v) => v.Where(double.IsFinite).DefaultIfEmpty(double.NaN).ToList() is var l && l.Count > 0 && double.IsFinite(l[0])
                ? string.Create(Inv, $"{l.Min():0.0} to {l.Max():0.0}")
                : "-";

            totals[0] += keyed.Count;
            totals[1] += lane.Count(j => j == "called");
            totals[2] += lane.Count(j => j == "short");
            totals[3] += lane.Count(j => j == "missed");
            totals[4] += gated.Count(j => j == "called");
            totals[5] += gated.Count(j => j == "short");
            totals[6] += gated.Count(j => j == "missed");
            totals[7] += laneExtra.Count;

            output.WriteLine(string.Create(Inv,
                $"| {c} | {from:0.0} | {(fist.Fades ? $"+6 at {Math.Round(c * Cycle, 1):0.0}, -6 at {from + (Cycle / 2):0.0}" : "flat")} | {Range(hops.Select(h => h.DownDb))} | {Range(hops.Select(h => h.UpDb))} | {Range(hops.Select(h => h.ContrastDb))} | {Range(win.Select(x => x.SenderDb))} | {Range(ups.Select(x => x.KeyUpDb!.Value))} | {keyed.Count} | {lane.Count(j => j == "called")} / {lane.Count(j => j == "short")} / {lane.Count(j => j == "missed")} | {gated.Count(j => j == "called")} / {gated.Count(j => j == "short")} / {gated.Count(j => j == "missed")} | {laneExtra.Count} | {string.Join(" ", gateExtra.Select(m => $"{(m.To - m.From) * 1000:0}"))} |"));
        }

        // Where the gate lost a key-down: by the fade's phase at the mark, and by kind.
        var lost = downs.Where(d => Judge(d, near) != "called").ToList();
        string Phase((double From, double To) d) => FadeDb((d.From + d.To) / 2, fist.Fades) is var f && f >= 3 ? "peak" : f <= -3 ? "trough" : "slope";
        var ditLength = 1.2 / fist.Wpm;
        output.WriteLine(string.Create(Inv, $"the gate lost {lost.Count} of {downs.Count}: by phase {string.Join(", ", lost.GroupBy(Phase).OrderBy(g => g.Key).Select(g => $"{g.Key} {g.Count()} of {downs.Count(d => Phase(d) == g.Key)}"))}; by kind dits {lost.Count(d => d.To - d.From < 1.6 * ditLength)} of {downs.Count(d => d.To - d.From < 1.6 * ditLength)}, dahs {lost.Count(d => d.To - d.From >= 1.6 * ditLength)} of {downs.Count(d => d.To - d.From >= 1.6 * ditLength)}"));
        output.WriteLine("the gate's marks within 50 Hz, 6 to 12 s, start+length/fade dB, against the key-downs: " + string.Join(" ", handed.Where(m => Math.Abs(m.PitchHz - CwFistBank.ToneHz) <= CwEnvelopeDetector.HandoverHz && m.FromSeconds is >= 6 and < 12).OrderBy(m => m.FromSeconds).Select(m => string.Create(Inv, $"{m.FromSeconds:0.000}+{m.LengthMs:0}/{m.PitchHz:0}"))));
        output.WriteLine("key-downs 6 to 12 s: " + string.Join(" ", downs.Where(d => d.From is >= 6 and < 12).Select(d => string.Create(Inv, $"{d.From:0.000}+{(d.To - d.From) * 1000:0}/{FadeDb((d.From + d.To) / 2, fist.Fades):0}"))));

        // How far each mark's level sits from what its sender's recent marks say it should be: the mean of the last three (as the
        // gates judge now), and a line fitted through the last n marks' levels against their time, carried to this mark.
        var seq = handed.Where(m => Math.Abs(m.PitchHz - CwFistBank.ToneHz) <= CwEnvelopeDetector.HandoverHz).OrderBy(m => m.FromSeconds).ToList();
        string Off(Func<int, double> predict)
        {
            var offs = Enumerable.Range(8, Math.Max(0, seq.Count - 8)).Select(i => Math.Abs(seq[i].LevelDb - predict(i))).Where(double.IsFinite).ToList();
            return offs.Count == 0 ? "-" : string.Create(Inv, $"over 3 dB {offs.Count(o => o > 3)} of {offs.Count}, p90 {offs.Order().ElementAt((int)(0.9 * (offs.Count - 1))):0.0} dB");
        }
        double Line(int i, int n)
        {
            var pts = seq.Skip(i - n).Take(n).Select(m => (T: (m.FromSeconds + m.ToSeconds) / 2, L: m.LevelDb)).ToList();
            var mt = pts.Average(p => p.T);
            var ml = pts.Average(p => p.L);
            var sxx = pts.Sum(p => (p.T - mt) * (p.T - mt));
            var slope = sxx > 0 ? pts.Sum(p => (p.T - mt) * (p.L - ml)) / sxx : 0;
            return ml + (slope * (((seq[i].FromSeconds + seq[i].ToSeconds) / 2) - mt));
        }
        output.WriteLine($"level against the last three's mean: {Off(i => seq.Skip(i - 3).Take(3).Average(m => m.LevelDb))}; against the last mark: {Off(i => seq[i - 1].LevelDb)}");
        output.WriteLine("levels: " + string.Join(" ", seq.Where(m => m.FromSeconds is >= 6 and < 12).Select(m => string.Create(Inv, $"{m.FromSeconds:0.00}:{m.LevelDb:0.0}/{FadeDb((m.FromSeconds + m.ToSeconds) / 2, fist.Fades):0}/{m.LengthMs:0}"))));
        foreach (var n in new[] { 3, 4, 5, 6, 8 })
        {
            output.WriteLine($"level against a line through the last {n}: {Off(i => Line(i, n))}");
        }

        // The tolerance grown by the sender's own pace: the median rate its levels moved between its last n marks, times the time
        // since the reference, times c; the reference the last mark's level or the last three's mean.
        double Mid(CwMark m) => (m.FromSeconds + m.ToSeconds) / 2;
        string Paced(int n, double c, bool last)
        {
            var over = 0;
            var all = 0;
            var extras = new List<double>();

            for (var i = n + 1; i < seq.Count; i++)
            {
                var rates = Enumerable.Range(i - n, n).Select(k => Math.Abs(seq[k].LevelDb - seq[k - 1].LevelDb) / Math.Max(0.01, Mid(seq[k]) - Mid(seq[k - 1]))).Order().ToList();
                var rate = rates[rates.Count / 2];
                var refLevel = last ? seq[i - 1].LevelDb : seq.Skip(i - 3).Take(3).Average(m => m.LevelDb);
                var refAt = last ? Mid(seq[i - 1]) : seq.Skip(i - 3).Take(3).Average(Mid);
                var extra = c * rate * (Mid(seq[i]) - refAt);

                extras.Add(extra);
                all++;
                over += Math.Abs(seq[i].LevelDb - refLevel) > 3 + extra ? 1 : 0;
            }

            return all == 0 ? "-" : string.Create(Inv, $"n {n} c {c:0.0} {(last ? "last" : "mean3")}: over {over} of {all}, extra median {extras.Order().ElementAt(extras.Count / 2):0.0} max {extras.Max():0.0} dB");
        }

        foreach (var last in new[] { true, false })
        {
            output.WriteLine("paced: " + string.Join("; ", new[] { (4, 1.0), (8, 1.0), (8, 1.5), (8, 2.0) }.Select(v => Paced(v.Item1, v.Item2, last))));
        }

        // The same, the pace taken as the net rate over the last n marks (a fade moves one way; jitter does not), against the
        // last mark, capped at the swing of the last eight.
        string Net(int n, double c)
        {
            var over = 0;
            var all = 0;
            var extras = new List<double>();

            for (var i = Math.Max(n, 8); i < seq.Count; i++)
            {
                var a = seq[i - n];
                var b = seq[i - 1];
                var rate = Math.Abs(b.LevelDb - a.LevelDb) / Math.Max(0.01, Mid(b) - Mid(a));
                var swing = seq.Skip(i - 8).Take(8).Max(m => m.LevelDb) - seq.Skip(i - 8).Take(8).Min(m => m.LevelDb);
                var extra = Math.Clamp(c * rate * (Mid(seq[i]) - Mid(b)), 0, swing);

                extras.Add(extra);
                all++;
                over += Math.Abs(seq[i].LevelDb - b.LevelDb) > 3 + extra ? 1 : 0;
            }

            return all == 0 ? "-" : string.Create(Inv, $"net n {n} c {c:0.0}: over {over} of {all}, extra median {extras.Order().ElementAt(extras.Count / 2):0.0} p90 {extras.Order().ElementAt((int)(0.9 * (extras.Count - 1))):0.0} dB");
        }

        output.WriteLine("net: " + string.Join("; ", new[] { (3, 1.5), (4, 1.5), (4, 2.0), (5, 2.0), (6, 2.0) }.Select(v => Net(v.Item1, v.Item2))));

        // The net pace against the mean of the last three, as the gates' reference stands, capped at the swing of the last eight.
        string Net3(int n, double c)
        {
            var over = 0;
            var all = 0;
            var extras = new List<double>();

            for (var i = Math.Max(n, 8); i < seq.Count; i++)
            {
                var a = seq[i - n];
                var b = seq[i - 1];
                var rate = Math.Abs(b.LevelDb - a.LevelDb) / Math.Max(0.01, Mid(b) - Mid(a));
                var swing = seq.Skip(i - 8).Take(8).Max(m => m.LevelDb) - seq.Skip(i - 8).Take(8).Min(m => m.LevelDb);
                var three = seq.Skip(i - 3).Take(3).ToList();
                var extra = Math.Clamp(c * rate * (Mid(seq[i]) - three.Average(Mid)), 0, swing);

                extras.Add(extra);
                all++;
                over += Math.Abs(seq[i].LevelDb - three.Average(m => m.LevelDb)) > 3 + extra ? 1 : 0;
            }

            return all == 0 ? "-" : string.Create(Inv, $"n {n} c {c:0.0}: over {over} of {all}, extra median {extras.Order().ElementAt(extras.Count / 2):0.0} p90 {extras.Order().ElementAt((int)(0.9 * (extras.Count - 1))):0.0} dB");
        }

        output.WriteLine("net3: " + string.Join("; ", new[] { (3, 1.0), (4, 1.0), (4, 1.5), (5, 1.0), (5, 1.5), (8, 1.0) }.Select(v => Net3(v.Item1, v.Item2))));

        // The pace as the signed median of the pairwise rates over the last n marks, against the mean of the last three.
        string Signed(int n, double c)
        {
            var over = 0;
            var all = 0;
            var extras = new List<double>();

            for (var i = Math.Max(n, 8); i < seq.Count; i++)
            {
                var rates = Enumerable.Range(i - n + 1, n - 1).Select(k => (seq[k].LevelDb - seq[k - 1].LevelDb) / Math.Max(0.01, Mid(seq[k]) - Mid(seq[k - 1]))).Order().ToList();
                var rate = Math.Abs(rates[rates.Count / 2]);
                var swing = seq.Skip(i - 8).Take(8).Max(m => m.LevelDb) - seq.Skip(i - 8).Take(8).Min(m => m.LevelDb);
                var three = seq.Skip(i - 3).Take(3).ToList();
                var extra = Math.Clamp(c * rate * (Mid(seq[i]) - three.Average(Mid)), 0, swing);

                extras.Add(extra);
                all++;
                over += Math.Abs(seq[i].LevelDb - three.Average(m => m.LevelDb)) > 3 + extra ? 1 : 0;
            }

            return all == 0 ? "-" : string.Create(Inv, $"n {n} c {c:0.0}: over {over} of {all}, extra median {extras.Order().ElementAt(extras.Count / 2):0.0} p90 {extras.Order().ElementAt((int)(0.9 * (extras.Count - 1))):0.0} dB");
        }

        output.WriteLine("signed: " + string.Join("; ", new[] { (4, 1.5), (5, 1.5), (6, 1.5), (8, 1.5), (5, 2.0), (6, 2.0) }.Select(v => Signed(v.Item1, v.Item2))));
        output.WriteLine("median, mean3: " + string.Join("; ", new[] { (8, 1.5), (6, 1.5), (8, 1.0) }.Select(v => Paced(v.Item1, v.Item2, false))));

        output.WriteLine(string.Create(Inv, $"totals: keyed {totals[0]}; the lane called {totals[1]}, short {totals[2]}, missed {totals[3]}; the gate called {totals[4]}, short {totals[5]}, missed {totals[6]}; lane extra {totals[7]}"));

        if (fist.Fades)
        {
            // How far the lane's key-down lags the fade: the shift of the true fade that best matches the line, in 5 ms hops.
            var line = probe.Where(p => double.IsFinite(p.DownDb)).ToList();
            var best = (Lag: 0.0, R: double.NegativeInfinity);

            for (var lag = 0.0; lag <= Cycle; lag += 0.01)
            {
                var r = Correlation(line.Select(p => p.DownDb).ToList(), line.Select(p => FadeDb(p.At - lag, true)).ToList());

                if (r > best.R)
                {
                    best = (lag, r);
                }
            }

            var swing = line.Where(p => p.At > 5).Select(p => p.DownDb).ToList();
            var winLine = window.Where(x => double.IsFinite(x.SenderDb)).ToList();
            var winBest = (Lag: 0.0, R: double.NegativeInfinity);

            for (var lag = 0.0; lag <= Cycle && winLine.Count > 0; lag += 0.01)
            {
                var r = Correlation(winLine.Select(p => p.SenderDb).ToList(), winLine.Select(p => FadeDb(p.Seconds - lag, true)).ToList());

                if (r > winBest.R)
                {
                    winBest = (lag, r);
                }
            }

            var winSwing = winLine.Select(x => x.SenderDb).ToList();

            output.WriteLine(string.Create(Inv,
                $"the lane's key-down lags the fade by {best.Lag * 1000:0} ms (r {best.R:0.00}) and swings {(swing.Count > 0 ? swing.Max() - swing.Min() : double.NaN):0.0} dB against the fade's 12; the window's key-down lags it by {winBest.Lag * 1000:0} ms (r {winBest.R:0.00}) and swings {(winSwing.Count > 0 ? winSwing.Max() - winSwing.Min() : double.NaN):0.0} dB; marks keyed in a trough the lane did not call whole: {troughMissed} of {troughMarks}; lane marks over no key-down: {extraAll}, at a peak {peakExtra}"));
        }

        output.WriteLine(string.Empty);

        var reports = Path.Combine(Root(), ".run-unit", "reports");

        if (Directory.Exists(reports))
        {
            File.WriteAllText(Path.Combine(reports, $"unit582-trace-{fist.Name}.tsv"), full.ToString());
        }
    }

    private static double Correlation(List<double> a, List<double> b)
    {
        var ma = a.Average();
        var mb = b.Average();
        var num = a.Zip(b, (x, y) => (x - ma) * (y - mb)).Sum();
        var den = Math.Sqrt(a.Sum(x => (x - ma) * (x - ma)) * b.Sum(y => (y - mb) * (y - mb)));

        return den > 0 ? num / den : double.NaN;
    }

    private static string Root()
    {
        var at = new DirectoryInfo(AppContext.BaseDirectory);

        while (at is not null && !File.Exists(Path.Combine(at.FullName, "Hamlet.sln")))
        {
            at = at.Parent;
        }

        return at?.FullName ?? throw new InvalidOperationException("no Hamlet.sln above the test binary");
    }
}

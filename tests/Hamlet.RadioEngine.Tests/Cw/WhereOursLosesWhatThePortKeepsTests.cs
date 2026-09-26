using System.Globalization;
using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every sure letter ours prints wrong where the fldigi port reads the same key
/// letter right, traced to where the lost element went in our decoder (work
/// instruction 459, task 1; PHASE_PLAN.md 9.4; HM-REQ-129 and HM-REQ-010).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT EITHER DECODER.** It asserts only
/// that its copy of our lattice spells what the decoder spelled, read by read, so
/// the segments it prints are the decoder's own.</para>
/// <para>**BOTH DECODERS AS <see cref="BothDecodersAreScoredAlikeTests"/> SCORES
/// THEM**, the same stretches and alignments; ours is then driven again, hop by
/// hop exactly as there, recording at each settled letter the window, the read's
/// speed, the gaps it was given and the pitch it mixed at. **EVERY REAL KEY IS
/// INFERRED** (V-13); **EVERY SYNTHETIC KEY IS EXACT** (12.5).</para>
/// <para>**THE PRINT-LESS WINS ARE LEFT OUT BY NAME**: 032129 and 004108, where
/// the port wins by printing less (458's task 3). The 0 dB cases and 17:37's
/// opening fall out by the rule itself: ours prints nothing sure-wrong there that
/// the port reads right.</para>
/// </remarks>
public sealed class WhereOursLosesWhatThePortKeepsTests
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <summary>The recordings the port wins on only by printing less (work instruction 459, task 1).</summary>
    private static readonly string[] PrintLess =
    {
        "unadjudicated/cw-2026-08-22-032129",
        "unadjudicated/cw-2026-09-24-004108",
    };

    // The lattice's constants, copied from CwProbabilisticDecoder where they are private;
    // the sameness count below fails the copy if they drift.
    private const double ShortestShare = 0.45;
    private const double LongestShare = 2.2;
    private const double LengthToleranceShare = 0.35;
    private static readonly (int Units, bool Down, string Token)[] Kinds =
    {
        (1, true, "."), (3, true, "-"), (1, false, ""), (3, false, "|"), (7, false, " "),
    };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhereOursLosesWhatThePortKeepsTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>What one read of the stream held when a letter settled.</summary>
    internal sealed record Reading(
        double[] Window, long WindowStartHop, CwProbabilisticResult Last, double[]? GapMs, bool UnitMeasured, double ToneHz);

    /// <summary>One segment of the winning path: which kind, and its hops in the window.</summary>
    internal readonly record struct Segment(int Kind, int Start, int End)
    {
        public bool IsMark => Kind <= 1;

        public int Hops => End - Start;
    }

    /// <summary>One letter on the winning path, before any letter was judged.</summary>
    internal sealed record PathLetter(string Pattern, int Start, int End, double SpanRatio, IReadOnlyList<Segment> Segments, Segment? After)
    {
        public double PerHop => End - Start <= 0 ? 0 : SpanRatio / (End - Start);

        public bool Printed => PerHop >= CwProbabilisticDecoder.CharacterMargin
            && !(Pattern.Length == 1 && SpanRatio < CwProbabilisticDecoder.StrayElementSpan);
    }

    /// <summary>Ours driven as <see cref="BothDecodersAreScoredAlikeTests"/> drives it, with the read behind each letter kept.</summary>
    internal static (List<CwCharacter> Settled, List<Reading> Readings) Drive(string path, double startingHz)
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var t = typeof(CwProbabilisticStream);
        var envelopeField = t.GetField("_envelope", flags)!;
        var countField = t.GetField("_envelopeCount", flags)!;
        var hopsField = t.GetField("_hopsSeen", flags)!;
        var heldField = t.GetField("_structureHeld", flags)!;
        var gapsField = t.GetField("_heldGaps", flags)!;
        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, startingHz);
        var settled = new List<CwCharacter>();
        var readings = new List<Reading>();

        decoder.CharacterSettled += c =>
        {
            var s = decoder.Stream;
            var count = (int)countField.GetValue(s)!;
            var window = ((double[])envelopeField.GetValue(s)!).Take(count).ToArray();
            var held = (bool)heldField.GetValue(s)!;
            var gaps = (CwUnitEstimator.CwGapLengths)gapsField.GetValue(s)!;

            settled.Add(c);
            readings.Add(new Reading(
                window, (long)hopsField.GetValue(s)! - count, s.Last,
                held ? new[] { gaps.ElementMilliseconds, gaps.CharacterMilliseconds, gaps.WordMilliseconds } : null,
                s.UnitWasMeasured, s.ToneHz));
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return (settled, readings);
    }

    /// <summary>The winning path at one speed, segment by segment: CwProbabilisticDecoder.DecodeAt, copied to keep what it throws away.</summary>
    internal static IReadOnlyList<PathLetter> Path(double[] keyDown, double[] keyUp, double wpm, double[]? gapMs)
    {
        var count = keyDown.Length;
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var unit = 1200.0 / wpm / hopMs;
        var downTo = new double[count + 1];
        var upTo = new double[count + 1];

        for (var i = 0; i < count; i++)
        {
            downTo[i + 1] = downTo[i] + keyDown[i];
            upTo[i + 1] = upTo[i] + keyUp[i];
        }

        var best = new double[count + 1];
        var fromHop = new int[count + 1];
        var kindAt = new int[count + 1];
        var wasDown = new bool[count + 1];

        Array.Fill(best, double.NegativeInfinity);
        Array.Fill(fromHop, -1);
        best[0] = 0;

        for (var i = 1; i <= count; i++)
        {
            for (var k = 0; k < Kinds.Length; k++)
            {
                var want = gapMs is not null && !Kinds[k].Down ? gapMs[k - 2] / hopMs : Kinds[k].Units * unit;
                var shortest = Math.Max(1, (int)(want * ShortestShare));
                var longest = Math.Max(shortest + 1, (int)(want * LongestShare));
                var ceiling = Math.Min(longest, i);

                for (var span = shortest; span <= ceiling; span++)
                {
                    var j = i - span;

                    if (double.IsNegativeInfinity(best[j]) || (j > 0 && wasDown[j] == Kinds[k].Down))
                    {
                        continue;
                    }

                    var evidence = Kinds[k].Down ? downTo[i] - downTo[j] : upTo[i] - upTo[j];
                    var off = Math.Log(Math.Max(span, 1e-9) / want) / LengthToleranceShare;
                    var score = best[j] + evidence - (0.5 * off * off);

                    if (score > best[i])
                    {
                        best[i] = score;
                        fromHop[i] = j;
                        kindAt[i] = k;
                        wasDown[i] = Kinds[k].Down;
                    }
                }
            }
        }

        var path = new List<Segment>();

        for (var at = count; at > 0 && fromHop[at] >= 0; at = fromHop[at])
        {
            path.Add(new Segment(kindAt[at], fromHop[at], at));
        }

        path.Reverse();

        var letters = new List<PathLetter>();
        var segments = new List<Segment>();
        var pattern = "";
        var ratio = 0.0;

        foreach (var s in path)
        {
            if (s.IsMark)
            {
                pattern += Kinds[s.Kind].Token;
                ratio += downTo[s.End] - downTo[s.Start] - (upTo[s.End] - upTo[s.Start]);
                segments.Add(s);
                continue;
            }

            if (s.Kind == 2)
            {
                if (pattern.Length > 0)
                {
                    segments.Add(s);
                }

                continue;
            }

            if (pattern.Length > 0)
            {
                letters.Add(new PathLetter(pattern, segments[0].Start, segments[^1].End, ratio, segments.ToList(), s));
            }

            pattern = "";
            ratio = 0;
            segments.Clear();
        }

        if (pattern.Length > 0)
        {
            letters.Add(new PathLetter(pattern, segments[0].Start, segments[^1].End, ratio, segments.ToList(), null));
        }

        return letters;
    }

    /// <summary>Runs of hops where the key-down log-likelihood beats key-up: where our model says a mark is, before any length is asked.</summary>
    internal static IReadOnlyList<(int Start, int End)> MarkRuns(double[] keyDown, double[] keyUp, int from, int to)
    {
        var runs = new List<(int, int)>();
        var start = -1;

        for (var i = Math.Max(0, from); i < Math.Min(keyDown.Length, to); i++)
        {
            var down = keyDown[i] > keyUp[i];

            if (down && start < 0)
            {
                start = i;
            }
            else if (!down && start >= 0)
            {
                runs.Add((start, i));
                start = -1;
            }
        }

        if (start >= 0)
        {
            runs.Add((start, Math.Min(keyDown.Length, to)));
        }

        return runs;
    }

    /// <summary>One departure: ours sure and wrong where the port is sure and right on the same key letter.</summary>
    internal sealed record Departure(
        string Name, string Set, string Kind, int Stretch, int KeyIndex, string Key, CwCharacter Ours, string OursNote, string PortText, string PortNote,
        string Group, string Why, IReadOnlyList<string> Lines)
    {
        /// <summary>Our mix pitch at the read less the pitch the port was handed (the instrument's median).</summary>
        public double PitchOffHz { get; init; } = double.NaN;

        /// <summary>More than 25 Hz off: the port was handed the pitch, which no mechanism of its signal path is.</summary>
        public bool OffPitch => Math.Abs(PitchOffHz) > 25;

        /// <summary>Where ours stands in what it settled.</summary>
        public int Index { get; init; } = -1;

        /// <summary>Our own lattice, at the port's speed on the same window, reads the key letter there.</summary>
        public bool SpeedCaused { get; init; }

        /// <summary>The port mechanism that keeps what ours lost here.</summary>
        public string Behind => SpeedCaused ? "D" : Group switch { "a" => "A", "b" => "B", "c" => "C", "d" => "D", "x" => "-", _ => "?" };
    }

    /// <summary>
    /// A stretch's envelope against the noise scale and keyed level our model scores
    /// it by, taken as CwProbabilisticDecoder.Estimate takes them: the 2.5 s around
    /// it, sigma from the quarter point, the keyed level from the 97th percentile.
    /// </summary>
    internal static string Level(double[] window, int start, int end)
    {
        var span = (int)(CwProbabilisticDecoder.NoiseSpanSeconds * 1000.0 / CwProbabilisticDecoder.HopMilliseconds);
        var mid = (start + end) / 2;
        var from = Math.Clamp(mid - span / 2, 0, Math.Max(0, window.Length - span));
        var around = window.Skip(from).Take(span).OrderBy(v => v).ToArray();
        var sigma = Pct(around, 25) / CwProbabilisticDecoder.RayleighQuarterPoint;
        var keyed = Math.Max(Pct(around, 97), sigma * 1.05);
        var inside = window.Skip(start).Take(Math.Max(1, end - start)).OrderBy(v => v).ToArray();

        return string.Create(Invariant,
            $"median {Pct(inside, 50) / sigma:0.0} sigma, peak {inside[^1] / sigma:0.0}, keyed level {keyed / sigma:0.0}");
    }

    private static double Pct(double[] sorted, double percent)
    {
        if (sorted.Length == 0)
        {
            return double.NaN;
        }

        var at = percent / 100.0 * (sorted.Length - 1);
        var low = (int)at;
        var high = Math.Min(low + 1, sorted.Length - 1);

        return sorted[low] + ((sorted[high] - sorted[low]) * (at - low));
    }

    private static string PatternOf(string text)
        => MorseAlphabet.All.FirstOrDefault(kv => kv.Value == text).Key ?? "?";

    private static string U(double hops, double unitHops) => (hops / unitHops).ToString("0.00", Invariant) + "u";

    /// <summary>Every departure over every row, traced.</summary>
    internal static IReadOnlyList<Departure> Trace(out int sameness, out int readsChecked)
    {
        var departures = new List<Departure>();
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        sameness = 0;
        readsChecked = 0;

        foreach (var row in BothDecodersAreScoredAlikeTests.Rows)
        {
            if (PrintLess.Contains(row.Name) || row.PortMeasured is not { NotComputable: null } pm || row.Port is null
                || row.OursMeasured.NotComputable is not null)
            {
                continue;
            }

            var found = new List<(int Stretch, CwMetricStep Step, CwCharacter Ours, CwCharacter Port, string PortNote)>();

            for (var i = 0; i < pm.Scores.Count && i < row.OursMeasured.Scores.Count; i++)
            {
                var ours = Steps(row.Ours, row.OursMeasured, i);
                var port = Steps(row.Port, pm, i);

                foreach (var (step, c) in ours)
                {
                    if (step.Key is not { } key || step.Decoded is not { Class: CwSymbolClass.Sure } d || d.Text == key)
                    {
                        continue;
                    }

                    var match = port.Where(p => p.Step.Key == key && p.Step.KeyBefore == step.KeyBefore
                        && p.Step.Decoded is { Class: CwSymbolClass.Sure } pd && pd.Text == key).ToList();

                    if (match.Count == 1)
                    {
                        var note = row.Port.Notes[IndexOf(row.Port.Settled, match[0].Character)];
                        found.Add((i, step, c, match[0].Character, note));
                    }
                }
            }

            if (found.Count == 0)
            {
                continue;
            }

            var file = System.IO.Path.Combine(CwToneSurveyTests.RepositoryRoot(), row.AudioFile);
            var (settled, readings) = Drive(file, row.Real ? 600 : SyntheticCq.StartingPitchHz);
            var sampleRate = WavAudio.Read(file).SampleRate;

            if (settled.Count != row.Ours.Settled.Count
                || !settled.Select(c => c.Text).SequenceEqual(row.Ours.Settled.Select(c => c.Text)))
            {
                throw new InvalidOperationException($"{row.Name}: the second drive settled differently from the scored one");
            }

            foreach (var (stretch, step, oursChar, portChar, portNote) in found)
            {
                var index = IndexOf(row.Ours.Settled, oursChar);
                var r = readings[index];
                var c = settled[index];
                var (keyDown, keyUp) = CwProbabilisticDecoder.LogLikelihoods(r.Window);
                var wpm = r.Last.WordsPerMinute;
                var letters = Path(keyDown, keyUp, wpm, r.GapMs);

                // Sameness: every letter the read printed is on the copied path, at its hop, with its pattern.
                readsChecked++;
                var printed = r.Last.Characters.Where(x => x.Pattern.Length > 0).ToList();
                var onPath = letters.Where(l => l.Printed).ToList();

                if (printed.Count == onPath.Count && printed.Zip(onPath).All(z => z.First.Pattern == z.Second.Pattern && z.First.EndHop == z.Second.End))
                {
                    sameness++;
                }

                var endLocal = (int)(Math.Round(c.At.TotalMilliseconds / hopMs) - r.WindowStartHop);
                var at = letters.Select((l, n) => (l, n)).FirstOrDefault(x => x.l.End == endLocal && x.l.Pattern == c.Pattern);
                var lines = new List<string>();
                var unitHops = 1200.0 / wpm / hopMs;
                var unitMs = 1200.0 / wpm;
                var keyPattern = PatternOf(step.Key!);
                var samplesPerHop = sampleRate * hopMs / 1000.0;

                string Seg(Segment s)
                {
                    var name = s.Kind switch { 0 => "dit", 1 => "dah", 2 => "element gap", 3 => "letter gap", _ => "word gap" };

                    return string.Create(Invariant,
                        $"{name} {s.Hops * hopMs:0} ms/{s.Hops * samplesPerHop:0} samples/{U(s.Hops, unitHops)} at {(r.WindowStartHop + s.Start) * hopMs / 1000.0:0.000} s, {Level(r.Window, s.Start, s.End)}");
                }

                if (at.l is null)
                {
                    lines.Add("not found on the copied path at its settled hop");
                    departures.Add(new Departure(row.Name, row.Set, CwMetrics.KindWord(row.Kind), stretch, step.KeyBefore, step.Key!, c,
                        $"pattern {c.Pattern} at {wpm:0.0} WPM", portChar.Text, portNote, "?", "not found", lines));
                    continue;
                }

                var here = at.l;
                var prev = at.n > 0 ? letters[at.n - 1] : null;
                var next = at.n + 1 < letters.Count ? letters[at.n + 1] : null;

                // The region: from the start of the letter before to the end of the letter after.
                var from = prev?.Start ?? Math.Max(0, here.Start - (int)(7 * unitHops));
                var to = next?.End ?? Math.Min(r.Window.Length, here.End + (int)(7 * unitHops));
                var runs = MarkRuns(keyDown, keyUp, from, to);
                var pathMarks = letters.SelectMany(l => l.Segments).Where(s => s.IsMark).ToList();
                var uncovered = runs.Where(run => !pathMarks.Any(m => m.Start < run.End && run.Start < m.End)).ToList();
                var nearUncovered = uncovered.Where(run => run.End > here.Start - (int)(3 * unitHops) && run.Start < here.End + (int)(3 * unitHops)).ToList();
                var merged = here.Segments.Where(s => s.IsMark && runs.Count(run => run.Start < s.End && s.Start < run.End && run.End - run.Start >= 2) > 1).ToList();

                var gapHold = r.GapMs is { } g
                    ? string.Create(Invariant, $"held gaps {g[0]:0}/{g[1]:0}/{g[2]:0} ms, element|letter at {Math.Sqrt(g[0] * g[1]):0} ms ({U(Math.Sqrt(g[0] * g[1]) / hopMs, unitHops)}), letter|word at {Math.Sqrt(g[1] * g[2]):0} ms")
                    : string.Create(Invariant, $"textbook gaps, element|letter at {Math.Sqrt(3) * unitMs:0} ms (1.73u), letter|word at {Math.Sqrt(21) * unitMs:0} ms (4.58u)");

                lines.Add(string.Create(Invariant,
                    $"in force: {wpm:0.0} WPM ({(r.UnitMeasured ? "measured from the window" : "won on the grid")}), unit {unitMs:0.0} ms = {unitHops:0.0} hops = {unitHops * samplesPerHop:0} samples at {sampleRate} Hz, pitch {r.ToneHz:0.0} Hz"));
                lines.Add(string.Create(Invariant,
                    $"thresholds: mark where key-down log-likelihood beats key-up, hop by hop; dit|dah at 1.73u ({Math.Sqrt(3) * unitMs:0} ms); shortest dit the lattice holds {Math.Max(1, (int)(unitHops * ShortestShare)) * hopMs:0} ms, shortest dah {Math.Max(1, (int)(3 * unitHops * ShortestShare)) * hopMs:0} ms; {gapHold}; printed only at {CwProbabilisticDecoder.CharacterMargin:0.0} per hop and, one element alone, a span of {CwProbabilisticDecoder.StrayElementSpan:0}"));

                foreach (var (label, l) in new[] { ("before", prev), ("this", here), ("after", next) })
                {
                    if (l is null)
                    {
                        continue;
                    }

                    lines.Add(string.Create(Invariant,
                        $"path {label}: `{MorseAlphabet.Lookup(l.Pattern) ?? "#"}` {l.Pattern} {(l.Printed ? "printed" : "NOT printed")} ({l.PerHop:0.00} per hop, span {l.SpanRatio:0.0}) | {string.Join(" + ", l.Segments.Select(Seg))}{(l.After is { } a ? " | then " + Seg(a) : "")}"));
                }

                lines.Add("mark runs (key-down beats key-up) over the region: " + string.Join(", ", runs.Select(run => string.Create(Invariant,
                    $"{(run.End - run.Start) * hopMs:0} ms/{U(run.End - run.Start, unitHops)} at {(r.WindowStartHop + run.Start) * hopMs / 1000.0:0.000}{(uncovered.Contains(run) ? " UNUSED" : "")}"))));

                // Where the element went.
                string group;
                string why;
                var speedCaused = false;
                (PathLetter? Piece, int Gap) split = prev is not null && prev.Pattern + here.Pattern == keyPattern ? (prev, here.Start - prev.End)
                    : next is not null && here.Pattern + next.Pattern == keyPattern ? (next, next.Start - here.End)
                    : (null, 0);

                // The same window, our own lattice, at the speed the port held for this letter.
                var portWpm = (double)portChar.WordsPerMinute;
                var atPortSpeed = portWpm > 0 ? Path(keyDown, keyUp, portWpm, r.GapMs) : Array.Empty<PathLetter>();
                var slack = (int)(2 * unitHops);
                var readAtPort = atPortSpeed.FirstOrDefault(l => l.Pattern == keyPattern && l.End > here.Start - slack && l.Start < here.End + slack);

                var reread = string.Join(" ", atPortSpeed.Where(l => l.End > from && l.Start < to)
                    .Select(l => $"`{MorseAlphabet.Lookup(l.Pattern) ?? "#"}` {l.Pattern}{(l.Printed ? "" : " (not printed)")}"));
                var verdict = readAtPort is null ? " - the key letter is not read there" : $" - reads `{step.Key}` there{(readAtPort.Printed ? "" : ", not printed")}";

                lines.Add(string.Create(Invariant, $"our lattice on the same window at the port's {portWpm:0} WPM: {reread}{verdict}"));

                if (readAtPort is not null && Math.Abs(wpm - portWpm) / portWpm > 0.1)
                {
                    group = here.Pattern.Length == keyPattern.Length ? "c" : "d";
                    speedCaused = true;
                    why = string.Create(Invariant,
                        $"read at {wpm:0.0} WPM where the port held {portWpm:0}: our own lattice on the same window at {portWpm:0} WPM reads `{step.Key}` {keyPattern} there, so every element was keyed and the speed {(group == "c" ? "classed them" : "cut the letter")}: {here.Pattern} for {keyPattern}");
                }
                else if (here.Pattern.Length > keyPattern.Length)
                {
                    group = "x";
                    why = $"an element added: {here.Pattern} for {keyPattern}";
                }
                else if (split.Piece is { } piece)
                {
                    group = "d";
                    why = string.Create(Invariant,
                        $"the rest ({piece.Pattern}) is the next or last letter on the path, {(piece.Printed ? "printed" : "not printed")}: the gap between, {split.Gap * hopMs:0} ms = {U(split.Gap, unitHops)}, was read as a letter gap at {wpm:0.0} WPM");
                }
                else if (here.Pattern.Length == keyPattern.Length)
                {
                    group = "c";
                    why = $"as many elements, classed otherwise: {here.Pattern} for {keyPattern}";
                }
                else if (merged.Count > 0)
                {
                    group = "c";
                    why = $"two mark runs read as one {(merged[0].Kind == 1 ? "dah" : "dit")}: {here.Pattern} for {keyPattern}";
                }
                else if (nearUncovered.Count > 0 || (prev is { Printed: false } && prev.End > here.Start - (int)(3 * unitHops))
                         || (next is { Printed: false } && next.Start < here.End + (int)(3 * unitHops)))
                {
                    group = "b";
                    why = nearUncovered.Count > 0
                        ? string.Create(Invariant, $"{nearUncovered.Count} mark run(s) within 3 units the path took as key-up: {string.Join(", ", nearUncovered.Select(u => U(u.End - u.Start, unitHops)))}")
                        : "the rest is on the path in a letter not printed";
                }
                else
                {
                    group = "a";
                    why = $"no mark run nearby for the missing element(s): {here.Pattern} for {keyPattern}";
                }

                departures.Add(new Departure(row.Name, row.Set, CwMetrics.KindWord(row.Kind), stretch, step.KeyBefore, step.Key!, c,
                    string.Create(Invariant, $"pattern {c.Pattern} at {wpm:0.0} WPM, mixed at {r.ToneHz:0.0} Hz"), portChar.Text,
                    string.Create(Invariant, $"{portNote}, given {row.GivenPitchHz:0.0} Hz"), group, why, lines)
                {
                    PitchOffHz = r.ToneHz - row.GivenPitchHz,
                    SpeedCaused = speedCaused,
                    Index = index,
                });
            }
        }

        return departures;
    }

    private static List<(CwMetricStep Step, CwCharacter Character)> Steps(
        BothDecodersAreScoredAlikeTests.Decoded run, TheRequirementsAreMeasuredTests.Measured m, int stretch)
    {
        var covered = TheRequirementsAreMeasuredTests.Covered(run.Settled, m.Scores[stretch]).Where(c => !c.IsWordGap).ToList();
        var list = new List<(CwMetricStep, CwCharacter)>();
        var d = 0;

        foreach (var step in m.Stretches[stretch].Steps)
        {
            if (step.Decoded is not null)
            {
                list.Add((step, covered[d++]));
            }
        }

        return list;
    }

    private static int IndexOf(IReadOnlyList<CwCharacter> list, CwCharacter c)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], c))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>The port mechanism, from 458's list, that keeps what each group loses (cw.cxx at 61b97f41).</summary>
    internal static readonly IReadOnlyDictionary<string, string> Mechanism = new Dictionary<string, string>
    {
        ["a"] = "(A) detection: hysteresis between the tracked noise floor and AGC peak - cw.cxx:610-623, 640-641, 649-656",
        ["b"] = "(B) spike rule: only a key-down shorter than half a dot at the tracked speed is dropped - cw.cxx:515, 818-822",
        ["c"] = "(C) classing: a dot is anything up to two_dots, a dash longer - cw.cxx:846-855",
        ["d"] = "(D)/(E) speed tracking and gap rule: two_dots follows dot-dash pairs through a 16-sample average, and a letter ends only after 2 dot lengths of key-up - cw.cxx:524-535, 831-843, 502, 880-888",
        ["x"] = "none of (A) to (E) by itself: an element ours added",
        ["?"] = "not traced",
    };

    /// <summary>
    /// The speed the port's pair rule holds over a run of marks in order: a mark under
    /// half the held dot is a spike and skipped; each mark more than two and less than
    /// four times the one before it, or the reverse, puts their mean into a 16-long
    /// moving average that fills with its first value (cw.cxx:524-535, 815-843; Cmovavg).
    /// </summary>
    internal static (double Wpm, int Pairs) PairSpeed(IReadOnlyList<double> marksMs)
    {
        var filter = new Queue<double>();
        var last = 0.0;
        var twoDots = double.NaN;
        var pairs = 0;

        foreach (var mark in marksMs)
        {
            if (!double.IsNaN(twoDots) && mark < twoDots / 4)
            {
                continue;
            }

            if (last > 0)
            {
                var (dot, dash) = mark > 2 * last && mark < 4 * last ? (last, mark)
                    : last > 2 * mark && last < 4 * mark ? (mark, last)
                    : (0.0, 0.0);

                if (dot > 0)
                {
                    var two = (dot + dash) / 2;

                    if (filter.Count == 0)
                    {
                        for (var i = 0; i < 16; i++)
                        {
                            filter.Enqueue(two);
                        }
                    }
                    else
                    {
                        filter.Dequeue();
                        filter.Enqueue(two);
                    }

                    twoDots = filter.Average();
                    pairs++;
                }
            }

            last = mark;
        }

        return (double.IsNaN(twoDots) ? double.NaN : 1200.0 / (twoDots / 2), pairs);
    }

    /// <remarks>
    /// Proves nothing about either decoder; work instruction 459 task 2's evidence
    /// before any change: at each departure's read, the window's short mark and
    /// short gap clusters that set our speed, the unit its marks imply, and the
    /// speed the port's pair rule would hold over the same window's marks in order.
    /// </remarks>
    [Fact]
    public void WhatSetTheSpeedAtEachDeparture()
    {
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;

        _output.WriteLine("speedset | recording | key | ours | wpm read | short mark ms | short gap ms | window's measured wpm | marks' wpm | pairs' wpm | pairs | port wpm");

        var all = Trace(out _, out _);

        foreach (var row in BothDecodersAreScoredAlikeTests.Rows.Where(r => r.Port is not null))
        {
            var departures = all.Where(d => d.Name == row.Name).ToList();

            if (departures.Count == 0)
            {
                continue;
            }

            var file = System.IO.Path.Combine(CwToneSurveyTests.RepositoryRoot(), row.AudioFile);
            var (settled, readings) = Drive(file, row.Real ? 600 : SyntheticCq.StartingPitchHz);

            foreach (var d in departures)
            {
                var r = readings[d.Index];
                var measured = CwUnitEstimator.Measure(r.Window, hopMs);
                var marks = CwUnitEstimator.Elements(r.Window, hopMs).Marks;
                var (pairWpm, pairs) = PairSpeed(marks);

                _output.WriteLine(string.Create(Invariant,
                    $"speedset | {d.Name} | `{d.Key}` | `{d.Ours.Text}` | {r.Last.WordsPerMinute:0.0} | {measured.DitMarkMilliseconds:0} | {measured.ElementGapMilliseconds:0} | "
                    + $"{measured.WordsPerMinute:0.0} | {1200.0 / CwUnitEstimator.MarkUnit(marks):0.0} | {pairWpm:0.0} | {pairs} | {d.PortNote}"));
                _output.WriteLine($"speedmarks | {d.Name} | `{d.Key}` | marks in order, ms | {string.Join(" ", marks.Select(m => m.ToString("0", Invariant)))}");
            }
        }
    }

    /// <remarks>
    /// Proves nothing about either decoder; prints work instruction 459 task 1
    /// (HM-REQ-129, HM-REQ-010): every sure letter ours prints wrong where the
    /// port is sure and right on the same key letter, with our decoder's marks,
    /// gaps, speed, pitch and thresholds under it, where the lost element went,
    /// the port's own evidence beside it, and the groups. Asserts only that the
    /// copied lattice spells what the decoder spelled on every read traced.
    /// </remarks>
    [Fact]
    public void EveryLetterOursLosesThatThePortKeeps()
    {
        var departures = Trace(out var sameness, out var reads);

        _output.WriteLine("departure | recording | key kind | stretch | key index | key | ours | ours evidence | port | port evidence | group | where the element went");

        foreach (var d in departures)
        {
            _output.WriteLine(string.Create(Invariant,
                $"departure | {d.Name} | {d.Kind} | {d.Stretch + 1} | {d.KeyIndex} | `{d.Key}` {PatternOf(d.Key)} | `{d.Ours.Text}` at {d.Ours.At.TotalSeconds:0.000} s | {d.OursNote} | `{d.PortText}` | {d.PortNote} | ({d.Group}) | {d.Why}"));

            foreach (var line in d.Lines)
            {
                _output.WriteLine($"  evidence | {d.Name} | `{d.Key}` | {line}");
            }

            _output.WriteLine($"  mechanism | {d.Name} | `{d.Key}` | behind ({d.Behind}){(d.SpeedCaused ? ", the speed" : "")} | {Mechanism[d.SpeedCaused ? "d" : d.Group]}");
        }

        _output.WriteLine("group | group | departures | of them, ours mixed more than 25 Hz off the port's given pitch | on pitch | recordings | the port's mechanism");

        foreach (var g in departures.GroupBy(d => d.Group).OrderByDescending(g => g.Count()).ThenBy(g => g.Key, StringComparer.Ordinal))
        {
            _output.WriteLine($"group | ({g.Key}) | {g.Count()} | {g.Count(d => d.OffPitch)} | {g.Count(d => !d.OffPitch)} | "
                + $"{string.Join(", ", g.Select(d => d.Name).Distinct())} | {Mechanism[g.Key]}");
        }

        _output.WriteLine("behind | port mechanism | departures | of them real | groups | recordings");

        foreach (var g in departures.GroupBy(d => d.Behind).OrderByDescending(g => g.Count()).ThenBy(g => g.Key, StringComparer.Ordinal))
        {
            _output.WriteLine($"behind | ({g.Key}) | {g.Count()} | {g.Count(d => d.Kind == "inferred")} | "
                + $"{string.Join(", ", g.GroupBy(d => d.Group).Select(x => $"({x.Key}) {x.Count()}"))} | {string.Join(", ", g.Select(d => d.Name).Distinct())}");
        }

        _output.WriteLine($"sameness | {sameness} of {reads} reads spelled alike by the copied lattice | {departures.Count} departures");

        Assert.Equal(reads, sameness);
    }
}

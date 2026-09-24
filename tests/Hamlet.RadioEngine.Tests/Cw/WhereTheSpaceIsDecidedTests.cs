using System.Reflection;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// What re-deciding each gap the path read as between letters or between words
/// would do, printed before anything is built (work instruction 415, task 1).
/// </summary>
/// <remarks>
/// <para>**WHERE KIND 3 AND KIND 4 DIFFER DOWNSTREAM OF THE PATH.** In
/// <c>CwProbabilisticDecoder.Spell</c> both close the letter before them the same
/// way, with the same end hop and the same span ratio, and kind 4 adds one
/// <c>" "</c> whose end hop is the gap's end. <c>Judged</c> passes every space and
/// judges each letter on its own marks. The one place a space can reach a letter is
/// the stream's settle loop: a settled space advances <c>_settledThrough</c> to the
/// gap's end where a letter advances it only to the key-up, and anything a later
/// read puts at or before that mark is never announced.</para>
/// <para>**SO THE SETTLE LOOP IS EMULATED THREE WAYS ON EVERY READ**, from the
/// stream's own window, speed and held gaps, read by reflection after the read:
/// as it is (checked against what really settled), with the relabel applied to the
/// decoder's characters before the loop, and with the loop's bookkeeping left on
/// the path's own characters and only the announcement of spaces re-decided. The
/// path is read ungated at the same speed and gaps, which is the same path before
/// <c>Judged</c>, so every gap between two marks is seen whether or not the letters
/// either side cleared their margin.</para>
/// <para>A printer. It asserts nothing.</para>
/// </remarks>
public sealed class WhereTheSpaceIsDecidedTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly FieldInfo EnvelopeField = typeof(CwProbabilisticStream).GetField("_envelope", Private)!;
    private static readonly FieldInfo CountField = typeof(CwProbabilisticStream).GetField("_envelopeCount", Private)!;
    private static readonly FieldInfo HopsSeenField = typeof(CwProbabilisticStream).GetField("_hopsSeen", Private)!;
    private static readonly FieldInfo DelayField = typeof(CwProbabilisticStream).GetField("_delayHops", Private)!;
    private static readonly FieldInfo StructureHeldField = typeof(CwProbabilisticStream).GetField("_structureHeld", Private)!;
    private static readonly FieldInfo HeldGapsField = typeof(CwProbabilisticStream).GetField("_heldGaps", Private)!;

    private static readonly MethodInfo ThreeMeans = typeof(CwUnitEstimator)
        .GetMethod("ThreeMeansOnLogs", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo IsTrough = typeof(CwUnitEstimator)
        .GetMethod("IsTrough", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo DecodeFull = typeof(CwProbabilisticDecoder)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
        .Single(m => m.Name == "Decode" && m.GetParameters().Length == 6);

    /// <summary>
    /// The starting boundary, as a multiple of the measured character centroid: the
    /// geometric mean of the centroid and seven thirds of it (author's, overrulable).
    /// </summary>
    internal static readonly double StartingShare = Math.Sqrt(7.0 / 3.0);

    /// <summary>Whether the centroid must sit inside MeasureGaps' own clip; off for the first run.</summary>
    internal static bool GuardOn { get; set; } = true;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhereTheSpaceIsDecidedTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One gap between two marks, as the path read it in one read.</summary>
    private sealed record Gap(
        long StartAbs, long EndAbs, int EndHop, double SpanMs, double UnitMs, double? CentroidMs,
        bool PathWord, bool Held, double WordFromMs)
    {
        public bool RelabelWord(double share)
            => CentroidMs is { } c ? SpanMs > c * share : PathWord;
    }

    /// <summary>One thing announced by an emulated settle loop.</summary>
    private sealed record Emitted(string Text, string Pattern, long Abs, int SpanHops, Gap? Gap);

    /// <summary>Everything one recording produced under the three emulations.</summary>
    private sealed class Run
    {
        public List<CwCharacter> Real { get; } = new();

        public List<Emitted> Entry { get; } = new();

        public List<Emitted> Naive { get; } = new();

        public List<Emitted> After { get; } = new();

        public int Reads { get; set; }

        public int ReadsWithCentroid { get; set; }

        public int ReadsHeld { get; set; }

        public int SpaceEndMismatches { get; set; }

        /// <summary>Each interior gap at the read its end first fell behind the delay.</summary>
        public Dictionary<long, Gap> Decided { get; } = new();

        public List<double> CentroidUnits { get; } = new();
    }

    /// <summary>
    /// The character centroid <c>MeasureGaps</c> would find in this window when the
    /// element and character heaps are apart and have their trough, whether or not
    /// the word trough holds; null otherwise.
    /// </summary>
    internal static double? CharacterCentroid(double[] window)
    {
        var unit = CwUnitEstimator.Measure(window, CwProbabilisticDecoder.HopMilliseconds);

        if (!unit.IsReady)
        {
            return null;
        }

        var gaps = CwUnitEstimator.Elements(window, CwProbabilisticDecoder.HopMilliseconds).Gaps;

        if (gaps.Count < 12)
        {
            return null;
        }

        var c = (double[])ThreeMeans.Invoke(null, new object[] { gaps })!;

        if (c[1] / c[0] < 1.5 || !(bool)IsTrough.Invoke(null, new object[] { gaps, c[0], c[1] })!)
        {
            return null;
        }

        // The first run found the element heap standing in for the character heap
        // on 013347, 1.1 units, and every character gap then read as a word gap. So
        // the boundary between the two heaps has to land where MeasureGaps' own
        // clip would take it without moving it, 1.3 to 2.6 units.
        var boundary = Math.Sqrt(c[0] * c[1]) / unit.UnitMilliseconds;

        if (GuardOn && (boundary < 1.3 || boundary > 2.6))
        {
            return null;
        }

        return c[1];
    }

    private static Run Replay(string name, double share)
    {
        var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var stream = decoder.Stream;
        var run = new Run();
        var flushing = false;
        long thrEntry = -1, thrNaive = -1, thrAfter = -1, thrSpace = -1;

        decoder.CharacterSettled += run.Real.Add;

        stream.LeadingEdgeChanged += _ =>
        {
            var count = (int)CountField.GetValue(stream)!;

            if (count == 0)
            {
                return;
            }

            run.Reads++;

            var window = ((double[])EnvelopeField.GetValue(stream)!)[..count];
            var hopsSeen = (long)HopsSeenField.GetValue(stream)!;
            var delay = (int)DelayField.GetValue(stream)!;
            var held = (bool)StructureHeldField.GetValue(stream)!;
            var heldGaps = (CwUnitEstimator.CwGapLengths)HeldGapsField.GetValue(stream)!;
            var last = stream.Last;
            var windowStart = hopsSeen - count;
            var settleBefore = flushing ? count + 1 : count - delay;

            if (held)
            {
                run.ReadsHeld++;
            }

            var measured = CwUnitEstimator.Measure(window, CwProbabilisticDecoder.HopMilliseconds);
            var speed = measured.IsReady
                && measured.WordsPerMinute >= CwProbabilisticDecoder.SlowestWpm
                && measured.WordsPerMinute <= CwProbabilisticDecoder.FastestWpm
                    ? measured.WordsPerMinute
                    : (double?)null;
            IReadOnlyList<double>? gapsIn = held
                ? new[] { heldGaps.ElementMilliseconds, heldGaps.CharacterMilliseconds, heldGaps.WordMilliseconds }
                : null;

            var centroid = CharacterCentroid(window);
            var unitMs = 1200.0 / Math.Max(1e-9, last.WordsPerMinute);

            if (centroid is { } cm)
            {
                run.ReadsWithCentroid++;
                run.CentroidUnits.Add(cm / unitMs);
            }

            var characterWant = held ? heldGaps.CharacterMilliseconds : 3 * unitMs;
            var wordWant = held ? heldGaps.WordMilliseconds : 7 * unitMs;
            var wordFrom = Math.Sqrt(characterWant * wordWant);

            // The path itself, before Judged: same window, speed and gaps, ungated.
            var gaps = new List<Gap>();

            if (last.Characters.Count > 0)
            {
                var path = ((CwProbabilisticResult)DecodeFull.Invoke(null, new object?[]
                {
                    window, stream.ToneHz, speed, gapsIn, true, CwProbabilisticDecoder.NoiseSpanSeconds,
                })!).Characters;

                CwProbabilisticCharacter? previous = null;
                int? spaceEnd = null;

                foreach (var c in path)
                {
                    if (c.Pattern.Length == 0)
                    {
                        spaceEnd = c.EndHop;
                        continue;
                    }

                    if (previous is { } p)
                    {
                        var start = p.EndHop;
                        var end = c.EndHop - c.SpanHops;

                        if (spaceEnd is { } se && se != end)
                        {
                            run.SpaceEndMismatches++;
                        }

                        gaps.Add(new Gap(
                            windowStart + start, windowStart + end, end,
                            (end - start) * CwProbabilisticDecoder.HopMilliseconds, unitMs, centroid,
                            spaceEnd is not null, held, wordFrom));
                    }

                    previous = c;
                    spaceEnd = null;
                }
            }

            foreach (var g in gaps.Where(g => g.EndHop < settleBefore))
            {
                run.Decided.TryAdd(g.StartAbs, g);
            }

            var byEnd = gaps.ToDictionary(g => g.EndHop);
            var judged = last.Characters;

            bool Settles(CwProbabilisticCharacter ch)
                => !(flushing && ch.Text == "#" && ch.EndHop >= count - delay) && ch.EndHop < settleBefore;

            // As it is.
            foreach (var ch in judged)
            {
                if (!Settles(ch))
                {
                    continue;
                }

                var abs = windowStart + ch.EndHop;

                if (abs <= thrEntry)
                {
                    continue;
                }

                thrEntry = abs;
                run.Entry.Add(new Emitted(ch.Text, ch.Pattern, abs, ch.SpanHops, byEnd.GetValueOrDefault(ch.EndHop)));
            }

            // The decoder's characters relabeled, then the loop as it is.
            var merged = new List<(CwProbabilisticCharacter Ch, string Role, Gap? Gap)>();

            foreach (var ch in judged)
            {
                if (ch.Pattern.Length == 0 && byEnd.TryGetValue(ch.EndHop, out var g) && !g.RelabelWord(share))
                {
                    merged.Add((ch, "removed", g));
                    continue;
                }

                merged.Add((ch, "path", ch.Pattern.Length == 0 ? byEnd.GetValueOrDefault(ch.EndHop) : null));
            }

            foreach (var g in gaps.Where(g => !g.PathWord && g.RelabelWord(share)))
            {
                merged.Add((new CwProbabilisticCharacter(" ", "", g.EndHop), "added", g));
            }

            merged = merged.OrderBy(m => m.Ch.EndHop).ThenBy(m => m.Ch.Pattern.Length == 0 ? 1 : 0).ToList();

            foreach (var (ch, _, g) in merged.Where(m => m.Role != "removed"))
            {
                if (!Settles(ch))
                {
                    continue;
                }

                var abs = windowStart + ch.EndHop;

                if (abs <= thrNaive)
                {
                    continue;
                }

                thrNaive = abs;
                run.Naive.Add(new Emitted(ch.Text, ch.Pattern, abs, ch.SpanHops, g));
            }

            // The bookkeeping on the path's own characters; only a space's announcement moves.
            foreach (var (ch, role, g) in merged)
            {
                if (!Settles(ch))
                {
                    continue;
                }

                var abs = windowStart + ch.EndHop;

                if (role == "added")
                {
                    if (abs > thrAfter && abs > thrSpace)
                    {
                        thrSpace = abs;
                        run.After.Add(new Emitted(" ", "", abs, 0, g));
                    }

                    continue;
                }

                if (abs <= thrAfter)
                {
                    continue;
                }

                thrAfter = abs;

                if (role == "removed")
                {
                    continue;
                }

                if (ch.Pattern.Length == 0)
                {
                    if (abs <= thrSpace)
                    {
                        continue;
                    }

                    thrSpace = abs;
                }

                run.After.Add(new Emitted(ch.Text, ch.Pattern, abs, ch.SpanHops, g));
            }
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        flushing = true;
        decoder.Flush();

        return run;
    }

    private static string TextOf(IEnumerable<Emitted> e)
        => string.Concat(e.Select(x => x.Text == "#" ? MorseAlphabet.Unreadable : x.Text));

    /// <summary>Letters that are not in both, by moment and text: what a relabel moved.</summary>
    private static int LettersMoved(IReadOnlyList<Emitted> a, IReadOnlyList<Emitted> b)
    {
        var left = a.Where(x => x.Pattern.Length > 0).Select(x => (x.Abs, x.Text)).ToList();
        var right = b.Where(x => x.Pattern.Length > 0).Select(x => (x.Abs, x.Text)).ToList();

        return left.Except(right).Count() + right.Except(left).Count();
    }

    private static string Spread(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToArray();

        if (sorted.Length == 0)
        {
            return "none";
        }

        double At(double share) => sorted[(int)Math.Round(share * (sorted.Length - 1))];

        return $"n {sorted.Length}, min {sorted[0]:0.00}, quartile {At(0.25):0.00}, median {At(0.5):0.00}, "
               + $"quartile {At(0.75):0.00}, max {sorted[^1]:0.00}";
    }

    private static IEnumerable<string> Names()
        => TheCapturesThatDecodeKeepDecodingTests.Floors
            .Select(row => (string)row[0])
            .Append(TheSeventeenThirtySevenCaptureTests.Name);

    private void Flip(string name, string direction, Emitted around, Gap g, double share)
        => _output.WriteLine(
            $"flip | {name} | {direction} | {around.Abs * CwProbabilisticDecoder.HopMilliseconds / 1000.0,8:0.000} s | "
            + $"span {g.SpanMs:0} ms, {g.SpanMs / g.UnitMs:0.0} units | centroid {g.CentroidMs:0} ms, {g.CentroidMs / g.UnitMs:0.0} units | "
            + $"boundary {g.CentroidMs * share:0} ms, {g.SpanMs / g.CentroidMs:0.00} of centroid | "
            + $"{(g.Held ? "held" : "textbook")} word from {g.WordFromMs:0} ms");

    /// <remarks>
    /// Proves task 1's second print: for all 51 capture rows and 17:37, what the
    /// relabel at the starting boundary would change, per recording, in each
    /// direction; whether the emulation of the settle loop reproduces what really
    /// settled; and how many letters move when the relabel feeds the settle loop
    /// against when it is placed after it. Asserts nothing.
    /// </remarks>
    [Fact]
    public void WhatARelabelWouldChange()
    {
        var share = StartingShare;
        var named = 0;
        var added = 0;
        var removed = 0;
        var naiveMoved = 0;
        var afterMoved = 0;
        var faithful = 0;
        var all = 0;

        _output.WriteLine($"boundary | {share:0.000} of the measured character centroid");
        _output.WriteLine("row | recording | reads | held | with a centroid | emulation matches what settled | spaces added | spaces removed | letters moved, relabel before the settle loop | letters moved, relabel after it | space end mismatches");

        foreach (var name in Names())
        {
            var run = Replay(name, share);
            var realText = string.Concat(run.Real.Select(c => c.Text));
            var matches = realText == TextOf(run.Entry)
                && run.Real.Count == run.Entry.Count;
            var naive = LettersMoved(run.Entry, run.Naive);
            var after = LettersMoved(run.Entry, run.After);
            var adds = run.After.Where(e => e.Pattern.Length == 0 && e.Gap is { PathWord: false }).ToList();
            var entrySpaces = run.Entry.Where(e => e.Pattern.Length == 0).Select(e => e.Abs).ToHashSet();
            var afterSpaces = run.After.Where(e => e.Pattern.Length == 0).Select(e => e.Abs).ToHashSet();
            var removes = run.Entry.Where(e => e.Pattern.Length == 0 && !afterSpaces.Contains(e.Abs)).ToList();

            all++;
            faithful += matches ? 1 : 0;
            added += adds.Count;
            removed += removes.Count;
            naiveMoved += naive;
            afterMoved += after;
            named += run.Entry.Count(e => e.Pattern.Length > 0 && e.Text != "#");

            _output.WriteLine(
                $"row | {name} | {run.Reads} | {run.ReadsHeld} | {run.ReadsWithCentroid} | {(matches ? "yes" : "NO")} | "
                + $"{adds.Count} | {removes.Count} | {naive} | {after} | {run.SpaceEndMismatches}");
            _output.WriteLine($"text | {name} | entry  `{TextOf(run.Entry)}`");
            _output.WriteLine($"text | {name} | after  `{TextOf(run.After)}`");
            _output.WriteLine($"text | {name} | real   `{string.Concat(run.Real.Select(c => c.Text == "#" ? MorseAlphabet.Unreadable : c.Text))}`");

            if (naive > 0)
            {
                _output.WriteLine($"text | {name} | before `{TextOf(run.Naive)}`");
            }

            if (run.CentroidUnits.Count > 0)
            {
                _output.WriteLine($"centroid | {name} | in units | {Spread(run.CentroidUnits)}");
            }

            foreach (var e in removes)
            {
                if (e.Gap is { } g)
                {
                    Flip(name, "removed", e, g, share);
                }
            }

            foreach (var e in adds)
            {
                Flip(name, "added", e, e.Gap!, share);
            }
        }

        _output.WriteLine(
            $"summary | {all} recordings | emulation matches what settled on {faithful} | spaces added {added} | spaces removed {removed} | "
            + $"letters moved with the relabel before the settle loop {naiveMoved} | after it {afterMoved} | named at entry {named}");
    }

    /// <summary>A boundary between two named characters the key's alignment reaches.</summary>
    private sealed record Boundary(string Name, bool KeySpace, bool DecodeSpace, Gap? Gap)
    {
        public string Kind => (KeySpace, DecodeSpace) switch
        {
            (false, true) => "inserted",
            (true, false) => "missing",
            (true, true) => "word kept",
            _ => "joined",
        };
    }

    private static IEnumerable<Boundary> Boundaries(string name, Run run, IReadOnlyList<Emitted> settled, CwScore score)
    {
        var owner = new List<int>();

        for (var i = 0; i < settled.Count; i++)
        {
            owner.AddRange(Enumerable.Repeat(i, settled[i].Text.Length));
        }

        var d = score.Start;
        int? previous = null;
        var keySpace = false;
        var decodeSpace = false;

        foreach (var step in score.Steps)
        {
            if (step.Key == ' ')
            {
                keySpace = true;
            }

            if (step.Decoded is null)
            {
                continue;
            }

            var index = owner[d];
            var c = settled[index];

            d++;

            if (c.Pattern.Length == 0)
            {
                decodeSpace = true;
                continue;
            }

            if (c.Text == "#" || index == previous)
            {
                continue;
            }

            if (previous is { } p)
            {
                yield return new Boundary(
                    name, keySpace, decodeSpace, run.Decided.GetValueOrDefault(settled[p].Abs));
            }

            previous = index;
            keySpace = false;
            decodeSpace = false;
        }
    }

    /// <remarks>
    /// Proves task 1's first print: over the ten keyed captures and 17:37, the
    /// span of every boundary between two named characters as a share of the
    /// character centroid measured in the read that settled it, split as unit 413
    /// split them against the inferred keys - inserted, joined, word kept, missing -
    /// with every decided gap's share beside them regardless of any key, so the
    /// boundary can be chosen from the heaps. Asserts nothing.
    /// </remarks>
    [Fact]
    public void TheSpansTheBoundaryIsChosenFrom()
    {
        var boundaries = new List<Boundary>();
        var decided = new List<Gap>();

        foreach (var (name, _) in TheBenchmarkIsKeyedTests.Run)
        {
            var stretches = TheBenchmarkIsKeyedTests.Stretches(name);

            if (stretches.Count == 0)
            {
                continue;
            }

            var run = Replay(name, StartingShare);
            var reading = CwReading.Of(run.Real);

            decided.AddRange(run.Decided.Values);

            foreach (var stretch in stretches)
            {
                boundaries.AddRange(Boundaries(name, run, run.Entry, CwScorer.Within(reading, stretch.Key, CwKeyKind.Inferred)));
            }
        }

        var tenCount = boundaries.Count;

        {
            var name = TheSeventeenThirtySevenCaptureTests.Name;
            var run = Replay(name, StartingShare);
            var reading = CwReading.Of(run.Real);
            var region = CwScorer.FromFirst(reading, "CQ");
            var score = CwScorer.Whole(region, TheSeventeenThirtySevenCaptureTests.InferredKey, CwKeyKind.Inferred);

            decided.AddRange(run.Decided.Values);
            boundaries.AddRange(Boundaries(name, run, run.Entry, score with { Start = reading.Text.IndexOf("CQ", StringComparison.Ordinal) }));
        }

        _output.WriteLine("every gap between two marks at the read it settled, the ten and 17:37, span over the measured character centroid, no key:");

        var shares = decided.Where(g => g.CentroidMs is not null).Select(g => g.SpanMs / g.CentroidMs!.Value).ToList();

        for (var low = 0.2; low < 4.0; low += 0.1)
        {
            var n = shares.Count(s => s >= low && s < low + 0.1);
            var word = decided.Where(g => g.CentroidMs is not null && g.PathWord)
                .Count(g => g.SpanMs / g.CentroidMs!.Value >= low && g.SpanMs / g.CentroidMs.Value < low + 0.1);

            _output.WriteLine($"heap | {low:0.0} to {low + 0.1:0.0} | {n,4} | path word {word,3} | {new string('#', Math.Min(n, 90))}");
        }

        _output.WriteLine($"heap | 4.0 and over | {shares.Count(s => s >= 4.0)}");
        _output.WriteLine($"heap | no centroid measured | {decided.Count(g => g.CentroidMs is null)} of {decided.Count}");

        foreach (var (label, set) in new[]
                 {
                     ("the ten", boundaries.Take(tenCount).ToList()),
                     ("17:37", boundaries.Skip(tenCount).ToList()),
                     ("all", boundaries),
                 })
        {
            _output.WriteLine(
                $"summary | {label} | boundaries {set.Count} | inserted {set.Count(b => b.Kind == "inserted")} | "
                + $"missing {set.Count(b => b.Kind == "missing")} | word kept {set.Count(b => b.Kind == "word kept")} | "
                + $"joined {set.Count(b => b.Kind == "joined")} | no gap found {set.Count(b => b.Gap is null)} | "
                + $"no centroid {set.Count(b => b.Gap is { CentroidMs: null })}");

            foreach (var kind in new[] { "inserted", "joined", "word kept", "missing" })
            {
                var of = set.Where(b => b.Kind == kind && b.Gap is { CentroidMs: not null }).Select(b => b.Gap!).ToList();

                _output.WriteLine($"spread | {label} | {kind} | span over centroid | {Spread(of.Select(g => g.SpanMs / g.CentroidMs!.Value))}");
                _output.WriteLine($"spread | {label} | {kind} | span in units | {Spread(of.Select(g => g.SpanMs / g.UnitMs))}");
                _output.WriteLine($"spread | {label} | {kind} | centroid in units | {Spread(of.Select(g => g.CentroidMs!.Value / g.UnitMs))}");
            }

            foreach (var share in new[] { 1.30, 1.40, StartingShare, 1.60, 1.65, 1.70, 1.80, 2.00 })
            {
                var measured = set.Where(b => b.Gap is { CentroidMs: not null }).ToList();

                bool Word(Boundary b) => b.Gap!.SpanMs > b.Gap.CentroidMs!.Value * share;

                _output.WriteLine(
                    $"at | {label} | {share:0.000} | inserted joined {measured.Count(b => b.Kind == "inserted" && !Word(b))} of {measured.Count(b => b.Kind == "inserted")} | "
                    + $"word kept lost {measured.Count(b => b.Kind == "word kept" && !Word(b))} of {measured.Count(b => b.Kind == "word kept")} | "
                    + $"joined split {measured.Count(b => b.Kind == "joined" && Word(b))} of {measured.Count(b => b.Kind == "joined")} | "
                    + $"missing restored {measured.Count(b => b.Kind == "missing" && Word(b))} of {measured.Count(b => b.Kind == "missing")}");
            }
        }
    }
}

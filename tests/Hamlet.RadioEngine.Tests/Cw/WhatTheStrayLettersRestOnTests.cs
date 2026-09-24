using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every stray single-element letter on the keyed recordings, with the span it
/// stood on and the gate that let it through, and the span of every named
/// character in the tree beside it (work instruction 421, task 1; PHASE_PLAN.md
/// R69, R71, criteria 3.6 and 3.7).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.** R71 says a floor counts only
/// characters whose span is at or above a stated bar, and that the bar is chosen
/// from the trace of the stray characters, never from which changes it would let
/// through. This is that trace, and nothing is decided in it.</para>
/// <para>**EVERY KEY IS INFERRED** (R61). Each settled character on a keyed
/// recording is labeled by the scorer's own alignment, the one the baseline counts
/// its edits from: right, wrong, added, or outside every scored stretch. A
/// character whose text is several characters long, a prosign, is right only where
/// every one of them is.</para>
/// <para>**THREE FIGURES, BECAUSE WHICH ONE MEANS THE SAME THING ON TWO RECORDINGS
/// IS A QUESTION THE PRINT ANSWERS.** The raw span ratio, the per-hop margin that
/// <see cref="CwProbabilisticDecoder.CharacterMargin"/> gates on, and the per-hop
/// margin over the median of the recording's own named characters, which needs no
/// key.</para>
/// </remarks>
public sealed class WhatTheStrayLettersRestOnTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhatTheStrayLettersRestOnTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>What the key's alignment made of a settled character.</summary>
    internal enum Label
    {
        /// <summary>The key has it, in place.</summary>
        Right,

        /// <summary>The key has a different character in its place.</summary>
        Wrong,

        /// <summary>The key has nothing in its place.</summary>
        Added,

        /// <summary>Outside every scored stretch, or on a recording with no key.</summary>
        Unscored,
    }

    /// <summary>One named character with its label and its three span figures.</summary>
    /// <param name="Name">The recording.</param>
    /// <param name="Character">What settled.</param>
    /// <param name="Label">What the key made of it.</param>
    /// <param name="RecordingMedian">The median per-hop margin over the recording's named characters.</param>
    internal sealed record Traced(string Name, CwCharacter Character, Label Label, double RecordingMedian)
    {
        /// <summary>One element, a dit or a dah.</summary>
        public bool Single => Character.Pattern.Length == 1;

        /// <summary>The raw figure over the character's own marks.</summary>
        public double Raw => Character.SpanLogLikelihoodRatio;

        /// <summary>The raw figure over its hops, the one the emission gate reads.</summary>
        public double PerHop => Character.SpanMarginForRecord;

        /// <summary>The per-hop figure over the recording's own median.</summary>
        public double Relative => RecordingMedian > 0 ? PerHop / RecordingMedian : double.NaN;

        /// <summary>Whether the pass left the span unmeasured.</summary>
        public bool Unmeasured => double.IsNaN(Character.SpanLogLikelihoodRatio);
    }

    /// <summary>A keyed recording, which total it belongs to, and how it is scored.</summary>
    /// <param name="Name">The recording.</param>
    /// <param name="Set">17:37, baseline, outside or the ten.</param>
    /// <param name="Score">Its scores against the reading, one per scored stretch.</param>
    internal sealed record Keyed(string Name, string Set, Func<CwReading, IReadOnlyList<CwScore>> Score);

    /// <summary>Every keyed recording, scored exactly as the baseline and the benchmark score it.</summary>
    internal static IReadOnlyList<Keyed> KeyedRecordings { get; } = BuildKeyed();

    private static IReadOnlyList<Keyed> BuildKeyed()
    {
        var keyed = new List<Keyed>
        {
            new(TheSeventeenThirtySevenCaptureTests.Name, "17:37", reading =>
            {
                var from = reading.Text.IndexOf("CQ", StringComparison.Ordinal);

                return from < 0
                    ? Array.Empty<CwScore>()
                    : new[]
                    {
                        CwScorer.Whole(
                            CwScorer.FromFirst(reading, "CQ"),
                            TheSeventeenThirtySevenCaptureTests.InferredKey,
                            CwKeyKind.Inferred) with { Start = from },
                    };
            }),
        };

        foreach (var adjudicated in TheAdjudicatedReadingsKeepReadingTests.All)
        {
            keyed.Add(new Keyed(
                adjudicated.Name,
                TheBaselineIsScoredTests.Anchors.Contains(adjudicated.Name) ? "baseline" : "outside",
                reading => new[] { CwScorer.Within(reading, adjudicated.Adjudicated, CwKeyKind.Inferred) }));
        }

        foreach (var (name, _) in TheBenchmarkIsKeyedTests.Run)
        {
            var stretches = TheBenchmarkIsKeyedTests.Stretches(name);

            if (stretches.Count == 0)
            {
                continue;
            }

            keyed.Add(new Keyed(
                name,
                "the ten",
                reading => stretches
                    .Select(s => CwScorer.Within(reading, s.Key, CwKeyKind.Inferred))
                    .ToList()));
        }

        return keyed;
    }

    /// <summary>Each settled character's label, from the alignments of every scored stretch.</summary>
    /// <param name="settled">What settled, word gaps included.</param>
    /// <param name="scores">The scores, each with its alignment and where it starts.</param>
    /// <returns>One label per settled character; right outranks wrong outranks added.</returns>
    internal static Label[] Labels(IReadOnlyList<CwCharacter> settled, IEnumerable<CwScore> scores)
    {
        var owner = new List<int>();

        for (var i = 0; i < settled.Count; i++)
        {
            owner.AddRange(Enumerable.Repeat(i, settled[i].Text.Length));
        }

        var labels = Enumerable.Repeat(Label.Unscored, settled.Count).ToArray();

        foreach (var score in scores)
        {
            var edits = new Dictionary<int, List<CwEdit>>();
            var d = score.Start;

            foreach (var step in score.Steps)
            {
                if (step.Decoded is null)
                {
                    continue;
                }

                var index = owner[d++];

                if (!edits.TryGetValue(index, out var list))
                {
                    edits[index] = list = new List<CwEdit>();
                }

                list.Add(step.Edit);
            }

            foreach (var (index, list) in edits)
            {
                var label = list.All(e => e == CwEdit.Same) ? Label.Right
                    : list.All(e => e == CwEdit.Added) ? Label.Added
                    : Label.Wrong;

                if (label < labels[index])
                {
                    labels[index] = label;
                }
            }
        }

        return labels;
    }

    /// <summary>A named character: neither a word gap nor a placeholder (R57).</summary>
    internal static bool IsNamed(CwCharacter c) => !c.IsWordGap && !c.IsUnreadable;

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray();

        return sorted.Length == 0 ? double.NaN : sorted[sorted.Length / 2];
    }

    private static string Spread(IEnumerable<double> values)
    {
        var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray();

        if (sorted.Length == 0)
        {
            return "none";
        }

        double At(double share) => sorted[(int)Math.Round(share * (sorted.Length - 1))];

        return $"n {sorted.Length}, min {sorted[0]:G4}, tenth {At(0.1):G4}, quartile {At(0.25):G4}, "
               + $"median {At(0.5):G4}, quartile {At(0.75):G4}, ninetieth {At(0.9):G4}, max {sorted[^1]:G4}";
    }

    private static string Of(Traced t)
        => $"`{t.Character.Text}` {t.Character.Pattern} at {t.Character.At.TotalSeconds:0.000} s";

    /// <remarks>
    /// Proves task 1: table 1, every added or wrong single-element character on
    /// every keyed recording with its span figures and the gate that admitted it,
    /// and per recording the added and wrong letters; table 2, every named
    /// character on the 51 capture rows and 17:37, its span figures by label, the
    /// lowest span the key calls right and the highest an added single element
    /// reaches, per recording and overall, and the unmeasured spans. Asserts
    /// nothing.
    /// </remarks>
    [Fact]
    public void TheStraysAndTheSpansTheyStandOn()
    {
        var settledBy = new Dictionary<string, IReadOnlyList<CwCharacter>>(StringComparer.Ordinal);

        IReadOnlyList<CwCharacter> Settled(string name)
            => settledBy.TryGetValue(name, out var s)
                ? s
                : settledBy[name] = TheSeventeenThirtySevenCaptureTests.Settle(name);

        var rows = TheCapturesThatDecodeKeepDecodingTests.Floors.Select(row => (string)row[0]).ToList();
        var names = rows.Append(TheSeventeenThirtySevenCaptureTests.Name).ToList();
        var keyedNames = KeyedRecordings.Select(k => k.Name).ToHashSet(StringComparer.Ordinal);
        var traced = new List<Traced>();
        var edits = 0;
        var length = 0;

        _output.WriteLine(
            $"gates | window Gate {CwProbabilisticDecoder.Gate:0.00} on Score, the window's likelihood ratio per hop | "
            + $"CharacterMargin {CwProbabilisticDecoder.CharacterMargin:0.00} on the character's own per-hop span margin, "
            + "SpanMarginForRecord, which is SpanLogLikelihoodRatio over SpanHops | a word gap passes both unjudged");

        foreach (var name in names)
        {
            var settled = Settled(name);
            var keyed = KeyedRecordings.SingleOrDefault(k => k.Name == name);
            var scores = keyed is null ? Array.Empty<CwScore>() : keyed.Score(CwReading.Of(settled));
            var labels = Labels(settled, scores);
            var median = Median(settled.Where(IsNamed).Select(c => c.SpanMarginForRecord));

            edits += scores.Sum(s => s.Edits);
            length += scores.Sum(s => s.ScoredLength);

            for (var i = 0; i < settled.Count; i++)
            {
                if (IsNamed(settled[i]))
                {
                    traced.Add(new Traced(name, settled[i], labels[i], median));
                }
            }
        }

        _output.WriteLine(
            $"check | the alignments used here sum to {edits} edits over {length} characters against inferred keys, "
            + $"{KeyedRecordings.Count} keyed recordings");

        // ---- TABLE 1 ----
        _output.WriteLine("");
        _output.WriteLine("TABLE 1 | every added or wrong single-element character on the keyed recordings");
        _output.WriteLine(
            "stray | recording | at | label | text | pattern | Score | SpanLogLikelihoodRatio | SpanHops | SpanMarginForRecord | admitted by");

        foreach (var k in KeyedRecordings)
        {
            foreach (var t in traced.Where(t => t.Name == k.Name && t.Single && t.Label is Label.Added or Label.Wrong))
            {
                var c = t.Character;

                _output.WriteLine(
                    $"stray | {k.Name} | {c.At.TotalSeconds:0.000} s | {t.Label.ToString().ToLowerInvariant()} | `{c.Text}` | {c.Pattern} | "
                    + $"{c.Score:0.000} | {c.SpanLogLikelihoodRatio:G6} | {c.SpanHops} | {c.SpanMarginForRecord:0.000} | "
                    + $"CharacterMargin on per-hop {c.SpanMarginForRecord:0.000} >= {CwProbabilisticDecoder.CharacterMargin:0.0}, "
                    + $"window Gate on Score {c.Score:0.000} >= {CwProbabilisticDecoder.Gate:0.00}");
            }
        }

        _output.WriteLine(
            "count | recording | set | named | right | wrong | wrong single-element | added | added single-element | unscored");

        foreach (var set in new[] { "17:37", "baseline", "outside", "the ten" })
        {
            foreach (var k in KeyedRecordings.Where(k => k.Set == set))
            {
                var of = traced.Where(t => t.Name == k.Name).ToList();

                _output.WriteLine(
                    $"count | {k.Name} | {set} | {of.Count} | {of.Count(t => t.Label == Label.Right)} | "
                    + $"{of.Count(t => t.Label == Label.Wrong)} | {of.Count(t => t.Label == Label.Wrong && t.Single)} | "
                    + $"{of.Count(t => t.Label == Label.Added)} | {of.Count(t => t.Label == Label.Added && t.Single)} | "
                    + $"{of.Count(t => t.Label == Label.Unscored)}");
            }
        }

        var onKeyed = traced.Where(t => keyedNames.Contains(t.Name)).ToList();

        _output.WriteLine(
            $"count | all keyed | | {onKeyed.Count} | {onKeyed.Count(t => t.Label == Label.Right)} | "
            + $"{onKeyed.Count(t => t.Label == Label.Wrong)} | {onKeyed.Count(t => t.Label == Label.Wrong && t.Single)} | "
            + $"{onKeyed.Count(t => t.Label == Label.Added)} | {onKeyed.Count(t => t.Label == Label.Added && t.Single)} | "
            + $"{onKeyed.Count(t => t.Label == Label.Unscored)}");

        // ---- TABLE 2 ----
        _output.WriteLine("");
        _output.WriteLine("TABLE 2 | every named character on the 51 capture rows and 17:37");

        foreach (var name in names)
        {
            var of = traced.Where(t => t.Name == name).ToList();

            _output.WriteLine(
                $"nan | {name} | {of.Count(t => t.Unmeasured)} of {of.Count} named with no measured span | "
                + $"hops of nought {of.Count(t => t.Character.SpanHops <= 0)}");
        }

        _output.WriteLine(
            $"nan | all | {traced.Count(t => t.Unmeasured)} of {traced.Count} named with no measured span");

        var classes = new (string Label, Func<Traced, bool> In)[]
        {
            ("all named, 52 recordings", _ => true),
            ("unkeyed recordings", t => !keyedNames.Contains(t.Name)),
            ("keyed, right", t => keyedNames.Contains(t.Name) && t.Label == Label.Right),
            ("keyed, right single-element", t => keyedNames.Contains(t.Name) && t.Label == Label.Right && t.Single),
            ("keyed, right longer", t => keyedNames.Contains(t.Name) && t.Label == Label.Right && !t.Single),
            ("keyed, wrong", t => keyedNames.Contains(t.Name) && t.Label == Label.Wrong),
            ("keyed, wrong single-element", t => keyedNames.Contains(t.Name) && t.Label == Label.Wrong && t.Single),
            ("keyed, added single-element", t => keyedNames.Contains(t.Name) && t.Label == Label.Added && t.Single),
            ("keyed, added longer", t => keyedNames.Contains(t.Name) && t.Label == Label.Added && !t.Single),
            ("keyed, outside every stretch", t => keyedNames.Contains(t.Name) && t.Label == Label.Unscored),
        };

        var figures = new (string Label, Func<Traced, double> Of)[]
        {
            ("raw SpanLogLikelihoodRatio", t => t.Raw),
            ("per hop SpanMarginForRecord", t => t.PerHop),
            ("per hop over the recording's median", t => t.Relative),
        };

        foreach (var (figure, value) in figures)
        {
            foreach (var (label, @in) in classes)
            {
                _output.WriteLine($"spread | {figure} | {label} | {Spread(traced.Where(@in).Select(value))}");
            }
        }

        // A histogram on quarter decades, one column per class, so the heaps can be read side by side.
        foreach (var (figure, value) in figures)
        {
            var columns = classes.Skip(1).ToArray();

            _output.WriteLine(
                $"heap | {figure} | from | to | " + string.Join(" | ", columns.Select(c => c.Label)));

            var finite = traced.Select(value).Where(v => !double.IsNaN(v) && v > 0).ToList();

            if (finite.Count == 0)
            {
                continue;
            }

            var low = Math.Floor(Math.Log10(finite.Min()) * 4) / 4;
            var high = Math.Log10(finite.Max());

            _output.WriteLine(
                $"heap | {figure} | nought or below | | "
                + string.Join(" | ", columns.Select(c => traced.Where(c.In).Count(t => value(t) <= 0))));

            for (var q = low; q <= high; q += 0.25)
            {
                var from = Math.Pow(10, q);
                var to = Math.Pow(10, q + 0.25);

                _output.WriteLine(
                    $"heap | {figure} | {from:G4} | {to:G4} | "
                    + string.Join(" | ", columns.Select(c => traced.Where(c.In).Count(t => value(t) >= from && value(t) < to))));
            }
        }

        // The lowest right and highest added single element, per keyed recording and overall, on each figure.
        foreach (var (figure, value) in figures)
        {
            _output.WriteLine(
                $"edge | {figure} | recording | lowest right | the character | highest added single-element | the character | lowest right single-element");

            foreach (var k in KeyedRecordings)
            {
                var right = traced.Where(t => t.Name == k.Name && t.Label == Label.Right).OrderBy(value).FirstOrDefault();
                var rightSingle = traced.Where(t => t.Name == k.Name && t.Label == Label.Right && t.Single).OrderBy(value).FirstOrDefault();
                var added = traced.Where(t => t.Name == k.Name && t.Label == Label.Added && t.Single).OrderByDescending(value).FirstOrDefault();

                _output.WriteLine(
                    $"edge | {figure} | {k.Name} | "
                    + (right is null ? "none | " : $"{value(right):G4} | {Of(right)} | ")
                    + (added is null ? "none | " : $"{value(added):G4} | {Of(added)} | ")
                    + (rightSingle is null ? "none" : $"{value(rightSingle):G4} {Of(rightSingle)}"));
            }

            var allRight = onKeyed.Where(t => t.Label == Label.Right).OrderBy(value).First();
            var allAdded = onKeyed.Where(t => t.Label == Label.Added && t.Single).OrderByDescending(value).First();
            var rightBelow = onKeyed.Count(t => t.Label == Label.Right && value(t) <= value(allAdded));
            var addedAbove = onKeyed.Count(t => t.Label == Label.Added && t.Single && value(t) >= value(allRight));

            _output.WriteLine(
                $"edge | {figure} | overall | lowest right {value(allRight):G4} on {allRight.Name} {Of(allRight)} | "
                + $"highest added single-element {value(allAdded):G4} on {allAdded.Name} {Of(allAdded)} | "
                + $"right at or under that highest added {rightBelow} of {onKeyed.Count(t => t.Label == Label.Right)} | "
                + $"added single-element at or over that lowest right {addedAbove} of {onKeyed.Count(t => t.Label == Label.Added && t.Single)}");

            // How far each keyed recording's right characters sit from each other's on this figure.
            var medians = KeyedRecordings
                .Select(k => Median(traced.Where(t => t.Name == k.Name && t.Label == Label.Right).Select(value)))
                .Where(m => !double.IsNaN(m) && m > 0)
                .ToList();

            _output.WriteLine(
                $"across | {figure} | the median right character per keyed recording | {Spread(medians)} | "
                + $"largest over smallest {(medians.Count > 0 ? medians.Max() / medians.Min() : double.NaN):G4}");
        }

        // The added and wrong single elements against every right one, on each figure, counted at a ladder of bars.
        foreach (var (figure, value) in figures)
        {
            var ladder = onKeyed.Where(t => t.Label == Label.Right).Select(value).Where(v => !double.IsNaN(v)).OrderBy(v => v).Take(5)
                .Concat(onKeyed.Where(t => t.Single && t.Label is Label.Added or Label.Wrong).Select(value).Where(v => !double.IsNaN(v)))
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            foreach (var bar in ladder)
            {
                _output.WriteLine(
                    $"ladder | {figure} | bar {bar:G6} | right below {onKeyed.Count(t => t.Label == Label.Right && value(t) < bar)} | "
                    + $"added single-element below {onKeyed.Count(t => t.Label == Label.Added && t.Single && value(t) < bar)} of "
                    + $"{onKeyed.Count(t => t.Label == Label.Added && t.Single)} | "
                    + $"wrong single-element below {onKeyed.Count(t => t.Label == Label.Wrong && t.Single && value(t) < bar)} | "
                    + $"all named below, 52 recordings {traced.Count(t => value(t) < bar)} of {traced.Count}");
            }
        }
    }

    /// <summary>What settled, with the pitch each hop was mixed at and the envelope the stream read at it.</summary>
    /// <param name="Settled">What settled, word gaps included, in order.</param>
    /// <param name="Tone">The mixing pitch in force for each hop, on the audio clock.</param>
    /// <param name="Envelope">The stream's own envelope magnitude for each hop, on the same clock.</param>
    internal sealed record Heard(IReadOnlyList<CwCharacter> Settled, double[] Tone, double[] Envelope);

    /// <summary>
    /// Settles a recording exactly as <see cref="TheSeventeenThirtySevenCaptureTests.Settle"/>
    /// does, and reads the stream's pitch and newest envelope after every hop.
    /// </summary>
    /// <param name="name">The recording.</param>
    /// <returns>What settled, and the two per-hop records.</returns>
    /// <remarks>
    /// Both are read after the hop has been fed and change nothing. The decoder
    /// is stepped a hop at a time and pushes one envelope magnitude per hop, so
    /// entry <c>k</c> of each is hop <c>k</c> on the clock a character's
    /// <see cref="CwCharacter.At"/> is stamped from, and the envelope is the one
    /// the path chose its marks on.
    /// </remarks>
    internal static Heard Hear(string name)
    {
        var audio = WavAudio.Read(
            Path.Combine(CapturedSignalTests.Folder, name + ".wav"));

        var decoder = new CwDecoder(audio.SampleRate, 600);
        var settled = new List<CwCharacter>();
        var tone = new List<double>();
        var envelope = new List<double>();

        decoder.CharacterSettled += settled.Add;

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            tone.Add(decoder.Stream.ToneHz);
            envelope.Add(decoder.Stream.NewestEnvelope);
        }

        decoder.Flush();

        return new Heard(settled, tone.ToArray(), envelope.ToArray());
    }

    /// <summary>One single-element named character and the figures task 1 of work instruction 425 asks for.</summary>
    /// <param name="Name">The recording.</param>
    /// <param name="IsKeyed">Whether the recording has an inferred key.</param>
    /// <param name="Label">What the key made of it; unscored on a recording with no key.</param>
    /// <param name="Character">What settled.</param>
    /// <param name="GapBefore">Key-up from the previous settled character's end to its start, in units of the read's speed.</param>
    /// <param name="GapAfter">Key-up from its end to the next settled character's start, in the same units.</param>
    /// <param name="KeyDown">Its own key-down length, in the same units.</param>
    /// <param name="Alone">Whether a word gap, or the recording's edge, stands on both sides.</param>
    /// <param name="ToneHz">The median pitch its hops were mixed at.</param>
    /// <param name="PitchHz">The recording's sender pitch: the median of that figure over its named characters.</param>
    /// <param name="Energy">The median envelope over its hops, over the median of the same figure for its three named neighbors each side.</param>
    internal sealed record SingleElement(
        string Name,
        bool IsKeyed,
        Label Label,
        CwCharacter Character,
        double GapBefore,
        double GapAfter,
        double KeyDown,
        bool Alone,
        double ToneHz,
        double PitchHz,
        double Energy)
    {
        /// <summary>A dah is three units long by the book and a dit one.</summary>
        public double KeyDownOverNominal => KeyDown / (Character.Pattern == "-" ? 3.0 : 1.0);

        /// <summary>How far its pitch sat from the sender's, either way.</summary>
        public double PitchOffset => Math.Abs(ToneHz - PitchHz);

        /// <summary>Which of the four heaps it is in.</summary>
        public string Heap => !IsKeyed ? "unkeyed" : Label switch
        {
            Label.Right => "right",
            Label.Wrong => "wrong",
            Label.Added => "added",
            _ => "keyed, unscored",
        };
    }

    /// <summary>The figures of every single-element named character on one recording.</summary>
    /// <param name="name">The recording.</param>
    /// <param name="isKeyed">Whether it has a key.</param>
    /// <param name="heard">What settled, with the per-hop records.</param>
    /// <param name="labels">One label per settled character.</param>
    /// <returns>One entry per single-element named character.</returns>
    internal static IReadOnlyList<SingleElement> Singles(
        string name, bool isKeyed, Heard heard, Label[] labels)
    {
        var settled = heard.Settled;
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;

        int EndHop(CwCharacter c) => (int)Math.Round(c.At.TotalMilliseconds / hopMs);
        int StartHop(CwCharacter c) => EndHop(c) - c.SpanHops;

        double MedianOver(double[] series, CwCharacter c)
        {
            var from = Math.Max(0, StartHop(c));
            var to = Math.Min(series.Length, EndHop(c));

            return to > from ? Median(series.Skip(from).Take(to - from)) : double.NaN;
        }

        var named = Enumerable.Range(0, settled.Count).Where(i => IsNamed(settled[i])).ToList();
        var pitch = Median(named.Select(i => MedianOver(heard.Tone, settled[i])));
        var singles = new List<SingleElement>();

        for (var n = 0; n < named.Count; n++)
        {
            var i = named[n];
            var c = settled[i];

            if (c.Pattern.Length != 1)
            {
                continue;
            }

            var unit = c.WordsPerMinute > 0 ? 1200.0 / c.WordsPerMinute / hopMs : double.NaN;

            var before = Enumerable.Range(0, i).Reverse().Select(j => settled[j]).FirstOrDefault(p => !p.IsWordGap);
            var after = Enumerable.Range(i + 1, settled.Count - i - 1).Select(j => settled[j]).FirstOrDefault(p => !p.IsWordGap);

            var neighbors = named.Take(n).TakeLast(3).Concat(named.Skip(n + 1).Take(3))
                .Select(j => MedianOver(heard.Envelope, settled[j]));

            singles.Add(new SingleElement(
                name,
                isKeyed,
                labels[i],
                c,
                before is null ? double.NaN : (StartHop(c) - EndHop(before)) / unit,
                after is null ? double.NaN : (StartHop(after) - EndHop(c)) / unit,
                c.SpanHops / unit,
                (i == 0 || settled[i - 1].IsWordGap) && (i == settled.Count - 1 || settled[i + 1].IsWordGap),
                MedianOver(heard.Tone, c),
                pitch,
                MedianOver(heard.Envelope, c) / Median(neighbors)));
        }

        return singles;
    }

    /// <remarks>
    /// Proves task 1 of work instruction 425: every single-element named
    /// character on the keyed recordings and on every unkeyed capture row, with
    /// the key's verdict, the raw span, the window Gate score, the gaps either
    /// side and its own length in units of the read's speed, whether it stands
    /// alone between word gaps, its pitch against the sender's, and its energy
    /// against its neighbors'; then, measure by measure, which side the added
    /// letters fall on and what else stands on that side of each one; then the
    /// same for every pair of measures together. **Asserts nothing, and chooses
    /// nothing:** the side a measure's strays fall on is read off the medians of
    /// the added and right heaps, and every count is printed whole.
    /// </remarks>
    [Fact]
    public void WhatSeparatesAStrayFromALetter()
    {
        var rows = TheCapturesThatDecodeKeepDecodingTests.Floors.Select(row => (string)row[0]).ToList();
        var names = rows.Append(TheSeventeenThirtySevenCaptureTests.Name).Distinct().ToList();
        var singles = new List<SingleElement>();
        var edits = 0;
        var length = 0;
        var keyedSeen = 0;

        foreach (var name in names)
        {
            var heard = Hear(name);
            var keyed = KeyedRecordings.SingleOrDefault(k => k.Name == name);
            var scores = keyed is null ? Array.Empty<CwScore>() : keyed.Score(CwReading.Of(heard.Settled));

            edits += scores.Sum(s => s.Edits);
            length += scores.Sum(s => s.ScoredLength);
            keyedSeen += keyed is null ? 0 : 1;

            singles.AddRange(Singles(name, keyed is not null, heard, Labels(heard.Settled, scores)));
        }

        _output.WriteLine(
            $"check | read here {edits} edits over {length} characters against inferred keys, "
            + $"{keyedSeen} keyed recordings of {KeyedRecordings.Count}, {names.Count} recordings in all, "
            + $"{names.Count - keyedSeen} with no key");

        _output.WriteLine(
            $"check | the bar | StrayElementSpan {CwProbabilisticDecoder.StrayElementSpan:0.0} on the raw span; "
            + $"single-element named characters below it anywhere: {singles.Count(s => s.Character.SpanLogLikelihoodRatio < CwProbabilisticDecoder.StrayElementSpan)}");

        _output.WriteLine("");
        _output.WriteLine("SINGLES | every single-element named character, keyed recordings first");
        _output.WriteLine(
            "single | recording | heap | at | text | Score | raw span | gap before u | gap after u | key-down u | "
            + "alone | tone Hz | sender pitch Hz | off pitch Hz | energy over neighbors | wpm");

        foreach (var s in singles.OrderBy(s => s.IsKeyed ? 0 : 1).ThenBy(s => s.Name, StringComparer.Ordinal).ThenBy(s => s.Character.At))
        {
            var c = s.Character;

            _output.WriteLine(
                $"single | {s.Name} | {s.Heap} | {c.At.TotalSeconds:0.000} s | `{c.Text}` | {c.Score:0.000} | "
                + $"{c.SpanLogLikelihoodRatio:0.0} | {s.GapBefore:0.00} | {s.GapAfter:0.00} | {s.KeyDown:0.00} | "
                + $"{(s.Alone ? "alone" : "-")} | {s.ToneHz:0.0} | {s.PitchHz:0.0} | {s.PitchOffset:0.0} | "
                + $"{s.Energy:0.000} | {c.WordsPerMinute}");
        }

        var heaps = new[] { "added", "right", "wrong", "keyed, unscored", "unkeyed" };

        _output.WriteLine(
            "count | heap | singles | " + string.Join(" | ", heaps.Select(h => $"{h} {singles.Count(s => s.Heap == h)}")));

        var measures = new (string Name, Func<SingleElement, double> Of)[]
        {
            ("window Gate score", s => s.Character.Score),
            ("raw span", s => s.Character.SpanLogLikelihoodRatio),
            ("gap before, units", s => s.GapBefore),
            ("gap after, units", s => s.GapAfter),
            ("key-down, units", s => s.KeyDown),
            ("key-down over its nominal element", s => s.KeyDownOverNominal),
            ("alone between word gaps, 1 or 0", s => s.Alone ? 1 : 0),
            ("off the sender pitch, Hz", s => s.PitchOffset),
            ("energy over its neighbors' median", s => s.Energy),
        };

        var added = singles.Where(s => s.Heap == "added").ToList();
        var right = singles.Where(s => s.Heap == "right").ToList();
        var others = singles.Where(s => s.Heap != "added").ToList();
        var side = new Dictionary<string, bool>();

        // ---- ONE MEASURE AT A TIME ----
        _output.WriteLine("");
        _output.WriteLine("EDGES | per measure: the side the added heap sits on, read off the two medians, and what stands on that side of each added one");

        foreach (var (measure, of) in measures)
        {
            var a = added.Select(of).Where(v => !double.IsNaN(v)).ToList();
            var r = right.Select(of).Where(v => !double.IsNaN(v)).ToList();
            var low = Median(a) <= Median(r);

            side[measure] = low;

            bool OnStraySide(double v, double bar) => !double.IsNaN(v) && (low ? v <= bar : v >= bar);

            var leastStray = low ? a.Max() : a.Min();
            var nearestRight = low ? r.Min() : r.Max();

            _output.WriteLine(
                $"edge | {measure} | added {(low ? "low" : "high")} | added median {Median(a):G4}, right median {Median(r):G4} | "
                + $"{(low ? "highest added" : "lowest added")} {leastStray:G4} | {(low ? "lowest right" : "highest right")} {nearestRight:G4} | "
                + $"added NaN {added.Count - a.Count}, right NaN {right.Count - r.Count} | "
                + $"on the stray side of every added one: "
                + string.Join(", ", heaps.Skip(1).Select(h => $"{h} {singles.Count(s => s.Heap == h && OnStraySide(of(s), leastStray))}")));

            var order = added.Where(s => !double.IsNaN(of(s)))
                .OrderBy(s => low ? of(s) : -of(s))
                .ToList();

            for (var k = 0; k < order.Count; k++)
            {
                var bar = of(order[k]);

                _output.WriteLine(
                    $"take | {measure} | {k + 1} of {added.Count} | bar {bar:G5} at {order[k].Name} {order[k].Character.At.TotalSeconds:0.000} s `{order[k].Character.Text}` | "
                    + $"added taken {added.Count(s => OnStraySide(of(s), bar))} | "
                    + string.Join(" | ", heaps.Skip(1).Select(h => $"{h} {singles.Count(s => s.Heap == h && OnStraySide(of(s), bar))}")));
            }
        }

        // ---- TWO MEASURES TOGETHER ----
        _output.WriteLine("");
        _output.WriteLine("PAIRS | both measures on the stray side of the least stray added one: what else is inside that box");

        for (var x = 0; x < measures.Length; x++)
        {
            for (var y = x + 1; y < measures.Length; y++)
            {
                var (mx, ofx) = measures[x];
                var (my, ofy) = measures[y];

                double Edge(Func<SingleElement, double> of, bool low)
                {
                    var values = added.Select(of).Where(v => !double.IsNaN(v)).ToList();

                    return low ? values.Max() : values.Min();
                }

                var bx = Edge(ofx, side[mx]);
                var by = Edge(ofy, side[my]);

                bool Inside(SingleElement s)
                    => !double.IsNaN(ofx(s)) && !double.IsNaN(ofy(s))
                       && (side[mx] ? ofx(s) <= bx : ofx(s) >= bx)
                       && (side[my] ? ofy(s) <= by : ofy(s) >= by);

                _output.WriteLine(
                    $"pair | {mx} {(side[mx] ? "<=" : ">=")} {bx:G4} | {my} {(side[my] ? "<=" : ">=")} {by:G4} | "
                    + $"added inside {added.Count(Inside)} of {added.Count} | "
                    + string.Join(" | ", heaps.Skip(1).Select(h => $"{h} {singles.Count(s => s.Heap == h && Inside(s))}")));
            }
        }

        // ---- THE VERDICT THE COUNTS GIVE, PER MEASURE ----
        _output.WriteLine("");

        foreach (var (measure, of) in measures)
        {
            var low = side[measure];
            var order = added.Where(s => !double.IsNaN(of(s))).OrderBy(s => low ? of(s) : -of(s)).ToList();
            var clean = 0;

            foreach (var s in order)
            {
                var bar = of(s);

                if (others.Any(o => !double.IsNaN(of(o)) && (low ? of(o) <= bar : of(o) >= bar)))
                {
                    break;
                }

                clean++;
            }

            _output.WriteLine(
                $"verdict | {measure} | added taken before any other single-element character is on the stray side: {clean} of {added.Count}");
        }
    }
}

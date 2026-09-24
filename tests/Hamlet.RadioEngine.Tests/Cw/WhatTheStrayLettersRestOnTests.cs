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
}

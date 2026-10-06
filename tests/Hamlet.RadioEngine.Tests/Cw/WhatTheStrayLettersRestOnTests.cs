using System.Reflection;
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
/// the retired probabilistic decoder's character margin gated on, and the per-hop
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

        using var chain = new CwChain(audio.SampleRate);
        var decoder = chain.Decoder;
        var settled = new List<CwCharacter>();
        var tone = new List<double>();
        var envelope = new List<double>();

        decoder.CharacterSettled += settled.Add;

        var hop = Math.Max(4, audio.SampleRate / 200);

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            chain.Process(new AudioChunk(
                at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));

            tone.Add(decoder.RunsPrintingHz);
            // The old stream's envelope came out with work instruction 545.
            envelope.Add(double.NaN);
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
        var hopMs = CwCharacter.HopMilliseconds;

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

}

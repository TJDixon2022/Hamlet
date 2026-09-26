using System.Globalization;
using System.Reflection;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// HM-REQ-084 measured on the acceptance spans of DEV_ANALYSIS_2026-08-27 §4 whose
/// audio is in the tree, each printed as the operator reads it with every mark and
/// gap under it against the word's own Morse (work instruction 453, task 1;
/// PHASE_PLAN.md 6.5).
/// </summary>
/// <remarks>
/// <para>**THE WORDS ARE A JUDGING KEY AND NEVER AN INPUT** (R72, HM-REQ-004).
/// They live in this printer and nowhere in `src`; nothing here feeds the decoder
/// anything but the audio. Their truth grade is inferred (V-13): nobody wrote
/// down what was sent.</para>
/// <para>**A SPAN IS A STRETCH OF AUDIO, NOT A STRETCH OF TEXT.** Each is fixed by
/// its start and end on the recording's clock, found once from the decoded text
/// and the envelope, so a change to the decoder cannot move the span it is judged
/// on. The text over it is every settled character whose own span overlaps it.</para>
/// <para>**THE MARKS AND GAPS ARE TIMED AT HALF AMPLITUDE**: key-down where the
/// decoder's own envelope stands within 6 dB of its highest level in the second
/// either side, and 10 dB over the recording's quiet fifth. The estimator's
/// whole-window level is printed beside it for the gap clusters, and is not used
/// for the spans: on `013637` it counts a weaker signal 10 to 15 dB under the
/// sender as key-down and joins it to the `A` of `ABOVE`. Each mark is tied to the
/// letter whose span it overlaps most, and to that letter's pattern in order.
/// What the decoder called each element is its letter's pattern and the spaces it
/// settled; what the word says it should be is the word's Morse, laid beside it
/// mark by mark.</para>
/// <para>**MET** is the word, a boundary on each side and none inside, every
/// letter sure. A printer. It asserts nothing.</para>
/// </remarks>
public sealed class WhatTheNamedWordsReadTests
{
    private const double HopMs = CwProbabilisticDecoder.HopMilliseconds;

    private static readonly MethodInfo ThreeMeans = typeof(CwUnitEstimator)
        .GetMethod("ThreeMeansOnLogs", BindingFlags.NonPublic | BindingFlags.Static)!;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurement is printed.</param>
    public WhatTheNamedWordsReadTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One named span: the word, its recording, and where it sits on the recording's clock.</summary>
    internal sealed record Span(string Word, string Recording, double FromSeconds, double ToSeconds, string HowFound);

    private const string R013637 = "unadjudicated/cw-2026-08-25-013637";

    private const string R021410 = "unadjudicated/cw-2026-08-25-021410";

    /// <summary>The spans whose audio is in the tree, and how each was found.</summary>
    internal static readonly IReadOnlyList<Span> Spans = new Span[]
    {
        new("ABOVE", R013637, 9.40, 11.50,
            "HEAD's text `G OT AB OVE 7`: from the end of the word gap after `T` (9.11 to 9.35 s) to the end of the last mark before the gap ahead of `7`"),
        new("BREEZE", R013637, 20.30, 22.47,
            "HEAD's text `LI TE BR EEZE ALL`: from the end of the gap after `TE` to the end of the last mark before the gap ahead of `ALL`"),
        new("FLEX", R021410, 22.25, 24.97,
            "HEAD's text `IS  FLENT 66OAM`: from the end of the 1.75 s pause after `IS` to the end of the last mark before the 0.99 s pause ahead of `6`"),
    };

    /// <summary>The named spans that cannot be measured on the tree's audio, and why.</summary>
    internal static readonly (string Word, string Recording, string Why, string Recorded)[] NotHere =
    {
        ("WEEKEND", R021410,
            "not measurable here - span not in the recording's audio: the file is the last 30 s the application held; its sidecar credits 37 of the line's characters to this file, and `WEEKEND` stands 60 characters back from the line's end, `THINKING` 47; the file's first keying is the `A` after `NG`",
            "ATEEKEND"),
        ("THINKING", R021410,
            "not measurable here - span not in the recording's audio, as `WEEKEND`",
            "TTHINKING"),
        ("USED TO USE A FIRM", "011447 / 011514",
            "not measurable here - recording not in the tree",
            "USEDTOUSEAFIRM"),
    };

    /// <summary>The recordings the spans sit in.</summary>
    internal static readonly string[] Recordings = { R013637, R021410 };

    /// <summary>Stretches whose envelope level is printed hop by hop.</summary>
    internal static readonly (string Recording, double From, double To)[] Levels =
    {
        (R013637, 9.30, 9.70),
        (R021410, 0.0, 2.25),
        (R021410, 24.15, 24.95),
    };

    /// <summary>One run of the key, placed on the recording's clock.</summary>
    internal sealed record Run(bool Mark, long StartHop, int Hops)
    {
        public double StartMs => StartHop * HopMs;

        public double EndMs => (StartHop + Hops) * HopMs;

        public double Ms => Hops * HopMs;
    }

    /// <summary>The pitch in force when a character settled, and what could be said about it.</summary>
    internal sealed record Pitch(double StreamHz, double TrackerHz, CwPitchProof Proof);

    /// <summary>One element under a span: a mark or a gap, what the decoder called it and what the word says.</summary>
    internal sealed record Element(Run Run, string Decoder, string Word, double UnitMs, string Letter);

    private static string Invariant(FormattableString s) => s.ToString(CultureInfo.InvariantCulture);

    private static double[] Db(double[] envelope) => envelope.Select(v => 20 * Math.Log10(Math.Max(v, 1e-12))).ToArray();

    /// <summary>Key-down within 6 dB of the highest level in the second either side and 10 dB over the quiet fifth.</summary>
    internal static List<Run> HalfAmplitudeRuns(double[] db)
    {
        var sorted = db.OrderBy(v => v).ToArray();
        var floor = sorted[sorted.Length / 5] + 10;
        var reach = (int)(1000 / HopMs);
        var down = new bool[db.Length];

        for (var h = 0; h < db.Length; h++)
        {
            var peak = double.MinValue;

            for (var k = Math.Max(0, h - reach); k < Math.Min(db.Length, h + reach + 1); k++)
            {
                peak = Math.Max(peak, db[k]);
            }

            down[h] = db[h] > Math.Max(peak - 6, floor);
        }

        // A single hop against both neighbours is not a run.
        for (var h = 1; h + 1 < db.Length; h++)
        {
            if (down[h] != down[h - 1] && down[h] != down[h + 1])
            {
                down[h] = down[h - 1];
            }
        }

        var runs = new List<Run>();
        var start = 0;

        for (var h = 1; h <= db.Length; h++)
        {
            if (h == db.Length || down[h] != down[start])
            {
                runs.Add(new Run(down[start], start, h - start));
                start = h;
            }
        }

        return runs;
    }

    /// <summary>The pitch in force at each settled character, from a second decode of the same audio.</summary>
    private static List<Pitch> Pitches(string path)
    {
        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var pitches = new List<Pitch>();

        decoder.CharacterSettled += _ => pitches.Add(new Pitch(decoder.Stream.ToneHz, decoder.Tracker.ToneHz, decoder.Tracker.PitchProof));

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return pitches;
    }

    /// <summary>A letter's Morse, from the decoder's own table, for judging only.</summary>
    internal static string MorseOf(char letter)
        => MorseAlphabet.All.First(p => p.Value == letter.ToString()).Key;

    private static string Class(CwCharacter c)
        => c.IsWordGap ? "space" : c.IsUnreadable ? "unknown" : c.Confidence == CwConfidence.High ? "sure" : "dim";

    /// <summary>The text as the operator reads it: dim letters in brackets, the unknown as its block.</summary>
    internal static string AsRead(IEnumerable<CwCharacter> characters)
        => string.Concat(characters.Select(c =>
            c.IsWordGap ? " "
            : c.IsUnreadable || c.Confidence == CwConfidence.High ? c.Text
            : $"[{c.Text}]"));

    /// <summary>Levenshtein distance, every character including spaces counted.</summary>
    internal static int Distance(string a, string b)
    {
        var d = new int[a.Length + 1, b.Length + 1];

        for (var i = 0; i <= a.Length; i++)
        {
            d[i, 0] = i;
        }

        for (var j = 0; j <= b.Length; j++)
        {
            d[0, j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            for (var j = 1; j <= b.Length; j++)
            {
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            }
        }

        return d[a.Length, b.Length];
    }

    /// <summary>The whole recording's envelope, each hop from the latest read whose window holds it.</summary>
    internal static double[] Stitched(IReadOnlyList<WhereTheWordBoundariesGoWrongTests.Seen> settled)
    {
        var reads = settled.Select(s => s.Read).DistinctBy(r => r.Index).OrderBy(r => r.Index).ToList();
        var length = reads.Max(r => r.WindowStartHop + r.Window.Length);
        var envelope = new double[length];

        foreach (var r in reads)
        {
            Array.Copy(r.Window, 0, envelope, r.WindowStartHop, r.Window.Length);
        }

        return envelope;
    }

    /// <summary>Every settled character, then every run at half amplitude, for one recording, on its own clock.</summary>
    private void Listing(string name, IReadOnlyList<WhereTheWordBoundariesGoWrongTests.Seen> settled, IReadOnlyList<Pitch> pitches, IReadOnlyList<Run> runs)
    {
        for (var i = 0; i < settled.Count; i++)
        {
            var s = settled[i];
            var c = s.Character;
            var p = i < pitches.Count ? pitches[i] : null;

            _output.WriteLine(Invariant(
                $"char | {name} | {i} | {s.StartMs / 1000:0.000} to {s.EndMs / 1000:0.000} s | `{c.Text}` | {Class(c)} | {c.Pattern} | {s.Read.Result.WordsPerMinute:0.0} wpm, speed {s.Read.Proof} | pitch {p?.StreamHz:0.0} Hz, tracker {p?.TrackerHz:0.0} Hz, {p?.Proof} | read {s.Read.Index}"));
        }

        foreach (var run in runs)
        {
            var r = settled.Where(s => !s.Character.IsWordGap).MinBy(s => Math.Abs(s.EndMs - run.StartMs))!.Read;

            _output.WriteLine(Invariant(
                $"run | {name} | {run.StartMs / 1000:0.000} s | {(run.Mark ? "mark" : "gap")} | {run.Ms:0} ms | {run.Ms / r.UnitMs:0.00} u of {r.UnitMs:0} ms | read {r.Index}"));
        }
    }

    /// <summary>The three gap heaps, as DEV_ANALYSIS §4 gave them, from one cut of the whole recording.</summary>
    private void Clusters(string name, double[] envelope, IReadOnlyList<Run> halfRuns)
    {
        var whole = CwUnitEstimator.Measure(envelope, HopMs);
        var estimatorGaps = CwUnitEstimator.Elements(envelope, HopMs).Gaps;
        var halfGaps = halfRuns.Skip(1).SkipLast(1).Where(r => !r.Mark).Select(r => r.Ms).ToList();
        var halfMarks = halfRuns.Where(r => r.Mark).Select(r => r.Ms).ToList();

        foreach (var (how, gaps) in new[] { ("the estimator's level over the whole recording", estimatorGaps), ("half amplitude", (IReadOnlyList<double>)halfGaps) })
        {
            var c = (double[])ThreeMeans.Invoke(null, new object[] { gaps })!;
            var unit = whole.IsReady ? whole.UnitMilliseconds : double.NaN;

            _output.WriteLine(Invariant(
                $"clusters | {name} | {how} | {gaps.Count} gaps | heaps {c[0]:0} / {c[1]:0} / {c[2]:0} ms | {c[0] / unit:0.00} / {c[1] / unit:0.00} / {c[2] / unit:0.00} u of {unit:0} ms ({(whole.IsReady ? whole.WordsPerMinute : double.NaN):0.0} wpm, the estimator over the whole recording) | heaps 1 and 2 {c[1] - c[0]:0} ms apart"));
        }

        var dits = halfMarks.Where(m => m < halfMarks.Average()).ToList();

        _output.WriteLine(Invariant(
            $"marks | {name} | half amplitude | {halfMarks.Count} marks | short median {Median(dits):0} ms, long median {Median(halfMarks.Except(dits).ToList()):0} ms | the short median as a unit is {1200 / Median(dits):0.0} wpm"));
    }

    private static double Median(IReadOnlyList<double> values)
        => values.Count == 0 ? double.NaN : values.OrderBy(v => v).ElementAt(values.Count / 2);

    /// <summary>The word's own elements in order: its marks and the gaps inside it.</summary>
    private static List<string> WordElements(string word)
    {
        var elements = new List<string>();

        for (var i = 0; i < word.Length; i++)
        {
            var morse = MorseOf(word[i]);

            for (var k = 0; k < morse.Length; k++)
            {
                elements.Add(morse[k] == '.' ? "dit" : "dah");

                if (k + 1 < morse.Length)
                {
                    elements.Add("element gap");
                }
            }

            if (i + 1 < word.Length)
            {
                elements.Add("letter gap");
            }
        }

        return elements;
    }

    private static bool Overlaps(Run r, WhereTheWordBoundariesGoWrongTests.Seen s)
        => r.StartMs < s.EndMs && r.EndMs > s.StartMs;

    /// <summary>One span measured, then traced.</summary>
    private (string Kind, bool Met, int Distance, string Text) Measure(
        Span span, IReadOnlyList<WhereTheWordBoundariesGoWrongTests.Seen> settled, IReadOnlyList<Pitch> pitches, List<Run> runs)
    {
        double from = span.FromSeconds * 1000, to = span.ToSeconds * 1000;
        var inside = Enumerable.Range(0, settled.Count)
            .Where(i => !settled[i].Character.IsWordGap && settled[i].StartMs < to && settled[i].EndMs > from)
            .ToList();

        if (inside.Count == 0)
        {
            _output.WriteLine(Invariant($"span | {span.Word} | {span.Recording} | {span.FromSeconds:0.000} to {span.ToSeconds:0.000} s | nothing settled over the span | not met | distance {span.Word.Length} | found: {span.HowFound}"));

            return ("nothing emitted", false, span.Word.Length, "");
        }

        int first = inside[0], last = inside[^1];
        var over = settled.Skip(first).Take(last - first + 1).Select(s => s.Character).ToList();
        var inner = AsRead(over);
        var spaceBefore = first == 0 || settled[first - 1].Character.IsWordGap;
        var spaceAfter = last == settled.Count - 1 || settled[last + 1].Character.IsWordGap;
        var edged = (spaceBefore ? " " : "") + inner + (spaceAfter ? " " : "");
        var distance = Distance(edged, " " + span.Word + " ");
        var sure = over.Where(c => !c.IsWordGap).All(c => !c.IsUnreadable && c.Confidence == CwConfidence.High);
        var met = inner == span.Word && spaceBefore && spaceAfter && sure;
        var classes = string.Join(" ", over.Where(c => !c.IsWordGap).Select(c => $"{c.Text}:{Class(c)}"));

        _output.WriteLine(Invariant(
            $"span | {span.Word} | {span.Recording} | {span.FromSeconds:0.000} to {span.ToSeconds:0.000} s | reads `{inner}` | as read with its edges `{edged}` against ` {span.Word} ` | classes {classes} | boundary before {(spaceBefore ? "yes" : "no")}, after {(spaceAfter ? "yes" : "no")} | {(met ? "met" : "not met")} | distance {distance} | found: {span.HowFound}"));

        var lastRead = settled[last].Read;
        var speeds = string.Join(", ", inside.Select(i => Invariant($"{settled[i].Read.Result.WordsPerMinute:0.0} wpm {settled[i].Read.Proof}")).Distinct());
        var pitchesIn = string.Join(", ", inside.Where(i => i < pitches.Count).Select(i => Invariant($"{pitches[i].StreamHz:0.0} Hz {pitches[i].Proof}")).Distinct());

        _output.WriteLine($"in force | {span.Word} | speed {speeds} | pitch {pitchesIn} | thresholds at the last letter's read: {lastRead.Thresholds}");

        // The marks under the span, the gap before the first and the gap after the last.
        var marks = runs.Where(r => r.Mark && r.StartMs >= from && r.EndMs <= to).ToList();
        var firstMark = runs.IndexOf(marks[0]);
        var lastMark = runs.IndexOf(marks[^1]);
        var under = runs.Skip(firstMark - 1).Take(lastMark - firstMark + 3).ToList();

        // Each mark to the letter it overlaps most, and that letter's pattern in order.
        var owner = new Dictionary<Run, int>();

        foreach (var m in marks)
        {
            var best = inside.Where(i => Overlaps(m, settled[i]))
                .Select(i => (i, overlap: Math.Min(m.EndMs, settled[i].EndMs) - Math.Max(m.StartMs, settled[i].StartMs)))
                .OrderByDescending(x => x.overlap).FirstOrDefault();

            if (best.overlap > 0)
            {
                owner[m] = best.i;
            }
        }

        var calledMark = new Dictionary<Run, string>();

        foreach (var i in inside)
        {
            var mine = marks.Where(m => owner.TryGetValue(m, out var o) && o == i).ToList();
            var pattern = settled[i].Character.Pattern;

            for (var k = 0; k < mine.Count; k++)
            {
                calledMark[mine[k]] = mine.Count == pattern.Length
                    ? (pattern[k] == '.' ? "dit" : "dah") + $" of `{settled[i].Character.Text}`"
                    : $"one of `{settled[i].Character.Text}` {pattern}, {mine.Count} marks under its {pattern.Length} elements";
            }
        }

        var wordElements = WordElements(span.Word);
        var wordMarks = wordElements.Count(e => e is "dit" or "dah");
        var elements = new List<Element>();
        var w = -1;

        if (marks.Count != wordMarks)
        {
            _output.WriteLine($"note | {span.Word} | {marks.Count} marks under the span against the word's {wordMarks}; laid in order");
        }

        string SpaceCall(Run gap)
        {
            var before = runs[runs.IndexOf(gap) - 1];
            var after = runs[runs.IndexOf(gap) + 1];
            int? a = owner.TryGetValue(before, out var x) ? x : null;
            int? b = owner.TryGetValue(after, out var y) ? y : null;

            if (gap == under[0])
            {
                return spaceBefore ? "word gap" : "letter gap, no space settled";
            }

            if (gap == under[^1])
            {
                return spaceAfter ? "word gap" : "letter gap, no space settled";
            }

            if (a is null || b is null)
            {
                return "beside a mark no letter holds";
            }

            if (a == b)
            {
                return "element gap";
            }

            return Enumerable.Range(a.Value, b.Value - a.Value).Any(i => settled[i].Character.IsWordGap) ? "word gap" : "letter gap";
        }

        foreach (var r in under)
        {
            string word;

            if (r == under[0] || r == under[^1])
            {
                word = "word gap";
            }
            else if (r.Mark)
            {
                // The mark's own place in the word; the gap after it takes the next slot.
                w = w < 0 ? 0 : w + 2;
                word = w < wordElements.Count ? wordElements[w] : "beyond the word";
            }
            else
            {
                word = w + 1 < wordElements.Count ? wordElements[w + 1] : "beyond the word";
            }

            var decoder = r.Mark
                ? calledMark.TryGetValue(r, out var called) ? called : "not read as a mark: inside a gap the path placed"
                : SpaceCall(r);
            var holder = r.Mark && owner.TryGetValue(r, out var o2) ? settled[o2] : settled.Where(s => !s.Character.IsWordGap).MinBy(s => Math.Abs(s.EndMs - r.StartMs))!;

            elements.Add(new Element(r, decoder, word, holder.Read.UnitMs, holder.Character.Text));
        }

        string? departure = null;

        foreach (var e in elements)
        {
            var agrees = e.Decoder.StartsWith(e.Word, StringComparison.Ordinal);
            var consequence = !e.Run.Mark && e.Decoder.StartsWith("beside a mark", StringComparison.Ordinal);
            var flag = agrees ? "" : consequence ? " | follows from the mark beside it" : " | DEPARTS";

            _output.WriteLine(Invariant(
                $"element | {span.Word} | {e.Run.StartMs / 1000:0.000} s | {(e.Run.Mark ? "mark" : "gap")} | {e.Run.Ms:0} ms | {e.Run.Ms / e.UnitMs:0.00} u of {e.UnitMs:0} ms | decoder: {e.Decoder} | word: {e.Word}{flag}"));

            // The settled space inside the span, and the read that settled it.
            if (!e.Run.Mark && e.Decoder == "word gap" && e.Run != under[0] && e.Run != under[^1])
            {
                var space = Enumerable.Range(first, last - first).Where(i => settled[i].Character.IsWordGap)
                    .MinBy(i => Math.Abs(settled[i].EndMs - e.Run.EndMs));
                var r = settled[space].Read;
                var own = r.Result.Gaps.Where(g => g.EndHop == settled[space].EndHop - r.WindowStartHop).Cast<CwPathGap?>().FirstOrDefault();

                _output.WriteLine(Invariant(
                    $"space | {span.Word} | {e.Run.StartMs / 1000:0.000} s | path's gap {(own is { } pg ? pg.SpanHops * HopMs : double.NaN):0} ms, read as a {(own is { } pg2 && pg2.IsWordGap ? "word" : "letter")} gap | settled in: {r.Thresholds}"));
            }

            if (!agrees && !consequence && departure is null)
            {
                departure = e.Run.Mark
                    ? e.Decoder.StartsWith("not read", StringComparison.Ordinal)
                        ? Invariant($"a mark misread: a {e.Word} of {e.Run.Ms:0} ms ({e.Run.Ms / e.UnitMs:0.00} u) at {e.Run.StartMs / 1000:0.000} s not read as a mark")
                        : Invariant($"a mark misread: the word's {e.Word} at {e.Run.StartMs / 1000:0.000} s, {e.Run.Ms:0} ms ({e.Run.Ms / e.UnitMs:0.00} u), read as {e.Decoder}")
                    : Invariant($"a gap misread: the word's {e.Word} at {e.Run.StartMs / 1000:0.000} s, {e.Run.Ms:0} ms ({e.Run.Ms / e.UnitMs:0.00} u), read as a {e.Decoder}");
            }
        }

        var kind = departure is null
            ? "no departure in the marks and gaps"
            : departure.StartsWith("a mark", StringComparison.Ordinal)
                ? "mark misread"
                : departure.Contains("letter gap at", StringComparison.Ordinal) && departure.EndsWith("read as a word gap", StringComparison.Ordinal)
                    ? "gap misread, a letter gap read as a word gap"
                    : "gap misread, other";

        _output.WriteLine($"departure | {span.Word} | {kind} | {departure ?? "none"}");

        return (kind, met, distance, inner);
    }

    /// <remarks>
    /// Proves 6.5's measurement: each named span in the tree printed with its
    /// recording, its time, the text over it as the operator reads it with each
    /// letter's class, met or not met, and the edit distance to the word; each span
    /// not in the tree's audio named as not measurable here; then every mark and gap
    /// inside a span and one gap either side, in milliseconds and units, what the
    /// decoder called it and what the word's own Morse says, the speed and the
    /// pitch with their proof states, the thresholds in force, and where the read
    /// first departs from the word. Asserts nothing.
    /// </remarks>
    [Fact]
    public void EachNamedSpanAsTheOperatorReadsIt()
    {
        var results = new List<(Span Span, string Kind, bool Met, int Distance, string Text)>();

        foreach (var name in Recordings)
        {
            var path = Path.Combine(CapturedSignalTests.Folder, name + ".wav");
            var decoded = WhereTheWordBoundariesGoWrongTests.Decode(path, 600);
            var pitches = Pitches(path);
            var envelope = Stitched(decoded.Settled);
            var db = Db(envelope);
            var runs = HalfAmplitudeRuns(db);

            var firstMark = runs.First(r => r.Mark);
            var sortedDb = db.OrderBy(v => v).ToArray();

            _output.WriteLine(
                Invariant($"recording | {name} | {decoded.Settled.Count} settled | pitch record {pitches.Count} | first mark at {firstMark.StartMs / 1000:0.000} s | ")
                + Invariant($"loudest hop before it {db.Take((int)firstMark.StartHop).DefaultIfEmpty(double.NaN).Max():0.0} dB, the recording's quiet fifth {sortedDb[sortedDb.Length / 5]:0.0} dB, its loudest hop {sortedDb[^1]:0.0} dB | ")
                + $"text `{AsRead(decoded.Settled.Select(s => s.Character))}`");
            Clusters(name, envelope, runs);

            foreach (var span in Spans.Where(s => s.Recording == name))
            {
                var (kind, met, distance, text) = Measure(span, decoded.Settled, pitches, runs);

                results.Add((span, kind, met, distance, text));
            }

            Listing(name, decoded.Settled, pitches, runs);

            foreach (var (from, to) in Levels.Where(l => l.Recording == name).Select(l => (l.From, l.To)))
            {
                for (var h = (int)(from * 1000 / HopMs); h < Math.Min(db.Length, (int)(to * 1000 / HopMs)); h++)
                {
                    _output.WriteLine(Invariant($"level | {name} | {h * HopMs / 1000:0.000} s | {db[h]:0.0} dB"));
                }
            }
        }

        foreach (var (word, recording, why, recorded) in NotHere)
        {
            _output.WriteLine($"span | {word} | {recording} | {why} | DEV_ANALYSIS §4 recorded `{recorded}`");
        }

        _output.WriteLine($"summary | {results.Count(r => r.Met)} of {results.Count} measurable spans met | {NotHere.Length} not measurable here | "
                          + string.Join(", ", results.Select(r => $"{r.Span.Word} `{r.Text}` {(r.Met ? "met" : "not met")} distance {r.Distance}")));

        foreach (var group in results.GroupBy(r => r.Kind))
        {
            _output.WriteLine($"group | {group.Key} | {group.Count()} | {string.Join(", ", group.Select(r => r.Span.Word))}");
        }
    }
}

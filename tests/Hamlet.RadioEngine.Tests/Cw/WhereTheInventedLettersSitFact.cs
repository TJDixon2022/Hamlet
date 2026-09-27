using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every sure letter MET-INVENTED counts, added or substituted, placed against the
/// sent text: in a sent word gap, in a sent character gap, one of two or more letters
/// over one sent character (a split), one letter over two or more (a merge), or in
/// place of one (a stand-in); with every sure right letter beside them in the same
/// terms (work instruction 472, task 1; PHASE_PLAN.md 3.2; HM-REQ-011).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.**</para>
/// <para>**THE SAME HARNESS AND THE SAME ARITHMETIC AS
/// <see cref="TheRequirementsAreMeasuredTests"/>**: the decoder driven hop by hop from
/// the same starting pitch, flushed, the baseline's scored stretches, and
/// <see cref="CwMetrics.Align"/> deciding added against substituted. No second
/// alignment is made.</para>
/// <para>**WHERE A LETTER SITS, READ TWO WAYS, NEITHER A SECOND ALIGNMENT** (work
/// instruction 472, DECIDED (4)). On the alignment: an added letter is in a word gap
/// where the key has a word boundary, or the stretch's end, at the key position it
/// was added at, and in a character gap otherwise; a letter is part of a split where
/// it and its neighbours in the alignment, one of them standing for a key character
/// and the rest added, spell that key character's Morse; a substituted letter is a
/// merge where its Morse is the key characters' it stands for and one or two deleted
/// neighbours spelled together; any other substituted letter is a stand-in. On the
/// synthetic set, whose key is exact by construction, also on the key's timing:
/// the sent marks rebuilt from <see cref="CwFixtureGenerator.KeyEdges"/>, the letter's
/// span moved by the median lag of the sure right letters in its case, and the letter
/// placed by which sent characters have a mark centre inside that span.</para>
/// <para>**THE MARKS ARE THE ENVELOPE'S**, cut as <see cref="CwUnitEstimator.InnerElements"/>
/// cuts them over the letter's span one unit either side, as unit 445 read them. The
/// gaps either side and the levels are this printer's own cut, a threshold halfway
/// between the 20th and 95th percentile in dB over twelve units either side, and are
/// for the record; the span-to-span gaps beside them are the decoder's own spans.</para>
/// <para>**THE KEY IS READ BY THIS PRINTER AND NEVER HANDED TO THE DECODER** (R72).
/// **EVERY REAL KEY IS INFERRED** (V-13); **EVERY SYNTHETIC KEY IS EXACT** and never
/// sole evidence (12.5).</para>
/// </remarks>
public sealed class WhereTheInventedLettersSitFact
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    private static readonly System.Reflection.FieldInfo HeldField =
        typeof(CwProbabilisticStream).GetField("_structureHeld", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

    private static readonly System.Reflection.FieldInfo HeldGapsField =
        typeof(CwProbabilisticStream).GetField("_heldGaps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

    private static readonly string[] Positions ={ "word gap", "character gap", "split", "merge", "stand-in", "one for one", "no sent mark" };

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the fact.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhereTheInventedLettersSitFact(ITestOutputHelper output)
        => _output = output;

    /// <summary>A decode with what was in force at each emission and the envelope per hop.</summary>
    private sealed record Heard(
        IReadOnlyList<CwCharacter> Settled, double[] Envelope, IReadOnlyList<double> UnitMs, IReadOnlyList<double> PitchHz,
        IReadOnlyList<CwPitchProof> Pitch, IReadOnlyList<CwSpeedProof> Speed, IReadOnlyList<double[]?> HeldGaps);

    /// <summary>One sent character of a synthetic case, its marks on the audio clock.</summary>
    private sealed record SentCharacter(int Index, char Text, int Word, IReadOnlyList<(double Start, double End)> Marks)
    {
        public double Start => Marks[0].Start;

        public double End => Marks[^1].End;
    }

    /// <summary>One sure letter over a scored stretch, placed.</summary>
    private sealed record Placed(
        string Name, string Set, string Condition, CwKeyKind Kind, CwCharacter Character, int Index, string Sent, string Class,
        string Aligned, string Timed, string TimedSent, double UnitMs, double PitchHz, CwPitchProof Pitch, CwSpeedProof Speed,
        IReadOnlyList<double> Marks, IReadOnlyList<double> Gaps, double CutGapBeforeMs, double CutGapAfterMs,
        double SpanGapBeforeMs, double SpanGapAfterMs, double MarkDb, double GapBeforeDb, double GapAfterDb, bool StartsInsidePrevious,
        bool NextStartsInside, double[]? Held, bool WordGapBefore, bool WordGapAfter)
    {
        /// <summary>The character gap in force: the stream's held length, or three units where none is held.</summary>
        public double CharacterMs => Held is { } h ? h[1] : 3 * UnitMs;

        /// <summary>
        /// The shorter key-up beside the letter that the decoder read as a character gap
        /// (no word gap settled there), over the character gap in force; NaN where there is none.
        /// </summary>
        public double NearCharacterGapRatio
        {
            get
            {
                var sides = new List<double>();

                if (!WordGapBefore && SpanGapBeforeMs > 0)
                {
                    sides.Add(SpanGapBeforeMs);
                }

                if (!WordGapAfter && SpanGapAfterMs > 0)
                {
                    sides.Add(SpanGapAfterMs);
                }

                return sides.Count == 0 ? double.NaN : sides.Min() / CharacterMs;
            }
        }

        public bool IsRight => Class == "right";

        public bool Invented => !IsRight;

        /// <summary>The reading the table uses: the key's timing where the key is exact, the alignment otherwise.</summary>
        public string Position => Kind == CwKeyKind.Exact ? Timed : Aligned;

        public bool Acquiring => Pitch != CwPitchProof.Proved || Speed != CwSpeedProof.Proved;

        public double WorstMark => Marks.Count == 0 ? double.PositiveInfinity
            : Marks.Max(m => WhatTheSureLettersMarksLookLikeTests.MarkDistance(m / UnitMs));

        public double WorstGap => Gaps.Count == 0 ? 1 : Gaps.Max(g => WhatTheSureLettersMarksLookLikeTests.Distance(g / UnitMs, 1));

        /// <summary>The smaller of the mark level over either neighbouring gap's, in dB.</summary>
        public double Contrast => Math.Min(MarkDb - GapBeforeDb, MarkDb - GapAfterDb);

        /// <summary>Unit 442's rival margin under one nat.</summary>
        public bool UnderRivalMargin => !double.IsNaN(Character.MarginLlr) && Character.MarginLlr < 1;

        /// <summary>Unit 445's inner-gap edge, past 6.5 units.</summary>
        public bool PastInnerGapEdge => WorstGap > 6.5;
    }

    private static Heard Decode(string path, double pitchHz)
    {
        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, pitchHz);
        var settled = new List<CwCharacter>();
        var unit = new List<double>();
        var pitch = new List<double>();
        var pitchProof = new List<CwPitchProof>();
        var speedProof = new List<CwSpeedProof>();
        var envelope = new List<double>();
        var held = new List<double[]?>();

        decoder.CharacterSettled += c =>
        {
            settled.Add(c);

            var gaps = (CwUnitEstimator.CwGapLengths)HeldGapsField.GetValue(decoder.Stream)!;

            held.Add((bool)HeldField.GetValue(decoder.Stream)!
                ? new[] { gaps.ElementMilliseconds, gaps.CharacterMilliseconds, gaps.WordMilliseconds }
                : null);

            var wpm = decoder.Stream.Last.WordsPerMinute;

            unit.Add(wpm > 0 ? 1200.0 / wpm : double.NaN);
            pitch.Add(decoder.Tracker.ToneHz);
            pitchProof.Add(decoder.Tracker.PitchProof);
            speedProof.Add(decoder.SpeedProof);
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            envelope.Add(decoder.Stream.NewestEnvelope);
        }

        decoder.Flush();

        return new Heard(settled, envelope.ToArray(), unit, pitch, pitchProof, speedProof, held);
    }

    private static (int Start, int End) Span(CwCharacter c)
    {
        var end = (int)Math.Round(c.At.TotalMilliseconds / CwProbabilisticDecoder.HopMilliseconds);

        return (end - Math.Max(1, c.SpanHops), end);
    }

    /// <summary>The sent characters of a synthetic case, from the generator's own key edges.</summary>
    private static List<SentCharacter> SentOf(CwFixtureRecipe recipe)
    {
        var edges = CwFixtureGenerator.KeyEdges(recipe, out _);
        var sent = new List<SentCharacter>();
        var mark = 0;
        var word = 0;

        foreach (var w in recipe.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var ch in w)
            {
                var pattern = MorseCode.Spell(ch);

                if (pattern is null)
                {
                    continue;
                }

                var marks = new List<(double, double)>();

                foreach (var _ in pattern)
                {
                    marks.Add((edges[2 * mark], edges[(2 * mark) + 1]));
                    mark++;
                }

                sent.Add(new SentCharacter(sent.Count, ch, word, marks));
            }

            word++;
        }

        return sent;
    }

    /// <summary>The sent characters with a mark centre inside a span, in seconds.</summary>
    private static List<SentCharacter> Covering(IReadOnlyList<SentCharacter> sent, double from, double to)
        => sent.Where(s => s.Marks.Any(m => (m.Start + m.End) / 2 >= from && (m.Start + m.End) / 2 <= to)).ToList();

    private static string Spell(string text) => text.Length == 1 ? MorseCode.Spell(text[0]) ?? "?" : "?";

    /// <summary>The alignment's reading of where a decoded step sits.</summary>
    private static string AlignedPosition(CwMetricAlignment a, int i, IReadOnlyList<string> patterns)
    {
        var steps = a.Steps;
        var step = steps[i];
        var right = step.Key is not null && string.Equals(step.Key, step.Decoded!.Value.Text, StringComparison.Ordinal);

        // A split: a run of decoded steps holding this one, exactly one standing for a
        // key character and at least one added, that spells the key character.
        for (var len = 2; len <= 4; len++)
        {
            for (var from = i - len + 1; from <= i; from++)
            {
                if (from < 0 || from + len > steps.Count)
                {
                    continue;
                }

                var window = Enumerable.Range(from, len).ToList();

                if (window.Any(j => steps[j].Decoded is null) || window.Count(j => steps[j].Key is not null) != 1)
                {
                    continue;
                }

                var key = steps[window.Single(j => steps[j].Key is not null)].Key!;

                if (string.Concat(window.Select(j => patterns[j])) == Spell(key))
                {
                    return "split";
                }
            }
        }

        if (right)
        {
            return "one for one";
        }

        if (step.Key is null)
        {
            var p = step.KeyBefore;

            return p == 0 || p == a.KeyCharacters || a.KeyBoundaries.Contains(p) ? "word gap" : "character gap";
        }

        // A merge: this letter's Morse is its key character's and one or two deleted neighbours' spelled together.
        for (var len = 2; len <= 3; len++)
        {
            for (var from = i - len + 1; from <= i; from++)
            {
                if (from < 0 || from + len > steps.Count)
                {
                    continue;
                }

                var window = Enumerable.Range(from, len).ToList();

                if (window.Any(j => j != i && (steps[j].Decoded is not null || steps[j].Key is null)))
                {
                    continue;
                }

                if (string.Concat(window.Select(j => Spell(steps[j].Key!))) == patterns[i])
                {
                    return "merge";
                }
            }
        }

        return "stand-in";
    }

    /// <summary>This printer's own cut around a letter: the gaps either side and the levels, for the record.</summary>
    private static (double GapBeforeMs, double GapAfterMs, double MarkDb, double BeforeDb, double AfterDb) Cut(
        double[] envelope, (int Start, int End) span, double unitMs)
    {
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var pad = double.IsNaN(unitMs) ? 60 : (int)Math.Round(12 * unitMs / hopMs);
        var from = Math.Max(0, span.Start - pad);
        var to = Math.Min(envelope.Length, span.End + pad);

        if (to - from < 4)
        {
            return (double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
        }

        var db = envelope.Skip(from).Take(to - from).Select(e => 20 * Math.Log10(Math.Max(e, 1e-12))).ToArray();
        var sorted = db.OrderBy(v => v).ToArray();
        var cut = (sorted[(int)(0.20 * (sorted.Length - 1))] + sorted[(int)(0.95 * (sorted.Length - 1))]) / 2;
        var runs = new List<(bool Mark, int Start, int End)>();
        var start = 0;

        for (var i = 1; i <= db.Length; i++)
        {
            if (i == db.Length || (db[i] > cut) != (db[start] > cut))
            {
                runs.Add((db[start] > cut, from + start, from + i));
                start = i;
            }
        }

        var inside = runs.Select((r, k) => (r, k))
            .Where(p => p.r.Mark && (p.r.Start + p.r.End) / 2.0 >= span.Start && (p.r.Start + p.r.End) / 2.0 <= span.End)
            .ToList();

        if (inside.Count == 0)
        {
            return (double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
        }

        double Mean(int a, int b) => a >= b ? double.NaN : db.Skip(a - from).Take(b - a).Average();

        var first = inside[0].k;
        var last = inside[^1].k;
        var before = first > 0 && !runs[first - 1].Mark && first - 1 > 0 ? runs[first - 1] : ((bool, int, int)?)null;
        var after = last + 1 < runs.Count - 1 && !runs[last + 1].Mark ? runs[last + 1] : ((bool, int, int)?)null;
        var markDb = inside.SelectMany(p => Enumerable.Range(p.r.Start, p.r.End - p.r.Start)).Select(h => db[h - from]).Average();

        return (
            before is { } b1 ? (b1.Item3 - b1.Item2) * hopMs : double.NaN,
            after is { } a1 ? (a1.Item3 - a1.Item2) * hopMs : double.NaN,
            markDb,
            before is { } b2 ? Mean(b2.Item2, b2.Item3) : double.NaN,
            after is { } a2 ? Mean(a2.Item2, a2.Item3) : double.NaN);
    }

    private static List<Placed> Place(
        string name, string set, string condition, CwKeyKind kind, Heard heard, IEnumerable<CwScore> scores,
        IReadOnlyList<SentCharacter>? sent, Action<string> print)
    {
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var settled = heard.Settled;
        var index = new Dictionary<CwCharacter, int>(ReferenceEqualityComparer.Instance);

        for (var i = 0; i < settled.Count; i++)
        {
            index[settled[i]] = i;
        }

        var named = settled.Where(c => !c.IsWordGap).ToList();
        var steps = new List<(CwMetricAlignment A, int Step, CwCharacter C, List<string> Patterns)>();

        foreach (var score in scores)
        {
            var covered = TheRequirementsAreMeasuredTests.Covered(settled, score);
            var characters = covered.Where(c => !c.IsWordGap).ToList();
            var alignment = CwMetrics.Align(CwMetrics.Symbols(covered), score.Key, kind);
            var patterns = new List<string>();
            var d = 0;

            foreach (var step in alignment.Steps)
            {
                patterns.Add(step.Decoded is null ? "" : characters[d++].Pattern);
            }

            d = 0;

            for (var s = 0; s < alignment.Steps.Count; s++)
            {
                if (alignment.Steps[s].Decoded is not null)
                {
                    steps.Add((alignment, s, characters[d++], patterns));
                }
            }
        }

        // The lag of the decoder's spans behind the sent marks, from the sure right letters (exact key only).
        double lagStart = double.NaN, lagEnd = double.NaN;

        if (sent is not null)
        {
            var pairs = steps
                .Where(p => p.A.Steps[p.Step] is { Key: { } k, Decoded: { Class: CwSymbolClass.Sure } dd } && k == dd.Text)
                .Where(p => p.A.Steps[p.Step].KeyBefore < sent.Count)
                .Select(p => (Emitted: Span(p.C), Sent: sent[p.A.Steps[p.Step].KeyBefore]))
                .ToList();

            if (pairs.Count > 0)
            {
                lagStart = Median(pairs.Select(p => (p.Emitted.Start * hopMs / 1000.0) - p.Sent.Start));
                lagEnd = Median(pairs.Select(p => (p.Emitted.End * hopMs / 1000.0) - p.Sent.End));
            }

            print(string.Create(Invariant,
                $"lag | {name} | sent characters {sent.Count} | key characters {steps.FirstOrDefault().A?.KeyCharacters} | sure right pairs {pairs.Count} | "
                + $"span start lag {lagStart * 1000:0} ms | span end lag {lagEnd * 1000:0} ms"));
        }

        var placed = new List<Placed>();

        foreach (var (a, s, c, patterns) in steps)
        {
            var step = a.Steps[s];

            if (step.Decoded!.Value.Class != CwSymbolClass.Sure)
            {
                continue;
            }

            var i = index[c];
            var n = named.IndexOf(c);
            var cls = step.Key is null ? "added" : string.Equals(step.Key, step.Decoded.Value.Text, StringComparison.Ordinal) ? "right" : "wrong";
            var span = Span(c);
            var unitMs = heard.UnitMs[i];
            var unitHops = double.IsNaN(unitMs) ? 0 : (int)Math.Round(unitMs / hopMs);
            var from = Math.Max(0, span.Start - unitHops);
            var to = Math.Min(heard.Envelope.Length, span.End + unitHops);
            var (marks, gaps) = CwUnitEstimator.InnerElements(heard.Envelope.Skip(from).Take(Math.Max(0, to - from)).ToList(), hopMs);
            var cut = Cut(heard.Envelope, span, unitMs);
            var previous = n > 0 ? Span(named[n - 1]) : ((int, int)?)null;
            var next = n + 1 < named.Count ? Span(named[n + 1]) : ((int, int)?)null;

            var timed = "-";
            var timedSent = "-";

            if (sent is not null && !double.IsNaN(lagStart))
            {
                var t0 = (span.Start * hopMs / 1000.0) - lagStart;
                var t1 = (span.End * hopMs / 1000.0) - lagEnd;
                var over = Covering(sent, t0, t1);

                timedSent = over.Count == 0 ? "(none)" : string.Concat(over.Select(o => o.Text));

                if (over.Count >= 2)
                {
                    timed = "merge";
                }
                else if (over.Count == 1)
                {
                    var o = over[0];
                    var sharing = named.Count(e =>
                    {
                        var es = Span(e);

                        return Covering(new[] { o }, (es.Start * hopMs / 1000.0) - lagStart, (es.End * hopMs / 1000.0) - lagEnd).Count > 0;
                    });

                    timed = sharing >= 2 ? "split" : cls == "right" ? "one for one" : "stand-in";
                }
                else
                {
                    var mid = (t0 + t1) / 2;
                    var after = sent.FirstOrDefault(x => x.Start > mid);
                    var before = sent.LastOrDefault(x => x.End < mid);

                    timed = after is null || before is null || after.Word != before.Word ? "word gap" : "character gap";
                    timedSent = $"(none; between `{before?.Text.ToString() ?? "start"}` and `{after?.Text.ToString() ?? "end"}`)";
                }
            }

            placed.Add(new Placed(
                name, set, condition, kind, c, i, step.Key ?? "-", cls, AlignedPosition(a, s, patterns), timed, timedSent,
                unitMs, heard.PitchHz[i], heard.Pitch[i], heard.Speed[i], marks, gaps, cut.GapBeforeMs, cut.GapAfterMs,
                previous is { } p ? (span.Start - p.Item2) * hopMs : double.NaN,
                next is { } q ? (q.Item1 - span.End) * hopMs : double.NaN,
                cut.MarkDb, cut.BeforeDb, cut.AfterDb, previous is { } pp && span.Start < pp.Item2,
                next is { } nn && nn.Item1 < span.End, heard.HeldGaps[i],
                i > 0 && settled[i - 1].IsWordGap, i + 1 < settled.Count && settled[i + 1].IsWordGap));
        }

        return placed;
    }

    private static double Median(IEnumerable<double> values)
    {
        var v = values.OrderBy(x => x).ToArray();

        return v.Length == 0 ? double.NaN : v.Length % 2 == 1 ? v[v.Length / 2] : (v[(v.Length / 2) - 1] + v[v.Length / 2]) / 2;
    }

    private static string U(double ms, double unitMs) => double.IsNaN(ms) ? "-" : string.Create(Invariant, $"{ms:0}ms/{ms / unitMs:0.00}u");

    private static string Db(double v) => double.IsNaN(v) ? "-" : v.ToString("0.0", Invariant);

    private static string Short(string condition)
        => condition
            .Replace("real HF, no CH-* profile, SNR_2500 not measured, ", "", StringComparison.Ordinal)
            .Replace("synthetic, no fading, shaped noise band (not shown to be CH-AWGN), ", "", StringComparison.Ordinal)
            .Replace(" in the passband (not restated in the 2500 Hz reference)", "", StringComparison.Ordinal)
            .Replace(", inside TX-FARNS's 3 to 7", "", StringComparison.Ordinal);

    private static string Routes(Placed p)
    {
        var routes = new List<string>();

        if (p.Acquiring)
        {
            routes.Add("470/471 acquiring");
        }

        if (p.StartsInsidePrevious || p.NextStartsInside)
        {
            routes.Add("443 overlaps a neighbour");
        }

        if (p.PastInnerGapEdge)
        {
            routes.Add("445 inner gap past 6.5");
        }

        if (p.UnderRivalMargin)
        {
            routes.Add("442 margin under 1 nat");
        }

        return routes.Count == 0 ? "none" : string.Join(", ", routes);
    }

    private void Print(IReadOnlyList<Placed> letters)
    {
        _output.WriteLine(
            "letter | set | recording | condition | key | span s | sent there (alignment) | emitted | class | pattern | position | alignment reading | timing reading | sent under the span (timing) | "
            + "unit ms | marks ms/u | inner gaps ms/u | worst mark | worst inner gap | span gap before | span gap after | cut gap before | cut gap after | "
            + "mark dB | gap before dB | gap after dB | WPM | pitch Hz | pitch proof | speed proof | acquiring | margin nats | rival | earlier routes on it | "
            + "held gaps ms (element, character, word) | word gap settled before, after | nearer character gap over the character gap in force");

        foreach (var p in letters)
        {
            var c = p.Character;
            var (s0, s1) = Span(c);
            var hop = CwProbabilisticDecoder.HopMilliseconds / 1000.0;

            _output.WriteLine(string.Create(Invariant,
                $"letter | {p.Set} | {p.Name} | {Short(p.Condition)} | {CwMetrics.KindWord(p.Kind)} | {s0 * hop:0.000} to {s1 * hop:0.000} | `{p.Sent}` | `{c.Text}` | {p.Class} | {c.Pattern} | "
                + $"{p.Position} | {p.Aligned} | {p.Timed} | {p.TimedSent} | {p.UnitMs:0.0} | "
                + $"{string.Join(" ", p.Marks.Select(m => U(m, p.UnitMs)))} | {string.Join(" ", p.Gaps.Select(g => U(g, p.UnitMs)))} | {p.WorstMark:0.00} | {p.WorstGap:0.00} | "
                + $"{U(p.SpanGapBeforeMs, p.UnitMs)} | {U(p.SpanGapAfterMs, p.UnitMs)} | {U(p.CutGapBeforeMs, p.UnitMs)} | {U(p.CutGapAfterMs, p.UnitMs)} | "
                + $"{Db(p.MarkDb)} | {Db(p.GapBeforeDb)} | {Db(p.GapAfterDb)} | {1200.0 / p.UnitMs:0.0} | {p.PitchHz:0.0} | {p.Pitch.ToString().ToLowerInvariant()} | "
                + $"{p.Speed.ToString().ToLowerInvariant()} | {(p.Acquiring ? "yes" : "no")} | {(double.IsNaN(c.MarginLlr) ? "-" : c.MarginLlr.ToString("0.00", Invariant))} | "
                + $"{c.RivalReading ?? "-"} | {Routes(p)} | {(p.Held is { } h ? $"{h[0]:0}, {h[1]:0}, {h[2]:0}" : "not held (1, 3, 7 units)")} | "
                + $"{(p.WordGapBefore ? "yes" : "no")}, {(p.WordGapAfter ? "yes" : "no")} | {p.NearCharacterGapRatio:0.000}"));
        }
    }

    private void Table(IReadOnlyList<Placed> real, IReadOnlyList<Placed> synthetic)
    {
        _output.WriteLine("position | set | key | invented | added | substituted | of them acted on by an earlier route | acted on by none | sure right in the same position | by condition (invented)");

        foreach (var position in Positions)
        {
            foreach (var (set, letters) in new[] { ("real", real), ("synthetic", synthetic) })
            {
                var inv = letters.Where(l => l.Invented && l.Position == position).ToList();
                var rights = letters.Count(l => l.IsRight && l.Position == position);
                var kinds = set == "real" ? "inferred" : "exact";
                var by = string.Join("; ", inv.GroupBy(l => Short(l.Condition)).OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => $"{g.Key} {g.Count()}"));

                _output.WriteLine(
                    $"position | {set} | {kinds} | {position} | {inv.Count} | {inv.Count(l => l.Class == "added")} | {inv.Count(l => l.Class == "wrong")} | "
                    + $"{inv.Count(l => Routes(l) != "none")} | {inv.Count(l => Routes(l) == "none")} | {rights} | {(by.Length == 0 ? "-" : by)}");
            }
        }

        foreach (var (set, letters) in new[] { ("real", real), ("synthetic", synthetic) })
        {
            var inv = letters.Where(l => l.Invented).ToList();

            _output.WriteLine(
                $"routes | {set} | " + string.Join("; ", inv.GroupBy(l => (l.Position, Routes(l))).OrderBy(g => g.Key.Position, StringComparer.Ordinal)
                    .Select(g => $"{g.Key.Position}: {g.Key.Item2} {g.Count()}")));
        }

        var agree = synthetic.Where(l => l.Invented).Count(l => l.Aligned == l.Timed);

        _output.WriteLine($"readings | synthetic | invented letters where the alignment's reading and the timing's agree | {agree} of {synthetic.Count(l => l.Invented)} | "
            + string.Join("; ", synthetic.Where(l => l.Invented && l.Aligned != l.Timed).Select(l => string.Create(Invariant,
                $"{l.Name} {l.Character.At.TotalSeconds:0.000} `{l.Character.Text}` alignment {l.Aligned}, timing {l.Timed}"))));
    }

    /// <summary>For every measured quantity, where the sure right letters lie on both sets, and which invented letters of each position lie past them.</summary>
    private void Edges(IReadOnlyList<Placed> all)
    {
        var quantities = new (string Name, Func<Placed, double> Of)[]
        {
            ("span gap before, units", p => p.SpanGapBeforeMs / p.UnitMs),
            ("span gap after, units", p => p.SpanGapAfterMs / p.UnitMs),
            ("cut gap before, units", p => p.CutGapBeforeMs / p.UnitMs),
            ("cut gap after, units", p => p.CutGapAfterMs / p.UnitMs),
            ("smaller span gap either side, units", p => Math.Min(p.SpanGapBeforeMs, p.SpanGapAfterMs) / p.UnitMs),
            ("worst mark distance", p => p.WorstMark),
            ("worst inner gap distance", p => p.WorstGap),
            ("mark over the nearer gap, dB", p => p.Contrast),
            ("rival margin, nats", p => p.Character.MarginLlr),
            ("span, units", p => Math.Max(1, p.Character.SpanHops) * CwProbabilisticDecoder.HopMilliseconds / p.UnitMs),
            ("nearer character gap over the character gap in force", p => p.NearCharacterGapRatio),
        };

        _output.WriteLine("edge | quantity | right letters measured | right lowest | right highest | position | invented measured | below every right | above every right | which");

        foreach (var (qname, of) in quantities)
        {
            var rights = all.Where(p => p.IsRight).Select(of).Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToList();
            var lo = rights.Count == 0 ? double.NaN : rights.Min();
            var hi = rights.Count == 0 ? double.NaN : rights.Max();

            foreach (var position in Positions)
            {
                var inv = all.Where(p => p.Invented && p.Position == position).ToList();

                if (inv.Count == 0)
                {
                    continue;
                }

                var measured = inv.Where(p => !double.IsNaN(of(p)) && !double.IsInfinity(of(p))).ToList();
                var below = measured.Where(p => of(p) < lo).ToList();
                var above = measured.Where(p => of(p) > hi).ToList();

                var which = string.Join(", ", below.Concat(above).Select(p => string.Create(Invariant, $"{p.Name} {p.Character.At.TotalSeconds:0.000} `{p.Character.Text}` {of(p):0.00}")));

                _output.WriteLine(string.Create(Invariant,
                    $"edge | {qname} | {rights.Count} | {lo:0.000} | {hi:0.000} | {position} | {measured.Count} of {inv.Count} | {below.Count} | {above.Count} | {which}"));
            }
        }
    }

    /// <summary>
    /// The largest group no earlier route acted on, both sets together, and every sure
    /// right letter whose nearer character gap over the character gap in force is no
    /// larger than that group's largest: what a change on that quantity would put at risk.
    /// </summary>
    private void AtRisk(IReadOnlyList<Placed> real, IReadOnlyList<Placed> synthetic)
    {
        var all = real.Concat(synthetic).ToList();
        var group = Positions
            .Select(p => (Position: p, Rows: all.Where(l => l.Invented && l.Position == p && Routes(l) == "none").ToList()))
            .OrderByDescending(g => g.Rows.Count)
            .First();
        var ratios = group.Rows.Select(l => l.NearCharacterGapRatio).Where(v => !double.IsNaN(v)).ToList();
        var top = ratios.Count == 0 ? double.NaN : ratios.Max();

        _output.WriteLine(string.Create(Invariant,
            $"group | {group.Position} | acted on by none {group.Rows.Count} (real {group.Rows.Count(l => l.Set == "real")}, synthetic {group.Rows.Count(l => l.Set == "synthetic")}) | "
            + $"nearer character gap over the character gap in force {(ratios.Count == 0 ? "-" : $"{ratios.Min():0.000} to {top:0.000}")}"));

        foreach (var (set, letters) in new[] { ("real", real), ("synthetic", synthetic) })
        {
            var risk = letters.Where(l => l.IsRight && !double.IsNaN(l.NearCharacterGapRatio) && l.NearCharacterGapRatio <= top).ToList();
            var shown = string.Join(", ", risk.OrderBy(l => l.NearCharacterGapRatio).Select(l => string.Create(Invariant,
                $"{l.Name} {l.Character.At.TotalSeconds:0.000} `{l.Character.Text}` {l.NearCharacterGapRatio:0.000}")));

            _output.WriteLine($"at risk | {set} | sure right letters at or under {top:0.000} | {risk.Count} of {letters.Count(l => l.IsRight)} | {shown}");
        }
    }

    /// <remarks>
    /// Proves nothing about the decoder; prints task 1 of work instruction 472: every sure
    /// invented letter over the scored stretches of the 23 real keyed recordings and the
    /// 12 synthetic cases, where it sits against the sent text, its marks and gaps in
    /// units, the levels either side, the speed and pitch in force with their proof
    /// states, and whether it was emitted while acquiring; every sure right letter in the
    /// same terms; the five-position table per set with the earlier routes beside each;
    /// and for every measured quantity where the right letters lie and which invented
    /// letters lie past all of them.
    /// </remarks>
    [Fact]
    public void EverySureInventedLetterAgainstTheSentText()
    {
        var real = new List<Placed>();

        foreach (var k in WhatTheStrayLettersRestOnTests.KeyedRecordings)
        {
            var heard = Decode(Path.Combine(CapturedSignalTests.Folder, k.Name + ".wav"), 600);

            real.AddRange(Place(k.Name, "real", TheRequirementsAreMeasuredTests.RealCondition(k.Name), CwKeyKind.Inferred,
                heard, k.Score(CwReading.Of(heard.Settled)), null, _output.WriteLine));
        }

        var synthetic = new List<Placed>();

        foreach (var recipe in SyntheticCq.All)
        {
            var heard = Decode(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"), SyntheticCq.StartingPitchHz);
            var reading = CwReading.Of(heard.Settled);
            var from = reading.Text.TakeWhile(char.IsWhiteSpace).Count();

            synthetic.AddRange(Place(recipe.Name, "synthetic", TheRequirementsAreMeasuredTests.SyntheticCondition(recipe), CwKeyKind.Exact,
                heard, new[] { SyntheticCq.Whole(reading) with { Start = from } }, SentOf(recipe), _output.WriteLine));
        }

        Print(real.Where(p => p.Invented).ToList());
        Print(synthetic.Where(p => p.Invented).ToList());
        Table(real, synthetic);
        Edges(real.Concat(synthetic).ToList());
        AtRisk(real, synthetic);
        Print(real.Where(p => p.IsRight).ToList());
        Print(synthetic.Where(p => p.IsRight).ToList());

        foreach (var (set, letters) in new[] { ("real", real), ("synthetic", synthetic) })
        {
            _output.WriteLine(
                $"total | {set} | sure {letters.Count}: right {letters.Count(l => l.IsRight)}, added {letters.Count(l => l.Class == "added")}, "
                + $"substituted {letters.Count(l => l.Class == "wrong")} | invented {letters.Count(l => l.Invented)} (cross-check against TheRequirementsAreMeasuredTests)");
        }
    }
}

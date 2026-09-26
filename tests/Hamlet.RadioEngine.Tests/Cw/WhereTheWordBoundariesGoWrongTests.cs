using System.Globalization;
using System.Reflection;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// MET-WBE per condition with inserted and deleted boundaries apart, and every
/// wrong word boundary on the real and synthetic sets traced to the gap and the
/// thresholds that made it (work instruction 452, task 1; PHASE_PLAN.md 6.3;
/// HM-REQ-080, 081, 082).
/// </summary>
/// <remarks>
/// <para>**THE BOUNDARIES ARE MET-WBE'S OWN.** Each recording is decoded and scored
/// exactly as <see cref="TheRequirementsAreMeasuredTests"/> does, the alignment is
/// <see cref="CwMetrics.Align"/>'s, and a boundary is inserted or deleted by the
/// walk <see cref="CwMetrics.WordBoundaries"/> makes. Beside every count the metric's
/// own count is printed, so a disagreement between the two would show.</para>
/// <para>**WHAT DECIDES A SPACE.** The path reads a gap as a letter or a word gap by
/// its length penalty, whose balance point is the word-from the stream gave it:
/// the geometric mean of the held character and word gaps, or of three and seven
/// units. Then the relabel in <c>CwProbabilisticStream.Spaced</c> takes a word gap
/// out where it is shorter than this read's measured character gap times the root
/// of seven thirds, or, with no character gap measured, shorter than the word-from.
/// It never puts one in. The read's state is taken by reflection at the moment each
/// character settles and the relabel is re-run on the same inputs to find the
/// spaces it removed. It reads; it changes nothing.</para>
/// <para>**A GAP IS MEASURED FROM WHAT SETTLED**: the next letter's start taken
/// from the previous one's end, in units of the unit the path was read at.</para>
/// <para>**EVERY REAL KEY IS INFERRED** (V-13) and every synthetic key exact;
/// the synthetic boundaries come from the generator, never from the decode
/// (CLAUDE.md 12.5). A printer. It asserts nothing.</para>
/// </remarks>
public sealed class WhereTheWordBoundariesGoWrongTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly FieldInfo StructureHeld = typeof(CwProbabilisticStream).GetField("_structureHeld", Private)!;

    private static readonly FieldInfo HeldGaps = typeof(CwProbabilisticStream).GetField("_heldGaps", Private)!;

    private static readonly FieldInfo Envelope = typeof(CwProbabilisticStream).GetField("_envelope", Private)!;

    private static readonly FieldInfo EnvelopeCount = typeof(CwProbabilisticStream).GetField("_envelopeCount", Private)!;

    private static readonly FieldInfo HopsSeen = typeof(CwProbabilisticStream).GetField("_hopsSeen", Private)!;

    private static readonly FieldInfo SettledThrough = typeof(CwProbabilisticStream).GetField("_settledThrough", Private)!;

    private static readonly MethodInfo Spaced = typeof(CwProbabilisticStream)
        .GetMethod("Spaced", BindingFlags.NonPublic | BindingFlags.Static)!;

    /// <summary>Unit 452's letter-space boundary, where the tree has it, so the relabel is re-run as the stream runs it.</summary>
    private static readonly MethodInfo? LetterSpaceBoundary = typeof(CwProbabilisticStream)
        .GetMethod("LetterSpaceBoundary", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly MethodInfo ThreeMeans = typeof(CwUnitEstimator)
        .GetMethod("ThreeMeansOnLogs", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo IsTrough = typeof(CwUnitEstimator)
        .GetMethod("IsTrough", BindingFlags.NonPublic | BindingFlags.Static)!;

    private const double HopMs = CwProbabilisticDecoder.HopMilliseconds;

    /// <summary>Why <see cref="CwUnitEstimator.MeasureCharacterGap"/> gave what it gave on a window, its own tests re-run in its order.</summary>
    private static string CharacterGapWhy(double[] window, CwUnitReading measured)
    {
        if (!measured.IsReady)
        {
            return "no unit measured";
        }

        var unit = measured.UnitMilliseconds;
        var gaps = CwUnitEstimator.Elements(window, HopMs).Gaps;

        if (gaps.Count < 12)
        {
            return Invariant($"refused: {gaps.Count} gaps, under 12");
        }

        var c = (double[])ThreeMeans.Invoke(null, new object[] { gaps })!;
        var shortest = gaps.Min();
        var underHalf = gaps.Count(g => g < unit / 2);
        var heaps = Invariant($"heaps {c[0]:0}/{c[1]:0}/{c[2]:0} ms ({c[0] / unit:0.00}/{c[1] / unit:0.00}/{c[2] / unit:0.00} u of the estimator's {unit:0}), ")
                    + Invariant($"{gaps.Count} gaps, shortest {shortest:0} ms, {underHalf} under half a unit");

        if (c[1] / c[0] < 1.5)
        {
            return "refused, heaps 1 and 2 too close: " + heaps;
        }

        if (!(bool)IsTrough.Invoke(null, new object[] { gaps, c[0], c[1] })!)
        {
            return "refused, no trough between heaps 1 and 2: " + heaps;
        }

        var boundary = Math.Sqrt(c[0] * c[1]) / unit;

        return boundary is >= 1.3 and <= 2.6
            ? "measured: " + heaps
            : Invariant($"refused, boundary {boundary:0.00} u outside 1.3 to 2.6: ") + heaps;
    }

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhereTheWordBoundariesGoWrongTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One read of the stream, as it stood when its characters settled.</summary>
    internal sealed record Read(
        int Index, CwProbabilisticResult Result, bool UnitMeasured, CwSpeedProof Proof,
        bool Held, CwUnitEstimator.CwGapLengths Gaps, int HeldFromRead,
        double? CharacterGapMs, double WordFromMs, double[] Window, long WindowStartHop, string CharacterGapWhy)
    {
        public double UnitMs => Result.WordsPerMinute > 0 ? 1200.0 / Result.WordsPerMinute : double.NaN;

        /// <summary>The relabel's boundary: a word gap shorter than this is taken out.</summary>
        public double RelabelMs => CharacterGapMs is { } g ? g * CwProbabilisticStream.WordGapShare : WordFromMs;

        public string Thresholds
            => Invariant($"unit {UnitMs:0} ms at {Result.WordsPerMinute:0.0} wpm, unit {(UnitMeasured ? "measured" : "from the grid")}, speed {Proof} | ")
               + (Held
                   ? Invariant($"path wants held gaps {Gaps.ElementMilliseconds:0}/{Gaps.CharacterMilliseconds:0}/{Gaps.WordMilliseconds:0} ms from read {HeldFromRead}")
                   : "path wants textbook 3 and 7 units")
               + Invariant($", path word-from {WordFromMs:0} ms ({WordFromMs / UnitMs:0.00} u) | relabel {RelabelMs:0} ms ({RelabelMs / UnitMs:0.00} u) from ")
               + (CharacterGapMs is { } g
                   ? Invariant($"this read's character gap {g:0} ms x 1.53")
                   : "the path's word-from, no character gap measured in this read")
               + Invariant($" | read {Index} | marks' unit {MarksUnitMs:0} ms, {MarksUnitMs / UnitMs:0.00} of the path's | character gap {CharacterGapWhy}");

        /// <summary>The unit the window's marks alone imply (<see cref="CwUnitEstimator.MarkUnit"/>), which reads long by the skirt.</summary>
        public double MarksUnitMs => CwUnitEstimator.MarkUnit(CwUnitEstimator.Elements(Window, HopMs).Marks);
    }

    /// <summary>A settled character and the read it settled in.</summary>
    internal sealed record Seen(CwCharacter Character, Read Read)
    {
        public double EndMs => Character.At.TotalMilliseconds;

        public double StartMs => Character.At.TotalMilliseconds - (Character.SpanHops * HopMs);

        public long EndHop => (long)Math.Round(EndMs / HopMs);
    }

    /// <summary>A word gap the path read that the relabel took out.</summary>
    internal sealed record Removed(long EndHop, int SpanHops, Read Read);

    /// <summary>One recording decoded, with every read's state.</summary>
    internal sealed record Decoded(IReadOnlyList<Seen> Settled, IReadOnlyList<Removed> Removed);

    /// <summary>One wrong boundary.</summary>
    internal sealed record Wrong(
        string Recording, string Set, string Condition, bool Inserted, Seen? Before, Seen? After,
        Seen? Space, Removed? RemovedSpace, CwPathGap? PathGap, string KeyAround, string DecodeAround, int KeyLettersBetween)
    {
        public Read? Context => Space?.Read ?? RemovedSpace?.Read ?? After?.Read ?? Before?.Read;

        public double GapMs => Before is null || After is null ? double.NaN : After.StartMs - Before.EndMs;

        public double Units => GapMs / (Context?.UnitMs ?? double.NaN);
    }

    private static string Invariant(FormattableString s) => s.ToString(CultureInfo.InvariantCulture);

    /// <summary>Decodes a recording as the metrics do, keeping each read's state.</summary>
    internal static Decoded Decode(string path, double pitchHz)
    {
        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, pitchHz);
        var stream = decoder.Stream;
        var settled = new List<Seen>();
        var removed = new List<Removed>();
        var reads = 0;
        Read? current = null;
        long through = -1;
        CwUnitEstimator.CwGapLengths lastGaps = default;
        var heldFrom = 0;

        Read Current()
        {
            if (current is { } c && c.Index == reads + 1)
            {
                return c;
            }

            var count = (int)EnvelopeCount.GetValue(stream)!;
            var window = ((double[])Envelope.GetValue(stream)!)[..count];
            var held = (bool)StructureHeld.GetValue(stream)!;
            var gaps = (CwUnitEstimator.CwGapLengths)HeldGaps.GetValue(stream)!;

            if (!gaps.Equals(lastGaps))
            {
                lastGaps = gaps;
                heldFrom = reads + 1;
            }

            var last = stream.Last;
            var measured = CwUnitEstimator.Measure(window, HopMs);
            var characterGap = measured.IsReady
                ? CwUnitEstimator.MeasureCharacterGap(window, HopMs, measured.UnitMilliseconds)
                : null;
            var unitMs = last.WordsPerMinute > 0 ? 1200.0 / last.WordsPerMinute : 0;
            var wordFrom = held
                ? Math.Sqrt(gaps.CharacterMilliseconds * gaps.WordMilliseconds)
                : Math.Sqrt(3 * 7) * unitMs;

            if (!held && characterGap is null && LetterSpaceBoundary is { } letterSpaces)
            {
                wordFrom = Math.Max(wordFrom, (double)letterSpaces.Invoke(null, new object[] { last })!);
            }

            current = new Read(reads + 1, last, stream.UnitWasMeasured, decoder.SpeedProof, held, gaps, heldFrom,
                characterGap, wordFrom, window, (long)HopsSeen.GetValue(stream)! - count, CharacterGapWhy(window, measured));

            return current;
        }

        decoder.CharacterSettled += c => settled.Add(new Seen(c, Current()));

        stream.LeadingEdgeChanged += _ =>
        {
            if ((int)EnvelopeCount.GetValue(stream)! == 0)
            {
                // A restart, not a read.
                return;
            }

            var r = Current();
            var now = (long)SettledThrough.GetValue(stream)!;

            if (now > through)
            {
                var spaced = (IEnumerable<(CwProbabilisticCharacter Character, bool Removed)>)Spaced
                    .Invoke(null, new object?[] { r.Result, r.CharacterGapMs, r.WordFromMs })!;

                foreach (var (c, gone) in spaced)
                {
                    var absolute = r.WindowStartHop + c.EndHop;

                    if (gone && absolute > through && absolute <= now)
                    {
                        var span = r.Result.Gaps.FirstOrDefault(g => g.EndHop == c.EndHop).SpanHops;

                        removed.Add(new Removed(absolute, span, r));
                    }
                }

                through = now;
            }

            reads++;
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
        }

        decoder.Flush();

        return new Decoded(settled, removed);
    }

    /// <summary>A recording decoded and measured, with the settled index each aligned symbol came from.</summary>
    internal sealed record Traced(
        string Name, string Set, string Condition, CwKeyKind Kind, Decoded Decoded,
        IReadOnlyList<(CwMetricAlignment Alignment, IReadOnlyList<int> Owner, string Key)> Stretches, string? NotComputable);

    private static Traced Trace(string name, string set, string condition, CwKeyKind kind, Decoded decoded,
        Func<CwReading, IReadOnlyList<CwScore>> score)
    {
        var characters = decoded.Settled.Select(s => s.Character).ToList();
        var reading = CwReading.Of(characters);
        var scores = score(reading);

        if (scores.Count == 0)
        {
            return new Traced(name, set, condition, kind, decoded, Array.Empty<(CwMetricAlignment, IReadOnlyList<int>, string)>(),
                "no scored stretch");
        }

        var owner = new List<int>();

        for (var i = 0; i < characters.Count; i++)
        {
            owner.AddRange(Enumerable.Repeat(i, characters[i].Text.Length));
        }

        var stretches = new List<(CwMetricAlignment, IReadOnlyList<int>, string)>();

        foreach (var s in scores)
        {
            var covered = TheRequirementsAreMeasuredTests.Covered(characters, s);
            var first = s.Region.Length == 0 ? 0 : owner[s.Start];
            var alignment = CwMetrics.Align(CwMetrics.Symbols(covered), s.Key, kind);
            var named = new List<int>();

            for (var i = 0; i < covered.Count; i++)
            {
                if (!covered[i].IsWordGap)
                {
                    named.Add(first + i);
                }
            }

            stretches.Add((alignment, named, s.Key));
        }

        return new Traced(name, set, condition, kind, decoded, stretches, null);
    }

    private static readonly Lazy<IReadOnlyList<Traced>> All = new(() =>
    {
        var list = new List<Traced>();

        foreach (var k in WhatTheStrayLettersRestOnTests.KeyedRecordings)
        {
            var decoded = Decode(Path.Combine(CapturedSignalTests.Folder, k.Name + ".wav"), 600);

            list.Add(Trace(k.Name, "real", TheRequirementsAreMeasuredTests.RealCondition(k.Name), CwKeyKind.Inferred, decoded, k.Score));
        }

        foreach (var recipe in SyntheticCq.All)
        {
            var decoded = Decode(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"), SyntheticCq.StartingPitchHz);

            list.Add(Trace(recipe.Name, "synthetic", TheRequirementsAreMeasuredTests.SyntheticCondition(recipe), CwKeyKind.Exact, decoded,
                reading =>
                {
                    var from = reading.Text.TakeWhile(char.IsWhiteSpace).Count();

                    return new[] { SyntheticCq.Whole(reading) with { Start = from } };
                }));
        }

        return list;
    });

    /// <summary>The key's text with a bar where the key has consumed <paramref name="at"/> characters, a word either side.</summary>
    private static string Around(IReadOnlyList<string> tokens, int at)
    {
        // tokens: characters and " " for boundaries, in order; at counts characters.
        var text = new StringBuilder();
        var seen = 0;
        var mark = -1;

        foreach (var t in tokens)
        {
            if (seen == at && mark < 0 && t != " ")
            {
                mark = text.Length;
                text.Append('|');
            }

            text.Append(t);

            if (t != " ")
            {
                seen++;
            }
        }

        if (mark < 0)
        {
            mark = text.Length;
            text.Append('|');
        }

        var s = text.ToString();
        var from = mark;
        var spaces = 0;

        while (from > 0 && !(s[from - 1] == ' ' && ++spaces == 2))
        {
            from--;
        }

        var to = mark;
        spaces = 0;

        while (to < s.Length && !(s[to] == ' ' && ++spaces == 2))
        {
            to++;
        }

        return s[from..to].Replace(' ', '_');
    }

    private static IEnumerable<Wrong> Wrongs(Traced t)
    {
        var settled = t.Decoded.Settled;

        foreach (var (a, owner, key) in t.Stretches)
        {
            var keyTokens = CwMetrics.KeySymbols(key).Select(s => s.Class == CwSymbolClass.WordGap ? " " : s.Text).ToList();
            var decodedTokens = new List<string>();
            var decodedAt = new List<int>();
            var d = 0;

            foreach (var s in a.Steps.Where(s => s.Decoded is not null))
            {
                if (s.GapBefore && decodedTokens.Count > 0)
                {
                    decodedTokens.Add(" ");
                }

                decodedAt.Add(decodedTokens.Count);
                decodedTokens.Add(s.Decoded!.Value.Text);
                d++;
            }

            string DecodeAround(int index)
            {
                var tokens = decodedTokens.Select(x => x).ToList();
                var at = index < decodedAt.Count ? decodedAt[index] : tokens.Count;
                var named = tokens.Take(at).Count(x => x != " ");

                return Around(tokens, named);
            }

            Seen? SpaceBetween(int from, int to)
            {
                for (var i = from + 1; i < to; i++)
                {
                    if (settled[i].Character.IsWordGap)
                    {
                        return settled[i];
                    }
                }

                return null;
            }

            Removed? RemovedBetween(Seen before, Seen after)
                => t.Decoded.Removed.FirstOrDefault(r => r.EndHop >= before.EndHop && r.EndHop <= after.EndHop + 1);

            CwPathGap? PathGap(Seen before, Seen after)
            {
                var r = after.Read;
                var fromHop = before.EndHop - r.WindowStartHop;
                var toHop = after.EndHop - r.WindowStartHop - after.Character.SpanHops;
                var gaps = r.Result.Gaps.Where(g => g.EndHop >= fromHop - 2 && g.StartHop <= toHop + 2).ToList();

                return gaps.Count == 0 ? null : gaps.OrderByDescending(g => g.SpanHops).First();
            }

            var unclaimed = new SortedSet<int>(a.KeyBoundaries);
            int? low = null;
            var index = 0;
            var previous = -1;

            foreach (var s in a.Steps)
            {
                if (s.Decoded is null)
                {
                    continue;
                }

                if (s.GapBefore && low is { } l)
                {
                    var high = s.KeyBefore;
                    var match = unclaimed.GetViewBetween(l, Math.Max(l, high)).Cast<int?>().FirstOrDefault();

                    if (match is { } m)
                    {
                        unclaimed.Remove(m);
                    }
                    else
                    {
                        var before = settled[owner[previous]];
                        var after = settled[owner[index]];

                        yield return new Wrong(t.Name, t.Set, t.Condition, true, before, after,
                            SpaceBetween(owner[previous], owner[index]), null, PathGap(before, after),
                            Around(keyTokens, high), DecodeAround(index), high - l);
                    }
                }

                low = s.KeyAfter;
                previous = index;
                index++;
            }

            foreach (var b in unclaimed)
            {
                var decodedSteps = a.Steps.Where(s => s.Decoded is not null).ToList();
                var beforeIndex = decodedSteps.FindLastIndex(s => s.KeyAfter <= b);
                var afterIndex = beforeIndex + 1 < decodedSteps.Count ? beforeIndex + 1 : -1;
                var before = beforeIndex >= 0 ? settled[owner[beforeIndex]] : null;
                var after = afterIndex >= 0 ? settled[owner[afterIndex]] : null;

                yield return new Wrong(t.Name, t.Set, t.Condition, false, before, after, null,
                    before is not null && after is not null ? RemovedBetween(before, after) : null,
                    before is not null && after is not null ? PathGap(before, after) : null,
                    Around(keyTokens, b), DecodeAround(Math.Max(0, afterIndex < 0 ? decodedSteps.Count : afterIndex)),
                    beforeIndex >= 0 && afterIndex >= 0 ? decodedSteps[afterIndex].KeyBefore - decodedSteps[beforeIndex].KeyAfter : -1);
            }
        }
    }

    /// <summary>A letter's whole marks and the key-ups between them, cut as the estimator cuts.</summary>
    private static string Marks(Seen? s, out int shortKeyUps)
    {
        shortKeyUps = 0;

        if (s is null || s.Character.IsWordGap)
        {
            return "-";
        }

        var r = s.Read;
        var unitHops = double.IsNaN(r.UnitMs) ? 0 : (int)Math.Round(r.UnitMs / HopMs);
        var end = (int)(s.EndHop - r.WindowStartHop);
        var from = Math.Max(0, end - s.Character.SpanHops - unitHops);
        var to = Math.Min(r.Window.Length, end + unitHops);

        if (to - from < 2 || end <= 0)
        {
            return "outside the window";
        }

        var (marks, gaps) = CwUnitEstimator.InnerElements(new ArraySegment<double>(r.Window, from, to - from), HopMs);
        var half = r.UnitMs / 2;

        shortKeyUps = gaps.Count(g => g < half);

        return Invariant($"`{s.Character.Text}` {s.Character.Pattern} marks [{string.Join(" ", marks.Select(m => m.ToString("0", CultureInfo.InvariantCulture)))}] ")
               + Invariant($"key-ups [{string.Join(" ", gaps.Select(g => g.ToString("0", CultureInfo.InvariantCulture)))}]");
    }

    /// <summary>The whole marks the envelope shows inside the gap itself, or null where the gap is not in the window.</summary>
    private static IReadOnlyList<double>? MarksInside(Wrong w)
    {
        if (w.Before is null || w.After is null)
        {
            return null;
        }

        var r = w.After.Read;
        var from = (int)(w.Before.EndHop - r.WindowStartHop);
        var to = (int)(w.After.EndHop - r.WindowStartHop - w.After.Character.SpanHops);

        if (from < 0 || to > r.Window.Length || to - from < 2)
        {
            return null;
        }

        var start = Math.Max(0, from - 2);

        return CwUnitEstimator.InnerElements(new ArraySegment<double>(r.Window, start, Math.Min(r.Window.Length, to + 2) - start), HopMs).Marks;
    }

    private static string Between(Wrong w)
        => MarksInside(w) is not { } marks
            ? "-"
            : marks.Count == 0
                ? "no mark inside the gap"
                : Invariant($"{marks.Count} mark(s) inside the gap [{string.Join(" ", marks.Select(m => m.ToString("0", CultureInfo.InvariantCulture)))}] ms");

    /// <summary>
    /// What the wrong boundary has in common with others: first whether the letters
    /// beside it were read at all, then which mechanism set the boundary in force.
    /// </summary>
    /// <remarks>
    /// Mechanism only, from the stream's own state; no count or score chose a group.
    /// Where the alignment has key letters between the two decoded letters that the
    /// decode never read, the boundary stands where letters were lost rather than on
    /// a spacing (the marks inside the gap are printed beside it and are not used:
    /// the path's letter spans sit late against the envelope's marks, so a letter's
    /// own first mark can fall in what is measured as the gap).
    /// </remarks>
    internal static string Group(Wrong w)
    {
        if (w.Before is null || w.After is null || w.Context is null)
        {
            return "G6 no decoded letter on one side";
        }

        if (w.KeyLettersBetween > 0)
        {
            return "G5 key letters lost between the two decoded letters";
        }

        if (w.Inserted)
        {
            return w.Context.CharacterGapMs is not null
                ? "G2 inserted, this read's character gap x 1.53 under the gap"
                : w.Context.Held
                    ? "G3 inserted, held gaps' word-from under the gap"
                    : "G1 inserted, no character gap measured, textbook word-from 4.58 u under the gap";
        }

        return w.RemovedSpace is not null
            ? "G4 deleted, the relabel took out the word gap the path read"
            : "G7 deleted, the path read a letter gap";
    }

    private static string Decision(Wrong w)
    {
        var r = w.Context;

        if (r is null)
        {
            return "no decoded character on one side";
        }

        if (w.Inserted)
        {
            // A settled space carries no span; the path's own gap ending where it settled does.
            var own = w.Space is { } sp
                ? sp.Read.Result.Gaps.Where(g => g.EndHop == sp.EndHop - sp.Read.WindowStartHop).Cast<CwPathGap?>().FirstOrDefault()
                : null;
            var span = (own ?? w.PathGap) is { } pg ? pg.SpanHops * HopMs : double.NaN;

            return Invariant($"path read a word gap of {span:0} ms; relabel kept it ({span / r.RelabelMs:0.00} of its boundary)");
        }

        if (w.RemovedSpace is { } rm)
        {
            return Invariant($"path read a word gap of {rm.SpanHops * HopMs:0} ms; relabel took it out ({rm.SpanHops * HopMs / rm.Read.RelabelMs:0.00} of its boundary)");
        }

        return w.PathGap is { } g
            ? Invariant($"path read a {(g.IsWordGap ? "word" : "letter")} gap of {g.SpanHops * HopMs:0} ms ({g.SpanHops * HopMs / r.WordFromMs:0.00} of its word-from)")
            : "path gap not found in the read";
    }

    private void Line(Wrong w)
    {
        var at = w.After?.Character.At.TotalSeconds ?? w.Before?.Character.At.TotalSeconds ?? double.NaN;
        var before = Marks(w.Before, out var shortBefore);
        var after = Marks(w.After, out var shortAfter);

        _output.WriteLine(
            Invariant($"boundary | {(w.Inserted ? "inserted" : "deleted")} | {w.Recording} | {w.Set} | {at:0.000} s | {Group(w)[..2]} | ")
            + $"key `{w.KeyAround}` | decode `{w.DecodeAround}` | "
            + Invariant($"gap {w.GapMs:0} ms, {w.Units:0.00} u | ")
            + Decision(w) + " | "
            + (w.Context?.Thresholds ?? "-") + " | "
            + $"before {before} | after {after} | key-ups under half a unit {shortBefore + shortAfter} | {Between(w)}");
    }

    private static string Spread(IEnumerable<double> values)
    {
        var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray();

        if (sorted.Length == 0)
        {
            return "none";
        }

        double At(double share) => sorted[(int)Math.Round(share * (sorted.Length - 1))];

        return Invariant($"n {sorted.Length}, min {sorted[0]:0.00}, quartile {At(0.25):0.00}, median {At(0.5):0.00}, quartile {At(0.75):0.00}, max {sorted[^1]:0.00}");
    }

    /// <summary>Every gap between two decoded letters that align to consecutive key characters, split by what the key has there.</summary>
    private static (List<double> Letter, List<double> Word) KeySpaces(Traced t)
    {
        var letter = new List<double>();
        var word = new List<double>();
        var settled = t.Decoded.Settled;

        foreach (var (a, owner, _) in t.Stretches)
        {
            var boundaries = a.KeyBoundaries.ToHashSet();
            var steps = a.Steps.Where(s => s.Decoded is not null).ToList();

            for (var i = 1; i < steps.Count; i++)
            {
                if (steps[i - 1].Key is null || steps[i].Key is null || steps[i].KeyBefore != steps[i - 1].KeyAfter)
                {
                    continue;
                }

                var before = settled[owner[i - 1]];
                var after = settled[owner[i]];
                var units = (after.StartMs - before.EndMs) / after.Read.UnitMs;

                (boundaries.Contains(steps[i].KeyBefore) ? word : letter).Add(units);
            }
        }

        return (letter, word);
    }

    /// <remarks>
    /// Proves 6.3's measurement and trace: MET-WBE per condition and per recording
    /// with inserted and deleted boundaries apart and the key's kind beside each
    /// number; every wrong boundary with its time, the words either side as the
    /// key and the decoder give them, the gap in milliseconds and units, the
    /// thresholds in force and the read that set them, the marks either side and
    /// any key-up under half a unit inside them, the speed and its proof state;
    /// and each recording's letter and word spaces against the key, so whether
    /// they overlap can be read. Asserts nothing.
    /// </remarks>
    [Fact]
    public void EveryWrongBoundaryIsTraced()
    {
        var all = All.Value;
        var wrongs = all.Where(t => t.NotComputable is null).SelectMany(Wrongs).ToList();

        _output.WriteLine("condition | set | condition | key | words | inserted | deleted | wrong | MET-WBE | recordings | CwMetrics.WordBoundaries agrees");

        foreach (var set in new[] { "real", "synthetic" })
        {
            foreach (var group in all.Where(t => t.Set == set).GroupBy(t => t.Condition).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                var parts = group.SelectMany(t => t.Stretches).Select(s => CwMetrics.WordBoundaries(s.Alignment)).ToList();
                int words = parts.Sum(p => p.WordsSent), ins = parts.Sum(p => p.Inserted), del = parts.Sum(p => p.Deleted);
                var mine = wrongs.Where(w => w.Set == set && w.Condition == group.Key).ToList();
                var agrees = mine.Count(w => w.Inserted) == ins && mine.Count(w => !w.Inserted) == del;

                _output.WriteLine(Invariant(
                    $"condition | {set} | {group.Key} | {CwMetrics.KindWord(group.First().Kind)} | {words} | {ins} | {del} | {ins + del} | {(words == 0 ? "no number" : ((ins + del) / (double)words).ToString("0.0000", CultureInfo.InvariantCulture))} | {group.Count()} | {(agrees ? "yes" : "NO")}"));
            }

            var totals = all.Where(t => t.Set == set).SelectMany(t => t.Stretches).Select(s => CwMetrics.WordBoundaries(s.Alignment)).ToList();
            int tw = totals.Sum(p => p.WordsSent), ti = totals.Sum(p => p.Inserted), td = totals.Sum(p => p.Deleted);

            _output.WriteLine(Invariant(
                $"total | {set} | {(set == "real" ? "inferred" : "exact")} | {tw} words | {ti} inserted | {td} deleted | {ti + td} wrong | {(ti + td) / (double)tw:0.0000}"));
        }

        _output.WriteLine("recording | name | set | key | words | inserted | deleted | MET-WBE");

        foreach (var t in all.Where(t => t.NotComputable is null))
        {
            var parts = t.Stretches.Select(s => CwMetrics.WordBoundaries(s.Alignment)).ToList();
            int words = parts.Sum(p => p.WordsSent), ins = parts.Sum(p => p.Inserted), del = parts.Sum(p => p.Deleted);

            if (ins + del > 0)
            {
                _output.WriteLine(Invariant(
                    $"recording | {t.Name} | {t.Set} | {CwMetrics.KindWord(t.Kind)} | {words} | {ins} | {del} | {(ins + del) / (double)words:0.0000}"));
            }
        }

        _output.WriteLine("boundary | kind | recording | set | at | key around | decode around | gap | what the path and the relabel did | thresholds in force | before | after | short key-ups | inside the gap");

        foreach (var w in wrongs)
        {
            Line(w);
        }

        _output.WriteLine("spaces | recording | set | wrong | key letter spaces in units | key word spaces in units | overlap");

        foreach (var t in all.Where(t => t.NotComputable is null))
        {
            var (letter, word) = KeySpaces(t);
            var wrong = wrongs.Count(w => w.Recording == t.Name);
            var overlap = letter.Count > 0 && word.Count > 0 && letter.Max() >= word.Min()
                ? Invariant($"yes: {letter.Count(v => v >= word.Min())} letter spaces at or past the shortest word space {word.Min():0.00} u, {word.Count(v => v <= letter.Max())} word spaces at or under the longest letter space {letter.Max():0.00} u")
                : "no";

            _output.WriteLine($"spaces | {t.Name} | {t.Set} | {wrong} | {Spread(letter)} | {Spread(word)} | {overlap}");
        }

        _output.WriteLine("group | group | real | synthetic | synthetic at 0 dB | recordings | marks' unit over the path's | example");

        foreach (var group in wrongs.GroupBy(Group).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            var example = group.FirstOrDefault(w => w.Set == "real") ?? group.First();

            _output.WriteLine(
                $"group | {group.Key} | {group.Count(w => w.Set == "real")} | {group.Count(w => w.Set == "synthetic" && !w.Recording.Contains("-0db", StringComparison.Ordinal))} | "
                + $"{group.Count(w => w.Recording.Contains("-0db", StringComparison.Ordinal))} | {string.Join(", ", group.Select(w => w.Recording).Distinct())} | "
                + $"{Spread(group.Where(w => w.Context is not null).Select(w => w.Context!.MarksUnitMs / w.Context.UnitMs))} | "
                + $"{example.Recording} at {example.After?.Character.At.TotalSeconds ?? double.NaN:0.000} s key `{example.KeyAround}` decode `{example.DecodeAround}`");
        }
    }
}

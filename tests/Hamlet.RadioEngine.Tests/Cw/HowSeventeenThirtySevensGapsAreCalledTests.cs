using System.Globalization;
using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every gap between marks in 17:37's CQ, from the first `C` to the end, with how
/// long it was, the unit and thresholds in force at the read that decided it, how
/// the decoder called it and how the key calls it (work instruction 444, task 1;
/// PHASE_PLAN.md 2.5; HM-REQ-080, 081, 082). Written beside
/// <see cref="WhereTheSureAddedLettersComeFromTests"/>.
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT THE DECODER.** It asserts only that
/// it found marks to print.</para>
/// <para>**SELF-CONTAINED ON PURPOSE.** It reads nothing the decoder gained after
/// f14b2453, the commit before G1: the stream's held spacing and window are read by
/// reflection, and G1's condition is worked from <see cref="CwUnitEstimator"/>'s own
/// clustering, so the same file runs at HEAD and with the decoder checked out
/// there, and the two printouts line up gap by gap on the audio clock.</para>
/// <para>**THE MARKS ARE THE PRINTER'S, NOT THE PATH'S.** The path records where
/// each letter begins and ends and where it put each space, but not where each
/// element sits inside a letter. The printer cuts the envelope with the estimator's
/// own level (<c>Otsu</c>) and trigger depth (<see cref="CwUnitEstimator.HysteresisDb"/>),
/// and calls a gap an element gap where the path put a letter across it.</para>
/// <para>**THE KEY IS INFERRED** (V-13). Its call on a gap comes from lining the
/// envelope's marks, dit or dah, up with the key's elements; where the two marks
/// either side are not matched to consecutive elements of the key, the printout
/// says the alignment cannot say.</para>
/// </remarks>
public sealed class HowSeventeenThirtySevensGapsAreCalledTests
{
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the gaps are printed.</param>
    public HowSeventeenThirtySevensGapsAreCalledTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>What one read of the stream stood on.</summary>
    private sealed record Read(
        int Index, long WindowStartHop, CwProbabilisticResult Result, bool StructureHeld,
        CwUnitEstimator.CwGapLengths Held, string ThisRead, double? CharacterGap)
    {
        public double UnitMs => Result.WordsPerMinute > 0 ? 1200.0 / Result.WordsPerMinute : double.NaN;

        /// <summary>What the character-gap measurement stood on in this read's window.</summary>
        public string CharacterGapStoodOn { get; init; } = "";

        /// <summary>The three gap lengths the path was given: held, or one, three and seven units.</summary>
        public (double Element, double Character, double Word) Want
            => StructureHeld
                ? (Held.ElementMilliseconds, Held.CharacterMilliseconds, Held.WordMilliseconds)
                : (UnitMs, 3 * UnitMs, 7 * UnitMs);

        public double LetterThresholdMs => Math.Sqrt(Want.Element * Want.Character);

        public double WordThresholdMs => Math.Sqrt(Want.Character * Want.Word);

        /// <summary>Where the stream's relabel takes a path's space out below.</summary>
        public double RelabelMs => CharacterGap is { } gap ? gap * Math.Sqrt(7.0 / 3.0) : WordThresholdMs;

        public string Source => StructureHeld ? "measured gaps, held" : "textbook 1/3/7 on the path's unit";
    }

    private sealed record Settled(CwCharacter Character, int Read);

    private sealed record Mark(int StartHop, int EndHop)
    {
        public double Ms => (EndHop - StartHop) * CwProbabilisticDecoder.HopMilliseconds;
    }

    /// <summary>
    /// What <see cref="CwUnitEstimator.MeasureGaps"/> makes of a window, worked from its
    /// own clustering, and whether G1's condition holds on it.
    /// </summary>
    private static string Classify(double[] window, double unit)
    {
        if (unit <= 0)
        {
            return "no unit";
        }

        var (_, gaps) = CwUnitEstimator.Elements(window, CwProbabilisticDecoder.HopMilliseconds);

        if (gaps.Count < 12)
        {
            return "too few gaps";
        }

        var three = typeof(CwUnitEstimator).GetMethod("ThreeMeansOnLogs", PrivateStatic)!;
        var trough = typeof(CwUnitEstimator).GetMethod("IsTrough", PrivateStatic)!;
        var c = (double[])three.Invoke(null, new object[] { gaps })!;

        if (c[1] / c[0] < 1.5 || c[2] / c[1] < 1.5)
        {
            return "not separated";
        }

        if (!(bool)trough.Invoke(null, new object[] { gaps, c[0], c[1] })!
            || !(bool)trough.Invoke(null, new object[] { gaps, c[1], c[2] })!)
        {
            return "no trough";
        }

        var characterBoundary = Math.Clamp(Math.Sqrt(c[0] * c[1]), 1.3 * unit, 2.6 * unit);
        var character = characterBoundary * characterBoundary / c[0];
        var wordBoundary = Math.Clamp(Math.Sqrt(character * c[2]), 3.5 * unit, 6.5 * unit);
        var word = wordBoundary * wordBoundary / character;
        var centroids = string.Create(CultureInfo.InvariantCulture,
            $"centroids {c[0]:0}/{c[1]:0}/{c[2]:0} ms, clipped {c[0]:0}/{character:0}/{word:0}");

        return character >= word ? $"G1's condition: character past word ({centroids})" : $"measured ({centroids})";
    }

    /// <summary>
    /// What <see cref="CwUnitEstimator.MeasureCharacterGap"/> stood on in a window, worked
    /// from its own clustering: the unit, the two short centroids, the boundary in units
    /// against its 1.3 to 2.6, the trough, and how many gaps sit in the shortest heap.
    /// </summary>
    private static string CharacterGapWhy(double[] window, double unit)
    {
        if (unit <= 0)
        {
            return "no unit";
        }

        var (_, gaps) = CwUnitEstimator.Elements(window, CwProbabilisticDecoder.HopMilliseconds);

        if (gaps.Count < 12)
        {
            return "too few gaps";
        }

        var three = typeof(CwUnitEstimator).GetMethod("ThreeMeansOnLogs", PrivateStatic)!;
        var trough = typeof(CwUnitEstimator).GetMethod("IsTrough", PrivateStatic)!;
        var c = (double[])three.Invoke(null, new object[] { gaps })!;
        var boundary = Math.Sqrt(c[0] * c[1]);
        var shortest = gaps.Count(g => Math.Abs(Math.Log(g / c[0])) < Math.Abs(Math.Log(g / c[1])));
        var shortestGap = gaps.Min();

        return string.Create(CultureInfo.InvariantCulture,
            $"unit {unit:0.0} ms, centroids {c[0]:0}/{c[1]:0} ms, boundary {boundary / unit:0.00}u, "
            + $"trough {((bool)trough.Invoke(null, new object[] { gaps, c[0], c[1] })! ? "yes" : "no")}, "
            + $"shortest heap {shortest} of {gaps.Count} gaps, shortest gap {shortestGap:0} ms/{shortestGap / unit:0.00}u");
    }

    private static (List<Settled> Settled, List<Read> Reads, double[] Envelope) Decode()
    {
        var audio = WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, TheSeventeenThirtySevenCaptureTests.Name + ".wav"));
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var stream = decoder.Stream;
        var type = typeof(CwProbabilisticStream);
        var settled = new List<Settled>();
        var reads = new List<Read>();
        var envelope = new List<double>();

        decoder.CharacterSettled += c => settled.Add(new Settled(c, reads.Count));

        stream.LeadingEdgeChanged += _ =>
        {
            var count = (int)type.GetField("_envelopeCount", Private)!.GetValue(stream)!;
            var window = ((double[])type.GetField("_envelope", Private)!.GetValue(stream)!).Take(count).ToArray();
            var hopsSeen = (long)type.GetField("_hopsSeen", Private)!.GetValue(stream)!;
            var measured = CwUnitEstimator.Measure(window, CwProbabilisticDecoder.HopMilliseconds);
            var characterGap = measured.IsReady
                ? CwUnitEstimator.MeasureCharacterGap(window, CwProbabilisticDecoder.HopMilliseconds, measured.UnitMilliseconds)
                : null;

            reads.Add(new Read(
                reads.Count,
                hopsSeen - count,
                stream.Last,
                (bool)type.GetField("_structureHeld", Private)!.GetValue(stream)!,
                (CwUnitEstimator.CwGapLengths)type.GetField("_heldGaps", Private)!.GetValue(stream)!,
                Classify(window, measured.UnitMilliseconds),
                characterGap)
            {
                CharacterGapStoodOn = CharacterGapWhy(window, measured.UnitMilliseconds),
            });
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            envelope.Add(stream.NewestEnvelope);
        }

        decoder.Flush();

        return (settled, reads, envelope.ToArray());
    }

    private static int EndHop(CwCharacter c) => (int)Math.Round(c.At.TotalMilliseconds / CwProbabilisticDecoder.HopMilliseconds);

    private static int StartHop(CwCharacter c) => EndHop(c) - c.SpanHops;

    /// <summary>The envelope's marks, cut with the estimator's own level and trigger depth.</summary>
    private static List<Mark> Marks(double[] envelope, int from)
    {
        var db = envelope.Skip(from).Select(e => 20 * Math.Log10(Math.Max(e, 1e-12))).ToArray();
        var cut = (double)typeof(CwUnitEstimator).GetMethod("Otsu", PrivateStatic)!.Invoke(null, new object[] { db })!;
        var on = cut + CwUnitEstimator.HysteresisDb;
        var off = cut - CwUnitEstimator.HysteresisDb;
        var marks = new List<Mark>();
        var down = db[0] > on;
        var start = 0;

        for (var i = 1; i < db.Length; i++)
        {
            if (!(down ? db[i] < off : db[i] > on))
            {
                continue;
            }

            if (down && i - start >= 2)
            {
                marks.Add(new Mark(from + start, from + i));
            }

            down = !down;
            start = i;
        }

        return marks;
    }

    /// <summary>The key's elements, each a dit or a dah with the gap that follows it.</summary>
    private static List<(bool Dah, string After)> KeyElements()
    {
        var byLetter = MorseAlphabet.All.GroupBy(p => p.Value).ToDictionary(g => g.Key, g => g.First().Key);
        var words = TheSeventeenThirtySevenCaptureTests.InferredKey.Split(' ');
        var elements = new List<(bool, string)>();

        for (var w = 0; w < words.Length; w++)
        {
            for (var l = 0; l < words[w].Length; l++)
            {
                var pattern = byLetter[words[w][l].ToString()];

                for (var e = 0; e < pattern.Length; e++)
                {
                    var after = e < pattern.Length - 1 ? "element"
                        : l < words[w].Length - 1 ? "letter"
                        : w < words.Length - 1 ? "word"
                        : "end";

                    elements.Add((pattern[e] == '-', after));
                }
            }
        }

        return elements;
    }

    /// <summary>Each mark's element in the key, or -1: an edit alignment on dit and dah.</summary>
    private static int[] Align(IReadOnlyList<bool> marks, IReadOnlyList<(bool Dah, string After)> key)
    {
        var cost = new int[marks.Count + 1, key.Count + 1];

        for (var i = 0; i <= marks.Count; i++)
        {
            cost[i, 0] = i;
        }

        for (var j = 0; j <= key.Count; j++)
        {
            cost[0, j] = j;
        }

        for (var i = 1; i <= marks.Count; i++)
        {
            for (var j = 1; j <= key.Count; j++)
            {
                cost[i, j] = Math.Min(
                    cost[i - 1, j - 1] + (marks[i - 1] == key[j - 1].Dah ? 0 : 1),
                    Math.Min(cost[i - 1, j] + 1, cost[i, j - 1] + 1));
            }
        }

        var matched = Enumerable.Repeat(-1, marks.Count).ToArray();
        var (a, b) = (marks.Count, key.Count);

        while (a > 0 && b > 0)
        {
            if (cost[a, b] == cost[a - 1, b - 1] + (marks[a - 1] == key[b - 1].Dah ? 0 : 1))
            {
                if (marks[a - 1] == key[b - 1].Dah)
                {
                    matched[a - 1] = b - 1;
                }

                a--;
                b--;
            }
            else if (cost[a, b] == cost[a - 1, b] + 1)
            {
                a--;
            }
            else
            {
                b--;
            }
        }

        return matched;
    }

    private static string Ms(double ms, double unit)
        => string.Create(CultureInfo.InvariantCulture, $"{ms:0} ms/{ms / unit:0.00}u");

    /// <remarks>
    /// Proves 2.5's measurement: every gap between two marks from 17:37's first `C`
    /// to the end, with its time, its length in milliseconds and units, the unit in
    /// force and where it came from, the letter and word thresholds in force at the
    /// read that decided it, the relabel's boundary, the decoder's call and the
    /// path's, the key's call where the alignment can say, and the marks either
    /// side. Then every read over the stretch: what the window's gaps measured and
    /// whether G1's condition held. Asserts only that marks were found.
    /// </remarks>
    [Fact]
    public void EveryGapInTheCqAndHowItWasCalled()
    {
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var (settled, reads, envelope) = Decode();
        var text = string.Concat(settled.Select(s => s.Character.IsWordGap ? " " : s.Character.Text));
        var cq = text.IndexOf("CQ CQ", StringComparison.Ordinal);

        Assert.True(cq >= 0, "no CQ CQ was read");

        // The index in text is the index in settled: one character each.
        var region = settled.Skip(cq).ToList();
        var letters = region.Where(s => !s.Character.IsWordGap).ToList();
        var from = Math.Max(0, StartHop(letters[0].Character) - 4);
        var marks = Marks(envelope, from).Where(m => m.EndHop > StartHop(letters[0].Character)).ToList();

        Assert.NotEmpty(marks);

        // The reads whose window reaches the stretch, and the middle of their units.
        var units = reads.Where(r => r.UnitMs > 0 && r.WindowStartHop + (12000 / hopMs) >= from).Select(r => r.UnitMs).OrderBy(u => u).ToList();
        var medianUnit = units[units.Count / 2];

        var key = KeyElements();
        var matched = Align(marks.Select(m => m.Ms >= 2 * medianUnit).ToList(), key);

        _output.WriteLine($"text | {TheSeventeenThirtySevenCaptureTests.Name} | `{text[cq..].Trim()}` | key `{TheSeventeenThirtySevenCaptureTests.InferredKey}`, inferred");
        _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"marks | {marks.Count} envelope marks from {from * hopMs / 1000.0:0.000} s, dit or dah split at 2 units of the median path unit {medianUnit:0.0} ms | key elements {key.Count} | matched {matched.Count(m => m >= 0)}"));
        _output.WriteLine(
            "gap | at s | length | read | unit ms | unit from | element/letter/word wanted ms | letter threshold | word threshold | relabel boundary | "
            + "this read's window | path call | decoder call | key call | agrees | mark before | mark after");

        var calls = new List<string>();

        for (var i = 0; i + 1 < marks.Count; i++)
        {
            var gapStart = marks[i].EndHop;
            var gapEnd = marks[i + 1].StartHop;
            var middle = (gapStart + gapEnd) / 2.0;
            var gapMs = (gapEnd - gapStart) * hopMs;

            // The decoder's call: inside a letter's span, or between two letters with or without a space.
            var inside = letters.FirstOrDefault(s => StartHop(s.Character) < middle && EndHop(s.Character) > middle);
            var next = letters.FirstOrDefault(s => StartHop(s.Character) >= middle - 1);
            var before = letters.LastOrDefault(s => EndHop(s.Character) <= middle + 1);
            var space = region.FirstOrDefault(s => s.Character.IsWordGap
                && before is not null && next is not null
                && EndHop(s.Character) > EndHop(before.Character) && EndHop(s.Character) <= StartHop(next.Character) + 1);

            string decoderCall;
            Settled? decider;

            if (inside is not null)
            {
                decoderCall = $"element (inside `{inside.Character.Text}`)";
                decider = inside;
            }
            else if (before is null || next is null)
            {
                decoderCall = "no letter on one side";
                decider = next ?? before;
            }
            else
            {
                decoderCall = space is not null ? "word" : "letter";
                decider = space ?? next;
            }

            var read = decider is null ? null : reads.ElementAtOrDefault(decider.Read);

            // The path's own call at that read, before the relabel.
            var pathCall = "no read";

            if (read is not null)
            {
                var local = middle - read.WindowStartHop;
                var gap = read.Result.Gaps.FirstOrDefault(g => g.StartHop - 1 <= local && g.EndHop + 1 >= local);
                var character = read.Result.Characters.FirstOrDefault(c => c.Pattern.Length > 0 && c.EndHop - c.SpanHops < local && c.EndHop > local);

                pathCall = character.Pattern is { Length: > 0 } ? "element"
                    : gap.EndHop > 0 ? (gap.IsWordGap ? "word" : "letter")
                    : "outside the path's letters";
            }

            var keyCall = matched[i] >= 0 && matched[i + 1] == matched[i] + 1
                ? key[matched[i]].After
                : "the alignment cannot say";
            var decoderKind = decoderCall.Split(' ')[0];
            var agrees = keyCall == "the alignment cannot say" ? "-" : keyCall == decoderKind ? "yes" : "NO";
            var unit = read?.UnitMs ?? medianUnit;

            calls.Add(decoderKind);

            var standing = read is null
                ? "- | - | - | - | - | - | - | - | "
                : FormattableString.Invariant($"{read.Index} | {read.UnitMs:0.0} | {read.Source} | {read.Want.Element:0}/{read.Want.Character:0}/{read.Want.Word:0} | ")
                  + $"{Ms(read.LetterThresholdMs, unit)} | {Ms(read.WordThresholdMs, unit)} | "
                  + $"{Ms(read.RelabelMs, unit)} ({(read.CharacterGap is null ? "path's word threshold" : "character gap x sqrt(7/3)")}) | {read.ThisRead} | ";

            _output.WriteLine(
                FormattableString.Invariant($"gap | {gapStart * hopMs / 1000.0:0.000} | ") + $"{Ms(gapMs, unit)} | "
                + standing
                + $"{pathCall} | {decoderCall} | {keyCall} | {agrees} | "
                + $"{Ms(marks[i].Ms, unit)} | {Ms(marks[i + 1].Ms, unit)}");
        }

        _output.WriteLine(
            $"total | gaps {calls.Count} | decoder element {calls.Count(c => c == "element")}, letter {calls.Count(c => c == "letter")}, "
            + $"word {calls.Count(c => c == "word")} | key calls where the alignment can say: see agrees");

        _output.WriteLine("read | index | window start s | wpm | unit ms | structure | held element/character/word ms | this read's window | character gap ms | character gap stood on | text");

        foreach (var r in reads.Where(r => (r.WindowStartHop * hopMs) + 12000 >= from * hopMs))
        {
            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"read | {r.Index} | {r.WindowStartHop * hopMs / 1000.0:0.000} | {r.Result.WordsPerMinute:0.0} | {r.UnitMs:0.0} | {r.Source} | "
                + $"{r.Held.ElementMilliseconds:0}/{r.Held.CharacterMilliseconds:0}/{r.Held.WordMilliseconds:0} | {r.ThisRead} | "
                + $"{(r.CharacterGap is { } g ? g.ToString("0", CultureInfo.InvariantCulture) : "none")} | {r.CharacterGapStoodOn} | `{r.Result.Text}`"));
        }
    }
}

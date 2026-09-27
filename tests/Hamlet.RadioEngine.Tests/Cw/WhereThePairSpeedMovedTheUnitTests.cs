using System.Globalization;
using System.Reflection;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every read of our stream on the recordings unit 459's refused pair speed moved,
/// with the dot-dash pairs that rule rested on, the unit before and after, the unit
/// the window's marks imply and the unit the key's own letters imply (work
/// instruction 460, task 2; PHASE_PLAN.md 2.5 and 9.4; HM-REQ-010 and HM-REQ-129).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT THE DECODER**, and it needs no
/// change under `src`: the pair rule is carried here as `6a0b65a1` wrote it, and
/// the same printout taken with that commit's diff in the working tree shows the
/// stream's own speed after the move, so the two printouts are joined read by
/// read. It asserts only that it printed every recording it names.</para>
/// <para>**THE KEY'S UNIT** at a read is the median, over the letters ours settled
/// sure and right inside that read's window, of each letter's own span, first
/// mark to last, over the units its pattern holds: a dit and an inner gap one, a
/// dah three. A letter the key confirms was read element for element, so its span
/// is the sender's timing whatever speed the path was given. **EVERY KEY HERE IS
/// INFERRED** (V-13).</para>
/// </remarks>
public sealed class WhereThePairSpeedMovedTheUnitTests
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <summary>The recordings work instruction 460 names: three the pair speed helped, four it broke.</summary>
    internal static readonly (string Name, string Moved)[] Named =
    {
        ("unadjudicated/cw-2026-08-18-003758", "helped"),
        ("unadjudicated/cw-2026-08-22-031838", "helped"),
        ("unadjudicated/cw-2026-08-22-032113", "helped"),
        ("unadjudicated/cw-2026-08-22-031948", "broke 110,"),
        ("cw-2026-08-18-004507", "broke EACH"),
        ("unadjudicated/cw-2026-08-22-031905", "broke PREDICTED"),
        ("unadjudicated/cw-2026-08-22-032050", "broke word boundaries"),
    };

    // 6a0b65a1's constants: MarksOverruleRatio in CwProbabilisticStream, and
    // update_tracking's bounds as CwUnitEstimator.PairUnit took them (cw.cxx:526-527).
    private const double MoveRatio = 1.25;
    private const int PairsTracked = 16;
    private const double ShortestDit = 1200.0 / 200;
    private const double LongestDah = 3 * 1200.0 / 5;

    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the trace is printed.</param>
    public WhereThePairSpeedMovedTheUnitTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One read of the stream: its window and what the read set.</summary>
    internal sealed record Read(
        int Index, double[] Window, long WindowStartHop, double Wpm, double ToneHz, IReadOnlyList<CwProbabilisticCharacter> Characters);

    /// <summary>One dot-dash pair the rule took, in the order the marks came.</summary>
    internal readonly record struct Pair(int MarkIndex, double DitMs, double DahMs, double AverageAfterMs);

    /// <summary>What the pair rule makes of one window's marks.</summary>
    internal sealed record PairTrace(IReadOnlyList<Pair> Pairs, IReadOnlyList<double> Spikes, double UnitMs);

    /// <summary>
    /// `6a0b65a1`'s CwUnitEstimator.PairUnit, line for line, keeping every pair it
    /// took and every mark it passed over as a spike.
    /// </summary>
    /// <param name="marks">Mark lengths in milliseconds, in order.</param>
    /// <returns>The pairs, the spikes and the unit, NaN where no pair was found.</returns>
    internal static PairTrace Pairs(IReadOnlyList<double> marks)
    {
        var window = new Queue<double>(PairsTracked);
        var sum = 0.0;
        var last = 0.0;
        var pairs = new List<Pair>();
        var spikes = new List<double>();

        for (var i = 0; i < marks.Count; i++)
        {
            var mark = marks[i];

            if (window.Count > 0 && mark < sum / window.Count / 4)
            {
                spikes.Add(mark);
                continue;
            }

            if (last > 0)
            {
                var (dit, dah) = mark > 2 * last && mark < 4 * last ? (last, mark)
                    : last > 2 * mark && last < 4 * mark ? (mark, last)
                    : (0.0, 0.0);

                if (dit >= ShortestDit && dah <= LongestDah)
                {
                    var twoDits = (dit + dah) / 2;

                    if (window.Count == 0)
                    {
                        for (var n = 0; n < PairsTracked; n++)
                        {
                            window.Enqueue(twoDits);
                        }

                        sum = PairsTracked * twoDits;
                    }
                    else
                    {
                        sum += twoDits - window.Dequeue();
                        window.Enqueue(twoDits);
                    }

                    pairs.Add(new Pair(i, dit, dah, sum / window.Count / 2));
                }
            }

            last = mark;
        }

        return new PairTrace(pairs, spikes, window.Count == 0 ? double.NaN : sum / window.Count / 2);
    }

    /// <summary>Ours driven hop by hop as the metrics drive it, every read kept, and the read each letter settled at.</summary>
    internal static (List<CwCharacter> Settled, List<int> SettledAtRead, List<Read> Reads, int SampleRate) Drive(string path)
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var t = typeof(CwProbabilisticStream);
        var envelopeField = t.GetField("_envelope", flags)!;
        var countField = t.GetField("_envelopeCount", flags)!;
        var hopsField = t.GetField("_hopsSeen", flags)!;
        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var settled = new List<CwCharacter>();
        var settledAt = new List<int>();
        var reads = new List<Read>();
        CwProbabilisticResult lastSeen = default;

        // A read puts a new character list into Last, so a Last unequal to the one
        // seen is a new read.
        void Capture()
        {
            var s = decoder.Stream;

            if (s.Last.Characters is null || s.Last.Equals(lastSeen))
            {
                return;
            }

            lastSeen = s.Last;
            var count = (int)countField.GetValue(s)!;
            var window = ((double[])envelopeField.GetValue(s)!).Take(count).ToArray();

            reads.Add(new Read(reads.Count, window, (long)hopsField.GetValue(s)! - count, s.Last.WordsPerMinute, s.ToneHz, s.Last.Characters));
        }

        decoder.CharacterSettled += c =>
        {
            Capture();
            settled.Add(c);
            settledAt.Add(reads.Count - 1);
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            Capture();
        }

        decoder.Flush();
        Capture();

        return (settled, settledAt, reads, audio.SampleRate);
    }

    /// <summary>A pattern's length in units, first mark to last: a dit and an inner gap one, a dah three.</summary>
    internal static int Units(string pattern)
        => pattern.Sum(e => e == '-' ? 3 : 1) + Math.Max(0, pattern.Length - 1);

    /// <remarks>
    /// Proves nothing about the decoder; prints work instruction 460 task 2's
    /// trace (HM-REQ-010, HM-REQ-129): for every read of the seven named
    /// recordings, the path's speed, the window's measured and mark units, and the
    /// pairs `6a0b65a1`'s rule took with its unit and whether it would move the
    /// read; for every settled letter, the read it settled at, its class, its key
    /// letter and its own span. Asserts only that every named recording printed.
    /// </remarks>
    [Fact]
    public void EveryReadOnTheNamedRecordings()
    {
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var printed = 0;

        _output.WriteLine("read | recording | read | window from s | window to s | path wpm | path unit ms | measured wpm | marks unit ms | marks | spikes | pairs | pair unit ms | pair wpm | would move | first pair dit/dah ms | pairs dit/dah ms, average unit after");
        _output.WriteLine("letter | recording | read | at s | text | pattern | class | stretch | key | verdict | span ms | units | unit ms");

        foreach (var (name, moved) in Named)
        {
            var keyed = WhatTheStrayLettersRestOnTests.KeyedRecordings.Single(k => k.Name == name);
            var (settled, settledAt, reads, rate) = Drive(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
            var scores = keyed.Score(CwReading.Of(settled));
            var measured = TheRequirementsAreMeasuredTests.Measure(name, keyed.Set, TheRequirementsAreMeasuredTests.RealCondition(name),
                CwKeyKind.Inferred, settled, scores);

            _output.WriteLine(string.Create(Invariant, $"recording | {name} | {moved} | {rate} Hz | {reads.Count} reads | {settled.Count} settled | {scores.Count} stretches"));

            foreach (var r in reads)
            {
                var elements = CwUnitEstimator.Elements(r.Window, hopMs);
                var marks = elements.Marks;
                var m = CwUnitEstimator.Measure(r.Window, hopMs);
                var marksUnit = CwUnitEstimator.MarkUnit(marks);
                var p = Pairs(marks);
                var pairWpm = 1200.0 / p.UnitMs;
                var moves = r.Wpm > 0 && p.UnitMs * r.Wpm / 1200.0 > MoveRatio
                    && pairWpm >= CwProbabilisticDecoder.SlowestWpm && pairWpm <= CwProbabilisticDecoder.FastestWpm;
                var from = r.WindowStartHop * hopMs / 1000.0;
                var to = (r.WindowStartHop + r.Window.Length) * hopMs / 1000.0;
                var samples = rate / 1000.0;

                var head = string.Create(Invariant,
                    $"read | {name} | {r.Index} | {from:0.000} | {to:0.000} | {r.Wpm:0.000} | {(r.Wpm > 0 ? 1200.0 / r.Wpm : double.NaN):0.0} | ");
                var units = string.Create(Invariant,
                    $"{(m.IsReady ? m.WordsPerMinute : double.NaN):0.0} | {marksUnit:0.0} | {marks.Count} | {p.Spikes.Count} | {p.Pairs.Count} | {p.UnitMs:0.0} | {pairWpm:0.000} | {(moves ? "yes" : "no")} | ");
                var first = p.Pairs.Count > 0 ? string.Create(Invariant, $"{p.Pairs[0].DitMs:0}/{p.Pairs[0].DahMs:0}") : "-";
                var all = string.Join(" ", p.Pairs.Select(q => string.Create(Invariant,
                    $"{q.DitMs:0}/{q.DahMs:0}ms={q.DitMs * samples:0}/{q.DahMs * samples:0}smp->{q.AverageAfterMs:0.0}")));

                _output.WriteLine(head + units + first + " | " + all);

                if (moves)
                {
                    _output.WriteLine(string.Create(Invariant,
                        $"marks | {name} | {r.Index} | {string.Join(" ", marks.Select(x => x.ToString("0", Invariant)))} | spikes skipped {string.Join(" ", p.Spikes.Select(x => x.ToString("0", Invariant)))}"));
                }
            }

            // Every settled letter, its class and its key letter where a stretch covers it.
            var verdicts = new Dictionary<CwCharacter, (int Stretch, string Key, string Verdict)>(ReferenceEqualityComparer.Instance);

            for (var s = 0; measured.NotComputable is null && s < measured.Stretches.Count; s++)
            {
                var covered = TheRequirementsAreMeasuredTests.Covered(settled, measured.Scores[s]).Where(c => !c.IsWordGap).ToList();
                var d = 0;

                foreach (var step in measured.Stretches[s].Steps)
                {
                    if (step.Decoded is not { } decoded)
                    {
                        continue;
                    }

                    var c = covered[d++];
                    var verdict = step.Key is null ? "added" : decoded.Class == CwSymbolClass.Placeholder ? "placeholder"
                        : decoded.Text == step.Key ? "right" : "wrong";

                    verdicts[c] = (s, step.Key ?? "-", verdict);
                }
            }

            for (var i = 0; i < settled.Count; i++)
            {
                var c = settled[i];

                if (c.IsWordGap)
                {
                    _output.WriteLine(string.Create(Invariant, $"letter | {name} | {settledAt[i]} | {c.At.TotalSeconds:0.000} | _ | | gap | | | | | |"));
                    continue;
                }

                var cls = CwSymbol.Of(c).Class;
                var (stretch, key, v) = verdicts.TryGetValue(c, out var found) ? found : (-1, "-", "-");
                var span = "";
                var units = "";
                var unit = "";

                // The letter's own span, from the read it settled at.
                if (c.Pattern.Length > 0 && settledAt[i] >= 0)
                {
                    var r = reads[settledAt[i]];
                    var endLocal = (int)(Math.Round(c.At.TotalMilliseconds / hopMs) - r.WindowStartHop);
                    var own = r.Characters.Where(x => x.Pattern == c.Pattern && Math.Abs(x.EndHop - endLocal) <= 1).ToList();

                    if (own.Count == 1 && own[0].SpanHops > 0)
                    {
                        span = (own[0].SpanHops * hopMs).ToString("0", Invariant);
                        units = Units(c.Pattern).ToString(Invariant);
                        unit = (own[0].SpanHops * hopMs / Units(c.Pattern)).ToString("0.0", Invariant);
                    }
                }

                _output.WriteLine(string.Create(Invariant,
                    $"letter | {name} | {settledAt[i]} | {c.At.TotalSeconds:0.000} | {c.Text} | {c.Pattern} | {cls} | {(stretch < 0 ? "-" : stretch.ToString(Invariant))} | {key} | {v} | {span} | {units} | {unit}"));
            }

            printed++;
        }

        Assert.Equal(Named.Length, printed);
    }
}

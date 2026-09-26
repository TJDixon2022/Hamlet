using System.Globalization;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every character the decoder emitted sure where the key says a different one
/// was sent, traced and grouped by what they share (work instruction 440, task 1;
/// PHASE_PLAN.md 2.1; HM-REQ-010).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING ABOUT THE DECODER.** It asserts only that
/// it found the same sure-wrong count <see cref="TheRequirementsAreMeasuredTests"/>
/// counts, so the trace and the metric are over the same characters.</para>
/// <para>**EVERY KEY HERE IS INFERRED** (R61). A character against it is wrong by
/// the key, which is an indication and not proof (V-13). A wrong character whose
/// key file says its own reading is doubtful is counted apart and left out of the
/// groups; no scored stretch in the tree says that of itself today.</para>
/// <para>**THE MARKS ARE THE ENVELOPE'S, NOT THE PATH'S.** The decoder's path does
/// not expose the segments it chose, so each character's marks and gaps are read
/// by <see cref="CwUnitEstimator.Elements"/> over the character's own span with one
/// unit of the read's speed either side. That is an independent segmentation of
/// the same envelope the path chose its marks on, printed beside the path's own
/// pattern so a reader sees where the two differ.</para>
/// <para>**THE GROUPS ARE BY HOW THE EMITTED PATTERN RELATES TO THE SENT ONE**,
/// because the pattern is what the decoder decided and the letter only follows
/// from it. A part of the sent letter (split), the sent letter and part of a
/// neighbor (merged), the same elements with one of the wrong kind, one element
/// lost, one gained, or none of these.</para>
/// </remarks>
public sealed class WhereTheSureWrongLettersComeFromTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhereTheSureWrongLettersComeFromTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>One sure character the key says is wrong, with everything task 1 asks for.</summary>
    /// <param name="Name">The recording.</param>
    /// <param name="Character">What settled.</param>
    /// <param name="Sent">What the key says was sent there.</param>
    /// <param name="SentPattern">That character's pattern, or "?" where the alphabet has none.</param>
    /// <param name="Group">How the emitted pattern relates to the sent one.</param>
    /// <param name="SpanBefore">The span of the named character before it, or NaN.</param>
    /// <param name="SpanAfter">The span of the named character after it, or NaN.</param>
    /// <param name="ToneHz">The median pitch its hops were mixed at.</param>
    /// <param name="PitchHz">The recording's sender pitch, the median over its named characters.</param>
    /// <param name="Marks">The envelope's marks over its span, in milliseconds.</param>
    /// <param name="Gaps">The envelope's gaps over its span, in milliseconds.</param>
    /// <param name="InsertedSpace">Whether the decoder put a word boundary beside it where the key has none.</param>
    /// <param name="Held">Whether the stream held this sender's own gaps when it settled.</param>
    /// <param name="HeldGaps">The gaps it held, meaningful only when held.</param>
    /// <param name="Estimator">What the estimator read from the window it settled in.</param>
    /// <param name="WindowMarksUnit">The unit that window's marks alone imply, or NaN.</param>
    internal sealed record Wrong(
        string Name, CwCharacter Character, string Sent, string SentPattern, string Group,
        double SpanBefore, double SpanAfter, double ToneHz, double PitchHz,
        IReadOnlyList<double> Marks, IReadOnlyList<double> Gaps, bool InsertedSpace,
        bool Held, CwUnitEstimator.CwGapLengths HeldGaps,
        CwUnitReading Estimator, double WindowMarksUnit)
    {
        /// <summary>One unit at the read's speed, in milliseconds.</summary>
        public double UnitMs => Character.WordsPerMinute > 0 ? 1200.0 / Character.WordsPerMinute : double.NaN;

        /// <summary>Where the path's speed came from: the estimator when it read one inside the grid's range, the grid otherwise.</summary>
        public string Source => Estimator.IsReady
            && Estimator.WordsPerMinute >= CwProbabilisticDecoder.SlowestWpm
            && Estimator.WordsPerMinute <= CwProbabilisticDecoder.FastestWpm
                ? "estimator"
                : "grid";

        /// <summary>The unit this character's own marks imply, or NaN.</summary>
        public double LocalMarksUnit => MarksUnit(Marks, Character.Pattern);
    }

    private static readonly IReadOnlyDictionary<string, string> PatternOf = MorseAlphabet.All
        .GroupBy(p => p.Value, StringComparer.Ordinal)
        .ToDictionary(g => g.Key, g => g.First().Key, StringComparer.Ordinal);

    /// <summary>How an emitted pattern relates to the one that was sent.</summary>
    /// <param name="sent">The sent pattern.</param>
    /// <param name="emitted">The emitted pattern.</param>
    /// <returns>The group's name.</returns>
    internal static string Relate(string sent, string emitted)
    {
        if (sent == "?")
        {
            return "sent character has no pattern in the alphabet";
        }

        if (emitted.Length < sent.Length && (sent.StartsWith(emitted, StringComparison.Ordinal) || sent.EndsWith(emitted, StringComparison.Ordinal)))
        {
            return "split: a part of the sent letter";
        }

        if (emitted.Length > sent.Length && (emitted.StartsWith(sent, StringComparison.Ordinal) || emitted.EndsWith(sent, StringComparison.Ordinal)))
        {
            return "merged: the sent letter and part of a neighbor";
        }

        if (emitted.Length == sent.Length)
        {
            var differ = sent.Zip(emitted).Count(p => p.First != p.Second);

            return differ == 1 ? "one element of the wrong kind" : "same length, several elements differ";
        }

        bool OneRemoved(string longer, string shorter)
            => Enumerable.Range(0, longer.Length).Any(i => longer.Remove(i, 1) == shorter);

        if (emitted.Length == sent.Length - 1 && OneRemoved(sent, emitted))
        {
            return "one element lost inside the letter";
        }

        if (emitted.Length == sent.Length + 1 && OneRemoved(emitted, sent))
        {
            return "one element gained inside the letter";
        }

        return "none of these";
    }

    /// <summary>A decode with the per-hop pitch and envelope and, for each settled character, the gap reading in force when it settled.</summary>
    /// <param name="Settled">What settled.</param>
    /// <param name="Tone">The mix pitch after each hop.</param>
    /// <param name="Envelope">The envelope magnitude of each hop.</param>
    /// <param name="Reading">Per settled character: whether the stream held this sender's own gaps, and the gaps it held.</param>
    /// <param name="Speed">Per settled character: what the estimator read from the window it settled in, and the unit that window's marks alone imply.</param>
    internal sealed record Decoded(
        IReadOnlyList<CwCharacter> Settled, double[] Tone, double[] Envelope,
        IReadOnlyList<(bool Held, CwUnitEstimator.CwGapLengths Gaps)> Reading,
        IReadOnlyList<(CwUnitReading Estimator, double WindowMarksUnit)> Speed);

    /// <summary>
    /// The unit a list of marks implies on its own, dits and dahs taken apart on
    /// the logarithm (work instruction 441, task 2).
    /// </summary>
    /// <param name="marks">Mark lengths in milliseconds.</param>
    /// <param name="pattern">The pattern the path read over them, used only when every mark is one kind.</param>
    /// <returns>The unit in milliseconds, or NaN where the marks cannot say.</returns>
    /// <remarks>
    /// Where the longest mark is at least twice the shortest, both kinds are
    /// there: split at the geometric mean, and the unit is the geometric mean of
    /// the dits' median and a third of the dahs' median. Where they are all one
    /// kind the marks cannot say which, and the path's own pattern is asked only
    /// if it is all one kind too. The envelope's marks read long by the cut's
    /// skirt, so this unit is long by that much.
    /// </remarks>
    internal static double MarksUnit(IReadOnlyList<double> marks, string? pattern)
    {
        if (marks.Count == 0)
        {
            return double.NaN;
        }

        double MedianOf(IEnumerable<double> v)
        {
            var s = v.OrderBy(x => x).ToArray();

            return s[s.Length / 2];
        }

        if (marks.Max() >= 2 * marks.Min())
        {
            return CwUnitEstimator.MarkUnit(marks);
        }

        return pattern switch
        {
            { Length: > 0 } p when p.All(e => e == '.') => MedianOf(marks),
            { Length: > 0 } p when p.All(e => e == '-') => MedianOf(marks) / 3,
            _ => double.NaN,
        };
    }

    /// <summary>Settles a recording exactly as the floors do, reading the stream's state as each character settles and changing nothing.</summary>
    /// <param name="name">The recording, under the captures folder.</param>
    /// <returns>The decode.</returns>
    internal static Decoded Decode(string name)
    {
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var heldField = typeof(CwProbabilisticStream).GetField("_structureHeld", flags)!;
        var gapsField = typeof(CwProbabilisticStream).GetField("_heldGaps", flags)!;
        var windowField = typeof(CwProbabilisticStream).GetField("_envelope", flags)!;
        var countField = typeof(CwProbabilisticStream).GetField("_envelopeCount", flags)!;
        var audio = Hamlet.RadioEngine.Audio.WavAudio.Read(Path.Combine(CapturedSignalTests.Folder, name + ".wav"));
        var decoder = new CwDecoder(audio.SampleRate, 600);
        var settled = new List<CwCharacter>();
        var reading = new List<(bool, CwUnitEstimator.CwGapLengths)>();
        var speed = new List<(CwUnitReading, double)>();
        var tone = new List<double>();
        var envelope = new List<double>();

        decoder.CharacterSettled += c =>
        {
            settled.Add(c);
            reading.Add(((bool)heldField.GetValue(decoder.Stream)!, (CwUnitEstimator.CwGapLengths)gapsField.GetValue(decoder.Stream)!));

            // The window this character settled from, exactly as the stream read it.
            var window = ((double[])windowField.GetValue(decoder.Stream)!).Take((int)countField.GetValue(decoder.Stream)!).ToList();
            var hopMs = CwProbabilisticDecoder.HopMilliseconds;

            speed.Add((CwUnitEstimator.Measure(window, hopMs), MarksUnit(CwUnitEstimator.Elements(window, hopMs).Marks, null)));
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new Hamlet.RadioEngine.Audio.AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            tone.Add(decoder.Stream.ToneHz);
            envelope.Add(decoder.Stream.NewestEnvelope);
        }

        decoder.Flush();

        return new Decoded(settled, tone.ToArray(), envelope.ToArray(), reading, speed);
    }

    /// <summary>Every sure-wrong character on the real keyed recordings.</summary>
    /// <returns>The characters, recording by recording.</returns>
    internal static IReadOnlyList<Wrong> Trace() => Trace(out _);

    /// <summary>Every sure-wrong character on the real keyed recordings, and every sure-right one beside them.</summary>
    /// <param name="rights">The sure characters the key has in place, traced the same way, group `right`.</param>
    /// <returns>The wrong characters, recording by recording.</returns>
    internal static IReadOnlyList<Wrong> Trace(out IReadOnlyList<Wrong> rights)
    {
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var wrongs = new List<Wrong>();
        var right = new List<Wrong>();

        rights = right;

        foreach (var keyed in WhatTheStrayLettersRestOnTests.KeyedRecordings)
        {
            var heard = Decode(keyed.Name);
            var settled = heard.Settled;
            var scores = keyed.Score(CwReading.Of(settled));

            int EndHop(CwCharacter c) => (int)Math.Round(c.At.TotalMilliseconds / hopMs);
            int StartHop(CwCharacter c) => EndHop(c) - c.SpanHops;

            double Median(IEnumerable<double> values)
            {
                var sorted = values.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToArray();

                return sorted.Length == 0 ? double.NaN : sorted[sorted.Length / 2];
            }

            double ToneOver(CwCharacter c)
            {
                var from = Math.Max(0, StartHop(c));
                var to = Math.Min(heard.Tone.Length, Math.Max(from + 1, EndHop(c)));

                return to > from ? Median(heard.Tone.Skip(from).Take(to - from)) : double.NaN;
            }

            var named = settled.Where(WhatTheStrayLettersRestOnTests.IsNamed).ToList();
            var pitch = Median(named.Select(ToneOver));

            foreach (var score in scores)
            {
                var covered = TheRequirementsAreMeasuredTests.Covered(settled, score);
                var characters = covered.Where(c => !c.IsWordGap).ToList();
                var alignment = CwMetrics.Align(CwMetrics.Symbols(covered), score.Key, CwKeyKind.Inferred);
                var keyBoundaries = alignment.KeyBoundaries.ToHashSet();
                var d = 0;

                for (var s = 0; s < alignment.Steps.Count; s++)
                {
                    var step = alignment.Steps[s];

                    if (step.Decoded is null)
                    {
                        continue;
                    }

                    var c = characters[d++];

                    if (step.Decoded.Value.Class != CwSymbolClass.Sure || step.Key is null)
                    {
                        continue;
                    }

                    var isRight = string.Equals(step.Key, step.Decoded.Value.Text, StringComparison.Ordinal);
                    var at = settled.ToList().IndexOf(c);
                    var before = settled.Take(at).LastOrDefault(WhatTheStrayLettersRestOnTests.IsNamed);
                    var after = settled.Skip(at + 1).FirstOrDefault(WhatTheStrayLettersRestOnTests.IsNamed);

                    // A boundary the decoder put beside it: before it, or before the next decoded character.
                    var next = alignment.Steps.Skip(s + 1).FirstOrDefault(x => x.Decoded is not null);
                    var inserted = (step.GapBefore && !keyBoundaries.Contains(step.KeyBefore))
                        || (next.Decoded is not null && next.GapBefore && !keyBoundaries.Contains(next.KeyBefore));

                    var unitHops = c.WordsPerMinute > 0 ? (int)Math.Round(1200.0 / c.WordsPerMinute / hopMs) : 0;
                    var from = Math.Max(0, StartHop(c) - unitHops);
                    var to = Math.Min(heard.Envelope.Length, EndHop(c) + unitHops);
                    var (marks, gaps) = to > from
                        ? CwUnitEstimator.Elements(heard.Envelope.Skip(from).Take(to - from).ToList(), hopMs)
                        : (Array.Empty<double>(), Array.Empty<double>());
                    var sentPattern = PatternOf.TryGetValue(step.Key, out var p) ? p : "?";

                    (isRight ? right : wrongs).Add(new Wrong(
                        keyed.Name, c, step.Key, sentPattern, isRight ? "right" : Relate(sentPattern, c.Pattern),
                        before?.SpanLogLikelihoodRatio ?? double.NaN, after?.SpanLogLikelihoodRatio ?? double.NaN,
                        ToneOver(c), pitch, marks, gaps, inserted, heard.Reading[at].Held, heard.Reading[at].Gaps,
                        heard.Speed[at].Estimator, heard.Speed[at].WindowMarksUnit));
                }
            }
        }

        return wrongs;
    }

    private static string Wpm(double unitMs)
        => double.IsNaN(unitMs) ? "-" : string.Create(CultureInfo.InvariantCulture, $"{1200.0 / unitMs:0.0}");

    private static string Bin(double ratio)
        => double.IsNaN(ratio) ? "marks cannot say"
            : ratio < 1 / 1.5 ? "under 0.67"
            : ratio < 1 / 1.25 ? "0.67 to 0.80"
            : ratio <= 1.25 ? "0.80 to 1.25"
            : ratio <= 1.5 ? "1.25 to 1.50"
            : ratio <= 2 ? "1.50 to 2.00"
            : "over 2.00";

    /// <remarks>
    /// Proves nothing about the decoder; prints the fact task 2 of work
    /// instruction 441 asks for. For each sure-wrong character: the speed the path
    /// was given and where it came from, the unit the character's own marks imply
    /// (its span, one unit either side) and the unit the whole window's marks
    /// imply, each as words a minute and as a ratio over the unit the path was
    /// timed with, above one where the path's clock ran fast. Then the same
    /// ratios over the sure-right characters, bin by bin, so a ratio can be chosen
    /// that separates them. Asserts only that it traced the metric's own count.
    /// </remarks>
    [Fact]
    public void EachSureWrongLetterAgainstTheSpeedItsMarksImply()
    {
        var wrongs = Trace(out var rights);

        _output.WriteLine(
            "speed | recording | at s | sent | emitted | group | given wpm | source | gaps | estimator dit mark ms | estimator element gap ms | "
            + "envelope marks ms | marks wpm, this character | ratio | marks wpm, the window | ratio");

        foreach (var w in wrongs)
        {
            var c = w.Character;

            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"speed | {w.Name} | {c.At.TotalSeconds:0.000} | `{w.Sent}` | `{c.Text}` | {w.Group} | {c.WordsPerMinute} | {w.Source} | "
                + $"{(w.Held ? "held" : "textbook")} | {w.Estimator.DitMarkMilliseconds:0} | {w.Estimator.ElementGapMilliseconds:0} | "
                + $"{string.Join(" ", w.Marks.Select(m => m.ToString("0", CultureInfo.InvariantCulture)))} | "
                + $"{Wpm(w.LocalMarksUnit)} | {w.LocalMarksUnit / w.UnitMs:0.00} | {Wpm(w.WindowMarksUnit)} | {w.WindowMarksUnit / w.UnitMs:0.00}"));
        }

        foreach (var (label, pick) in new (string, Func<Wrong, double>)[]
        {
            ("this character's marks", w => w.LocalMarksUnit / w.UnitMs),
            ("the window's marks", w => w.WindowMarksUnit / w.UnitMs),
        })
        {
            _output.WriteLine($"bin | ratio from {label} | sure wrong | sure right | wrong from the grid | wrong from the estimator");

            foreach (var bin in new[] { "under 0.67", "0.67 to 0.80", "0.80 to 1.25", "1.25 to 1.50", "1.50 to 2.00", "over 2.00", "marks cannot say" })
            {
                var inWrong = wrongs.Where(w => Bin(pick(w)) == bin).ToList();

                _output.WriteLine(
                    $"bin | {bin} | {inWrong.Count} | {rights.Count(w => Bin(pick(w)) == bin)} | "
                    + $"{inWrong.Count(w => w.Source == "grid")} | {inWrong.Count(w => w.Source == "estimator")}");
            }
        }

        _output.WriteLine(
            $"total | {wrongs.Count} sure wrong, {rights.Count} sure right | "
            + $"sources of the wrong: {wrongs.Count(w => w.Source == "grid")} grid, {wrongs.Count(w => w.Source == "estimator")} estimator, "
            + $"{wrongs.Count(w => w.Held)} under held gaps");

        var metric = TheRequirementsAreMeasuredTests.Real
            .Where(m => m.NotComputable is null)
            .SelectMany(m => m.Stretches)
            .Sum(a => CwMetrics.Invented(a).SureWrong);

        Assert.Equal(metric, wrongs.Count);
    }

    private static string Reading(Wrong w)
        => w.Held
            ? string.Create(CultureInfo.InvariantCulture,
                $"held element {w.HeldGaps.ElementMilliseconds:0} ms, character {w.HeldGaps.CharacterMilliseconds:0} ms, word {w.HeldGaps.WordMilliseconds:0} ms, element-to-character crossover {Math.Sqrt(w.HeldGaps.ElementMilliseconds * w.HeldGaps.CharacterMilliseconds):0} ms")
            : string.Create(CultureInfo.InvariantCulture,
                $"textbook 1, 3, 7 units, element-to-character crossover {Math.Sqrt(3) * w.UnitMs:0} ms");

    private static string Units(IEnumerable<double> ms, double unit)
        => string.Join(" ", ms.Select(m => string.Create(CultureInfo.InvariantCulture, $"{m:0}ms/{m / unit:0.0}u")));

    /// <remarks>
    /// Proves 2.1: every sure-but-wrong character on the real keyed recordings,
    /// with its recording and time, what the key says was sent and what was
    /// emitted, the path's pattern against the sent pattern, its span and its
    /// neighbors' spans, the speed and pitch in force, and the envelope's marks and
    /// gaps over it in milliseconds and units; then the groups by how the emitted
    /// pattern relates to the sent one, each crossed with whether a space the
    /// decoder inserted sits beside it and how far off the sender's pitch it was
    /// mixed. Asserts only that it traced the metric's own count.
    /// </remarks>
    [Fact]
    public void EverySureWrongCharacterAndWhatItShares()
    {
        var wrongs = Trace();

        _output.WriteLine(
            "wrong | recording | at s | sent | emitted | sent pattern | emitted pattern | group | span | span before | span after | "
            + "wpm | unit ms | pitch at hop Hz | sender pitch Hz | off Hz | beside an inserted space | envelope marks | envelope gaps | gap reading");

        foreach (var w in wrongs)
        {
            var c = w.Character;

            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"wrong | {w.Name} | {c.At.TotalSeconds:0.000} | `{w.Sent}` | `{c.Text}` | {w.SentPattern} | {c.Pattern} | {w.Group} | "
                + $"{c.SpanLogLikelihoodRatio:G4} | {w.SpanBefore:G4} | {w.SpanAfter:G4} | {c.WordsPerMinute} | {w.UnitMs:0.0} | "
                + $"{w.ToneHz:0.0} | {w.PitchHz:0.0} | {Math.Abs(w.ToneHz - w.PitchHz):0.0} | {(w.InsertedSpace ? "yes" : "no")} | "
                + $"{Units(w.Marks, w.UnitMs)} | {Units(w.Gaps, w.UnitMs)} | {Reading(w)}"));
        }

        _output.WriteLine("group | group | count | beside an inserted space | mixed 25 Hz or more off the sender | read under held gaps | recordings | sent -> emitted");

        foreach (var g in wrongs.GroupBy(w => w.Group).OrderByDescending(g => g.Count()))
        {
            _output.WriteLine(
                $"group | {g.Key} | {g.Count()} | {g.Count(w => w.InsertedSpace)} | {g.Count(w => Math.Abs(w.ToneHz - w.PitchHz) >= 25)} | {g.Count(w => w.Held)} | "
                + $"{g.Select(w => w.Name).Distinct().Count()} | "
                + string.Join(", ", g.GroupBy(w => $"{w.Sent}->{w.Character.Text}").OrderByDescending(p => p.Count()).Select(p => $"{p.Key} x{p.Count()}")));
        }

        _output.WriteLine("pair | sent -> emitted | count | group");

        foreach (var p in wrongs.GroupBy(w => $"{w.Sent} {w.SentPattern} -> {w.Character.Text} {w.Character.Pattern}").OrderByDescending(p => p.Count()))
        {
            _output.WriteLine($"pair | {p.Key} | {p.Count()} | {p.First().Group}");
        }

        _output.WriteLine("recording | recording | sure wrong");

        foreach (var r in wrongs.GroupBy(w => w.Name).OrderByDescending(r => r.Count()))
        {
            _output.WriteLine($"recording | {r.Key} | {r.Count()}");
        }

        _output.WriteLine(
            $"total | {wrongs.Count} sure-but-wrong characters over {wrongs.Select(w => w.Name).Distinct().Count()} recordings, inferred keys | "
            + "0 left out as key-doubtful: no scored stretch's key file calls its own reading doubtful | "
            + $"{wrongs.Count(w => w.InsertedSpace)} beside a space the decoder inserted | "
            + $"{wrongs.Count(w => Math.Abs(w.ToneHz - w.PitchHz) >= 25)} mixed 25 Hz or more off the sender");

        var metric = TheRequirementsAreMeasuredTests.Real
            .Where(m => m.NotComputable is null)
            .SelectMany(m => m.Stretches)
            .Sum(a => CwMetrics.Invented(a).SureWrong);

        Assert.Equal(metric, wrongs.Count);
    }
}

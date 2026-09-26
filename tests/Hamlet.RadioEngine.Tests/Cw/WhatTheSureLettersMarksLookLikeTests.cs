using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// Every sure letter, right, wrong and added, against the shape a dit, a dah and
/// an element gap have at the unit in force when it was emitted (work instruction
/// 445, task 1; PHASE_PLAN.md 3.2; HM-REQ-011).
/// </summary>
/// <remarks>
/// <para>**A PRINTER. IT ASSERTS NOTHING.** It prints the measurement 3.2's change
/// would be built from, and the edge if there is one: the smallest distance past
/// which no right letter lies.</para>
/// <para>**THE MARKS ARE THE ENVELOPE'S, READ FROM WHAT THE STREAM HOLDS AT
/// EMISSION.** The letter's span, one unit of the path's speed either side, cut
/// exactly as <see cref="CwUnitEstimator.Elements"/> cuts it; only the marks that
/// begin and end inside that stretch are kept, and only the gaps between two of
/// them, so neither the padding nor a neighbor's tail is counted as the letter's.
/// The unit is the one the path was read with (<see cref="CwProbabilisticStream.Last"/>),
/// not the whole number the character carries.</para>
/// <para>**A DISTANCE IS A RATIO, ONE OR MORE.** A mark's is how many times longer
/// or shorter it is than the nearer of 1 and 3 units; a gap's is the same against
/// 1 unit. The bins were set before any distance was read.</para>
/// <para>**THE REAL KEYS ARE INFERRED** (R61, V-13); the synthetic keys are exact.</para>
/// </remarks>
public sealed class WhatTheSureLettersMarksLookLikeTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the tables are printed.</param>
    public WhatTheSureLettersMarksLookLikeTests(ITestOutputHelper output)
        => _output = output;

    /// <summary>The bin edges, set before any distance was read.</summary>
    internal static readonly double[] Edges = { 1.25, 1.5, 2, 3, 5 };

    /// <summary>One sure letter and its shape.</summary>
    internal sealed record Letter(
        string Name, string Set, CwCharacter Character, string Sent, string Class,
        double UnitMs, string Source, IReadOnlyList<double> Marks, IReadOnlyList<double> Gaps)
    {
        /// <summary>The worst mark's distance, or infinity where no whole mark was found.</summary>
        public double WorstMark => Marks.Count == 0 ? double.PositiveInfinity : Marks.Max(m => MarkDistance(m / UnitMs));

        /// <summary>The worst inner gap's distance, or 1 where there is no inner gap.</summary>
        public double WorstGap => Gaps.Count == 0 ? 1 : Gaps.Max(g => Distance(g / UnitMs, 1));

        /// <summary>The worse of the two.</summary>
        public double Worst => Math.Max(WorstMark, WorstGap);

        /// <summary>Right, or not.</summary>
        public bool IsRight => Class == "right";
    }

    /// <summary>How many times longer or shorter than the target, one or more.</summary>
    internal static double Distance(double units, double target)
        => units <= 0 ? double.PositiveInfinity : Math.Max(units / target, target / units);

    /// <summary>A mark's distance from the nearer of a dit and a dah.</summary>
    internal static double MarkDistance(double units) => Math.Min(Distance(units, 1), Distance(units, 3));

    private static readonly Func<double[], double> Otsu = (Func<double[], double>)Delegate.CreateDelegate(
        typeof(Func<double[], double>),
        typeof(CwUnitEstimator).GetMethod("Otsu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!);

    private static readonly int ShortestRunHops = (int)typeof(CwUnitEstimator)
        .GetField("ShortestRunHops", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
        .GetRawConstantValue()!;

    /// <summary>
    /// The marks that begin and end inside the stretch and the gaps between them,
    /// cut exactly as <see cref="CwUnitEstimator.Elements"/> cuts.
    /// </summary>
    /// <param name="envelope">The stretch's envelope magnitudes.</param>
    /// <param name="hopMilliseconds">How long one hop lasts.</param>
    /// <returns>Mark and gap lengths in milliseconds.</returns>
    internal static (IReadOnlyList<double> Marks, IReadOnlyList<double> Gaps) Inner(
        IReadOnlyList<double> envelope, double hopMilliseconds)
    {
        if (envelope.Count < 2)
        {
            return (Array.Empty<double>(), Array.Empty<double>());
        }

        var db = envelope.Select(e => 20 * Math.Log10(Math.Max(e, 1e-12))).ToArray();
        var cut = Otsu(db);
        var on = cut + CwUnitEstimator.HysteresisDb;
        var off = cut - CwUnitEstimator.HysteresisDb;
        var runs = new List<(bool Mark, int Hops)>();
        var keyDown = db[0] > on;
        var runStart = 0;

        for (var i = 1; i < db.Length; i++)
        {
            if (!(keyDown ? db[i] < off : db[i] > on))
            {
                continue;
            }

            var hops = i - runStart;

            // The first run touches the stretch's start, so it is not whole; the
            // last never ends inside it and is never recorded.
            if (runStart > 0 && hops >= ShortestRunHops)
            {
                runs.Add((keyDown, hops));
            }

            keyDown = !keyDown;
            runStart = i;
        }

        var first = runs.FindIndex(r => r.Mark);
        var last = runs.FindLastIndex(r => r.Mark);
        var marks = runs.Where(r => r.Mark).Select(r => r.Hops * hopMilliseconds).ToList();
        var gaps = first < 0
            ? new List<double>()
            : runs.Skip(first).Take(last - first + 1).Where(r => !r.Mark).Select(r => r.Hops * hopMilliseconds).ToList();

        return (marks, gaps);
    }

    private sealed record Heard(IReadOnlyList<CwCharacter> Settled, double[] Envelope, IReadOnlyList<(double UnitMs, string Source)> Speed);

    private static Heard Decode(string path, double pitchHz)
    {
        var audio = WavAudio.Read(path);
        var decoder = new CwDecoder(audio.SampleRate, pitchHz);
        var settled = new List<CwCharacter>();
        var speed = new List<(double, string)>();
        var envelope = new List<double>();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var windowField = typeof(CwProbabilisticStream).GetField("_envelope", flags)!;
        var countField = typeof(CwProbabilisticStream).GetField("_envelopeCount", flags)!;

        decoder.CharacterSettled += c =>
        {
            settled.Add(c);

            var wpm = decoder.Stream.Last.WordsPerMinute;
            var window = ((double[])windowField.GetValue(decoder.Stream)!).Take((int)countField.GetValue(decoder.Stream)!).ToList();
            var hopMs = CwProbabilisticDecoder.HopMilliseconds;
            var estimator = CwUnitEstimator.Measure(window, hopMs);
            var marksUnit = CwUnitEstimator.MarkUnit(CwUnitEstimator.Elements(window, hopMs).Marks);
            var source = Math.Abs(wpm - 1200.0 / marksUnit) < 1e-6 ? "marks (441)"
                : estimator.IsReady && Math.Abs(wpm - estimator.WordsPerMinute) < 1e-6 ? "estimator"
                : "grid";

            speed.Add((wpm > 0 ? 1200.0 / wpm : double.NaN, source));
        };

        var hop = decoder.Tracker.HopSamples;

        for (var at = 0L; at + hop <= audio.Samples.Length; at += hop)
        {
            decoder.Process(new AudioChunk(at, audio.SampleRate, audio.Samples.AsSpan((int)at, hop)));
            envelope.Add(decoder.Stream.NewestEnvelope);
        }

        decoder.Flush();

        return new Heard(settled, envelope.ToArray(), speed);
    }

    private static IEnumerable<Letter> Letters(string name, string set, Heard heard, IEnumerable<CwScore> scores, CwKeyKind kind)
    {
        var hopMs = CwProbabilisticDecoder.HopMilliseconds;
        var settled = heard.Settled.ToList();

        foreach (var score in scores)
        {
            var covered = TheRequirementsAreMeasuredTests.Covered(settled, score);
            var characters = covered.Where(c => !c.IsWordGap).ToList();
            var alignment = CwMetrics.Align(CwMetrics.Symbols(covered), score.Key, kind);
            var d = 0;

            foreach (var step in alignment.Steps)
            {
                if (step.Decoded is null)
                {
                    continue;
                }

                var c = characters[d++];

                if (step.Decoded.Value.Class != CwSymbolClass.Sure)
                {
                    continue;
                }

                var at = settled.IndexOf(c);
                var (unitMs, source) = heard.Speed[at];
                var endHop = (int)Math.Round(c.At.TotalMilliseconds / hopMs);
                var unitHops = double.IsNaN(unitMs) ? 0 : (int)Math.Round(unitMs / hopMs);
                var from = Math.Max(0, endHop - c.SpanHops - unitHops);
                var to = Math.Min(heard.Envelope.Length, endHop + unitHops);
                var (marks, gaps) = Inner(heard.Envelope.Skip(from).Take(Math.Max(0, to - from)).ToList(), hopMs);
                var cls = step.Key is null ? "added"
                    : string.Equals(step.Key, step.Decoded.Value.Text, StringComparison.Ordinal) ? "right" : "wrong";

                yield return new Letter(name, set, c, step.Key ?? "-", cls, unitMs, source, marks, gaps);
            }
        }
    }

    private static IReadOnlyList<Letter> Real()
        => WhatTheStrayLettersRestOnTests.KeyedRecordings
            .SelectMany(k =>
            {
                var heard = Decode(Path.Combine(CapturedSignalTests.Folder, k.Name + ".wav"), 600);

                return Letters(k.Name, "real", heard, k.Score(CwReading.Of(heard.Settled)), CwKeyKind.Inferred);
            })
            .ToList();

    private static IReadOnlyList<Letter> Synthetic()
        => SyntheticCq.All
            .SelectMany(recipe =>
            {
                var heard = Decode(Path.Combine(SyntheticCq.Folder, recipe.Name + ".wav"), SyntheticCq.StartingPitchHz);
                var reading = CwReading.Of(heard.Settled);
                var from = reading.Text.TakeWhile(char.IsWhiteSpace).Count();

                return Letters(recipe.Name, "synthetic", heard, new[] { SyntheticCq.Whole(reading) with { Start = from } }, CwKeyKind.Exact);
            })
            .ToList();

    private static string Bin(double distance)
    {
        if (double.IsPositiveInfinity(distance))
        {
            return "no whole mark";
        }

        var low = 1.0;

        foreach (var e in Edges)
        {
            if (distance < e)
            {
                return string.Create(CultureInfo.InvariantCulture, $"{low:0.00} to {e:0.00}");
            }

            low = e;
        }

        return string.Create(CultureInfo.InvariantCulture, $"{Edges[^1]:0.00} and over");
    }

    private static string Units(IEnumerable<double> ms, double unitMs, Func<double, double> distance)
        => string.Join(" ", ms.Select(m => string.Create(CultureInfo.InvariantCulture, $"{m:0}ms/{m / unitMs:0.00}u/d{distance(m / unitMs):0.00}")));

    private void Print(string set, IReadOnlyList<Letter> letters)
    {
        _output.WriteLine(
            $"letter | set | recording | at s | sent | emitted | class | pattern | unit ms | unit from | marks ms/units/distance | "
            + "inner gaps ms/units/distance | worst mark | worst gap | marks against elements");

        foreach (var l in letters)
        {
            var c = l.Character;

            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"letter | {set} | {l.Name} | {c.At.TotalSeconds:0.000} | `{l.Sent}` | `{c.Text}` | {l.Class} | {c.Pattern} | {l.UnitMs:0.0} | {l.Source} | "
                + $"{Units(l.Marks, l.UnitMs, MarkDistance)} | {Units(l.Gaps, l.UnitMs, u => Distance(u, 1))} | "
                + $"{l.WorstMark:0.00} | {l.WorstGap:0.00} | {l.Marks.Count} of {c.Pattern.Length}"));
        }

        foreach (var (label, pick) in new (string, Func<Letter, double>)[]
        {
            ("worst mark", l => l.WorstMark),
            ("worst gap", l => l.WorstGap),
            ("together, the worse of the two", l => l.Worst),
        })
        {
            _output.WriteLine($"bin | {set} | {label} | distance | right | wrong | added | wrong or added | right at or past the bin | wrong or added at or past the bin");

            var bins = new[] { "1.00 to 1.25" }
                .Concat(Edges.Zip(Edges.Skip(1), (a, b) => string.Create(CultureInfo.InvariantCulture, $"{a:0.00} to {b:0.00}")))
                .Concat(new[] { string.Create(CultureInfo.InvariantCulture, $"{Edges[^1]:0.00} and over"), "no whole mark" })
                .ToList();

            for (var b = 0; b < bins.Count; b++)
            {
                var inBin = letters.Where(l => Bin(pick(l)) == bins[b]).ToList();
                var past = letters.Where(l => bins.IndexOf(Bin(pick(l))) >= b).ToList();

                _output.WriteLine(
                    $"bin | {set} | {label} | {bins[b]} | {inBin.Count(l => l.IsRight)} | {inBin.Count(l => l.Class == "wrong")} | "
                    + $"{inBin.Count(l => l.Class == "added")} | {inBin.Count(l => !l.IsRight)} | {past.Count(l => l.IsRight)} | {past.Count(l => !l.IsRight)}");
            }

            // The edge: the farthest any right letter lies. Past it, no right letter.
            var rights = letters.Where(l => l.IsRight).Select(pick).ToList();
            var edge = rights.Count == 0 ? double.NaN : rights.Max();
            var caught = letters.Where(l => !l.IsRight && pick(l) > edge).ToList();

            var caughtText = string.Join(", ", caught.Select(l => string.Create(CultureInfo.InvariantCulture,
                $"{l.Name} {l.Character.At.TotalSeconds:0.000} `{l.Sent}`->`{l.Character.Text}` {pick(l):0.00}")));

            _output.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"edge | {set} | {label} | farthest right letter {edge:0.000} | wrong or added past it {caught.Count} of {letters.Count(l => !l.IsRight)} | {caughtText}"));

            var farthest = string.Join(", ", letters.Where(l => l.IsRight).OrderByDescending(pick).Take(5).Select(l => string.Create(CultureInfo.InvariantCulture,
                $"{l.Name} {l.Character.At.TotalSeconds:0.000} `{l.Character.Text}` {pick(l):0.00}")));

            _output.WriteLine($"farthest right | {set} | {label} | {farthest}");
        }

        _output.WriteLine(
            $"total | {set} | {letters.Count} sure letters: {letters.Count(l => l.IsRight)} right, {letters.Count(l => l.Class == "wrong")} wrong, "
            + $"{letters.Count(l => l.Class == "added")} added | {letters.Count(l => l.Marks.Count != l.Character.Pattern.Length)} whose whole marks differ in number from the pattern's elements | "
            + $"unit from: {string.Join(", ", letters.GroupBy(l => l.Source).Select(g => $"{g.Key} {g.Count()}"))} | "
            + $"{letters.Count(l => (int)Math.Round(1200.0 / l.UnitMs) != l.Character.WordsPerMinute)} whose rounded unit differs from the character's wpm");
    }

    /// <remarks>
    /// Proves nothing about the decoder; prints the fact task 1 of work
    /// instruction 445 asks for, on the real keyed recordings with inferred keys
    /// and then on the synthetic set with exact keys.
    /// </remarks>
    [Fact]
    public void EverySureLetterAgainstADitADahAndAnElementGap()
    {
        Print("real", Real());
        Print("synthetic", Synthetic());
    }
}

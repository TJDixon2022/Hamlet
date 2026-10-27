using System.Globalization;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE HAND WHOSE DAH IS NOT TWICE HIS DIT** (work instruction 577, HM-DEC-281). A straight key at 12 WPM whose dah is 1.7
/// times his dit, the far side of most ragchews, beside the same hand at 2.0 and 2.5, each at three seeds. The yardstick has a
/// truth; Ken on <c>cw-2026-10-19-141708</c> is the specimen.
/// </summary>
/// <remarks>
/// **SYNTHETIC AUDIO WRITTEN HERE, NOTHING READ FROM DISK**: dits and dahs each spread ±15%, gaps inside letters one dit ±20%,
/// letter gaps 2 to 3.5 dits, word gaps 4 to 6, at 20 dB over the noise on unit 502's scale, at 625 Hz.
/// </remarks>
public sealed class TheStraightKeyReadsTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>What the hand sends.</summary>
    internal const string Text = "CQ CQ DE N0CALL N0CALL K GM KEN NICE TO MEET U RPRT UR 559 QTH IS CHICOPEE MA K";

    /// <summary>The three seeds.</summary>
    internal static readonly int[] Seeds = [5770, 5771, 5772];

    /// <summary>The dah over the dit of the three hands.</summary>
    internal static readonly double[] Ratios = [1.7, 2.0, 2.5];

    private const double Pitch = 625;

    /// <summary>The hand's key-down and key-up stretches: 12 WPM, the dah <paramref name="ratio"/> dits.</summary>
    internal static List<(double Seconds, bool On)> Hand(double ratio, int seed)
    {
        var random = new Random(seed);
        const double Dit = 0.1;
        var keyed = new List<(double Seconds, bool On)> { (3.0, false) };

        double Spread(double ideal, double s) => ideal * (1 + (s * ((2 * random.NextDouble()) - 1)));
        double Between(double low, double high) => Dit * (low + ((high - low) * random.NextDouble()));

        var words = Text.Split(' ');

        for (var w = 0; w < words.Length; w++)
        {
            for (var c = 0; c < words[w].Length; c++)
            {
                var code = AHandIsReadAgainstItselfTests.Morse[words[w][c]];

                for (var e = 0; e < code.Length; e++)
                {
                    keyed.Add((Spread(code[e] == '-' ? ratio * Dit : Dit, 0.15), true));

                    if (e + 1 < code.Length)
                    {
                        keyed.Add((Spread(Dit, 0.2), false));
                    }
                }

                if (c + 1 < words[w].Length)
                {
                    keyed.Add((Between(2, 3.5), false));
                }
                else if (w + 1 < words.Length)
                {
                    keyed.Add((Between(4, 6), false));
                }
            }
        }

        keyed.Add((3.0, false));

        return keyed;
    }

    /// <summary>The hand's audio at 20 dB.</summary>
    internal static float[] Audio(double ratio, int seed) => AHandIsReadAgainstItselfTests.Synthesize(Hand(ratio, seed), seed, 20, 0, Pitch);

    /// <summary>One reading of one hand: letters right of sent, and what the gate made of its marks.</summary>
    internal sealed record Row(double Ratio, int Seed, int Right, int Of, ThePatternIsTheGateTests.Reading Reading, double WidestJump, double CentreRatio, double Shape);

    /// <summary>
    /// Reads one hand through the app's chain, the rig's passband set as the fist bank's hands are read (work instruction 589,
    /// section 4): the sequence reader lays its lanes across the rig's passband, and with none set it is not built at all.
    /// </summary>
    internal static Row Read(double ratio, int seed)
    {
        var r = ThePatternIsTheGateTests.Read(Audio(ratio, seed), Pitch, d => d.SetPassband(600, 500));
        var of = Text.Count(c => c != ' ');
        var right = TheRecordingsScoreboardTests.Right(r.Text, Text);

        // What the gate saw: the hand's own marks as the grid called them, their widest neighbour jump, and the centres
        // two means in log length settle on from it.
        var marks = Marks(ratio, seed);
        var lengths = marks.Where(m => Math.Abs(m.PitchHz - Pitch) <= 5).Select(m => m.ToSeconds - m.FromSeconds).Order().ToList();
        var (jump, centres) = Kinds(lengths);
        var shape = marks.Count >= 10 ? CwSequenceShape.Of(marks.Take(16).ToList(), marks.Count).Score : double.NaN;

        return new Row(ratio, seed, right, of, r, jump, centres, shape);
    }

    /// <summary>The candidate marks within the pitch tolerance the grid called on the hand.</summary>
    private static List<CwMark> Marks(double ratio, int seed)
    {
        var samples = Audio(ratio, seed);
        using var chain = new CwChain(8000);

        for (var at = 0; at + 80 <= samples.Length; at += 80)
        {
            chain.Process(new Hamlet.RadioEngine.Audio.AudioChunk(at, 8000, samples.AsSpan(at, 80)));
        }

        chain.Decoder.Flush();

        return chain.Detector.CandidatesKept.Where(m => Math.Abs(m.PitchHz - Pitch) <= CwSenderGate.PitchToleranceHz).OrderBy(m => m.FromSeconds).ToList();
    }

    /// <summary>The widest neighbour ratio among sorted lengths, and the dah centre over the dit centre two means settle on.</summary>
    internal static (double Jump, double CentreRatio) Kinds(IReadOnlyList<double> sorted)
    {
        if (sorted.Count < 2)
        {
            return (double.NaN, double.NaN);
        }

        var at = 1;
        var widest = 0.0;

        for (var i = 1; i < sorted.Count; i++)
        {
            if (sorted[i] / sorted[i - 1] > widest)
            {
                widest = sorted[i] / sorted[i - 1];
                at = i;
            }
        }

        static double Centre(IEnumerable<double> l) => Math.Exp(l.Average(x => Math.Log(x)));

        for (var k = 0; k < 8; k++)
        {
            var line = Math.Sqrt(Centre(sorted.Take(at)) * Centre(sorted.Skip(at)));
            var next = sorted.Count(l => l < line);

            if (next == at || next == 0 || next == sorted.Count)
            {
                break;
            }

            at = next;
        }

        return (widest, Centre(sorted.Skip(at)) / Centre(sorted.Take(at)));
    }

    /// <remarks>Work instruction 577, section 3: the grid's candidate marks on one hand, by pitch, with their lengths. Asserts nothing.</remarks>
    [Theory]
    [InlineData(1.7, 5770)]
    [InlineData(2.0, 5771)]
    [InlineData(2.5, 5770)]
    public void TheMarksTheGridCalled(double ratio, int seed)
    {
        var marks = Marks(ratio, seed);

        foreach (var g in marks.GroupBy(m => Math.Round(m.PitchHz)).OrderBy(g => g.Key))
        {
            var lengths = g.Select(m => (m.ToSeconds - m.FromSeconds) * 1000).Order().ToList();
            var (jump, centres) = Kinds(lengths);

            output.WriteLine(string.Create(Inv, $"{g.Key:0} Hz: {g.Count()} marks, widest jump {jump:0.00}, centres {centres:0.00}: {string.Join(" ", lengths.Select(l => l.ToString("0", Inv)))}"));
        }

        var all = marks.Select(m => (m.ToSeconds - m.FromSeconds) * 1000).Order().ToList();
        output.WriteLine(string.Create(Inv, $"in time order, first 60: {string.Join(" ", marks.Take(60).Select(m => $"{m.FromSeconds:0.00}/{(m.ToSeconds - m.FromSeconds) * 1000:0}/{m.PitchHz:0}"))}"));
        output.WriteLine(string.Create(Inv, $"all {all.Count}: centres {Kinds(all).CentreRatio:0.00}"));
    }

    /// <summary>The two kinds two means in log length settle on from the widest jump: the dits, the dahs, and their spreads.</summary>
    internal static (List<double> Dits, List<double> Dahs, double Ratio, double Spreads) TwoMeans(IReadOnlyList<double> sorted)
    {
        if (sorted.Count < 4)
        {
            return ([], [], double.NaN, double.NaN);
        }

        var at = 1;
        var widest = 0.0;

        for (var i = 1; i < sorted.Count; i++)
        {
            if (sorted[i] / sorted[i - 1] > widest)
            {
                widest = sorted[i] / sorted[i - 1];
                at = i;
            }
        }

        static (double Mu, double Sd) Stats(IReadOnlyCollection<double> l)
        {
            var logs = l.Select(x => Math.Log(x)).ToList();
            var mu = logs.Average();

            return (mu, Math.Sqrt(logs.Sum(x => (x - mu) * (x - mu)) / logs.Count));
        }

        for (var k = 0; k < 8; k++)
        {
            var line = Math.Exp((Stats(sorted.Take(at).ToList()).Mu + Stats(sorted.Skip(at).ToList()).Mu) / 2);
            var next = sorted.Count(l => l < line);

            if (next == at || next == 0 || next == sorted.Count)
            {
                break;
            }

            at = next;
        }

        var dits = sorted.Take(at).ToList();
        var dahs = sorted.Skip(at).ToList();
        var (a, b) = (Stats(dits), Stats(dahs));

        return (dits, dahs, Math.Exp(b.Mu - a.Mu), (b.Mu - a.Mu) / Math.Max(1e-9, Math.Max(a.Sd, b.Sd)));
    }

    /// <summary>Ken's two overs: about 80 to 100 s, the burst the stray capture fired on, and 272 to 300 s.</summary>
    public static TheoryData<double, double, bool> KensOvers => new() { { 80, 100, false }, { 272, 300, false }, { 80, 100, true }, { 272, 300, true } };

    /// <remarks>
    /// Work instruction 577, section 3: Ken through the chain as shipped. The grid's sequence near 437 Hz and why it never
    /// qualifies, the bank's lane at 425 Hz and why it is not handed on, and what a reader would make of his marks kinded at
    /// their natural break. Asserts nothing.
    /// </remarks>
    [Theory]
    [MemberData(nameof(KensOvers))]
    public void KensOverTraced(double from, double to, bool rule)
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(TheFoundWeakStationTests.Ken));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(TheFoundWeakStationTests.Ken);
        var rate = audio.SampleRate;
        var chunk = rate / 100;
        using var chain = new CwChain(rate);
        var handover = new List<string>();
        var letters = new List<(double At, double PitchHz, string Text, bool Space)>();
        var space = false;
        var gate = chain.Decoder.Runs;

        chain.Detector.SetPassband(pitch, width);
        chain.Detector.HandoverTrace = handover;
        gate.CharacterRead += c => space |= c.Text == MorseAlphabet.WordGap;
        gate.RunRead += (c, run) =>
        {
            letters.Add((run[^1].ToSeconds, run.Average(m => m.PitchHz), c.Text, space));
            space = false;
        };

        using var off = rule ? null : CwRules.Off(CwRules.ShortDah);

        output.WriteLine(string.Create(Inv, $"## Ken, {from:0} to {to:0} s, {(rule ? "with two stable clusters under a jump of two" : "with the rule off")}"));

        for (var at = 0; at + chunk <= audio.Samples.Length && at < (to + 2) * rate; at += chunk)
        {
            chain.Process(new Hamlet.RadioEngine.Audio.AudioChunk(at, rate, audio.Samples.AsSpan(at, chunk)));

            var now = (at + chunk) / (double)rate;

            if (now < from || now > to || (at / chunk) % 100 != 0)
            {
                continue;
            }

            var grid = chain.Detector.SequencesNow(now).Split(" | ", StringSplitOptions.RemoveEmptyEntries)
                .Where(d => double.TryParse(d.Split(' ')[0], NumberStyles.Float, Inv, out var p) && p >= 400 && p <= 475);
            var lanes = chain.Detector.Bank is { } bank
                ? bank.Standing(now).Where(s => Math.Abs(s.Sequence.PitchHz - 425) <= 30).Select(s => string.Create(Inv, $"{s.Sequence.PitchHz:0}/{CwLaneBank.DitSeconds[s.Dit] * 1000:0} {s.Sequence.Shape.Score:0.00}"))
                : [];
            var senders = gate.SenderTests.Where(s => s.PitchHz >= 400 && s.PitchHz <= 475)
                .Select(s => string.Create(Inv, $"{s.PitchHz:0} Hz {s.Marks} marks, keyed runs {s.KeyedRuns}, two kinds {(s.TwoKinds ? "yes" : "no")}, letter gap {(s.LetterGap ? "yes" : "no")}, kinds held {(s.KindsHeld ? "yes" : "no")}, sure {(s.Sure ? "yes" : "no")}, shape {s.Shape:0.00}"));

            output.WriteLine(string.Create(Inv, $"{now:0} s: grid {string.Join(" | ", grid)}; bank {string.Join(", ", lanes)}; senders {string.Join("; ", senders)}"));
        }

        foreach (var h in handover.Where(h => double.TryParse(h.Split(' ')[0], NumberStyles.Float, Inv, out var s) && s >= from - 5 && s <= to))
        {
            output.WriteLine("handover: " + h);
        }

        // 1. The grid's marks near his pitch, called and stood, and the two kinds they make.
        foreach (var (what, marks) in new[] { ("called", chain.Detector.CandidatesKept), ("stood", chain.Detector.MarksSince(0).Marks) })
        {
            var near = marks.Where(m => m.FromSeconds >= from && m.FromSeconds <= to && m.PitchHz >= 410 && m.PitchHz <= 465).OrderBy(m => m.FromSeconds).ToList();
            var lengths = near.Select(m => (m.ToSeconds - m.FromSeconds) * 1000).Order().ToList();
            var (dits, dahs, ratio, spreads) = TwoMeans(lengths);
            var (jump, _) = Kinds(lengths);

            output.WriteLine(string.Create(Inv, $"1. the grid's marks {what} at 410 to 465 Hz: {near.Count}, at {string.Join(", ", near.GroupBy(m => Math.Round(m.PitchHz / 5) * 5).Select(g => $"{g.Key:0} Hz {g.Count()}"))}"));
            output.WriteLine("   in order (ms): " + string.Join(" ", near.Select((m, i) => (i > 0 ? $"[{(m.FromSeconds - near[i - 1].ToSeconds) * 1000:0}] " : string.Empty) + ((m.ToSeconds - m.FromSeconds) * 1000).ToString("0", Inv))));
            output.WriteLine(string.Create(Inv, $"   sorted: {string.Join(" ", lengths.Select(l => l.ToString("0", Inv)))}; widest neighbour jump {jump:0.00}; two means: {dits.Count} dits centre {Math.Exp(dits.DefaultIfEmpty(1).Average(x => Math.Log(x))):0} ms, {dahs.Count} dahs centre {Math.Exp(dahs.DefaultIfEmpty(1).Average(x => Math.Log(x))):0} ms, ratio {ratio:0.00}, {spreads:0.0} spreads apart"));
        }

        // 2. What printed.
        var mine = letters.Where(l => l.At >= from && l.At <= to + 1).ToList();
        string Text(IEnumerable<(double At, double PitchHz, string Text, bool Space)> ls) => string.Concat(ls.Select((l, i) => (i > 0 && l.Space ? " " : string.Empty) + l.Text));
        var strays = Text(mine).Split(' ', StringSplitOptions.RemoveEmptyEntries).Count(w => w is "E" or "T" or "I" or "A" or "N" or "M") * 10 / (to - from);
        var long3 = Text(mine).Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length >= 3).ToList();
        output.WriteLine(string.Create(Inv, $"printed: `{Text(mine)}`; near his pitch: `{Text(mine.Where(l => l.PitchHz >= 400 && l.PitchHz <= 475))}`; lone one- and two-element words {strays:0.0} per 10 s; words of three letters or more: {string.Join(", ", long3)}"));

        // 3. By hand, kinded at the natural break, and read through the table.
        var (hand, contrast, hz) = TheFoundWeakStationTests.ByHand(audio.Samples, rate, from, to, 430, CwLaneBank.DitSeconds[0]);
        var ceiling = Ceiling(hand);
        output.WriteLine(string.Create(Inv, $"3. by hand at {hz:0} Hz, contrast {contrast:0.0} dB, {hand.Count} marks: {string.Join(" ", hand.Select(m => ((m.To - m.From) * 1000).ToString("0", Inv)))}"));
        output.WriteLine(string.Create(Inv, $"   {ceiling.How}; reads `{ceiling.Text}`"));
        output.WriteLine(string.Empty);
    }

    /// <summary>
    /// What a reader would make of some marks kinded at their natural break: two means in log length from the widest jump,
    /// with the largest jump reported beside it; gaps inside letters under the two means' line of the gaps under five dits, word gaps past five dits.
    /// </summary>
    internal static (string Text, string How) Ceiling(IReadOnlyList<(double From, double To)> marks)
    {
        var sorted = marks.Select(m => m.To - m.From).Order().ToList();

        if (sorted.Count < 8)
        {
            return (string.Empty, "too few marks");
        }

        var at = 3;

        for (var i = 3; i <= sorted.Count - 3; i++)
        {
            if (sorted[i] / sorted[i - 1] > sorted[at] / sorted[at - 1])
            {
                at = i;
            }
        }

        var jumpAt = at;
        var (ditKind, dahKind, _, _) = TwoMeans(sorted);

        at = ditKind.Count;

        var markLine = Math.Sqrt(sorted[at] * sorted[at - 1]);
        var dit = Math.Exp(sorted.Take(at).Average(x => Math.Log(x)));
        var gaps = marks.Skip(1).Select((m, i) => m.From - marks[i].To).ToList();
        var under = gaps.Where(g => g < 5 * dit).Order().ToList();
        var (inside, letter, _, _) = TwoMeans(under);
        var gapLine = inside.Count > 0 && letter.Count > 0 ? Math.Sqrt(inside.Max() * letter.Min()) : 2 * dit;
        var text = new System.Text.StringBuilder();
        var pattern = new System.Text.StringBuilder();

        void End()
        {
            if (pattern.Length > 0)
            {
                text.Append(MorseAlphabet.Lookup(pattern.ToString()) ?? "■");
                pattern.Clear();
            }
        }

        for (var i = 0; i < marks.Count; i++)
        {
            if (i > 0)
            {
                var g = gaps[i - 1];

                if (g >= 5 * dit)
                {
                    End();
                    text.Append(' ');
                }
                else if (g > gapLine)
                {
                    End();
                }
            }

            pattern.Append(marks[i].To - marks[i].From > markLine ? '-' : '.');
        }

        End();

        var words = text.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return (text.ToString(), string.Create(Inv, $"largest jump {sorted[jumpAt] / sorted[jumpAt - 1]:0.00} at {sorted[jumpAt - 1] * 1000:0} ms; kinded by two means at {markLine * 1000:0} ms, the jump there {sorted[at] / sorted[at - 1]:0.00}, dit {dit * 1000:0} ms, dah over dit {Math.Exp(sorted.Skip(at).Average(x => Math.Log(x))) / dit:0.00}; letter line {gapLine * 1000:0} ms, word line {5 * dit * 1000:0} ms; {words.Length} words, {words.Count(w => w.Length >= 3)} of three letters or more"));
    }

    /// <summary>The two means' clusters apart in spreads, each spread floored at the gate's own floor, over every run of <paramref name="n"/> marks.</summary>
    private static List<(double Ratio, double Spreads, double Sd)> Windows(IReadOnlyList<CwMark> marks, int n)
    {
        var all = new List<(double, double, double)>();

        for (var i = 0; i + n <= marks.Count; i++)
        {
            var sorted = marks.Skip(i).Take(n).Select(m => m.ToSeconds - m.FromSeconds).Order().ToList();
            var (dits, dahs, ratio, _) = TwoMeans(sorted);

            if (dits.Count < 2 || dahs.Count < 2)
            {
                all.Add((ratio, 0, double.NaN));
                continue;
            }

            var (a, b) = (CwSenderGate.LogStats(dits), CwSenderGate.LogStats(dahs));

            all.Add((ratio, (b.Mu - a.Mu) / Math.Max(a.Sd, b.Sd), Math.Max(a.Sd, b.Sd)));
        }

        return all;
    }

    /// <remarks>
    /// Work instruction 577, section 3: where the floor and the spread sit. Over every run of ten and of sixteen marks at the
    /// pitch, the dah over the dit and the clusters apart in spreads two means find, on the three hands and on the carrier keyed
    /// at random at the twenty seeds of its own test. Asserts nothing.
    /// </remarks>
    [Fact]
    public void WhereTheClustersStand()
    {
        static string P(IEnumerable<double> v)
        {
            var s = v.Where(double.IsFinite).Order().ToList();

            return s.Count == 0 ? "none" : string.Create(Inv, $"least {s[0]:0.00}, p5 {s[(int)(0.05 * s.Count)]:0.00}, median {s[s.Count / 2]:0.00}, p95 {s[(int)(0.95 * (s.Count - 1))]:0.00}, most {s[^1]:0.00}");
        }

        foreach (var n in new[] { 10, 16 })
        {
            foreach (var ratio in Ratios)
            {
                var w = Seeds.SelectMany(seed => Windows(Marks(ratio, seed).Where(m => Math.Abs(m.PitchHz - Pitch) <= 5).ToList(), n)).ToList();

                output.WriteLine(string.Create(Inv, $"{n} marks, the {ratio:0.0} hand, {w.Count} runs: dah over dit {P(w.Select(x => x.Ratio))}; spreads {P(w.Select(x => x.Spreads))}"));
            }

            var carrier = Enumerable.Range(5193, 20).AsParallel().SelectMany(seed =>
            {
                var audio = TheShapePicksTheSenderTests.Noise(25, 5192);

                TheShapePicksTheSenderTests.Key(audio, TheShapePicksTheSenderTests.RandomKeying(2.5, 23, seed), 625, 24);

                using var chain = new CwChain(8000);

                for (var at = 0; at + 80 <= audio.Length; at += 80)
                {
                    chain.Process(new Hamlet.RadioEngine.Audio.AudioChunk(at, 8000, audio.AsSpan(at, 80)));
                }

                return Windows(chain.Detector.CandidatesKept.Where(m => Math.Abs(m.PitchHz - Pitch) <= 5).OrderBy(m => m.FromSeconds).ToList(), n);
            }).ToList();

            output.WriteLine(string.Create(Inv, $"{n} marks, the carrier at twenty seeds, {carrier.Count} runs: dah over dit {P(carrier.Select(x => x.Ratio))}; spreads {P(carrier.Select(x => x.Spreads))}"));

            var hand = Seeds.SelectMany(seed => Windows(Marks(1.7, seed).Where(m => Math.Abs(m.PitchHz - Pitch) <= 5).ToList(), n)).ToList();

            foreach (var spreads in new[] { 3.0, 4.0, 5.0 })
            {
                foreach (var sd in new[] { 0.12, 0.15, 0.2, 1.0 })
                {
                    bool Passes((double Ratio, double Spreads, double Sd) c) => c.Ratio >= 1.5 && c.Ratio < 2 && c.Spreads >= spreads && c.Sd <= sd;

                    output.WriteLine(string.Create(Inv, $"   at a floor of 1.5, {spreads:0.0} spreads and each cluster within {sd:0.00}: carrier runs passing {carrier.Count(Passes)} of {carrier.Count}, the 1.7 hand's {hand.Count(Passes)} of {hand.Count}"));
                }
            }
        }
    }

    /// <remarks>
    /// **EVERY STRAIGHT-KEY HAND READS 48 OF 60** (the owner's ruling on HM-DEC-285, 2026-10-20): with the short dah on by default
    /// the 1.7, 2.0 and 2.5 hands at each of the three seeds read 60 of 60 at the ship; each is held at 48.
    /// </remarks>
    [Fact]
    public void EveryHandReadsFortyEight()
    {
        var hands = Ratios.SelectMany(ratio => Seeds.Select(seed => (ratio, seed))).ToList();
        var rows = new Row[hands.Count];

        Parallel.For(0, hands.Count, i => rows[i] = Read(hands[i].ratio, hands[i].seed));

        foreach (var r in rows)
        {
            output.WriteLine(string.Create(Inv, $"dah over dit {r.Ratio:0.0}, seed {r.Seed}: {r.Right} of {r.Of}, `{r.Reading.Text}`"));
        }

        Assert.All(rows, r => Assert.True(r.Right >= 48, string.Create(Inv, $"the {r.Ratio:0.0} hand at seed {r.Seed} read {r.Right} of {r.Of}")));
    }

    /// <remarks>
    /// Work instruction 577, section 2: the ratio table, letters right at each ratio and seed, as it ships and with two stable
    /// clusters under a jump of two (<see cref="CwRules.ShortDah"/>, built and left off, HM-DEC-281). As it ships the 1.7 and 2.0
    /// hands read 0 of 60 at every seed and the 2.5 hand 60; with the rule all nine read 60 of 60. Asserts nothing.
    /// </remarks>
    /// <param name="rule">Whether the rule is chosen.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TheRatioTable(bool rule)
    {
        output.WriteLine(rule ? "with two stable clusters under a jump of two, as it ships since the owner's ruling on HM-DEC-285" : "with the rule off");
        output.WriteLine("| dah/dit | seed | right of sent | candidates, stood, printed | widest jump | centres | shape | reads |");
        output.WriteLine("|---|---|---|---|---|---|---|---|");

        using var off = rule ? null : CwRules.Off(CwRules.ShortDah);
        var state = CwRules.Current;
        var hands = Ratios.SelectMany(ratio => Seeds.Select(seed => (ratio, seed))).ToList();
        var rows = new Row[hands.Count];

        Parallel.For(0, hands.Count, i =>
        {
            using var _ = CwRules.Use(state);

            rows[i] = Read(hands[i].ratio, hands[i].seed);
        });

        foreach (var r in rows)
        {
            {
                output.WriteLine(string.Create(Inv, $"| {r.Ratio:0.0} | {r.Seed} | {r.Right} of {r.Of} | {r.Reading.Candidates}, {r.Reading.Stood}, {r.Reading.Printed} | {r.WidestJump:0.00} | {r.CentreRatio:0.00} | {r.Shape:0.00} | `{r.Reading.Text}` |"));
            }
        }
    }
}

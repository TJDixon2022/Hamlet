using System.Globalization;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **THE SEQUENCE READER: THE WHOLE KEYING EXPLAINS THE ENVELOPE** (work instruction 588, HM-DEC-292). Of every legal sequence of
/// dits, dahs, inside gaps, letter gaps and word gaps at the sender's own rhythm, the one that best explains a narrow lane's whole
/// envelope: an explicit-duration (semi-Markov) Viterbi decoder. It never decides a mark on its own; it finds the path.
/// </summary>
/// <remarks>
/// <para>**A PORT, NOT A REINTERPRETATION.** The static half is the Python prototype in <c>tools\prototype\sequence-reader\</c>
/// line for line: <see cref="Envelope"/> is <c>envelope_db</c>, <see cref="Decode"/> is <c>decode</c>, <see cref="RangesFromDit"/>
/// is <c>ranges_from_dit</c>, <see cref="Learn"/> is <c>learn</c>, <see cref="Text"/> is <c>to_text</c> and <see cref="Read(ReadOnlySpan{float}, int, double, double, double, int)"/>
/// is <c>cwhmm2.run</c>, three passes, each re-learning the sender's dit, dah, gap medians and his two levels from its own decode.
/// The one level in the whole reader is the Gaussian score of a hop against the sender's own key-down and key-up, learned.</para>
/// <para>**THE LIVE HALF** (the instance) lays a lane every <see cref="LaneSpacingHz"/> across the rig's passband, decodes each lane's
/// last <see cref="WindowSeconds"/> every <see cref="DecodeEverySeconds"/>, and commits the letters of the printed lane that end at
/// least <see cref="CommitLagSeconds"/> behind the newest audio. A lane prints only where its decode's pattern is Morse
/// (<see cref="Pattern"/>): no floor, no contrast line, no carrier or shadow rule stands in front of it.</para>
/// <para>**WHERE THE C# DEPARTS FROM THE PROTOTYPE, AND WHY, ONE LINE EACH:**</para>
/// <list type="bullet">
/// <item>the Butterworth is two RBJ biquads by the bilinear transform prewarped at the cutoff, the same filter <c>scipy.signal.butter</c>
/// designs, run per sample rather than over an array;</item>
/// <item>the predecessor maximum is taken once per hop for the two predecessor sets (any gap or the start; dit or dah), which is the
/// prototype's own maximum with its own tie order, computed once rather than per state;</item>
/// <item>the first pass's dit, where nobody gives one, is two-means on the run lengths above the lane's midpoint
/// (<see cref="GuessDitHops"/>), as the work instruction names: the prototype's <c>cwhmm2.py</c> takes it as an argument;</item>
/// <item>live, a lane that printed carries its learned rhythm and levels into its next window's first pass, so a sender's rhythm is
/// his across windows; a lane that did not starts from the dit guess, as the prototype does;</item>
/// <item>live, a mark whose mean level lies under the midpoint of the learned key-down and key-up is silence: the longest word gap
/// is two and a half of the sender's, so in a long pause the path must lay a mark somewhere, and that mark is not keying;</item>
/// <item>live, the lane's audio is not trimmed to a stretch: the window slides over everything the lane has heard.</item>
/// </list>
/// </remarks>
public sealed class CwSequenceReader
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>The envelope's hop, as the prototype's: 5 ms.</summary>
    public const double HopMs = 5;

    /// <summary>The passes, each re-learning the sender from its own decode, as the prototype's.</summary>
    public const int Passes = 3;

    /// <summary>The lanes' spacing across the passband, as the bank's.</summary>
    public const double LaneSpacingHz = 25;

    /// <summary>
    /// The lanes' low-pass cutoff: narrow, so the station beside drops out, and no narrower than a station halfway between two lanes
    /// loses under half a decibel at (a fourth-order Butterworth at 12.5 Hz off a 16 Hz cutoff gives 0.45 dB).
    /// </summary>
    public const double LaneBandwidthHz = 16;

    /// <summary>The window each lane is decoded over, live.</summary>
    public const double WindowSeconds = 12;

    /// <summary>How often the lanes are decoded, live.</summary>
    public const double DecodeEverySeconds = 2;

    /// <summary>How far behind the newest audio a letter must end before it is committed and printed, live.</summary>
    public const double CommitLagSeconds = 2;

    /// <summary>The least share of dahs among the marks, for the decode to be Morse: real text 0.34 to 0.42, nothing 0.09.</summary>
    public const double DahShareLeast = 0.2;

    /// <summary>The most the dahs may spread, their median absolute deviation over their median: real 0.07 to 0.09, nothing 0.12 to 0.19.</summary>
    public const double DahTightnessMost = 0.12;

    /// <summary>The least the learned key-down may stand over the key-up, in the sum of the two spreads: real 1.7 to 2.2, nothing 1.1.</summary>
    public const double SeparationLeast = 1.5;

    /// <summary>The least marks a window's decode must hold before its pattern is judged.</summary>
    public const int PatternLeastMarks = 16;

    /// <summary>The least a lane must have heard before it is decoded, live: a shorter window teaches the passes too little of the sender.</summary>
    public const double LeastWindowSeconds = 6;

    /// <summary>How long a printed lane may commit nothing before another lane that reads may take the terminal.</summary>
    public const double YieldSeconds = 4;

    /// <summary>The span either side of a lane within which another lane reading the same sequence is its shadow.</summary>
    public const double ShadowReachHz = 100;

    /// <summary>How near two marks' starts must sit to be the same mark, for the shadow test.</summary>
    public const double SameMarkSeconds = 0.03;

    private const double Neg = -1e18;

    // The states: dit, dah, inside gap, letter gap, word gap.
    internal const int Dit = 0;
    internal const int Dah = 1;
    internal const int InsideGap = 2;
    internal const int LetterGap = 3;
    internal const int WordGap = 4;

    private static readonly bool[] OnState = [true, true, false, false, false];

    // ---------------------------------------------------------------------------------------------------------------------------
    // The port: the prototype's functions.
    // ---------------------------------------------------------------------------------------------------------------------------

    /// <summary>One segment of a decoded sequence: its state and its hops, from inclusive, to exclusive.</summary>
    public readonly record struct Segment(int State, int From, int To)
    {
        /// <summary>Its length in hops.</summary>
        public int Length => To - From;

        /// <summary>Whether it is a mark, a dit or a dah.</summary>
        public bool IsMark => State <= Dah;
    }

    /// <summary>A state's duration range in hops, both ends inclusive.</summary>
    public readonly record struct Range(int Lo, int Hi);

    /// <summary>The two levels, key-down and key-up, each a median and a spread, in dB.</summary>
    public readonly record struct Levels(double Top, double SpreadOn, double Floor, double SpreadOff)
    {
        /// <inheritdoc />
        public override string ToString() => string.Create(Inv, $"top {Top:0.0}±{SpreadOn:0.0} floor {Floor:0.0}±{SpreadOff:0.0}");
    }

    /// <summary>What a decode taught: the sender's medians in hops and his levels, and the ranges the next pass takes.</summary>
    public sealed record Learned(double Dit, double Dah, double InsideGap, double LetterGap, double WordGap, Levels Levels, Range[] Ranges)
    {
        /// <inheritdoc />
        public override string ToString()
            => string.Create(Inv, $"dit {Dit * HopMs:0} dah {Dah * HopMs:0} ig {InsideGap * HopMs:0} lg {LetterGap * HopMs:0} wg {WordGap * HopMs:0} ms  {Levels}");
    }

    /// <summary>The pattern tests on a decode: whether someone is there.</summary>
    public readonly record struct PatternStats(int Marks, double DahShare, double DahTightness, double Separation)
    {
        /// <summary>Whether the decode's pattern is Morse.</summary>
        public bool IsMorse => Marks >= PatternLeastMarks && DahShare >= DahShareLeast && DahTightness <= DahTightnessMost && Separation >= SeparationLeast;

        /// <inheritdoc />
        public override string ToString()
            => string.Create(Inv, $"{Marks} marks, dah share {DahShare:0.00}, dah tightness {DahTightness:0.00}, separation {Separation:0.00}{(IsMorse ? ", Morse" : string.Empty)}");
    }

    /// <summary>A whole reading: the last pass's sequence, its text, each pass's learning, and the pattern.</summary>
    public sealed record Reading(IReadOnlyList<Segment> Segments, string Text, IReadOnlyList<Learned> Passes, Levels Levels, PatternStats Pattern);

    /// <summary>
    /// <c>envelope_db</c>: the audio mixed to the pitch, a fourth-order Butterworth low-pass on both quadratures, the amplitude in dB
    /// every hop.
    /// </summary>
    public static double[] Envelope(ReadOnlySpan<float> samples, int rate, double pitchHz, double bandwidthHz, double hopMs = HopMs)
    {
        var lane = new LaneFilter(pitchHz, rate, bandwidthHz);
        var hop = (int)(rate * hopMs / 1000);
        var db = new double[(samples.Length + hop - 1) / hop];

        for (var n = 0; n < samples.Length; n++)
        {
            var amplitude = lane.Push(samples[n]);

            // env[::hop]: the first sample and every hop after it.
            if (n % hop == 0)
            {
                db[n / hop] = 20 * Math.Log10(amplitude + 1e-9);
            }
        }

        return db;
    }

    /// <summary><c>ranges_from_dit</c>: each state's range in hops from a dit.</summary>
    public static Range[] RangesFromDit(double d)
        => Rounded([(0.5 * d, 1.6 * d), (2.0 * d, 4.5 * d), (0.4 * d, 1.7 * d), (1.8 * d, 5.0 * d), (5.0 * d, 14 * d)]);

    /// <summary>The first pass's levels where nobody learned any: the 90th and 20th percentiles, spread 4 dB, as the prototype's.</summary>
    public static Levels LevelsFromPercentiles(ReadOnlySpan<double> db)
        => new(Percentile(db, 90), 4.0, Percentile(db, 20), 4.0);

    /// <summary>
    /// <c>decode</c>: the explicit-duration Viterbi. For each end hop and each state, the best start over the state's duration range
    /// from the allowed predecessors, each segment scored in O(1) from cumulative emission sums, with a flat prior of −log(range).
    /// </summary>
    public static List<Segment> Decode(ReadOnlySpan<double> db, IReadOnlyList<Range> rng, Levels levels)
    {
        var n = db.Length;
        var cOn = new double[n + 1];
        var cOff = new double[n + 1];

        for (var k = 0; k < n; k++)
        {
            var on = (db[k] - levels.Top) / levels.SpreadOn;
            var off = (db[k] - levels.Floor) / levels.SpreadOff;

            cOn[k + 1] = cOn[k] - (0.5 * on * on);
            cOff[k + 1] = cOff[k] - (0.5 * off * off);
        }

        var best = new double[5][];
        var backState = new sbyte[5][];
        var backStart = new int[5][];

        for (var s = 0; s < 5; s++)
        {
            best[s] = new double[n + 1];
            backState[s] = new sbyte[n + 1];
            backStart[s] = new int[n + 1];
            Array.Fill(best[s], Neg);
        }

        // The predecessors' best at each start hop, taken once: a mark follows any gap or the start (2, 3, 4, -1, first wins a
        // tie, as the prototype's strict comparison in list order); an inside or letter gap follows a dit or a dah (0, 1); a word
        // gap follows a dit, a dah or the start (0, 1, -1).
        var afterGap = new double[n + 1];
        var afterGapState = new sbyte[n + 1];
        var afterMark = new double[n + 1];
        var afterMarkState = new sbyte[n + 1];
        var prior = new double[5];

        for (var s = 0; s < 5; s++)
        {
            prior[s] = Math.Log(rng[s].Hi - rng[s].Lo + 1);
        }

        afterGap[0] = 0;
        afterGapState[0] = -1;
        afterMark[0] = Neg;
        afterMarkState[0] = -1;

        for (var j = 1; j <= n; j++)
        {
            for (var s = 0; s < 5; s++)
            {
                var (lo, hi) = (rng[s].Lo, rng[s].Hi);

                if (j < lo)
                {
                    continue;
                }

                var c = OnState[s] ? cOn : cOff;
                var top = Math.Min(hi, j);
                var bestTotal = double.NegativeInfinity;
                var bestStart = -1;
                sbyte bestPrev = -1;

                for (var d = lo; d <= top; d++)
                {
                    var i = j - d;
                    double prev;
                    sbyte ps;

                    if (s <= Dah)
                    {
                        prev = afterGap[i];
                        ps = afterGapState[i];
                    }
                    else if (s == WordGap)
                    {
                        prev = afterMark[i];
                        ps = afterMarkState[i];

                        if (i == 0 && 0.0 > prev)
                        {
                            prev = 0;
                            ps = -1;
                        }
                    }
                    else
                    {
                        prev = i == 0 ? Neg : afterMark[i];
                        ps = i == 0 ? (sbyte)-1 : afterMarkState[i];
                    }

                    var total = prev + (c[j] - c[i]) - prior[s];

                    // np.argmax: the first maximum, the shortest duration.
                    if (total > bestTotal)
                    {
                        bestTotal = total;
                        bestStart = i;
                        bestPrev = ps;
                    }
                }

                if (bestStart >= 0 && bestTotal > Neg / 2)
                {
                    best[s][j] = bestTotal;
                    backState[s][j] = bestPrev;
                    backStart[s][j] = bestStart;
                }
            }

            // The predecessors' best at j, for every segment starting here.
            var g = Neg;
            sbyte gs = -1;

            foreach (var p in new[] { InsideGap, LetterGap, WordGap })
            {
                if (best[p][j] > g)
                {
                    g = best[p][j];
                    gs = (sbyte)p;
                }
            }

            afterGap[j] = g;
            afterGapState[j] = gs;

            var m = Neg;
            sbyte ms = -1;

            foreach (var p in new[] { Dit, Dah })
            {
                if (best[p][j] > m)
                {
                    m = best[p][j];
                    ms = (sbyte)p;
                }
            }

            afterMark[j] = m;
            afterMarkState[j] = ms;
        }

        // The end: the best state at the last hop, then back.
        var state = 0;

        for (var s = 1; s < 5; s++)
        {
            if (best[s][n] > best[state][n])
            {
                state = s;
            }
        }

        var segs = new List<Segment>();
        var at = n;

        while (at > 0)
        {
            var p = backState[state][at];
            var i = backStart[state][at];

            segs.Add(new Segment(state, i, at));
            state = p;
            at = i;

            if (state < 0)
            {
                break;
            }
        }

        segs.Reverse();
        return segs;
    }

    /// <summary>
    /// <c>learn</c>: the sender's dit, dah, inside gap, letter gap and word gap medians from a decode, the class boundaries at the
    /// midpoints between them, and his key-down and key-up levels from the decode's own on and off stretches; null under three dits.
    /// </summary>
    public static Learned? Learn(IReadOnlyList<Segment> segs, ReadOnlySpan<double> db)
    {
        var lengths = new List<double>[5];

        for (var s = 0; s < 5; s++)
        {
            lengths[s] = segs.Where(g => g.State == s).Select(g => (double)g.Length).ToList();
        }

        double? Med(int s, double? fallback) => lengths[s].Count >= 3 ? Median(lengths[s]) : fallback;

        if (Med(Dit, null) is not { } d)
        {
            return null;
        }

        var dah = Med(Dah, 3 * d)!.Value;
        var ig = Med(InsideGap, d)!.Value;
        var lg = Med(LetterGap, 3 * d)!.Value;
        var wg = Med(WordGap, 7 * d)!.Value;

        var r = Rounded([(0.55 * d, (d + dah) / 2), ((d + dah) / 2, 1.8 * dah), (0.4 * ig, (ig + lg) / 2), ((ig + lg) / 2, (lg + wg) / 2), ((lg + wg) / 2, 2.5 * wg)]);

        var on = new List<double>();
        var off = new List<double>();

        foreach (var g in segs)
        {
            var into = g.IsMark ? on : off;

            for (var k = g.From; k < g.To; k++)
            {
                into.Add(db[k]);
            }
        }

        if (on.Count == 0 || off.Count == 0)
        {
            return null;
        }

        var top = Median(on);
        var floor = Median(off);
        var levels = new Levels(top, Math.Max(2.0, 1.4826 * Median(on.Select(v => Math.Abs(v - top)).ToList())), floor, Math.Max(2.0, 1.4826 * Median(off.Select(v => Math.Abs(v - floor)).ToList())));

        return new Learned(d, dah, ig, lg, wg, levels, r);
    }

    /// <summary>
    /// <c>run</c>: the passes over one stretch of envelope, from the ranges and levels given; each pass decodes, then learns the
    /// ranges and levels the next takes. The reading is the last pass's decode.
    /// </summary>
    public static Reading Read(ReadOnlySpan<double> db, Range[] ranges, Levels levels, int passes = Passes)
    {
        var learned = new List<Learned>();
        List<Segment> segs = [];

        for (var it = 0; it < passes; it++)
        {
            segs = Decode(db, ranges, levels);

            if (Learn(segs, db) is not { } l)
            {
                break;
            }

            learned.Add(l);
            ranges = l.Ranges;
            levels = l.Levels;
        }

        return new Reading(segs, Text(segs), learned, levels, Pattern(segs, levels));
    }

    /// <summary>The dits, in hops, a cold lane's passes start from beside its own guess: 40, 70 and 120 ms, across Morse's range.</summary>
    public static readonly int[] ColdStartDits = [8, 14, 24];

    /// <summary>
    /// **WHERE NOBODY KNOWS THE SENDER YET, THE START THAT EXPLAINS THE ENVELOPE BEST** (round two): the passes begin from the dit
    /// guess and from each of <see cref="ColdStartDits"/>, one pass each; the start whose decode best explains the envelope under the
    /// levels it learned (<see cref="LogLikelihood"/>) takes the remaining passes. Traced on the owner's recordings: from a guess at its
    /// 25 ms floor the passes settled on dit 40 and dah 80 ms with one-hop inside gaps and read every dit as a dah, where the same
    /// window from a 95 ms start read <c>NF C HAT</c>.
    /// </summary>
    public static Reading ReadCold(ReadOnlySpan<double> db, int passes = Passes)
    {
        var levels = LevelsFromPercentiles(db);
        Reading? best = null;
        var bestScore = double.NegativeInfinity;

        foreach (var dit in new[] { GuessDitHops(db) }.Concat(ColdStartDits).Distinct())
        {
            var first = Read(db, RangesFromDit(dit), levels, passes: 1);

            if (first.Passes.Count == 0)
            {
                best ??= first;
                continue;
            }

            var score = LogLikelihood(first.Segments, db, first.Levels);

            if (score > bestScore)
            {
                bestScore = score;
                best = first;
            }
        }

        if (best is null || best.Passes.Count == 0 || passes <= 1)
        {
            return best!;
        }

        var rest = Read(db, best.Passes[^1].Ranges, best.Levels, passes - 1);

        return rest with { Passes = best.Passes.Concat(rest.Passes).ToList() };
    }

    /// <summary>How well a decode explains the envelope: the Gaussian log-likelihood of every hop under its state's level and spread.</summary>
    public static double LogLikelihood(IReadOnlyList<Segment> segs, ReadOnlySpan<double> db, Levels levels)
    {
        var sum = 0.0;

        foreach (var g in segs)
        {
            var (mu, s) = g.IsMark ? (levels.Top, levels.SpreadOn) : (levels.Floor, levels.SpreadOff);

            for (var k = g.From; k < g.To; k++)
            {
                var z = (db[k] - mu) / s;

                sum -= (0.5 * z * z) + Math.Log(s);
            }
        }

        return sum;
    }

    /// <summary><c>cwhmm2.run</c> on audio: the envelope at a pitch and bandwidth, the first ranges from a dit guess in milliseconds.</summary>
    public static Reading Read(ReadOnlySpan<float> samples, int rate, double pitchHz, double bandwidthHz, double ditMs, int passes = Passes)
    {
        var db = Envelope(samples, rate, pitchHz, bandwidthHz);

        return Read(db, RangesFromDit((int)Math.Round(ditMs / HopMs)), LevelsFromPercentiles(db), passes);
    }

    /// <summary><c>to_text</c>: the dits and dahs between letter gaps looked up, a word gap a space, an unknown pattern the placeholder.</summary>
    public static string Text(IReadOnlyList<Segment> segs)
    {
        var sb = new System.Text.StringBuilder();
        var cur = new System.Text.StringBuilder();

        void Letter()
        {
            if (cur.Length > 0)
            {
                sb.Append(MorseAlphabet.Lookup(cur.ToString()) ?? MorseAlphabet.Unreadable);
            }

            cur.Clear();
        }

        foreach (var g in segs)
        {
            switch (g.State)
            {
                case Dit:
                    cur.Append('.');
                    break;
                case Dah:
                    cur.Append('-');
                    break;
                case LetterGap:
                    Letter();
                    break;
                case WordGap:
                    Letter();
                    sb.Append(' ');
                    break;
            }
        }

        Letter();
        return sb.ToString().Replace("  ", " ", StringComparison.Ordinal);
    }

    /// <summary>
    /// **SOMEONE IS THERE** (section 2.6): the share of dahs among the marks, the dahs' spread (median absolute deviation over the
    /// median), and the learned key-down over the key-up in the sum of the two spreads. Pattern tests on the decode, not a level.
    /// </summary>
    public static PatternStats Pattern(IReadOnlyList<Segment> segs, Levels levels)
    {
        var dits = segs.Count(g => g.State == Dit);
        var dahs = segs.Where(g => g.State == Dah).Select(g => (double)g.Length).ToList();
        var marks = dits + dahs.Count;
        var share = marks == 0 ? 0 : dahs.Count / (double)marks;
        var tight = double.PositiveInfinity;

        if (dahs.Count >= 3)
        {
            var med = Median(dahs);

            tight = Median(dahs.Select(v => Math.Abs(v - med)).ToList()) / med;
        }

        var separation = (levels.Top - levels.Floor) / (levels.SpreadOn + levels.SpreadOff);

        return new PatternStats(marks, share, tight, separation);
    }

    /// <summary>
    /// The first pass's dit where nobody gives one: the runs above the midpoint of the 90th and 20th percentiles, parted by two-means
    /// in log length, the shorter cluster's centre; clamped to Morse's fastest and slowest dit. A run opens above 60 per cent of the
    /// way from the 20th to the 90th and closes under 40, so the noise's flicker about the midpoint is not a run: measured on Ken's
    /// stretch of <c>cw-2026-10-19-141708</c>, a run at the midpoint itself made the flicker the shorter cluster and the guess sat
    /// at its 25 ms floor, from which the passes settled on 5 ms inside gaps.
    /// </summary>
    public static int GuessDitHops(ReadOnlySpan<double> db)
    {
        const int fastest = 5; // 25 ms, 48 WPM
        const int slowest = 40; // 200 ms, 6 WPM
        var low = Percentile(db, 20);
        var high = Percentile(db, 90);
        var up = low + (0.6 * (high - low));
        var down = low + (0.4 * (high - low));
        var runs = new List<double>();
        var length = 0;

        foreach (var v in db)
        {
            if (length > 0 ? v > down : v > up)
            {
                length++;
            }
            else if (length > 0)
            {
                runs.Add(Math.Log(length));
                length = 0;
            }
        }

        runs.RemoveAll(r => r < Math.Log(2));

        if (runs.Count < 4)
        {
            return 12;
        }

        var lo = runs.Min();
        var hi = runs.Max();

        for (var it = 0; it < 20 && hi > lo; it++)
        {
            var split = (lo + hi) / 2;
            var a = runs.Where(r => r <= split).ToList();
            var b = runs.Where(r => r > split).ToList();

            if (a.Count == 0 || b.Count == 0)
            {
                break;
            }

            lo = a.Average();
            hi = b.Average();
        }

        // One cluster only: the runs are mostly dits.
        var dit = hi - lo < Math.Log(1.5) ? Math.Exp(Median(runs)) : Math.Exp(lo);

        return (int)Math.Clamp(Math.Round(dit), fastest, slowest);
    }

    internal static double Median(IReadOnlyList<double> values)
    {
        var sorted = values.Order().ToArray();
        var n = sorted.Length;

        return n % 2 == 1 ? sorted[n / 2] : (sorted[(n / 2) - 1] + sorted[n / 2]) / 2;
    }

    // np.percentile, linear.
    internal static double Percentile(ReadOnlySpan<double> values, double q)
    {
        var sorted = values.ToArray();

        Array.Sort(sorted);

        var pos = q / 100 * (sorted.Length - 1);
        var below = (int)Math.Floor(pos);
        var above = Math.Min(sorted.Length - 1, below + 1);

        return sorted[below] + ((pos - below) * (sorted[above] - sorted[below]));
    }

    private static Range[] Rounded((double Lo, double Hi)[] r)
        => r.Select(x => new Range(Math.Max(1, (int)Math.Round(x.Lo)), Math.Max(2, (int)Math.Round(x.Hi)))).ToArray();

    // ---------------------------------------------------------------------------------------------------------------------------
    // Live: lanes across the passband, decoded in windows, the printed lane's letters committed behind the newest audio.
    // ---------------------------------------------------------------------------------------------------------------------------

    private readonly int _rate;
    private readonly int _hopSamples;
    private readonly int _windowHops;
    private readonly int _everyHops;
    private readonly int _lagHops;
    private readonly Lane[] _lanes;
    private readonly List<CwSymbol> _out = [];
    private int _hopFill;
    private long _hops;
    private Lane? _printed;
    private long _lastPrintedEnd = long.MinValue / 2;
    private bool _printedAny;

    /// <summary>Lanes every <see cref="LaneSpacingHz"/> across a passband.</summary>
    /// <param name="sampleRate">The audio's rate.</param>
    /// <param name="lowHz">The passband's low edge.</param>
    /// <param name="highHz">The passband's high edge.</param>
    public CwSequenceReader(int sampleRate, double lowHz, double highHz)
    {
        _rate = sampleRate;
        _hopSamples = Math.Max(1, (int)(sampleRate * HopMs / 1000));
        _windowHops = (int)(WindowSeconds * 1000 / HopMs);
        _everyHops = (int)(DecodeEverySeconds * 1000 / HopMs);
        _lagHops = (int)(CommitLagSeconds * 1000 / HopMs);

        var first = (int)Math.Ceiling(lowHz / LaneSpacingHz);
        var last = (int)Math.Floor(highHz / LaneSpacingHz);

        _lanes = Enumerable.Range(first, Math.Max(0, last - first + 1))
            .Select(k => new Lane(k * LaneSpacingHz, sampleRate, LaneBandwidthHz, _windowHops + _everyHops))
            .ToArray();
    }

    /// <summary>How many lanes it holds.</summary>
    public int Lanes => _lanes.Length;

    /// <summary>The pitch of the lane printing now, or NaN.</summary>
    public double PrintedPitchHz => _printed?.PitchHz ?? double.NaN;

    /// <summary>Where set, a line per decode tick and per commit, for the tests' traces.</summary>
    internal List<string>? Trace { get; set; }

    /// <summary>What each lane read at its last decode: pitch, pattern, learned, text; for the tests' traces.</summary>
    internal IEnumerable<(double PitchHz, PatternStats Pattern, Learned? Learned, string Text)> LaneReadings
        => _lanes.Select(l => (l.PitchHz, l.Pattern, l.Learned, l.Segments is null ? string.Empty : Text(l.Segments)));

    /// <summary>Hear one sample.</summary>
    public void Push(float sample)
    {
        var take = _hopFill == 0;

        foreach (var lane in _lanes)
        {
            var amplitude = lane.Filter.Push(sample);

            if (take)
            {
                lane.Add(20 * Math.Log10(amplitude + 1e-9));
            }
        }

        if (take)
        {
            _hops++;

            if (_hops % _everyHops == 0)
            {
                Tick(final: false);
            }
        }

        _hopFill = (_hopFill + 1) % _hopSamples;
    }

    /// <summary>The symbols committed since the last call, for the lookup table.</summary>
    /// <param name="final">True where the audio is over: every lane is decoded once more and everything committed.</param>
    public IReadOnlyList<CwSymbol> Take(bool final)
    {
        if (final)
        {
            Tick(final: true);
        }

        var symbols = _out.ToArray();

        _out.Clear();
        return symbols;
    }

    private double Seconds(long hop) => hop * _hopSamples / (double)_rate;

    private void Tick(bool final)
    {
        var newest = _hops;

        foreach (var lane in _lanes)
        {
            lane.Decode(newest, _windowHops);
        }

        // Shadows: a lane reading the same sequence as a better-separated lane within reach is not a station; the printed lane is
        // judged first, so a station between two lanes is not handed back and forth between them.
        var reading = _lanes.Where(l => l.Pattern.IsMorse).OrderBy(l => l == _printed ? 0 : 1).ThenByDescending(l => l.Pattern.Separation).ToList();
        var stations = new List<Lane>();

        foreach (var lane in reading)
        {
            if (!stations.Any(s => Math.Abs(s.PitchHz - lane.PitchHz) <= ShadowReachHz && SameSequence(s, lane)))
            {
                stations.Add(lane);
            }
        }

        var limit = final ? newest : newest - _lagHops;

        // The printed lane keeps the terminal while it reads and has committed something lately; else the best-separated station
        // with marks lately takes it, printing from after the last letter printed.
        if (_printed is not null && (!stations.Contains(_printed) || newest - _printed.LastCommitted > YieldSeconds * 1000 / HopMs))
        {
            var other = stations.Where(s => s != _printed && s.HasMarksAfter(Math.Max(_lastPrintedEnd, newest - _windowHops))).FirstOrDefault();

            if (other is not null || !stations.Contains(_printed))
            {
                Trace?.Add(string.Create(Inv, $"{Seconds(newest):0.00} s let go {_printed.PitchHz:0} Hz{(other is null ? string.Empty : string.Create(Inv, $" for {other.PitchHz:0} Hz"))}"));
                _printed = other;
                Hand(other);
            }
        }

        if (_printed is null && stations.FirstOrDefault(s => s.HasMarksAfter(Math.Max(_lastPrintedEnd, newest - _windowHops))) is { } chosen)
        {
            Trace?.Add(string.Create(Inv, $"{Seconds(newest):0.00} s holds {chosen.PitchHz:0} Hz, {chosen.Pattern}, {chosen.Learned}"));
            _printed = chosen;
            Hand(chosen);
        }

        if (_printed is not null)
        {
            Commit(_printed, limit, newest);
        }
    }

    // A lane taking the terminal prints nothing from before the last letter printed.
    private void Hand(Lane? lane)
    {
        if (lane is not null && _lastPrintedEnd > lane.Committed)
        {
            lane.Committed = _lastPrintedEnd;
        }

        if (lane is not null)
        {
            lane.LastCommitted = _hops;
        }
    }

    private static bool SameSequence(Lane a, Lane b)
    {
        var sa = a.MarkStarts();
        var sb = b.MarkStarts();

        if (sa.Count == 0 || sb.Count == 0)
        {
            return false;
        }

        var near = SameMarkSeconds * 1000 / HopMs;
        var shared = sb.Count(x => sa.Any(y => Math.Abs(x - y) <= near));

        return shared >= 2.0 / 3 * Math.Min(sa.Count, sb.Count);
    }

    // The printed lane's letters that end before the limit and begin after what it last committed.
    private void Commit(Lane lane, long limit, long newest)
    {
        if (lane.Segments is not { } segs || lane.Learned is not { } learned)
        {
            return;
        }

        var levels = lane.Levels;
        var mid = (levels.Top + levels.Floor) / 2;
        var origin = lane.DecodedFrom;

        // Marks under the midpoint of the two levels are silence; consecutive gaps merge, and a merged gap is a word gap.
        var items = new List<(int State, long From, long To, double Mean)>();

        foreach (var g in segs)
        {
            var from = origin + g.From;
            var to = origin + g.To;
            var mean = g.IsMark ? lane.Mean(from, to) : double.NaN;
            var state = g.IsMark && mean < mid ? WordGap : g.State;

            if (!g.IsMark || state == WordGap)
            {
                if (items.Count > 0 && items[^1].State >= InsideGap)
                {
                    var prev = items[^1];

                    items[^1] = (prev.State == state ? state : WordGap, prev.From, to, double.NaN);
                    continue;
                }
            }

            items.Add((state, from, to, mean));
        }

        // Letters: marks between letter or word gaps.
        var k = 0;

        while (k < items.Count)
        {
            if (items[k].State > Dah)
            {
                k++;
                continue;
            }

            var start = k;

            while (k < items.Count && (items[k].State <= Dah || items[k].State == InsideGap))
            {
                k++;
            }

            // The letter is its marks from start to the last mark before k.
            var marks = items.Skip(start).Take(k - start).Where(x => x.State <= Dah).ToList();
            var closed = k < items.Count;
            var letterFrom = marks[0].From;
            var letterTo = marks[^1].To;

            // A letter already committed, or one the window's start cut into, is not printed again.
            if (letterFrom < lane.Committed - 1 || (start == 0 && origin > 0))
            {
                continue;
            }

            if (letterTo > limit || (!closed && limit < newest))
            {
                break;
            }

            var before = start > 0 ? items[start - 1] : default;
            var space = _printedAny && (start == 0 || before.State == WordGap || letterFrom - _lastPrintedEnd > 7 * learned.Dit * 1.5);
            var wpm = (int)Math.Round(1200 / (learned.Dit * HopMs));

            if (space)
            {
                _out.Add(new CwSymbol(CwSymbolKind.WordEnd)
                {
                    Reading = new CwCharacter(MorseAlphabet.WordGap, CwConfidence.High, 1, string.Empty, double.NaN, wpm, TimeSpan.FromSeconds(Seconds(letterFrom)))
                    {
                        Stage = CwReadingStage.Settled,
                    },
                });
            }

            var cwMarks = new List<CwMark>();

            foreach (var m in marks)
            {
                _out.Add(new CwSymbol(m.State == Dah ? CwSymbolKind.Dash : CwSymbolKind.Dot));
                cwMarks.Add(new CwMark(0, Seconds(m.From), Seconds(m.To), lane.PitchHz, m.Mean, m.Mean - levels.Floor) { Keyed = true, Stood = true });
            }

            // How sure: the most doubtful mark's mean between the midpoint (nought) and the key-down (one).
            var score = marks.Min(m => Math.Clamp((m.Mean - mid) / Math.Max(1e-6, levels.Top - mid), 0, 1));
            var pattern = string.Concat(marks.Select(m => m.State == Dah ? "-" : "."));

            _out.Add(new CwSymbol(CwSymbolKind.LetterEnd)
            {
                Reading = new CwCharacter(MorseAlphabet.Unreadable, score >= 0.5 ? CwConfidence.High : CwConfidence.Low, score, pattern, levels.Top - levels.Floor, wpm, TimeSpan.FromSeconds(Seconds(letterTo)))
                {
                    Stage = CwReadingStage.Settled,
                    SpanHops = (int)(letterTo - letterFrom),
                },
                Marks = cwMarks,
            });

            Trace?.Add(string.Create(Inv, $"{Seconds(newest):0.00} s commits {pattern} at {Seconds(letterFrom):0.00}-{Seconds(letterTo):0.00} s from {lane.PitchHz:0} Hz{(space ? ", a space before" : string.Empty)}"));

            lane.Committed = letterTo;
            lane.LastCommitted = newest;
            _lastPrintedEnd = letterTo;
            _printedAny = true;
        }
    }

    // One lane: its filter, its envelope history, and its last decode.
    private sealed class Lane(double pitchHz, int rate, double bandwidthHz, int keep)
    {
        private readonly List<double> _db = [];

        public double PitchHz { get; } = pitchHz;

        public LaneFilter Filter { get; } = new(pitchHz, rate, bandwidthHz);

        // The absolute hop of _db[0].
        public long Origin { get; private set; }

        public List<Segment>? Segments { get; private set; }

        public Learned? Learned { get; private set; }

        public Levels Levels { get; private set; }

        public PatternStats Pattern { get; private set; }

        // The end of the last letter committed from this lane, in absolute hops.
        public long Committed { get; set; } = long.MinValue / 2;

        // The hop at which this lane last committed a letter, or was handed the terminal.
        public long LastCommitted { get; set; }

        // The absolute hop the last decode's segments start at.
        private long _decodedFrom;

        public long DecodedFrom => _decodedFrom;

        public void Add(double db)
        {
            _db.Add(db);

            if (_db.Count > keep)
            {
                var drop = _db.Count - keep;

                _db.RemoveRange(0, drop);
                Origin += drop;
            }
        }

        public double Mean(long from, long to)
        {
            var a = (int)Math.Max(0, from - Origin);
            var b = (int)Math.Min(_db.Count, to - Origin);

            if (b <= a)
            {
                return double.NaN;
            }

            var sum = 0.0;

            for (var k = a; k < b; k++)
            {
                sum += _db[k];
            }

            return sum / (b - a);
        }

        public void Decode(long newest, int window)
        {
            var count = (int)Math.Min(window, _db.Count);

            if (count < LeastWindowSeconds * 1000 / HopMs)
            {
                Segments = null;
                Pattern = default;
                return;
            }

            var span = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_db)[^count..];
            // A lane that read a sender carries his rhythm; a cold lane tries its starts (round two).
            var reading = Learned is { } l && Pattern.IsMorse ? Read(span, l.Ranges, l.Levels) : ReadCold(span);
            var shift = newest - count;

            // Segments are kept relative to the window; Origin for Commit is the window's first hop.
            Segments = reading.Segments.Select(g => g).ToList();
            _decodedFrom = shift;
            Learned = reading.Passes.Count > 0 ? reading.Passes[^1] : null;
            Levels = reading.Levels;
            Pattern = reading.Pattern;
        }

        public List<long> MarkStarts()
            => Segments is null ? [] : Segments.Where(g => g.IsMark).Select(g => _decodedFrom + g.From).ToList();

        public bool HasMarksAfter(long hop)
            => Segments is not null && Segments.Any(g => g.IsMark && _decodedFrom + g.From >= hop && Mean(_decodedFrom + g.From, _decodedFrom + g.To) >= (Levels.Top + Levels.Floor) / 2);
    }

    /// <summary>A mixer and a fourth-order Butterworth low-pass on both quadratures; the amplitude.</summary>
    internal sealed class LaneFilter
    {
        private static readonly double[] SectionQ = [1 / (2 * Math.Sin(Math.PI / 8)), 1 / (2 * Math.Sin(3 * Math.PI / 8))];
        private readonly double _cosStep;
        private readonly double _sinStep;
        private readonly Biquad[] _i = [new(), new()];
        private readonly Biquad[] _q = [new(), new()];
        private double _c = 1;
        private double _s;
        private int _renorm;

        public LaneFilter(double hz, double rate, double bandwidthHz)
        {
            _cosStep = Math.Cos(2 * Math.PI * hz / rate);
            _sinStep = Math.Sin(2 * Math.PI * hz / rate);

            for (var k = 0; k < 2; k++)
            {
                _i[k].Design(bandwidthHz, SectionQ[k], rate);
                _q[k].Design(bandwidthHz, SectionQ[k], rate);
            }
        }

        // x · e^{-j2πft}: the real part x·cos, the imaginary −x·sin.
        public double Push(double x)
        {
            var i = _i[1].Next(_i[0].Next(x * _c));
            var q = _q[1].Next(_q[0].Next(-x * _s));
            var c = (_c * _cosStep) - (_s * _sinStep);
            var s = (_s * _cosStep) + (_c * _sinStep);

            _c = c;
            _s = s;

            if (++_renorm == 4096)
            {
                var r = Math.Sqrt((_c * _c) + (_s * _s));

                _c /= r;
                _s /= r;
                _renorm = 0;
            }

            return Math.Sqrt((i * i) + (q * q));
        }

        private sealed class Biquad
        {
            private double _b0, _b1, _b2, _a1, _a2;
            private double _x1, _x2, _y1, _y2;

            public void Design(double cutoffHz, double q, double sampleRate)
            {
                var w0 = 2 * Math.PI * cutoffHz / sampleRate;
                var alpha = Math.Sin(w0) / (2 * q);
                var cos = Math.Cos(w0);
                var a0 = 1 + alpha;

                _b0 = (1 - cos) / 2 / a0;
                _b1 = (1 - cos) / a0;
                _b2 = (1 - cos) / 2 / a0;
                _a1 = -2 * cos / a0;
                _a2 = (1 - alpha) / a0;
            }

            public double Next(double x)
            {
                var y = (_b0 * x) + (_b1 * _x1) + (_b2 * _x2) - (_a1 * _y1) - (_a2 * _y2);

                _x2 = _x1;
                _x1 = x;
                _y2 = _y1;
                _y1 = y;

                return y;
            }
        }
    }
}

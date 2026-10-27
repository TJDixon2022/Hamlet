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
/// <item>**a sixth state, nobody keying** (work instruction 589, change 1): key-up level, from one word gap up with no upper bound,
/// entered from a dit, a dah or the start and left to a dit or a dah, so a long pause is explained as silence instead of a mark laid
/// in it; it ends a letter and a word as a word gap does, and nothing prints from it;</item>
/// <item>live, a lane is filtered at three widths and decodes at the one its sender's dit asks for (<see cref="LaneHzPerDit"/>,
/// change 2); the prototype's lane is a width given by hand;</item>
/// <item>live, a lane that read Morse keeps its learned rhythm and levels for <see cref="KeepRhythmSeconds"/> (change 3), so a
/// sender's rhythm is his across windows and pauses; a lane that did not starts cold, from several dits;</item>
/// <item>live, the pattern is judged over the lane's last <see cref="PatternSeconds"/> of keying, not over one window (change 3);</item>
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
    /// **THE LANE IS SIZED TO THE SPEED** (work instruction 589, change 2): a lane's low-pass cutoff is this over the sender's dit in
    /// seconds, so its rise is about a third of his dit. Traced at a single 16 Hz: W1AW at 32 WPM (a 37 ms dit) and KM3STU at 23 WPM
    /// (52 ms) had their inside gaps filled by the lane's own rise, the key-up spread to 9 and 10 dB, and the separation sat at the
    /// bar.
    /// </summary>
    public const double LaneHzPerDit = 1.5;

    /// <summary>
    /// The widths every lane is filtered at, each the cutoff <see cref="LaneHzPerDit"/> gives a dit of 100, 62 and 37.5 ms (12, 19 and
    /// 32 WPM); a lane decodes at the one nearest its sender's dit. A tone halfway between two lanes, 12.5 Hz off, loses 0.91 dB at 15
    /// Hz, 0.02 dB at 24 and nothing at 40.
    /// </summary>
    public static readonly double[] LaneWidthsHz = [15, 24, 40];

    /// <summary>The width, as an index into <see cref="LaneWidthsHz"/>, nearest in ratio to <see cref="LaneHzPerDit"/> over a dit in hops.</summary>
    public static int WidthFor(double ditHops)
    {
        var want = LaneHzPerDit / (ditHops * HopMs / 1000);

        return Enumerable.Range(0, LaneWidthsHz.Length).MinBy(k => Math.Abs(Math.Log(LaneWidthsHz[k] / want)));
    }

    /// <summary>The window each lane is decoded over, live.</summary>
    public const double WindowSeconds = 12;

    /// <summary>How often the lanes are decoded, live.</summary>
    public const double DecodeEverySeconds = 2;

    /// <summary>How far behind the newest audio a letter must end before it is committed and printed, live.</summary>
    public const double CommitLagSeconds = 2;

    /// <summary>The least share of dahs among the marks, for the decode to be Morse: real text 0.34 to 0.42, nothing 0.09.</summary>
    public const double DahShareLeast = 0.2;

    /// <summary>
    /// The most share of dahs among the marks, for the decode to be Morse (work instruction 589, round two): real text 0.34 to 0.42,
    /// the fist bank's ragchew 0.47. Traced on the 12 WPM CQ and W1AW's 14 WPM opening: a lane 25 Hz off the station learned a
    /// rhythm from noise in the narrow width, read the station at 40 Hz with it, split every element into dahs, and held the terminal
    /// at a dah share of 1.00.
    /// </summary>
    public const double DahShareMost = 0.65;

    /// <summary>The most the dahs may spread, their median absolute deviation over their median: real 0.07 to 0.09, nothing 0.12 to 0.19.</summary>
    public const double DahTightnessMost = 0.12;

    /// <summary>The least the learned key-down may stand over the key-up, in the sum of the two spreads: real 1.7 to 2.2, nothing 1.1.</summary>
    public const double SeparationLeast = 1.5;

    /// <summary>
    /// How much each learned gap must stand over the one under it, inside gap to letter gap to word gap (work instruction 590,
    /// change 1): Morse's own steps are three and seven thirds, and a hand's letter gaps run down toward twice his inside gap.
    /// </summary>
    public const double GapStep = 1.4;

    /// <summary>
    /// The least a learned inside gap may be, in the learned dit: a hand's runs from about two thirds of his dit to past it, and an
    /// inside gap of one hop under a 50 ms dit is a mark in pieces, not keying (traced on W1AW's piece-02, a lane 25 Hz off him).
    /// </summary>
    public const double InsideGapLeast = 0.4;

    /// <summary>Whether a decode's learned gaps climb, inside gap under letter gap under word gap, each by <see cref="GapStep"/>.</summary>
    public static bool GapsClimb(Learned learned) => RhythmHolds(learned.Dit, learned.InsideGap, learned.LetterGap, learned.WordGap);

    /// <summary>
    /// How far a warm decode's pattern may fall from the one its lane kept before the lane reads the window cold as well (work
    /// instruction 590, change 3): its separation by this share of the kept, or its dah share by this much.
    /// </summary>
    public const double PatternDrop = 0.25;

    /// <summary>Whether a warm decode's pattern has fallen from the lane's kept one by <see cref="PatternDrop"/>.</summary>
    public static bool Falls(PatternStats warm, PatternStats kept)
        => kept.Marks > 0 && (warm.Separation < (1 - PatternDrop) * kept.Separation || Math.Abs(warm.DahShare - kept.DahShare) > PatternDrop);

    /// <summary>Whether one pattern stands better than another: wider separation, its dahs no looser.</summary>
    public static bool Better(PatternStats a, PatternStats b) => a.Separation > b.Separation && a.DahTightness <= b.DahTightness + 0.02;

    /// <summary>The rhythm test: the inside gap at least <see cref="InsideGapLeast"/> of the dit, each gap after it by <see cref="GapStep"/>.</summary>
    public static bool RhythmHolds(double dit, double insideGap, double letterGap, double wordGap)
        => insideGap >= InsideGapLeast * dit && letterGap >= GapStep * insideGap && wordGap >= GapStep * letterGap;

    /// <summary>The least marks a window's decode must hold before its pattern is judged.</summary>
    public const int PatternLeastMarks = 16;

    /// <summary>The least a lane must have heard before it is decoded, live: a shorter window teaches the passes too little of the sender.</summary>
    public const double LeastWindowSeconds = 6;

    /// <summary>How long a printed lane may commit nothing before another lane that reads may take the terminal.</summary>
    public const double YieldSeconds = 4;

    /// <summary>
    /// How long a lane keeps its sender's rhythm and levels after its pattern last read Morse (work instruction 589, change 3): in
    /// his pause it decodes nobody keying, and when he comes back it reads him warm.
    /// </summary>
    public const double KeepRhythmSeconds = 20;

    /// <summary>The keying, gaps capped at the sender's word gap, the pattern tests are judged over (work instruction 589, change 3).</summary>
    public const double PatternSeconds = 10;

    /// <summary>The span either side of a lane within which another lane reading the same sequence is its shadow.</summary>
    public const double ShadowReachHz = 100;

    /// <summary>
    /// How much better separated another lane reading the printed lane's sequence must stand to take the terminal from it (work
    /// instruction 590, round two): the station's own lane against one 25 Hz off stood 2.25 against 1.64 on 144020.
    /// </summary>
    public const double SeparationMargin = 0.25;

    /// <summary>How near two marks' starts must sit to be the same mark, for the shadow test.</summary>
    public const double SameMarkSeconds = 0.03;

    private const double Neg = -1e18;

    // The states: dit, dah, inside gap, letter gap, word gap, and nobody keying.
    internal const int Dit = 0;
    internal const int Dah = 1;
    internal const int InsideGap = 2;
    internal const int LetterGap = 3;
    internal const int WordGap = 4;
    internal const int Silence = 5;

    private const int States = 6;

    private static readonly bool[] OnState = [true, true, false, false, false, false];

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
    public readonly record struct PatternStats(int Marks, double DahShare, double DahTightness, double Separation, bool GapsInOrder = true)
    {
        /// <summary>Whether the decode's pattern is Morse.</summary>
        public bool IsMorse => Marks >= PatternLeastMarks && DahShare >= DahShareLeast && DahShare <= DahShareMost && DahTightness <= DahTightnessMost && Separation >= SeparationLeast && GapsInOrder;

        /// <inheritdoc />
        public override string ToString()
            => string.Create(Inv, $"{Marks} marks, dah share {DahShare:0.00}, dah tightness {DahTightness:0.00}, separation {Separation:0.00}{(GapsInOrder ? string.Empty : ", gaps crossed")}{(IsMorse ? ", Morse" : string.Empty)}");
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

    /// <summary><c>ranges_from_dit</c>: each state's range in hops from a dit; nobody keying from a word gap of seven dits up, unbounded.</summary>
    public static Range[] RangesFromDit(double d)
        => [.. Rounded([(0.5 * d, 1.6 * d), (2.0 * d, 4.5 * d), (0.4 * d, 1.7 * d), (1.8 * d, 5.0 * d), (5.0 * d, 14 * d)]), SilenceFrom(7 * d)];

    // Nobody keying: from one word gap up, with no upper bound.
    private static Range SilenceFrom(double wordGap) => new(Math.Max(2, (int)Math.Round(wordGap)), int.MaxValue);

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

        var best = new double[States][];
        var backState = new sbyte[States][];
        var backStart = new int[States][];

        for (var s = 0; s < States; s++)
        {
            best[s] = new double[n + 1];
            backState[s] = new sbyte[n + 1];
            backStart[s] = new int[n + 1];
            Array.Fill(best[s], Neg);
        }

        // The predecessors' best at each start hop, taken once: a mark follows any gap, nobody keying or the start (2, 3, 4, 5,
        // -1, first wins a tie, as the prototype's strict comparison in list order); an inside or letter gap follows a dit or a dah
        // (0, 1); a word gap and nobody keying follow a dit, a dah or the start (0, 1, -1).
        var afterGap = new double[n + 1];
        var afterGapState = new sbyte[n + 1];
        var afterMark = new double[n + 1];
        var afterMarkState = new sbyte[n + 1];
        var prior = new double[States];

        for (var s = 0; s <= WordGap; s++)
        {
            prior[s] = Math.Log(rng[s].Hi - rng[s].Lo + 1);
        }

        // **NOBODY KEYING COSTS NOTHING BUT ITS EMISSIONS** (work instruction 589, change 1): no upper bound, so its only price
        // is the word gap's own, and where the two explain a pause alike the word gap, first in list order, keeps it.
        var silent = rng.Count > Silence;
        var silenceLo = silent ? rng[Silence].Lo : int.MaxValue;

        prior[Silence] = prior[WordGap];

        // The best start for nobody keying ending at j: the running maximum over every start at least its least duration back.
        var silenceBest = Neg;
        var silenceStart = -1;
        sbyte silencePrev = -1;

        afterGap[0] = 0;
        afterGapState[0] = -1;
        afterMark[0] = Neg;
        afterMarkState[0] = -1;

        for (var j = 1; j <= n; j++)
        {
            if (silent && j - silenceLo >= 0)
            {
                var i = j - silenceLo;
                var (prev, ps) = i == 0 ? (Math.Max(0.0, afterMark[0]), (sbyte)-1) : (afterMark[i], afterMarkState[i]);

                if (prev > Neg / 2 && prev - cOff[i] > silenceBest)
                {
                    silenceBest = prev - cOff[i];
                    silenceStart = i;
                    silencePrev = ps;
                }

                if (silenceStart >= 0)
                {
                    best[Silence][j] = silenceBest + cOff[j] - prior[Silence];
                    backState[Silence][j] = silencePrev;
                    backStart[Silence][j] = silenceStart;
                }
            }

            for (var s = 0; s <= WordGap; s++)
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

            foreach (var p in new[] { InsideGap, LetterGap, WordGap, Silence })
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

        for (var s = 1; s < States; s++)
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

        Range[] r = [.. Rounded([(0.55 * d, (d + dah) / 2), ((d + dah) / 2, 1.8 * dah), (0.4 * ig, (ig + lg) / 2), ((ig + lg) / 2, (lg + wg) / 2), ((lg + wg) / 2, 2.5 * wg)]), SilenceFrom(wg)];

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
        var one = db.ToArray();

        return ReadCold(_ => one, passes).Reading;
    }

    /// <summary>
    /// <see cref="ReadCold(ReadOnlySpan{double}, int)"/> where the lane is filtered at every width of <see cref="LaneWidthsHz"/>
    /// (work instruction 589, change 2): every start is read in the narrowest width, as the cold read was built, and scored under the
    /// levels every start's first pass decodes with, so the scores compare paths and not each start's own learned spreads; the best
    /// start takes the remaining passes in the width the dit it learned asks for. Traced (<c>TheColdStartsScored</c>): scored under
    /// its own learned levels, a start that splits each mark into pieces one hop apart won on KM3STU and the first recording, since
    /// a key-down spread of 2 dB makes every edge hop of a whole mark dear; and a score in one width does not compare with a score
    /// in another.
    /// </summary>
    public static (Reading Reading, int Width) ReadCold(Func<int, double[]> envelope, int passes = Passes)
    {
        const int cold = 0;
        var db = envelope(cold);
        var levels = LevelsFromPercentiles(db);
        Reading? best = null;
        var bestScore = double.NegativeInfinity;
        var bestMorse = false;

        foreach (var dit in new[] { GuessDitHops(db) }.Concat(ColdStartDits).Distinct())
        {
            var ranges = RangesFromDit(dit);
            var first = Read(db, ranges, levels, passes: 1);

            if (first.Passes.Count == 0)
            {
                best ??= first;
                continue;
            }

            var score = LogLikelihood(first.Segments, db, levels) - PriorCost(first.Segments, ranges);

            // **THE COLD READ IS CHOSEN BY PATTERN** (work instruction 590, change 1): a start whose decode is Morse, its gaps in
            // order, is kept before any that is not; among them, and failing them among all, the likeliest.
            var morse = first.Pattern.IsMorse;

            if ((morse && !bestMorse) || (morse == bestMorse && score > bestScore))
            {
                bestScore = score;
                bestMorse = morse;
                best = first;
            }
        }

        if (best is null || best.Passes.Count == 0 || passes <= 1)
        {
            return (best!, cold);
        }

        // The rest of the passes in the width the learned dit asks for: where it is another width, the levels are learned again.
        var learned = best.Passes[^1];
        var next = WidthFor(learned.Dit);
        var envelopeNext = envelope(next);
        var rest = Read(envelopeNext, learned.Ranges, next == cold ? best.Levels : LevelsFromPercentiles(envelopeNext), passes - 1);

        return (rest with { Passes = best.Passes.Concat(rest.Passes).ToList() }, next);
    }

    /// <summary>What a decode's segments pay for their durations under the ranges, as <see cref="Decode"/> charges them.</summary>
    public static double PriorCost(IReadOnlyList<Segment> segs, IReadOnlyList<Range> rng)
        => segs.Sum(g => Math.Log(rng[Math.Min(g.State, WordGap)].Hi - rng[Math.Min(g.State, WordGap)].Lo + 1));

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
                case Silence:
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

        // The gaps in order (work instruction 590, change 1): the decode's own medians, each where it laid three or more.
        double? Med(int s) => segs.Count(g => g.State == s) >= 3 ? Median(segs.Where(g => g.State == s).Select(g => (double)g.Length).ToList()) : null;
        var inOrder = Med(Dit) is not { } d || RhythmHolds(d, Med(InsideGap) ?? d, Med(LetterGap) ?? 3 * d, Med(WordGap) ?? 7 * d);

        return new PatternStats(marks, share, tight, separation, inOrder);
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

    /// <summary>Lanes every <see cref="LaneSpacingHz"/> across a passband, each filtered at every width of <see cref="LaneWidthsHz"/>.</summary>
    /// <param name="sampleRate">The audio's rate.</param>
    /// <param name="lowHz">The passband's low edge.</param>
    /// <param name="highHz">The passband's high edge.</param>
    public CwSequenceReader(int sampleRate, double lowHz, double highHz)
    {
        _rate = sampleRate;
        _hopSamples = Math.Max(1, (int)(sampleRate * HopMs / 1000));
        _windowHops = Hops(WindowSeconds);
        _everyHops = Hops(DecodeEverySeconds);
        _lagHops = Hops(CommitLagSeconds);

        var first = (int)Math.Ceiling(lowHz / LaneSpacingHz);
        var last = (int)Math.Floor(highHz / LaneSpacingHz);

        _lanes = Enumerable.Range(first, Math.Max(0, last - first + 1))
            .Select(k => new Lane(k * LaneSpacingHz, sampleRate, _windowHops + _everyHops))
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

    private static int Hops(double seconds) => (int)Math.Round(seconds * 1000 / HopMs);

    /// <summary>Hear one sample.</summary>
    public void Push(float sample)
    {
        var take = _hopFill == 0;

        foreach (var lane in _lanes)
        {
            lane.Push(sample, take);
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
            lane.Decode(newest, _windowHops, Trace);
        }

        // Shadows: a lane reading the same sequence as a better-separated lane within reach is not a station; the printed lane is
        // judged first, so a station between two lanes is not handed back and forth between them.
        // **A STATION'S LANE KEEPS ITS SENDER THROUGH A PAUSE** (work instruction 589, change 3): the printed lane stays a station
        // while it read Morse within the last KeepRhythmSeconds; in his pause it decodes nobody keying and prints nothing.
        var keep = Hops(KeepRhythmSeconds);
        // **ONE STATION'S LANES: THE BEST-SEPARATED PRINTS** (work instruction 590, round two): the printed lane comes first among
        // the lanes reading its sequence only while none stands SeparationMargin better. Traced on 144020: at 20 s the terminal
        // went to 625 Hz, 25 Hz off the second station, at separation 1.64; from 22 s the 600 Hz lane read him at 2.25 and was
        // dropped as the printed lane's shadow, and 625 Hz printed him in a crossed rhythm.
        var printedSeparation = _printed?.Pattern.Separation ?? 0;
        var reading = _lanes.Where(l => l.Pattern.IsMorse || (l == _printed && newest - l.LastMorse <= keep)).OrderByDescending(l => l == _printed ? printedSeparation + SeparationMargin : l.Pattern.Separation).ToList();
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
        if (_printed is not null && (!stations.Contains(_printed) || newest - _printed.LastCommitted > Hops(YieldSeconds)))
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
            Trace?.Add(string.Create(Inv, $"{Seconds(newest):0.00} s holds {chosen.PitchHz:0} Hz in its {LaneWidthsHz[chosen.Width]:0} Hz lane, {chosen.Pattern}, {chosen.Learned}"));
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

        // Nobody keying ends a letter and a word as a word gap does; nothing prints from it.
        var items = new List<(int State, long From, long To, double Mean)>();

        foreach (var g in segs)
        {
            var from = origin + g.From;
            var to = origin + g.To;

            items.Add((g.State == Silence ? WordGap : g.State, from, to, g.IsMark ? lane.Mean(from, to) : double.NaN));
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

            // A letter already committed is not printed again.
            if (letterFrom < lane.Committed - 1)
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

            Trace?.Add(string.Create(Inv, $"{Seconds(newest):0.00} s commits {pattern} at {Seconds(letterFrom):0.00}-{Seconds(letterTo):0.00} s from {lane.PitchHz:0} Hz{(space ? ", a space before" : string.Empty)}; {learned}, {LaneWidthsHz[lane.Width]:0} Hz lane"));

            lane.Committed = letterTo;
            lane.LastCommitted = newest;
            _lastPrintedEnd = letterTo;
            _printedAny = true;
        }
    }

    // One lane: its filters at every width, its envelope history at each, its last decode, and the marks it decoded lately.
    private sealed class Lane
    {
        private readonly int _keep;
        private readonly LaneFilter[] _filters;
        private readonly List<double>[] _db;
        private readonly List<(long From, long To, bool Dah)> _marks = [];

        public Lane(double pitchHz, int rate, int keep)
        {
            PitchHz = pitchHz;
            _keep = keep;
            _filters = LaneWidthsHz.Select(w => new LaneFilter(pitchHz, rate, w)).ToArray();
            _db = LaneWidthsHz.Select(_ => new List<double>()).ToArray();
        }

        public double PitchHz { get; }

        // The absolute hop of every history's first entry.
        public long Origin { get; private set; }

        public List<Segment>? Segments { get; private set; }

        public Learned? Learned { get; private set; }

        public Levels Levels { get; private set; }

        public PatternStats Pattern { get; private set; }

        // The width, an index into LaneWidthsHz, the last decode read in.
        public int Width { get; private set; } = LaneWidthsHz.Length / 2;

        // The hop at which this lane's pattern last read Morse.
        public long LastMorse { get; private set; } = long.MinValue / 2;

        // The end of the last letter committed from this lane, in absolute hops.
        public long Committed { get; set; } = long.MinValue / 2;

        // The hop at which this lane last committed a letter, or was handed the terminal.
        public long LastCommitted { get; set; }

        // The absolute hop the last decode's segments start at.
        public long DecodedFrom { get; private set; }

        public void Push(float sample, bool take)
        {
            for (var w = 0; w < _filters.Length; w++)
            {
                var amplitude = _filters[w].Push(sample);

                if (take)
                {
                    _db[w].Add(20 * Math.Log10(amplitude + 1e-9));
                }
            }

            if (take && _db[0].Count > _keep)
            {
                var drop = _db[0].Count - _keep;

                foreach (var h in _db)
                {
                    h.RemoveRange(0, drop);
                }

                Origin += drop;
            }
        }

        public double Mean(long from, long to)
        {
            var db = _db[Width];
            var a = (int)Math.Max(0, from - Origin);
            var b = (int)Math.Min(db.Count, to - Origin);

            if (b <= a)
            {
                return double.NaN;
            }

            var sum = 0.0;

            for (var k = a; k < b; k++)
            {
                sum += db[k];
            }

            return sum / (b - a);
        }

        public void Decode(long newest, int window, List<string>? trace = null)
        {
            var count = Math.Min(window, _db[0].Count);

            if (count < Hops(LeastWindowSeconds))
            {
                Segments = null;
                Pattern = default;
                return;
            }

            var cut = new double[LaneWidthsHz.Length][];

            double[] Cut(int w) => cut[w] ??= _db[w].GetRange(_db[w].Count - count, count).ToArray();

            // **A LANE KEEPS ITS SENDER'S RHYTHM AND LEVELS FOR KeepRhythmSeconds** (change 3): it is not let go and does not start
            // cold when he comes back; a cold lane tries its starts in the widths they ask for (change 2).
            var warm = Learned is not null && newest - LastMorse <= Hops(KeepRhythmSeconds);
            Reading reading;

            if (warm)
            {
                var width = WidthFor(Learned!.Dit);
                var db = Cut(width);

                reading = Read(db, Learned.Ranges, width == Width ? Levels : LevelsFromPercentiles(db));
                Width = width;

                // **A KEPT RHYTHM THAT NO LONGER READS THE WINDOW GIVES WAY TO A COLD READ THAT DOES** (round three). Traced on
                // W1AW's piece-02 at 237 s: he went from 14 to 35 WPM, the lane read on at its kept 80 ms dit in the 15 Hz width,
                // which smears a 35 ms dit, so the decode never showed the new rhythm to learn from and 60 s printed placeholders.
                // **AND WHERE ITS FIGURES FALL, A SECOND STATION IN THE LANE** (work instruction 590, change 3): a warm decode whose
                // pattern falls from the one the lane kept by PatternDrop is read cold as well, and the cold read is taken where
                // its pattern is Morse and stands better.
                var fell = !reading.Pattern.IsMorse || Falls(reading.Pattern, Kept);

                if (fell || trace is not null)
                {
                    var (cold, coldWidth) = ReadCold(Cut);
                    var take = fell && cold.Pattern.IsMorse && (!reading.Pattern.IsMorse || Better(cold.Pattern, reading.Pattern));

                    trace?.Add(string.Create(Inv, $"{newest * HopMs / 1000:0.00} s {PitchHz:0} Hz warm: {reading.Pattern}; kept: {Kept}; cold: {cold.Pattern}, {(cold.Passes.Count > 0 ? cold.Passes[^1] : null)}; {(take ? "takes the cold read" : fell ? "keeps its rhythm, the cold read no better" : "keeps its rhythm")}"));

                    if (take)
                    {
                        reading = cold;
                        Width = coldWidth;
                    }
                }
            }
            else
            {
                (reading, var width) = ReadCold(Cut);

                Width = width;
            }

            var shift = newest - count;

            Segments = reading.Segments.ToList();
            DecodedFrom = shift;

            // A window of nobody keying teaches nothing: a warm lane keeps what it knew of its sender.
            if (reading.Passes.Count > 0)
            {
                Learned = reading.Passes[^1];
                Levels = reading.Levels;
            }
            else if (!warm)
            {
                Learned = null;
                Levels = reading.Levels;
            }

            // The marks decoded lately: this window's replace what an earlier decode laid over the same hops.
            _marks.RemoveAll(m => m.To > shift || m.From < newest - Hops(KeepRhythmSeconds + WindowSeconds));

            foreach (var g in Segments.Where(g => g.IsMark))
            {
                _marks.Add((shift + g.From, shift + g.To, g.State == Dah));
            }

            // The marks of the last PatternSeconds, judged with the rhythm the lane learned: its gaps in order (change 1).
            Pattern = LatePattern() with { GapsInOrder = Learned is null || GapsClimb(Learned) };

            if (Pattern.IsMorse)
            {
                LastMorse = newest;
            }

            if (reading.Pattern.IsMorse)
            {
                Kept = reading.Pattern;
            }
        }

        // The window pattern the lane last read Morse with, what a warm decode's figures are held to (change 3).
        public PatternStats Kept { get; private set; }

        // **SOMEONE IS THERE, OVER THE LANE'S LAST PatternSeconds OF KEYING** (change 3): its marks from the newest back, each gap
        // counted no longer than the sender's word gap, so a pause does not empty the pattern and a short exchange after one is
        // judged with what came before it.
        private PatternStats LatePattern()
        {
            var cap = Learned?.WordGap ?? Hops(0.5);
            var keyed = 0.0;
            var taken = new List<Segment>();

            for (var k = _marks.Count - 1; k >= 0; k--)
            {
                var m = _marks[k];

                keyed += m.To - m.From;

                if (k < _marks.Count - 1)
                {
                    keyed += Math.Min(_marks[k + 1].From - m.To, cap);
                }

                if (keyed > Hops(PatternSeconds))
                {
                    break;
                }

                taken.Add(new Segment(m.Dah ? Dah : Dit, 0, (int)(m.To - m.From)));
            }

            return CwSequenceReader.Pattern(taken, Levels);
        }

        public List<long> MarkStarts()
            => Segments is null ? [] : Segments.Where(g => g.IsMark).Select(g => DecodedFrom + g.From).ToList();

        public bool HasMarksAfter(long hop)
            => Segments is not null && Segments.Any(g => g.IsMark && DecodedFrom + g.From >= hop);
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

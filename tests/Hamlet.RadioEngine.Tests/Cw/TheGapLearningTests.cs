using System.Globalization;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Tests.Cw.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **HOW THE SEQUENCE READER FINDS AND LEARNS A HAND'S GAPS** (work instruction 593): the trace the change is built from.
/// </summary>
public sealed class TheGapLearningTests(ITestOutputHelper output)
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <remarks>
    /// **THE TRACE, FIRST** (work instruction 593, section 2): on `bug` and `mobile` at 20 dB, their own speed, the QSO text, and on Ken 20-60 s and
    /// Bryan 140-180 s, three 12 s windows each, one lane: every cold start, as built and as section 3 adds them, with the decode's likelihood,
    /// its validity, what one pass learned by medians and by the fit, the sent or measured gaps, and what three passes then read, with its edits
    /// against the text. Asserts nothing; never on a line.
    /// </remarks>
    [Fact]
    [Trait("Category", "Survey")]
    public void TheColdStartsTraced()
    {
        var truths = CwWildHands.Truths();
        var wav = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(CwWildHands.Recording));
        var bug = CwWildBank.Recipes[1];
        var mobile = CwWildBank.Recipes[0];

        Hand("Ken 20-60 s, 500 Hz (measured)", wav, 500, [32, 46, 60], bug, truths[0].Text);
        Hand("Bryan 140-180 s, 500 Hz (measured)", wav, 500, [152, 166, 180], mobile, truths[1].Text);

        foreach (var recipe in new[] { bug, mobile })
        {
            var hand = new CwWildHand(recipe, OwnSpeed: true, Db: 20, Qso: true);
            var audio = hand.Audio();
            var seconds = audio.Samples.Length / (double)audio.SampleRate;

            Hand($"recipe `{recipe.Name}`, 20 dB, own speed, QSO text (sent), {seconds:0} s", audio, Math.Round(CwFistBank.ToneHz / 25) * 25, [seconds * 0.3, seconds * 0.55, seconds * 0.8], recipe, hand.Text);
        }
    }

    private void Hand(string name, MonoAudio audio, double pitch, double[] ends, CwWildRecipe recipe, string truth)
    {
        var whole = CwSequenceReader.LaneWidthsHz.Select(w => CwSequenceReader.Envelope(audio.Samples, audio.SampleRate, pitch, w)).ToArray();
        var (i, l, w) = recipe.Medians;
        var d = recipe.DitMs;

        output.WriteLine(string.Empty);
        output.WriteLine(string.Create(Inv, $"## {name}: dit {d:0} dah {recipe.Ratio * d:0} inside {i * d:0} letter {l * d:0} word {w * d:0} ms"));

        foreach (var end in ends)
        {
            var hops = (int)(end * 1000 / CwSequenceReader.HopMs);
            var from = Math.Max(0, hops - (int)(CwSequenceReader.WindowSeconds * 1000 / CwSequenceReader.HopMs));
            var cut = whole.Select(e => e[from..Math.Min(hops, e.Length)]).ToArray();
            var db = cut[0];
            var levels = CwSequenceReader.LevelsFromPercentiles(db);
            var dits = new[] { CwSequenceReader.GuessDitHops(db) }.Concat(CwSequenceReader.ColdStartDits).Distinct().ToList();
            var rows = new List<Row>();

            foreach (var dit in dits)
            {
                rows.Add(Start("built", dit, CwSequenceReader.RangesFromDit(dit), db, levels, cut, truth));
                rows.Add(Start("wide", dit, CwSequenceReader.ColdRanges(dit), db, levels, cut, truth));

                foreach (var line in new[] { 1.5, 2.2, 3.0 })
                {
                    rows.Add(Start(string.Create(Inv, $"line {line:0.0}"), dit, CwSequenceReader.ColdRanges(dit, line), db, levels, cut, truth));
                }
            }

            var built = Taken(rows.Where(r => r.Kind == "built").ToList(), r => r.Score);
            var likeliest = Taken(rows.Where(r => r.Kind != "built").ToList(), r => r.Score);
            var valid = rows.Where(r => r.Kind != "built" && r.Validity).ToList();
            var chosen = valid.Count > 0 ? Taken(valid, r => r.Score) : rows.Where(r => r.Kind != "built").MaxBy(r => r.Letters)!;
            var bestEdits = rows.MaxBy(r => r.RightFit)!;

            output.WriteLine(string.Empty);
            output.WriteLine(string.Create(Inv, $"### window {end - CwSequenceReader.WindowSeconds:0}-{end:0} s"));
            output.WriteLine("| start | ranges | score/hop | Morse | words | near lines | one pass, medians ig/lg/wg | one pass, fit ig/lg/wg | three by medians: right of letters, spaces added, last classes | three by the fit: right of letters, spaces added, last classes | taken |");
            output.WriteLine("|---|---|---|---|---|---|---|---|---|---|---|");

            foreach (var r in rows)
            {
                var taken = (r == built ? "built " : string.Empty) + (r == likeliest ? "likeliest " : string.Empty) + (r == chosen ? "VALID " : string.Empty) + (r == bestEdits ? "fewest-edits" : string.Empty);

                output.WriteLine(string.Create(Inv, $"| {r.Dit * CwSequenceReader.HopMs:0} ms | {r.Kind} | {r.Score:0.000} | {(r.Morse ? "yes" : "no")} | {r.Letters:0%} | {r.Near:0%} | {r.Medians} | {r.Fit} | {r.Median3} | {r.Fit3} | {taken} |"));
            }

            output.WriteLine(string.Create(Inv, $"built:  `{built.TextMedian}`"));
            output.WriteLine(string.Create(Inv, $"valid:  `{chosen.TextFit}`"));
            output.WriteLine(string.Create(Inv, $"fewest: `{bestEdits.TextFit}`"));
        }
    }

    private static Row Taken(IReadOnlyList<Row> rows, Func<Row, double> by)
    {
        var morse = rows.Where(r => r.Morse).ToList();

        return (morse.Count > 0 ? morse : rows).MaxBy(by)!;
    }

    private static Row Start(string kind, int dit, CwSequenceReader.Range[] ranges, double[] db, CwSequenceReader.Levels levels, double[][] cut, string truth)
    {
        var first = CwSequenceReader.Read(db, ranges, levels, passes: 1);
        var score = (CwSequenceReader.LogLikelihood(first.Segments, db, levels) - CwSequenceReader.PriorCost(first.Segments, ranges)) / db.Length;
        var medians = first.Passes.Count > 0 ? first.Passes[^1] : null;
        var fit = CwSequenceReader.LearnByFit(first.Segments, db, ranges);
        var fitReading = first with { Passes = fit is null ? [] : [fit] };
        var validity = CwSequenceReader.Validity(fitReading);

        string Show(CwSequenceReader.Learned? x) => x is null ? "-" : string.Create(Inv, $"{x.InsideGap * CwSequenceReader.HopMs:0}/{x.LetterGap * CwSequenceReader.HopMs:0}/{x.WordGap * CwSequenceReader.HopMs:0} (lines {x.Ranges[2].Hi * CwSequenceReader.HopMs:0}, {x.Ranges[3].Hi * CwSequenceReader.HopMs:0})");

        // The rest of the passes, as the reader takes them: in the width the learned dit asks for.
        (string Text, CwSequenceReader.Learned? Last) More(CwSequenceReader.Learned? x, bool byFit)
        {
            if (x is null)
            {
                return (first.Text, null);
            }

            var width = CwSequenceReader.WidthFor(x.Dit);
            var next = cut[width];
            var ranges2 = x.Ranges;
            var lv = width == 0 ? x.Levels : CwSequenceReader.LevelsFromPercentiles(next);
            var segs = first.Segments;
            var last = x;

            for (var pass = 0; pass < CwSequenceReader.Passes - 1; pass++)
            {
                segs = CwSequenceReader.Decode(next, ranges2, lv);

                if ((byFit ? CwSequenceReader.LearnByFit(segs, next, ranges2) : CwSequenceReader.Learn(segs, next)) is not { } y)
                {
                    break;
                }

                last = y;
                ranges2 = y.Ranges;
                lv = y.Levels;
            }

            return (CwSequenceReader.Text(segs), last);
        }

        var median3 = More(medians, byFit: false);
        var fit3 = More(fit, byFit: true);
        var a = Against(median3.Text, truth);
        var b = Against(fit3.Text, truth);

        return new Row(dit, kind, score, first.Pattern.IsMorse, validity.Words, validity.Gaps, validity.Valid == true, Show(medians), Show(fit), string.Create(Inv, $"{a.Right} of {a.Letters}, {a.Added}+ {Show(median3.Last)}"), median3.Text, string.Create(Inv, $"{b.Right} of {b.Letters}, {b.Added}+ {Show(fit3.Last)}"), b.Right, fit3.Text);
    }

    // The decode against the best stretch of the truth, both ends free: its letters that match, and the spaces it lays inside the truth's words.
    private static (int Right, int Letters, int Added) Against(string text, string truth)
    {
        var words = truth.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(w => w != "?").Select(TheRecordingsScoreboardTests.Letters).ToList();
        var key = string.Concat(words);
        var inWord = new bool[key.Length + 1];
        var at = 0;

        foreach (var w in words)
        {
            for (var k = 1; k < w.Length; k++)
            {
                inWord[at + k] = true;
            }

            at += w.Length;
        }

        var printed = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(TheRecordingsScoreboardTests.Letters).ToList();
        var decode = string.Concat(printed);

        if (decode.Length == 0)
        {
            return (0, 0, 0);
        }

        var spaceBefore = new bool[decode.Length + 1];
        var pos = 0;

        foreach (var w in printed)
        {
            spaceBefore[pos] = pos > 0;
            pos += w.Length;
        }

        var cost = new int[decode.Length + 1, key.Length + 1];

        for (var i = 1; i <= decode.Length; i++)
        {
            cost[i, 0] = i;

            for (var j = 1; j <= key.Length; j++)
            {
                cost[i, j] = Math.Min(cost[i - 1, j - 1] + (decode[i - 1] == key[j - 1] ? 0 : 1), Math.Min(cost[i - 1, j] + 1, cost[i, j - 1] + 1));
            }
        }

        var end = Enumerable.Range(0, key.Length + 1).MinBy(j => cost[decode.Length, j]);
        var (right, added) = (0, 0);
        var (x, y) = (decode.Length, end);

        while (x > 0)
        {
            if (y > 0 && cost[x, y] == cost[x - 1, y - 1] + (decode[x - 1] == key[y - 1] ? 0 : 1))
            {
                if (decode[x - 1] == key[y - 1])
                {
                    right++;

                    if (spaceBefore[x - 1] && inWord[y - 1])
                    {
                        added++;
                    }
                }

                x--;
                y--;
            }
            else if (y > 0 && cost[x, y] == cost[x, y - 1] + 1)
            {
                y--;
            }
            else
            {
                x--;
            }
        }

        return (right, decode.Length, added);
    }

    /// <remarks>The rule ships off (HM-DEC-297), and with it off the reader's ranges are as they were.</remarks>
    [Fact]
    public void TheRuleShipsOff()
    {
        Assert.Contains(CwRules.GapLearning, CwRules.OffByDefault);
        Assert.False(CwRules.On(CwRules.GapLearning));

        using var on = CwRules.Choose(CwRules.GapLearning);

        Assert.True(CwRules.On(CwRules.GapLearning));
    }

    /// <remarks>Section 3.4: the fit finds a hand's three classes and each class's members, from centres near them; from a poor start it can settle elsewhere, which is why a fit is judged before it is taken.</remarks>
    [Fact]
    public void TheFitFindsAHandsClasses()
    {
        double[] gaps = [20, 21, 22, 23, 24, 25, 60, 62, 64, 66, 68, 150, 160, 170];
        var (centres, counts) = CwSequenceReader.OrderedFit(gaps, [20, 50, 120]);

        Assert.Equal(new[] { 6, 5, 3 }, counts);
        Assert.InRange(centres[0], 21, 24);
        Assert.InRange(centres[1], 62, 66);
        Assert.InRange(centres[2], 155, 165);
    }

    /// <remarks>
    /// Round two: a window nearly all inside gaps does not part them into two classes. The letter centre would stand under twice the inside
    /// centre, so the fit is refused and the boundaries given stand.
    /// </remarks>
    [Fact]
    public void AWindowOfInsideGapsKeepsTheClasses()
    {
        var segs = new List<CwSequenceReader.Segment>();
        var at = 0;

        void Add(int state, int length)
        {
            segs.Add(new CwSequenceReader.Segment(state, at, at + length));
            at += length;
        }

        foreach (var inside in new[] { 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34 })
        {
            Add(CwSequenceReader.Dit, 14);
            Add(CwSequenceReader.InsideGap, inside);
            Add(CwSequenceReader.Dah, 42);
            Add(CwSequenceReader.LetterGap, inside + 14);
        }

        var db = new double[at];
        CwSequenceReader.Range[] previous = [new(8, 26), new(26, 76), new(10, 45), new(45, 90), new(90, 300), new(100, int.MaxValue)];

        var learned = CwSequenceReader.LearnByFit(segs, db, previous);

        Assert.NotNull(learned);
        Assert.Equal(45, learned.Ranges[CwSequenceReader.InsideGap].Hi);
    }

    /// <remarks>Work instruction 594: marks stay with the medians. A window of two mark lengths and three clear gap classes learns the medians' dit, dah and line, and the fit's gap lines.</remarks>
    [Fact]
    public void MarksStayWithTheMediansAndClearGapsTakeTheFit()
    {
        var (segs, db) = Segments([20, 21, 22, 23, 24, 25, 60, 62, 64, 66, 68, 150, 160, 170]);
        var medians = CwSequenceReader.Learn(segs, db)!;
        var learned = CwSequenceReader.LearnByFit(segs, db, Previous)!;

        Assert.Equal(medians.Dit, learned.Dit);
        Assert.Equal(medians.Dah, learned.Dah);
        Assert.Equal(medians.Ranges[CwSequenceReader.Dit], learned.Ranges[CwSequenceReader.Dit]);
        Assert.Equal(medians.Ranges[CwSequenceReader.Dah], learned.Ranges[CwSequenceReader.Dah]);
        Assert.InRange(learned.Ranges[CwSequenceReader.InsideGap].Hi, 36, 40);
    }

    /// <remarks>Work instruction 594, round three: gaps spread across the fit's lines, as `bug`'s are, keep the medians' own classes.</remarks>
    [Fact]
    public void GapsSpreadAcrossTheLinesKeepTheMedians()
    {
        var (segs, db) = Segments(Enumerable.Range(0, 27).Select(k => (int)Math.Round(15 * Math.Pow(1.1, k))).ToArray());
        var medians = CwSequenceReader.Learn(segs, db)!;
        var learned = CwSequenceReader.LearnByFit(segs, db, Previous)!;

        Assert.Equal(medians.Ranges, learned.Ranges);
    }

    private static readonly CwSequenceReader.Range[] Previous = [new(8, 26), new(26, 76), new(10, 45), new(45, 100), new(100, 300), new(100, int.MaxValue)];

    // Marks of 14 and 42 hops, each followed by a gap of the lengths given, the state as the given ranges lay it.
    private static (List<CwSequenceReader.Segment> Segs, double[] Db) Segments(int[] gaps)
    {
        var segs = new List<CwSequenceReader.Segment>();
        var at = 0;

        for (var k = 0; k < gaps.Length; k++)
        {
            var mark = k % 2 == 0 ? 14 : 42;

            segs.Add(new CwSequenceReader.Segment(k % 2 == 0 ? CwSequenceReader.Dit : CwSequenceReader.Dah, at, at + mark));
            at += mark;
            segs.Add(new CwSequenceReader.Segment(gaps[k] < 45 ? CwSequenceReader.InsideGap : gaps[k] < 100 ? CwSequenceReader.LetterGap : CwSequenceReader.WordGap, at, at + gaps[k]));
            at += gaps[k];
        }

        var db = new double[at];

        foreach (var g in segs.Where(g => g.IsMark))
        {
            for (var h = g.From; h < g.To; h++)
            {
                db[h] = 30;
            }
        }

        return (segs, db);
    }

    /// <remarks>
    /// Section 5: W1AW's piece-04 through the chain with the rule and without, the process's CPU time, the first read thrown away to warm
    /// the process. Asserts nothing; the bar is the report's.
    /// </remarks>
    [Fact]
    [Trait("Category", "Survey")]
    public void TheCost()
    {
        const string Piece = "w1aw-2026-10-07/piece-04";
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Piece));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Piece);

        (double Cpu, double Wall, string Text) Time(bool on)
        {
            // A rule of the sequence reader's, read with the sequence reader: the learned reader off (work instruction 599).
            using var pinned = CwRules.Off(CwRules.LearnedReader);
            using var chosen = on ? CwRules.Choose(CwRules.GapLearning) : null;
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var cpu = process.TotalProcessorTime;
            var wall = System.Diagnostics.Stopwatch.StartNew();
            var (_, text) = TheRecordingsScoreboardTests.ReadLive(audio.Samples, audio.SampleRate, pitch, width);

            process.Refresh();

            return ((process.TotalProcessorTime - cpu).TotalSeconds, wall.Elapsed.TotalSeconds, text);
        }

        Time(false);

        var before = Time(false);
        var after = Time(true);

        output.WriteLine(string.Create(Inv, $"{Piece}: {audio.Samples.Length / (double)audio.SampleRate:0} s"));
        output.WriteLine(string.Create(Inv, $"as shipped: {before.Cpu:0.0} s CPU, {before.Wall:0.0} s wall"));
        output.WriteLine(string.Create(Inv, $"the change: {after.Cpu:0.0} s CPU, {after.Wall:0.0} s wall, {(after.Cpu - before.Cpu) / before.Cpu:+0%;-0%}"));
    }

    /// <remarks>
    /// The rounds' trace (work instruction 593, section 4): one hand of the wild bank read through the chain, what it printed and when, and the
    /// lane's every decode: how it read, what it learned, its pattern and path. Asserts nothing; never on a line.
    /// </remarks>
    [Theory]
    [Trait("Category", "Survey")]
    [InlineData("bug", true, 20.0, true, 625.0)]
    [InlineData("bug", false, 20.0, false, 625.0)]
    [InlineData("bug", false, 20.0, true, 625.0)]
    [InlineData("mobile", true, 20.0, true, 625.0)]
    [InlineData("farnsworth", true, 20.0, true, 625.0)]
    public void TheHandTraced(string recipe, bool own, double db, bool qso, double watchHz)
    {
        var hand = new CwWildHand(CwWildBank.Recipes.Single(r => r.Name == recipe), own, db, qso);
        var audio = hand.Audio();
        var trace = new List<string>();
        using var on = CwRules.Choose(CwRules.GapLearning);
        var letters = TheSequenceReaderTests.ReadLiveTraced(audio.Samples, audio.SampleRate, 600, 500, trace, watchHz);

        output.WriteLine(hand.Text);
        output.WriteLine(TheRecordingsScoreboardTests.Text(letters));

        foreach (var line in trace.Where(l => !l.Contains(" s commits ", StringComparison.Ordinal)))
        {
            output.WriteLine(line);
        }
    }

    /// <remarks>
    /// **THE TRACE ON `bug`** (work instruction 594, section 2): 20 dB, his own speed, the QSO text, the windows ending at 40 to 66 s, one lane, a lane's warm
    /// chain with the first window cold. Each pass: the dit and dah the medians give, the dit and dah a two-cluster fit gives on the same marks, the
    /// dah share, the letters right. Twice: with the medians driving the next pass, as the reader now does; and with the fit replayed here driving it, as
    /// HM-DEC-297 built it, since the engine no longer holds it. Then the live lane. Asserts nothing; never on a line.
    /// </remarks>
    [Fact]
    [Trait("Category", "Survey")]
    public void TheBugTraced()
    {
        var recipe = CwWildBank.Recipes.Single(r => r.Name == "bug");
        var hand = new CwWildHand(recipe, OwnSpeed: true, Db: 20, Qso: true);
        var audio = hand.Audio();
        var pitch = Math.Round(CwFistBank.ToneHz / 25) * 25;
        var whole = CwSequenceReader.LaneWidthsHz.Select(w => CwSequenceReader.Envelope(audio.Samples, audio.SampleRate, pitch, w)).ToArray();
        using var on = CwRules.Choose(CwRules.GapLearning);

        output.WriteLine(hand.Text);
        BugChain("the medians drive the next pass (this unit)", whole, hand.Text, driveByFit: false);
        BugChain("the mark fit, replayed here, drives the next pass (as HM-DEC-297 built it)", whole, hand.Text, driveByFit: true);

        var trace = new List<string>();
        var letters = TheSequenceReaderTests.ReadLiveTraced(audio.Samples, audio.SampleRate, 600, 500, trace, pitch);

        output.WriteLine(string.Empty);
        output.WriteLine("## the live lane");
        output.WriteLine(TheRecordingsScoreboardTests.Text(letters));

        foreach (var line in trace.Where(l => l.Contains(" passes, read ", StringComparison.Ordinal) && Second(l) >= 38 && Second(l) <= 68))
        {
            output.WriteLine(line);
        }
    }

    private void BugChain(string name, double[][] whole, string truth, bool driveByFit)
    {
        const double HopMs = CwSequenceReader.HopMs;
        CwSequenceReader.Learned? learned = null;
        var levels = default(CwSequenceReader.Levels);
        var lastWidth = 0;

        output.WriteLine(string.Empty);
        output.WriteLine("## " + name);
        output.WriteLine("| window ends | pass | lane Hz | medians dit/dah ms | fit dit/dah ms | dah share | letters right |");
        output.WriteLine("|---|---|---|---|---|---|---|");

        for (var end = 40; end <= 66; end += 2)
        {
            var to = (int)(end * 1000 / HopMs);
            var from = Math.Max(0, to - (int)(CwSequenceReader.WindowSeconds * 1000 / HopMs));
            var cut = whole.Select(e => e[from..Math.Min(to, e.Length)]).ToArray();

            if (learned is null)
            {
                var (cold, width) = CwSequenceReader.ReadCold(w => cut[w]);

                learned = cold.Passes[^1];
                levels = cold.Levels;
                lastWidth = width;
            }

            var next = CwSequenceReader.WidthFor(learned.Dit);
            var db = cut[next];
            var ranges = learned.Ranges;

            levels = next == lastWidth ? levels : CwSequenceReader.LevelsFromPercentiles(db);
            lastWidth = next;

            for (var pass = 1; pass <= CwSequenceReader.Passes; pass++)
            {
                var segs = CwSequenceReader.Decode(db, ranges, levels);

                if (CwSequenceReader.LearnByFit(segs, db, ranges) is not { } l)
                {
                    break;
                }

                var marks = segs.Where(g => g.IsMark).Select(g => (double)g.Length).ToList();
                var (m, mc) = CwSequenceReader.OrderedFit(marks, [l.Dit, l.Dah]);
                var dit = mc[0] >= CwSequenceReader.LeastInCluster ? m[0] : l.Dit;
                var dah = mc[1] >= CwSequenceReader.LeastInCluster ? m[1] : l.Dah;
                var line = mc[0] >= CwSequenceReader.LeastInCluster && mc[1] >= CwSequenceReader.LeastInCluster ? Math.Sqrt(dit * dah) : ranges[CwSequenceReader.Dit].Hi;
                var a = Against(CwSequenceReader.Text(segs), truth);

                output.WriteLine(string.Create(Inv, $"| {end} | {pass} | {CwSequenceReader.LaneWidthsHz[next]:0} | {l.Dit * HopMs:0} / {l.Dah * HopMs:0} | {dit * HopMs:0} / {dah * HopMs:0} | {CwSequenceReader.Pattern(segs, levels).DahShare:0.00} | {a.Right} of {a.Letters} |"));

                if (driveByFit)
                {
                    var r = l.Ranges.ToArray();

                    r[CwSequenceReader.Dit] = new CwSequenceReader.Range(Math.Max(1, (int)Math.Round(0.55 * dit)), Math.Max(2, (int)Math.Round(line)));
                    r[CwSequenceReader.Dah] = new CwSequenceReader.Range(Math.Max(1, (int)Math.Round(line)), Math.Max(2, (int)Math.Round(1.8 * dah)));
                    l = l with { Dit = dit, Dah = dah, Ranges = r };
                }

                learned = l;
                ranges = l.Ranges;
                levels = l.Levels;
            }
        }
    }

    private static double Second(string line) => double.Parse(line[..line.IndexOf(' ', StringComparison.Ordinal)], Inv);

    /// <remarks>
    /// **THE TRACE ON KEN, 60 TO 80 S** (work instruction 594, section 2): each window's starts with the one taken and its validity, the classes each pass
    /// learned against his measured 136, 344 and 608 ms, the live lane's own passes, and every letter that differs from the draft truth, with the rule on
    /// and off, each named a mark or a gap fault from the patterns as laid. Asserts nothing; never on a line.
    /// </remarks>
    [Fact]
    [Trait("Category", "Survey")]
    public void TheKenTraced()
    {
        const double From = 60;
        const double To = 80;
        const double HopMs = CwSequenceReader.HopMs;
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(CwWildHands.Recording));
        var truth = CwWildHands.Truths().Single(t => t.Name.StartsWith("Ken", StringComparison.Ordinal));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(CwWildHands.Recording);
        var whole = CwSequenceReader.LaneWidthsHz.Select(w => CwSequenceReader.Envelope(audio.Samples, audio.SampleRate, truth.PitchHz, w)).ToArray();

        using (CwRules.Choose(CwRules.GapLearning))
        {
            output.WriteLine("## the starts, each window, the rule on; his measured gaps 136 / 344 / 608 ms");

            for (var end = 62; end <= 82; end += 4)
            {
                var hops = (int)(end * 1000 / HopMs);
                var from = hops - (int)(CwSequenceReader.WindowSeconds * 1000 / HopMs);
                var cut = whole.Select(e => e[from..hops]).ToArray();
                var db = cut[0];
                var levels = CwSequenceReader.LevelsFromPercentiles(db);
                var (cold, _) = CwSequenceReader.ReadCold(w => cut[w]);

                output.WriteLine(string.Empty);
                output.WriteLine(string.Create(Inv, $"### window {end - CwSequenceReader.WindowSeconds:0}-{end} s"));

                foreach (var dit in new[] { CwSequenceReader.GuessDitHops(db) }.Concat(CwSequenceReader.ColdStartDits).Distinct())
                {
                    var ranges = CwSequenceReader.ColdRanges(dit);
                    var first = CwSequenceReader.Read(db, ranges, levels, passes: 1);
                    var v = CwSequenceReader.Validity(first);
                    var score = (CwSequenceReader.LogLikelihood(first.Segments, db, levels) - CwSequenceReader.PriorCost(first.Segments, ranges)) / db.Length;
                    var taken = first.Passes.Count > 0 && cold.Passes.Count > 0 && first.Passes[0].ToString() == cold.Passes[0].ToString();

                    output.WriteLine(string.Create(Inv, $"start {dit * HopMs:0} ms: score/hop {score:0.000}, Morse {first.Pattern.IsMorse}, words {v.Words:0%} of {v.Counted}, near lines {v.Gaps:0%}, valid {v.Valid}{(taken ? ", TAKEN" : string.Empty)}: `{first.Text}`"));
                }

                for (var p = 0; p < cold.Passes.Count; p++)
                {
                    output.WriteLine(string.Create(Inv, $"pass {p + 1}: {cold.Passes[p]}"));
                }

                output.WriteLine("read: `" + cold.Text + "`");
            }
        }

        foreach (var ruleOn in new[] { false, true })
        {
            using var chosen = ruleOn ? CwRules.Choose(CwRules.GapLearning) : null;
            var trace = new List<string>();
            var letters = TheSequenceReaderTests.ReadLiveTraced(audio.Samples, audio.SampleRate, pitch, width, trace, truth.PitchHz);
            var map = TheKeyingMapTests.Read(CwWildHands.Recording, audio.Samples, audio.SampleRate);
            var score = CwWildHands.ScoreOf(truth, letters, map);
            var window = score.Windows.Single(w => Math.Abs(w.From - From) < 0.1);

            output.WriteLine(string.Empty);
            output.WriteLine(string.Create(Inv, $"## the rule {(ruleOn ? "on" : "off")}: {score.Right} of {score.OutOf} right; 60-80 s, {window.Right} right, `{window.Printed}`"));
            output.WriteLine("truth: `" + truth.Text + "`");

            foreach (var line in trace.Where(l => l.Contains(" passes, read ", StringComparison.Ordinal) && Second(l) >= From && Second(l) <= To + 6))
            {
                output.WriteLine(line);
            }

            KenFaults(score.Letters, truth, trace, From, To);
        }
    }

    // One row per letter that differs in the span, each run of differing letters named by comparing its patterns as laid with the truth's.
    private void KenFaults(IReadOnlyList<TheRecordingsScoreboardTests.Printed> letters, CwWildHands.Truth truth, IReadOnlyList<string> trace, double from, double to)
    {
        var key = new StringBuilder();
        var free = new HashSet<int>();

        foreach (var word in truth.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (word == "?")
            {
                free.Add(key.Length);
            }
            else
            {
                key.Append(TheRecordingsScoreboardTests.Letters(word));
            }
        }

        var decode = string.Concat(letters.Select(l => TheRecordingsScoreboardTests.Letters(l.Text)));
        var of = letters.SelectMany((l, i) => Enumerable.Repeat(i, TheRecordingsScoreboardTests.Letters(l.Text).Length)).ToArray();
        var (kinds, at, _, _) = CwWildHands.Align(decode, key.ToString(), free);
        var laid = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var line in trace.Where(l => l.Contains(" commits ", StringComparison.Ordinal)))
        {
            var parts = line.Split(' ');
            var c = Array.IndexOf(parts, "commits");

            laid[parts[c + 3].Split('-')[0]] = parts[c + 1];
        }

        var patternOf = MorseAlphabet.All.GroupBy(p => p.Value, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().Key, StringComparer.Ordinal);

        string Laid(int d) => laid.TryGetValue(letters[of[d]].From.ToString("0.00", Inv), out var p) ? p : "?";
        string Sent(int k) => patternOf.TryGetValue(key[k].ToString(), out var p) ? p : "?";

        // The alignment in order: a printed letter and a letter of the truth, or either alone.
        var ops = new List<(int? D, int? K)>();
        var next = 0;

        for (var k = 0; k < key.Length; k++)
        {
            if (at[k] is { } d)
            {
                for (; next < d; next++)
                {
                    if (kinds[next] == CwWildHands.LetterIs.Added)
                    {
                        ops.Add((next, null));
                    }
                }

                ops.Add((d, k));
                next = d + 1;
            }
            else
            {
                ops.Add((null, k));
            }
        }

        output.WriteLine("| second | sent | printed | sent as | laid as | fault |");
        output.WriteLine("|---|---|---|---|---|---|");

        var i = 0;
        var seconds = 0.0;

        while (i < ops.Count)
        {
            if (ops[i].D is { } a && ops[i].K is { } b && decode[a] == key[b])
            {
                seconds = letters[of[a]].From;
                i++;
                continue;
            }

            var run = new List<(int? D, int? K)>();

            for (; i < ops.Count && !(ops[i].D is { } x && ops[i].K is { } y && decode[x] == key[y]); i++)
            {
                run.Add(ops[i]);
            }

            var second = run.Where(r => r.D is not null).Select(r => letters[of[r.D!.Value]].From).DefaultIfEmpty(seconds).First();

            if (second < from || second >= to)
            {
                continue;
            }

            var sent = string.Concat(run.Where(r => r.K is not null).Select(r => Sent(r.K!.Value)));
            var read = string.Concat(run.Where(r => r.D is not null).Select(r => Laid(r.D!.Value)));
            var fault = sent == read ? "gap: the same marks, parted elsewhere"
                : sent.Length != read.Length ? string.Create(Inv, $"marks: {sent.Length} sent, {read.Length} laid")
                : run.All(r => r.D is not null && r.K is not null && Laid(r.D.Value).Length == Sent(r.K.Value).Length) ? "mark: a dit and a dah exchanged"
                : "gap and mark";

            foreach (var r in run)
            {
                var when = r.D is { } dd ? letters[of[dd]].From : second;

                output.WriteLine(string.Create(Inv, $"| {when:0.00} | {(r.K is { } kk ? key[kk].ToString() : "-")} | {(r.D is { } d2 ? decode[d2].ToString() : "-")} | {(r.K is { } k2 ? Sent(k2) : "-")} | {(r.D is { } d3 ? Laid(d3) : "-")} | {fault} |"));
            }
        }
    }

    /// <remarks>The rounds' trace on a recording: what the chain printed, and every decode of the lanes that printed it. Asserts nothing; never on a line.</remarks>
    [Theory]
    [Trait("Category", "Survey")]
    [InlineData("cw-2026-10-08-121324")]
    [InlineData("cw-2026-10-03-221548")]
    [InlineData("w1aw-2026-10-07/piece-03")]
    public void TheRecordingTraced(string recording)
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(recording));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(recording);
        var trace = new List<string>();
        using var on = CwRules.Choose(CwRules.GapLearning);
        var letters = TheSequenceReaderTests.ReadLiveTraced(audio.Samples, audio.SampleRate, pitch, width, trace);

        output.WriteLine(TheRecordingsScoreboardTests.Text(letters));

        foreach (var line in trace.Where(l => l.Contains(" holds ", StringComparison.Ordinal) || l.Contains(" let go ", StringComparison.Ordinal) || l.Contains(" commits ", StringComparison.Ordinal) || l.Contains(" passes, read ", StringComparison.Ordinal) || l.Contains(" warm: ", StringComparison.Ordinal)))
        {
            output.WriteLine(line);
        }
    }

    /// <remarks>
    /// **THE FOUR TRACES** (work instruction 595, section 3): what the shipped judge (the share of `E`, `T` and the placeholder in the whole window, or the gaps
    /// near the lines, over 0.35) and the word judge say of a lane's every decode in a span, and when the terminal moves. Asserts nothing; never on a line.
    /// </remarks>
    [Theory]
    [Trait("Category", "Survey")]
    [InlineData("tightfist-easy")]
    [InlineData("221548")]
    [InlineData("bug-qso")]
    [InlineData("bug-bank")]
    [InlineData("221745")]
    [InlineData("221530")]
    [InlineData("mobile-bank")]
    [InlineData("carrier-5199")]
    [InlineData("carrier-5207")]
    [InlineData("221828")]
    [InlineData("cq-12wpm-5db")]
    public void TheJudgesTraced(string which)
    {
        switch (which)
        {
            case "cq-12wpm-5db":
                {
                    var audio = WavAudio.Read(Path.Combine(SyntheticCq.Folder, which + ".wav"));

                    Judged(audio, 600, 500, 0, 16, null);
                    break;
                }

            case "carrier-5199":
            case "carrier-5207":
                {
                    var carrier = TheShapePicksTheSenderTests.Noise(25, 5192);

                    TheShapePicksTheSenderTests.Key(carrier, TheShapePicksTheSenderTests.RandomKeying(2.5, 23, int.Parse(which[8..], Inv)), 625, 24);
                    Judged(new MonoAudio(8000, carrier), 600, 500, 0, 25, [625]);
                    break;
                }

            case "tightfist-easy":
                {
                    var audio = WavAudio.Read(TheGridStepsAsideTests.Fixture(which));

                    output.WriteLine("truth: " + TheGridStepsAsideTests.Truth(which));
                    Judged(audio, 600, 500, 0, 1000, null);
                    break;
                }

            case "221548":
            case "221745":
            case "221530":
            case "221828":
                {
                    var name = "cw-2026-10-03-" + which;
                    var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(name));
                    var (pitch, width) = TheRecordingsScoreboardTests.RadioState(name);

                    if (which == "221530")
                    {
                        Judged(audio, pitch, width, 0, 30, [500, 600]);
                    }
                    else
                    {
                        Judged(audio, pitch, width, which == "221548" ? 16 : 0, 32, which == "221828" ? null : [500]);
                    }

                    break;
                }

            case "mobile-bank":
                {
                    var hand = new CwWildHand(CwWildBank.Recipes.Single(r => r.Name == "mobile"), OwnSpeed: false, Db: 20, Qso: false);

                    output.WriteLine("truth: " + hand.Text);
                    Judged(hand.Audio(), 600, 500, 0, 70, [Math.Round(CwFistBank.ToneHz / 25) * 25]);
                    break;
                }

            default:
                {
                    var hand = new CwWildHand(CwWildBank.Recipes.Single(r => r.Name == "bug"), OwnSpeed: false, Db: 20, Qso: which == "bug-qso");
                    var tone = Math.Round(CwFistBank.ToneHz / 25) * 25;

                    output.WriteLine("truth: " + hand.Text);
                    Judged(hand.Audio(), 600, 500, 6, 24, [tone - 50, tone, tone + 50]);
                    break;
                }
        }
    }

    private void Judged(MonoAudio audio, double pitch, double width, double from, double to, double[]? lanes)
    {
        var trace = new List<string>();
        using var on = CwRules.Choose(CwRules.GapLearning);
        var letters = TheSequenceReaderTests.ReadLiveTraced(audio.Samples, audio.SampleRate, pitch, width, trace);

        output.WriteLine("printed: " + TheRecordingsScoreboardTests.Text(letters));

        var held = new HashSet<string>(StringComparer.Ordinal);

        foreach (var line in trace.Where(l => l.Contains(" holds ", StringComparison.Ordinal) || l.Contains(" let go ", StringComparison.Ordinal)))
        {
            output.WriteLine(line);
        }

        // The lanes named, or, where none is, every lane whose decode read Morse at some point in the span.
        foreach (var line in trace.Where(l => l.Contains(" passes, read ", StringComparison.Ordinal) && Second(l) >= from && Second(l) <= to))
        {
            var parts = line.Split(' ');
            var hz = double.Parse(parts[2], Inv);

            if (lanes is null ? !line.Contains("morse True", StringComparison.Ordinal) : !lanes.Contains(hz))
            {
                continue;
            }

            var text = line[(line.IndexOf("text `", StringComparison.Ordinal) + 6)..^1];
            string After(string key) => line[(line.IndexOf(key, StringComparison.Ordinal) + key.Length)..].Split(',')[0].Split(' ')[0];
            var words = double.Parse(After(", words "), Inv);
            var counted = int.Parse(After(" of "), Inv);
            var share = double.Parse(After(", letters "), Inv);
            var verdict = counted < CwSequenceReader.ValidityLeast ? "no verdict" : After(", refused ") == "True" ? "REFUSED" : "kept";

            output.WriteLine(string.Create(Inv, $"{parts[0]} s {hz:0} Hz, E,T,■ {share:0.00} {(share > CwSequenceReader.ValidityShare ? "refuses" : "keeps")}, words {words:0.00} of {counted} {(words < CwSequenceReader.ValidWords ? "refuses" : "keeps")}, together {verdict}, near {After(", near ")}, standing {After(", standing ")}; `{text}`"));
        }
    }

    /// <remarks>
    /// **THE FOURTH TRACE, `221828`** (work instruction 595, section 3): the letters lost to the fit's keep rule. At each window, the lane's cold read and, on its last decode,
    /// the gaps, the fit's lines, the share of his gaps near them, and whether <see cref="CwSequenceReader.FitNearMost"/> keeps or refuses the fit. Asserts nothing; never on a line.
    /// </remarks>
    [Fact]
    [Trait("Category", "Survey")]
    public void TheKeepRuleTraced()
    {
        const string Name = "cw-2026-10-03-221828";
        const double HopMs = CwSequenceReader.HopMs;
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav(Name));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState(Name);
        var whole = CwSequenceReader.LaneWidthsHz.Select(w => CwSequenceReader.Envelope(audio.Samples, audio.SampleRate, 600, w)).ToArray();
        var seconds = audio.Samples.Length / (double)audio.SampleRate;
        using var on = CwRules.Choose(CwRules.GapLearning);
        var trace = new List<string>();
        var letters = TheSequenceReaderTests.ReadLiveTraced(audio.Samples, audio.SampleRate, pitch, width, trace, 600);

        output.WriteLine("printed: " + TheRecordingsScoreboardTests.Text(letters));

        foreach (var line in trace.Where(l => l.Contains(" holds ", StringComparison.Ordinal) || l.Contains(" let go ", StringComparison.Ordinal) || l.Contains(" passes, read ", StringComparison.Ordinal)))
        {
            output.WriteLine(line);
        }

        output.WriteLine("| window ends, s | gaps | fit centres, ms | lines, ms | near lines | the fit |");
        output.WriteLine("|---|---|---|---|---|---|");

        for (var end = 2.0; end <= seconds; end += 2)
        {
            var to = (int)(end * 1000 / HopMs);
            var from = Math.Max(0, to - (int)(CwSequenceReader.WindowSeconds * 1000 / HopMs));
            var cut = whole.Select(e => e[from..Math.Min(to, e.Length)]).ToArray();
            var (cold, used) = CwSequenceReader.ReadCold(w => cut[w]);

            if (cold.Passes.Count == 0 || CwSequenceReader.Learn(cold.Segments, cut[used]) is not { } medians)
            {
                continue;
            }

            var count = cut[used].Length;
            var longest = CwSequenceReader.FitWordGapMost * medians.Dit;
            var gaps = cold.Segments.Where(g => g.State is CwSequenceReader.InsideGap or CwSequenceReader.LetterGap or CwSequenceReader.WordGap || (g.State == CwSequenceReader.Silence && g.From > 0 && g.To < count && g.Length <= longest)).Select(g => (double)g.Length).ToList();

            if (gaps.Count == 0)
            {
                continue;
            }

            var (g, gc) = CwSequenceReader.OrderedFit(gaps, [medians.InsideGap, medians.LetterGap, medians.WordGap]);
            var stands = g[1] >= CwSequenceReader.FitLetterStep * g[0] && g[2] >= CwSequenceReader.GapStep * g[1];
            var (a, b) = (Math.Sqrt(g[0] * g[1]), Math.Sqrt(g[1] * g[2]));
            var near = gaps.Count(v => Math.Abs(v - a) <= CwSequenceReader.BoundaryNear * a || Math.Abs(v - b) <= CwSequenceReader.BoundaryNear * b) / (double)gaps.Count;
            var kept = !stands ? "refused, centres too close" : near > CwSequenceReader.FitNearMost ? "REFUSED by FitNearMost" : "kept";

            output.WriteLine(string.Create(Inv, $"| {end:0} | {gaps.Count} | {g[0] * HopMs:0} / {g[1] * HopMs:0} / {g[2] * HopMs:0} | {a * HopMs:0}, {b * HopMs:0} | {near:0.00} | {kept} | `{cold.Text}`"));
        }
    }

    /// <remarks>Round three: a hand's lane in the chain, what it read each decode, and each window's cold starts under the word judge. Asserts nothing; never on a line.</remarks>
    [Theory]
    [Trait("Category", "Survey")]
    [InlineData("mobile", false, 20.0, false)]
    [InlineData("bug", true, 10.0, true)]
    public void TheStartsTraced(string recipe, bool own, double level, bool qso)
    {
        const double HopMs = CwSequenceReader.HopMs;
        var hand = new CwWildHand(CwWildBank.Recipes.Single(r => r.Name == recipe), own, level, qso);
        var audio = hand.Audio();
        var tone = Math.Round(CwFistBank.ToneHz / 25) * 25;
        var whole = CwSequenceReader.LaneWidthsHz.Select(w => CwSequenceReader.Envelope(audio.Samples, audio.SampleRate, tone, w)).ToArray();
        using var on = CwRules.Choose(CwRules.GapLearning);
        var trace = new List<string>();
        var letters = TheSequenceReaderTests.ReadLiveTraced(audio.Samples, audio.SampleRate, 600, 500, trace, tone);

        output.WriteLine("truth: " + hand.Text);
        output.WriteLine("printed: " + TheRecordingsScoreboardTests.Text(letters));

        foreach (var line in trace.Where(l => l.Contains(" holds ", StringComparison.Ordinal) || l.Contains(" let go ", StringComparison.Ordinal) || l.Contains(" warm: ", StringComparison.Ordinal) || l.Contains(" passes, read ", StringComparison.Ordinal)))
        {
            output.WriteLine(line.Length > 330 ? line[..200] + " ... " + line[^100..] : line);
        }

        for (var end = 12.0; end <= audio.Samples.Length / (double)audio.SampleRate; end += 4)
        {
            var hops = (int)(end * 1000 / HopMs);
            var db = whole[0][(hops - (int)(CwSequenceReader.WindowSeconds * 1000 / HopMs))..Math.Min(hops, whole[0].Length)];
            var levels = CwSequenceReader.LevelsFromPercentiles(db);

            output.WriteLine(string.Create(Inv, $"### window ends {end:0} s"));

            foreach (var dit in new[] { CwSequenceReader.GuessDitHops(db) }.Concat(CwSequenceReader.ColdStartDits).Distinct())
            {
                var ranges = CwSequenceReader.ColdRanges(dit);
                var first = CwSequenceReader.Read(db, ranges, levels, passes: 1);
                var v = CwSequenceReader.Validity(first);
                var all = first.Text.Where(c => c != (char)32).ToList();
                var old = all.Count == 0 ? 1 : all.Count(c => c is (char)69 or (char)84 || c.ToString() == MorseAlphabet.Unreadable) / (double)all.Count;
                var score = (CwSequenceReader.LogLikelihood(first.Segments, db, levels) - CwSequenceReader.PriorCost(first.Segments, ranges)) / db.Length;

                output.WriteLine(string.Create(Inv, $"start {dit * HopMs:0} ms: score/hop {score:0.000}, Morse {first.Pattern.IsMorse}, words {v.Words:0.00} of {v.Counted}, covered {CwVocabulary.Covered(first.Text):0.00}, E,T,â–  {old:0.00}, near {v.Gaps:0.00}: `{first.Text}`"));
            }
        }
    }

    /// <remarks>The twelve-plus-three, the rule on, every stretch. Asserts nothing; never on a line.</remarks>
    [Fact]
    [Trait("Category", "Survey")]
    public void TheBoardTraced()
    {
        using var pinned = CwRules.Off(CwRules.LearnedReader);
        using var on = CwRules.Choose(CwRules.GapLearning);
        output.WriteLine(TheRecordingsScoreboardTests.Table(TheRecordingsScoreboardTests.Score()));
    }

    /// <remarks>W1AW's stretches where the rule on and off differ, each with what it printed and its figures. Asserts nothing; never on a line.</remarks>
    [Fact]
    [Trait("Category", "Survey")]
    public void TheW1awTraced()
    {
        using var pinned = CwRules.Off(CwRules.LearnedReader);
        var off = TheW1awTableTests.Score();
        using var on = CwRules.Choose(CwRules.GapLearning);
        var chosen = TheW1awTableTests.Score();

        output.WriteLine(string.Create(Inv, $"off {off.Score}, on {chosen.Score}"));

        foreach (var (a, b) in off.Stretches.Zip(chosen.Stretches).Where(p => p.First.PrintedText != p.Second.PrintedText))
        {
            output.WriteLine(string.Create(Inv, $"{a.Stretch.Recording[^8..]} {a.Stretch.From:0.#}-{a.Stretch.To:0.#} s, {a.Stretch.Confidence}: right {a.Right} to {b.Right}, wrong {a.Wrong} to {b.Wrong}"));
            output.WriteLine("  ref `" + a.Stretch.Reference + "`");
            output.WriteLine("  off `" + a.PrintedText + "`");
            output.WriteLine("  on  `" + b.PrintedText + "`");
        }
    }

    /// <remarks>Section 4: the carrier keyed at random and the rest of the noise, read with the rule off and on. Asserts nothing; never on a line.</remarks>
    [Fact]
    [Trait("Category", "Survey")]
    public void TheNoiseBothWays()
    {
        using var pinned = CwRules.Off(CwRules.LearnedReader);

        foreach (var ruleOn in new[] { false, true })
        {
            using var chosen = ruleOn ? CwRules.Choose(CwRules.GapLearning) : null;
            var runs = TheRecordingsScoreboardTests.NoiseRuns();

            output.WriteLine(string.Create(Inv, $"## the rule {(ruleOn ? "on" : "off")}: the random carrier printed at {runs.Count(n => n.What.StartsWith("a carrier", StringComparison.Ordinal) && n.Reads.Length > 0)} seeds of 20"));

            foreach (var (what, reads) in runs)
            {
                output.WriteLine($"{what}: `{reads}`");
            }
        }
    }

    /// <remarks>
    /// The rounds' quick measure: the wild bank by recipe, read in parallel as the survey reads it. The survey's figures are the record's; these
    /// say whether a round is worth one. Asserts nothing; never on a line.
    /// </remarks>
    [Fact]
    [Trait("Category", "Survey")]
    public void TheQuickMeasure()
    {
        var rows = new System.Collections.Concurrent.ConcurrentBag<(CwWildHand Hand, TheCwSurveyTests.Row Row)>();
        using var pinned = CwRules.Off(CwRules.LearnedReader);
        using var on = CwRules.Choose(CwRules.GapLearning);
        var rules = CwRules.Current;

        Parallel.ForEach(CwWildBank.All, new ParallelOptions { MaxDegreeOfParallelism = 4 }, hand =>
        {
            using var here = CwRules.Use(rules);

            rows.Add((hand, TheCwSurveyTests.Wild(hand)));
        });

        foreach (var recipe in CwWildBank.Recipes)
        {
            var mine = rows.Where(r => r.Hand.Recipe == recipe).ToList();

            output.WriteLine(string.Create(Inv, $"{recipe.Name}: {mine.Sum(r => r.Row.Right ?? 0)} right, {mine.Sum(r => r.Row.Wrong ?? 0)} wrong, {mine.Sum(r => r.Row.Invented ?? 0)} invented, score {mine.Sum(r => r.Row.Score ?? 0)}; {string.Join(", ", mine.OrderBy(r => r.Hand.Name, StringComparer.Ordinal).Select(r => r.Row.Score ?? 0))}"));
        }

        output.WriteLine(string.Create(Inv, $"total: {rows.Sum(r => r.Row.Right ?? 0)} right, {rows.Sum(r => r.Row.Wrong ?? 0)} wrong, {rows.Sum(r => r.Row.Invented ?? 0)} invented, score {rows.Sum(r => r.Row.Score ?? 0)}"));

        var board = TheRecordingsScoreboardTests.Score();
        var w1aw = TheW1awTableTests.Score();

        output.WriteLine(string.Create(Inv, $"the twelve-plus-three {board.Score}, limits as guarded {(board.LimitsHold ? "hold" : "do not hold")}; W1AW {w1aw.Score}"));
        output.WriteLine("the first recording: " + board.FirstReads);
        output.WriteLine(string.Create(Inv, $"letters in silences: {board.SilencePrints.Count}"));

        foreach (var n in board.Noise)
        {
            output.WriteLine($"{n.What}: `{n.Reads}`");
        }
    }

    private sealed record Row(
        int Dit, string Kind, double Score, bool Morse, double Letters, double Near, bool Validity, string Medians, string Fit,
        string Median3, string TextMedian, string Fit3, int RightFit, string TextFit);
}
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
            var valid = rows.Where(r => r.Kind != "built" && r.Validity <= CwSequenceReader.ValidityShare).ToList();
            var chosen = valid.Count > 0 ? Taken(valid, r => r.Score) : rows.Where(r => r.Kind != "built").MinBy(r => r.Validity)!;
            var bestEdits = rows.MaxBy(r => r.RightFit)!;

            output.WriteLine(string.Empty);
            output.WriteLine(string.Create(Inv, $"### window {end - CwSequenceReader.WindowSeconds:0}-{end:0} s"));
            output.WriteLine("| start | ranges | score/hop | Morse | E,T,■ | near lines | one pass, medians ig/lg/wg | one pass, fit ig/lg/wg | three by medians: right of letters, spaces added, last classes | three by the fit: right of letters, spaces added, last classes | taken |");
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

        return new Row(dit, kind, score, first.Pattern.IsMorse, validity.Letters, validity.Gaps, validity.Figure, Show(medians), Show(fit), string.Create(Inv, $"{a.Right} of {a.Letters}, {a.Added}+ {Show(median3.Last)}"), median3.Text, string.Create(Inv, $"{b.Right} of {b.Letters}, {b.Added}+ {Show(fit3.Last)}"), b.Right, fit3.Text);
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

    /// <remarks>Section 3.4: the fit finds a hand's three classes from a poor start, and each class's members.</remarks>
    [Fact]
    public void TheFitFindsAHandsClasses()
    {
        double[] gaps = [20, 21, 22, 23, 24, 25, 60, 62, 64, 66, 68, 150, 160, 170];
        var (centres, counts) = CwSequenceReader.OrderedFit(gaps, [20, 30, 100]);

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
    /// The rounds' quick measure: the wild bank by recipe, read in parallel as the survey reads it. The survey's figures are the record's; these
    /// say whether a round is worth one. Asserts nothing; never on a line.
    /// </remarks>
    [Fact]
    [Trait("Category", "Survey")]
    public void TheQuickMeasure()
    {
        var rows = new System.Collections.Concurrent.ConcurrentBag<(CwWildHand Hand, TheCwSurveyTests.Row Row)>();
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
        int Dit, string Kind, double Score, bool Morse, double Letters, double Near, double Validity, string Medians, string Fit,
        string Median3, string TextMedian, string Fit3, int RightFit, string TextFit);
}
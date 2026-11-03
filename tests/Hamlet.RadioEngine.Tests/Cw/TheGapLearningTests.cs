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
            using var off = on ? null : CwRules.Off(CwRules.GapLearning);
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

    private sealed record Row(
        int Dit, string Kind, double Score, bool Morse, double Letters, double Near, double Validity, string Medians, string Fit,
        string Median3, string TextMedian, string Fit3, int RightFit, string TextFit);
}
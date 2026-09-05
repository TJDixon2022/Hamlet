using Ft8Sharp.Deep;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>What a third and fourth hearing buy over a second, on the ladder, below the single-slot
/// crossing.</b> Three methods, split so no one of them approaches the watchdog.
/// </summary>
/// <remarks>
/// <para>
/// <b>THE BREAKAGE THESE WOULD HAVE CAUGHT.</b> Until unit 254 <c>Ft8DeepRepeatDecoder</c> summed two
/// hearings at a time however many slots it remembered, and <c>RunRepeats</c> labelled its third
/// column from the repeat count, so <b>a column headed <c>combined x4</c> measured the 3.01 dB two
/// hearings are worth while its name claimed the 6.02 that four are</b>. Nothing in the tree could
/// have told a reader that: no test asserted anything about three hearings and no count reported the
/// depth of a sum. <see cref="Ft8LadderHarness.RepeatsRun.DeepestHearings"/> is printed on every row
/// below and is the number that catches it.
/// </para>
/// <para>
/// <b>THE ISOLATION, STATED AS ONE.</b> 4a compares <em>the same repeat count with accumulation on
/// and off</em>. A 2-versus-4 comparison is not the isolation: <c>RunRepeats</c> scores the combined
/// column on the union over the trial's slots, so four slots is four single-slot attempts as well as
/// deeper sums, and the two effects would be conflated.
/// </para>
/// <para>
/// <b>Nothing here asserts a rate.</b> Zero wrong is asserted on every row, and every combined decode
/// is checked against the message the ladder knows it transmitted. A column that returns nothing is a
/// measurement.
/// </para>
/// </remarks>
public class Ft8Unit254DepthLadderTests(ITestOutputHelper output)
{
    /// <summary>1.2 dB below the single-slot 50 per cent crossing of -19.81 dB.</summary>
    private const double Rung = -21.0;

    /// <summary>Unit 247's jitter, and the honest case: a real station's oscillator moves.</summary>
    private const double FrequencyJitterHz = 2.0;

    /// <summary>Unit 247's jitter: 480 samples, 0.040 s.</summary>
    private const int OffsetJitterSamples = 480;

    /// <summary>One whole block of the 51-message population.</summary>
    private const int Block = 51;

    /// <summary>Six whole blocks, and the count every figure in this phase was taken at.</summary>
    private const int Trials = 306;

    /// <summary>
    /// <b>4a — THE DEPTH SWEEP, AND THE CLEAN ISOLATION.</b> One block of 51 trials, jittered, at
    /// -21 dB: <c>repeats = 3</c> and <c>repeats = 4</c>, each of them chained pairwise and then
    /// accumulated, with nothing else different between the two.
    /// </summary>
    /// <remarks>
    /// <b>This is where the answer exists even if the night ends early.</b> The history depth is
    /// <c>repeats - 1</c> in every configuration, so the pairwise and the accumulated column reach
    /// back over exactly the same slots and the only difference is how many hearings go into one call
    /// of <c>Ft8DeepSoftCombiner.Combine</c>.
    /// </remarks>
    [Fact]
    public void TheDepthSweepOverOneBlockOf51TrialsJitteredAtMinus21Decibels()
    {
        output.WriteLine("=========================================================================");
        output.WriteLine(
            $"UNIT 254 TASK 4a - DEPTH SWEEP AT {Rung:F1} dB, {Block} TRIALS, JITTERED");
        output.WriteLine(
            $"  the later slots sit {FrequencyJitterHz:F2} Hz and {OffsetJitterSamples} samples "
            + "further on, each from the one before it");
        output.WriteLine("  THE ISOLATION: the same repeat count with accumulation on and off.");
        output.WriteLine("=========================================================================");
        output.WriteLine(string.Empty);

        var runs = new List<(string Label, Ft8LadderHarness.RepeatsRun Run)>();

        foreach (var repeats in new[] { 3, 4 })
        {
            var history = repeats - 1;

            foreach (var accumulation in new[] { 1, history })
            {
                var settings = new Ft8DeepCombineSettings(
                    historyDepth: history, accumulationDepth: accumulation);

                var run = Ft8LadderHarness.RunRepeats(
                    Rung,
                    Block,
                    repeats: repeats,
                    frequencyJitterHz: FrequencyJitterHz,
                    offsetJitterSamples: OffsetJitterSamples,
                    combining: settings);

                var label = accumulation == 1
                    ? $"x{repeats} PAIRWISE"
                    : $"x{repeats} ACCUMULATED";

                runs.Add((label, run));
                Row(label, repeats, history, accumulation, run);
            }
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  WHAT ACCUMULATION BOUGHT, PAIRED ON IDENTICAL AUDIO:");

        for (var i = 0; i < runs.Count; i += 2)
        {
            var pairwise = runs[i];
            var accumulated = runs[i + 1];
            var (onlyPairwise, onlyAccumulated) = Ft8LadderHarness.Discordance(
                pairwise.Run.Rows[2], accumulated.Run.Rows[2]);

            output.WriteLine(
                $"    {pairwise.Label,-18} {pairwise.Run.Rows[2].Decoded,3} of {Block}   vs   "
                + $"{accumulated.Label,-18} {accumulated.Run.Rows[2].Decoded,3} of {Block}   "
                + $"net {Signed(accumulated.Run.Rows[2].Decoded - pairwise.Run.Rows[2].Decoded),3}");
            output.WriteLine(
                $"      discordant: only pairwise {onlyPairwise}, only accumulated "
                + $"{onlyAccumulated}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            "  WHAT THE TRACE PREDICTED (docs/unit254-combining-depth.md 1.4): summing R hearings is");
        output.WriteLine(
            "  10 log10 R dB - 3.01 at two, 4.77 at three, 6.02 at four - so the third hearing is");
        output.WriteLine(
            "  worth 1.76 dB and the fourth 1.25 dB. The prediction on the record before this run was");
        output.WriteLine(
            "  that accumulated x4 beats pairwise x4 by between 0 and 10 of 51, much less than the");
        output.WriteLine(
            "  gain suggests, because a chain needs the search to offer a candidate in EVERY slot.");

        foreach (var (label, run) in runs)
        {
            AssertRowsAreClean(label, run);
        }
    }

    /// <summary>
    /// <b>4b — THE SCOREBOARD, 306 TRIALS, JITTERED, AT -21 dB.</b> Four columns: the port; single
    /// slot with ordered statistics; combined x2, the pairwise before; and the accumulated four-hearing
    /// sum, the after.
    /// </summary>
    /// <remarks>
    /// <b>Two walks of the ladder, not one.</b> <c>RunRepeats</c> reports three columns for one repeat
    /// count, so the two-repeat walk supplies the port, the ordered statistics column and the pairwise
    /// combined column, and the four-repeat walk supplies the accumulated one. The port and the
    /// ordered statistics columns appear in both and <b>must agree exactly</b> — they see the same
    /// first slot at the same seed — which is a free check that the two walks are the same experiment.
    /// </remarks>
    [Fact]
    public void TheScoreboardOver306TrialsJitteredAtMinus21Decibels()
    {
        output.WriteLine("=========================================================================");
        output.WriteLine($"UNIT 254 TASK 4b - THE SCOREBOARD AT {Rung:F1} dB, {Trials} TRIALS, JITTERED");
        output.WriteLine("=========================================================================");
        output.WriteLine(string.Empty);

        var pairwise = Ft8LadderHarness.RunRepeats(
            Rung,
            Trials,
            repeats: 2,
            frequencyJitterHz: FrequencyJitterHz,
            offsetJitterSamples: OffsetJitterSamples);

        var accumulated = Ft8LadderHarness.RunRepeats(
            Rung,
            Trials,
            repeats: 4,
            frequencyJitterHz: FrequencyJitterHz,
            offsetJitterSamples: OffsetJitterSamples,
            combining: new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3));

        var columns = new[]
        {
            pairwise.Rows[0],
            pairwise.Rows[1],
            pairwise.Rows[2],
            accumulated.Rows[2],
        };

        foreach (var line in Ft8LadderHarness.Report(columns))
        {
            output.WriteLine(line);
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  THE TWO-REPEAT WALK, WHOLE:");
        foreach (var line in Ft8LadderHarness.RepeatsReport(pairwise))
        {
            output.WriteLine(line);
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  THE FOUR-REPEAT ACCUMULATED WALK, WHOLE:");
        foreach (var line in Ft8LadderHarness.RepeatsReport(accumulated))
        {
            output.WriteLine(line);
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  THE DISCORDANT COUNTS AGAINST combined x2, on identical audio:");
        foreach (var other in new[] { columns[0], columns[1], columns[3] })
        {
            var (onlyCombined, onlyOther) = Ft8LadderHarness.Discordance(columns[2], other);
            output.WriteLine(
                $"    combined x2 vs {other.Decoder,-13}  only combined x2 {onlyCombined,4}   "
                + $"only {other.Decoder} {onlyOther,4}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            "  The port and the ordered statistics columns are read twice, once in each walk, and");
        output.WriteLine("  must agree: they see the same first slot at the same seed.");
        output.WriteLine(
            $"    port         two-repeat walk {pairwise.Rows[0].Decoded,4}   four-repeat walk "
            + $"{accumulated.Rows[0].Decoded,4}");
        output.WriteLine(
            $"    single + OSD two-repeat walk {pairwise.Rows[1].Decoded,4}   four-repeat walk "
            + $"{accumulated.Rows[1].Decoded,4}");

        Assert.Equal(pairwise.Rows[0].Decoded, accumulated.Rows[0].Decoded);
        Assert.Equal(pairwise.Rows[1].Decoded, accumulated.Rows[1].Decoded);

        AssertRowsAreClean("combined x2 walk", pairwise);
        AssertRowsAreClean("summed x4 walk", accumulated);
    }

    /// <summary>
    /// <b>4c — THE STACK, AND IT IS WHAT HAMLET ACTUALLY SHIPS.</b> Combining with fine sync on and
    /// ordered statistics on at the settled default, against combining alone, at the same rung with
    /// the same jitter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>NOT PART OF THE ISOLATION, and labelled so.</b> 4a's comparison holds everything but the
    /// accumulation depth fixed; this one changes two whole stages at once. It exists because
    /// <c>src/Hamlet.RadioEngine/Audio/Ft8Reception.cs</c> runs with both stages on and the combined
    /// column has never been measured that way — unit 247 §5 item 1 says in terms that the two stack
    /// in principle and were not run stacked, and item 2 says placement jitter cost more than half
    /// the combining gain and fine sync, which did not exist then, is what would recover it.
    /// </para>
    /// <para>
    /// <b>This is the run that says whether it does.</b>
    /// </para>
    /// </remarks>
    [Fact]
    public void TheStackOver306TrialsJitteredAtMinus21Decibels()
    {
        output.WriteLine("=========================================================================");
        output.WriteLine($"UNIT 254 TASK 4c - THE STACK AT {Rung:F1} dB, {Trials} TRIALS, JITTERED");
        output.WriteLine("  NOT PART OF THE ISOLATION: two stages change at once, deliberately.");
        output.WriteLine("=========================================================================");
        output.WriteLine(string.Empty);

        var alone = Ft8LadderHarness.RunRepeats(
            Rung,
            Trials,
            repeats: 2,
            frequencyJitterHz: FrequencyJitterHz,
            offsetJitterSamples: OffsetJitterSamples);

        var stacked = Ft8LadderHarness.RunRepeats(
            Rung,
            Trials,
            repeats: 2,
            frequencyJitterHz: FrequencyJitterHz,
            offsetJitterSamples: OffsetJitterSamples,
            combinedOsd: Ft8DeepOsdSettings.Default,
            combinedFineSync: Ft8DeepFineSyncSettings.Default);

        var columns = new[]
        {
            alone.Rows[0],
            alone.Rows[1],
            alone.Rows[2],
            stacked.Rows[2],
        };

        // The two combined rows carry the same label, so the stacked one is renamed for the report.
        output.WriteLine(
            "  Row 3 is combining ALONE (ordered statistics off, fine sync off) and row 4 is the");
        output.WriteLine("  SAME combining with both stages on at their settled defaults.");
        output.WriteLine(string.Empty);

        foreach (var line in Ft8LadderHarness.Report(columns))
        {
            output.WriteLine(line);
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  COMBINING ALONE, WHOLE:");
        foreach (var line in Ft8LadderHarness.RepeatsReport(alone))
        {
            output.WriteLine(line);
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  COMBINING STACKED WITH FINE SYNC AND ORDERED STATISTICS, WHOLE:");
        foreach (var line in Ft8LadderHarness.RepeatsReport(stacked))
        {
            output.WriteLine(line);
        }

        var (onlyAlone, onlyStacked) = Ft8LadderHarness.Discordance(alone.Rows[2], stacked.Rows[2]);
        var (onlyOsd, onlyStackedOverOsd) = Ft8LadderHarness.Discordance(
            alone.Rows[1], stacked.Rows[2]);

        output.WriteLine(string.Empty);
        output.WriteLine("  THE DISCORDANT COUNTS, on identical audio:");
        output.WriteLine(
            $"    combining alone {alone.Rows[2].Decoded} of {Trials}  vs  stacked "
            + $"{stacked.Rows[2].Decoded} of {Trials}   only alone {onlyAlone}, only stacked "
            + $"{onlyStacked}");
        output.WriteLine(
            $"    single + OSD {alone.Rows[1].Decoded} of {Trials}  vs  stacked "
            + $"{stacked.Rows[2].Decoded} of {Trials}   only OSD {onlyOsd}, only stacked "
            + $"{onlyStackedOverOsd}");

        AssertRowsAreClean("combining alone", alone);
        AssertRowsAreClean("combining stacked", stacked);
    }

    /// <summary>A difference with its sign on it, so a reader cannot mistake a drop for a gain.</summary>
    private static string Signed(int difference) =>
        difference > 0 ? $"+{difference}" : difference.ToString();

    /// <summary>One configuration's row, with everything work instruction 254 task 4a asks for.</summary>
    private void Row(
        string label,
        int repeats,
        int history,
        int accumulation,
        Ft8LadderHarness.RepeatsRun run)
    {
        var combined = run.Rows[2];
        var (lower, upper) = combined.Interval;

        output.WriteLine(
            $"{label,-18} repeats {repeats}  history {history}  accumulation {accumulation}");
        output.WriteLine(
            $"    decoded {combined.Decoded,3} of {combined.Trials}  wrong {combined.Wrong,3}  "
            + $"rate {combined.Rate,5:F1} ({lower:F1}-{upper:F1})  only-combined {run.OnlyCombined,3}  "
            + $"lost {run.LostByCombining,2}");
        output.WriteLine(
            $"    submitted {run.CombinationsSubmitted,6}  accepted {run.CodewordsAccepted,5}  "
            + $"DEEPEST COMBINATION {run.DeepestHearings} hearings  "
            + $"added {run.CombinedDecodes,3} of which {run.CombinedDecodesVerified,3} verified");
        output.WriteLine(
            $"    {combined.MillisecondsPerTrial,7:F1} ms a trial   worst slot "
            + $"{run.WorstSlotMilliseconds,6:F1} ms ({15000.0 / run.WorstSlotMilliseconds:F0}x "
            + "against 15 s)");
        output.WriteLine(string.Empty);
    }

    /// <summary>
    /// <b>The only assertions in this file, and they are the two this phase cannot trade.</b> A
    /// message nobody sent, on any row at any depth; and a combined decode that was not checked
    /// against the message the ladder knows it transmitted.
    /// </summary>
    private void AssertRowsAreClean(string label, Ft8LadderHarness.RepeatsRun run)
    {
        foreach (var row in run.Rows)
        {
            if (row.Wrong != 0)
            {
                foreach (var wrong in row.WrongReturns)
                {
                    output.WriteLine($"  {label} / {row.Decoder}: {wrong}");
                }
            }

            Assert.Equal(0, row.Wrong);
        }

        Assert.Equal(0, run.LostByCombining);
        Assert.Equal(run.CombinedDecodes, run.CombinedDecodesVerified);
    }
}

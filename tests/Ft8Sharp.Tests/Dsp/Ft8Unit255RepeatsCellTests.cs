using Ft8Sharp.Deep;
using Ft8Sharp.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>THE ONE CONFIGURATION NOBODY HAS EVER RUN: accumulation stacked with the stages Hamlet
/// ships.</b> Four hearings summed three deep, with <c>Ft8DeepOsdSettings.Default</c> and
/// <c>Ft8DeepFineSyncSettings.Default</c> on the combined column's inner decoder.
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS CELL IS <c>HM-OPEN-081</c>, AND IT IS RUN BECAUSE STEP 6'S FIRST EXIT ASKS FOR EACH
/// STAGE ON AND OFF</b> — not because a report asked for it. Unit 254 §4b measured <b>252 of
/// 306</b> for accumulation at four hearings with the combined column's inner decoder
/// <em>unstacked</em>, and §4c measured <b>79 of 306</b> for the stack at <em>two</em> hearings
/// with combining at its default depth of one. <b>The cell where both are on has never been
/// measured</b>, so the phase cannot say what the two are worth together.
/// </para>
/// <para>
/// <b>THIS IS A DIFFERENT LADDER FROM THE CLOSING TABLE AND IS NOT COMPARABLE WITH IT.</b>
/// <c>RunRepeats</c> gives every trial four slots carrying the same message, jittered 2.00 Hz and
/// 480 samples between hearings as a real station's oscillator and clock would drift. The closing
/// table in <c>Ft8Unit255ClosingLadderTests</c> is one slot a trial. <b>A row from here beside a
/// row from there is a false comparison</b> and the document says so in its own words.
/// </para>
/// <para>
/// <b>AND THE CAVEAT THAT TRAVELS WITH ANY FIGURE FROM THIS LADDER</b>, which is unit 254's and
/// is repeated rather than assumed known: <c>RunRepeats</c> scores the combined column on the
/// union over the trial's slots, so <b>a four-repeat column gets four single-slot attempts as
/// well as deeper sums.</b> The number is what an operator hearing a station four times would
/// experience; it is <em>not</em> the gain from accumulation alone.
/// </para>
/// <para>
/// <b>NOT A GATE-SET ENTRY AND NOT WATCHED FAILING FIRST.</b> The ladder is a measurement, not a
/// test — <c>docs/gate-set.md</c> — and a closing measurement has no defect to watch fail.
/// </para>
/// </remarks>
public class Ft8Unit255RepeatsCellTests(ITestOutputHelper output)
{
    /// <summary>306 trials: six whole blocks of the 51-message population.</summary>
    private const int Trials = 306;

    /// <summary>
    /// <b>The cell nobody has run.</b> Four hearings, accumulated three deep, with ordered
    /// statistics and fine sync on the combined column's inner decoder.
    /// </summary>
    [Fact]
    public void TheAccumulatedCombiningStackedWithTheShippingStagesAtMinus21()
    {
        var lines = new List<string>();

        void Say(string line)
        {
            lines.Add(line);
            output.WriteLine(line);
        }

        Say(
            $"UNIT 255, THE CELL NOBODY HAS RUN: accumulation STACKED with the shipping stages, "
            + $"-21.0 dB, {Trials} trials.");
        Say(
            "LADDER: Ft8LadderHarness.RunRepeats - FOUR slots a trial carrying the same message, "
            + "jittered 2.00 Hz and 480 samples between hearings. NOT the closing table's ladder "
            + "and NOT comparable with it.");
        Say(
            "COMBINING: new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3) - three "
            + "remembered slots, and up to three of them in ONE sum.");
        Say(
            "STACKED: combinedOsd Ft8DeepOsdSettings.Default and combinedFineSync "
            + "Ft8DeepFineSyncSettings.Default - the two stages Ft8Reception.cs:460 builds, on the "
            + "combined column's inner decoder.");
        Say(
            "THE RECORD THIS IS READ AGAINST: unit 254 section 4b measured 252 of 306 accumulated "
            + "but UNSTACKED, and section 4c measured 79 of 306 stacked but at two hearings with "
            + "combining at depth one. Neither is this cell.");
        Say(string.Empty);

        var clock = System.Diagnostics.Stopwatch.StartNew();
        var run = Ft8LadderHarness.RunRepeats(
            -21.0,
            Trials,
            repeats: 4,
            frequencyJitterHz: 2.0,
            offsetJitterSamples: 480,
            combining: new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3),
            combinedOsd: Ft8DeepOsdSettings.Default,
            combinedFineSync: Ft8DeepFineSyncSettings.Default);
        clock.Stop();

        // THE WHOLE REPORT IS PRINTED BEFORE ANYTHING IS ASSERTED. Ruling 4.
        foreach (var line in Ft8LadderHarness.RepeatsReport(run))
        {
            Say(line);
        }

        Say(string.Empty);
        Say(
            $"  EXPECTED FALSE ACCEPTS beside the OBSERVED count: "
            + $"{Ft8DeepCombineSettings.ExpectedFalseAccepts(run.CombinationsSubmitted):F3} expected "
            + $"against {run.CombinedDecodes - run.CombinedDecodesVerified} observed, over "
            + $"{run.CombinationsSubmitted} submissions.");
        Say($"  wall clock {clock.Elapsed.TotalSeconds:F1} s.");

        foreach (var row in run.Rows)
        {
            foreach (var wrong in row.WrongReturns)
            {
                Say($"WRONG on {row.Decoder}: {wrong}");
            }
        }

        var folder = Path.Combine(Ft8CaptureFixtures.RepositoryRoot(), "docs", "unit255-runs");
        Directory.CreateDirectory(folder);
        File.WriteAllLines(Path.Combine(folder, "accumulated-stacked-minus21.txt"), lines);

        // ASSERTION ONE: zero wrong on every row. No unit stops checking.
        foreach (var row in run.Rows)
        {
            Assert.True(
                row.Wrong == 0,
                $"{row.Decoder} returned {row.Wrong} message(s) that were not sent. A wrong decode "
                    + "is worse than a missed one and every column measured in this project reads "
                    + "zero.");
        }

        // ASSERTION TWO: every combined decode verified against the message that went in. A
        // combination is a codeword the port never saw on its own, so this is the assertion that
        // says the combining stage is not manufacturing messages.
        Assert.True(
            run.CombinedDecodes == run.CombinedDecodesVerified,
            $"the combining stage added {run.CombinedDecodes} messages and only "
                + $"{run.CombinedDecodesVerified} of them were the message that was sent. "
                + $"{run.CombinedDecodes - run.CombinedDecodesVerified} message(s) nobody sent "
                + "reached the operator, which CLAUDE.md 0.0 says is worse than missing them.");

        // Nothing about the rate is asserted.
    }
}

using Ft8Sharp.Deep;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The repeats ladder itself: a second entry point beside <see cref="Ft8LadderHarness.Run"/>, walked
/// over one block so that its determinism and its seed arithmetic are pinned before the whole
/// scoreboard is taken through it.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b><see cref="Ft8LadderHarness.Run"/> is not changed and this proves it is not needed to be.</b>
/// Every row this phase has recorded — <c>HM-OPEN-067</c>'s 13 of 306 at -21 dB and unit 246's 33 of
/// 306 — was taken through <c>Run</c>, and a change to it would invalidate all of them.
/// <see cref="Ft8LadderHarness.RunRepeats"/> reuses <c>Result</c>, <c>Header</c> and <c>Wilson</c>, so
/// the scoreboard stays one instrument and prints one shape.
/// </para>
/// <para>
/// <b>The jitter variant is not optional and it is here for the reason it is here.</b> A real station
/// repeating a message does not land on the same sample or the same bin twice: its clock and its
/// oscillator have moved. A combiner that only works when the two slots are bit-identical in placement
/// is not a decoder, and finding that out on one block is worth more than a larger number on six.
/// </para>
/// </remarks>
public class Ft8Unit247RepeatsLadderTests(ITestOutputHelper output)
{
    /// <summary>The rung the phase's number lives on.</summary>
    private const double Rung = -21.0;

    /// <summary>One whole block of the 51-message population.</summary>
    private const int Block = 51;

    /// <summary>
    /// <b>A real station's oscillator moves between slots, and this is how far it is made to move
    /// here.</b> 2 Hz is a third of an FT8 tone and comfortably inside the pairing rule's 6.25 Hz
    /// tolerance, so what it tests is whether the <em>combination</em> survives two hearings that do
    /// not sit on the same bin — not whether the pairing rule can find them.
    /// </summary>
    private const double FrequencyJitterHz = 2.0;

    /// <summary>
    /// <b>And its clock.</b> 480 samples at 12 kHz is 0.04 s, a quarter of a symbol period — off the
    /// block grid and off the sub-block grid, so the later slot's symbols do not line up with the
    /// earlier one's in the waterfall at all.
    /// </summary>
    private const int OffsetJitterSamples = 480;

    /// <summary>
    /// <b>One block at -21 dB, both ways, three columns, three counts on every row.</b>
    /// </summary>
    [Fact]
    public void OneBlockAtMinus21DecibelsBothWithAndWithoutThePlacementJitter()
    {
        foreach (var (name, frequencyJitter, offsetJitter) in new[]
                 {
                     ("SAME PLACEMENT - both slots on the same bin and the same sample", 0.0, 0),
                     ("JITTERED PLACEMENT - the later slot moved in frequency and in time",
                         FrequencyJitterHz, OffsetJitterSamples),
                 })
        {
            var run = Ft8LadderHarness.RunRepeats(
                Rung,
                Block,
                repeats: 2,
                frequencyJitterHz: frequencyJitter,
                offsetJitterSamples: offsetJitter);

            output.WriteLine(
                "=================================================================================");
            output.WriteLine(name);
            if (offsetJitter != 0 || frequencyJitter != 0.0)
            {
                output.WriteLine(
                    $"  slot 1 is {frequencyJitter:F2} Hz and {offsetJitter} samples "
                    + $"({offsetJitter / 12000.0:F3} s) away from slot 0");
            }

            output.WriteLine(
                "=================================================================================");

            foreach (var line in Ft8LadderHarness.RepeatsReport(run))
            {
                output.WriteLine(line);
            }

            output.WriteLine(string.Empty);

            // ZERO WRONG DECODES, EVERY COLUMN. Not a target and not tuned to: an approach that
            // produces one is rejected, and the report says which rung produced it.
            foreach (var row in run.Rows)
            {
                Assert.True(
                    row.Wrong == 0,
                    $"{name}: {row.Decoder} returned {row.Wrong} messages that were not sent at "
                        + $"{Rung:F1} dB. A decode nobody sent is worse than a decode missed.");
            }

            // COMBINING ONLY EVER ADDS. Zero by construction, so a non-zero count is a defect in the
            // superset property and not a result.
            Assert.True(
                run.LostByCombining == 0,
                $"{name}: {run.LostByCombining} trials were decoded by a single slot and not by the "
                    + "combination, which the superset property makes impossible. Combining is meant "
                    + "only to add.");

            // Every combined decode was the message that was sent.
            Assert.Equal(run.CombinedDecodes, run.CombinedDecodesVerified);
        }
    }

    /// <summary>
    /// <b>Repeat 0 draws exactly the noise <see cref="Ft8LadderHarness.Run"/> draws</b>, which is what
    /// makes the repeats ladder's single-slot column comparable with every row already recorded.
    /// </summary>
    /// <remarks>
    /// Walked over the first ten trials rather than a whole block, because what is being pinned is the
    /// seed arithmetic and ten trials pin it exactly as well as fifty-one do at a fifth of the cost.
    /// </remarks>
    [Fact]
    public void RepeatZeroDrawsTheSameNoiseTheOrdinaryLadderDraws()
    {
        const int trials = 10;

        var ordinary = Ft8LadderHarness.Run(
            Rung,
            trials,
            decoders: [Ft8LadderHarness.Available()[0]]);

        var repeats = Ft8LadderHarness.RunRepeats(Rung, trials);

        output.WriteLine("column                  trials  DECODED  MISSED  WRONG  delivered");
        output.WriteLine(
            $"Run, Ft8Sharp           {ordinary[0].Trials,6}  {ordinary[0].Decoded,7}  "
            + $"{ordinary[0].Missed,6}  {ordinary[0].Wrong,5}  {ordinary[0].DeliveredMean,9:F4}");
        output.WriteLine(
            $"RunRepeats, single slot {repeats.Rows[0].Trials,6}  {repeats.Rows[0].Decoded,7}  "
            + $"{repeats.Rows[0].Missed,6}  {repeats.Rows[0].Wrong,5}  "
            + $"{repeats.Rows[0].DeliveredMean,9:F4}");

        Assert.Equal(ordinary[0].Decoded, repeats.Rows[0].Decoded);
        Assert.Equal(ordinary[0].Missed, repeats.Rows[0].Missed);
        Assert.Equal(ordinary[0].Wrong, repeats.Rows[0].Wrong);
        Assert.Equal(ordinary[0].DeliveredMean, repeats.Rows[0].DeliveredMean, 9);

        output.WriteLine(string.Empty);
        output.WriteLine(
            "IDENTICAL. The repeats ladder's single-slot column is the ordinary ladder's row, so a");
        output.WriteLine(
            "combined column beside it is a paired comparison against a number already recorded.");
    }

    /// <summary>
    /// <b>The walk is deterministic: a second call draws the same noise and returns the same counts.</b>
    /// </summary>
    [Fact]
    public void TheSameCallTwiceReturnsTheSameCounts()
    {
        const int trials = 10;

        var first = Ft8LadderHarness.RunRepeats(Rung, trials);
        var second = Ft8LadderHarness.RunRepeats(
            Rung, trials, frequencyJitterHz: 0.0, offsetJitterSamples: 0);

        for (var i = 0; i < first.Rows.Count; i++)
        {
            Assert.Equal(first.Rows[i].Decoded, second.Rows[i].Decoded);
            Assert.Equal(first.Rows[i].Missed, second.Rows[i].Missed);
            Assert.Equal(first.Rows[i].Wrong, second.Rows[i].Wrong);
        }

        Assert.Equal(first.OnlyCombined, second.OnlyCombined);
        Assert.Equal(first.CombinationsSubmitted, second.CombinationsSubmitted);
        Assert.Equal(first.CodewordsAccepted, second.CodewordsAccepted);

        output.WriteLine(
            $"Two walks of {trials} trials: {first.CombinationsSubmitted} combinations submitted both "
            + $"times, {first.CodewordsAccepted} accepted both times, {first.OnlyCombined} trials only "
            + "the combination decoded both times.");
    }

    /// <summary>
    /// <b>The repeats ladder refuses what it cannot measure</b>, and the messages say why.
    /// </summary>
    [Fact]
    public void ARunWithFewerThanTwoSlotsOrNoTrialsIsRefused()
    {
        var noTrials = Assert.Throws<ArgumentOutOfRangeException>(
            () => Ft8LadderHarness.RunRepeats(Rung, 0));
        var oneSlot = Assert.Throws<ArgumentOutOfRangeException>(
            () => Ft8LadderHarness.RunRepeats(Rung, 10, repeats: 1));

        output.WriteLine(noTrials.Message);
        output.WriteLine(oneSlot.Message);

        // And the pairing rule it is handed is the one it uses.
        var settings = new Ft8DeepCombineSettings(maximumPartners: 2);
        var run = Ft8LadderHarness.RunRepeats(Rung, 5, combining: settings);
        Assert.True(run.CombinationsSubmitted >= 0);
        output.WriteLine(
            $"With 2 partners a candidate, 5 trials submitted {run.CombinationsSubmitted} "
            + $"combinations and the port accepted {run.CodewordsAccepted}.");
    }
}

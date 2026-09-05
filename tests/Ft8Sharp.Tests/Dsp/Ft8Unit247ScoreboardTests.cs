using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The scoreboard, whole. Nothing this unit claims is claimed without it.</b> 306 trials at -21 dB
/// and at -24 dB, three columns, both with and without the placement jitter, three counts on every
/// row.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the only place a rate is allowed to come from.</b> <c>PHASE_PLAN.md</c>: no unit in
/// steps 1 to 6 may report an improvement except as a number on step 0's instrument, and a decode rate
/// quoted without it is not evidence. Every row here carries its trial count, its Wilson interval and
/// its wrong-decode count.
/// </para>
/// <para>
/// <b>306 trials is six whole blocks of the 51-message population</b>, which is the count
/// <c>HM-OPEN-067</c>'s 13 of 306 and unit 246's 33 of 306 were both taken at. A partial block is
/// still deterministic and is not comparable to a whole one.
/// </para>
/// <para>
/// <b>Nothing here asserts a rate.</b> Two things are asserted: zero wrong decodes on every row of
/// every rung, which is the criterion this step cannot trade, and that no trial a single slot decoded
/// was lost by the combination, which the superset property makes impossible. Everything else is
/// printed and read.
/// </para>
/// </remarks>
public class Ft8Unit247ScoreboardTests(ITestOutputHelper output)
{
    /// <summary>Six whole blocks of the population.</summary>
    private const int Trials = 306;

    /// <summary>See <c>Ft8Unit247RepeatsLadderTests</c> for what these two numbers stand for.</summary>
    private const double FrequencyJitterHz = 2.0;

    private const int OffsetJitterSamples = 480;

    /// <summary>
    /// <b>The whole thing: two rungs, two placements, 306 trials each.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>-21 dB is where this phase's number lives</b> and it is 1.2 dB below the single-slot 50 per
    /// cent crossing of -19.81 dB that unit 246 left. <b>-24 dB is
    /// <c>Ft8Step6Ladder.CollapseBottomDecibels</c></b>, 4.2 dB below that crossing, where <em>no
    /// single slot could decode this alone</em> needs no argument at all.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheRepeatsLadderAtMinus21AndMinus24DecibelsOver306Trials()
    {
        foreach (var rung in new[] { -21.0, -24.0 })
        {
            foreach (var (placement, frequencyJitter, offsetJitter) in new[]
                     {
                         ("SAME PLACEMENT", 0.0, 0),
                         ("JITTERED PLACEMENT", FrequencyJitterHz, OffsetJitterSamples),
                     })
            {
                var run = Ft8LadderHarness.RunRepeats(
                    rung,
                    Trials,
                    repeats: 2,
                    frequencyJitterHz: frequencyJitter,
                    offsetJitterSamples: offsetJitter);

                output.WriteLine(
                    "=============================================================================");
                output.WriteLine($"{rung:F1} dB, {Trials} TRIALS, {placement}");
                if (offsetJitter != 0)
                {
                    output.WriteLine(
                        $"  the later slot sits {frequencyJitter:F2} Hz and {offsetJitter} samples "
                        + $"({offsetJitter / 12000.0:F3} s) from the earlier one");
                }

                output.WriteLine(
                    "=============================================================================");

                foreach (var line in Ft8LadderHarness.RepeatsReport(run))
                {
                    output.WriteLine(line);
                }

                output.WriteLine(string.Empty);

                foreach (var row in run.Rows)
                {
                    Assert.True(
                        row.Wrong == 0,
                        $"{rung:F1} dB, {placement}: {row.Decoder} returned {row.Wrong} messages "
                            + "that were not sent. A decode nobody sent is worse than a decode "
                            + "missed, and an approach that produces one is rejected.");
                }

                Assert.True(
                    run.LostByCombining == 0,
                    $"{rung:F1} dB, {placement}: {run.LostByCombining} trials were decoded by a "
                        + "single slot and not by the combination. Combining only ever adds.");

                Assert.Equal(run.CombinedDecodes, run.CombinedDecodesVerified);
            }
        }
    }

    /// <summary>
    /// <b>The regression check: nothing tonight moved step 2's number underneath the new one.</b>
    /// </summary>
    /// <remarks>
    /// Unit 246 left the ordered statistics column at <b>10.8 per cent, 33 of 306, zero wrong</b> at
    /// -21 dB, with the OSD-off column equal to the port at 4.2 per cent, 13 of 306. Both are asserted
    /// here rather than assumed, because unit 247 added a hearing-capture path to
    /// <c>Ft8DeepSlotDecoder</c> and a claim that it changes no decision is worth exactly what it is
    /// measured at.
    /// </remarks>
    [Fact]
    public void TheOrderedStatisticsColumnStillReadsWhatUnit246LeftIt()
    {
        var results = Ft8LadderHarness.Run(
            -21.0,
            Trials,
            decoders:
            [
                Ft8LadderHarness.Available()[0],
                new Ft8LadderHarness.Decoder(
                    "Deep OSD on",
                    samples => new Ft8Sharp.Deep.Ft8DeepSlotDecoder(
                        osd: Ft8Sharp.Deep.Ft8DeepOsdSettings.Default).Decode(samples)),
            ]);

        foreach (var line in Ft8LadderHarness.Report(results))
        {
            output.WriteLine(line);
        }

        Assert.Equal(13, results[0].Decoded);
        Assert.Equal(0, results[0].Wrong);
        Assert.Equal(33, results[1].Decoded);
        Assert.Equal(0, results[1].Wrong);

        output.WriteLine(string.Empty);
        output.WriteLine(
            "Ft8Sharp 13 of 306 and Deep OSD on 33 of 306, both with zero wrong, which is exactly");
        output.WriteLine(
            "what unit 246 left. Step 2's number did not move underneath step 6's.");
    }
}

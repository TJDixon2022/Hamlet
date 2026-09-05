using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Is the instrument where unit 247 left it?</b> The two-repeat combined column re-measured at
/// -21 dB over 306 trials, four sibling versions later, against the figures
/// <c>docs/unit247-combining.md</c> §4 recorded at <c>Ft8Sharp.Deep</c> 0.3.0.
/// </summary>
/// <remarks>
/// <para>
/// <b>THE BREAKAGE THIS WOULD HAVE CAUGHT.</b> Unit 247's 13 / 33 / 68 of 306 is the figure every
/// later claim about combining is compared against, and four units landed underneath it — 248's
/// baseband and fine sync, 251's SNR estimator and its three new types, 252's ordered statistics
/// window, 253's subtraction. Every one of them is off by default and none of them is supposed to
/// touch these three columns. <b>A default that quietly moved</b> — a stage arriving on rather than
/// off, a settings object whose default was edited while a grid was measured around it — would make
/// tonight's combined column a measurement of something else wearing unit 247's name, and no other
/// test in the tree compares the three columns against a recorded row.
/// </para>
/// <para>
/// <b>Nothing here asserts a rate.</b> Two things are asserted: zero wrong decodes on every row,
/// which is the criterion this phase cannot trade, and that no trial a single slot decoded was lost
/// by the combination. The reproduction itself is <b>printed and read</b>, never asserted — unit
/// 247's figures are evidence and not a gate (work instruction 254, ruling 2), so a difference is a
/// finding to report beside what it read, not a defect to chase.
/// </para>
/// <para>
/// <b>Nothing under <c>src/Ft8Sharp/</c> is touched and <c>Ft8LadderHarness.Run</c> is not called.</b>
/// This walks <c>RunRepeats</c>, unit 247's own entry point, with unit 247's own arguments.
/// </para>
/// </remarks>
public class Ft8Unit254ReproductionTests(ITestOutputHelper output)
{
    /// <summary>Six whole blocks of the 51-message population.</summary>
    private const int Trials = 306;

    /// <summary>Unit 247's jitter: the later slot 2.00 Hz from the earlier one.</summary>
    private const double FrequencyJitterHz = 2.0;

    /// <summary>Unit 247's jitter: the later slot 480 samples — 0.040 s — from the earlier one.</summary>
    private const int OffsetJitterSamples = 480;

    /// <summary>The rung. 1.2 dB below the single-slot 50 per cent crossing of -19.81 dB.</summary>
    private const double Rung = -21.0;

    /// <summary>
    /// <b>The reproduction that matters: -21 dB, jittered placement, 306 trials, two repeats.</b>
    /// </summary>
    /// <remarks>
    /// <b>The jittered case is the one to protect</b> because it is the honest one — a real station's
    /// clock and oscillator move between slots — and because it is the row <c>HM-OPEN-075</c> hangs
    /// on. Unit 247 read 13 / 33 / 68 of 306 here, with 55 of 306 trials no single slot decoded
    /// alone, 0 lost, 50 677 pairs offered, 516 submitted and 88 accepted.
    /// </remarks>
    [Fact]
    public void TheJitteredTwoRepeatColumnAtMinus21DecibelsOver306TrialsAgainstUnit247()
    {
        Walk("JITTERED PLACEMENT", FrequencyJitterHz, OffsetJitterSamples, 13, 33, 68, 55, 0);
    }

    /// <summary>
    /// <b>The easy end of the pair: -21 dB, both slots on the same bin and the same sample.</b>
    /// </summary>
    /// <remarks>
    /// Unit 247 read 13 / 33 / 217 of 306 here with 200 of 306 only-combined, and called it the upper
    /// end of a pair rather than the result. It is reproduced because a difference that appeared in
    /// only one of the two placements would say which of the two costs moved.
    /// </remarks>
    [Fact]
    public void TheSamePlacementTwoRepeatColumnAtMinus21DecibelsOver306TrialsAgainstUnit247()
    {
        Walk("SAME PLACEMENT", 0.0, 0, 13, 33, 217, 200, 0);
    }

    /// <summary>
    /// One walk, printed whole and asserted narrowly. <b>The recorded figures are arguments so the
    /// comparison prints beside the measurement rather than in a reader's head.</b>
    /// </summary>
    private void Walk(
        string placement,
        double frequencyJitter,
        int offsetJitter,
        int recordedPort,
        int recordedOsd,
        int recordedCombined,
        int recordedOnlyCombined,
        int recordedLost)
    {
        var run = Ft8LadderHarness.RunRepeats(
            Rung,
            Trials,
            repeats: 2,
            frequencyJitterHz: frequencyJitter,
            offsetJitterSamples: offsetJitter);

        output.WriteLine("=========================================================================");
        output.WriteLine($"UNIT 254 TASK 2 - REPRODUCTION AT {Rung:F1} dB, {Trials} TRIALS, {placement}");
        if (offsetJitter != 0)
        {
            output.WriteLine(
                $"  the later slot sits {frequencyJitter:F2} Hz and {offsetJitter} samples "
                + $"({offsetJitter / 12000.0:F3} s) from the earlier one");
        }

        output.WriteLine("=========================================================================");
        output.WriteLine(string.Empty);

        foreach (var line in Ft8LadderHarness.RepeatsReport(run))
        {
            output.WriteLine(line);
        }

        // THE PAIRED STATISTIC. Two overlapping Wilson intervals do not mean two paired columns are
        // indistinguishable; the whole evidence about which column is better lives in the trials
        // where they disagree. HM-OPEN-078, and unit 253 established it.
        var (portOnly, combinedOnly) = Ft8LadderHarness.Discordance(run.Rows[0], run.Rows[2]);
        var (osdOnly, combinedOverOsd) = Ft8LadderHarness.Discordance(run.Rows[1], run.Rows[2]);

        output.WriteLine(string.Empty);
        output.WriteLine("  THE DISCORDANT COUNTS - the paired statistic, on identical audio:");
        output.WriteLine(
            $"    trials only THE PORT decoded and the combination did not:   {portOnly}");
        output.WriteLine(
            $"    trials only THE COMBINATION decoded and the port did not:   {combinedOnly}");
        output.WriteLine(
            $"    trials only SINGLE + OSD decoded and the combination did not: {osdOnly}");
        output.WriteLine(
            $"    trials only THE COMBINATION decoded and single + OSD did not: {combinedOverOsd}");

        output.WriteLine(string.Empty);
        output.WriteLine("  AGAINST WHAT UNIT 247 RECORDED AT Ft8Sharp.Deep 0.3.0:");
        output.WriteLine("  row              unit 247    tonight   moved by");
        output.WriteLine(
            $"  single slot      {recordedPort,8}   {run.Rows[0].Decoded,8}   "
            + $"{Signed(run.Rows[0].Decoded - recordedPort),8}");
        output.WriteLine(
            $"  single + OSD     {recordedOsd,8}   {run.Rows[1].Decoded,8}   "
            + $"{Signed(run.Rows[1].Decoded - recordedOsd),8}");
        output.WriteLine(
            $"  combined x2      {recordedCombined,8}   {run.Rows[2].Decoded,8}   "
            + $"{Signed(run.Rows[2].Decoded - recordedCombined),8}");
        output.WriteLine(
            $"  only combined    {recordedOnlyCombined,8}   {run.OnlyCombined,8}   "
            + $"{Signed(run.OnlyCombined - recordedOnlyCombined),8}");
        output.WriteLine(
            $"  lost by combining{recordedLost,8}   {run.LostByCombining,8}   "
            + $"{Signed(run.LostByCombining - recordedLost),8}");

        var moved = run.Rows[0].Decoded != recordedPort
            || run.Rows[1].Decoded != recordedOsd
            || run.Rows[2].Decoded != recordedCombined
            || run.OnlyCombined != recordedOnlyCombined;

        output.WriteLine(string.Empty);
        output.WriteLine(
            moved
                ? "  THE INSTRUMENT MOVED. Reported, not chased - work instruction 254 ruling 2. The "
                    + "units that landed under unit 247's figures are 248 (baseband and fine sync), "
                    + "251 (the SNR estimator), 252 (the ordered statistics window) and 253 "
                    + "(subtraction), and all four are off by default in this configuration."
                : "  THE INSTRUMENT DID NOT MOVE. Every column reads exactly what unit 247 read at "
                    + "Ft8Sharp.Deep 0.3.0, four sibling versions ago.");

        // ASSERTED, AND ONLY THIS. A wrong decode is worse than a missed one and no unit may be the
        // one that stops checking.
        foreach (var row in run.Rows)
        {
            Assert.Equal(0, row.Wrong);
        }

        // Combining only ever adds, and the combined column is the union over the trial's slots, so
        // a non-zero count here is a defect rather than a result.
        Assert.Equal(0, run.LostByCombining);

        // Every combined decode against the ladder's own ground truth. Step 5's third exit.
        Assert.Equal(run.CombinedDecodes, run.CombinedDecodesVerified);
    }

    /// <summary>A difference with its sign on it, so a reader cannot mistake a drop for a gain.</summary>
    private static string Signed(int difference) =>
        difference > 0 ? $"+{difference}" : difference.ToString();
}

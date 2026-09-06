using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>COMBINING ON AND OFF AT THE CLOSING TABLE'S OWN THREE RUNGS, AT BOTH PLACEMENTS, WITH THE
/// JITTER SET TO ZERO.</b> Step 6's first exit asks for each stage on and off at <b>-19, -20 and
/// -21 dB, on grid and at the cell centre, 306 trials a cell, with wrong counts</b>. Combining is
/// the one stage that has never been answered at those coordinates: unit 256's panel
/// (<c>Ft8Unit256CombiningPanelTests</c>) answers it at -21, -22 and -23 dB in one mixed
/// placement. <b>This is the same instrument at the table's own rungs and at both placements.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>WHY THE JITTER IS ZERO, AND IT IS THE WHOLE OF THIS CLASS.</b>
/// <c>Ft8LadderHarness.cs:573-574</c> reads <c>var slotFrequency = frequencyHz + (r *
/// frequencyJitterHz);</c> and <c>var slotOffset = offset + (r * offsetJitterSamples);</c>, so
/// <c>frequencyHz</c> and <c>offsetSamples</c> are the origin of hearing <c>r = 0</c> and every
/// later hearing steps from it. <b>With both jitters zero every hearing of every trial sits at
/// exactly the stated placement</b>, and <em>on grid</em> and <em>at the cell centre</em> then
/// mean precisely what they mean in <c>docs/unit255-closing-measurement.md</c> §3.1 and §3.2.
/// Unit 256's panel is jittered and its placement is therefore mixed, which is why it measured
/// one placement and this one measures two.
/// </para>
/// <para>
/// <b>A NEW CONFIGURATION AND NOT A CORRECTION.</b> Unit 256's §5.4 panel stands unchanged, is
/// not re-run and is not corrected. It is the conservative reading — a station whose oscillator
/// drifts between hearings — and this is the aligned-repeats reading, a station whose four
/// transmissions land in the same place. <b>No row here is comparable with a row there, and no
/// row of either is comparable with a row of §3's table</b>, which gives each trial one slot
/// where this gives each trial four. <c>docs/unit255-closing-measurement.md</c> §5.0.
/// </para>
/// <para>
/// <b>THREE ROWS AND NOT FOUR</b>, from <c>Ft8LadderHarness.cs:518-523</c>, and the third is
/// labelled <c>summed x4</c> rather than <c>combined x4</c> because <c>AccumulationDepth</c> is 3
/// and the label is taken from the depth of the sum and never from the repeat count
/// (<c>:514</c>). That distinction is <c>B17</c>.
/// </para>
/// <para>
/// <b>THE CAVEAT THAT TRAVELS WITH EVERY FIGURE HERE, IN UNIT 254'S OWN WORDS.</b>
/// <c>RunRepeats</c> scores the combined column on the union over the trial's slots, so <b>a
/// four-repeat column gets four single-slot attempts as well as deeper sums.</b>
/// <c>OnlyCombined</c> — the trials no single slot decoded alone — is printed on every panel row
/// and is the honest statement of what combining added.
/// </para>
/// <para>
/// <b>A MEASUREMENT AND NOT A TEST, AND NOT IN THE GATE SET.</b> <c>docs/gate-set.md</c> line 57:
/// the ladder is a measurement. <b>No red is manufactured for a walk</b> and <b>no bound is
/// asserted on any rate</b> — saturation at the table's rungs is a result and is reported as one.
/// The two assertions are the two this project never relaxes: <b>zero wrong on every row</b>, and
/// <b>every combined decode verified against the ladder's own ground truth</b>.
/// </para>
/// <para>
/// <b>WHY <c>DeepestHearings</c> IS PRINTED AND NOT ASSERTED</b>, which is a deliberate departure
/// from <c>Ft8Unit256CombiningPanelTests:214</c>. That panel asserts <c>== 4</c>; unit 256's own
/// summary records that at -23 dB it read <b>3</b>, not because of <c>B17</c> but because so
/// little decoded that no slot ever held four hearings' worth of candidates to accumulate. On a
/// walk whose rung is the variable, an assertion that only means anything where the combiner has
/// candidates to work with would manufacture a red for a measurement. <b>It is printed on every
/// panel, and where it is not 4 the panel says so in words.</b>
/// </para>
/// <para>
/// <b>ONE METHOD PER RUNG-PLACEMENT, AND THAT IS STRUCTURAL.</b> Each is one foregrounded call
/// priced at ~147 s in <c>docs/unit257-combining-placement.md</c> §1.4 against a 480 s ceiling,
/// and a red at one rung must not stop the other five.
/// </para>
/// </remarks>
public class Ft8Unit257PlacementPanelTests(ITestOutputHelper output)
{
    /// <summary>306 trials: six whole blocks of the 51-message population.</summary>
    private const int Trials = 306;

    /// <summary>FT8's whole slot, which the worst-slot margin is quoted against.</summary>
    private const double SlotBudgetMilliseconds = 15_000.0;

    /// <summary>
    /// <b>Unit 248's cell centre</b>, transcribed from
    /// <c>Ft8Unit255ClosingLadderTests:62</c> and <c>:64</c> and from nowhere else, so tonight's
    /// cell-centre rows are comparable with §3.2's. <b>These two numbers and no others.</b>
    /// </summary>
    private const double CellCentreFrequencyOffsetHz = 1.56;

    private const int CellCentreOffsetSamples = 480;

    /// <summary>
    /// <b>Where a walk's report is written, as well as to the test output.</b> VSTest discards
    /// <c>ITestOutputHelper</c> for a test that PASSES and it cost unit 255 a four-minute call.
    /// <b>Every figure the closing document quotes is transcribed from one of these files and
    /// never from a console buffer.</b>
    /// </summary>
    private static string RunLogPath(string stem)
    {
        var folder = Path.Combine(Ft8CaptureFixtures.RepositoryRoot(), "docs", "unit257-runs");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, stem + ".txt");
    }

    /// <summary>
    /// <b>One panel rung at one placement.</b> The call is unit 256's, transcribed argument for
    /// argument at <c>Ft8Unit256CombiningPanelTests:112</c>, differing in exactly the arguments
    /// named below and in no others.
    /// </summary>
    /// <param name="rung">The ratio to deliver, in decibels in the 2500 Hz reference bandwidth.</param>
    /// <param name="placement">The label — <c>on grid</c> or <c>cell centre</c>.</param>
    /// <param name="frequencyHz">Where every hearing's lowest tone sits. Zero jitter makes it every hearing's.</param>
    /// <param name="offsetSamples">Where in every slot the transmission begins.</param>
    /// <param name="stem">The artefact's file name under <c>docs/unit257-runs/</c>.</param>
    private void Panel(
        double rung, string placement, double frequencyHz, int offsetSamples, string stem)
    {
        var lines = new List<string>();

        void Say(string line)
        {
            lines.Add(line);
            output.WriteLine(line);
        }

        Say($"UNIT 257, THE PLACEMENT PANEL AT {rung:F1} dB, {placement.ToUpperInvariant()}, "
            + $"{Trials} trials, 3 rows.");
        Say(
            "LADDER: Ft8LadderHarness.RunRepeats - FOUR slots a trial carrying the same message. "
            + "THIS IS NOT THE CLOSING TABLE'S LADDER AND NO ROW HERE IS COMPARABLE WITH ONE "
            + "THERE, which gives each trial ONE slot. Unit 255 section 5.0.");
        Say(
            "JITTER: ZERO on both axes - frequencyJitterHz 0.0 and offsetJitterSamples 0. "
            + "Ft8LadderHarness.cs:573-574 makes frequencyHz and offsetSamples the origin of "
            + "hearing r=0 and every later hearing step from it, so AT ZERO JITTER EVERY HEARING "
            + "OF EVERY TRIAL SITS AT EXACTLY THE PLACEMENT NAMED BELOW. That is what makes the "
            + "placement label true rather than approximate.");
        Say(
            $"PLACEMENT: {placement} - {frequencyHz:F2} Hz, {offsetSamples} samples in. "
            + $"On grid is Ft8LadderHarness.DefaultFrequencyHz {Ft8LadderHarness.DefaultFrequencyHz:F2} Hz "
            + $"and DefaultOffsetSamples {Ft8LadderHarness.DefaultOffsetSamples}; the cell centre "
            + $"adds +{CellCentreFrequencyOffsetHz} Hz and +{CellCentreOffsetSamples} samples, "
            + "unit 248's two constants from Ft8Unit255ClosingLadderTests:62 and :64. At 3.125 Hz "
            + "a waterfall bin, +1.56 Hz is half a bin - the furthest it is possible to be from a "
            + "bin centre.");
        Say(
            "NOT UNIT 256's PANEL. That one is jittered 2.00 Hz and 480 samples between hearings "
            + "and is therefore a MIXED-placement instrument; it stands unchanged at "
            + "docs/unit255-closing-measurement.md section 5.4 and is NOT re-run or corrected "
            + "here. This panel models a station whose four transmissions land in the SAME place "
            + "off the grid - a stable oscillator over two minutes - and it does NOT model drift.");
        Say(
            "COMBINING: new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3) - three "
            + "remembered slots, and up to three of them in ONE sum.");
        Say(
            "STACKED: combinedOsd Ft8DeepOsdSettings.Default and combinedFineSync "
            + "Ft8DeepFineSyncSettings.Default - the two stages Ft8Reception.cs:460 builds, on "
            + "the combined column's inner decoder.");
        Say(
            "THE CAVEAT, UNIT 254'S OWN, AND IT TRAVELS WITH EVERY FIGURE BELOW: RunRepeats "
            + "scores the combined column on the union over the trial's slots, so A FOUR-REPEAT "
            + "COLUMN GETS FOUR SINGLE-SLOT ATTEMPTS AS WELL AS DEEPER SUMS. OnlyCombined is the "
            + "honest statement of what combining added and it is printed below.");
        Say(
            "NOTHING IS ASSERTED ABOUT ANY RATE. Saturation at these rungs is a RESULT and is "
            + "reported as one. The two assertions are zero wrong on every row, and every "
            + "combined decode verified against the ladder's own ground truth.");
        Say(string.Empty);

        var clock = Stopwatch.StartNew();
        var run = Ft8LadderHarness.RunRepeats(
            rung,
            Trials,
            repeats: 4,
            frequencyHz: frequencyHz,
            offsetSamples: offsetSamples,
            frequencyJitterHz: 0.0,
            offsetJitterSamples: 0,
            combining: new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3),
            combinedOsd: Ft8DeepOsdSettings.Default,
            combinedFineSync: Ft8DeepFineSyncSettings.Default);
        clock.Stop();

        // THE WHOLE REPORT IS PRINTED BEFORE ANYTHING IS ASSERTED.
        foreach (var line in Ft8LadderHarness.RepeatsReport(run))
        {
            Say(line);
        }

        Say(string.Empty);
        Say("EVERY FIELD THIS PANEL IS JUDGED ON, ON ITS OWN LINE:");
        Say($"  requested                                {rung:F1} dB");

        foreach (var row in run.Rows)
        {
            Say(
                $"  delivered to {row.Decoder,-14}            {row.DeliveredMean:F3} dB   "
                + $"(worst trial off by {row.WorstDeliveryError:F3} dB from the {rung:F1} dB "
                + "requested)");
        }

        Say($"  repeats                                  {run.Repeats}");
        Say(
            $"  delivered per slot                       "
            + string.Join(", ", run.Delivered.Select(d => $"{d:F3} dB")));
        Say($"  OnlyCombined                             {run.OnlyCombined} of {Trials}");
        Say($"  AnySlotAlone                             {run.AnySlotAlone} of {Trials}");
        Say($"  LostByCombining                          {run.LostByCombining} of {Trials}");
        Say($"  PairsOffered                             {run.PairsOffered}");
        Say($"  CombinationsSubmitted                    {run.CombinationsSubmitted}");
        Say($"  CodewordsAccepted                        {run.CodewordsAccepted}");
        Say($"  CombinedDecodes                          {run.CombinedDecodes}");
        Say($"  CombinedDecodesVerified                  {run.CombinedDecodesVerified}");
        Say(
            $"  of those, a message that was NOT sent    "
            + $"{run.CombinedDecodes - run.CombinedDecodesVerified}");
        Say(
            $"  ExpectedFalseAccepts                     "
            + $"{Ft8DeepCombineSettings.ExpectedFalseAccepts(run.CombinationsSubmitted):F3}"
            + "   (Ft8DeepCombineSettings.ExpectedFalseAccepts, a static on the settings type - "
            + "it is NOT a field of RepeatsRun)");
        Say(
            $"  DeepestHearings                          {run.DeepestHearings}"
            + (run.DeepestHearings == 4
                ? "   (four hearings did reach one sum, so the summed x4 label is earned - B17)"
                : "   *** NOT 4, AND IT IS PRINTED RATHER THAN ASSERTED. At a rung where little "
                  + "decodes no slot ever holds four hearings' worth of candidates to "
                  + "accumulate, so the deepest sum is shallower than the label. That is a "
                  + "property of the rung and NOT B17, and this panel does not manufacture a red "
                  + "for it - unit 256 recorded the same reading at -23 dB. ***"));
        Say(
            $"  WorstSlotMilliseconds                    {run.WorstSlotMilliseconds:F1} ms, "
            + $"carrying {run.WorstSlotCandidates} candidates and {run.WorstSlotCombinations} "
            + $"combinations - a margin of "
            + $"{SlotBudgetMilliseconds / Math.Max(run.WorstSlotMilliseconds, 0.001):F0}x against "
            + "FT8's 15 000 ms");

        Say(string.Empty);
        Say("ONLYCOMBINED ON EVERY ROW, WHICH IS WHAT THE CAVEAT REQUIRES:");

        foreach (var row in run.Rows)
        {
            var (lower, upper) = row.Interval;
            Say(
                $"  {row.Decoder,-14} {row.Decoded,4} of {row.Trials}  {row.Rate,5:F1} % "
                + $"({lower,5:F1} - {upper,5:F1})  WRONG {row.Wrong}   "
                + $"trials no single slot decoded alone and the combination DID: "
                + $"{run.OnlyCombined} of {Trials}");
        }

        Say(string.Empty);
        Say($"PANEL WALL CLOCK {clock.Elapsed.TotalSeconds:F1} s, against ~147 s predicted in "
            + "docs/unit257-combining-placement.md section 1.4 and a 480 s ceiling.");

        // EVERY WRONG RETURN IS PRINTED, SENT BESIDE RETURNED, BEFORE IT IS ASSERTED AWAY.
        var wrongTotal = 0;

        foreach (var row in run.Rows)
        {
            wrongTotal += row.Wrong;

            foreach (var wrong in row.WrongReturns)
            {
                Say($"WRONG on {row.Decoder}: {wrong}");
            }
        }

        Say($"TOTAL WRONG ACROSS ALL THREE ROWS AT THIS RUNG-PLACEMENT: {wrongTotal}.");
        Say($"TOTAL SCORED SLOT DECODES AT THIS RUNG-PLACEMENT: {run.Rows.Count * Trials}.");

        // THE ARTEFACT IS WRITTEN BEFORE THE ASSERTIONS RUN, so a red costs none of its own
        // numbers and the closing document can still be written from a committed file.
        File.WriteAllLines(RunLogPath(stem), lines);

        // ASSERTION ONE OF TWO: zero wrong on every row. A message shown that nobody sent is
        // worse than a message missed, and no unit may be the one that stops checking.
        foreach (var row in run.Rows)
        {
            Assert.True(
                row.Wrong == 0,
                $"{row.Decoder} at {rung:F1} dB {placement} on the repeats ladder returned "
                    + $"{row.Wrong} message(s) that were not sent. A wrong decode is counted "
                    + "separately from a missed one everywhere in this project. See "
                    + $"docs/unit257-runs/{stem}.txt for the full table and every wrong return "
                    + "with the message that was sent beside it.");
        }

        // ASSERTION TWO OF TWO: every combined decode verified against the ladder's own ground
        // truth. Step 5's rule that a gain is never inferred from another decoder.
        Assert.True(
            run.CombinedDecodes == run.CombinedDecodesVerified,
            $"at {rung:F1} dB {placement} the combining stage added {run.CombinedDecodes} "
                + $"messages and only {run.CombinedDecodesVerified} of them were the message that "
                + $"was sent. {run.CombinedDecodes - run.CombinedDecodesVerified} came back that "
                + "nobody sent, against "
                + $"{Ft8DeepCombineSettings.ExpectedFalseAccepts(run.CombinationsSubmitted):F3} "
                + $"naively expected over {run.CombinationsSubmitted} submissions.");

        // Nothing about any rate is asserted, at any rung, at either placement, on any row.
    }

    /// <summary>On grid, at the deepest of the closing table's three rungs.</summary>
    /// <remarks>
    /// <b>The rung with the hardest consistency check available tonight.</b> Its two
    /// combining-OFF rows see only slot 0, which at zero jitter is the same audio three separate
    /// records already walked: <c>docs/unit247-combining.md</c> §4, <c>unit255</c> §3.1 and
    /// <c>unit255</c> §5.3 all read <b>13 of 306</b> and <b>33 of 306</b> at -21 dB on grid.
    /// The combined row has a directional bound only — unit 247 §4's <c>combined x2</c> reads
    /// <b>217 of 306</b> at two hearings with no accumulation and no stacked stages, so four
    /// hearings stacked should be at or above it. <b>A sanity bound, not a reproduction.</b>
    /// </remarks>
    [Fact]
    public void ThePlacementPanelOnGridAtMinus21() =>
        Panel(
            -21.0,
            "on grid",
            Ft8LadderHarness.DefaultFrequencyHz,
            Ft8LadderHarness.DefaultOffsetSamples,
            "placement-panel-on-grid-minus21");

    /// <summary>On grid, the closing table's middle rung. Combining has never been measured here.</summary>
    [Fact]
    public void ThePlacementPanelOnGridAtMinus20() =>
        Panel(
            -20.0,
            "on grid",
            Ft8LadderHarness.DefaultFrequencyHz,
            Ft8LadderHarness.DefaultOffsetSamples,
            "placement-panel-on-grid-minus20");

    /// <summary>On grid, the closing table's shallowest rung. Combining has never been measured here.</summary>
    [Fact]
    public void ThePlacementPanelOnGridAtMinus19() =>
        Panel(
            -19.0,
            "on grid",
            Ft8LadderHarness.DefaultFrequencyHz,
            Ft8LadderHarness.DefaultOffsetSamples,
            "placement-panel-on-grid-minus19");

    /// <summary>
    /// <b>At the cell centre, -21 dB. This is the half of tonight nobody has ever measured.</b>
    /// Combining has never been measured off the analysis grid anywhere in this project.
    /// §3.2 reads the bare port at <b>0 of 306</b> here, so the two combining-OFF rows are
    /// expected to fall a long way. <b>Whatever the combined row does is the answer.</b>
    /// </summary>
    [Fact]
    public void ThePlacementPanelAtCellCentreAtMinus21() =>
        Panel(
            -21.0,
            "cell centre",
            Ft8LadderHarness.DefaultFrequencyHz + CellCentreFrequencyOffsetHz,
            Ft8LadderHarness.DefaultOffsetSamples + CellCentreOffsetSamples,
            "placement-panel-cell-centre-minus21");

    /// <summary>At the cell centre, -20 dB. §3.2 reads the bare port at <b>0 of 306</b> here.</summary>
    [Fact]
    public void ThePlacementPanelAtCellCentreAtMinus20() =>
        Panel(
            -20.0,
            "cell centre",
            Ft8LadderHarness.DefaultFrequencyHz + CellCentreFrequencyOffsetHz,
            Ft8LadderHarness.DefaultOffsetSamples + CellCentreOffsetSamples,
            "placement-panel-cell-centre-minus20");

    /// <summary>At the cell centre, -19 dB. §3.2 reads the bare port at <b>6 of 306</b> here.</summary>
    [Fact]
    public void ThePlacementPanelAtCellCentreAtMinus19() =>
        Panel(
            -19.0,
            "cell centre",
            Ft8LadderHarness.DefaultFrequencyHz + CellCentreFrequencyOffsetHz,
            Ft8LadderHarness.DefaultOffsetSamples + CellCentreOffsetSamples,
            "placement-panel-cell-centre-minus19");

    /// <summary>
    /// <b>THE DOWNWARD EXTENSION, FIRST STEP. On grid, -22 dB.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Not one of exit criterion 1's rungs, and it is here to serve exit 2.</b> The three
    /// rungs the closing table quotes leave <c>summed x4</c> on the grid reading <b>306 of 306 at
    /// all three</b>, so its 50 per cent crossing lies below -21 dB and this ladder cannot say
    /// where without walking further. <b>The decision to spend this call was taken at the start of
    /// task 4 and written into <c>docs/unit257-combining-placement.md</c> §3.1 before it was
    /// spent</b>, with its reason and its price.
    /// </para>
    /// <para>
    /// <b>THE SEARCH IS CAPPED AT -23 dB.</b> A column still unbracketed there is reported
    /// against that stated ceiling and <b>nothing is extrapolated</b>.
    /// </para>
    /// <para>
    /// <b>This rung is likelier than any other call tonight to return a wrong decode.</b>
    /// <c>HM-OPEN-082</c> was found at -23 dB on the jittered panel, from the combined column's
    /// inner decoder at four times the exposure of the <c>single + OSD</c> row. <b>If it goes
    /// red it stays red</b>, printed with sent beside returned, and it is not weakened.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheDownwardExtensionOnGridAtMinus22() =>
        Panel(
            -22.0,
            "on grid",
            Ft8LadderHarness.DefaultFrequencyHz,
            Ft8LadderHarness.DefaultOffsetSamples,
            "extension-on-grid-minus22");

    /// <summary>
    /// <b>THE DOWNWARD EXTENSION, SECOND AND LAST STEP. On grid, -23 dB — the stated cap.</b>
    /// </summary>
    /// <remarks>
    /// Spent only because <c>summed x4</c> on the grid was still above 50 per cent at -22 dB.
    /// <b>Nothing below -23 dB is licensed by this instruction and nothing below it is walked</b>;
    /// a column unbracketed here is reported as unbracketed against this ceiling.
    /// </remarks>
    [Fact]
    public void TheDownwardExtensionOnGridAtMinus23() =>
        Panel(
            -23.0,
            "on grid",
            Ft8LadderHarness.DefaultFrequencyHz,
            Ft8LadderHarness.DefaultOffsetSamples,
            "extension-on-grid-minus23");
}

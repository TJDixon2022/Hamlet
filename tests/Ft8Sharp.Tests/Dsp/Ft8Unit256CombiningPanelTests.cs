using System.Diagnostics;
using Ft8Sharp.Deep;
using Ft8Sharp.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>COMBINING'S OWN ON-AND-OFF PANEL, AND ITS OWN 50 PER CENT CROSSING.</b> Step 6's first
/// exit asks for the port and Deep <em>with each stage on and off</em>, and the closing table has
/// six columns of which combining is not one — it appears there only as a citation from unit 254
/// taken on a different ladder. <b>Combining is the stage this phase found most worth having and
/// the closing table cannot show it turned off.</b> This is that reading.
/// </summary>
/// <remarks>
/// <para>
/// <b>A PANEL AND NOT A COLUMN, AND THE PANEL IS ON THE REPEATS LADDER.</b>
/// <c>Ft8LadderHarness.RunRepeats</c> gives every trial four slots carrying the same message.
/// <c>docs/unit255-closing-measurement.md</c> §5.0 already rules that a row from it cannot sit in
/// §3's table, so combining's <em>on and off</em> is this panel's own three rows — the port on
/// one slot, the sibling with ordered statistics on one slot, and the accumulated sum of four
/// hearings stacked with the shipping stages — <b>paired on identical audio, which is what makes
/// the comparison mean anything.</b>
/// </para>
/// <para>
/// <b>THREE ROWS AND NOT FOUR</b>, from <c>Ft8LadderHarness.cs:518</c>, and the third is labelled
/// <c>summed x4</c> rather than <c>combined x4</c> because <c>AccumulationDepth</c> is 3 and the
/// label is taken from the depth of the sum and never from the repeat count (<c>:514</c>). That
/// distinction is <c>B17</c> and it is asserted below.
/// </para>
/// <para>
/// <b>ONE PLACEMENT, AND THAT IS DELIBERATE.</b> The first slot starts on grid and every later
/// hearing is jittered 2.00 Hz and 480 samples from the one before it, so <b>the panel is already
/// a mixed-placement instrument</b> and a second column labelled <em>cell centre</em> would not
/// mean what that phrase means in §3. It is measured at the same placement as unit 255 §5.3 so
/// that section's -21 dB row can be cited rather than re-run.
/// </para>
/// <para>
/// <b>THE CAVEAT THAT TRAVELS WITH EVERY FIGURE HERE, IN UNIT 254'S OWN WORDS.</b>
/// <c>RunRepeats</c> scores the combined column on the union over the trial's slots, so <b>a
/// four-repeat column gets four single-slot attempts as well as deeper sums.</b>
/// <c>OnlyCombined</c> — the trials no single slot decoded alone — is printed on every panel row
/// and is the honest statement of what combining added.
/// </para>
/// <para>
/// <b>A MEASUREMENT AND NOT A TEST, AND NOT IN THE GATE SET.</b> No red is manufactured for a
/// walk. Unit 256's one watched failure is <c>Ft8Unit256CrossingIntervalTests</c>.
/// </para>
/// </remarks>
public class Ft8Unit256CombiningPanelTests(ITestOutputHelper output)
{
    /// <summary>306 trials: six whole blocks of the 51-message population.</summary>
    private const int Trials = 306;

    /// <summary>FT8's whole slot, which the worst-slot margin is quoted against.</summary>
    private const double SlotBudgetMilliseconds = 15_000.0;

    /// <summary>
    /// <b>Where a walk's report is written, as well as to the test output.</b> VSTest discards
    /// <c>ITestOutputHelper</c> for a test that PASSES and it cost unit 255 a four-minute call.
    /// </summary>
    private static string RunLogPath(string stem)
    {
        var folder = Path.Combine(Ft8CaptureFixtures.RepositoryRoot(), "docs", "unit256-runs");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, stem + ".txt");
    }

    /// <summary>
    /// <b>One panel rung. The call is unit 255 §5.3's, transcribed argument for argument, with
    /// only <paramref name="rung"/> changed.</b>
    /// </summary>
    private void Panel(double rung, string why)
    {
        var lines = new List<string>();

        void Say(string line)
        {
            lines.Add(line);
            output.WriteLine(line);
        }

        Say(
            $"UNIT 256, TASK 4, THE COMBINING PANEL AT {rung:F1} dB, {Trials} trials, 3 rows.");
        Say(
            "LADDER: Ft8LadderHarness.RunRepeats - FOUR slots a trial carrying the same message, "
            + "jittered 2.00 Hz and 480 samples between hearings. THIS IS NOT THE CLOSING TABLE'S "
            + "LADDER AND NO ROW HERE IS COMPARABLE WITH ONE THERE. Unit 255 section 5.0.");
        Say(
            "PLACEMENT: ONE, and deliberately. The first slot starts on grid and every later "
            + "hearing is jittered from the one before it, so this ladder is already a "
            + "mixed-placement instrument; a second column labelled \"cell centre\" would not "
            + "mean what that phrase means in section 3. Same placement as unit 255 section 5.3, "
            + "so its -21 dB row is CITED rather than re-run.");
        Say(
            "COMBINING: new Ft8DeepCombineSettings(historyDepth: 3, accumulationDepth: 3) - three "
            + "remembered slots, and up to three of them in ONE sum.");
        Say(
            "STACKED: combinedOsd Ft8DeepOsdSettings.Default and combinedFineSync "
            + "Ft8DeepFineSyncSettings.Default - the two stages Ft8Reception.cs:460 builds, on "
            + "the combined column's inner decoder.");
        Say($"WHY THIS RUNG: {why}");
        Say(
            "THE CAVEAT, UNIT 254'S OWN, AND IT TRAVELS WITH EVERY FIGURE BELOW: RunRepeats "
            + "scores the combined column on the union over the trial's slots, so A FOUR-REPEAT "
            + "COLUMN GETS FOUR SINGLE-SLOT ATTEMPTS AS WELL AS DEEPER SUMS. OnlyCombined is the "
            + "honest statement of what combining added and it is printed below.");
        Say(string.Empty);

        var clock = Stopwatch.StartNew();
        var run = Ft8LadderHarness.RunRepeats(
            rung,
            Trials,
            repeats: 4,
            frequencyJitterHz: 2.0,
            offsetJitterSamples: 480,
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
        Say($"  repeats                                  {run.Repeats}");
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
        Say($"  DeepestHearings                          {run.DeepestHearings}");
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
            Say(
                $"  {row.Decoder,-14} {row.Decoded,4} of {row.Trials}  {row.Rate,5:F1} %   "
                + $"trials no single slot decoded alone and the combination DID: "
                + $"{run.OnlyCombined} of {Trials}");
        }

        Say(string.Empty);
        Say($"PANEL WALL CLOCK {clock.Elapsed.TotalSeconds:F1} s.");

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

        Say($"TOTAL WRONG ACROSS ALL THREE ROWS AT THIS RUNG: {wrongTotal}.");
        Say($"TOTAL SCORED SLOT DECODES AT THIS RUNG: {run.Rows.Count * Trials}.");

        File.WriteAllLines(RunLogPath($"combining-panel-minus{Math.Abs(rung):F0}"), lines);

        // ASSERTION ONE OF THREE: zero wrong on every row. A message shown that nobody sent is
        // worse than a message missed, and no unit may be the one that stops checking.
        foreach (var row in run.Rows)
        {
            Assert.True(
                row.Wrong == 0,
                $"{row.Decoder} at {rung:F1} dB on the repeats ladder returned {row.Wrong} "
                    + "message(s) that were not sent. A wrong decode is counted separately from a "
                    + "missed one everywhere in this project and every column measured reads "
                    + "zero.");
        }

        // ASSERTION TWO OF THREE: every combined decode verified against the ladder's own ground
        // truth. Step 5's rule that a gain is never inferred from another decoder.
        Assert.True(
            run.CombinedDecodes == run.CombinedDecodesVerified,
            $"at {rung:F1} dB the combining stage added {run.CombinedDecodes} messages and only "
                + $"{run.CombinedDecodesVerified} of them were the message that was sent. "
                + $"{run.CombinedDecodes - run.CombinedDecodesVerified} came back that nobody "
                + $"sent, against {Ft8DeepCombineSettings.ExpectedFalseAccepts(run.CombinationsSubmitted):F3} "
                + $"naively expected over {run.CombinationsSubmitted} submissions.");

        // ASSERTION THREE OF THREE: B17. A column that says four hearings carries four. Before
        // unit 254 this read 2 at every repeat count, because the combiner was called through its
        // two-hearing overload once per remembered slot - a chain of pairs, never a sum of four,
        // measuring the 3.01 dB two hearings are worth while its name claimed the 6.02 of four.
        Assert.True(
            run.DeepestHearings == 4,
            $"at {rung:F1} dB the deepest combination anywhere in the walk carried "
                + $"{run.DeepestHearings} hearings, not 4. A panel labelled from a sum of four "
                + "that never summed four is B17 in docs/breakage-record.md, and it would make "
                + "every rate on this panel a measurement of something other than what it says.");

        // Nothing about any rate is asserted, at any rung, on any row.
    }

    /// <summary>
    /// <b>-22 dB.</b> The panel's combined row reads <b>83.0 per cent at -21 dB</b> (unit 255
    /// §5.3, cited and not re-run), so its crossing lies below -21 and this is the first step of
    /// the search that brings it into this project.
    /// </summary>
    [Fact]
    public void TheCombiningPanelAtMinus22() =>
        Panel(
            -22.0,
            "the combined row reads 83.0 per cent at -21 dB (unit 255 section 5.3, cited and not "
            + "re-run), so its 50 per cent crossing lies BELOW -21 dB and no unit has ever quoted "
            + "one. This is the first step of the search.");

    /// <summary>
    /// <b>-23 dB.</b> The second step of the search. If the combined row is still above 50 per
    /// cent here, one rung at -24 dB is licensed and nothing below it is.
    /// <para>
    /// <b>THIS METHOD IS RED IN THE TREE AND IT IS RED BECAUSE OF WHAT IT MEASURED, NOT BECAUSE
    /// OF HOW IT WAS WRITTEN. DO NOT WEAKEN THE ASSERTION TO MAKE IT PASS.</b> At -23 dB the
    /// <c>summed x4</c> row returned <b>one message nobody sent</b> — trial 29, seed 220771,
    /// <c>SENT "CQ PY2ABC GG66"</c>, <c>RETURNED "WN8ESU/P JG5HKE/P R AH58"</c> — and it
    /// reproduces on the identical trial and seed on a second run, so it is deterministic rather
    /// than a flake. <b>It is the first wrong decode measured anywhere in this phase</b>, against
    /// zero in all thirty-six cells of unit 255's closing table and zero at every rung of unit
    /// 256's own cell-centre walks.
    /// </para>
    /// <para>
    /// <b>It did NOT come from a combination.</b> <c>CodewordsAccepted</c> is 1,
    /// <c>CombinedDecodes</c> is 1 and <c>CombinedDecodesVerified</c> is 1 — the single
    /// combination the port took past both gates was the message that was sent. So the wrong
    /// return came from the combined column's <em>inner</em> decoder acting on a single slot: the
    /// shipping stack, <c>osd: Default, fineSync: Default</c>, run on all four slots of every
    /// trial and therefore at four times the exposure of the <c>single + OSD</c> row, which reads
    /// zero wrong here. <b>That is a finding about the shipping stack at -23 dB and it is logged
    /// rather than chased</b>; unit 256 measured it and could not close it.
    /// </para>
    /// <para>
    /// <b>The full table is committed at <c>docs/unit256-runs/combining-panel-minus23.txt</c>
    /// and it is printed in full before the assertion runs</b>, so the red costs none of its own
    /// numbers.
    /// </para>
    /// <para>
    /// <b>And a second reading, which the assertion order hides.</b> <c>DeepestHearings</c> is
    /// <b>3</b> at this rung, not 4, so the third assertion would have gone red too — but that is
    /// not <c>B17</c>. At -23 dB almost nothing decodes, so no slot ever held four hearings'
    /// worth of candidates to accumulate. <b>The <c>== 4</c> assertion only means anything at a
    /// rung where the combiner has candidates to work with</b>, and saying so is part of this
    /// unit's report.
    /// </para>
    /// </summary>
    [Fact]
    public void TheCombiningPanelAtMinus23() =>
        Panel(
            -23.0,
            "the second step of the search for the combined row's 50 per cent crossing. If it is "
            + "still above 50 here, ONE rung at -24 dB is licensed and nothing below -24 dB is.");
}

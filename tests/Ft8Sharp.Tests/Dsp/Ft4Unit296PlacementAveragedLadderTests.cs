using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 296 task 7 — step 1's fourth criterion, the sensitivity ladder, re-drawn
/// placement-averaged beside the on-grid one rather than instead of it.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>WHAT THIS CATCHES, AND <c>Ft4SensitivityLadderTests</c> cannot.</b> That ladder is drawn at
/// <c>const float frequency = 1000.0f</c> through <c>Ft4Waveform.SynthesizeSlot</c>, which is a bin
/// centre and a whole number of time steps — so it is FT4's threshold <em>on Hamlet's own analysis
/// grid</em>. A station on 14.080 does not land there. The breakage is every unit after this one
/// reading that curve as FT4's threshold and sizing its own work against a figure two decibels
/// better than the band will give: the ladder is what a phase quotes when it says how weak a signal
/// this mode reads, and quoting the on-grid one alone is a claim about a placement Hamlet chose.
/// </para>
/// <para>
/// <b>Every rung of the on-grid ladder is present, and this is deliberate.</b> A ladder missing
/// rungs reads like a complete one and would misstate FT4's threshold for every unit after this. The
/// eight rungs are <c>Ft4SensitivityLadderTests</c>'s own, unchanged. What is smaller is the corpus
/// subset, and it is <b>stated</b> rather than left implicit — sixteen messages a cell against that
/// ladder's hundred and six, because twenty-five cells multiply everything by twenty-five.
/// </para>
/// <para>
/// <b>A wrong decode is counted separately from a missed one at every rung and every cell</b>, and
/// reported where it is zero. One transmission goes into each slot, so any other message coming out
/// of it is a message nobody sent.
/// </para>
/// <para>
/// <b>WHAT THIS IS NOT.</b> Everything <c>Ft4SensitivityLadderTests</c>'s own remarks disclaim
/// applies here unchanged: it is not a step 6 result, it is not comparable with any published FT4
/// threshold, and it is a curve over audio this library synthesized itself — no fading, no drift, no
/// neighbours, one signal in an empty slot, one process, one seed. What it adds is the one axis that
/// ladder had no way to vary.
/// </para>
/// </remarks>
public class Ft4Unit296PlacementAveragedLadderTests(ITestOutputHelper output)
{
    /// <summary><c>Ft4SensitivityLadderTests</c>'s own eight rungs, unchanged.</summary>
    private static readonly double[] Rungs = [0.0, -5.0, -10.0, -13.0, -15.0, -17.0, -19.0, -21.0];

    /// <summary>The seed. Fixed, and it is this unit's throughout.</summary>
    private const int Seed = 296;

    /// <summary>
    /// The corpus subset, stated: every seventh entry of <c>Ft4RoundTripCorpus</c>'s 106, which is
    /// sixteen messages spanning the corpus's kinds rather than a block of one of them. Sixteen a
    /// cell over twenty-five cells is <b>400 trials a rung</b> placement-averaged, and 3200 slot
    /// decodes for the whole ladder.
    /// </summary>
    private const int SubsetStride = 7;

    /// <summary>
    /// <b>The ladder, drawn twice: on the analysis grid and averaged over the whole lattice.</b>
    /// </summary>
    [Fact]
    public void TheFt4LadderIsDrawnPlacementAveragedBesideTheOnGridOne()
    {
        var started = Stopwatch.StartNew();
        var corpus = Ft4RoundTripCorpus.Build();
        var subset = Ft4Unit296PlacementLattice.Subset(corpus, SubsetStride);
        var decoder = new Ft4SlotDecoder();
        var geometry = decoder.Geometry;
        var lattice = Ft4Unit296PlacementLattice.Build();

        output.WriteLine("THE FT4 SENSITIVITY LADDER, PLACEMENT-AVERAGED");
        output.WriteLine(
            $"  grid              the shipping one - time oversampling "
            + $"{geometry.TimeOversampling}, frequency oversampling "
            + $"{geometry.FrequencyOversampling}");
        output.WriteLine(
            $"  lattice           {lattice.Count} cells spanning one whole tone spacing "
            + $"({Ft4Unit296PlacementLattice.ToneSpacingHz:F4} Hz) and one whole symbol period "
            + $"({Ft4Unit296PlacementLattice.SymbolPeriodSamples} samples)");
        output.WriteLine(
            $"  corpus subset     every {SubsetStride}th of {corpus.Count} = {subset.Count} "
            + $"messages, each sent once per cell per rung");
        output.WriteLine(
            $"  trials a rung     {subset.Count} on grid, {subset.Count * lattice.Count} "
            + "placement-averaged");
        output.WriteLine(
            $"  noise             white Gaussian, seed {Seed}, the same draw at every cell of a rung");
        output.WriteLine(
            "  axis              power in a 2500 Hz reference bandwidth, measured per trial");
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"{"rung",-8}{"delivered",-12}{"on grid",-16}{"worst cell",-20}"
            + $"{"placement-averaged",-24}WRONG");

        var totalWrong = 0;
        var totalTrials = 0;

        foreach (var rung in Rungs)
        {
            var atRung = lattice
                .Select(p => Ft4Unit296PlacementLattice.Measure(decoder, subset, p, rung, Seed))
                .ToList();

            var onGrid = atRung.Single(c => c.Placement.OnGrid);
            var worst = atRung.OrderBy(c => c.Decoded).First();
            var decoded = atRung.Sum(c => c.Decoded);
            var trials = atRung.Sum(c => c.Trials);
            var wrong = atRung.Sum(c => c.Wrong);

            totalWrong += wrong;
            totalTrials += trials;

            output.WriteLine(
                $"{rung,-8:F1}{atRung.Average(c => c.DeliveredMean),-12:F2}"
                + $"{onGrid.Decoded + " of " + onGrid.Trials + $" ({onGrid.Rate:P0})",-16}"
                + $"{worst.Decoded + " of " + worst.Trials + " at " + worst.Placement.Name,-20}"
                + $"{decoded + " of " + trials + $" ({decoded / (double)trials:P1})",-24}{wrong}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine(
            $"WRONG DECODES OVER THE WHOLE LADDER: {totalWrong} in {totalTrials} slots");
        output.WriteLine(
            "  reported whether zero or not. One transmission a slot, so anything else out of one");
        output.WriteLine("  is a message nobody sent.");
        output.WriteLine(string.Empty);
        output.WriteLine(
            $"drawn in {started.Elapsed.TotalMinutes:F1} minutes at "
            + $"{started.Elapsed.TotalMilliseconds / totalTrials:F0} ms a slot including synthesis "
            + "and noise");

        // CRITERION 4 IS NICE-TO-PASS AND A CURVE RATHER THAN A GATE, so no rate is asserted. What
        // IS asserted is the phase's own ruling, which is not a curve: a wrong decode is counted
        // separately and there are none.
        Assert.Equal(0, totalWrong);

        Assert.True(
            totalTrials == Rungs.Length * subset.Count * lattice.Count,
            $"the ladder drew {totalTrials} trials and eight rungs of {subset.Count} messages over "
            + $"{lattice.Count} cells is {Rungs.Length * subset.Count * lattice.Count}. A ladder "
            + "missing rungs reads like a complete one.");
    }
}

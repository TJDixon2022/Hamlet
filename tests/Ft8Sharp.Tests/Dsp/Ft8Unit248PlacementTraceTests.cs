// Copyright (c) Hamlet contributors. Licensed under the MIT licence.

using System;
using System.Collections.Generic;
using System.Linq;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 248 task 1: what does the coarse grid cost, measured with nothing new.</b> Not a line of
/// production code is exercised here that did not exist at unit 247 — every number comes out of
/// <see cref="Ft8LadderHarness.Run"/>, which already takes a frequency and an offset.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this is measured before the re-sync is written.</b> The waterfall places a candidate to
/// within a quarter of a symbol in time and a quarter of a tone in frequency, and every figure this
/// phase has recorded was taken at <see cref="Ft8LadderHarness.DefaultFrequencyHz"/> and
/// <see cref="Ft8LadderHarness.DefaultOffsetSamples"/> — a placement that sits exactly on a bin
/// centre and exactly on a sub-block boundary. <b>Whether that flatters the baseline is unknown
/// until it is swept</b>, and the spread across one cell is the ceiling on what any fine
/// synchroniser could recover on this instrument.
/// </para>
/// <para>
/// <b>Nothing here asserts a rate.</b> One thing is asserted: zero wrong decodes, which is the
/// criterion this phase cannot trade. Everything else is printed and read.
/// </para>
/// </remarks>
public class Ft8Unit248PlacementTraceTests(ITestOutputHelper output)
{
    /// <summary>One whole block of the 51-message population: enough for the shape.</summary>
    private const int SweepTrials = 51;

    /// <summary>Six whole blocks, the count every figure this phase quotes was taken at.</summary>
    private const int CornerTrials = 306;

    /// <summary>The rung nearest the crossing, where the rate is most sensitive to placement.</summary>
    private const double SweepRung = -20.0;

    /// <summary>
    /// Quarter steps of one 3.125 Hz transform bin, in hertz, added to the default frequency.
    /// </summary>
    private static readonly double[] FrequencySteps = { 0.0, 0.78, 1.56, 2.34 };

    /// <summary>Quarter steps of one 960-sample sub-block, added to the default offset.</summary>
    private static readonly int[] TimeSteps = { 0, 240, 480, 720 };

    /// <summary>
    /// <b>Task 1 step 1: the grid arithmetic, confirmed against the tree rather than quoted.</b>
    /// </summary>
    /// <remarks>
    /// The instruction states the grid from the tree's own constants and asks for it to be checked.
    /// <b>If the ladder's default placement is not on-grid in both axes, the rest of task 1 changes
    /// meaning</b>, so that is asserted here rather than printed and hoped for.
    /// </remarks>
    [Fact]
    public void TheGridArithmeticAndWhereTheLaddersDefaultPlacementSitsOnIt()
    {
        var geometry = new Ft8WaterfallGeometry(Ft8WaterfallGeometry.DefaultSampleRate);

        output.WriteLine("THE COARSE GRID, from the tree's own constants at 12 kHz");
        output.WriteLine("=======================================================");
        output.WriteLine($"  SymbolPeriodSeconds     {Ft8WaterfallGeometry.SymbolPeriodSeconds:F3} s");
        output.WriteLine($"  BlockSize               {geometry.BlockSize} samples");
        output.WriteLine($"  SubblockSize            {geometry.SubblockSize} samples "
            + $"({geometry.SubblockSize / (double)Ft8WaterfallGeometry.DefaultSampleRate:F3} s)");
        output.WriteLine($"  TransformLength         {geometry.TransformLength}");
        output.WriteLine($"  TransformBinSpacingHz   {geometry.TransformBinSpacingHz:F4} Hz");
        output.WriteLine($"  ToneSpacingHz           {geometry.ToneSpacingHz:F4} Hz");
        output.WriteLine($"  TimeOversampling        {geometry.TimeOversampling}");
        output.WriteLine($"  FrequencyOversampling   {geometry.FrequencyOversampling}");
        output.WriteLine(string.Empty);
        output.WriteLine("  A candidate is therefore placed to within +/- half a sub-block in time");
        output.WriteLine($"  (+/- {geometry.SubblockSize / 2} samples, "
            + $"{geometry.SubblockSize / 2.0 / Ft8WaterfallGeometry.DefaultSampleRate:F3} s, a quarter "
            + "of a symbol) and +/- half a");
        output.WriteLine($"  transform bin in frequency (+/- {geometry.TransformBinSpacingHz / 2.0:F4} Hz, "
            + $"a quarter of a {geometry.ToneSpacingHz:F2} Hz tone).");
        output.WriteLine(string.Empty);

        // WHERE THE LADDER'S DEFAULT SITS. Both axes, from the harness's own constants.
        var binsExactly = Ft8LadderHarness.DefaultFrequencyHz / geometry.TransformBinSpacingHz;
        var onBinCentre = geometry.TryBinFor(Ft8LadderHarness.DefaultFrequencyHz, out var bin, out var sub);
        var placedAt = geometry.FrequencyHz(bin, sub);
        var subblocks = Ft8LadderHarness.DefaultOffsetSamples / (double)geometry.SubblockSize;

        output.WriteLine("THE LADDER'S DEFAULT PLACEMENT");
        output.WriteLine("==============================");
        output.WriteLine($"  DefaultFrequencyHz      {Ft8LadderHarness.DefaultFrequencyHz:F4} Hz "
            + $"= {binsExactly:F6} transform bins");
        output.WriteLine($"  nearest waterfall bin   bin {bin} sub {sub}, centred at {placedAt:F4} Hz, "
            + $"error {Ft8LadderHarness.DefaultFrequencyHz - placedAt:F6} Hz");
        output.WriteLine($"  in passband             {onBinCentre}");
        output.WriteLine($"  DefaultOffsetSamples    {Ft8LadderHarness.DefaultOffsetSamples} samples "
            + $"= {subblocks:F6} sub-blocks "
            + $"= {Ft8LadderHarness.DefaultOffsetSamples / (double)geometry.BlockSize:F6} blocks");
        output.WriteLine(string.Empty);

        Assert.Equal(1920, geometry.BlockSize);
        Assert.Equal(960, geometry.SubblockSize);
        Assert.Equal(3.125, geometry.TransformBinSpacingHz, 9);

        // SIX PLACES AND NOT NINE, and the reason is worth a line: SymbolPeriodSeconds is a float
        // const, so 1/0.160f is 6.2500001397 rather than 6.25 and every product downstream of it
        // carries that. The question here is whether the placement is on the grid, not whether a
        // float round-trips, and 0.14 microhertz is not a placement error.
        Assert.Equal(6.25, geometry.ToneSpacingHz, 6);

        // ON-GRID IN BOTH AXES, asserted rather than assumed.
        Assert.Equal(320.0, binsExactly, 9);
        Assert.Equal(Ft8LadderHarness.DefaultFrequencyHz, placedAt, 3);
        Assert.Equal(0, Ft8LadderHarness.DefaultOffsetSamples % geometry.SubblockSize);
        Assert.Equal(6.0, subblocks, 9);

        output.WriteLine("The ladder's default is exactly on a bin centre and exactly on a sub-block");
        output.WriteLine("boundary. Every figure this phase has recorded was taken at the one placement");
        output.WriteLine("where the coarse grid has nothing to lose.");
    }

    /// <summary>
    /// <b>Task 1 step 2: the placement sweep. Sixteen placements across one cell of the grid.</b>
    /// </summary>
    /// <remarks>
    /// Four quarter-steps of a transform bin crossed with four quarter-steps of a sub-block, at
    /// -20 dB, one whole block of 51 trials each, the port and the ordered-statistics column side by
    /// side. <b>The noise draw is identical between the two columns at every placement</b>, which is
    /// what <see cref="Ft8LadderHarness.Run"/> buys.
    /// </remarks>
    [Fact]
    public void ThePlacementSweepAcrossOneCellAtMinus20Decibels()
    {
        var rows = new List<string>();

        output.WriteLine($"THE PLACEMENT SWEEP: 16 placements, {SweepTrials} trials each, "
            + $"{SweepRung:F1} dB");
        output.WriteLine("================================================================");
        output.WriteLine("  dHz is added to 1000.0000 Hz; dSamp is added to 5760 samples.");
        output.WriteLine(string.Empty);
        output.WriteLine("   dHz  dSamp   PORT dec  miss  WRONG    rate    DEEP-OSD dec  miss  WRONG    rate");
        output.WriteLine("  ------------------------------------------------------------------------------");

        foreach (var dHz in FrequencySteps)
        {
            foreach (var dSamples in TimeSteps)
            {
                var results = Run(SweepRung, SweepTrials, dHz, dSamples);

                var line =
                    $"  {dHz,4:F2}  {dSamples,5}   "
                    + $"     {results[0].Decoded,4} {results[0].Missed,5}  {results[0].Wrong,5} "
                    + $"{results[0].Rate,7:F1}    "
                    + $"         {results[1].Decoded,4} {results[1].Missed,5}  {results[1].Wrong,5} "
                    + $"{results[1].Rate,7:F1}";

                rows.Add(line);
                output.WriteLine(line);

                foreach (var result in results)
                {
                    Assert.True(
                        result.Wrong == 0,
                        $"placement +{dHz:F2} Hz +{dSamples} samples: {result.Decoder} returned "
                            + $"{result.Wrong} messages that were not sent.");
                }
            }
        }

        output.WriteLine(string.Empty);
        output.WriteLine("Read the two rate columns down: the spread between the best and the worst");
        output.WriteLine("cell position is what perfect alignment could recover on this instrument.");
    }

    /// <summary>
    /// <b>Task 1 step 3: the two corners the sweep found, at full weight.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The placements are the sweep's answer, written down.</b> The best and the worst cell
    /// position that <see cref="ThePlacementSweepAcrossOneCellAtMinus20Decibels"/> printed, re-run at
    /// 306 trials at -20 dB and again at -21 dB. <b>This is the size of the prize</b>: the difference
    /// between the on-grid rate the whole phase has quoted and the worst-cell rate is what a fine
    /// synchroniser could recover, and it is measured rather than argued.
    /// </para>
    /// <para>
    /// The two corners are constants here rather than a re-sweep because 306 trials at four
    /// configurations is already 2448 slot decodes, and re-deriving them would double the wall clock
    /// to learn nothing. <b>Change them only against a fresh sweep printout.</b>
    /// </para>
    /// </remarks>
    [Fact]
    public void TheBestAndWorstPlacementsAt306TrialsOnTwoRungs()
    {
        foreach (var (label, dHz, dSamples) in Corners)
        {
            foreach (var rung in new[] { -20.0, -21.0 })
            {
                var results = Run(rung, CornerTrials, dHz, dSamples);

                output.WriteLine(
                    "==============================================================================");
                output.WriteLine($"{label}: +{dHz:F2} Hz, +{dSamples} samples, {rung:F1} dB, "
                    + $"{CornerTrials} trials");
                output.WriteLine(
                    "==============================================================================");
                output.WriteLine(Ft8LadderHarness.Header);

                foreach (var result in results)
                {
                    output.WriteLine(result.AsRow());

                    Assert.True(
                        result.Wrong == 0,
                        $"{label} at {rung:F1} dB: {result.Decoder} returned {result.Wrong} messages "
                            + "that were not sent.");
                }

                output.WriteLine(string.Empty);
            }
        }
    }

    /// <summary>
    /// <b>Task 1 step 5: the placement-averaged rate beside the on-grid one.</b>
    /// </summary>
    /// <remarks>
    /// <b>Real air is uniform over the cell and the ladder's default is one corner of it.</b> This
    /// walks the same 16 placements and states the mean, which is what the phase's baseline would
    /// read if the transmitter did not helpfully land on a bin centre. <b>It changes no target and
    /// restates no baseline</b> — that is the arbiter's to do with the number.
    /// </remarks>
    [Fact]
    public void ThePlacementAveragedRateBesideTheOnGridOne()
    {
        var portDecoded = 0;
        var osdDecoded = 0;
        var total = 0;
        var onGridPort = 0;
        var onGridOsd = 0;

        foreach (var dHz in FrequencySteps)
        {
            foreach (var dSamples in TimeSteps)
            {
                var results = Run(SweepRung, SweepTrials, dHz, dSamples);

                portDecoded += results[0].Decoded;
                osdDecoded += results[1].Decoded;
                total += SweepTrials;

                if (dHz == 0.0 && dSamples == 0)
                {
                    onGridPort = results[0].Decoded;
                    onGridOsd = results[1].Decoded;
                }

                foreach (var result in results)
                {
                    Assert.Equal(0, result.Wrong);
                }
            }
        }

        var (portLow, portHigh) = Ft8Step6Ladder.Wilson(portDecoded, total);
        var (osdLow, osdHigh) = Ft8Step6Ladder.Wilson(osdDecoded, total);

        output.WriteLine($"THE PLACEMENT-AVERAGED RATE AT {SweepRung:F1} dB");
        output.WriteLine("=====================================");
        output.WriteLine($"  uniform over the cell, {total} trials (16 placements x {SweepTrials})");
        output.WriteLine($"    Ft8Sharp        {portDecoded,5} of {total}  "
            + $"{100.0 * portDecoded / total,6:F1} per cent  [{portLow:F1}, {portHigh:F1}]");
        output.WriteLine($"    Deep OSD on     {osdDecoded,5} of {total}  "
            + $"{100.0 * osdDecoded / total,6:F1} per cent  [{osdLow:F1}, {osdHigh:F1}]");
        output.WriteLine(string.Empty);
        output.WriteLine($"  on-grid alone, {SweepTrials} trials");
        output.WriteLine($"    Ft8Sharp        {onGridPort,5} of {SweepTrials}  "
            + $"{100.0 * onGridPort / SweepTrials,6:F1} per cent");
        output.WriteLine($"    Deep OSD on     {onGridOsd,5} of {SweepTrials}  "
            + $"{100.0 * onGridOsd / SweepTrials,6:F1} per cent");
    }

    /// <summary>
    /// The best and the worst placement the sweep found, as a label and a displacement.
    /// </summary>
    internal static IReadOnlyList<(string Label, double FrequencyHz, int OffsetSamples)> Corners =>
        new[]
        {
            ("BEST PLACEMENT (on grid)", 0.00, 0),
            ("WORST PLACEMENT (cell centre)", 1.56, 480),
        };

    /// <summary>
    /// The port and the ordered-statistics column at one placement, through the unmodified harness.
    /// </summary>
    private static IReadOnlyList<Ft8LadderHarness.Result> Run(
        double rung, int trials, double frequencyOffsetHz, int offsetSamples) =>
        Ft8LadderHarness.Run(
            rung,
            trials,
            decoders:
            [
                Ft8LadderHarness.Available()[0],
                new Ft8LadderHarness.Decoder(
                    "Deep OSD on",
                    samples => new Ft8Sharp.Deep.Ft8DeepSlotDecoder(
                        osd: Ft8Sharp.Deep.Ft8DeepOsdSettings.Default).Decode(samples)),
            ],
            frequencyHz: Ft8LadderHarness.DefaultFrequencyHz + frequencyOffsetHz,
            offsetSamples: Ft8LadderHarness.DefaultOffsetSamples + offsetSamples);
}

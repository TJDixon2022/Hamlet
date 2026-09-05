// Copyright (c) Hamlet contributors. Licensed under the MIT licence.

using System;
using System.Collections.Generic;
using System.Linq;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The scoreboard, whole. Nothing unit 248 claims is claimed without it.</b> Three rungs, three
/// columns, both placements, 306 trials each, three counts on every row.
/// </summary>
/// <remarks>
/// <para>
/// <b>The three columns are chosen so that one difference is one named change.</b> Column one is the
/// port. Column two is the sibling with <b>fine sync on and ordered statistics and combining OFF</b>,
/// so the difference between one and two is <em>where the eight magnitudes are measured from and at
/// what position</em> and nothing else - which is step 4's fourth exit. Column three is the sibling
/// with <b>ordered statistics on and fine sync off</b>, which is step 2's regression column and is
/// here to show that nothing tonight moved a number underneath it. <b>Fine sync and OSD are never
/// stacked here</b> and no combined figure is reported as step 4's.
/// </para>
/// <para>
/// <b>Exit 2 is judged at the ladder's default on-grid placement</b>, because that is where every
/// figure this phase has recorded was taken and a crossing compared across two placements is not a
/// comparison. The cell-centre pair is quoted beside it and is a separate statement.
/// </para>
/// <para>
/// <b>The 50 per cent crossing is an interpolation and is quoted as one</b>, linearly between the
/// -19 and -20 rungs, which is the arithmetic <c>HM-OPEN-067</c>'s "near -19.5" was read off and the
/// one unit 246 used for -19.54 and -19.81.
/// </para>
/// </remarks>
public class Ft8Unit248ScoreboardTests(ITestOutputHelper output)
{
    /// <summary>Six whole blocks of the 51-message population.</summary>
    private const int Trials = 306;

    /// <summary>The worst placement task 1 found: the centre of one coarse cell.</summary>
    private const double WorstFrequencyOffsetHz = 1.56;

    private const int WorstOffsetSamples = 480;

    /// <summary>
    /// <b>The whole thing: three rungs, three columns, both placements, 306 trials each.</b>
    /// </summary>
    [Fact]
    public void TheLadderAtMinus19Minus20AndMinus21OverThreeColumnsAndBothPlacements()
    {
        foreach (var (placement, offsetHz, offsetSamples) in Placements)
        {
            var rates = new Dictionary<string, Dictionary<double, double>>();

            output.WriteLine(
                "=============================================================================");
            output.WriteLine($"{placement}: +{offsetHz:F2} Hz, +{offsetSamples} samples, "
                + $"{Trials} trials a rung");
            output.WriteLine(
                "=============================================================================");

            foreach (var rung in new[] { -19.0, -20.0, -21.0 })
            {
                var results = Ft8LadderHarness.Run(
                    rung,
                    Trials,
                    decoders: Columns(),
                    frequencyHz: Ft8LadderHarness.DefaultFrequencyHz + offsetHz,
                    offsetSamples: Ft8LadderHarness.DefaultOffsetSamples + offsetSamples);

                output.WriteLine(Ft8LadderHarness.Header);
                foreach (var result in results)
                {
                    output.WriteLine(result.AsRow());

                    if (!rates.TryGetValue(result.Decoder, out var byRung))
                    {
                        byRung = new Dictionary<double, double>();
                        rates[result.Decoder] = byRung;
                    }

                    byRung[rung] = result.Rate;

                    Assert.True(
                        result.Wrong == 0,
                        $"{placement} at {rung:F1} dB: {result.Decoder} returned {result.Wrong} "
                            + "messages that were not sent. A decode nobody sent is worse than a "
                            + "decode missed, and an approach that produces one is rejected."
                            + Environment.NewLine
                            + string.Join(Environment.NewLine, result.WrongReturns));
                }

                output.WriteLine(string.Empty);
            }

            output.WriteLine("  THE 50 PER CENT CROSSING, interpolated linearly between the -19 and");
            output.WriteLine("  -20 dB rungs, which is how HM-OPEN-067 and unit 246 both read theirs.");
            output.WriteLine("  column                     -19 dB   -20 dB   crossing");
            output.WriteLine("  ------------------------------------------------------");

            foreach (var column in rates.Keys)
            {
                var high = rates[column][-19.0];
                var low = rates[column][-20.0];
                var crossing = Crossing(high, low);

                output.WriteLine(
                    $"  {column,-24} {high,6:F1}   {low,6:F1}   "
                    + (double.IsNaN(crossing) ? "  not bracketed" : $"{crossing,8:F2} dB"));
            }

            output.WriteLine(string.Empty);
        }
    }

    /// <summary>
    /// <b>The regression check: nothing tonight moved step 2's number underneath the new one.</b>
    /// </summary>
    /// <remarks>
    /// Unit 246 left the ordered statistics column at <b>10.8 per cent, 33 of 306, zero wrong</b> at
    /// -21 dB with the port at <b>4.2 per cent, 13 of 306</b>, and unit 247 asserted both again.
    /// <b>Unit 248 changed <c>Ft8DeepSlotDecoder</c>'s samples-carrying entry point</b>, so a claim
    /// that it changed no decision is worth exactly what it is measured at.
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
                    samples => new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default)
                        .Decode(samples)),
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
            "what unit 246 left and unit 247 kept. Step 2's number did not move underneath step 4's.");
    }

    /// <summary>
    /// <b>What the fine sync stage actually did, and the submission arithmetic in full.</b>
    /// </summary>
    /// <remarks>
    /// <b>A rate that moved with no visible re-sync activity behind it is not evidence.</b> This walks
    /// one whole block at each rung and each placement with the counts read off the decoder after
    /// every slot, and it reports the worst single slot rather than the mean, with its candidate
    /// count and how many of them were re-synced.
    /// </remarks>
    [Fact]
    public void WhatTheFineSyncStageDidAndWhatItSubmitted()
    {
        output.WriteLine("  rung  placement      slots   cand   offered   resync   accepted   "
            + "mean |dt| s   mean |df| Hz   worst dt   worst df   edge t   edge f");
        output.WriteLine(new string('-', 150));

        var submissionsAcrossEverything = 0L;
        var worstSlotMilliseconds = 0.0;
        var worstSlotCandidates = 0;
        var worstSlotResynced = 0;
        var worstSlotWhere = string.Empty;

        foreach (var (placement, offsetHz, offsetSamples) in Placements)
        {
            foreach (var rung in new[] { -19.0, -20.0, -21.0 })
            {
                var decoder = new Ft8DeepSlotDecoder(fineSync: Ft8DeepFineSyncSettings.Default);

                var slots = 0;
                var candidates = 0L;
                var offered = 0L;
                var resynced = 0L;
                var accepted = 0L;
                var timeEdges = 0L;
                var frequencyEdges = 0L;
                var timeTotal = 0.0;
                var frequencyTotal = 0.0;
                var worstTime = 0.0;
                var worstFrequency = 0.0;

                Ft8LadderHarness.Run(
                    rung,
                    51,
                    decoders:
                    [
                        new Ft8LadderHarness.Decoder(
                            "Deep fine sync",
                            samples =>
                            {
                                var clock = System.Diagnostics.Stopwatch.StartNew();
                                var result = decoder.Decode(samples);
                                clock.Stop();

                                var counts = decoder.LastFineSync;
                                slots++;
                                candidates += result.CandidateCount;
                                offered += counts.Offered;
                                resynced += counts.Resynced;
                                accepted += counts.Accepted;
                                timeEdges += counts.OnTimeEdge;
                                frequencyEdges += counts.OnFrequencyEdge;
                                timeTotal += counts.TotalTimeShiftSeconds;
                                frequencyTotal += counts.TotalFrequencyShiftHz;
                                worstTime = Math.Max(worstTime, counts.WorstTimeShiftSeconds);
                                worstFrequency =
                                    Math.Max(worstFrequency, counts.WorstFrequencyShiftHz);

                                if (clock.Elapsed.TotalMilliseconds > worstSlotMilliseconds)
                                {
                                    worstSlotMilliseconds = clock.Elapsed.TotalMilliseconds;
                                    worstSlotCandidates = result.CandidateCount;
                                    worstSlotResynced = counts.Resynced;
                                    worstSlotWhere = $"{rung:F1} dB, {placement}";
                                }

                                return result;
                            }),
                    ],
                    frequencyHz: Ft8LadderHarness.DefaultFrequencyHz + offsetHz,
                    offsetSamples: Ft8LadderHarness.DefaultOffsetSamples + offsetSamples);

                submissionsAcrossEverything += resynced;

                output.WriteLine(
                    $"  {rung,5:F1}  {placement,-13}  {slots,5}  {candidates,5}   {offered,7}   "
                    + $"{resynced,6}   {accepted,8}   "
                    + $"{(resynced == 0 ? 0.0 : timeTotal / resynced),11:F4}   "
                    + $"{(resynced == 0 ? 0.0 : frequencyTotal / resynced),12:F4}   "
                    + $"{worstTime,8:F4}   {worstFrequency,8:F4}   "
                    + $"{(resynced == 0 ? 0.0 : 100.0 * timeEdges / resynced),5:F1}%   "
                    + $"{(resynced == 0 ? 0.0 : 100.0 * frequencyEdges / resynced),5:F1}%");

                // ONE SUBMISSION PER CANDIDATE OFFERED, ASSERTED RATHER THAN INTENDED.
                Assert.Equal(offered, resynced);
            }
        }

        output.WriteLine(string.Empty);
        output.WriteLine("THE SUBMISSION ARITHMETIC, IN FULL");
        output.WriteLine("==================================");
        output.WriteLine($"  submissions across this whole measurement   {submissionsAcrossEverything}");
        output.WriteLine($"  expected false accepts at one in 16384      "
            + $"{Ft8DeepCombineSettings.ExpectedFalseAccepts(submissionsAcrossEverything):F3}");
        output.WriteLine($"  worst a single slot could submit            "
            + $"{new Ft8SyncSearch().CandidateLimit} (the candidate limit)");
        output.WriteLine($"  which is                                    "
            + $"{Ft8DeepCombineSettings.ExpectedFalseAccepts(140):F4} expected false accepts a slot");
        output.WriteLine(string.Empty);
        output.WriteLine("WORST-CASE TIME PER SLOT");
        output.WriteLine("========================");
        output.WriteLine($"  the worst single slot observed   {worstSlotMilliseconds:F1} ms "
            + $"at {worstSlotWhere}");
        output.WriteLine($"  over                             {worstSlotCandidates} candidates, "
            + $"{worstSlotResynced} of them re-synced");
        output.WriteLine($"  margin against FT8's 15 seconds  "
            + $"{15000.0 / Math.Max(worstSlotMilliseconds, 1e-9):F0}-fold");
        output.WriteLine($"  against the tenfold margin of 1.5 s: "
            + (worstSlotMilliseconds < 1500.0 ? "inside it" : "OUTSIDE IT"));
    }

    /// <summary>The two placements every table above is quoted at.</summary>
    private static IReadOnlyList<(string Placement, double OffsetHz, int OffsetSamples)> Placements =>
        new[]
        {
            ("ON GRID", 0.0, 0),
            ("CELL CENTRE", WorstFrequencyOffsetHz, WorstOffsetSamples),
        };

    /// <summary>
    /// <b>Three columns and one named change between the first two.</b> A fresh decoder per trial,
    /// as unit 247 had it, so no state carries between slots.
    /// </summary>
    private static IReadOnlyList<Ft8LadderHarness.Decoder> Columns() =>
    [
        Ft8LadderHarness.Available()[0],
        new Ft8LadderHarness.Decoder(
            "Deep fine sync",
            samples => new Ft8DeepSlotDecoder(fineSync: Ft8DeepFineSyncSettings.Default)
                .Decode(samples)),
        new Ft8LadderHarness.Decoder(
            "Deep OSD on",
            samples => new Ft8DeepSlotDecoder(osd: Ft8DeepOsdSettings.Default).Decode(samples)),
    ];

    /// <summary>
    /// <b>The 50 per cent crossing by linear interpolation between two rungs a decibel apart.</b>
    /// </summary>
    /// <remarks>
    /// <b>Quoted as an interpolation and never as a measured crossing.</b> The rung above must be at
    /// or over 50 per cent and the rung below under it, or 50 per cent is not bracketed at all and
    /// this returns <see cref="double.NaN"/> rather than extrapolating - which is what
    /// <c>HM-OPEN-067</c>'s "near -19.5" and unit 246's -19.54 and -19.81 were all read off.
    /// </remarks>
    private static double Crossing(double rateAtMinus19, double rateAtMinus20)
    {
        if (rateAtMinus19 < 50.0 || rateAtMinus20 > 50.0)
        {
            return double.NaN;
        }

        if (Math.Abs(rateAtMinus19 - rateAtMinus20) < 1e-9)
        {
            return double.NaN;
        }

        return -19.0 - ((rateAtMinus19 - 50.0) / (rateAtMinus19 - rateAtMinus20));
    }
}

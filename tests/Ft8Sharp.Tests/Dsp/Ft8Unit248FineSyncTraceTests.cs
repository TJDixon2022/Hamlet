// Copyright (c) Hamlet contributors. Licensed under the MIT licence.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ft8Sharp.Deep;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 248 task 3: what the search's step buys, how often it stops at its own edge, and what one
/// candidate's re-sync costs in milliseconds.</b>
/// </summary>
/// <remarks>
/// <b>The step is measured on task 2's distance instrument rather than chosen.</b> A step finer than
/// the measurement can distinguish is tuning, and the whole point of measuring is to be able to say
/// which side of that line the default sits on.
/// </remarks>
public class Ft8Unit248FineSyncTraceTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private const int N = LdpcDecoder.CodewordBits;

    /// <summary>The worst placement task 1 found: the centre of one coarse cell.</summary>
    private const double WorstFrequencyOffsetHz = 1.56;

    private const int WorstOffsetSamples = 480;

    /// <summary>Measured by <c>Ft8Unit248ExtractorTraceTests</c>. Exactly one symbol.</summary>
    private const double CandidateTimeBiasSeconds = -0.16;

    /// <summary>
    /// <b>What the step buys, on the distance instrument, in both axes.</b>
    /// </summary>
    /// <remarks>
    /// One whole block at -20 dB at the worst placement, the closest candidate taken in each trial,
    /// and the fine search run at a range of steps. <b>The column that decides it is the median
    /// hard-decision distance</b>, because that is what the code has to recover from; the cost column
    /// is what it is paid for. <b>A step whose row is indistinguishable from the row above it is
    /// finer than the measurement can resolve.</b>
    /// </remarks>
    [Fact]
    public void WhatTheStepBuysInEachAxisMeasuredOnTheDistanceInstrument()
    {
        const double rung = -20.0;

        var geometry = new Ft8WaterfallGeometry(Rate);
        var trials = Trace(rung);

        output.WriteLine($"WHAT THE STEP BUYS at {rung:F1} dB, {trials.Count} trials, cell centre");
        output.WriteLine("=============================================================");
        output.WriteLine("  the candidate's own position, unmoved, is the row to beat:");
        output.WriteLine($"    median {Median(trials.Select(t => t.GridDistance).ToList())} of {N}, "
            + $"{trials.Count(t => t.GridDistance <= 17)} of {trials.Count} inside 17");
        output.WriteLine($"    the ORACLE position reaches median "
            + $"{Median(trials.Select(t => t.OracleDistance).ToList())}, "
            + $"{trials.Count(t => t.OracleDistance <= 17)} inside 17");
        output.WriteLine(string.Empty);
        output.WriteLine("   time step   freq step   positions   median   <=17   ms/candidate   edge t   edge f");
        output.WriteLine("  ----------------------------------------------------------------------------------");

        foreach (var (timeStep, frequencyStep) in new[]
                 {
                     (0.020, 1.5625 / 3.0),
                     (0.010, 1.5625 / 3.0),
                     (0.005, 1.5625 / 3.0),
                     (0.0025, 1.5625 / 3.0),
                     (0.005, 1.5625),
                     (0.005, 1.5625 / 2.0),
                     (0.005, 1.5625 / 6.0),
                 })
        {
            var settings = new Ft8DeepFineSyncSettings(
                Ft8DeepFineSyncSettings.CellTimeSeconds,
                timeStep,
                Ft8DeepFineSyncSettings.CellFrequencyHz,
                frequencyStep);

            var search = new Ft8DeepFineSync(settings);
            var distances = new List<int>(trials.Count);
            var timeEdges = 0;
            var frequencyEdges = 0;
            var clock = new Stopwatch();

            foreach (var trial in trials)
            {
                clock.Start();
                var found = search.Search(trial.Baseband, trial.NominalSeconds);
                clock.Stop();

                if (found.OnTimeEdge)
                {
                    timeEdges++;
                }

                if (found.OnFrequencyEdge)
                {
                    frequencyEdges++;
                }

                distances.Add(BasebandDistance(
                    trial.Baseband, found.StartSeconds, found.FrequencyOffsetHz, trial.Codeword));
            }

            output.WriteLine(
                $"  {timeStep,10:F4}   {frequencyStep,9:F4}   {settings.PositionCount,9}   "
                + $"{Median(distances),6}   {distances.Count(d => d <= 17),4}   "
                + $"{clock.Elapsed.TotalMilliseconds / trials.Count,12:F2}   "
                + $"{100.0 * timeEdges / trials.Count,5:F1}%   {100.0 * frequencyEdges / trials.Count,5:F1}%");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  The cell is +/-0.0400 s and +/-1.5625 Hz; the default is 0.0050 s and 0.5208 Hz.");

        Assert.True(Ft8DeepFineSyncSettings.Default.CoversTheCell(geometry));
    }

    /// <summary>
    /// <b>The edge-hit rate and the distribution of offsets the search actually applied, on real
    /// candidates at both placements.</b>
    /// </summary>
    /// <remarks>
    /// <b>A high edge rate means the extent is too small and the search is reporting its edge rather
    /// than a peak.</b> The number is printed whichever way it comes out and the grid is not quietly
    /// widened to hide it. Note that <b>every candidate is searched here, not only the ones the port
    /// refused</b> - this is a trace of what the search does, and the loop's own rule is ruling 4's
    /// and is measured in <c>Ft8Unit248ScoreboardTests</c>.
    /// </remarks>
    [Fact]
    public void TheEdgeHitRateAndTheOffsetsTheSearchApplied()
    {
        var search = new Ft8DeepFineSync();
        var geometry = new Ft8WaterfallGeometry(Rate);

        foreach (var rung in new[] { -20.0, -21.0 })
        {
            foreach (var (label, offsetHz, offsetSamples) in new[]
                     {
                         ("ON GRID", 0.0, 0),
                         ("CELL CENTRE", WorstFrequencyOffsetHz, WorstOffsetSamples),
                     })
            {
                var monitor = new Ft8Monitor(geometry);
                var syncSearch = new Ft8SyncSearch();
                var population = Ft8Step6Ladder.Population();
                var noise = new GaussianNoise(
                    Ft8LadderHarness.DefaultSeed + (int)Math.Round(rung * 10.0));

                var timeShifts = new List<double>();
                var frequencyShifts = new List<double>();
                var timeEdges = 0;
                var frequencyEdges = 0;
                var candidatesSeen = 0;
                var clock = new Stopwatch();
                var worstSlotMilliseconds = 0.0;
                var worstSlotCandidates = 0;

                foreach (var entry in population)
                {
                    var placed = Ft8LadderHarness.DefaultFrequencyHz + offsetHz;
                    var (clean, _) = SearchFixture.OneSignal(
                        Rate, entry, placed, Ft8LadderHarness.DefaultOffsetSamples + offsetSamples);

                    var signalPower = SearchFixture.TransmissionPower(Rate, entry, placed);
                    var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);
                    var samples = SearchFixture.AddNoise(clean, noise, sigma, out _);

                    var waterfall = monitor.Analyse(samples);
                    var candidates = syncSearch.Find(waterfall);

                    var slotClock = Stopwatch.StartNew();

                    foreach (var candidate in candidates)
                    {
                        clock.Start();
                        var baseband = Ft8DeepBaseband.Build(
                            samples,
                            Rate,
                            geometry.FrequencyHz(candidate.BinOffset, candidate.FrequencySubOffset));

                        var found = search.Search(
                            baseband,
                            geometry.TimeSeconds(candidate.BlockOffset, candidate.TimeSubOffset)
                                + CandidateTimeBiasSeconds);
                        clock.Stop();

                        candidatesSeen++;
                        timeShifts.Add(found.TimeShiftSeconds);
                        frequencyShifts.Add(found.FrequencyShiftHz);

                        if (found.OnTimeEdge)
                        {
                            timeEdges++;
                        }

                        if (found.OnFrequencyEdge)
                        {
                            frequencyEdges++;
                        }
                    }

                    slotClock.Stop();
                    if (slotClock.Elapsed.TotalMilliseconds > worstSlotMilliseconds)
                    {
                        worstSlotMilliseconds = slotClock.Elapsed.TotalMilliseconds;
                        worstSlotCandidates = candidates.Count;
                    }
                }

                output.WriteLine(
                    "=========================================================================");
                output.WriteLine($"{rung:F1} dB, {label}: {population.Count} slots, "
                    + $"{candidatesSeen} candidates, every one searched");
                output.WriteLine(
                    "=========================================================================");
                output.WriteLine($"  edge hits    time {100.0 * timeEdges / candidatesSeen,5:F1}%   "
                    + $"frequency {100.0 * frequencyEdges / candidatesSeen,5:F1}%");
                output.WriteLine($"  cost         {clock.Elapsed.TotalMilliseconds / candidatesSeen,6:F2} ms "
                    + "a candidate, mix and filter and search together");
                output.WriteLine($"  worst slot   {worstSlotMilliseconds,8:F1} ms over "
                    + $"{worstSlotCandidates} candidates, all of them searched");
                output.WriteLine(string.Empty);
                output.WriteLine($"  time shift applied, seconds:      {Spread(timeShifts)}");
                output.WriteLine($"  frequency shift applied, hertz:   {Spread(frequencyShifts)}");
                output.WriteLine(string.Empty);
                output.WriteLine("  time shift histogram");
                foreach (var line in Histogram(timeShifts, 0.005))
                {
                    output.WriteLine(line);
                }

                output.WriteLine("  frequency shift histogram");
                foreach (var line in Histogram(frequencyShifts, 1.5625 / 3.0))
                {
                    output.WriteLine(line);
                }

                output.WriteLine(string.Empty);
            }
        }
    }

    /// <summary>One trial of the trace: the baseband, where the candidate said it was, and the truth.</summary>
    private sealed record Trial(
        Ft8DeepBaseband Baseband,
        double NominalSeconds,
        byte[] Codeword,
        int GridDistance,
        int OracleDistance);

    /// <summary>
    /// The closest candidate of each trial at the worst placement, with its baseband already built,
    /// so that a sweep over steps pays for the mixing once rather than once per step.
    /// </summary>
    private static List<Trial> Trace(double rung)
    {
        var geometry = new Ft8WaterfallGeometry(Rate);
        var monitor = new Ft8Monitor(geometry);
        var search = new Ft8SyncSearch();
        var population = Ft8Step6Ladder.Population();
        var noise = new GaussianNoise(Ft8LadderHarness.DefaultSeed + (int)Math.Round(rung * 10.0));

        var trials = new List<Trial>(population.Count);

        foreach (var entry in population)
        {
            var placed = Ft8LadderHarness.DefaultFrequencyHz + WorstFrequencyOffsetHz;
            var (clean, truth) = SearchFixture.OneSignal(
                Rate, entry, placed, Ft8LadderHarness.DefaultOffsetSamples + WorstOffsetSamples);

            var signalPower = SearchFixture.TransmissionPower(Rate, entry, placed);
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);
            var samples = SearchFixture.AddNoise(clean, noise, sigma, out _);
            var codeword = TrueCodeword(entry);

            var waterfall = monitor.Analyse(samples);
            var candidates = search.Find(waterfall);
            if (candidates.Count == 0)
            {
                continue;
            }

            var rank = ClosestRank(waterfall, candidates, codeword);
            var candidate = candidates[rank];

            var baseband = Ft8DeepBaseband.Build(
                samples, Rate, geometry.FrequencyHz(candidate.BinOffset, candidate.FrequencySubOffset));

            var nominal = geometry.TimeSeconds(candidate.BlockOffset, candidate.TimeSubOffset)
                + CandidateTimeBiasSeconds;

            var oracleBaseband = Ft8DeepBaseband.Build(samples, Rate, truth.BaseFrequencyHz);

            trials.Add(new Trial(
                baseband,
                nominal,
                codeword,
                BasebandDistance(baseband, nominal, 0.0, codeword),
                BasebandDistance(oracleBaseband, truth.OffsetSamples / (double)Rate, 0.0, codeword)));
        }

        return trials;
    }

    private static int ClosestRank(
        Ft8Waterfall waterfall, IReadOnlyList<Ft8Candidate> candidates, byte[] codeword)
    {
        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var hard = new byte[N];
        var rank = 0;
        var distance = N;

        for (var i = 0; i < candidates.Count; i++)
        {
            Ft8SoftSymbols.Extract(waterfall, candidates[i], ratios);
            Ft8SoftSymbols.Normalise(ratios);
            Ft8SoftSymbols.HardDecision(ratios, hard);

            var d = Distance(hard, codeword);
            if (d < distance)
            {
                distance = d;
                rank = i;
            }
        }

        return rank;
    }

    private static int BasebandDistance(
        Ft8DeepBaseband baseband, double seconds, double frequencyOffsetHz, byte[] codeword)
    {
        var ratios = new float[Ft8SoftSymbols.RatioCount];
        Ft8DeepBasebandExtractor.Extract(baseband, seconds, frequencyOffsetHz, ratios);
        Ft8SoftSymbols.Normalise(ratios);

        var hard = new byte[N];
        Ft8SoftSymbols.HardDecision(ratios, hard);
        return Distance(hard, codeword);
    }

    private static string Spread(List<double> values)
    {
        if (values.Count == 0)
        {
            return "none";
        }

        var sorted = values.OrderBy(v => v).ToArray();
        var mean = values.Average();
        var meanAbsolute = values.Average(Math.Abs);

        return $"mean {mean,8:F4}  mean |shift| {meanAbsolute,8:F4}  "
            + $"min {sorted[0],8:F4}  max {sorted[^1],8:F4}  "
            + $"zero {100.0 * values.Count(v => Math.Abs(v) < 1e-12) / values.Count,5:F1}%";
    }

    private static IEnumerable<string> Histogram(List<double> values, double bucket)
    {
        if (values.Count == 0)
        {
            yield break;
        }

        var counts = values
            .GroupBy(v => (int)Math.Round(v / bucket))
            .OrderBy(g => g.Key)
            .ToArray();

        foreach (var group in counts)
        {
            var share = 100.0 * group.Count() / values.Count;
            yield return $"    {group.Key * bucket,9:F4}  {group.Count(),6}  {share,5:F1}%  "
                + new string('#', (int)Math.Round(share / 2.0));
        }
    }

    private static int Median(List<int> values)
    {
        if (values.Count == 0)
        {
            return N;
        }

        var sorted = values.OrderBy(v => v).ToArray();
        return sorted[sorted.Length / 2];
    }

    private static int Distance(ReadOnlySpan<byte> hard, ReadOnlySpan<byte> codeword)
    {
        var distance = 0;
        for (var i = 0; i < hard.Length; i++)
        {
            if (hard[i] != codeword[i])
            {
                distance++;
            }
        }

        return distance;
    }

    private static byte[] TrueCodeword(EncodeCorpus.Entry entry)
    {
        var payload = new byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(entry.Message, payload);

        var packed = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, packed);

        var bits = new byte[N];
        for (var i = 0; i < N; i++)
        {
            bits[i] = (byte)((packed[i / 8] >> (7 - (i % 8))) & 1);
        }

        return bits;
    }
}

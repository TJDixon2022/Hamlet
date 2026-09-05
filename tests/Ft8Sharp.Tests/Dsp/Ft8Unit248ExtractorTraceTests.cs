// Copyright (c) Hamlet contributors. Licensed under the MIT licence.

using System;
using System.Collections.Generic;
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
/// <b>Unit 248 task 2: the baseband extractor measured against the port's, at the same position,
/// before anything is credited to a fine search.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>The control comes first because nothing after it means anything if the extractor is worse.</b>
/// If <c>Ft8DeepBasebandExtractor</c> reading from the samples at a coarse candidate's own grid
/// position decodes less than <c>Ft8SoftSymbols.Extract</c> reading from the waterfall at the same
/// place, then the extractor is the problem and no amount of re-synchronisation afterwards can be
/// credited or blamed.
/// </para>
/// <para>
/// <b>The oracle number is an oracle number and is added to no total.</b>
/// <c>SearchFixture.Truth</c> is used here and nowhere else - not in the fine search, not in the
/// scoreboard, not in any column that is scored. It says how far a perfect synchroniser could get,
/// which is the ceiling on everything task 3 can win.
/// </para>
/// </remarks>
public class Ft8Unit248ExtractorTraceTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private const int N = LdpcDecoder.CodewordBits;

    /// <summary>The worst placement <c>Ft8Unit248PlacementTraceTests</c> found: the cell centre.</summary>
    private const double WorstFrequencyOffsetHz = 1.56;

    private const int WorstOffsetSamples = 480;

    /// <summary>
    /// <b>Where a coarse candidate's nominal time actually is, relative to the signal it found.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Ft8WaterfallGeometry.TimeSeconds</c> says in its own remarks that it returns <em>the
    /// block's nominal position and not the centre of the window that produced it</em>, that the
    /// analysis frame is prefilled with zeros and slides so the samples behind a block reach back
    /// before it, and that <b>the exact alignment could not be settled by reading and is not
    /// asserted</b>. So it is measured here rather than derived, by
    /// <see cref="TheBiasBetweenACandidatesNominalTimeAndTheSignalItFound"/>, and the number that
    /// test prints is the constant below.
    /// </para>
    /// <para>
    /// <b>Getting this wrong would be a constant time error in every position this unit reports</b>,
    /// and it would look exactly like a fine search that does not work.
    /// </para>
    /// </remarks>
    private const double CandidateTimeBiasSeconds = -0.16;

    /// <summary>
    /// <b>The calibration: how far a candidate's nominal time sits from the signal's true start.</b>
    /// </summary>
    /// <remarks>
    /// Swept on the distance instrument rather than reasoned about. One block of the population at a
    /// loud rung, the closest candidate to the truth taken in each trial, and the new extractor run
    /// at that candidate's nominal time plus a bias running over two symbols either way in half-symbol
    /// steps. <b>The bias that minimises the median hard-decision distance is the answer</b>, and it
    /// should be flat-bottomed over nothing wider than the half-symbol step, or the sweep is not
    /// resolving what it claims to.
    /// </remarks>
    [Fact]
    public void TheBiasBetweenACandidatesNominalTimeAndTheSignalItFound()
    {
        const double rung = -14.0;

        var geometry = new Ft8WaterfallGeometry(Rate);
        var monitor = new Ft8Monitor(geometry);
        var search = new Ft8SyncSearch();
        var population = Ft8Step6Ladder.Population();

        var biases = Enumerable.Range(-4, 9)
            .Select(i => i * Ft8WaterfallGeometry.SymbolPeriodSeconds / 2.0)
            .ToArray();

        var distances = biases.Select(_ => new List<int>()).ToArray();
        var portDistances = new List<int>();

        output.WriteLine($"THE TIME BIAS SWEEP at {rung:F1} dB over {population.Count} trials");
        output.WriteLine("===================================================");

        var noise = BlockNoise(rung);

        for (var trial = 0; trial < population.Count; trial++)
        {
            var entry = population[trial];
            var (clean, _) = SearchFixture.OneSignal(
                Rate,
                entry,
                Ft8LadderHarness.DefaultFrequencyHz,
                Ft8LadderHarness.DefaultOffsetSamples);

            var samples = Noisy(
                clean, entry, Ft8LadderHarness.DefaultFrequencyHz, rung, noise);
            var codeword = TrueCodeword(entry);

            var waterfall = monitor.Analyse(samples);
            var candidates = search.Find(waterfall);
            if (candidates.Count == 0)
            {
                continue;
            }

            var closest = Closest(waterfall, candidates, codeword);
            portDistances.Add(closest.Distance);

            var candidate = candidates[closest.Rank];
            var baseband = Ft8DeepBaseband.Build(
                samples, Rate, geometry.FrequencyHz(candidate.BinOffset, candidate.FrequencySubOffset));

            var nominal = geometry.TimeSeconds(candidate.BlockOffset, candidate.TimeSubOffset);

            for (var b = 0; b < biases.Length; b++)
            {
                distances[b].Add(BasebandDistance(baseband, nominal + biases[b], 0.0, codeword));
            }
        }

        output.WriteLine($"  the port at the same candidates: median {Median(portDistances)} of {N}");
        output.WriteLine(string.Empty);
        output.WriteLine("   bias s   bias symbols   median distance   best   trials at 17 or less");
        output.WriteLine("  -------------------------------------------------------------------");

        var bestBias = 0.0;
        var bestMedian = int.MaxValue;

        for (var b = 0; b < biases.Length; b++)
        {
            var median = Median(distances[b]);
            var best = distances[b].Count == 0 ? N : distances[b].Min();
            var recoverable = distances[b].Count(d => d <= 17);

            output.WriteLine(
                $"  {biases[b],7:F3}   {biases[b] / Ft8WaterfallGeometry.SymbolPeriodSeconds,12:F1}   "
                + $"{median,15}   {best,4}   {recoverable,20}");

            if (median < bestMedian)
            {
                bestMedian = median;
                bestBias = biases[b];
            }
        }

        output.WriteLine(string.Empty);
        output.WriteLine($"  the bias that minimises the median is {bestBias:F3} s, "
            + $"which is {bestBias / Ft8WaterfallGeometry.SymbolPeriodSeconds:F1} symbols");

        Assert.Equal(CandidateTimeBiasSeconds, bestBias, 6);
    }

    /// <summary>
    /// <b>THE CONTROL. The new extractor at each coarse candidate's own grid position, against the
    /// port's, on the same slots and the same noise.</b>
    /// </summary>
    /// <remarks>
    /// One whole block of 51 trials at -21 dB, which is the rung this phase's number lives at. Both
    /// columns run through <see cref="Ft8LadderHarness.Run"/> unmodified so the noise draw is
    /// identical between them and the difference is the one named change: <b>where the eight
    /// magnitudes are measured from.</b>
    /// </remarks>
    [Fact]
    public void TheNewExtractorAtTheCoarseGridPositionAgainstThePortAtTheSamePosition()
    {
        foreach (var (label, offsetHz, offsetSamples) in new[]
                 {
                     ("ON GRID", 0.0, 0),
                     ("CELL CENTRE", WorstFrequencyOffsetHz, WorstOffsetSamples),
                 })
        {
            var results = Ft8LadderHarness.Run(
                -21.0,
                51,
                decoders:
                [
                    Ft8LadderHarness.Available()[0],
                    new Ft8LadderHarness.Decoder("baseband@grid", BasebandAtGrid),
                ],
                frequencyHz: Ft8LadderHarness.DefaultFrequencyHz + offsetHz,
                offsetSamples: Ft8LadderHarness.DefaultOffsetSamples + offsetSamples);

            output.WriteLine($"THE CONTROL at -21.0 dB, 51 trials, {label}");
            output.WriteLine("=========================================================");
            output.WriteLine(Ft8LadderHarness.Header);
            foreach (var result in results)
            {
                output.WriteLine(result.AsRow());
                Assert.Equal(0, result.Wrong);
            }

            output.WriteLine(string.Empty);
        }
    }

    /// <summary>
    /// <b>THE ORACLE CEILING. Three distance rows, and the third is what a perfect synchroniser
    /// would reach.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// At the worst placement task 1 found - the centre of one coarse cell - one whole block at
    /// -21 dB and one at -20 dB, measuring the hard-decision distance to the transmitted codeword
    /// from three places: the closest candidate's ratios through the <b>port's</b> <c>Extract</c>;
    /// the new extractor at that <b>same grid position</b>; and the new extractor at the
    /// <b>oracle position</b>, which is <c>Truth</c>'s exact frequency and exact offset.
    /// </para>
    /// <para>
    /// <b>The code's iterative recovery reaches zero at about 17 of 174.</b> How far the oracle row
    /// falls below the grid row is the ceiling on everything a fine search can win.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheOracleCeilingAtTheWorstPlacementTask1Found()
    {
        var geometry = new Ft8WaterfallGeometry(Rate);
        var monitor = new Ft8Monitor(geometry);
        var search = new Ft8SyncSearch();
        var population = Ft8Step6Ladder.Population();

        foreach (var rung in new[] { -21.0, -20.0 })
        {
            var port = new List<int>();
            var grid = new List<int>();
            var oracle = new List<int>();
            var noCandidate = 0;
            var noise = BlockNoise(rung);

            for (var trial = 0; trial < population.Count; trial++)
            {
                var entry = population[trial];
                var placed = Ft8LadderHarness.DefaultFrequencyHz + WorstFrequencyOffsetHz;
                var (clean, truth) = SearchFixture.OneSignal(
                    Rate,
                    entry,
                    placed,
                    Ft8LadderHarness.DefaultOffsetSamples + WorstOffsetSamples);

                var samples = Noisy(clean, entry, placed, rung, noise);
                var codeword = TrueCodeword(entry);

                var waterfall = monitor.Analyse(samples);
                var candidates = search.Find(waterfall);

                // THE ORACLE ROW IS TAKEN WHETHER OR NOT THE SEARCH FOUND ANYTHING, because it does
                // not use the search. That is the point of it.
                var oracleBaseband = Ft8DeepBaseband.Build(samples, Rate, truth.BaseFrequencyHz);
                oracle.Add(BasebandDistance(
                    oracleBaseband, truth.OffsetSamples / (double)Rate, 0.0, codeword));

                if (candidates.Count == 0)
                {
                    noCandidate++;
                    continue;
                }

                var closest = Closest(waterfall, candidates, codeword);
                port.Add(closest.Distance);

                var candidate = candidates[closest.Rank];
                var gridBaseband = Ft8DeepBaseband.Build(
                    samples,
                    Rate,
                    geometry.FrequencyHz(candidate.BinOffset, candidate.FrequencySubOffset));

                grid.Add(BasebandDistance(
                    gridBaseband,
                    geometry.TimeSeconds(candidate.BlockOffset, candidate.TimeSubOffset)
                        + CandidateTimeBiasSeconds,
                    0.0,
                    codeword));
            }

            output.WriteLine(
                "=============================================================================");
            output.WriteLine($"THE ORACLE CEILING at {rung:F1} dB, {population.Count} trials, "
                + $"cell centre (+{WorstFrequencyOffsetHz:F2} Hz, +{WorstOffsetSamples} samples)");
            output.WriteLine(
                "=============================================================================");
            output.WriteLine($"  trials with no candidate at all: {noCandidate}");
            output.WriteLine(string.Empty);
            output.WriteLine("  row                                    trials  median  best   worst   <=17");
            output.WriteLine("  -----------------------------------------------------------------------");
            output.WriteLine(Row("port Extract, closest candidate", port));
            output.WriteLine(Row("baseband, same grid position", grid));
            output.WriteLine(Row("baseband, ORACLE position", oracle));
            output.WriteLine(string.Empty);
            output.WriteLine("  Chance is 87 of 174. The code's iterative recovery reaches zero at about 17.");
            output.WriteLine(string.Empty);
        }
    }

    /// <summary>
    /// <b><c>HM-OPEN-074</c> re-measured over 306 trials, which is what that entry asked the unit
    /// taking step 4 to do before quoting its figure.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The entry says: about four per cent of trials at -21 dB have no candidate near the signal at
    /// all, measured as <b>two of 51</b> with the closest candidate at 71 and 81 of 174 against a
    /// chance distance of 87 — and that two trials is an estimate with a wide interval rather than a
    /// figure. <b>Six whole blocks is what settles it</b>, and the same walk at the cell centre says
    /// whether the placement changes it.
    /// </para>
    /// <para>
    /// <b>60 of 174 is the threshold the entry itself used</b> and it is kept rather than chosen
    /// afresh, so the two numbers are comparable. Nothing here re-syncs anything: refining a
    /// candidate that does not exist cannot help, and <c>Ft8SyncSearch</c> is the port's and is
    /// untouchable this phase.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheCandidateAvailabilityOfHmOpen074RemeasuredOver306Trials()
    {
        const double rung = -21.0;
        const int blocks = 6;

        var geometry = new Ft8WaterfallGeometry(Rate);
        var monitor = new Ft8Monitor(geometry);
        var search = new Ft8SyncSearch();
        var population = Ft8Step6Ladder.Population();

        output.WriteLine($"HM-OPEN-074 RE-MEASURED at {rung:F1} dB over "
            + $"{blocks * population.Count} trials");
        output.WriteLine("=================================================================");
        output.WriteLine("  placement      trials   none   >60 of 174   >=87 (chance)   median   "
            + "worst");
        output.WriteLine("  ------------------------------------------------------------------------");

        foreach (var (label, offsetHz, offsetSamples) in new[]
                 {
                     ("ON GRID", 0.0, 0),
                     ("CELL CENTRE", WorstFrequencyOffsetHz, WorstOffsetSamples),
                 })
        {
            var closest = new List<int>();
            var none = 0;

            for (var block = 0; block < blocks; block++)
            {
                var noise = new GaussianNoise(
                    Ft8LadderHarness.DefaultSeed + block + (int)Math.Round(rung * 10.0));

                foreach (var entry in population)
                {
                    var placed = Ft8LadderHarness.DefaultFrequencyHz + offsetHz;
                    var (clean, _) = SearchFixture.OneSignal(
                        Rate, entry, placed, Ft8LadderHarness.DefaultOffsetSamples + offsetSamples);

                    var samples = Noisy(clean, entry, placed, rung, noise);
                    var waterfall = monitor.Analyse(samples);
                    var candidates = search.Find(waterfall);

                    if (candidates.Count == 0)
                    {
                        none++;
                        continue;
                    }

                    closest.Add(Closest(waterfall, candidates, TrueCodeword(entry)).Distance);
                }
            }

            var far = closest.Count(d => d > 60);
            var chance = closest.Count(d => d >= 87);

            output.WriteLine(
                $"  {label,-13}  {closest.Count + none,6}   {none,4}   "
                + $"{far,4} ({100.0 * far / (closest.Count + none),4:F1}%)   "
                + $"{chance,6} ({100.0 * chance / (closest.Count + none),4:F1}%)   "
                + $"{Median(closest),6}   {closest.Max(),5}");
        }

        output.WriteLine(string.Empty);
        output.WriteLine("  HM-OPEN-074 read 2 of 51 above 60, which it called about four per cent");
        output.WriteLine("  and said was an estimate with a wide interval. This is the figure.");
    }

    /// <summary>
    /// <b>The window shape, measured once rather than swept.</b> A rectangular window exactly one
    /// symbol long against the <c>Ft8Monitor.HannSquaredSine</c> taper the waterfall applies.
    /// </summary>
    /// <remarks>
    /// The instruction is to pick one, measure the other once, and report both numbers. The
    /// rectangular window is the one this library uses and the reason is written at
    /// <c>Ft8DeepBaseband.TonePowerGrid</c>: over exactly one symbol the eight tone exponentials are
    /// orthogonal, so it is the matched filter for the alphabet. <b>The tapered figure below is a
    /// measurement and not an option offered.</b>
    /// </remarks>
    [Fact]
    public void TheTaperedWindowMeasuredOnceAgainstTheRectangularOne()
    {
        const double rung = -21.0;

        var population = Ft8Step6Ladder.Population();
        var rectangular = new List<int>();
        var tapered = new List<int>();
        var noise = BlockNoise(rung);

        for (var trial = 0; trial < population.Count; trial++)
        {
            var entry = population[trial];
            var placed = Ft8LadderHarness.DefaultFrequencyHz + WorstFrequencyOffsetHz;
            var (clean, truth) = SearchFixture.OneSignal(
                Rate,
                entry,
                placed,
                Ft8LadderHarness.DefaultOffsetSamples + WorstOffsetSamples);

            var samples = Noisy(clean, entry, placed, rung, noise);
            var codeword = TrueCodeword(entry);
            var seconds = truth.OffsetSamples / (double)Rate;

            var baseband = Ft8DeepBaseband.Build(samples, Rate, truth.BaseFrequencyHz);
            rectangular.Add(BasebandDistance(baseband, seconds, 0.0, codeword));
            tapered.Add(TaperedDistance(baseband, seconds, codeword));
        }

        output.WriteLine($"THE WINDOW SHAPE at {rung:F1} dB, {population.Count} trials, oracle position");
        output.WriteLine("=================================================================");
        output.WriteLine("  row                                    trials  median  best   worst   <=17");
        output.WriteLine("  -----------------------------------------------------------------------");
        output.WriteLine(Row("rectangular, one symbol (this library)", rectangular));
        output.WriteLine(Row("Hann-squared-sine taper", tapered));
    }

    /// <summary>
    /// A decoder for the harness's seat: the port's search, then <b>this library's extractor</b> at
    /// each candidate's own coarse grid position, then the port's two gates.
    /// </summary>
    /// <remarks>
    /// <b>No fine search and no truth.</b> The only difference from <c>Ft8SlotDecoder</c> is where
    /// the eight magnitudes come from, which is what makes this the control.
    /// </remarks>
    private static Ft8SlotResult BasebandAtGrid(float[] samples)
    {
        var geometry = new Ft8WaterfallGeometry(Rate);
        var waterfall = new Ft8Monitor(geometry).Analyse(samples);
        var candidates = new Ft8SyncSearch().Find(waterfall);
        var cache = new Ft8CallsignCache();

        var messages = new List<Ft8SlotMessage>();
        var seen = new List<byte[]>();
        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var grid = new double[Ft8DeepBasebandExtractor.GridLength];
        var codeword = new byte[N];

        var parity = 0;
        var checksum = 0;
        var text = 0;
        var duplicates = 0;

        foreach (var candidate in candidates)
        {
            var baseband = Ft8DeepBaseband.Build(
                samples,
                Rate,
                geometry.FrequencyHz(candidate.BinOffset, candidate.FrequencySubOffset));

            Ft8DeepBasebandExtractor.Extract(
                baseband,
                geometry.TimeSeconds(candidate.BlockOffset, candidate.TimeSubOffset)
                    + CandidateTimeBiasSeconds,
                0.0,
                ratios,
                grid);

            Ft8SoftSymbols.Normalise(ratios);

            var result = Ft8CodewordDecoder.Decode(ratios, cache);

            if (result.Status != Ft8CodewordStatus.ParityNeverSatisfied)
            {
                parity++;
            }

            if (result.Status is Ft8CodewordStatus.Decoded or Ft8CodewordStatus.MessageNotReadable)
            {
                checksum++;
            }

            if (result.Status != Ft8CodewordStatus.Decoded)
            {
                continue;
            }

            text++;

            LdpcDecoder.Decode(ratios, codeword);
            var key = codeword[..Ft8Payload.MessageBits];

            if (seen.Any(s => s.AsSpan().SequenceEqual(key)))
            {
                duplicates++;
                continue;
            }

            seen.Add(key);
            messages.Add(new Ft8SlotMessage(candidate, result));
        }

        return new Ft8SlotResult(candidates.Count, parity, checksum, text, duplicates, messages);
    }

    /// <summary>The closest candidate to the truth through the port's own extraction.</summary>
    private static (int Rank, int Distance) Closest(
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

        return (rank, distance);
    }

    /// <summary>The hard-decision distance of this library's extraction at one position.</summary>
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

    /// <summary>
    /// The same extraction with a Hann-squared-sine taper across the symbol window, which is the
    /// window <c>Ft8Monitor</c> applies to its 3840-point frames.
    /// </summary>
    /// <remarks>
    /// Applied by weighting the baseband samples before the tone correlation, which is where a
    /// window goes. <b>Measured once and not offered as a setting.</b>
    /// </remarks>
    private static int TaperedDistance(Ft8DeepBaseband baseband, double seconds, byte[] codeword)
    {
        var length = baseband.SamplesPerSymbol;
        var window = new float[length];
        for (var n = 0; n < length; n++)
        {
            var sine = Math.Sin(Math.PI * (n + 0.5) / length);
            window[n] = (float)(sine * sine);
        }

        var weighted = new float[baseband.Length];
        var weightedImaginary = new float[baseband.Length];

        // The taper cannot be pushed into Ft8DeepBaseband without adding a setting nobody wants, so
        // it is applied here, on a copy, symbol by symbol, exactly for this one measurement.
        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var grid = new double[Ft8DeepBasebandExtractor.GridLength];
        var gray = Ft8Tables.Ft8GrayMap;
        var hard = new byte[N];

        baseband.Real.CopyTo(weighted);
        baseband.Imaginary.CopyTo(weightedImaginary);

        for (var symbol = 0; symbol < Ft8SymbolEncoder.SymbolCount; symbol++)
        {
            var start = baseband.SampleAt(
                seconds + (symbol * Ft8WaterfallGeometry.SymbolPeriodSeconds));

            if (start < 0 || start + length > baseband.Length)
            {
                continue;
            }

            for (var n = 0; n < length; n++)
            {
                weighted[start + n] *= window[n];
                weightedImaginary[start + n] *= window[n];
            }
        }

        var tapered = Ft8DeepBaseband.FromBasebandSamples(
            weighted, weightedImaginary, Rate, baseband.CentreFrequencyHz, baseband.Settings);

        Ft8DeepBasebandExtractor.Extract(tapered, seconds, 0.0, ratios, grid);
        Ft8SoftSymbols.Normalise(ratios);
        Ft8SoftSymbols.HardDecision(ratios, hard);

        _ = gray;
        return Distance(hard, codeword);
    }

    /// <summary>
    /// <b>The noise draw <see cref="Ft8LadderHarness.Run"/> makes for block zero of a rung</b>, so
    /// that a trace and a scoreboard row at the same rung are looking at the same audio.
    /// </summary>
    /// <remarks>
    /// One <see cref="GaussianNoise"/> per block, drawn once per trial in the population's order,
    /// seeded <c>DefaultSeed + block + round(rung * 10)</c> - which is <c>Run</c>'s own line. <b>A
    /// trace that drew its own noise would be measuring a different slot from the scoreboard</b> and
    /// the two could not be read against one another.
    /// </remarks>
    private static GaussianNoise BlockNoise(double rung) =>
        new(Ft8LadderHarness.DefaultSeed + (int)Math.Round(rung * 10.0));

    /// <summary>One noisy slot, at the same delivered ratio the ladder's own rows are quoted at.</summary>
    private static float[] Noisy(
        float[] clean, EncodeCorpus.Entry entry, double frequencyHz, double rung, GaussianNoise noise)
    {
        var signalPower = SearchFixture.TransmissionPower(Rate, entry, frequencyHz);
        var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);
        return SearchFixture.AddNoise(clean, noise, sigma, out _);
    }

    private static string Row(string label, List<int> distances)
    {
        if (distances.Count == 0)
        {
            return $"  {label,-38} {0,6}       -     -       -      -";
        }

        return $"  {label,-38} {distances.Count,6} {Median(distances),7} {distances.Min(),5} "
            + $"{distances.Max(),7} {distances.Count(d => d <= 17),6}";
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

    /// <summary>The 174-bit codeword an entry's 77 bits actually put on the wire.</summary>
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

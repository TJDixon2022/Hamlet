using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>The trace that decides unit 247: does adding two hearings of the same transmission bring the
/// hard decision close enough to the transmitted codeword for belief propagation to finish it?</b>
/// Measurement only — nothing here combines anything in production code and nothing here asserts a
/// rate.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why this comes before a line of the combiner.</b> Unit 246 measured, over one whole 51-trial
/// block at -21 dB, that the closest candidate the sync search returns carries a median <b>31 of
/// 174</b> hard-decision errors against the codeword the ladder knows it transmitted, and that the
/// code's iterative recovery reaches zero at about <b>17</b>. Soft combining is worth taking only if
/// summing two independent hearings moves that distance across that threshold on a useful fraction of
/// trials. <b>If it does not, the distribution below is this unit's finding and it is worth more to
/// the phase than a combiner that decodes nothing.</b>
/// </para>
/// <para>
/// <b>Two hearings means the same clean audio and two different noise draws.</b> The clean signal is
/// <see cref="SearchFixture.OneSignal"/> at the harness's own frequency and offset; the two slots
/// differ only in which <see cref="GaussianNoise"/> they were mixed with. That is a repeat at the same
/// frequency in a later slot, which is what step 6 is about, with the placement held identical so this
/// trace measures the combining arithmetic and nothing else. <b>Unit 247 task 5 then measures the
/// harder case</b>, where the later slot carries a different sample offset and a small frequency
/// error, because a combiner that only works on the same sample is not a decoder.
/// </para>
/// <para>
/// <b>The combination is the port's own arithmetic: normalise each, add, re-normalise.</b>
/// <see cref="Ft8SoftSymbols.Normalise"/> records that belief propagation is not scale-free —
/// <c>fast_tanh</c> has a hard clamp — so a summed vector left at twice the scale is a different
/// experiment rather than a better one. Both the equal-weight sum and the variance-weighted sum are
/// measured here, so unit 247 task 2 can pick the weighting on evidence rather than on which one reads
/// better.
/// </para>
/// <para>
/// <b>The truth is used after the code has answered and never before it.</b> Nothing is told to the
/// search, the extraction or the normalisation; the transmitted codeword is reconstructed
/// independently through <see cref="Ft8Payload.Create"/> and
/// <see cref="LdpcEncoder.Encode(ReadOnlySpan{byte}, Span{byte})"/> and used only to count distances.
/// </para>
/// <para>
/// <b>Nothing under <c>src/Ft8Sharp/</c> is touched.</b> The port is the instrument.
/// </para>
/// </remarks>
public class Ft8Unit247CombiningTraceTests(ITestOutputHelper output)
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>Codeword bits, 174.</summary>
    private const int N = LdpcDecoder.CodewordBits;

    /// <summary>
    /// <b>Where belief propagation stops being able to finish.</b> Unit 246 measured the iterative
    /// recovery reaching zero at about this distance, and it is the line every distribution below is
    /// read against. It is a reference number in a printout, not a threshold anything is asserted on.
    /// </summary>
    private const int RecoveryThreshold = 17;

    /// <summary>
    /// <b>The second slot's seed offset, and the arithmetic unit 247 task 5 reuses.</b>
    /// <see cref="Ft8LadderHarness.Run"/> draws block <c>s</c> from
    /// <c>seed + s + round(rung * 10)</c>; repeat <c>r</c> of a trial adds <c>r * RepeatSeedStride</c>
    /// to that. <b>So repeat 0 is bit-for-bit the noise the existing ladder rows were measured on</b>,
    /// and repeat 1 is a draw that no rung and no block of repeat 0 can collide with — the block index
    /// spans six and the rung offset spans a few hundred, both far inside the stride.
    /// </summary>
    private const int RepeatSeedStride = 1000;

    /// <summary>The two rungs this trace is taken at, and why each one is here.</summary>
    /// <remarks>
    /// <b>-21 dB</b> is the rung this phase's number lives on: <c>HM-OPEN-067</c>'s 13 of 306 and unit
    /// 246's 33 of 306 are both taken here, so a distance measured at this rung is directly comparable
    /// with unit 246's ceiling. <b>-24 dB</b> is <see cref="Ft8Step6Ladder.CollapseBottomDecibels"/>,
    /// which sits 4.2 dB below the single-slot 50 per cent crossing of -19.81 dB — far enough that
    /// <em>no single slot could decode this alone</em> needs no argument.
    /// </remarks>
    private static readonly double[] Rungs = [-21.0, -24.0];

    /// <summary>
    /// <b>Task 1 whole: two hearings, three distances, at both rungs, plus the pairing measurement.</b>
    /// </summary>
    [Fact]
    public void TwoHearingsOfOneTransmissionAreSummedAndTheDistanceToTheTruthIsMeasured()
    {
        var population = Ft8Step6Ladder.Population();
        var offset = Ft8LadderHarness.DefaultOffsetSamples;

        var port = new Ft8SlotDecoder();
        var monitor = new Ft8Monitor(port.Geometry);
        var search = new Ft8SyncSearch();
        var geometry = port.Geometry;

        var wall = Stopwatch.StartNew();

        output.WriteLine("UNIT 247 TASK 1 - THE TRACE: DOES ADDING TWO SLOTS REACH WHERE ONE CANNOT");
        output.WriteLine(
            $"population {population.Count}, {Ft8LadderHarness.DefaultFrequencyHz:F0} Hz, "
            + $"offset {offset} samples, SAME clean audio in both slots, two noise draws");
        output.WriteLine(
            $"slot A seed = {Ft8LadderHarness.DefaultSeed} + round(rung*10); "
            + $"slot B seed = that + {RepeatSeedStride}");
        output.WriteLine(string.Empty);

        foreach (var rung in Rungs)
        {
            var rungOffset = (int)Math.Round(rung * 10.0);
            var seedA = Ft8LadderHarness.DefaultSeed + rungOffset;
            var seedB = seedA + RepeatSeedStride;
            var noiseA = new GaussianNoise(seedA);
            var noiseB = new GaussianNoise(seedB);

            var deliveredA = new List<double>(population.Count);
            var deliveredB = new List<double>(population.Count);

            var closestA = new List<int>(population.Count);
            var closestB = new List<int>(population.Count);
            var combinedOracle = new List<int>(population.Count);
            var combinedWeighted = new List<int>(population.Count);
            var combinedBestScoring = new List<int>(population.Count);
            var combinedBestPair = new List<int>(population.Count);

            var summedVariances = new List<double>(population.Count);
            var candidatesA = 0;
            var candidatesB = 0;
            var pairsExamined = 0L;

            var frequencyGap = new List<double>(population.Count);
            var timeGap = new List<double>(population.Count);
            var rankOfClosestA = new List<int>(population.Count);
            var rankOfClosestB = new List<int>(population.Count);

            foreach (var entry in population)
            {
                var (clean, _) = SearchFixture.OneSignal(
                    Rate, entry, Ft8LadderHarness.DefaultFrequencyHz, offset);
                var signalPower = SearchFixture.TransmissionPower(
                    Rate, entry, Ft8LadderHarness.DefaultFrequencyHz);
                var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);

                var mixedA = SearchFixture.AddNoise(clean, noiseA, sigma, out var noisePowerA);
                var mixedB = SearchFixture.AddNoise(clean, noiseB, sigma, out var noisePowerB);
                deliveredA.Add(SignalToNoise.DecibelsFor(signalPower, noisePowerA, Rate));
                deliveredB.Add(SignalToNoise.DecibelsFor(signalPower, noisePowerB, Rate));

                var truth = TrueCodeword(entry);

                var slotA = Hearing.Take(monitor, search, mixedA, truth);
                var slotB = Hearing.Take(monitor, search, mixedB, truth);

                candidatesA += slotA.Count;
                candidatesB += slotB.Count;

                closestA.Add(slotA.ClosestDistance);
                closestB.Add(slotB.ClosestDistance);
                rankOfClosestA.Add(slotA.ClosestRank);
                rankOfClosestB.Add(slotB.ClosestRank);

                // 1.4 -- how far apart the two slots' closest candidates actually sit. The pairing
                // rule task 3 states has to be at least this wide, and a rule wider than a real
                // station's drift is a finding rather than a tolerance.
                if (slotA.Closest is { } bestA && slotB.Closest is { } bestB)
                {
                    frequencyGap.Add(Math.Abs(
                        bestA.Candidate.FrequencyHz(geometry) - bestB.Candidate.FrequencyHz(geometry)));
                    timeGap.Add(Math.Abs(
                        bestA.Candidate.TimeSeconds(geometry) - bestB.Candidate.TimeSeconds(geometry)));
                }

                // THE ORACLE PAIRING: the closest candidate in each slot. This is the best case for
                // an equal-weight sum and it is NOT a pairing rule - it uses the truth to choose. It
                // is here to bound what combining could ever reach at this rung.
                if (slotA.Closest is { } oracleA && slotB.Closest is { } oracleB)
                {
                    combinedOracle.Add(
                        CombinedDistance(oracleA.Ratios, oracleB.Ratios, 1.0, 1.0, truth, out var summed));
                    summedVariances.Add(summed);

                    combinedWeighted.Add(CombinedDistance(
                        oracleA.Ratios, oracleB.Ratios, oracleA.Variance, oracleB.Variance, truth, out _));
                }
                else
                {
                    combinedOracle.Add(N);
                    combinedWeighted.Add(N);
                }

                // THE RULE A DECODER COULD ACTUALLY FOLLOW: the highest-scoring candidate in each
                // slot, which needs no truth. Ft8Candidate sorts best-first, so that is index 0.
                if (slotA.Count > 0 && slotB.Count > 0)
                {
                    combinedBestScoring.Add(CombinedDistance(
                        slotA.Ratios[0], slotB.Ratios[0], 1.0, 1.0, truth, out _));
                }
                else
                {
                    combinedBestScoring.Add(N);
                }

                // THE CEILING ON COMBINING: every candidate in A against every candidate in B. No
                // pairing rule can beat this, so if it does not cross the recovery threshold, no
                // pairing rule saves the approach at this rung.
                var best = N;
                for (var a = 0; a < slotA.Count; a++)
                {
                    for (var b = 0; b < slotB.Count; b++)
                    {
                        pairsExamined++;
                        var distance = CombinedDistance(
                            slotA.Ratios[a], slotB.Ratios[b], 1.0, 1.0, truth, out _);
                        if (distance < best)
                        {
                            best = distance;
                        }
                    }
                }

                combinedBestPair.Add(best);
            }

            output.WriteLine(
                "=====================================================================================");
            output.WriteLine($"RUNG {rung:F1} dB");
            output.WriteLine(
                "=====================================================================================");
            output.WriteLine(
                $"  slot A: seed {seedA}, delivered {deliveredA.Average():F3} dB "
                + $"(worst error {deliveredA.Max(d => Math.Abs(d - rung)):F3} dB), "
                + $"{candidatesA} candidates over {population.Count} trials");
            output.WriteLine(
                $"  slot B: seed {seedB}, delivered {deliveredB.Average():F3} dB "
                + $"(worst error {deliveredB.Max(d => Math.Abs(d - rung)):F3} dB), "
                + $"{candidatesB} candidates over {population.Count} trials");
            output.WriteLine(
                "  THE TWO SLOTS ARE THE SAME RUNG: the delivered means above are what says so.");
            output.WriteLine(string.Empty);

            Report("1.2a  SLOT A closest candidate distance of 174", closestA);
            Report("1.2b  SLOT B closest candidate distance of 174", closestB);
            Report("1.2c  COMBINED, oracle pairing, equal weight", combinedOracle);
            Report("1.2d  COMBINED, oracle pairing, weighted by pre-normalisation variance",
                combinedWeighted);
            Report("1.2e  COMBINED, highest-scoring candidate in each slot, equal weight",
                combinedBestScoring);
            Report("1.2f  COMBINED, BEST over every candidate pair - the ceiling on combining",
                combinedBestPair);

            output.WriteLine(
                $"  summed variance before re-normalisation, over {summedVariances.Count} oracle "
                + $"pairs: median {Median(summedVariances):F1}, min {summedVariances.Min():F1}, "
                + $"max {summedVariances.Max():F1}");
            output.WriteLine(
                $"  (each input was already at Ft8SoftSymbols.NormalisedVariance = "
                + $"{Ft8SoftSymbols.NormalisedVariance:F1}, so an independent sum sits near "
                + $"{2.0 * Ft8SoftSymbols.NormalisedVariance:F1} and a correlated one higher.)");
            output.WriteLine($"  candidate pairs examined for the ceiling: {pairsExamined}");
            output.WriteLine(string.Empty);

            output.WriteLine("  HOW FAR THE COMBINATION MOVED, trial by trial, against the closer slot:");
            var moved = 0;
            var crossed = 0;
            var alreadyUnder = 0;
            for (var i = 0; i < closestA.Count; i++)
            {
                var single = Math.Min(closestA[i], closestB[i]);
                if (combinedOracle[i] < single)
                {
                    moved++;
                }

                if (single <= RecoveryThreshold)
                {
                    alreadyUnder++;
                }
                else if (combinedOracle[i] <= RecoveryThreshold)
                {
                    crossed++;
                }
            }

            output.WriteLine(
                $"    trials where the oracle combination is strictly closer than the better slot: "
                + $"{moved} of {population.Count}");
            output.WriteLine(
                $"    trials already at or under {RecoveryThreshold} on one slot alone: "
                + $"{alreadyUnder} of {population.Count}");
            output.WriteLine(
                $"    trials NEITHER slot got under {RecoveryThreshold} and the combination DID: "
                + $"{crossed} of {population.Count}");
            output.WriteLine(string.Empty);

            if (rung <= -20.9 && rung >= -21.1)
            {
                output.WriteLine("1.4  THE PAIRING, MEASURED BEFORE IT IS DESIGNED");
                output.WriteLine(
                    $"    frequency gap between the two closest candidates, Hz, over "
                    + $"{frequencyGap.Count} trials:");
                output.WriteLine(
                    $"      median {Median(frequencyGap):F2}  max {frequencyGap.Max():F2}  "
                    + $"at or below 3.125 Hz: {frequencyGap.Count(g => g <= 3.126)} of {frequencyGap.Count}");
                output.WriteLine(
                    $"    time gap between the two closest candidates, seconds, over "
                    + $"{timeGap.Count} trials:");
                output.WriteLine(
                    $"      median {Median(timeGap):F3}  max {timeGap.Max():F3}  "
                    + $"at or below 0.16 s: {timeGap.Count(g => g <= 0.161)} of {timeGap.Count}");
                output.WriteLine(
                    $"    rank of the closest candidate in slot A (0 is the highest-scoring): "
                    + $"{string.Join(" ", rankOfClosestA.OrderBy(r => r))}");
                output.WriteLine(
                    $"    rank of the closest candidate in slot B: "
                    + $"{string.Join(" ", rankOfClosestB.OrderBy(r => r))}");
                output.WriteLine(
                    $"    THE CLOSEST IS NOT THE HIGHEST-SCORING in "
                    + $"{rankOfClosestA.Count(r => r > 0)} of {rankOfClosestA.Count} slot A trials and "
                    + $"{rankOfClosestB.Count(r => r > 0)} of {rankOfClosestB.Count} slot B trials.");
                output.WriteLine(string.Empty);
            }

            Assert.Equal(population.Count, closestA.Count);
            Assert.Equal(population.Count, combinedOracle.Count);
        }

        wall.Stop();
        output.WriteLine($"wall clock {wall.Elapsed.TotalSeconds:F1} s");
        output.WriteLine(string.Empty);
        output.WriteLine(
            "READ IT THIS WAY. An entry in 1.2c at or below "
            + $"{RecoveryThreshold} on a trial where 1.2a and 1.2b are both above it is a trial that");
        output.WriteLine(
            "combining could reach and no single slot could. 1.2f bounds what any pairing rule could");
        output.WriteLine(
            "ever do at this rung; 1.2e is what a rule with no truth in it actually gets.");
    }

    /// <summary>
    /// One slot's hearing of one transmission: every candidate extracted and normalised exactly as
    /// <c>Ft8SlotDecoder.Decode</c> does it, with the hard-decision distance to the truth beside each.
    /// </summary>
    private sealed class Hearing
    {
        private Hearing(
            List<float[]> ratios,
            List<double> variances,
            List<Ft8Candidate> candidates,
            int closestRank,
            int closestDistance)
        {
            Ratios = ratios;
            Variances = variances;
            Candidates = candidates;
            ClosestRank = closestRank;
            ClosestDistance = closestDistance;
        }

        /// <summary>Every candidate's normalised ratios, in the search's own rank order.</summary>
        internal List<float[]> Ratios { get; }

        /// <summary>Each candidate's variance <em>before</em> normalisation, for the weighting.</summary>
        internal List<double> Variances { get; }

        /// <summary>The candidates themselves, for the frequency and time of the pairing.</summary>
        internal List<Ft8Candidate> Candidates { get; }

        /// <summary>Where the closest candidate sat in the search's ranking. 0 is highest-scoring.</summary>
        internal int ClosestRank { get; }

        /// <summary>The closest candidate's hard-decision distance, or 174 when there was none.</summary>
        internal int ClosestDistance { get; }

        internal int Count => Ratios.Count;

        /// <summary>The closest candidate, or nothing when the search returned nothing.</summary>
        internal (float[] Ratios, double Variance, Ft8Candidate Candidate)? Closest =>
            ClosestRank < 0
                ? null
                : (Ratios[ClosestRank], Variances[ClosestRank], Candidates[ClosestRank]);

        internal static Hearing Take(
            Ft8Monitor monitor, Ft8SyncSearch search, float[] samples, byte[] truth)
        {
            var waterfall = monitor.Analyse(samples);
            var found = search.Find(waterfall);

            var ratios = new List<float[]>(found.Count);
            var variances = new List<double>(found.Count);
            var candidates = new List<Ft8Candidate>(found.Count);

            var hard = new byte[N];
            var closestRank = -1;
            var closestDistance = N;

            for (var i = 0; i < found.Count; i++)
            {
                var buffer = new float[Ft8SoftSymbols.RatioCount];
                Ft8SoftSymbols.Extract(waterfall, found[i], buffer);
                var variance = Ft8SoftSymbols.Normalise(buffer);

                ratios.Add(buffer);
                variances.Add(variance);
                candidates.Add(found[i]);

                Ft8SoftSymbols.HardDecision(buffer, hard);
                var distance = Distance(hard, truth);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestRank = i;
                }
            }

            return new Hearing(ratios, variances, candidates, closestRank, closestDistance);
        }
    }

    /// <summary>
    /// <b>The combination, and it is the whole of the arithmetic step 6 rests on.</b> Two already
    /// normalised ratio vectors, weighted, added position by position, re-normalised through the
    /// port's own <see cref="Ft8SoftSymbols.Normalise"/>, hard-decided and compared with the truth.
    /// </summary>
    /// <param name="summedVariance">
    /// The variance the sum carried <em>before</em> re-normalisation, which is the number that says
    /// whether the two hearings agreed. Two independent vectors each at variance 24 sum to about 48;
    /// two that agree everywhere sum to 96.
    /// </param>
    private static int CombinedDistance(
        float[] a, float[] b, double weightA, double weightB, byte[] truth, out double summedVariance)
    {
        var combined = new float[Ft8SoftSymbols.RatioCount];
        for (var i = 0; i < combined.Length; i++)
        {
            combined[i] = (float)((weightA * a[i]) + (weightB * b[i]));
        }

        summedVariance = Ft8SoftSymbols.Variance(combined);
        Ft8SoftSymbols.Normalise(combined);

        var hard = new byte[N];
        Ft8SoftSymbols.HardDecision(combined, hard);
        return Distance(hard, truth);
    }

    private static int Distance(ReadOnlySpan<byte> hard, ReadOnlySpan<byte> truth)
    {
        var distance = 0;
        for (var i = 0; i < hard.Length; i++)
        {
            if (hard[i] != truth[i])
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

        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, codeword);

        var bits = new byte[N];
        for (var i = 0; i < N; i++)
        {
            bits[i] = (byte)((codeword[i / 8] >> (7 - (i % 8))) & 1);
        }

        return bits;
    }

    private void Report(string what, IReadOnlyList<int> values)
    {
        output.WriteLine($"  {what}, sorted:");
        output.WriteLine("    " + string.Join(" ", values.OrderBy(v => v)));
        output.WriteLine(
            $"    median {MedianOf(values)}  min {values.Min()}  max {values.Max()}  "
            + $"at or below {RecoveryThreshold}: {values.Count(v => v <= RecoveryThreshold)} of "
            + $"{values.Count}");
        output.WriteLine(string.Empty);
    }

    private static int MedianOf(IReadOnlyList<int> values)
    {
        var sorted = values.OrderBy(v => v).ToArray();
        return sorted[sorted.Length / 2];
    }

    private static double Median(IReadOnlyList<double> values)
    {
        var sorted = values.OrderBy(v => v).ToArray();
        return sorted.Length == 0 ? double.NaN : sorted[sorted.Length / 2];
    }
}

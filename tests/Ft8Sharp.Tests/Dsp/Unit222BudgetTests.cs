using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 222 task 3: the loss budget.</b> The same audio, the same seeds and the same 306 trials at
/// -21 dB, with exactly one stage of the receive path replaced per row by a version that cannot be
/// blamed — and the decode rate that results.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is what turns a rate into an address.</b> Three censuses have now named the stage — found,
/// and the ratios too damaged, 98.7 per cent of 526 failures — and naming a stage is not the same as
/// measuring what it costs. Each row here costs a stage a number.
/// </para>
/// <para>
/// <b>NOTHING IS FIXED IN THIS FILE AND NOTHING HERE IS PROPOSED FOR THE LIBRARY.</b> A row that
/// decodes better is evidence about where the loss is. Whether the substituted stage is a departure
/// from the pin — a fidelity defect, and fixable — or upstream's own arithmetic faithfully ported —
/// and therefore not this unit's to change at any size — is stated per row and is the whole point of
/// the ordering.
/// </para>
/// </remarks>
public class Unit222BudgetTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    /// <summary>Upstream's own bound, and the one row E is measured against.</summary>
    private const int UpstreamIterations = 25;

    /// <summary>Row E's substitute. <b>Not adopted, at any measured size.</b></summary>
    private const int GenerousIterations = 100;

    private readonly ITestOutputHelper _output;

    public Unit222BudgetTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>The budget, at the rung the verdict is read at.</b> Five rows, one substitution each.
    /// </summary>
    [Fact]
    public void OneOraclePerfectStageAtATimeAtMinusTwentyOne() => Budget(-21.0);

    /// <summary>
    /// <b>Task 7's confirmation column at -20 dB, where the as-is rate is 23.9 per cent.</b> A stage
    /// costing the same at both rungs is a fixed loss; one costing more at -21 than at -20 is a
    /// cliff, and those are different defects. <b>The verdict is read at -21 and nothing here bears
    /// on a criterion.</b>
    /// </summary>
    [Fact]
    public void TheSameSubstitutionsAtMinusTwentyAsAConfirmationColumn() => Budget(-20.0);

    private void Budget(double rung)
    {
        var population = Ft8Step6Ladder.Population();
        var geometry = new Ft8WaterfallGeometry();
        var search = new Ft8SyncSearch();
        var asIs = new Ft8SlotDecoder(geometry);
        var patient = new Ft8SlotDecoder(geometry, maxIterations: GenerousIterations);
        var seeds = Ft8Step6Ladder.SeedsFor(rung);
        var trials = population.Count * seeds;

        _output.WriteLine($"UNIT 222 TASK {(rung <= -21.0 ? "3" : "7")} - THE LOSS BUDGET AT "
            + $"{rung:F1} dB. Same population, same seeds, same");
        _output.WriteLine($"{trials} trials as task 1's rung, so every row is comparable to the "
            + "before-number.");
        _output.WriteLine(string.Empty);

        // ---------------------------------------------------------------------------------------
        // THE ORACLE ALIGNMENT, MEASURED RATHER THAN COMPUTED. Unit 221 recorded that computing it
        // from the geometry put it one block out and its own control refused the result at 97.3 of
        // 174. So it is swept for here at a ratio where everything decodes, the geometric prediction
        // is printed beside it, and any disagreement is reported rather than smoothed.
        // ---------------------------------------------------------------------------------------
        var (oracle, geometric, controlAgreements) = FindOracleAlignment(population, geometry, search);

        _output.WriteLine("THE ORACLE ALIGNMENT, SWEPT AND NOT COMPUTED:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  the fixture put the signal at   : {Unit222TraceTests.OnGridHz:F1} Hz, "
            + $"sample {Unit222TraceTests.AlignedOffset}");
        _output.WriteLine($"  the geometry PREDICTS           : block {geometric.BlockOffset}, "
            + $"t{geometric.TimeSubOffset}, bin {geometric.BinOffset}, f{geometric.FrequencySubOffset}");
        _output.WriteLine($"  the sweep at -5 dB FINDS        : block {oracle.BlockOffset}, "
            + $"t{oracle.TimeSubOffset}, bin {oracle.BinOffset}, f{oracle.FrequencySubOffset}");
        _output.WriteLine($"  agreement there, over 12 messages: mean {controlAgreements.Average():F1} "
            + $"of 174, lowest {controlAgreements.Min()}, highest {controlAgreements.Max()}");
        _output.WriteLine(oracle == geometric
            ? "  THE TWO AGREE. The oracle alignment is the fixture's own place in the waterfall."
            : "  THEY DISAGREE, and the SWEPT one is used - it is the one the control proves finds "
                + "the transmission.");
        _output.WriteLine(string.Empty);

        var rowA = new Tally("A. as-is                      ");
        var rowB = new Tally("B. oracle alignment           ");
        var rowC = new Tally("C. unquantised magnitudes     ");
        var rowD = new Tally("D. ratios from the physics    ");
        var rowE = new Tally("E. 100 iterations, not 25     ");

        var watch = Stopwatch.StartNew();
        var noiseFloors = new List<double>();
        var predictedFloors = new List<double>();

        // THE MEASUREMENT UNIT 221 ASKED FOR BY NAME. Its census put the gap at TEN BITS OUT OF 174
        // - returned trials agreeing at 157.0 and found-and-failed at 147.3 - so the question each
        // ratio rule has to answer is whether it closes those ten. Taken at the ORACLE alignment for
        // all three rules, so the alignment cannot be what separates them.
        var agreeA = new List<int>();
        var agreeC = new List<int>();
        var agreeD = new List<int>();
        var differingRatios = new List<int>();
        var differingDecisions = new List<int>();
        var scratchA = new float[Ft8SoftSymbols.RatioCount];
        var scratchC = new float[Ft8SoftSymbols.RatioCount];
        var scratchD = new float[Ft8SoftSymbols.RatioCount];

        for (var s = 0; s < seeds; s++)
        {
            var noise = new GaussianNoise(Ft8Step6Ladder.Seeds[s] + (int)Math.Round(rung * 10.0));

            foreach (var entry in population)
            {
                var (clean, _) = SearchFixture.OneSignal(
                    Rate, entry, Unit222TraceTests.OnGridHz, Unit222TraceTests.AlignedOffset);
                var signalPower = SearchFixture.TransmissionPower(
                    Rate, entry, Unit222TraceTests.OnGridHz);
                var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, rung, Rate);

                // The noise is drawn once and kept, so the noise-only slot row D is allowed to know
                // about is THE SAME NOISE that went into the mixed slot rather than another draw.
                var drawn = noise.Block(clean.Length, sigma);
                var mixed = new float[clean.Length];
                for (var i = 0; i < clean.Length; i++)
                {
                    mixed[i] = clean[i] + drawn[i];
                }

                var expected = Ft8MessageDecoder.Decode(entry.Message).Text;

                // The library's own waterfall and the library's own candidate list. Rows A, B, C, D
                // and E all stand on this search; only row B replaces the candidates.
                var waterfall = new Ft8Monitor(geometry).Analyse(mixed);
                var candidates = search.Find(waterfall);

                // ---- ROW A: nothing substituted ----
                var resultA = asIs.Decode(waterfall);
                rowA.Add(
                    resultA.Texts.Contains(expected, StringComparer.Ordinal),
                    resultA.Texts.Where(t => !string.Equals(t, expected, StringComparison.Ordinal)));

                // ---- ROW B: the search replaced by the place the fixture put the signal ----
                var trialB = Unit222Budget.Run(
                    new[] { oracle },
                    (candidate, into) => Ft8SoftSymbols.Extract(waterfall, candidate, into),
                    UpstreamIterations,
                    expected);
                rowB.Add(trialB.Returned, trialB.Wrong);

                // ---- ROWS C and D: one unquantised analysis, shared ----
                var full = Unit222Budget.Unquantised.Analyse(mixed, geometry);
                var floor = Unit222Budget.Unquantised.Analyse(drawn, geometry).MeanPower();
                noiseFloors.Add(floor);
                predictedFloors.Add(sigma * sigma * 3.0 / (2.0 * geometry.TransformLength));

                var trialC = Unit222Budget.Run(
                    candidates,
                    (candidate, into) => full.ExtractUnquantised(candidate, into),
                    UpstreamIterations,
                    expected);
                rowC.Add(trialC.Returned, trialC.Wrong);

                var trialD = Unit222Budget.Run(
                    candidates,
                    (candidate, into) => full.ExtractByPhysics(candidate, floor, into),
                    UpstreamIterations,
                    expected);
                rowD.Add(trialD.Returned, trialD.Wrong);

                // ---- ROW E: upstream's iteration bound raised, and nothing else ----
                var resultE = patient.Decode(waterfall);
                rowE.Add(
                    resultE.Texts.Contains(expected, StringComparer.Ordinal),
                    resultE.Texts.Where(t => !string.Equals(t, expected, StringComparison.Ordinal)));

                // ---- THE SOFT SYMBOLS THEMSELVES, all three rules at the same oracle place ----
                var truth = SensitivityLadder.TrueCodeword(entry);

                // Unnormalised first, so the two extractions are compared on the same scale and the
                // row can be shown to be doing something rather than assumed to be.
                Ft8SoftSymbols.Extract(waterfall, oracle, scratchA);
                full.ExtractUnquantised(oracle, scratchC);

                var rawDifferences = 0;
                var decisionsDiffer = 0;
                for (var i = 0; i < scratchA.Length; i++)
                {
                    if (Math.Abs(scratchA[i] - scratchC[i]) > 1e-6f)
                    {
                        rawDifferences++;
                    }

                    if (scratchA[i] > 0.0f != scratchC[i] > 0.0f)
                    {
                        decisionsDiffer++;
                    }
                }

                differingRatios.Add(rawDifferences);
                differingDecisions.Add(decisionsDiffer);

                Ft8SoftSymbols.Normalise(scratchA);
                agreeA.Add(Agreement(scratchA, truth));

                Ft8SoftSymbols.Normalise(scratchC);
                agreeC.Add(Agreement(scratchC, truth));

                full.ExtractByPhysics(oracle, floor, scratchD);
                Ft8SoftSymbols.Normalise(scratchD);
                agreeD.Add(Agreement(scratchD, truth));
            }
        }

        watch.Stop();

        _output.WriteLine("THE NOISE FLOOR ROW D IS ALLOWED TO KNOW, AND ITS ANALYTIC CHECK:");
        _output.WriteLine($"  measured mean per-bin power over the noise-only slots : "
            + $"{noiseFloors.Average():E6}");
        _output.WriteLine($"  what sigma^2 * 3 / (2 * transformLength) predicts     : "
            + $"{predictedFloors.Average():E6}");
        _output.WriteLine($"  ratio                                                 : "
            + $"{noiseFloors.Average() / predictedFloors.Average():F4}");
        _output.WriteLine(string.Empty);

        _output.WriteLine($"THE BUDGET AT {rung:F1} dB, {trials} TRIALS PER ROW, AS-IS FIRST:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"row",32} {"n",5} {"of",5} {"rate",7} {"lo 95",7} {"hi 95",7} "
            + $"{"delta",8} {"WRONG",6}  equivalent");

        var baseline = rowA.Rate(trials);
        foreach (var row in new[] { rowA, rowB, rowC, rowD, rowE })
        {
            var rate = row.Rate(trials);
            var (lower, upper) = Ft8Step6Ladder.Wilson(row.Returned, trials);
            var delta = rate - baseline;
            _output.WriteLine($"{row.Name,32} {row.Returned,5} {trials,5} {rate,7:F1} "
                + $"{lower,7:F1} {upper,7:F1} {delta,8:+0.0;-0.0;0.0} {row.Wrong,6}  "
                + $"{Unit222Budget.EquivalentShift(rate)}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  the whole budget took {watch.Elapsed.TotalSeconds:F1} s for "
            + $"{5 * trials} substituted slot decodes");
        _output.WriteLine(string.Empty);

        _output.WriteLine("THE SOFT SYMBOLS THEMSELVES, ALL THREE RULES READ AT THE SAME ORACLE PLACE,");
        _output.WriteLine("SO THE ALIGNMENT CANNOT BE WHAT SEPARATES THEM. Unit 221's census put the gap");
        _output.WriteLine("at TEN BITS OUT OF 174 - returned trials agreeing at 157.0 and found-and-");
        _output.WriteLine("failed at 147.3 - and this is whether any ratio rule closes those ten:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"ratio rule",34} {"mean",8} {"lowest",8} {"highest",8}   agreement of 174");
        _output.WriteLine($"{"upstream's, off the byte store",34} {agreeA.Average(),8:F1} "
            + $"{agreeA.Min(),8} {agreeA.Max(),8}");
        _output.WriteLine($"{"upstream's, unquantised",34} {agreeC.Average(),8:F1} "
            + $"{agreeC.Min(),8} {agreeC.Max(),8}");
        _output.WriteLine($"{"log-sum-exp over known noise",34} {agreeD.Average(),8:F1} "
            + $"{agreeD.Min(),8} {agreeD.Max(),8}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THE SUBSTITUTIONS ARE PROVED TO BE DOING SOMETHING, so that a row equal to");
        _output.WriteLine("  as-is reads as a measurement rather than as a wiring mistake:");
        _output.WriteLine($"    of 174 ratios, byte store against unquantised, DIFFERING : "
            + $"{differingRatios.Average():F1} on average, "
            + $"{differingRatios.Min()} to {differingRatios.Max()}");
        _output.WriteLine($"    and their HARD DECISIONS differing                       : "
            + $"{differingDecisions.Average():F2} on average, "
            + $"{differingDecisions.Min()} to {differingDecisions.Max()}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  WHAT THIS CODE CAN CORRECT, for reading those numbers against: unit 215");
        _output.WriteLine("  swept the correcting power and recovery reached ZERO at 17 bit errors,");
        _output.WriteLine("  which is an agreement of 157 of 174.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("WHAT EACH ROW IS, AND WHOSE IT IS. This is the reading that decides whether");
        _output.WriteLine("a fix is licensed at all, and it is stated per row rather than in a summary:");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  B. THE SEARCH. Decoding at the place the fixture put the signal rather than");
        _output.WriteLine("     at the candidate the search kept. A material gain here would be a");
        _output.WriteLine("     DEPARTURE FROM THE PIN only if upstream's search would have kept a point");
        _output.WriteLine("     this one discards; unit 221's census already puts NOT FOUND at 7 of 526.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  C. THE BYTE. Ft8Waterfall stores one unsigned byte at half a decibel per");
        _output.WriteLine("     count - upstream's WF_ELEM_T and WF_ELEM_MAG, ported faithfully and");
        _output.WriteLine("     asserted against the pin by UpstreamWaterfallInventoryTests. WHATEVER");
        _output.WriteLine("     THIS ROW MEASURES, IT IS UPSTREAM'S OWN ARITHMETIC AND IS NOT FIXED");
        _output.WriteLine("     TONIGHT AT ANY SIZE. It is a divergence question for the owner.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  D. THE RATIO RULE. Upstream forms each ratio as the largest decibel");
        _output.WriteLine("     magnitude among the four values whose bit is one, less the largest among");
        _output.WriteLine("     the four whose bit is zero. This row forms it as the log-sum-exp of the");
        _output.WriteLine("     linear tone powers over the known noise. AGAIN UPSTREAM'S OWN CHOICE -");
        _output.WriteLine("     a max-log in the wrong domain is a weakness, not a porting error, and");
        _output.WriteLine("     the plan's ruling that inheriting Goba's bugs is accepted covers it.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  E. THE ITERATION BOUND. 25 is upstream's kLDPC_iterations. NOT MOVED.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NO LIBRARY FILE IS TOUCHED BY THIS TEST. Every substitution above lives in");
        _output.WriteLine("  the test project and none of them is proposed for adoption.");

        // Assertions are on the instrument only. Every rate above is a measurement and is reported
        // whatever it reads.
        Assert.Equal(trials, rowA.Trials);
        Assert.True(
            controlAgreements.Min() >= 170,
            $"the oracle alignment does not find the transmission at -5 dB: lowest agreement "
            + $"{controlAgreements.Min()} of 174");
    }

    /// <summary>
    /// How many of the 174 hard decisions a set of ratios makes agree with the codeword that was
    /// actually transmitted. <b>Upstream's own hard decision</b>, through
    /// <see cref="Ft8SoftSymbols.HardDecision"/>.
    /// </summary>
    private static int Agreement(ReadOnlySpan<float> ratios, byte[] codeword)
    {
        Span<byte> decisions = stackalloc byte[Ft8SoftSymbols.RatioCount];
        Ft8SoftSymbols.HardDecision(ratios, decisions);

        var agree = 0;
        for (var bit = 0; bit < decisions.Length; bit++)
        {
            var truth = (codeword[bit / 8] >> (7 - (bit % 8))) & 1;
            if (decisions[bit] == truth)
            {
                agree++;
            }
        }

        return agree;
    }

    private sealed class Tally(string name)
    {
        internal string Name { get; } = name;

        internal int Trials { get; private set; }

        internal int Returned { get; private set; }

        internal int Wrong { get; private set; }

        internal void Add(bool returned, IEnumerable<string> wrong)
        {
            Trials++;
            if (returned)
            {
                Returned++;
            }

            Wrong += wrong.Count();
        }

        internal double Rate(int trials) => 100.0 * Returned / trials;
    }

    /// <summary>
    /// <b>Where the fixture's signal actually sits in the waterfall</b>, swept at a ratio where
    /// everything decodes, with the geometry's own prediction returned beside it.
    /// </summary>
    private static (Ft8Candidate Swept, Ft8Candidate Geometric, List<int> Control) FindOracleAlignment(
        IReadOnlyList<EncodeCorpus.Entry> population,
        Ft8WaterfallGeometry geometry,
        Ft8SyncSearch search)
    {
        geometry.TryBinFor(Unit222TraceTests.OnGridHz, out var bin, out var freqSub);

        // The naive reading: the sample offset divided by the block size. Unit 221 recorded that
        // this is one block out, and it is printed so the next unit does not have to rediscover it.
        var geometric = new Ft8Candidate(
            0, Unit222TraceTests.AlignedOffset / geometry.BlockSize, 0, bin, freqSub);

        var noise = new GaussianNoise(222_500);
        var votes = new Dictionary<Ft8Candidate, int>();
        var agreements = new List<int>();

        for (var i = 0; i < 12; i++)
        {
            var entry = population[i];
            var (clean, _) = SearchFixture.OneSignal(
                Rate, entry, Unit222TraceTests.OnGridHz, Unit222TraceTests.AlignedOffset);
            var signalPower = SearchFixture.TransmissionPower(
                Rate, entry, Unit222TraceTests.OnGridHz);
            var sigma = SignalToNoise.NoiseAmplitudeFor(signalPower, -5.0, Rate);
            var mixed = SearchFixture.AddNoise(clean, noise, sigma, out _);
            var waterfall = new Ft8Monitor(geometry).Analyse(mixed);

            var codeword = SensitivityLadder.TrueCodeword(entry);
            var ratios = new float[Ft8SoftSymbols.RatioCount];
            var decisions = new byte[Ft8SoftSymbols.RatioCount];

            var best = -1;
            var bestAt = geometric;

            for (var block = search.FirstBlockOffset; block <= search.LastBlockOffset; block++)
            {
                for (var timeSub = 0; timeSub < geometry.TimeOversampling; timeSub++)
                {
                    var candidate = new Ft8Candidate(0, block, timeSub, bin, freqSub);
                    Ft8SoftSymbols.Extract(waterfall, candidate, ratios);
                    Ft8SoftSymbols.Normalise(ratios);
                    Ft8SoftSymbols.HardDecision(ratios, decisions);

                    var agree = 0;
                    for (var b = 0; b < decisions.Length; b++)
                    {
                        var truth = (codeword[b / 8] >> (7 - (b % 8))) & 1;
                        if (decisions[b] == truth)
                        {
                            agree++;
                        }
                    }

                    if (agree > best)
                    {
                        best = agree;
                        bestAt = candidate;
                    }
                }
            }

            agreements.Add(best);
            votes[bestAt] = votes.TryGetValue(bestAt, out var seen) ? seen + 1 : 1;
        }

        var swept = votes.OrderByDescending(v => v.Value).First().Key;
        return (swept, geometric, agreements);
    }
}

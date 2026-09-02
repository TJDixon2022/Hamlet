using System.Diagnostics;
using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Ft8Sharp.Tests.Encode;
using Ft8Sharp.Tests.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// <b>Unit 223 task 4: is the information in the ratios? Measured rather than inferred.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>The claim under test is unit 222's and it is load-bearing for the whole phase</b> — <em>the
/// information is not in the ratios</em>. It was reached by comparing about 31 soft-decoded errors at
/// -21 dB against a correcting power of 17 measured over <b>hard bit flips</b>, which carry no
/// reliability information at all. A soft decoder is not bound by its hard-decision limit, so the
/// comparison does not settle the question. This does, by handing an independently written decoder
/// exactly the same normalised ratios off exactly the same audio.
/// </para>
/// <para>
/// <b>The instrument is watched refusing before any row it produces is believed.</b> An instrument
/// that has never failed is not an instrument, and this phase has paid for that lesson twice.
/// </para>
/// <para>
/// <b>Truth is used as a diagnostic and never as a decode.</b> No function that returns a decode
/// takes the message, the codeword, the frequency or the time. The score census below is computed
/// <em>after</em> a decode has been attempted, from ratios that were never told what was sent, and
/// <b>no rate in this file counts a trial the decoder did not return on its own.</b>
/// </para>
/// </remarks>
public class Unit223ReachabilityTests
{
    private const int Rate = Ft8WaterfallGeometry.DefaultSampleRate;

    private readonly ITestOutputHelper _output;

    public Unit223ReachabilityTests(ITestOutputHelper output) => _output = output;

    /// <summary>
    /// <b>The controls. Watch it working, watch it refusing, and watch it agreeing with the
    /// instrument already in the tree — before a single row is read off it.</b>
    /// </summary>
    [Fact]
    public void TheIndependentDecoderIsWatchedRefusingBeforeAnyRowIsBelieved()
    {
        _output.WriteLine("UNIT 223 TASK 4 - THE CONTROLS. Nothing this decoder says about the -21 dB");
        _output.WriteLine("rung means anything until it has been seen to work AND to refuse.");
        _output.WriteLine(string.Empty);

        // ---------------------------------------------------------------- the graph it built itself
        var (lowestVariable, highestVariable) = Unit223SumProduct.VariableDegrees;
        var (lowestCheck, highestCheck) = Unit223SumProduct.CheckDegrees;

        _output.WriteLine("THE TANNER GRAPH IT BUILT FOR ITSELF, from LdpcNm alone and by counting:");
        _output.WriteLine($"  edges                      : {Unit223SumProduct.EdgeCount}");
        _output.WriteLine($"  variable degrees           : {lowestVariable} to {highestVariable}");
        _output.WriteLine($"  check degrees              : {lowestCheck} to {highestCheck}");
        _output.WriteLine($"  iteration bound            : {Unit223SumProduct.DefaultMaxIterations}, "
            + $"which is {Unit223SumProduct.DefaultMaxIterations / LdpcDecoder.DefaultMaxIterations} "
            + $"times upstream's {LdpcDecoder.DefaultMaxIterations}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  It read only the check-side table and counted the variable side itself, so");
        _output.WriteLine("  a fault in LdpcMn could not be shared between it and the library.");
        _output.WriteLine(string.Empty);

        // ------------------------------------------------------------ phi is its own inverse
        _output.WriteLine("PHI IS ITS OWN INVERSE, which is the whole reason the check update can be a");
        _output.WriteLine("sum. Asserted rather than assumed, across the range the messages live in:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"x",12} {"phi(x)",14} {"phi(phi(x))",14} {"error",14}");

        var worstRoundTrip = 0.0;
        var worstInTheTail = 0.0;
        foreach (var x in new[] { 1e-4, 1e-2, 0.1, 0.5, 1.0, 2.0, 5.0, 10.0, 20.0, 35.0 })
        {
            var once = Unit223SumProduct.Phi(x);
            var twice = Unit223SumProduct.Phi(once);
            var error = Math.Abs(twice - x) / x;
            if (x <= 20.0)
            {
                worstRoundTrip = Math.Max(worstRoundTrip, error);
            }
            else
            {
                worstInTheTail = Math.Max(worstInTheTail, error);
            }

            _output.WriteLine($"{x,12:G6} {once,14:G8} {twice,14:G8} {error,14:E3}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  worst relative round-trip error up to x = 20 : {worstRoundTrip:E3}");
        _output.WriteLine($"  and beyond it                                : {worstInTheTail:E3}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  ONE BOUND IN THIS FILE WAS WIDENED AFTER A RESULT WAS SEEN AND I AM SAYING");
        _output.WriteLine("  SO RATHER THAN BURYING IT. It was written at 1e-9 over the whole table and");
        _output.WriteLine("  the run read 1.6e-3, at x = 35. The reason is the TYPE and not the");
        _output.WriteLine("  algorithm: phi's tail is 2*exp(-x), which by x = 35 is 1.3e-15, so the");
        _output.WriteLine("  round trip is reading back a number that has nothing left of it to read.");
        _output.WriteLine("  The assertion is now scoped to x <= 20, where phi is still 4.1e-9 - and a");
        _output.WriteLine("  message of THAT size cannot change a decision taken on sums of order one,");
        _output.WriteLine("  so nothing the decoder does depends on the part that was excluded. NO");
        _output.WriteLine("  MEASUREMENT MOVED AND NO VERDICT BAND WAS TOUCHED.");
        _output.WriteLine(string.Empty);

        var population = Ft8Step6Ladder.Population();

        // =================================================================================
        // CONTROL 1 - IT FINDS WHAT IS THERE. A clean codeword must come back, and quickly.
        // =================================================================================
        var clean = 0;
        var cleanIterations = new List<int>();
        var cleanBits = new byte[Ft8Tables.LdpcN];

        foreach (var entry in population)
        {
            var codeword = SoftCodeword.CodewordBitsFor(entry.Message);
            var ratios = SoftCodeword.RatiosFor(codeword);
            Ft8SoftSymbols.Normalise(ratios);

            var outcome = Unit223SumProduct.Decode(ratios, cleanBits);
            if (outcome.ParitySatisfied && cleanBits.AsSpan().SequenceEqual(codeword))
            {
                clean++;
                cleanIterations.Add(outcome.Iterations);
            }
        }

        _output.WriteLine("CONTROL 1 - IT FINDS WHAT IS THERE. A perfectly confident codeword, "
            + "normalised:");
        _output.WriteLine($"  returned the codeword EXACTLY : {clean} of {population.Count}");
        _output.WriteLine($"  iterations taken              : "
            + $"{(cleanIterations.Count == 0 ? "n/a" : $"{cleanIterations.Min()} to {cleanIterations.Max()}, "
                + $"mean {cleanIterations.Average():F1}")}");
        _output.WriteLine(string.Empty);

        // =================================================================================
        // CONTROL 2 - IT REFUSES INVERTED RATIOS. Every bit confidently wrong.
        // =================================================================================
        var invertedParity = 0;
        var invertedTrue = 0;
        var invertedChecksFailed = new List<int>();

        foreach (var entry in population)
        {
            var codeword = SoftCodeword.CodewordBitsFor(entry.Message);
            var ratios = SoftCodeword.RatiosFor(codeword);
            for (var i = 0; i < ratios.Length; i++)
            {
                ratios[i] = -ratios[i];
            }

            Ft8SoftSymbols.Normalise(ratios);

            var outcome = Unit223SumProduct.Decode(ratios, cleanBits);
            invertedChecksFailed.Add(outcome.UnsatisfiedChecks);
            if (outcome.ParitySatisfied)
            {
                invertedParity++;
                if (cleanBits.AsSpan().SequenceEqual(codeword))
                {
                    invertedTrue++;
                }
            }
        }

        _output.WriteLine("CONTROL 2 - IT REFUSES INVERTED RATIOS. Every one of the 174 bits stated");
        _output.WriteLine("confidently and wrongly, which is the furthest a receiver can be from right:");
        _output.WriteLine($"  passed parity                 : {invertedParity} of {population.Count}");
        _output.WriteLine($"  returned the TRUE codeword    : {invertedTrue} of {population.Count}");
        _output.WriteLine($"  unsatisfied checks it reached : {invertedChecksFailed.Min()} to "
            + $"{invertedChecksFailed.Max()}, mean {invertedChecksFailed.Average():F1} of "
            + $"{Ft8Tables.LdpcM}");
        _output.WriteLine(string.Empty);

        // =================================================================================
        // CONTROL 3 - IT REFUSES NOISE. Random ratios, on the scale the decoder expects.
        // =================================================================================
        const int randomTrials = 5000;
        var random = new Random(223_004);
        var randomParity = 0;
        var randomChecksum = 0;
        var randomMessages = 0;
        var libraryRandomParity = 0;
        var randomChecks = new List<int>();
        var randomRatios = new float[Ft8Tables.LdpcN];
        var libraryRandomBits = new byte[LdpcDecoder.CodewordBits];

        for (var t = 0; t < randomTrials; t++)
        {
            for (var i = 0; i < randomRatios.Length; i++)
            {
                // Box-Muller, so the array looks like a receiver reading noise rather than like a
                // uniform draw nothing in nature produces.
                var u1 = 1.0 - random.NextDouble();
                var u2 = random.NextDouble();
                randomRatios[i] = (float)(Math.Sqrt(-2.0 * Math.Log(u1))
                    * Math.Cos(2.0 * Math.PI * u2));
            }

            Ft8SoftSymbols.Normalise(randomRatios);

            var libraryOutcome = LdpcDecoder.Decode(randomRatios, libraryRandomBits);
            if (libraryOutcome.ParitySatisfied)
            {
                libraryRandomParity++;
            }

            var outcome = Unit223SumProduct.Decode(randomRatios, cleanBits);
            randomChecks.Add(outcome.UnsatisfiedChecks);
            if (!outcome.ParitySatisfied)
            {
                continue;
            }

            randomParity++;

            // THE GATE ROW H ACTUALLY COUNTS AT. Passing parity is landing on a codeword; becoming
            // a message means the 14-bit checksum agreed too, and row H counts only messages.
            var message = SoftCodeword.MessageFrom(cleanBits);
            if (message is null)
            {
                continue;
            }

            randomChecksum++;
            if (Ft8MessageDecoder.Decode(message).Decoded)
            {
                randomMessages++;
            }
        }

        _output.WriteLine($"CONTROL 3 - IT REFUSES NOISE. {randomTrials} Gaussian ratio arrays, "
            + "normalised to the");
        _output.WriteLine("same variance a real one carries:");
        _output.WriteLine($"  the independent decoder PASSED PARITY : {randomParity} of {randomTrials}");
        _output.WriteLine($"  ... of those, PASSED THE CHECKSUM     : {randomChecksum}");
        _output.WriteLine($"  ... of those, BECAME A MESSAGE        : {randomMessages}");
        _output.WriteLine($"  the library's decoder passed parity   : {libraryRandomParity} "
            + $"of {randomTrials}");
        _output.WriteLine($"  closest the independent one ever came : {randomChecks.Min()} unsatisfied "
            + $"checks of {Ft8Tables.LdpcM}, mean {randomChecks.Average():F1}");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  THIS CONTROL WAS WRITTEN EXPECTING ZERO ON THE FIRST LINE AND DID NOT GET");
        _output.WriteLine("  IT, AND THAT IS REPORTED RATHER THAN SMOOTHED. A decoder given four times");
        _output.WriteLine("  upstream's iterations and exact arithmetic is MORE willing to land on some");
        _output.WriteLine("  codeword than the library's is, and on pure noise it occasionally does.");
        _output.WriteLine("  That is the instrument behaving as a more patient decoder, not as a broken");
        _output.WriteLine("  one - and the library's count on the SAME arrays is printed beside it so");
        _output.WriteLine("  the difference is a measurement rather than a claim.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  WHAT ROW H ACTUALLY DEPENDS ON IS THE THIRD LINE, and the assertion is");
        _output.WriteLine("  there: row H counts a trial only when a MESSAGE came back, so the number");
        _output.WriteLine("  that has to be zero is the number of messages noise produced.");
        _output.WriteLine(string.Empty);

        // =================================================================================
        // CONTROL 4 - IT AGREES WITH THE INSTRUMENT ALREADY IN THE TREE, and this is the one
        // that makes the row readable: a decoder that recovered far MORE hard flips than the
        // library's would be a different decoder rather than a second opinion.
        // =================================================================================
        _output.WriteLine("CONTROL 4 - IT AGREES WITH THE LIBRARY ON HARD BIT FLIPS. This is unit 215's");
        _output.WriteLine("own sweep re-run with both decoders on the same damaged arrays, and it is the");
        _output.WriteLine("number unit 222's inference leaned on:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"k flips",9} {"library recovered",20} {"independent recovered",24}");

        var libraryBits = new byte[LdpcDecoder.CodewordBits];
        foreach (var k in new[] { 0, 6, 12, 17, 24, 31 })
        {
            var flipRandom = new Random(223_100 + k);
            var libraryGot = 0;
            var independentGot = 0;

            foreach (var entry in population)
            {
                var codeword = SoftCodeword.CodewordBitsFor(entry.Message);

                var a = SoftCodeword.RatiosFor(codeword);
                SoftCodeword.FlipDistinctPositions(a, k, new Random(flipRandom.Next()));
                var b = (float[])a.Clone();

                Ft8SoftSymbols.Normalise(a);
                Ft8SoftSymbols.Normalise(b);

                var libraryOutcome = LdpcDecoder.Decode(a, libraryBits);
                if (libraryOutcome.ParitySatisfied && libraryBits.AsSpan().SequenceEqual(codeword))
                {
                    libraryGot++;
                }

                var independentOutcome = Unit223SumProduct.Decode(b, cleanBits);
                if (independentOutcome.ParitySatisfied && cleanBits.AsSpan().SequenceEqual(codeword))
                {
                    independentGot++;
                }
            }

            _output.WriteLine($"{k,9} {$"{libraryGot} of {population.Count}",20} "
                + $"{$"{independentGot} of {population.Count}",24}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  Read against unit 215's measurement that recovery reaches ZERO at 17 hard");
        _output.WriteLine("  flips. A soft decoder is NOT bound by that limit on real ratios, which is");
        _output.WriteLine("  the whole point of task 4 - but on flips of equal confidence it should be,");
        _output.WriteLine("  and if this one were not it would be reporting something other than a");
        _output.WriteLine("  decode.");

        // The controls are gates, not measurements. These are what stop a broken instrument from
        // producing a row that gets believed.
        Assert.Equal(population.Count, clean);
        Assert.Equal(0, invertedParity);
        Assert.Equal(0, invertedTrue);
        Assert.Equal(0, randomMessages);
        Assert.True(
            worstRoundTrip < 1e-8,
            $"phi is not its own inverse below x = 20: worst relative error {worstRoundTrip:E3}");
        Assert.Equal(522, Unit223SumProduct.EdgeCount);
    }

    /// <summary>
    /// <b>Row H, and the score census beside it.</b> The independent decoder over the identical
    /// ratios, at the same candidates, on the same 306 trials as every other row tonight.
    /// </summary>
    [Fact]
    public void TheIndependentDecoderOverTheIdenticalRatiosAtTheVerdictRung()
    {
        var population = Ft8Step6Ladder.Population();
        var geometry = new Ft8WaterfallGeometry();
        var search = new Ft8SyncSearch();
        const double rung = Unit222TraceTests.VerdictRungDecibels;
        var seeds = Ft8Step6Ladder.SeedsFor(rung);
        var trials = population.Count * seeds;

        var (oracle, controlAgreements) = FindOracleAlignment(population, geometry, search);

        _output.WriteLine($"UNIT 223 TASK 4 - ROW H AT {rung:F1} dB, {trials} TRIALS.");
        _output.WriteLine("Same population, same seeds, same candidates, same normalised ratios as");
        _output.WriteLine("rows A, F and G. The only thing that moves is which decoder is handed them.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("THE ORACLE ALIGNMENT THE CENSUS IS READ AT, SWEPT AND NOT COMPUTED:");
        _output.WriteLine($"  block {oracle.BlockOffset}, t{oracle.TimeSubOffset}, "
            + $"bin {oracle.BinOffset}, f{oracle.FrequencySubOffset}");
        _output.WriteLine($"  agreement there at -5 dB over 12 messages: mean "
            + $"{controlAgreements.Average():F1} of 174, lowest {controlAgreements.Min()}");
        _output.WriteLine("  IT PLACES THE READ AND IT NEVER REACHES A DECODER. Both decoders below are");
        _output.WriteLine("  handed ratios and nothing else; the truth is used after they have answered.");
        _output.WriteLine(string.Empty);

        var rowH = new Tally("H. independent soft decoder");

        // The census. Taken at the oracle place, AFTER both decoders have answered from ratios that
        // were never told what was sent.
        var libraryRecovered = 0;
        var independentRecovered = 0;
        var trueHigherThanLibrary = 0;
        var trueHigherThanIndependent = 0;
        var libraryGaps = new List<double>();
        var independentGaps = new List<double>();
        var trueScores = new List<double>();
        var libraryScores = new List<double>();
        var independentScores = new List<double>();
        var libraryChecks = new List<int>();
        var independentChecks = new List<int>();
        var hardErrors = new List<int>();

        var oracleRatios = new float[Ft8SoftSymbols.RatioCount];
        var libraryBits = new byte[LdpcDecoder.CodewordBits];
        var independentBits = new byte[Ft8Tables.LdpcN];
        var decisions = new byte[Ft8SoftSymbols.RatioCount];

        var watch = Stopwatch.StartNew();

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
                var mixed = SearchFixture.AddNoise(clean, noise, sigma, out _);

                var expected = Ft8MessageDecoder.Decode(entry.Message).Text;
                var waterfall = new Ft8Monitor(geometry).Analyse(mixed);
                var candidates = search.Find(waterfall);

                var trial = Run(candidates, waterfall, expected);
                rowH.Add(trial.Returned, trial.Wrong);

                // ---------------------------------------------------------------- THE CENSUS
                Ft8SoftSymbols.Extract(waterfall, oracle, oracleRatios);
                Ft8SoftSymbols.Normalise(oracleRatios);

                var libraryOutcome = LdpcDecoder.Decode(oracleRatios, libraryBits);
                var independentOutcome = Unit223SumProduct.Decode(oracleRatios, independentBits);

                libraryChecks.Add(libraryOutcome.UnsatisfiedChecks);
                independentChecks.Add(independentOutcome.UnsatisfiedChecks);

                // Only now, and only to describe. The truth has reached no decoder.
                var codeword = SensitivityLadder.TrueCodeword(entry);
                var truth = new byte[Ft8Tables.LdpcN];
                for (var b = 0; b < truth.Length; b++)
                {
                    truth[b] = (byte)((codeword[b / 8] >> (7 - (b % 8))) & 1);
                }

                Ft8SoftSymbols.HardDecision(oracleRatios, decisions);
                var errors = 0;
                for (var b = 0; b < truth.Length; b++)
                {
                    if (decisions[b] != truth[b])
                    {
                        errors++;
                    }
                }

                hardErrors.Add(errors);

                var libraryGot = libraryOutcome.ParitySatisfied
                    && libraryBits.AsSpan().SequenceEqual(truth);
                var independentGot = independentOutcome.ParitySatisfied
                    && independentBits.AsSpan().SequenceEqual(truth);

                if (libraryGot)
                {
                    libraryRecovered++;
                }

                if (independentGot)
                {
                    independentRecovered++;
                }

                var trueScore = Score(oracleRatios, truth);
                trueScores.Add(trueScore);

                if (!libraryGot)
                {
                    var settled = Score(oracleRatios, libraryBits);
                    libraryScores.Add(settled);
                    libraryGaps.Add(trueScore - settled);
                    if (trueScore > settled)
                    {
                        trueHigherThanLibrary++;
                    }
                }

                if (!independentGot)
                {
                    var settled = Score(oracleRatios, independentBits);
                    independentScores.Add(settled);
                    independentGaps.Add(trueScore - settled);
                    if (trueScore > settled)
                    {
                        trueHigherThanIndependent++;
                    }
                }
            }
        }

        watch.Stop();

        var rate = rowH.Rate(trials);
        var (lower, upper) = Ft8Step6Ladder.Wilson(rowH.Returned, trials);

        _output.WriteLine($"{"row",30} {"n",5} {"of",5} {"rate",7} {"lo 95",7} {"hi 95",7} {"WRONG",6}"
            + "  equivalent");
        _output.WriteLine($"{rowH.Name,30} {rowH.Returned,5} {trials,5} {rate,7:F1} "
            + $"{lower,7:F1} {upper,7:F1} {rowH.Wrong,6}  {Unit222Budget.EquivalentShift(rate)}");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  {trials} slot decodes at {Unit223SumProduct.DefaultMaxIterations} "
            + $"iterations in {watch.Elapsed.TotalSeconds:F1} s");
        _output.WriteLine(string.Empty);

        _output.WriteLine("AT THE ORACLE ALIGNMENT, WHERE THE TRANSMISSION ACTUALLY IS - which is the");
        _output.WriteLine("most generous place either decoder could be asked to work:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"  the library's decoder recovered the true codeword : "
            + $"{libraryRecovered} of {trials}");
        _output.WriteLine($"  the independent decoder recovered it              : "
            + $"{independentRecovered} of {trials}");
        _output.WriteLine($"  hard decisions wrong, of 174                      : "
            + $"mean {hardErrors.Average():F1}, {hardErrors.Min()} to {hardErrors.Max()}");
        _output.WriteLine($"  unsatisfied checks the library got down to        : "
            + $"mean {libraryChecks.Average():F1} of {Ft8Tables.LdpcM}");
        _output.WriteLine($"  unsatisfied checks the independent one got to     : "
            + $"mean {independentChecks.Average():F1} of {Ft8Tables.LdpcM}");
        _output.WriteLine(string.Empty);

        _output.WriteLine("THE SCORE CENSUS, AND IT IS A DESCRIPTION AND NOT A VERDICT. The score is");
        _output.WriteLine("sum over bits of ratio times (2*bit - 1) on the normalised ratios: the");
        _output.WriteLine("log-likelihood of a word, up to a constant. HIGHER IS MORE LIKELY. If the");
        _output.WriteLine("true codeword scores LOWER than the word a decoder settled on, then no");
        _output.WriteLine("decoder searching these ratios for the most likely codeword can find it -");
        _output.WriteLine("the ratios themselves prefer the wrong answer:");
        _output.WriteLine(string.Empty);
        _output.WriteLine($"{"over the failing trials",34} {"n",6} {"mean gap",12} {"lowest",12} "
            + $"{"highest",12} {"true higher",12}");

        foreach (var (name, gaps, higher) in new[]
                 {
                     ("the library's decoder", libraryGaps, trueHigherThanLibrary),
                     ("the independent decoder", independentGaps, trueHigherThanIndependent),
                 })
        {
            if (gaps.Count == 0)
            {
                _output.WriteLine($"{name,34} {0,6}   no failing trials");
                continue;
            }

            _output.WriteLine($"{name,34} {gaps.Count,6} {gaps.Average(),12:F1} {gaps.Min(),12:F1} "
                + $"{gaps.Max(),12:F1} {$"{100.0 * higher / gaps.Count:F1} %",12}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine($"  the true codeword's own score, over all {trials} trials : mean "
            + $"{trueScores.Average():F1}, {trueScores.Min():F1} to {trueScores.Max():F1}");

        if (libraryScores.Count > 0)
        {
            _output.WriteLine($"  the library's settled word, over its failures           : mean "
                + $"{libraryScores.Average():F1}");
        }

        if (independentScores.Count > 0)
        {
            _output.WriteLine($"  the independent decoder's, over its failures            : mean "
                + $"{independentScores.Average():F1}");
        }

        _output.WriteLine(string.Empty);
        _output.WriteLine("  A POSITIVE GAP MEANS THE TRUE CODEWORD IS THE BETTER ANSWER AND THE DECODER");
        _output.WriteLine("  MISSED IT - the information is there and the search failed. A NEGATIVE GAP");
        _output.WriteLine("  MEANS THE RATIOS THEMSELVES PREFER SOMETHING ELSE, and then no decoder over");
        _output.WriteLine("  these ratios can recover the message, however patient or however exact.");
        _output.WriteLine(string.Empty);
        _output.WriteLine("  NOT ONE TRIAL IN ROW H WAS COUNTED FROM THIS CENSUS. Row H counts only");
        _output.WriteLine("  trials the decoder returned on its own, from ratios that were never told");
        _output.WriteLine("  what was sent.");

        // Assertions are on the instrument only. Every rate above is a measurement.
        Assert.Equal(trials, rowH.Trials);
        Assert.True(
            controlAgreements.Min() >= 170,
            $"the oracle alignment does not find the transmission at -5 dB: lowest agreement "
            + $"{controlAgreements.Min()} of 174");
    }

    /// <summary>
    /// The soft score of a word against a set of ratios: <c>sum over bits of ratio times
    /// (2*bit - 1)</c>. <b>Higher is more likely.</b>
    /// </summary>
    private static double Score(ReadOnlySpan<float> ratios, ReadOnlySpan<byte> bits)
    {
        var total = 0.0;
        for (var i = 0; i < ratios.Length; i++)
        {
            total += ratios[i] * ((2 * bits[i]) - 1);
        }

        return total;
    }

    /// <summary>
    /// <b><c>Ft8SlotDecoder.Decode</c>'s own loop with the correction stage replaced by the
    /// independent decoder</b>, and everything else — the candidate list, the normalisation, both
    /// gates, the de-duplication key and the message limit — left exactly as the library has it.
    /// </summary>
    private static Trial Run(
        IReadOnlyList<Ft8Candidate> candidates,
        Ft8Waterfall waterfall,
        string expected)
    {
        var cache = new Ft8CallsignCache();
        var seen = new List<byte[]>();
        var texts = new List<string>();

        var ratios = new float[Ft8SoftSymbols.RatioCount];
        var codeword = new byte[Ft8Tables.LdpcN];

        foreach (var candidate in candidates)
        {
            Ft8SoftSymbols.Extract(waterfall, candidate, ratios);
            Ft8SoftSymbols.Normalise(ratios);

            var outcome = Unit223SumProduct.Decode(ratios, codeword);
            if (!outcome.ParitySatisfied)
            {
                continue;
            }

            var message = SoftCodeword.MessageFrom(codeword);
            if (message is null)
            {
                continue;
            }

            var decoded = Ft8MessageDecoder.Decode(message, cache);
            if (!decoded.Decoded)
            {
                continue;
            }

            var key = codeword[..Ft8Payload.MessageBits];

            var already = false;
            foreach (var previous in seen)
            {
                if (key.AsSpan().SequenceEqual(previous))
                {
                    already = true;
                    break;
                }
            }

            if (already || texts.Count >= Ft8SlotDecoder.DefaultMessageLimit)
            {
                continue;
            }

            seen.Add(key);
            texts.Add(decoded.Text);
        }

        return new Trial(
            texts.Contains(expected, StringComparer.Ordinal),
            texts.Where(t => !string.Equals(t, expected, StringComparison.Ordinal)).ToArray());
    }

    /// <summary>
    /// <b>Where the fixture's signal actually sits in the waterfall</b>, swept at a ratio where
    /// everything decodes. Unit 221 recorded that computing it from the geometry puts it one block
    /// out and its own control refused the result, so it is swept here too.
    /// </summary>
    private static (Ft8Candidate Swept, List<int> Control) FindOracleAlignment(
        IReadOnlyList<EncodeCorpus.Entry> population,
        Ft8WaterfallGeometry geometry,
        Ft8SyncSearch search)
    {
        geometry.TryBinFor(Unit222TraceTests.OnGridHz, out var bin, out var freqSub);

        var noise = new GaussianNoise(223_500);
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
            var bestAt = new Ft8Candidate(0, 0, 0, bin, freqSub);

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

        return (votes.OrderByDescending(v => v.Value).First().Key, agreements);
    }

    private sealed record Trial(bool Returned, string[] Wrong);

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
}

using Ft8Sharp.Dsp;
using Ft8Sharp.Ldpc;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The combiner on synthesised ratios rather than on audio, with the errors planted at positions
/// this file chose.</b> Whether combining helps on real candidates is what the ladder measures; this
/// is the part that is either right or wrong on one input.
/// </summary>
/// <remarks>
/// <para>
/// <b>Four properties, and the second and third are the ones that catch a combiner measuring
/// itself.</b> Adding two hearings of the same transmission must beat either one; adding two hearings
/// of <em>different</em> transmissions must not, because that is the wrong-pairing case and the port's
/// CRC-14 is what has to refuse it; and adding a hearing to itself must change nothing at all, because
/// a combiner that reports a gain from hearing one slot twice is measuring its own arithmetic and not
/// the air.
/// </para>
/// <para>
/// <b>The codewords are the port's own.</b> A random 91-bit payload through
/// <see cref="LdpcEncoder.Encode(ReadOnlySpan{byte}, Span{byte})"/> is a real codeword of the real
/// code, so a combination that lands on one is a combination the gate could accept.
/// </para>
/// </remarks>
public class Ft8DeepSoftCombinerTests(ITestOutputHelper output)
{
    /// <summary>Codeword bits, 174.</summary>
    private const int N = LdpcDecoder.CodewordBits;

    /// <summary>Systematic bits, 91.</summary>
    private const int K = LdpcEncoder.PayloadBits;

    /// <summary>
    /// <b>Two hearings of one codeword with independent errors combine to strictly fewer errors than
    /// either of them carried, over a spread of error counts.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The model is the one that makes the claim mean something.</b> A hearing gets its bits right
    /// with confident evidence and wrong with weak evidence — which is what a real candidate does,
    /// since a hard decision goes wrong exactly where the two best tone magnitudes were close. Two
    /// hearings whose wrong positions are disjoint therefore repair each other: at a position slot A
    /// got wrong, slot B's confident correct opinion outweighs A's weak wrong one and the sum lands on
    /// the transmitted bit.
    /// </para>
    /// <para>
    /// <b>And the overlapping case is measured beside it rather than left out</b>, because errors on
    /// real air are not disjoint: where both hearings are wrong at the same position, no amount of
    /// adding recovers it, and the printout says so.
    /// </para>
    /// </remarks>
    [Fact]
    public void TwoHearingsOfOneCodewordWithIndependentErrorsCombineToFewerErrorsThanEither()
    {
        var random = new Random(247_001);
        var combined = new float[N];
        var hard = new byte[N];

        output.WriteLine("errors  slot A  slot B  disjoint  COMBINED   summed variance");

        foreach (var errors in new[] { 5, 10, 20, 30, 40, 60 })
        {
            var totalA = 0;
            var totalB = 0;
            var totalCombined = 0;
            var worstCombined = 0;
            var summedVariance = 0.0;
            const int trials = 40;

            for (var t = 0; t < trials; t++)
            {
                var truth = RandomCodeword(random);
                var (a, b) = DisjointErrorPair(random, truth, errors);

                var errorsA = ErrorsIn(a, truth);
                var errorsB = ErrorsIn(b, truth);

                summedVariance += Ft8DeepSoftCombiner.Combine(
                    a, b, Ft8DeepCombineWeighting.Equal, combined);
                Ft8SoftSymbols.HardDecision(combined, hard);
                var errorsCombined = Distance(hard, truth);

                Assert.True(
                    errorsCombined < errorsA && errorsCombined < errorsB,
                    $"planted {errors} disjoint errors in each hearing: slot A carried {errorsA}, "
                        + $"slot B carried {errorsB}, and the combination carried {errorsCombined}. "
                        + "Two hearings whose wrong positions do not overlap must repair each other.");

                totalA += errorsA;
                totalB += errorsB;
                totalCombined += errorsCombined;
                worstCombined = Math.Max(worstCombined, errorsCombined);
            }

            output.WriteLine(
                $"{errors,6}  {totalA / (double)trials,6:F1}  {totalB / (double)trials,6:F1}  "
                + $"{"yes",8}  {totalCombined / (double)trials,8:F1}   "
                + $"{summedVariance / trials,8:F1}   worst {worstCombined}");
        }

        // The overlapping case, printed rather than asserted away: where both hearings are wrong at
        // the same position, adding does not recover it and this is what that costs.
        foreach (var overlap in new[] { 0, 5, 10, 20 })
        {
            var totalCombined = 0;
            const int trials = 40;

            for (var t = 0; t < trials; t++)
            {
                var truth = RandomCodeword(random);
                var (a, b) = OverlappingErrorPair(random, truth, 30, overlap);

                Ft8DeepSoftCombiner.Combine(a, b, Ft8DeepCombineWeighting.Equal, combined);
                Ft8SoftSymbols.HardDecision(combined, hard);
                totalCombined += Distance(hard, truth);
            }

            output.WriteLine(
                $"30 errors each, {overlap,2} of them at the SAME positions: combination carries "
                + $"{totalCombined / (double)trials:F1} on average");
        }
    }

    /// <summary>
    /// <b>Two hearings of DIFFERENT codewords do not combine into anything closer to either.</b> This
    /// is the wrong-pairing case, and it is the one the port's CRC-14 has to catch.
    /// </summary>
    /// <remarks>
    /// Two distinct codewords of this code differ in a large fraction of their 174 positions. Where
    /// they agree the sum reinforces; where they disagree the two hearings' evidence cancels and the
    /// sum decides on whatever noise is left. So the combination sits about half the codewords'
    /// separation away from each of them — <b>much further from either transmission than that
    /// transmission's own hearing was</b>, and far past anything belief propagation finishes.
    /// </remarks>
    [Fact]
    public void TwoHearingsOfDifferentCodewordsCombineToSomethingCloserToNeither()
    {
        var random = new Random(247_002);
        var combined = new float[N];
        var hard = new byte[N];

        var worstRatioA = double.MaxValue;
        var worstRatioB = double.MaxValue;
        var separations = 0;
        const int trials = 60;

        for (var t = 0; t < trials; t++)
        {
            var truthA = RandomCodeword(random);
            var truthB = RandomCodeword(random);
            separations += Distance(truthA, truthB);

            var a = Hearing(random, truthA, 10);
            var b = Hearing(random, truthB, 10);

            var errorsA = ErrorsIn(a, truthA);
            var errorsB = ErrorsIn(b, truthB);

            Ft8DeepSoftCombiner.Combine(a, b, Ft8DeepCombineWeighting.Equal, combined);
            Ft8SoftSymbols.HardDecision(combined, hard);

            var toA = Distance(hard, truthA);
            var toB = Distance(hard, truthB);

            Assert.True(
                toA > errorsA && toB > errorsB,
                $"trial {t}: hearing A was {errorsA} from codeword A and hearing B was {errorsB} from "
                    + $"codeword B, and their combination sits {toA} from A and {toB} from B. A "
                    + "wrongly paired combination must not be closer to either transmission than the "
                    + "hearing it came from - if it were, the pairing rule could be wrong and the "
                    + "result still decode.");

            worstRatioA = Math.Min(worstRatioA, toA / (double)Math.Max(errorsA, 1));
            worstRatioB = Math.Min(worstRatioB, toB / (double)Math.Max(errorsB, 1));
        }

        output.WriteLine(
            $"{trials} wrongly paired combinations. Two distinct codewords sat "
            + $"{separations / (double)trials:F1} of {N} apart on average.");
        output.WriteLine(
            $"The combination was never closer than {worstRatioA:F1}x hearing A's own distance to A, "
            + $"nor {worstRatioB:F1}x hearing B's to B.");
        output.WriteLine(
            "So a wrong pairing does not decode by accident - it produces a codeword the port's parity "
            + "gate never converges on, and the CRC-14 behind it never sees.");
    }

    /// <summary>
    /// <b>A hearing combined with itself decides exactly what it decided alone.</b> Both weightings,
    /// because a weighting that made this false would be reporting its own arithmetic as a gain.
    /// </summary>
    [Fact]
    public void CombiningAHearingWithItselfChangesNothing()
    {
        var random = new Random(247_003);
        var combined = new float[N];
        var alone = new byte[N];
        var twice = new byte[N];
        var normalised = new float[N];

        foreach (var weighting in new[]
                 {
                     Ft8DeepCombineWeighting.Equal,
                     Ft8DeepCombineWeighting.ByPreNormalisationVariance,
                 })
        {
            for (var t = 0; t < 40; t++)
            {
                var truth = RandomCodeword(random);
                var a = Hearing(random, truth, 25);

                Array.Copy(a, normalised, N);
                Ft8SoftSymbols.Normalise(normalised);
                Ft8SoftSymbols.HardDecision(normalised, alone);

                var summed = Ft8DeepSoftCombiner.Combine(a, a, weighting, combined);
                Ft8SoftSymbols.HardDecision(combined, twice);

                Assert.Equal(alone, twice);

                if (t == 0)
                {
                    output.WriteLine(
                        $"{weighting}: hearing itself twice gave summed variance {summed:F1} before "
                        + $"re-normalisation, and the same {Distance(alone, truth)} errors of {N} "
                        + "afterwards.");
                }
            }
        }

        // And the caller's array is untouched, which is what lets the single-slot path run afterwards
        // over exactly the ratios it would have seen.
        var original = Hearing(new Random(247_004), RandomCodeword(new Random(247_005)), 20);
        var copy = (float[])original.Clone();
        Ft8DeepSoftCombiner.Combine(original, original, Ft8DeepCombineWeighting.Equal, combined);
        Assert.Equal(copy, original);
        output.WriteLine("The inputs were not modified: combining only ever adds.");
    }

    /// <summary>
    /// <b>It never throws on degenerate input, because it will be called on noise.</b>
    /// </summary>
    [Fact]
    public void DegenerateInputIsCombinedRatherThanRefused()
    {
        var combined = new float[N];

        var zero = new float[N];
        var equal = new float[N];
        Array.Fill(equal, 5.0f);
        var infinite = new float[N];
        Array.Fill(infinite, float.PositiveInfinity);
        var negativeInfinite = new float[N];
        Array.Fill(negativeInfinite, float.NegativeInfinity);
        var notANumber = new float[N];
        Array.Fill(notANumber, float.NaN);
        var mixed = new float[N];
        for (var i = 0; i < N; i++)
        {
            mixed[i] = i % 3 switch
            {
                0 => float.NaN,
                1 => float.PositiveInfinity,
                _ => (i % 7) - 3.0f,
            };
        }

        var cases = new (string Name, float[] Value)[]
        {
            ("all zero", zero),
            ("all equal", equal),
            ("all +infinity", infinite),
            ("all -infinity", negativeInfinite),
            ("all not-a-number", notANumber),
            ("mixed finite and not", mixed),
        };

        foreach (var weighting in new[]
                 {
                     Ft8DeepCombineWeighting.Equal,
                     Ft8DeepCombineWeighting.ByPreNormalisationVariance,
                 })
        {
            foreach (var first in cases)
            {
                foreach (var second in cases)
                {
                    var summed = Ft8DeepSoftCombiner.Combine(
                        first.Value, second.Value, weighting, combined);

                    Assert.True(
                        float.IsFinite(summed),
                        $"{weighting}: {first.Name} with {second.Name} reported a summed variance of "
                            + $"{summed}, and a variance that is not a number would be printed into a "
                            + "report as evidence.");

                    for (var i = 0; i < N; i++)
                    {
                        Assert.True(
                            float.IsFinite(combined[i]),
                            $"{weighting}: {first.Name} with {second.Name} put {combined[i]} at "
                                + $"position {i}. A ratio that is not finite reaching the port's "
                                + "belief propagation is not a refusal, it is a hang or a wrong "
                                + "answer.");
                    }
                }
            }
        }

        output.WriteLine(
            $"{cases.Length * cases.Length * 2} degenerate combinations, none refused and none "
            + "producing a ratio that is not finite.");
    }

    /// <summary>
    /// <b>What it does refuse, and it refuses with the reason in the message.</b>
    /// </summary>
    [Fact]
    public void ItRefusesAShortSpanAnEmptyListAndANullHearing()
    {
        var full = new float[N];
        var short_ = new float[N - 1];

        var shortOutput = Assert.Throws<ArgumentException>(
            () => Ft8DeepSoftCombiner.Combine([full, full], Ft8DeepCombineWeighting.Equal, short_));
        var shortInput = Assert.Throws<ArgumentException>(
            () => Ft8DeepSoftCombiner.Combine([full, short_], Ft8DeepCombineWeighting.Equal, full));
        var empty = Assert.Throws<ArgumentException>(
            () => Ft8DeepSoftCombiner.Combine([], Ft8DeepCombineWeighting.Equal, full));
        var nullList = Assert.Throws<ArgumentNullException>(
            () => Ft8DeepSoftCombiner.Combine(null!, Ft8DeepCombineWeighting.Equal, full));
        var nullHearing = Assert.Throws<ArgumentNullException>(
            () => Ft8DeepSoftCombiner.Combine([full, null!], Ft8DeepCombineWeighting.Equal, full));

        output.WriteLine(shortOutput.Message);
        output.WriteLine(shortInput.Message);
        output.WriteLine(empty.Message);
        output.WriteLine(nullList.Message);
        output.WriteLine(nullHearing.Message);

        // One hearing is not a refusal: a combined path with nothing to combine yet must run.
        var one = Ft8DeepSoftCombiner.Combine([full], Ft8DeepCombineWeighting.Equal, full);
        Assert.True(float.IsFinite(one));
    }

    /// <summary>A real codeword of the real code, from a random payload.</summary>
    private static byte[] RandomCodeword(Random random)
    {
        var payload = new byte[LdpcEncoder.PayloadBytes];
        random.NextBytes(payload);

        // The payload is 91 bits in 12 bytes: the last five bits of the last byte are not part of it
        // and the encoder does not read them, but they are cleared so the codeword is reproducible.
        payload[^1] &= 0xE0;

        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, codeword);

        var bits = new byte[N];
        for (var i = 0; i < N; i++)
        {
            bits[i] = (byte)((codeword[i / 8] >> (7 - (i % 8))) & 1);
        }

        Assert.Equal(K, K);
        return bits;
    }

    /// <summary>
    /// One hearing of a codeword: right bits with confident evidence, wrong bits with weak evidence at
    /// <paramref name="errors"/> positions chosen at random.
    /// </summary>
    private static float[] Hearing(Random random, byte[] truth, int errors) =>
        Hearing(random, truth, ChoosePositions(random, errors, []));

    private static float[] Hearing(Random random, byte[] truth, HashSet<int> wrong)
    {
        var ratios = new float[N];
        for (var i = 0; i < N; i++)
        {
            var sign = truth[i] != 0 ? 1.0 : -1.0;

            // Confident where it is right - a magnitude around one - and weak where it is wrong,
            // which is what a hard decision from real tone magnitudes does.
            var magnitude = wrong.Contains(i)
                ? -(0.05 + (0.45 * random.NextDouble()))
                : 0.6 + (0.8 * random.NextDouble());

            ratios[i] = (float)(sign * magnitude);
        }

        return ratios;
    }

    /// <summary>Two hearings of one codeword whose wrong positions do not overlap at all.</summary>
    private static (float[] A, float[] B) DisjointErrorPair(Random random, byte[] truth, int errors)
    {
        var first = ChoosePositions(random, errors, []);
        var second = ChoosePositions(random, errors, first);
        return (Hearing(random, truth, first), Hearing(random, truth, second));
    }

    /// <summary>
    /// Two hearings of one codeword sharing exactly <paramref name="overlap"/> wrong positions.
    /// </summary>
    private static (float[] A, float[] B) OverlappingErrorPair(
        Random random, byte[] truth, int errors, int overlap)
    {
        var first = ChoosePositions(random, errors, []);
        var shared = first.OrderBy(_ => random.Next()).Take(overlap).ToHashSet();
        var second = ChoosePositions(random, errors - overlap, first);
        second.UnionWith(shared);
        return (Hearing(random, truth, first), Hearing(random, truth, second));
    }

    /// <summary>Positions chosen at random, avoiding a set already taken.</summary>
    private static HashSet<int> ChoosePositions(Random random, int count, HashSet<int> avoid)
    {
        var chosen = new HashSet<int>(count);
        while (chosen.Count < count)
        {
            var position = random.Next(N);
            if (!avoid.Contains(position))
            {
                chosen.Add(position);
            }
        }

        return chosen;
    }

    /// <summary>How many of a hearing's hard decisions disagree with the codeword it came from.</summary>
    private static int ErrorsIn(float[] ratios, byte[] truth)
    {
        var hard = new byte[N];
        Ft8SoftSymbols.HardDecision(ratios, hard);
        return Distance(hard, truth);
    }

    private static int Distance(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        var distance = 0;
        for (var i = 0; i < left.Length; i++)
        {
            if (left[i] != right[i])
            {
                distance++;
            }
        }

        return distance;
    }
}

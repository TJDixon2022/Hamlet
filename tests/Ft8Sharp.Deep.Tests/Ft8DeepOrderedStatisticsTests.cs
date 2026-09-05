using Ft8Sharp.Encode;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;
using Xunit;
using Xunit.Abstractions;

namespace Ft8Sharp.Deep.Tests;

/// <summary>
/// <b>The ordered statistics core, on synthesised ratios rather than on audio, with errors planted at
/// known positions.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>The contract a unit test can actually pin is the order contract.</b> An order-λ decode reaches
/// the transmitted codeword when the most reliable basis carries at most λ errors, and does not when
/// it carries λ+1. Everything else about this algorithm is a question about a distribution over real
/// candidates, which is what the ladder is for; this is the part that is either right or wrong on one
/// input.
/// </para>
/// <para>
/// <b>The basis is made knowable rather than guessed at.</b> Every ratio is given the same magnitude,
/// so the reliability ordering is position order, ties broken on index; the first 91 columns are the
/// systematic ones and are therefore independent, so the most reliable basis is exactly positions 0
/// to 90. An error planted at position 7 is then an error <em>inside</em> the basis, and one planted
/// at position 120 is outside it, by construction and not by hope.
/// </para>
/// <para>
/// <b>Why the true codeword must win the ranking, and not merely be reachable.</b> With equal
/// magnitudes the soft distance is the magnitude times the Hamming distance from the hard decision.
/// The transmitted codeword sits λ away; any other codeword sits at least <c>d - λ</c> away, where
/// <c>d</c> is the code's minimum distance, which for this code is comfortably above 8. So for the
/// orders tested here the transmitted codeword is the unique minimum and the ranking cannot pick
/// something else by accident.
/// </para>
/// </remarks>
public class Ft8DeepOrderedStatisticsTests(ITestOutputHelper output)
{
    private const int N = Ft8DeepOrderedStatistics.CodewordBits;
    private const int K = Ft8DeepOrderedStatistics.BasisBits;

    /// <summary>Every ratio gets this magnitude, so the reliability order is position order.</summary>
    private const float Magnitude = 4.0f;

    /// <summary>A codeword the port itself encoded, from a payload with a checksum on it.</summary>
    private static byte[] TransmittedCodeword(string text)
    {
        var message = new byte[Ft8Payload.MessageBytes];
        var packed = Ft8FreeText.TryPackText(text, message);
        Assert.Equal(Ft8PackResult.Ok, packed);

        var payload = new byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(message, payload);

        var packedCodeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, packedCodeword);

        var bits = new byte[N];
        for (var i = 0; i < N; i++)
        {
            bits[i] = (byte)((packedCodeword[i / 8] >> (7 - (i % 8))) & 1);
        }

        return bits;
    }

    /// <summary>
    /// Ratios that say exactly what the codeword says, every one of them equally loudly. Positive
    /// means the bit is more likely 1, which is the port's convention.
    /// </summary>
    private static float[] RatiosFor(ReadOnlySpan<byte> codeword)
    {
        var ratios = new float[N];
        for (var i = 0; i < N; i++)
        {
            ratios[i] = codeword[i] != 0 ? Magnitude : -Magnitude;
        }

        return ratios;
    }

    private static int Distance(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
    {
        var d = 0;
        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                d++;
            }
        }

        return d;
    }

    /// <summary>A clean codeword comes back at order 0, with one re-encoding and no search.</summary>
    [Fact]
    public void ACleanCodewordRecoversAtOrderZero()
    {
        var truth = TransmittedCodeword("HAMLET 246");
        var ratios = RatiosFor(truth);

        var osd = new Ft8DeepOrderedStatistics();
        var recovered = new byte[N];
        var result = osd.Decode(ratios, 0, recovered);

        output.WriteLine($"soft distance {result.SoftDistance:F3}, re-encodings {result.Reencodings}");

        Assert.Equal(truth, recovered);
        Assert.Equal(1, result.Reencodings);
        Assert.Equal(0.0f, result.SoftDistance);
    }

    /// <summary>
    /// <b>Errors below the basis cost nothing.</b> Twenty of them, all outside the most reliable 91,
    /// and order 0 still returns the transmitted codeword - because re-encoding overwrites everything
    /// that is not in the basis. This is the whole reason the ceiling is the basis error count and not
    /// the total error count.
    /// </summary>
    [Fact]
    public void ErrorsOutsideTheBasisAreOverwrittenForFreeAtOrderZero()
    {
        var truth = TransmittedCodeword("HAMLET 246");
        var ratios = RatiosFor(truth);

        for (var i = K; i < K + 20; i++)
        {
            ratios[i] = -ratios[i];
        }

        var osd = new Ft8DeepOrderedStatistics();
        var recovered = new byte[N];
        var result = osd.Decode(ratios, 0, recovered);

        output.WriteLine(
            $"20 errors planted at positions {K}..{K + 19}, all below the basis. "
            + $"soft distance {result.SoftDistance:F3}, re-encodings {result.Reencodings}");

        Assert.Equal(truth, recovered);
        Assert.Equal(1, result.Reencodings);
    }

    /// <summary>
    /// <b>THE ALGORITHM'S CONTRACT: λ errors inside the basis recover at order λ, and λ+1 do not.</b>
    /// </summary>
    /// <remarks>
    /// The errors are planted at positions 3, 17, 41, 62 and 88, all inside the basis, taken in that
    /// order. Nothing about those five positions matters except that they are inside the basis and
    /// fixed, so the test does not depend on a draw.
    /// </remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void LambdaErrorsInsideTheBasisRecoverAtOrderLambdaAndOneMoreDoesNot(int order)
    {
        int[] positions = [3, 17, 41, 62, 88];

        var truth = TransmittedCodeword("HAMLET 246");
        var osd = new Ft8DeepOrderedStatistics();
        var recovered = new byte[N];

        var atOrder = RatiosFor(truth);
        for (var i = 0; i < order; i++)
        {
            atOrder[positions[i]] = -atOrder[positions[i]];
        }

        var reached = osd.Decode(atOrder, order, recovered);
        var reachedDistance = Distance(truth, recovered);

        output.WriteLine(
            $"order {order}, {order} error(s) in the basis: recovered at Hamming {reachedDistance} "
            + $"from the truth, soft distance {reached.SoftDistance:F3}, "
            + $"{reached.Reencodings} re-encodings");

        Assert.Equal(truth, recovered);

        var oneMore = RatiosFor(truth);
        for (var i = 0; i <= order; i++)
        {
            oneMore[positions[i]] = -oneMore[positions[i]];
        }

        var missed = osd.Decode(oneMore, order, recovered);
        var missedDistance = Distance(truth, recovered);

        output.WriteLine(
            $"order {order}, {order + 1} error(s) in the basis: came back at Hamming "
            + $"{missedDistance} from the truth, soft distance {missed.SoftDistance:F3}, "
            + $"{missed.Reencodings} re-encodings");

        Assert.NotEqual(0, missedDistance);
        Assert.Equal(reached.Reencodings, missed.Reencodings);
    }

    /// <summary>
    /// <b>The re-encoding count is what an order costs, and it is reported rather than estimated.</b>
    /// Step 2's fourth exit asks for the cost each order buys, so the count is pinned here against the
    /// arithmetic: one for order 0, plus the number of subsets of the 91 basis positions of each size
    /// up to the order.
    /// </summary>
    [Theory]
    [InlineData(0, 1L)]
    [InlineData(1, 92L)]
    [InlineData(2, 4187L)]
    [InlineData(3, 125672L)]
    public void TheCostOfAnOrderIsTheNumberOfSubsetsOfTheBasis(int order, long expected)
    {
        var truth = TransmittedCodeword("HAMLET 246");
        var ratios = RatiosFor(truth);

        var osd = new Ft8DeepOrderedStatistics();
        var recovered = new byte[N];
        var result = osd.Decode(ratios, order, recovered);

        output.WriteLine($"order {order}: {result.Reencodings} re-encodings");

        Assert.Equal(expected, result.Reencodings);
    }

    /// <summary>
    /// <b>The elimination returns 91 independent columns for every input tried, and it never
    /// throws.</b> It is called on noise, up to 140 times a slot, so degenerate is the ordinary case
    /// rather than the exceptional one.
    /// </summary>
    [Fact]
    public void TheEliminationFindsAWholeBasisOnEveryInputIncludingTheDegenerateOnes()
    {
        var osd = new Ft8DeepOrderedStatistics();
        var recovered = new byte[N];
        var random = new Random(246);

        var cases = new List<(string What, float[] Ratios)>
        {
            ("a real codeword", RatiosFor(TransmittedCodeword("HAMLET 246"))),
            ("all zero", new float[N]),
            ("all equal and positive", Filled(1.0f)),
            ("all equal and negative", Filled(-1.0f)),
            ("all positive infinity", Filled(float.PositiveInfinity)),
            ("all negative infinity", Filled(float.NegativeInfinity)),
            ("all not a number", Filled(float.NaN)),
            ("alternating extremes", Alternating()),
        };

        for (var t = 0; t < 20; t++)
        {
            var noise = new float[N];
            for (var i = 0; i < N; i++)
            {
                noise[i] = (float)((random.NextDouble() * 4.0) - 2.0);
            }

            cases.Add(($"noise {t}", noise));
        }

        foreach (var (what, ratios) in cases)
        {
            var result = osd.Decode(ratios, 1, recovered);

            var basis = osd.MostReliableBasis.ToArray();
            var seen = new HashSet<int>();

            Assert.Equal(K, basis.Length);
            foreach (var column in basis)
            {
                Assert.InRange(column, 0, N - 1);
                Assert.True(seen.Add(column), $"{what}: column {column} appeared twice in the basis.");
            }

            // Whatever came back must be a codeword of the code, whatever the input was - and that
            // is checked through the port's own encoder rather than through its decoder. The
            // decoder cannot be used here: it refuses the all-zero word outright, in upstream's own
            // words, "message converged to all-zeros, which is prohibited", so on all-zero ratios it
            // would report a perfectly valid codeword as a failure. Re-encoding the first 91 bits
            // and requiring the whole 174 back is the same question asked in a way that has no such
            // exception.
            var payload = new byte[LdpcEncoder.PayloadBytes];
            for (var i = 0; i < K; i++)
            {
                if (recovered[i] != 0)
                {
                    payload[i / 8] |= (byte)(0x80u >> (i % 8));
                }
            }

            var reencoded = new byte[LdpcEncoder.CodewordBytes];
            LdpcEncoder.Encode(payload, reencoded);

            var mismatches = 0;
            for (var i = 0; i < N; i++)
            {
                if (((reencoded[i / 8] >> (7 - (i % 8))) & 1) != recovered[i])
                {
                    mismatches++;
                }
            }

            output.WriteLine(
                $"{what,-24} basis {basis.Length}, soft distance {result.SoftDistance,10:F3}, "
                + $"re-encodings {result.Reencodings}, bits disagreeing with a re-encode of its own "
                + $"first 91 {mismatches}");

            Assert.True(mismatches == 0, $"{what}: what came back is not a codeword.");
        }

        static float[] Filled(float value)
        {
            var ratios = new float[N];
            Array.Fill(ratios, value);
            return ratios;
        }

        static float[] Alternating()
        {
            var ratios = new float[N];
            for (var i = 0; i < N; i++)
            {
                ratios[i] = i % 2 == 0 ? float.MaxValue : float.MinValue;
            }

            return ratios;
        }
    }

    /// <summary>The refusals are caller mistakes, not bad signals, and they are loud.</summary>
    [Fact]
    public void TheWrongSizedSpanAndTheOutOfRangeOrderAreRefused()
    {
        var osd = new Ft8DeepOrderedStatistics();

        Assert.Throws<ArgumentException>(() => osd.Decode(new float[N - 1], 0, new byte[N]));
        Assert.Throws<ArgumentException>(() => osd.Decode(new float[N], 0, new byte[N - 1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => osd.Decode(new float[N], -1, new byte[N]));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => osd.Decode(new float[N], Ft8DeepOsdSettings.MaximumOrder + 1, new byte[N]));
    }
}

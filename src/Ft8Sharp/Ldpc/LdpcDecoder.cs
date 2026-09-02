using System;

namespace Ft8Sharp.Ldpc;

/// <summary>
/// The FT8 LDPC(174,91) belief-propagation decoder: given 174 log-likelihood ratios, one per
/// codeword bit, recovers the 174 bits and says how many parity checks they still fail.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/ldpc.c</c>, function <c>bp_decode</c></b>, in the pinned ft8_lib clone
/// at <see cref="Ft8Tables.UpstreamCommit"/>.
/// </para>
/// <para>
/// <b>UPSTREAM HAS TWO DECODERS AND THIS IS THE ONE ITS OWN DECODE PATH CALLS.</b>
/// <c>ft8/ldpc.h</c> declares both <c>bp_decode</c> and <c>ldpc_decode</c> with identical
/// signatures, and <c>ftx_decode_candidate</c> in <c>ft8/decode.c</c> calls <c>bp_decode</c>
/// with the call to the other commented out on the line below it. They are the same
/// sum-product algorithm; <c>ldpc_decode</c> carries two dense <c>[83][174]</c> float matrices
/// -- about 120 kB -- where this one carries only the 522 edges the Tanner graph actually has.
/// <c>ldpc_decode</c> was read and <b>deliberately not ported</b>; porting the one upstream does
/// not run would be porting something nothing has ever exercised.
/// </para>
/// <para>
/// <b>THE SIGN CONVENTION, WHICH IS THE ONE THING THAT CAN BE SELF-CONSISTENTLY WRONG.</b>
/// </para>
/// <para>
/// <b>A positive ratio means the bit is more likely 1. A negative ratio means 0.</b> That is
/// <c>log(P(bit = 1) / P(bit = 0))</c>, and it is the convention this decoder assumes, the
/// convention any extraction feeding it must produce, and the convention upstream actually
/// uses.
/// </para>
/// <para>
/// It was settled by reading, not by a round trip -- a decoder whose convention is backwards
/// round-trips perfectly against ratios its own tests generated and is stone deaf on the first
/// real signal. Three independent things in upstream's source agree:
/// </para>
/// <list type="number">
///   <item><description>
///     <b>Extraction.</b> <c>ft8_decode_multi_symbols</c> in <c>ft8/decode.c</c> says in its own
///     comment that it computes <c>log(p(1) / p(0))</c>, and its arithmetic is
///     <c>max_one - max_zero</c> -- larger when the tones carrying a 1 hold more energy.
///   </description></item>
///   <item><description>
///     <b>The hard decision.</b> Both of upstream's decoders write <c>(l &gt; 0) ? 1 : 0</c>.
///   </description></item>
///   <item><description>
///     <b>The check-node update.</b> <c>-2 · atanh(∏ tanh(-T/2))</c>. Writing <c>L</c> for
///     <c>log(P(0)/P(1))</c>, the sum-product check rule is <c>+2 atanh(∏ tanh(L/2))</c>;
///     substituting <c>L = -λ</c> for <c>λ = log(P(1)/P(0))</c> gives upstream's expression
///     exactly. <b>The two extra minus signs are the convention, not decoration.</b>
///   </description></item>
/// </list>
/// <para>
/// <b>And upstream's own <c>ft8/ldpc.c</c> opens by stating the opposite convention</b> --
/// "log-likelihood of zero", <c>codeword[i] = log(P(x=0) / P(x=1))</c>. That comment
/// contradicts all three readings above and the code was followed instead.
/// <c>UpstreamLdpcDecoderInventoryTests</c> asserts the wrong comment is still present, so a
/// re-pin that corrects it goes red rather than quietly removing the trap.
/// </para>
/// <para>
/// <b>This decoder is not scale-free and the ratios' magnitudes matter.</b> <c>tanh</c> and its
/// clamp are not homogeneous, so multiplying every ratio by a constant changes the answer.
/// Upstream rescales the whole array to a fixed variance in <c>ftx_normalize_logl</c> before
/// <c>bp_decode</c> sees it. <b>That normalisation is not ported here</b> -- it sits between
/// extraction and correction and belongs to extraction, which this library does not yet have.
/// A caller feeding this decoder ratios off a waterfall has to put them on upstream's scale.
/// </para>
/// <para>
/// <b>Nothing here depends on ambient state.</b> No clock, no random source, no environment, no
/// parallelism, no dictionary. Every buffer is allocated per call and every loop runs in index
/// order, so the same ratios give the same bits, the same iteration count and the same
/// unsatisfied-check count, in any order of calls, forever.
/// </para>
/// <para>
/// <b>And it takes ratios and nothing else.</b> No message, no expected payload, no truth of
/// any kind appears in the signature. A decoder with a truth parameter cannot be shown not to
/// have used it, so the parameter does not exist.
/// </para>
/// </remarks>
public static class LdpcDecoder
{
    /// <summary>
    /// How many iterations upstream's own application asks for.
    /// </summary>
    /// <remarks>
    /// <b>This is a weak anchor and is exposed rather than buried.</b> The number is
    /// <c>kLDPC_iterations</c>, a file-scope constant in <c>demo/decode_ft8.c</c>; it appears in
    /// no file under <c>ft8/</c> at all, so it is a choice upstream's <em>application</em> made
    /// and not a property of the code. <c>ftx_decode_candidate</c> takes it as a parameter, and
    /// so does <see cref="Decode"/>, with upstream's value as the default. A caller trading
    /// sensitivity for speed changes it; nothing in the library assumes it.
    /// </remarks>
    public const int DefaultMaxIterations = 25;

    /// <summary>The number of log-likelihood ratios a decode takes -- one per codeword bit.</summary>
    public const int RatioCount = Ft8Tables.LdpcN;

    /// <summary>The number of bits a decode returns.</summary>
    public const int CodewordBits = Ft8Tables.LdpcN;

    /// <summary>The number of check nodes each variable node takes part in.</summary>
    private const int VariableDegree = Ft8Tables.LdpcMnRowWidth;

    /// <summary>
    /// The one place upstream's index base is taken off, and the only one.
    /// </summary>
    /// <remarks>
    /// Unit 202 measured the base of both <c>Nm</c> and <c>Mn</c> as 1 rather than assuming it,
    /// and the ruling is that the tables stay exactly as upstream wrote them -- a renumbered
    /// table can no longer be compared against the source it came from. So the one comes off
    /// here, at the point of use, named, and nowhere else in this file.
    /// </remarks>
    private const int UpstreamIndexBase = 1;

    /// <summary>
    /// Recovers a codeword from its log-likelihood ratios.
    /// </summary>
    /// <param name="ratios">
    /// <see cref="RatioCount"/> ratios, one per codeword bit, in codeword bit order.
    /// <b>Positive means the bit is more likely 1.</b> A magnitude of zero is no information at
    /// all about that bit.
    /// </param>
    /// <param name="codewordBits">
    /// <see cref="CodewordBits"/> bytes, written in full, each 0 or 1. <b>Meaningless unless
    /// <see cref="LdpcDecodeResult.ParitySatisfied"/> is true</b> -- they are the closest thing
    /// the decoder reached, not a codeword.
    /// </param>
    /// <param name="maxIterations">
    /// How hard to try, defaulting to <see cref="DefaultMaxIterations"/>. Zero is legal and
    /// returns immediately with nothing decoded.
    /// </param>
    /// <exception cref="ArgumentException">
    /// If either span is the wrong length, or <paramref name="maxIterations"/> is negative.
    /// </exception>
    public static LdpcDecodeResult Decode(
        ReadOnlySpan<float> ratios,
        Span<byte> codewordBits,
        int maxIterations = DefaultMaxIterations)
    {
        if (ratios.Length != RatioCount)
        {
            throw new ArgumentException(
                $"The decoder takes exactly {RatioCount} log-likelihood ratios, one per codeword "
                + $"bit, and this call passed {ratios.Length}. A short array cannot be padded and a "
                + "long one cannot be trimmed: either would silently decode a different code.",
                nameof(ratios));
        }

        if (codewordBits.Length != CodewordBits)
        {
            throw new ArgumentException(
                $"The output buffer must be exactly {CodewordBits} bytes, one per codeword bit, "
                + $"and this one is {codewordBits.Length}.",
                nameof(codewordBits));
        }

        if (maxIterations < 0)
        {
            throw new ArgumentException(
                $"A maximum iteration count of {maxIterations} is not a number of iterations. Zero "
                + "is legal and means do not try; a negative count is a caller's arithmetic going "
                + "wrong somewhere above, and running it as though it were zero would hide that.",
                nameof(maxIterations));
        }

        // Upstream leaves its output array untouched when the loop body never runs, which in C is
        // whatever was on the stack. There is no faithful port of undefined content, so the buffer
        // is cleared: an all-zero answer with LdpcM checks unsatisfied is the honest report of
        // having decided nothing. Recorded as divergence 21 in porting-notes.md.
        codewordBits.Clear();

        // The edges of the Tanner graph, one message in each direction. Upstream's tov[N][3] and
        // toc[M][7], flattened. Allocated per call, which is what makes two calls independent.
        var toVariable = new float[Ft8Tables.LdpcN * VariableDegree];
        var toCheck = new float[Ft8Tables.LdpcM * Ft8Tables.LdpcNmRowWidth];

        var nm = Ft8Tables.LdpcNm;
        var mn = Ft8Tables.LdpcMn;
        var numRows = Ft8Tables.LdpcNumRows;

        // Upstream's min_errors: the running best across the iterations, starting at "every check
        // fails" so that any hard decision at all improves on it.
        var minErrors = Ft8Tables.LdpcM;
        var iterations = 0;

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            iterations = iteration + 1;

            // The hard decision, taken at the TOP of the iteration. On the first pass every
            // to-variable message is still zero, so this is the decision the raw ratios alone
            // would give -- which is why a clean codeword costs exactly one iteration.
            var plainSum = 0;
            for (var n = 0; n < Ft8Tables.LdpcN; n++)
            {
                var total = ratios[n]
                    + toVariable[(n * VariableDegree) + 0]
                    + toVariable[(n * VariableDegree) + 1]
                    + toVariable[(n * VariableDegree) + 2];

                var bit = (byte)(total > 0 ? 1 : 0);
                codewordBits[n] = bit;
                plainSum += bit;
            }

            if (plainSum == 0)
            {
                // The all-zero word satisfies every check of any linear code, so it would
                // otherwise be reported as a perfect decode. It carries no message, and upstream
                // refuses it in these words: "message converged to all-zeros, which is
                // prohibited". Note this leaves minErrors where it was -- the decode is refused,
                // not scored.
                break;
            }

            var errors = UnsatisfiedChecks(codewordBits, nm, numRows);

            if (errors < minErrors)
            {
                minErrors = errors;

                if (errors == 0)
                {
                    break;
                }
            }

            // Variable nodes to check nodes. Each message leaves out the check it is being sent
            // to, which is what stops a node's own belief being fed back to it as evidence.
            for (var m = 0; m < Ft8Tables.LdpcM; m++)
            {
                var checkRow = m * Ft8Tables.LdpcNmRowWidth;
                for (var nIndex = 0; nIndex < numRows[m]; nIndex++)
                {
                    var n = ZeroBased(nm[checkRow + nIndex]);
                    var variableRow = n * VariableDegree;

                    var tnm = ratios[n];
                    for (var mIndex = 0; mIndex < VariableDegree; mIndex++)
                    {
                        if (ZeroBased(mn[variableRow + mIndex]) != m)
                        {
                            tnm += toVariable[variableRow + mIndex];
                        }
                    }

                    toCheck[checkRow + nIndex] = FastTanh(-tnm / 2);
                }
            }

            // Check nodes to variable nodes, and the negations here are the sign convention.
            for (var n = 0; n < Ft8Tables.LdpcN; n++)
            {
                var variableRow = n * VariableDegree;
                for (var mIndex = 0; mIndex < VariableDegree; mIndex++)
                {
                    var m = ZeroBased(mn[variableRow + mIndex]);
                    var checkRow = m * Ft8Tables.LdpcNmRowWidth;

                    var tmn = 1.0f;
                    for (var nIndex = 0; nIndex < numRows[m]; nIndex++)
                    {
                        if (ZeroBased(nm[checkRow + nIndex]) != n)
                        {
                            tmn *= toCheck[checkRow + nIndex];
                        }
                    }

                    toVariable[variableRow + mIndex] = -2 * FastAtanh(tmn);
                }
            }
        }

        return new LdpcDecodeResult(minErrors, iterations);
    }

    /// <summary>Turns an index as upstream wrote it into a zero-based one.</summary>
    private static int ZeroBased(byte upstreamIndex) => upstreamIndex - UpstreamIndexBase;

    /// <summary>
    /// How many of the code's parity checks a set of hard-decided bits fails.
    /// </summary>
    /// <remarks>
    /// Upstream's <c>ldpc_check</c>. <b>The bound is <c>LdpcNumRows[m]</c> and not the row
    /// width</b>: 59 of the 581 slots in <c>LdpcNm</c> are padding, and a decoder that trusted
    /// the width would read each of them as variable index 0 and fold codeword bit 0 into 59
    /// checks it has nothing to do with.
    /// </remarks>
    private static int UnsatisfiedChecks(
        ReadOnlySpan<byte> codewordBits,
        ReadOnlySpan<byte> nm,
        ReadOnlySpan<byte> numRows)
    {
        var errors = 0;
        for (var m = 0; m < Ft8Tables.LdpcM; m++)
        {
            var row = m * Ft8Tables.LdpcNmRowWidth;
            var x = 0;
            for (var i = 0; i < numRows[m]; i++)
            {
                x ^= codewordBits[ZeroBased(nm[row + i])];
            }

            if (x != 0)
            {
                errors++;
            }
        }

        return errors;
    }

    /// <summary>
    /// Upstream's <c>fast_tanh</c> -- a rational approximation with a hard clamp, reproduced
    /// rather than improved on.
    /// </summary>
    /// <remarks>
    /// <b>Calling <see cref="MathF.Tanh"/> would be more accurate and would stop this being a
    /// port.</b> Unit 212 measured the same lesson on the transmit side: a phase step held in
    /// double precision was more accurate than upstream's single-precision one and disagreed
    /// with upstream by a hundred counts, where the faithful version agreed to one. Here the
    /// stakes are higher, because the approximation's error sits inside the loop that decides
    /// bits. The clamp at ±4.97 is load-bearing in its own right: it is what stops the
    /// saturated ±1 being handed to <see cref="FastAtanh"/>, whose denominator vanishes there.
    /// Inheriting Goba's arithmetic is the ruling in force, and step 6 is what would reveal a
    /// weakness in it.
    /// </remarks>
    private static float FastTanh(float x)
    {
        if (x < -4.97f)
        {
            return -1.0f;
        }

        if (x > 4.97f)
        {
            return 1.0f;
        }

        var x2 = x * x;
        var a = x * (945.0f + (x2 * (105.0f + x2)));
        var b = 945.0f + (x2 * (420.0f + (x2 * 15.0f)));
        return a / b;
    }

    /// <summary>Upstream's <c>fast_atanh</c>, on the same terms as <see cref="FastTanh"/>.</summary>
    private static float FastAtanh(float x)
    {
        var x2 = x * x;
        var a = x * (945.0f + (x2 * (-735.0f + (x2 * 64.0f))));
        var b = 945.0f + (x2 * (-1050.0f + (x2 * 225.0f)));
        return a / b;
    }
}

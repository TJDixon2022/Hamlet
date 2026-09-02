using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// <b>Unit 223's substituted arithmetic: upstream's belief propagation with its two rational
/// approximations replaced by the real functions, and nothing else moved.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS EXISTS TO PRICE UPSTREAM'S ARITHMETIC AND IT IS NOT A CANDIDATE FOR ADOPTION.</b> Karlis
/// Goba's <c>fast_tanh</c> and <c>fast_atanh</c> are what upstream calls and what this port calls,
/// faithfully, which unit 222 audited constant by constant and found identical term for term. The
/// plan's ruling that <em>inheriting Goba's bugs is accepted</em> is what licenses measuring the cost
/// of that arithmetic — that is exactly what <em>revealing an algorithmic weakness</em> means — and it
/// is equally what forbids taking the better one. <b>A row that decodes better is evidence about
/// where the loss is and is never a licence to adopt it.</b> Every line of this file is in the test
/// project and <b>nothing under <c>src/Ft8Sharp/</c> changes for it.</b>
/// </para>
/// <para>
/// <b>Exactly two terms move, and the copy is otherwise term for term.</b> The loop bound, the hard
/// decision at the top, the all-zero refusal, <c>ldpc_check</c> with its running minimum and its
/// break at zero, <c>min_errors</c> starting at <c>FTX_LDPC_M</c>, both message passes with their
/// exclusions, the parity row bound at <c>NUM_ROWS[m]</c> and the single-precision message arrays are
/// all <see cref="LdpcDecoder"/>'s own, transcribed. <b>What changes is which function computes the
/// hyperbolic tangent and its inverse.</b>
/// </para>
/// <para>
/// <b>Why the arithmetic is worth a row at all, stated before any of it ran.</b> Upstream's
/// <c>fast_tanh</c> is the <em>lowest order</em> of four rational approximations in its own source —
/// unit 222 found three higher-order ones commented out beside it — its clamp saturates at ±4.97, and
/// <c>fast_atanh</c> has no clamp and a denominator that does not vanish where the true function goes
/// to infinity. Both sit inside the loop that decides bits, and <b>every row of unit 222's loss
/// budget shared them</b>, so no row of that budget could see them.
/// </para>
/// </remarks>
internal static class Unit223Arithmetic
{
    /// <summary>Which pair of functions the check-node update runs through.</summary>
    internal enum Kind
    {
        /// <summary>Upstream's <c>fast_tanh</c> and <c>fast_atanh</c>, as the library calls them.</summary>
        Upstream,

        /// <summary><see cref="Math.Tanh"/> and <see cref="Math.Atanh"/> in double precision.</summary>
        Exact,
    }

    /// <summary>
    /// <b>The clamp on the argument to the exact <c>atanh</c>, and it is the machine's own limit
    /// rather than a number chosen by this unit.</b>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>Math.Atanh(1.0)</c> is positive infinity and <c>Math.Atanh</c> of anything past one is NaN,
    /// and a single NaN entering the message arrays poisons every bit of the codeword. In exact
    /// arithmetic the product of tanh values is strictly inside ±1 and no clamp would ever be needed;
    /// in floating point <c>tanh</c> of a large argument rounds to exactly ±1 and the product does
    /// reach the pole.
    /// </para>
    /// <para>
    /// <b>The clamp is <see cref="Math.BitDecrement"/> of one — the largest double strictly below
    /// one — and it is chosen that way on purpose.</b> A clamp is a threshold, and a threshold picked
    /// after seeing a rate is the failure this whole phase has spent units avoiding. This one cannot
    /// be tuned: it is the smallest step the type can take away from the pole, so it is the
    /// <em>most</em> generous clamp double precision admits and there is no value between it and one
    /// to move to. It admits messages up to about ±37.4, against upstream's <c>fast_atanh</c>, which
    /// cannot return more than about 2.28 whatever it is given.
    /// </para>
    /// </remarks>
    internal static readonly double AtanhClamp = Math.BitDecrement(1.0);

    /// <summary>The largest magnitude a to-variable message can carry under the exact arithmetic.</summary>
    internal static double ExactMessageCeiling => 2.0 * Math.Atanh(AtanhClamp);

    /// <summary>
    /// What the arithmetic did, gathered as it ran. <b>Counted rather than argued</b>: a clamp that
    /// never fires is not in play, and saying so needs a number.
    /// </summary>
    internal sealed class Census
    {
        internal long TanhCalls { get; private set; }

        /// <summary>How many of those landed on <c>fast_tanh</c>'s ±4.97 saturation.</summary>
        internal long TanhClamped { get; private set; }

        internal long AtanhCalls { get; private set; }

        /// <summary>How many of those were handed a product that had already reached ±1.</summary>
        internal long AtanhAtThePole { get; private set; }

        /// <summary>
        /// <b>The largest magnitude ever handed to the inverse function.</b> In exact arithmetic a
        /// product of hyperbolic tangents cannot leave <c>[-1, 1]</c>; <c>fast_tanh</c> is a rational
        /// approximation and <b>overshoots one just below its own clamp</b>, so the product can and
        /// does leave the range the inverse was fitted on. This is the number that says by how much.
        /// </summary>
        internal double LargestAtanhArgument { get; private set; }

        internal long Messages { get; private set; }

        internal double LargestMessage { get; private set; }

        private double _sumOfMagnitudes;

        internal double MeanMessage => Messages == 0 ? 0.0 : _sumOfMagnitudes / Messages;

        internal double ClampedFraction => TanhCalls == 0 ? 0.0 : (double)TanhClamped / TanhCalls;

        internal void Tanh(float argument)
        {
            TanhCalls++;
            if (argument < -4.97f || argument > 4.97f)
            {
                TanhClamped++;
            }
        }

        internal void Atanh(float product)
        {
            AtanhCalls++;
            var magnitude = Math.Abs((double)product);
            if (magnitude >= 1.0)
            {
                AtanhAtThePole++;
            }

            if (magnitude > LargestAtanhArgument)
            {
                LargestAtanhArgument = magnitude;
            }
        }

        internal void Message(float value)
        {
            Messages++;
            var magnitude = Math.Abs((double)value);
            _sumOfMagnitudes += magnitude;
            if (magnitude > LargestMessage)
            {
                LargestMessage = magnitude;
            }
        }
    }

    /// <summary>What one decode reached. <see cref="LdpcDecodeResult"/>'s shape, built here because
    /// its constructor is the library's own.</summary>
    internal readonly record struct Outcome(int UnsatisfiedChecks, int Iterations)
    {
        internal bool ParitySatisfied => UnsatisfiedChecks == 0;
    }

    private const int VariableDegree = Ft8Tables.LdpcMnRowWidth;

    private const int UpstreamIndexBase = 1;

    /// <summary>
    /// <b><see cref="LdpcDecoder.Decode"/> transcribed, with the two named functions swapped and
    /// nothing else.</b>
    /// </summary>
    internal static Outcome Decode(
        ReadOnlySpan<float> ratios,
        Span<byte> codewordBits,
        int maxIterations,
        Kind kind,
        Census? census = null)
    {
        codewordBits.Clear();

        var toVariable = new float[Ft8Tables.LdpcN * VariableDegree];
        var toCheck = new float[Ft8Tables.LdpcM * Ft8Tables.LdpcNmRowWidth];

        var nm = Ft8Tables.LdpcNm;
        var mn = Ft8Tables.LdpcMn;
        var numRows = Ft8Tables.LdpcNumRows;

        var minErrors = Ft8Tables.LdpcM;
        var iterations = 0;

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            iterations = iteration + 1;

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

                    // ---- SUBSTITUTED TERM 1 OF 2 ----
                    var argument = -tnm / 2;
                    census?.Tanh(argument);
                    toCheck[checkRow + nIndex] = kind == Kind.Upstream
                        ? FastTanh(argument)
                        : (float)Math.Tanh(argument);
                }
            }

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

                    // ---- SUBSTITUTED TERM 2 OF 2 ----
                    census?.Atanh(tmn);
                    var message = kind == Kind.Upstream
                        ? -2 * FastAtanh(tmn)
                        : (float)(-2.0 * Math.Atanh(Math.Clamp((double)tmn, -AtanhClamp, AtanhClamp)));

                    toVariable[variableRow + mIndex] = message;
                    census?.Message(message);
                }
            }
        }

        return new Outcome(minErrors, iterations);
    }

    /// <summary>
    /// <b><c>Ft8CodewordDecoder.Decode</c>'s two gates, transcribed, over a decoder the caller
    /// chooses.</b> Parity, then the checksum, then the message — the same order, the same refusals.
    /// </summary>
    /// <remarks>
    /// <b>Written out rather than reached into.</b> <c>Ft8CodewordDecoder</c> has no seam for a
    /// substituted correction stage, and adding one would be a library change made to serve a
    /// measurement — which is exactly what task 6's two conditions exist to prevent. So the gate is
    /// transcribed here and the library is left alone, the same choice unit 222 made for the slot
    /// loop.
    /// </remarks>
    internal static Gated Gate(
        ReadOnlySpan<byte> codewordBits,
        Ft8CallsignCache? cache,
        Outcome correction)
    {
        if (!correction.ParitySatisfied)
        {
            return new Gated(false, false, false, string.Empty);
        }

        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        Pack(codewordBits[..Ft8Payload.PayloadBits], payload);

        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];
        if (!Ft8Payload.TryRead(payload, message))
        {
            return new Gated(true, false, false, string.Empty);
        }

        var decoded = Ft8MessageDecoder.Decode(message, cache);
        return decoded.Decoded
            ? new Gated(true, true, true, decoded.Text)
            : new Gated(true, true, false, string.Empty);
    }

    /// <summary>Which gate a set of ratios reached, and the text if it reached the end.</summary>
    internal readonly record struct Gated(bool Parity, bool Checksum, bool Readable, string Text);

    /// <summary>Upstream's <c>pack_bits</c>, most significant bit of each byte first.</summary>
    private static void Pack(ReadOnlySpan<byte> bits, Span<byte> packed)
    {
        packed.Clear();
        for (var i = 0; i < bits.Length; i++)
        {
            if (bits[i] != 0)
            {
                packed[i / 8] |= (byte)(0x80u >> (i % 8));
            }
        }
    }

    private static int ZeroBased(byte upstreamIndex) => upstreamIndex - UpstreamIndexBase;

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
    /// <b>Upstream's <c>fast_tanh</c>, re-declared here so the error table can be printed.</b> The
    /// constants are the pin's, in the pin's order — <c>-4.97 -1 4.97 1 945 105 945 420 15</c> — and
    /// <c>Unit222LdpcAuditTests</c> asserts that the library's copy holds the same nine in the same
    /// order against the pin itself.
    /// </summary>
    internal static float FastTanh(float x)
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

    /// <summary>
    /// <b>Upstream's <c>fast_atanh</c>, on the same terms.</b> Constants <c>945 -735 64 945 -1050
    /// 225</c>, in the pin's order. <b>There is no clamp</b>, and the denominator does not vanish
    /// anywhere on <c>[-1, 1]</c>, so the function returns a small finite number exactly where the
    /// true one goes to infinity.
    /// </summary>
    internal static float FastAtanh(float x)
    {
        var x2 = x * x;
        var a = x * (945.0f + (x2 * (-735.0f + (x2 * 64.0f))));
        var b = 945.0f + (x2 * (-1050.0f + (x2 * 225.0f)));
        return a / b;
    }
}

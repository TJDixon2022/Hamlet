using Ft8Sharp;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;

namespace Ft8Sharp.Tests.Ldpc;

/// <summary>
/// Turns a message into the codeword it encodes to, and that codeword into the log-likelihood
/// ratios a decoder is handed -- with whatever damage a test wants to do on the way.
/// </summary>
/// <remarks>
/// <para>
/// <b>The truth here is exact, and that is the whole point of taking this half of step 5 on its
/// own.</b> Every codeword this file produces comes out of <see cref="LdpcEncoder"/>, whose
/// parity was proved for <em>every</em> payload by linearity over all 91 basis payloads in unit
/// 209's <c>BasisProof</c>, and whose syndrome is computed independently in this project by
/// <see cref="LdpcCheck"/>, which reads the check tables and never calls the library. So a
/// recovery claim tonight is a claim about the decoder and not about a chain of agreements.
/// </para>
/// <para>
/// <b>Nothing here touches an audio sample, a waterfall or a candidate.</b> These ratios are
/// constructed from a known codeword and from nowhere else. The real thing -- reading
/// magnitudes out of the waterfall at a candidate's position and demapping three bits per
/// symbol through the Gray code -- is the next unit's whole night, and none of it exists yet.
/// </para>
/// <para>
/// <b>THE SIGN CONVENTION IS UPSTREAM'S, READ FROM UPSTREAM, AND IT IS NOT ASSUMED HERE.</b>
/// A confident 1 becomes a positive ratio and a confident 0 a negative one, because
/// <c>ft8/decode.c</c> computes <c>log(p(1)/p(0))</c> as <c>max_one - max_zero</c>. <b>That this
/// file and the decoder agree proves nothing about the convention</b> -- a pair with the
/// convention backwards agrees with itself perfectly. The evidence is the reading in
/// <c>UpstreamLdpcDecoderInventoryTests</c>, and the refusal in
/// <c>Ft8LdpcDecoderDeterminismTests</c> where every ratio is negated.
/// </para>
/// </remarks>
internal static class SoftCodeword
{
    /// <summary>
    /// The magnitude a bit the receiver is sure of arrives with, on upstream's own scale.
    /// </summary>
    /// <remarks>
    /// <b>Not a round number chosen for looking tidy.</b> <c>ftx_normalize_logl</c> in
    /// <c>ft8/decode.c</c> rescales the 174 ratios so their variance is 24 before
    /// <c>bp_decode</c> ever sees them. An array of ±A about a mean of roughly zero has variance
    /// A², so A = √24 ≈ 4.899 is the magnitude at which a perfectly confident array is already
    /// on the scale upstream's decoder expects. <b>This matters because the decoder is not
    /// scale-free</b> -- <c>fast_tanh</c> saturates at ±4.97 -- so a magnitude picked for
    /// convenience would be measuring a decoder nobody will ever run.
    /// </remarks>
    public static readonly float ConfidentMagnitude = MathF.Sqrt(24.0f);

    /// <summary>The 174 codeword bits a 77-bit message encodes to, one byte per bit.</summary>
    public static byte[] CodewordBitsFor(ReadOnlySpan<byte> message)
    {
        var payload = new byte[Ft8Payload.PayloadBytes];
        Ft8Payload.Create(message, payload);

        var codeword = new byte[LdpcEncoder.CodewordBytes];
        LdpcEncoder.Encode(payload, codeword);

        return LdpcCheck.UnpackMsbFirst(codeword, Ft8Tables.LdpcN);
    }

    /// <summary>
    /// The ratios a receiver perfectly certain of every bit would produce: positive for a 1,
    /// negative for a 0, all at <see cref="ConfidentMagnitude"/>.
    /// </summary>
    public static float[] RatiosFor(ReadOnlySpan<byte> codewordBits) =>
        RatiosFor(codewordBits, ConfidentMagnitude);

    /// <summary>The same, at a stated magnitude.</summary>
    public static float[] RatiosFor(ReadOnlySpan<byte> codewordBits, float magnitude)
    {
        var ratios = new float[Ft8Tables.LdpcN];
        for (var i = 0; i < ratios.Length; i++)
        {
            ratios[i] = codewordBits[i] != 0 ? magnitude : -magnitude;
        }

        return ratios;
    }

    /// <summary>
    /// Flips <paramref name="k"/> distinct bit positions of a ratio array, chosen by a seeded
    /// generator, and returns which positions were chosen.
    /// </summary>
    /// <remarks>
    /// <b>A flip is a sign change and not a zeroing.</b> The receiver is left just as certain as
    /// it was and certain of the wrong thing, which is the crisp instrument: <c>k</c> errors,
    /// each of them maximally confident, and no ambiguity about how much damage was done. The
    /// selection is a partial Fisher-Yates over the 174 positions, so the positions are distinct
    /// and the draw is uniform without a rejection loop whose cost grows with <c>k</c>.
    /// </remarks>
    public static int[] FlipDistinctPositions(Span<float> ratios, int k, Random random)
    {
        var order = new int[ratios.Length];
        for (var i = 0; i < order.Length; i++)
        {
            order[i] = i;
        }

        var chosen = new int[k];
        for (var i = 0; i < k; i++)
        {
            var j = random.Next(i, order.Length);
            (order[i], order[j]) = (order[j], order[i]);
            chosen[i] = order[i];
            ratios[order[i]] = -ratios[order[i]];
        }

        return chosen;
    }

    /// <summary>
    /// The 77-bit message a decoded codeword carries, or <see langword="null"/> where the CRC
    /// does not check out.
    /// </summary>
    /// <remarks>
    /// <b>This is the test project's own reading of a decoded codeword</b>, used where a test
    /// wants to know what came back without going through the library's gate. It composes the
    /// library's proven <see cref="Ft8Payload.TryRead"/>, which is where the CRC check already
    /// lives; nothing here re-implements one.
    /// </remarks>
    public static byte[]? MessageFrom(ReadOnlySpan<byte> codewordBits)
    {
        var payload = LdpcCheck.PackMsbFirst(codewordBits[..LdpcEncoder.PayloadBits]);
        var message = new byte[Ft8Payload.MessageBytes];
        return Ft8Payload.TryRead(payload, message) ? message : null;
    }
}

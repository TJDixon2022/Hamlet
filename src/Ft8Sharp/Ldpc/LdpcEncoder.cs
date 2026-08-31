using System;

namespace Ft8Sharp.Ldpc;

/// <summary>
/// The FT8 LDPC(174,91) generator-matrix multiply: given a 91-bit payload, produces the
/// 83 parity bits and the 174-bit codeword that carries them.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/encode.c</c>, function <c>encode174</c></b>, in the pinned
/// ft8_lib clone at <see cref="Ft8Tables.UpstreamCommit"/>. That function is followed and
/// nothing around it is: upstream's <c>ft8_encode</c> also adds a CRC and maps the
/// codeword onto tones, and both of those are later steps' work. This type computes
/// parity and stops.
/// </para>
/// <para>
/// <b>This is not a decoder and cannot become one by accident.</b> It runs one direction
/// only -- payload to codeword -- and corrects nothing.
/// </para>
/// <para>
/// <b>The layout, all four parts of it measured rather than assumed</b>, by
/// <c>Ft8LdpcLayoutTests</c> in the test project, which tries each alternative reading and
/// shows it failing against the reference parity tables:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>Bit order is most-significant-first</b>, within both the payload bytes and the
///     generator rows. Bit <c>k</c> of the payload sits at <c>1 &lt;&lt; (7 - k % 8)</c> of
///     byte <c>k / 8</c>. The other order fails every one of the 83 checks.
///   </description></item>
///   <item><description>
///     <b>The codeword carries the message first and the parity appended</b>: payload bits
///     occupy codeword positions 0 to 90 and parity occupies 91 to 173. The other reading,
///     parity first, fails.
///   </description></item>
///   <item><description>
///     <b><see cref="Ft8Tables.LdpcKBytes"/> is 12 bytes -- 96 bits -- for a 91-bit
///     payload</b>, so bits 91 to 95 of every payload and of every generator row are
///     spare. They are zero in the checked-in tables, measured, and this encoder requires
///     the payload's to be zero too rather than quietly ignoring them.
///   </description></item>
///   <item><description>
///     <b>Index bases are upstream's and nothing here renumbers a table.</b> The generator
///     is a bit matrix and has no indices to rebase; the 1-based indices in
///     <c>LdpcNm</c> and <c>LdpcMn</c> belong to the parity checker, not to this file.
///   </description></item>
/// </list>
/// </remarks>
public static class LdpcEncoder
{
    /// <summary>The payload buffer size in bytes -- 12, holding 91 used bits and 5 spare.</summary>
    public const int PayloadBytes = Ft8Tables.LdpcKBytes;

    /// <summary>The number of payload bits the code actually carries.</summary>
    public const int PayloadBits = Ft8Tables.LdpcN - Ft8Tables.LdpcM;

    /// <summary>The codeword buffer size in bytes -- 22, holding 174 used bits and 2 spare.</summary>
    public const int CodewordBytes = (Ft8Tables.LdpcN + 7) / 8;

    /// <summary>
    /// Encodes a payload through the checked-in generator table.
    /// </summary>
    /// <param name="payload">
    /// <see cref="PayloadBytes"/> bytes holding <see cref="PayloadBits"/> bits, most
    /// significant bit first. The trailing spare bits must be zero.
    /// </param>
    /// <param name="codeword">
    /// <see cref="CodewordBytes"/> bytes, written in full. On return it holds the payload
    /// in bits 0 to <see cref="PayloadBits"/>-1 and the parity after it, most significant
    /// bit first, with the buffer's own trailing spare bits zero.
    /// </param>
    public static void Encode(ReadOnlySpan<byte> payload, Span<byte> codeword) =>
        Encode(Ft8Tables.LdpcGenerator, payload, codeword);

    /// <summary>
    /// Encodes a payload through a caller-supplied generator table.
    /// </summary>
    /// <remarks>
    /// <b>This overload exists so the parity proof can be watched refusing.</b> A syndrome
    /// check that has never rejected a corrupted table says nothing about an uncorrupted
    /// one, and the only honest way to corrupt one is on a copy -- the checked-in
    /// generated file is never touched. Production callers want <see cref="Encode(ReadOnlySpan{byte}, Span{byte})"/>.
    /// </remarks>
    /// <param name="generator">
    /// <see cref="Ft8Tables.LdpcM"/> rows of <see cref="PayloadBytes"/> bytes, flattened
    /// row-major, in the same shape and bit order as <see cref="Ft8Tables.LdpcGenerator"/>.
    /// </param>
    /// <param name="payload">As <see cref="Encode(ReadOnlySpan{byte}, Span{byte})"/>.</param>
    /// <param name="codeword">As <see cref="Encode(ReadOnlySpan{byte}, Span{byte})"/>.</param>
    public static void Encode(ReadOnlySpan<byte> generator, ReadOnlySpan<byte> payload, Span<byte> codeword)
    {
        if (generator.Length != Ft8Tables.LdpcM * PayloadBytes)
        {
            throw new ArgumentException(
                $"The generator must be {Ft8Tables.LdpcM} rows of {PayloadBytes} bytes, "
                + $"{Ft8Tables.LdpcM * PayloadBytes} in all, and this one is {generator.Length}.",
                nameof(generator));
        }

        if (payload.Length != PayloadBytes)
        {
            throw new ArgumentException(
                $"The payload must be {PayloadBytes} bytes, and this one is {payload.Length}.",
                nameof(payload));
        }

        if (codeword.Length != CodewordBytes)
        {
            throw new ArgumentException(
                $"The codeword buffer must be {CodewordBytes} bytes, and this one is {codeword.Length}.",
                nameof(codeword));
        }

        // The five bits past the 91st are not part of the code. Upstream's own encoder
        // ANDs across all 12 bytes and would silently fold them into the parity if a
        // caller set them; refusing is the reading of the prime directive that applies
        // here, because the resulting codeword would look perfectly well formed.
        var spare = payload[PayloadBytes - 1] & SpareMask;
        if (spare != 0)
        {
            throw new ArgumentException(
                $"The payload's last {(PayloadBytes * 8) - PayloadBits} bits are spare and must be "
                + "zero. They are outside the code, and parity computed with them set would be "
                + "wrong in a way nothing downstream could see.",
                nameof(payload));
        }

        // Message first, zeros after: only the one bits of the parity get written below.
        codeword.Clear();
        payload.CopyTo(codeword[..PayloadBytes]);

        // Where parity bit 0 lands: bit PayloadBits of the codeword, counted MSB-first.
        var parityByte = PayloadBits / 8;
        var parityMask = (byte)(0x80u >> (PayloadBits % 8));

        for (var check = 0; check < Ft8Tables.LdpcM; check++)
        {
            var row = generator.Slice(check * PayloadBytes, PayloadBytes);

            // The dot product of the payload with this generator row, modulo 2. AND
            // selects the payload bits the row names; the parity of the whole selection
            // is their sum over GF(2).
            var sum = 0;
            for (var j = 0; j < PayloadBytes; j++)
            {
                sum ^= Parity8((byte)(payload[j] & row[j]));
            }

            if (sum != 0)
            {
                codeword[parityByte] |= parityMask;
            }

            parityMask >>= 1;
            if (parityMask == 0)
            {
                parityMask = 0x80;
                parityByte++;
            }
        }
    }

    /// <summary>The bits of the payload's last byte that lie past <see cref="PayloadBits"/>.</summary>
    private const int SpareMask = (1 << ((PayloadBytes * 8) - PayloadBits)) - 1;

    /// <summary>1 if an odd number of bits are set, 0 otherwise.</summary>
    private static int Parity8(byte x)
    {
        x ^= (byte)(x >> 4);
        x ^= (byte)(x >> 2);
        x ^= (byte)(x >> 1);
        return x & 1;
    }
}

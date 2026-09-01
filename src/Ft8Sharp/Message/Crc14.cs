using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The 14-bit CRC that FT8 and FT4 put on every message: modulo-2 division of a bit sequence
/// by the protocol's polynomial, most significant bit first.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/crc.c</c>, function <c>ftx_compute_crc</c></b>, in the pinned ft8_lib
/// clone at <see cref="Ft8Tables.UpstreamCommit"/>. Upstream's own comment credits the shape of
/// the loop to a published note on CRC calculation in C; the two scalars it divides by are
/// <c>ft8/constants.h</c>'s and are asserted against that file at run time by
/// <c>UpstreamCrcProvenanceTests</c> in the test project. This is a transcription with a machine
/// checking it, not a value somebody typed and hoped for.
/// </para>
/// <para>
/// <b>What was deliberately left behind.</b> Upstream keeps the two message-layout helpers,
/// <c>ftx_add_crc</c> and <c>ftx_extract_crc</c>, in the same file. They are not checksum
/// arithmetic — they are facts about where the 14 bits sit inside a 91-bit payload — so they are
/// ported into <see cref="Ft8Payload"/>, which is the type that owns that layout. This file
/// computes a remainder and does nothing else.
/// </para>
/// <para>
/// <b>What was measured rather than assumed.</b> Three things, each of which changes the answer
/// if it is read the other way, and each of which the test project settles against the pin:
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>The initial remainder is zero and there is no final XOR</b> — only a mask down to
///     <see cref="Width"/> bits. That is what makes the map linear over GF(2), which is what lets
///     77 basis computations stand for all 2^77 messages instead of a corpus standing for a
///     sample of them. Read out of the ported function, not assumed from the usual shape of a CRC.
///   </description></item>
///   <item><description>
///     <b>The message is consumed most significant bit first</b>, a whole byte brought into the
///     remainder at every eighth bit and shifted up by <see cref="Width"/> minus 8. The count of
///     bytes read is therefore <c>ceil(bitCount / 8)</c>, and a caller passing a bit count that
///     runs past its buffer is refused rather than read past.
///   </description></item>
///   <item><description>
///     <b>Upstream's accumulator is a <c>uint16_t</c> and is not masked inside the loop</b>, so
///     bits above <see cref="Width"/> accumulate in it and are shifted out of the top. They can
///     never reach the bit the division tests, and the final mask discards them, so the result is
///     unaffected — but the arithmetic here is done in 16 bits explicitly rather than in C#'s
///     wider <c>int</c>, because "it cannot matter" is a thing worth being wrong about only once.
///   </description></item>
/// </list>
/// <para>
/// <b>Bugs are inherited deliberately.</b> Where this differs from upstream it is wrong, whatever
/// a standard says, because a checksum that disagrees with every other station on the band is not
/// a better checksum.
/// </para>
/// </remarks>
public static class Crc14
{
    /// <summary>The number of bits the checksum carries.</summary>
    /// <remarks>
    /// Matched against <c>ft8/constants.h</c> at run time. The value is here because the port
    /// needs it; it does not appear in any report, commit message or note, by ruling.
    /// </remarks>
    public const int Width = 14;

    /// <summary>
    /// The generator polynomial, without its leading term, as upstream spells it.
    /// </summary>
    /// <remarks>Matched against <c>ft8/constants.h</c> at run time, exactly as <see cref="Width"/> is.</remarks>
    public const ushort Polynomial = 0x2757;

    /// <summary>The bit the division tests: the top bit of a <see cref="Width"/>-bit remainder.</summary>
    private const uint TopBit = 1u << (Width - 1);

    /// <summary>Everything the result may carry — <see cref="Width"/> bits and no more.</summary>
    public const ushort Mask = (ushort)((TopBit << 1) - 1u);

    /// <summary>
    /// Computes the checksum of the first <paramref name="bitCount"/> bits of
    /// <paramref name="message"/>, counted most significant bit first from byte zero.
    /// </summary>
    /// <param name="message">
    /// The bit sequence, packed most significant bit first. Bits past
    /// <paramref name="bitCount"/> in the last byte read are consumed by upstream's loop and by
    /// this one — a caller who wants them ignored zeroes them, which is what
    /// <see cref="Ft8Payload"/> does.
    /// </param>
    /// <param name="bitCount">How many bits to divide. Zero is legal and gives zero.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bitCount"/> is negative.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="message"/> is shorter than the <paramref name="bitCount"/> bits it is
    /// asked for. Upstream reads past the end of the buffer in that case; refusing is the reading
    /// of the prime directive that applies, because the checksum that came back would look
    /// perfectly well formed.
    /// </exception>
    public static ushort Compute(ReadOnlySpan<byte> message, int bitCount)
    {
        if (bitCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bitCount), bitCount, "A bit count cannot be negative.");
        }

        var bytesNeeded = (bitCount + 7) / 8;
        if (message.Length < bytesNeeded)
        {
            throw new ArgumentException(
                $"{bitCount} bits need {bytesNeeded} bytes and the message is {message.Length}.",
                nameof(message));
        }

        // uint16_t upstream. Held in a uint and masked at every step so the C truncation is
        // reproduced rather than relied upon not to matter.
        uint remainder = 0;
        var byteIndex = 0;

        for (var bit = 0; bit < bitCount; bit++)
        {
            if (bit % 8 == 0)
            {
                remainder = (remainder ^ ((uint)message[byteIndex] << (Width - 8))) & 0xFFFFu;
                byteIndex++;
            }

            remainder = (remainder & TopBit) != 0
                ? ((remainder << 1) ^ Polynomial) & 0xFFFFu
                : (remainder << 1) & 0xFFFFu;
        }

        return (ushort)(remainder & Mask);
    }
}

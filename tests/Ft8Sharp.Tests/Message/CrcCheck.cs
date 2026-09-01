using Ft8Sharp.Message;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// A second opinion on the CRC, arrived at a different way, so the port is not agreeing with
/// itself.
/// </summary>
/// <remarks>
/// <para>
/// <b>Nothing here calls <see cref="Crc14"/>, and that is the whole point.</b> A checksum tested
/// by another copy of its own loop proves that the copy was faithful and nothing else.
/// <c>LdpcCheck</c> is the pattern — it checks the encoder against a different description of the
/// same code — and this does the same thing one layer down.
/// </para>
/// <para>
/// <b>Where the independence actually lives.</b> The library divides bit by bit, walking the
/// message a bit at a time and bringing a byte into the register at every eighth. This computes
/// the same quantity as arithmetic on the message polynomial instead: a 256-entry table built
/// once from the polynomial consumes the message a whole byte per step, giving the message
/// multiplied by <c>x^14</c> modulo the generator, and then the count of bits the last byte
/// carried beyond the message is divided back out by repeated multiplication by the inverse of
/// <c>x</c>. The two share the polynomial and the width, because those are the protocol; they
/// share no arithmetic and they do not even run in the same direction.
/// </para>
/// <para>
/// <b>Why the tail needs the inverse at all.</b> FT8 checksums 82 bits — ten whole bytes and two
/// — and upstream's loop brings the eleventh byte into the register whole and then stops the
/// division two steps in. Expressed as polynomial arithmetic that is six multiplications by
/// <c>x</c> too many, so six are taken back off. That is the step most likely to be got wrong in
/// either implementation, which is why the corpus is run across every bit length the protocol
/// uses rather than only the one it needs.
/// </para>
/// </remarks>
internal static class CrcCheck
{
    /// <summary>The polynomial and the width are the protocol's, and are the only things shared.</summary>
    private const int Width = Crc14.Width;

    private const ushort Polynomial = Crc14.Polynomial;

    private const ushort Mask = Crc14.Mask;

    /// <summary>The top bit of a <see cref="Width"/>-bit register.</summary>
    private const uint TopBit = 1u << (Width - 1);

    /// <summary>
    /// For each of the 256 possible byte values, that byte multiplied by <c>x^14</c> modulo the
    /// generator. Built once, from the polynomial, and never transcribed.
    /// </summary>
    private static readonly ushort[] Table = BuildTable();

    private static ushort[] BuildTable()
    {
        var table = new ushort[256];
        for (var b = 0; b < 256; b++)
        {
            var remainder = (uint)b << (Width - 8);
            for (var i = 0; i < 8; i++)
            {
                remainder = (remainder & TopBit) != 0
                    ? ((remainder << 1) ^ Polynomial) & Mask
                    : (remainder << 1) & Mask;
            }

            table[b] = (ushort)remainder;
        }

        return table;
    }

    /// <summary>
    /// The checksum of the first <paramref name="bitCount"/> bits of <paramref name="message"/>,
    /// most significant bit first.
    /// </summary>
    public static ushort Compute(ReadOnlySpan<byte> message, int bitCount)
    {
        var bytesRead = (bitCount + 7) / 8;
        var overshoot = (bytesRead * 8) - bitCount;

        // The message, whole bytes at a time, multiplied by x^14 modulo the generator.
        uint remainder = 0;
        for (var i = 0; i < bytesRead; i++)
        {
            var index = (byte)(((remainder >> (Width - 8)) ^ message[i]) & 0xFFu);
            remainder = (uint)(Table[index] ^ ((remainder << 8) & Mask));
        }

        // The last byte entered whole, so it was multiplied by x once for every bit of it the
        // message did not have. Take those back off.
        for (var i = 0; i < overshoot; i++)
        {
            remainder = (remainder & 1u) != 0
                ? (((remainder ^ Polynomial) >> 1) | TopBit)
                : (remainder >> 1);
        }

        return (ushort)(remainder & Mask);
    }
}

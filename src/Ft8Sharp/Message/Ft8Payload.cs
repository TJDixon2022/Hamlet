using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The container every FT8 message travels in: 77 message bits, a 14-bit CRC after them, and the
/// 12-byte buffer the LDPC encoder takes.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/crc.c</c>, functions <c>ftx_add_crc</c> and <c>ftx_extract_crc</c></b>,
/// in the pinned ft8_lib clone at <see cref="Ft8Tables.UpstreamCommit"/>. Those two are not
/// checksum arithmetic — they are facts about where 14 bits sit inside 91 — so they live here
/// rather than in <see cref="Crc14"/>, which computes a remainder and nothing else.
/// </para>
/// <para>
/// <b>What this does not do, and it is most of the message layer.</b> No packing and no unpacking
/// of any message type; no callsigns, no grids, no reports, no free text, no telemetry, and no
/// callsign hashing. This type never looks at what the 77 bits mean. It is the envelope, and it
/// exists first so that when a packer arrives, a round trip that fails is the packer's fault and
/// not the plumbing's.
/// </para>
/// <para>
/// <b>The layout, measured from the pinned source rather than assumed:</b>
/// </para>
/// <list type="bullet">
///   <item><description>
///     <b>The checksum covers <see cref="CrcBitCount"/> bits, not <see cref="MessageBits"/>.</b>
///     Upstream zero-extends the 77 message bits to 82 and checksums that, and its own comment
///     quotes the protocol description saying so. Getting this wrong produces a CRC that is
///     perfectly self-consistent and wrong against every other station on the band.
///   </description></item>
///   <item><description>
///     <b>The checksum sits at bits 77 to 90, most significant bit first</b>, straddling three
///     bytes. That is read out of upstream's shifts rather than inferred from the bit count.
///   </description></item>
///   <item><description>
///     <b>Bits 91 to 95 are spare and are always zero in a payload this produces</b>, which is
///     what keeps <c>LdpcEncoder.Encode</c> from refusing one. It falls out of the layout — the
///     last checksum bit lands at bit 90 and nothing writes below it — and it is asserted
///     directly by the tests rather than inferred from the encode succeeding.
///   </description></item>
/// </list>
/// <para>
/// <b>One deliberate departure from upstream, and it is a refusal rather than a different
/// answer.</b> <c>ftx_add_crc</c> silently clears the three bits between the end of the message
/// and the start of the checksum. This refuses a message that has them set instead, for the same
/// reason <c>LdpcEncoder</c> refuses a payload with its spare bits set: a caller who put something
/// there meant something by it, and quietly dropping it produces a payload that looks perfectly
/// well formed. No message this refuses would have been encoded differently — it would have been
/// encoded as though the bits had never been set.
/// </para>
/// </remarks>
public static class Ft8Payload
{
    /// <summary>The number of message bits an FT8 payload carries, before the checksum.</summary>
    public const int MessageBits = 77;

    /// <summary>The number of bytes a packed message occupies — 77 bits in 10 bytes, 3 spare.</summary>
    public const int MessageBytes = (MessageBits + 7) / 8;

    /// <summary>The number of checksum bits appended to the message.</summary>
    public const int CrcBits = Crc14.Width;

    /// <summary>Message plus checksum: the payload the LDPC code carries.</summary>
    public const int PayloadBits = MessageBits + CrcBits;

    /// <summary>The buffer size the LDPC encoder takes — 12 bytes, 91 used bits and 5 spare.</summary>
    public const int PayloadBytes = 12;

    /// <summary>
    /// The number of bits the checksum is computed over: the message zero-extended past its own
    /// length, as the protocol specifies and as upstream implements.
    /// </summary>
    public const int CrcBitCount = (PayloadBytes * 8) - CrcBits;

    /// <summary>The bits of the payload's last byte that lie past <see cref="PayloadBits"/>.</summary>
    private const int SpareMask = (1 << ((PayloadBytes * 8) - PayloadBits)) - 1;

    /// <summary>The bits of the message's last byte that lie past <see cref="MessageBits"/>.</summary>
    private const int MessageSpareMask = (1 << ((MessageBytes * 8) - MessageBits)) - 1;

    /// <summary>
    /// Builds a payload from a packed 77-bit message: copies the message, checksums it, and writes
    /// the checksum after it.
    /// </summary>
    /// <param name="message">
    /// <see cref="MessageBytes"/> bytes holding <see cref="MessageBits"/> bits, most significant
    /// bit first. The three bits past the last message bit must be zero.
    /// </param>
    /// <param name="payload">
    /// <see cref="PayloadBytes"/> bytes, written in full: the message, the checksum, and zero in
    /// the spare bits.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Either span is the wrong length, or the message has bits set past its <see cref="MessageBits"/>th.
    /// </exception>
    public static void Create(ReadOnlySpan<byte> message, Span<byte> payload)
    {
        if (message.Length != MessageBytes)
        {
            throw new ArgumentException(
                $"A message is {MessageBytes} bytes and this one is {message.Length}.", nameof(message));
        }

        if (payload.Length != PayloadBytes)
        {
            throw new ArgumentException(
                $"A payload is {PayloadBytes} bytes and this one is {payload.Length}.", nameof(payload));
        }

        if ((message[MessageBytes - 1] & MessageSpareMask) != 0)
        {
            throw new ArgumentException(
                $"The {(MessageBytes * 8) - MessageBits} bits past the message's {MessageBits}th are "
                + "not part of it and must be zero. Upstream clears them; refusing instead means a "
                + "caller who put something there finds out, rather than getting a payload that "
                + "looks right and does not carry what was meant.",
                nameof(message));
        }

        payload.Clear();
        message.CopyTo(payload[..MessageBytes]);

        // The checksum is computed over the message zero-extended — which the Clear above has
        // already arranged, since everything past the message is zero at this point.
        var checksum = Crc14.Compute(payload, CrcBitCount);

        // Bits 77 to 90, most significant first, straddling the last three bytes.
        payload[9] |= (byte)(checksum >> 11);
        payload[10] = (byte)(checksum >> 3);
        payload[11] = (byte)(checksum << 5);
    }

    /// <summary>
    /// Reads a payload back: checks its checksum and, if it holds, writes out the 77 message bits.
    /// </summary>
    /// <param name="payload"><see cref="PayloadBytes"/> bytes.</param>
    /// <param name="message">
    /// <see cref="MessageBytes"/> bytes, written only when this returns <see langword="true"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the payload's checksum is the checksum of its own message and its
    /// spare bits are zero; <see langword="false"/> otherwise.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <b>This never throws for a correctly sized buffer, whatever is in it.</b> Every one of the
    /// 2^96 possible 12-byte buffers either validates or is refused; none produces an exception,
    /// and none produces a message that the caller could mistake for a checked one. A wrong length
    /// is a different thing — that is a caller mistake rather than a bad signal, and it is refused
    /// loudly.
    /// </para>
    /// <para>
    /// <b>A payload that fails is never returned as valid</b>, and <paramref name="message"/> is
    /// left untouched when it does, so a caller who ignores the return value gets whatever it
    /// already had rather than an unchecked message dressed as a checked one.
    /// </para>
    /// <para>
    /// <b>The spare bits are checked as well as the checksum.</b> Bits 91 to 95 are outside the
    /// code; a buffer with them set is not a payload this library will hand on to an encoder, so
    /// it is not one this library will call valid either.
    /// </para>
    /// </remarks>
    public static bool TryRead(ReadOnlySpan<byte> payload, Span<byte> message)
    {
        if (payload.Length != PayloadBytes)
        {
            throw new ArgumentException(
                $"A payload is {PayloadBytes} bytes and this one is {payload.Length}.", nameof(payload));
        }

        if (message.Length != MessageBytes)
        {
            throw new ArgumentException(
                $"A message is {MessageBytes} bytes and this one is {message.Length}.", nameof(message));
        }

        if ((payload[PayloadBytes - 1] & SpareMask) != 0)
        {
            return false;
        }

        // Rebuild what the checksum was computed over: the message, zero-extended.
        Span<byte> checksummed = stackalloc byte[PayloadBytes];
        payload[..MessageBytes].CopyTo(checksummed);
        checksummed[MessageBytes - 1] &= unchecked((byte)~MessageSpareMask);

        if (Crc14.Compute(checksummed, CrcBitCount) != ExtractCrc(payload))
        {
            return false;
        }

        checksummed[..MessageBytes].CopyTo(message);
        return true;
    }

    /// <summary>
    /// The checksum stored in a payload, whether or not it is the right one.
    /// </summary>
    /// <remarks>
    /// Ported from <c>ftx_extract_crc</c>. This reads what is there and judges nothing; a caller
    /// wanting to know whether a payload is sound wants <see cref="TryRead"/>.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="payload"/> is the wrong length.</exception>
    public static ushort ExtractCrc(ReadOnlySpan<byte> payload)
    {
        if (payload.Length != PayloadBytes)
        {
            throw new ArgumentException(
                $"A payload is {PayloadBytes} bytes and this one is {payload.Length}.", nameof(payload));
        }

        return (ushort)(((payload[9] & 0x07) << 11) | (payload[10] << 3) | (payload[11] >> 5));
    }
}

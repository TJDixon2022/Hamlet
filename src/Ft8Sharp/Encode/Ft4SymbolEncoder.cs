using System;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;

namespace Ft8Sharp.Encode;

/// <summary>
/// Turns a packed 77-bit message into the 105 channel symbols an FT4 transmission actually sends:
/// the payload exclusive-ORed with FT4's own pseudorandom sequence, checksummed, LDPC-encoded, then
/// mapped two bits at a time through the four-tone Gray code and interleaved with two ramp symbols
/// and <b>four different</b> Costas sync groups.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/encode.c</c>, function <c>ft4_encode</c></b>, in the pinned ft8_lib clone
/// at <see cref="Ft8Tables.UpstreamCommit"/>. The geometry comes from <c>ft8/constants.h</c>, where
/// all six of its scalars are macros; the assembly order — where the ramps and the sync groups sit,
/// which way the Gray map runs, and how two bits are taken from the codeword — is read off
/// expressions inside that function's own body, which is the weaker anchoring and is the same
/// weakness <see cref="Ft8SymbolEncoder"/> records for FT8.
/// </para>
/// <para>
/// <b>Beside <see cref="Ft8SymbolEncoder"/> and not inside it.</b> That type's seven constants are
/// FT8's, and an FT8 regression has to stay attributable to the FT8 side; a shared encoder
/// parameterised by protocol would put every FT4 change one edit away from the sequence fifty-one
/// messages were proved identical to upstream's. Nothing in this file is reachable from FT8's path
/// and nothing in FT8's path is changed by it.
/// </para>
/// <para>
/// <b>What FT4 does that FT8 does not, and it is the whole of the difference at this layer.</b>
/// Four things: the ten payload bytes are exclusive-ORed with <see cref="Ft8Tables.Ft4XorSequence"/>
/// <em>before</em> the checksum and the parity are computed; a symbol carries two bits rather than
/// three; the four sync groups carry four <em>different</em> four-symbol patterns rather than one
/// pattern three times; and symbols 0 and 104 are ramps carrying no information at all. Upstream's
/// own comment quotes the protocol description on the first of those — the exclusive-OR exists so
/// that a CQ does not put a long run of zeros on the air.
/// </para>
/// <para>
/// <b>The LDPC code is the same one FT8 uses</b> — <c>encode174</c>, the (174,91) generator in
/// <see cref="Ft8Tables.LdpcGenerator"/> — and so is the CRC-14 and the 77-bit container. That is
/// upstream's arrangement, not a convenience: the two protocols share their whole message layer and
/// differ only in the modulation and in the exclusive-OR above.
/// </para>
/// <para>
/// <b>Nothing here goes near audio, a sound device or a transmitter.</b> This type produces an array
/// of small integers. <c>CLAUDE.md</c> §0.2.
/// </para>
/// <para>
/// <b>This library's agreement with itself is not agreement with anybody else.</b> Every assertion
/// this type carries is about its own output. What settles it is
/// <c>Ft4SymbolBitIdentityTests.EverySymbolOfEveryMessageIsIdenticalToUpstreamsFt4</c>, which holds
/// these symbols against the ones <c>gen_ft8 -ft4</c> prints for the same text.
/// </para>
/// </remarks>
public static class Ft4SymbolEncoder
{
    /// <summary>Total channel symbols in a transmission. Upstream's <c>FT4_NN</c>.</summary>
    public const int SymbolCount = 105;

    /// <summary>Symbols that carry codeword bits. Upstream's <c>FT4_ND</c>.</summary>
    public const int DataSymbolCount = 87;

    /// <summary>Symbols that carry nothing at all, one at each end. Upstream's <c>FT4_NR</c>.</summary>
    public const int RampSymbolCount = 2;

    /// <summary>Symbols in each sync group. Upstream's <c>FT4_LENGTH_SYNC</c>.</summary>
    public const int SyncGroupLength = 4;

    /// <summary>How many sync groups a transmission carries. Upstream's <c>FT4_NUM_SYNC</c>.</summary>
    public const int SyncGroupCount = 4;

    /// <summary>
    /// The distance between the starts of consecutive sync groups. Upstream's
    /// <c>FT4_SYNC_OFFSET</c>.
    /// </summary>
    /// <remarks>
    /// Upstream writes the four group positions as literal ranges in <c>ft4_encode</c>'s own guards —
    /// 1 to 4, 34 to 37, 67 to 70, 100 to 103 — and as <c>1 + (FT4_SYNC_OFFSET * m) + k</c> in
    /// <c>ft4_sync_score</c>. They are derived from the macro here, and the derivation is checked
    /// against upstream's literals rather than believed.
    /// </remarks>
    public const int SyncGroupOffset = 33;

    /// <summary>The first symbol of the first sync group: symbol 0 is a ramp, so the groups start at 1.</summary>
    public const int FirstSyncGroupStart = 1;

    /// <summary>Codeword bits carried by each data symbol.</summary>
    public const int BitsPerSymbol = 2;

    /// <summary>How many distinct tones the alphabet holds.</summary>
    public const int ToneCount = 1 << BitsPerSymbol;

    /// <summary>The packed message size this takes, in bytes.</summary>
    public const int MessageBytes = Ft8Payload.MessageBytes;

    /// <summary>The start index of the given sync group.</summary>
    /// <param name="group">Zero to <see cref="SyncGroupCount"/> minus one.</param>
    /// <exception cref="ArgumentOutOfRangeException">The group index is not one of the four.</exception>
    public static int SyncGroupStart(int group)
    {
        if (group < 0 || group >= SyncGroupCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(group),
                group,
                $"A transmission carries {SyncGroupCount} sync groups, numbered from zero.");
        }

        return FirstSyncGroupStart + (group * SyncGroupOffset);
    }

    /// <summary>Whether the given symbol index is one of the two ramps.</summary>
    public static bool IsRampSymbol(int symbolIndex) =>
        symbolIndex == 0 || symbolIndex == SymbolCount - 1;

    /// <summary>
    /// Whether the given symbol index falls inside one of the sync groups, and which group and
    /// position if it does.
    /// </summary>
    /// <returns>False for a ramp symbol and for a data symbol.</returns>
    public static bool TrySyncPosition(int symbolIndex, out int group, out int position)
    {
        for (var candidate = 0; candidate < SyncGroupCount; candidate++)
        {
            var start = SyncGroupStart(candidate);
            if (symbolIndex >= start && symbolIndex < start + SyncGroupLength)
            {
                group = candidate;
                position = symbolIndex - start;
                return true;
            }
        }

        group = -1;
        position = -1;
        return false;
    }

    /// <summary>Whether the given symbol index carries codeword bits.</summary>
    public static bool IsDataSymbol(int symbolIndex) =>
        !IsRampSymbol(symbolIndex) && !TrySyncPosition(symbolIndex, out _, out _);

    /// <summary>
    /// The message a transmission carries, exclusive-ORed with FT4's own pseudorandom sequence.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Its own inverse, and that is why one method serves both directions.</b> The encoder applies
    /// it before the checksum; the decoder applies it after the checksum has been checked, which is
    /// where <c>ftx_decode_candidate</c> puts it. Both ends call this.
    /// </para>
    /// <para>
    /// <b>The three bits past the message's 77th survive it.</b> The sequence's last byte is
    /// <c>0x28</c> and its low three bits are zero — upstream's own comment writes it out as
    /// <c>00101 [000]</c> — so a message whose spare bits are clear stays clear, and
    /// <see cref="Ft8Payload.Create"/>'s refusal is not tripped by the exclusive-OR itself.
    /// </para>
    /// </remarks>
    /// <param name="message"><see cref="MessageBytes"/> bytes.</param>
    /// <param name="scrambled"><see cref="MessageBytes"/> bytes, written in full.</param>
    /// <exception cref="ArgumentException">Either span is the wrong length.</exception>
    public static void ApplyPayloadXor(ReadOnlySpan<byte> message, Span<byte> scrambled)
    {
        if (message.Length != MessageBytes)
        {
            throw new ArgumentException(
                $"A packed message is {MessageBytes} bytes and this one is {message.Length}.",
                nameof(message));
        }

        if (scrambled.Length != MessageBytes)
        {
            throw new ArgumentException(
                $"The result is {MessageBytes} bytes and this buffer holds {scrambled.Length}.",
                nameof(scrambled));
        }

        var sequence = Ft8Tables.Ft4XorSequence;
        if (sequence.Length != MessageBytes)
        {
            throw new InvalidOperationException(
                $"The checked-in FT4 exclusive-OR sequence holds {sequence.Length} bytes and a "
                + $"message is {MessageBytes}.");
        }

        for (var i = 0; i < MessageBytes; i++)
        {
            scrambled[i] = (byte)(message[i] ^ sequence[i]);
        }
    }

    /// <summary>
    /// Encodes a packed message into the channel symbols that carry it.
    /// </summary>
    /// <param name="message">
    /// <see cref="MessageBytes"/> bytes holding <see cref="Ft8Payload.MessageBits"/> bits, most
    /// significant bit first, with the bits past the last one zero.
    /// </param>
    /// <param name="symbols">
    /// <see cref="SymbolCount"/> bytes, each written with a tone in
    /// <c>0..<see cref="ToneCount"/>-1</c>. Written only on success, and in one move.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Either span is the wrong length, or the message has bits set past its last. In neither case
    /// is <paramref name="symbols"/> touched.
    /// </exception>
    public static void Encode(ReadOnlySpan<byte> message, Span<byte> symbols)
    {
        if (message.Length != MessageBytes)
        {
            throw new ArgumentException(
                $"A packed message is {MessageBytes} bytes and this one is {message.Length}.",
                nameof(message));
        }

        if (symbols.Length != SymbolCount)
        {
            throw new ArgumentException(
                $"An FT4 transmission is {SymbolCount} channel symbols and this buffer holds "
                + $"{symbols.Length}. Nothing has been written to it.",
                nameof(symbols));
        }

        // Everything below lands in stack buffers, so a throw anywhere in here leaves the caller's
        // span exactly as it arrived rather than half-filled.
        Span<byte> scrambled = stackalloc byte[MessageBytes];
        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        Span<byte> codeword = stackalloc byte[LdpcEncoder.CodewordBytes];
        Span<byte> assembled = stackalloc byte[SymbolCount];

        // THE EXCLUSIVE-OR COMES FIRST, BEFORE THE CHECKSUM. Upstream's order, and it is the whole
        // reason FT4's CRC is not FT8's CRC of the same message.
        ApplyPayloadXor(message, scrambled);

        Ft8Payload.Create(scrambled, payload);
        LdpcEncoder.Encode(payload, codeword);

        Lay(codeword, assembled);
        assembled.CopyTo(symbols);
    }

    /// <summary>Encodes a packed message and returns its channel symbols in a fresh array.</summary>
    public static byte[] Encode(ReadOnlySpan<byte> message)
    {
        var symbols = new byte[SymbolCount];
        Encode(message, symbols);
        return symbols;
    }

    /// <summary>
    /// Lays the codeword out across the data symbols and drops the ramps and the four sync groups in
    /// among them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The bit walk is continuous and neither the ramps nor the sync groups interrupt it</b>, the
    /// same property <see cref="Ft8SymbolEncoder"/> records: a symbol that carries no codeword bit
    /// consumes none, so the reader's position carries across.
    /// </para>
    /// <para>
    /// <b>Group <c>m</c> takes row <c>m</c> of the Costas table, and that is the structural
    /// difference from FT8.</b> FT8 sends one seven-symbol pattern three times; FT4 sends four
    /// different four-symbol patterns. A port that repeated row 0 would produce a sequence of the
    /// right length with every value inside the alphabet and the groups in the right places, and
    /// nothing this library asserts about its own output would catch it.
    /// </para>
    /// </remarks>
    private static void Lay(ReadOnlySpan<byte> codeword, Span<byte> symbols)
    {
        var costas = Ft8Tables.Ft4CostasPattern;
        var gray = Ft8Tables.Ft4GrayMap;

        if (costas.Length != SyncGroupCount * SyncGroupLength)
        {
            throw new InvalidOperationException(
                $"The checked-in FT4 Costas table holds {costas.Length} symbols and FT4 sends "
                + $"{SyncGroupCount} groups of {SyncGroupLength}.");
        }

        if (gray.Length != ToneCount)
        {
            throw new InvalidOperationException(
                $"The checked-in FT4 Gray map holds {gray.Length} entries and the tone alphabet is "
                + $"{ToneCount}.");
        }

        var bitIndex = 0;
        var dataSymbols = 0;
        var ramps = 0;

        for (var symbol = 0; symbol < SymbolCount; symbol++)
        {
            if (IsRampSymbol(symbol))
            {
                // Upstream writes tone 0 here. A ramp carries no payload; what it is for is giving
                // the envelope somewhere to rise and fall.
                symbols[symbol] = 0;
                ramps++;
                continue;
            }

            if (TrySyncPosition(symbol, out var group, out var position))
            {
                symbols[symbol] = Inside(
                    costas[(group * SyncGroupLength) + position], "FT4 Costas table");
                continue;
            }

            var bits = 0;
            for (var bit = 0; bit < BitsPerSymbol; bit++)
            {
                bits = (bits << 1) | ReadBit(codeword, bitIndex++);
            }

            symbols[symbol] = Inside(gray[bits], "FT4 Gray map");
            dataSymbols++;
        }

        // Arithmetic over what just happened, not a second reading of the pin.
        if (dataSymbols != DataSymbolCount
            || ramps != RampSymbolCount
            || bitIndex != DataSymbolCount * BitsPerSymbol)
        {
            throw new InvalidOperationException(
                $"The layout produced {dataSymbols} data symbols and {ramps} ramps, consuming "
                + $"{bitIndex} codeword bits, and the geometry says {DataSymbolCount}, "
                + $"{RampSymbolCount} and {DataSymbolCount * BitsPerSymbol}.");
        }
    }

    /// <summary>One codeword bit, most significant bit of the first byte first.</summary>
    private static int ReadBit(ReadOnlySpan<byte> codeword, int bitIndex) =>
        (codeword[bitIndex >> 3] >> (7 - (bitIndex & 7))) & 1;

    /// <summary>Refuses a table value that is not a tone rather than putting it in the sequence.</summary>
    private static byte Inside(byte tone, string source)
    {
        if (tone >= ToneCount)
        {
            throw new InvalidOperationException(
                $"The checked-in {source} holds a value outside the {ToneCount}-tone alphabet. No "
                + "sequence is returned; a table that cannot be trusted cannot produce one.");
        }

        return tone;
    }
}

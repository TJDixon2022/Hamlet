using System;
using Ft8Sharp.Ldpc;
using Ft8Sharp.Message;

namespace Ft8Sharp.Encode;

/// <summary>
/// Turns a packed 77-bit message into the 79 channel symbols an FT8 transmission actually sends:
/// the LDPC codeword mapped three bits at a time through the Gray code, interleaved with the three
/// Costas sync blocks.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/encode.c</c>, function <c>ft8_encode</c></b>, in the pinned ft8_lib clone
/// at <see cref="Ft8Tables.UpstreamCommit"/>. The geometry comes from <c>ft8/constants.h</c>, where
/// all five of its scalars are macros; the assembly order — where the sync blocks sit, which way
/// the Gray map runs, and how three bits are taken from the codeword — is read off expressions
/// inside that function's own body, which is the weaker anchoring and is recorded as such in
/// <c>porting-notes.md</c>.
/// </para>
/// <para>
/// <b>The input is the 77 bits, not the message text.</b> That is upstream's own boundary —
/// <c>ft8_encode</c> takes a packed payload and never sees a string — and it keeps this type out of
/// the business of deciding which message type some text is. A caller with text runs it through
/// <c>Ft8MessageDecoder</c>'s packing side first and hands the bits here.
/// </para>
/// <para>
/// <b>Nothing here goes near audio, a sound device or a transmitter, and nothing here is capable
/// of it.</b> This type produces an array of small integers. Turning those into a waveform is a
/// later unit's, and routing any of it to a radio is forbidden outright by <c>CLAUDE.md</c> §0.2
/// and by the phase plan's first named boundary: the encoder is a test oracle.
/// </para>
/// <para>
/// <b>What it refuses, rather than guessing.</b> HM-DEC-009. A message of the wrong length, a
/// message with bits set past its 77th, a symbol buffer of the wrong length, and — through
/// <see cref="LdpcEncoder"/>, which is not caught here — a payload whose spare bits are set.
/// <b>No partial sequence is ever returned.</b> The whole assembly happens in stack buffers and is
/// copied out in one move at the end, so a caller whose call threw finds its own buffer exactly as
/// it left it rather than half-written with a plausible-looking tail.
/// </para>
/// <para>
/// <b>No state and nothing static and mutable.</b> Two calls with the same message produce the same
/// symbols in any order, on any thread, in any process. Step 4 has <em>candidate ranking is stable
/// across runs</em> waiting on this habit.
/// </para>
/// <para>
/// <b>This library's agreement with itself is not agreement with anybody else.</b> Every assertion
/// this type carries is about its own output: the length, the alphabet, the sync blocks. A port
/// that ran the Gray map backwards would satisfy all of them. Step 3's second exit criterion — the
/// symbol sequence is bit-identical to <c>ft8_lib</c>'s — is what settles that, and unit 209 could
/// not take it, because there is no C toolchain on the machine to build the reference generator
/// with. It is open.
/// </para>
/// </remarks>
public static class Ft8SymbolEncoder
{
    /// <summary>Total channel symbols in a transmission. Upstream's <c>FT8_NN</c>.</summary>
    public const int SymbolCount = 79;

    /// <summary>Symbols that carry codeword bits. Upstream's <c>FT8_ND</c>.</summary>
    public const int DataSymbolCount = 58;

    /// <summary>Symbols in each sync block. Upstream's <c>FT8_LENGTH_SYNC</c>.</summary>
    public const int SyncBlockLength = 7;

    /// <summary>How many sync blocks a transmission carries. Upstream's <c>FT8_NUM_SYNC</c>.</summary>
    public const int SyncBlockCount = 3;

    /// <summary>
    /// The distance between the starts of consecutive sync blocks. Upstream's
    /// <c>FT8_SYNC_OFFSET</c>.
    /// </summary>
    /// <remarks>
    /// Upstream writes the three block positions as literal ranges in its own guards rather than as
    /// arithmetic on this macro. They are derived from it here, and
    /// <c>UpstreamSymbolAssemblyProvenanceTests</c> checks the derivation against those literals so
    /// that the two readings have to agree.
    /// </remarks>
    public const int SyncBlockOffset = 36;

    /// <summary>Codeword bits carried by each data symbol.</summary>
    public const int BitsPerSymbol = 3;

    /// <summary>
    /// How many distinct tones the alphabet holds — exactly what <see cref="BitsPerSymbol"/> bits
    /// address, and the declared extent of upstream's Gray map.
    /// </summary>
    public const int ToneCount = 1 << BitsPerSymbol;

    /// <summary>The packed message size this takes, in bytes.</summary>
    public const int MessageBytes = Ft8Payload.MessageBytes;

    /// <summary>The start index of the given sync block.</summary>
    /// <param name="block">Zero to <see cref="SyncBlockCount"/> minus one.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block index is not one of the three.</exception>
    public static int SyncBlockStart(int block)
    {
        if (block < 0 || block >= SyncBlockCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(block),
                block,
                $"A transmission carries {SyncBlockCount} sync blocks, numbered from zero.");
        }

        return block * SyncBlockOffset;
    }

    /// <summary>Whether the given symbol index falls inside one of the sync blocks.</summary>
    public static bool IsSyncSymbol(int symbolIndex)
    {
        for (var block = 0; block < SyncBlockCount; block++)
        {
            var start = block * SyncBlockOffset;
            if (symbolIndex >= start && symbolIndex < start + SyncBlockLength)
            {
                return true;
            }
        }

        return false;
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
    /// Either span is the wrong length, or the message has bits set past its last, or the payload
    /// built from it has its spare bits set. In none of these cases is
    /// <paramref name="symbols"/> touched.
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
                $"A transmission is {SymbolCount} channel symbols and this buffer holds "
                + $"{symbols.Length}. Nothing has been written to it.",
                nameof(symbols));
        }

        // Everything below lands in stack buffers. The caller's span is written once, at the end,
        // so a throw anywhere in here leaves it exactly as it arrived rather than half-filled.
        Span<byte> payload = stackalloc byte[Ft8Payload.PayloadBytes];
        Span<byte> codeword = stackalloc byte[LdpcEncoder.CodewordBytes];
        Span<byte> assembled = stackalloc byte[SymbolCount];

        // Both of these refuse rather than correcting, and neither refusal is caught here: a
        // message with bits past its 77th, or a payload with its spare bits set, is a caller
        // mistake and the caller is the one who has to hear about it.
        Ft8Payload.Create(message, payload);
        LdpcEncoder.Encode(payload, codeword);

        Lay(codeword, assembled);
        assembled.CopyTo(symbols);
    }

    /// <summary>
    /// Encodes a packed message and returns its channel symbols in a fresh array.
    /// </summary>
    /// <remarks>
    /// The allocating convenience. <see cref="Encode(ReadOnlySpan{byte}, Span{byte})"/> is the one
    /// a decoder's inner loop would want; this is the one a test and a caller with a message in
    /// hand want, and it cannot return a partial sequence because it does not return at all unless
    /// the whole thing succeeded.
    /// </remarks>
    public static byte[] Encode(ReadOnlySpan<byte> message)
    {
        var symbols = new byte[SymbolCount];
        Encode(message, symbols);
        return symbols;
    }

    /// <summary>
    /// Lays the codeword out across the data symbols and drops the three sync blocks in among them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>The bit walk is continuous and the sync blocks do not interrupt it.</b> A sync symbol
    /// consumes no codeword bit, so the reader's position carries across a block rather than
    /// restarting after it. That is upstream's shape and it is the one a plausible reading gets
    /// wrong: restarting the walk at each block would produce a sequence of the right length, with
    /// every value inside the alphabet and the sync blocks in the right places, and nothing this
    /// library asserts about its own output would catch it.
    /// </para>
    /// <para>
    /// <b>The three bits are taken most significant first</b>, from a codeword walked most
    /// significant bit first — which is the order <see cref="LdpcEncoder"/> writes it in — and the
    /// group so assembled <em>indexes</em> the Gray map. The map's element is the tone. The
    /// opposite direction is the decoder's inverse permutation and is not what runs here.
    /// </para>
    /// </remarks>
    private static void Lay(ReadOnlySpan<byte> codeword, Span<byte> symbols)
    {
        var costas = Ft8Tables.Ft8CostasPattern;
        var gray = Ft8Tables.Ft8GrayMap;

        // A table that has gone wrong cannot be allowed to put a value on the air that is not a
        // tone. This is cheap, it runs once per call, and it is the seam that keeps "every symbol
        // is inside the alphabet" a property of the encoder rather than of the tests alone.
        if (costas.Length != SyncBlockLength)
        {
            throw new InvalidOperationException(
                $"The checked-in Costas pattern holds {costas.Length} symbols and a sync block is "
                + $"{SyncBlockLength}.");
        }

        if (gray.Length != ToneCount)
        {
            throw new InvalidOperationException(
                $"The checked-in Gray map holds {gray.Length} entries and the tone alphabet is "
                + $"{ToneCount}.");
        }

        var bitIndex = 0;
        var dataSymbols = 0;

        for (var symbol = 0; symbol < SymbolCount; symbol++)
        {
            var sync = -1;
            for (var block = 0; block < SyncBlockCount; block++)
            {
                var start = block * SyncBlockOffset;
                if (symbol >= start && symbol < start + SyncBlockLength)
                {
                    sync = symbol - start;
                    break;
                }
            }

            if (sync >= 0)
            {
                symbols[symbol] = Inside(costas[sync], "Costas pattern");
                continue;
            }

            var bits = 0;
            for (var bit = 0; bit < BitsPerSymbol; bit++)
            {
                bits = (bits << 1) | ReadBit(codeword, bitIndex++);
            }

            symbols[symbol] = Inside(gray[bits], "Gray map");
            dataSymbols++;
        }

        // Arithmetic over what just happened, not a second reading of the pin. If either of these
        // is wrong the geometry constants disagree with each other, and the caller gets an
        // exception rather than a sequence that is the right length and means nothing.
        if (dataSymbols != DataSymbolCount || bitIndex != DataSymbolCount * BitsPerSymbol)
        {
            throw new InvalidOperationException(
                $"The layout produced {dataSymbols} data symbols consuming {bitIndex} codeword bits, "
                + $"and the geometry says {DataSymbolCount} and {DataSymbolCount * BitsPerSymbol}.");
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

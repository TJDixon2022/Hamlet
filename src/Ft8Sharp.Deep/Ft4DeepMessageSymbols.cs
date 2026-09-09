using System;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>The 105 channel symbols a decoded FT4 message must have been carried on, recovered by packing
/// it again — or nothing, where they cannot be recovered exactly.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>IT IS <c>Ft8DeepMessageSymbols</c> WITH ONE CALL CHANGED, AND THE PACKING IS NOT COPIED.</b>
/// The two protocols share their whole message layer — the same 77-bit container, the same CRC-14
/// and the same (174,91) LDPC code — and differ in the modulation and in FT4's payload
/// exclusive-OR, both of which are inside <see cref="Ft4SymbolEncoder.Encode(ReadOnlySpan{byte},
/// Span{byte})"/>. So this type calls <c>Ft8DeepMessageSymbols.TryPackAgain</c> for the bits, the
/// callsign cache and the round-trip guard, and then FT4's encoder for the symbols. <b>A second copy
/// of the packing would be a second place for the guard to be got wrong.</b>
/// </para>
/// <para>
/// <b>WHY THIS EXISTS AT ALL: THE DECODE RESULT HANDS BACK TEXT AND CARRIES NO BITS.</b>
/// <c>Ft4SlotDecoder</c> returns the same <c>Ft8SlotResult</c> FT8's path does, and the corrected 77
/// bits live in a <c>stackalloc</c> inside the codeword decoder for about four statements and are
/// then gone. <c>Ft8Sharp</c> is a faithful port that this phase changes not one line of.
/// </para>
/// <para>
/// <b>THE FAILURE MODE, NAMED: A MESSAGE THAT PACKS TO DIFFERENT BITS THAN WERE SENT.</b> A callsign
/// that travelled as a hash comes back in angle brackets; a contest form this library can read and
/// cannot write has no packer at all. Packing such a message again produces a different 77 bits, a
/// different 105 symbols, and a signal-to-noise ratio measured against a transmission that was never
/// made. <b>A plausible number nothing measured is the fault <c>CLAUDE.md</c> §0.0 exists for</b>, and
/// it is worse here than on a screen, because a report goes on the air and into another operator's
/// log. <b>The refusal is a null and never a floor.</b>
/// </para>
/// <para>
/// <b>PURE, AND IT DECIDES NOTHING.</b> Nothing here is called from a decode path, and a refusal
/// costs a message nothing but its signal-to-noise figure.
/// </para>
/// </remarks>
public static class Ft4DeepMessageSymbols
{
    /// <summary>
    /// The channel symbols behind a decoded FT4 message, or <see langword="false"/> where they cannot
    /// be recovered exactly.
    /// </summary>
    /// <param name="decoded">What the message layer made of the 77 bits.</param>
    /// <param name="symbols">
    /// <see cref="Ft4SymbolEncoder.SymbolCount"/> bytes, each written with a tone in 0..3.
    /// <b>Written only on success</b>, so a caller that ignores the return value gets whatever it
    /// arrived with rather than half a frame.
    /// </param>
    /// <returns>True where the round trip held.</returns>
    /// <exception cref="ArgumentException"><paramref name="symbols"/> is the wrong length.</exception>
    public static bool TryEncode(in Ft8DecodeResult decoded, Span<byte> symbols)
    {
        if (symbols.Length != Ft4SymbolEncoder.SymbolCount)
        {
            throw new ArgumentException(
                $"An FT4 transmission is {Ft4SymbolEncoder.SymbolCount} channel symbols and this "
                + $"buffer holds {symbols.Length}. Nothing has been written to it.",
                nameof(symbols));
        }

        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];

        if (!Ft8DeepMessageSymbols.TryPackAgain(decoded, message))
        {
            return false;
        }

        // THE ONE LINE THAT IS FT4'S. Everything above it is the shared message layer.
        Ft4SymbolEncoder.Encode(message, symbols);
        return true;
    }

    /// <summary>The allocating convenience: the channel symbols, or <see langword="null"/>.</summary>
    /// <param name="decoded">What the message layer made of the 77 bits.</param>
    /// <returns>105 tone indices, or null where the round trip did not hold.</returns>
    /// <remarks>
    /// It cannot return a partial sequence, because it does not return one at all unless the whole
    /// round trip held.
    /// </remarks>
    public static byte[]? TryEncode(in Ft8DecodeResult decoded)
    {
        var symbols = new byte[Ft4SymbolEncoder.SymbolCount];
        return TryEncode(decoded, symbols) ? symbols : null;
    }
}

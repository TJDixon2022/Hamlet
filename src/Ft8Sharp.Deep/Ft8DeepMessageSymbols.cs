using System;
using Ft8Sharp.Encode;
using Ft8Sharp.Message;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>The 79 channel symbols a decoded message must have been carried on, recovered by packing it
/// again — or nothing, where they cannot be recovered exactly.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>WHY THIS EXISTS AT ALL: THE DECODE RESULT HANDS BACK TEXT AND CARRIES NO BITS.</b>
/// <c>Ft8DecodeResult</c> carries a type, a status, text and three fields; <c>LdpcDecodeResult</c>
/// carries an unsatisfied-check count and an iteration count; <c>Ft8CodewordResult</c> carries the
/// two of them. The corrected 77 bits live in a <c>stackalloc</c> inside
/// <c>Ft8CodewordDecoder.Decode</c> for about four statements and are then gone, and
/// <c>Ft8Sharp</c> is a faithful port that this phase changes not one line of.
/// </para>
/// <para>
/// <b>AND THE OTHER ROUTE DOES NOT COVER THE MESSAGES THIS IS FOR.</b>
/// <c>Ft8DeepSlotDecoder</c> holds a codeword in <c>_osdCodeword</c> only where ordered statistics
/// rescued a candidate belief propagation had given up on. <b>Where the port decoded outright, OSD
/// is never asked and there is no codeword in this library at all</b>, and that is the great
/// majority of every message on the screen. So the bits are recovered from the words.
/// </para>
/// <para>
/// <b>THE FAILURE MODE, NAMED: A MESSAGE THAT PACKS TO DIFFERENT BITS THAN WERE SENT.</b> A
/// callsign that travelled as a hash comes back in angle brackets, which is an output convention
/// and not something every packer will take back; a contest form this library can read and cannot
/// write has no packer at all. Packing such a message again produces a different 77 bits, a
/// different 79 symbols, and a signal-to-noise ratio measured against a transmission that was never
/// made. <b>A plausible number nothing measured is the fault <c>CLAUDE.md</c> §0.0 exists for.</b>
/// </para>
/// <para>
/// <b>SO THE ROUND TRIP IS THE GUARD AND IT IS NOT OPTIONAL.</b> The bits produced here are put
/// straight back through <c>Ft8MessageDecoder.Decode</c>, against the same callsign cache, and the
/// text compared <b>ordinally</b> with the text the decoder returned. Anything but an exact match
/// and there are no symbols — <b>no measurement, rather than a measurement of something else.</b>
/// </para>
/// <para>
/// <b>PURE, AND IT DECIDES NOTHING.</b> Nothing here is called from a decode path, and a refusal
/// costs a message nothing but its signal-to-noise figure.
/// </para>
/// </remarks>
public static class Ft8DeepMessageSymbols
{
    /// <summary>
    /// The channel symbols behind a decoded message, or <see langword="false"/> where they cannot be
    /// recovered exactly.
    /// </summary>
    /// <param name="decoded">What the message layer made of the 77 bits.</param>
    /// <param name="symbols">
    /// <see cref="Ft8SymbolEncoder.SymbolCount"/> bytes, each written with a tone in 0..7.
    /// <b>Written only on success</b>, so a caller that ignores the return value gets whatever it
    /// arrived with rather than half a frame.
    /// </param>
    /// <returns>True where the round trip held.</returns>
    /// <exception cref="ArgumentException"><paramref name="symbols"/> is the wrong length.</exception>
    public static bool TryEncode(in Ft8DecodeResult decoded, Span<byte> symbols)
    {
        if (symbols.Length != Ft8SymbolEncoder.SymbolCount)
        {
            throw new ArgumentException(
                $"A transmission is {Ft8SymbolEncoder.SymbolCount} channel symbols and this buffer "
                + $"holds {symbols.Length}. Nothing has been written to it.",
                nameof(symbols));
        }

        if (!decoded.Decoded)
        {
            return false;
        }

        // ONE CACHE, USED FOR BOTH DIRECTIONS. A call that travelled as a hash is packed into this
        // cache and then resolved back out of it, so the round trip below compares like with like.
        // A cache per call and never shared: nothing here depends on what any other message said.
        var cache = new Ft8CallsignCache();
        Span<byte> message = stackalloc byte[Ft8Payload.MessageBytes];

        if (!TryPack(decoded, cache, message))
        {
            return false;
        }

        // THE GUARD. Ordinal, because a comparison that ignores case or culture would let through
        // exactly the near-misses this is here to catch.
        var again = Ft8MessageDecoder.Decode(message, cache);
        if (!again.Decoded || !string.Equals(again.Text, decoded.Text, StringComparison.Ordinal))
        {
            return false;
        }

        Ft8SymbolEncoder.Encode(message, symbols);
        return true;
    }

    /// <summary>
    /// The allocating convenience: the channel symbols, or <see langword="null"/>.
    /// </summary>
    /// <param name="decoded">What the message layer made of the 77 bits.</param>
    /// <returns>79 tone indices, or null where the round trip did not hold.</returns>
    /// <remarks>
    /// It cannot return a partial sequence, because it does not return one at all unless the whole
    /// round trip held.
    /// </remarks>
    public static byte[]? TryEncode(in Ft8DecodeResult decoded)
    {
        var symbols = new byte[Ft8SymbolEncoder.SymbolCount];
        return TryEncode(decoded, symbols) ? symbols : null;
    }

    /// <summary>Packs one decoded message back into 77 bits, by type.</summary>
    /// <remarks>
    /// <b>Four types are attempted and the rest are refused by name.</b> The contest forms this
    /// library can neither read nor write never reach here — <c>Ft8MessageDecoder</c> refuses them
    /// first — and the two it can read but not write are refused here. <b>A refusal is a correct
    /// answer</b> and the caller has a null measurement, which is the state
    /// <c>Ft8SlotLevel</c>'s remarks argue for at length.
    /// </remarks>
    private static bool TryPack(in Ft8DecodeResult decoded, Ft8CallsignCache cache, Span<byte> message)
    {
        switch (decoded.Type)
        {
            case Ft8MessageType.Standard:
            {
                var fields = decoded.Fields;
                return Ft8StandardMessage.TryPack(
                    Unbracket(fields.CallTo, cache),
                    Unbracket(fields.CallDe, cache),
                    fields.Extra,
                    cache,
                    message) == Ft8PackResult.Ok;
            }

            case Ft8MessageType.NonstandardCallsign:
            {
                var fields = decoded.Fields;
                return Ft8NonstandardMessage.TryPack(
                    Unbracket(fields.CallTo, cache),
                    Unbracket(fields.CallDe, cache),
                    fields.Extra,
                    cache,
                    message) == Ft8PackResult.Ok;
            }

            case Ft8MessageType.FreeText:
                return Ft8FreeText.TryPackText(decoded.Text, message) == Ft8PackResult.Ok;

            case Ft8MessageType.Telemetry:
            {
                Span<byte> telemetry = stackalloc byte[Ft8FreeText.BodyBytes];
                return TryReadHex(decoded.Text, telemetry)
                    && Ft8FreeText.TryPackTelemetry(telemetry, message) == Ft8PackResult.Ok;
            }

            default:
                return false;
        }
    }

    /// <summary>
    /// Strips the angle brackets a call recovered from a hash is printed in, and puts the call into
    /// the cache so packing it again hashes to the same bits.
    /// </summary>
    /// <remarks>
    /// <b>The brackets are an output convention</b> — they mark a call that was recovered from a
    /// hash rather than read out of the bits — and neither packer takes them: the angle bracket is
    /// not in the alphabet a callsign hash packs against, and
    /// <c>Ft8NonstandardMessage.TryPack</c>'s own remarks record that upstream refuses a bracketed
    /// call for that reason. <b>Saving the stripped call is what makes the round trip closeable</b>:
    /// the packer hashes it, the unpacker resolves the same hash out of the same cache, and the two
    /// texts either match exactly or this message gets no measurement.
    /// </remarks>
    private static string Unbracket(string call, Ft8CallsignCache cache)
    {
        if (call.Length < 3 || call[0] != '<' || call[^1] != '>')
        {
            return call;
        }

        var stripped = call[1..^1];
        cache.Save(stripped);
        return stripped;
    }

    /// <summary>
    /// Reads <c>2 * <see cref="Ft8FreeText.BodyBytes"/></c> hexadecimal digits back into bytes.
    /// </summary>
    /// <remarks>
    /// <c>Ft8FreeText.UnpackTelemetryHex</c> is what put them there. Upper case only, because that
    /// is what it writes and a case-insensitive read here would accept a string this library never
    /// produces.
    /// </remarks>
    private static bool TryReadHex(string text, Span<byte> bytes)
    {
        if (text.Length != bytes.Length * 2)
        {
            return false;
        }

        for (var i = 0; i < bytes.Length; i++)
        {
            if (!TryDigit(text[i * 2], out var high) || !TryDigit(text[(i * 2) + 1], out var low))
            {
                return false;
            }

            bytes[i] = (byte)((high << 4) | low);
        }

        return true;

        static bool TryDigit(char c, out int value)
        {
            if (c >= '0' && c <= '9')
            {
                value = c - '0';
                return true;
            }

            if (c >= 'A' && c <= 'F')
            {
                value = (c - 'A') + 10;
                return true;
            }

            value = 0;
            return false;
        }
    }
}

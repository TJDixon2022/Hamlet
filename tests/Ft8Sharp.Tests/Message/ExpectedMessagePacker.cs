using Ft8Sharp.Message;

namespace Ft8Sharp.Tests.Message;

/// <summary>
/// <b>Text back into 77 bits, through this library's own packers and nothing else.</b> The
/// instrument tasks 3, 4 and 5 of unit 217 all stand on: it answers <em>can this library represent
/// this expected line at all</em>, and where it can, it hands back the true message bits that were
/// on the air.
/// </summary>
/// <remarks>
/// <para>
/// <b>THIS IS A DIAGNOSTIC AND IT NEVER TOUCHES THE DECODE PATH.</b> Nothing here is referenced by
/// <c>Ft8SlotDecoder</c>, <c>Ft8SoftSymbols</c>, <c>Ft8SyncSearch</c> or <c>Ft8CodewordDecoder</c>,
/// and nothing it produces is ever passed to one of them. It lives in the test project, it is fed
/// only by an expected decode list, and its output is compared against what the untold path returned
/// — never handed to it. A decoder that knows the answer is worthless; a diagnostic that knows it is
/// a diagnostic.
/// </para>
/// <para>
/// <b>It refuses rather than guesses, and it says which refusal it made.</b> A line it cannot
/// represent is a real measurement — it is the ceiling on criterion 3 — so every failure carries a
/// named <see cref="PackFailure"/> rather than a bare false.
/// </para>
/// <para>
/// <b>The round trip is part of the test, not an extra.</b> A packing that does not decode back to
/// the same normalised text is not a representation of that line: it is a different message that
/// happens to pack. So every candidate shape is packed <em>and</em> decoded, and only an exact text
/// match is accepted. That is what stops this instrument flattering the ceiling.
/// </para>
/// <para>
/// <b>Splitting a standard message into three fields is ambiguous and the ambiguity is enumerated
/// rather than assumed.</b> <c>CQ DX K1ABC FN42</c> is four tokens and three fields; <c>K1ABC W9XYZ
/// FN42</c> is three tokens and three fields. Rather than parse, every plausible split is tried and
/// the round trip decides. A line that round-trips under one split is representable; the split that
/// achieved it is not interesting and is not reported.
/// </para>
/// </remarks>
internal static class ExpectedMessagePacker
{
    /// <summary>Why a line could not be turned back into 77 bits.</summary>
    internal enum PackFailure
    {
        /// <summary>It packed and round-tripped.</summary>
        None,

        /// <summary>
        /// The line names a station by a hash the <em>list's own writer</em> could not resolve, and
        /// prints it <c>&lt;...&gt;</c>. <b>The callsign is gone from the line</b>, so nothing can
        /// re-pack it — this is a property of the list and not of this library, and it is counted
        /// apart from everything else for exactly that reason.
        /// </summary>
        HashedCallsignLostInTheList,

        /// <summary>
        /// No shape this library builds packs it: it is not a standard message, not a non-standard
        /// callsign message, not telemetry, and too long or outside the alphabet for free text.
        /// <b>This is where a message type this library has not built lands.</b>
        /// </summary>
        NoShapeThisLibraryBuildsAcceptsIt,

        /// <summary>
        /// A shape packed and the round trip came back as different text. Counted separately because
        /// it is a defect rather than a limit — the bits went somewhere and came back meaning
        /// something else.
        /// </summary>
        PackedButDidNotRoundTrip,
    }

    /// <summary>The literal upstream writes when its own hash table could not name a station.</summary>
    internal const string UnresolvedMarker = "<...>";

    /// <summary>
    /// Turns one expected line's text into the 77 message bits that were on the air, or says why it
    /// cannot.
    /// </summary>
    /// <param name="text">The normalised expected text.</param>
    /// <param name="message">Ten bytes, written only on success.</param>
    /// <returns><see cref="PackFailure.None"/> on success.</returns>
    internal static PackFailure TryPack(string text, out byte[] message)
    {
        message = new byte[Ft8Payload.MessageBytes];
        text = (text ?? string.Empty).Trim();

        if (text.Length == 0)
        {
            return PackFailure.NoShapeThisLibraryBuildsAcceptsIt;
        }

        // The list's writer lost the callsign. Nothing downstream can recover it and no improvement
        // to this receiver could ever match the line, so it is its own bucket.
        if (text.Contains(UnresolvedMarker, StringComparison.Ordinal))
        {
            return PackFailure.HashedCallsignLostInTheList;
        }

        var packedSomething = false;
        var buffer = new byte[Ft8Payload.MessageBytes];

        foreach (var (to, de, extra) in Splits(text))
        {
            // A fresh cache per attempt, seeded with whatever calls the line itself spells out in
            // angle brackets. That is the same knowledge a slot would have had: the bracket means
            // the call was recovered from a hash, so something in that slot had heard it in full.
            var cache = Seeded(to, de, extra);

            if (Ft8StandardMessage.TryPack(Strip(to), Strip(de), Strip(extra), cache, buffer) == Ft8PackResult.Ok)
            {
                packedSomething = true;
                if (RoundTrips(buffer, cache, text))
                {
                    buffer.CopyTo(message, 0);
                    return PackFailure.None;
                }
            }

            if (Ft8NonstandardMessage.TryPack(Strip(to), Strip(de), Strip(extra), cache, buffer) == Ft8PackResult.Ok)
            {
                packedSomething = true;
                if (RoundTrips(buffer, cache, text))
                {
                    buffer.CopyTo(message, 0);
                    return PackFailure.None;
                }
            }
        }

        // Telemetry is eighteen hexadecimal digits and nothing else, so it is tried on its own shape
        // rather than as a field split.
        if (text.Length == TelemetryDigits && text.All(IsHexDigit))
        {
            var telemetry = new byte[TelemetryDigits / 2];
            for (var i = 0; i < telemetry.Length; i++)
            {
                telemetry[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
            }

            if (Ft8FreeText.TryPackTelemetry(telemetry, buffer) == Ft8PackResult.Ok)
            {
                packedSomething = true;
                if (RoundTrips(buffer, null, text))
                {
                    buffer.CopyTo(message, 0);
                    return PackFailure.None;
                }
            }
        }

        // Free text last, because it accepts almost anything of the right length and would otherwise
        // swallow lines that are really standard messages this library cannot pack.
        if (Ft8FreeText.TryPackText(text, buffer) == Ft8PackResult.Ok)
        {
            packedSomething = true;
            if (RoundTrips(buffer, null, text))
            {
                buffer.CopyTo(message, 0);
                return PackFailure.None;
            }
        }

        return packedSomething
            ? PackFailure.PackedButDidNotRoundTrip
            : PackFailure.NoShapeThisLibraryBuildsAcceptsIt;
    }

    /// <summary>Compares two packed messages on their 77 message bits, ignoring the three spare.</summary>
    internal static bool SameMessage(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        for (var bit = 0; bit < Ft8Payload.MessageBits; bit++)
        {
            var l = (left[bit / 8] >> (7 - (bit % 8))) & 1;
            var r = (right[bit / 8] >> (7 - (bit % 8))) & 1;
            if (l != r)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// The 77 message bits of a packed message, one byte per bit, so a payload recovered from a
    /// codeword and a payload built from text can be compared in the same currency.
    /// </summary>
    internal static byte[] MessageBits(ReadOnlySpan<byte> packed)
    {
        var bits = new byte[Ft8Payload.MessageBits];
        for (var bit = 0; bit < bits.Length; bit++)
        {
            bits[bit] = (byte)((packed[bit / 8] >> (7 - (bit % 8))) & 1);
        }

        return bits;
    }

    /// <summary>Packs 77 one-byte-per-bit values into ten bytes, most significant bit first.</summary>
    internal static byte[] FromBits(ReadOnlySpan<byte> bits)
    {
        var packed = new byte[Ft8Payload.MessageBytes];
        for (var bit = 0; bit < Ft8Payload.MessageBits && bit < bits.Length; bit++)
        {
            if (bits[bit] != 0)
            {
                packed[bit / 8] |= (byte)(0x80u >> (bit % 8));
            }
        }

        return packed;
    }

    private const int TelemetryDigits = 18;

    private static bool IsHexDigit(char c) =>
        c is (>= '0' and <= '9') or (>= 'A' and <= 'F') or (>= 'a' and <= 'f');

    private static bool RoundTrips(ReadOnlySpan<byte> packed, Ft8CallsignCache? cache, string text)
    {
        var decoded = Ft8MessageDecoder.Decode(packed, cache);
        return decoded.Decoded && string.Equals(decoded.Text.Trim(), text, StringComparison.Ordinal);
    }

    private static string Strip(string field) =>
        field.StartsWith('<') && field.EndsWith('>') && field.Length > 2 ? field[1..^1] : field;

    private static Ft8CallsignCache Seeded(params string[] fields)
    {
        var cache = new Ft8CallsignCache();
        foreach (var field in fields)
        {
            if (field.StartsWith('<') && field.EndsWith('>') && field.Length > 2)
            {
                cache.Save(field[1..^1]);
            }
        }

        return cache;
    }

    /// <summary>
    /// Every plausible way of reading a line as three fields. The addressed station can be one token
    /// or two — <c>CQ</c>, <c>CQ DX</c>, <c>CQ 123</c>, <c>CQ ABCD</c> — and the extra can be absent.
    /// </summary>
    private static IEnumerable<(string To, string De, string Extra)> Splits(string text)
    {
        var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        switch (tokens.Length)
        {
            case 1:
                yield return (tokens[0], string.Empty, string.Empty);
                break;

            case 2:
                yield return (tokens[0], tokens[1], string.Empty);
                break;

            case 3:
                yield return (tokens[0], tokens[1], tokens[2]);
                yield return ($"{tokens[0]} {tokens[1]}", tokens[2], string.Empty);
                break;

            case 4:
                yield return ($"{tokens[0]} {tokens[1]}", tokens[2], tokens[3]);
                yield return (tokens[0], tokens[1], $"{tokens[2]} {tokens[3]}");
                break;

            default:
                // Five tokens or more is not a shape any type this library builds carries as
                // fields. It still gets its chance as free text further down.
                break;
        }
    }
}

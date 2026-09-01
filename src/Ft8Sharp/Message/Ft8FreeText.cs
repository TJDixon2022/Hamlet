using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The two message types that sit on the same 71 bits under the type selector: thirteen characters
/// of free text, and eighteen hexadecimal digits of telemetry.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/message.c</c> in the pinned clone</b>, functions
/// <c>ftx_message_encode_free</c>, <c>ftx_message_decode_free</c>,
/// <c>ftx_message_encode_telemetry</c> and <c>ftx_message_decode_telemetry</c>. Both are cheap
/// because both stand on the character primitives already ported.
/// </para>
/// <para>
/// <b>Free text is a base-42 number, not a string of characters.</b> Thirteen positions over the
/// full alphabet is 42^13, which does not fit 71 bits — it needs 70.1 — so upstream carries it as
/// a long integer built by repeated multiply-and-carry, and reads it back by repeated division.
/// Ported as written: expressing it as thirteen packed indices would be the same size and a
/// different number.
/// </para>
/// <para>
/// <b>Telemetry is the same 71 bits with no interpretation at all</b>, shifted one place so they
/// sit against the bottom of nine bytes. Free text is built on top of it, which is why they share
/// this file.
/// </para>
/// <para>
/// <b>Where free text does not round-trip, it is upstream's shape and it is reported rather than
/// narrowed away.</b> The encoder pads a short string to thirteen positions with spaces and the
/// decoder trims them off again, so leading and trailing spaces do not survive; the tests measure
/// that rather than avoiding it.
/// </para>
/// </remarks>
public static class Ft8FreeText
{
    /// <summary>The number of characters a free-text message carries.</summary>
    public const int TextLength = 13;

    /// <summary>The number of bytes the 71-bit body occupies, right-aligned.</summary>
    public const int BodyBytes = 9;

    /// <summary>The number of hexadecimal digits a telemetry message prints as.</summary>
    public const int TelemetryDigits = BodyBytes * 2;

    /// <summary>The alphabet a free-text message packs against.</summary>
    private const Ft8CharTable Alphabet = Ft8CharTable.Full;

    /// <summary>
    /// Packs up to <see cref="TextLength"/> characters of free text into a 77-bit message.
    /// </summary>
    /// <param name="text">The text. Longer than <see cref="TextLength"/> is refused.</param>
    /// <param name="message"><see cref="Ft8Payload.MessageBytes"/> bytes, written only on success.</param>
    /// <remarks>
    /// <b>Every character must be in the alphabet.</b> A character outside it is refused rather
    /// than substituted — a message that silently became a different message would be a decode the
    /// operator could not tell from a real one.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="message"/> is the wrong length.</exception>
    public static Ft8PackResult TryPackText(string text, Span<byte> message)
    {
        if (message.Length != Ft8Payload.MessageBytes)
        {
            throw new ArgumentException(
                $"A message is {Ft8Payload.MessageBytes} bytes and this one is {message.Length}.",
                nameof(message));
        }

        text ??= string.Empty;

        if (text.Length > TextLength)
        {
            return Ft8PackResult.UnsupportedType;
        }

        Span<byte> body = stackalloc byte[BodyBytes];
        body.Clear();

        for (var index = 0; index < TextLength; index++)
        {
            // Short text is padded to the full width with spaces, which is why leading and trailing
            // spaces do not survive a round trip.
            var c = index < text.Length ? text[index] : ' ';
            var code = Ft8Text.Index(c, Alphabet);
            if (code < 0)
            {
                return Ft8PackResult.UnsupportedType;
            }

            // A long multiply by the alphabet size, carried from the bottom byte upward.
            var carry = (uint)code;
            for (var i = BodyBytes - 1; i >= 0; i--)
            {
                carry += (uint)(body[i] * 42);
                body[i] = (byte)(carry & 0xFF);
                carry >>= 8;
            }
        }

        WriteBody(body, message);

        // Free text is both selectors at zero. The secondary's high bit lives in the bit the shift
        // has already vacated, and it stays clear.
        message[9] = 0;
        return Ft8PackResult.Ok;
    }

    /// <summary>
    /// Reads a free-text message back out of its 77 bits, or refuses a body that is not thirteen
    /// characters of this alphabet.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Never throws for a correctly sized buffer</b>, whatever is in it.
    /// </para>
    /// <para>
    /// <b>A body outside the type's range is refused, and that is a deliberate divergence.</b>
    /// Thirteen positions over a 42-character alphabet is fewer values than 71 bits can hold, so
    /// rather more than half of all bodies are numbers no free-text message could have been. The
    /// repeated division still produces thirteen characters for them, and upstream shows those
    /// characters; what it is showing is the low part of a number, not the message that was sent.
    /// Refusing is HM-DEC-009 — the text would be a guess presented as a decode — and the check is
    /// a range test on the body rather than a comparison against a re-pack, so it is not the test
    /// asserting its own conclusion.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="message"/> is the wrong length.</exception>
    public static Ft8DecodeStatus TryUnpackText(ReadOnlySpan<byte> message, out string text)
    {
        text = string.Empty;

        Span<byte> body = stackalloc byte[BodyBytes];
        UnpackTelemetry(message, body);

        Span<char> characters = stackalloc char[TextLength];

        for (var index = TextLength - 1; index >= 0; index--)
        {
            // Divide the long integer by the alphabet size, from the top byte down, keeping the
            // remainder as this position's character.
            uint remainder = 0;
            for (var i = 0; i < BodyBytes; i++)
            {
                remainder = (remainder << 8) | body[i];
                body[i] = (byte)(remainder / 42);
                remainder %= 42;
            }

            characters[index] = Ft8Text.Character((int)remainder, Alphabet);
        }

        // Thirteen divisions have taken out everything a free-text message can carry. Anything
        // still here means the body was a larger number than this type has room for.
        foreach (var leftover in body)
        {
            if (leftover != 0)
            {
                return Ft8DecodeStatus.MalformedField;
            }
        }

        text = characters.Trim(' ').ToString();
        return Ft8DecodeStatus.Decoded;
    }

    /// <summary>
    /// Packs a 71-bit telemetry body into a 77-bit message, shifted one place left to right-align
    /// the data as upstream does.
    /// </summary>
    /// <param name="telemetry"><see cref="BodyBytes"/> bytes. Only the low 71 bits are carried.</param>
    /// <param name="message"><see cref="Ft8Payload.MessageBytes"/> bytes, written in full.</param>
    /// <remarks>
    /// <para>
    /// <b>Upstream's own telemetry packer sets no type selectors at all</b>, and says so in a
    /// comment asking whether it or the caller should. A message it produces therefore declares
    /// itself free text rather than telemetry. This library sets them, because a packer that
    /// produces a message of the wrong type is a packer whose output cannot be read back — and the
    /// bit the secondary selector needs is exactly the one the left shift has just vacated, which
    /// is what the shift is for.
    /// </para>
    /// <para>
    /// Only the low 71 bits of <paramref name="telemetry"/> are carried; the top bit of its first
    /// byte is outside the field and is dropped.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">Either span is the wrong length.</exception>
    public static Ft8PackResult TryPackTelemetry(ReadOnlySpan<byte> telemetry, Span<byte> message)
    {
        if (telemetry.Length != BodyBytes)
        {
            throw new ArgumentException(
                $"A telemetry body is {BodyBytes} bytes and this one is {telemetry.Length}.",
                nameof(telemetry));
        }

        if (message.Length != Ft8Payload.MessageBytes)
        {
            throw new ArgumentException(
                $"A message is {Ft8Payload.MessageBytes} bytes and this one is {message.Length}.",
                nameof(message));
        }

        WriteBody(telemetry, message);

        // Primary selector zero, secondary selector five: its high bit is the one the shift
        // vacated at the bottom of the ninth byte, and its low two are at the top of the last.
        message[8] = (byte)(message[8] | 0x01);
        message[9] = 0x40;

        return Ft8PackResult.Ok;
    }

    /// <summary>
    /// Writes a 71-bit body into the message shifted one place left, leaving the bit at the bottom
    /// of the ninth byte and the whole of the last byte for the type selectors.
    /// </summary>
    private static void WriteBody(ReadOnlySpan<byte> body, Span<byte> message)
    {
        message.Clear();

        byte carry = 0;
        for (var i = BodyBytes - 1; i >= 0; i--)
        {
            message[i] = (byte)((body[i] << 1) | (carry >> 7));
            carry = (byte)(body[i] & 0x80);
        }
    }

    /// <summary>Reads the 71-bit telemetry body back out of a message.</summary>
    /// <exception cref="ArgumentException">Either span is the wrong length.</exception>
    public static void UnpackTelemetry(ReadOnlySpan<byte> message, Span<byte> telemetry)
    {
        if (message.Length != Ft8Payload.MessageBytes)
        {
            throw new ArgumentException(
                $"A message is {Ft8Payload.MessageBytes} bytes and this one is {message.Length}.",
                nameof(message));
        }

        if (telemetry.Length != BodyBytes)
        {
            throw new ArgumentException(
                $"A telemetry body is {BodyBytes} bytes and this one is {telemetry.Length}.",
                nameof(telemetry));
        }

        byte carry = 0;
        for (var i = 0; i < BodyBytes; i++)
        {
            telemetry[i] = (byte)((carry << 7) | (message[i] >> 1));
            carry = (byte)(message[i] & 0x01);
        }
    }

    /// <summary>The telemetry body as the hexadecimal string upstream prints it as.</summary>
    /// <exception cref="ArgumentException"><paramref name="message"/> is the wrong length.</exception>
    public static string UnpackTelemetryHex(ReadOnlySpan<byte> message)
    {
        Span<byte> body = stackalloc byte[BodyBytes];
        UnpackTelemetry(message, body);

        Span<char> hex = stackalloc char[TelemetryDigits];
        for (var i = 0; i < BodyBytes; i++)
        {
            hex[i * 2] = HexDigit(body[i] >> 4);
            hex[(i * 2) + 1] = HexDigit(body[i] & 0x0F);
        }

        return hex.ToString();
    }

    private static char HexDigit(int nibble) =>
        nibble > 9 ? (char)(nibble - 10 + 'A') : (char)(nibble + '0');
}

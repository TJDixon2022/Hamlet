using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The one entry point that takes 77 bits and returns either a message or a refusal that says why.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the type the prime directive is about.</b> Every pattern that arrives here is
/// something a demodulator thought might be a message; most of them, in noise, are not. A decoder
/// that guesses fills the operator's screen with plausible callsigns that were never on the air.
/// So the promise here is absolute and is tested rather than argued for:
/// </para>
/// <list type="bullet">
///   <item><description><b>It never throws</b>, for any ten bytes.</description></item>
///   <item><description>
///     <b>It never returns a message for a type it has not built.</b> Every combination of the two
///     type selectors has a defined answer — a decode or a refusal — and there is no third
///     outcome.
///   </description></item>
///   <item><description>
///     <b>It never returns a message with a callsign it could not resolve.</b> A hashed callsign
///     without the rolling cache is refused as unresolved, whole; there is no placeholder, no
///     partial message, and no numeric field dressed as a call.
///   </description></item>
/// </list>
/// <para>
/// <b>A refusal is not an exception and not an error.</b> Most of what a receiver hands this is
/// noise that passed a checksum by accident or a message type this library does not read yet, and
/// saying so plainly is the correct answer in both cases.
/// </para>
/// </remarks>
public static class Ft8MessageDecoder
{
    /// <summary>The number of bytes a packed message occupies.</summary>
    public const int MessageBytes = Ft8Payload.MessageBytes;

    /// <summary>
    /// Reads a 77-bit message, or says why it cannot.
    /// </summary>
    /// <param name="message"><see cref="MessageBytes"/> bytes.</param>
    /// <remarks>
    /// <b>The three bits past the seventy-seventh are ignored rather than refused.</b> A caller who
    /// got these bytes out of <see cref="Ft8Payload.TryRead"/> has already had them checked; a
    /// caller who assembled them itself is asking what the message bits say, and the answer does
    /// not depend on the padding.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="message"/> is the wrong length.</exception>
    public static Ft8DecodeResult Decode(ReadOnlySpan<byte> message) => Decode(message, null);

    /// <summary>
    /// Reads a 77-bit message, resolving hashed callsigns through the cache, or says why it cannot.
    /// </summary>
    /// <param name="message"><see cref="MessageBytes"/> bytes.</param>
    /// <param name="cache">
    /// The rolling cache of callsigns heard so far, or <see langword="null"/> for none. Passing
    /// <see langword="null"/> is the same as passing a cache that has heard nothing, and every
    /// message carrying a hashed call is refused as unresolved.
    /// </param>
    /// <remarks>
    /// <para>
    /// <b>Decoding fills the cache as well as reading it.</b> A message that spells a callsign out
    /// teaches the cache that call, which is what lets the next message name that station by its
    /// hash alone. The cache is therefore modified by a successful decode, and by construction it is
    /// the caller's object rather than a shared one, so what one decoder learns cannot leak into
    /// another.
    /// </para>
    /// <para>
    /// <b>The three promises above hold with a cache exactly as they hold without one.</b> A hash
    /// the cache has not heard is refused; a hash two cached callsigns share is refused; and neither
    /// writes a character.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="message"/> is the wrong length.</exception>
    public static Ft8DecodeResult Decode(ReadOnlySpan<byte> message, Ft8CallsignCache? cache)
    {
        if (message.Length != MessageBytes)
        {
            throw new ArgumentException(
                $"A message is {MessageBytes} bytes and this one is {message.Length}.", nameof(message));
        }

        var type = Ft8MessageTypes.TypeOf(message);

        switch (type)
        {
            case Ft8MessageType.Standard:
            {
                var status = Ft8StandardMessage.TryUnpack(message, cache, out var fields);
                return status == Ft8DecodeStatus.Decoded
                    ? Ft8DecodeResult.Message(type, fields.Text, fields)
                    : Ft8DecodeResult.Refusal(type, status);
            }

            case Ft8MessageType.FreeText:
            {
                var status = Ft8FreeText.TryUnpackText(message, out var text);
                return status == Ft8DecodeStatus.Decoded
                    ? Ft8DecodeResult.Message(type, text, default)
                    : Ft8DecodeResult.Refusal(type, status);
            }

            case Ft8MessageType.Telemetry:
                // Every 71-bit body is telemetry: the type carries raw bits and interprets none of
                // them, so there is nothing here that could be malformed.
                return Ft8DecodeResult.Message(type, Ft8FreeText.UnpackTelemetryHex(message), default);

            default:
                // Everything this library has not built, by name. A correct answer, and the only
                // honest one available until the type is ported.
                return Ft8DecodeResult.Refusal(type, Ft8DecodeStatus.UnsupportedType);
        }
    }
}

/// <summary>Why a 77-bit pattern did or did not become a message.</summary>
public enum Ft8DecodeStatus
{
    /// <summary>It became a message.</summary>
    Decoded,

    /// <summary>Its type is one this library does not read.</summary>
    UnsupportedType,

    /// <summary>
    /// It carries a callsign held as a hash, and resolving one needs the rolling cache this
    /// library does not have.
    /// </summary>
    UnresolvedCallsign,

    /// <summary>One of its fields holds a value the protocol does not define.</summary>
    MalformedField,
}

/// <summary>
/// What the dispatcher made of 77 bits: either a message, or a refusal naming the reason.
/// </summary>
/// <remarks>
/// <b>The two are not the same shape and cannot be confused.</b> <see cref="Text"/> is empty and
/// <see cref="Fields"/> is default on every refusal, so a caller that ignores
/// <see cref="Status"/> gets nothing to display rather than something that looks like a decode.
/// </remarks>
public readonly struct Ft8DecodeResult
{
    private Ft8DecodeResult(Ft8MessageType type, Ft8DecodeStatus status, string text, Ft8StandardFields fields)
    {
        Type = type;
        Status = status;
        Text = text;
        Fields = fields;
    }

    /// <summary>The type the message declared itself to be, whether or not it could be read.</summary>
    public Ft8MessageType Type { get; }

    /// <summary>Whether it decoded, and if not, why not.</summary>
    public Ft8DecodeStatus Status { get; }

    /// <summary>The message as an operator would read it, or the empty string on a refusal.</summary>
    public string Text { get; }

    /// <summary>The decoded fields, or the default on a refusal.</summary>
    public Ft8StandardFields Fields { get; }

    /// <summary>Whether this is a message rather than a refusal.</summary>
    public bool Decoded => Status == Ft8DecodeStatus.Decoded;

    internal static Ft8DecodeResult Message(Ft8MessageType type, string text, Ft8StandardFields fields) =>
        new(type, Ft8DecodeStatus.Decoded, text, fields);

    internal static Ft8DecodeResult Refusal(Ft8MessageType type, Ft8DecodeStatus status) =>
        new(type, status, string.Empty, default);
}

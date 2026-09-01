using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The standard message — two callsigns, two suffix flags, an <c>R</c> flag and a grid-or-report
/// field — packed into the 77 bits that go on the air, and read back out of them.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/message.c</c> in the pinned clone</b>, functions
/// <c>ftx_message_encode_std</c> and <c>ftx_message_decode_std</c>. This is the type that carries
/// most of what is on the band.
/// </para>
/// <para>
/// <b>This works on the 77-bit message and nothing else.</b> It computes no checksum, it does not
/// touch the encoder, and it does not reimplement the container: <see cref="Ft8Payload.Create"/> is
/// what carries these bits onward. The three bits past the seventy-seventh are never written, so a
/// message this produces never trips that container's own refusal — asserted directly rather than
/// discovered through an exception.
/// </para>
/// <para>
/// <b>Two type codes, one packing.</b> The primary selector says whether a suffix flag means
/// <c>/R</c> or <c>/P</c>; the bits either side of it are identical. Both are built, and the
/// round-trip corpus covers both.
/// </para>
/// </remarks>
public static class Ft8StandardMessage
{
    /// <summary>The number of bytes a packed message occupies.</summary>
    public const int MessageBytes = Ft8Payload.MessageBytes;

    /// <summary>
    /// Packs two callsigns and a grid, report or token into the 77 bits of a standard message.
    /// </summary>
    /// <param name="callTo">The addressed station, or one of the tokens the field admits.</param>
    /// <param name="callDe">The transmitting station.</param>
    /// <param name="extra">A grid square, a signal report, one of three tokens, or nothing.</param>
    /// <param name="message"><see cref="MessageBytes"/> bytes, written only on success.</param>
    /// <remarks>
    /// <b>Never throws</b> for a correctly sized buffer, whatever the strings are. Every refusal
    /// says which field it was about.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="message"/> is the wrong length.</exception>
    public static Ft8PackResult TryPack(string callTo, string callDe, string extra, Span<byte> message)
    {
        if (message.Length != MessageBytes)
        {
            throw new ArgumentException(
                $"A message is {MessageBytes} bytes and this one is {message.Length}.", nameof(message));
        }

        callTo ??= string.Empty;
        callDe ??= string.Empty;

        var toResult = Ft8CallsignField.TryPack(callTo, out var n28a, out var suffixA);
        if (toResult != Ft8FieldResult.Ok)
        {
            return toResult == Ft8FieldResult.RequiresHashCache
                ? Ft8PackResult.FirstCallRequiresHashCache
                : Ft8PackResult.FirstCallInvalid;
        }

        var deResult = Ft8CallsignField.TryPack(callDe, out var n28b, out var suffixB);
        if (deResult != Ft8FieldResult.Ok)
        {
            return deResult == Ft8FieldResult.RequiresHashCache
                ? Ft8PackResult.SecondCallRequiresHashCache
                : Ft8PackResult.SecondCallInvalid;
        }

        var i3 = Ft8MessageTypes.PrimaryStandard;
        if (EndsWith(callTo, "/P") || EndsWith(callDe, "/P"))
        {
            i3 = Ft8MessageTypes.PrimaryStandardWithP;
            if (EndsWith(callTo, "/R") || EndsWith(callDe, "/R"))
            {
                // One message carries one suffix meaning, so a /P and a /R together cannot be said.
                return Ft8PackResult.SuffixConflict;
            }
        }

        // A calling station cannot address a non-standard call in this message type — that needs
        // the type built on a hashed call, which this library does not have.
        var slash = callDe.IndexOf('/');
        var isCq = callTo == "CQ" || StartsWith(callTo, "CQ ");
        if (slash >= 2 && isCq && callDe[slash..] is not ("/P" or "/R"))
        {
            return Ft8PackResult.SecondCallRequiresHashCache;
        }

        var grid = Ft8GridField.Pack(extra);

        var n29a = (n28a << 1) | (suffixA ? 1u : 0u);
        var n29b = (n28b << 1) | (suffixB ? 1u : 0u);

        // Upstream re-applies the addressed station's suffix flag here having already had it from
        // the field packer. Kept as written: it is a no-op for the flag and it is what decides the
        // type code for a /P.
        if (EndsWith(callTo, "/R"))
        {
            n29a |= 1;
        }
        else if (EndsWith(callTo, "/P"))
        {
            n29a |= 1;
            i3 = Ft8MessageTypes.PrimaryStandardWithP;
        }

        // (28 + 1) + (28 + 1) + (1 + 15) + 3 bits. The R flag rides in with the grid field, in the
        // bit above it, exactly as upstream's packer carries the two together.
        message[0] = (byte)(n29a >> 21);
        message[1] = (byte)(n29a >> 13);
        message[2] = (byte)(n29a >> 5);
        message[3] = (byte)((n29a << 3) | ((uint)n29b >> 26));
        message[4] = (byte)(n29b >> 18);
        message[5] = (byte)(n29b >> 10);
        message[6] = (byte)(n29b >> 2);
        message[7] = (byte)((n29b << 6) | ((uint)grid >> 10));
        message[8] = (byte)(grid >> 2);
        message[9] = (byte)((grid << 6) | (i3 << 3));

        return Ft8PackResult.Ok;
    }

    /// <summary>
    /// Reads a standard message back out of its 77 bits.
    /// </summary>
    /// <param name="message"><see cref="MessageBytes"/> bytes.</param>
    /// <param name="decoded">The three fields, written only on success.</param>
    /// <remarks>
    /// <b>Never throws for a correctly sized buffer, whatever is in it.</b> That property is what
    /// the dispatcher's promise rests on. A callsign field this library cannot resolve without the
    /// rolling hash cache is refused as unresolved, and nothing partial is returned.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="message"/> is the wrong length.</exception>
    public static Ft8DecodeStatus TryUnpack(ReadOnlySpan<byte> message, out Ft8StandardFields decoded)
    {
        decoded = default;

        if (message.Length != MessageBytes)
        {
            throw new ArgumentException(
                $"A message is {MessageBytes} bytes and this one is {message.Length}.", nameof(message));
        }

        var n29a = ((uint)message[0] << 21)
            | ((uint)message[1] << 13)
            | ((uint)message[2] << 5)
            | ((uint)message[3] >> 3);

        var n29b = (((uint)message[3] & 0x07u) << 26)
            | ((uint)message[4] << 18)
            | ((uint)message[5] << 10)
            | ((uint)message[6] << 2)
            | ((uint)message[7] >> 6);

        var reportFlag = (message[7] & 0x20) != 0;
        var grid = ((message[7] & 0x1F) << 10) | (message[8] << 2) | (message[9] >> 6);

        var i3 = Ft8MessageTypes.Primary(message);

        var toResult = Ft8CallsignField.TryUnpack(
            n29a >> 1, (n29a & 1u) != 0, i3, out var callTo, out var toType);
        if (toResult != Ft8FieldResult.Ok)
        {
            return toResult == Ft8FieldResult.UnresolvedCallsign
                ? Ft8DecodeStatus.UnresolvedCallsign
                : Ft8DecodeStatus.MalformedField;
        }

        var deResult = Ft8CallsignField.TryUnpack(
            n29b >> 1, (n29b & 1u) != 0, i3, out var callDe, out var deType);
        if (deResult != Ft8FieldResult.Ok)
        {
            return deResult == Ft8FieldResult.UnresolvedCallsign
                ? Ft8DecodeStatus.UnresolvedCallsign
                : Ft8DecodeStatus.MalformedField;
        }

        var extraResult = Ft8GridField.TryUnpack(grid, reportFlag, out var extra, out var extraType);
        if (extraResult != Ft8FieldResult.Ok)
        {
            return Ft8DecodeStatus.MalformedField;
        }

        decoded = new Ft8StandardFields(callTo, toType, callDe, deType, extra, extraType);
        return Ft8DecodeStatus.Decoded;
    }

    private static bool StartsWith(string text, string prefix) =>
        text.AsSpan().StartsWith(prefix.AsSpan(), StringComparison.Ordinal);

    private static bool EndsWith(string text, string suffix) =>
        text.AsSpan().EndsWith(suffix.AsSpan(), StringComparison.Ordinal);
}

/// <summary>The three fields of a decoded standard message, with what each one turned out to be.</summary>
/// <param name="CallTo">The addressed station or token.</param>
/// <param name="CallToType">What the first field is.</param>
/// <param name="CallDe">The transmitting station.</param>
/// <param name="CallDeType">What the second field is.</param>
/// <param name="Extra">The grid, report or token, or the empty string where there is none.</param>
/// <param name="ExtraType">What the third field is.</param>
public readonly record struct Ft8StandardFields(
    string CallTo,
    Ft8FieldType CallToType,
    string CallDe,
    Ft8FieldType CallDeType,
    string Extra,
    Ft8FieldType ExtraType)
{
    /// <summary>The three fields joined the way an operator would read them.</summary>
    /// <remarks>
    /// The third is left off entirely when the message carries none, rather than joined as a
    /// trailing space — a message with no report is a message with two fields.
    /// </remarks>
    public string Text => string.IsNullOrEmpty(Extra) ? $"{CallTo} {CallDe}" : $"{CallTo} {CallDe} {Extra}";
}

/// <summary>What happened when a message was packed.</summary>
public enum Ft8PackResult
{
    /// <summary>The message was packed.</summary>
    Ok,

    /// <summary>The addressed station is not something this message type can carry.</summary>
    FirstCallInvalid,

    /// <summary>The transmitting station is not something this message type can carry.</summary>
    SecondCallInvalid,

    /// <summary>
    /// The addressed station is a non-standard callsign, which packs as a hash and needs the
    /// rolling cache this library does not have.
    /// </summary>
    FirstCallRequiresHashCache,

    /// <summary>
    /// The transmitting station is a non-standard callsign, which packs as a hash and needs the
    /// rolling cache this library does not have.
    /// </summary>
    SecondCallRequiresHashCache,

    /// <summary>
    /// One call carries <c>/P</c> and the other <c>/R</c>, and one message carries one suffix
    /// meaning.
    /// </summary>
    SuffixConflict,

    /// <summary>The text is not of a type this library builds.</summary>
    UnsupportedType,
}

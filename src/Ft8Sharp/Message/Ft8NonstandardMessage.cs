using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The message that carries one non-standard callsign in full and names the other station only by a
/// twelve-bit hash of its call.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/message.c</c> in the pinned clone</b>, functions
/// <c>ftx_message_encode_nonstd</c> and <c>ftx_message_decode_nonstd</c>, together with the
/// fifty-eight-bit callsign packer and unpacker they sit on. It works on the 77-bit message and
/// nothing else: it computes no checksum, does not touch the encoder, and does not reimplement
/// <see cref="Ft8Payload"/>. The three bits past the seventy-seventh are never written.
/// </para>
/// <para>
/// <b>This is the type the whole hash exists for.</b> Twelve bits is nowhere near enough to hold a
/// callsign, and it is not meant to: it is a reminder of a call the receiver is expected to have
/// heard spelled out earlier in the same session. That expectation is the rolling cache, and where
/// the cache cannot meet it — because it never heard the station, or because it heard two whose
/// calls share the hash — <b>the whole message is refused</b>, exactly as
/// <see cref="Ft8StandardMessage"/> already refuses one whose 22-bit hashed field cannot be
/// resolved.
/// </para>
/// <para>
/// <b>The fifty-eight-bit field is the honest half.</b> It carries eleven characters of an
/// alphabet of thirty-eight, which is a callsign and not a hash of one, and it round-trips with no
/// cache at all. A CQ under this type has no hashed companion — the twelve bits are written as zero
/// and never read — so a general call from a station with a portable prefix is fully readable by a
/// receiver that has heard nothing.
/// </para>
/// </remarks>
public static class Ft8NonstandardMessage
{
    /// <summary>The number of bytes a packed message occupies.</summary>
    public const int MessageBytes = Ft8Payload.MessageBytes;

    /// <summary>The width of the hashed callsign field.</summary>
    public const int HashBits = Ft8CallsignHash.Bits12;

    /// <summary>The width of the field that carries a callsign in full.</summary>
    public const int CallBits = 58;

    /// <summary>The number of characters the 58-bit field can hold.</summary>
    /// <remarks>
    /// The same eleven the hash reads, which is not a coincidence: thirty-eight to the eleventh is
    /// what fits in fifty-eight bits, and it is why a non-standard callsign is limited to eleven
    /// characters in the first place.
    /// </remarks>
    public const int CallLength = Ft8CallsignHash.MaxCallsignLength;

    /// <summary>The base the 58-bit field packs its characters against.</summary>
    public const ulong CallBase = Ft8CallsignHash.PackingBase;

    /// <summary>The shortest thing either field will accept as a callsign.</summary>
    public const int MinimumCallLength = 3;

    /// <summary>The report code meaning no report at all.</summary>
    public const int ReportNone = 0;

    /// <summary>The report code for <c>RRR</c>.</summary>
    public const int ReportRrr = 1;

    /// <summary>The report code for <c>RR73</c>.</summary>
    public const int ReportRr73 = 2;

    /// <summary>The report code for <c>73</c>.</summary>
    public const int ReportSeventyThree = 3;

    /// <summary>
    /// Packs a non-standard-callsign message into the 77 bits that go on the air.
    /// </summary>
    /// <param name="callTo">
    /// The addressed station, or <c>CQ</c>. Written as a bare callsign rather than in angle
    /// brackets — see the remarks.
    /// </param>
    /// <param name="callDe">The transmitting station, which is the one carried in full.</param>
    /// <param name="extra">One of <c>RRR</c>, <c>RR73</c> or <c>73</c>, or nothing.</param>
    /// <param name="cache">
    /// The rolling cache. Both callsigns are stored in it, because a station that transmits a
    /// message like this one has to be able to read the reply. Where it is <see langword="null"/>
    /// and the message needs a hash, the message is refused as
    /// <see cref="Ft8PackResult.FirstCallRequiresHashCache"/>.
    /// </param>
    /// <param name="message"><see cref="MessageBytes"/> bytes, written only on success.</param>
    /// <remarks>
    /// <para>
    /// <b>A callsign in angle brackets is refused here, and that is upstream's behaviour rather than
    /// a choice.</b> The pin's own header comment writes this message type's example with the
    /// hashed call bracketed, but the angle bracket is not in the alphabet its hash packs against,
    /// so its own packer refuses a bracketed call. Brackets are an output convention: they mark a
    /// call that was recovered from a hash rather than read out of the bits. Reported rather than
    /// repaired, because repairing it would change what this library puts on the air relative to
    /// every other station.
    /// </para>
    /// <para>
    /// <b>Never throws</b> for a correctly sized buffer, whatever the strings are.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="message"/> is the wrong length.</exception>
    public static Ft8PackResult TryPack(
        string callTo,
        string callDe,
        string extra,
        Ft8CallsignCache? cache,
        Span<byte> message)
    {
        if (message.Length != MessageBytes)
        {
            throw new ArgumentException(
                $"A message is {MessageBytes} bytes and this one is {message.Length}.", nameof(message));
        }

        callTo ??= string.Empty;
        callDe ??= string.Empty;
        extra ??= string.Empty;

        var isCq = callTo == "CQ" || StartsWith(callTo, "CQ ");

        if (!isCq && callTo.Length < MinimumCallLength)
        {
            return Ft8PackResult.FirstCallInvalid;
        }

        if (callDe.Length < MinimumCallLength)
        {
            return Ft8PackResult.SecondCallInvalid;
        }

        var flip = false;
        uint hash12 = 0;
        string call58;

        if (!isCq)
        {
            // Which of the two callsigns goes in full and which goes as a hash. Upstream reads the
            // *addressed* call's length while indexing the *transmitting* call's characters, which
            // is a defect in the pin; see NonstandardFlipIsUpstreamsIndexing below for exactly what
            // is reproduced and what is not.
            flip = LooksBracketed(callDe, callTo.Length);

            var call12 = flip ? callDe : callTo;
            call58 = flip ? callTo : callDe;

            if (cache is null)
            {
                // The twelve bits would be a hash nothing could compute back into a call.
                return Ft8PackResult.FirstCallRequiresHashCache;
            }

            var stored = cache.Save(call12, out _, out hash12, out _);
            if (stored is Ft8CacheStore.NotHashable or Ft8CacheStore.TooShort)
            {
                return Ft8PackResult.FirstCallInvalid;
            }
        }
        else
        {
            // A general call names nobody, so there is no hashed companion and no cache is needed.
            call58 = callDe;
        }

        if (!TryPackCall(call58, cache, out var n58))
        {
            return Ft8PackResult.SecondCallInvalid;
        }

        var report = isCq ? ReportNone : ReportOf(extra);

        // 12 + 58 + 1 + 2 + 1 + 3 == 77 bits.
        var flipBit = flip ? 1u : 0u;
        var cqBit = isCq ? 1u : 0u;

        message[0] = (byte)(hash12 >> 4);
        message[1] = (byte)((hash12 << 4) | (uint)(n58 >> 54));
        message[2] = (byte)(n58 >> 46);
        message[3] = (byte)(n58 >> 38);
        message[4] = (byte)(n58 >> 30);
        message[5] = (byte)(n58 >> 22);
        message[6] = (byte)(n58 >> 14);
        message[7] = (byte)(n58 >> 6);
        message[8] = (byte)((n58 << 2) | (flipBit << 1) | ((uint)report >> 1));
        message[9] = (byte)(((uint)report << 7) | (cqBit << 6) | (Ft8MessageTypes.PrimaryNonstandard << 3));

        return Ft8PackResult.Ok;
    }

    /// <summary>
    /// Reads a non-standard-callsign message back out of its 77 bits.
    /// </summary>
    /// <param name="message"><see cref="MessageBytes"/> bytes.</param>
    /// <param name="cache">
    /// The rolling cache, or <see langword="null"/> for none. The callsign carried in full is stored
    /// in it, which is how a receiver comes to be able to resolve that station's hash later.
    /// </param>
    /// <param name="decoded">The three fields, written only on success.</param>
    /// <remarks>
    /// <b>Never throws for a correctly sized buffer, whatever is in it.</b> A hashed companion the
    /// cache cannot resolve — because it has not heard the station, or because two stations it has
    /// heard share the hash — refuses the whole message. Upstream writes a literal placeholder into
    /// the field instead and returns the message with a station's name missing from it.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="message"/> is the wrong length.</exception>
    public static Ft8DecodeStatus TryUnpack(
        ReadOnlySpan<byte> message,
        Ft8CallsignCache? cache,
        out Ft8StandardFields decoded)
    {
        decoded = default;

        if (message.Length != MessageBytes)
        {
            throw new ArgumentException(
                $"A message is {MessageBytes} bytes and this one is {message.Length}.", nameof(message));
        }

        var hash12 = ((uint)message[0] << 4) | ((uint)message[1] >> 4);

        var n58 = ((ulong)(message[1] & 0x0F) << 54)
            | ((ulong)message[2] << 46)
            | ((ulong)message[3] << 38)
            | ((ulong)message[4] << 30)
            | ((ulong)message[5] << 22)
            | ((ulong)message[6] << 14)
            | ((ulong)message[7] << 6)
            | ((ulong)message[8] >> 2);

        var flip = (message[8] & 0x02) != 0;
        var report = ((message[8] & 0x01) << 1) | (message[9] >> 7);
        var isCq = (message[9] & 0x40) != 0;

        // Which of the two decoded calls each field ends up holding, and therefore which of the two
        // sources actually has to succeed. Upstream computes both regardless and then picks; picking
        // first is the same message with one fewer way to return something nothing needed.
        var needsFullCall = !flip || !isCq;
        var needsHashedCall = flip || !isCq;

        var validFullCall = TryUnpackCall(n58, out var fullCall);
        if (needsFullCall && !validFullCall)
        {
            // Fewer than three characters is not a callsign. Upstream ignores this failure and uses
            // the text anyway, which puts a one or two character station on the screen.
            return Ft8DecodeStatus.MalformedField;
        }

        // THE LOOKUP COMES BEFORE THE STORE, AND UPSTREAM'S DOES NOT. A hashed field names a station
        // the receiver is expected to have heard already, so it is resolved against what the receiver
        // knew before this message arrived. Upstream stores this message's own spelled-out call
        // first, so where the two calls in one message happen to share a 12-bit hash — which happens
        // once in about four thousand messages — its lookup finds the call the message is carrying in
        // full and reports the addressed station as the transmitting one. Recorded in
        // porting-notes.md; it refuses fewer messages than the alternative rather than more.
        var hashedCall = string.Empty;
        var unresolved = false;
        if (needsHashedCall)
        {
            if (cache is null
                || cache.TryLookup(Ft8CallsignHashWidth.Bits12, hash12, out var resolved) != Ft8CacheLookup.Found)
            {
                unresolved = true;
            }
            else
            {
                hashedCall = Ft8CallsignField.Bracket(resolved);
            }
        }

        // Now remember the call this message spelled out, which is how the next message gets to name
        // that station by its hash alone. Remembered even where the message is about to be refused:
        // the call was really in these bits and the checksum passed, and a receiver that threw it
        // away would never warm up from the very messages it cannot yet read.
        if (validFullCall)
        {
            cache?.Save(fullCall);
        }

        if (unresolved)
        {
            return Ft8DecodeStatus.UnresolvedCallsign;
        }

        var first = flip ? fullCall : hashedCall;
        var second = flip ? hashedCall : fullCall;

        string callTo;
        Ft8FieldType callToType;
        string extra;
        Ft8FieldType extraType;

        if (isCq)
        {
            callTo = "CQ";
            callToType = Ft8FieldType.Token;
            extra = string.Empty;
            extraType = Ft8FieldType.None;
        }
        else
        {
            callTo = first;
            callToType = Ft8FieldType.Callsign;
            switch (report)
            {
                case ReportRrr:
                    extra = "RRR";
                    extraType = Ft8FieldType.Token;
                    break;
                case ReportRr73:
                    extra = "RR73";
                    extraType = Ft8FieldType.Token;
                    break;
                case ReportSeventyThree:
                    extra = "73";
                    extraType = Ft8FieldType.Token;
                    break;
                default:
                    extra = string.Empty;
                    extraType = Ft8FieldType.None;
                    break;
            }
        }

        decoded = new Ft8StandardFields(callTo, callToType, second, Ft8FieldType.Callsign, extra, extraType);
        return Ft8DecodeStatus.Decoded;
    }

    /// <summary>
    /// Packs a callsign into the 58-bit field, and stores it in the cache on the way past.
    /// </summary>
    /// <remarks>
    /// <b>One divergence, recorded.</b> Upstream reads eleven characters and stops without checking
    /// whether there were more, so a twelve-character call is silently packed as its first eleven —
    /// a wrong callsign, written as though it were certain. This refuses instead. Everything else is
    /// upstream's: a leading angle bracket is skipped, a second one ends the call, and any character
    /// outside the alphabet refuses.
    /// </remarks>
    private static bool TryPackCall(string callsign, Ft8CallsignCache? cache, out ulong value)
    {
        value = 0;

        var from = callsign.Length > 0 && callsign[0] == '<' ? 1 : 0;
        var length = 0;
        ulong packed = 0;
        Span<char> read = stackalloc char[CallLength];

        for (var i = from; i < callsign.Length; i++)
        {
            var c = callsign[i];
            if (c == '<')
            {
                break;
            }

            if (length == CallLength)
            {
                // More characters than the field can hold. Upstream truncates here.
                return false;
            }

            var index = Ft8Text.Index(c, Ft8CharTable.AlphanumericSpaceSlash);
            if (index < 0)
            {
                return false;
            }

            read[length] = c;
            packed = (packed * CallBase) + (ulong)index;
            length++;
        }

        if (length < MinimumCallLength)
        {
            return false;
        }

        // Upstream stores the call it read rather than the call it was handed, which is the same
        // thing less any brackets that were stripped above.
        cache?.Save(new string(read[..length]));

        value = packed;
        return true;
    }

    /// <summary>
    /// Unpacks a callsign out of the 58-bit field.
    /// </summary>
    /// <remarks>
    /// The eleven characters come out right-aligned, because a shorter call packed to a smaller
    /// number and the leading positions are the alphabet's space. Trimming them off is what recovers
    /// the call. <b>Nothing is stored here</b>; the caller stores it after the hashed field has been
    /// resolved, for the reason given at that call site.
    /// </remarks>
    private static bool TryUnpackCall(ulong value, out string callsign)
    {
        Span<char> text = stackalloc char[CallLength];
        var n = value;
        for (var i = CallLength - 1; ; i--)
        {
            text[i] = Ft8Text.Character((int)(n % CallBase), Ft8CharTable.AlphanumericSpaceSlash);
            if (i == 0)
            {
                break;
            }

            n /= CallBase;
        }

        callsign = text.Trim(' ').ToString();
        return callsign.Length >= MinimumCallLength;
    }

    /// <summary>The report code for a third field, or <see cref="ReportNone"/> for anything else.</summary>
    private static int ReportOf(string extra) => extra switch
    {
        "RRR" => ReportRrr,
        "RR73" => ReportRr73,
        "73" => ReportSeventyThree,
        _ => ReportNone,
    };

    /// <summary>
    /// Whether the transmitting call reads as bracketed, using upstream's own indexing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This reproduces a defect in the pin, deliberately, and stops exactly where the defect
    /// stops being defined.</b> Upstream tests the <em>transmitting</em> call's first character for
    /// an opening bracket and then tests that same call's character at <em>the addressed call's
    /// length, less one</em> for a closing one. Where the two calls are the same length it does what
    /// it plainly meant to; where they are not, it reads a character of one string chosen by the
    /// length of another.
    /// </para>
    /// <para>
    /// In C both are twelve-byte stack buffers, so the read stays inside the buffer but may land
    /// past the string's terminator on bytes that were never written — which has no defined value to
    /// port. So: the index is reproduced as upstream computes it, up to and including the position
    /// of the terminator, and anything past that is treated as not a closing bracket rather than as
    /// whatever happened to be on the stack. Recorded as a divergence in <c>porting-notes.md</c>.
    /// </para>
    /// <para>
    /// It matters less than it looks: a bracketed call cannot be packed at all, because the bracket
    /// is not in the alphabet the hash packs against, so upstream refuses every message this flag
    /// would have been set for. It is ported because it is what decides a bit that goes on the air,
    /// and a received message may carry that bit set however it got there.
    /// </para>
    /// </remarks>
    private static bool LooksBracketed(string callDe, int callToLength)
    {
        if (callDe.Length == 0 || callDe[0] != '<')
        {
            return false;
        }

        var at = callToLength - 1;
        return at >= 0 && at < callDe.Length && callDe[at] == '>';
    }

    private static bool StartsWith(string text, string prefix) =>
        text.AsSpan().StartsWith(prefix.AsSpan(), StringComparison.Ordinal);
}

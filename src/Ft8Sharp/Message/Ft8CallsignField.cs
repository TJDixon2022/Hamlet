using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The 28-bit callsign field of the standard message, and the one spare bit beside it that carries
/// a <c>/R</c> or <c>/P</c> suffix.
/// </summary>
/// <remarks>
/// <para>
/// <b>Ported from <c>ft8/message.c</c> in the pinned clone</b> — functions <c>pack28</c>,
/// <c>unpack28</c>, <c>pack_basecall</c> and <c>parse_cq_modifier</c>. The field is one integer
/// with three sub-ranges laid end to end, and the boundaries between them are two named constants
/// in the pin rather than anything this port chose. Both are asserted against the pin by machine.
/// </para>
/// <para>
/// <b>The seam, and it is the most important thing in this file.</b> The middle sub-range holds a
/// 22-bit hash of a callsign, not a callsign. Resolving one needs a rolling cache of calls heard
/// earlier in the session, and that cache is deliberately not built yet. <b>Where this unpacker
/// meets a value in that range it fails as unresolved.</b> It does not return a placeholder, it
/// does not return the hash dressed as a call, and it does not return a message with a hole in it.
/// That is <c>CLAUDE.md</c> §0.0 / HM-DEC-009 — never present a guess as a decode — applied at the
/// exact place where it would be easiest to fudge. Upstream, in the same position and with no
/// cache attached, writes a literal placeholder into the caller's buffer and reports success; this
/// library reports <see cref="Ft8FieldResult.UnresolvedCallsign"/> instead, and that divergence is
/// recorded in <c>porting-notes.md</c>.
/// </para>
/// <para>
/// <b>The packer side of the same seam.</b> A callsign that is not a standard basecall packs as a
/// hash, which means computing one and storing it. That is the cache, so
/// <see cref="TryPack"/> refuses such a callsign as
/// <see cref="Ft8FieldResult.RequiresHashCache"/> rather than producing a value it could not read
/// back.
/// </para>
/// <para>
/// <b>Not proven correct by round-tripping.</b> That this packs and unpacks to itself over a large
/// corpus shows the two are inverses; it shows nothing about whether the integers agree with the
/// reference implementation's. What corroborates the structure is the machine-checked provenance
/// of the boundaries and widths; what would settle the arithmetic is step 3's bit-identical symbol
/// comparison.
/// </para>
/// </remarks>
public static class Ft8CallsignField
{
    /// <summary>The width of the callsign field in the standard message.</summary>
    public const int Bits = 28;

    /// <summary>One past the largest value the field can hold.</summary>
    public const uint Range = 1u << Bits;

    /// <summary>
    /// The size of the hashed-callsign sub-range: every value a 22-bit hash can take.
    /// </summary>
    /// <remarks>Upstream's <c>MAX22</c>. Asserted against the pin by machine.</remarks>
    public const uint HashRangeSize = 4194304u;

    /// <summary>
    /// Where the hashed-callsign sub-range begins, and therefore how many values the special
    /// tokens reserve below it.
    /// </summary>
    /// <remarks>
    /// Upstream's <c>NTOKENS</c>. Asserted against the pin by machine. Note that the tokens
    /// actually defined use only the bottom of this range — see <see cref="LastDefinedToken"/> —
    /// and the rest of it is unspecified and refused.
    /// </remarks>
    public const uint TokenRangeSize = 2063592u;

    /// <summary>Where the standard-basecall sub-range begins.</summary>
    public const uint BasecallBase = TokenRangeSize + HashRangeSize;

    /// <summary>The code for the <c>DE</c> token.</summary>
    public const uint TokenDe = 0;

    /// <summary>The code for the <c>QRZ</c> token.</summary>
    public const uint TokenQrz = 1;

    /// <summary>The code for the bare <c>CQ</c> token.</summary>
    public const uint TokenCq = 2;

    /// <summary>The first code of the <c>CQ nnn</c> sub-range.</summary>
    public const uint FirstNumericCq = 3;

    /// <summary>The last code of the <c>CQ nnn</c> sub-range — a thousand three-digit values.</summary>
    public const uint LastNumericCq = 1002;

    /// <summary>The first code of the <c>CQ abcd</c> sub-range.</summary>
    public const uint FirstLetteredCq = 1003;

    /// <summary>
    /// The last code of the <c>CQ abcd</c> sub-range — four characters over a 27-symbol alphabet.
    /// </summary>
    public const uint LastLetteredCq = 532443;

    /// <summary>
    /// The last code below <see cref="TokenRangeSize"/> that means anything. Everything between
    /// this and the start of the hash range is unspecified upstream and is refused here.
    /// </summary>
    public const uint LastDefinedToken = LastLetteredCq;

    /// <summary>The number of characters a standard basecall packs into.</summary>
    private const int BasecallLength = 6;

    /// <summary>The alphabet sizes the six basecall positions pack against, in order.</summary>
    /// <remarks>
    /// Not a table lifted from anywhere: these are the lengths of six of upstream's own alphabets,
    /// each of which is asserted against the pin by machine through <see cref="Ft8Text.Length"/>.
    /// </remarks>
    private static readonly Ft8CharTable[] BasecallTables =
    {
        Ft8CharTable.AlphanumericSpace,
        Ft8CharTable.Alphanumeric,
        Ft8CharTable.Numeric,
        Ft8CharTable.LettersSpace,
        Ft8CharTable.LettersSpace,
        Ft8CharTable.LettersSpace,
    };

    /// <summary>
    /// Packs a standard basecall into the integer upstream's <c>pack_basecall</c> produces, or
    /// returns -1 where the callsign is not one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Includes both of the prefix work-arounds upstream carries — the Swaziland and Guinea
    /// rewrites that let a seven-character call fit six positions. Those are on the air and they
    /// are ported as written; a port that dropped them as "special cases" would silently fail
    /// every call from two countries.
    /// </para>
    /// <para>
    /// <paramref name="length"/> is the length of the base call, which is the callsign's length
    /// less two when it carries a <c>/R</c> or <c>/P</c> suffix. Passing the whole length instead
    /// is how a suffixed call quietly fails to pack.
    /// </para>
    /// </remarks>
    public static int PackBasecall(string callsign, int length)
    {
        if (length <= 2)
        {
            return -1;
        }

        // Six spaces, then the callsign written into them wherever its digit says it belongs.
        Span<char> c6 = stackalloc char[BasecallLength];
        c6.Fill(' ');

        if (StartsWith(callsign, "3DA0") && length > 4 && length <= 7)
        {
            // Swaziland: 3DA0XYZ packs as 3D0XYZ.
            Write(c6, 0, "3D0");
            Write(c6, 3, callsign.AsSpan(4, length - 4));
        }
        else if (StartsWith(callsign, "3X") && Ft8Text.IsLetter(At(callsign, 2)) && length <= 7)
        {
            // Guinea: 3XA0XYZ packs as QA0XYZ.
            Write(c6, 0, "Q");
            Write(c6, 1, callsign.AsSpan(2, length - 2));
        }
        else if (Ft8Text.IsDigit(At(callsign, 2)) && length <= 6)
        {
            Write(c6, 0, callsign.AsSpan(0, length));
        }
        else if (Ft8Text.IsDigit(At(callsign, 1)) && length <= 5)
        {
            Write(c6, 1, callsign.AsSpan(0, length));
        }

        // Six positions, six alphabets. Any character outside its own alphabet and this is not a
        // standard callsign — which is a refusal, not an error.
        Span<int> index = stackalloc int[BasecallLength];
        for (var i = 0; i < BasecallLength; i++)
        {
            index[i] = Ft8Text.Index(c6[i], BasecallTables[i]);
            if (index[i] < 0)
            {
                return -1;
            }
        }

        var n = (long)index[0];
        n = (n * 36) + index[1];
        n = (n * 10) + index[2];
        n = (n * 27) + index[3];
        n = (n * 27) + index[4];
        n = (n * 27) + index[5];
        return (int)n;
    }

    /// <summary>
    /// The numeric value of a <c>CQ nnn</c> or <c>CQ abcd</c> modifier, or -1 where the text after
    /// <c>CQ </c> is neither.
    /// </summary>
    /// <remarks>
    /// Upstream's <c>parse_cq_modifier</c>, including its window: it looks at the five characters
    /// after <c>CQ </c> and no further, so a longer modifier is refused by the length test in
    /// <see cref="TryPack"/> rather than here.
    /// </remarks>
    public static int ParseCqModifier(string text)
    {
        var digits = 0;
        var letters = 0;
        var m = 0;

        for (var i = 3; i < 8; i++)
        {
            var c = At(text, i);
            if (c == '\0' || Ft8Text.IsSpace(c))
            {
                break;
            }

            if (Ft8Text.IsDigit(c))
            {
                digits++;
            }
            else if (Ft8Text.IsLetter(c))
            {
                letters++;
                m = (27 * m) + (c - 'A' + 1);
            }
            else
            {
                return -1;
            }
        }

        if (digits == 3 && letters == 0)
        {
            return Ft8Text.DdToInt(text[3..], 3);
        }

        if (digits == 0 && letters <= 4)
        {
            return 1000 + m;
        }

        return -1;
    }

    /// <summary>
    /// Packs a callsign or special token into the 28-bit field, and says whether the suffix bit
    /// beside it should be set.
    /// </summary>
    /// <param name="callsign">The token or callsign, already trimmed and upper-cased.</param>
    /// <param name="value">The field value, written only on <see cref="Ft8FieldResult.Ok"/>.</param>
    /// <param name="suffix">
    /// The one-bit flag that says the call carried a <c>/R</c> or <c>/P</c>; written only on
    /// <see cref="Ft8FieldResult.Ok"/>. Which of the two it was is carried by the message's own
    /// type code, not by this bit.
    /// </param>
    /// <remarks>
    /// <b>Never throws.</b> Every string, of every length, including the empty one, gets one of the
    /// three answers.
    /// </remarks>
    public static Ft8FieldResult TryPack(string callsign, out uint value, out bool suffix) =>
        TryPack(callsign, null, out value, out suffix);

    /// <summary>
    /// Packs a callsign or special token into the 28-bit field, hashing a non-standard call into the
    /// cache rather than refusing it.
    /// </summary>
    /// <param name="callsign">The token or callsign, already trimmed and upper-cased.</param>
    /// <param name="cache">
    /// The rolling cache. Where it is <see langword="null"/> this behaves exactly as the overload
    /// without one: a non-standard callsign is refused as
    /// <see cref="Ft8FieldResult.RequiresHashCache"/> rather than written as a value nothing could
    /// read back.
    /// </param>
    /// <param name="value">The field value, written only on <see cref="Ft8FieldResult.Ok"/>.</param>
    /// <param name="suffix">The suffix flag, written only on <see cref="Ft8FieldResult.Ok"/>.</param>
    /// <remarks>
    /// <b>A standard basecall is stored in the cache too.</b> Upstream hashes and stores every call
    /// it packs, standard or not, which is what lets a later message refer to a station by the hash
    /// of a call this one spelled out in full. That is the whole mechanism, and leaving it out would
    /// give a cache that only ever remembered the calls it could not read.
    /// </remarks>
    public static Ft8FieldResult TryPack(
        string callsign,
        Ft8CallsignCache? cache,
        out uint value,
        out bool suffix)
    {
        value = 0;
        suffix = false;

        if (callsign is null)
        {
            return Ft8FieldResult.Malformed;
        }

        if (callsign == "DE")
        {
            value = TokenDe;
            return Ft8FieldResult.Ok;
        }

        if (callsign == "QRZ")
        {
            value = TokenQrz;
            return Ft8FieldResult.Ok;
        }

        if (callsign == "CQ")
        {
            value = TokenCq;
            return Ft8FieldResult.Ok;
        }

        var length = callsign.Length;

        if (StartsWith(callsign, "CQ ") && length < 8)
        {
            var modifier = ParseCqModifier(callsign);
            if (modifier < 0)
            {
                return Ft8FieldResult.Malformed;
            }

            value = FirstNumericCq + (uint)modifier;
            return Ft8FieldResult.Ok;
        }

        var baseLength = length;
        if (EndsWith(callsign, "/P") || EndsWith(callsign, "/R"))
        {
            suffix = true;
            baseLength = length - 2;
        }

        var basecall = PackBasecall(callsign, baseLength);
        if (basecall >= 0)
        {
            // Upstream hashes and stores the call at this point, which cannot fail for characters
            // that already packed as a basecall. Without a cache the character check is kept as the
            // part of that step which is not the cache; with one, the call is really stored, which
            // is how a later message gets to name this station by its hash alone.
            if (cache is null)
            {
                if (!IsHashable(callsign))
                {
                    return Ft8FieldResult.Malformed;
                }
            }
            else if (cache.Save(callsign) == Ft8CacheStore.NotHashable)
            {
                return Ft8FieldResult.Malformed;
            }

            value = BasecallBase + (uint)basecall;
            return Ft8FieldResult.Ok;
        }

        if (length is >= 3 and <= 11)
        {
            // A non-standard callsign packs as its 22-bit hash.
            suffix = false;

            if (cache is null)
            {
                // No cache, so nothing could read this value back. Refused rather than written.
                return Ft8FieldResult.RequiresHashCache;
            }

            var stored = cache.Save(callsign, out var hash22, out _, out _);
            if (stored == Ft8CacheStore.NotHashable)
            {
                return Ft8FieldResult.Malformed;
            }

            value = TokenRangeSize + hash22;
            return Ft8FieldResult.Ok;
        }

        return Ft8FieldResult.Malformed;
    }

    /// <summary>
    /// Unpacks the 28-bit field back to a token or a callsign, or says why it cannot.
    /// </summary>
    /// <param name="value">The field value. Anything at or above <see cref="Range"/> is refused.</param>
    /// <param name="suffix">The suffix bit that sat beside the field.</param>
    /// <param name="messageType">
    /// The message's own type code, which is what decides whether the suffix bit means <c>/R</c> or
    /// <c>/P</c>. A suffix bit under any other type code is refused rather than guessed at.
    /// </param>
    /// <param name="text">The token or callsign, written only on <see cref="Ft8FieldResult.Ok"/>.</param>
    /// <param name="fieldType">What kind of thing was decoded, written only on success.</param>
    /// <remarks>
    /// <b>Never throws, for any 32-bit input.</b> That property is what the dispatcher's promise
    /// rests on, and it is asserted directly over a large random corpus rather than argued for.
    /// </remarks>
    public static Ft8FieldResult TryUnpack(
        uint value,
        bool suffix,
        int messageType,
        out string text,
        out Ft8FieldType fieldType) =>
        TryUnpack(value, suffix, messageType, null, out text, out fieldType);

    /// <summary>
    /// Unpacks the 28-bit field back to a token or a callsign, resolving a hashed one through the
    /// cache where the cache has heard it.
    /// </summary>
    /// <param name="value">The field value. Anything at or above <see cref="Range"/> is refused.</param>
    /// <param name="suffix">The suffix bit that sat beside the field.</param>
    /// <param name="messageType">The message's own type code, which says what the suffix bit means.</param>
    /// <param name="cache">
    /// The rolling cache, or <see langword="null"/> for none. A <see langword="null"/> cache behaves
    /// exactly as a cold one: every hashed value is refused as
    /// <see cref="Ft8FieldResult.UnresolvedCallsign"/>.
    /// </param>
    /// <param name="text">The token or callsign, written only on <see cref="Ft8FieldResult.Ok"/>.</param>
    /// <param name="fieldType">What kind of thing was decoded, written only on success.</param>
    /// <remarks>
    /// <para>
    /// <b>A resolved call comes back inside angle brackets, and that is not decoration.</b> Upstream
    /// writes <c>&lt;CALL&gt;</c> for a call it recovered from a hash, and this does the same. The
    /// brackets say the call was not in these bits — it was remembered from an earlier transmission
    /// and matched by a hash. An operator reading a log needs to be able to tell those two apart,
    /// and this is upstream's own way of telling them.
    /// </para>
    /// <para>
    /// <b>A miss and a collision are both refusals and neither writes a character.</b> Upstream
    /// writes a literal <c>&lt;...&gt;</c> on a miss and its first probe-chain match on a collision;
    /// this refuses both. HM-DEC-009.
    /// </para>
    /// </remarks>
    public static Ft8FieldResult TryUnpack(
        uint value,
        bool suffix,
        int messageType,
        Ft8CallsignCache? cache,
        out string text,
        out Ft8FieldType fieldType)
    {
        text = string.Empty;
        fieldType = Ft8FieldType.Unknown;

        if (value >= Range)
        {
            return Ft8FieldResult.Malformed;
        }

        if (value < TokenRangeSize)
        {
            return TryUnpackToken(value, out text, out fieldType);
        }

        var n = value - TokenRangeSize;
        if (n < HashRangeSize)
        {
            // The seam. These bits are a 22-bit hash, and the only thing that can turn one back into
            // a callsign is having heard that callsign already.
            if (cache is null || cache.TryLookup(Ft8CallsignHashWidth.Bits22, n, out var resolved)
                != Ft8CacheLookup.Found)
            {
                return Ft8FieldResult.UnresolvedCallsign;
            }

            text = Bracket(resolved);
            fieldType = Ft8FieldType.Callsign;
            return Ft8FieldResult.Ok;
        }

        n -= HashRangeSize;

        Span<char> callsign = stackalloc char[BasecallLength];
        callsign[5] = Ft8Text.Character((int)(n % 27), Ft8CharTable.LettersSpace);
        n /= 27;
        callsign[4] = Ft8Text.Character((int)(n % 27), Ft8CharTable.LettersSpace);
        n /= 27;
        callsign[3] = Ft8Text.Character((int)(n % 27), Ft8CharTable.LettersSpace);
        n /= 27;
        callsign[2] = Ft8Text.Character((int)(n % 10), Ft8CharTable.Numeric);
        n /= 10;
        callsign[1] = Ft8Text.Character((int)(n % 36), Ft8CharTable.Alphanumeric);
        n /= 36;
        callsign[0] = Ft8Text.Character((int)(n % 37), Ft8CharTable.AlphanumericSpace);

        string result;
        if (callsign.StartsWith("3D0") && !Ft8Text.IsSpace(callsign[3]))
        {
            result = "3DA0" + TrimCopy(callsign[3..]);
        }
        else if (callsign[0] == 'Q' && Ft8Text.IsLetter(callsign[1]))
        {
            result = "3X" + TrimCopy(callsign[1..]);
        }
        else
        {
            result = TrimCopy(callsign);
        }

        if (result.Length < 3)
        {
            return Ft8FieldResult.Malformed;
        }

        if (suffix)
        {
            if (messageType == 1)
            {
                result += "/R";
            }
            else if (messageType == 2)
            {
                result += "/P";
            }
            else
            {
                return Ft8FieldResult.Malformed;
            }
        }

        // Upstream remembers every standard call it reads, and this is the other half of the
        // mechanism: a station that spells its call out once can be named by its hash afterwards,
        // and it is a message like this one that teaches the cache the call.
        cache?.Save(result);

        text = result;
        fieldType = Ft8FieldType.Callsign;
        return Ft8FieldResult.Ok;
    }

    /// <summary>
    /// A callsign wrapped in the angle brackets that mark it as recovered from a hash rather than
    /// read out of the bits.
    /// </summary>
    /// <remarks>Upstream's own <c>add_brackets</c>, and its own convention.</remarks>
    internal static string Bracket(string callsign) => "<" + callsign + ">";

    /// <summary>The token sub-range: the three bare tokens and the two families of CQ modifier.</summary>
    private static Ft8FieldResult TryUnpackToken(uint value, out string text, out Ft8FieldType fieldType)
    {
        text = string.Empty;
        fieldType = Ft8FieldType.Unknown;

        if (value <= TokenCq)
        {
            text = value switch
            {
                TokenDe => "DE",
                TokenQrz => "QRZ",
                _ => "CQ",
            };
            fieldType = Ft8FieldType.Token;
            return Ft8FieldResult.Ok;
        }

        if (value <= LastNumericCq)
        {
            text = "CQ " + Ft8Text.IntToDd((int)(value - FirstNumericCq), 3, false);
            fieldType = Ft8FieldType.TokenWithArgument;
            return Ft8FieldResult.Ok;
        }

        if (value <= LastLetteredCq)
        {
            var n = value - FirstLetteredCq;
            Span<char> letters = stackalloc char[4];
            for (var i = 3; ; i--)
            {
                letters[i] = Ft8Text.Character((int)(n % 27), Ft8CharTable.LettersSpace);
                if (i == 0)
                {
                    break;
                }

                n /= 27;
            }

            // Right-aligned in four positions, so the leading spaces come off. The trailing ones
            // do not: upstream trims only the front here, and "CQ " with nothing after it is a
            // value the field really can hold.
            var trimmed = letters.ToString().TrimStart(' ');
            text = "CQ " + trimmed;
            fieldType = Ft8FieldType.TokenWithArgument;
            return Ft8FieldResult.Ok;
        }

        // Everything from here to the start of the hash range is unspecified upstream, which
        // returns -1 for it. Refused as malformed rather than decoded to anything.
        return Ft8FieldResult.Malformed;
    }

    /// <summary>
    /// Whether every character of a callsign is in the alphabet upstream's hash step requires.
    /// </summary>
    /// <remarks>
    /// This is the only part of upstream's <c>save_callsign</c> that is ported: the check that
    /// makes it return false. Nothing is hashed, nothing is stored, and no cache exists to store
    /// it in.
    /// </remarks>
    private static bool IsHashable(string callsign)
    {
        var limit = Math.Min(callsign.Length, 11);
        for (var i = 0; i < limit; i++)
        {
            if (Ft8Text.Index(callsign[i], Ft8CharTable.AlphanumericSpaceSlash) < 0)
            {
                return false;
            }
        }

        return true;
    }

    private static string TrimCopy(ReadOnlySpan<char> text) => text.Trim(' ').ToString();

    /// <summary>The character at <paramref name="index"/>, or <c>\0</c> past the end, as C reads it.</summary>
    private static char At(string text, int index) => index < text.Length ? text[index] : '\0';

    private static bool StartsWith(string text, string prefix) =>
        text.AsSpan().StartsWith(prefix.AsSpan(), StringComparison.Ordinal);

    private static bool EndsWith(string text, string suffix) =>
        text.AsSpan().EndsWith(suffix.AsSpan(), StringComparison.Ordinal);

    private static void Write(Span<char> destination, int at, ReadOnlySpan<char> source) =>
        source.CopyTo(destination[at..]);
}

/// <summary>What happened when a field was packed or unpacked.</summary>
/// <remarks>
/// <b>Every one of these is an answer rather than an exception.</b> A decoder handed noise must be
/// able to say what it could not read without unwinding a stack, and a caller must be able to tell
/// "these bits are a callsign I have not heard yet" from "these bits are not a message".
/// </remarks>
public enum Ft8FieldResult
{
    /// <summary>The field was read or written.</summary>
    Ok,

    /// <summary>
    /// The field holds a hashed callsign, and resolving it needs the rolling hash cache this
    /// library does not have yet.
    /// </summary>
    UnresolvedCallsign,

    /// <summary>
    /// Packing this would need the rolling hash cache: the callsign is not a standard basecall.
    /// </summary>
    RequiresHashCache,

    /// <summary>The field holds a value the protocol does not define, or text it cannot carry.</summary>
    Malformed,
}

/// <summary>What kind of thing a decoded field turned out to be.</summary>
/// <remarks>Upstream's <c>ftx_field_t</c>, less the members this library has no use for yet.</remarks>
public enum Ft8FieldType
{
    /// <summary>Not decoded.</summary>
    Unknown,

    /// <summary>The field was empty — the message carries no third field at all.</summary>
    None,

    /// <summary>A fixed token: <c>DE</c>, <c>QRZ</c>, <c>CQ</c>, <c>RRR</c>, <c>RR73</c>, <c>73</c>.</summary>
    Token,

    /// <summary>A token carrying an argument: <c>CQ nnn</c> or <c>CQ abcd</c>.</summary>
    TokenWithArgument,

    /// <summary>A callsign.</summary>
    Callsign,

    /// <summary>A four-character Maidenhead grid square.</summary>
    Grid,

    /// <summary>A signal report.</summary>
    Report,
}

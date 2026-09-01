using System;

namespace Ft8Sharp.Message;

/// <summary>
/// The six character alphabets the FT8 message layer packs against, and the mapping between an
/// index in one of them and an ASCII character.
/// </summary>
/// <remarks>
/// <para>
/// <b>These are not tables and this is not a lookup.</b> Upstream holds the alphabets as
/// enumeration members with the mapping computed by branching arithmetic, so there is no literal
/// anywhere for the checked-in table converter to lift, and nothing here goes near
/// <c>Tables/Ft8Tables.g.cs</c>. Unit 206 measured that: 15 sources scanned in the pin,
/// 0 alphabets as string literals and 0 as char arrays. What follows is a port of two small
/// functions, and the arithmetic is upstream's arithmetic rather than a rewrite that happens to
/// agree — a table walked in a different order round-trips perfectly and is wholly wrong on the
/// air.
/// </para>
/// <para>
/// <b>Measured rather than assumed.</b> The number of alphabets, their names and the length of
/// each are read out of the pin at run time by <c>UpstreamMessageProvenanceTests</c> and asserted
/// against <see cref="Ft8CharTable"/> and <see cref="Length"/>. That corroborates the shape of
/// this file by machine; it cannot corroborate the branching, which is what step 3's
/// bit-identical symbol comparison against the reference implementation is for.
/// </para>
/// <para>
/// <b>Deliberately left behind.</b> Upstream's string helpers — trimming, token copying, the
/// message formatter — are C string plumbing that a .NET caller has no use for, so only the
/// pieces the packer needs are here: the two alphabet conversions, the character predicates the
/// callsign field tests against, and the fixed-width integer formatting the report field needs.
/// The callsign hash functions in the same upstream file are not ported: they belong to the
/// rolling hash cache, which is deliberately not built yet.
/// </para>
/// </remarks>
public static class Ft8Text
{
    /// <summary>The number of characters each alphabet admits.</summary>
    /// <remarks>
    /// Indexed by <see cref="Ft8CharTable"/>. Every one of these is asserted against the pin by
    /// machine rather than trusted.
    /// </remarks>
    public static int Length(Ft8CharTable table) => table switch
    {
        Ft8CharTable.Full => 42,
        Ft8CharTable.AlphanumericSpaceSlash => 38,
        Ft8CharTable.AlphanumericSpace => 37,
        Ft8CharTable.LettersSpace => 27,
        Ft8CharTable.Alphanumeric => 36,
        Ft8CharTable.Numeric => 10,
        _ => 0,
    };

    /// <summary>Whether <paramref name="table"/> is one of the six the protocol declares.</summary>
    public static bool IsDefined(Ft8CharTable table) => Length(table) > 0;

    /// <summary>
    /// The character at index <paramref name="index"/> of <paramref name="table"/>, or
    /// <see cref="Unknown"/> when the index is outside the alphabet.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The structure is upstream's: each alphabet is a prefix selection over the same four
    /// segments — space, digits, letters, punctuation — and the branches subtract the segments
    /// this alphabet includes as they are passed over. Ported as written, including the fallthrough
    /// that returns <see cref="Unknown"/> rather than throwing.
    /// </para>
    /// <para>
    /// <b>The range check at the top is where this diverges, and only for a negative index.</b>
    /// An index at or past the alphabet's length falls through upstream's branches to the same
    /// <see cref="Unknown"/> this returns — checked for all six alphabets, at the length and one
    /// past it — so the guard is a shortcut there and not a behaviour change. A <em>negative</em>
    /// index is different: C reaches the digit branch with it and indexes off the front of a
    /// range, where this returns <see cref="Unknown"/>. That divergence is recorded in
    /// <c>porting-notes.md</c>. No caller in this library can produce a negative index, so it
    /// changes no packing; what it buys is a total function, which is what lets the dispatcher
    /// promise never to throw.
    /// </para>
    /// </remarks>
    public static char Character(int index, Ft8CharTable table)
    {
        if (!IsDefined(table) || index < 0 || index >= Length(table))
        {
            return Unknown;
        }

        var c = index;

        if (table != Ft8CharTable.Alphanumeric && table != Ft8CharTable.Numeric)
        {
            if (c == 0)
            {
                return ' ';
            }

            c -= 1;
        }

        if (table != Ft8CharTable.LettersSpace)
        {
            if (c < 10)
            {
                return (char)('0' + c);
            }

            c -= 10;
        }

        if (table != Ft8CharTable.Numeric)
        {
            if (c < 26)
            {
                return (char)('A' + c);
            }

            c -= 26;
        }

        if (table == Ft8CharTable.Full)
        {
            if (c < Punctuation.Length)
            {
                return Punctuation[c];
            }
        }
        else if (table == Ft8CharTable.AlphanumericSpaceSlash)
        {
            if (c == 0)
            {
                return '/';
            }
        }

        return Unknown;
    }

    /// <summary>
    /// The index of <paramref name="c"/> in <paramref name="table"/>, or <see cref="NotFound"/>
    /// when the character is not in that alphabet.
    /// </summary>
    /// <remarks>
    /// Upstream's own inverse, running the same four segments forward and accumulating the offset
    /// of each segment it passes over. It is not derived from <see cref="Character"/> — deriving
    /// one from the other would make the round-trip test in this project tautological, and the
    /// round-trip is one of the few things that catches an ordinary porting slip.
    /// </remarks>
    public static int Index(char c, Ft8CharTable table)
    {
        if (!IsDefined(table))
        {
            return NotFound;
        }

        var n = 0;

        if (table != Ft8CharTable.Alphanumeric && table != Ft8CharTable.Numeric)
        {
            if (c == ' ')
            {
                return n + 0;
            }

            n += 1;
        }

        if (table != Ft8CharTable.LettersSpace)
        {
            if (c is >= '0' and <= '9')
            {
                return n + (c - '0');
            }

            n += 10;
        }

        if (table != Ft8CharTable.Numeric)
        {
            if (c is >= 'A' and <= 'Z')
            {
                return n + (c - 'A');
            }

            n += 26;
        }

        if (table == Ft8CharTable.Full)
        {
            var at = Punctuation.IndexOf(c);
            if (at >= 0)
            {
                return n + at;
            }
        }
        else if (table == Ft8CharTable.AlphanumericSpaceSlash)
        {
            if (c == '/')
            {
                return n + 0;
            }
        }

        return NotFound;
    }

    /// <summary>What <see cref="Character"/> answers for an index no alphabet defines.</summary>
    /// <remarks>
    /// Upstream's own sentinel, kept rather than replaced by an exception. Nothing in this library
    /// puts it into a decoded message: a field that produces it is a field that failed, and the
    /// unpacker refuses the message rather than showing the operator a character that was never
    /// on the air.
    /// </remarks>
    public const char Unknown = '_';

    /// <summary>What <see cref="Index"/> answers for a character the alphabet does not admit.</summary>
    public const int NotFound = -1;

    /// <summary>
    /// The punctuation segment of the full alphabet, in the order the protocol packs it.
    /// </summary>
    /// <remarks>
    /// The one segment that is a literal rather than a range, upstream included. It is five
    /// characters and it is part of the wire format, not a table in the sense the licensing ruling
    /// governs — a table there means the LDPC and Costas data in <c>constants.c</c>, which has its
    /// own machine-converted route and is untouched by this file.
    /// </remarks>
    private const string Punctuation = "+-./?";

    /// <summary>Upstream's <c>to_upper</c>: ASCII only, and everything else passes through.</summary>
    /// <remarks>
    /// Not <see cref="char.ToUpperInvariant(char)"/>. The protocol's alphabets are ASCII, and a
    /// culture-aware or Unicode-aware upcasing would map characters upstream leaves alone, which
    /// is exactly the class of "reads better, behaves differently" change the port is meant to
    /// avoid.
    /// </remarks>
    public static char ToUpper(char c) => c is >= 'a' and <= 'z' ? (char)(c - 'a' + 'A') : c;

    /// <summary>Upstream's <c>is_digit</c>.</summary>
    public static bool IsDigit(char c) => c is >= '0' and <= '9';

    /// <summary>Upstream's <c>is_letter</c>, which admits both cases.</summary>
    public static bool IsLetter(char c) => c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');

    /// <summary>Upstream's <c>is_space</c>, which is a space and nothing else — not a tab, not a newline.</summary>
    public static bool IsSpace(char c) => c == ' ';

    /// <summary>Upstream's <c>in_range</c>, inclusive at both ends.</summary>
    public static bool InRange(char c, char min, char max) => c >= min && c <= max;

    /// <summary>
    /// Upstream's <c>int_to_dd</c>: a signed integer as a fixed-width decimal string, with the
    /// leading <c>+</c> written only when asked for.
    /// </summary>
    /// <remarks>
    /// The report field needs this and its exact shape is on the air, so it is ported rather than
    /// expressed as a format string — <c>value.ToString("+00;-00")</c> is the same answer for the
    /// values this library produces and a different one at the edges, and the edges are where a
    /// port goes wrong.
    /// </remarks>
    public static string IntToDd(int value, int width, bool fullSign)
    {
        var text = new System.Text.StringBuilder(width + 1);

        if (value < 0)
        {
            text.Append('-');
            value = -value;
        }
        else if (fullSign)
        {
            text.Append('+');
        }

        var divisor = 1;
        for (var i = 0; i < width - 1; i++)
        {
            divisor *= 10;
        }

        while (divisor >= 1)
        {
            var digit = value / divisor;
            text.Append((char)('0' + digit));
            value -= digit * divisor;
            divisor /= 10;
        }

        return text.ToString();
    }

    /// <summary>
    /// Upstream's <c>dd_to_int</c>: a leading sign, then digits, stopping at the first character
    /// that is not one.
    /// </summary>
    /// <remarks>
    /// Deliberately not <see cref="int.TryParse(string, out int)"/>. Upstream stops at the first
    /// non-digit and returns what it has, where <c>TryParse</c> refuses the whole string, and the
    /// difference decides what a malformed report field decodes to.
    /// </remarks>
    public static int DdToInt(string text, int length)
    {
        var negative = false;
        var i = 0;

        if (text.Length > 0 && text[0] == '-')
        {
            negative = true;
            i = 1;
        }
        else if (text.Length > 0 && text[0] == '+')
        {
            i = 1;
        }

        var result = 0;
        while (i < length && i < text.Length)
        {
            if (!IsDigit(text[i]))
            {
                break;
            }

            result = (result * 10) + (text[i] - '0');
            i++;
        }

        return negative ? -result : result;
    }
}

/// <summary>The six character alphabets the FT8 message layer packs against.</summary>
/// <remarks>
/// The names are this library's; the order is upstream's, because the enumeration's own ordinal
/// values are not what selects an alphabet — the branching in <see cref="Ft8Text.Character"/>
/// tests identity, not order. The count and each alphabet's length are asserted against the pin
/// by machine.
/// </remarks>
public enum Ft8CharTable
{
    /// <summary>Space, digits, letters, and the five punctuation characters.</summary>
    Full,

    /// <summary>Space, digits, letters and the slash.</summary>
    AlphanumericSpaceSlash,

    /// <summary>Space, digits and letters.</summary>
    AlphanumericSpace,

    /// <summary>Space and letters, with no digits at all.</summary>
    LettersSpace,

    /// <summary>Digits and letters, with no leading space.</summary>
    Alphanumeric,

    /// <summary>Digits only.</summary>
    Numeric,
}

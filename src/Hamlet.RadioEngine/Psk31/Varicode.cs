using System.Globalization;
using System.Reflection;
using System.Text;

namespace Hamlet.RadioEngine.Psk31;

/// <summary>
/// **The PSK31 varicode: text to bits and back, with `00` between characters.**
/// </summary>
/// <remarks>
/// <para>**PETER MARTINEZ G3PLX'S CODE, PUBLISHED IN 1998 WITH THE MODE ITSELF.** A
/// self-delimiting variable-length code: common letters are short - space is `1`, `e` is
/// `11`, `t` is `101` - **every code starts and ends with `1`, and no code contains
/// `00`**. So `00` is the character separator and nothing else in a transmission can
/// look like one. A decoder needs no framing, no sync word and no length field: it
/// splits on `00` and looks the pieces up.</para>
/// <para>**THE TABLE IS DATA WITH A CITATION AND IT IS NOT TYPED HERE** (§0, §R5, and
/// unit 252's pattern for the DXCC list). `data/psk31/varicode.csv` carries 256 rows and
/// `data/psk31/varicode-SOURCE.md` says where each bit string came from: extracted
/// mechanically from `src/psk/pskvaricode.cxx` at the pinned `fldigi` commit
/// `61b97f41`, which is GPL-3 as Hamlet is. **The code is the published standard;
/// fldigi is the citation for the exact bits.** Transcribing a published table with its
/// citation is data, not a port - and a table written from memory is the pattern this
/// project has spent a month learning not to trust.</para>
/// <para>**IT KNOWS NOTHING ABOUT AUDIO, TABS OR RADIOS** (§0.1). Characters in, bits
/// out, and the reverse. What samples those bits came from is the demodulator's
/// business.</para>
/// <para>**BYTES, NOT CHARACTERS.** The code maps the 256 byte values, and the
/// modulator that made this project's fixtures encodes latin-1 bytes. A character
/// outside that range has no varicode and is not invented one.</para>
/// </remarks>
public static class Varicode
{
    /// <summary>Where the table is read from.</summary>
    public const string Resource = "Hamlet.RadioEngine.Data.Psk31.varicode.csv";

    /// <summary>What the table is cited to.</summary>
    /// <remarks>
    /// **THE FILE BESIDE IT SAYS THE REST**, and this constant exists so a test can
    /// assert the citation travels with the data rather than living only in a comment.
    /// </remarks>
    public const string Cite =
        "PSK31 varicode, Peter Martinez G3PLX 1998; bit strings extracted from "
        + "w1hkj/fldigi src/psk/pskvaricode.cxx at commit "
        + "61b97f4133c488063f3de1795c894d22d5032e8a, GPL-3. See "
        + "data/psk31/varicode-SOURCE.md";

    /// <summary>The character separator, and the one thing no code contains.</summary>
    public const string Separator = "00";

    private static readonly string[] ByCode = Load();

    private static readonly Dictionary<string, byte> ByBits = Index();

    /// <summary>Every code in the table, in byte order.</summary>
    public static IReadOnlyList<string> Codes => ByCode;

    /// <summary>The bits for one byte, or null where the table has none.</summary>
    /// <param name="value">The byte.</param>
    /// <returns>Its varicode, as a string of `0` and `1`.</returns>
    public static string? BitsFor(byte value) => ByCode[value];

    /// <summary>The byte one code stands for, or null where it is not a code.</summary>
    /// <param name="bits">A run of `0` and `1` with no `00` in it.</param>
    /// <returns>The byte, or null.</returns>
    /// <remarks>
    /// **NULL IS A REAL ANSWER AND THE COMMON ONE ON THE AIR** (§0.0). A run of bits
    /// that is not in the table is noise that happened to survive the squelch, and a
    /// decoder that guessed the nearest code would be presenting a guess as a decode.
    /// **Nothing here guesses, and nothing substitutes a `?`** - a character Hamlet is
    /// not sure of is not shown at all.
    /// </remarks>
    public static byte? ByteFor(string bits)
        => !string.IsNullOrEmpty(bits) && ByBits.TryGetValue(bits, out var value)
            ? value
            : null;

    /// <summary>Text to a bit stream, each character followed by `00`.</summary>
    /// <param name="text">What to send.</param>
    /// <returns>The bits, as a string of `0` and `1`.</returns>
    /// <remarks>
    /// **EACH CHARACTER IS ITS CODE THEN THE SEPARATOR**, which is what the fixtures
    /// were made with and what every PSK31 station sends. A character the table cannot
    /// carry is skipped rather than replaced.
    /// </remarks>
    public static string Encode(string? text)
    {
        var bits = new StringBuilder();

        foreach (var value in Encoding.Latin1.GetBytes(text ?? ""))
        {
            if (ByCode[value] is { Length: > 0 } code)
            {
                bits.Append(code).Append(Separator);
            }
        }

        return bits.ToString();
    }

    /// <summary>A bit stream to text, splitting on `00`.</summary>
    /// <param name="bits">A run of `0` and `1`.</param>
    /// <returns>What it spells.</returns>
    /// <remarks>
    /// <para>**IDLE IS A RUN OF `0`s AND CARRIES NOTHING.** A station sends continuous
    /// reversals before it starts typing and between characters, so a stream opens and
    /// closes with zeros and the pieces between separators are sometimes empty. An empty
    /// piece is idle, not a character.</para>
    /// <para>**AND A PIECE THAT IS NOT IN THE TABLE PRODUCES NOTHING** (§0.0). Not a
    /// question mark, not the nearest code: silence. On a real signal that is a burst of
    /// noise the squelch let through, and inventing a letter for it is the fault this
    /// whole project is written against.</para>
    /// </remarks>
    public static string Decode(string? bits)
    {
        var stream = bits ?? "";
        var text = new StringBuilder();
        var piece = new StringBuilder();

        for (var at = 0; at < stream.Length; at++)
        {
            if (stream[at] == '0' && at + 1 < stream.Length && stream[at + 1] == '0')
            {
                Take(text, piece);

                // **RUN PAST THE WHOLE RUN OF ZEROS.** Forty idle bits is twenty
                // separators to a loop that only steps over two, and each one after the
                // first would take an empty piece - harmless, and this says the
                // intention rather than relying on that.
                while (at + 1 < stream.Length && stream[at + 1] == '0')
                {
                    at++;
                }

                continue;
            }

            piece.Append(stream[at]);
        }

        Take(text, piece);

        return text.ToString();
    }

    /// <summary>Turn one gathered piece into a character, or drop it.</summary>
    private static void Take(StringBuilder text, StringBuilder piece)
    {
        if (piece.Length == 0)
        {
            return;
        }

        if (ByteFor(piece.ToString()) is { } value)
        {
            text.Append((char)value);
        }

        piece.Clear();
    }

    /// <summary>Read the table off the embedded file.</summary>
    /// <remarks>
    /// **THE NAME COLUMN IS NOT READ.** It is there for a person reading the file and
    /// carries a quoted comma for the double-quote row; the two columns this needs are
    /// the code and the bits, and taking them by position avoids a CSV quoting rule
    /// nothing here depends on.
    /// </remarks>
    private static string[] Load()
    {
        var table = new string[256];

        using var stream = typeof(Varicode).Assembly
            .GetManifestResourceStream(Resource)
            ?? throw new InvalidOperationException(
                "the varicode table is missing from this build: " + Resource);

        using var reader = new StreamReader(stream, Encoding.UTF8);

        var line = reader.ReadLine();

        while ((line = reader.ReadLine()) is not null)
        {
            if (line.Length == 0)
            {
                continue;
            }

            var firstComma = line.IndexOf(',', StringComparison.Ordinal);

            if (firstComma <= 0)
            {
                continue;
            }

            var secondComma = line.IndexOf(',', firstComma + 1);

            if (secondComma <= firstComma)
            {
                continue;
            }

            if (!int.TryParse(
                    line[..firstComma], NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var code)
                || code is < 0 or > 255)
            {
                continue;
            }

            table[code] = line[(firstComma + 1)..secondComma];
        }

        for (var code = 0; code < table.Length; code++)
        {
            if (string.IsNullOrEmpty(table[code]))
            {
                throw new InvalidOperationException(
                    "the varicode table has no entry for byte " + code);
            }
        }

        return table;
    }

    /// <summary>The same table, the other way round.</summary>
    private static Dictionary<string, byte> Index()
    {
        var index = new Dictionary<string, byte>(StringComparer.Ordinal);

        for (var code = 0; code < ByCode.Length; code++)
        {
            // **A DUPLICATE WOULD MAKE ONE BYTE UNREACHABLE AND IS REFUSED HERE**
            // rather than quietly taking the last one. The table's own source note
            // records that all 256 are unique; this is what would catch it if a later
            // edit broke that.
            if (!index.TryAdd(ByCode[code], (byte)code))
            {
                throw new InvalidOperationException(
                    "two bytes share the varicode " + ByCode[code]);
            }
        }

        return index;
    }
}

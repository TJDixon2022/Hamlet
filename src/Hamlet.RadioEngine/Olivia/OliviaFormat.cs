using System.Globalization;
using System.Text.Json;

namespace Hamlet.RadioEngine.Olivia;

/// <summary>One Olivia variant's parameters, as <c>data/olivia/format.json</c> states them.</summary>
/// <param name="Name">The variant as the air names it, e.g. "16/500".</param>
/// <param name="Tones">How many tones.</param>
/// <param name="BandwidthHz">The bandwidth the tones span.</param>
/// <param name="BitsPerSymbol">Bits one tone carries, which is also the characters in one block.</param>
/// <param name="ToneSpacingHz">How far apart the tones are, which is also the symbol rate.</param>
/// <param name="SymbolSeconds">How long one symbol lasts.</param>
/// <param name="FirstToneOffsetHz">Where the lowest tone sits against the center; negative is below it.</param>
public sealed record OliviaVariant(
    string Name,
    int Tones,
    int BandwidthHz,
    int BitsPerSymbol,
    double ToneSpacingHz,
    double SymbolSeconds,
    double FirstToneOffsetHz);

/// <summary>
/// **The facts of the Olivia format a receiver must match bit for bit, read from
/// <c>data/olivia/format.json</c>.**
/// </summary>
/// <remarks>
/// <para>**CITED DATA, NOT A PORT** (work instruction 361 decision H). The scrambling code, the
/// shift per character, the character-to-Walsh mapping, the Gray code and each variant's
/// parameters are transcribed from Pawel Jalocha's `pj_mfsk.h`, `pj_fht.h` and `pj_gray.h` with
/// the line each stands on, the way `rsid-codes.json` carries fldigi's RSID tables. **There is
/// no format literal in this file**: every value is the file's.</para>
/// <para>**STRICT, FOR `RsidCodes`'s REASON.** A value missing or of the wrong kind, or a variant
/// row whose spacing, symbol length and tone count disagree with one another, fails the whole
/// file rather than being skipped or repaired, because a half-read format decodes nonsense that
/// looks like text.</para>
/// </remarks>
public sealed class OliviaFormat
{
    /// <summary>The file's name, as a sentence about it names it.</summary>
    public const string FilePath = "data/olivia/format.json";

    private readonly Dictionary<string, OliviaVariant> _variants;

    private OliviaFormat(
        string source,
        int bitsPerCharacter,
        int symbolsPerBlock,
        int characterMask,
        bool upperHalfNegated,
        int[,] walshInverseKernel,
        ulong scramblingCode,
        int scramblingShiftPerCharacter,
        int toneBitRotationPerSymbol,
        bool negativeSetsTheBit,
        IReadOnlyList<int> symbolToTone,
        int analysisWindowSymbols,
        int nullCharacter,
        IReadOnlyList<OliviaVariant> variants)
    {
        Source = source;
        BitsPerCharacter = bitsPerCharacter;
        SymbolsPerBlock = symbolsPerBlock;
        CharacterMask = characterMask;
        UpperHalfNegated = upperHalfNegated;
        WalshInverseKernel = walshInverseKernel;
        ScramblingCode = scramblingCode;
        ScramblingShiftPerCharacter = scramblingShiftPerCharacter;
        ToneBitRotationPerSymbol = toneBitRotationPerSymbol;
        NegativeSetsTheBit = negativeSetsTheBit;
        SymbolToTone = symbolToTone;
        AnalysisWindowSymbols = analysisWindowSymbols;
        NullCharacter = nullCharacter;
        Variants = variants;
        _variants = variants.ToDictionary(v => v.Name, StringComparer.Ordinal);
    }

    /// <summary>Where the facts were transcribed from, as the file states it.</summary>
    public string Source { get; }

    /// <summary>Bits in one character.</summary>
    public int BitsPerCharacter { get; }

    /// <summary>Symbols in one block, which is also the length of one Walsh function.</summary>
    public int SymbolsPerBlock { get; }

    /// <summary>What a character is masked with before it is encoded.</summary>
    public int CharacterMask { get; }

    /// <summary>Whether a character in the upper half is its index less the half, negated.</summary>
    public bool UpperHalfNegated { get; }

    /// <summary>The inverse Walsh transform's butterfly: rows are outputs, columns inputs.</summary>
    public int[,] WalshInverseKernel { get; }

    /// <summary>The 64-bit scrambling code.</summary>
    public ulong ScramblingCode { get; }

    /// <summary>How far the code is moved along for each character of a block.</summary>
    public int ScramblingShiftPerCharacter { get; }

    /// <summary>How far a character's bit moves across the tone's bits from one symbol to the next.</summary>
    public int ToneBitRotationPerSymbol { get; }

    /// <summary>Whether a negative Walsh value sets the tone's bit.</summary>
    public bool NegativeSetsTheBit { get; }

    /// <summary>Which tone each symbol value is sent on: the Gray code, as a table.</summary>
    public IReadOnlyList<int> SymbolToTone { get; }

    /// <summary>How many symbol periods one symbol's shaped tone spans.</summary>
    public int AnalysisWindowSymbols { get; }

    /// <summary>The character a short block is filled with, which is idle and not text.</summary>
    public int NullCharacter { get; }

    /// <summary>Every variant the file carries, in the file's order.</summary>
    public IReadOnlyList<OliviaVariant> Variants { get; }

    /// <summary>A variant by the name the air uses for it, or null where the file has none.</summary>
    /// <param name="name">The variant, e.g. "16/500".</param>
    /// <returns>The variant, or null.</returns>
    public OliviaVariant? Variant(string name)
        => _variants.TryGetValue(name ?? "", out var variant) ? variant : null;

    /// <summary>Read the file's contents.</summary>
    /// <param name="json">The file's contents.</param>
    /// <returns>The format.</returns>
    /// <exception cref="InvalidDataException">The file is not what it should be, in words.</exception>
    public static OliviaFormat Parse(string json)
    {
        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                "it is not readable JSON (line " + (error.LineNumber + 1) + ")", error);
        }

        using (document)
        {
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("it does not hold a table");
            }

            var source = root.TryGetProperty("source", out var s) && s.ValueKind == JsonValueKind.String
                         && !string.IsNullOrWhiteSpace(s.GetString())
                ? s.GetString()!
                : throw new InvalidDataException("it does not say where its facts came from");

            var bitsPerCharacter = Whole(root, "bits_per_character", "its bits per character", 1);
            var symbolsPerBlock = Whole(root, "symbols_per_block", "its symbols per block", 2);

            if (symbolsPerBlock != 1 << (bitsPerCharacter - 1))
            {
                throw new InvalidDataException(
                    "its symbols per block, " + symbolsPerBlock + ", is not two to the power of one less than its "
                    + bitsPerCharacter + " bits per character");
            }

            var characterMask = Whole(root, "character_mask", "its character mask", 1);
            var upperHalfNegated = Flag(root, "upper_half_negated", "whether the upper half is negated");
            var kernel = Kernel(Value(root, "walsh_inverse_kernel", "its Walsh butterfly"));
            var code = Code(Value(root, "scrambling_code", "its scrambling code"));
            var shift = Whole(root, "scrambling_shift_per_character", "its scrambling shift", 0);
            var rotation = Whole(root, "tone_bit_rotation_per_symbol", "its tone bit rotation", 0);
            var negativeSets = Flag(root, "negative_sets_the_bit", "which sign sets a bit");
            var window = Whole(root, "analysis_window_symbols", "its analysis window", 1);
            var nullCharacter = Whole(root, "null_character", "its null character", 0);

            var tableElement = Value(root, "symbol_to_tone", "its Gray code table");

            if (tableElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException("its Gray code table is not a list");
            }

            var table = new List<int>();

            foreach (var entry in tableElement.EnumerateArray())
            {
                if (entry.ValueKind != JsonValueKind.Number || !entry.TryGetInt32(out var tone) || tone < 0)
                {
                    throw new InvalidDataException("its Gray code table holds something that is not a tone");
                }

                table.Add(tone);
            }

            if (table.Distinct().Count() != table.Count || table.Any(t => t >= table.Count))
            {
                throw new InvalidDataException("its Gray code table does not send each symbol on a tone of its own");
            }

            if (!root.TryGetProperty("variants", out var variantsElement)
                || variantsElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException("it carries no variants");
            }

            var variants = new List<OliviaVariant>();

            foreach (var row in variantsElement.EnumerateArray())
            {
                variants.Add(VariantRow(row, table.Count));
            }

            if (variants.Count == 0)
            {
                throw new InvalidDataException("it carries no variants");
            }

            if (variants.Select(v => v.Name).Distinct(StringComparer.Ordinal).Count() != variants.Count)
            {
                throw new InvalidDataException("it names a variant twice");
            }

            return new OliviaFormat(
                source, bitsPerCharacter, symbolsPerBlock, characterMask, upperHalfNegated, kernel, code, shift,
                rotation, negativeSets, table, window, nullCharacter, variants);
        }
    }

    private static OliviaVariant VariantRow(JsonElement row, int tableLength)
    {
        if (row.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException("a variant row is not a table");
        }

        var name = row.TryGetProperty("variant", out var n) && n.ValueKind == JsonValueKind.String
                   && !string.IsNullOrWhiteSpace(n.GetString())
            ? n.GetString()!
            : throw new InvalidDataException("a variant row has no name");

        var tones = RowWhole(row, "tones", name);
        var bandwidth = RowWhole(row, "bandwidth_hz", name);
        var bits = RowWhole(row, "bits_per_symbol", name);
        var spacing = RowNumber(row, "tone_spacing_hz", name);
        var symbol = RowNumber(row, "symbol_seconds", name);
        var offset = row.TryGetProperty("first_tone_offset_hz", out var o) && o.ValueKind == JsonValueKind.Number
            ? o.GetDouble()
            : throw new InvalidDataException("the " + name + " row's first tone offset is missing or not a number");

        // **THE ROW MUST AGREE WITH ITSELF**, by the rule the file states, or it is malformed.
        if (1 << bits != tones)
        {
            throw new InvalidDataException("the " + name + " row's " + tones + " tones are not two to the power of its " + bits + " bits");
        }

        if (tones > tableLength)
        {
            throw new InvalidDataException("the " + name + " row has more tones than the Gray code table covers");
        }

        if (Math.Abs((spacing * tones) - bandwidth) > 1e-9
            || Math.Abs((spacing * symbol) - 1) > 1e-9
            || Math.Abs(offset + ((tones - 1) / 2.0 * spacing)) > 1e-9)
        {
            throw new InvalidDataException(
                "the " + name + " row's spacing, symbol length and first tone disagree with its "
                + tones + " tones across " + bandwidth.ToString(CultureInfo.InvariantCulture) + " Hz");
        }

        return new OliviaVariant(name, tones, bandwidth, bits, spacing, symbol, offset);
    }

    private static JsonElement Value(JsonElement root, string name, string what)
    {
        if (!root.TryGetProperty(name, out var entry)
            || entry.ValueKind != JsonValueKind.Object
            || !entry.TryGetProperty("value", out var value)
            || !entry.TryGetProperty("cite", out var cite)
            || cite.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(cite.GetString()))
        {
            throw new InvalidDataException(what + " is missing or carries no citation");
        }

        return value;
    }

    private static int Whole(JsonElement root, string name, string what, int least)
    {
        var value = Value(root, name, what);

        return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var whole) && whole >= least
            ? whole
            : throw new InvalidDataException(what + " is not a whole number");
    }

    private static bool Flag(JsonElement root, string name, string what)
    {
        var value = Value(root, name, what);

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => throw new InvalidDataException(what + " is not true or false"),
        };
    }

    private static int[,] Kernel(JsonElement value)
    {
        var kernel = new int[2, 2];

        if (value.ValueKind != JsonValueKind.Array || value.GetArrayLength() != 2)
        {
            throw new InvalidDataException("its Walsh butterfly is not two rows");
        }

        var r = 0;

        foreach (var row in value.EnumerateArray())
        {
            if (row.ValueKind != JsonValueKind.Array || row.GetArrayLength() != 2)
            {
                throw new InvalidDataException("its Walsh butterfly is not two by two");
            }

            var c = 0;

            foreach (var cell in row.EnumerateArray())
            {
                if (cell.ValueKind != JsonValueKind.Number || !cell.TryGetInt32(out var sign) || Math.Abs(sign) != 1)
                {
                    throw new InvalidDataException("its Walsh butterfly holds something that is not plus or minus one");
                }

                kernel[r, c++] = sign;
            }

            r++;
        }

        if ((kernel[0, 0] * kernel[1, 1]) - (kernel[0, 1] * kernel[1, 0]) == 0)
        {
            throw new InvalidDataException("its Walsh butterfly cannot be undone");
        }

        return kernel;
    }

    private static ulong Code(JsonElement value)
        => value.ValueKind == JsonValueKind.String
           && ulong.TryParse(value.GetString(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var code)
            ? code
            : throw new InvalidDataException("its scrambling code is not a hexadecimal number");

    private static int RowWhole(JsonElement row, string name, string variant)
        => row.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var whole) && whole > 0
            ? whole
            : throw new InvalidDataException("the " + variant + " row's " + name.Replace('_', ' ') + " is missing or not a whole number");

    private static double RowNumber(JsonElement row, string name, string variant)
        => row.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number && v.GetDouble() > 0
            ? v.GetDouble()
            : throw new InvalidDataException("the " + variant + " row's " + name.Replace('_', ' ') + " is missing or not a number");
}

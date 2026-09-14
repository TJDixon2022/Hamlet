using System.Text.Json;

namespace Hamlet.RadioEngine.Rsid;

/// <summary>
/// **The RSID mode codes and their tone sequences, read from
/// <c>data/rsid/rsid-codes.json</c>.**
/// </summary>
/// <remarks>
/// <para>**PORTED DATA, CITED, AND NOTHING ELSE** (`PHASE_PLAN.md` R27). The codes and the
/// fifteen-tone sequences are fldigi's `rsid.cxx` and `rsid_defs.cxx` (GPL-3), and the file
/// says which revision. **There is no code literal in this file**: a mode code typed from
/// memory that is one off names a different mode on the air, and nothing downstream could
/// tell (§0.0, HM-DEC-054).</para>
/// <para>**IT DETECTS NOTHING AND SENDS NOTHING** (work instruction 358 task 1). Step 1 of
/// the Olivia phase builds the detector and the burst on top of this; step 0 only proves the
/// file is in the tree and read.</para>
/// </remarks>
public sealed class RsidCodes
{
    /// <summary>The file's name, as a sentence about it names it.</summary>
    public const string FilePath = "data/rsid/rsid-codes.json";

    private RsidCodes(
        string source,
        double symbolRateHz,
        int symbols,
        int silenceSymbolsBefore,
        int firstToneOffsetSymbols,
        IReadOnlyDictionary<string, int> codes,
        IReadOnlyDictionary<string, IReadOnlyList<int>> toneSequences)
    {
        Source = source;
        SymbolRateHz = symbolRateHz;
        Symbols = symbols;
        SilenceSymbolsBefore = silenceSymbolsBefore;
        FirstToneOffsetSymbols = firstToneOffsetSymbols;
        Codes = codes;
        ToneSequences = toneSequences;
    }

    /// <summary>How many symbols of silence a burst starts with, as the file states it.</summary>
    /// <remarks>
    /// **READ, NOT TYPED** (work instruction 359 task 2). The detector and the burst both
    /// need the burst's shape, and the file is the one place it is written.
    /// </remarks>
    public int SilenceSymbolsBefore { get; }

    /// <summary>
    /// Where tone 0 sits, in tone steps from the center - negative is below it - as the file
    /// states it.
    /// </summary>
    public int FirstToneOffsetSymbols { get; }

    /// <summary>Where the codes were ported from, as the file states it.</summary>
    public string Source { get; }

    /// <summary>The burst's symbol rate in hertz, as the file states it.</summary>
    public double SymbolRateHz { get; }

    /// <summary>How many tones one burst carries.</summary>
    public int Symbols { get; }

    /// <summary>Every mode code the file carries, by fldigi's name for the mode.</summary>
    public IReadOnlyDictionary<string, int> Codes { get; }

    /// <summary>The tone sequence of each burst the file carries, by the same names.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<int>> ToneSequences { get; }

    /// <summary>The code for a mode, or null where the file has none.</summary>
    /// <param name="name">fldigi's name for the mode, e.g. "OLIVIA_8_250".</param>
    /// <returns>The code, or null.</returns>
    /// <remarks>
    /// **A NAME THE FILE DOES NOT CARRY HAS NO CODE**, rather than the nearest one. A near
    /// miss here is a different mode announced on the air.
    /// </remarks>
    public int? CodeOf(string? name)
        => name is not null && Codes.TryGetValue(name.Trim(), out var code) ? code : null;

    /// <summary>Parse the codes from the file's text.</summary>
    /// <param name="json">The file's contents.</param>
    /// <returns>The codes.</returns>
    /// <exception cref="InvalidDataException">The file is not what it should be, in words.</exception>
    /// <remarks>
    /// **STRICT, BECAUSE A HALF-READ TABLE IS WORSE THAN NONE.** A code that is not a whole
    /// number, or a sequence that is not as long as the burst, fails the whole file rather
    /// than being skipped: skipping it would leave a table that looks complete.
    /// </remarks>
    public static RsidCodes Parse(string json)
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
                ? s.GetString()!
                : throw new InvalidDataException("it does not say where its codes came from");

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new InvalidDataException("it does not say where its codes came from");
            }

            var rate = root.TryGetProperty("symbol_rate_hz", out var r)
                       && r.ValueKind == JsonValueKind.Number
                       && r.GetDouble() > 0
                ? r.GetDouble()
                : throw new InvalidDataException("its symbol rate is missing or not a number");

            var symbols = root.TryGetProperty("symbols", out var n)
                          && n.ValueKind == JsonValueKind.Number
                          && n.TryGetInt32(out var count)
                          && count > 0
                ? count
                : throw new InvalidDataException("its symbol count is missing or not a whole number");

            var silence = root.TryGetProperty("silence_symbols_before", out var q)
                          && q.ValueKind == JsonValueKind.Number
                          && q.TryGetInt32(out var quiet)
                          && quiet >= 0
                ? quiet
                : throw new InvalidDataException("its silence before a burst is missing or not a whole number");

            var firstTone = root.TryGetProperty("first_tone_offset_symbols", out var f)
                            && f.ValueKind == JsonValueKind.Number
                            && f.TryGetInt32(out var offset)
                ? offset
                : throw new InvalidDataException("its first tone's offset is missing or not a whole number");

            if (!root.TryGetProperty("codes", out var codesElement)
                || codesElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("it carries no codes");
            }

            var codes = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var code in codesElement.EnumerateObject())
            {
                if (code.Value.ValueKind != JsonValueKind.Number
                    || !code.Value.TryGetInt32(out var value)
                    || value < 0)
                {
                    throw new InvalidDataException(
                        "the code for " + code.Name + " is not a whole number");
                }

                codes[code.Name] = value;
            }

            if (codes.Count == 0)
            {
                throw new InvalidDataException("it carries no codes");
            }

            var sequences = new Dictionary<string, IReadOnlyList<int>>(StringComparer.Ordinal);

            if (root.TryGetProperty("tone_sequences", out var sequencesElement))
            {
                if (sequencesElement.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidDataException("its tone sequences are not a table");
                }

                foreach (var sequence in sequencesElement.EnumerateObject())
                {
                    if (sequence.Value.ValueKind != JsonValueKind.Array)
                    {
                        throw new InvalidDataException(
                            "the tone sequence for " + sequence.Name + " is not a list");
                    }

                    var tones = new List<int>();

                    foreach (var tone in sequence.Value.EnumerateArray())
                    {
                        if (tone.ValueKind != JsonValueKind.Number
                            || !tone.TryGetInt32(out var value)
                            || value < 0)
                        {
                            throw new InvalidDataException(
                                "the tone sequence for " + sequence.Name
                                + " holds something that is not a tone");
                        }

                        tones.Add(value);
                    }

                    if (tones.Count != symbols)
                    {
                        throw new InvalidDataException(
                            "the tone sequence for " + sequence.Name + " is " + tones.Count
                            + " tones long and a burst is " + symbols);
                    }

                    sequences[sequence.Name] = tones;
                }
            }

            return new RsidCodes(source, rate, symbols, silence, firstTone, codes, sequences);
        }
    }
}

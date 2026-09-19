using System.Text.Json;

namespace Hamlet.RadioEngine.Olivia;

/// <summary>
/// **Seconds per character for each Olivia variant, read from <c>data/olivia/timing.json</c>.**
/// </summary>
/// <remarks>
/// <para>**THE FIRST ROW OF THE MODE'S TIMING TABLE** (`PHASE_PLAN.md` §3.2, step 2 criterion 2.6),
/// which every later timing rule derives from. The file says whether its figures were estimated
/// or measured and how, and this hands out both the figures and that word.</para>
/// <para>**STRICT, FOR `RsidCodes`'s REASON.** A row without a positive figure fails the file
/// rather than being skipped.</para>
/// </remarks>
public sealed class OliviaTiming
{
    /// <summary>The file's name, as a sentence about it names it.</summary>
    public const string FilePath = "data/olivia/timing.json";

    private readonly Dictionary<string, double> _secondsPerCharacter;

    private OliviaTiming(string source, string method, Dictionary<string, double> secondsPerCharacter, int retireAfterCharacters)
    {
        Source = source;
        Method = method;
        _secondsPerCharacter = secondsPerCharacter;
        RetireAfterCharacters = retireAfterCharacters;
    }

    /// <summary>Whether the figures were estimated or measured, as the file says.</summary>
    public string Source { get; }

    /// <summary>How they were arrived at, as the file says.</summary>
    public string Method { get; }

    /// <summary>Every variant's figure, by the name the air uses for the variant.</summary>
    public IReadOnlyDictionary<string, double> SecondsPerCharacter => _secondsPerCharacter;

    /// <summary>**The retire window's factor, in the variant's characters**, as the file states it.</summary>
    /// <remarks>
    /// Step 3 criterion 3.4 and work instruction 364 decision AJ: a channel is retired once this many
    /// characters' worth of its variant's time has passed since its last accepted block ended. The
    /// file says why the number is what it is.
    /// </remarks>
    public int RetireAfterCharacters { get; }

    /// <summary>**The retire window for a variant, in seconds**: its seconds per character times <see cref="RetireAfterCharacters"/>.</summary>
    /// <param name="variant">The variant's name, as the air spells it.</param>
    /// <returns>The window, or NaN for a variant the table has no row for.</returns>
    /// <remarks>
    /// **NEVER A FIGURE IN SECONDS** (`PHASE_PLAN.md` §3.2): every timing rule scales with the variant.
    /// A variant with no row has no window, and nothing is retired on a guess.
    /// </remarks>
    public double RetireWindowSeconds(string variant)
        => _secondsPerCharacter.TryGetValue(variant, out var seconds) ? seconds * RetireAfterCharacters : double.NaN;

    /// <summary>Read the file's contents.</summary>
    /// <param name="json">The file's contents.</param>
    /// <returns>The table.</returns>
    /// <exception cref="InvalidDataException">The file is not what it should be, in words.</exception>
    public static OliviaTiming Parse(string json)
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

            var source = Text(root, "source") ?? throw new InvalidDataException("it does not say whether it was estimated or measured");
            var method = Text(root, "method") ?? throw new InvalidDataException("it does not say how its figures were arrived at");

            if (!root.TryGetProperty("variants", out var rows) || rows.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException("it carries no variants");
            }

            var figures = new Dictionary<string, double>(StringComparer.Ordinal);

            foreach (var row in rows.EnumerateArray())
            {
                var name = row.ValueKind == JsonValueKind.Object ? Text(row, "variant") : null;

                if (name is null)
                {
                    throw new InvalidDataException("a variant row has no name");
                }

                if (!row.TryGetProperty("seconds_per_character", out var figure)
                    || figure.ValueKind != JsonValueKind.Number
                    || figure.GetDouble() <= 0)
                {
                    throw new InvalidDataException("the " + name + " row's seconds per character is missing or not a positive number");
                }

                if (!figures.TryAdd(name, figure.GetDouble()))
                {
                    throw new InvalidDataException("it names " + name + " twice");
                }
            }

            if (figures.Count == 0)
            {
                throw new InvalidDataException("it carries no variants");
            }

            if (!root.TryGetProperty("retire_after_characters", out var retire)
                || retire.ValueKind != JsonValueKind.Number
                || !retire.TryGetInt32(out var factor)
                || factor <= 0)
            {
                throw new InvalidDataException("its retire_after_characters is missing or not a positive whole number");
            }

            return new OliviaTiming(source, method, figures, factor);
        }
    }

    private static string? Text(JsonElement element, string name)
        => element.TryGetProperty(name, out var value)
           && value.ValueKind == JsonValueKind.String
           && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()
            : null;
}

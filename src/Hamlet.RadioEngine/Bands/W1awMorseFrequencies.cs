using System.Reflection;
using System.Text.Json;

namespace Hamlet.RadioEngine.Bands;

/// <summary>One of W1AW's Morse code frequencies.</summary>
/// <param name="Band">The band as Hamlet spells it, e.g. "40 m".</param>
/// <param name="FrequencyHz">The frequency W1AW sends Morse on there, in hertz.</param>
public sealed record W1awMorseRow(string Band, long FrequencyHz);

/// <summary>
/// **Where W1AW sends Morse, read from <c>data/bands/w1aw-morse.json</c>** (work instruction 494,
/// HM-DEC-199).
/// </summary>
/// <remarks>
/// <para>**THE ARRL'S OWN SCHEDULE IS THE SOURCE**, cited in the file, and there is no frequency
/// literal in this class: a correction is one edit to the file (§0).</para>
/// <para>**FREQUENCIES AND NOTHING ABOUT WHEN.** W1AW's times are US Central, the same all year,
/// so they move against UTC with daylight saving; the file carries no times and nothing here
/// computes one, because a wrong time on screen is a false sentence (§0.0).</para>
/// <para>**EVERY ROW THE ARRL GIVES**, 2 m included. Which of them a surface offers is the
/// surface's to decide, from what the radio can tune and what Hamlet knows of the spectrum.</para>
/// </remarks>
public sealed class W1awMorseFrequencies
{
    /// <summary>The embedded resource the file is built into.</summary>
    public const string ResourceName = "Hamlet.RadioEngine.Data.Bands.w1aw-morse.json";

    private static readonly Lazy<W1awMorseFrequencies> Shipped = new(Load);

    private W1awMorseFrequencies(string about, string source, string speeds, IReadOnlyList<W1awMorseRow> rows)
    {
        About = about;
        Source = source;
        Speeds = speeds;
        Rows = rows;
    }

    /// <summary>The table the build carries.</summary>
    public static W1awMorseFrequencies Default => Shipped.Value;

    /// <summary>What the file says it is, in its own words.</summary>
    public string About { get; }

    /// <summary>Where the rows were read from, as the file states it.</summary>
    public string Source { get; }

    /// <summary>What W1AW sends and how fast, as the file states it.</summary>
    public string Speeds { get; }

    /// <summary>Every row, lowest band first, as the file gives them.</summary>
    public IReadOnlyList<W1awMorseRow> Rows { get; }

    /// <summary>
    /// The amateur band a frequency is in, named as Hamlet names it, or null where it is in none the
    /// privileges data carries (work instruction 495).
    /// </summary>
    /// <param name="frequencyHz">A frequency.</param>
    /// <returns>"40 m", or null.</returns>
    /// <remarks>
    /// Read from the Extra class's allocations in the privileges data, the widest there are, so every
    /// amateur band is named whether Hamlet maps it or not. The regulation's 75 m is the top of 80 m,
    /// joined as <see cref="HfBands"/> joins them.
    /// </remarks>
    public static string? AmateurBandFor(long frequencyHz)
    {
        if (!Licensing.PrivilegeData.Current.ClassBands.TryGetValue(Licensing.LicenseClass.Extra, out var ranges))
        {
            return null;
        }

        foreach (var range in ranges)
        {
            if (frequencyHz >= range.LowHz && frequencyHz <= range.HighHz)
            {
                return range.Band == "75 m" ? "80 m" : range.Band;
            }
        }

        return null;
    }

    /// <summary>W1AW's row for the band a frequency is in, or null where it has none there.</summary>
    /// <param name="frequencyHz">Where the dial is.</param>
    /// <returns>The row, or null.</returns>
    public W1awMorseRow? RowForBandOf(long frequencyHz)
        => AmateurBandFor(frequencyHz) is { } band
            ? Rows.FirstOrDefault(r => string.Equals(r.Band, band, StringComparison.Ordinal))
            : null;

    /// <summary>Parse the table from the file's text.</summary>
    /// <param name="json">The file's contents.</param>
    /// <returns>The table.</returns>
    /// <exception cref="InvalidDataException">The file is not what it should be, in words.</exception>
    /// <remarks>Strict: a row skipped is a band silently without a button.</remarks>
    public static W1awMorseFrequencies Parse(string json)
    {
        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("it is not readable JSON (line " + (error.LineNumber + 1) + ")", error);
        }

        using (document)
        {
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("it does not hold a table");
            }

            var source = Text(root, "source");

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new InvalidDataException("it does not say where its rows came from");
            }

            var about = Text(root, "_about");

            if (string.IsNullOrWhiteSpace(about))
            {
                throw new InvalidDataException("it does not say what kind of table it is");
            }

            if (!root.TryGetProperty("rows", out var rowsElement) || rowsElement.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException("it carries no rows");
            }

            var rows = new List<W1awMorseRow>();

            foreach (var element in rowsElement.EnumerateArray())
            {
                var band = element.ValueKind == JsonValueKind.Object ? Text(element, "band") : "";

                if (string.IsNullOrWhiteSpace(band))
                {
                    throw new InvalidDataException("row " + (rows.Count + 1) + " names no band");
                }

                if (!element.TryGetProperty("hz", out var hzElement)
                    || hzElement.ValueKind != JsonValueKind.Number
                    || !hzElement.TryGetInt64(out var hz)
                    || hz <= 0)
                {
                    throw new InvalidDataException("the " + band + " row's frequency is missing or not a whole number of hertz");
                }

                rows.Add(new W1awMorseRow(band, hz));
            }

            if (rows.Count == 0)
            {
                throw new InvalidDataException("it carries no rows");
            }

            return new W1awMorseFrequencies(about, source, Text(root, "speeds"), rows);
        }
    }

    private static W1awMorseFrequencies Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                "W1AW's frequencies are missing: the build did not include data/bands/w1aw-morse.json");
        using var reader = new StreamReader(stream);

        return Parse(reader.ReadToEnd());
    }

    private static string Text(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";
}

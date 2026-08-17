using System.Text.Json;
using System.Text.Json.Serialization;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.RadioEngine.Scan;

/// <summary>
/// One stretch of band a scan is allowed inside (HM-DEC-107, §0.2.1).
/// </summary>
/// <param name="Band">Which band, by its display name.</param>
/// <param name="Name">What the stretch is called.</param>
/// <param name="LowHz">Its lower edge.</param>
/// <param name="HighHz">Its upper edge.</param>
/// <param name="Cite">Where the frequencies came from.</param>
/// <param name="Enabled">Whether the operator wants it scanned.</param>
/// <remarks>
/// **THE EDGES ARE THE SCAN'S WALLS AND NOT A PREFERENCE.** A scanner that can
/// leave its segment can put the operator's dial anywhere, including outside his
/// privileges and outside the band, so the check is against these two numbers
/// every time the dial moves rather than once when the scan starts (§0.2.1).
/// </remarks>
public sealed record ScanSegment(
    string Band, string Name, long LowHz, long HighHz, string Cite, bool Enabled = true)
{
    /// <summary>True when the frequency lies inside this stretch.</summary>
    public bool Contains(long hz) => hz >= LowHz && hz <= HighHz;

    /// <summary>How wide it is, in hertz.</summary>
    public long WidthHz => Math.Max(0, HighHz - LowHz);
}

/// <summary>
/// Where a scan may go, as the operator configured it (HM-DEC-107, §0.2.1).
/// </summary>
/// <remarks>
/// <para>**THERE IS NO FREQUENCY LITERAL IN THIS FILE AND THAT IS THE POINT.**
/// §0.2.1 requires the scanned stretch to come from a data file the operator
/// edits, and §0 requires anything derivable from a source of truth to be
/// derived rather than copied. Both are satisfied the same way: the shipped
/// default is **generated** from the cited Morse rows in
/// <c>data/bands/us-neighborhoods.json</c>, so every segment arrives carrying
/// the citation the neighborhood row carried, and a correction to that file
/// reaches the scanner without anybody remembering to make it twice.</para>
/// <para>**THE OPERATOR'S OWN FILE WINS OUTRIGHT WHEN IT EXISTS.** Not merged,
/// not defaulted into: merging would mean a segment he deleted comes back on
/// the next release, which is the app overruling him about where his own radio
/// may go.</para>
/// <para>A file that cannot be read is refused loudly rather than quietly
/// replaced with the default, because a scan running over a stretch the
/// operator did not choose is exactly what §0.2.1 forbids.</para>
/// </remarks>
public sealed class ScanSegments
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private ScanSegments(IReadOnlyList<ScanSegment> segments, string origin)
    {
        All = segments;
        Origin = origin;
    }

    /// <summary>Every stretch in the file, in the order it was written.</summary>
    public IReadOnlyList<ScanSegment> All { get; }

    /// <summary>Where these came from, for the record and the screen.</summary>
    public string Origin { get; }

    /// <summary>The ones the operator has switched on.</summary>
    public IReadOnlyList<ScanSegment> Enabled
        => All.Where(s => s.Enabled).ToList();

    /// <summary>
    /// The shipped default, generated from the cited Morse neighborhoods.
    /// </summary>
    /// <remarks>
    /// Morse only, because this is the CW scanner and a dwell scored by a Morse
    /// decoder has nothing to say about a stretch of FT8. Widening it is a
    /// ruling and not a session's to take.
    /// </remarks>
    public static ScanSegments Default { get; } = BuildDefault();

    /// <summary>
    /// Load the operator's file, or fall back to the generated default.
    /// </summary>
    /// <param name="path">Where his file would be.</param>
    /// <returns>What a scan may use.</returns>
    /// <exception cref="InvalidDataException">
    /// The file exists and cannot be read. **Deliberately thrown rather than
    /// swallowed**: silently substituting the default would run the scan over a
    /// stretch he did not choose (§0.2.1).
    /// </exception>
    public static ScanSegments LoadOrDefault(string path)
    {
        if (!File.Exists(path))
        {
            return Default;
        }

        string json;

        try
        {
            json = File.ReadAllText(path);
        }
        catch (IOException error)
        {
            throw new InvalidDataException(
                $"the scan segments file at {path} could not be read: {error.Message}",
                error);
        }
        catch (UnauthorizedAccessException error)
        {
            throw new InvalidDataException(
                $"the scan segments file at {path} could not be read: {error.Message}",
                error);
        }

        return Parse(json, $"the file at {path}");
    }

    /// <summary>Parse scan segments from JSON.</summary>
    /// <param name="json">The file's contents.</param>
    /// <param name="origin">What to call it when reporting.</param>
    /// <returns>The parsed segments.</returns>
    /// <exception cref="InvalidDataException">The file is unusable.</exception>
    public static ScanSegments Parse(string json, string origin = "a file")
    {
        ScanFile? file;

        try
        {
            file = JsonSerializer.Deserialize<ScanFile>(json, JsonOptions);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                $"{origin} is not readable JSON: {error.Message}", error);
        }

        if (file?.Segments is null || file.Segments.Count == 0)
        {
            throw new InvalidDataException($"{origin} names no segments at all");
        }

        var segments = new List<ScanSegment>();

        foreach (var row in file.Segments)
        {
            if (row.HighHz <= row.LowHz)
            {
                throw new InvalidDataException(
                    $"{origin} has a segment named '{row.Name}' running from "
                    + $"{row.LowHz} to {row.HighHz}, which is backwards or empty");
            }

            segments.Add(new ScanSegment(
                row.Band ?? "",
                row.Name ?? "",
                row.LowHz,
                row.HighHz,
                row.Cite ?? "no source stated in the file",
                row.Enabled));
        }

        return new ScanSegments(segments, origin);
    }

    /// <summary>
    /// Write the default out so there is something to edit.
    /// </summary>
    /// <param name="path">Where to write it.</param>
    /// <remarks>
    /// **WRITTEN ONCE AND NEVER OVERWRITTEN.** A file the operator has edited is
    /// his, and a release that refreshed it would silently undo his decisions
    /// about where his own radio may go (§0.2.1).
    /// </remarks>
    public static void WriteDefaultIfMissing(string path)
    {
        if (File.Exists(path))
        {
            return;
        }

        var folder = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllText(path, Default.ToJson());
    }

    /// <summary>Render as the JSON the operator would edit.</summary>
    public string ToJson()
    {
        var file = new ScanFile
        {
            Schema = "hamlet.scan-segments/1",
            About = new[]
            {
                "Where Hamlet's Morse scanner is allowed to move your dial.",
                "",
                "THIS FILE IS YOURS. Hamlet writes it once, the first time it",
                "needs it, and never touches it again. Delete a segment and the",
                "scanner will not go there; set enabled to false to keep the row",
                "and skip it for now.",
                "",
                "The scanner will not tune outside these edges, whatever else",
                "goes wrong. That is the point of the file (CLAUDE.md 0.2.1).",
                "",
                "The numbers Hamlet generated are not typed in anywhere. They",
                "come from the Morse rows of data/bands/us-neighborhoods.json,",
                "so each one carries the source that row carried, and a",
                "correction there reaches the scanner without being made twice",
                "(CLAUDE.md 0).",
                "",
                "WHAT MAY BE TRANSMITTED IS NOT IN THIS FILE and this file has",
                "no legal weight. Your privileges live in",
                "data/privileges/us-part97-privileges.json. A scan never",
                "transmits in any case (CLAUDE.md 0.2).",
            },
            Segments = All.Select(s => new SegmentRow
            {
                Band = s.Band,
                Name = s.Name,
                LowHz = s.LowHz,
                HighHz = s.HighHz,
                Cite = s.Cite,
                Enabled = s.Enabled,
            }).ToList(),
        };

        return JsonSerializer.Serialize(file, JsonOptions);
    }

    private static ScanSegments BuildDefault()
    {
        var data = NeighborhoodData.Current;
        var segments = new List<ScanSegment>();

        foreach (var band in data.Bands)
        {
            foreach (var hood in data.ForBand(band))
            {
                if (hood.Family != ModeFamily.Cw)
                {
                    continue;
                }

                segments.Add(new ScanSegment(
                    band,
                    hood.Name,
                    hood.LowHz,
                    hood.HighHz,
                    string.IsNullOrWhiteSpace(hood.Cite)
                        ? "no source stated on the neighborhood row"
                        : hood.Cite));
            }
        }

        return new ScanSegments(
            segments,
            "the Morse rows of data/bands/us-neighborhoods.json");
    }

    private sealed class ScanFile
    {
        [JsonPropertyName("schema")]
        public string? Schema { get; set; }

        [JsonPropertyName("_about")]
        public IReadOnlyList<string>? About { get; set; }

        [JsonPropertyName("segments")]
        public List<SegmentRow>? Segments { get; set; }
    }

    private sealed class SegmentRow
    {
        [JsonPropertyName("band")]
        public string? Band { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("lowHz")]
        public long LowHz { get; set; }

        [JsonPropertyName("highHz")]
        public long HighHz { get; set; }

        [JsonPropertyName("cite")]
        public string? Cite { get; set; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true;
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hamlet.RadioEngine.Licensing;

/// <summary>One frequency range from a table in the regulation.</summary>
/// <param name="Band">Band name as the CFR names it, e.g. "40 m".</param>
/// <param name="LowHz">Lower edge.</param>
/// <param name="HighHz">Upper edge.</param>
public readonly record struct FrequencyRange(string Band, long LowHz, long HighHz)
{
    /// <summary>True when the frequency lies inside this range.</summary>
    /// <param name="hz">Frequency in hertz.</param>
    /// <returns>True when contained.</returns>
    public bool Contains(long hz) => hz >= LowHz && hz <= HighHz;
}

/// <summary>A row of 47 CFR 97.305(c): where an emission type may be sent.</summary>
/// <param name="Range">The frequencies.</param>
/// <param name="Emissions">Emission types this row authorises.</param>
/// <param name="Cite">The paragraph, e.g. "97.305(c)(3)(vii)".</param>
/// <param name="Standards">Standards from 97.307(f) that qualify it.</param>
public sealed record EmissionRange(
    FrequencyRange Range,
    IReadOnlyList<TransmitMode> Emissions,
    string Cite,
    IReadOnlyList<string> Standards);

/// <summary>A citation for something Hamlet states about the regulation.</summary>
/// <param name="Id">Section, e.g. "97.301(d)".</param>
/// <param name="Title">Human-readable title.</param>
/// <param name="Url">Where to read it.</param>
/// <param name="Authority">"regulation" or "convenience".</param>
public sealed record PrivilegeSource(string Id, string Title, string Url, string Authority);

/// <summary>A figure the source does not state, carried loud (CLAUDE.md §4).</summary>
/// <param name="Topic">What is missing.</param>
/// <param name="Reason">Why, and what must happen before it is needed.</param>
public sealed record PrivilegeUnknown(string Topic, string Reason);

/// <summary>
/// The cited Part 97 privileges data, loaded from
/// <c>data/privileges/us-part97-privileges.json</c>.
/// </summary>
/// <remarks>
/// <para>This closes the data half of HM-OPEN-005. The frequencies here are a
/// transcription of 47 CFR 97.301 and 97.305 taken from eCFR, not knowledge
/// carried in anybody's head, because being wrong here means telling somebody
/// they may transmit where they may not.</para>
/// <para>The two CFR tables stay separate, as they are in the source, and the
/// join that answers "may this class send this mode here" happens in
/// <see cref="PrivilegePlan"/> with tests. A pre-joined table would be a third
/// artefact free to disagree with both its parents (§0).</para>
/// <para>The file is an embedded resource so it ships with the app while
/// living in <c>/data</c> as its single source of truth — one file, no copy to
/// drift.</para>
/// </remarks>
public sealed class PrivilegeData
{
    /// <summary>Resource name of the embedded privileges file.</summary>
    public const string ResourceName =
        "Hamlet.RadioEngine.Data.Privileges.us-part97-privileges.json";

    private static readonly Lazy<PrivilegeData> Shared = new(LoadEmbedded);

    private PrivilegeData(
        IReadOnlyDictionary<LicenseClass, IReadOnlyList<FrequencyRange>> classBands,
        IReadOnlyList<EmissionRange> emissionRanges,
        IReadOnlyList<PrivilegeSource> sources,
        IReadOnlyList<PrivilegeUnknown> unknowns,
        IReadOnlyDictionary<LicenseClass, UpgradePath> upgrades,
        string retrievedUtc)
    {
        ClassBands = classBands;
        EmissionRanges = emissionRanges;
        Sources = sources;
        Unknowns = unknowns;
        Upgrades = upgrades;
        RetrievedUtc = retrievedUtc;
    }

    /// <summary>The shipped data.</summary>
    public static PrivilegeData Current => Shared.Value;

    /// <summary>47 CFR 97.301: frequencies each class may transmit on.</summary>
    public IReadOnlyDictionary<LicenseClass, IReadOnlyList<FrequencyRange>> ClassBands { get; }

    /// <summary>47 CFR 97.305(c): where each non-CW emission may be sent.</summary>
    public IReadOnlyList<EmissionRange> EmissionRanges { get; }

    /// <summary>What this data cites.</summary>
    public IReadOnlyList<PrivilegeSource> Sources { get; }

    /// <summary>What it deliberately does not cover.</summary>
    public IReadOnlyList<PrivilegeUnknown> Unknowns { get; }

    /// <summary>What the next class up buys, per class.</summary>
    public IReadOnlyDictionary<LicenseClass, UpgradePath> Upgrades { get; }

    /// <summary>When the regulation was read, as stated in the file.</summary>
    public string RetrievedUtc { get; }

    /// <summary>The regulation sources, excluding convenience references.</summary>
    public IEnumerable<PrivilegeSource> Authorities
        => Sources.Where(s => string.Equals(s.Authority, "regulation", StringComparison.Ordinal));

    /// <summary>Parse privileges data from JSON.</summary>
    /// <param name="json">The file's contents.</param>
    /// <returns>The parsed data.</returns>
    /// <exception cref="InvalidDataException">The file is unusable.</exception>
    public static PrivilegeData Parse(string json)
    {
        var dto = JsonSerializer.Deserialize<PrivilegeFile>(json, JsonOptions)
                  ?? throw new InvalidDataException("privileges file is empty");

        if (dto.ClassFrequencyBands is null || dto.EmissionRanges?.Ranges is null)
        {
            throw new InvalidDataException("privileges file is missing its CFR tables");
        }

        var classBands = new Dictionary<LicenseClass, IReadOnlyList<FrequencyRange>>();
        foreach (var pair in dto.ClassFrequencyBands)
        {
            // "_cite" and "_note" sit beside the class entries so the file can
            // carry its own citations. They are documentation, not data.
            if (pair.Key.StartsWith('_')
                || !Enum.TryParse<LicenseClass>(pair.Key, out var cls)
                || pair.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var band = pair.Value.Deserialize<ClassBandsDto>(JsonOptions);
            if (band?.Ranges is null)
            {
                continue;
            }

            classBands[cls] = band.Ranges
                .Select(r => new FrequencyRange(r.Band ?? "", r.LowHz, r.HighHz))
                .ToList();
        }

        var emissions = dto.EmissionRanges.Ranges
            .Select(r => new EmissionRange(
                new FrequencyRange(r.Band ?? "", r.LowHz, r.HighHz),
                (r.Emissions ?? Array.Empty<string>())
                    .Select(ParseMode)
                    .Where(m => m is not null)
                    .Select(m => m!.Value)
                    .ToList(),
                r.Cite ?? "",
                r.Standards ?? Array.Empty<string>()))
            .ToList();

        var upgrades = new Dictionary<LicenseClass, UpgradePath>();
        foreach (var pair in dto.UpgradePaths ?? new Dictionary<string, JsonElement>())
        {
            if (pair.Key.StartsWith('_')
                || !Enum.TryParse<LicenseClass>(pair.Key, out var cls)
                || pair.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var path = pair.Value.Deserialize<UpgradeDto>(JsonOptions);
            if (path is null)
            {
                continue;
            }

            var next = Enum.TryParse<LicenseClass>(path.Next ?? "", out var n)
                ? n
                : LicenseClass.Unknown;

            upgrades[cls] = new UpgradePath(next, path.Headline ?? "");
        }

        if (classBands.Count == 0 || emissions.Count == 0)
        {
            throw new InvalidDataException("privileges file parsed to nothing usable");
        }

        return new PrivilegeData(
            classBands,
            emissions,
            (dto.Sources ?? Array.Empty<SourceDto>())
                .Select(s => new PrivilegeSource(
                    s.Id ?? "", s.Title ?? "", s.Url ?? "", s.Authority ?? ""))
                .ToList(),
            (dto.Unknowns ?? Array.Empty<UnknownDto>())
                .Select(u => new PrivilegeUnknown(u.Topic ?? "", u.Reason ?? ""))
                .ToList(),
            upgrades,
            dto.RetrievedUtc ?? "");
    }

    private static TransmitMode? ParseMode(string name)
        => name.Trim().ToUpperInvariant() switch
        {
            "CW" => TransmitMode.Cw,
            "DATA" or "RTTY" or "RTTY, DATA" => TransmitMode.Data,
            "PHONE" => TransmitMode.Phone,
            "IMAGE" => TransmitMode.Image,
            _ => null,
        };

    private static PrivilegeData LoadEmbedded()
    {
        var assembly = typeof(PrivilegeData).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException(
                $"embedded privileges data '{ResourceName}' is missing; "
                + "Hamlet cannot state anything about transmit privileges without it");

        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private sealed class PrivilegeFile
    {
        [JsonPropertyName("retrievedUtc")]
        public string? RetrievedUtc { get; set; }

        [JsonPropertyName("sources")]
        public SourceDto[]? Sources { get; set; }

        [JsonPropertyName("classFrequencyBands")]
        public Dictionary<string, JsonElement>? ClassFrequencyBands { get; set; }

        [JsonPropertyName("emissionRanges")]
        public EmissionRangesDto? EmissionRanges { get; set; }

        [JsonPropertyName("upgradePaths")]
        public Dictionary<string, JsonElement>? UpgradePaths { get; set; }

        [JsonPropertyName("unknowns")]
        public UnknownDto[]? Unknowns { get; set; }
    }

    private sealed class ClassBandsDto
    {
        [JsonPropertyName("ranges")]
        public RangeDto[]? Ranges { get; set; }
    }

    private sealed class EmissionRangesDto
    {
        [JsonPropertyName("ranges")]
        public RangeDto[]? Ranges { get; set; }
    }

    private sealed class RangeDto
    {
        [JsonPropertyName("band")]
        public string? Band { get; set; }

        [JsonPropertyName("lowHz")]
        public long LowHz { get; set; }

        [JsonPropertyName("highHz")]
        public long HighHz { get; set; }

        [JsonPropertyName("emissions")]
        public string[]? Emissions { get; set; }

        [JsonPropertyName("cite")]
        public string? Cite { get; set; }

        [JsonPropertyName("standards")]
        public string[]? Standards { get; set; }
    }

    private sealed class UpgradeDto
    {
        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("headline")]
        public string? Headline { get; set; }
    }

    private sealed class SourceDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("authority")]
        public string? Authority { get; set; }
    }

    private sealed class UnknownDto
    {
        [JsonPropertyName("topic")]
        public string? Topic { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }
}

/// <summary>What the next license class up buys.</summary>
/// <param name="Next">The class above, or Unknown at the top.</param>
/// <param name="Headline">One line naming what it opens.</param>
public sealed record UpgradePath(LicenseClass Next, string Headline);

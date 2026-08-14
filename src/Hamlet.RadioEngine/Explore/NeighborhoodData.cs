using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>Where a neighborhood's frequencies came from.</summary>
/// <param name="Id">The id rows cite.</param>
/// <param name="Title">What the source is called.</param>
/// <param name="Publisher">Who publishes it.</param>
/// <param name="Url">Where to check it.</param>
/// <param name="Authority">
/// "convention" for a community's own published list, "derived" for something
/// computed from cited data elsewhere in the repository.
/// </param>
public sealed record NeighborhoodSource(
    string Id, string Title, string Publisher, string Url, string Authority);

/// <summary>Something the file deliberately does not say (CLAUDE.md §4).</summary>
/// <param name="Topic">What is missing.</param>
/// <param name="Reason">Why, and what has to happen before it is filled in.</param>
public sealed record NeighborhoodUnknown(string Topic, string Reason);

/// <summary>
/// The cited neighborhood conventions, loaded from
/// <c>data/bands/us-neighborhoods.json</c>.
/// </summary>
/// <remarks>
/// <para>THE BUG THAT PUT THIS FILE HERE. The map labeled the whole of 14.000
/// to 14.150 as Morse, so an operator who tuned to 14.075 and heard what he
/// described as whale song had no way to learn that he was sitting in the FT8
/// watering hole, one of the busiest slices of spectrum on Earth. The card
/// under the map said his license covered Morse there and invited him to call
/// away, which is true about the regulation and wrong about the world
/// (HM-DEC-054).</para>
/// <para>CONVENTION, NOT REGULATION, and the two are kept in separate files on
/// purpose. What may be transmitted is in <c>data/privileges</c> and has legal
/// weight. What will actually be found is here and has none. They disagree
/// deliberately: 14.074 is legal for Morse and is the worst place on the band
/// to send it.</para>
/// <para>Every row carries its source, and a convention nobody could cite is
/// recorded as an unknown rather than filled in from recollection. A
/// neighborhood invented from memory is the prime directive broken in the data
/// layer, where it is hardest to see and where it outlives everybody who could
/// correct it (§0.0, §4).</para>
/// </remarks>
public sealed class NeighborhoodData
{
    /// <summary>Resource name of the embedded neighborhoods file.</summary>
    public const string ResourceName = "Hamlet.RadioEngine.Data.Bands.us-neighborhoods.json";

    private static readonly Lazy<NeighborhoodData> Shared = new(LoadEmbedded);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly IReadOnlyDictionary<string, IReadOnlyList<Neighborhood>> _byBand;

    private NeighborhoodData(
        IReadOnlyDictionary<string, IReadOnlyList<Neighborhood>> byBand,
        IReadOnlyList<NeighborhoodSource> sources,
        IReadOnlyList<NeighborhoodUnknown> unknowns,
        string retrievedUtc)
    {
        _byBand = byBand;
        Sources = sources;
        Unknowns = unknowns;
        RetrievedUtc = retrievedUtc;
    }

    /// <summary>The shipped data.</summary>
    public static NeighborhoodData Current => Shared.Value;

    /// <summary>What this data cites.</summary>
    public IReadOnlyList<NeighborhoodSource> Sources { get; }

    /// <summary>What it deliberately does not cover.</summary>
    public IReadOnlyList<NeighborhoodUnknown> Unknowns { get; }

    /// <summary>When the sources were read, as stated in the file.</summary>
    public string RetrievedUtc { get; }

    /// <summary>Every band the file describes.</summary>
    public IEnumerable<string> Bands => _byBand.Keys;

    /// <summary>The cited rows for a band, lowest first, or an empty list.</summary>
    /// <param name="bandName">The band's display name, e.g. "20 m".</param>
    /// <returns>The rows, which may not cover the whole band.</returns>
    /// <remarks>
    /// Deliberately not gap-free. These are the stretches somebody published a
    /// convention for; the space between them is filled from the band's own
    /// structure by <see cref="NeighborhoodPlan"/> rather than being invented
    /// here.
    /// </remarks>
    public IReadOnlyList<Neighborhood> ForBand(string bandName)
        => _byBand.TryGetValue(bandName, out var hoods)
            ? hoods
            : Array.Empty<Neighborhood>();

    /// <summary>Parse neighborhood data from JSON.</summary>
    /// <param name="json">The file's contents.</param>
    /// <returns>The parsed data.</returns>
    /// <exception cref="InvalidDataException">The file is unusable.</exception>
    public static NeighborhoodData Parse(string json)
    {
        var dto = JsonSerializer.Deserialize<NeighborhoodFile>(json, JsonOptions)
                  ?? throw new InvalidDataException("neighborhood file is empty");

        if (dto.Bands is null || dto.Bands.Length == 0)
        {
            throw new InvalidDataException("neighborhood file describes no bands");
        }

        var byBand = new Dictionary<string, IReadOnlyList<Neighborhood>>(StringComparer.Ordinal);

        foreach (var band in dto.Bands)
        {
            if (string.IsNullOrWhiteSpace(band.Band) || band.Neighborhoods is null)
            {
                continue;
            }

            byBand[band.Band] = band.Neighborhoods
                .Select(Convert)
                .OrderBy(n => n.LowHz)
                .ToList();
        }

        if (byBand.Count == 0)
        {
            throw new InvalidDataException("neighborhood file parsed to nothing usable");
        }

        return new NeighborhoodData(
            byBand,
            (dto.Sources ?? Array.Empty<SourceDto>())
                .Select(s => new NeighborhoodSource(
                    s.Id ?? "", s.Title ?? "", s.Publisher ?? "", s.Url ?? "",
                    s.Authority ?? ""))
                .ToList(),
            (dto.Unknowns ?? Array.Empty<UnknownDto>())
                .Select(u => new NeighborhoodUnknown(u.Topic ?? "", u.Reason ?? ""))
                .ToList(),
            dto.RetrievedUtc ?? "");
    }

    private static Neighborhood Convert(NeighborhoodDto dto)
        => new(
            dto.Name ?? "",
            dto.ShortName ?? "",
            dto.LowHz,
            dto.HighHz,
            dto.Vibe ?? "",
            dto.Blurb ?? "",
            dto.JumpHz == 0 ? dto.LowHz : dto.JumpHz,
            ParseFamily(dto.Family),
            dto.Cite ?? "",
            dto.Caution);

    private static ModeFamily ParseFamily(string? name) => name?.Trim().ToUpperInvariant() switch
    {
        "CW" => ModeFamily.Cw,
        "DIGITAL" => ModeFamily.Digital,
        "PHONE" => ModeFamily.Phone,
        _ => ModeFamily.Open,
    };

    private static NeighborhoodData LoadEmbedded()
    {
        var assembly = typeof(NeighborhoodData).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException(
                $"embedded neighborhood data '{ResourceName}' is missing; "
                + "the build did not include data/bands/us-neighborhoods.json");

        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    private sealed class NeighborhoodFile
    {
        public string? RetrievedUtc { get; set; }

        public SourceDto[]? Sources { get; set; }

        public BandDto[]? Bands { get; set; }

        public UnknownDto[]? Unknowns { get; set; }
    }

    private sealed class BandDto
    {
        public string? Band { get; set; }

        public NeighborhoodDto[]? Neighborhoods { get; set; }
    }

    private sealed class NeighborhoodDto
    {
        public string? Name { get; set; }

        public string? ShortName { get; set; }

        public long LowHz { get; set; }

        public long HighHz { get; set; }

        public long JumpHz { get; set; }

        public string? Family { get; set; }

        public string? Vibe { get; set; }

        public string? Blurb { get; set; }

        public string? Cite { get; set; }

        public string? Caution { get; set; }
    }

    private sealed class SourceDto
    {
        public string? Id { get; set; }

        public string? Title { get; set; }

        public string? Publisher { get; set; }

        public string? Url { get; set; }

        public string? Authority { get; set; }
    }

    private sealed class UnknownDto
    {
        public string? Topic { get; set; }

        public string? Reason { get; set; }
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// A country, with a compass word in front of it only where the entity is long
/// enough north to south for that word to mean something.
/// </summary>
/// <remarks>
/// <para>**COUNTRY ALWAYS, COMPASS WORD RARELY** (Tim's ruling, 2026-09-07:
/// *northern Italy* yes, *northern Belgium* no). Italy runs 725 miles from Sicily
/// to the Alps and the halves are different places; Belgium is barely two grid
/// squares tall and a compass word in front of it says nothing a reader did not
/// already have.</para>
/// <para>**THE JUDGEMENT IS IN A COMMITTED TABLE WITH A REASON BESIDE EACH ROW**,
/// `data/callsigns/entity-extents.json`, and not in this file. *Is this country
/// long enough* is an editorial judgement and the instruction is explicit that it
/// belongs where somebody can argue with it. The table records the entities that
/// take a qualifier **and the ones deliberately refused**, each with why, so a
/// reader can see that Belgium was considered rather than forgotten.</para>
/// <para>**NO PLACE BELOW THE COUNTRY.** No cities, no provinces, no regions. A
/// compass word is a direction within an entity that has already been named from
/// the callsign, and it is the only thing added.</para>
/// <para>**THE COUNTRY NEVER COMES FROM THE GRID.** That is
/// <see cref="DxccPrefixes"/>'s answer, from the callsign, and where the two
/// disagree the callsign wins — `W4/YV7AXM` is in the United States whatever his
/// grid says. **The grid supplies only the latitude**, which is arithmetic on a
/// value the message itself carries, and it is used to place a station inside a
/// country the callsign already settled.</para>
/// </remarks>
public static class EntityQualifier
{
    private const string ResourceName =
        "Hamlet.RadioEngine.Data.Callsigns.entity-extents.json";

    private static readonly Lazy<Table> Shared = new(LoadEmbedded);

    /// <summary>How many entities take a compass qualifier.</summary>
    public static int QualifiedCount => Shared.Value.Qualified.Count;

    /// <summary>How many were considered and deliberately refused one.</summary>
    public static int DeclinedCount => Shared.Value.Declined.Count;

    /// <summary>
    /// The entity, with a compass word in front of it where the table allows one.
    /// </summary>
    /// <param name="entity">The entity name, exactly as `DxccPrefixes` gives it.</param>
    /// <param name="latitude">The station's latitude, from its own grid square.</param>
    /// <returns>e.g. "northern Italy", or "Italy", or "Belgium".</returns>
    /// <remarks>
    /// <para>**THIRDS, AND THE MIDDLE THIRD GETS NO WORD.** Two bands would put a
    /// station one mile south of the midpoint in the *southern half* of a country
    /// it is plainly in the middle of, and a grid square is seventy miles across.
    /// Leaving the middle unqualified is the honest reading of that.</para>
    /// <para>**AND THERE IS NO MIDDLE WORD BY DESIGN.** *Central Italy* is a thing
    /// people say and *central Norway* is not, so a middle band would need its own
    /// judgement per entity for a word that adds nothing. The country on its own
    /// is always true.</para>
    /// <para>**AN ENTITY NOT ON THE TABLE IS RETURNED UNCHANGED**, which is the
    /// instruction's own *anything not on it gets the country alone*. So is an
    /// entity on it whose latitude cannot be read.</para>
    /// </remarks>
    public static string Describe(string? entity, double? latitude)
    {
        var name = (entity ?? "").Trim();

        if (name.Length == 0)
        {
            return "";
        }

        if (latitude is not { } lat
            || !Shared.Value.ByEntity.TryGetValue(name, out var extent))
        {
            return name;
        }

        var span = extent.North - extent.South;

        if (span <= 0)
        {
            return name;
        }

        var third = span / 3.0;

        if (lat <= extent.South + third)
        {
            return "southern " + name;
        }

        if (lat >= extent.North - third)
        {
            return "northern " + name;
        }

        return name;
    }

    /// <summary>The same, taking the station's grid rather than its latitude.</summary>
    /// <param name="entity">The entity name from the callsign.</param>
    /// <param name="grid">The station's Maidenhead locator.</param>
    /// <returns>The entity, qualified where the table allows it.</returns>
    public static string DescribeFromGrid(string? entity, string? grid)
        => Describe(entity, OperatorLocation.FromGrid(grid)?.Latitude);

    /// <summary>Why an entity was refused a qualifier, or null.</summary>
    /// <param name="entity">The entity name.</param>
    /// <returns>The recorded reason, for the report and the About window.</returns>
    public static string? WhyDeclined(string? entity)
        => Shared.Value.Declined
            .FirstOrDefault(d => string.Equals(
                d.Entity, (entity ?? "").Trim(), StringComparison.Ordinal))
            ?.Reason;

    private static Table LoadEmbedded()
    {
        var assembly = typeof(EntityQualifier).Assembly;

        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException(
                $"embedded extent data '{ResourceName}' is missing; the build "
                + "did not include data/callsigns/entity-extents.json");

        var table = JsonSerializer.Deserialize<Table>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException(
                "the embedded extent data could not be read");

        table.ByEntity = table.Qualified.ToDictionary(
            e => e.Entity, e => e, StringComparer.Ordinal);

        return table;
    }

    /// <summary>The shape of the committed table.</summary>
    private sealed class Table
    {
        [JsonPropertyName("qualified")]
        public List<Extent> Qualified { get; set; } = new();

        [JsonPropertyName("declined")]
        public List<Refusal> Declined { get; set; } = new();

        [JsonIgnore]
        public Dictionary<string, Extent> ByEntity { get; set; } = new();
    }

    /// <summary>One entity that takes a compass qualifier.</summary>
    private sealed class Extent
    {
        [JsonPropertyName("entity")]
        public string Entity { get; set; } = "";

        [JsonPropertyName("south")]
        public double South { get; set; }

        [JsonPropertyName("north")]
        public double North { get; set; }
    }

    /// <summary>One entity considered and refused one.</summary>
    private sealed class Refusal
    {
        [JsonPropertyName("entity")]
        public string Entity { get; set; } = "";

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = "";
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>One country inside one of Hamlet's own regions.</summary>
/// <param name="Entity">The DXCC entity name, as the cited transcription spells it.</param>
/// <param name="Continent">Its continent code, from the cited column.</param>
/// <param name="Prefix">
/// The shortest prefix the generated table gives it, which is what a callsign
/// starts with. **Read out of the table and never typed.**
/// </param>
public sealed record RegionMember(string Entity, string Continent, string? Prefix);

/// <summary>One of Hamlet's own groupings inside a continent.</summary>
/// <param name="Name">What it is called: `Central America`.</param>
/// <param name="Continent">The continent it sits inside.</param>
/// <param name="Members">The countries in it.</param>
public sealed record HamletRegion(
    string Name, string Continent, IReadOnlyList<RegionMember> Members);

/// <summary>
/// **Hamlet's own groupings of DXCC entities. Not DXCC's, and the screen says
/// so.**
/// </summary>
/// <remarks>
/// <para>**`ACHIEVEMENTS_PHILOSOPHY.md` §4 LICENSES THESE AND REQUIRES THEM TO BE
/// DECLARED**: *no sub-region is presented as official. DXCC defines continents.
/// Scandinavia, Central America and the Caribbean are Hamlet's own groupings and
/// the screen says so rather than implying a category somebody else recognises.*
/// <see cref="Note"/> is that sentence and every group carrying a region shows
/// it.</para>
/// <para>**WHAT IS EDITORIAL IS SMALL AND WHAT IS CHECKED IS EVERYTHING ELSE.**
/// Which regions exist and who is in each is a judgement, written in
/// `tools/dxcc/hamlet-subregions.txt`. **Every entity name in it was checked
/// against the cited transcription and every prefix was read out of the generated
/// prefix table**, so a region cannot name a country that does not exist and a
/// nudge cannot name a prefix nobody holds.</para>
/// <para>**A REGION THAT SPANS TWO CONTINENTS IS REFUSED RATHER THAN
/// MISPLACED.** The screen draws a region inside a continent, and the
/// Mediterranean's members are on two - Cyprus is Asia in the ARRL's own column -
/// so it is in the data file's `refused` list with its reason rather than filed
/// under one of them. <see cref="Refused"/> reads it back, so the omission is
/// visible rather than silent.</para>
/// </remarks>
public static class HamletRegions
{
    private const string ResourceName =
        "Hamlet.RadioEngine.Data.Callsigns.hamlet-subregions.json";

    private static readonly Lazy<Table> Shared = new(LoadEmbedded);

    /// <summary>
    /// The sentence a screen must show beside any of these (§4).
    /// </summary>
    public const string Note =
        "Hamlet's own grouping, not an official DXCC category";

    /// <summary>Every region, in the order the editorial file lists them.</summary>
    public static IReadOnlyList<HamletRegion> All
        => Shared.Value.Regions
            .Select(r => new HamletRegion(
                r.Name,
                r.Continent ?? "",
                r.Members
                    .Select(m => new RegionMember(m.Entity, m.Continent, m.Prefix))
                    .ToList()))
            .ToList();

    /// <summary>What was left out, and why.</summary>
    /// <remarks>
    /// **AN OMISSION THAT IS VISIBLE IS NOT A GAP** (§12.4). A region or a member
    /// the generator refused is here with its reason, so a later unit reads why
    /// rather than rediscovering it.
    /// </remarks>
    public static IReadOnlyList<string> Refused
        => Shared.Value.RefusedRows
            .Select(r => r.Region + " - " + r.Entity + ": " + r.Why)
            .ToList();

    /// <summary>The regions inside one continent.</summary>
    /// <param name="code">The continent code.</param>
    /// <returns>Its regions, which may be none.</returns>
    public static IReadOnlyList<HamletRegion> On(string? code)
    {
        var key = (code ?? "").Trim();

        return All.Where(
            r => string.Equals(r.Continent, key, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>The region one entity belongs to, or null.</summary>
    /// <param name="entity">The DXCC entity name.</param>
    /// <returns>The region, or null where no grouping holds it.</returns>
    /// <remarks>
    /// **MOST ENTITIES ARE IN NO REGION AND THAT IS ORDINARY.** These four are a
    /// handful, not a partition of the world (§3.3), so null is the common answer
    /// and never a gap.
    /// </remarks>
    public static HamletRegion? For(string? entity)
    {
        var name = (entity ?? "").Trim();

        return name.Length == 0
            ? null
            : All.FirstOrDefault(
                r => r.Members.Any(
                    m => string.Equals(
                        m.Entity, name, StringComparison.OrdinalIgnoreCase)));
    }

    private static Table LoadEmbedded()
    {
        var assembly = typeof(HamletRegions).Assembly;

        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException(
                $"embedded region data '{ResourceName}' is missing; the build did "
                + "not include data/callsigns/hamlet-subregions.json");

        var table = JsonSerializer.Deserialize<Table>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return table ?? throw new InvalidDataException(
            "the embedded region data could not be read");
    }

    /// <summary>The shape of the generated data file.</summary>
    private sealed class Table
    {
        [JsonPropertyName("regions")]
        public List<RegionRow> Regions { get; set; } = new();

        [JsonPropertyName("refused")]
        public List<RefusedRow> RefusedRows { get; set; } = new();
    }

    private sealed class RegionRow
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("continent")]
        public string? Continent { get; set; }

        [JsonPropertyName("members")]
        public List<MemberRow> Members { get; set; } = new();
    }

    private sealed class MemberRow
    {
        [JsonPropertyName("entity")]
        public string Entity { get; set; } = "";

        [JsonPropertyName("continent")]
        public string Continent { get; set; } = "";

        [JsonPropertyName("prefix")]
        public string? Prefix { get; set; }
    }

    private sealed class RefusedRow
    {
        [JsonPropertyName("region")]
        public string Region { get; set; } = "";

        [JsonPropertyName("entity")]
        public string Entity { get; set; } = "";

        [JsonPropertyName("why")]
        public string Why { get; set; } = "";
    }
}

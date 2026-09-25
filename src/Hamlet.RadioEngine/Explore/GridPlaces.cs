using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>Where a grid square is, where Hamlet's table can say.</summary>
/// <param name="Grid">The four-character square that was placed.</param>
/// <param name="Place">What the card says: `California`, `the lower 48`, `Alaska`.</param>
/// <param name="Entity">The DXCC entity, exactly as `DxccPrefixes` spells it.</param>
public sealed record GridPlace(string Grid, string Place, string Entity);

/// <summary>
/// **A station whose grid says one entity and whose callsign says another.**
/// </summary>
/// <param name="Callsign">The station.</param>
/// <param name="CallEntity">The entity his prefix implies.</param>
/// <param name="Where">Where his grid says he is.</param>
/// <remarks>
/// **THE GRID WINS** (HM-DEC-180, Tim 2026-09-23). A prefix says where a callsign was
/// issued; a grid he sent says where he is.
/// </remarks>
public sealed record GridOverPrefix(string Callsign, string CallEntity, GridPlace Where);

/// <summary>
/// **Which entity a grid square is in, for the squares a committed table places whole.**
/// </summary>
/// <remarks>
/// <para>**HM-DEC-180 OVERRULES <see cref="EntityQualifier"/>'S *THE CALLSIGN WINS*** where
/// the two disagree. Before it nothing in the tree could say which entity a grid is in, so the
/// callsign answered every question about place; this is the smallest thing that can answer
/// the one the ruling asks.</para>
/// <para>**THE TABLE IS `data/callsigns/grid-places.json` AND THE JUDGEMENT IS THERE**, with a
/// reason beside every box. A square is placed only where the whole square lies inside one
/// box, so a square on a border - the case a grid cannot settle - is never placed.</para>
/// <para>**A SQUARE THE TABLE DOES NOT PLACE CHANGES NOTHING** (§0.0). The prefix's entity
/// stands exactly as before; nothing here guesses.</para>
/// </remarks>
public static class GridPlaces
{
    private const string ResourceName =
        "Hamlet.RadioEngine.Data.Callsigns.grid-places.json";

    private static readonly Lazy<IReadOnlyList<Box>> Shared = new(LoadEmbedded);

    /// <summary>How many boxes the table holds.</summary>
    public static int BoxCount => Shared.Value.Count;

    /// <summary>Where a grid square is, or null where the table cannot say.</summary>
    /// <param name="grid">A Maidenhead locator of four or more characters.</param>
    /// <returns>The place, or null for a square the table does not hold whole.</returns>
    public static GridPlace? Of(string? grid)
    {
        if (Square(grid) is not { } square)
        {
            return null;
        }

        var (west, south, name) = square;

        foreach (var box in Shared.Value)
        {
            if (west >= box.West && west + 2 <= box.East
                && south >= box.South && south + 1 <= box.North)
            {
                return new GridPlace(name, box.Place, box.Entity);
            }
        }

        return null;
    }

    /// <summary>
    /// The contradiction between a station's prefix and his grid, or null where there is none.
    /// </summary>
    /// <param name="callsign">The station.</param>
    /// <param name="grid">The grid he sent, or null.</param>
    /// <returns>Both sides, or null where they agree or either is unknown.</returns>
    public static GridOverPrefix? Contradiction(string? callsign, string? grid)
    {
        if (DxccPrefixes.EntityOf(callsign) is not { Length: > 0 } callEntity
            || Of(grid) is not { } where
            || string.Equals(callEntity, where.Entity, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new GridOverPrefix(callsign!.Trim().ToUpperInvariant(), callEntity, where);
    }

    /// <summary>The square's south-west corner and its name, or null where it will not read.</summary>
    private static (int West, int South, string Name)? Square(string? grid)
    {
        var text = (grid ?? "").Trim().ToUpperInvariant();

        if (text.Length < 4
            || text[0] is < 'A' or > 'R' || text[1] is < 'A' or > 'R'
            || !char.IsAsciiDigit(text[2]) || !char.IsAsciiDigit(text[3]))
        {
            return null;
        }

        var west = (text[0] - 'A') * 20 - 180 + (text[2] - '0') * 2;
        var south = (text[1] - 'A') * 10 - 90 + (text[3] - '0');

        return (west, south, text[..4]);
    }

    private static IReadOnlyList<Box> LoadEmbedded()
    {
        var assembly = typeof(GridPlaces).Assembly;

        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException(
                $"embedded place data '{ResourceName}' is missing; the build "
                + "did not include data/callsigns/grid-places.json");

        var table = JsonSerializer.Deserialize<Table>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException(
                "the embedded place data could not be read");

        return table.Places;
    }

    /// <summary>The shape of the committed table.</summary>
    private sealed class Table
    {
        [JsonPropertyName("places")]
        public List<Box> Places { get; set; } = new();
    }

    /// <summary>One box whose land lies wholly in one entity.</summary>
    private sealed class Box
    {
        [JsonPropertyName("place")]
        public string Place { get; set; } = "";

        [JsonPropertyName("entity")]
        public string Entity { get; set; } = "";

        [JsonPropertyName("south")]
        public double South { get; set; }

        [JsonPropertyName("north")]
        public double North { get; set; }

        [JsonPropertyName("west")]
        public double West { get; set; }

        [JsonPropertyName("east")]
        public double East { get; set; }
    }
}

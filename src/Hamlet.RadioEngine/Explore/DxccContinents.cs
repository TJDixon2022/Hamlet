using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// **Which continent a DXCC entity is on, from the ARRL's own column.**
/// </summary>
/// <remarks>
/// <para>**IT IS CITED DATA AND NOT A LOOKUP TABLE SOMEBODY TYPED** (§0, and work
/// instruction 252 task 3's rule for the prefixes one file over). The continent
/// comes from the Continent column of the ARRL DXCC List, transcribed into
/// `tools/dxcc/arrl-dxcc-continents.txt` and generated into
/// `data/callsigns/dxcc-continents.json` by `tools/dxcc/transcribe-continents.py`.
/// **Nothing here was written from memory**, and a card reading *Europe, 4 worked*
/// is a claim nobody would ever check, which is precisely why it may not be.</para>
/// <para>**EVERY ROW WAS CROSS-CHECKED AGAINST THE PREFIX TABLE.** The generator
/// joins each row to `arrl-dxcc-current.txt`, which unit 252 transcribed from the
/// same publication, and drops anything it cannot join. The counts are in the file's
/// own `crossCheck` block and <see cref="CrossCheckLine"/> puts them on screen.</para>
/// <para>**IT DECLINES RATHER THAN CHOOSING.** An entity the publication gives two
/// continents - Maldives is `AS,AF` and Republic of Turkiye is `EU,AS` - is not in
/// the table at all, so <see cref="Of"/> answers null for it. That is the same rule
/// <see cref="DxccPrefixes"/> applies to a prefix two entities claim: say nothing
/// where we do not know.</para>
/// <para>**A CONTINENT IS DXCC'S OWN CATEGORY AND A SUB-REGION IS NOT.** Anything
/// finer than these seven codes is Hamlet's own grouping and has to say so
/// (`ACHIEVEMENTS_PHILOSOPHY.md` §4). Nothing in this file invents one.</para>
/// </remarks>
public static class DxccContinents
{
    private const string ResourceName =
        "Hamlet.RadioEngine.Data.Callsigns.dxcc-continents.json";

    private static readonly Lazy<Table> Shared = new(LoadEmbedded);

    /// <summary>How many entities carry a cited continent.</summary>
    public static int EntityCount => Shared.Value.Continents.Count;

    /// <summary>The seven codes, and what each one is called.</summary>
    /// <remarks>
    /// **THE CODES ARE THE PUBLICATION'S** - `AF`, `AN`, `AS`, `EU`, `NA`, `OC`,
    /// `SA` - and the names beside them are the ordinary English for each. A screen
    /// shows the name; a comparison uses the code.
    /// </remarks>
    public static IReadOnlyDictionary<string, string> Codes => Shared.Value.Codes;

    /// <summary>Where the table came from, for the About window and the record.</summary>
    public static string SourceLine
        => Shared.Value.Source.Name + ", " + Shared.Value.Source.DocumentDate
           + ", retrieved " + Shared.Value.Source.Retrieved;

    /// <summary>What the cross-check found, in one line.</summary>
    /// <remarks>
    /// **A DERIVED TABLE SAYS HOW IT WAS DERIVED** (§0.0.1). The numbers are the
    /// generator's own and are written into the file at generation time, so nothing
    /// here counts anything a second way.
    /// </remarks>
    public static string CrossCheckLine
    {
        get
        {
            var check = Shared.Value.CrossCheck;

            return $"{check.JoinedByPrefixAndName + check.JoinedByUniquePrefixAlone}"
                + $" of {check.RowsInTheCommittedTranscription} rows joined to the "
                + "prefix table, "
                + $"{check.DeclinedForTwoContinents} declined for carrying two "
                + $"continents, {check.NotJoined} not joined.";
        }
    }

    /// <summary>The continent code for an entity, or null.</summary>
    /// <param name="entity">
    /// The entity name exactly as <see cref="DxccPrefixes.EntityOf"/> returns it.
    /// </param>
    /// <returns>`EU`, `NA` and so on, or null where the table declines.</returns>
    /// <remarks>
    /// **NULL IS A REAL ANSWER AND IS NOT A GAP.** An entity the publication gives
    /// two continents, or one the cross-check could not join, comes back null and
    /// the caller says nothing about where it is rather than guessing.
    /// </remarks>
    public static string? Of(string? entity)
    {
        var name = (entity ?? "").Trim();

        return name.Length > 0
               && Shared.Value.Continents.TryGetValue(name, out var code)
            ? code
            : null;
    }

    /// <summary>The continent code for a callsign, or null.</summary>
    /// <param name="callsign">The station's callsign.</param>
    /// <returns>The code, or null where the entity or the continent is unknown.</returns>
    /// <remarks>
    /// **THE CALLSIGN NAMES THE ENTITY AND THE ENTITY NAMES THE CONTINENT**, in that
    /// order and through <see cref="DxccPrefixes.EntityOf"/>, so a shared prefix is
    /// silent here for the reason it is silent there.
    /// </remarks>
    public static string? ForCallsign(string? callsign)
        => Of(DxccPrefixes.EntityOf(callsign));

    /// <summary>What a continent code is called, or the code itself.</summary>
    /// <param name="code">One of the seven.</param>
    /// <returns>`Europe`, `North America`, and so on.</returns>
    public static string NameOf(string? code)
    {
        var key = (code ?? "").Trim().ToUpperInvariant();

        return Shared.Value.Codes.TryGetValue(key, out var name) ? name : key;
    }

    /// <summary>How many entities the table holds on one continent.</summary>
    /// <param name="code">One of the seven.</param>
    /// <returns>The count, which is the denominator a card shows.</returns>
    /// <remarks>
    /// **IT COUNTS WHAT THE TABLE HOLDS AND NOT WHAT DXCC HAS.** The publication
    /// states 340 current entities and this table carries 303 of them, so a card
    /// reading *4 of 63* is against what Hamlet can recognise. **That is a smaller
    /// claim than the truth and it is the honest direction to be wrong in**: every
    /// entity in the denominator is one a callsign here can actually resolve to.
    /// </remarks>
    public static int EntitiesOn(string? code)
    {
        var key = (code ?? "").Trim().ToUpperInvariant();

        return Shared.Value.Continents.Values.Count(
            c => string.Equals(c, key, StringComparison.OrdinalIgnoreCase));
    }

    private static Table LoadEmbedded()
    {
        var assembly = typeof(DxccContinents).Assembly;

        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException(
                $"embedded continent data '{ResourceName}' is missing; the build "
                + "did not include data/callsigns/dxcc-continents.json");

        var table = JsonSerializer.Deserialize<Table>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return table ?? throw new InvalidDataException(
            "the embedded continent data could not be read");
    }

    /// <summary>The shape of the generated data file.</summary>
    private sealed class Table
    {
        [JsonPropertyName("source")]
        public SourceNote Source { get; set; } = new();

        [JsonPropertyName("crossCheck")]
        public CrossCheckNote CrossCheck { get; set; } = new();

        [JsonPropertyName("codes")]
        public Dictionary<string, string> Codes { get; set; } = new();

        [JsonPropertyName("continents")]
        public Dictionary<string, string> Continents { get; set; }
            = new(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Where the rows came from.</summary>
    private sealed class SourceNote
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("documentDate")]
        public string DocumentDate { get; set; } = "";

        [JsonPropertyName("retrieved")]
        public string Retrieved { get; set; } = "";
    }

    /// <summary>What the generator's cross-check found.</summary>
    private sealed class CrossCheckNote
    {
        [JsonPropertyName("rowsInTheCommittedTranscription")]
        public int RowsInTheCommittedTranscription { get; set; }

        [JsonPropertyName("joinedByPrefixAndName")]
        public int JoinedByPrefixAndName { get; set; }

        [JsonPropertyName("joinedByUniquePrefixAlone")]
        public int JoinedByUniquePrefixAlone { get; set; }

        [JsonPropertyName("notJoined")]
        public int NotJoined { get; set; }

        [JsonPropertyName("declinedForTwoContinents")]
        public int DeclinedForTwoContinents { get; set; }
    }
}

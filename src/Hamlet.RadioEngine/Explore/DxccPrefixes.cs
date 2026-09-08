using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// Which entity a callsign belongs to, where that is certain, and nothing at
/// all where it is not.
/// </summary>
/// <remarks>
/// <para>**CERTAIN, OR SILENT** (Tim's ruling, 2026-09-05: *say nothing if we
/// don't know*). Not a hedge, not *probably*, not *unknown*. A callsign this
/// cannot resolve gets no country, in the same way an unrecognised payload gets
/// no tooltip. **This is the surface §0.0 is most exposed to in this unit**: a
/// confident wrong country sends the operator after an entity he already has, or
/// past one he needs, and he has no way to tell it is wrong.</para>
/// <para>**THE TABLE IS PUBLISHED, CITED AND GENERATED, NOT TYPED.** It comes
/// from the ARRL DXCC List of current entities, January 2026, transcribed to
/// `tools/dxcc/arrl-dxcc-current.txt` and expanded to
/// `data/callsigns/dxcc-prefixes.json` by `tools/dxcc/expand-prefixes.py`. The
/// entity names are the ARRL's own words. A prefix table written from memory is
/// the pattern this project has spent a fortnight learning not to trust, and it
/// is the one thing the instruction forbids twice.</para>
/// <para>**A PREFIX TWO ENTITIES SHARE RESOLVES TO NOTHING.** Fourteen do —
/// `3D2` is Fiji, Conway Reef and Rotuma; `VK9` is five different islands; `CE0`
/// is three. They are in the data file under their own heading so the silence is
/// visible as a decision rather than as an omission.</para>
/// <para>**WHY DXCC AND NOT THE ITU TABLE.** ITU Appendix 42 was obtained first
/// and is the other published allocation. It answers a different question: it
/// allocates series to *countries*, so it has `I = Italy` and cannot produce
/// Sardinia, which the ruling's own example requires. DXCC is what an operator
/// means by *country* and it is one source rather than two. **Two authorities
/// merged is how a table acquires a disagreement nobody can see** (§0), so the
/// ITU table is cited here and deliberately not used.</para>
/// <para>**WHAT THIS IS NOT.** It is not a location. HM-DEC-038 forbids deriving
/// a distance or a position from a callsign prefix and that is untouched: an
/// entity is a fact about the licence, and where the station is standing is not
/// knowable from it. Distance comes from a grid square or from nowhere.</para>
/// </remarks>
public static class DxccPrefixes
{
    private const string ResourceName =
        "Hamlet.RadioEngine.Data.Callsigns.dxcc-prefixes.json";

    private static readonly Lazy<Table> Shared = new(LoadEmbedded);

    /// <summary>How many prefixes resolve to exactly one entity.</summary>
    public static int CertainPrefixCount => Shared.Value.Prefixes.Count;

    /// <summary>How many prefixes are shared and therefore silent.</summary>
    public static int SharedPrefixCount => Shared.Value.Shared.Count;

    /// <summary>Every entity name in the cited list, in the ARRL's own words.</summary>
    /// <remarks>
    /// **SO A NAME WRITTEN DOWN SOMEWHERE ELSE CAN BE CHECKED AGAINST IT** (work
    /// instruction 281 task 5). <see cref="EntitySpoken"/> keys a short spoken form
    /// by the ARRL's name; a key that stops matching one, because the list was
    /// re-transcribed or a name changed, would silently do nothing at all. This is
    /// what lets a test say so instead.
    /// </remarks>
    public static IReadOnlySet<string> Entities
        => Shared.Value.Prefixes.Values.ToHashSet(StringComparer.Ordinal);

    /// <summary>Where the table came from, for the About window and the record.</summary>
    public static string SourceLine
        => $"{Shared.Value.Source.Name}, {Shared.Value.Source.DocumentDate}, "
           + $"retrieved {Shared.Value.Source.Retrieved}";

    /// <summary>
    /// The entity a callsign belongs to, or null where that is not certain.
    /// </summary>
    /// <param name="callsign">A callsign, compound or plain.</param>
    /// <returns>The entity's name in the ARRL's own words, or null.</returns>
    /// <remarks>
    /// <para>**THE PREFIX BEFORE THE SLASH WINS, WHEN THERE IS ONE** (the
    /// instruction's own rule, and it is the case that makes this dangerous).
    /// `W4/YV7AXM` is a Venezuelan operator transmitting from Florida and
    /// `IS0/IK2YCW` is an Italian operator on Sardinia. Reading the home call
    /// instead would name Venezuela and Italy: right about the licence, wrong
    /// about the contact, and wrong in the direction that costs him a new
    /// one.</para>
    /// <para>**A SUFFIX IS NOT A PLACE AND IS IGNORED.** `/P`, `/M`, `/QRP` and
    /// the rest say how somebody is operating rather than where, so the base call
    /// answers. They are recognised by being short and not resolving as a prefix
    /// themselves, which is the same shape `DecodedFilterRule.IsSameStation`
    /// uses one project over.</para>
    /// <para>**`/MM` AND `/AM` RESOLVE TO NOTHING, DELIBERATELY.** Maritime and
    /// aeronautical mobile means the station is at sea or in the air, and no
    /// entity is the right answer. Naming the one on the licence would be the
    /// confident wrong country in its purest form.</para>
    /// <para>**THE LONGEST MATCH WINS.** `IS0` beats `I`, `EA6` beats `EA`, `KL`
    /// beats `K`. Without that the specific entities are unreachable and every
    /// Sardinian station reads as Italy.</para>
    /// </remarks>
    public static string? EntityOf(string? callsign)
    {
        var call = (callsign ?? "").Trim().ToUpperInvariant();

        if (call.Length == 0)
        {
            return null;
        }

        var pieces = call.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (pieces.Length == 0)
        {
            return null;
        }

        if (pieces.Length > 1)
        {
            // At sea or in the air: the licence says nothing about where.
            foreach (var piece in pieces)
            {
                if (piece is "MM" or "AM")
                {
                    return null;
                }
            }

            // A leading piece that resolves on its own is where they are
            // operating from, and it overrides the home call.
            var lead = LongestMatch(pieces[0]);

            if (lead is not null)
            {
                return lead;
            }
        }

        // Otherwise the base call answers, which is the longest piece: a prefix
        // or a suffix is short and the callsign is the long one.
        var baseCall = pieces[0];

        foreach (var piece in pieces)
        {
            if (piece.Length > baseCall.Length)
            {
                baseCall = piece;
            }
        }

        return LongestMatch(baseCall);
    }

    /// <summary>Whether a prefix is one two or more entities share.</summary>
    /// <param name="prefix">The prefix, upper case.</param>
    /// <returns>True when the table declines it on purpose.</returns>
    public static bool IsSharedPrefix(string? prefix)
        => Shared.Value.Shared.ContainsKey((prefix ?? "").Trim().ToUpperInvariant());

    /// <summary>The longest prefix of this call that the table is certain about.</summary>
    /// <remarks>
    /// **A SHARED PREFIX STOPS THE SEARCH RATHER THAN BEING SKIPPED PAST.** `VK9`
    /// is five islands; falling back to a shorter match would answer *Australia*
    /// for a station the table has just said it cannot place, which is a guess
    /// arrived at by persistence.
    /// </remarks>
    private static string? LongestMatch(string call)
    {
        var table = Shared.Value;

        for (var length = Math.Min(call.Length, 6); length >= 1; length--)
        {
            var candidate = call[..length];

            if (table.Shared.ContainsKey(candidate))
            {
                return null;
            }

            if (table.Prefixes.TryGetValue(candidate, out var entity))
            {
                return entity;
            }
        }

        return null;
    }

    private static Table LoadEmbedded()
    {
        var assembly = typeof(DxccPrefixes).Assembly;

        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidDataException(
                $"embedded prefix data '{ResourceName}' is missing; the build "
                + "did not include data/callsigns/dxcc-prefixes.json");

        var table = JsonSerializer.Deserialize<Table>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return table ?? throw new InvalidDataException(
            "the embedded prefix data could not be read");
    }

    /// <summary>The shape of the generated data file.</summary>
    private sealed class Table
    {
        [JsonPropertyName("source")]
        public SourceNote Source { get; set; } = new();

        [JsonPropertyName("prefixes")]
        public Dictionary<string, string> Prefixes { get; set; } = new();

        [JsonPropertyName("sharedAndThereforeSilent")]
        public Dictionary<string, List<string>> Shared { get; set; } = new();
    }

    /// <summary>Where the table came from.</summary>
    private sealed class SourceNote
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("documentDate")]
        public string DocumentDate { get; set; } = "";

        [JsonPropertyName("retrieved")]
        public string Retrieved { get; set; } = "";
    }
}

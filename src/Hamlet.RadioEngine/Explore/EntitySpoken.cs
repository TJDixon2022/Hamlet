using System.Collections.Generic;

namespace Hamlet.RadioEngine.Explore;

/// <summary>
/// An entity's name as a person would say it in a sentence.
/// </summary>
/// <remarks>
/// <para>**OBSERVED 2026-09-08**, on the operator's own CQ: *KC3QIS is calling
/// anyone. He is in United States of America, in grid FN00.* The name is the ARRL's
/// and it is correct; it reads as officialese where every other entity in that
/// sentence reads short, and §0.7 asks for the ordinary word wherever it differs
/// from the correct one.</para>
/// <para>**THE CITED FILE IS NOT TOUCHED** (§6.1). `data/callsigns/dxcc-prefixes.json`
/// quotes the ARRL DXCC List and a quotation that has been tidied is no longer a
/// quotation, so this is a spoken form laid over it and used only where a name goes
/// into prose. Anywhere a name is a label or a citation it stays as the ARRL wrote
/// it.</para>
/// <para>**FOUR, NOT THIRTEEN.** Thirteen entity names run past twenty characters
/// and most of them are simply their names — *Agalega &amp; St. Brandon Is.* is not
/// long-windedness, it is what the place is called. These four are the ones where a
/// short form is what everybody says and there is no second thing it could mean.</para>
/// <para>**AND THE TWO CONGOS ARE DELIBERATELY LEFT ALONE.** *Republic of the Congo*
/// and *Democratic Republic of the Congo* are two different entities, and every
/// short form that fits one of them reads as the other to somebody. A name that is
/// long is a small cost; a name that names the wrong country is §0.0 broken.</para>
/// </remarks>
public static class EntitySpoken
{
    /// <summary>The ARRL's name, and the way it is said aloud.</summary>
    /// <remarks>
    /// **THE KEYS ARE CHECKED AGAINST THE CITED FILE BY A TEST**, so a key that stops
    /// matching an ARRL name — because the list was re-transcribed, or a name changed
    /// — fails rather than silently doing nothing.
    /// </remarks>
    public static IReadOnlyDictionary<string, string> Shortened { get; } =
        new Dictionary<string, string>
        {
            ["United States of America"] = "the United States",
            ["Democratic Peoples Rep of Korea"] = "North Korea",
            ["Lao Peoples Democratic Rep"] = "Laos",
            ["Tanzania (United Republic of)"] = "Tanzania",
        };

    /// <summary>The name to put in a sentence.</summary>
    /// <param name="entity">The entity, exactly as `DxccPrefixes` gives it.</param>
    /// <returns>The spoken form, or the name unchanged.</returns>
    public static string Of(string? entity)
    {
        var name = (entity ?? "").Trim();

        return name.Length > 0 && Shortened.TryGetValue(name, out var spoken)
            ? spoken
            : name;
    }
}

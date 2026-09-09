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

    /// <summary>
    /// The longest a name may be before a card looks for a shorter one.
    /// </summary>
    /// <remarks>
    /// **MEASURED RATHER THAN CHOSEN** (work instruction 299 task 4). Over the 275
    /// entities the cited table holds, the median name is **8 characters** and the
    /// longest is **33** - `Sovereign Military Order of Malta`. **31 names run over
    /// 16 and 13 run over 20.** Sixteen is where the tail starts: under it are the
    /// ordinary country names a card carries beside a callsign and a distance, and
    /// over it are the constructions nobody says out loud.
    /// </remarks>
    public const int CardLimit = 16;

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

    /// <summary>
    /// **The shortest honest name for a place, for a card that has a callsign and a
    /// distance beside it.**
    /// </summary>
    /// <param name="entity">The entity, exactly as `DxccPrefixes` gives it.</param>
    /// <returns>A short form, or the spoken form where it is already short.</returns>
    /// <remarks>
    /// <para>**TIM RAISED IT ON 2026-09-09**: `the United States` where the card
    /// wants a place. The same fault had been raised against the tooltips and never
    /// fixed.</para>
    /// <para>**IT SHORTENS AND IT NEVER NARROWS.** Nothing here names a place
    /// *below* the country - not a state, not a city - because the callsign gives
    /// neither and a four-character grid square is a box seventy miles across that
    /// straddles state lines. **`the United States` becomes `United States` and never
    /// `Arizona`**; unit 297 and unit 298 both found the same wall and the state
    /// waits on the parked callook instruction rather than on a shorter string.</para>
    /// <para>**THE RULES ARE MECHANICAL AND THE EXCEPTIONS ARE DECLARED.** A leading
    /// article goes, a trailing parenthetical goes, and `Is.` stays as the ARRL
    /// abbreviates it. What those cannot reach is in <see cref="OnACard"/>'s own
    /// table above, where a test checks every key against the cited file - so a name
    /// that stops existing fails rather than silently doing nothing.</para>
    /// </remarks>
    public static string Short(string? entity)
    {
        var name = Of(entity);

        if (name.Length == 0)
        {
            return "";
        }

        if (OnACard.TryGetValue(name, out var shortened))
        {
            return shortened;
        }

        // **A LEADING ARTICLE IS PROSE AND A CARD IS NOT PROSE.** `the United
        // States` reads correctly in a sentence and wastes four characters in a
        // header beside a callsign.
        if (name.StartsWith("the ", StringComparison.Ordinal))
        {
            name = name[4..];
        }

        // **A PARENTHETICAL IS THE ARRL DISAMBIGUATING, NOT PART OF THE NAME.**
        var bracket = name.IndexOf(" (", StringComparison.Ordinal);

        if (bracket > 0)
        {
            name = name[..bracket];
        }

        return name;
    }

    /// <summary>What a card calls the places whose names the rules cannot reach.</summary>
    /// <remarks>
    /// <para>**EVERY KEY IS A NAME THE CITED TABLE ACTUALLY HOLDS**, checked by a
    /// test, and every value is the ordinary English for the same place. **Nothing
    /// here renames a country into something it is not** and nothing narrows one to a
    /// region inside it.</para>
    /// <para>**THE LIST IS SHORT BECAUSE THE MEASUREMENT SAID IT COULD BE.** Thirteen
    /// names run over twenty characters and these are the ones a card would otherwise
    /// carry unreadably.</para>
    /// </remarks>
    public static IReadOnlyDictionary<string, string> OnACard { get; } =
        new Dictionary<string, string>
        {
            ["the United States"] = "United States",
            ["Sovereign Military Order of Malta"] = "Order of Malta",
            ["New Zealand Subantarctic Islands"] = "NZ Subantarctic",
            ["Democratic Republic of the Congo"] = "DR Congo",
            ["Republic of the Congo"] = "Congo",
            ["Tristan da Cunha & Gough I."] = "Tristan da Cunha",
            ["Prince Edward & Marion Is."] = "Prince Edward Is.",
            ["Agalega & St. Brandon Is."] = "Agalega Is.",
            ["St. Pierre & Miquelon"] = "St. Pierre",
            ["Andaman & Nicobar Is."] = "Andaman Is.",
            ["United Arab Emirates"] = "UAE",
            ["Syrian Arab Republic"] = "Syria",
            ["Palmyra & Jarvis Is."] = "Palmyra I.",
            ["Baker & Howland Is."] = "Baker I.",
            ["Sao Tome & Principe"] = "Sao Tome",
        };
}

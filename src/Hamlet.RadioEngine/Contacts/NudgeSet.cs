using System;
using System.Collections.Generic;
using System.Linq;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>What kind of thing working a station would open.</summary>
public enum NudgeKind
{
    /// <summary>Nothing. Working him opens no card that is not already in play.</summary>
    None = 0,

    /// <summary>A country he has not worked, on a continent he has.</summary>
    /// <remarks>
    /// **VISIBLE, IN THE PHILOSOPHY'S SENSE** (§3.1). The adjacency is already
    /// satisfied - the continent is open - so the card for that country is one the
    /// achievements screen would show, and it may be named on hover.
    /// </remarks>
    Visible = 1,

    /// <summary>A continent he has never worked at all.</summary>
    /// <remarks>
    /// **A DOOR, AND IT IS MARKED BUT NOT NAMED** (Tim, 2026-09-10). §3.1 is
    /// *absent, not dimmed*: **the CQ list must not be the thing that tells him
    /// Eastern Europe exists.** The mark says something new would open; it does not
    /// say what.
    /// </remarks>
    Door = 2,
}

/// <summary>
/// **What is still in play, read once from the log.**
/// </summary>
/// <remarks>
/// <para>**THE UNEARNED CARDS ARE THE SOURCE AND THERE IS NO RARITY ORDERING**
/// (Tim, 2026-09-10). The achievement set already knows what is in play; the mark is
/// the intersection of *what is unearned* with *tonight's callers*. **No difficulty
/// number is computed, stored or persisted anywhere**, because no cited source for
/// continent-versus-state difficulty exists and anything finer would be invented and
/// dressed as data.</para>
/// <para>**THE UNEARNED SET IS DERIVED RATHER THAN ENUMERATED, AND THAT IS A
/// FINDING.** The achievements screen builds its place cards from
/// `AchievementLog.Continents` - the continents he **has** worked - so an unearned
/// country card does not exist as an object anywhere to be listed. What does exist is
/// everything needed to derive one: the cited entity table knows every entity and its
/// continent, and the log knows which he has worked.</para>
/// <para>**READ ONCE AND CACHED** (unit 278's shape). Fourteen messages a slot and
/// four slots a minute; this is built from the log once and asked about a callsign
/// thereafter.</para>
/// <para>**IT COUNTS *WORKED*, NEVER *CONFIRMED*** (§4). Hamlet has contacts and no
/// confirmations.</para>
/// </remarks>
public sealed class NudgeSet
{
    private readonly HashSet<string> _workedEntities;
    private readonly HashSet<string> _workedContinents;

    /// <summary>Build from what has been worked.</summary>
    /// <param name="entities">Every entity in the log.</param>
    /// <param name="continents">Every continent in the log.</param>
    /// <remarks>
    /// **THE TWO LISTS RATHER THAN THE LOG ITSELF**, so this type has no opinion
    /// about where a log comes from and a test can state a position in one line.
    /// <see cref="From"/> is the production route.
    /// </remarks>
    public NudgeSet(
        IEnumerable<string?>? entities, IEnumerable<string?>? continents)
    {
        _workedEntities = new HashSet<string>(
            (entities ?? Array.Empty<string?>())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e!.Trim()),
            StringComparer.OrdinalIgnoreCase);

        _workedContinents = new HashSet<string>(
            (continents ?? Array.Empty<string?>())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c!.Trim()),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Build from what the log holds.</summary>
    /// <param name="log">The contact log.</param>
    /// <returns>What is still in play.</returns>
    /// <exception cref="ArgumentNullException">There is no log.</exception>
    public static NudgeSet From(AchievementLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

        return new NudgeSet(log.Entities, log.Continents);
    }

    /// <summary>How many entities the log already holds.</summary>
    public int WorkedEntities => _workedEntities.Count;

    /// <summary>How many continents the log already holds.</summary>
    public int WorkedContinents => _workedContinents.Count;

    /// <summary>What working this station would open, if anything.</summary>
    /// <param name="callsign">Whoever is calling.</param>
    /// <returns>The kind, and the entity where it may be named.</returns>
    /// <remarks>
    /// <para>**SILENT WHERE THE TABLE DECLINES** (unit 252). A callsign the entity
    /// table cannot resolve opens nothing as far as Hamlet knows, and guessing an
    /// entity from a prefix it does not carry would be §0.0's fault with a flag on
    /// it.</para>
    /// <para>**THE ENTITY IS RETURNED ONLY FOR A VISIBLE CARD.** For a door it is
    /// deliberately left empty, so that nothing downstream can name an area he has
    /// never opened even by accident.</para>
    /// </remarks>
    public (NudgeKind Kind, string Entity) WouldOpen(string? callsign)
    {
        var entity = DxccPrefixes.EntityOf(callsign);

        if (string.IsNullOrWhiteSpace(entity))
        {
            return (NudgeKind.None, "");
        }

        if (_workedEntities.Contains(entity))
        {
            return (NudgeKind.None, "");
        }

        var continent = DxccContinents.Of(entity);

        if (string.IsNullOrWhiteSpace(continent))
        {
            return (NudgeKind.None, "");
        }

        // **A CONTINENT HE HAS NEVER WORKED IS A DOOR, AND ITS NAME DOES NOT LEAVE
        // THIS METHOD.**
        return _workedContinents.Contains(continent)
            ? (NudgeKind.Visible, entity)
            : (NudgeKind.Door, "");
    }
}

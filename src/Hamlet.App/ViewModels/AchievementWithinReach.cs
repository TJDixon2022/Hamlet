using Hamlet.RadioEngine.Contacts;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.App.ViewModels;

/// <summary>
/// One station on the CQ list who would earn him something, and what (work instruction 506 task 3).
/// </summary>
/// <param name="Callsign">The station.</param>
/// <param name="Place">Its entity in the short form, or "".</param>
/// <param name="Earns">What it earns, in words: `opens South America`, `a new country and grid`.</param>
/// <param name="OpensContinent">True where it opens a continent: a door, ringed (R19), and the words say so.</param>
public sealed record ReachableStation(string Callsign, string Place, string Earns, bool OpensContinent)
{
    /// <summary>True where it only adds to a count: the plain green mark.</summary>
    public bool IsCounter => !OpensContinent;
}

/// <summary>
/// **Within reach right now**: up to three stations calling CQ who would earn him something (work instruction
/// 506 task 3, the dark strip along the bottom of `assets/achievements-look/opening-page.html`).
/// </summary>
/// <remarks>
/// <para>**A LIST TO READ.** It sends nothing and tunes nothing; the list is the one the window was opened
/// with, read once, and says when.</para>
/// <para>**WHAT THE SNAPSHOT CAN SAY AND NO MORE** (§0.0): it carries the time it was read and no band, so the
/// heading says the time and not the band the picture shows.</para>
/// <para>**DOORS FIRST**, then a new country and a new grid, then a new country, then a new grid; in each, the
/// order the list holds them.</para>
/// </remarks>
public sealed class AchievementWithinReach
{
    /// <summary>How many stations the strip draws at most.</summary>
    public const int Most = 3;

    /// <summary>Build the strip from the log and the CQ list.</summary>
    /// <param name="log">The contacts.</param>
    /// <param name="calling">The CQ list as the window was opened with it.</param>
    public AchievementWithinReach(AchievementLog log, CqSnapshot calling)
    {
        ArgumentNullException.ThrowIfNull(log);
        ArgumentNullException.ThrowIfNull(calling);

        var entities = new HashSet<string>(log.Entities, StringComparer.OrdinalIgnoreCase);
        var continents = new HashSet<string>(log.Continents, StringComparer.OrdinalIgnoreCase);
        var grids = new HashSet<string>(log.Grids, StringComparer.OrdinalIgnoreCase);
        var ranked = new List<(int Rank, ReachableStation Station)>();

        foreach (var call in calling.Calls)
        {
            var entity = DxccPrefixes.EntityOf(call.Callsign);
            var place = entity is null ? "" : EntitySpoken.Short(entity);
            var continent = DxccContinents.Of(entity);
            var grid = call.Grid.Length >= 4 ? call.Grid[..4].ToUpperInvariant() : "";
            var newCountry = entity is not null && !entities.Contains(entity);
            var newGrid = grid.Length == 4 && !grids.Contains(grid);

            if (newCountry && continent is not null && !continents.Contains(continent))
            {
                ranked.Add((0, new ReachableStation(call.Callsign, place, "opens " + DxccContinents.NameOf(continent), true)));
            }
            else if (newCountry && newGrid)
            {
                ranked.Add((1, new ReachableStation(call.Callsign, place, "a new country and grid", false)));
            }
            else if (newCountry)
            {
                ranked.Add((2, new ReachableStation(call.Callsign, place, "a new country", false)));
            }
            else if (newGrid)
            {
                ranked.Add((3, new ReachableStation(call.Callsign, place, "a new grid", false)));
            }
        }

        Stations = ranked.OrderBy(r => r.Rank).Take(Most).Select(r => r.Station).ToList();

        Heading = calling.ReadAt.Length > 0 ? "from the CQ list read at " + calling.ReadAt : "";

        NobodyLine = calling.ReadUtc is null ? "No CQ list was read when this opened."
            : calling.Calls.Count == 0 ? "Nobody was calling CQ when this opened."
            : Stations.Count == 0 ? "Nobody calling CQ would earn you something new."
            : "";
    }

    /// <summary>The stations, at most three.</summary>
    public IReadOnlyList<ReachableStation> Stations { get; }

    /// <summary>True where there is a station to draw.</summary>
    public bool HasStations => Stations.Count > 0;

    /// <summary>`from the CQ list read at 14:32 UTC`, or "" where the snapshot carries no time.</summary>
    public string Heading { get; }

    /// <summary>The one line where nobody would earn him anything; otherwise "".</summary>
    public string NobodyLine { get; }

    /// <summary>True where the strip draws the one line instead of stations.</summary>
    public bool HasNobody => NobodyLine.Length > 0;
}

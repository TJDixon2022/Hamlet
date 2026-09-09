using System.Globalization;
using Hamlet.RadioEngine.Explore;

namespace Hamlet.RadioEngine.Contacts;

/// <summary>
/// **One logged contact, reduced to the facts an achievement can be computed
/// from.**
/// </summary>
/// <remarks>
/// <para>**EVERY MEMBER IS NULLABLE AND NULL MEANS THE RECORD DID NOT SAY** (§0.0).
/// A record with no `GRIDSQUARE` has no distance and must not be counted as a zero;
/// a record whose callsign resolves to no entity has no country and no continent.
/// **A card built over these skips what is null rather than filling it.**</para>
/// <para>**NOTHING HERE IS A JUDGEMENT.** It is the log's own fields, parsed into
/// the shapes a comparison needs, plus two lookups - the DXCC entity and its
/// continent - both of which are cited tables that decline rather than guess.</para>
/// </remarks>
/// <param name="Callsign">The station worked.</param>
/// <param name="Entity">The DXCC entity, or null where the prefix is shared.</param>
/// <param name="Continent">The continent code, or null.</param>
/// <param name="Band">The band, as ADIF spells it, or null.</param>
/// <param name="Mode">The mode, or null where the record names none Hamlet knows.</param>
/// <param name="StartedUtc">When it started, or null.</param>
/// <param name="EndedUtc">When it ended, or null.</param>
/// <param name="Grid">His four-character locator, upper case, or null.</param>
/// <param name="MyGrid">
/// The operator's own locator **as that record carries it**, or null. It is the
/// record's and not Settings', because a contact made from somewhere else was
/// made from somewhere else and a bearing computed from today's grid would be
/// about a path that never existed.
/// </param>
/// <param name="ReportSent">The report the operator sent, in decibels, or null.</param>
/// <param name="ReportReceived">The report he sent back, in decibels, or null.</param>
/// <param name="Miles">
/// Great-circle distance from the operator's own grid, or null where either grid is
/// missing.
/// </param>
public sealed record AchievementContact(
    string Callsign,
    string? Entity,
    string? Continent,
    string? Band,
    ContactMode? Mode,
    DateTime? StartedUtc,
    DateTime? EndedUtc,
    string? Grid,
    string? MyGrid,
    int? ReportSent,
    int? ReportReceived,
    double? Miles);

/// <summary>
/// **What the contact log holds, in the shapes the achievements screen asks it
/// questions in.**
/// </summary>
/// <remarks>
/// <para>**IT READS AND IT NEVER WRITES** (the instruction's own standing rule). A
/// log record is a statement the operator made and no member here can revise, hide
/// or repair one.</para>
/// <para>**IT ANSWERS AND IT DOES NOT DECIDE WHAT TO SHOW.** Which cards are
/// visible is `ACHIEVEMENTS_PHILOSOPHY.md` §3.1's question and it is asked in the
/// shell; this type answers *what has he worked* and *what is his furthest*, and it
/// would answer them the same on a screen that showed everything.</para>
/// <para>**A RECORD WITH NO END MARKER IS NOT READ**, which is the source the
/// *worked* mark and unit 287's mode firsts have always used. An achievement claimed
/// off a half-written line is a claim about a contact whose record is damaged.</para>
/// </remarks>
public sealed class AchievementLog
{
    private readonly List<AchievementContact> _contacts;

    /// <summary>Read the log.</summary>
    /// <param name="records">Every record in the file, faults and all.</param>
    /// <param name="operatorGrid">
    /// The operator's own locator, from Settings. **Where this is absent no contact
    /// has a distance**, which is honest: a great-circle needs two ends.
    /// </param>
    /// <exception cref="ArgumentNullException">The records are null.</exception>
    public AchievementLog(
        IReadOnlyList<AdifLogRecord> records, string? operatorGrid = null)
    {
        ArgumentNullException.ThrowIfNull(records);

        var here = OperatorLocation.FromGrid(operatorGrid);

        _contacts = records
            .Where(r => r.Terminated)
            .Select(r => Read(r.Contact, here))
            .Where(c => c is not null)
            .Select(c => c!)
            .ToList();
    }

    /// <summary>Every contact the log holds, in file order.</summary>
    public IReadOnlyList<AchievementContact> Contacts => _contacts;

    /// <summary>How many contacts there are.</summary>
    public int Count => _contacts.Count;

    /// <summary>The bands he has worked, each named once.</summary>
    /// <remarks>
    /// **AS ADIF SPELLS THEM**, because that is what the record carries and what
    /// `HfBands` maps back for a reader. A record with no `BAND` contributes
    /// nothing rather than an empty band.
    /// </remarks>
    public IReadOnlyList<string> Bands => Distinct(c => c.Band);

    /// <summary>The modes he has worked, each named once.</summary>
    public IReadOnlyList<string> Modes => Distinct(c => c.Mode?.Name);

    /// <summary>The DXCC entities he has worked, each named once.</summary>
    public IReadOnlyList<string> Entities => Distinct(c => c.Entity);

    /// <summary>The continents he has worked, as codes, each named once.</summary>
    public IReadOnlyList<string> Continents => Distinct(c => c.Continent);

    /// <summary>The four-character grid squares he has worked, each named once.</summary>
    public IReadOnlyList<string> Grids
        => Distinct(c => c.Grid is { Length: >= 4 } g ? g[..4].ToUpperInvariant() : null);

    /// <summary>Every contact on one band.</summary>
    /// <param name="band">The band, as ADIF spells it.</param>
    /// <returns>The contacts, which may be empty.</returns>
    public IReadOnlyList<AchievementContact> OnBand(string? band)
        => _contacts.Where(
            c => Same(c.Band, band)).ToList();

    /// <summary>Every contact in one mode.</summary>
    /// <param name="mode">The mode's name.</param>
    /// <returns>The contacts, which may be empty.</returns>
    public IReadOnlyList<AchievementContact> InMode(string? mode)
        => _contacts.Where(c => Same(c.Mode?.Name, mode)).ToList();

    /// <summary>Every contact on one continent.</summary>
    /// <param name="code">The continent code.</param>
    /// <returns>The contacts, which may be empty.</returns>
    public IReadOnlyList<AchievementContact> OnContinent(string? code)
        => _contacts.Where(c => Same(c.Continent, code)).ToList();

    /// <summary>How many distinct entities he has worked on one continent.</summary>
    /// <param name="code">The continent code.</param>
    /// <returns>The count of entities, never of contacts.</returns>
    /// <remarks>
    /// **ENTITIES AND NOT CONTACTS**, because *4 of 63* is about places and forty
    /// contacts with one station is still one place.
    /// </remarks>
    public int EntitiesOn(string? code)
        => OnContinent(code)
            .Select(c => c.Entity)
            .Where(e => e is not null)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

    /// <summary>The contact furthest from the operator, or null.</summary>
    /// <param name="among">The contacts to look at.</param>
    /// <returns>The one with the greatest distance, or null where none has one.</returns>
    public static AchievementContact? Furthest(
        IReadOnlyList<AchievementContact> among)
        => among.Where(c => c.Miles is not null)
            .OrderByDescending(c => c.Miles!.Value)
            .FirstOrDefault();

    /// <summary>The lowest report he was given, or null.</summary>
    /// <param name="among">The contacts to look at.</param>
    /// <returns>The contact, or null where none carries a received report.</returns>
    /// <remarks>
    /// **LOWEST IS THE ACHIEVEMENT.** A signal read at -21 dB travelled further into
    /// the noise than one read at +02, so the weakest report he was given is the
    /// hardest thing his signal has done.
    /// </remarks>
    public static AchievementContact? WeakestReceived(
        IReadOnlyList<AchievementContact> among)
        => among.Where(c => c.ReportReceived is not null)
            .OrderBy(c => c.ReportReceived!.Value)
            .FirstOrDefault();

    /// <summary>The lowest report he gave, or null.</summary>
    public static AchievementContact? WeakestSent(
        IReadOnlyList<AchievementContact> among)
        => among.Where(c => c.ReportSent is not null)
            .OrderBy(c => c.ReportSent!.Value)
            .FirstOrDefault();

    /// <summary>The earliest contact, or null.</summary>
    public static AchievementContact? First(
        IReadOnlyList<AchievementContact> among)
        => among.Where(c => c.StartedUtc is not null)
            .OrderBy(c => c.StartedUtc!.Value)
            .FirstOrDefault();

    /// <summary>The latest contact, or null.</summary>
    public static AchievementContact? MostRecent(
        IReadOnlyList<AchievementContact> among)
        => among.Where(c => c.StartedUtc is not null)
            .OrderByDescending(c => c.StartedUtc!.Value)
            .FirstOrDefault();

    /// <summary>The day with the most contacts, and how many, or null.</summary>
    /// <param name="among">The contacts to look at.</param>
    /// <returns>The date and the count, or null where none carries a time.</returns>
    /// <remarks>
    /// **IN UTC, BECAUSE THAT IS WHAT THE RECORD CARRIES.** A log is kept in UTC and
    /// converting to a local day would put a contact on a different date from the one
    /// written in the file he can open and read.
    /// </remarks>
    public static (DateTime Day, int Count)? BusiestDay(
        IReadOnlyList<AchievementContact> among)
        => among.Where(c => c.StartedUtc is not null)
            .GroupBy(c => c.StartedUtc!.Value.Date)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => ((DateTime, int)?)(g.Key, g.Count()))
            .FirstOrDefault();

    /// <summary>The hour of the day with the most contacts, and how many, or null.</summary>
    public static (int Hour, int Count)? BusiestHour(
        IReadOnlyList<AchievementContact> among)
        => among.Where(c => c.StartedUtc is not null)
            .GroupBy(c => c.StartedUtc!.Value.Hour)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => ((int, int)?)(g.Key, g.Count()))
            .FirstOrDefault();

    /// <summary>Turn one record into the facts, or null where it names no station.</summary>
    private static AchievementContact? Read(AdifContact contact, LatLon? here)
    {
        var call = (contact.Call ?? "").Trim();

        if (call.Length == 0)
        {
            return null;
        }

        var entity = DxccPrefixes.EntityOf(call);
        var grid = Blank(contact.GridSquare)?.ToUpperInvariant();
        var there = OperatorLocation.FromGrid(grid);

        return new AchievementContact(
            Callsign: call.ToUpperInvariant(),
            Entity: entity,
            Continent: DxccContinents.Of(entity),
            Band: Blank(contact.Band),
            Mode: ContactModes.Six.FirstOrDefault(
                m => m.Matches(contact.Mode, contact.Submode)),
            StartedUtc: contact.StartedUtc,
            EndedUtc: contact.EndedUtc,
            Grid: grid,
            MyGrid: Blank(contact.MyGridSquare)?.ToUpperInvariant(),
            ReportSent: Decibels(contact.ReportSent),
            ReportReceived: Decibels(contact.ReportReceived),
            Miles: here is { } from && there is { } to
                ? GridPath.MilesBetween(from, to)
                : null);
    }

    /// <summary>A report field as a signed number, or null.</summary>
    /// <remarks>
    /// **A REPORT HAMLET WROTE IS `+00;-00`** (`Ft8ContactLogEntry`), so `-09` and
    /// `+02` parse. **An RST from somebody else's file does not** - `59` has no sign
    /// and is not decibels - and it comes back null rather than being read as a
    /// decibel figure ninety times too large.
    /// </remarks>
    private static int? Decibels(string? report)
    {
        var text = (report ?? "").Trim();

        if (text.Length < 2 || (text[0] != '+' && text[0] != '-'))
        {
            return null;
        }

        return int.TryParse(
            text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture,
            out var value)
            ? value
            : null;
    }

    private static string? Blank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool Same(string? left, string? right)
        => left is not null && right is not null
           && string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    /// <summary>Every distinct non-null answer, in the order first worked.</summary>
    private IReadOnlyList<string> Distinct(
        Func<AchievementContact, string?> of)
    {
        var seen = new List<string>();

        foreach (var contact in _contacts)
        {
            if (of(contact) is { Length: > 0 } value
                && !seen.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                seen.Add(value);
            }
        }

        return seen;
    }
}

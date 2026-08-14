using Hamlet.RadioEngine.Bands;

namespace Hamlet.RadioEngine.Explore;

/// <summary>Somewhere the operator stopped, and who was there if anybody knew
/// (HM-DEC-072).</summary>
/// <param name="FrequencyHz">Where it is.</param>
/// <param name="Station">
/// The callsign, or "" when nothing identified one. Empty is the ordinary case
/// and it is never filled with a guess.
/// </param>
/// <param name="Mode">The mode at the time, or "".</param>
/// <param name="BandName">Which band, e.g. "40 m".</param>
/// <param name="Neighborhood">What the map said lives there, or "".</param>
/// <param name="VisitedUtc">When the visit was recorded.</param>
public sealed record RecentStation(
    long FrequencyHz,
    string Station,
    string Mode,
    string BandName,
    string Neighborhood,
    DateTime VisitedUtc)
{
    /// <summary>The frequency as the app writes it, e.g. "7.030".</summary>
    public string FrequencyLabel
        => (FrequencyHz / 1_000_000.0).ToString(
            "0.000", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>True when a station was actually identified here.</summary>
    public bool IsIdentified => Station.Length > 0;

    /// <summary>
    /// What the entry reads as: a station where one was identified, and a place
    /// where none was.
    /// </summary>
    /// <remarks>
    /// THE WHOLE HONESTY OF THIS FEATURE IS IN THIS ONE PROPERTY (§0.0). An
    /// entry that read "7.030, W1AW" for a frequency the operator merely sat on
    /// would be Hamlet putting a station on the air that nobody heard. So the
    /// callsign appears only where something identified it, and everywhere else
    /// the entry is a place: the frequency and what the map says lives there,
    /// which is exactly what a favorite says when nobody typed a name
    /// (HM-DEC-060).
    /// </remarks>
    public string Label
    {
        get
        {
            if (IsIdentified)
            {
                return $"{Station} on {FrequencyLabel}";
            }

            return Neighborhood.Length > 0
                ? $"{FrequencyLabel}, {Lowercase(Neighborhood)}"
                : BandName.Length > 0
                    ? $"{FrequencyLabel} on {BandName}"
                    : FrequencyLabel;
        }
    }

    private static string Lowercase(string name)
        => name.Length > 1 && char.IsUpper(name[0]) && !char.IsUpper(name[1])
            ? char.ToLowerInvariant(name[0]) + name[1..]
            : name;
}

/// <summary>
/// Where the operator has been, so he can go back without the number
/// (HM-DEC-072).
/// </summary>
/// <remarks>
/// <para>THE SIBLING OF FAVORITES, AND IT BEHAVES LIKE ONE. A favorite is a
/// place he chose; this is a place he was. Both carry the context the map
/// already knows, both tune on a click, and an entry here can be starred into a
/// favorite carrying exactly what a directly saved one carries, because that is
/// how most favorites will actually be born: somebody was somewhere good, did
/// not think to save it, and wants it the following evening.</para>
/// <para>DWELL, NOT LANDING. The dial is a scroll wheel, so a literal history
/// would fill with near-identical entries between 7.029 and 7.031 and be
/// useless inside a minute. An entry appears only once he has stayed put, on
/// the reasoning that stopping somewhere is what makes it worth
/// remembering.</para>
/// <para>Pure: a list and a visit in, a list out. No clock is read here; the
/// moment is passed in (§5).</para>
/// </remarks>
public static class RecentStations
{
    /// <summary>How many places are remembered.</summary>
    /// <remarks>
    /// Ten, against favorites' ninety-nine, and the difference is the point.
    /// Favorites are a library somebody curates; this is the last few places
    /// he was, and a list long enough to need scrolling has stopped answering
    /// "where was I just now".
    /// </remarks>
    public const int Maximum = 10;

    /// <summary>
    /// How long the dial must sit still before the place is remembered.
    /// </summary>
    /// <remarks>
    /// <para>TWENTY SECONDS, AND THE FIGURE COMES FROM MORSE RATHER THAN FROM
    /// ROUNDNESS. Hunting across a band with the wheel, no frequency holds the
    /// dial for more than a second or two, so nothing is recorded while
    /// somebody is looking. Deciding whether a signal is worth staying for
    /// takes about one CQ call, and a full "CQ CQ CQ DE W1AW W1AW W1AW K" at a
    /// relaxed thirteen words a minute runs close to twenty-five seconds
    /// (HM-DEC-066). So the threshold sits just inside one call: long enough
    /// that passing through never counts, short enough that hearing somebody
    /// out always does.</para>
    /// <para>One named place rather than a literal at the call site, and
    /// deliberately not a setting. It is a judgment about what counts as
    /// stopping, and a slider would ask the operator to make it before he has
    /// any way to know.</para>
    /// </remarks>
    public static readonly TimeSpan Dwell = TimeSpan.FromSeconds(20);

    /// <summary>
    /// How far apart two frequencies can be and still be the same place.
    /// </summary>
    /// <remarks>
    /// <para>THE SAME WIDTH THE APP ALREADY CALLS ONE SIGNAL
    /// (<see cref="SpotIdentity.FrequencyBucketHz"/>), read from there rather
    /// than chosen again, because two numbers meaning "near enough" would drift
    /// apart and nobody would notice.</para>
    /// <para>A TOLERANCE RATHER THAN A BUCKET. Bucketing by division puts an
    /// invisible boundary every two hundred hertz, so 7.030.150 and 7.030.250
    /// would be separate entries while 7.030.010 and 7.030.190 merged, which is
    /// unpredictable in exactly the way that makes somebody stop trusting a
    /// list. Measuring the gap instead means near is always near.</para>
    /// <para>The tradeoff is stated rather than hidden: on Morse two notes two
    /// hundred hertz apart are usually two different stations, so a wide
    /// tolerance can merge two visits into one entry. That costs the older
    /// entry, and the alternative costs the whole list to near-duplicates,
    /// which is the failure this feature exists to avoid.</para>
    /// </remarks>
    public const long SamePlaceHz = SpotIdentity.FrequencyBucketHz;

    /// <summary>Whether two frequencies count as the same place.</summary>
    /// <param name="a">One frequency.</param>
    /// <param name="b">The other.</param>
    /// <returns>True when they are near enough to be one entry.</returns>
    public static bool IsSamePlace(long a, long b) => Math.Abs(a - b) <= SamePlaceHz;

    /// <summary>Build an entry from where the operator has been sitting.</summary>
    /// <param name="frequencyHz">The frequency.</param>
    /// <param name="station">The callsign if one was identified, or null.</param>
    /// <param name="mode">The mode, or "".</param>
    /// <param name="here">The neighborhood, or null.</param>
    /// <param name="nowUtc">The moment.</param>
    /// <returns>The entry.</returns>
    public static RecentStation From(
        long frequencyHz, string? station, string? mode, Neighborhood? here,
        DateTime nowUtc)
        => new(
            frequencyHz,
            (station ?? "").Trim().ToUpperInvariant(),
            (mode ?? "").Trim(),
            BandPlan.BandFor(frequencyHz)?.Name ?? "",
            here?.Name ?? "",
            nowUtc);

    /// <summary>
    /// Take a visit into the list, most recent first.
    /// </summary>
    /// <param name="existing">The list as it stands.</param>
    /// <param name="arriving">Where the operator has just been.</param>
    /// <returns>The new list, never longer than <see cref="Maximum"/>.</returns>
    /// <remarks>
    /// <para>RETURNING SOMEWHERE MOVES THE ENTRY RATHER THAN ADDING A SECOND
    /// COPY. A list with the same place in it three times is a list that has
    /// stopped being ten places.</para>
    /// <para>AND THE NEWEST VISIT'S IDENTIFICATION WINS, INCLUDING WHEN IT IS
    /// EMPTY. If Hamlet knew a callsign the first time and knows nothing this
    /// time, the entry stops carrying the callsign, because keeping it would say
    /// that station is there now and nothing checked (§0.0). The place survives
    /// either way, which is what the operator is actually navigating by.</para>
    /// </remarks>
    public static IReadOnlyList<RecentStation> Remember(
        IEnumerable<RecentStation> existing, RecentStation arriving)
    {
        ArgumentNullException.ThrowIfNull(existing);
        ArgumentNullException.ThrowIfNull(arriving);

        var kept = new List<RecentStation> { arriving };

        kept.AddRange(
            existing.Where(e => !IsSamePlace(e.FrequencyHz, arriving.FrequencyHz)));

        return kept.Count > Maximum ? kept.GetRange(0, Maximum) : kept;
    }

    /// <summary>
    /// Star an entry into a favorite, carrying the same context.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="here">The neighborhood, or null when the map has none.</param>
    /// <param name="nowUtc">The moment it is saved.</param>
    /// <returns>The favorite.</returns>
    /// <remarks>
    /// THE SAME CONTEXT A DIRECT SAVE CAPTURES (HM-DEC-060), because a favorite
    /// born this way must be indistinguishable from one born at the star. The
    /// neighborhood is passed in rather than taken off the entry so the name
    /// comes from <see cref="Favorites.NameFor"/> and there is one naming rule
    /// rather than two.
    /// </remarks>
    public static Favorite ToFavorite(
        RecentStation entry, Neighborhood? here, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return Favorites.From(entry.FrequencyHz, entry.Mode, here, nowUtc);
    }
}

/// <summary>
/// Whether the dial has sat still long enough to be worth remembering
/// (HM-DEC-072).
/// </summary>
/// <remarks>
/// <para>Deliberately not a timer. It is told where the dial is and what time
/// it is, and it answers; the clock lives with whoever is driving it. That is
/// what makes the dwell rule testable to the second without waiting twenty of
/// them (§5).</para>
/// <para>It reports a settle exactly once per stop. Somebody who leaves the
/// radio on one frequency for an hour has been in one place, not in a hundred
/// and eighty of them.</para>
/// </remarks>
public sealed class DwellTracker
{
    private long _frequencyHz;
    private DateTime _arrivedUtc = DateTime.MinValue;
    private bool _reported = true;

    /// <summary>Where the dial is now.</summary>
    public long FrequencyHz => _frequencyHz;

    /// <summary>The dial moved.</summary>
    /// <param name="frequencyHz">Where it moved to.</param>
    /// <param name="nowUtc">When.</param>
    /// <remarks>
    /// A move to where it already is does not restart the clock. Several
    /// surfaces can announce the same frequency in one gesture, and each one
    /// resetting the count would mean the dial never settles.
    /// </remarks>
    public void Moved(long frequencyHz, DateTime nowUtc)
    {
        if (frequencyHz == _frequencyHz && _arrivedUtc != DateTime.MinValue)
        {
            return;
        }

        _frequencyHz = frequencyHz;
        _arrivedUtc = nowUtc;
        _reported = false;
    }

    /// <summary>
    /// Has the dial now been still long enough?
    /// </summary>
    /// <param name="nowUtc">The moment.</param>
    /// <returns>True once per stop, on the first ask after the dwell passes.</returns>
    public bool Settled(DateTime nowUtc)
    {
        if (_reported || _arrivedUtc == DateTime.MinValue)
        {
            return false;
        }

        if (nowUtc - _arrivedUtc < RecentStations.Dwell)
        {
            return false;
        }

        _reported = true;
        return true;
    }
}

using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Telemetry;

/// <summary>
/// The rig state, as Hamlet believed it at a moment, in the record
/// (HM-DEC-077).
/// </summary>
/// <remarks>
/// <para>THE MODEL HELD THIRTY-ONE VALUES AND NOT ONE APPEARED IN TELEMETRY.
/// The evening this was written, break-in could only be learned by opening a
/// window and photographing it, which is §0.0.1 failing at the one job it has:
/// when something is wrong, the app's own record must be enough to tell whether
/// the fault is in the signal, the radio, or Hamlet.</para>
/// <para>A ROW NOBODY READ SAYS UNKNOWN AND NEVER ZERO (HM-DEC-050). That is
/// the whole reason the snapshot carries provenance per value rather than a bag
/// of numbers: a break-in of zero and a break-in nobody has read produce
/// identical numbers and completely different diagnoses.</para>
/// <para>FULL ON THE EVENTS THAT MATTER, DELTA ON THE HEARTBEAT. A full snapshot
/// on every connect, every readiness evaluation and every decoder transition,
/// because those are the moments somebody reconstructs afterward. A delta on the
/// heartbeat naming only what changed, because a session that goes quiet still
/// needs a spine and thirty-one rows a minute would bury the events worth
/// finding.</para>
/// <para>Pure: state in, a bag out. Nothing identifying can enter, because
/// nothing here reads anything but rig fields (HM-DEC-018).</para>
/// </remarks>
public static class RigSnapshot
{
    /// <summary>How long a reading counts as current for the record.</summary>
    /// <remarks>
    /// A minute. Long enough that an ordinary poll cycle does not mark
    /// everything stale, short enough that a value carried over from before a
    /// reconnect is visible as the old news it is.
    /// </remarks>
    public static readonly TimeSpan FreshFor = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Every value Hamlet holds, with its provenance and age.
    /// </summary>
    /// <param name="state">What Hamlet knows.</param>
    /// <param name="nowUtc">The moment.</param>
    /// <returns>The bag, keyed by field name.</returns>
    public static IReadOnlyDictionary<string, object?> Full(
        RigState state, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(state);

        var rows = state.All()
            .Select(v => DeterminedBy.From(v, nowUtc, FreshFor).ToBag())
            .ToList();

        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["rig"] = rows,
            ["rigKnownCount"] = state.KnownCount,
            ["rigFieldCount"] = rows.Count,
        };
    }

    /// <summary>
    /// Only what changed since the last snapshot.
    /// </summary>
    /// <param name="previous">What was reported last time, or null.</param>
    /// <param name="state">What Hamlet knows now.</param>
    /// <param name="nowUtc">The moment.</param>
    /// <returns>The bag, carrying only the rows that moved.</returns>
    /// <remarks>
    /// A CHANGE IS A CHANGE OF STATE OR OF VALUE, not of age. Everything ages
    /// every second, so treating that as a change would make every delta a full
    /// snapshot and defeat the point.
    /// </remarks>
    public static IReadOnlyDictionary<string, object?> Delta(
        RigState? previous, RigState state, DateTime nowUtc)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (previous is null)
        {
            return Full(state, nowUtc);
        }

        var changed = new List<IReadOnlyDictionary<string, object?>>();

        foreach (var value in state.All())
        {
            var was = previous[value.Field];

            if (was.State == value.State
                && Nullable.Equals(was.Number, value.Number))
            {
                continue;
            }

            changed.Add(DeterminedBy.From(value, nowUtc, FreshFor).ToBag());
        }

        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["rigChanged"] = changed,
            ["rigKnownCount"] = state.KnownCount,
        };
    }
}

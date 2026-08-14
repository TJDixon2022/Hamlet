using Hamlet.RadioEngine.Civ;

namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// Everything Hamlet knows about the radio right now, in one place.
/// </summary>
/// <remarks>
/// <para>ONE MODEL, READ BY EVERY SURFACE (HM-DEC-050). The mode badge, the
/// filter designator, the S-meter, the CW terminal's header and the diagnostics
/// screen all read this, so they cannot disagree about what the radio is doing.
/// Before it existed the mode badge said "CW" whatever the radio was set to,
/// which meant the screen lied whenever somebody switched to sideband.</para>
/// <para>Immutable, and replaced wholesale rather than mutated. The poll loop
/// runs on a background thread and the UI reads on its own, so handing out a
/// snapshot that cannot change underneath a reader is simpler and cheaper than
/// locking every field.</para>
/// <para>Absent means unknown. A field that has never been read is not in the
/// dictionary and <see cref="this[RigField]"/> answers
/// <see cref="RigValue.Unknown"/> for it, so there is no path by which a caller
/// gets a zero that looks like a reading (§0.0).</para>
/// </remarks>
public sealed class RigState
{
    private readonly IReadOnlyDictionary<RigField, RigValue> _values;

    /// <summary>An empty state: everything unknown.</summary>
    public static RigState Empty { get; } = new(
        new Dictionary<RigField, RigValue>(), null);

    /// <summary>Creates a state.</summary>
    /// <param name="values">What is known.</param>
    /// <param name="rigName">Which radio this describes, or null.</param>
    public RigState(IReadOnlyDictionary<RigField, RigValue> values, string? rigName)
    {
        _values = values;
        RigName = rigName;
    }

    /// <summary>Which radio this describes, or null when none is connected.</summary>
    public string? RigName { get; }

    /// <summary>What is known about one field. Never null.</summary>
    /// <param name="field">The field.</param>
    /// <returns>The value, or an unknown one.</returns>
    public RigValue this[RigField field]
        => _values.TryGetValue(field, out var value) ? value : RigValue.Unknown(field);

    /// <summary>Every field in the order the diagnostics screen shows them.</summary>
    /// <returns>One value per field, unknown ones included.</returns>
    /// <remarks>
    /// Unknown fields are listed rather than hidden. A diagnostics screen that
    /// showed only what it had would leave somebody wondering whether a missing
    /// row means "not read" or "not a thing", which is the question the screen
    /// exists to answer.
    /// </remarks>
    public IReadOnlyList<RigValue> All()
        => Enum.GetValues<RigField>().Select(f => this[f]).ToList();

    /// <summary>A copy with one field replaced.</summary>
    /// <param name="value">The new value.</param>
    /// <returns>The new state.</returns>
    public RigState With(RigValue value)
    {
        var next = new Dictionary<RigField, RigValue>(_values) { [value.Field] = value };
        return new RigState(next, RigName);
    }

    /// <summary>A copy with several fields replaced.</summary>
    /// <param name="values">The new values.</param>
    /// <returns>The new state.</returns>
    public RigState With(IEnumerable<RigValue> values)
    {
        var next = new Dictionary<RigField, RigValue>(_values);

        foreach (var value in values)
        {
            next[value.Field] = value;
        }

        return new RigState(next, RigName);
    }

    /// <summary>A copy naming the radio.</summary>
    /// <param name="rigName">The radio's name.</param>
    /// <returns>The new state.</returns>
    public RigState WithRigName(string? rigName)
        => new(new Dictionary<RigField, RigValue>(_values), rigName);

    /// <summary>The mode, or null when it has not been read.</summary>
    public CivMode? Mode
        => this[RigField.Mode] is { IsKnown: true, Number: { } n }
            ? (CivMode)(int)n
            : null;

    /// <summary>The filter's width in hertz, or null.</summary>
    public int? FilterBandwidthHz
        => this[RigField.FilterBandwidth] is { IsKnown: true, Number: { } n } ? (int)n : null;

    /// <summary>Where the S-meter sits, 0 to 1, or null when it is unknown.</summary>
    /// <remarks>
    /// Null rather than zero, and the caller must keep it as null. A needle
    /// resting at zero looks exactly like a measurement of a quiet band, and
    /// that is the confident guess this whole model exists to prevent.
    /// </remarks>
    public double? SMeterFraction
        => this[RigField.SMeter] is { IsKnown: true, Number: { } n }
            ? CivSMeter.Fraction((int)n)
            : null;

    /// <summary>True when the radio is known to be transmitting.</summary>
    /// <remarks>
    /// Known to be. An unread status is not "receiving", it is unread, and
    /// anything that keys a transmitter later has to treat those differently
    /// (§0.2).
    /// </remarks>
    public bool IsTransmitting
        => this[RigField.TransmitStatus] is { IsKnown: true, Number: 1 };

    /// <summary>How many fields carry a real reading.</summary>
    public int KnownCount => _values.Values.Count(v => v.IsKnown);
}

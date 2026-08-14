namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// One thing Hamlet knows, or does not know, about the radio right now.
/// </summary>
/// <param name="Field">Which thing.</param>
/// <param name="State">How much is known about it.</param>
/// <param name="Number">
/// The value as a number where one makes sense: hertz, words a minute, an
/// S-meter unit. Null whenever <see cref="State"/> is not
/// <see cref="RigValueState.Known"/>, and null for values that are not
/// numbers.
/// </param>
/// <param name="Text">
/// What to show the operator. Never a stand-in: when the state is not known
/// this says so in words rather than showing a plausible figure.
/// </param>
/// <param name="AtUtc">When it was read. Null unless it was.</param>
/// <param name="Source">
/// Where it came from, verbatim enough to check: "CI-V 1A 03", "transceive 00",
/// "capabilities". §0.0.1 wants a wrong reading to arrive with its provenance
/// attached, and this is that.
/// </param>
/// <remarks>
/// <para>ONE SHAPE FOR EVERYTHING, so the diagnostics screen can list the whole
/// model without knowing what any particular field means, and so a new field
/// added next month appears there without anybody remembering to add it.</para>
/// <para>The timestamp is what makes staleness expressible. A value read four
/// minutes ago is not the same claim as one read a second ago, and an S-meter
/// is the field where the difference matters most.</para>
/// </remarks>
public sealed record RigValue(
    RigField Field,
    RigValueState State,
    double? Number,
    string Text,
    DateTime? AtUtc,
    string Source)
{
    /// <summary>True when there is a real reading behind this.</summary>
    public bool IsKnown => State == RigValueState.Known;

    /// <summary>A field never read, or whose read failed.</summary>
    /// <param name="field">Which field.</param>
    /// <param name="source">Why it is unknown, e.g. "not read yet".</param>
    /// <returns>The value.</returns>
    public static RigValue Unknown(RigField field, string source = "not read yet")
        => new(field, RigValueState.Unknown, null, "unknown", null, source);

    /// <summary>A field this radio does not have (HM-DEC-030).</summary>
    /// <param name="field">Which field.</param>
    /// <param name="source">What decided that, e.g. "training radio".</param>
    /// <returns>The value.</returns>
    public static RigValue Unsupported(RigField field, string source)
        => new(field, RigValueState.Unsupported, null, "not on this radio", null, source);

    /// <summary>
    /// A field the manual documents no command for.
    /// </summary>
    /// <param name="field">Which field.</param>
    /// <param name="reason">Why, in a sentence somebody can act on.</param>
    /// <returns>The value.</returns>
    /// <remarks>
    /// Loud on purpose (§4). The alternative is a plausible command byte that
    /// nobody can check, which is how a radio ends up being sent something its
    /// maker never described.
    /// </remarks>
    public static RigValue Undocumented(RigField field, string reason)
        => new(field, RigValueState.Undocumented, null, "no documented command", null, reason);

    /// <summary>A field just read from the radio.</summary>
    /// <param name="field">Which field.</param>
    /// <param name="number">The numeric value, or null when it is not a number.</param>
    /// <param name="text">What to show.</param>
    /// <param name="atUtc">When it was read.</param>
    /// <param name="source">Which command produced it.</param>
    /// <returns>The value.</returns>
    public static RigValue Known(
        RigField field, double? number, string text, DateTime atUtc, string source)
        => new(field, RigValueState.Known, number, text, atUtc, source);

    /// <summary>How old this reading is, or null when there is none.</summary>
    /// <param name="nowUtc">Reference time.</param>
    /// <returns>The age, or null.</returns>
    public TimeSpan? Age(DateTime nowUtc)
        => AtUtc is null ? null : nowUtc - AtUtc.Value;

    /// <summary>
    /// True when this reading is older than its kind should be.
    /// </summary>
    /// <param name="nowUtc">Reference time.</param>
    /// <param name="freshFor">How long a reading of this kind stays current.</param>
    /// <returns>True when the UI must say so rather than showing it as current.</returns>
    /// <remarks>
    /// A stale number shown as current is the same failure as an invented one,
    /// only harder to spot. The S-meter is the case that matters: a reading
    /// from a minute ago is a fact about a minute ago.
    /// </remarks>
    public bool IsStale(DateTime nowUtc, TimeSpan freshFor)
        => IsKnown && Age(nowUtc) is { } age && age > freshFor;
}

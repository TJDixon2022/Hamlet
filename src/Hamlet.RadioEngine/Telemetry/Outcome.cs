using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Telemetry;

/// <summary>Which way a decision point went (HM-DEC-077).</summary>
/// <remarks>
/// THE VOCABULARY WAS ALL COMPLETIONS AND THAT WAS THE FAULT. Every event in
/// the file was a thing Hamlet finished doing, so there was no event anywhere
/// for a thing it chose not to do or tried and failed at. A refusal is an
/// outcome. A failure is an outcome. Both are as loggable as success and more
/// useful, because success is the case nobody ever has to diagnose.
/// </remarks>
public enum Outcome
{
    /// <summary>It went ahead.</summary>
    Proceeded,

    /// <summary>Hamlet declined, on purpose, for a reason it can name.</summary>
    Refused,

    /// <summary>It was attempted and did not work.</summary>
    Failed,

    /// <summary>It went ahead with less than it wanted.</summary>
    Degraded,
}

/// <summary>
/// One state value that decided an outcome, with its provenance (HM-DEC-077).
/// </summary>
/// <param name="Field">Which value, as a stable token.</param>
/// <param name="Provenance">read, unknown, unsupported, undocumented or stale.</param>
/// <param name="Value">
/// The reading as a number, or null when there was not one. Never a stand-in:
/// a field nobody read carries null and says unknown, never zero (HM-DEC-050).
/// </param>
/// <param name="AgeSeconds">How old the reading was, or null when unread.</param>
public sealed record DeterminedBy(
    string Field, string Provenance, double? Value, double? AgeSeconds)
{
    /// <summary>A value that was read, because Hamlet asked for it.</summary>
    public const string Read = "read";

    /// <summary>
    /// A value the radio volunteered, which nobody asked for (HM-DEC-050).
    /// </summary>
    /// <remarks>
    /// **THIS WORD DID NOT EXIST AND ITS ABSENCE WAS READ AS A MEASUREMENT.**
    /// Every known value was recorded as `read`, whatever mechanism produced it,
    /// so a frequency pushed by the operator's own dial and one polled thirty
    /// seconds later were the same word in the file. Six sessions were then
    /// counted, "broadcast by any label 0" came back, and that was taken as
    /// evidence the broadcast path was dead — when it is what this vocabulary
    /// returns for a working one. The same count comes back zero on **every**
    /// telemetry file this project has ever written, including the builds that
    /// tracked the dial perfectly.
    /// </remarks>
    public const string Broadcast = "broadcast";

    /// <summary>A value nobody has read yet, or whose read failed.</summary>
    public const string Unknown = "unknown";

    /// <summary>A value this radio genuinely does not have.</summary>
    public const string Unsupported = "unsupported";

    /// <summary>A value the manual does not document.</summary>
    public const string Undocumented = "undocumented";

    /// <summary>A value that was read, a while ago.</summary>
    public const string Stale = "stale";

    /// <summary>
    /// Describe a rig value as the thing that decided something.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="nowUtc">The moment, for the age.</param>
    /// <param name="freshFor">How long a reading counts as current.</param>
    /// <returns>The record, never null.</returns>
    /// <remarks>
    /// <para>UNKNOWN AND OFF MUST SURVIVE INTO THE FILE AS DIFFERENT THINGS.
    /// Refusing on unknown is correct (HM-DEC-050) and refusing on off is
    /// something the operator can walk over to the radio and fix, and a record
    /// that conflates them is worth nothing on the evening it is needed. So the
    /// provenance is carried beside the number rather than being folded into
    /// it.</para>
    /// <para>Stale is a provenance and not a separate flag, because a reading
    /// four minutes old decided the outcome just as much as a fresh one and the
    /// difference is the whole diagnosis.</para>
    /// </remarks>
    public static DeterminedBy From(
        RigValue value, DateTime nowUtc, TimeSpan? freshFor = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        var age = value.Age(nowUtc);

        var provenance = value.State switch
        {
            RigValueState.Known when freshFor is { } fresh && value.IsStale(nowUtc, fresh)
                => Stale,

            // **HOW IT ARRIVED IS PART OF WHAT IT IS.** A value the radio
            // volunteered cannot be stale by the poll's standard and cannot be
            // late by anybody's, and telling it apart from a polled one is what
            // makes "the dial is being tracked" checkable rather than arguable.
            RigValueState.Known when value.IsBroadcast => Broadcast,
            RigValueState.Known => Read,
            RigValueState.Unsupported => Unsupported,
            RigValueState.Undocumented => Undocumented,
            _ => Unknown,
        };

        return new DeterminedBy(
            value.Field.ToString(),
            provenance,
            value.IsKnown ? value.Number : null,
            age?.TotalSeconds is { } seconds ? Math.Round(seconds, 1) : null);
    }

    /// <summary>Describe something that is not a rig value.</summary>
    /// <param name="field">A stable token for it.</param>
    /// <param name="value">Its value as a number, or null.</param>
    /// <param name="known">Whether it is known at all.</param>
    /// <returns>The record.</returns>
    public static DeterminedBy Fact(string field, double? value, bool known = true)
        => new(field, known ? Read : Unknown, known ? value : null, null);

    /// <summary>The bag this becomes in a telemetry payload.</summary>
    /// <returns>A dictionary, safe to serialize.</returns>
    public IReadOnlyDictionary<string, object?> ToBag()
    {
        var bag = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["field"] = Field,
            ["provenance"] = Provenance,
        };

        if (Value is { } number)
        {
            bag["value"] = number;
        }

        if (AgeSeconds is { } age)
        {
            bag["ageSeconds"] = age;
        }

        return bag;
    }
}

/// <summary>
/// The shape every decision event carries (HM-DEC-077).
/// </summary>
/// <remarks>
/// <para>THE PRINCIPLE, IN ONE PLACE SO IT CANNOT BE HALF-APPLIED: every
/// decision point that can go more than one way emits an event naming the branch
/// taken and the state that determined it.</para>
/// <para>The reason is a stable machine token rather than a display string. A
/// display string is written for a person, gets reworded the next time somebody
/// improves the copy, and takes every filter and every comparison across
/// sessions with it. The sentence the operator reads lives in the UI; the token
/// lives here.</para>
/// <para>NOTHING IDENTIFYING GOES IN, EVER (HM-DEC-018). This carries field
/// names, provenances, numbers and tokens. There is no member here that could
/// hold a callsign, a message, or decoded text, which is the point: the shape
/// itself refuses.</para>
/// </remarks>
public sealed record OutcomeEvent(
    Outcome Outcome, string Reason, IReadOnlyList<DeterminedBy> DeterminedBy)
{
    /// <summary>Nothing stood in the way.</summary>
    public const string Ok = "ok";

    /// <summary>Build the payload bag for this event.</summary>
    /// <param name="extra">Anything else worth recording, or null.</param>
    /// <returns>The bag.</returns>
    public IReadOnlyDictionary<string, object?> ToBag(
        IReadOnlyDictionary<string, object?>? extra = null)
    {
        var bag = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["outcome"] = Outcome.ToString().ToLowerInvariant(),
            ["reason"] = Reason,
            ["determinedBy"] = DeterminedBy.Select(d => d.ToBag()).ToList(),
        };

        if (extra is not null)
        {
            foreach (var pair in extra)
            {
                bag[pair.Key] = pair.Value;
            }
        }

        return bag;
    }

    /// <summary>
    /// The level this outcome deserves.
    /// </summary>
    /// <remarks>
    /// LEVELS START MEANING SOMETHING (HM-DEC-077). Everything was info, so
    /// nothing could be found by scanning and a reconnect nobody asked for was
    /// logged identically to a healthy one. A refusal is a warning because
    /// somebody wanted something and did not get it; a failure is an error.
    /// </remarks>
    public TelemetryLevel Level => Outcome switch
    {
        Outcome.Proceeded => TelemetryLevel.Info,
        Outcome.Refused => TelemetryLevel.Warn,
        Outcome.Degraded => TelemetryLevel.Warn,
        _ => TelemetryLevel.Error,
    };
}

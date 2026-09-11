namespace Hamlet.RadioEngine.Telemetry;

/// <summary>
/// Telemetry categories. Each is independently switchable in Settings; all
/// default on (HM-DEC-018). The set is the schema — adding a category is a
/// deliberate act, not a string typed at a call site.
/// </summary>
public enum TelemetryCategory
{
    /// <summary>App start/stop, version, unhandled errors.</summary>
    Diagnostics,

    /// <summary>Connect attempts, timeouts, CI-V errors, port and rig type.</summary>
    Rig,

    /// <summary>Band changes and where a tune came from (map, tape, digits, spot).</summary>
    Tuning,

    /// <summary>Neighborhood clicks, field-guide opens, spot tunes.</summary>
    Explore,

    /// <summary>Decoder runs and confidence statistics — never message content.</summary>
    Decode,

    /// <summary>
    /// Transmissions: when a slot went out, where, how long for, and how the
    /// radio came out of transmit — never message content and never a callsign.
    /// </summary>
    /// <remarks>
    /// **ADDED DELIBERATELY** (work instruction 255, task 4), which this enum's
    /// own comment requires of any addition. Step 3's fourth exit criterion is
    /// that the transmitted slot is recorded, and a transmission is not a decode:
    /// it is the one thing Hamlet does that reaches other people, and it is the
    /// category an operator would most want to be able to switch on and read
    /// back. HM-DEC-018 governs what may be in it and
    /// <see cref="TransmitRecord"/> is the shape that enforces that.
    /// </remarks>
    Transmit,

    /// <summary>Frame rates, render timings, spectrum throughput.</summary>
    Performance,

    /// <summary>
    /// The PSK31 path: what the search saw, which carriers appeared and why they went,
    /// what the squelch did, and what the transmit side composed and refused.
    /// </summary>
    /// <remarks>
    /// <para>**ADDED DELIBERATELY** (work instruction 322, task 2), which this enum's own
    /// comment requires of any addition. **HM-DEC-018 named six categories and this is an
    /// eighth**; the ruling's substance is *what may be in the file and who may switch it
    /// off*, not the count, and both of those are unchanged. It is on by default and
    /// switchable exactly as the others are, because an absent key means enabled.</para>
    /// <para>**WHY NOT `Decode`.** The measured reason: across ten sessions and 12,587
    /// events on 2026-09-11, the FT8 path wrote 3,448 `ft8_slot` and 2,317
    /// `decode_quality`, and **the PSK31 path wrote nothing at all**. Folding a
    /// continuously searching mode into the same category as a slotted one would put the
    /// new events under a switch somebody turns off to quieten FT8, and the whole point
    /// of them is that they are the only way to answer *was the band empty or was the
    /// squelch shut* without a screenshot.</para>
    /// <para>**WHAT MAY BE IN IT IS NARROWER THAN THE RULING REQUIRES.** An offset, a
    /// score, a kind, a count, a reason. **Never a callsign, never decoded text, never a
    /// grid** - §2.1 and HM-DEC-018 - and the test that pins this scans the serialised
    /// JSON rather than trusting each call site to remember.</para>
    /// </remarks>
    Psk31,
}

/// <summary>Severity of a telemetry event.</summary>
public enum TelemetryLevel
{
    /// <summary>Normal activity.</summary>
    Info,

    /// <summary>Something recoverable went wrong.</summary>
    Warn,

    /// <summary>Something failed.</summary>
    Error,
}

/// <summary>
/// One telemetry line, schema B (HM-DEC-018): timestamp, session, level,
/// app version, category, event name, and a small data bag.
/// </summary>
/// <param name="TimestampUtc">When it happened.</param>
/// <param name="SessionId">Short id correlating one run of the app.</param>
/// <param name="Level">Severity.</param>
/// <param name="AppVersion">Which build produced it.</param>
/// <param name="Category">Which switch governs it.</param>
/// <param name="Event">Snake_case event name, e.g. "connect_failed".</param>
/// <param name="Data">Event-specific values. Never callsigns, never decoded
/// message content, never anything identifying a person or contact.</param>
public sealed record TelemetryEvent(
    DateTime TimestampUtc,
    string SessionId,
    TelemetryLevel Level,
    string AppVersion,
    TelemetryCategory Category,
    string Event,
    IReadOnlyDictionary<string, object?> Data);

/// <summary>
/// The engine's telemetry seam. Writing is fire-and-forget and never throws:
/// logging that can crash the app is worse than no logging (§8).
/// </summary>
public interface ITelemetry
{
    /// <summary>Record an event, if its category is enabled.</summary>
    void Write(TelemetryCategory category, string eventName,
        IReadOnlyDictionary<string, object?>? data = null,
        TelemetryLevel level = TelemetryLevel.Info);

    /// <summary>Events dropped because a write failed — surfaced rather than
    /// hidden, so a silent logger is detectable.</summary>
    long DroppedEventCount { get; }
}

/// <summary>Telemetry sink that records nothing. For tests and for the case
/// where every category is switched off.</summary>
public sealed class NullTelemetry : ITelemetry
{
    /// <summary>Shared instance.</summary>
    public static NullTelemetry Instance { get; } = new();

    /// <inheritdoc/>
    public long DroppedEventCount => 0;

    /// <inheritdoc/>
    public void Write(TelemetryCategory category, string eventName,
        IReadOnlyDictionary<string, object?>? data = null,
        TelemetryLevel level = TelemetryLevel.Info)
    {
        // Deliberately nothing.
    }
}

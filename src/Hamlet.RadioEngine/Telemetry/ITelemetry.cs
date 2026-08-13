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

    /// <summary>Frame rates, render timings, spectrum throughput.</summary>
    Performance,
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

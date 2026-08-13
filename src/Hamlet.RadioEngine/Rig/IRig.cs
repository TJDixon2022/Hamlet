namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// The seam between Hamlet and a transceiver (HM-DEC-003). The real
/// implementation speaks CI-V to an IC-7300 over a serial port; the fake
/// implementation backs UI development and tests with no radio attached.
/// </summary>
/// <remarks>
/// Seed shape, expected to grow: mode control, CW keying (CI-V 0x17),
/// spectrum scope subscription (CI-V 0x27, HM-DEC-005) arrive with phase 1
/// plumbing. Grows by ruling, not by drift — additions land with the work
/// that needs them and are named in the delivery.
/// </remarks>
public interface IRig
{
    /// <summary>True while a rig is connected and responding.</summary>
    bool IsConnected { get; }

    /// <summary>
    /// Raised when the rig reports a frequency change from any source —
    /// including the operator turning the physical VFO knob. Raised on a
    /// background thread; the UI layer marshals.
    /// </summary>
    event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

    /// <summary>Open the connection. Returns false on failure; never throws for
    /// an unreachable rig — unreachable is an expected condition, not an error.</summary>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>Close the connection. Safe to call when not connected.</summary>
    Task DisconnectAsync();

    /// <summary>Read the current VFO frequency in hertz.</summary>
    Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default);

    /// <summary>Set the VFO frequency in hertz.</summary>
    Task SetFrequencyHzAsync(long frequencyHz, CancellationToken cancellationToken = default);
}

/// <summary>Payload for <see cref="IRig.FrequencyChanged"/>.</summary>
public sealed class FrequencyChangedEventArgs : EventArgs
{
    /// <summary>Creates the payload carrying the new VFO frequency.</summary>
    public FrequencyChangedEventArgs(long frequencyHz) => FrequencyHz = frequencyHz;

    /// <summary>The new VFO frequency in hertz.</summary>
    public long FrequencyHz { get; }
}

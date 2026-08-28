using Hamlet.RadioEngine.Civ;
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
    /// True when there is no radio behind this — the training radio rather
    /// than hardware on the air.
    /// </summary>
    /// <remarks>
    /// <para>Get-only, and answered by the implementation rather than
    /// configured on it. Connection state IS the mode (HM-DEC-026): the
    /// waterfall's "these signals are simulated" label and the choice of
    /// spectrum source are both derived from this, so there is no separate
    /// practice mode to enter and no setting that could put synthetic signals
    /// on screen unlabeled.</para>
    /// <para>A property rather than a type check at the call site, because a
    /// type check is a rule spread across every caller and this is a rule
    /// that has to hold in one place.</para>
    /// </remarks>
    bool IsSimulated { get; }

    /// <summary>What this particular radio can do (HM-DEC-030).</summary>
    /// <remarks>
    /// Reported by the implementation. The UI reads this rather than assuming
    /// IC-7300 features, so a radio without a spectrum scope or a built-in
    /// keyer degrades to an honest "this radio does not do that" instead of
    /// offering a control that cannot work.
    /// </remarks>
    RigCapabilities Capabilities { get; }

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

    /// <summary>
    /// Read one thing about the radio's current state.
    /// </summary>
    /// <param name="field">What to read.</param>
    /// <param name="context">
    /// What is already known, for the few readings that need it. A filter index
    /// means nothing without the mode, because the scale it sits on depends on
    /// it.
    /// </param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>
    /// One or more values. Never empty, and never throws for an unreachable
    /// value: a read that times out comes back
    /// <see cref="RigValueState.Unknown"/> with the reason attached, because a
    /// radio that stopped answering is a condition rather than an error and
    /// retrying it in a loop would flood a slow bus (HM-DEC-050).
    /// </returns>
    /// <remarks>
    /// One command answers two fields in one case: mode and filter selection
    /// arrive together, so asking for the mode returns both rather than
    /// spending a second transaction on a bus this slow.
    /// </remarks>
    Task<IReadOnlyList<RigValue>> ReadAsync(
        RigField field, RigState context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the operating mode, and its data variant.
    /// </summary>
    /// <param name="mode">The mode to select.</param>
    /// <param name="dataMode">
    /// Whether the data variant is wanted. USB and USB-D are different facts to
    /// this radio, and it is the difference between hearing FT8 and hearing
    /// nothing useful (HM-DEC-056).
    /// </param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>
    /// What happened, and never a throw. A radio that did not confirm leaves the
    /// mode unknown rather than assumed: a mode Hamlet believes it set and did
    /// not is a guess presented as a decode (§0.0).
    /// </returns>
    /// <remarks>
    /// THE FIRST WRITE THIS APP MAKES, and the pattern the setting writes
    /// inherited. Nothing here goes near keying the transmitter (§0.2).
    /// </remarks>
    /// <param name="filterSlot">
    /// Which filter preset to select — FIL1, FIL2 or FIL3 — or null to leave the
    /// radio to pick that mode's own default.
    /// </param>
    /// <remarks>
    /// **THE FILTER IS A PARAMETER BECAUSE THE DEFAULT WAS WRONG SOMEWHERE THAT
    /// MATTERED** (work instruction 040). Skipping it selects the mode's default
    /// (p. 19-11), and on 2026-08-28 that put the operator on a window far
    /// narrower than the FT8 block, on a correctly tuned radio, for an hour.
    /// **Choosing a slot is still not knowing a width**: what the slot opens onto
    /// is whatever the operator configured, so it is read back.
    /// </remarks>
    Task<RigWriteResult> SetModeAsync(
        Civ.CivMode mode, bool dataMode, byte? filterSlot = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Set one documented setting, and read it back (HM-DEC-084).
    /// </summary>
    /// <param name="write">Which setting, with its citation.</param>
    /// <param name="value">The value, in the units the write's note describes.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>What became of it. Never throws.</returns>
    /// <remarks>
    /// **READ BEFORE WRITE, READ BACK AFTER.** A write the radio acknowledged is
    /// not a write that took effect, and the read-back is what tells them apart.
    /// A write that cannot be confirmed reports as unconfirmed and never as done.
    /// </remarks>
    Task<RigWriteResult> SetSettingAsync(
        CivWrite write, int value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hand one keyer message to the radio (HM-DEC-059).
    /// </summary>
    /// <param name="message">
    /// Up to thirty characters from the keyer's own character set. The caller
    /// has already cleaned and split it; this sends exactly what it is given.
    /// </param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>
    /// True when the radio acknowledged it. Never throws: a radio that stopped
    /// answering is a condition rather than an error.
    /// </returns>
    /// <remarks>
    /// THE ONE CALL IN THIS INTERFACE THAT PUTS A SIGNAL ON THE AIR (§0.2). It
    /// is reached only through <see cref="Cw.ICwSender"/>, which checks the
    /// transmit guard and the break-in precondition first, and nothing here
    /// checks them again: one gate, in one place, that every path goes through.
    /// </remarks>
    Task<bool> SendCwAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop a keyer message in progress, on this thread, awaiting nothing.
    /// </summary>
    /// <remarks>
    /// COMMAND 17 WITH FF (p. 19-11), written straight at the port rather than
    /// behind the command gate, because a stop that waits its turn behind the
    /// send it is stopping is not a stop (§0.2). Safe when nothing is sending,
    /// safe twice, and it never throws.
    /// </remarks>
    void AbortCw();

    /// <summary>
    /// Raised when the radio volunteers a value without being asked.
    /// </summary>
    /// <remarks>
    /// The IC-7300 broadcasts frequency and mode changes as they happen, which
    /// is better than polling for the same fact in every way: it is instant, it
    /// costs no bus traffic, and it cannot be stale. Anything the radio will
    /// tell Hamlet unprompted is not polled for.
    /// </remarks>
    event EventHandler<RigValuesReportedEventArgs>? ValuesReported;
}

/// <summary>Payload for <see cref="IRig.ValuesReported"/>.</summary>
public sealed class RigValuesReportedEventArgs : EventArgs
{
    /// <summary>Creates the payload.</summary>
    /// <param name="values">What the radio volunteered.</param>
    public RigValuesReportedEventArgs(IReadOnlyList<RigValue> values) => Values = values;

    /// <summary>What the radio volunteered.</summary>
    public IReadOnlyList<RigValue> Values { get; }
}

/// <summary>Payload for <see cref="IRig.FrequencyChanged"/>.</summary>
public sealed class FrequencyChangedEventArgs : EventArgs
{
    /// <summary>Creates the payload carrying the new VFO frequency.</summary>
    public FrequencyChangedEventArgs(long frequencyHz) => FrequencyHz = frequencyHz;

    /// <summary>The new VFO frequency in hertz.</summary>
    public long FrequencyHz { get; }
}

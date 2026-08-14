namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// One thing Hamlet can know about the radio's current state.
/// </summary>
/// <remarks>
/// <para>WHY THERE IS A LIST AT ALL. Hamlet used to read frequency and mode and
/// nothing else, so everything on the radio's front panel was invisible to it.
/// The first live connection produced garbage from the CW decoder and took half
/// an hour to diagnose, by asking the operator to walk to the radio and read
/// menu settings out loud: what is the filter set to, what is the ACC output
/// level, is the squelch open. Every one of those is a CI-V read the app could
/// have answered instantly. The filter was wide open, which was the whole
/// problem, and Hamlet had no idea (HM-DEC-050).</para>
/// <para>The order here is the order the diagnostics screen shows them, which
/// is roughly the order somebody would ask about them.</para>
/// </remarks>
public enum RigField
{
    /// <summary>The VFO frequency in hertz.</summary>
    Frequency,

    /// <summary>Which mode the radio is in: CW, USB, AM and the rest.</summary>
    Mode,

    /// <summary>
    /// Whether the mode's data variant is on, which is the difference between
    /// USB and USB-D.
    /// </summary>
    /// <remarks>
    /// A separate field because the radio treats it as one: the mode read gives
    /// USB either way, and only command 26 says which of the two the radio is
    /// actually in (p. 19-11). It matters because they are different radios to
    /// the operator, one with the microphone live and one routing the computer's
    /// audio, and it is the difference between hearing FT8 and hearing nothing
    /// useful (HM-DEC-056).
    /// </remarks>
    DataMode,

    /// <summary>Which of the three IF filters is selected: FIL1, FIL2 or FIL3.</summary>
    FilterSelection,

    /// <summary>
    /// How wide that filter actually is, in hertz.
    /// </summary>
    /// <remarks>
    /// THE ONE THAT COST AN EVENING. A filter designator says which of three
    /// slots is in use and nothing about how wide it is, because every slot is
    /// adjustable. This is the number that says whether three signals are
    /// landing in the decoder at once.
    /// </remarks>
    FilterBandwidth,

    /// <summary>The S-meter, live.</summary>
    SMeter,

    /// <summary>Whether the radio is receiving or transmitting.</summary>
    TransmitStatus,

    /// <summary>Whether the front end is overloading.</summary>
    Overflow,

    /// <summary>Transmit power.</summary>
    RfPower,

    /// <summary>Receiver gain.</summary>
    RfGain,

    /// <summary>The level a signal has to reach before the audio opens.</summary>
    Squelch,

    /// <summary>Whether the squelch is open right now, letting audio through.</summary>
    SquelchStatus,

    /// <summary>How quickly the receiver's gain follows a signal.</summary>
    Agc,

    /// <summary>The receive preamplifier, off or one of two stages.</summary>
    Preamp,

    /// <summary>The receive attenuator.</summary>
    Attenuator,

    /// <summary>Whether the noise blanker is on.</summary>
    NoiseBlanker,

    /// <summary>How hard the noise blanker is working.</summary>
    NoiseBlankerLevel,

    /// <summary>Whether noise reduction is on.</summary>
    NoiseReduction,

    /// <summary>How hard noise reduction is working.</summary>
    NoiseReductionLevel,

    /// <summary>Whether the automatic notch is on.</summary>
    AutoNotch,

    /// <summary>Whether the manual notch is on.</summary>
    ManualNotch,

    /// <summary>Break-in: off, semi, or full.</summary>
    BreakIn,

    /// <summary>How fast the radio's own keyer sends, in words a minute.</summary>
    KeyerSpeed,

    /// <summary>The pitch a CW signal is tuned to produce.</summary>
    CwPitch,

    /// <summary>Whether the audio sent to the computer is AF or IF.</summary>
    AccUsbOutputSelect,

    /// <summary>How loud the audio sent to the computer is.</summary>
    AccUsbAfLevel,

    /// <summary>Whether the squelch gates the audio sent to the computer.</summary>
    AccUsbSquelch,

    /// <summary>Whether the split function is on.</summary>
    Split,

    /// <summary>Whether the spectrum scope is switched on.</summary>
    ScopeOn,

    /// <summary>
    /// Whether the radio is sending its scope data to the computer.
    /// </summary>
    /// <remarks>
    /// Separate from the scope being on, and both are needed before a single
    /// sweep arrives (p. 19-7). The output setting also depends on two radio
    /// menu settings Hamlet has no command for, which is why the app reads these
    /// and says what is missing rather than trying to fix it (HM-DEC-062).
    /// </remarks>
    ScopeOutput,

    /// <summary>Which VFO is selected, A or B.</summary>
    Vfo,
}

/// <summary>How much Hamlet knows about one field.</summary>
/// <remarks>
/// FOUR STATES, AND THE POINT IS THAT NONE OF THEM IS A NUMBER (§0.0). A
/// default that quietly stands in for a real reading is the confident guess
/// HM-DEC-009 forbids, and an S-meter is where it would be easiest to slip: a
/// needle resting at zero looks exactly like a measurement of a quiet band.
/// </remarks>
public enum RigValueState
{
    /// <summary>Never read, or the read failed. Not zero, not a default.</summary>
    Unknown,

    /// <summary>Read from the radio, with the time it was read.</summary>
    Known,

    /// <summary>
    /// This radio does not have this. Different from unknown: nothing will ever
    /// arrive, so the UI can stop offering it rather than waiting (HM-DEC-030).
    /// </summary>
    Unsupported,

    /// <summary>
    /// The radio may well have it and the manual documents no command for
    /// reading it. Different again, because the gap is in Hamlet's knowledge
    /// rather than in the radio, and inventing a command byte to close it is
    /// exactly what §4 forbids.
    /// </summary>
    Undocumented,
}

using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Rig;

namespace Hamlet.RadioEngine.Cw;

/// <summary>
/// **ONE WIRING FOR EVERY LISTENER** (work instruction 542, HM-DEC-246): the detector, the sender's window, the gate and
/// the reader, wired to each other exactly as the app wires them, for the app, the scoreboard and the scan's ear alike.
/// </summary>
/// <remarks>
/// <para>**TWICE A LISTENER BUILT ITS OWN AND MISSED A WIRE**: a bench helper that never told the detector what it printed
/// never opened the sender's window, and the scan's ear built its own chain. A wire written once cannot be forgotten in a
/// second place.</para>
/// <para>**THE APP'S WIRING IS THE TRUTH**, moved here from <c>MainWindowViewModel</c> unchanged:</para>
/// <list type="bullet">
/// <item>the decoder reads the detector's marks (<see cref="CwDecoder.DetectorMarks"/>) into its gate and reader;</item>
/// <item>the detector follows what the gate prints and what waits to print (<see cref="CwEnvelopeDetector.PrintedPitch"/>,
/// <see cref="CwEnvelopeDetector.WaitingPitch"/>), which is what opens the sender's own window;</item>
/// <item>the decoder is handed the detector's keying, pitch and blocks, each behind its own switch, off as in the app;</item>
/// <item>**THE DECODER HEARS EACH CHUNK BEFORE THE DETECTOR DOES.** The app subscribes the decoder to the audio first, so the
/// gate takes the marks the detector had called up to the chunk before; <see cref="Listen"/> and <see cref="Process"/>
/// keep that order;</item>
/// <item>the passband is the radio's own CW pitch plus and minus half its filter, in CW or CW-R, and the whole audio band in
/// any other mode or where either is unread (<see cref="Passband"/>).</item>
/// </list>
/// </remarks>
public sealed class CwChain : IDisposable
{
    /// <summary>Builds a chain: a decoder and a detector, wired as the app wires them.</summary>
    /// <param name="sampleRate">The audio's rate.</param>
    /// <param name="cwPitchHz">The decoder's starting pitch.</param>
    /// <param name="secondReader">Whether the decoder runs its second reader, as the app's does.</param>
    public CwChain(int sampleRate, double cwPitchHz, bool secondReader = true)
    {
        Decoder = new CwDecoder(sampleRate, cwPitchHz, secondReader: secondReader);
        Detector = new CwEnvelopeDetector(sampleRate);
        Wire(Decoder, Detector);
    }

    /// <summary>The decoder: its gate and reader print what the terminal shows.</summary>
    public CwDecoder Decoder { get; }

    /// <summary>The detector: it finds the marks.</summary>
    public CwEnvelopeDetector Detector { get; }

    /// <summary>
    /// Wire a decoder and a detector to each other, exactly as the app does. Every listener that builds the two itself
    /// calls this, and nothing else wires them.
    /// </summary>
    /// <param name="decoder">The decoder.</param>
    /// <param name="detector">The detector.</param>
    public static void Wire(CwDecoder decoder, CwEnvelopeDetector detector)
    {
        ArgumentNullException.ThrowIfNull(decoder);
        ArgumentNullException.ThrowIfNull(detector);

        // **NO DETECTION, NO LETTERS** (work instruction 485, R97, HM-DEC-190), behind its switch.
        decoder.KeyingGate = () => detector.Reading.Keying;

        // **AND IT LISTENS WHERE THE DETECTOR HEARS** (work instructions 486 and 488, HM-DEC-193), behind its switch.
        decoder.DetectorPitch = () => PitchForTheDecoder(detector.Reading);

        // **AND THE READING FOLLOWS WHAT THE TERMINAL PRINTS** (work instruction 519, HM-DEC-223).
        detector.PrintedPitch = () => decoder.RunsPrintingHz;

        // **AND THE LIGHT CLAIMS NO MORE THAN THE PRINTER** (work instruction 535, HM-DEC-239).
        detector.WaitingPitch = () => decoder.RunsWaitingHz;

        // **AND A LETTER NEEDS BLOCKS** (work instruction 487, R99), behind its switch.
        decoder.DetectorBlocks = detector.BlocksBetween;

        // **A CHARACTER IS A RUN OF MARKS THAT AGREE** (work instruction 490, HM-DEC-195): the gate and reader read the marks.
        decoder.DetectorMarks = detector.MarksSince;
    }

    /// <summary>
    /// The pitch the decoder's second rung is fed: the reading's own pitch while it says keying, NaN otherwise (work
    /// instruction 488).
    /// </summary>
    /// <param name="reading">The detector's last reading.</param>
    /// <returns>A pitch in hertz, or NaN.</returns>
    public static double PitchForTheDecoder(CwEnvelopeReading reading) =>
        reading.Keying && double.IsFinite(reading.PitchHz) && reading.PitchHz > 0 ? reading.PitchHz : double.NaN;

    /// <summary>
    /// **THE PASSBAND IS THE RADIO'S, READ, OR THE WHOLE BAND** (work instruction 476): in CW or CW-R the radio's own CW
    /// pitch and filter width; in any other mode, or where either is unread, nulls, and the detector sums the whole band.
    /// </summary>
    /// <param name="state">What Hamlet knows about the radio.</param>
    /// <returns>The pitch and width to hand <see cref="CwEnvelopeDetector.SetPassband"/>.</returns>
    public static (double? PitchHz, double? WidthHz) Passband(RigState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var cw = state[RigField.Mode] is { IsKnown: true, Number: { } mode } && CivValues.IsCw((CivMode)(int)mode);
        var pitch = cw && state[RigField.CwPitch] is { IsKnown: true, Number: { } hz } ? hz : (double?)null;
        var width = cw ? state.FilterBandwidthHz : null;

        return (pitch, width);
    }

    /// <summary>Set the detector's passband from what Hamlet knows about the radio, as the app does on every scope tick.</summary>
    /// <param name="state">What Hamlet knows about the radio.</param>
    public void SetPassband(RigState state)
    {
        var (pitch, width) = Passband(state);

        Detector.SetPassband(pitch, width);
    }

    /// <summary>Listen to a source: the decoder first, then the detector, as the app subscribes them.</summary>
    /// <param name="source">The audio, or null to stop listening.</param>
    public void Listen(IAudioSource? source)
    {
        Decoder.Listen(source);
        Detector.Listen(source);
    }

    /// <summary>Hear one chunk offline, in the order a live source delivers it: the decoder, then the detector.</summary>
    /// <param name="chunk">The chunk.</param>
    public void Process(in AudioChunk chunk)
    {
        Decoder.Process(chunk);
        Detector.Process(chunk.Samples);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Decoder.Listen(null);
        Detector.Listen(null);
    }
}

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
/// <item>the keying, pitch and blocks it used to hand the old decoder came out with it (work instruction 545);</item>
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
    /// <remarks>
    /// **NO PITCH**: the detector finds the shape at every pitch the filter passes, and the decoder that took a starting
    /// pitch and a second reader came out with the old decoder (work instruction 545).
    /// </remarks>
    public CwChain(int sampleRate)
    {
        Decoder = new CwDecoder(sampleRate);
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
    /// <remarks>
    /// The keying gate, the detector's pitch and the block rule it also handed the old decoder, each behind a switch that
    /// was off, came out with it (work instruction 545).
    /// </remarks>
    public static void Wire(CwDecoder decoder, CwEnvelopeDetector detector)
    {
        ArgumentNullException.ThrowIfNull(decoder);
        ArgumentNullException.ThrowIfNull(detector);

        // **THE READING FOLLOWS WHAT THE TERMINAL PRINTS** (work instruction 519, HM-DEC-223).
        detector.PrintedPitch = () => decoder.RunsPrintingHz;

        // **AND THE LIGHT CLAIMS NO MORE THAN THE PRINTER** (work instruction 535, HM-DEC-239).
        detector.WaitingPitch = () => decoder.RunsWaitingHz;

        // **A CHARACTER IS A RUN OF MARKS THAT AGREE** (work instruction 490, HM-DEC-195): the gate and reader read the marks.
        decoder.DetectorMarks = detector.MarksSince;
    }
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

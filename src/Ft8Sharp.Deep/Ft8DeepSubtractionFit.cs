using System;

namespace Ft8Sharp.Deep;

/// <summary>
/// <b>What one subtraction actually fitted and removed — the gain, the carrier phase, the place it
/// settled on, the energy it took out, and how many symbols of the frame lay inside the slot.</b>
/// </summary>
/// <param name="Gain">
/// The amplitude the transmission arrived at, against <c>Ft8Waveform.Synthesize</c>'s unit-amplitude
/// rendering. <b>Not decibels and not a ratio against anything else in the slot.</b>
/// </param>
/// <param name="PhaseRadians">
/// The carrier phase the transmission arrived at, in radians in <c>[-pi, pi]</c>. <b>Nothing in the
/// tree reported this before</b>: <c>Ft8DeepSnrEstimate</c> carries a ratio, a symbol count and two
/// place adjustments, and no phase at all.
/// </param>
/// <param name="StartSeconds">
/// Where the fit put the transmission's first symbol, in seconds from the start of the slot, after
/// its own search. <b>The start of the signal, not a candidate's nominal time</b> — the two differ
/// by <see cref="Ft8DeepSlotDecoder.CandidateTimeBiasSeconds"/>.
/// </param>
/// <param name="BaseFrequencyHz">The frequency the fit put the lowest of the eight tones at.</param>
/// <param name="DecibelsRemoved">
/// <b>10 log10(energy before / energy after) over the samples the transmission occupies.</b> A
/// measurement and <b>never a threshold</b>: no code in this library compares it against a bound,
/// because a bound picked on the night the fit was written would be a target written after the fact.
/// <para>
/// <b>AND IT IS NOT A CANCELLATION DEPTH ON A SLOT THAT HAS ANYTHING ELSE IN IT.</b> The energy
/// after includes the noise and every other station, so this figure is bounded above by the
/// transmission's own signal-to-noise ratio over the frame however perfect the fit is: unit 253
/// measured 286 dB on a noiseless slot and <b>7.06 dB on the same transmission with noise
/// added</b>, from a fit that removed the message completely in both cases. A reader comparing two
/// of these numbers is comparing two slots and not two fits.
/// </para>
/// <para>
/// <b>AND IT DOES NOT ANSWER "IS THE MESSAGE GONE".</b> <c>Ft8SoftSymbols.Normalise</c> normalises
/// a candidate's ratios, so the decoder is scale-invariant: in a slot holding nothing else, a
/// residue 42.82 dB down still decoded. What decides the question is whether the residue is below
/// whatever else is in the slot, which is why <c>Ft8DeepSubtractionTests</c> asserts the decode and
/// prints the decibels rather than the other way round.
/// </para>
/// </param>
/// <param name="Symbols">
/// How many of the 79 symbols had their samples inside the slot. A fit over 40 of them is a
/// different quantity from one over 79 and a caller cannot tell from the gain.
/// </param>
/// <param name="TimeSearchSamplesMoved">
/// How far the fit's own search moved the start from the place it was given, in samples. <b>A
/// distribution of these piling up against <c>Ft8DeepSubtractionSettings.TimeSearchSamples</c> says
/// the extent is too narrow.</b>
/// </param>
/// <param name="FrequencySearchHzMoved">The same, in hertz.</param>
/// <remarks>
/// <para>
/// <b>A NOT-FITTED RESULT IS A REAL ANSWER AND CARRIES NO NUMBERS.</b> <see cref="NotFitted"/> has
/// <see cref="double.NaN"/> in every measured field rather than a zero, for the reason
/// <c>Ft8DeepSnrEstimate</c> gives at length: a substituted number is indistinguishable downstream
/// from a measured one, and a gain of zero reads as *the transmission was not there* rather than as
/// *nothing was measured*.
/// </para>
/// </remarks>
public readonly record struct Ft8DeepSubtractionFit(
    double Gain,
    double PhaseRadians,
    double StartSeconds,
    double BaseFrequencyHz,
    double DecibelsRemoved,
    int Symbols,
    int TimeSearchSamplesMoved,
    double FrequencySearchHzMoved)
{
    /// <summary>Nothing was fitted and nothing was subtracted.</summary>
    public static Ft8DeepSubtractionFit NotFitted { get; } =
        new(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, 0, 0, double.NaN);

    /// <summary>True where a transmission was fitted and removed from the buffer.</summary>
    public bool IsFitted => !double.IsNaN(Gain);

    /// <summary>The one line a report prints this as.</summary>
    public override string ToString() =>
        IsFitted
            ? $"gain {Gain:F5} phase {PhaseRadians,7:F4} rad at {BaseFrequencyHz:F3} Hz "
                + $"{StartSeconds:F5} s, {DecibelsRemoved:F2} dB removed over {Symbols} symbols "
                + $"(search moved {TimeSearchSamplesMoved} samples, {FrequencySearchHzMoved:F3} Hz)"
            : "NOT FITTED - nothing was subtracted and there are no numbers to report";
}

using Ft8Sharp.Dsp;
using Ft8Sharp.Encode;
using Ft8Sharp.Tests.Encode;

namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// Builds slots of audio with transmissions in them at places the test chooses, and holds the truth
/// about where it put them — <b>on the assertion side of the line and nowhere near the search.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>THE SEARCH IS GIVEN THE SAMPLES AND THE GEOMETRY AND NOTHING ELSE.</b> Everything in this file
/// is the test's own knowledge: the base frequency was chosen here and handed to the synthesizer,
/// the sample offset was chosen here and used to place the signal, and both are compared against
/// what the search answered <em>after</em> it has answered. Neither ever appears in a call to
/// <see cref="Ft8SyncSearch"/>, which has no parameter that could carry one.
/// </para>
/// <para>
/// <b><see cref="ToneRecovery.AlignmentFor"/> is not used here and must not be.</b> That helper
/// computes, from an offset the caller already knows, which block and sub-offset a symbol lands in —
/// which is exactly the knowledge the search is supposed to do without. Unit 213's measurements were
/// taken at positions it was handed; tonight's are not.
/// </para>
/// <para>
/// <b>Signals are summed into a slot built here</b>, as unit 213 did it and for the same reason:
/// <c>Ft8Waveform.SynthesizeSlot</c> places a signal at <c>PaddingSampleCount</c> and nowhere else,
/// so arbitrary offsets are built in the test project and step 3's proven code is not changed to
/// make tonight easier.
/// </para>
/// </remarks>
internal static class SearchFixture
{
    /// <summary>One transmission the test put into a slot, and where it put it.</summary>
    /// <param name="Label">Which message of the corpus it is.</param>
    /// <param name="BaseFrequencyHz">The frequency of its lowest tone, as handed to the synthesizer.</param>
    /// <param name="OffsetSamples">Where its first sample was written in the slot.</param>
    internal sealed record Truth(string Label, double BaseFrequencyHz, int OffsetSamples)
    {
        /// <summary>The time its first symbol begins, in seconds from the start of the slot.</summary>
        internal double TimeSeconds(int sampleRate) => (double)OffsetSamples / sampleRate;
    }

    /// <summary>An empty slot of the length <c>Ft8Waveform</c> defines for one transmission period.</summary>
    internal static float[] EmptySlot(int sampleRate) =>
        new float[Ft8Waveform.SlotSampleCount(sampleRate)];

    /// <summary>
    /// Sums one transmission into a slot at a chosen frequency and a chosen sample offset.
    /// </summary>
    /// <remarks>
    /// <b>Summed, not copied.</b> Twenty stations sharing a slot add to one another, which is what a
    /// receiver actually gets; overwriting would build a fixture easier than the air.
    /// </remarks>
    internal static Truth Place(
        float[] slot,
        int sampleRate,
        EncodeCorpus.Entry entry,
        double baseFrequencyHz,
        int offsetSamples)
    {
        var symbols = Ft8SymbolEncoder.Encode(entry.Message);
        var signal = Ft8Waveform.Synthesize(symbols, sampleRate, (float)baseFrequencyHz);

        if (offsetSamples < 0 || offsetSamples + signal.Length > slot.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offsetSamples),
                offsetSamples,
                $"A transmission of {signal.Length} samples does not fit in a slot of {slot.Length} "
                + $"at offset {offsetSamples}. Placing part of it outside the slot would measure the "
                + "search against a signal that is not all there.");
        }

        for (var i = 0; i < signal.Length; i++)
        {
            slot[offsetSamples + i] += signal[i];
        }

        return new Truth(entry.Label, baseFrequencyHz, offsetSamples);
    }

    /// <summary>A slot holding exactly one transmission, and the truth about where it is.</summary>
    internal static (float[] Slot, Truth Where) OneSignal(
        int sampleRate, EncodeCorpus.Entry entry, double baseFrequencyHz, int offsetSamples)
    {
        var slot = EmptySlot(sampleRate);
        return (slot, Place(slot, sampleRate, entry, baseFrequencyHz, offsetSamples));
    }

    /// <summary>
    /// The mean square of the transmissions in a slot, for turning a wanted signal-to-noise ratio
    /// into a noise amplitude. <b>Measured over the whole slot</b>, including the silence around a
    /// short transmission, so it is the power a receiver actually sees rather than the power inside
    /// the burst.
    /// </summary>
    internal static double SlotPower(ReadOnlySpan<float> slot) => SignalToNoise.MeanSquare(slot);

    /// <summary>
    /// The power of one transmission alone, measured over the samples it actually occupies. This is
    /// the number the published signal-to-noise convention is quoted against.
    /// </summary>
    internal static double TransmissionPower(int sampleRate, EncodeCorpus.Entry entry, double baseFrequencyHz)
    {
        var symbols = Ft8SymbolEncoder.Encode(entry.Message);
        var signal = Ft8Waveform.Synthesize(symbols, sampleRate, (float)baseFrequencyHz);
        return SignalToNoise.MeanSquare(signal);
    }
}

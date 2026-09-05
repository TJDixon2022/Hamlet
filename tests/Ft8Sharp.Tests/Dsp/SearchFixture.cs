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
    /// Sums one transmission into a slot at a chosen frequency, a chosen sample offset and a chosen
    /// amplitude.
    /// </summary>
    /// <param name="slot">The slot to sum into.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="entry">Which message of the corpus to place.</param>
    /// <param name="baseFrequencyHz">The frequency of its lowest tone.</param>
    /// <param name="offsetSamples">Where its first sample goes.</param>
    /// <param name="amplitude">
    /// <b>What to scale the synthesizer's unit-amplitude output by before summing. One by default,
    /// and one is bit-identical to what this method did before the parameter existed.</b>
    /// </param>
    /// <remarks>
    /// <para>
    /// <b>Summed, not copied.</b> Twenty stations sharing a slot add to one another, which is what a
    /// receiver actually gets; overwriting would build a fixture easier than the air.
    /// </para>
    /// <para>
    /// <b>THE AMPLITUDE WAS ADDED BY UNIT 253 AND IT IS OPTIONAL FOR A REASON.</b> Every recorded
    /// figure in this phase was taken through a call site that does not pass it —
    /// <see cref="OneSignal"/>, <see cref="ManySignals"/>, and the passband and slot-decoder tests
    /// through them. <c>(float)(1.0 * signal[i])</c> is <c>signal[i]</c> exactly in IEEE 754, so the
    /// default path is bit-identical rather than nearly so, and
    /// <c>Ft8Unit253MaskingSurveyTests.UnitAmplitudePlacesBitIdenticalSamplesToTheSynthesizersOwn</c>
    /// asserts that sample-for-sample rather than leaving it to this paragraph.
    /// </para>
    /// <para>
    /// <b>Why a fixture needs it at all.</b> Until unit 253 this project had only ever measured a
    /// slot containing exactly one transmission, so every station in every fixture was as loud as
    /// every other. Masking is a question about level differences and cannot be asked without one.
    /// </para>
    /// </remarks>
    internal static Truth Place(
        float[] slot,
        int sampleRate,
        EncodeCorpus.Entry entry,
        double baseFrequencyHz,
        int offsetSamples,
        double amplitude = 1.0)
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
            slot[offsetSamples + i] += (float)(amplitude * signal[i]);
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
    /// One slot carrying <paramref name="count"/> different messages at different frequencies across
    /// the passband and at different time offsets — the real case, and the one a 3 kHz slice of 20 m
    /// actually presents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Nothing in it is aligned to anything else.</b> The frequencies are spread across the
    /// passband and every one of them carries a different fraction of a bin, so no two sit on the
    /// grid the same way; the offsets rotate through five values on and off the block and sub-block
    /// grids, because stations do not start together and a fixture where all twenty begin at the
    /// same sample is easier than the air.
    /// </para>
    /// <para>
    /// <b>The frequencies stop below the top of the passband on purpose.</b> A candidate spans eight
    /// tones, so a transmission whose lowest tone sits in one of the top seven bins has nowhere for
    /// its highest to go. That is a property of the passband, not of the search.
    /// </para>
    /// </remarks>
    internal static (float[] Slot, IReadOnlyList<Truth> Truths) ManySignals(
        int sampleRate,
        IReadOnlyList<EncodeCorpus.Entry> corpus,
        int count,
        double lowestHz,
        double spacingHz,
        double binHz)
    {
        var slot = EmptySlot(sampleRate);
        var truths = new List<Truth>(count);

        // Five offsets: one on the block grid, one on the sub-block grid, three on neither.
        var offsets = new[] { 0, 960 * 5, 1920 * 2, 3701, 12345 };

        for (var i = 0; i < count; i++)
        {
            // Every signal at a different fraction of a bin, cycling through quarters, so the
            // easy case and the hard one are both in the same slot rather than in different runs.
            var frequency = lowestHz + (i * spacingHz) + (i % 4 * (binHz / 4));
            truths.Add(Place(slot, sampleRate, corpus[i % corpus.Count], frequency, offsets[i % offsets.Length]));
        }

        return (slot, truths);
    }

    /// <summary>
    /// The mean square of the transmissions in a slot, for turning a wanted signal-to-noise ratio
    /// into a noise amplitude. <b>Measured over the whole slot</b>, including the silence around a
    /// short transmission, so it is the power a receiver actually sees rather than the power inside
    /// the burst.
    /// </summary>
    internal static double SlotPower(ReadOnlySpan<float> slot) => SignalToNoise.MeanSquare(slot);

    /// <summary>
    /// Adds noise to a copy of a slot and <b>reports the power it actually delivered</b> rather than
    /// the power it was asked for.
    /// </summary>
    /// <remarks>
    /// <see cref="GaussianNoise.AddedTo"/> keeps its noise to itself, so the ratio a test quotes
    /// would be the one it requested. A finite draw is not its own standard deviation, so the noise
    /// is drawn here, measured, and then summed — and the ratio in the report is the delivered one.
    /// </remarks>
    internal static float[] AddNoise(
        ReadOnlySpan<float> slot,
        GaussianNoise noise,
        double rootMeanSquare,
        out double deliveredNoisePower)
    {
        var drawn = noise.Block(slot.Length, rootMeanSquare);
        deliveredNoisePower = SignalToNoise.MeanSquare(drawn);

        var mixed = new float[slot.Length];
        for (var i = 0; i < slot.Length; i++)
        {
            mixed[i] = slot[i] + drawn[i];
        }

        return mixed;
    }

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

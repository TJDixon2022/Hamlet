namespace Hamlet.RadioEngine.Transmit;

/// <summary>What a sink did with the samples it was handed.</summary>
/// <param name="SamplesPlayed">
/// How many of them went out. **A number, not a flag, because the interesting
/// failure is the partial one**: a sink that played four seconds of a twelve
/// second transmission and returned without complaint has failed, and a boolean
/// cannot say so.
/// </param>
/// <param name="Took">
/// How long the playing took, measured by whoever did it. The sink is the only
/// thing in a position to know - the caller cannot read a clock without becoming
/// something that watches one.
/// </param>
public readonly record struct PlayedAudio(int SamplesPlayed, TimeSpan Took);

/// <summary>
/// Where a transmission's samples go to be played.
/// </summary>
/// <remarks>
/// <para>**ONE METHOD, BECAUSE THE SEQUENCE ASKS ONE QUESTION.** It has a slot
/// of audio and a rate, and it needs to know when the audio has finished and how
/// much of it went out. Everything else about playing sound - which device, what
/// format, how deep the buffer, what the endpoint negotiated - belongs to the
/// implementation and to nothing above it.</para>
/// <para>**NARROW ENOUGH THAT A REAL RENDER PATH AND A FAKE BOTH FIT.** A WASAPI
/// implementation opens a render client, writes the buffer, waits for it to
/// drain and returns what it wrote; a test's fake returns immediately, or throws,
/// or says it played half. Both are the same shape, which is the point: the
/// sequence around this interface can be proved against the second before the
/// first exists.</para>
/// <para>**IT DOES NOT KEY ANYTHING AND IT KNOWS NOTHING ABOUT A RADIO.** No
/// PTT, no serial port, no rig. The one thing that holds those two ends together
/// is <see cref="Ft8TransmitSequence"/>, where the unkey is guaranteed.</para>
/// <para>**IT IS IMPLEMENTED FOR A REAL DEVICE BY
/// <c>Hamlet.RadioEngine.Audio.WasapiTransmitSink</c>** (unit 256), which opens a
/// render endpoint the caller names - never the one the machine happens to
/// default to - at that endpoint's own mix format in WASAPI shared mode,
/// converts these floats to whatever it speaks, and **waits for the endpoint to
/// empty before returning**, so <see cref="PlayedAudio.SamplesPlayed"/> is what
/// the card consumed rather than what was handed to it. The other implementation
/// is the tests' <c>FakeTransmitAudioSink</c>, and the sequence around this
/// interface is proved against both.</para>
/// <para>**THE RATE IS NOT NEGOTIABLE FROM THIS SIDE, AND
/// <see cref="EndpointSampleRate"/> IS HOW A CALLER FINDS OUT WHAT IT IS.** The
/// real sink refuses a <c>sampleRate</c> that is not the endpoint's own rather
/// than letting shared-mode WASAPI resample it quietly, so the caller composes at
/// the rate the endpoint declares. That is a fact about sound cards, not a
/// restriction this interface imposes.</para>
/// <para>**THIS PARAGRAPH USED TO END THERE, AND IT INSTRUCTED A CALLER TO DO
/// SOMETHING NO CALLER COULD DO** (work instruction 262). *The caller composes at
/// the rate the endpoint declares* was written before anything on this interface
/// carried that rate, so the one caller in the tree composed at
/// <c>Ft8Waveform.DefaultSampleRate</c> instead - and every send through a real
/// endpoint threw, after the radio had already been keyed. Prose a neighbour
/// falsifies is worse than no prose, so the member was added rather than the
/// sentence softened.</para>
/// </remarks>
public interface ITransmitAudioSink
{
    /// <summary>
    /// The rate this sink will accept, and the rate a caller must compose at.
    /// </summary>
    /// <remarks>
    /// <para>**IT IS THE ENDPOINT'S, NOT A PREFERENCE.** For
    /// <c>WasapiTransmitSink</c> it is the shared-mode mix format the device
    /// declared when it was opened - 48000 Hz on ordinary hardware - and
    /// <see cref="PlayAsync"/> throws on any other number. A caller that hands
    /// over something else has not made a slightly worse choice; it has made a
    /// transmission that cannot go out.</para>
    /// <para>**IT IS KNOWN AS SOON AS THE SINK EXISTS**, which is what lets a
    /// caller refuse an unusable device at the moment it is chosen rather than
    /// after the radio is keyed. `Ft8TransmitSequence` writes PTT on before it
    /// reaches the sink, so a rate discovered inside <see cref="PlayAsync"/> is
    /// discovered one frame too late.</para>
    /// <para>**NOTHING HERE PROMISES FT8 CAN BE BUILT AT IT.** A sound card may
    /// declare a rate at which a channel symbol is not a whole number of samples;
    /// <c>Ft8Composer.RateIsUsable</c> is what answers that, and it is the
    /// caller's to ask.</para>
    /// </remarks>
    int EndpointSampleRate { get; }

    /// <summary>Play these samples, and return when they have been played.</summary>
    /// <param name="samples">The audio, in the range -1 to +1.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="cancellationToken">
    /// Stops the playing. **Cancelling this does not unkey the radio** - the
    /// sequence does that, and it does it whether this returns, throws or is
    /// cancelled.
    /// </param>
    /// <returns>How much went out, and how long it took.</returns>
    Task<PlayedAudio> PlayAsync(
        ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken);
}

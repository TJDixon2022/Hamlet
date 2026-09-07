using Hamlet.RadioEngine.Transmit;

namespace Hamlet.RadioEngine.Tests.Transmit;

/// <summary>
/// An <see cref="ITransmitAudioSink"/> that plays nothing and can be told to
/// fail in each of the ways a real one can.
/// </summary>
/// <remarks>
/// <para>**IT OPENS NO DEVICE AND MAKES NO SOUND** (work instruction 255). The
/// render implementation is the next unit's; what is being proved here is the
/// sequence around the sink, and every way a sink can end - returning, throwing,
/// being cancelled, and the quiet one, returning having played less than it was
/// given.</para>
/// <para>It records what it was handed so a test can assert the sink was never
/// touched, which is what a licence refusal has to prove.</para>
/// <para>**AND SINCE UNIT 262 IT CAN DECLARE A RATE AND REFUSE EVERY OTHER ONE**,
/// which is the last way a real sink can fail that this could not - see
/// <see cref="DeclaredSampleRate"/>.</para>
/// </remarks>
internal sealed class FakeTransmitAudioSink : ITransmitAudioSink
{
    /// <summary>How many times something asked it to play.</summary>
    public int TimesCalled { get; private set; }

    /// <summary>How many samples it was handed, last time.</summary>
    public int SamplesHandedOver { get; private set; }

    /// <summary>What rate it was asked for, last time.</summary>
    public int RateAskedFor { get; private set; }

    /// <summary>True where nothing ever reached it.</summary>
    public bool WasNeverTouched => TimesCalled == 0;

    /// <summary>Throw this instead of playing.</summary>
    public Exception? Throws { get; set; }

    /// <summary>Report having played only this many samples.</summary>
    public int? PlaysOnly { get; set; }

    /// <summary>
    /// The rate this endpoint declares, or null to take whatever it is handed.
    /// </summary>
    /// <remarks>
    /// <para>**THE ONE BEHAVIOUR THIS FAKE LACKED, AND WHY THAT MATTERED** (work
    /// instruction 262, task 2). <c>WasapiTransmitSink</c> has a rate it will
    /// accept - the endpoint's shared-mode mix format - and throws on anything
    /// else, rather than letting shared-mode WASAPI resample quietly. This fake
    /// recorded the rate it was asked for and played anyway, which made it **more
    /// permissive than the thing it stands for**: every test that handed it 12000
    /// Hz passed, against a real sink that would have refused, and sixteen units
    /// of green tests sat over a send path that could not transmit on any endpoint
    /// this machine has.</para>
    /// <para>**NULL BY DEFAULT, SO NO EXISTING TEST CHANGES MEANING.** A fake that
    /// suddenly refused would be a different fixture wearing the same name.
    /// Setting this is how a test says *stand for a real endpoint*.</para>
    /// </remarks>
    public int? DeclaredSampleRate { get; set; }

    /// <inheritdoc/>
    /// <remarks>
    /// **12000 WHERE NOTHING WAS DECLARED**, which is the rate every test written
    /// before unit 262 composes at, so those tests keep the meaning they had.
    /// </remarks>
    public int EndpointSampleRate => DeclaredSampleRate ?? Ft8Composer.DefaultSampleRate;

    /// <summary>How long it claims the playing took.</summary>
    public TimeSpan Took { get; set; } = TimeSpan.FromSeconds(12.64);

    /// <inheritdoc/>
    public Task<PlayedAudio> PlayAsync(
        ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
    {
        TimesCalled++;
        SamplesHandedOver = samples.Length;
        RateAskedFor = sampleRate;

        if (DeclaredSampleRate is int declared && sampleRate != declared)
        {
            // **THE SAME SHAPE OF MESSAGE AS `WasapiTransmitSink.cs:306-309`**,
            // word for word, because a fake whose refusal reads differently sends
            // whoever reads the red looking in the wrong place.
            throw new InvalidOperationException(
                $"the samples are at {sampleRate} Hz and the endpoint speaks "
                + $"{declared} Hz. Nothing is played rather than a rate being "
                + "silently changed on the way out.");
        }

        if (Throws is not null)
        {
            throw Throws;
        }

        return Task.FromResult(new PlayedAudio(PlaysOnly ?? samples.Length, Took));
    }
}

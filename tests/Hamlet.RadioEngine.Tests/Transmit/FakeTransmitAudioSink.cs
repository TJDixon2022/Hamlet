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

    /// <summary>How long it claims the playing took.</summary>
    public TimeSpan Took { get; set; } = TimeSpan.FromSeconds(12.64);

    /// <inheritdoc/>
    public Task<PlayedAudio> PlayAsync(
        ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
    {
        TimesCalled++;
        SamplesHandedOver = samples.Length;
        RateAskedFor = sampleRate;

        if (Throws is not null)
        {
            throw Throws;
        }

        return Task.FromResult(new PlayedAudio(PlaysOnly ?? samples.Length, Took));
    }
}

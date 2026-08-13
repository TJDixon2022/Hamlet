using NAudio.Wave;

namespace Hamlet.App.Audio;

/// <summary>
/// Feeds a generated buffer to NAudio, once, then reports end of stream.
/// </summary>
/// <remarks>
/// <para>Lives in its own file, and internal rather than private, so the
/// playback path can be tested without a sound device. It used to be a
/// private nested class, which meant the only coverage audio had was that
/// <see cref="Hamlet.RadioEngine.Training.ModeAudio"/> produced buffers of the
/// right length and loudness — nothing ever read one back out through the
/// thing that plays it. That gap is what let a reported playback failure sit
/// outside every test in the suite. The engine follows the same pattern for
/// the same reason (see its <c>InternalsVisibleTo</c>).</para>
/// <para>The copy is a typed <see cref="Span{T}"/> copy. The non-generic
/// <c>Array.Copy</c> it replaces takes <c>Array</c> parameters and checks
/// element types at run time, which is both slower on a path called every few
/// milliseconds by the audio thread and capable of failing at run time on a
/// mistake the compiler could have caught. A span copy cannot raise
/// <see cref="ArrayTypeMismatchException"/> at all: there is no conversion for
/// it to get wrong.</para>
/// </remarks>
internal sealed class FloatSampleProvider : ISampleProvider
{
    private readonly float[] _samples;
    private int _position;

    /// <summary>Wraps a generated buffer.</summary>
    /// <param name="samples">Mono samples in [-1, 1].</param>
    /// <param name="sampleRate">Rate the samples were generated at.</param>
    public FloatSampleProvider(float[] samples, int sampleRate)
    {
        _samples = samples ?? throw new ArgumentNullException(nameof(samples));
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
    }

    /// <inheritdoc/>
    public WaveFormat WaveFormat { get; }

    /// <summary>How many samples are left to hand out.</summary>
    public int Remaining => _samples.Length - _position;

    /// <inheritdoc/>
    /// <remarks>
    /// Returns 0 once exhausted, which is how NAudio learns the sample has
    /// finished. Never returns more than <paramref name="count"/>, and never
    /// writes past <paramref name="offset"/> plus that.
    /// </remarks>
    public int Read(float[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (offset < 0 || count < 0 || offset + count > buffer.Length)
        {
            throw new ArgumentException(
                "the destination buffer is too small for the requested read", nameof(buffer));
        }

        var take = Math.Min(Remaining, count);
        if (take <= 0)
        {
            return 0;
        }

        _samples.AsSpan(_position, take).CopyTo(buffer.AsSpan(offset, take));
        _position += take;
        return take;
    }
}

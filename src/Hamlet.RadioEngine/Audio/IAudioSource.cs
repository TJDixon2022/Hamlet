namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// A run of audio samples and where they sit in the stream.
/// </summary>
/// <remarks>
/// <para>A ref struct for the same reason <c>SpectrumFrame</c> is one: audio
/// arrives continuously, the buffer belongs to the source and is reused on the
/// next callback, and a handler that stashed it would be reading somebody
/// else's samples a moment later. The compiler enforces that here rather than
/// a comment asking nicely.</para>
/// <para><see cref="FirstSampleIndex"/> is the part that makes decoding
/// reproducible. Everything downstream derives elapsed time by counting
/// samples, never by reading a clock, so the same audio decodes to the same
/// text on a fast machine, a slow machine, and a test that pumps ten minutes
/// of signal through in a millisecond (§5).</para>
/// </remarks>
public readonly ref struct AudioChunk
{
    /// <summary>Creates a chunk over a caller-owned buffer.</summary>
    /// <param name="firstSampleIndex">Index of the first sample since the source started.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="samples">Mono samples, nominally in [-1, 1].</param>
    public AudioChunk(long firstSampleIndex, int sampleRate, ReadOnlySpan<float> samples)
    {
        FirstSampleIndex = firstSampleIndex;
        SampleRate = sampleRate;
        Samples = samples;
    }

    /// <summary>How many samples the source produced before this chunk.</summary>
    public long FirstSampleIndex { get; }

    /// <summary>Samples per second.</summary>
    public int SampleRate { get; }

    /// <summary>
    /// Mono samples. Valid only for the duration of the call that delivered
    /// them.
    /// </summary>
    public ReadOnlySpan<float> Samples { get; }

    /// <summary>Where this chunk starts, as time since the source began.</summary>
    public TimeSpan Offset
        => SampleRate <= 0
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds((double)FirstSampleIndex / SampleRate);
}

/// <summary>Receives audio.</summary>
/// <param name="chunk">The samples; valid only for the duration of the call.</param>
/// <remarks>
/// A named delegate rather than <c>EventHandler&lt;T&gt;</c>, because
/// <see cref="AudioChunk"/> is a ref struct and cannot be a generic type
/// argument. That restriction is the same one that keeps the buffer from
/// escaping, so it is a feature here rather than a nuisance.
/// </remarks>
public delegate void AudioChunkHandler(in AudioChunk chunk);

/// <summary>
/// Something producing receive audio: the sound card the radio's USB codec
/// shows up as, the training radio, or a WAV file being replayed.
/// </summary>
/// <remarks>
/// <para>The engine's seam over audio input, built the way <c>ISerialPort</c>
/// was (§6: hand-rolled interfaces, no mocking framework). The real
/// implementation wraps WASAPI through NAudio; tests substitute an in-memory
/// source and never touch a sound card.</para>
/// <para><see cref="IsSimulated"/> carries the same guarantee
/// <c>ISpectrumSource</c> carries (HM-DEC-026): a source either is or is not
/// listening to real radio, it is the only thing that knows, and there is no
/// setter for anything else to get wrong. A decode from synthesized audio must
/// never reach the screen looking like something that was on the air.</para>
/// </remarks>
public interface IAudioSource : IDisposable
{
    /// <summary>What this source is, in words the operator recognizes.</summary>
    string DeviceName { get; }

    /// <summary>Samples per second.</summary>
    int SampleRate { get; }

    /// <summary>
    /// True when these samples are synthesized rather than received off the
    /// air.
    /// </summary>
    /// <remarks>
    /// Get-only, on the source itself, for the reason HM-DEC-026 gives: a flag
    /// the UI keeps beside the source is a flag somebody forgets to set.
    /// </remarks>
    bool IsSimulated { get; }

    /// <summary>True while samples are being produced.</summary>
    bool IsRunning { get; }

    /// <summary>Raised for each run of samples, usually on a background thread.</summary>
    event AudioChunkHandler? SamplesReady;

    /// <summary>Begin producing samples.</summary>
    void Start();

    /// <summary>Stop producing samples.</summary>
    void Stop();
}

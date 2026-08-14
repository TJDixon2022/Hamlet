namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// An <see cref="IAudioSource"/> over samples already in memory: a WAV
/// fixture, a generated signal, or an array a test built by hand.
/// </summary>
/// <remarks>
/// <para>The seam that lets every decoder test run without a sound card
/// (HM-DEC-007). It is also how a captured failure becomes a regression test:
/// the WAV goes in the fixtures folder, this plays it back, and the decode is
/// asserted (§8).</para>
/// <para><see cref="PumpAll"/> and <see cref="PumpOnce"/> deliver samples with
/// no timer involved, so a test can push ten minutes of audio through in a
/// millisecond and get exactly the text a real ten minutes would have
/// produced. That only holds because nothing downstream reads a clock, which
/// is the discipline §5 asks for and the reason the fixtures are worth
/// anything.</para>
/// <para><see cref="Start"/> is deliberately not a timer either. This source
/// has no real-time obligation, and pretending to one would put a thread and a
/// scheduling assumption into the one path that exists to be reproducible.</para>
/// </remarks>
public sealed class BufferedAudioSource : IAudioSource
{
    /// <summary>Samples handed over per pump by default, about a fiftieth of a second.</summary>
    public const int DefaultChunkSamples = 960;

    private readonly float[] _samples;
    private readonly int _chunkSamples;
    private int _position;

    /// <summary>Creates a source over a buffer.</summary>
    /// <param name="audio">The audio to play back.</param>
    /// <param name="deviceName">What to call this source.</param>
    /// <param name="chunkSamples">How many samples each chunk carries.</param>
    public BufferedAudioSource(
        MonoAudio audio,
        string deviceName = "Recorded audio",
        int chunkSamples = DefaultChunkSamples)
    {
        ArgumentNullException.ThrowIfNull(audio);

        _samples = audio.Samples;
        SampleRate = audio.SampleRate;
        DeviceName = deviceName;
        _chunkSamples = Math.Max(1, chunkSamples);
    }

    /// <summary>Creates a source over raw samples.</summary>
    /// <param name="samples">Mono samples in [-1, 1].</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="deviceName">What to call this source.</param>
    /// <param name="chunkSamples">How many samples each chunk carries.</param>
    public BufferedAudioSource(
        float[] samples,
        int sampleRate,
        string deviceName = "Recorded audio",
        int chunkSamples = DefaultChunkSamples)
        : this(new MonoAudio(sampleRate, samples), deviceName, chunkSamples)
    {
    }

    /// <inheritdoc/>
    public string DeviceName { get; }

    /// <inheritdoc/>
    public int SampleRate { get; }

    /// <inheritdoc/>
    /// <remarks>
    /// Always true, with no setter. Playback of a recording is not reception,
    /// and a decode from a fixture must never reach the screen dressed as
    /// something that happened on the air (HM-DEC-026).
    /// </remarks>
    public bool IsSimulated => true;

    /// <inheritdoc/>
    public bool IsRunning { get; private set; }

    /// <summary>True once every sample has been delivered.</summary>
    public bool IsFinished => _position >= _samples.Length;

    /// <inheritdoc/>
    public event AudioChunkHandler? SamplesReady;

    /// <inheritdoc/>
    public void Start() => IsRunning = true;

    /// <inheritdoc/>
    public void Stop() => IsRunning = false;

    /// <summary>Rewind to the beginning.</summary>
    public void Reset() => _position = 0;

    /// <summary>
    /// Deliver one chunk.
    /// </summary>
    /// <returns>True when samples were delivered, false at the end.</returns>
    public bool PumpOnce()
    {
        if (IsFinished)
        {
            return false;
        }

        var count = Math.Min(_chunkSamples, _samples.Length - _position);
        var chunk = new AudioChunk(
            _position, SampleRate, _samples.AsSpan(_position, count));

        _position += count;
        SamplesReady?.Invoke(in chunk);
        return true;
    }

    /// <summary>Deliver everything remaining, chunk by chunk.</summary>
    public void PumpAll()
    {
        while (PumpOnce())
        {
        }
    }

    /// <inheritdoc/>
    public void Dispose() => Stop();
}

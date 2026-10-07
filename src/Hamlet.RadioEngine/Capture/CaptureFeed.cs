using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Capture;

/// <summary>
/// **THE CAPTURE THREAD WRITES THE DISK; THE SOUND CARD'S THREAD NEVER DOES** (work instruction 549, task 1): a bounded queue
/// in front of a <see cref="ContinuousCapture"/>, drained on a thread of its own.
/// </summary>
/// <remarks>
/// <para>**THE LESSON OF THE UNIT BEFORE.** A callback that runs long is audio the device overwrites, and the CW chain was
/// moved off the callback for exactly that (work instruction 548). A disk write can stall far longer than a decode, so the
/// capture is not let near the callback either: the callback copies the chunk into <see cref="AudioHandoff"/>, the queue
/// the chain and the waterfall already use, and returns.</para>
/// <para>**WHAT THE QUEUE DROPS, THE SHEET SAYS.** It holds three seconds; a disk that stalls longer drops the oldest chunk,
/// and the capture sees the audio clock jump and notes the hole on the piece's sheet.</para>
/// </remarks>
public sealed class CaptureFeed
{
    private readonly ContinuousCapture _capture;
    private readonly AudioHandoff _handoff;
    private readonly Thread _worker;

    /// <summary>Start draining into a capture.</summary>
    /// <param name="capture">The capture.</param>
    /// <param name="sampleRate">The audio's rate, to size the queue.</param>
    public CaptureFeed(ContinuousCapture capture, int sampleRate)
    {
        _capture = capture ?? throw new ArgumentNullException(nameof(capture));
        _handoff = new AudioHandoff(sampleRate, Math.Max(1, sampleRate * WasapiAudioSource.BufferMilliseconds / 1000));
        _worker = new Thread(Drain) { IsBackground = true, Name = "auto-capture" };
        _worker.Start();
    }

    /// <summary>Chunks the queue dropped because the disk fell behind.</summary>
    public long DroppedChunks => _handoff.DroppedChunks;

    /// <summary>Hand a chunk over; never blocks.</summary>
    /// <param name="firstSampleIndex">Its place on the audio clock.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="samples">The samples, copied before this returns.</param>
    public void Offer(long firstSampleIndex, int sampleRate, ReadOnlySpan<float> samples)
        => _handoff.Offer(firstSampleIndex, sampleRate, samples);

    /// <summary>Wait until every chunk handed over has been written: for the tests.</summary>
    /// <param name="timeout">How long to wait.</param>
    /// <returns>Whether it drained in time.</returns>
    public bool WaitUntilWritten(TimeSpan timeout) => _handoff.WaitUntilDrained(timeout);

    /// <summary>Write what is queued, then stop the thread.</summary>
    public void Stop()
    {
        _handoff.Close(discard: false);
        _worker.Join(TimeSpan.FromSeconds(5));
    }

    private void Drain()
    {
        var buffer = new float[1];

        while (_handoff.Take(ref buffer, out var count, out var first, out var rate))
        {
            try
            {
                _capture.Append(first, rate, buffer.AsSpan(0, count));
            }
            catch
            {
                // Never-throw (§8): a chunk that could not be written is lost, and the thread lives.
            }

            _handoff.Completed();
        }
    }
}

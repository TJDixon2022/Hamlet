namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// A bounded hand-off between the device's callback thread and one decoder
/// worker, which drops the oldest chunk when it is full and counts what it
/// dropped.
/// </summary>
/// <remarks>
/// <para>**THE FAULT THIS EXISTS AGAINST.** `WasapiAudioSource.OnDataAvailable`
/// invoked `SamplesReady` inline on WASAPI's own capture thread, and the only
/// subscriber that mattered ran the pitch tracker, the mixer and the
/// probabilistic decoder on that call. So the device's callback did not return
/// until a whole CW decode had finished, and every millisecond it spent there
/// was a millisecond WASAPI could not deliver into. Measured on the shack
/// machine 2026-09-03: audio reached the tap at about **13% of real time**, and
/// four consecutive press captures were byte-identical prefixes of one another.
/// Each "fifteen-second slot" handed to FT8 was 720,000 samples spanning about
/// two minutes of wall clock, in fragments. FT8 needs 12.64 s of continuous,
/// phase-coherent audio, so nothing could survive that and no decoder change
/// could have helped.</para>
/// <para>**BOUNDED, AND IT DROPS THE OLDEST.** Unbounded trades starvation for
/// memory growth, which is the same failure arriving later and harder to read.
/// Blocking puts the callback back exactly where it was. Dropping the oldest
/// rather than the newest is deliberate: when the decoder has fallen behind, the
/// audio still worth decoding is the audio nearest to now, and the operator is
/// reading a live band rather than a recording.</para>
/// <para>**AND EVERY DROP IS COUNTED.** A dropped chunk that leaves no number is
/// the fault this whole unit is about (HM-DEC-093): the tap was starved for
/// weeks and nothing counted it. `DroppedChunks` and `DroppedSamples` are read by
/// the census, the sidecar and the slot refusal so the shortfall reaches the
/// operator rather than a log.</para>
/// <para>**IT IS NOT ON THE TAP'S PATH.** The tap is fed synchronously from the
/// callback, before and independently of this queue. A tap on the far side of a
/// queue would be the original fault with a different name, which is why the
/// work instruction forbids it in terms.</para>
/// </remarks>
public sealed class AudioHandoff
{
    /// <summary>How many seconds of audio the queue holds before it drops.</summary>
    /// <remarks>
    /// **THREE SECONDS, WHICH IS A DECODER HICCUP AND NOT A DECODER FAILURE.**
    /// At 48 kHz in the 960-sample chunks `BufferedAudioSource` delivers, three
    /// seconds is 150 chunks and about 600 KB of buffers — small enough to be
    /// uninteresting and long enough to ride out a garbage collection, a page
    /// fault or a slow window on the UI thread.
    /// <para>It is deliberately **not** sized to survive a decoder that is
    /// persistently slower than real time. Nothing can: a queue in front of a
    /// consumer that cannot keep up fills at a constant rate whatever its size,
    /// and a bigger one only delays the first dropped sample while making the
    /// audio staler when it arrives. What the size buys is that a transient does
    /// not cost anything, and what the counter buys is that a persistent
    /// shortfall is visible instead of silent.</para>
    /// </remarks>
    public const double SecondsHeld = 3.0;

    private readonly object _gate = new();
    private readonly Slot[] _slots;
    private readonly int _capacity;

    private int _head;
    private int _count;
    private int _pending;
    private bool _closed;

    /// <summary>Creates a hand-off sized for one sample rate.</summary>
    /// <param name="sampleRate">Samples per second the source delivers.</param>
    /// <param name="chunkSamples">
    /// The chunk size to size the ring against. Chunks larger than this are
    /// still accepted; their buffers simply grow on first use.
    /// </param>
    public AudioHandoff(int sampleRate, int chunkSamples = 960)
    {
        var rate = Math.Max(1, sampleRate);
        var chunk = Math.Max(1, chunkSamples);

        _capacity = Math.Max(2, (int)(SecondsHeld * rate / chunk));
        _slots = new Slot[_capacity];

        for (var i = 0; i < _capacity; i++)
        {
            _slots[i].Samples = new float[chunk];
        }
    }

    /// <summary>How many chunks the queue can hold before it drops.</summary>
    public int Capacity => _capacity;

    /// <summary>How many chunks are waiting for the worker right now.</summary>
    public int Depth
    {
        get { lock (_gate) { return _count; } }
    }

    /// <summary>How many chunks were dropped because the queue was full.</summary>
    public long DroppedChunks { get; private set; }

    /// <summary>How many samples those dropped chunks carried.</summary>
    /// <remarks>
    /// The count the operator's arrival ratio needs. A chunk count alone cannot
    /// be turned into a fraction of a second of audio.
    /// </remarks>
    public long DroppedSamples { get; private set; }

    /// <summary>How many chunks have been handed over.</summary>
    public long OfferedChunks { get; private set; }

    /// <summary>
    /// Hand one chunk to the worker. Never blocks and never throws.
    /// </summary>
    /// <param name="firstSampleIndex">The chunk's place on the audio clock.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="samples">The samples, copied before this returns.</param>
    /// <returns>True where it was queued, false where it displaced an older chunk.</returns>
    /// <remarks>
    /// **THE SAMPLES ARE COPIED AND THAT IS NOT OPTIONAL.** `AudioChunk` is a
    /// `ref struct` over the source's reused scratch array, valid only for the
    /// duration of the call that delivered it. The next callback overwrites it.
    /// </remarks>
    public bool Offer(long firstSampleIndex, int sampleRate, ReadOnlySpan<float> samples)
    {
        lock (_gate)
        {
            if (_closed)
            {
                return false;
            }

            var displaced = false;

            if (_count == _capacity)
            {
                // **THE OLDEST GOES, AND IT IS COUNTED BEFORE IT GOES.**
                DroppedChunks++;
                DroppedSamples += _slots[_head].Count;

                // A dropped chunk will never be completed, so it must leave the
                // in-flight count here or WaitUntilDrained would wait forever
                // on work that is not coming.
                _pending--;
                _head = (_head + 1) % _capacity;
                _count--;
                displaced = true;
            }

            var at = (_head + _count) % _capacity;

            if (_slots[at].Samples.Length < samples.Length)
            {
                _slots[at].Samples = new float[samples.Length];
            }

            samples.CopyTo(_slots[at].Samples);
            _slots[at].Count = samples.Length;
            _slots[at].FirstSampleIndex = firstSampleIndex;
            _slots[at].SampleRate = sampleRate;

            _count++;
            _pending++;
            OfferedChunks++;

            Monitor.Pulse(_gate);

            return !displaced;
        }
    }

    /// <summary>
    /// Wait for the next chunk and copy it out, in the order it was offered.
    /// </summary>
    /// <param name="into">
    /// The worker's own buffer, grown in place when it is too small.
    /// </param>
    /// <param name="count">How many samples were written into it.</param>
    /// <param name="firstSampleIndex">The chunk's place on the audio clock.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>False once the hand-off is closed and drained.</returns>
    /// <remarks>
    /// **ONE CONSUMER, IN ORDER.** `FirstSampleIndex` must stay monotonic for
    /// HM-DEC-147's audio clock, and two workers would interleave chunks and
    /// misdate every character read afterwards.
    /// </remarks>
    public bool Take(ref float[] into, out int count, out long firstSampleIndex, out int sampleRate)
    {
        lock (_gate)
        {
            while (_count == 0 && !_closed)
            {
                Monitor.Wait(_gate);
            }

            if (_count == 0)
            {
                count = 0;
                firstSampleIndex = 0;
                sampleRate = 0;

                return false;
            }

            ref var slot = ref _slots[_head];

            if (into.Length < slot.Count)
            {
                into = new float[slot.Count];
            }

            slot.Samples.AsSpan(0, slot.Count).CopyTo(into);
            count = slot.Count;
            firstSampleIndex = slot.FirstSampleIndex;
            sampleRate = slot.SampleRate;

            _head = (_head + 1) % _capacity;
            _count--;

            return true;
        }
    }

    /// <summary>Tell the hand-off one taken chunk has been fully processed.</summary>
    /// <remarks>
    /// Called by the worker after `Process` returns, so `WaitUntilDrained` waits
    /// for the decode to finish and not merely for the queue to empty. The
    /// difference is one chunk, and it is the difference between a deterministic
    /// test and a flaky one.
    /// </remarks>
    public void Completed()
    {
        lock (_gate)
        {
            if (_pending > 0)
            {
                _pending--;
            }

            Monitor.PulseAll(_gate);
        }
    }

    /// <summary>Wait until nothing offered is still waiting or in progress.</summary>
    /// <param name="timeout">How long to wait before giving up.</param>
    /// <returns>True where the hand-off went quiet, false on the timeout.</returns>
    /// <remarks>
    /// **THIS IS WHAT KEEPS THE FIXTURE PATH DETERMINISTIC.** The CW harness
    /// does `Listen(source); source.PumpAll(); decoder.Flush();`, and once the
    /// decode moved onto a worker `PumpAll` returned when the audio was QUEUED
    /// rather than when it was READ. Without this wait, every CW test in the
    /// repository would have become a race against a background thread, and the
    /// ones that still passed would have been passing by timing.
    /// </remarks>
    public bool WaitUntilDrained(TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        lock (_gate)
        {
            while (_pending > 0)
            {
                var left = deadline - DateTime.UtcNow;

                if (left <= TimeSpan.Zero)
                {
                    return false;
                }

                Monitor.Wait(_gate, left);
            }

            return true;
        }
    }

    /// <summary>
    /// Stop accepting chunks and release the worker.
    /// </summary>
    /// <param name="discard">
    /// True to drop what is queued, false to let the worker drain it first.
    /// </param>
    /// <remarks>
    /// **NO WORKER OUTLIVES THE SOURCE.** Either arm ends with `Take` returning
    /// false, which is the worker's only exit.
    /// </remarks>
    public void Close(bool discard)
    {
        lock (_gate)
        {
            _closed = true;

            if (discard)
            {
                _pending -= _count;

                if (_pending < 0)
                {
                    _pending = 0;
                }

                _count = 0;
                _head = 0;
            }

            Monitor.PulseAll(_gate);
        }
    }

    private struct Slot
    {
        public float[] Samples;
        public int Count;
        public long FirstSampleIndex;
        public int SampleRate;
    }
}

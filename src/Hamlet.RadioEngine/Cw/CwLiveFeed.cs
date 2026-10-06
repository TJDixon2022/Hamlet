using System.Diagnostics;
using Hamlet.RadioEngine.Audio;

namespace Hamlet.RadioEngine.Cw;

/// <summary>What the live decode path received and lost, as counted (work instruction 548, task 2).</summary>
/// <param name="ChunksReceived">Chunks the capture handed over.</param>
/// <param name="SamplesReceived">Samples in them.</param>
/// <param name="SamplesExpected">Samples the time elapsed since the first chunk should have brought.</param>
/// <param name="ChunksDropped">Chunks the queue dropped because the decode had fallen behind.</param>
/// <param name="SamplesDropped">Samples in them.</param>
/// <param name="LostMilliseconds">Audio that never reached the decode: dropped, or never delivered by the capture.</param>
/// <param name="LostLastMinuteMilliseconds">Of that, how much in the last minute.</param>
/// <param name="LongestStallMilliseconds">The longest a callback came late, past the audio the one before it carried.</param>
/// <param name="QueuePeak">The deepest the queue got, in chunks.</param>
/// <param name="Holes">How many times the decode went on after audio the queue had dropped.</param>
public sealed record AudioContinuity(
    long ChunksReceived,
    long SamplesReceived,
    long SamplesExpected,
    long ChunksDropped,
    long SamplesDropped,
    double LostMilliseconds,
    double LostLastMinuteMilliseconds,
    double LongestStallMilliseconds,
    int QueuePeak,
    long Holes)
{
    /// <summary>Nothing received yet.</summary>
    public static AudioContinuity None { get; } = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>The sheet's line: lost, the longest stall and the queue's peak.</summary>
    public string SheetLine => FormattableString.Invariant(
        $"lost {LostMilliseconds:0} ms, longest stall {LongestStallMilliseconds:0} ms, queue peak {QueuePeak}");
}

/// <summary>
/// **THE LIVE PATH LOSES NO AUDIO** (work instruction 548, task 1): the CW chain fed from a queue on its own thread, the tap
/// fed on the capture's.
/// </summary>
/// <remarks>
/// <para>**THE FAULT.** The decoder and the detector both ran inside the capture's callback, on WASAPI's own thread. A callback
/// that runs longer than the device's 100 ms buffer is audio the device overwrote, and the chain was never told: a dah cut by
/// 50 ms read as a dit, a gap shortened ran two letters into one. Fed W1AW's bulletin with one 50 ms chunk lost a second, the
/// chain printed `TYAEE IEA RADIO EMII EEIOTS` where it reads `TYPE IV RADIO EMISSIONS` whole: the live junk's kind.</para>
/// <para>**THE CURE IS THE ONE THE TAP ALREADY HAS** (`AudioHandoff`, HM-DEC-093): the callback copies the chunk into a
/// bounded queue and returns; the chain drains it on a thread of its own. When the decode falls three seconds behind, the
/// oldest chunk is dropped and counted. Telling the gate where audio went missing, so that nothing measured across it is
/// printed, was measured on W1AW fed with losses and read worse as often as better; it is not built (work instruction 548).</para>
/// <para>**THE TAP STAYS ON THE CALLBACK.** Record and FT8 read it, and a tap behind a queue is the starvation it was moved
/// off the queue to end.</para>
/// </remarks>
public sealed class CwLiveFeed : IDisposable
{
    private readonly CwDecoder _decoder;
    private readonly CwEnvelopeDetector _detector;
    private readonly Func<long> _clock;
    private readonly object _gate = new();
    private readonly Queue<(long Ticks, double Milliseconds)> _losses = new();

    private IAudioSource? _attached;
    private AudioHandoff? _handoff;
    private Thread? _worker;
    private float[] _fromQueue = new float[1];

    private long _chunks;
    private long _samples;
    private long _firstTicks = -1;
    private long _lastTicks = -1;
    private double _lastChunkSeconds;
    private double _longestStallMs;
    private int _queuePeak;
    private long _holes;
    private long _shortfallCounted;
    private long _droppedCounted;
    private long _expectedNext = -1;

    /// <summary>Feeds a chain from a queue.</summary>
    /// <param name="decoder">The decoder; its tap is fed on the capture's thread.</param>
    /// <param name="detector">The detector, wired to the decoder.</param>
    /// <param name="clock">The clock in <see cref="Stopwatch"/> ticks; the stopwatch's own unless a test gives one.</param>
    public CwLiveFeed(CwDecoder decoder, CwEnvelopeDetector detector, Func<long>? clock = null)
    {
        _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
        _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        _clock = clock ?? Stopwatch.GetTimestamp;
    }

    /// <summary>The source listened to, or null.</summary>
    public IAudioSource? Source => _attached;

    /// <summary>Listen to a source, or to nothing.</summary>
    /// <param name="source">The source, or null to stop.</param>
    public void Listen(IAudioSource? source)
    {
        if (ReferenceEquals(_attached, source))
        {
            return;
        }

        if (_attached is not null)
        {
            _attached.SamplesReady -= OnSamples;
            StopWorker();
        }

        _attached = source;

        if (_attached is not null)
        {
            var rate = _attached.SampleRate;

            _handoff = new AudioHandoff(rate, Math.Max(1, rate * WasapiAudioSource.BufferMilliseconds / 1000));
            _fromQueue = new float[Math.Max(1, rate)];
            _worker = new Thread(Drain) { IsBackground = true, Name = "cw-chain" };
            _worker.Start();
            _attached.SamplesReady += OnSamples;
        }
    }

    /// <summary>Wait until every chunk handed over has been decoded: for the tests.</summary>
    /// <param name="timeout">How long to wait.</param>
    /// <returns>Whether it drained in time.</returns>
    public bool WaitUntilDrained(TimeSpan timeout) => _handoff?.WaitUntilDrained(timeout) ?? true;

    /// <summary>What has been received and lost so far.</summary>
    public AudioContinuity Continuity
    {
        get
        {
            lock (_gate)
            {
                var rate = _attached?.SampleRate ?? _decoder.SampleRate;
                var expected = _firstTicks < 0 ? 0 : (long)((_clock() - _firstTicks) / (double)Stopwatch.Frequency * rate);
                var dropped = _handoff?.DroppedSamples ?? 0;

                Count(expected, dropped, rate);

                var now = _clock();
                var minute = Stopwatch.Frequency * 60;

                while (_losses.Count > 0 && now - _losses.Peek().Ticks > minute)
                {
                    _losses.Dequeue();
                }

                return new AudioContinuity(
                    _chunks, _samples, expected, _handoff?.DroppedChunks ?? 0, dropped,
                    (_shortfallCounted + _droppedCounted) * 1000.0 / rate,
                    _losses.Sum(l => l.Milliseconds),
                    _longestStallMs, _queuePeak, Interlocked.Read(ref _holes));
            }
        }
    }

    /// <summary>Stop listening and stop the thread.</summary>
    public void Dispose() => Listen(null);

    // Losses are counted where they are seen: what the queue dropped, and what the capture fell short by past one device
    // buffer of the clock's own jitter. Each new loss is kept with its time, for the last minute's figure.
    private void Count(long expected, long dropped, int rate)
    {
        var slack = rate * WasapiAudioSource.BufferMilliseconds / 1000;
        var shortfall = Math.Max(0, expected - _samples - dropped - slack);
        var now = _clock();

        if (shortfall > _shortfallCounted)
        {
            _losses.Enqueue((now, (shortfall - _shortfallCounted) * 1000.0 / rate));
            _shortfallCounted = shortfall;
        }

        if (dropped > _droppedCounted)
        {
            _losses.Enqueue((now, (dropped - _droppedCounted) * 1000.0 / rate));
            _droppedCounted = dropped;
        }
    }

    private void OnSamples(in AudioChunk chunk)
    {
        // The tap first, on this thread, exactly as before: what Record and FT8 read is what arrived.
        _decoder.Tap.Take(chunk.Samples, chunk.SampleRate);

        var now = _clock();

        lock (_gate)
        {
            if (_firstTicks < 0)
            {
                _firstTicks = now;
            }
            else
            {
                var since = (now - _lastTicks) * 1000.0 / Stopwatch.Frequency;

                _longestStallMs = Math.Max(_longestStallMs, since - (_lastChunkSeconds * 1000));
            }

            _lastTicks = now;
            _lastChunkSeconds = chunk.Samples.Length / (double)chunk.SampleRate;
            _chunks++;
            _samples += chunk.Samples.Length;
        }

        var handoff = _handoff;

        if (handoff is null)
        {
            return;
        }

        handoff.Offer(chunk.FirstSampleIndex, chunk.SampleRate, chunk.Samples);

        lock (_gate)
        {
            _queuePeak = Math.Max(_queuePeak, handoff.Depth);
        }
    }

    private void Drain()
    {
        var handoff = _handoff;

        if (handoff is null)
        {
            return;
        }

        while (handoff.Take(ref _fromQueue, out var count, out var first, out var rate))
        {
            try
            {
                // **A HOLE IS COUNTED, NOT FILLED** (work instruction 548, task 1): a chunk that does not start where the last one
                // ended follows audio the queue dropped. Silence put in its place would read as a key-up, so nothing is put
                // there; the hole is counted for the sheet and the telemetry.
                if (_expectedNext >= 0 && first > _expectedNext)
                {
                    Interlocked.Increment(ref _holes);
                }

                _expectedNext = first + count;

                var samples = _fromQueue.AsSpan(0, count);

                _decoder.Process(new AudioChunk(first, rate, samples), tap: false);
                _detector.Process(samples);
            }
            catch
            {
                // Never-throw (§8): a chunk that failed to decode is lost, and the thread lives.
            }

            handoff.Completed();
        }
    }

    private void StopWorker()
    {
        _handoff?.Close(discard: false);
        _worker?.Join(TimeSpan.FromSeconds(2));
        _worker = null;
        _handoff = null;
        _expectedNext = -1;
    }
}

using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// Overlapping spectra of the received audio, for the digital waterfall.
/// </summary>
/// <remarks>
/// <para>**THIS IS AN FFT OF THE CODEC AUDIO, NOT THE RADIO'S SCOPE** (Tim's
/// ruling of 2026-08-28). The scope stream is band-wide RF in kilohertz and FT8
/// occupies a fifty hertz sliver of a three kilohertz audio passband, so a
/// picture built from it would show nothing useful and the slot grid would have
/// nothing to align to.</para>
/// <para>**AND IT IS NOT THE DECODER'S OWN INTERNAL ARRAY**, for the reason that
/// ruling gives: tying the picture to the decoder means the waterfall goes dark
/// exactly when the decoder is silent, and that is the case most worth
/// seeing.</para>
/// <para>**IT SUBSCRIBES; IT DOES NOT OPEN A SECOND CAPTURE.**
/// <see cref="IAudioSource.SamplesReady"/> is an ordinary multicast event, so
/// this rides alongside the CW decoder without either knowing about the other
/// and without the audio path being restructured.</para>
/// <para>**PURE BELOW THE PUMP** (§5.4). Samples and a timestamp go in, frames
/// come out; nothing here reads a clock, so a fixture produces the same frames
/// on every run.</para>
/// </remarks>
public sealed class AudioSpectrumSource : ISpectrumSource, IDisposable
{
    /// <summary>The lowest frequency the waterfall shows.</summary>
    /// <remarks>
    /// Two hundred hertz, from the order. Below it is where a receiver's own
    /// rumble lives and no digital mode sits there.
    /// </remarks>
    public const int LowHz = 200;

    /// <summary>The highest frequency the waterfall shows.</summary>
    /// <remarks>Three thousand, which is the top of an SSB passband.</remarks>
    public const int HighHz = 3000;

    /// <summary>Samples per transform window.</summary>
    /// <remarks>
    /// <para>**CHOSEN FROM FT8'S TONE SPACING AND STATED RATHER THAN ASSUMED.**
    /// FT8 places its eight tones 6.25 hertz apart, which is the reciprocal of
    /// its own 0.16 second symbol. To see them as separate stripes the bin width
    /// has to be meaningfully finer than that spacing.</para>
    /// <para>At 48 kHz a 16384-sample window gives a bin every **2.93 Hz** and
    /// covers **0.341 s** of audio. That is a little over two bins per FT8 tone,
    /// which resolves them, and about two symbols of smear in time, which is
    /// what buys the frequency resolution. **A wider window would resolve better
    /// and blur the fifteen-second slot edges the grid exists to show**, and
    /// those edges are what make FT8 recognisable at a glance.</para>
    /// <para>At 8 kHz the same window is 2.05 s, far too long. The window is
    /// therefore chosen from the sample rate in <see cref="WindowFor"/> rather
    /// than fixed, so a fixture at 8 kHz and a radio at 48 kHz both produce a
    /// sensible picture.</para>
    /// </remarks>
    public const int WindowAt48K = 16384;

    /// <summary>How much of a window is fresh audio.</summary>
    /// <remarks>
    /// A quarter, so consecutive frames overlap by three quarters. Overlap is
    /// what makes a waterfall read as continuous rather than as a flip-book, and
    /// a quarter-hop at 48 kHz is a new row about every 85 milliseconds.
    /// </remarks>
    public const int HopDivisor = 4;

    private readonly object _gate = new();
    private readonly int _sampleRate;
    private readonly RealFft _fft;
    private readonly float[] _ring;
    private readonly float[] _window;
    private readonly double[] _taper;
    private readonly double[] _magnitudes;
    private readonly double[] _real;
    private readonly double[] _imaginary;
    private readonly byte[] _bins;
    private readonly double[] _binFloorDb;
    private readonly double[] _binSpreadDb;

    /// <summary>How fast a bin's noise spread is averaged.</summary>
    /// <remarks>
    /// **SLOW, BECAUSE IT IS A PROPERTY OF THE BAND AND NOT OF A MOMENT.** It
    /// rises into a signal as well as into noise, which is why it is slow enough
    /// that a transmission cannot lift its own zero out from under it.
    /// </remarks>
    private const double SpreadFollowsAt = 0.002;

    /// <summary>How fast a bin's floor follows the level down.</summary>
    /// <remarks>A quarter of the way each frame: noise is followed at once.</remarks>
    private const double FloorFallsAt = 0.25;

    /// <summary>How fast a bin's floor follows the level up.</summary>
    /// <remarks>
    /// **SLOW ENOUGH THAT A STATION DOES NOT ERASE ITSELF.** At roughly twelve
    /// frames a second this is a time constant of minutes, so a fifteen-second
    /// transmission stays bright for all of it.
    /// </remarks>
    private const double FloorRisesAt = 0.0008;

    /// <summary>How many decibels above its own floor a bin needs to be white.</summary>
    /// <remarks>
    /// Thirty-five. With the floor now per bin rather than across the picture,
    /// this is the dynamic range of one bin against its own quiet level rather
    /// than of the whole passband against its quietest corner.
    /// </remarks>
    private const double RangeDb = 35;
    private readonly int _hop;
    private readonly int _firstBin;
    private readonly int _lastBin;

    private IAudioSource? _attached;
    private int _fill;
    private int _sinceHop;
    private long _samplesSeen;

    /// <summary>The queue between the device callback and the transform.</summary>
    /// <remarks>
    /// <para>**UNIT 238'S HAND-OFF, A SECOND INSTANCE OF IT, AND NOT A SECOND
    /// MECHANISM.** It already solves bounded, ordered, oldest-dropped, one
    /// consumer, samples copied before the call returns, never blocking and
    /// never throwing. Every one of those is required here for the same reasons
    /// it was required for the CW decoder, and a second mechanism would be a
    /// second set of the same bugs.</para>
    /// <para>**DROPPING A WATERFALL ROW IS FREE AND DROPPING AUDIO IS THE FAULT
    /// THIS UNIT EXISTS TO REMOVE.** So when the queue fills, the picture loses
    /// a row and the count says so; the callback is never made to wait.</para>
    /// </remarks>
    private AudioHandoff? _handoff;

    private Thread? _worker;

    private float[] _fromQueue = [];

    /// <summary>The longest a single frame took the worker, in microseconds.</summary>
    /// <remarks>
    /// **THE NUMBER THAT SAYS WHETHER MOVING IT HELPED OR ONLY HID IT**
    /// (HM-DEC-093). Work moved off the callback thread has not gone away; it
    /// has gone somewhere a slow frame costs the picture instead of the radio.
    /// A figure climbing here with no drops is a machine coping; one climbing
    /// with drops is the picture falling behind, which is a fact worth having
    /// rather than one to discover from a screenshot.
    /// </remarks>
    public double LongestFrameMicroseconds { get; private set; }

    /// <summary>Rows the picture lost because the worker was behind.</summary>
    public long DroppedFrames => _handoff?.DroppedChunks ?? 0;

    /// <summary>Samples those dropped rows carried.</summary>
    public long DroppedFrameSamples => _handoff?.DroppedSamples ?? 0;

    /// <summary>Where the next sample goes, and the oldest sample once full.</summary>
    /// <remarks>
    /// <para>**THE RING USED TO HAVE NO CURSOR, AND THAT WAS THE WHOLE FAULT.**
    /// It was a linear array with the oldest sample at index 0, so making room
    /// for a new one meant `Array.Copy(_ring, 1, _ring, 0, _ring.Length - 1)` -
    /// moving 16,383 floats - **once per sample**. Measured in unit 240 task 1:
    /// 63,120 microseconds for a single 4,800-sample buffer, which is 78 million
    /// floats moved per 100 ms of audio and **99.5% of everything the device
    /// callback was doing**.</para>
    /// <para>**WITH A CURSOR NOTHING MOVES AT ALL.** A buffer is written where
    /// the cursor points, in at most two `Array.Copy` calls, and the cursor
    /// advances. It is what `AudioTap` has always done a few files away.</para>
    /// <para>**THE READER PAYS FOR IT INSTEAD, AND THAT IS THE RIGHT PLACE.**
    /// `Emit` has to walk the ring from the oldest sample rather than from index
    /// zero, so it does two passes over 16,384 floats where it used to do one.
    /// That runs once a hop rather than once a sample - a factor of 4,096 - and
    /// after task 3 it does not run on the callback thread at all.</para>
    /// </remarks>
    private int _write;

    /// <summary>Creates a spectrum source over one sample rate.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <param name="simulated">
    /// Whether the audio behind it is synthesised (HM-DEC-026).
    /// </param>
    public AudioSpectrumSource(int sampleRate, bool simulated = false)
    {
        _sampleRate = Math.Max(1000, sampleRate);
        IsSimulated = simulated;

        var size = WindowFor(_sampleRate);
        _fft = new RealFft(size);
        _hop = size / HopDivisor;

        _ring = new float[size];
        _window = new float[size];
        _taper = new double[size];
        _magnitudes = new double[_fft.BinCount];
        _real = new double[size];
        _imaginary = new double[size];

        // A Hann taper, so a tone that does not land exactly on a bin does not
        // smear across the whole picture.
        for (var i = 0; i < size; i++)
        {
            _taper[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (size - 1)));
        }

        _firstBin = (int)Math.Floor(LowHz * (double)size / _sampleRate);
        _lastBin = Math.Min(
            _fft.BinCount - 1,
            (int)Math.Ceiling(HighHz * (double)size / _sampleRate));

        _bins = new byte[Math.Max(1, _lastBin - _firstBin + 1)];
        _binFloorDb = new double[_bins.Length];
        Array.Fill(_binFloorDb, double.NegativeInfinity);

        _binSpreadDb = new double[_bins.Length];
        Array.Fill(_binSpreadDb, double.NegativeInfinity);
    }

    /// <summary>The window size that suits a sample rate.</summary>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>A power of two.</returns>
    /// <remarks>
    /// **SCALED SO EVERY RATE SEES THE SAME SPAN OF TIME**, about a third of a
    /// second, which is what fixes the bin width near three hertz whatever the
    /// radio or the fixture runs at.
    /// </remarks>
    public static int WindowFor(int sampleRate)
    {
        var wanted = sampleRate * WindowAt48K / 48000.0;
        var size = 256;

        while (size * 2 <= wanted)
        {
            size *= 2;
        }

        return size;
    }

    /// <summary>How wide one bin is, in hertz.</summary>
    public double BinWidthHz => (double)_sampleRate / _fft.Size;

    /// <summary>How long one window covers, in seconds.</summary>
    public double WindowSeconds => (double)_fft.Size / _sampleRate;

    /// <inheritdoc />
    public bool IsSimulated { get; }

    /// <inheritdoc />
    public bool IsRunning { get; private set; }

    /// <inheritdoc />
    public event SpectrumFrameHandler? FrameReady;

    /// <inheritdoc />
    public void Start() => IsRunning = true;

    /// <inheritdoc />
    public void Stop() => IsRunning = false;

    /// <summary>Ride along with an audio source, or stop riding.</summary>
    /// <param name="source">The source, or null to detach.</param>
    /// <remarks>
    /// **IT SUBSCRIBES AND NEVER STARTS OR STOPS THE SOURCE.** Whoever opened
    /// the audio owns its lifetime; a second consumer that could stop it would
    /// be able to silence the CW decoder from the Digital tab.
    /// </remarks>
    public void Listen(IAudioSource? source)
    {
        if (ReferenceEquals(_attached, source))
        {
            return;
        }

        if (_attached is not null)
        {
            _attached.SamplesReady -= OnSamples;
        }

        // **UNSUBSCRIBED FIRST, THEN THE WORKER STOPPED.** The other order
        // leaves a window where a callback can offer onto a closed queue.
        StopWorker();

        _attached = source;

        if (_attached is not null)
        {
            StartWorker(_attached.SampleRate);
            _attached.SamplesReady += OnSamples;
        }
    }

    /// <summary>Bring up the queue and the thread that drains it.</summary>
    /// <param name="sampleRate">What the source delivers, to size the queue.</param>
    private void StartWorker(int sampleRate)
    {
        // Sized against the device buffer rather than the hop: what is queued is
        // what the callback hands over, and that is one device buffer at a time.
        var handoff = new AudioHandoff(
            sampleRate,
            Math.Max(1, sampleRate * WasapiAudioSource.BufferMilliseconds / 1000));

        _handoff = handoff;
        _fromQueue = new float[Math.Max(1, sampleRate)];

        var worker = new Thread(Drain)
        {
            IsBackground = true,
            Name = "spectrum-frames",

            // **BELOW NORMAL, AND IT IS THE POINT OF THE WHOLE TASK.** The
            // picture must never be the reason the audio thread waits, and on a
            // busy machine the scheduler should starve a waterfall row before it
            // starves anything else this application is doing.
            Priority = ThreadPriority.BelowNormal,
        };

        _worker = worker;
        worker.Start();
    }

    /// <summary>Close the queue and let the worker finish, without hanging.</summary>
    /// <remarks>
    /// **DISCARDED RATHER THAN DRAINED, BECAUSE THESE ARE PICTURES.** Unit 238's
    /// decoder drains on the way out so no audio is lost; a waterfall row that
    /// arrives after the source was detached has nowhere to be drawn. The join
    /// is bounded so a stalled `FrameReady` handler cannot hold a shutdown open.
    /// </remarks>
    private void StopWorker()
    {
        var handoff = _handoff;
        var worker = _worker;

        _handoff = null;
        _worker = null;

        handoff?.Close(discard: true);
        worker?.Join(TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// The device callback's whole share of the waterfall: a copy and a return.
    /// </summary>
    /// <remarks>
    /// <para>**IT USED TO CALL `Push` IN FULL, ON WASAPI'S OWN CAPTURE THREAD.**
    /// Task 1 measured that at 62,271 microseconds for one 100 ms buffer against
    /// a 100,000 microsecond period, and the shack machine reported 554 of 561
    /// callbacks running past half their budget.</para>
    /// <para>**TASK 2 TOOK THAT TO 270 MICROSECONDS AND THIS IS STILL WORTH
    /// DOING.** What is left is a real 16,384-point transform, and the argument
    /// against it is not its size but its variance: it runs on the thread
    /// carrying the radio's audio, where a garbage collection, a descheduled
    /// core, or a `FrameReady` handler that decides to touch the UI costs
    /// samples that no later work can recover. **A dropped waterfall row is
    /// free. Dropped audio is the fault this unit exists to remove.**</para>
    /// <para>**`Push` ITSELF IS UNCHANGED AND STILL SYNCHRONOUS**, because it is
    /// the pump and the class's determinism lives in it: a fixture pushed
    /// directly produces the same frames in the same order with the same times
    /// on every run. What moved is who calls it.</para>
    /// </remarks>
    private void OnSamples(in AudioChunk chunk)
    {
        var handoff = _handoff;

        if (handoff is null || !IsRunning)
        {
            return;
        }

        handoff.Offer(chunk.FirstSampleIndex, chunk.SampleRate, chunk.Samples);
    }

    /// <summary>How many frame workers are alive across the whole process.</summary>
    /// <remarks>
    /// **A COUNT, BECAUSE COUNTING PROCESS THREADS IS THE WRONG INSTRUMENT.**
    /// The first version of the leak test counted every thread in the process,
    /// which passed alone and failed beside the rest of the suite: the threads
    /// it saw appear were other tests' and had nothing to do with this class.
    /// This counts exactly the workers this class started, so it is exact and
    /// cannot be moved by anything running alongside it.
    /// </remarks>
    internal static int LiveWorkers => System.Threading.Volatile.Read(ref _liveWorkers);

    private static int _liveWorkers;

    /// <summary>Drain the queue, doing the transform work off the callback.</summary>
    private void Drain()
    {
        var handoff = _handoff;

        if (handoff is null)
        {
            return;
        }

        Interlocked.Increment(ref _liveWorkers);

        try
        {
            DrainLoop(handoff);
        }
        finally
        {
            Interlocked.Decrement(ref _liveWorkers);
        }
    }

    private void DrainLoop(AudioHandoff handoff)
    {

        while (handoff.Take(ref _fromQueue, out var count, out _, out _))
        {
            var started = System.Diagnostics.Stopwatch.GetTimestamp();

            try
            {
                Push(_fromQueue.AsSpan(0, count));
            }
            catch
            {
                // **NEVER-THROW, ON THE SAME GROUND AS EVERY OTHER WORKER HERE**
                // (§8). A transform that threw would take the thread with it and
                // the picture would stop with no record of why, which is worse
                // than a missing row. The frame is lost and the worker lives.
            }

            // **AFTER THE WORK, NOT AFTER THE TAKE.** `WaitUntilDrained` waits
            // on this rather than on the queue emptying, and the difference is
            // one frame - which is the difference between a deterministic test
            // and a flaky one.
            handoff.Completed();

            var micros = (System.Diagnostics.Stopwatch.GetTimestamp() - started)
                * 1_000_000.0 / System.Diagnostics.Stopwatch.Frequency;

            if (micros > LongestFrameMicroseconds)
            {
                LongestFrameMicroseconds = micros;
            }
        }
    }

    /// <summary>
    /// Feed samples in and raise a frame whenever a hop has filled.
    /// </summary>
    /// <param name="samples">The newest audio.</param>
    /// <remarks>
    /// **DETERMINISTIC: NO CLOCK IS READ HERE.** A frame's timestamp is derived
    /// from how many samples have been seen, so the same fixture produces the
    /// same frames with the same times on every run (§5.4).
    /// </remarks>
    public void Push(ReadOnlySpan<float> samples)
    {
        if (!IsRunning)
        {
            return;
        }

        // **THE SAMPLES ARE WRITTEN IN RUNS, AND THE HOP IS WHAT BREAKS THEM
        // UP.** A frame has to be raised at exactly the sample the old code
        // raised it at, or the picture changes cadence and every frame carries
        // the wrong time - so the buffer is cut at hop boundaries and no
        // further. Between two boundaries nothing is examined per sample at all.
        var from = 0;

        while (from < samples.Length)
        {
            int take;
            bool emit;

            lock (_gate)
            {
                // How many samples until this hop completes. Before the ring has
                // filled there is no hop to complete, because the old code only
                // emitted once `_fill` had reached `_ring.Length`.
                var untilHop = _hop - _sinceHop;
                var untilFull = _ring.Length - _fill;

                take = samples.Length - from;
                emit = false;

                if (untilHop <= take && untilFull <= take)
                {
                    take = Math.Max(untilHop, untilFull);
                    emit = true;
                }

                Write(samples.Slice(from, take));

                _samplesSeen += take;
                _sinceHop += take;

                if (emit)
                {
                    _sinceHop = 0;
                }
            }

            from += take;

            if (emit)
            {
                Emit();
            }
        }
    }

    /// <summary>Write one run into the ring, without moving anything.</summary>
    /// <param name="samples">The run. Never longer than the ring.</param>
    /// <remarks>
    /// **AT MOST TWO `Array.Copy` CALLS, AND USUALLY ONE.** The ring is
    /// contiguous either side of the cursor, so a run that does not reach the
    /// end is one copy and a run that wraps is two. Nothing is shifted and
    /// nothing is examined per sample.
    /// </remarks>
    private void Write(ReadOnlySpan<float> samples)
    {
        var count = samples.Length;

        if (count <= 0)
        {
            return;
        }

        // A run longer than the ring can only leave its own tail behind, and
        // taking the tail is what the old code's repeated shifting amounted to.
        if (count > _ring.Length)
        {
            samples = samples[^_ring.Length..];
            count = _ring.Length;
        }

        var first = Math.Min(count, _ring.Length - _write);

        samples[..first].CopyTo(_ring.AsSpan(_write, first));

        if (first < count)
        {
            samples[first..].CopyTo(_ring.AsSpan(0, count - first));
        }

        _write = (_write + count) % _ring.Length;
        _fill = Math.Min(_ring.Length, _fill + count);
    }

    private void Emit()
    {
        var handler = FrameReady;

        if (handler is null)
        {
            return;
        }

        lock (_gate)
        {
            // **OLDEST FIRST, WHICH IS WHERE THE CURSOR POINTS ONCE THE RING IS
            // FULL.** The old linear ring kept the oldest sample at index 0 and
            // paid for that ordering on every single sample. This reads the same
            // order out of a ring that costs nothing to write, and the taper
            // still lands on the same sample it always did - which is why the
            // pinned frames in `tests/fixtures/spectrum` come out identical.
            var oldest = _fill < _ring.Length ? 0 : _write;

            for (var i = 0; i < _ring.Length; i++)
            {
                var index = oldest + i;

                if (index >= _ring.Length)
                {
                    index -= _ring.Length;
                }

                _window[i] = (float)(_ring[index] * _taper[i]);
            }
        }

        _fft.Magnitudes(_window, _magnitudes, _real, _imaginary);

        // **THE PICTURE IS IN DECIBELS AGAINST A FLOOR THAT FOLLOWS THE BAND.**
        // A fixed scale makes a quiet band black and a loud one solid white; the
        // floor tracks the quietest quarter of the visible span so the picture
        // stays readable as conditions move, which is what every waterfall does
        // and what makes one legible at all.
        var span = _lastBin - _firstBin + 1;

        // **THE FLOOR IS PER BIN AND TRACKED OVER TIME, NOT A PERCENTILE ACROSS
        // THE PICTURE.** This is the fault the operator photographed on
        // 2026-08-28: a single floor taken across the display span sits wherever
        // the quietest quarter of that span is, and when the radio's filter
        // leaves most of 200 to 3000 Hz in the stopband, that is tens of
        // decibels below the noise inside the passband. Everything in band then
        // saturates, and the filter skirt reads as a hard vertical edge.
        //
        // **MEASURED BEFORE AND AFTER, ON A RECORDING THAT HOLDS NOTHING**: it
        // drew 12.3 % of its bins saturated with a worst neighbour-to-neighbour
        // step of 226 of 255, which is band noise painted as signal.
        //
        // **EACH BIN AGAINST ITS OWN RECENT QUIET LEVEL** removes the filter's
        // shape from the picture entirely, which is what makes the remaining
        // brightness mean something: a bin is bright because it is louder than
        // it usually is, not because it sits where the receiver happens to pass.
        //
        // **DOWN FAST, UP SLOWLY**, which is the standard minimum-tracking
        // estimator. A bin that goes quiet is noise and the floor should follow
        // it at once; a bin that goes loud may be a station and the floor must
        // not chase it, or a steady signal erases itself. At about twelve frames
        // a second the rise constant here is minutes, so a fifteen-second FT8
        // transmission never fades into its own floor.
        for (var i = 0; i < span; i++)
        {
            var db = ToDb(_magnitudes[_firstBin + i]);

            if (_binFloorDb[i] is double.NegativeInfinity)
            {
                _binFloorDb[i] = db;
            }
            else
            {
                var alpha = db < _binFloorDb[i] ? FloorFallsAt : FloorRisesAt;
                _binFloorDb[i] += (db - _binFloorDb[i]) * alpha;
            }

            // **DARK MEANS INDISTINGUISHABLE FROM THIS BIN'S OWN NOISE, AND
            // THAT ZERO IS MEASURED RATHER THAN CHOSEN.** A floor that follows
            // the level down quickly settles near the *minimum* of the noise,
            // not its typical level, so noise sits several decibels above its
            // own floor and the picture reads mid-grey everywhere. Measured:
            // with the floor alone, a recording holding nothing drew only 15 %
            // of its bins dark.
            //
            // The spread is how far this bin usually sits above its own floor,
            // averaged over time. Subtracting it puts the display zero at the
            // noise's ordinary level, so a bin has to beat its own noise to
            // brighten at all. Nothing here is a tuned constant: the number
            // comes from the audio.
            var over = db - _binFloorDb[i];

            _binSpreadDb[i] = _binSpreadDb[i] is double.NegativeInfinity
                ? over
                : _binSpreadDb[i] + ((over - _binSpreadDb[i]) * SpreadFollowsAt);

            var above = (over - _binSpreadDb[i]) / RangeDb;

            _bins[i] = (byte)Math.Clamp(above * 255, 0, 255);
        }

        // Derived from the sample count so a replay is identical to a live run.
        var at = DateTime.UnixEpoch.AddSeconds((double)_samplesSeen / _sampleRate);

        handler(new SpectrumFrame(
            (long)Math.Round(_firstBin * BinWidthHz),
            (long)Math.Round((_lastBin + 1) * BinWidthHz),
            at,
            _bins));
    }

    private static double ToDb(double magnitude)
        => 20 * Math.Log10(Math.Max(magnitude, 1e-12));

    /// <summary>Stop riding along.</summary>
    public void Dispose()
    {
        Listen(null);

        // Listen(null) already stops the worker; this is here for the case where
        // nothing was ever attached and a queue was still built.
        StopWorker();
    }
}

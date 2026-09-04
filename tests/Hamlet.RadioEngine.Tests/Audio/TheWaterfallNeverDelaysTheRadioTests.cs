using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 240, task 3: the device callback hands the waterfall its
/// samples and returns, whatever the picture is doing.
/// </summary>
/// <remarks>
/// <para>**A DROPPED WATERFALL ROW IS FREE AND DROPPED AUDIO IS NOT.** That is
/// the whole design, and these tests are the two halves of it: the callback
/// returns immediately even when the frame consumer has stopped entirely, and
/// when the queue behind it fills, the rows that are lost are counted rather
/// than silent.</para>
/// <para>**TASK 2 ALREADY TOOK `Push` FROM 62,271 MICROSECONDS TO 270, SO THIS
/// IS NOT WHERE THE TIME WENT.** It is where the *variance* lives. A
/// 16,384-point transform on the thread carrying the radio's audio is fine until
/// a collection, a descheduled core, or a `FrameReady` handler that touches the
/// UI - and then it costs samples nothing later can recover.</para>
/// </remarks>
public sealed class TheWaterfallNeverDelaysTheRadioTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the figures are printed.</param>
    public TheWaterfallNeverDelaysTheRadioTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;

    /// <summary>One 100 ms device buffer, the period unit 239 set.</summary>
    private const int Buffer = 4_800;

    /// <summary>How long a stalled frame handler sits there.</summary>
    private static readonly TimeSpan Stall = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// With the frame handler stalled, the device callback still returns in
    /// under a millisecond every time.
    /// </summary>
    /// <remarks>
    /// **THE SAME MEASUREMENT IS TAKEN BOTH WAYS IN ONE RUN.** The "before" arm
    /// calls `Push` synchronously, which is exactly what `OnSamples` used to do,
    /// so the comparison is between the two designs on one machine in one
    /// process rather than between two runs on two evenings.
    /// </remarks>
    [Fact]
    public void AStalledPictureCostsTheCallbackNothing()
    {
        var samples = Tone(Buffer);

        // --- the old design: OnSamples called Push in full, inline -----------
        var inline = new AudioSpectrumSource(Rate);
        var inlineFrames = 0;

        inline.FrameReady += (in SpectrumFrame _) =>
        {
            inlineFrames++;
            Thread.Sleep(Stall);
        };

        inline.Start();
        inline.Push(Tone(AudioSpectrumSource.WindowAt48K));

        var before = Worst(6, () => inline.Push(samples));

        // --- the new design: the callback offers and returns ------------------
        using var source = new FakeSource(Rate, Buffer);
        using var spectrum = new AudioSpectrumSource(Rate);
        var queuedFrames = 0;

        spectrum.FrameReady += (in SpectrumFrame _) =>
        {
            Interlocked.Increment(ref queuedFrames);
            Thread.Sleep(Stall);
        };

        spectrum.Start();
        spectrum.Listen(source);

        // Fill the ring through the real path, so the measured deliveries take
        // the same branch the old arm did.
        for (var i = 0; i < AudioSpectrumSource.WindowAt48K / Buffer + 1; i++)
        {
            source.Deliver(samples);
        }

        var after = Worst(6, () => source.Deliver(samples));

        _output.WriteLine("frame handler stalled for "
            + Stall.TotalMilliseconds + " ms on every frame");
        _output.WriteLine("");
        _output.WriteLine("  before, Push inline on the callback : "
            + before.ToString("0") + " us   (frames " + inlineFrames + ")");
        _output.WriteLine("  after, OnSamples offers and returns : "
            + after.ToString("0") + " us   (frames "
            + Volatile.Read(ref queuedFrames) + ")");
        _output.WriteLine("");
        _output.WriteLine("  the bound is 1,000 us; the buffer period is 100,000");

        // **THE OLD DESIGN HAS TO ACTUALLY FAIL THE BOUND**, or this test would
        // pass on a machine where the stall never happened and prove nothing.
        Assert.True(
            before > 1_000,
            "the inline arm returned in " + before.ToString("0")
            + " us, so the stall never reached it and there is nothing to compare");

        Assert.True(
            after < 1_000,
            "the device callback took " + after.ToString("0")
            + " us with the picture stalled, against a bound of 1,000 - the "
            + "waterfall is still able to delay the radio");
    }

    /// <summary>
    /// When the queue fills, rows are dropped and counted, and the audio path
    /// loses nothing.
    /// </summary>
    /// <remarks>
    /// **THE TAP IS FED BY A DIFFERENT SUBSCRIBER AND MUST NOT NOTICE.** That is
    /// the point of the whole arrangement: the waterfall falling behind is a
    /// picture problem, and it stays one.
    /// </remarks>
    [Fact]
    public void AFullQueueDropsRowsAndCountsThemWhileTheAudioSurvives()
    {
        using var source = new FakeSource(Rate, Buffer);
        using var spectrum = new AudioSpectrumSource(Rate);

        var tap = new AudioTap();

        // A second subscriber, standing in for the one that feeds the tap.
        source.SamplesReady += (in AudioChunk chunk)
            => tap.Take(chunk.Samples, chunk.SampleRate);

        spectrum.FrameReady += (in SpectrumFrame _) => Thread.Sleep(Stall);
        spectrum.Start();
        spectrum.Listen(source);

        var samples = Tone(Buffer);
        var delivered = 0L;

        // Three seconds of audio as fast as the machine will take it, which is
        // far faster than a handler sleeping 250 ms a frame can keep up with.
        for (var i = 0; i < 120; i++)
        {
            source.Deliver(samples);
            delivered += samples.Length;
        }

        _output.WriteLine("delivered      : " + delivered + " samples");
        _output.WriteLine("tap holds      : " + tap.SamplesSeen + " samples");
        _output.WriteLine("rows dropped   : " + spectrum.DroppedFrames);
        _output.WriteLine("their samples  : " + spectrum.DroppedFrameSamples);
        _output.WriteLine("worst frame    : "
            + spectrum.LongestFrameMicroseconds.ToString("0") + " us on the worker");

        Assert.True(
            spectrum.DroppedFrames > 0,
            "nothing was dropped, so the queue never filled and this proves "
            + "nothing about what happens when it does");

        // **NOT ONE SAMPLE.** The tap's subscriber ran on the same callback and
        // saw every delivery, because the waterfall's queue filling has nothing
        // to do with it.
        Assert.Equal(delivered, tap.SamplesSeen);
    }

    /// <summary>Detaching stops the worker and nothing outlives the source.</summary>
    /// <remarks>
    /// **IT COUNTS THIS CLASS'S OWN WORKERS AND NOT THE PROCESS'S THREADS.** The
    /// first version counted every thread in the process, passed alone, and
    /// failed beside the rest of the suite - the threads it saw appear belonged
    /// to other tests. A count the class keeps itself cannot be moved by
    /// anything running alongside it.
    /// </remarks>
    [Fact]
    public void NoWorkerOutlivesTheSource()
    {
        var before = AudioSpectrumSource.LiveWorkers;

        for (var i = 0; i < 5; i++)
        {
            using var source = new FakeSource(Rate, Buffer);
            using var spectrum = new AudioSpectrumSource(Rate);

            spectrum.Start();
            spectrum.Listen(source);
            source.Deliver(Tone(Buffer));

            Assert.True(
                AudioSpectrumSource.LiveWorkers > before,
                "attaching started no worker, so detaching proves nothing");

            spectrum.Listen(null);
        }

        // A worker that has been told to stop is on its way out; give it a
        // bounded moment to finish leaving rather than asserting on a race.
        var deadline = Stopwatch.GetTimestamp() + (Stopwatch.Frequency * 3);

        while (AudioSpectrumSource.LiveWorkers > before
            && Stopwatch.GetTimestamp() < deadline)
        {
            Thread.Sleep(20);
        }

        _output.WriteLine("live frame workers before : " + before);
        _output.WriteLine("after five attach/detach  : "
            + AudioSpectrumSource.LiveWorkers);

        Assert.Equal(before, AudioSpectrumSource.LiveWorkers);
    }

    private static double Worst(int times, Action work)
    {
        var worst = 0.0;

        for (var i = 0; i < times; i++)
        {
            var started = Stopwatch.GetTimestamp();
            work();
            var micros = (Stopwatch.GetTimestamp() - started) * 1_000_000.0
                / Stopwatch.Frequency;

            if (micros > worst)
            {
                worst = micros;
            }
        }

        return worst;
    }

    private static float[] Tone(int count)
    {
        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            samples[i] = MathF.Sin(i * 0.09f) * 0.3f;
        }

        return samples;
    }

    /// <summary>A source that raises `SamplesReady` synchronously, as WASAPI does.</summary>
    private sealed class FakeSource : IAudioSource
    {
        private readonly float[] _scratch;
        private long _delivered;

        public FakeSource(int sampleRate, int chunk)
        {
            SampleRate = sampleRate;
            _scratch = new float[chunk];
        }

        public int SampleRate { get; }

        public string DeviceName => "fake";

        public bool IsSimulated => true;

        public bool IsRunning { get; private set; } = true;

        public event AudioChunkHandler? SamplesReady;

        public void Start() => IsRunning = true;

        public void Stop() => IsRunning = false;

        public void Dispose() => Stop();

        /// <summary>Deliver one buffer the way the device callback delivers one.</summary>
        public void Deliver(float[] samples)
        {
            // **THROUGH THE SCRATCH ARRAY ON PURPOSE.** `AudioChunk` is a ref
            // struct over the source's own reused buffer, valid only for the
            // call - so a consumer that kept the span instead of copying would
            // be caught here rather than on the radio.
            samples.AsSpan(0, Math.Min(samples.Length, _scratch.Length))
                .CopyTo(_scratch);

            var chunk = new AudioChunk(_delivered, SampleRate, _scratch);

            _delivered += _scratch.Length;
            SamplesReady?.Invoke(in chunk);
        }
    }
}

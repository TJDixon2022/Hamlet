using System.Diagnostics;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Audio;

/// <summary>
/// Work instruction 238, task 2: the tap is fed from the device's callback and
/// the decoder is fed from a bounded queue behind it.
/// </summary>
/// <remarks>
/// <para>**THE FAULT.** `CwDecoder.Process` was the tap's only feed, and it ran
/// the tracker, the mixer and the probabilistic decoder on the callback thread
/// before returning. So the tap filled at whatever fraction of real time the CW
/// decode happened to run at. Measured on the shack machine 2026-09-03: **13%**,
/// with four consecutive press captures byte-identical prefixes of one another.
/// FT8 needs 12.64 s of continuous audio and was being handed fragments spanning
/// two minutes.</para>
/// <para>**WHAT IS ASSERTED HERE IS THE PROPERTY, NOT THE SPEED.** A throughput
/// threshold would fail on a slower machine and prove nothing about the design.
/// What these assert is that the tap receives every sample **while a decoder
/// that cannot keep up is attached**, that the callback returns without waiting
/// for it, and that the samples the queue could not carry are counted rather
/// than lost.</para>
/// </remarks>
public sealed class TheTapIsNotBehindTheDecoderTests
{
    private readonly ITestOutputHelper _output;

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the measurements are printed.</param>
    public TheTapIsNotBehindTheDecoderTests(ITestOutputHelper output)
        => _output = output;

    private const int Rate = 48_000;
    private const int Chunk = 960;

    /// <summary>
    /// Every sample a source delivers reaches the tap, with a decoder attached
    /// that is far slower than real time, and the callback never waits for it.
    /// </summary>
    /// <remarks>
    /// **THE CALLBACK BOUND IS ONE BUFFER DURATION.** 960 samples at 48 kHz is
    /// 20 ms, and a callback that takes longer than the audio it carries is a
    /// callback the device is queueing behind. It is asserted generously at one
    /// buffer because this is a shared machine running a test suite; the
    /// measured figure is printed so a regression shows as a number rather than
    /// only as a pass.
    /// </remarks>
    [Fact]
    public void TheTapIsWholeWhileTheDecoderCrawls()
    {
        using var source = new FakeSource(Rate);
        var decoder = new CwDecoder(Rate, 600);

        // **THE DELAY MUST EXCEED ONE BUFFER OR THE ASSERTION CANNOT
        // DISCRIMINATE.** 30 ms of stall against 20 ms of audio is a decoder at
        // two thirds of real time. Watched against the old inline design at
        // 5 ms this test PASSED, because a 5 ms inline decode still returns
        // inside a 20 ms buffer - the test proved nothing and looked like it
        // proved something, which is the failure section 12.5 is about.
        decoder.ProcessDelayForTests = TimeSpan.FromMilliseconds(30);
        decoder.Listen(source);

        const int chunks = 100;
        var worst = 0.0;

        for (var i = 0; i < chunks; i++)
        {
            var started = Stopwatch.GetTimestamp();
            source.Deliver(Chunk);
            var micros = (Stopwatch.GetTimestamp() - started) * 1_000_000.0 / Stopwatch.Frequency;

            worst = Math.Max(worst, micros);
        }

        decoder.Listen(null);

        var expected = (long)chunks * Chunk;
        var bufferMicros = 1_000_000.0 * Chunk / Rate;

        _output.WriteLine("delivered   : " + expected + " samples in " + chunks + " chunks");
        _output.WriteLine("tap saw     : " + decoder.Tap.SamplesSeen);
        _output.WriteLine("worst call  : " + worst.ToString("0") + " us against a "
            + bufferMicros.ToString("0") + " us buffer");
        _output.WriteLine("queue drops : " + decoder.DecodeQueueDroppedChunks + " chunks");

        Assert.Equal(expected, decoder.Tap.SamplesSeen);
        Assert.True(
            worst < bufferMicros,
            "a callback took " + worst.ToString("0") + " us, longer than the "
            + bufferMicros.ToString("0") + " us of audio it carried - the device is "
            + "queueing behind the decoder, which is the fault this unit exists against");
    }

    /// <summary>
    /// With the decoder slower than real time for long enough to fill the queue,
    /// the drop counter moves and the tap is still whole.
    /// </summary>
    /// <remarks>
    /// **A DROPPED CHUNK THAT LEAVES NO NUMBER IS THE FAULT THIS UNIT IS ABOUT**
    /// (HM-DEC-093). This is the assertion that the drop is counted rather than
    /// silent, and it is deliberately paired with the tap assertion: losing
    /// decode audio while keeping the tap whole is the trade the design makes,
    /// and both halves have to be true for it to be the right one.
    /// </remarks>
    [Fact]
    public void AFullQueueDropsAndCounts()
    {
        using var source = new FakeSource(Rate);
        var decoder = new CwDecoder(Rate, 600);

        decoder.ProcessDelayForTests = TimeSpan.FromMilliseconds(20);
        decoder.Listen(source);

        // Three seconds of queue at 960-sample chunks is 150; deliver well past
        // it, faster than a 20 ms-per-chunk consumer can possibly drain.
        const int chunks = 400;

        for (var i = 0; i < chunks; i++)
        {
            source.Deliver(Chunk);
        }

        var dropped = decoder.DecodeQueueDroppedChunks;
        var droppedSamples = decoder.DecodeQueueDroppedSamples;
        var seen = decoder.Tap.SamplesSeen;

        decoder.Listen(null);

        _output.WriteLine("delivered      : " + (long)chunks * Chunk + " samples");
        _output.WriteLine("tap saw        : " + seen);
        _output.WriteLine("queue dropped  : " + dropped + " chunks, "
            + droppedSamples + " samples");

        Assert.Equal((long)chunks * Chunk, seen);
        Assert.True(dropped > 0,
            "the queue never dropped, so this test proved nothing about counting drops");
        Assert.Equal(dropped * Chunk, droppedSamples);
    }

    /// <summary>
    /// `Process` with no source attached still feeds the tap, and with a source
    /// attached the tap sees each sample exactly once.
    /// </summary>
    /// <remarks>
    /// **THE FIXTURE PATH IS LOAD-BEARING.** Most of this repository's CW
    /// evidence is a test calling `Process` directly with no source, and a tap
    /// that only filled through the callback would silently empty every one of
    /// them. The second half is the other error: tapping in both the callback
    /// and the worker would double every sample FT8 reads, which would look like
    /// a working tap and be a corrupt recording.
    /// </remarks>
    [Fact]
    public void TheTapIsFedOnceWhicheverWayTheAudioArrives()
    {
        var direct = new CwDecoder(Rate, 600);
        var samples = new float[Chunk];

        direct.Process(new AudioChunk(0, Rate, samples));

        _output.WriteLine("no source, one Process call -> tap " + direct.Tap.SamplesSeen);
        Assert.Equal(Chunk, direct.Tap.SamplesSeen);

        using var source = new FakeSource(Rate);
        var attached = new CwDecoder(Rate, 600);

        attached.Listen(source);
        source.Deliver(Chunk);
        source.Deliver(Chunk);
        attached.Listen(null);

        _output.WriteLine("source attached, two chunks -> tap " + attached.Tap.SamplesSeen);
        Assert.Equal(2 * Chunk, attached.Tap.SamplesSeen);
    }

    /// <summary>A source that delivers on the caller's thread, like WASAPI does.</summary>
    private sealed class FakeSource : IAudioSource
    {
        private readonly float[] _buffer;
        private long _delivered;

        public FakeSource(int sampleRate)
        {
            SampleRate = sampleRate;
            _buffer = new float[8192];
        }

        public int SampleRate { get; }

        public string DeviceName => "fake";

        public bool IsSimulated => true;

        public bool IsRunning { get; private set; }

        public event AudioChunkHandler? SamplesReady;

        public void Start() => IsRunning = true;

        public void Stop() => IsRunning = false;

        public void Dispose() => Stop();

        /// <summary>Deliver one chunk, synchronously, as the device callback does.</summary>
        public void Deliver(int count)
        {
            var chunk = new AudioChunk(_delivered, SampleRate, _buffer.AsSpan(0, count));

            _delivered += count;
            SamplesReady?.Invoke(in chunk);
        }
    }
}

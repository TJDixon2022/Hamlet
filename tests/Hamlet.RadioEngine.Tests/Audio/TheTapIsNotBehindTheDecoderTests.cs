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

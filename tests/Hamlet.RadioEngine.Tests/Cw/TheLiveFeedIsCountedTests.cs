using System.Diagnostics;
using System.Text;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Cw;

/// <summary>
/// **THE LIVE FEED** (work instruction 548, tasks 1 and 2): the CW chain behind a queue, off the capture's thread, reads what
/// the chain reads; and what the live path received and lost is counted.
/// </summary>
/// <remarks>R88 is lifted for W1AW's capture of 2026-10-06; the counters' case is synthetic.</remarks>
public sealed class TheLiveFeedIsCountedTests(ITestOutputHelper output)
{
    /// <summary>A source that delivers on the caller's thread, as WASAPI does on its own.</summary>
    private sealed class FakeSource(int rate) : IAudioSource
    {
        public int SampleRate { get; } = rate;

        public string DeviceName => "fake";

        public bool IsSimulated => true;

        public bool IsRunning { get; private set; }

        public event AudioChunkHandler? SamplesReady;

        public void Start() => IsRunning = true;

        public void Stop() => IsRunning = false;

        public void Dispose() => Stop();

        public void Deliver(long first, ReadOnlySpan<float> samples)
        {
            var chunk = new AudioChunk(first, SampleRate, samples);

            SamplesReady?.Invoke(in chunk);
        }
    }

    /// <remarks>
    /// Task 1: W1AW's bulletin delivered in the live capture's 50 ms chunks through the feed reads exactly what the chain reads
    /// fed directly, and the tap holds every sample once.
    /// </remarks>
    [Fact]
    public void ThroughTheQueueItReadsWhatTheChainReads()
    {
        var audio = WavAudio.Read(TheOwnersRecordingReadsTests.Wav("cw-2026-10-06-212015"));
        var (pitch, width) = TheRecordingsScoreboardTests.RadioState("cw-2026-10-06-212015");
        var rate = audio.SampleRate;
        var chunk = rate / 20;

        string Direct()
        {
            using var chain = new CwChain(rate);
            var text = new StringBuilder();

            chain.Detector.SetPassband(pitch, width);
            chain.Decoder.CharacterSettled += c => text.Append(c.Text);

            for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
            {
                chain.Process(new AudioChunk(at, rate, audio.Samples.AsSpan(at, chunk)));
            }

            chain.Decoder.Flush();

            return text.ToString();
        }

        var decoder = new CwDecoder(rate);
        var detector = new CwEnvelopeDetector(rate);
        var queued = new StringBuilder();
        var source = new FakeSource(rate);
        var longest = 0.0;

        CwChain.Wire(decoder, detector);
        detector.SetPassband(pitch, width);
        decoder.CharacterSettled += c => queued.Append(c.Text);

        using (var feed = new CwLiveFeed(decoder, detector))
        {
            feed.Listen(source);

            for (var at = 0; at + chunk <= audio.Samples.Length; at += chunk)
            {
                var started = Stopwatch.GetTimestamp();

                source.Deliver(at, audio.Samples.AsSpan(at, chunk));
                longest = Math.Max(longest, Stopwatch.GetElapsedTime(started).TotalMilliseconds);

                // Paced as a device paces it: the next chunk comes once this one is decoded, never faster than the chain reads.
                Assert.True(feed.WaitUntilDrained(TimeSpan.FromSeconds(10)));
            }

            Assert.True(feed.WaitUntilDrained(TimeSpan.FromSeconds(60)));

            output.WriteLine($"the longest callback through the feed: {longest:0.00} ms; continuity {feed.Continuity.SheetLine}");
        }

        decoder.Flush();

        var direct = Direct();

        output.WriteLine($"direct `{direct}`");
        output.WriteLine($"queued `{queued}`");

        Assert.Equal(direct, queued.ToString());
        Assert.Equal(audio.Samples.Length / chunk * chunk, decoder.Tap.SamplesSeen);
    }

    /// <remarks>
    /// Task 2: a fake capture on a fake clock, 50 ms chunks every 50 ms, then 400 ms in which the device delivers nothing, then
    /// a chunk whose place on the audio clock jumps a second ahead, as one after a dropped chunk does: the counters read the
    /// lost audio past one device buffer of clock jitter - the gap lost 350 ms, since the chunk after it carries 50, and less
    /// the 100 ms buffer the count can say 250 for certain - the 400 ms stall, and the hole.
    /// </remarks>
    [Fact]
    public void TheCountersReadWhatWasDropped()
    {
        const int rate = 8000;
        var ticks = 0L;
        var decoder = new CwDecoder(rate);
        var detector = new CwEnvelopeDetector(rate);
        var source = new FakeSource(rate);
        var chunk = new float[rate / 20];
        var at = 0L;

        CwChain.Wire(decoder, detector);

        using var feed = new CwLiveFeed(decoder, detector, () => ticks);

        feed.Listen(source);

        void Step(double ms) => ticks += (long)(ms / 1000 * Stopwatch.Frequency);

        for (var i = 0; i < 20; i++)
        {
            source.Deliver(at, chunk);
            at += chunk.Length;
            Step(50);
        }

        var before = feed.Continuity;

        // The device delivers nothing for 400 ms more.
        Step(400);
        source.Deliver(at, chunk);
        at += chunk.Length;
        Step(50);

        // A chunk whose place jumps a second ahead: one the queue would have dropped before it.
        at += rate;
        source.Deliver(at, chunk);
        at += chunk.Length;

        Assert.True(feed.WaitUntilDrained(TimeSpan.FromSeconds(10)));

        var after = feed.Continuity;

        output.WriteLine($"before the gap: {before.SheetLine}; lost in the last minute {before.LostLastMinuteMilliseconds:0} ms");
        output.WriteLine($"after: {after.SheetLine}; lost in the last minute {after.LostLastMinuteMilliseconds:0} ms; chunks {after.ChunksReceived}, holes {after.Holes}");

        Assert.Equal(0, before.LostMilliseconds);
        Assert.InRange(after.LongestStallMilliseconds, 399, 401);
        Assert.InRange(after.LostMilliseconds, 249, 251);
        Assert.Equal(after.LostMilliseconds, after.LostLastMinuteMilliseconds);
        Assert.Equal(22, after.ChunksReceived);
        Assert.Equal(1, after.Holes);
        Assert.Equal(0, after.ChunksDropped);
    }
}

using System.Diagnostics;
using System.Text.Json;
using Hamlet.App.Telemetry;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Telemetry;
using Hamlet.RadioEngine.Training;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.App.Tests.Telemetry;

/// <summary>
/// **THE TELEMETRY SAYS WHAT IS HAPPENING, EVERY TEN SECONDS** (work instruction 549, task 3, HM-DEC-253): a minute of
/// <c>cw_listen</c> rows from the live path, on synthetic Morse, a fake capture and a fake clock. No recording is read.
/// </summary>
public sealed class TheListenRowTests(ITestOutputHelper output)
{
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

    private sealed class Rows : ITelemetry
    {
        public List<(string Event, IReadOnlyDictionary<string, object?> Data)> All { get; } = [];

        public long DroppedEventCount => 0;

        public void Write(
            TelemetryCategory category, string eventName, IReadOnlyDictionary<string, object?>? data = null, TelemetryLevel level = TelemetryLevel.Info)
            => All.Add((eventName, data ?? new Dictionary<string, object?>()));
    }

    [Fact]
    public void AMinuteOfRows()
    {
        const int rate = 8000;

        // A station at 600 Hz for the minute, a second at 760 Hz from twenty seconds, and noise over both.
        var first = CwSignal.Generate(new CwSignalRequest(
            string.Join(' ', Enumerable.Repeat("CQ CQ DE K1ABC K1ABC K", 6)), WordsPerMinute: 18, ToneHz: 600, SampleRate: rate,
            Amplitude: 0.4, NoiseAmplitude: 0.05, LeadInSeconds: 0.5, TailSeconds: 0, Seed: 5491)).Samples;
        var second = CwSignal.Generate(new CwSignalRequest(
            string.Join(' ', Enumerable.Repeat("TEST DE N2XYZ K", 6)), WordsPerMinute: 22, ToneHz: 760, SampleRate: rate,
            Amplitude: 0.3, LeadInSeconds: 20, TailSeconds: 0, Seed: 5492)).Samples;
        var audio = new float[60 * rate];

        for (var i = 0; i < audio.Length; i++)
        {
            audio[i] = (i < first.Length ? first[i] : 0) + (i < second.Length ? second[i] : 0);
        }

        var ticks = 0L;
        var decoder = new CwDecoder(rate);
        var detector = new CwEnvelopeDetector(rate);

        CwChain.Wire(decoder, detector);

        var source = new FakeSource(rate);
        using var feed = new CwLiveFeed(decoder, detector, () => ticks);

        feed.Listen(source);

        var sampler = new CwListenSampler();
        var rows = new Rows();
        var start = new DateTime(2026, 10, 7, 21, 15, 0, DateTimeKind.Utc);
        var chunk = rate / 20;

        for (var at = 0; at + chunk <= audio.Length; at += chunk)
        {
            ticks += Stopwatch.Frequency / 20;

            // Half a second the capture never delivered, at thirty-four seconds.
            if (at >= 34 * rate && at < (34 * rate) + (rate / 2))
            {
                continue;
            }

            source.Deliver(at, audio.AsSpan(at, chunk));

            if ((at + chunk) % rate == 0)
            {
                Assert.True(feed.WaitUntilDrained(TimeSpan.FromSeconds(10)));

                var now = start.AddSeconds((at + chunk) / rate);

                if (sampler.Sample(now, feed.Continuity, decoder.ShapeSide, "reading", now.Second < 30 ? null : "w1aw-2026-10-07-205900") is { } sample)
                {
                    AppEvents.CwListen(rows, sample);
                }
            }
        }

        foreach (var (_, data) in rows.All)
        {
            output.WriteLine(JsonSerializer.Serialize(data));
        }

        Assert.Equal(6, rows.All.Count);
        Assert.All(rows.All, r => Assert.Equal("cw_listen", r.Event));
        Assert.Contains(rows.All, r => (long)r.Data["lettersPrinted10s"]! > 0);
        Assert.Contains(rows.All, r => (double)r.Data["audioLostLast10sMs"]! > 0);
        Assert.Contains(rows.All, r => r.Data["printedPitchHz"] is not null);
        Assert.All(rows.All, r => Assert.Equal(
            (int)r.Data["sendersHeld"]!, ((System.Collections.ICollection)r.Data["senders"]!).Count));
    }
}

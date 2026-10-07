using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Bands;
using Hamlet.RadioEngine.Capture;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Capture;

/// <summary>
/// **EVERY W1AW SESSION, CAPTURED WHOLE** (work instruction 549, task 1, HM-DEC-253), on a fake clock, a fake rig state and
/// fake audio. No recording is read.
/// </summary>
/// <remarks>
/// The audio runs at 100 samples a second so five minutes is 30,000 samples; a piece is cut by its sample count at any rate,
/// and the chunks are 37 samples so that every boundary falls inside one.
/// </remarks>
public sealed class EveryW1awSessionIsCapturedTests : IDisposable
{
    private const int Rate = 100;
    private const int Chunk = 37;
    private const long W1aw40m = 7_047_500;

    // Wednesday 2026-10-07: slow code practice 08:00 to 09:00 US Central, 13:00 to 14:00 UTC; nothing the evening before
    // after 23:00 Central, so its window, 12:59 to 14:02 UTC, stands alone.
    private static readonly DateTime WindowOpens = new(2026, 10, 7, 12, 59, 0, DateTimeKind.Utc);

    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-549-" + Guid.NewGuid().ToString("N"));
    private DateTime _now;
    private long _index;

    public EveryW1awSessionIsCapturedTests(ITestOutputHelper output) => _output = output;

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    [Fact]
    public void TheSessionsStartBeginsTheCaptureAndItsPiecesAreContinuous()
    {
        var auto = new CwAutoCapture(_folder, W1awMorseFrequencies.Default, () => _now, threaded: false)
        {
            SheetExtra = () => "frequency  7047500 Hz  (the fake rig)",
        };

        // Ten seconds before the window opens: nothing.
        _now = WindowOpens.AddSeconds(-10);
        auto.Tick(OnW1aw());
        Play(auto, seconds: 9);
        Assert.Null(auto.W1awCapture);
        Assert.Equal(string.Empty, auto.Line);

        _now = WindowOpens;
        auto.Tick(OnW1aw());
        Assert.NotNull(auto.W1awCapture);
        Assert.Equal("capturing W1AW · code practice · piece 1 · 0:00", auto.Line);

        var first = _index;

        // Twelve minutes, ticking once a second as the app does.
        Play(auto, seconds: 12 * 60);
        _output.WriteLine($"after twelve minutes the line reads `{auto.Line}`");
        Assert.Equal("capturing W1AW · code practice · piece 3 · 12:00", auto.Line);

        // The dial moves to 7.030: the capture ends there.
        auto.Tick(OnW1aw() with { FrequencyHz = 7_030_000 });
        Assert.Null(auto.W1awCapture);

        var folder = Assert.Single(Directory.GetDirectories(_folder));
        Assert.Equal("w1aw-2026-10-07-125900", Path.GetFileName(folder));

        var wavs = Directory.GetFiles(folder, "*.wav").Order().ToList();
        Assert.Equal(["piece-01.wav", "piece-02.wav", "piece-03.wav"], wavs.Select(Path.GetFileName));

        var pieces = wavs.Select(WavAudio.Read).ToList();
        Assert.Equal(30_000, pieces[0].Samples.Length);
        Assert.Equal(30_000, pieces[1].Samples.Length);

        // **CONTINUOUS ACROSS EVERY BOUNDARY**: the pieces add up to every sample fed, sample for sample.
        var all = pieces.SelectMany(p => p.Samples).ToArray();
        Assert.Equal(_index - first, all.Length);

        for (var i = 0; i < all.Length; i++)
        {
            Assert.Equal(Quantized(Ramp(first + i)), all[i], 4);
        }

        var sheets = Directory.GetFiles(folder, "*.txt").Order().Select(File.ReadAllText).ToList();

        for (var n = 0; n < 3; n++)
        {
            Assert.Contains("session    code practice · slow · scheduled 2026-10-07 13:00 to 14:00 UTC (08:00 to 09:00 America/Chicago)", sheets[n]);
            Assert.Contains($"piece      {n + 1}", sheets[n]);
            Assert.Contains("missing    0 samples", sheets[n]);
            Assert.Contains("frequency  7047500 Hz  (the fake rig)", sheets[n]);
        }

        Assert.Contains($"samples    {first} to {first + 30_000} on the audio clock", sheets[0]);
        Assert.Contains($"samples    {first + 30_000} to {first + 60_000} on the audio clock", sheets[1]);
        Assert.Contains("continues  piece-02.wav", sheets[0]);
        Assert.Contains("ended      the dial left W1AW's frequency", sheets[2]);

        _output.WriteLine("piece-02.txt:");
        _output.WriteLine(sheets[1]);
    }

    [Fact]
    public void TheWindowClosingEndsItAndListeningStoppingEndsIt()
    {
        var auto = new CwAutoCapture(_folder, W1awMorseFrequencies.Default, () => _now, threaded: false);

        _now = WindowOpens.AddMinutes(30);
        auto.Tick(OnW1aw());
        Play(auto, seconds: 3);
        _now = new DateTime(2026, 10, 7, 14, 1, 59, DateTimeKind.Utc);
        auto.Tick(OnW1aw());
        Assert.NotNull(auto.W1awCapture);

        _now = new DateTime(2026, 10, 7, 14, 2, 0, DateTimeKind.Utc);
        auto.Tick(OnW1aw());
        Assert.Null(auto.W1awCapture);

        var closed = Assert.Single(Directory.GetDirectories(_folder));
        Assert.Contains(
            "ended      the session's window closed, two minutes after its scheduled end",
            File.ReadAllText(Path.Combine(closed, "piece-01.txt")));

        // A second session, listening stopped part way through.
        Directory.Delete(closed, recursive: true);
        _now = WindowOpens.AddMinutes(10);
        auto.Tick(OnW1aw());
        Play(auto, seconds: 2);
        auto.Tick(OnW1aw() with { Listening = false });
        Assert.Null(auto.W1awCapture);
        Assert.Contains(
            "ended      listening stopped",
            File.ReadAllText(Path.Combine(Assert.Single(Directory.GetDirectories(_folder)), "piece-01.txt")));
    }

    [Fact]
    public void BehindItsQueueEverySampleStillReachesTheDisk()
    {
        // As the app runs it: the audio handed over on the caller's thread, written on the capture's own.
        var auto = new CwAutoCapture(_folder, W1awMorseFrequencies.Default, () => _now, threaded: true);

        _now = WindowOpens.AddMinutes(1);
        auto.Tick(OnW1aw());
        var first = _index;
        Play(auto, seconds: 7 * 60);
        auto.Dispose();

        var folder = Assert.Single(Directory.GetDirectories(_folder));
        var last = Path.Combine(folder, "piece-02.txt");
        var deadline = DateTime.UtcNow.AddSeconds(10);

        while (!(File.Exists(last) && File.ReadAllText(last).Contains("ended ", StringComparison.Ordinal)) && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(20);
        }

        var all = Directory.GetFiles(folder, "*.wav").Order().Select(WavAudio.Read).SelectMany(p => p.Samples).ToArray();

        Assert.Equal(_index - first, all.Length);
        Assert.Equal(Quantized(Ramp(first + 30_000)), all[30_000], 4);
        Assert.Contains("ended      listening stopped", File.ReadAllText(last));
    }

    [Fact]
    public void NoSessionMeansNoCapture()
    {
        var auto = new CwAutoCapture(_folder, W1awMorseFrequencies.Default, () => _now, threaded: false);

        // 13:00 US Central on a Wednesday: nothing is scheduled.
        _now = new DateTime(2026, 10, 7, 18, 0, 0, DateTimeKind.Utc);
        auto.Tick(OnW1aw());
        Play(auto, seconds: 30);

        // In a session's window but not on W1AW's frequency, scanning, or not in CW: none either.
        _now = WindowOpens.AddMinutes(5);
        auto.Tick(OnW1aw() with { FrequencyHz = 7_030_000 });
        auto.Tick(OnW1aw() with { Scanning = true });
        auto.Tick(OnW1aw() with { InCw = false });
        Play(auto, seconds: 5, tick: false);

        Assert.Null(auto.W1awCapture);
        Assert.False(Directory.Exists(_folder) && Directory.EnumerateFileSystemEntries(_folder).Any());
    }

    [Fact]
    public void TheDialWithinHalfTheFilterIsOnW1aw()
    {
        var table = W1awMorseFrequencies.Default;

        Assert.NotNull(W1awSessionWindow.RowAt(table, W1aw40m + 250, filterWidthHz: 500));
        Assert.Null(W1awSessionWindow.RowAt(table, W1aw40m + 251, filterWidthHz: 500));
        Assert.NotNull(W1awSessionWindow.RowAt(table, W1aw40m - 250, filterWidthHz: null));
        Assert.Equal("20 m", W1awSessionWindow.RowAt(table, 14_047_400, filterWidthHz: 250)!.Band);
    }

    private static AutoCaptureConditions OnW1aw() => new(Listening: true, InCw: true, Scanning: false, W1aw40m, 500);

    private static float Ramp(long i) => (i % 1000 / 1000f) - 0.5f;

    private static float Quantized(float s) => (float)Math.Round(s * short.MaxValue) / short.MaxValue;

    // Feed audio a chunk at a time, the clock moving with it and the tick once a second, as the app's would.
    private void Play(CwAutoCapture auto, int seconds, bool tick = true)
    {
        var end = _index + (seconds * Rate);
        var buffer = new float[Chunk];
        var nextTick = _now.AddSeconds(1);

        while (_index < end)
        {
            var n = (int)Math.Min(Chunk, end - _index);

            for (var i = 0; i < n; i++)
            {
                buffer[i] = Ramp(_index + i);
            }

            auto.Hear(_index, Rate, buffer.AsSpan(0, n));
            _index += n;
            _now = _now.AddSeconds(n / (double)Rate);

            if (_now >= nextTick)
            {
                // A second of audio arrives in a second; fed faster, the queue is let catch up as real time would.
                Assert.True(auto.WaitUntilWritten(TimeSpan.FromSeconds(5)));

                if (tick)
                {
                    auto.Tick(OnW1aw());
                }

                nextTick = nextTick.AddSeconds(1);
            }
        }
    }
}

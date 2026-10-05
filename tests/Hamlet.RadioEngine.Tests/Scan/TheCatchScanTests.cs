using System.Text.Json;
using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Cw;
using Hamlet.RadioEngine.Licensing;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Scan;
using Hamlet.RadioEngine.Training;
using Hamlet.RadioEngine.Transmit;
using Hamlet.RadioEngine.Transport;
using Xunit;
using Xunit.Abstractions;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// **THE SCAN: CATCH CW UNATTENDED, POSITIVES AND NEGATIVES, LISTEN ONLY** (work instruction 540, HM-DEC-244), on a fake
/// radio: a rig that answers and screams if anything keys it, a scope that shows three peaks on 40 m, and audio that
/// depends on where the dial is - a clean CW call at the first, band noise at the second, a steady carrier at the third.
/// </summary>
/// <remarks>
/// Time is virtual: the scan's delay advances a clock and feeds the audio, the scope and the rig's own reports for that
/// long, so a thirty-minute scan runs in seconds and the same code runs on the air (§5).
/// </remarks>
public sealed class TheCatchScanTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "hamlet-scan-tests", Guid.NewGuid().ToString("N"));

    /// <summary>Creates the tests.</summary>
    /// <param name="output">Where the scan's files are printed.</param>
    public TheCatchScanTests(ITestOutputHelper output) => _output = output;

    /// <inheritdoc/>
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

    internal const long Home = 7_031_000;
    internal const long Call = 7_010_000;
    internal const long Noise = 7_030_000;
    internal const long Carrier = 7_050_000;

    internal static readonly CwScanSettings Short = new(TimeSpan.FromMinutes(4), TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(30));

    /// <remarks>
    /// Tests 1 and 3: the scan visits all three peaks in frequency order, catches the call as a positive, the noise as empty
    /// (no tone to hear, work instruction 542) and the carrier as a negative - a tone with no shape - and
    /// leaves the negative at 30 s, and writes each catch's WAV and JSON and the scan's
    /// scan.json. The dial is put back where it was.
    /// </remarks>
    [Fact]
    public async Task ThreePeaksAreVisitedAndCaught()
    {
        using var world = await ScanWorld.Ready(keepsSending: true);
        var (scan, summary) = await world.Run(_folder, Short);

        Print(scan, summary);

        Assert.True(summary.Catches.Count >= 3, $"{summary.Catches.Count} catches");
        Assert.Equal([Call, Noise, Carrier], summary.Catches.Take(3).Select(c => Nearest(c.SignalHz)));
        Assert.Equal([CatchKind.Positive, CatchKind.Empty, CatchKind.Negative], summary.Catches.Take(3).Select(c => c.Kind));
        Assert.Equal(CwScanEnd.LengthReached, summary.Ended);

        foreach (var entry in summary.Catches)
        {
            Assert.True(File.Exists(Path.Combine(scan.ScanFolder!, entry.Wav)), entry.Wav);
            Assert.True(File.Exists(Path.Combine(scan.ScanFolder!, entry.Json)), entry.Json);
        }

        var empty = Read<CwCatch>(scan, summary.Catches[1].Json);

        // Steady noise at the scope's peak, no tone at it nor either side: empty, left after the three tries (work instruction 542).
        Assert.Equal(CatchLeft.NothingHeard, empty.Left);
        Assert.InRange((empty.EndUtc - empty.StartUtc).TotalSeconds, 0, 8);

        var negative = Read<CwCatch>(scan, summary.Catches[2].Json);

        Assert.Equal(CatchLeft.StayRanOut, negative.Left);
        Assert.InRange((negative.EndUtc - negative.StartUtc).TotalSeconds, 30, 31);
        Assert.InRange(negative.Seconds, 29, 31.5);
        Assert.Equal(ScanWorld.Rate, WavAudio.Read(Path.Combine(scan.ScanFolder!, negative.Wav)).SampleRate);

        var positive = Read<CwCatch>(scan, summary.Catches[0].Json);

        Assert.Contains(positive.Stations, s => s.Printed);
        Assert.Contains("W1AW", positive.Text.Replace(" ", ""), StringComparison.Ordinal);
        Assert.NotEmpty(positive.Letters);
        Assert.Contains(positive.Lights, l => l.Light is "reading" or "shape found");
        Assert.Contains(positive.Radio, f => f.Field == nameof(RigField.Frequency) && f.Known);
        Assert.True(File.Exists(Path.Combine(scan.ScanFolder!, "scan.json")));
        Assert.Equal(Home, world.Rig.FrequencyHz);
        Assert.Equal(0, world.Rig.KeyingAttempts);
    }

    /// <remarks>
    /// Work instruction 542, task 3, and 544, task 2: **landing by ear, and the tone at the pitch before the stay.** The
    /// scope draws the call's peak off the call, by a bin's error at a wide span or more; the scan lands there, hears the call
    /// (at the peak, or half a filter either side), and moves the dial until the tone sits at the 600 Hz pitch, within 15 Hz,
    /// measured again after each move. On a radio whose tone rises with the dial, as the owner's does in CW, and on one
    /// whose tone falls, which the scan learns from the audio. The empty stop beside it costs the three tries, not 30 s.
    /// </remarks>
    [Theory]
    [InlineData(260, 1)]
    [InlineData(260, -1)]
    [InlineData(150, 1)]
    [InlineData(150, -1)]
    [InlineData(200, 1)]
    public async Task TheScanLandsByEar(int offsetHz, int toneSign)
    {
        using var world = await ScanWorld.Ready(keepsSending: true);

        world.Scope.CallOffsetHz = offsetHz;
        world.Scope.BinCount = 4750;
        world.Scope.PeakHalfWidthHz = 20;
        world.ToneSign = toneSign;

        var (scan, summary) = await world.Run(_folder, Short with { Length = TimeSpan.FromSeconds(140) });
        var first = Read<CwCatch>(scan, summary.Catches[0].Json);
        var empty = Read<CwCatch>(scan, summary.Catches[1].Json);
        var heardAt = 600 + (toneSign * (first.DialHz - Call));

        _output.WriteLine($"scope {offsetHz} Hz off, tone {(toneSign > 0 ? "rising" : "falling")} with the dial: scope peak {first.ScopeHz} Hz, tone heard {first.ToneHz:0.0} Hz, after {first.ToneAfterHz:0.0} Hz, dial {first.DialHz} Hz, {first.DialHz - Call:+0;-0;0} Hz from the call, so heard at {heardAt} Hz; the scan's direction {first.ToneFollowsDial}; {first.Kind}, `{first.Text}`");
        _output.WriteLine($"empty stop at {empty.ScopeHz} Hz: {(empty.EndUtc - empty.StartUtc).TotalSeconds:0.0} s");

        Assert.InRange(first.ScopeHz, Call + offsetHz - 20, Call + offsetHz + 20);
        Assert.InRange(heardAt, 585, 615);
        Assert.InRange(first.ToneAfterHz!.Value, 585, 615);
        Assert.Equal(CatchKind.Positive, first.Kind);
        Assert.Equal(CatchKind.Empty, empty.Kind);
        Assert.InRange((empty.EndUtc - empty.StartUtc).TotalSeconds, 0, 8);
    }

    /// <remarks>Test 2: a positive that keeps sending is left when the positive stay, 90 s, runs out.</remarks>
    [Fact]
    public async Task APositiveThatKeepsSendingIsLeftAtNinetySeconds()
    {
        using var world = await ScanWorld.Ready(keepsSending: true);
        var (scan, summary) = await world.Run(_folder, Short with { Length = TimeSpan.FromMinutes(2) });
        var first = Read<CwCatch>(scan, summary.Catches[0].Json);

        _output.WriteLine($"{first.Kind} left {first.Left} after {(first.EndUtc - first.StartUtc).TotalSeconds:0.0} s: `{first.Text}`");

        Assert.Equal(CatchKind.Positive, first.Kind);
        Assert.Equal(CatchLeft.StayRanOut, first.Left);
        Assert.InRange((first.EndUtc - first.StartUtc).TotalSeconds, 90, 91);
    }

    /// <remarks>
    /// Test 2: a positive whose station stops is left once it has been silent <see cref="CwCatchScan.SilentSeconds"/>,
    /// read out, well before the positive stay.
    /// </remarks>
    [Fact]
    public async Task APositiveThatStopsIsLeftAfterItsSilence()
    {
        using var world = await ScanWorld.Ready(keepsSending: false);
        var (scan, summary) = await world.Run(_folder, Short with { Length = TimeSpan.FromMinutes(2) });
        var first = Read<CwCatch>(scan, summary.Catches[0].Json);
        var stayed = (first.EndUtc - first.StartUtc).TotalSeconds;

        // The call began when the dial first came near it; since the scan lands by ear (task 3) it may probe and retune before
        // the catch begins, so the silence is counted from the end of the call itself.
        var silent = (first.EndUtc - world.CallStartUtc!.Value.AddSeconds(world.CallSeconds)).TotalSeconds;

        _output.WriteLine($"{first.Kind} left {first.Left} after {stayed:0.0} s, {silent:0.0} s after the call ended, the call {world.CallSeconds:0.0} s long, heard at {first.ToneHz:0} Hz: `{first.Text}`");

        Assert.Equal(CatchKind.Positive, first.Kind);
        Assert.Equal(CatchLeft.ReadOut, first.Left);
        Assert.InRange(silent, CwCatchScan.SilentSeconds - 2, CwCatchScan.SilentSeconds + 3);
    }

    /// <remarks>Test 4: the scan stops on its own at its length, set short here, and the catch under way says so.</remarks>
    [Fact]
    public async Task TheScanStopsOnItsOwnAtItsLength()
    {
        using var world = await ScanWorld.Ready(keepsSending: true);
        var (scan, summary) = await world.Run(_folder, Short with { Length = TimeSpan.FromSeconds(40) });

        Assert.Equal(CwScanEnd.LengthReached, summary.Ended);
        Assert.Single(summary.Catches);
        Assert.Equal(CatchLeft.ScanEnded, summary.Catches[0].Left);
        Assert.InRange((summary.EndUtc!.Value - summary.StartUtc).TotalSeconds, 40, 41.5);
        Assert.StartsWith("scan done · 1 catch", summary.Sentence, StringComparison.Ordinal);
        Assert.Equal(Home, world.Rig.FrequencyHz);
    }

    /// <remarks>Test 5: Stop ends it at once, the catch under way saved and marked stopped, and the dial goes home.</remarks>
    [Fact]
    public async Task StopEndsItAtOnceAndSavesTheCatch()
    {
        using var world = await ScanWorld.Ready(keepsSending: true);
        CwCatchScan? running = null;

        world.At(20, () => running!.Stop());

        var (scan, summary) = await world.Run(_folder, Short, s => running = s);

        Assert.Equal(CwScanEnd.Stopped, summary.Ended);
        Assert.Single(summary.Catches);
        Assert.Equal(CatchLeft.Stopped, summary.Catches[0].Left);
        Assert.InRange((summary.EndUtc!.Value - summary.StartUtc).TotalSeconds, 20, 21);
        Assert.True(File.Exists(Path.Combine(scan.ScanFolder!, summary.Catches[0].Wav)));
        Assert.Equal(Home, world.Rig.FrequencyHz);
    }

    /// <remarks>Test 6: the dial moved by hand mid-catch - the scan carries on from there.</remarks>
    [Fact]
    public async Task ADialMovedByHandIsCarriedOnFrom()
    {
        using var world = await ScanWorld.Ready(keepsSending: true);

        world.At(15, () => world.Rig.OperatorTunesTo(Carrier - 1_000));

        var (scan, summary) = await world.Run(_folder, Short with { Length = TimeSpan.FromSeconds(80) });

        _output.WriteLine(string.Join(Environment.NewLine, summary.Catches.Select(c => $"{c.SignalHz} {c.Kind} {c.Left}")));

        Assert.Equal(CatchLeft.DialMoved, summary.Catches[0].Left);
        Assert.Equal(Carrier, Nearest(summary.Catches[1].SignalHz));
        Assert.Equal(CwScanEnd.LengthReached, summary.Ended);
    }

    /// <remarks>Test 6: a band change stops the scan, and the dial is left where the operator put it.</remarks>
    [Fact]
    public async Task ABandChangeStopsIt()
    {
        using var world = await ScanWorld.Ready(keepsSending: true);

        world.At(15, () => world.Rig.OperatorTunesTo(14_030_000));

        var (_, summary) = await world.Run(_folder, Short);

        Assert.Equal(CwScanEnd.BandChanged, summary.Ended);
        Assert.Equal(CatchLeft.BandChanged, summary.Catches[0].Left);
        Assert.Equal(14_030_000, world.Rig.FrequencyHz);
        Assert.StartsWith("scan stopped · the band changed", summary.Sentence, StringComparison.Ordinal);
    }

    /// <remarks>Test 7: the link drops - the scan aborts, saves what it has, and says so.</remarks>
    [Fact]
    public async Task ALinkThatDropsAbortsIt()
    {
        using var world = await ScanWorld.Ready(keepsSending: true);

        world.At(12, () => world.Rig.Connected = false);

        var (scan, summary) = await world.Run(_folder, Short);

        Assert.Equal(CwScanEnd.LinkDropped, summary.Ended);
        Assert.Equal(CatchLeft.LinkDropped, summary.Catches[0].Left);
        Assert.StartsWith("scan aborted · the radio's link dropped", summary.Sentence, StringComparison.Ordinal);
        Assert.Equal(CwScanEnd.LinkDropped, Read<CwScanSummary>(scan, "scan.json").Ended);
    }

    // Which of the three places a peak is: the scope's bins are 284 Hz wide here, so a peak is placed within half a bin.
    private static long Nearest(long hz) => new[] { Call, Noise, Carrier }.Single(p => Math.Abs(p - hz) <= 150);

    private T Read<T>(CwCatchScan scan, string file)
        => JsonSerializer.Deserialize<T>(File.ReadAllText(Path.Combine(scan.ScanFolder!, file)), Options)!;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) },
    };

    private void Print(CwCatchScan scan, CwScanSummary summary)
    {
        var size = new DirectoryInfo(scan.ScanFolder!).EnumerateFiles().Sum(f => f.Length);

        _output.WriteLine($"folder {Path.GetFileName(scan.ScanFolder)}: {size / 1_000_000.0:0.0} MB over {(summary.EndUtc!.Value - summary.StartUtc).TotalMinutes:0.0} min at {ScanWorld.Rate} Hz");
        _output.WriteLine("scan.json:");
        _output.WriteLine(File.ReadAllText(Path.Combine(scan.ScanFolder!, "scan.json")));

        foreach (var entry in summary.Catches.Take(2))
        {
            _output.WriteLine(entry.Json + ":");
            _output.WriteLine(File.ReadAllText(Path.Combine(scan.ScanFolder!, entry.Json)));
        }
    }

    /// <summary>The fake radio, scope and audio, on one virtual clock.</summary>
    internal sealed class ScanWorld : IDisposable
    {
        public const int Rate = 8000;

        private const double ChunkSeconds = 0.01;

        private readonly Dictionary<int, float[]> _calls = new();

        // The call keyed at a pitch, once per pitch: eight times over where it keeps sending.
        private float[] CallAt(int pitch)
        {
            if (!_calls.TryGetValue(pitch, out var samples))
            {
                var once = CwSignal.Generate(new CwSignalRequest(
                    "CQ CQ CQ DE W1AW W1AW W1AW K", WordsPerMinute: 18, ToneHz: pitch, SampleRate: Rate, Amplitude: 0.3,
                    NoiseAmplitude: 0, LeadInSeconds: 0.5, TailSeconds: 1.5, Seed: 5402)).Samples;

                samples = _keepsSending ? Enumerable.Repeat(once, 8).SelectMany(s => s).ToArray() : once;
                _calls[pitch] = samples;
            }

            return samples;
        }
        private readonly float[] _band;
        private readonly bool _keepsSending;
        private readonly List<(double At, Action Do)> _events = new();
        private readonly Random _scopeNoise = new(5401);
        private long _sample;
        private long? _callFrom;
        private double _nextFrame;
        private double _nextReport;

        private ScanWorld(bool keepsSending)
        {
            _keepsSending = keepsSending;
            Rig = new ScanFakeRig(Home, () => Now);
            Monitor = new RigStateMonitor(Rig, (_, _) => Task.CompletedTask);
            Scope = new FakeScope();
            Audio = new FakeAudio();

            CallSeconds = CallAt(600).Length / (double)Rate / (keepsSending ? 8 : 1);
            _band = CwSignal.Generate(new CwSignalRequest(
                " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.04, LeadInSeconds: 15, TailSeconds: 15, Seed: 5403)).Samples;
        }

        public ScanFakeRig Rig { get; }

        public RigStateMonitor Monitor { get; }

        public FakeScope Scope { get; }

        public FakeAudio Audio { get; }

        public ListenOnlyLock ListenOnly { get; } = new();

        public DateTime Now { get; private set; } = new(2026, 10, 4, 23, 0, 0, DateTimeKind.Utc);

        /// <summary>When the call began, the first time the dial came near it, or null where it never did.</summary>
        public DateTime? CallStartUtc => _callFrom is { } from ? new DateTime(2026, 10, 4, 23, 0, 0, DateTimeKind.Utc).AddSeconds(from / (double)Rate) : null;

        public double CallSeconds { get; }

        /// <summary>
        /// Which way a station's tone moves when the dial moves: 1, rising with it, as the owner's radio does in CW (work
        /// instruction 544); -1 the other way.
        /// </summary>
        public int ToneSign { get; set; } = 1;

        private double Seconds { get; set; }

        public static async Task<ScanWorld> Ready(bool keepsSending)
        {
            var world = new ScanWorld(keepsSending);

            world.Monitor.Start();
            await world.Monitor.Populated.WaitAsync(TimeSpan.FromSeconds(5));
            world.Monitor.Stop();

            return world;
        }

        public void At(double seconds, Action action) => _events.Add((seconds, action));

        public async Task<(CwCatchScan Scan, CwScanSummary Summary)> Run(string folder, CwScanSettings settings, Action<CwCatchScan>? started = null)
        {
            using var ear = new CwCatchEar(Audio, 600, 500);
            var scan = new CwCatchScan(Rig, Monitor, Scope, ear, ListenOnly, new MemoryHome(), folder, settings, Delay, () => Now);

            started?.Invoke(scan);

            return (scan, await scan.RunAsync());
        }

        private Task Delay(TimeSpan span, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var steps = (int)Math.Round(span.TotalSeconds / ChunkSeconds);

            for (var i = 0; i < steps; i++)
            {
                Step();

                foreach (var due in _events.Where(e => e.At <= Seconds).ToList())
                {
                    _events.Remove(due);
                    due.Do();
                }

                token.ThrowIfCancellationRequested();
            }

            return Task.CompletedTask;
        }

        private void Step()
        {
            var n = (int)(Rate * ChunkSeconds);
            var chunk = new float[n];
            var dial = Rig.FrequencyHz;

            for (var i = 0; i < n; i++)
            {
                var g = _sample + i;
                var s = _band[g % _band.Length];

                // Each station is heard at the pitch its offset from the dial puts it at, inside the radio's filter, as on the air.
                var call = Call - dial;
                var carrier = Carrier - dial;

                if (Math.Abs(call) < 250)
                {
                    _callFrom ??= g;

                    var k = g - _callFrom.Value;
                    var keyed = CallAt(600 + (ToneSign * (int)-call));

                    s += k < keyed.Length ? keyed[k] : 0;
                }
                else if (Math.Abs(carrier) < 250)
                {
                    s += (float)(0.3 * Math.Sin(2 * Math.PI * (600 + (ToneSign * -carrier)) * g / Rate));
                }

                chunk[i] = s;
            }

            Audio.Push(new AudioChunk(_sample, Rate, chunk));
            _sample += n;
            Seconds += ChunkSeconds;
            Now = Now.AddSeconds(ChunkSeconds);

            if (Seconds >= _nextFrame)
            {
                _nextFrame += 0.1;
                Scope.Emit(_scopeNoise);
            }

            if (Seconds >= _nextReport && Rig.Connected)
            {
                _nextReport += 0.25;
                Rig.Report();
            }
        }

        public void Dispose() => Monitor.Dispose();
    }

    /// <summary>A scope over 40 m's CW segment with a peak at each of the three places.</summary>
    internal sealed class FakeScope : ISpectrumSource
    {
        public const long Low = 6_995_000;
        public const long High = 7_130_000;

        /// <summary>How far from the call the scope draws its peak: a bin's error at a wide span (work instruction 542).</summary>
        public long CallOffsetHz { get; set; }

        public bool IsSimulated => true;

        public bool IsRunning => true;

        public event SpectrumFrameHandler? FrameReady;

        public void Start()
        {
        }

        public void Stop()
        {
        }

        /// <summary>How many bins a sweep has: the radio's 475, or more for a scope that places a peak to tens of hertz.</summary>
        public int BinCount { get; set; } = 475;

        /// <summary>How far either side of a peak the scope draws it.</summary>
        public long PeakHalfWidthHz { get; set; } = 150;

        public void Emit(Random noise)
        {
            var bins = new byte[BinCount];

            for (var i = 0; i < bins.Length; i++)
            {
                var at = Low + (long)((i + 0.5) / bins.Length * (High - Low));

                bins[i] = (byte)(20 + noise.Next(-3, 4));

                foreach (var peak in new[] { Call + CallOffsetHz, Noise, Carrier })
                {
                    if (Math.Abs(at - peak) <= PeakHalfWidthHz)
                    {
                        bins[i] = 90;
                    }
                }
            }

            var frame = new SpectrumFrame(Low, High, DateTime.UtcNow, bins);

            FrameReady?.Invoke(in frame);
        }
    }

    /// <summary>Audio pushed by the world.</summary>
    internal sealed class FakeAudio : IAudioSource
    {
        public string DeviceName => "scan test audio";

        public int SampleRate => ScanWorld.Rate;

        public bool IsSimulated => true;

        public bool IsRunning => true;

        public event AudioChunkHandler? SamplesReady;

        public void Push(AudioChunk chunk) => SamplesReady?.Invoke(in chunk);

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }
    }

    internal sealed class MemoryHome : IScanHome
    {
        public long? Pending { get; private set; }

        public void Remember(long frequencyHz) => Pending = frequencyHz;

        public void Clear() => Pending = null;
    }

    /// <summary>A port that keeps every frame it is given.</summary>
    internal sealed class RecordingPort : ISerialPort
    {
        public List<byte[]> Written { get; } = new();

        public bool IsOpen => true;

        public string PortName => "SCAN1";

        public int BaudRate => 115_200;

        public void Open()
        {
        }

        public void Close()
        {
        }

        public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken) => ValueTask.FromResult(0);

        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            Written.Add(buffer.ToArray());

            return ValueTask.CompletedTask;
        }

        public void Write(ReadOnlySpan<byte> buffer) => Written.Add(buffer.ToArray());

        public void Dispose()
        {
        }
    }

    /// <summary>A sound card that counts what it was asked to play.</summary>
    internal sealed class CountingSink : ITransmitAudioSink
    {
        public int Calls { get; private set; }

        public int EndpointSampleRate => 48_000;

        public Task<PlayedAudio> PlayAsync(ReadOnlyMemory<float> samples, int sampleRate, CancellationToken cancellationToken)
        {
            Calls++;

            return Task.FromResult(new PlayedAudio(samples.Length, TimeSpan.Zero));
        }
    }

    /// <summary>
    /// A radio that answers, echoes every tune as a radio does, reports itself on the virtual clock, and counts anything
    /// that tries to key it.
    /// </summary>
    internal sealed class ScanFakeRig(long frequencyHz, Func<DateTime> now) : IRig
    {
        public long FrequencyHz { get; private set; } = frequencyHz;

        public int KeyingAttempts { get; private set; }

        public bool Connected { get; set; } = true;

        public bool IsConnected => Connected;

        public bool IsSimulated => true;

        public RigCapabilities Capabilities { get; } = new("Scan test radio", true, true, true, true, Array.Empty<string>());

        public event EventHandler<FrequencyChangedEventArgs>? FrequencyChanged;

        public event EventHandler<RigValuesReportedEventArgs>? ValuesReported;

        public void OperatorTunesTo(long hz)
        {
            FrequencyHz = hz;
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(hz));
        }

        public void Report()
            => ValuesReported?.Invoke(this, new RigValuesReportedEventArgs(new[]
            {
                RigValue.Known(RigField.Frequency, FrequencyHz, $"{FrequencyHz / 1_000_000.0:0.000} MHz", now(), "scan test radio"),
                RigValue.Known(RigField.TransmitStatus, 0, "receiving", now(), "scan test radio"),
            }));

        public Task<bool> ConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task DisconnectAsync() => Task.CompletedTask;

        public Task<long> GetFrequencyHzAsync(CancellationToken cancellationToken = default)
            => Connected ? Task.FromResult(FrequencyHz) : Task.FromException<long>(new IOException("the radio stopped answering"));

        public Task SetFrequencyHzAsync(long frequencyHz, CancellationToken cancellationToken = default)
        {
            if (!Connected)
            {
                return Task.FromException(new IOException("the radio stopped answering"));
            }

            FrequencyHz = frequencyHz;
            FrequencyChanged?.Invoke(this, new FrequencyChangedEventArgs(frequencyHz));

            return Task.CompletedTask;
        }

        /// <summary>**THE TRIPWIRE.** Nothing in a scan may reach this (§0.2).</summary>
        public Task<bool> SendCwAsync(string message, CancellationToken cancellationToken = default)
        {
            KeyingAttempts++;
            return Task.FromResult(true);
        }

        public void AbortCw() => KeyingAttempts++;

        public Task<RigWriteResult> SetSettingAsync(CivWrite write, int value, CancellationToken cancellationToken = default)
        {
            KeyingAttempts++;
            return Task.FromResult(RigWriteResult.NotSupported("scan test radio"));
        }

        public Task<RigWriteResult> SetModeAsync(CivMode mode, bool dataMode, byte? filterSlot, CancellationToken cancellationToken = default)
        {
            KeyingAttempts++;
            return Task.FromResult(RigWriteResult.NotSupported("scan test radio"));
        }

        public Task<IReadOnlyList<RigValue>> ReadAsync(RigField field, RigState context, CancellationToken cancellationToken = default)
        {
            RigValue value = field switch
            {
                RigField.Frequency => RigValue.Known(field, FrequencyHz, $"{FrequencyHz / 1_000_000.0:0.000} MHz", now(), "scan test radio"),
                RigField.TransmitStatus => RigValue.Known(field, 0, "receiving", now(), "scan test radio"),
                RigField.Mode => RigValue.Known(field, 3, "CW", now(), "scan test radio"),
                RigField.BreakIn => RigValue.Known(field, 1, "semi break-in", now(), "scan test radio"),
                _ => RigValue.Known(field, 0, "0", now(), "scan test radio"),
            };

            return Task.FromResult<IReadOnlyList<RigValue>>(new[] { value });
        }
    }
}

using Hamlet.RadioEngine.Audio;
using Hamlet.RadioEngine.Rig;
using Hamlet.RadioEngine.Scan;
using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Tests.Scan;

/// <summary>
/// A fake radio whose scope is in its centre mode and follows the dial, as the owner's does: a span of ±10 kHz, 475 bins
/// of about 42 Hz, the floor clipped to nought, two noise blips a sweep at 6 or 7, and stations drawn as the scope draws a
/// keyed CW signal, a core and skirts on either side, up in some sweeps and not others (work instruction 543).
/// </summary>
/// <remarks>
/// <para>**THE SWEEP RATE IS THE RADIO'S OWN: ABOUT FOUR AND A HALF SWEEPS A SECOND**, as measured off the owner's radio and
/// recorded in <see cref="ScopeFlow.QuietAfter"/>. The earlier fake scope sent ten a second, which is more than twice what
/// the scan gets on the air.</para>
/// <para>Time is virtual: the scan's delay advances the clock and feeds the audio, the scope and the rig's reports.</para>
/// </remarks>
internal sealed class SpanWorld : IDisposable
{
    public const int Rate = 8000;
    public const long HalfSpanHz = 10_000;
    public const int BinCount = 475;
    public const double SweepSeconds = 1 / 4.5;

    private const double ChunkSeconds = 0.01;

    private readonly Random _random;
    private readonly float[] _band;
    private readonly List<(double At, Action Do)> _events = new();
    private long _sample;
    private double _nextFrame;
    private double _nextReport;

    private SpanWorld(long home, IReadOnlyList<ScopeStation> stations, int seed)
    {
        Stations = stations;
        _random = new Random(seed);
        Rig = new TheCatchScanTests.ScanFakeRig(home, () => Now);
        Monitor = new RigStateMonitor(Rig, (_, _) => Task.CompletedTask);
        Scope = new FollowingScope();
        _band = CwSignal.Generate(new CwSignalRequest(
            " ", SampleRate: Rate, Amplitude: 0, NoiseAmplitude: 0.04, LeadInSeconds: 15, TailSeconds: 15, Seed: seed)).Samples;
        Rig.FrequencyChanged += (_, e) => Tunes.Add((Seconds, e.FrequencyHz));
    }

    public TheCatchScanTests.ScanFakeRig Rig { get; }

    public RigStateMonitor Monitor { get; }

    public FollowingScope Scope { get; }

    public TheCatchScanTests.FakeAudio Audio { get; } = new();

    public ListenOnlyLock ListenOnly { get; } = new();

    public IReadOnlyList<ScopeStation> Stations { get; }

    public DateTime Now { get; private set; } = new(2026, 10, 5, 14, 0, 0, DateTimeKind.Utc);

    public double Seconds { get; private set; }

    /// <summary>Every sweep the scope sent: when, its edges, and its bins.</summary>
    public List<(double At, long Low, long High, byte[] Bins)> Sweeps { get; } = new();

    /// <summary>Every tune: when, and where to.</summary>
    public List<(double At, long Hz)> Tunes { get; } = new();

    public static async Task<SpanWorld> Ready(long home, IReadOnlyList<ScopeStation> stations, int seed = 5431)
    {
        var world = new SpanWorld(home, stations, seed);

        world.Monitor.Start();
        await world.Monitor.Populated.WaitAsync(TimeSpan.FromSeconds(5));
        world.Monitor.Stop();

        return world;
    }

    private CwCatchEar? _ear;

    public async Task<(CwCatchScan Scan, CwScanSummary Summary)> Run(string folder, CwScanSettings settings, Action<CwCatchScan>? started = null)
    {
        using var ear = new CwCatchEar(Audio, 600, 500);
        var scan = new CwCatchScan(Rig, Monitor, Scope, ear, ListenOnly, new TheCatchScanTests.MemoryHome(), folder, settings, Delay, () => Now);

        // Real time on the air: the ear reads behind a queue (work instruction 551), and is let catch up after each chunk.
        _ear = ear;

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

            chunk[i] = _band[g % _band.Length];

            // A station that sends is heard at the pitch its offset from the dial puts it at, rising with the dial as the
            // owner's radio does in CW, inside the filter; it sends its over, pauses, and sends it again.
            foreach (var station in Stations)
            {
                if (station.Sends is null || Math.Abs(dial - station.Hz) >= 250)
                {
                    continue;
                }

                var over = Over(station, 600 + (int)(dial - station.Hz));
                var k = g % (over.Length + (long)(station.PauseSeconds * Rate));

                chunk[i] += k < over.Length ? over[k] : 0;
            }
        }

        Audio.Push(new AudioChunk(_sample, Rate, chunk));
        _ear?.CatchUpForTests();
        _sample += n;
        Seconds += ChunkSeconds;
        Now = Now.AddSeconds(ChunkSeconds);

        if (Seconds >= _nextFrame)
        {
            _nextFrame += SweepSeconds;
            Sweep();
        }

        if (Seconds >= _nextReport && Rig.Connected)
        {
            _nextReport += 0.25;
            Rig.Report();
        }
    }

    private readonly Dictionary<(string, int), float[]> _overs = new();

    private float[] Over(ScopeStation station, int pitch)
    {
        if (!_overs.TryGetValue((station.Name, pitch), out var samples))
        {
            samples = CwSignal.Generate(new CwSignalRequest(
                station.Sends!, WordsPerMinute: 20, ToneHz: pitch, SampleRate: Rate, Amplitude: station.Amplitude,
                NoiseAmplitude: 0, LeadInSeconds: 0, TailSeconds: 0, Seed: 5443)).Samples;
            _overs[(station.Name, pitch)] = samples;
        }

        return samples;
    }

    /// <summary>Whether a station is sending now, not pausing between its overs.</summary>
    private bool Sending(ScopeStation station)
        => station.Sends is null
           || _sample % (Over(station, 600).Length + (long)(station.PauseSeconds * Rate)) < Over(station, 600).Length;

    /// <summary>Half the scope's span: ±10 kHz unless a test sets it, ±1.25 kHz for bins of about 5 Hz.</summary>
    public long HalfSpan { get; set; } = HalfSpanHz;

    /// <summary>How many noise blips, at 6 or 7, a sweep has.</summary>
    public int BlipsPerSweep { get; set; } = 2;

    private void Sweep()
    {
        var low = Rig.FrequencyHz - HalfSpan;
        var high = Rig.FrequencyHz + HalfSpan;
        var bins = new byte[BinCount];
        var binHz = (high - low) / (double)BinCount;

        // Noise blips, at 6 or 7, anywhere.
        for (var b = 0; b < BlipsPerSweep; b++)
        {
            bins[_random.Next(BinCount)] = (byte)(6 + _random.Next(2));
        }

        foreach (var station in Stations)
        {
            if (_random.NextDouble() >= station.UpShare || !Sending(station))
            {
                continue;
            }

            var centre = (station.Hz - low) / binHz;

            for (var d = -(station.Profile.Length - 1); d < station.Profile.Length; d++)
            {
                var i = (int)Math.Floor(centre) + d;

                if (i >= 0 && i < BinCount)
                {
                    bins[i] = Math.Max(bins[i], station.Profile[Math.Abs(d)]);
                }
            }
        }

        Sweeps.Add((Seconds, low, high, bins));
        Scope.Emit(low, high, bins);
    }

    public void Dispose() => Monitor.Dispose();

    /// <summary>A scope that sends what the world sweeps.</summary>
    internal sealed class FollowingScope : ISpectrumSource
    {
        public bool IsSimulated => true;

        public bool IsRunning => true;

        public event SpectrumFrameHandler? FrameReady;

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Emit(long low, long high, byte[] bins)
        {
            var frame = new SpectrumFrame(low, high, DateTime.UtcNow, bins);

            FrameReady?.Invoke(in frame);
        }
    }
}

/// <summary>A station as the scope draws it.</summary>
/// <param name="Name">What the trace calls it.</param>
/// <param name="Hz">Where it is.</param>
/// <param name="Profile">Its height on the scope's byte scale at its centre bin, then one bin out, two bins out, and so on.</param>
/// <param name="UpShare">The share of sweeps it is keyed down in.</param>
/// <param name="Sends">What it sends, where the ear should hear it; null for the scope alone.</param>
/// <param name="PauseSeconds">How long it pauses between its overs.</param>
/// <param name="Amplitude">How loud it sends, against band noise of 0.04.</param>
internal sealed record ScopeStation(string Name, long Hz, byte[] Profile, double UpShare, string? Sends = null, double PauseSeconds = 0, double Amplitude = 0.3);

using System.Diagnostics;
using Hamlet.RadioEngine.Bands;

namespace Hamlet.RadioEngine.Training;

/// <summary>
/// The training radio's spectrum: synthesised sweeps, delivered at a real
/// frame rate, over the same seam the IC-7300's scope will use.
/// </summary>
/// <remarks>
/// <para><see cref="IsSimulated"/> is hardcoded true and has no setter. That
/// is the whole mechanism behind HM-DEC-026: the waterfall's "simulated"
/// label is read off the source, so synthetic frames cannot reach the screen
/// unlabelled — there is no flag to forget to set and none to switch off.</para>
/// <para>The clock lives here and only here. The synthesiser is handed an
/// elapsed time, so everything below this class stays reproducible (§5), and
/// <see cref="PumpOnce"/> lets a test drive the whole path at chosen instants
/// without waiting for real seconds to pass.</para>
/// </remarks>
public sealed class TrainingSpectrumSource : ISpectrumSource, IDisposable
{
    /// <summary>Frames per second. Fast enough to read as motion, slow
    /// enough to leave the UI thread alone.</summary>
    public const int FramesPerSecond = 25;

    /// <summary>Bins per sweep — the waterfall's horizontal resolution.</summary>
    public const int DefaultBinCount = 512;

    private readonly byte[] _bins;
    private readonly SignalSynthesizer _synth;
    private readonly Func<TimeSpan>? _clock;
    private readonly Stopwatch _stopwatch = new();
    private readonly object _gate = new();

    private Timer? _timer;
    private bool _disposed;

    /// <summary>Creates a source for a band.</summary>
    /// <param name="band">Band to synthesise; the sweep spans it exactly.</param>
    /// <param name="seed">Seed; the same seed always yields the same band.</param>
    /// <param name="callsign">Operator callsign, for flavour only.</param>
    /// <param name="binCount">Bins per sweep.</param>
    /// <param name="clock">Elapsed-time source, injected by tests. Null uses
    /// a stopwatch started on <see cref="Start"/>.</param>
    public TrainingSpectrumSource(
        CwBand band,
        int seed = 20260813,
        string? callsign = null,
        int binCount = DefaultBinCount,
        Func<TimeSpan>? clock = null)
    {
        Band = band;
        _bins = new byte[Math.Max(16, binCount)];
        _synth = new SignalSynthesizer(
            TrainingBandPlan.ForBand(band, seed, callsign), band.LowHz, band.HighHz, seed);
        _clock = clock;
    }

    /// <summary>The band being synthesised.</summary>
    public CwBand Band { get; }

    /// <summary>The signals on the air, for tests and for the panel summary.</summary>
    public IReadOnlyList<SyntheticSignal> Signals => _synth.Signals;

    /// <inheritdoc/>
    /// <remarks>Always true. See the type remarks — this is the guarantee.</remarks>
    public bool IsSimulated => true;

    /// <inheritdoc/>
    public bool IsRunning => _timer is not null;

    /// <inheritdoc/>
    public event SpectrumFrameHandler? FrameReady;

    /// <inheritdoc/>
    public void Start()
    {
        lock (_gate)
        {
            if (_disposed || _timer is not null)
            {
                return;
            }

            if (_clock is null)
            {
                _stopwatch.Restart();
            }

            var period = TimeSpan.FromMilliseconds(1000.0 / FramesPerSecond);
            _timer = new Timer(_ => Tick(), null, TimeSpan.Zero, period);
        }
    }

    /// <inheritdoc/>
    public void Stop()
    {
        lock (_gate)
        {
            _timer?.Dispose();
            _timer = null;
            _stopwatch.Stop();
        }
    }

    /// <summary>
    /// Render and publish one frame at an explicit instant.
    /// </summary>
    /// <param name="elapsed">Time since the session began.</param>
    /// <remarks>
    /// The seam that makes the whole path testable: a test can ask for the
    /// frame at exactly 7.5 seconds and assert on its bytes, rather than
    /// sleeping and hoping.
    /// </remarks>
    public void PumpOnce(TimeSpan elapsed)
    {
        var handler = FrameReady;
        if (handler is null)
        {
            return;
        }

        lock (_gate)
        {
            _synth.Render(elapsed, _bins);
            var frame = new SpectrumFrame(
                Band.LowHz, Band.HighHz, DateTime.UtcNow, _bins);
            handler(in frame);
        }
    }

    /// <summary>Render one frame into a caller-owned buffer, publishing nothing.</summary>
    /// <param name="elapsed">Time since the session began.</param>
    /// <param name="bins">Destination buffer.</param>
    public void RenderInto(TimeSpan elapsed, Span<byte> bins)
        => _synth.Render(elapsed, bins);

    /// <inheritdoc/>
    public void Dispose()
    {
        lock (_gate)
        {
            _disposed = true;
        }

        Stop();
    }

    private void Tick()
    {
        try
        {
            PumpOnce(_clock?.Invoke() ?? _stopwatch.Elapsed);
        }
        catch (Exception)
        {
            // A render fault must not take down a timer thread with nobody to
            // catch it (§8). A dropped frame is a dropped frame.
        }
    }
}

using Hamlet.RadioEngine.Civ;
using Hamlet.RadioEngine.Training;

namespace Hamlet.RadioEngine.Rig;

/// <summary>
/// The radio's own spectrum scope, over the seam the synthesiser already uses
/// (HM-DEC-062, HM-DEC-005).
/// </summary>
/// <remarks>
/// <para>THE RADIO COMPUTES THIS AND HAMLET DOES NOT. The 7300's panadapter is
/// free, band-wide and already running, which is why HM-DEC-005 ruled that the
/// app never computes a wideband FFT the radio has already computed. What
/// arrives here is the finished sweep.</para>
/// <para><see cref="IsSimulated"/> is hardcoded false and has no setter, which
/// is the same mechanism the training source uses in the other direction
/// (HM-DEC-026). The waterfall's "simulated" label is read off the source on
/// every frame, so real data arriving cannot weaken it and synthetic data cannot
/// arrive unlabeled. There is no flag to forget.</para>
/// <para>NOTHING HERE WRITES TO THE RADIO. Turning the scope output on is a
/// write, and this class does not make one: it reads whether the two settings
/// are on and says what is missing (<see cref="ScopeReadiness"/>). That is not
/// only discipline, it is also the honest shape, because the stream needs two
/// radio menu settings Hamlet has no command for at all.</para>
/// <para>READING COSTS NO POLLING. The radio pushes these frames once its own
/// output is on, so the stream adds no commands to the bus and cannot starve the
/// poll loop by asking for anything (HM-DEC-050). It is a listener.</para>
/// </remarks>
public sealed class RigSpectrumSource : ISpectrumSource, IDisposable
{
    private readonly Ic7300Rig _rig;
    private readonly object _gate = new();
    private readonly List<byte> _assembling = new(CivScope.WaveformLength);

    private byte[] _bins = Array.Empty<byte>();
    private ScopeHeader? _header;
    private int _expected;
    private bool _disposed;

    /// <summary>Listen to a radio's scope stream.</summary>
    /// <param name="rig">The radio.</param>
    public RigSpectrumSource(Ic7300Rig rig)
        => _rig = rig ?? throw new ArgumentNullException(nameof(rig));

    /// <inheritdoc/>
    /// <remarks>
    /// False, with no setter anywhere. See the class remarks: this is the whole
    /// of HM-DEC-026 on this side of the seam.
    /// </remarks>
    public bool IsSimulated => false;

    /// <inheritdoc/>
    public bool IsRunning { get; private set; }

    /// <summary>How many complete sweeps have arrived.</summary>
    /// <remarks>
    /// §0.0.1 wants the app's own behavior visible. "Is the scope actually
    /// streaming" is a question somebody will ask, and a count answers it rather
    /// than being argued about.
    /// </remarks>
    public long SweepCount { get; private set; }

    /// <summary>How many sweeps were dropped because a part went missing.</summary>
    /// <remarks>
    /// Counted rather than hidden. A stream that is losing a third of its sweeps
    /// looks like a slow waterfall, which is the hardest kind of defect to
    /// attribute.
    /// </remarks>
    public long DroppedCount { get; private set; }

    /// <summary>
    /// Parts that arrived off the wire, whatever became of them (HM-DEC-093).
    /// </summary>
    /// <remarks>
    /// **THE FIRST ZERO IN THE CHAIN IS THE BUG'S ADDRESS.** Three sessions
    /// reported this feature working and not one of them could have said whether
    /// a single byte had ever arrived, because nothing counted. Received, parsed,
    /// rejected and delivered are four different numbers and the gaps between
    /// them are the diagnosis.
    /// </remarks>
    public long PartsReceived { get; private set; }

    /// <summary>Parts whose header this build could read.</summary>
    public long PartsParsed { get; private set; }

    /// <summary>Parts this build could not read at all.</summary>
    public long PartsRejected { get; private set; }

    /// <summary>
    /// Why the first rejected part was rejected, or "" (HM-DEC-093).
    /// </summary>
    /// <remarks>
    /// The first one rather than the last, because the first is the one that
    /// happened before anything else went wrong. A parser that quietly returns
    /// on a part it cannot read is a parser that can be wrong for months.
    /// </remarks>
    public string FirstRejection { get; private set; } = "";

    /// <summary>Complete sweeps handed to whoever is listening.</summary>
    public long SweepsDelivered { get; private set; }

    /// <summary>When the last part arrived, or null if none ever has.</summary>
    public DateTime? LastPartUtc { get; private set; }

    /// <inheritdoc/>
    public event SpectrumFrameHandler? FrameReady;

    /// <inheritdoc/>
    public void Start()
    {
        lock (_gate)
        {
            if (_disposed || IsRunning)
            {
                return;
            }

            _rig.ScopeData += OnScopeData;
            IsRunning = true;
        }
    }

    /// <inheritdoc/>
    public void Stop()
    {
        lock (_gate)
        {
            if (!IsRunning)
            {
                return;
            }

            _rig.ScopeData -= OnScopeData;
            IsRunning = false;
            Reset();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Stop();
        _disposed = true;
    }

    /// <summary>
    /// Take one frame off the wire and, when a sweep completes, publish it.
    /// </summary>
    /// <remarks>
    /// A PART THAT ARRIVES OUT OF ORDER DROPS THE SWEEP RATHER THAN PATCHING IT.
    /// A waterfall row assembled from two different sweeps would draw signals
    /// that were never simultaneously there, which is exactly the kind of
    /// plausible-looking invention §0.0 forbids on this surface.
    /// </remarks>
    private void OnScopeData(byte[] payload)
    {
        SpectrumFrame frame;
        ScopeHeader header;

        lock (_gate)
        {
            var span = payload.AsSpan();

            PartsReceived++;
            LastPartUtc = DateTime.UtcNow;

            if (CivScope.ReadPart(span) is not { } part)
            {
                // **NOT A SILENT RETURN ANY MORE** (HM-DEC-093). This line is
                // where every sweep would vanish if the real 11-part shape
                // differs from the constructed frames the parser was tested
                // against, and nothing anywhere would have said so.
                PartsRejected++;

                if (FirstRejection.Length == 0)
                {
                    FirstRejection =
                        $"part header unreadable, {payload.Length} bytes, "
                        + $"first bytes {Describe(payload)}";
                }

                return;
            }

            PartsParsed++;

            if (part.Sequence == 1)
            {
                _header = CivScope.ReadHeader(span);
                _expected = part.Total;
                _assembling.Clear();

                // A header that will not parse, or a radio saying the data is
                // out of range, means there is nothing honest to draw.
                if (_header is null || _header.OutOfRange)
                {
                    _header = null;
                }

                return;
            }

            if (_header is null || part.Total != _expected)
            {
                return;
            }

            // Parts arrive in order. One that does not is a sweep with a hole in
            // it, and half a row is worse than no row.
            if (part.Sequence != (_assembling.Count == 0 ? 2 : LastSequence + 1))
            {
                DroppedCount++;
                _header = null;
                _assembling.Clear();
                return;
            }

            LastSequence = part.Sequence;

            foreach (var amplitude in CivScope.Waveform(span))
            {
                _assembling.Add(CivScope.Scale(amplitude));
            }

            if (part.Sequence != _expected)
            {
                return;
            }

            header = _header;
            _header = null;

            if (_bins.Length != _assembling.Count)
            {
                _bins = new byte[_assembling.Count];
            }

            _assembling.CopyTo(_bins);
            _assembling.Clear();
            SweepCount++;

            frame = new SpectrumFrame(
                header.LowHz, header.HighHz, DateTime.UtcNow, _bins);
        }

        SweepsDelivered++;
        FrameReady?.Invoke(in frame);
    }

    /// <summary>The first few bytes of a part, for a rejection reason.</summary>
    /// <remarks>
    /// Enough to recognize the shape and never the whole payload: a rejection
    /// reason is a clue, and a screen of hexadecimal is not one.
    /// </remarks>
    private static string Describe(byte[] payload)
        => Convert.ToHexString(payload.AsSpan(0, Math.Min(6, payload.Length)));

    private int LastSequence { get; set; }

    private void Reset()
    {
        _header = null;
        _assembling.Clear();
        LastSequence = 0;
    }
}

namespace Hamlet.RadioEngine.Training;

/// <summary>
/// One sweep of the spectrum: amplitudes across a frequency span at an
/// instant. This is one row of the waterfall.
/// </summary>
/// <remarks>
/// <para>The shape is chosen to match what CI-V <c>0x27</c> will deliver in
/// phase 2 (HM-DEC-005), so the renderer built against synthesised frames
/// today keeps working when the radio starts supplying them: a span, a
/// timestamp, and a run of one-byte amplitudes. The IC-7300 reports scope
/// amplitude as bytes, so bytes are what this carries — converting to float
/// and back would be work done twice for no gain, and a byte maps straight
/// onto a palette index.</para>
/// <para>It is a <c>ref struct</c> on purpose. At twenty-five frames a second
/// a class here would be twenty-five allocations a second forever, and
/// HM-DEC-006 exists because allocation churn on this path is what makes a
/// waterfall stutter. Being a ref struct also means a handler physically
/// cannot stash it on a field or capture it in an <c>async</c> lambda: the
/// buffer belongs to the source and is reused on the next frame, and the
/// compiler now enforces what would otherwise be a comment nobody reads.</para>
/// </remarks>
public readonly ref struct SpectrumFrame
{
    /// <summary>Creates a frame over a caller-owned buffer.</summary>
    /// <param name="lowHz">Frequency of the first bin's lower edge.</param>
    /// <param name="highHz">Frequency of the last bin's upper edge.</param>
    /// <param name="timestampUtc">When the sweep was taken.</param>
    /// <param name="bins">Amplitudes, 0 (noise floor) to 255 (full scale).</param>
    public SpectrumFrame(
        long lowHz, long highHz, DateTime timestampUtc, ReadOnlySpan<byte> bins)
    {
        LowHz = lowHz;
        HighHz = highHz;
        TimestampUtc = timestampUtc;
        Bins = bins;
    }

    /// <summary>Frequency of the first bin's lower edge, in hertz.</summary>
    public long LowHz { get; }

    /// <summary>Frequency of the last bin's upper edge, in hertz.</summary>
    public long HighHz { get; }

    /// <summary>When the sweep was taken.</summary>
    public DateTime TimestampUtc { get; }

    /// <summary>
    /// Amplitudes across the span, 0 to 255. Valid only for the duration of
    /// the event that delivered this frame.
    /// </summary>
    public ReadOnlySpan<byte> Bins { get; }

    /// <summary>Width of the span in hertz.</summary>
    public long SpanHz => HighHz - LowHz;

    /// <summary>The centre frequency of one bin.</summary>
    /// <param name="index">Bin index.</param>
    /// <returns>Centre frequency in hertz.</returns>
    public long BinCenterHz(int index)
        => Bins.Length == 0
            ? LowHz
            : LowHz + (long)((index + 0.5) / Bins.Length * SpanHz);
}

/// <summary>Receives spectrum frames.</summary>
/// <param name="frame">The frame; valid only for the duration of the call.</param>
/// <remarks>
/// A custom delegate rather than <c>EventHandler&lt;T&gt;</c> because
/// <see cref="SpectrumFrame"/> is a ref struct and cannot be a generic type
/// argument — which is the same restriction that keeps the buffer from
/// escaping.
/// </remarks>
public delegate void SpectrumFrameHandler(in SpectrumFrame frame);

/// <summary>
/// Something producing spectrum sweeps: the training synthesiser now, the
/// radio's own scope in phase 2.
/// </summary>
public interface ISpectrumSource
{
    /// <summary>
    /// True when these frames are synthesised rather than received off the
    /// air.
    /// </summary>
    /// <remarks>
    /// <para>THE HONESTY RULE, in the one place it can be enforced
    /// (HM-DEC-026). This is a get-only property on the source itself, not a
    /// flag the UI keeps beside it and not a user setting. A source either is
    /// or is not receiving real radio, and it is the only thing that
    /// knows.</para>
    /// <para>The waterfall's "these signals are simulated" label is derived
    /// from this property on every read. There is no code path that produces
    /// synthetic frames with the label off, because there is nothing to
    /// switch off — which is what makes HM-DEC-009 structural here rather
    /// than a convention somebody has to remember.</para>
    /// </remarks>
    bool IsSimulated { get; }

    /// <summary>True while frames are being produced.</summary>
    bool IsRunning { get; }

    /// <summary>Raised for each sweep, on a background thread.</summary>
    event SpectrumFrameHandler? FrameReady;

    /// <summary>Begin producing frames.</summary>
    void Start();

    /// <summary>Stop producing frames.</summary>
    void Stop();
}

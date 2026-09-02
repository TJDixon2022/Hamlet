using System;

namespace Ft8Sharp.Dsp;

/// <summary>
/// The spectrogram of one slot: a magnitude in decibels for every block, time sub-offset, frequency
/// sub-offset and bin, stored the way upstream stores it.
/// </summary>
/// <remarks>
/// <para>
/// <b>One unsigned byte per magnitude, and that is faithful rather than convenient.</b> Task 2 read
/// <c>WF_ELEM_T</c> as <c>uint8_t</c> and <c>WF_ELEM_MAG(x)</c> as <c>x * 0.5f - 120.0f</c>: half a
/// decibel per count, covering -120 dB to +7.5 dB, clamped at both ends. A float array would be
/// finer and would be a different instrument from the one the published sensitivity figures were
/// measured against, which step 6 will hold this library to.
/// </para>
/// <para>
/// <b>The axis order is upstream's:</b> <c>[block][timeSubOffset][frequencySubOffset][bin]</c>, with
/// the bin varying fastest. The next unit's correlator walks this array directly, and a transposed
/// port would be silent rather than loud.
/// </para>
/// <para>
/// <b>Nothing is normalised.</b> Not per block, not per slot. A stored byte means an absolute
/// number of decibels and means the same thing in every block of every slot. Upstream tracks a
/// running maximum but only writes it to a debug field; it never divides anything.
/// </para>
/// </remarks>
public sealed class Ft8Waterfall
{
    private readonly byte[] _magnitudes;

    internal Ft8Waterfall(Ft8WaterfallGeometry geometry)
    {
        Geometry = geometry;
        _magnitudes = new byte[geometry.MagnitudeCount];
    }

    /// <summary>The extents this waterfall was built to.</summary>
    public Ft8WaterfallGeometry Geometry { get; }

    /// <summary>Blocks actually filled. At most <see cref="Ft8WaterfallGeometry.MaxBlocks"/>.</summary>
    public int BlockCount { get; internal set; }

    /// <summary>
    /// The largest magnitude seen while filling, in decibels, before the byte quantisation. Upstream
    /// keeps the same running maximum, and like upstream's it is a report and never a divisor.
    /// </summary>
    public double LargestDecibels { get; internal set; } = -120.0;

    /// <summary>The raw store, in upstream's layout. One byte per magnitude.</summary>
    public ReadOnlySpan<byte> Magnitudes => _magnitudes;

    /// <summary>The stored byte at one point of the waterfall.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Any index is outside its extent.</exception>
    public byte this[int block, int timeSubOffset, int frequencySubOffset, int bin] =>
        _magnitudes[IndexOf(block, timeSubOffset, frequencySubOffset, bin)];

    /// <summary>
    /// The magnitude at one point of the waterfall, in decibels, by upstream's own inverse scale.
    /// </summary>
    public double DecibelsAt(int block, int timeSubOffset, int frequencySubOffset, int bin) =>
        DecibelsFor(this[block, timeSubOffset, frequencySubOffset, bin]);

    /// <summary>Reads a stored byte back as decibels: upstream's <c>WF_ELEM_MAG</c>.</summary>
    public static double DecibelsFor(byte stored) => (stored * 0.5) - 120.0;

    /// <summary>
    /// Turns decibels into the byte upstream would store, by upstream's own arithmetic: twice the
    /// decibels plus 240, truncated toward zero, then clamped to the range of a byte.
    /// </summary>
    /// <remarks>
    /// <b>The clamp is applied to the signed integer, after the truncation</b> — which is upstream's
    /// order and matters at the bottom end, where a large negative reading truncates toward zero
    /// before it is clamped to zero rather than the other way round.
    /// </remarks>
    public static byte StoredFor(float decibels)
    {
        var scaled = (int)((2 * decibels) + 240);
        return scaled < 0 ? (byte)0 : scaled > 255 ? (byte)255 : (byte)scaled;
    }

    /// <summary>The index into <see cref="Magnitudes"/> of one point.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Any index is outside its extent.</exception>
    public int IndexOf(int block, int timeSubOffset, int frequencySubOffset, int bin)
    {
        if (block < 0 || block >= Geometry.MaxBlocks)
        {
            throw new ArgumentOutOfRangeException(
                nameof(block), block, $"There are {Geometry.MaxBlocks} blocks in this waterfall.");
        }

        if (timeSubOffset < 0 || timeSubOffset >= Geometry.TimeOversampling)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeSubOffset),
                timeSubOffset,
                $"There are {Geometry.TimeOversampling} time sub-offsets per block.");
        }

        if (frequencySubOffset < 0 || frequencySubOffset >= Geometry.FrequencyOversampling)
        {
            throw new ArgumentOutOfRangeException(
                nameof(frequencySubOffset),
                frequencySubOffset,
                $"There are {Geometry.FrequencyOversampling} frequency sub-offsets per bin.");
        }

        if (bin < 0 || bin >= Geometry.BinCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bin), bin, $"There are {Geometry.BinCount} bins in this waterfall.");
        }

        return (block * Geometry.BlockStride)
            + (((timeSubOffset * Geometry.FrequencyOversampling) + frequencySubOffset) * Geometry.BinCount)
            + bin;
    }

    internal Span<byte> Store => _magnitudes;
}

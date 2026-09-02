using System;

namespace Ft8Sharp.Dsp;

/// <summary>
/// Turns audio into a waterfall: the receive front end, ported faithfully from upstream's
/// <c>common/monitor.c</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Faithful, not improved.</b> The window is upstream's Hann written as a squared sine, over the
/// whole transform; the normalisation is folded into the window coefficients rather than applied to
/// the bins, because that is where upstream applies it and the two differ in the last place; the
/// analysis frame slides by a sub-block and is prefilled with zeros; the magnitude is decibels of
/// power with upstream's floor inside the logarithm; and the store is a byte at half a decibel per
/// count. <b>The geometry is computed in single precision</b>, which is load-bearing — see
/// <see cref="Ft8WaterfallGeometry"/>.
/// </para>
/// <para>
/// <b>What this does NOT do.</b> It does not search, it does not correlate against the Costas
/// pattern, it does not score, it does not rank and it does not decode. It turns samples into
/// magnitudes and stops. Finding a signal nobody told it the location of is the next unit's work,
/// and nothing here is evidence about it.
/// </para>
/// <para>
/// <b>One deliberate divergence in the arithmetic:</b> upstream's transform is single precision
/// (<c>kiss_fft_scalar</c> is <c>float</c>) and <see cref="Ft8RealFft"/> computes in double. There
/// is no bit-identity to lose, because this is a different transform algorithm from the one upstream
/// vendors and agreement in the last place was never available; and the store quantises to half a
/// decibel, which is coarser than either precision by ten orders of magnitude. Recorded in
/// <c>porting-notes.md</c>.
/// </para>
/// <para>
/// <b>Not thread-safe.</b> It owns the sliding frame and the transform's scratch. One per thread.
/// </para>
/// </remarks>
public sealed class Ft8Monitor
{
    private readonly Ft8RealFft _transform;
    private readonly double[] _window;
    private readonly double[] _frame;
    private readonly double[] _windowed;
    private readonly double[] _binReal;
    private readonly double[] _binImaginary;

    /// <summary>Builds a monitor for one geometry.</summary>
    /// <exception cref="ArgumentNullException">The geometry is null.</exception>
    public Ft8Monitor(Ft8WaterfallGeometry? geometry = null)
    {
        Geometry = geometry ?? new Ft8WaterfallGeometry();

        var n = Geometry.TransformLength;
        _transform = new Ft8RealFft(n);

        // The normalisation is multiplied into the window, which is where upstream puts it. Scaling
        // the samples going in and scaling the bins coming out are the same thing in exact
        // arithmetic and are not the same thing in floating point.
        var normalisation = 2.0f / n;
        _window = new double[n];
        for (var i = 0; i < n; i++)
        {
            _window[i] = normalisation * HannSquaredSine(i, n);
        }

        _frame = new double[n];
        _windowed = new double[n];
        _binReal = new double[_transform.BinCount];
        _binImaginary = new double[_transform.BinCount];

        Waterfall = new Ft8Waterfall(Geometry);
    }

    /// <summary>The extents this monitor analyses to.</summary>
    public Ft8WaterfallGeometry Geometry { get; }

    /// <summary>The waterfall being filled.</summary>
    public Ft8Waterfall Waterfall { get; private set; }

    /// <summary>
    /// Upstream's window: the square of a sine, which is a Hann window written the way
    /// <c>hann_i</c> writes it.
    /// </summary>
    /// <remarks>
    /// sin²(πi/N) and (1 - cos(2πi/N))/2 are the same function and are not the same arithmetic. The
    /// squared sine is what the pin computes, so it is what this computes.
    /// </remarks>
    public static double HannSquaredSine(int index, int length)
    {
        var x = Math.Sin(Math.PI * index / length);
        return x * x;
    }

    /// <summary>Empties the waterfall and the sliding analysis frame.</summary>
    /// <remarks>
    /// <b>The frame is zeroed, not left</b>, which is upstream's <c>calloc</c>. It means the first
    /// blocks of a slot are analysed through a window that is partly empty, and that is upstream's
    /// behaviour rather than a defect to correct here.
    /// </remarks>
    public void Reset()
    {
        Array.Clear(_frame);
        Waterfall = new Ft8Waterfall(Geometry);
    }

    /// <summary>
    /// Analyses one block — one symbol period of audio — and appends it to the waterfall.
    /// </summary>
    /// <param name="block">
    /// Exactly <see cref="Ft8WaterfallGeometry.BlockSize"/> samples, in the range the synthesizer
    /// produces.
    /// </param>
    /// <returns>
    /// False when the waterfall is already full, in which case nothing was written. <b>It reports
    /// rather than throws</b>, matching upstream, which returns early: a caller streaming a slot
    /// runs past the end as a matter of course and that is not an error.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The block is not exactly one block long. <b>Refused rather than analysed</b> — a short block
    /// analysed as a whole one would put a partly stale frame into the waterfall and report it as
    /// data. Refused before the frame is touched, so a rejected call leaves the monitor exactly as
    /// it found it and the next good block is analysed against the right history.
    /// </exception>
    public bool ProcessBlock(ReadOnlySpan<float> block)
    {
        if (block.Length != Geometry.BlockSize)
        {
            throw new ArgumentException(
                $"A block is exactly {Geometry.BlockSize} samples at {Geometry.SampleRate} Hz and "
                + $"{block.Length} were given. Refused rather than analysed over what arrived: the "
                + "analysis frame slides, so a short block would mix stale history into the "
                + "waterfall and store it as though it were signal. The frame has not been touched.",
                nameof(block));
        }

        if (Waterfall.BlockCount >= Geometry.MaxBlocks)
        {
            return false;
        }

        var n = Geometry.TransformLength;
        var advance = Geometry.SubblockSize;
        var store = Waterfall.Store;
        var offset = Waterfall.BlockCount * Geometry.BlockStride;
        var framePosition = 0;
        var largest = Waterfall.LargestDecibels;

        for (var timeSub = 0; timeSub < Geometry.TimeOversampling; timeSub++)
        {
            // Slide the analysis frame left by one sub-block and fill the tail from the input.
            Array.Copy(_frame, advance, _frame, 0, n - advance);
            for (var position = n - advance; position < n; position++)
            {
                _frame[position] = block[framePosition];
                framePosition++;
            }

            for (var position = 0; position < n; position++)
            {
                _windowed[position] = _window[position] * _frame[position];
            }

            _transform.Transform(_windowed, _binReal, _binImaginary);

            for (var freqSub = 0; freqSub < Geometry.FrequencyOversampling; freqSub++)
            {
                for (var bin = Geometry.MinBin; bin < Geometry.MaxBin; bin++)
                {
                    var source = (bin * Geometry.FrequencyOversampling) + freqSub;
                    var re = _binReal[source];
                    var im = _binImaginary[source];

                    // Imaginary part first, as upstream sums it. Same value, and the habit costs
                    // nothing.
                    var power = (im * im) + (re * re);

                    // Single precision at the point of storage, because the store is upstream's and
                    // so is the truncation that fills it.
                    var decibels = (float)(10.0 * Math.Log10(1e-12 + power));
                    store[offset] = Ft8Waterfall.StoredFor(decibels);
                    offset++;

                    if (decibels > largest)
                    {
                        largest = decibels;
                    }
                }
            }
        }

        Waterfall.LargestDecibels = largest;
        Waterfall.BlockCount++;
        return true;
    }

    /// <summary>
    /// Analyses as much of a signal as whole blocks cover, from the start, and returns the
    /// waterfall.
    /// </summary>
    /// <param name="samples">The audio. At least one block long.</param>
    /// <returns>The waterfall, with <see cref="Ft8Waterfall.BlockCount"/> blocks filled.</returns>
    /// <exception cref="ArgumentException">
    /// The signal is shorter than one block. <b>Refused rather than analysed</b>: returning an empty
    /// waterfall for a signal a caller believes it handed over is the failure that looks like a
    /// silent band.
    /// </exception>
    /// <remarks>
    /// <b>A trailing partial block is dropped, not padded</b>, which is upstream's own loop
    /// condition in <c>decode_ft8.c</c>: it advances while a whole block still fits. A slot of
    /// 180000 samples at 12 kHz gives 93 blocks and 1440 samples left over.
    /// </remarks>
    public Ft8Waterfall Analyse(ReadOnlySpan<float> samples)
    {
        if (samples.Length < Geometry.BlockSize)
        {
            throw new ArgumentException(
                $"An analysis needs at least one block of {Geometry.BlockSize} samples and was given "
                + $"{samples.Length}. Refused rather than returning an empty waterfall, which a "
                + "caller would read as a silent band rather than as a mistake.",
                nameof(samples));
        }

        Reset();

        for (var position = 0; position + Geometry.BlockSize <= samples.Length; position += Geometry.BlockSize)
        {
            if (!ProcessBlock(samples.Slice(position, Geometry.BlockSize)))
            {
                break;
            }
        }

        return Waterfall;
    }
}

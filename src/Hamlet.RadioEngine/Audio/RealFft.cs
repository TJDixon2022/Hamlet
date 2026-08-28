namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// A radix-2 fast Fourier transform, in place, for real input.
/// </summary>
/// <remarks>
/// <para>**THE ENGINE HAD NO FFT AND THAT WAS A RULING, NOT AN OVERSIGHT.**
/// `CLAUDE.md` §6 records it: *"None, and that is the answer rather than a
/// deferral … the CW decoder wants a couple of dozen known frequencies rather
/// than a whole spectrum, which is a Goertzel filter bank."* It names its own
/// reopening condition in the next sentence — *"The question reopens if phase 3
/// needs a wideband transform in software"* — and an FT8 waterfall is that
/// condition arriving.</para>
/// <para>**WHY GOERTZEL WILL NOT DO HERE.** A Goertzel costs one filter per bin.
/// The digital waterfall wants roughly four hundred and fifty bins across 200 to
/// 3000 hertz, several times a second, where the CW decoder wants twenty-five.
/// At that width the transform that shares work between bins is the whole point,
/// and running four hundred and fifty independent filters would be the same
/// arithmetic done four hundred and fifty times over.</para>
/// <para>**NO DEPENDENCY, BECAUSE THIS IS SMALL.** A radix-2 Cooley-Tukey over a
/// power-of-two window is about forty lines and has no state. Taking a package
/// for it would add a supply chain to a project heading for public release
/// (§2.1) in exchange for arithmetic that fits on a screen.</para>
/// <para>**PURE AND ALLOCATION-FREE PER CALL.** The caller owns the buffers and
/// the twiddle tables are built once per size, so nothing here allocates on the
/// audio thread (§8's never-throw, never-stutter discipline).</para>
/// </remarks>
public sealed class RealFft
{
    private readonly int _size;
    private readonly double[] _cos;
    private readonly double[] _sin;
    private readonly int[] _reversed;

    /// <summary>Creates a transform for one window size.</summary>
    /// <param name="size">Samples per window. Must be a power of two.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The size is not a power of two, or is smaller than four.
    /// </exception>
    public RealFft(int size)
    {
        if (size < 4 || (size & (size - 1)) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size), size, "the window must be a power of two, at least four");
        }

        _size = size;
        _cos = new double[size / 2];
        _sin = new double[size / 2];

        for (var i = 0; i < size / 2; i++)
        {
            var angle = -2 * Math.PI * i / size;
            _cos[i] = Math.Cos(angle);
            _sin[i] = Math.Sin(angle);
        }

        // Bit-reversal permutation, precomputed so the transform itself is
        // arithmetic and nothing else.
        _reversed = new int[size];
        var bits = (int)Math.Log2(size);

        for (var i = 0; i < size; i++)
        {
            var r = 0;

            for (var b = 0; b < bits; b++)
            {
                r = (r << 1) | ((i >> b) & 1);
            }

            _reversed[i] = r;
        }
    }

    /// <summary>Samples per window.</summary>
    public int Size => _size;

    /// <summary>How many magnitude bins a window yields.</summary>
    /// <remarks>
    /// Half the window plus one: a real signal's spectrum is symmetric, so
    /// everything above the halfway point is the mirror of what is below it and
    /// carries nothing new.
    /// </remarks>
    public int BinCount => (_size / 2) + 1;

    /// <summary>The frequency one bin sits at.</summary>
    /// <param name="bin">Which bin.</param>
    /// <param name="sampleRate">Samples per second.</param>
    /// <returns>The bin's center frequency in hertz.</returns>
    public double BinHz(int bin, int sampleRate)
        => (double)bin * sampleRate / _size;

    /// <summary>
    /// Transform one window of real samples into magnitudes.
    /// </summary>
    /// <param name="samples">
    /// The window, exactly <see cref="Size"/> long. Read, never written.
    /// </param>
    /// <param name="magnitudes">
    /// Filled with <see cref="BinCount"/> magnitudes. The caller owns it.
    /// </param>
    /// <param name="real">Working room, <see cref="Size"/> long.</param>
    /// <param name="imaginary">Working room, <see cref="Size"/> long.</param>
    /// <exception cref="ArgumentException">A buffer is the wrong length.</exception>
    /// <remarks>
    /// **THE WINDOW FUNCTION IS THE CALLER'S.** A transform that applied its own
    /// taper could not be used to ask what an untapered window looks like, and
    /// the two questions have different right answers.
    /// </remarks>
    public void Magnitudes(
        ReadOnlySpan<float> samples,
        Span<double> magnitudes,
        Span<double> real,
        Span<double> imaginary)
    {
        if (samples.Length != _size)
        {
            throw new ArgumentException(
                $"the window is {samples.Length} samples and this transform is "
                + $"built for {_size}",
                nameof(samples));
        }

        if (magnitudes.Length < BinCount)
        {
            throw new ArgumentException(
                $"there is room for {magnitudes.Length} magnitudes and this "
                + $"transform produces {BinCount}",
                nameof(magnitudes));
        }

        if (real.Length < _size || imaginary.Length < _size)
        {
            throw new ArgumentException(
                "the working buffers must each be as long as the window",
                nameof(real));
        }

        for (var i = 0; i < _size; i++)
        {
            real[_reversed[i]] = samples[i];
            imaginary[i] = 0;
        }

        for (var span = 2; span <= _size; span <<= 1)
        {
            var half = span / 2;
            var step = _size / span;

            for (var start = 0; start < _size; start += span)
            {
                for (var k = 0; k < half; k++)
                {
                    var twiddle = k * step;
                    var c = _cos[twiddle];
                    var s = _sin[twiddle];

                    var a = start + k;
                    var b = a + half;

                    var tr = (real[b] * c) - (imaginary[b] * s);
                    var ti = (real[b] * s) + (imaginary[b] * c);

                    real[b] = real[a] - tr;
                    imaginary[b] = imaginary[a] - ti;
                    real[a] += tr;
                    imaginary[a] += ti;
                }
            }
        }

        for (var bin = 0; bin < BinCount; bin++)
        {
            magnitudes[bin] = Math.Sqrt(
                (real[bin] * real[bin]) + (imaginary[bin] * imaginary[bin]));
        }
    }
}

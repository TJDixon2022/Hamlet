namespace Hamlet.RadioEngine.Audio;

/// <summary>
/// A windowed sinc, tabulated once and read with linear interpolation.
/// </summary>
/// <remarks>
/// <para>**ONE KERNEL, TWO RESAMPLERS** (§0, generated from a source of truth rather than
/// hand-copied). <see cref="Ft8Resample"/> puts a whole slot on the 12 kHz grid in one
/// block; <see cref="Psk31Resampler"/> puts a live stream on the 8 kHz grid a lump at a
/// time. **The filter is the same filter**, and a second copy of it would be a second
/// place for the taps to drift.</para>
/// <para>A slot at 48 kHz needs about twenty-five million taps. Evaluating a sine for
/// each is a second of arithmetic on the press; reading a table of nine thousand entries
/// is a few milliseconds, and the interpolation error is orders below the quantization of
/// the audio it is filtering.</para>
/// </remarks>
internal sealed class SincKernel
{
    private const int PerSample = 128;

    private readonly double[] _taps;

    /// <summary>Tabulates the kernel.</summary>
    /// <param name="cutoff">Where the filter goes, in cycles per input sample.</param>
    /// <param name="zeroCrossings">How many zero crossings each side are kept.</param>
    internal SincKernel(double cutoff, int zeroCrossings)
    {
        HalfWidth = zeroCrossings / (2 * cutoff);

        _taps = new double[(int)Math.Ceiling(HalfWidth * PerSample) + 2];

        for (var k = 0; k < _taps.Length; k++)
        {
            var x = k / (double)PerSample;
            _taps[k] = x > HalfWidth
                ? 0
                : Sinc(2 * cutoff * x) * Blackman(x / HalfWidth);
        }
    }

    /// <summary>How far the kernel reaches, in input samples.</summary>
    internal double HalfWidth { get; }

    /// <summary>The tap at a distance, in input samples, from the centre.</summary>
    /// <param name="x">The distance, in input samples.</param>
    /// <returns>The tap, or zero past the kernel's reach.</returns>
    internal double At(double x)
    {
        var place = Math.Abs(x) * PerSample;
        var k = (int)place;

        if (k + 1 >= _taps.Length)
        {
            return 0;
        }

        var fraction = place - k;

        return (_taps[k] * (1 - fraction)) + (_taps[k + 1] * fraction);
    }

    private static double Sinc(double u)
        => u == 0 ? 1 : Math.Sin(Math.PI * u) / (Math.PI * u);

    /// <summary>The Blackman window, over a half-width normalized to one.</summary>
    private static double Blackman(double w)
    {
        var t = (Math.Clamp(w, -1, 1) + 1) / 2;

        return 0.42
            - (0.5 * Math.Cos(2 * Math.PI * t))
            + (0.08 * Math.Cos(4 * Math.PI * t));
    }
}

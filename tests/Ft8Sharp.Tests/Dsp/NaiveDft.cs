namespace Ft8Sharp.Tests.Dsp;

/// <summary>
/// The discrete Fourier transform as it is defined, computed directly. <b>The independent side of
/// the comparison.</b>
/// </summary>
/// <remarks>
/// <para>
/// <b>This calls nothing in the library.</b> No plan, no factorisation, no twiddle table, no
/// recursion, no shared trigonometry — every term is <c>cos</c> and <c>sin</c> of its own angle,
/// computed where it is used. That is the whole point: an FFT checked against another FFT is two
/// programs agreeing, and an FFT checked against the defining sum is a program agreeing with the
/// definition. The definition is the stronger authority, because it is what the word means.
/// </para>
/// <code>
///   X[k] = sum over n of x[n] * exp(-2*pi*i*k*n/N)
/// </code>
/// <para>
/// <b>It is O(N^2) and that is not a defect.</b> Slowness is the evidence that it shares no
/// structure with the thing it checks. Nothing here is optimised, memoised or reordered, because
/// every one of those is a place the two implementations could come to share a mistake.
/// </para>
/// <para>
/// <b>The angle is reduced before the trigonometry.</b> <c>k*n</c> runs to N^2, and handing an angle
/// of ten million radians to <c>Math.Cos</c> asks it to do an argument reduction that costs more
/// accuracy than the transform under test has to spare. Taking <c>k*n mod N</c> first is exact in
/// integers and leaves an angle inside one turn. This is the only concession to arithmetic in the
/// file and it makes the reference <em>more</em> faithful to the definition, not less.
/// </para>
/// </remarks>
internal static class NaiveDft
{
    /// <summary>The forward transform of a complex sequence, term by term.</summary>
    public static void Forward(
        IReadOnlyList<double> real,
        IReadOnlyList<double> imaginary,
        double[] outputReal,
        double[] outputImaginary)
    {
        var n = real.Count;

        for (var k = 0; k < n; k++)
        {
            double sumReal = 0;
            double sumImaginary = 0;

            for (var t = 0; t < n; t++)
            {
                var turns = (long)k * t % n;
                var angle = -2.0 * Math.PI * turns / n;
                var c = Math.Cos(angle);
                var s = Math.Sin(angle);

                sumReal += (real[t] * c) - (imaginary[t] * s);
                sumImaginary += (real[t] * s) + (imaginary[t] * c);
            }

            outputReal[k] = sumReal;
            outputImaginary[k] = sumImaginary;
        }
    }

    /// <summary>
    /// The forward transform of a real sequence, over the one-sided bin range only. Same sum, with
    /// the imaginary part of every input taken as zero rather than passed in as zeros.
    /// </summary>
    public static void ForwardReal(IReadOnlyList<double> samples, double[] outputReal, double[] outputImaginary)
    {
        var n = samples.Count;
        var bins = (n / 2) + 1;

        for (var k = 0; k < bins; k++)
        {
            double sumReal = 0;
            double sumImaginary = 0;

            for (var t = 0; t < n; t++)
            {
                var turns = (long)k * t % n;
                var angle = -2.0 * Math.PI * turns / n;

                sumReal += samples[t] * Math.Cos(angle);
                sumImaginary += samples[t] * Math.Sin(angle);
            }

            outputReal[k] = sumReal;
            outputImaginary[k] = sumImaginary;
        }
    }
}

using System.Numerics;

namespace Hamlet.RadioEngine.Tests.Cw.Fixtures;

/// <summary>
/// Power spectra measured off rendered samples, for the tests that prove a
/// fixture is what it says (work instruction 461; CW_SPEC.md 8.1 and 9.1).
/// </summary>
/// <remarks>
/// <para>**THE TESTS' OWN INSTRUMENT, NOT THE ENGINE'S.** A fixture proved with the
/// decoder's transform would share any mistake in it (CLAUDE.md 12.5), so this is
/// a plain radix-two transform and a Welch average, written here and nowhere
/// else.</para>
/// <para>Every estimate is a Hann-windowed Welch average at half overlap,
/// normalised so the spectrum integrates to the mean power of what was
/// measured.</para>
/// </remarks>
public static class CwSpectrum
{
    /// <summary>An in-place radix-two transform.</summary>
    /// <param name="x">The samples; their count a power of two.</param>
    public static void Fft(Complex[] x)
    {
        var n = x.Length;

        for (int i = 1, j = 0; i < n; i++)
        {
            var bit = n >> 1;

            for (; (j & bit) != 0; bit >>= 1)
            {
                j ^= bit;
            }

            j ^= bit;

            if (i < j)
            {
                (x[i], x[j]) = (x[j], x[i]);
            }
        }

        for (var len = 2; len <= n; len <<= 1)
        {
            var step = Complex.FromPolarCoordinates(1, -2 * Math.PI / len);

            for (var i = 0; i < n; i += len)
            {
                var w = Complex.One;

                for (var k = 0; k < len / 2; k++)
                {
                    var u = x[i + k];
                    var v = x[i + k + (len / 2)] * w;

                    x[i + k] = u + v;
                    x[i + k + (len / 2)] = u - v;
                    w *= step;
                }
            }
        }
    }

    /// <summary>
    /// The two-sided power spectral density of a complex sequence, bin by bin from
    /// minus half the rate to just under plus half.
    /// </summary>
    /// <param name="x">The sequence.</param>
    /// <param name="rate">Its sample rate, in hertz.</param>
    /// <param name="length">The segment length, a power of two.</param>
    /// <returns>The frequency of each bin and the density there, power per hertz.</returns>
    public static (double[] Hz, double[] Density) Welch(Complex[] x, double rate, int length)
    {
        var window = Hann(length);
        var windowPower = window.Sum(w => w * w);
        var sum = new double[length];
        var segments = 0;

        for (var at = 0; at + length <= x.Length; at += length / 2)
        {
            var buffer = new Complex[length];

            for (var k = 0; k < length; k++)
            {
                buffer[k] = x[at + k] * window[k];
            }

            Fft(buffer);

            for (var k = 0; k < length; k++)
            {
                sum[k] += buffer[k].Magnitude * buffer[k].Magnitude;
            }

            segments++;
        }

        if (segments == 0)
        {
            throw new ArgumentException("Shorter than one segment.", nameof(x));
        }

        var hz = new double[length];
        var density = new double[length];

        // Rotate so the bins run from the most negative frequency up.
        for (var k = 0; k < length; k++)
        {
            var bin = (k + (length / 2)) % length;

            hz[k] = (bin < length / 2 ? bin : bin - length) * rate / length;
            density[k] = sum[bin] / segments / (windowPower * rate);
        }

        return (hz, density);
    }

    /// <summary>
    /// The one-sided power spectral density of a real sequence, from nought to half
    /// the rate.
    /// </summary>
    /// <param name="x">The samples.</param>
    /// <param name="rate">Their rate, in hertz.</param>
    /// <param name="length">The segment length, a power of two.</param>
    /// <returns>The frequency of each bin and the density there, power per hertz.</returns>
    public static (double[] Hz, double[] Density) WelchReal(IReadOnlyList<float> x, double rate, int length)
    {
        var complex = new Complex[x.Count];

        for (var k = 0; k < complex.Length; k++)
        {
            complex[k] = x[k];
        }

        var (hz, twoSided) = Welch(complex, rate, length);
        var half = length / 2;
        var outHz = new double[half + 1];
        var outDensity = new double[half + 1];

        // Bin h of the rotated array is frequency h - half; fold the negative side
        // onto the positive so the one-sided density integrates to the power.
        for (var k = 0; k <= half; k++)
        {
            var positive = half + k < length ? twoSided[half + k] : twoSided[0];
            var negative = twoSided[half - k];

            outHz[k] = k * rate / length;
            outDensity[k] = k == 0 || k == half ? positive : positive + negative;
        }

        return (outHz, outDensity);
    }

    /// <summary>The mean density over the bins within a stated distance of a frequency.</summary>
    /// <param name="hz">Bin frequencies.</param>
    /// <param name="density">Bin densities.</param>
    /// <param name="centreHz">Where.</param>
    /// <param name="halfWidthHz">How far either side.</param>
    /// <returns>The mean density there.</returns>
    public static double DensityNear(double[] hz, double[] density, double centreHz, double halfWidthHz)
    {
        double sum = 0;
        var count = 0;

        for (var k = 0; k < hz.Length; k++)
        {
            if (Math.Abs(hz[k] - centreHz) <= halfWidthHz)
            {
                sum += density[k];
                count++;
            }
        }

        return sum / Math.Max(1, count);
    }

    private static double[] Hann(int length)
    {
        var w = new double[length];

        for (var k = 0; k < length; k++)
        {
            w[k] = 0.5 * (1 - Math.Cos(2 * Math.PI * k / length));
        }

        return w;
    }
}

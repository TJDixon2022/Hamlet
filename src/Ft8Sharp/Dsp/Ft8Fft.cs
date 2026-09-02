using System;
using System.Collections.Generic;

namespace Ft8Sharp.Dsp;

/// <summary>
/// A complex discrete Fourier transform, computed by mixed-radix Cooley–Tukey decimation in time.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is written from the mathematics and is not a port of anything.</b> <c>ft8_lib</c> does not
/// implement a transform; it vendors one — KISS FFT, Copyright 2003–2010 Mark Borgerding, under
/// BSD-3-Clause, in its own folder. That is a second copyright holder under a second licence, and
/// <c>Ft8Sharp</c> carries one <c>LICENSE</c> (Tim's MIT) and a <c>NOTICE</c> crediting Goba.
/// Adding a third party's obligations to a library headed for publication is the owner's decision
/// and not a unit's, so nothing in that folder was read beyond its licence header — recorded by
/// <c>UpstreamWaterfallInventoryTests.TheVendoredFftsCopyrightAndLicenceAreOnTheRecord</c> so that
/// the decision stands on a measurement. Cooley–Tukey is sixty years of public literature and needs
/// no route through anyone's source.
/// </para>
/// <para>
/// <b>Why mixed radix and not radix-2 alone.</b> Upstream's monitor transforms one symbol block
/// multiplied by the frequency oversampling factor — 1920 × 2 = <b>3840 samples</b> at 12 kHz — and
/// 3840 is <b>not a power of two</b>; it is 2^8 × 3 × 5. A radix-2 transform alone cannot compute the
/// length this library actually needs. The decomposition here is the general one, of which radix-2 is
/// the special case: for a power-of-two length every stage <em>is</em> a radix-2 butterfly, and
/// <see cref="Factors"/> says so.
/// </para>
/// <para>
/// <b>The decomposition.</b> For n = p·m, split the input into p subsequences taken every p-th
/// sample; transform each to length m; then for every k in 0..m-1 twiddle the p results by
/// W_n^(jk) and combine them with a p-point transform, which lands X[k + q·m] for q in 0..p-1.
/// W denotes exp(-2πi/n). Radix-2 combines are written as an explicit butterfly; other radices use
/// the defining p-point sum, which is exact and, at p ∈ {3, 5}, cheap.
/// </para>
/// <para>
/// <b>Deterministic by construction.</b> One code path, one arithmetic order, no parallelism, no
/// accumulated state between calls. Step 4's third exit criterion is <i>candidate ranking is stable
/// across runs</i>, and bit-identical transform output is the floor that rests on.
/// </para>
/// <para>
/// <b>Double precision, and that is a deliberate divergence.</b> Upstream's <c>kiss_fft_scalar</c> is
/// <c>float</c>. This computes in <c>double</c>. There is no bit-identity to lose — this is a
/// different algorithm from the one upstream vendors, so agreement in the last place was never
/// available — and the waterfall stores magnitudes quantised to half a decibel, which is coarser than
/// either precision by ten orders of magnitude. Recorded in <c>porting-notes.md</c>.
/// </para>
/// <para>
/// <b>A plan is reusable and is not thread-safe.</b> It owns scratch buffers so that transforming a
/// slot does not allocate per block. Use one per thread.
/// </para>
/// </remarks>
public sealed class Ft8Fft
{
    private readonly int[] _factors;
    private readonly double[] _twiddleReal;
    private readonly double[] _twiddleImaginary;
    private readonly double[] _scratchReal;
    private readonly double[] _scratchImaginary;
    private readonly int _widestRadix;

    /// <summary>Builds a plan for transforms of exactly <paramref name="length"/> points.</summary>
    /// <param name="length">The transform length. Must be at least one.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The length is zero or negative. <b>Refused rather than computed</b> — HM-DEC-009. A
    /// zero-length transform has no defined output and returning an empty one quietly would let a
    /// caller that had mis-computed its geometry carry on.
    /// </exception>
    public Ft8Fft(int length)
    {
        if (length < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                length,
                "A transform length must be at least one. A length of zero or less has no defined "
                + "transform, and computing something for it would hide the caller's arithmetic error.");
        }

        Length = length;
        _factors = Factorise(length);

        // W_length^t for every t, so a sub-transform of length n uses stride length/n into the same
        // table. One table, one set of cosines, and every stage of every recursion reads it — which
        // is also what makes the arithmetic order fixed and the output reproducible.
        _twiddleReal = new double[length];
        _twiddleImaginary = new double[length];
        for (var t = 0; t < length; t++)
        {
            var angle = -2.0 * Math.PI * t / length;
            _twiddleReal[t] = Math.Cos(angle);
            _twiddleImaginary[t] = Math.Sin(angle);
        }

        var widest = 1;
        foreach (var factor in _factors)
        {
            if (factor > widest)
            {
                widest = factor;
            }
        }

        // Only one combine is ever running — the children have all returned before a parent gathers —
        // but a region per depth keeps that from being a thing a reader has to prove. Each level
        // gathers at most `widest` points before writing them back, so the combine is in-place-safe
        // without a second buffer the size of the transform.
        _widestRadix = widest;
        _scratchReal = new double[widest * (_factors.Length + 1)];
        _scratchImaginary = new double[widest * (_factors.Length + 1)];
    }

    /// <summary>The number of points this plan transforms.</summary>
    public int Length { get; }

    /// <summary>
    /// The radices, in the order the stages apply them, smallest prime first. For a power-of-two
    /// length every entry is 2 and the transform is a pure radix-2 Cooley–Tukey.
    /// </summary>
    public IReadOnlyList<int> Factors => _factors;

    /// <summary>
    /// True when every stage is a radix-2 butterfly, which is exactly when <see cref="Length"/> is a
    /// power of two.
    /// </summary>
    public bool IsPureRadix2 => Array.TrueForAll(_factors, f => f == 2);

    /// <summary>Transforms one complex sequence.</summary>
    /// <param name="inputReal">The real parts. Exactly <see cref="Length"/> of them.</param>
    /// <param name="inputImaginary">The imaginary parts. Exactly <see cref="Length"/> of them.</param>
    /// <param name="outputReal">Receives the real parts. Exactly <see cref="Length"/> of them.</param>
    /// <param name="outputImaginary">Receives the imaginary parts. Exactly <see cref="Length"/>.</param>
    /// <exception cref="ArgumentException">
    /// Any of the four spans is not exactly <see cref="Length"/> long. <b>Refused rather than
    /// computed, and refused before anything is written</b> — every span is measured first, so a
    /// rejected call cannot leave a partly transformed buffer behind for a caller that swallowed the
    /// exception to read back as data.
    /// </exception>
    public void Transform(
        ReadOnlySpan<double> inputReal,
        ReadOnlySpan<double> inputImaginary,
        Span<double> outputReal,
        Span<double> outputImaginary)
    {
        Require(inputReal.Length, nameof(inputReal));
        Require(inputImaginary.Length, nameof(inputImaginary));
        Require(outputReal.Length, nameof(outputReal));
        Require(outputImaginary.Length, nameof(outputImaginary));

        Decimate(inputReal, inputImaginary, 0, 1, outputReal, outputImaginary, 0, Length, 0);
    }

    private void Require(int actual, string name)
    {
        if (actual != Length)
        {
            throw new ArgumentException(
                $"This plan transforms exactly {Length} points and {name} holds {actual}. "
                + "A mismatched buffer is refused rather than transformed over the shorter of the "
                + "two, because a transform of a prefix is not a transform and would be reported as "
                + "one. Nothing has been written.",
                name);
        }
    }

    /// <summary>
    /// One stage of the decimation. Transforms the sub-sequence that starts at
    /// <paramref name="inputOffset"/> and steps by <paramref name="stride"/>, writing
    /// <paramref name="n"/> points contiguously from <paramref name="outputOffset"/>.
    /// </summary>
    private void Decimate(
        ReadOnlySpan<double> inputReal,
        ReadOnlySpan<double> inputImaginary,
        int inputOffset,
        int stride,
        Span<double> outputReal,
        Span<double> outputImaginary,
        int outputOffset,
        int n,
        int depth)
    {
        if (n == 1)
        {
            // The transform of a single point is the point. This is where the recursion bottoms out
            // and it is also what performs the digit reversal: the input offsets arrive here in the
            // permuted order and are written in natural order.
            outputReal[outputOffset] = inputReal[inputOffset];
            outputImaginary[outputOffset] = inputImaginary[inputOffset];
            return;
        }

        var radix = _factors[depth];
        var m = n / radix;

        for (var j = 0; j < radix; j++)
        {
            Decimate(
                inputReal,
                inputImaginary,
                inputOffset + (j * stride),
                stride * radix,
                outputReal,
                outputImaginary,
                outputOffset + (j * m),
                m,
                depth + 1);
        }

        Combine(outputReal, outputImaginary, outputOffset, n, radix, m, depth);
    }

    /// <summary>
    /// Twiddles the <paramref name="radix"/> sub-transforms sitting contiguously from
    /// <paramref name="outputOffset"/> and combines them with a <paramref name="radix"/>-point
    /// transform, in place.
    /// </summary>
    private void Combine(
        Span<double> outputReal,
        Span<double> outputImaginary,
        int outputOffset,
        int n,
        int radix,
        int m,
        int depth)
    {
        // W_n^t is W_Length^(t * Length/n), so one global table serves every stage.
        var twiddleStride = Length / n;
        var scratchBase = depth * _widestRadix;

        for (var k = 0; k < m; k++)
        {
            // Gather and twiddle: z_j = W_n^(jk) * Y_j[k].
            for (var j = 0; j < radix; j++)
            {
                var source = outputOffset + (j * m) + k;
                var yr = outputReal[source];
                var yi = outputImaginary[source];

                if (j == 0)
                {
                    // W^0 is one, and multiplying by it would round. Not an optimisation — it keeps
                    // the k = 0 column of a radix-2 stage exactly the sum and difference.
                    _scratchReal[scratchBase + j] = yr;
                    _scratchImaginary[scratchBase + j] = yi;
                    continue;
                }

                var index = j * k * twiddleStride % Length;
                var wr = _twiddleReal[index];
                var wi = _twiddleImaginary[index];

                _scratchReal[scratchBase + j] = (yr * wr) - (yi * wi);
                _scratchImaginary[scratchBase + j] = (yr * wi) + (yi * wr);
            }

            if (radix == 2)
            {
                // The butterfly, written out because it is the case that matters and because an
                // explicit sum and difference is exact where a two-point sum through the general
                // path would multiply by a cosine of one and a sine of zero.
                var ar = _scratchReal[scratchBase];
                var ai = _scratchImaginary[scratchBase];
                var br = _scratchReal[scratchBase + 1];
                var bi = _scratchImaginary[scratchBase + 1];

                outputReal[outputOffset + k] = ar + br;
                outputImaginary[outputOffset + k] = ai + bi;
                outputReal[outputOffset + m + k] = ar - br;
                outputImaginary[outputOffset + m + k] = ai - bi;
                continue;
            }

            // The general case: X[k + q*m] = sum_j z_j * W_radix^(jq). Written as the defining sum
            // rather than a specialised radix-3 or radix-5 kernel, because at these sizes the cost is
            // nothing and a sum you can read against the definition is a sum you can check.
            var innerStride = Length / radix;
            for (var q = 0; q < radix; q++)
            {
                var sumReal = _scratchReal[scratchBase];
                var sumImaginary = _scratchImaginary[scratchBase];

                for (var j = 1; j < radix; j++)
                {
                    // In long, because for a large prime radix j*q*innerStride runs to about
                    // radix times the transform length before the wrap brings it back.
                    var index = (int)((long)j * q * innerStride % Length);
                    var wr = _twiddleReal[index];
                    var wi = _twiddleImaginary[index];
                    var zr = _scratchReal[scratchBase + j];
                    var zi = _scratchImaginary[scratchBase + j];

                    sumReal += (zr * wr) - (zi * wi);
                    sumImaginary += (zr * wi) + (zi * wr);
                }

                outputReal[outputOffset + (q * m) + k] = sumReal;
                outputImaginary[outputOffset + (q * m) + k] = sumImaginary;
            }
        }
    }

    /// <summary>
    /// The radices, smallest prime first, so a power-of-two length is all twos and the radix-2
    /// stages of a mixed length run before the odd ones.
    /// </summary>
    private static int[] Factorise(int length)
    {
        var factors = new List<int>();
        var remaining = length;

        while (remaining % 2 == 0 && remaining > 1)
        {
            factors.Add(2);
            remaining /= 2;
        }

        for (var candidate = 3; (long)candidate * candidate <= remaining; candidate += 2)
        {
            while (remaining % candidate == 0)
            {
                factors.Add(candidate);
                remaining /= candidate;
            }
        }

        if (remaining > 1)
        {
            // A prime left over is its own radix. The combine for it is the defining sum, so the
            // result is right; the cost is quadratic in that prime, which is why the geometry this
            // library uses — 3840 = 2^8 x 3 x 5 — is entirely small primes.
            factors.Add(remaining);
        }

        return factors.ToArray();
    }
}

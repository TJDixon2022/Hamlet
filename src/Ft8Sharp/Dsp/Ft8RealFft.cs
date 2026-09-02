using System;
using System.Collections.Generic;

namespace Ft8Sharp.Dsp;

/// <summary>
/// The one-sided transform of a real signal: N real samples in, N/2 + 1 complex bins out.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the entry point upstream's monitor uses</b> — task 2 read <c>kiss_fftr</c> in
/// <c>monitor_process</c> and a frequency buffer declared as <c>nfft/2 + 1</c> — so it is the shape
/// the waterfall is built on. Like <see cref="Ft8Fft"/> it is written from the mathematics; nothing
/// in the pin's vendored FFT folder was read beyond its licence header.
/// </para>
/// <para>
/// <b>The method.</b> A real sequence of length N is packed into a complex sequence of length N/2 by
/// taking even-indexed samples as real parts and odd-indexed samples as imaginary parts. One complex
/// transform of length N/2 then carries both half-transforms superimposed, and they separate because
/// the transform of a real sequence is conjugate-symmetric:
/// </para>
/// <code>
///   z[t]  = x[2t] + i·x[2t+1],           t = 0 .. N/2-1
///   Z     = DFT_{N/2}(z)
///   E[k]  = (Z[k] + conj(Z[N/2-k])) / 2      the transform of the even samples
///   O[k]  = (Z[k] - conj(Z[N/2-k])) / 2i     the transform of the odd samples
///   X[k]  = E[k] + W_N^k · O[k],         k = 0 .. N/2
/// </code>
/// <para>
/// The indices into Z are taken modulo N/2, which is what makes k = 0 and k = N/2 fall out correctly
/// rather than needing to be special-cased: both read Z[0] against its own conjugate, and W_N^(N/2)
/// is exactly -1.
/// </para>
/// <para>
/// <b>Half the work of transforming the real signal as a complex one</b>, and the reason to bother is
/// task 5: a fifteen-second slot is 93 blocks, each block is two transforms of 3840 points, and that
/// runs once per message per base frequency per time offset.
/// </para>
/// <para><b>A plan is reusable and is not thread-safe.</b> Use one per thread.</para>
/// </remarks>
public sealed class Ft8RealFft
{
    private readonly Ft8Fft _half;
    private readonly double[] _packedReal;
    private readonly double[] _packedImaginary;
    private readonly double[] _spectrumReal;
    private readonly double[] _spectrumImaginary;
    private readonly double[] _twiddleReal;
    private readonly double[] _twiddleImaginary;

    /// <summary>Builds a plan for real transforms of exactly <paramref name="length"/> samples.</summary>
    /// <param name="length">The number of real samples. Must be positive and even.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The length is zero, negative, or odd. <b>Refused rather than computed</b> — HM-DEC-009. An odd
    /// length cannot be packed into pairs, and rounding it down would transform a signal one sample
    /// shorter than the caller handed over and report the answer as if it were the whole of it.
    /// </exception>
    public Ft8RealFft(int length)
    {
        if (length < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                length,
                "A real transform needs at least two samples. Fewer has no pair to pack and no "
                + "spectrum worth the name.");
        }

        if (length % 2 != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                length,
                "A real transform length must be even: the method packs the samples into pairs, and "
                + "an odd length has a sample with no partner. Refused rather than truncated to the "
                + "even length below it, which would transform a different signal from the one given.");
        }

        Length = length;
        BinCount = (length / 2) + 1;

        var half = length / 2;
        _half = new Ft8Fft(half);
        _packedReal = new double[half];
        _packedImaginary = new double[half];
        _spectrumReal = new double[half];
        _spectrumImaginary = new double[half];

        // W_N^k for k = 0 .. N/2. Over the full length N, not the half-length the inner plan uses,
        // which is exactly the factor that recombines the even and odd half-transforms.
        _twiddleReal = new double[BinCount];
        _twiddleImaginary = new double[BinCount];
        for (var k = 0; k < BinCount; k++)
        {
            var angle = -2.0 * Math.PI * k / length;
            _twiddleReal[k] = Math.Cos(angle);
            _twiddleImaginary[k] = Math.Sin(angle);
        }
    }

    /// <summary>The number of real samples this plan transforms.</summary>
    public int Length { get; }

    /// <summary>The number of complex bins produced: <see cref="Length"/> / 2 + 1.</summary>
    public int BinCount { get; }

    /// <summary>The radices of the inner half-length complex transform.</summary>
    public IReadOnlyList<int> Factors => _half.Factors;

    /// <summary>Transforms one real sequence.</summary>
    /// <param name="samples">The real signal. Exactly <see cref="Length"/> samples.</param>
    /// <param name="binReal">Receives the real parts. Exactly <see cref="BinCount"/> of them.</param>
    /// <param name="binImaginary">Receives the imaginary parts. Exactly <see cref="BinCount"/>.</param>
    /// <exception cref="ArgumentException">
    /// Any span is the wrong length. <b>Refused before anything is written</b>: all three are measured
    /// first, so a refused call leaves the caller's output buffers exactly as it found them.
    /// </exception>
    public void Transform(ReadOnlySpan<double> samples, Span<double> binReal, Span<double> binImaginary)
    {
        if (samples.Length != Length)
        {
            throw new ArgumentException(
                $"This plan transforms exactly {Length} real samples and was given {samples.Length}. "
                + "Refused rather than transforming a prefix, which would be reported as a transform "
                + "of the whole. Nothing has been written.",
                nameof(samples));
        }

        if (binReal.Length != BinCount)
        {
            throw new ArgumentException(
                $"A real transform of {Length} samples produces {BinCount} bins and {nameof(binReal)} "
                + $"holds {binReal.Length}. Nothing has been written.",
                nameof(binReal));
        }

        if (binImaginary.Length != BinCount)
        {
            throw new ArgumentException(
                $"A real transform of {Length} samples produces {BinCount} bins and "
                + $"{nameof(binImaginary)} holds {binImaginary.Length}. Nothing has been written.",
                nameof(binImaginary));
        }

        var half = Length / 2;

        for (var t = 0; t < half; t++)
        {
            _packedReal[t] = samples[2 * t];
            _packedImaginary[t] = samples[(2 * t) + 1];
        }

        _half.Transform(_packedReal, _packedImaginary, _spectrumReal, _spectrumImaginary);

        for (var k = 0; k < BinCount; k++)
        {
            var forward = k % half;
            var mirror = (half - k) % half;

            var ar = _spectrumReal[forward];
            var ai = _spectrumImaginary[forward];
            var br = _spectrumReal[mirror];
            var bi = -_spectrumImaginary[mirror];

            // E[k] = (Z[k] + conj(Z[-k])) / 2
            var evenReal = 0.5 * (ar + br);
            var evenImaginary = 0.5 * (ai + bi);

            // O[k] = (Z[k] - conj(Z[-k])) / 2i, and dividing by i turns (u + iv) into (v - iu).
            var differenceReal = 0.5 * (ar - br);
            var differenceImaginary = 0.5 * (ai - bi);
            var oddReal = differenceImaginary;
            var oddImaginary = -differenceReal;

            var wr = _twiddleReal[k];
            var wi = _twiddleImaginary[k];

            binReal[k] = evenReal + ((oddReal * wr) - (oddImaginary * wi));
            binImaginary[k] = evenImaginary + ((oddReal * wi) + (oddImaginary * wr));
        }
    }
}

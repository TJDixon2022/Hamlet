// ----------------------------------------------------------------------------
// Ported to C# for Hamlet from fldigi - receive path only (HM-REQ-122).
// Upstream:      https://github.com/w1hkj/fldigi
// Commit:        61b97f4133c488063f3de1795c894d22d5032e8a
// Upstream file: src/include/gfft.h (g_fft::ComplexFFT, g_fft::InverseComplexFFT)
//
// DEPARTURE, listed in work instruction 456's report: gfft.h is John Green's
// radix-2/4/8 FFT in 3392 lines. What fftfilt reads from it is the discrete
// Fourier transform: ComplexFFT unscaled (gfft.h:3316, ffts1) and
// InverseComplexFFT scaled by 1/N (gfft.h:3332, iffts1 at 2253). This file
// computes the same two transforms with an iterative radix-2 Cooley-Tukey, so
// the numbers agree to floating-point rounding, not bit for bit. The sign of
// the exponent does not reach fftfilt's output: fftfilt builds its filter with
// the same forward transform it applies to the data, and |H(k)| of a real h(t)
// is the same under either sign.
//
// The upstream header follows, verbatim.
// ----------------------------------------------------------------------------
//==============================================================================
// g_fft.h:
//
// FFT library
// Copyright (C) 2013
//           Dave Freese, W1HKJ
//
// based on public domain code by John Green <green_jt@vsdec.npt.nuwc.navy.mil>
// original version is available at
//   http://hyperarchive.lcs.mit.edu/
//         /HyperArchive/Archive/dev/src/ffts-for-risc-2-c.hqx
//
// ported to C++ for fldigi by Dave Freese, W1HKJ
//
// This file is part of fldigi.
//
// Fldigi is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Fldigi is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with fldigi.  If not, see <http://www.gnu.org/licenses/>.
//==============================================================================

using System.Numerics;

namespace Hamlet.RadioEngine.Cw.Second;

/// <summary>
/// The in-place complex transforms fldigi's <c>fftfilt</c> takes from
/// <c>g_fft</c>: forward unscaled, inverse scaled by 1/N.
/// </summary>
public sealed class FldigiFft
{
    private readonly int FFT_size;

    /// <summary>Ports <c>g_fft::g_fft</c> (gfft.h:121).</summary>
    /// <param name="M">The transform length, a power of two.</param>
    public FldigiFft(int M = 8192)
    {
        if (M < 16) M = 16;
        if (M > 268435456) M = 268435456;
        if ((M & (M - 1)) != 0)
        {
            throw new ArgumentException("g_fft takes a power of two.", nameof(M));
        }
        FFT_size = M;
    }

    /// <summary>Ports <c>g_fft::ComplexFFT</c> (gfft.h:3316): forward, unscaled.</summary>
    /// <param name="buf">The data, transformed in place.</param>
    public void ComplexFFT(Complex[] buf) => Transform(buf, -1);

    /// <summary>Ports <c>g_fft::InverseComplexFFT</c> (gfft.h:3332): inverse, scaled by 1/N.</summary>
    /// <param name="buf">The data, transformed in place.</param>
    public void InverseComplexFFT(Complex[] buf)
    {
        Transform(buf, +1);
        double scale = 1.0 / FFT_size;
        for (int i = 0; i < FFT_size; i++) buf[i] *= scale;
    }

    private void Transform(Complex[] buf, int sign)
    {
        int n = FFT_size;

        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) (buf[i], buf[j]) = (buf[j], buf[i]);
        }

        for (int size = 2; size <= n; size <<= 1)
        {
            double angle = sign * 2.0 * Math.PI / size;
            int half = size >> 1;
            for (int start = 0; start < n; start += size)
            {
                for (int k = 0; k < half; k++)
                {
                    var w = new Complex(Math.Cos(angle * k), Math.Sin(angle * k));
                    var even = buf[start + k];
                    var odd = buf[start + k + half] * w;
                    buf[start + k] = even + odd;
                    buf[start + k + half] = even - odd;
                }
            }
        }
    }
}

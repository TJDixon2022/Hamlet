// ----------------------------------------------------------------------------
// Ported to C# for Hamlet from fldigi - receive path only (HM-REQ-122).
// Upstream:      https://github.com/w1hkj/fldigi
// Commit:        61b97f4133c488063f3de1795c894d22d5032e8a
// Upstream files: src/filters/fftfilt.cxx and src/include/fftfilt.h
// The low-pass path the CW receiver uses; the band-pass constructor,
// create_hpf, flush_size and rtty_filter are not used by it and are left out.
// The upstream header of fftfilt.cxx follows, verbatim.
// ----------------------------------------------------------------------------
//	fftfilt.cxx  --  Fast convolution Overlap-Add filter
//
// Filter implemented using overlap-add FFT convolution method
// h(t) characterized by Windowed-Sinc impulse response
//
// Reference:
//	 "The Scientist and Engineer's Guide to Digital Signal Processing"
//	 by Dr. Steven W. Smith, http://www.dspguide.com
//	 Chapters 16, 18 and 21
//
// Copyright (C) 2006-2008 Dave Freese, W1HKJ
//
// This file is part of fldigi.
//
// Fldigi is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
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
// ----------------------------------------------------------------------------

using System.Numerics;

namespace Hamlet.RadioEngine.Cw.Second;

/// <summary>
/// fldigi's overlap-add FFT filter, <c>fftfilt</c>, low-pass path, as ported
/// (HM-REQ-122).
/// </summary>
public sealed class FldigiFftFilter
{
    private int flen;
    private int flen2;
    private FldigiFft fft = null!;
    private Complex[] ht = null!;
    private Complex[] filter = null!;
    private Complex[] timedata = null!;
    private Complex[] freqdata = null!;
    private Complex[] ovlbuf = null!;
    private Complex[] output = null!;
    private int inptr;
    private int pass;

    /// <summary>Ports <c>fftfilt::fftfilt(double f, int len)</c> (fftfilt.cxx:105): a low-pass filter.</summary>
    /// <param name="f">The cut-off as a fraction of the sample rate.</param>
    /// <param name="len">The FFT length.</param>
    public FldigiFftFilter(double f, int len)
    {
        flen = len;
        init_filter();
        create_lpf(f);
    }

    // Ports fftfilt::fsinc (fftfilt.h:47).
    private static double fsinc(double fc, int i, int len)
    {
        return (i == len / 2) ? 2.0 * fc :
                Math.Sin(2 * Math.PI * fc * (i - (len / 2))) / (Math.PI * (i - (len / 2)));
    }

    // Ports fftfilt::_blackman (fftfilt.h:51).
    private static double _blackman(int i, int len)
    {
        return 0.42 -
                 (0.50 * Math.Cos(2.0 * Math.PI * i / len)) +
                 (0.08 * Math.Cos(4.0 * Math.PI * i / len));
    }

    // Ports fftfilt::clear_filter (fftfilt.cxx:55).
    private void clear_filter()
    {
        for (int i = 0; i < flen; i++)
        {
            filter[i] = 0;
            timedata[i] = 0;
            freqdata[i] = 0;
            output[i] = 0;
            ht[i] = 0;
        }
        for (int i = 0; i < flen2; i++)
            ovlbuf[i] = 0;
        inptr = 0;
    }

    // Ports fftfilt::init_filter (fftfilt.cxx:69).
    private void init_filter()
    {
        flen2 = flen >> 1;
        fft = new FldigiFft(flen);

        filter = new Complex[flen];
        timedata = new Complex[flen];
        freqdata = new Complex[flen];
        output = new Complex[flen];
        ovlbuf = new Complex[flen2];
        ht = new Complex[flen];
    }

    /// <summary>Ports <c>fftfilt::create_filter</c> (fftfilt.cxx:124).</summary>
    /// <param name="f1">The high-pass edge, or zero.</param>
    /// <param name="f2">The low-pass edge, or zero.</param>
    public void create_filter(double f1, double f2)
    {
        clear_filter();
        // initialize the filter to zero
        for (int i = 0; i < flen; i++) ht[i] = 0;

        // create the filter shape coefficients by fft
        // filter values initialized to the ht response h(t)
        bool b_lowpass, b_highpass;
        b_lowpass = f2 != 0;
        b_highpass = f1 != 0;

        for (int i = 0; i < flen2; i++)
        {
            ht[i] = 0;
            // combine lowpass / highpass
            // lowpass @ f2
            if (b_lowpass) ht[i] += fsinc(f2, i, flen2);
            // highighpass @ f1
            if (b_highpass) ht[i] -= fsinc(f1, i, flen2);
        }
        // highpass is delta[flen2/2] - h(t)
        if (b_highpass && f2 < f1) ht[flen2 / 2] += 1;

        for (int i = 0; i < flen2; i++)
            ht[i] *= _blackman(i, flen2);

        // this may change since green fft is in place fft
        Array.Copy(ht, filter, flen);

        // ht is flen complex points with imaginary all zero
        // first half describes h(t), second half all zeros
        // perform the cmplx forward fft to obtain H(w)
        // filter is flen/2 complex values
        fft.ComplexFFT(filter);

        // normalize the output filter for unity gain
        double scale = 0, mag;
        for (int i = 0; i < flen2; i++)
        {
            mag = Complex.Abs(filter[i]);
            if (mag > scale) scale = mag;
        }
        if (scale != 0)
        {
            for (int i = 0; i < flen; i++)
                filter[i] /= scale;
        }

        // start output after 2 full passes are complete
        pass = 1;
    }

    /// <summary>Ports <c>fftfilt::create_lpf</c> (fftfilt.h:66).</summary>
    /// <param name="f">The cut-off as a fraction of the sample rate.</param>
    public void create_lpf(double f)
    {
        create_filter(0, f);
    }

    /// <summary>Ports <c>fftfilt::run</c> (fftfilt.cxx:202): filter with fast convolution (overlap-add).</summary>
    /// <param name="in">One input sample.</param>
    /// <param name="out">The output block when one is ready; the filter's own buffer, as upstream.</param>
    /// <returns>Zero, or flen/2 when a block is ready.</returns>
    public int run(Complex @in, out Complex[] @out)
    {
        @out = output;

        // collect flen/2 input samples
        timedata[inptr++] = @in;

        if (inptr < flen2)
            return 0;
        if (pass != 0) --pass; // filter output is not stable until 2 passes

        // FFT transpose to the frequency domain
        Array.Copy(timedata, freqdata, flen);
        fft.ComplexFFT(freqdata);

        // multiply with the filter shape
        for (int i = 0; i < flen; i++)
            freqdata[i] *= filter[i];

        // transform back to time domain
        fft.InverseComplexFFT(freqdata);

        // overlap and add
        // save the second half for overlapping next inverse FFT
        for (int i = 0; i < flen2; i++)
        {
            output[i] = ovlbuf[i] + freqdata[i];
            ovlbuf[i] = freqdata[i + flen2];
        }

        // clear inbuf pointer
        inptr = 0;

        // signal the caller there is flen/2 samples ready
        if (pass != 0) return 0;

        return flen2;
    }
}

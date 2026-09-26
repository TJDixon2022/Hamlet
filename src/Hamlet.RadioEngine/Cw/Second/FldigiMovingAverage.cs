// ----------------------------------------------------------------------------
// Ported to C# for Hamlet from fldigi - receive path only (HM-REQ-122).
// Upstream:      https://github.com/w1hkj/fldigi
// Commit:        61b97f4133c488063f3de1795c894d22d5032e8a
// Upstream files: src/filters/filters.cxx and src/include/filters.h (Cmovavg)
// The upstream header of filters.cxx follows, verbatim.
// ----------------------------------------------------------------------------
//
// filters.cxx  --  Several Digital Filter classes used in fldigi
//
// Copyright (C) 2006-2008 Dave Freese, W1HKJ
//
// These filters are based on the gmfsk design and the design notes given in
// "Digital Signal Processing, A Practical Guid for Engineers and Scientists"
// by Steven W. Smith.
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

namespace Hamlet.RadioEngine.Cw.Second;

/// <summary>
/// fldigi's moving average filter, <c>Cmovavg</c>, as ported (HM-REQ-122).
/// </summary>
public sealed class FldigiMovingAverage
{
    private double[]? @in;
    private double @out;
    private int len, pint;
    private bool empty;

    /// <summary>Ports <c>Cmovavg::Cmovavg</c> (filters.cxx:261).</summary>
    /// <param name="filtlen">The length.</param>
    public FldigiMovingAverage(int filtlen = 64)
    {
        len = filtlen;
        @in = new double[len];
        empty = true;
    }

    /// <summary>Ports <c>Cmovavg::run</c> (filters.cxx:273).</summary>
    /// <param name="a">The new sample.</param>
    /// <returns>The average.</returns>
    public double run(double a)
    {
        if (@in is null)
        {
            return a;
        }
        if (empty)
        {
            empty = false;
            @out = 0;
            for (int i = 0; i < len; i++)
            {
                @in[i] = a;
                @out += a;
            }
            pint = 0;
            return a;
        }
        @out = @out - @in[pint] + a;
        @in[pint] = a;
        if (++pint >= len) pint = 0;
        return @out / len;
    }

    /// <summary>Ports <c>Cmovavg::setLength</c> (filters.cxx:294).</summary>
    /// <param name="filtlen">The new length.</param>
    public void setLength(int filtlen)
    {
        if (filtlen > len)
        {
            @in = new double[filtlen];
        }
        len = filtlen;
        empty = true;
    }

    /// <summary>Ports <c>Cmovavg::reset</c> (filters.cxx:304).</summary>
    public void reset()
    {
        empty = true;
    }
}

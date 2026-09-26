// ----------------------------------------------------------------------------
// Ported to C# for Hamlet from fldigi - receive path only (HM-REQ-122).
// Upstream:      https://github.com/w1hkj/fldigi
// Commit:        61b97f4133c488063f3de1795c894d22d5032e8a
// Upstream file: src/include/misc.h (decayavg, clamp)
// The upstream header follows, verbatim.
// ----------------------------------------------------------------------------
// misc.h  --  Miscellaneous helper functions
//
// Copyright (C) 2006-2008
//		Dave Freese, W1HKJ
//
// This file is part of fldigi.  These filters were adapted from code contained
// in the gmfsk source code distribution.
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
/// The two helpers from fldigi's <c>misc.h</c> that the CW receiver calls, as
/// ported (HM-REQ-122).
/// </summary>
public static class FldigiMisc
{
    /// <summary>Ports <c>clamp</c> (misc.h:53).</summary>
    /// <param name="x">The value.</param>
    /// <param name="min">The lower bound.</param>
    /// <param name="max">The upper bound.</param>
    /// <returns>The value held inside the bounds.</returns>
    public static double clamp(double x, double min, double max)
    {
        return (x < min) ? min : ((x > max) ? max : x);
    }

    /// <summary>Ports <c>decayavg</c> (misc.h:59).</summary>
    /// <param name="average">The running average.</param>
    /// <param name="input">The new sample.</param>
    /// <param name="weight">The weight; one or less returns the input.</param>
    /// <returns>The new average.</returns>
    public static double decayavg(double average, double input, int weight)
    {
        if (weight <= 1) return input;
        return ((input - average) / (double)weight) + average;
    }
}

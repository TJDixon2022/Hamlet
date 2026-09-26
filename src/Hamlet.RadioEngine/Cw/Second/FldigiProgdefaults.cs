// ----------------------------------------------------------------------------
// Ported to C# for Hamlet from fldigi - receive path only (HM-REQ-122).
// Upstream:      https://github.com/w1hkj/fldigi
// Commit:        61b97f4133c488063f3de1795c894d22d5032e8a
// Upstream file: src/include/configuration.h (the progdefaults fields the CW
// receive path reads, each with its shipped default and its line there)
// The upstream header follows, verbatim.
// ----------------------------------------------------------------------------
// configuration.h
//
// Copyright (C) 2006-2010
//      Dave Freese, W1HKJ
// Copyright (C) 2008-2010
//      Stelios Bounanos, M0GLD
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
/// The <c>progdefaults</c> fields fldigi's CW receiver reads, at their shipped
/// defaults (HM-REQ-122).
/// </summary>
/// <remarks>
/// Upstream <c>progdefaults</c> is one process-wide object; here each decoder
/// holds its own, because the receiver writes into it (<c>CWupper</c> and
/// <c>CWlower</c> every sample, <c>CWbandwidth</c> under the matched filter).
/// </remarks>
public sealed class FldigiProgdefaults
{
    /// <summary>"Transmit speed (WPM)", 18 (configuration.h:513).</summary>
    public double CWspeed = 18;

    /// <summary>"Filter bandwidth (Hz)", 150 (configuration.h:525).</summary>
    public int CWbandwidth = 150;

    /// <summary>"Detector hysterisis, lower threshold", 0.4 (configuration.h:528).</summary>
    public double CWlower = 0.4;

    /// <summary>"Detector hysterisis, upper threshold", 0.6 (configuration.h:531).</summary>
    public double CWupper = 0.6;

    /// <summary>"Automatic receive speed tracking", true (configuration.h:537).</summary>
    public bool CWtrack = true;

    /// <summary>"Matched Filter in use", false (configuration.h:540).</summary>
    public bool CWmfilt = false;

    /// <summary>
    /// "Self Organizing Map decoding", false (configuration.h:543). The SOM
    /// mode is not ported; the decoder refuses this set true.
    /// </summary>
    public bool CWuseSOMdecoding = false;

    /// <summary>"Tracking range for CWTRACK (WPM)", 10 (configuration.h:546).</summary>
    public int CWrange = 10;

    /// <summary>"Lower RX limit (WPM)", 5 (configuration.h:549).</summary>
    public int CWlowerlimit = 5;

    /// <summary>"Upper TX limit (WPM)", 50 (configuration.h:552).</summary>
    public int CWupperlimit = 50;

    /// <summary>"rx squelch attack timing", 1 = MEDIUM (configuration.h:555).</summary>
    public int cwrx_attack = 1;

    /// <summary>"rx squelch decay timing", 1 = MEDIUM (configuration.h:559).</summary>
    public int cwrx_decay = 1;

    /// <summary>"use CW_noise character to display invalid CW decodes", '*' (configuration.h:249).</summary>
    public int CW_noise = '*';

    /// <summary>"Use open paren character; typically used in MARS ops", false (configuration.h:617).</summary>
    public bool CW_use_paren = false;

    /// <summary>"CW prosigns BT AA AS AR SK KN INT HM VE", "=~&lt;&gt;%+&amp;{}" (configuration.h:620).</summary>
    public string CW_prosigns = "=~<>%+&{}";

    /// <summary>"Display decoded prosign as assigned short cut key", false (configuration.h:623).</summary>
    public bool CW_prosign_display = false;

    /// <summary>configuration.h:629, true.</summary>
    public bool A_umlaut = true;

    /// <summary>configuration.h:632, false.</summary>
    public bool A_aelig = false;

    /// <summary>configuration.h:635, true.</summary>
    public bool A_ring = true;

    /// <summary>configuration.h:638, true.</summary>
    public bool C_cedilla = true;

    /// <summary>configuration.h:641, true.</summary>
    public bool E_grave = true;

    /// <summary>configuration.h:644, true.</summary>
    public bool E_acute = true;

    /// <summary>configuration.h:647, false.</summary>
    public bool O_acute = false;

    /// <summary>configuration.h:650, true.</summary>
    public bool O_umlaut = true;

    /// <summary>configuration.h:653, false.</summary>
    public bool O_slash = false;

    /// <summary>configuration.h:656, true.</summary>
    public bool N_tilde = true;

    /// <summary>configuration.h:659, true.</summary>
    public bool U_umlaut = true;

    /// <summary>configuration.h:662, false.</summary>
    public bool U_circ = false;

    /// <summary>configuration.h:219, true.</summary>
    public bool CW_backslash = true;

    /// <summary>configuration.h:222, true.</summary>
    public bool CW_single_quote = true;

    /// <summary>configuration.h:225, true.</summary>
    public bool CW_dollar_sign = true;

    /// <summary>configuration.h:228, true.</summary>
    public bool CW_open_paren = true;

    /// <summary>configuration.h:231, true.</summary>
    public bool CW_close_paren = true;

    /// <summary>configuration.h:234, true.</summary>
    public bool CW_colon = true;

    /// <summary>configuration.h:237, true.</summary>
    public bool CW_semi_colon = true;

    /// <summary>configuration.h:240, true.</summary>
    public bool CW_underscore = true;

    /// <summary>configuration.h:243, true.</summary>
    public bool CW_at_symbol = true;

    /// <summary>configuration.h:246, true.</summary>
    public bool CW_exclamation = true;
}

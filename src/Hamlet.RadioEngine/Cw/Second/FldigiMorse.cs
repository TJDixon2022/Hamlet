// ----------------------------------------------------------------------------
// Ported to C# for Hamlet from fldigi - receive path only (HM-REQ-122).
// Upstream:      https://github.com/w1hkj/fldigi
// Commit:        61b97f4133c488063f3de1795c894d22d5032e8a
// Upstream files: src/cw_rtty/morse.cxx and src/include/morse.h
// The receive half only: tx_lookup, tx_length and tx_print are transmit and
// are left out. The upstream header of morse.cxx follows, verbatim; it names
// no author.
// ----------------------------------------------------------------------------
/*
 *    morse.c  --  morse code tables
 *
 *    Copyright (C) 2017
 *
 *    Fldigi is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    Fldigi is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with fldigi.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

namespace Hamlet.RadioEngine.Cw.Second;

/// <summary>
/// fldigi's Morse table, <c>cMorse</c>, receive half, as ported (HM-REQ-122).
/// </summary>
public sealed class FldigiMorse
{
    /// <summary>morse.h:28.</summary>
    public const char CW_DOT_REPRESENTATION = '.';

    /// <summary>morse.h:29.</summary>
    public const char CW_DASH_REPRESENTATION = '-';

    // Ports struct CWstruct (morse.h:31).
    private sealed class CWstruct
    {
        public bool enabled;    // true if character is active
        public string chr;      // utf-8 string representation of character
        public string prt;      // utf-8 printable representation
        public string rpr;      // Dot-dash code representation

        public CWstruct(int enabled, string chr, string prt, string rpr)
        {
            this.enabled = enabled != 0;
            this.chr = chr;
            this.prt = prt;
            this.rpr = rpr;
        }
    }

    private readonly FldigiProgdefaults progdefaults;

    // Upstream this is a global set by the configuration dialog (morse.cxx:42).
    private readonly bool CW_table_changed = false;

    // Ports cMorse::cw_table (morse.cxx:44). Upstream it is static; here each
    // table belongs to one decoder, because init() writes into it.
    private readonly CWstruct[] cw_table =
    {
        // Prosigns
        new(1, "=", "<BT>", "-...-"), // 0
        new(0, "~", "<AA>", ".-.-"), // 1
        new(1, "<", "<AS>", ".-..."), // 2
        new(1, ">", "<AR>", ".-.-."), // 3
        new(1, "%", "<SK>", "...-.-"), // 4
        new(1, "+", "<KN>", "-.--."), // 5
        new(1, "&", "<INT>", "..-.-"), // 6
        new(1, "{", "<HM>", "....--"), // 7
        new(1, "}", "<VE>", "...-."), // 8
        // ASCII 7bit letters
        new(1, "A", "A", ".-"),
        new(1, "B", "B", "-..."),
        new(1, "C", "C", "-.-."),
        new(1, "D", "D", "-.."),
        new(1, "E", "E", "."),
        new(1, "F", "F", "..-."),
        new(1, "G", "G", "--."),
        new(1, "H", "H", "...."),
        new(1, "I", "I", ".."),
        new(1, "J", "J", ".---"),
        new(1, "K", "K", "-.-"),
        new(1, "L", "L", ".-.."),
        new(1, "M", "M", "--"),
        new(1, "N", "N", "-."),
        new(1, "O", "O", "---"),
        new(1, "P", "P", ".--."),
        new(1, "Q", "Q", "--.-"),
        new(1, "R", "R", ".-."),
        new(1, "S", "S", "..."),
        new(1, "T", "T", "-"),
        new(1, "U", "U", "..-"),
        new(1, "V", "V", "...-"),
        new(1, "W", "W", ".--"),
        new(1, "X", "X", "-..-"),
        new(1, "Y", "Y", "-.--"),
        new(1, "Z", "Z", "--.."),
        //
        new(1, "a", "A", ".-"),
        new(1, "b", "B", "-..."),
        new(1, "c", "C", "-.-."),
        new(1, "d", "D", "-.."),
        new(1, "e", "E", "."),
        new(1, "f", "F", "..-."),
        new(1, "g", "G", "--."),
        new(1, "h", "H", "...."),
        new(1, "i", "I", ".."),
        new(1, "j", "J", ".---"),
        new(1, "k", "K", "-.-"),
        new(1, "l", "L", ".-.."),
        new(1, "m", "M", "--"),
        new(1, "n", "N", "-."),
        new(1, "o", "O", "---"),
        new(1, "p", "P", ".--."),
        new(1, "q", "Q", "--.-"),
        new(1, "r", "R", ".-."),
        new(1, "s", "S", "..."),
        new(1, "t", "T", "-"),
        new(1, "u", "U", "..-"),
        new(1, "v", "V", "...-"),
        new(1, "w", "W", ".--"),
        new(1, "x", "X", "-..-"),
        new(1, "y", "Y", "-.--"),
        new(1, "z", "Z", "--.."),
        // Numerals
        new(1, "0", "0", "-----"),
        new(1, "1", "1", ".----"),
        new(1, "2", "2", "..---"),
        new(1, "3", "3", "...--"),
        new(1, "4", "4", "....-"),
        new(1, "5", "5", "....."),
        new(1, "6", "6", "-...."),
        new(1, "7", "7", "--..."),
        new(1, "8", "8", "---.."),
        new(1, "9", "9", "----."),
        // Punctuation
        new(1, "\\", "\\", ".-..-."),
        new(1, "\'", "'", ".----."),
        new(1, "$", "$", "...-..-"),
        new(1, "(", "(", "-.--."),
        new(1, ")", ")", "-.--.-"),
        new(1, ",", ",", "--..--"),
        new(1, "-", "-", "-....-"),
        new(1, ".", ".", ".-.-.-"),
        new(1, "/", "/", "-..-."),
        new(1, ":", ":", "---..."),
        new(1, ";", ";", "-.-.-."),
        new(1, "?", "?", "..--.."),
        new(1, "_", "_", "..--.-"),
        new(1, "@", "@", ".--.-."),
        new(1, "!", "!", "-.-.--"),
        // accented characters
        new(1, "Ä", "Ä", ".-.-"), // A umlaut
        new(1, "ä", "Ä", ".-.-"), // A umlaut
        new(0, "Æ", "Æ", ".-.-"), // A aelig
        new(0, "æ", "Æ", ".-.-"), // A aelig
        new(0, "Å", "Å", ".--.-"), // A ring
        new(0, "å", "Å", ".--.-"), // A ring
        new(1, "Ç", "Ç", "-.-.."), // C cedilla
        new(1, "ç", "Ç", "-.-.."), // C cedilla
        new(0, "È", "È", ".-..-"), // E grave
        new(0, "è", "È", ".-..-"), // E grave
        new(1, "É", "É", "..-.."), // E acute
        new(1, "é", "É", "..-.."), // E acute
        new(0, "Ó", "Ó", "---."), // O acute
        new(0, "ó", "Ó", "---."), // O acute
        new(1, "Ö", "Ö", "---."), // O umlaut
        new(1, "ö", "Ö", "---."), // O umlaut
        new(0, "Ø", "Ø", "---."), // O slash
        new(0, "ø", "Ø", "---."), // O slash
        new(1, "Ñ", "Ñ", "--.--"), // N tilde
        new(1, "ñ", "Ñ", "--.--"), // N tilde
        new(1, "Ü", "Ü", "..--"), // U umlaut
        new(1, "ü", "Ü", "..--"), // U umlaut
        new(0, "Û", "Û", "..--"), // U circ
        new(0, "û", "Û", "..--"), // U circ
        // array termination
        new(0, string.Empty, string.Empty, string.Empty),
    };

    /// <summary>Ports <c>cMorse::cMorse</c> (morse.h:45).</summary>
    /// <param name="progdefaults">The settings the table reads.</param>
    public FldigiMorse(FldigiProgdefaults progdefaults)
    {
        this.progdefaults = progdefaults;
        init();
    }

    /// <summary>Ports <c>cMorse::enable</c> (morse.cxx:167).</summary>
    /// <param name="s">The character or its printable form.</param>
    /// <param name="val">Whether it is active.</param>
    public void enable(string s, bool val)
    {
        for (int i = 0; cw_table[i].rpr.Length != 0; i++)
        {
            if (cw_table[i].chr == s || cw_table[i].prt == s)
            {
                cw_table[i].enabled = val;
                return;
            }
        }
    }

    /// <summary>
    /// Ports <c>cMorse::init</c> (morse.cxx:177). The transmit buffers it also
    /// clears (utf8, ptr, toprint) are not ported.
    /// </summary>
    public void init()
    {
        // Update the char / prosign relationship
        if (progdefaults.CW_prosigns.Length == 9)
        {
            for (int i = 0; i < 9; i++)
            {
                cw_table[i].chr = progdefaults.CW_prosigns[i].ToString();
            }
        }
        enable("<AA>", true);
        enable("Ä", false); enable("ä", false);
        enable("Æ", false); enable("æ", false);
        enable("Å", false); enable("å", false);
        enable("Ç", false); enable("ç", false);
        enable("È", false); enable("è", false);
        enable("É", false); enable("é", false);
        enable("Ó", false); enable("ó", false);
        enable("Ö", false); enable("ö", false);
        enable("Ø", false); enable("ø", false);
        enable("Ñ", false); enable("ñ", false);
        enable("Ü", false); enable("ü", false);
        enable("Û", false); enable("û", false);

        if (progdefaults.A_umlaut)
        { enable("Ä", true); enable("ä", true); enable("<AA>", false); }
        if (progdefaults.A_aelig)
        { enable("Æ", true); enable("æ", true); enable("<AA>", false); }
        if (progdefaults.A_ring)
        { enable("Å", true); enable("å", true); }
        if (progdefaults.C_cedilla)
        { enable("Ç", true); enable("ç", true); }
        if (progdefaults.E_grave)
        { enable("È", true); enable("è", true); }
        if (progdefaults.E_acute)
        { enable("É", true); enable("é", true); }
        if (progdefaults.O_acute)
        { enable("Ó", true); enable("ó", true); }
        if (progdefaults.O_umlaut)
        { enable("Ö", true); enable("ö", true); }
        if (progdefaults.O_slash)
        { enable("Ø", true); enable("ø", true); }
        if (progdefaults.N_tilde)
        { enable("Ñ", true); enable("ñ", true); }
        if (progdefaults.U_umlaut)
        { enable("Ü", true); enable("ü", true); }
        if (progdefaults.U_circ)
        { enable("Û", true); enable("û", true); }

        enable("\\", progdefaults.CW_backslash);
        enable("\'", progdefaults.CW_single_quote);
        enable("$", progdefaults.CW_dollar_sign);
        enable("(", progdefaults.CW_open_paren);
        enable(")", progdefaults.CW_close_paren);
        enable(":", progdefaults.CW_colon);
        enable(";", progdefaults.CW_semi_colon);
        enable("_", progdefaults.CW_underscore);
        enable("@", progdefaults.CW_at_symbol);
        enable("!", progdefaults.CW_exclamation);
    }

    /// <summary>Ports <c>cMorse::rx_lookup</c> (morse.cxx:242).</summary>
    /// <param name="rx">A run of dots and dashes.</param>
    /// <returns>What fldigi prints for it, or the empty string when nothing active matches.</returns>
    public string rx_lookup(string rx)
    {
        if (CW_table_changed) init();
        for (int i = 0; cw_table[i].rpr.Length != 0; i++)
        {
            if (rx == cw_table[i].rpr)
            {
                if (cw_table[i].enabled)
                {
                    if (progdefaults.CW_prosign_display)
                        return cw_table[i].chr;
                    return cw_table[i].prt;
                }
            }
        }
        return string.Empty;
    }
}

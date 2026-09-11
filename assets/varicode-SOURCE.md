# assets/varicode.csv - source

256 rows, `code,bits,name`. Extracted mechanically on 2026-09-11 from
`src/psk/pskvaricode.cxx` in the `w1hkj/fldigi` repository (master branch on GitHub;
file header: "varicode.cxx -- PSK31 Varicode, Copyright (C) 2006 Dave Freese W1HKJ,
adapted from gmfsk"), licence GPL-3 as stated in that header. The table is the
published PSK31 varicode by Peter Martinez, G3PLX (1998); fldigi is the citation for
the exact bit strings.

Checked at extraction: every code starts and ends with `1`, no code contains `00`,
all 256 are unique. So `00` is the character separator and the code is self-
delimiting. Spot checks against the published table: space = `1`, `e` = `11`,
`t` = `101`, `o` = `111`, `a` = `1011`.

The table may be transcribed into Hamlet's tree with this file as its citation. That
is data, not a port.

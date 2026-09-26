#!/bin/sh
# unit 452 - the per-condition rows of one trace printout, labels shortened.
# Usage: sh .run-unit/unit452-conds.sh <trace.txt>
cd /c/Source/HamLet || exit 1
grep -a "^ *condition | \(real\|synthetic\) | " "$1" | sed -E "s/ \(CW_SPEC[^)]*\)//; s/ \(not shown to be CH-AWGN\)//; s/ \(not restated in the 2500 Hz reference\)//; s/no fading, shaped noise band, //; s/real HF, no CH-\* profile, SNR_2500 not measured, //" | cut -c1-200

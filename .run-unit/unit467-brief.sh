#!/bin/sh
# unit 467 - part (b)'s printed rows, shortened: row, class, rule and table, emitted worse on.
# Usage: sh .run-unit/unit467-brief.sh <b-file-suffix e.g. b-harness-green>
cd /c/Source/HamLet || exit 1
grep -a "^ HM-REQ-128 (b)" .run-unit/unit467-$1.txt | cut -d"|" -f3,4,5,8,9 \
  | sed -e 's/real HF, no CH-\* profile, SNR_2500 not measured, //' -e 's/synthetic, no fading, shaped noise band (not shown to be CH-AWGN), /syn /' \
        -e 's/ in the passband (not restated in the 2500 Hz reference)//' -e 's/ (CW_SPEC[^|]*//' -e 's/character gap 5 units, inside TX-FARNS.s 3 to 7/char-gap-5/' -e 's/ (1:3:1:3:7)//'
grep -a -E "^\s+(Passed|Failed) " .run-unit/unit467-$1.txt | sed -E "s/^\s+//" | cut -c1-160

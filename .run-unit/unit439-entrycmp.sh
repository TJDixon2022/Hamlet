#!/bin/sh
# unit 439 - entry against unit 438's exit: every round output compared with timings stripped, then the headline figures.
cd /c/Source/HamLet/.run-unit || exit 1
for t in captures adjudicated named keyed baseline strays plateau opening
do
  sh unit439-same.sh unit438-$t-exit.txt unit439-$t-entry.txt 6
done
echo "=== keyed totals"
grep -hE "check \| read here|^ count \| added [0-9]+, single" unit439-strays-entry.txt | cut -c1-120
grep -hE "^ row \| unadjudicated/cw-2026-09-23-173723" unit439-baseline-entry.txt | cut -c1-140
echo "=== opening at entry"
grep -E "^ (opening \| text|cold \| cw-2026-09-24-0039(01|19) \| bench \|)" unit439-opening-entry.txt | head -4
echo "=== plateau at entry"
grep -E "^ (1|3|5|6|8) dB: |measured " unit439-plateau-entry.txt | sort -u

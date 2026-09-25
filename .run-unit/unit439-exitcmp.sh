#!/bin/sh
# unit 439 - exit against entry: every round output compared with timings stripped, then the headline figures.
cd /c/Source/HamLet/.run-unit || exit 1
for t in captures adjudicated named keyed baseline strays plateau
do
  sh unit439-same.sh unit439-$t-entry.txt unit439-$t-exit.txt 6
done
echo "=== keyed totals"
grep -hE "check \| read here|^ count \| added [0-9]+, single" unit439-strays-exit.txt | cut -c1-120
grep -hE "^ row \| unadjudicated/cw-2026-09-23-173723" unit439-baseline-exit.txt | cut -c1-140
echo "=== opening at exit"
grep -E "^ (opening \| text|cold \| cw-2026-09-24-0039(01|19) \| bench \|)" unit439-opening-exit.txt | head -4
grep -E "remix text \| (opening|cold) .*\| entry \|" unit439-opening-exit.txt
echo "=== plateau at exit"
grep -E "^ (1|3|5|6|8) dB: |measured " unit439-plateau-exit.txt | sort -u
echo "=== trace stop at exit"
grep -E "^ cut (stop|check \| the entry)" unit439-opening-exit.txt | cut -c1-260

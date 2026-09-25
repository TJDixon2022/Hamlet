#!/bin/sh
# unit 437 - every exit output beside its entry, timings stripped, and the opening's text at exit.
cd /c/Source/HamLet/.run-unit || exit 1
for t in captures adjudicated named keyed strays plateau
do
  sh unit437-same.sh unit437-$t-entry.txt unit437-$t-exit.txt 6
done
grep -hE "^ (cold \| cw-2026-09-24-0039(01|19) \| bench \||stream \| cw-2026-09-24-0039)" unit437-opening-exit.txt
grep -hE "check \| read here|count \| added [0-9]+, single" unit437-strays-exit.txt | cut -c1-90
grep -hE "over 25 characters" unit437-named-exit.txt | cut -c60-200

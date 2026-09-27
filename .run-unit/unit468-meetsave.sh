#!/bin/sh
# unit 465 - keep the meeting fact's printout as .run-unit/unit468-meet<suffix>.txt, the test runner's indent taken off.
# Usage: sh .run-unit/unit468-meetsave.sh <run-suffix> <out-suffix>
cd /c/Source/HamLet || exit 1
grep -a -E "^ *(fixed|clock|seam|pair|unspanned|clock check|count|p on disagreements) \|" .run-unit/unit468-$1.txt | sed 's/^ *//' > .run-unit/unit468-meet$2.txt
wc -l .run-unit/unit468-meet$2.txt
grep -c "^pair | " .run-unit/unit468-meet$2.txt

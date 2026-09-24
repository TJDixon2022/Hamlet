#!/bin/sh
# unit 415 - sum the reads with a centroid over the 52 rows of a relabel trace run.
# Usage: sh .run-unit/unit415-sum.sh <suffix>
cd /c/Source/HamLet || exit 1
grep -a -E "^ ?row \| (cw|unadj)" .run-unit/unit415-relabel-$1.txt | cut -d'|' -f5| awk '{ s += $1 } END { print s }'

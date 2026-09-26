#!/bin/sh
# unit 450 - sort one text printout and diff it against another sorted file.
# Usage: sh .run-unit/unit450-textsort.sh <suffix> <compare-to-sorted-file>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
grep -aE "^\s*(text|callsigns) \| " .run-unit/unit450-text-$1.txt | sort > .run-unit/unit450-text-$1.sorted.txt
wc -l .run-unit/unit450-text-$1.sorted.txt
diff "$2" .run-unit/unit450-text-$1.sorted.txt && echo "IDENTICAL"

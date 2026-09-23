#!/bin/sh
# unit 410 - compare the printed per-case lines of two runs of the same type.
# Usage: sh .run-unit/unit410-same.sh <pattern> <entry-outname> <exit-outname>
cd /c/Source/HamLet || exit 1
grep -E "$1" ".run-unit/unit410-$2.txt" | sed -E "s/^\s+//" | sort -u > ".run-unit/unit410-$2-same.txt"
grep -E "$1" ".run-unit/unit410-$3.txt" | sed -E "s/^\s+//" | sort -u > ".run-unit/unit410-$3-same.txt"
echo "lines $(wc -l < .run-unit/unit410-$2-same.txt) and $(wc -l < .run-unit/unit410-$3-same.txt)"
diff ".run-unit/unit410-$2-same.txt" ".run-unit/unit410-$3-same.txt" && echo "IDENTICAL"

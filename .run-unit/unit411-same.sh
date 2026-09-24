#!/bin/sh
# unit 411 - compare the printed per-case lines of two runs of the same type.
# Usage: sh .run-unit/unit411-same.sh <pattern> <entry-outname> <exit-outname>
cd /c/Source/HamLet || exit 1
grep -E "$1" ".run-unit/unit411-$2.txt" | sed -E "s/^\s+//" | sort -u > ".run-unit/unit411-$2-same.txt"
grep -E "$1" ".run-unit/unit411-$3.txt" | sed -E "s/^\s+//" | sort -u > ".run-unit/unit411-$3-same.txt"
echo "lines $(wc -l < .run-unit/unit411-$2-same.txt) and $(wc -l < .run-unit/unit411-$3-same.txt)"
diff ".run-unit/unit411-$2-same.txt" ".run-unit/unit411-$3-same.txt" && echo "IDENTICAL"

#!/bin/sh
# unit 409 - compare the printed per-case lines of two runs of the same type.
# Usage: sh .run-unit/unit409-same.sh <pattern> <entry-outname> <exit-outname>
cd /c/Source/HamLet || exit 1
grep -E "$1" ".run-unit/unit409-$2.txt" | sed -E "s/^\s+//" | sort -u > ".run-unit/unit409-$2-same.txt"
grep -E "$1" ".run-unit/unit409-$3.txt" | sed -E "s/^\s+//" | sort -u > ".run-unit/unit409-$3-same.txt"
echo "lines $(wc -l < .run-unit/unit409-$2-same.txt) and $(wc -l < .run-unit/unit409-$3-same.txt)"
diff ".run-unit/unit409-$2-same.txt" ".run-unit/unit409-$3-same.txt" && echo "IDENTICAL"

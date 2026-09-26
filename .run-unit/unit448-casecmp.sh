#!/bin/sh
# unit 448 - 447's instrument cases through the tracker, before and after; changed rows only.
# Usage: sh .run-unit/unit448-casecmp.sh <before-out> <after-out>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
grep -a "^ *tracker case | " .run-unit/unit448-$1.txt | grep -v "| case |" | sed -E "s/^ +//" | sort -u > .run-unit/unit448-cc-a.tmp
grep -a "^ *tracker case | " .run-unit/unit448-$2.txt | grep -v "| case |" | sed -E "s/^ +//" | sort -u > .run-unit/unit448-cc-b.tmp
echo "cases before $(wc -l < .run-unit/unit448-cc-a.tmp), after $(wc -l < .run-unit/unit448-cc-b.tmp)"
diff .run-unit/unit448-cc-a.tmp .run-unit/unit448-cc-b.tmp && echo "IDENTICAL"

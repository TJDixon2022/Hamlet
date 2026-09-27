#!/bin/sh
# unit 471 - which recordings' texts or classes moved between two saves, and each moved line.
# Usage: sh .run-unit/unit471-textdiff.sh <before-suffix> <after-suffix>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
diff .run-unit/unit471-text-$1.txt .run-unit/unit471-text-$2.txt > .run-unit/unit471-textdiff-$1-$2.txt
echo "== recordings with a moved line"
grep -a -E "^[<>] save" .run-unit/unit471-textdiff-$1-$2.txt | cut -d'|' -f3 | sort | uniq -c
echo "== moved lines (recording | index | text | class | p)"
grep -a -E "^[<>] save" .run-unit/unit471-textdiff-$1-$2.txt | cut -d'|' -f1,3-6 | cut -c1-140

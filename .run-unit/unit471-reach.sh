#!/bin/sh
# unit 471 - the letters a trace run found unsettled at the first proof after them, and the 449 and 470 placements.
# Usage: sh .run-unit/unit471-reach.sh <suffix>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
F=.run-unit/unit471-trace-$1.txt
echo "== unsettled at proof (column 'unsettled then' = yes)"
grep -a "^ *letter | " "$F" | awk -F'|' '$13 ~ /yes/' | cut -d'|' -f2-9,11-18 | cut -c1-330
echo "== 449"
grep -a "^ *449 | " "$F" | cut -c1-200
echo "== 470"
grep -a "^ *470 | " "$F" | cut -c1-200
echo "== letter lines"
grep -a -c "^ *letter | " "$F"

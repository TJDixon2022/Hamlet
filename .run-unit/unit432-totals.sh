#!/bin/sh
# unit 432 - the keyed totals and the added letters at a suffix, and the round against unit 430's exit.
# Usage: sh .run-unit/unit432-totals.sh <suffix>
cd /c/Source/HamLet/.run-unit || exit 1
S=$1
echo "== keyed and baseline totals"
grep -hE "total bench|total live|^\s*total \||^\s*outside \||all keyed" unit432-keyed-$S.txt unit432-baseline-$S.txt
echo "== added"
sh unit432-added.sh $S
F="Total time|RC=|WALL|elapsed|\[[0-9.]+ m?s\]|Duration"
grep -hvE "$F" unit431-keyed-exit.txt unit431-baseline-exit.txt > unit432-cmp-a.tmp
grep -hvE "$F" unit432-keyed-$S.txt unit432-baseline-$S.txt > unit432-cmp-b.tmp
echo "== keyed and baseline against unit 431 exit"
diff unit432-cmp-a.tmp unit432-cmp-b.tmp | head -20
grep -hvE "$F" unit431-adjudicated-exit.txt > unit432-cmp-a.tmp
grep -hvE "$F" unit432-adjudicated-$S.txt > unit432-cmp-b.tmp
echo "== adjudicated against unit 431 exit"
diff unit432-cmp-a.tmp unit432-cmp-b.tmp | head -20
grep -hvE "$F" unit431-strays-exit.txt > unit432-cmp-a.tmp
grep -hvE "$F" unit432-strays-$S.txt > unit432-cmp-b.tmp
echo "== strays against unit 431 exit"
diff unit432-cmp-a.tmp unit432-cmp-b.tmp | head -20
grep -hvE "$F" unit431-captures-exit.txt > unit432-cmp-a.tmp
grep -hvE "$F" unit432-captures-$S.txt > unit432-cmp-b.tmp
echo "== captures against unit 431 exit"
diff unit432-cmp-a.tmp unit432-cmp-b.tmp | head -20
echo "== end"

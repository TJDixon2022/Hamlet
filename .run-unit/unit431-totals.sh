#!/bin/sh
# unit 431 - the keyed totals and the added letters at a suffix, and the round against unit 430's exit.
# Usage: sh .run-unit/unit431-totals.sh <suffix>
cd /c/Source/HamLet/.run-unit || exit 1
S=$1
echo "== keyed and baseline totals"
grep -hE "total bench|total live|^\s*total \||^\s*outside \||all keyed" unit431-keyed-$S.txt unit431-baseline-$S.txt
echo "== added"
sh unit431-added.sh $S
F="Total time|RC=|WALL|elapsed|\[[0-9.]+ m?s\]|Duration"
grep -hvE "$F" unit430-keyed-exit.txt unit430-baseline-exit.txt > unit431-cmp-a.tmp
grep -hvE "$F" unit431-keyed-$S.txt unit431-baseline-$S.txt > unit431-cmp-b.tmp
echo "== keyed and baseline against unit 430 exit"
diff unit431-cmp-a.tmp unit431-cmp-b.tmp | head -20
grep -hvE "$F" unit430-adjudicated-exit.txt > unit431-cmp-a.tmp
grep -hvE "$F" unit431-adjudicated-$S.txt > unit431-cmp-b.tmp
echo "== adjudicated against unit 430 exit"
diff unit431-cmp-a.tmp unit431-cmp-b.tmp | head -20
grep -hvE "$F" unit430-strays-exit.txt > unit431-cmp-a.tmp
grep -hvE "$F" unit431-strays-$S.txt > unit431-cmp-b.tmp
echo "== strays against unit 430 exit"
diff unit431-cmp-a.tmp unit431-cmp-b.tmp | head -20
grep -hvE "$F" unit430-captures-exit.txt > unit431-cmp-a.tmp
grep -hvE "$F" unit431-captures-$S.txt > unit431-cmp-b.tmp
echo "== captures against unit 430 exit"
diff unit431-cmp-a.tmp unit431-cmp-b.tmp | head -20
echo "== end"

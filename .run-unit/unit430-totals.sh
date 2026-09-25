#!/bin/sh
# unit 430 - the keyed totals and the added letters at a suffix, and the round against unit 429's exit.
# Usage: sh .run-unit/unit430-totals.sh <suffix>
cd /c/Source/HamLet/.run-unit || exit 1
S=$1
echo "== keyed and baseline totals"
grep -hE "total bench|total live|^\s*total \||^\s*outside \||all keyed" unit430-keyed-$S.txt unit430-baseline-$S.txt
echo "== added"
sh unit430-added.sh $S
F="Total time|RC=|WALL|elapsed|\[[0-9.]+ m?s\]|Duration"
grep -hvE "$F" unit429-keyed-exit.txt unit429-baseline-exit.txt > unit430-cmp-a.tmp
grep -hvE "$F" unit430-keyed-$S.txt unit430-baseline-$S.txt > unit430-cmp-b.tmp
echo "== keyed and baseline against unit 429 exit"
diff unit430-cmp-a.tmp unit430-cmp-b.tmp | head -20
grep -hvE "$F" unit429-adjudicated-exit.txt > unit430-cmp-a.tmp
grep -hvE "$F" unit430-adjudicated-$S.txt > unit430-cmp-b.tmp
echo "== adjudicated against unit 429 exit"
diff unit430-cmp-a.tmp unit430-cmp-b.tmp | head -20
grep -hvE "$F" unit429-strays-entry.txt > unit430-cmp-a.tmp
grep -hvE "$F" unit430-strays-$S.txt > unit430-cmp-b.tmp
echo "== strays against unit 429 entry"
diff unit430-cmp-a.tmp unit430-cmp-b.tmp | head -20
grep -hvE "$F" unit429-captures-exit.txt > unit430-cmp-a.tmp
grep -hvE "$F" unit430-captures-$S.txt > unit430-cmp-b.tmp
echo "== captures against unit 429 exit"
diff unit430-cmp-a.tmp unit430-cmp-b.tmp | head -20
echo "== end"

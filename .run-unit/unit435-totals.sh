#!/bin/sh
# unit 435 - the keyed totals and the added letters at a suffix, and the round against unit 430's exit.
# Usage: sh .run-unit/unit435-totals.sh <suffix>
cd /c/Source/HamLet/.run-unit || exit 1
S=$1
echo "== keyed and baseline totals"
grep -hE "total bench|total live|^\s*total \||^\s*outside \||all keyed" unit435-keyed-$S.txt unit435-baseline-$S.txt
echo "== added"
sh unit435-added.sh $S
F="Total time|RC=|WALL|elapsed|\[[0-9.]+ m?s\]|Duration"
grep -hvE "$F" unit433-keyed-exit.txt unit433-baseline-exit.txt > unit435-cmp-a.tmp
grep -hvE "$F" unit435-keyed-$S.txt unit435-baseline-$S.txt > unit435-cmp-b.tmp
echo "== keyed and baseline against unit 433 exit"
diff unit435-cmp-a.tmp unit435-cmp-b.tmp | head -20
grep -hvE "$F" unit433-adjudicated-exit.txt > unit435-cmp-a.tmp
grep -hvE "$F" unit435-adjudicated-$S.txt > unit435-cmp-b.tmp
echo "== adjudicated against unit 433 exit"
diff unit435-cmp-a.tmp unit435-cmp-b.tmp | head -20
grep -hvE "$F" unit433-strays-exit.txt > unit435-cmp-a.tmp
grep -hvE "$F" unit435-strays-$S.txt > unit435-cmp-b.tmp
echo "== strays against unit 433 exit"
diff unit435-cmp-a.tmp unit435-cmp-b.tmp | head -20
grep -hvE "$F" unit433-captures-exit.txt > unit435-cmp-a.tmp
grep -hvE "$F" unit435-captures-$S.txt > unit435-cmp-b.tmp
echo "== captures against unit 433 exit"
diff unit435-cmp-a.tmp unit435-cmp-b.tmp | head -20
echo "== end"

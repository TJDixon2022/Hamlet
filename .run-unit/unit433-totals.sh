#!/bin/sh
# unit 433 - the keyed totals and the added letters at a suffix, and the round against unit 430's exit.
# Usage: sh .run-unit/unit433-totals.sh <suffix>
cd /c/Source/HamLet/.run-unit || exit 1
S=$1
echo "== keyed and baseline totals"
grep -hE "total bench|total live|^\s*total \||^\s*outside \||all keyed" unit433-keyed-$S.txt unit433-baseline-$S.txt
echo "== added"
sh unit433-added.sh $S
F="Total time|RC=|WALL|elapsed|\[[0-9.]+ m?s\]|Duration"
grep -hvE "$F" unit432-keyed-exit.txt unit432-baseline-exit.txt > unit433-cmp-a.tmp
grep -hvE "$F" unit433-keyed-$S.txt unit433-baseline-$S.txt > unit433-cmp-b.tmp
echo "== keyed and baseline against unit 432 exit"
diff unit433-cmp-a.tmp unit433-cmp-b.tmp | head -20
grep -hvE "$F" unit432-adjudicated-exit.txt > unit433-cmp-a.tmp
grep -hvE "$F" unit433-adjudicated-$S.txt > unit433-cmp-b.tmp
echo "== adjudicated against unit 432 exit"
diff unit433-cmp-a.tmp unit433-cmp-b.tmp | head -20
grep -hvE "$F" unit432-strays-exit.txt > unit433-cmp-a.tmp
grep -hvE "$F" unit433-strays-$S.txt > unit433-cmp-b.tmp
echo "== strays against unit 432 exit"
diff unit433-cmp-a.tmp unit433-cmp-b.tmp | head -20
grep -hvE "$F" unit432-captures-exit.txt > unit433-cmp-a.tmp
grep -hvE "$F" unit433-captures-$S.txt > unit433-cmp-b.tmp
echo "== captures against unit 432 exit"
diff unit433-cmp-a.tmp unit433-cmp-b.tmp | head -20
echo "== end"

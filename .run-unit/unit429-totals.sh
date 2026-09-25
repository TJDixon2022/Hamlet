#!/bin/sh
# unit 429 - the keyed totals and the added letters from a round, and the round against unit 428's exit.
# Usage: sh .run-unit/unit429-totals.sh <entry|exit>
cd /c/Source/HamLet/.run-unit || exit 1
S=$1
echo "== keyed and baseline totals"
grep -hE "total bench|total live|^\s*total \||^\s*outside \||all keyed" unit429-keyed-$S.txt unit429-baseline-$S.txt
echo "== added"
grep -hiE "added" unit429-strays-$S.txt | head -8
F="Total time|RC=|WALL|elapsed|\[[0-9.]+ m?s\]|Duration"
grep -hvE "$F" unit428-keyed-exit.txt unit428-baseline-exit.txt > unit429-cmp-a.tmp
grep -hvE "$F" unit429-keyed-$S.txt unit429-baseline-$S.txt > unit429-cmp-b.tmp
echo "== keyed and baseline against unit 428 exit"
diff unit429-cmp-a.tmp unit429-cmp-b.tmp | head -20
grep -hvE "$F" unit428-adjudicated-exit.txt > unit429-cmp-a.tmp
grep -hvE "$F" unit429-adjudicated-$S.txt > unit429-cmp-b.tmp
echo "== adjudicated against unit 428 exit"
diff unit429-cmp-a.tmp unit429-cmp-b.tmp | head -20
grep -hvE "$F" unit428-strays-entry.txt > unit429-cmp-a.tmp
grep -hvE "$F" unit429-strays-$S.txt > unit429-cmp-b.tmp
echo "== strays against unit 428 entry"
diff unit429-cmp-a.tmp unit429-cmp-b.tmp | head -20
echo "== end"

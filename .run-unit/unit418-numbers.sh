#!/bin/sh
# unit 416 - the 3.2 numbers from one round's baseline, keyed and named outputs.
# Usage: sh .run-unit/unit418-numbers.sh <suffix>
cd /c/Source/HamLet || exit 1
B=.run-unit/unit418-baseline-$1.txt
K=.run-unit/unit418-keyed-$1.txt
N=.run-unit/unit418-named-$1.txt
grep -a -E "^ (total|outside) \|" $B
grep -a -E "^ total bench \|" $K
BE=$(grep -a -E "^ total \|" $B | sed -E "s/^ total \| ([0-9]+) edits over ([0-9]+).*/\1 \2/")
OE=$(grep -a -E "^ outside \|" $B | sed -E "s/^ outside \| ([0-9]+) edits over ([0-9]+).*/\1 \2/")
KE=$(grep -a -E "^ total bench \|" $K | sed -E "s/^ total bench \| ([0-9]+) edits over ([0-9]+).*/\1 \2/")
echo "$BE $OE $KE" | awk '{ print "ALL KEYED:", $1 + $3 + $5, "over", $2 + $4 + $6 }'
echo "== 17:37 and the three adjudicated readings"
grep -a -E "^ row \| (unadjudicated/)?cw-2026-(09-23-173723|08-17-013347|08-17-134712|08-18-003758) \|" $B | cut -d'|' -f2,3,4,8,9
echo "== named floors"
grep -a -E "^\s+(Passed|Failed) " $N | sed -E "s/ \[[0-9.]+ ?m?s\]//" | wc -l
grep -a -i -E "named .*floor|floor of" $N | sed -E "s/^ +//" | sort | head -40

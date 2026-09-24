#!/bin/sh
# unit 424 - tally a Rig round from its per-type outputs.
# Usage: sh .run-unit/unit424-rigsum.sh <suffix>
cd /c/Source/HamLet || exit 1
TT=0
PP=0
for f in .run-unit/unit424-rig-$1-*Tests.txt
do
  T=$(grep -E "Total tests:" "$f" | grep -oE "[0-9]+" | head -1)
  P=$(grep -E "^\s+Passed:" "$f" | grep -oE "[0-9]+" | head -1)
  [ -z "$T" ] && T=0
  [ -z "$P" ] && P=0
  TT=$((TT+T))
  PP=$((PP+P))
  [ "$T" != "$P" ] && echo "NOT ALL GREEN: $f $P of $T"
done
N=$(ls .run-unit/unit424-rig-$1-*Tests.txt | wc -l)
echo "Rig round $1: $N types, $PP of $TT"

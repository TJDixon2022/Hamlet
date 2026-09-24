#!/bin/sh
# unit 421 - the readings and totals at a given suffix against entry, and src/Hamlet.RadioEngine/Cw against entry 23457c4b.
# Usage: sh .run-unit/unit421-same.sh <suffix>
cd /c/Source/HamLet || exit 1
R=.run-unit/unit421
for k in adjudicated baseline keyed
do
  grep -hE "read:|it reads:|^\s*(row|total|outside|guard|key|total live|total bench) \|" $R-$k-entry.txt | sed "s/\[[0-9.]* m\?s\]//" > $R-cmp-$k-entry.tmp
  grep -hE "read:|it reads:|^\s*(row|total|outside|guard|key|total live|total bench) \|" $R-$k-$1.txt | sed "s/\[[0-9.]* m\?s\]//" > $R-cmp-$k-$1.tmp
  if cmp -s $R-cmp-$k-entry.tmp $R-cmp-$k-$1.tmp
  then
    echo "$k: identical to entry, $(wc -l < $R-cmp-$k-entry.tmp) lines compared"
  else
    echo "$k: DIFFERS from entry"
    diff $R-cmp-$k-entry.tmp $R-cmp-$k-$1.tmp | head -20
  fi
done
echo "== src/Hamlet.RadioEngine/Cw against 23457c4b, working tree"
git diff --stat 23457c4b -- src/Hamlet.RadioEngine/Cw
echo "== end Cw"

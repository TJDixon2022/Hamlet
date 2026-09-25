#!/bin/sh
# unit 431 - the exit round against the entry: rows, keyed totals, adjudicated, strays, the opening, the source.
cd /c/Source/HamLet || exit 1
R=.run-unit/unit431
F="Total time|RC=|WALL|elapsed|\[[0-9.]+ m?s\]|Duration|\[xUnit.net|\[[0-9]+ m [0-9]+ s\]|\[[0-9]+ m\]"
echo "== rows, entry against exit"
sh $R-cmprows.sh entry exit | grep -E "^count|ABOVE|CHECK"
echo "== keyed and added at exit"
grep -h "check | read here" $R-strays-exit.txt
sh $R-added.sh exit
for T in keyed baseline adjudicated strays captures named
do
  grep -hvE "$F" $R-$T-entry.txt > $R-cmp-a.tmp
  grep -hvE "$F" $R-$T-exit.txt > $R-cmp-b.tmp
  echo "== $T, entry against exit, timing lines left out: $(diff $R-cmp-a.tmp $R-cmp-b.tmp | wc -l) diff lines"
  diff $R-cmp-a.tmp $R-cmp-b.tmp | head -6
done
echo "== the opening at exit"
LC_ALL=C.UTF-8 sh $R-opening.sh unit431-type-exit-WhatTheOpeningHeardTests.txt
echo "== source"
sh $R-srcdiff.sh

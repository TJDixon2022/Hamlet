#!/bin/sh
# unit 428 - the exit round against the entry: rows, keyed totals, the adjudicated readings, the source diff.
cd /c/Source/HamLet || exit 1
R=.run-unit/unit428
echo "== rows, entry against exit"
sh $R-cmprows.sh entry exit | grep -E "^count|ABOVE|CHECK"
echo "== keyed and baseline totals at exit"
grep -hE "total bench|total live|^\s*total \||^\s*outside \|" $R-keyed-exit.txt $R-baseline-exit.txt
echo "== keyed and baseline, entry against exit, timing lines left out"
grep -hvE "Total time|RC=|WALL|elapsed|\[[0-9.]+ m?s\]|Duration" $R-keyed-entry.txt $R-baseline-entry.txt > $R-cmp-keyed-a.tmp
grep -hvE "Total time|RC=|WALL|elapsed|\[[0-9.]+ m?s\]|Duration" $R-keyed-exit.txt $R-baseline-exit.txt > $R-cmp-keyed-b.tmp
diff $R-cmp-keyed-a.tmp $R-cmp-keyed-b.tmp | head -20
echo "== adjudicated, entry against exit, timing lines left out"
grep -hvE "Total time|RC=|WALL|elapsed|\[[0-9.]+ m?s\]|Duration" $R-adjudicated-entry.txt > $R-cmp-adj-a.tmp
grep -hvE "Total time|RC=|WALL|elapsed|\[[0-9.]+ m?s\]|Duration" $R-adjudicated-exit.txt > $R-cmp-adj-b.tmp
diff $R-cmp-adj-a.tmp $R-cmp-adj-b.tmp | head -20
echo "== source"
sh $R-srcdiff.sh

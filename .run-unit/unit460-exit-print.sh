#!/bin/sh
# unit 459 - keep the exit round's transmit diff, Cw/Second diff, text diffs and git status in one file.
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit460-tx-exit.txt
sh .run-unit/unit460-tx.sh > $OUT 2>&1
echo "== git diff 1f1c915d -- src/Hamlet.RadioEngine/Cw/Second/ (empty means no line changed):" >> $OUT
git diff 1f1c915d -- src/Hamlet.RadioEngine/Cw/Second/ >> $OUT
echo "== end Cw/Second" >> $OUT
echo "== git diff 1f1c915d -- src (empty means no src line changed over the unit):" >> $OUT
git diff --stat 1f1c915d -- src >> $OUT
echo "== end src" >> $OUT
echo "== the port's texts against task 0's save" >> $OUT
cmp .run-unit/unit460-port-before.txt .run-unit/unit460-port-exit.txt >> $OUT 2>&1 && echo "(byte-identical)" >> $OUT
echo "== our texts: diff of every recording's text against task 0's save" >> $OUT
diff .run-unit/unit460-text-before.sorted.txt .run-unit/unit460-text-exit.sorted.txt >> $OUT && echo "(no difference)" >> $OUT
echo "== git status --short at exit" >> $OUT
git status --short >> $OUT
cat $OUT

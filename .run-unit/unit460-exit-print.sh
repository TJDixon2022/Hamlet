#!/bin/sh
# unit 460 - the exit round's prints in one file: metric totals against entry, the transmit files, Cw/Second, src, texts and git status.
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit460-tx-exit.txt
: > $OUT
echo "== metric totals, entry then exit" >> $OUT
grep -a -E "^ total \| (real|synthetic)" .run-unit/unit460-metrics-entry.txt >> $OUT
grep -a -E "^ total \| (real|synthetic)" .run-unit/unit460-metrics-exit.txt >> $OUT
echo "== metric rows: entry against exit (no output between the markers means none moved)" >> $OUT
grep -a -E "^ (row|condition) \|" .run-unit/unit460-metrics-entry.txt > .run-unit/unit460-rows-entry.tmp
grep -a -E "^ (row|condition) \|" .run-unit/unit460-metrics-exit.txt > .run-unit/unit460-rows-exit.tmp
diff .run-unit/unit460-rows-entry.tmp .run-unit/unit460-rows-exit.tmp >> $OUT
echo "== end rows" >> $OUT
sh .run-unit/unit460-tx.sh >> $OUT 2>&1
echo "== git diff 1f1c915d -- src/Hamlet.RadioEngine/Cw/Second/ (empty means no line changed):" >> $OUT
git diff 1f1c915d -- src/Hamlet.RadioEngine/Cw/Second/ >> $OUT
echo "== end Cw/Second" >> $OUT
echo "== git diff 1f1c915d -- src (empty means no src line changed over the unit):" >> $OUT
git diff --stat 1f1c915d -- src >> $OUT
echo "== end src" >> $OUT
echo "== our texts: every recording's text against task 0's save" >> $OUT
diff .run-unit/unit460-text-before.sorted.txt .run-unit/unit460-text-exit.sorted.txt >> $OUT && echo "(no difference, 126 lines)" >> $OUT
echo "== git status --short at exit" >> $OUT
git status --short >> $OUT
cat $OUT

#!/bin/sh
# unit 458 - keep the exit round's transmit diff, Cw/Second diff, parity.md diff, text diff and git status in one file.
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit458-tx-exit.txt
sh .run-unit/unit458-tx.sh > $OUT 2>&1
echo "== git diff 845fd70f -- src/Hamlet.RadioEngine/Cw/Second/ (empty means no line changed):" >> $OUT
git diff 845fd70f -- src/Hamlet.RadioEngine/Cw/Second/ >> $OUT
echo "== end Cw/Second" >> $OUT
echo "== text: diff of every recording's text against task 0's save" >> $OUT
diff .run-unit/unit458-text-before.sorted.txt .run-unit/unit458-text-exit.sorted.txt >> $OUT && echo "(no difference)" >> $OUT
echo "== parity.md against its commit, numbers only (decode times move run to run):" >> $OUT
git diff --stat -- docs/phase-requirements/parity.md >> $OUT
git diff -U0 -- docs/phase-requirements/parity.md | grep -E "^[-+]\|" >> $OUT
echo "== git status --short at exit" >> $OUT
git status --short >> $OUT
cat $OUT

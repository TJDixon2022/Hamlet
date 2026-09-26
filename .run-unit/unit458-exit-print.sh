#!/bin/sh
# unit 457 - keep the exit round's transmit diff, src list and git status in one file.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit458-tx.sh > .run-unit/unit458-tx-exit.txt 2>&1
echo "== git status --short at exit" >> .run-unit/unit458-tx-exit.txt
git status --short >> .run-unit/unit458-tx-exit.txt
echo "== text: diff of every recording's text against task 0's save" >> .run-unit/unit458-tx-exit.txt
diff .run-unit/unit458-text-before.sorted.txt .run-unit/unit458-text-exit.sorted.txt >> .run-unit/unit458-tx-exit.txt && echo "(no difference)" >> .run-unit/unit458-tx-exit.txt
wc -l .run-unit/unit458-tx-exit.txt

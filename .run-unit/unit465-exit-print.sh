#!/bin/sh
# unit 465 task 4 - the exit prints: saves against task 0, the arbitrated and live transcripts, the port untouched, the transmit files, git status, the commit list.
cd /c/Source/HamLet || exit 1
export LC_ALL=C
sh .run-unit/unit465-save.sh exit "4 of 4" before
sh .run-unit/unit465-corpus.sh exit "4 of 4"
echo "== git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/:"
git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/
echo "(end)"
sh .run-unit/unit465-tx.sh 2>&1 | grep -v "^== src this unit" | sed -n '1,14p'
echo "== git status"
git status --short
echo "== this unit's commits"
git log --oneline 71ac86be..HEAD

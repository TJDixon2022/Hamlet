#!/bin/sh
# unit 460 - task 2 commit, second step: the staged set as it stands (the ignored .tmp files left out), then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
git add -- .run-unit/unit460-commit2b.sh
git commit -q -F .run-unit/unit460-msg2.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status --short
git status -sb | head -1

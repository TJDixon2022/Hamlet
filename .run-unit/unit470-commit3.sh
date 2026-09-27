#!/bin/sh
# unit 470 - task 3 commit: the exit round, output.md, the final status; never src or tests; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src tests || { echo "src or tests not clean - not committing"; exit 1; }
sh tools/status.sh COMPLETED "3 of 3" tim "output.md -> Claude Web" "Unit 470 done: the no-sure-while-acquiring gate refused - it would dim 293 right real letters to catch 29 wrong; screen unchanged, patch kept; exit round as at entry; step 2's count now 2 of 3"
git add -- PROJECT_STATUS.md output.md || exit 1
for f in .run-unit/unit470-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git commit -q -F .run-unit/unit470-msg3.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
git status --short

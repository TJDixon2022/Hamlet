#!/bin/sh
# unit 465 - task 4 commit: the exit printouts, output.md and the final status; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src tests || { echo "src or tests not clean - not committing"; exit 1; }
git diff --quiet -- docs/phase-requirements/parity.md || { echo "parity.md not restored - not committing"; exit 1; }
sh tools/status.sh COMPLETED "4 of 4" tim none "Unit 465 done: 9.6 ticked - the port reads live beside ours, one arbiter, one transcript, both readings on the sheet; port advisory everywhere today, so the screen is unchanged; output.md written"
git add -- output.md PROJECT_STATUS.md || exit 1
for f in .run-unit/unit465-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git commit -q -F .run-unit/unit465-msg4.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
git status --short

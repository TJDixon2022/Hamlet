#!/bin/sh
# unit 472 task 4 - the closing commit: status COMPLETED first, then the tracked changes, this unit's .run-unit files and output.md, then push.
# Usage: sh .run-unit/unit472-commit4.sh <msg-file> "<note>"
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "4 of 4" tim "output.md -> Claude Web" "$2"
git add -u -- .run-unit PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md docs Directory.Build.props PROJECT_STATUS.md PHASE_PLAN.md output.md || exit 1
for f in .run-unit/unit472-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
echo "== staged"
git diff --cached --stat | tail -40
echo "== not staged"
git status --short | grep -v "^[MADR] " | head -20
git commit -q -F "$1" || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
grep -E "^(STATE|TASK|BALL|NEXT_PASTE|UPDATED|NOTE):" PROJECT_STATUS.md

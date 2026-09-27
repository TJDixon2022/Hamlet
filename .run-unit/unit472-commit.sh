#!/bin/sh
# unit 471 - one task's commit: tracked changes in the named paths, this unit's .run-unit files, then push.
# Usage: sh .run-unit/unit472-commit.sh <msg-file> "<n of 4>" "<note>" [extra paths to add...]
cd /c/Source/HamLet || exit 1
MSG=$1
TASK=$2
NOTE=$3
shift 3
sh tools/status.sh EXECUTING "$TASK" code none "$NOTE"
git add -u -- .run-unit PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md docs Directory.Build.props PROJECT_STATUS.md PARKED.md PHASE_PLAN.md || exit 1
for f in .run-unit/unit472-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
for f in "$@"
do
  git add -- "$f" || exit 1
done
echo "== staged"
git diff --cached --stat | tail -40
echo "== not staged"
git status --short | grep -v "^[MADR] " | head -20
git commit -q -F "$MSG" || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1

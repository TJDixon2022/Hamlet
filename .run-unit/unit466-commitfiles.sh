#!/bin/sh
# unit 466 - commit named files plus this unit's .run-unit files and the status; then push.
# Usage: sh .run-unit/unit466-commitfiles.sh <message-file> "<n of 4>" "<status note>" [path ...]
cd /c/Source/HamLet || exit 1
MSG=$1
TASK=$2
NOTE=$3
shift 3
sh tools/status.sh EXECUTING "$TASK" code none "$NOTE"
git add -- PROJECT_STATUS.md || exit 1
for f in "$@"
do
  git add -- "$f" || exit 1
done
for f in .run-unit/unit466-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
echo "== staged"
git status --short | grep -v "^??"
echo "== untracked"
git status --short | grep "^??"
git commit -q -F "$MSG" || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1

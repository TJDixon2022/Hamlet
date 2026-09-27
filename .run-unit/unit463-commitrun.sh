#!/bin/sh
# unit 462 - commit this unit's .run-unit files and the status, never anything under src; then push.
# Usage: sh .run-unit/unit463-commitrun.sh <message-file> "<n of 4>" "<status note>"
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh EXECUTING "$2" code none "$3"
git add -- PROJECT_STATUS.md || exit 1
for f in .run-unit/unit463-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git commit -q -F "$1" || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1

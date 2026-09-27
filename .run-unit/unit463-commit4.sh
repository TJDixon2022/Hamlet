#!/bin/sh
# unit 463 - task 4 commit: the exit printouts, output.md, the final status; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh COMPLETED "4 of 4" code none "Unit 463 done: A1 and A2 both refused under R78 and reverted (A2 closest - adjudicated held, 11 recordings worse); every mechanism 9.3 named now refused; 9.4 open; exit as entry; output.md written"
git add -- PROJECT_STATUS.md output.md || exit 1
for f in .run-unit/unit463-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git commit -q -F .run-unit/unit463-msg4.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
git status --short

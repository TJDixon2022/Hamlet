#!/bin/sh
# unit 466 - task 4 commit: output.md, the exit printouts, and the final status; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh COMPLETED "4 of 4" tim none "Unit 466 done, 9.7 left open: switch built, HM-REQ-128 holds on the live row and all 4 losing rows but not on the aggregate synthetic, all; owner's reading asked in output.md section 4 item 1; screen unchanged"
git add -- output.md PROJECT_STATUS.md || exit 1
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
git commit -q -F .run-unit/unit466-msg4.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
head -11 PROJECT_STATUS.md

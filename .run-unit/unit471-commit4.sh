#!/bin/sh
# unit 471 - the last commit: status COMPLETED with the ball to the owner, the exit printouts and output.md, then push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "4 of 4" tim "output.md -> Claude Web" "Unit 471 done: the re-read at the proved pitch and speed refused - every letter it could reach was already right; step 2 closed partial, 2.4 ticked, MET-CER-SURE 33/436 real against one in a hundred"
git add -u -- .run-unit PROJECT_STATUS.md docs || exit 1
git add -- output.md || exit 1
for f in .run-unit/unit471-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
echo "== staged"
git diff --cached --stat | tail -30
echo "== not staged"
git status --short | grep -v "^[MADR] " | head -20
git commit -q -F .run-unit/unit471-msg4.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -3
grep -E "^(STATE|TASK|BALL|NEXT_PASTE|UPDATED|NOTE):" PROJECT_STATUS.md

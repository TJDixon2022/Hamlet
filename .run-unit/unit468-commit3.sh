#!/bin/sh
# unit 468 - task 3 commit: the exit round printouts, output.md, the final status; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh COMPLETED "3 of 3" tim none "Unit 468 done: 7.2 ticked - five TX-* fists generated and proved, two refused; HM-REQ-050 not met on any of the four must fists (tight fist reads 1 of 21); exit round as at entry; screen unchanged"
git add -- output.md PROJECT_STATUS.md || exit 1
for f in .run-unit/unit468-*
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
git commit -q -m "unit468 task 3: exit round - every entry figure as at entry; the sender proof 16 of 16; app line 277 of 278 on the rerun and TheChipSaysTheChosenModeTests alone 6 of 6 (DECIDED 7); src, port and transmit diffs empty; output.md written (7.2)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>" || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
head -12 PROJECT_STATUS.md

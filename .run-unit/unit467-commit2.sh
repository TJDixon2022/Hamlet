#!/bin/sh
# unit 467 - task 2 commit: output.md, the final status, this unit's .run-unit files; never src; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh COMPLETED "2 of 2" tim none "Unit 467 done: 9.7 and 9.8 ticked - HM-REQ-128 holds on all 10 condition rows and the live row, synthetic, all printed as a summary; step 9 open only at 9.4, R86 leaves no authorable step; screen unchanged"
git add -- output.md PROJECT_STATUS.md || exit 1
for f in .run-unit/unit467-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
echo "== staged, not this unit's run files"
git diff --cached --name-status | grep -v "\.run-unit/unit467-"
git commit -q -F .run-unit/unit467-msg2.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
git status --short
head -11 PROJECT_STATUS.md

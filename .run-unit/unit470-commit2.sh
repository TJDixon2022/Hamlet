#!/bin/sh
# unit 470 - task 2 commit, refused: metrics.md and this unit's .run-unit files only; never src or tests; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src tests || { echo "src or tests not clean - not committing"; exit 1; }
[ -z "$(git status --short -- src tests)" ] || { echo "src or tests have untracked files - not committing"; exit 1; }
sh tools/status.sh EXECUTING "2 of 3" code none "Task 2 done - gate refused on coverage (real 403 to 110, synthetic 159 to 66); patch kept, src restored, refusal in metrics.md; committing documents only"
git add -- PROJECT_STATUS.md docs/phase-requirements/metrics.md || exit 1
for f in .run-unit/unit470-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git commit -q -F .run-unit/unit470-msg2.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1

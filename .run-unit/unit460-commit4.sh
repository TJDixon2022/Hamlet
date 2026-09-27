#!/bin/sh
# unit 460 - task 4 commit: the exit printouts, the tick, output.md and the runner's watched.cpu; status COMPLETED; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh COMPLETED "4 of 4" code none "Unit 460 done: three named floors re-banked (17:37 38, 032113 43, 032129 42), 2.5 ticked with 3 of 3 commits green; pair-speed trace found no separating mechanism, task 3 not run; output.md written"
for f in .run-unit/unit460-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git add -- output.md PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md PROJECT_STATUS.md .run-unit/watched.cpu || exit 1
git commit -q -F .run-unit/unit460-msg4.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status --short
git status -sb | head -1

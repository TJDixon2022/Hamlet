#!/bin/sh
# unit 461 - task 4 commit: the exit printouts, the 7.5 tick, output.md and the runner's watched.cpu; status COMPLETED; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet e6140891 -- src || { echo "src differs from e6140891 - not committing"; exit 1; }
sh tools/status.sh COMPLETED "4 of 4" code none "Unit 461 done: all ten CH-* profiles with 9.1 values generated and proved, CH-MDV refused, channels.md written, 7.1 and 7.5 ticked; HM-REQ-013/040/041 read not met at their point; output.md written"
for f in .run-unit/unit461-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git add -- output.md PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md PROJECT_STATUS.md .run-unit/watched.cpu || exit 1
git commit -q -F .run-unit/unit461-msg4.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status --short
git status -sb | head -1

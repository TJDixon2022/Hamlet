#!/bin/sh
# unit 422 - final status, then the exit commit and push. Status is written first so the commit carries it.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 4 of 4" tim "output.md -> Claude Web" "Unit 422 complete: 6.4 ticked, every CW tab and band row control says what it does on hover, CW 7 to 15 of 15, band row 18 to 33 of 34; P16 and P20 red as at entry"
git add -- output.md PHASE_OUTCOME.md PHASE_PLAN.md PROJECT_STATUS.md docs/phase-correctness/PARKED.md .run-unit/unit422-*.sh .run-unit/unit422-*.txt || exit 1
git commit -q -F .run-unit/unit422-msg-t4.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short

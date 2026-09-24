#!/bin/sh
# unit 421 - final status, then the exit commit and push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 4 of 4" tim "output.md -> Claude Web" "Unit 421 complete: floors count above a raw span bar of 13, 3.7 ticked; lone E under the bar kept off, all keyed 167 to 165; 3.6 held for the owner, none blocking"
git add -- output.md PHASE_PLAN.md PHASE_OUTCOME.md PROJECT_STATUS.md .run-unit/unit421-*.txt .run-unit/unit421-*.sh || exit 1
git commit -q -F .run-unit/unit421-msg-t4.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short | grep -v "^ M .run-unit/\|^ D .run-unit/\|^?? .run-unit/reports/"

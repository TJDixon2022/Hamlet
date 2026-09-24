#!/bin/sh
# unit 416 - final status, exit commit with the report, push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 4 of 4" tim "output.md -> Claude Web" "Unit 416 complete: relabel kept under R66 and a second space change kept, all keyed 217 to 167 over 565, no letter moved; 3.2 and 3.3 ticked; 1 item parked as P9, none blocking"
git add -- output.md PHASE_PLAN.md PHASE_STATUS.md PHASE_OUTCOME.md PROJECT_STATUS.md docs/phase-correctness/PARKED.md .run-unit/unit416-* || exit 1
git commit -q -F .run-unit/unit416-msg-t4.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short | grep -v "^ M .run-unit/\|^ D .run-unit/\|SESSION.lock"

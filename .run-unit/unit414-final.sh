#!/bin/sh
# unit 414 - final status, exit commit with the report, push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 5 of 5" tim "output.md -> Claude Web" "Unit 414 complete: twelve synthetic CQ calls with exact keys, grid 102 over 243 and five-unit row 63 over 81; inferring-a-key.md written; step 1 done; 2 items parked, none blocking"
git add -- output.md PHASE_PLAN.md PHASE_STATUS.md PHASE_OUTCOME.md PROJECT_STATUS.md docs/phase-correctness/baseline.md .run-unit/unit414-*.txt .run-unit/unit414-*.sh .run-unit/unit414-*.norm || exit 1
git commit -q -F .run-unit/unit414-msg-t5.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short | grep -v "^ M .run-unit/\|^ D .run-unit/\|^?? .run-unit/reports\|SESSION.lock\|RUN_LEDGER\|WORK_INSTRUCTIONS"

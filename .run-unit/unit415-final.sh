#!/bin/sh
# unit 415 - final status, exit commit with the report, push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 4 of 4" tim "output.md -> Claude Web" "Unit 415 complete: two space-only relabels moved no letter on any recording, all keyed 191 and 185 over 565 from 217, both out on 3.2 test 3 at 134712; 3.5 ticked; 1 ruling asked on test 3, not halting"
git add -- output.md PHASE_PLAN.md PHASE_STATUS.md PHASE_OUTCOME.md PROJECT_STATUS.md docs/phase-correctness/baseline.md docs/phase-correctness/unit415-trace.md .run-unit/unit415-* || exit 1
git commit -q -F .run-unit/unit415-msg-t4.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short | grep -v "^ M .run-unit/\|^ D .run-unit/\|^?? .run-unit/reports\|SESSION.lock\|RUN_LEDGER\|WORK_INSTRUCTIONS"

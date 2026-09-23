#!/bin/sh
# unit 410 - the final status, then stage, commit and push the report.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 4 of 4" tim "output.md -> Claude Web" "Unit 410 complete: baseline 33 edits over 46 chars against inferred keys, 17:37 29 over 25; there 15 boundaries and 14 letters, so the bench letters are not all right; section 4 parks 2 items"
git add -- PHASE_PLAN.md PHASE_OUTCOME.md PROJECT_STATUS.md output.md docs/phase-correctness/PARKED.md .run-unit/unit410-* || exit 1
git commit -q -F .run-unit/unit410-msg-t4.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short

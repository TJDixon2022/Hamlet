#!/bin/sh
# unit 412 - the final status, then stage, commit and push the report.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 4 of 4" tim "output.md -> Claude Web" "Unit 412 complete: fourteen captures banked, floor rows 37 to 51; guard proved, E and T dropped took edits 33 to 21 and broke 13 of 13 named floors; 10 captures keyed; section 4 raises 1 item"
git add -- output.md PHASE_PLAN.md PHASE_STATUS.md PHASE_OUTCOME.md PROJECT_STATUS.md docs/phase-correctness/PARKED.md .run-unit/unit412-* || exit 1
git commit -q -F .run-unit/unit412-msg-t4.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short

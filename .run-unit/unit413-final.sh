#!/bin/sh
# unit 413 - the final status, then stage, commit and push the report.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 4 of 4" tim "output.md -> Claude Web" "Unit 413 complete: split traced to CwUnitEstimator.cs 216, 3.1 ticked; two repairs built and out under 3.2, all keyed 217 to 207 and to 193 but named counts fell; 217 at exit; section 4 raises 3 items, none blocking"
git add -- output.md PHASE_STATUS.md PHASE_OUTCOME.md PROJECT_STATUS.md docs/phase-correctness/baseline.md .run-unit/unit413-* || exit 1
git commit -q -F .run-unit/unit413-msg-t4.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short

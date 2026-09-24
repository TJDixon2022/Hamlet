#!/bin/sh
# unit 411 - the final status, then stage, commit and push the report.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 4 of 4" tim "output.md -> Claude Web" "Unit 411 complete: the RF gain banner states the read-back it holds, keying and elementHz lines true, tonePeak dropped; 4 contradicting sentences to 1; section 4 raises 2 items"
git add -- PHASE_PLAN.md PHASE_OUTCOME.md PROJECT_STATUS.md output.md .run-unit/unit411-* || exit 1
git commit -q -F .run-unit/unit411-msg-t4.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short

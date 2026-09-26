#!/bin/sh
# unit 445 - closing: status COMPLETED, then commit and push the exit record.
# The unit445 glob is left to git, quoted, so ignored .tmp files are skipped rather than refused.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "3 of 3" tim "output.md -> Claude Web" "Unit 445 complete: one kept change dims 2 wrong letters and no right one on 032129 (MET-INVENTED 47 to 45, coverage held); 3.2 and 3.3 ticked"
git add -- output.md PROJECT_STATUS.md PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md '.run-unit/unit445-*' || exit 1
git commit -q -F .run-unit/unit445-msg3.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
grep "^STATE\|^UPDATED" PROJECT_STATUS.md

#!/bin/sh
# unit 414 - tick 1.1 to 1.5 in PHASE_PLAN.md and mark step 1 done in PHASE_STATUS.md and PHASE_OUTCOME.md.
cd /c/Source/HamLet || exit 1
sed -i "s/^- \[ \] 1\.\([1-5]\) /- [x] 1.\1 /" PHASE_PLAN.md
sed -i "s/^STEP: 1 | partial | /STEP: 1 | done | /" PHASE_STATUS.md PHASE_OUTCOME.md
grep -n "^- \[.\] 1\." PHASE_PLAN.md | cut -c1-60
grep -n "^STEP: 1 |" PHASE_STATUS.md PHASE_OUTCOME.md | cut -c1-60

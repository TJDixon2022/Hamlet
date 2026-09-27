#!/bin/sh
# unit 464 - tick 9.5 in both copies of PHASE_PLAN.md, and nothing else; commit and push.
cd /c/Source/HamLet || exit 1
for f in PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
do
  sed -i 's/^- \[ \] 9\.5 HM-REQ-124:/- [x] 9.5 HM-REQ-124:/' "$f"
  grep -n "^- \[.\] 9\.[1-8]" "$f" | cut -c1-40
done
git diff --stat -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "the two copies are identical"
sh tools/status.sh EXECUTING "3 of 4" code none "Task 3 committed at efdd5d11; ticking 9.5 in both copies of PHASE_PLAN.md"
git add -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md PROJECT_STATUS.md .run-unit/unit464-tick.sh .run-unit/unit464-msg-tick.txt || exit 1
git commit -q -F .run-unit/unit464-msg-tick.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1

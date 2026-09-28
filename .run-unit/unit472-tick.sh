#!/bin/sh
# unit 472 task 4 - tick 3.6 in both copies of PHASE_PLAN.md, the box and nothing else.
cd /c/Source/HamLet || exit 1
for f in PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
do
  sed -i 's/^- \[ \] 3\.6 /- [x] 3.6 /' "$f"
  grep -n -E "^- \[.\] 3\.[0-9]" "$f" | cut -c1-60
done
git diff --stat -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "copies identical"

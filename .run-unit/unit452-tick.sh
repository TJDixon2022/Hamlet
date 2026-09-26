#!/bin/sh
# unit 452 - tick one criterion of step 4 in both copies of PHASE_PLAN.md.
# Usage: sh .run-unit/unit452-tick.sh <2.n>
cd /c/Source/HamLet || exit 1
N=$(echo "$1" | sed 's/\./\\./')
sed -i "s/^- \[ \] $N /- [x] $1 /" PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
grep -n "^- \[.\] $N " PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md | cut -c1-70

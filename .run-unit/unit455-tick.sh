#!/bin/sh
# unit 455 - tick one criterion in both copies of PHASE_PLAN.md.
# Usage: sh .run-unit/unit455-tick.sh <6.n>
cd /c/Source/HamLet || exit 1
N=$(echo "$1" | sed 's/\./\\./')
sed -i "s/^- \[ \] $N /- [x] $1 /" PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
grep -n "^- \[.\] $N " PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md | cut -c1-70

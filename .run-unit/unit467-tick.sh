#!/bin/sh
# unit 467 - tick one plan line in both PHASE_PLAN.md copies, commit it alone, push.
# Usage: sh .run-unit/unit467-tick.sh <9.7|9.8> <message-file> "<n of 2>" "<status note>"
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
for f in PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
do
  sed -i "s/^- \[ \] $1 /- [x] $1 /" "$f"
  grep -n "^- \[.\] $1 " "$f" | cut -c1-60
done
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "plan copies identical"
git diff --stat -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
sh tools/status.sh EXECUTING "$3" code none "$4"
git add -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md PROJECT_STATUS.md || exit 1
git commit -q -F "$2" || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1

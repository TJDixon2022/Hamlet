#!/bin/sh
# unit 482 task 3 - tick 11.3 in both copies of the plan, and nothing else.
cd /c/Source/HamLet || exit 1
for f in PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
do
  sed -i -e 's/^- \[ \] 11\.3 \(.*\)$/- [x] 11.3 \1 - met by unit 482: TheOwnersPressLandsInTheFileTests green (each button pressed in a headless window writes exactly 1 row to the file, 2 in all; the six things present, the survey asserted as an array), watched red on a dropped trackerHz, green at HEAD with no change to src; TheOwnersVerdictIsARowTests 7 of 7, TheVerdictCarriesTheScopeTests 4 of 4/' "$f"
  grep -n "^- \[.\] 11\." "$f" | cut -c1-60
done
git diff --no-index --stat PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
echo "copies compared"
git diff --stat -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md

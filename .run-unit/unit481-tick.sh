#!/bin/sh
# unit 481 task 3 - tick 11.5 and 11.6 in both copies of the plan, and nothing else.
cd /c/Source/HamLet || exit 1
for f in PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
do
  sed -i -e 's/^- \[ \] 11\.5 \(.*\)$/- [x] 11.5 \1 - met by unit 481: 278 of 278 at exit on the first run, no recording read/' \
         -e 's/^- \[ \] 11\.6 \(.*\)$/- [x] 11.6 \1 - met by unit 481: TheAnnouncedChangeMatchesTheChosenChangeTests green, and green at HEAD with no change to src; 423 1 of 1, 389 2 of 2/' "$f"
  grep -n "^- \[.\] 11\." "$f" | cut -c1-60
done
git diff --no-index --stat PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
echo "copies compared"

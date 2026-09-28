#!/bin/sh
# unit 478 task 2 - PHASE_PLAN.md, both copies: 11.1 and 11.2 get one appended clause; neither is unticked.
cd /c/Source/HamLet || exit 1
for f in PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
do
  grep -c "retired by unit 478" "$f"
  sed -i -E '/^- \[.\] 11\.[12] /s/(\r?)$/ - retired by unit 478 under R92; the scope shows what they showed\1/' "$f"
  grep -n "^- \[.\] 11\.[12] " "$f" | sed -E 's/^([0-9]+:.{14}).*(.{90})$/\1 ... \2/'
done

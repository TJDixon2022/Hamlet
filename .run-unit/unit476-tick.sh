#!/bin/sh
# unit 476 task 4 - tick 12.1 and 12.3 in both plan copies; 12.2 is left for the owner's eye, 12.4 is not authored.
cd /c/Source/HamLet || exit 1
for f in PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
do
  sed -i -e 's/^- \[ \] 12\.1 /- [x] 12.1 /' -e 's/^- \[ \] 12\.3 /- [x] 12.3 /' "$f"
  grep -n "^- \[.\] 12\.[1-4]" "$f" | cut -c1-60
done

#!/bin/sh
# unit 443 - append the outcome entry to both copies of PHASE_OUTCOME.md.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit443-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit443-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 442" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md

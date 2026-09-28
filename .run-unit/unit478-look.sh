#!/bin/sh
# unit 478 - what the tree held at entry beyond HEAD: the launcher's edits to the phase files.
cd /c/Source/HamLet || exit 1
git status --short
git diff -- PHASE_OUTCOME.md PHASE_STATUS.md | head -80
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "^## UNIT 47" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
grep -n "^- \[.\] 1[12]\.[0-9]" PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md | cut -c1-200

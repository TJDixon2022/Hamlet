#!/bin/sh
# unit 452 - task 0's record edits: PHASE_STATUS names 452 at step 6 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: 451 - the speed says whether it was proved\$/WORK_INSTRUCTION: 452 - where the words break/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: [0-9]\$/CURRENT_STEP: 6/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.138</Version>#<Version>1.13.139</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit452-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit452-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 452" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 3" code none "Entry: records name 452 at step 6; building, then the carry-forward lines, floors, metrics, pitch and every recording's text"

#!/bin/sh
# unit 451 - task 0's record edits: PHASE_STATUS names 451 at step 5 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: 450 - the pitch says whether it was proved\$/WORK_INSTRUCTION: 451 - the speed says whether it was proved/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: [0-9]\$/CURRENT_STEP: 5/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.137</Version>#<Version>1.13.138</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit451-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit451-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 451" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 3" code none "Entry: records name 451 at step 5; building, then the carry-forward lines, floors, metrics, pitch and every recording's text"

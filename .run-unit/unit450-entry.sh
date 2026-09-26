#!/bin/sh
# unit 450 - task 0's record edits: PHASE_STATUS names 450 at step 4 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: 449 - the forty sure-wrong letters left, one more change, or step 2 closes\$/WORK_INSTRUCTION: 450 - the pitch says whether it was proved/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: [0-9]\$/CURRENT_STEP: 4/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.136</Version>#<Version>1.13.137</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit450-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit450-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 450" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 3" code none "Entry: records name 450 at step 4; building, then the carry-forward lines, floors, metrics, pitch and every recording's text"

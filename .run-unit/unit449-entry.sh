#!/bin/sh
# unit 449 - task 0's record edits: PHASE_STATUS names 449 at step 2 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: 448 - the decoder listens for keying from cold, and the opening is measured\$/WORK_INSTRUCTION: 449 - the forty sure-wrong letters left, one more change, or step 2 closes/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: [0-9]\$/CURRENT_STEP: 2/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.135</Version>#<Version>1.13.136</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit449-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit449-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 449" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 3" code none "Entry: records name 449 at step 2; building, then the carry-forward lines, floors, metrics and pitch"

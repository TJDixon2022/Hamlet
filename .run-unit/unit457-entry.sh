#!/bin/sh
# unit 457 - task 0's record edits: PHASE_STATUS names 457 at step 9 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: .*\$/WORK_INSTRUCTION: 457 - why fldigi's port loses the first dit, and a case chosen before it is read/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: [0-9]\$/CURRENT_STEP: 9/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.143</Version>#<Version>1.13.144</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit457-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit457-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 457" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 5" code none "Entry: records name 457 at step 9, version 1.13.144; building, then carry-forward lines, floors, the four metrics and the port's case"

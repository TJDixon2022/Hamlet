#!/bin/sh
# unit 454 - task 0's record edits: PHASE_STATUS names 454 at step 9 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: 453 - the words the owner named\$/WORK_INSTRUCTION: 454 - the second decoder reads beside ours/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: [0-9]\$/CURRENT_STEP: 9/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.140</Version>#<Version>1.13.141</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit454-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit454-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 454" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 4" code none "Entry: records name 454 at step 9; git clone of fldigi refused; building, then carry-forward lines, floors and the four metrics"

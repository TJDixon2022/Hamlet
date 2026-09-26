#!/bin/sh
# unit 456 - task 0's record edits: PHASE_STATUS names 456 at step 9 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: .*\$/WORK_INSTRUCTION: 456 - fldigi's CW receiver, ported as the second decoder/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: [0-9]\$/CURRENT_STEP: 9/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.142</Version>#<Version>1.13.143</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit456-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit456-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 456" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 5" code none "Entry: records name 456 at step 9, version 1.13.143; building, then carry-forward lines, floors and the four metrics"

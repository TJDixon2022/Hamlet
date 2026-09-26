#!/bin/sh
# unit 445 - task 0's record edits: PHASE_STATUS names 445 at step 3 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: 444 - WB6RED read as one callsign again\$/WORK_INSTRUCTION: 445 - a letter whose marks do not fit the speed is not printed as sure/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: 2\$/CURRENT_STEP: 3/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.131</Version>#<Version>1.13.132</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit445-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit445-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 445" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 3" code none "Entry: records named 445 at step 3; building before the entry round"

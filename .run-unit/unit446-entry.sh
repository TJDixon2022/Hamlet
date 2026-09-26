#!/bin/sh
# unit 446 - task 0's record edits: PHASE_STATUS names 446 at step 5 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: 445 - a letter whose marks do not fit the speed is not printed as sure\$/WORK_INSTRUCTION: 446 - the speed search reaches 5 and 45 words a minute/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: [0-9]\$/CURRENT_STEP: 5/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.132</Version>#<Version>1.13.133</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit446-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit446-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 446" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 3" code none "Entry: records named 446 at step 5; building before the entry round"

#!/bin/sh
# unit 447 - task 0's record edits: PHASE_STATUS names 447 at step 4 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: 446 - the speed search reaches 5 and 45 words a minute\$/WORK_INSTRUCTION: 447 - a pitch instrument, then the tracker hears the sender's own note/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: [0-9]\$/CURRENT_STEP: 4/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.133</Version>#<Version>1.13.134</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit447-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit447-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 447" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 4" code none "Entry: records named 447 at step 4; building before the entry round"

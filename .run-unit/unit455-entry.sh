#!/bin/sh
# unit 455 - task 0's record edits: PHASE_STATUS names 455 at step 6 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: .*\$/WORK_INSTRUCTION: 455 - a prosign arrives as a prosign/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: [0-9]\$/CURRENT_STEP: 6/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.141</Version>#<Version>1.13.142</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit455-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit455-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 455" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 4" code none "Entry: records name 455 at step 6, version 1.13.142; building, then carry-forward lines, floors and the four metrics"

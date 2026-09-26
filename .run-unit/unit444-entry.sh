#!/bin/sh
# unit 444 - task 0's record edits: PHASE_STATUS names 444 at step 2 (both copies), patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: 443 - the letters the decoder adds between words\$/WORK_INSTRUCTION: 444 - WB6RED read as one callsign again/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: 3\$/CURRENT_STEP: 2/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.130</Version>#<Version>1.13.131</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
cat .run-unit/unit444-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit444-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
grep -c "UNIT 444" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 3" code none "Entry: records named 444 at step 2; building before the entry round"

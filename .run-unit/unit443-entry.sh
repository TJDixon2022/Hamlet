#!/bin/sh
# unit 443 - task 0's record edits: PHASE_STATUS names 443 at step 3, patch-bump, outcome entry.
cd /c/Source/HamLet || exit 1
sed -i "s/^WORK_INSTRUCTION: 442 - sure has to mean something\$/WORK_INSTRUCTION: 443 - the letters the decoder adds between words/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s/^CURRENT_STEP: 2\$/CURRENT_STEP: 3/" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
sed -i "s#<Version>1.13.129</Version>#<Version>1.13.130</Version>#" Directory.Build.props
grep -n "CURRENT_STEP\|WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
sh .run-unit/unit443-append.sh
git status --short | grep -v "^?? .run-unit\|^ M .run-unit\|^ D .run-unit"

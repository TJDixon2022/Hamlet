#!/bin/sh
# unit 460 task 0 - the record: outcome block in both copies, PHASE_STATUS in both copies, the patch bump.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit460-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit460-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 2/' -e "s/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 460 - the three named floors settled on the owner's answer, and step 2's commits kept green/" "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION):" "$f"
done
sed -i 's#<Version>1.13.146</Version>#<Version>1.13.147</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props
grep -c "^## UNIT 460 - STEP 2" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md

#!/bin/sh
# unit 459 task 0 - the record: outcome block in both copies, PHASE_STATUS in both copies, the patch bump.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit459-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit459-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 9/' -e 's/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 459 - one technique from the fldigi port taken into our decoder, judged under R78/' "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION):" "$f"
done
sed -i 's#<Version>1.13.145</Version>#<Version>1.13.146</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props

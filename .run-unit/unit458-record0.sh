#!/bin/sh
# unit 458 task 0 - the record: outcome block in both copies, PHASE_STATUS in both copies, the patch bump.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit458-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit458-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 9/' -e 's/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 458 - both decoders on the same audio, the same scorer and one parity table/' "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION):" "$f"
done
sed -i 's#<Version>1.13.144</Version>#<Version>1.13.145</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props

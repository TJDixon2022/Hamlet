#!/bin/sh
# unit 482 (the owner's press lands in the file) task 0 - the record in both copies, PHASE_STATUS in both copies, the patch bump.
cd /c/Source/HamLet || exit 1
E=.run-unit/unit482-outcome-entry.md
B=.run-unit/unit482-arbiter-block.tmp
sed -n '/^ARBITER-DECISION$/,/^END-ARBITER-DECISION$/p' WORK_INSTRUCTIONS.md | tr -d '\r' > $B
{
  echo ""
  echo "## UNIT 482 - STEP 11"
  echo ""
  grep -E "^(STEP|APPROACH|MOVE|WHY|STATE|DECIDED|LICENCE): " $B
  echo "ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0."
  grep -E "^ADVANCES: " $B
  echo "RUN: hand run of instruction 482 (the owner's press lands in the file); SESSION.lock was already present at the root when the session began, so the session did not take it and does not release it."
} > $E
cut -c1-100 $E
cat $E >> PHASE_OUTCOME.md
cat $E >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 11/' -e "s/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 482 - the owner's press lands in the file/" "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION|HEARTBEAT)" "$f" | cut -c1-120
done
sed -i 's#<Version>1.13.168</Version>#<Version>1.13.169</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props
grep -c "^## UNIT 482 - STEP 11" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md

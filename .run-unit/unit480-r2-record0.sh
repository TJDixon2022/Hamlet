#!/bin/sh
# unit 480 (second instruction, the letter over the bars) task 0 - the record in both copies, PHASE_STATUS in both copies, the patch bump.
cd /c/Source/HamLet || exit 1
E=.run-unit/unit480-r2-outcome-entry.md
B=.run-unit/unit480-r2-arbiter-block.tmp
sed -n '/^ARBITER-DECISION$/,/^END-ARBITER-DECISION$/p' WORK_INSTRUCTIONS.md > $B
{
  echo ""
  echo "## UNIT 480 - STEP 12"
  echo ""
  grep -E "^(STEP|APPROACH|MOVE|WHY|STATE|DECIDED|LICENCE): " $B
  echo "ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0."
  grep -E "^ADVANCES: " $B
  echo "RUN: hand run of the second instruction numbered 480 (the letter over the bars); SESSION.lock was already present at the root when the session began, taken seconds before, so the session did not take it again and did not release it."
} > $E
cut -c1-100 $E
cat $E >> PHASE_OUTCOME.md
cat $E >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 12/' -e "s/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 480 - the letter sits over the bars that made it/" "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION)" "$f" | cut -c1-120
done
sed -i 's#<Version>1.13.166</Version>#<Version>1.13.167</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props
grep -c "^## UNIT 480 - STEP 12" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md

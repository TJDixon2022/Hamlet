#!/bin/sh
# unit 478 task 0 - the record: outcome block from the instruction's ARBITER-DECISION in both copies; PHASE_STATUS in both copies; the patch bump.
cd /c/Source/HamLet || exit 1
E=.run-unit/unit478-outcome-entry.md
B=.run-unit/unit478-arbiter-block.tmp
sed -n '/^ARBITER-DECISION$/,/^END-ARBITER-DECISION$/p' WORK_INSTRUCTIONS.md > $B
{
  echo ""
  echo "## UNIT 478 - STEP 12"
  echo ""
  grep -E "^(STEP|APPROACH|MOVE|WHY|STATE|DECIDED|LICENCE): " $B
  echo "ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0."
  grep -E "^ADVANCES: " $B
  echo "RUN: session launched with SESSION.lock already present and an empty STOP at the root, both the launcher's; neither taken, released nor removed by the session."
} > $E
cut -c1-100 $E
cat $E >> PHASE_OUTCOME.md
[ -f docs/phase-requirements/PHASE_OUTCOME.md ] && cat $E >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  [ -f "$f" ] || continue
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 12/' -e "s/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 478 - the scope is the middle picture/" "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION)" "$f" | cut -c1-120
done
sed -i 's#<Version>1.13.163</Version>#<Version>1.13.164</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props
grep -c "^## UNIT 478 - STEP 12" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md

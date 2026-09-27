#!/bin/sh
# unit 462 task 0 - the record: outcome block from the instruction's ARBITER-DECISION, in both copies; PHASE_STATUS in both copies; the patch bump.
cd /c/Source/HamLet || exit 1
E=.run-unit/unit462-outcome-entry.md
B=.run-unit/unit462-arbiter-block.tmp
sed -n '/^ARBITER-DECISION$/,/^END-ARBITER-DECISION$/p' WORK_INSTRUCTIONS.md > $B
{
  echo ""
  echo "## UNIT 462 - STEP 9"
  echo ""
  grep -E "^(STEP|APPROACH|MOVE|WHY|STATE|DECIDED|LICENCE): " $B
  echo "ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0."
  grep -E "^ADVANCES: " $B
  echo "RUN: session launched with SESSION.lock already present (PID 40900, 23:02:00); the lock is the launcher's and was not taken or released by the session."
} > $E
cat $E | cut -c1-100
cat $E >> PHASE_OUTCOME.md
cat $E >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 9/' -e "s/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 462 - one fldigi technique earns its place in ours: the spike rule, the two-dot class edge or the word-space edge, screened under R78/" "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION):" "$f"
done
sed -i 's#<Version>1.13.148</Version>#<Version>1.13.149</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props
grep -c "^## UNIT 462 - STEP 9" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md

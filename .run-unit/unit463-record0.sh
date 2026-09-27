#!/bin/sh
# unit 463 task 0 - the record: outcome block from the instruction's ARBITER-DECISION, in both copies; PHASE_STATUS in both copies; the patch bump.
cd /c/Source/HamLet || exit 1
E=.run-unit/unit463-outcome-entry.md
B=.run-unit/unit463-arbiter-block.tmp
sed -n '/^ARBITER-DECISION$/,/^END-ARBITER-DECISION$/p' WORK_INSTRUCTIONS.md > $B
{
  echo ""
  echo "## UNIT 463 - STEP 9"
  echo ""
  grep -E "^(STEP|APPROACH|MOVE|WHY|STATE|DECIDED|LICENCE): " $B
  echo "ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0."
  grep -E "^ADVANCES: " $B
  echo "RUN: session launched with SESSION.lock already present (PID 38940, 00:32:05); the lock is the launcher's and was not taken or released by the session."
} > $E
cat $E | cut -c1-100
cat $E >> PHASE_OUTCOME.md
cat $E >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 9/' -e "s/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 463 - the last fldigi technique nobody has tried: its detection front end, a half-dit integrator at the speed in force and its AGC-normalized level, screened on every recording and kept under R78/" "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION):" "$f"
done
sed -i 's#<Version>1.13.149</Version>#<Version>1.13.150</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props
grep -c "^## UNIT 463 - STEP 9" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md

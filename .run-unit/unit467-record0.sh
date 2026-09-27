#!/bin/sh
# unit 467 task 0 - the record: outcome block from the instruction's ARBITER-DECISION, in both copies; PHASE_STATUS in both copies; the patch bump.
cd /c/Source/HamLet || exit 1
E=.run-unit/unit467-outcome-entry.md
B=.run-unit/unit467-arbiter-block.tmp
sed -n '/^ARBITER-DECISION$/,/^END-ARBITER-DECISION$/p' WORK_INSTRUCTIONS.md > $B
{
  echo ""
  echo "## UNIT 467 - STEP 9"
  echo ""
  grep -E "^(STEP|APPROACH|MOVE|WHY|STATE|DECIDED|LICENCE): " $B
  echo "ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0."
  grep -E "^ADVANCES: " $B
  echo "RUN: session launched with SESSION.lock already present (PID 35488, 08:53:59); the lock is the launcher's and was not taken or released by the session."
} > $E
cut -c1-100 $E
cat $E >> PHASE_OUTCOME.md
[ -f docs/phase-requirements/PHASE_OUTCOME.md ] && cat $E >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 9/' -e "s/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 467 - HM-REQ-128 judged on conditions as CW_SPEC.md defines them: a union row is a summary, not a condition, and 9.7 is closed on the switch unit 466 built/" "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION):" "$f"
done
sed -i 's#<Version>1.13.153</Version>#<Version>1.13.154</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props
grep -c "^## UNIT 467 - STEP 9" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 2" code none "Task 0 - record written, version 1.13.154; entry round next: build, carry-forward lines, floors, metrics, parity, the saves with class and p"

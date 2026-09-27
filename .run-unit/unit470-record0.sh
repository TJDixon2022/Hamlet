#!/bin/sh
# unit 470 task 0 - the record: outcome block from the instruction's ARBITER-DECISION, in both copies; PHASE_STATUS in both copies; the patch bump.
cd /c/Source/HamLet || exit 1
E=.run-unit/unit470-outcome-entry.md
B=.run-unit/unit470-arbiter-block.tmp
sed -n '/^ARBITER-DECISION$/,/^END-ARBITER-DECISION$/p' WORK_INSTRUCTIONS.md > $B
{
  echo ""
  echo "## UNIT 470 - STEP 2"
  echo ""
  grep -E "^(STEP|APPROACH|MOVE|WHY|STATE|DECIDED|LICENCE): " $B
  echo "ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0."
  grep -E "^ADVANCES: " $B
  echo "RUN: session launched with SESSION.lock already present; the lock is the launcher's and was not taken or released by the session."
} > $E
cut -c1-100 $E
cat $E >> PHASE_OUTCOME.md
[ -f docs/phase-requirements/PHASE_OUTCOME.md ] && cat $E >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 2/' -e "s/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 470 - no letter is printed sure while the decoder is still acquiring: HM-REQ-102's gate judged under R78 as step 2's next change against HM-REQ-010/" "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION):" "$f"
done
sed -i 's#<Version>1.13.156</Version>#<Version>1.13.157</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props
grep -c "^## UNIT 470 - STEP 2" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
sh tools/status.sh EXECUTING "0 of 3" code none "Task 0 - record written, version 1.13.156 to 1.13.157; entry round next: build, carry-forward lines, floors, metrics, arbitration, channels, senders, interference"

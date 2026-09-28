#!/bin/sh
# unit 475 task 0 - the record: outcome block from the instruction's ARBITER-DECISION, in both copies; PHASE_STATUS in both copies; the patch bump.
cd /c/Source/HamLet || exit 1
E=.run-unit/unit475-outcome-entry.md
B=.run-unit/unit475-arbiter-block.tmp
sed -n '/^ARBITER-DECISION$/,/^END-ARBITER-DECISION$/p' WORK_INSTRUCTIONS.md > $B
{
  echo ""
  echo "## UNIT 475 - STEP 11"
  echo ""
  grep -E "^(STEP|APPROACH|MOVE|WHY|STATE|DECIDED|LICENCE): " $B
  echo "ACCOMPLISHED: written in output.md at the end of the unit and not claimed here at task 0."
  grep -E "^ADVANCES: " $B
  echo "TICKS_FROM_474: 11.1 the light (TheLightSaysWhatHamletThinksItHearsTests 5/5), 11.2 the strip (TheStripShowsWhereTheDetectorLooksTests 7/7), 11.3 the buttons (TheOwnersVerdictIsARowTests 6/6) - evidenced by unit 474; NOT TICKED because neither copy of PHASE_PLAN.md carries a step 11 section or any 11.k line to tick; raised in output.md section 4."
  echo "RUN: session launched with SESSION.lock already present; the lock is the launcher's and was not taken or released by the session."
} > $E
cut -c1-100 $E
cat $E >> PHASE_OUTCOME.md
[ -f docs/phase-requirements/PHASE_OUTCOME.md ] && cat $E >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  [ -f "$f" ] || continue
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 11/' -e "s/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 475 - the swing bar the owner's ear says is wrong/" "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION|STEP: 11)" "$f" | cut -c1-120
done
sed -i 's#<Version>1.13.160</Version>#<Version>1.13.161</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props
grep -c "^## UNIT 475 - STEP 11" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md

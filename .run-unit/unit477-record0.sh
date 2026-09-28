#!/bin/sh
# unit 477 task 0 - the record: outcome block from the instruction's ARBITER-DECISION in both copies; PHASE_STATUS in both copies; the patch bump; HM-DEC-186 in DECISIONS.md and its CLAUDE.md row; 12.1 to 12.3 ticked from unit 476's work.
cd /c/Source/HamLet || exit 1
E=.run-unit/unit477-outcome-entry.md
B=.run-unit/unit477-arbiter-block.tmp
sed -n '/^ARBITER-DECISION$/,/^END-ARBITER-DECISION$/p' WORK_INSTRUCTIONS.md > $B
{
  echo ""
  echo "## UNIT 477 - STEP 12"
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
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 12/' -e "s/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 477 - bars, not waves/" "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION|STEP: 12)" "$f" | cut -c1-120
done
sed -i 's#<Version>1.13.162</Version>#<Version>1.13.163</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props
grep -c "^## UNIT 477 - STEP 12" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
do
  sed -i -e 's/^- \[ \] 12\.1 /- [x] 12.1 /' -e 's/^- \[ \] 12\.2 /- [x] 12.2 /' -e 's/^- \[ \] 12\.3 /- [x] 12.3 /' "$f"
  grep -n "^- \[.\] 12\.[1-5]" "$f" | cut -c1-60
done
# DECISIONS.md: the new block goes before the first entry's opening --- line
N=$(grep -n -m1 "^---" DECISIONS.md | cut -d: -f1)
[ -n "$N" ] || exit 2
head -n $((N-1)) DECISIONS.md > DECISIONS.md.new
sed 's/$/\r/' .run-unit/unit477-dec186.md >> DECISIONS.md.new
tail -n +$N DECISIONS.md >> DECISIONS.md.new
mv DECISIONS.md.new DECISIONS.md
sed -n "$N,$((N+3))p" DECISIONS.md
# CLAUDE.md section 1: the row goes immediately below the table's separator
H=$(grep -n -m1 "^| Date | Decision | Why | Ref |$" CLAUDE.md | cut -d: -f1)
[ -n "$H" ] || exit 3
S=$((H+1))
sed -n "${S}p" CLAUDE.md
head -n $S CLAUDE.md > CLAUDE.md.new
cat .run-unit/unit477-row186.md >> CLAUDE.md.new
tail -n +$((S+1)) CLAUDE.md >> CLAUDE.md.new
mv CLAUDE.md.new CLAUDE.md
sed -n "$H,$((S+2))p" CLAUDE.md | cut -c1-120

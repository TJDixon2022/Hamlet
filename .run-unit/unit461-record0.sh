#!/bin/sh
# unit 461 task 0 - the record: outcome block in both copies, PHASE_STATUS in both copies, the patch bump.
cd /c/Source/HamLet || exit 1
cat .run-unit/unit461-outcome-entry.md >> PHASE_OUTCOME.md
cat .run-unit/unit461-outcome-entry.md >> docs/phase-requirements/PHASE_OUTCOME.md
for f in PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
do
  sed -i -e 's/^CURRENT_STEP: .*/CURRENT_STEP: 7/' -e "s/^WORK_INSTRUCTION: .*/WORK_INSTRUCTION: 461 - the channel profiles are generated: CH-AWGN and the Watterson CH-* conditions, proved on their own audio/" "$f"
  grep -n -E "^(CURRENT_STEP|WORK_INSTRUCTION):" "$f"
done
sed -i 's#<Version>1.13.147</Version>#<Version>1.13.148</Version>#' Directory.Build.props
grep -n "<Version>" Directory.Build.props
grep -c "^## UNIT 461 - STEP 7" PHASE_OUTCOME.md docs/phase-requirements/PHASE_OUTCOME.md
echo "== plan lines"
grep -n -E "^- \[.\] (2\.[1-5]|7\.[1-5]) " PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md | cut -c1-80
echo "== generator"
ls tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/
grep -rln -E "CH-(AWGN|LM|MM|HM|LQ|LD|MQ|MD|HQ|HD)|Watterson" tests src | head

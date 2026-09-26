#!/bin/sh
# unit 449 - section 2's checks of the instruction against the tree.
cd /c/Source/HamLet || exit 1
echo "--- HEAD"
git rev-parse --short HEAD
echo "--- step 2 ticks"
grep -n "^- \[.\] 2\.[0-9]" PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md | cut -c1-600
echo "--- files"
ls tests/Hamlet.RadioEngine.Tests/Cw/WhereTheSureWrongLettersComeFromTests.cs tests/Hamlet.RadioEngine.Tests/Cw/Instruments/CwPitchInstrument.cs
git ls-files | grep "CwMetrics.cs"
echo "--- PARKED header"
head -30 PARKED.md
echo "--- requirements"
grep -n "HM-REQ-01[0-5]" CW_REQUIREMENTS.md | cut -c1-400
echo "--- V-11"
grep -n "V-11" CW_SPEC.md | head -3 | cut -c1-400
echo "--- no-kept count in PHASE_OUTCOME"
grep -n -i "no kept change\|kept nothing\|count of units" PHASE_OUTCOME.md | tail -20 | cut -c1-300

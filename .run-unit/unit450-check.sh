#!/bin/sh
# unit 450 - section 2's checks of the instruction against the tree.
cd /c/Source/HamLet || exit 1
echo "--- HEAD"
git rev-parse --short HEAD
echo "--- step 4 ticks"
grep -n "^- \[.\] 4\.[0-9]" PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md | cut -c1-400
echo "--- report fields"
grep -rn "PitchWasMeasured\|PitchWasAsserted" src --include=*.cs | cut -c1-260
echo "--- HasMeasuredPitch"
grep -rn "HasMeasuredPitch" src --include=*.cs | cut -c1-260
echo "--- CwPitchChoice and ToneForTheRecord"
git ls-files | grep "CwPitchChoice\|CwPitchInstrument\|WhereTheSureWrongLettersComeFrom\|WhatPitchTheDecoderIsOn"
grep -rn "ToneForTheRecord" src --include=*.cs | cut -c1-200
echo "--- traceability"
grep -n "093" docs/phase-requirements/traceability.md | cut -c1-600
echo "--- PARKED header"
head -12 PARKED.md
echo "--- outcome header"
head -30 PHASE_OUTCOME.md | cut -c1-200
echo "--- metric coverage in spec"
grep -n "MET-COVERAGE" CW_SPEC.md | cut -c1-200
grep -n "CPS-DEC-0183\|HM-DEC-165" CLAUDE.md | head -5 | cut -c1-200
echo "--- status"
grep -n "CURRENT_STEP\|WORK_INSTRUCTION\|VERSION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props

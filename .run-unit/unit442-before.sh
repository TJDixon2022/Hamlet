#!/bin/sh
# unit 442 - run one printer type with the decoder as it stood before 441's two kept changes
# (f14b2453, 441's entry), then put HEAD's files back. Nothing is committed from the old state.
# Usage: sh .run-unit/unit442-before.sh <outname> "<n of 3>" "<filter>" <timeout-s>
cd /c/Source/HamLet || exit 1
FILES="src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs src/Hamlet.RadioEngine/Cw/CwUnitEstimator.cs tests/Hamlet.RadioEngine.Tests/Cw/WhereTheSureWrongLettersComeFromTests.cs"
git checkout f14b2453 -- $FILES || exit 1
sh .run-unit/unit442-build.sh "before-$1" "$2" "Floors: building the decoder as it stood at f14b2453"
sh .run-unit/unit442-run.sh "$1" engine "$4" "$2" "Floors: printing at f14b2453's decoder" "$3" --no-build
git checkout HEAD -- $FILES
git status --short -- src tests
sh .run-unit/unit442-build.sh "restored-$1" "$2" "Floors: rebuilding HEAD after the before run"

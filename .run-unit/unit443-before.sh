#!/bin/sh
# unit 443 - run the added-character trace with the decoder's src as it stood at an earlier commit,
# as unit 442 did, then put HEAD's files back and rebuild. Nothing is committed from the old state.
# WhereTheSureWrongLettersComeFromTests reads CwUnitEstimator.MarkUnit, which 441 added, so it is
# taken from f14b2453 for the run and put back after, exactly as 442's before-run did.
# Usage: sh .run-unit/unit443-before.sh <commit> "<n of 3>"
cd /c/Source/HamLet || exit 1
C=$1
echo "src differences 1fb0bad6 to f14b2453:"
git diff --stat 1fb0bad6 f14b2453 -- src | tail -1
git checkout "$C" -- src/Hamlet.RadioEngine/Cw || exit 1
git checkout f14b2453 -- tests/Hamlet.RadioEngine.Tests/Cw/WhereTheSureWrongLettersComeFromTests.cs || exit 1
git status --short -- src tests
sh .run-unit/unit443-added.sh "$C" "$2" "Task 1 with the decoder at $C"
git checkout HEAD -- src/Hamlet.RadioEngine/Cw tests/Hamlet.RadioEngine.Tests/Cw/WhereTheSureWrongLettersComeFromTests.cs
echo "after restore:"
git status --short -- src tests
sh .run-unit/unit443-build.sh "restored-$C" "$2" "Task 1: rebuilding HEAD after the $C run"

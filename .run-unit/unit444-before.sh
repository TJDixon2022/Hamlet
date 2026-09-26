#!/bin/sh
# unit 444 - run the 17:37 gap printer and the metrics with the decoder's src as it stood at f14b2453,
# the commit before G1, as units 442 and 443 did, then put HEAD's files back and rebuild.
# WhereTheSureWrongLettersComeFromTests reads CwUnitEstimator.MarkUnit, which 441 added, so it is
# taken from f14b2453 for the run and put back after, exactly as 443's before-run did.
cd /c/Source/HamLet || exit 1
git checkout f14b2453 -- src/Hamlet.RadioEngine/Cw || exit 1
git checkout f14b2453 -- tests/Hamlet.RadioEngine.Tests/Cw/WhereTheSureWrongLettersComeFromTests.cs || exit 1
git status --short -- src tests
sh .run-unit/unit444-gaps.sh f14b2453 "1 of 3" "Task 1 with the decoder at f14b2453"
sh .run-unit/unit444-round.sh metrics f14b2453 "1 of 3"
git checkout HEAD -- src/Hamlet.RadioEngine/Cw tests/Hamlet.RadioEngine.Tests/Cw/WhereTheSureWrongLettersComeFromTests.cs
echo "after restore:"
git status --short -- src tests
sh .run-unit/unit444-build.sh restored-f14b2453 "1 of 3" "Task 1: rebuilding HEAD after the f14b2453 run"

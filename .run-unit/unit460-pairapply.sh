#!/bin/sh
# unit 460 task 2 - 6a0b65a1's src diff into the working tree only, build, the pair trace and our texts, then src back from HEAD.
cd /c/Source/HamLet || exit 1
git show 6a0b65a1 -- src > .run-unit/unit460-pair-diff.patch
git apply .run-unit/unit460-pair-diff.patch || exit 1
git diff --stat -- src
sh .run-unit/unit460-build.sh t2diff "2 of 4" "Task 2 - 6a0b65a1's pair speed in the working tree only; building, then the trace and every recording's text with it"
sh .run-unit/unit460-run.sh pair-diff engine 600 "2 of 4" "Task 2 - pair trace with 459's pair speed in the working tree" "FullyQualifiedName~.WhereThePairSpeedMovedTheUnitTests." --no-build
sh .run-unit/unit460-textsave.sh pairdiff "2 of 4" "Task 2 - every recording's text with 459's pair speed in the working tree" before
git checkout -- src
echo "== git diff -- src after the restore (empty):"
git diff -- src
echo "== end"

#!/bin/sh
# unit 449 - the narrowed test at HEAD: set the tracker change aside, build, run, put it back, build.
cd /c/Source/HamLet || exit 1
T=src/Hamlet.RadioEngine/Cw/CwToneTracker.cs
git diff -- "$T" > .run-unit/unit449-change.diff
git checkout -- "$T"
sh .run-unit/unit449-build.sh test-head2 "2 of 3" "Change: the narrowed test at HEAD, change set aside"
sh .run-unit/unit449-run.sh test-head2 engine 300 "2 of 3" "Change: the narrowed HM-REQ-010 test at HEAD, expected red" "FullyQualifiedName~.TheTrackerStaysWithTheStationItReadsTests." --no-build
grep -a "HM-REQ-010 |" .run-unit/unit449-test-head2.txt | head -1 | cut -c1-500
git apply .run-unit/unit449-change.diff
git diff --stat -- "$T"
sh .run-unit/unit449-build.sh change3 "2 of 3" "Change: tracker change back in; building"

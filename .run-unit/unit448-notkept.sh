#!/bin/sh
# unit 448 - the refused change: its diff with the test into .run-unit, src and tests back to task 1's commit.
cd /c/Source/HamLet || exit 1
T=tests/Hamlet.RadioEngine.Tests/Cw/TheFilterGoesToTheKeyingFromColdTests.cs
git add -N -- "$T"
git diff -- src/Hamlet.RadioEngine/Cw/CwToneTracker.cs src/Hamlet.RadioEngine/Cw/CwToneSurvey.cs "$T" > .run-unit/unit448-coldmove-notkept.diff
git reset -q -- "$T"
mv "$T" .run-unit/unit448-TheFilterGoesToTheKeyingFromColdTests.cs.txt
git checkout -- src/Hamlet.RadioEngine/Cw/CwToneTracker.cs src/Hamlet.RadioEngine/Cw/CwToneSurvey.cs
wc -l .run-unit/unit448-coldmove-notkept.diff
git status --short -- src tests

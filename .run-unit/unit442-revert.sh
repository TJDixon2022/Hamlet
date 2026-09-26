#!/bin/sh
# unit 442 - keep the not-kept confidence change as a diff under .run-unit, put src back to HEAD, rebuild.
cd /c/Source/HamLet || exit 1
git diff -- src > .run-unit/unit442-dim-notkept.diff
wc -l .run-unit/unit442-dim-notkept.diff
git checkout HEAD -- src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs
git status --short -- src tests
sh .run-unit/unit442-build.sh reverted "2 of 3" "Confidence: not kept (coverage 374 to 358); src back to the trace commit"

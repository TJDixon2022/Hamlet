#!/bin/sh
# unit 446 - save the refused change's diff, take it out of src, and show src is back at HEAD.
cd /c/Source/HamLet || exit 1
git diff -- src > .run-unit/unit446-speed-notkept.diff
wc -l .run-unit/unit446-speed-notkept.diff
git diff --stat -- src
git checkout -- src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs src/Hamlet.RadioEngine/Cw/CwDecoder.cs
git status --short -- src
echo "src diff lines now: $(git diff -- src | wc -l)"

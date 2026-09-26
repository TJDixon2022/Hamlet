#!/bin/sh
# unit 443 - keep the not-kept re-read change as a diff under .run-unit, put src back to HEAD, print texts, compare.
cd /c/Source/HamLet || exit 1
git diff -- src > .run-unit/unit443-added-notkept.diff
wc -l .run-unit/unit443-added-notkept.diff
git checkout HEAD -- src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs
git status --short -- src tests
sh .run-unit/unit443-text.sh head "2 of 3" "Task 2: re-read rule not kept, src back to HEAD"
diff .run-unit/unit443-text-head.sorted.txt .run-unit/unit443-text-change.sorted.txt > .run-unit/unit443-text-diff.txt
grep -c "^<" .run-unit/unit443-text-diff.txt

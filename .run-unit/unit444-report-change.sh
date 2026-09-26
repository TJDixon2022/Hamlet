#!/bin/sh
# unit 444 - under the not-kept rule: captures and named floors reported, every recording's text printed;
# then keep the diff under .run-unit, put src back to HEAD, print HEAD's texts and compare.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit444-round.sh captures change "2 of 3"
sh .run-unit/unit444-round.sh named change "2 of 3"
sh .run-unit/unit444-text.sh change "2 of 3" "Task 2 under the rule"
git diff -- src > .run-unit/unit444-wbe-notkept.diff
wc -l .run-unit/unit444-wbe-notkept.diff
git checkout HEAD -- src/Hamlet.RadioEngine/Cw/CwUnitEstimator.cs
git status --short -- src tests
sh .run-unit/unit444-text.sh head "2 of 3" "Task 2: rule not kept, src back to HEAD"
diff .run-unit/unit444-text-head.sorted.txt .run-unit/unit444-text-change.sorted.txt > .run-unit/unit444-text-diff.txt
grep -c "^<" .run-unit/unit444-text-diff.txt
grep -E "^\s+Failed " .run-unit/unit444-captures-change.txt .run-unit/unit444-named-change.txt | cut -c1-250

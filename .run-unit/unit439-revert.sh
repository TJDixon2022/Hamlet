#!/bin/sh
# unit 439 - put CwUnitEstimator.cs back exactly as it stood at this unit's entry (9db74ab2), and show the diff is empty.
cd /c/Source/HamLet || exit 1
git show 9db74ab2:src/Hamlet.RadioEngine/Cw/CwUnitEstimator.cs > src/Hamlet.RadioEngine/Cw/CwUnitEstimator.cs
git diff --stat 9db74ab2 -- src data
echo "src and data against entry: $(git diff 9db74ab2 -- src data | wc -l) diff lines"

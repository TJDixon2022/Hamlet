#!/bin/sh
# unit 438 - put CwUnitEstimator.cs back exactly as it stood at this unit's entry (755373d2), and show the diff is empty.
cd /c/Source/HamLet || exit 1
git show 755373d2:src/Hamlet.RadioEngine/Cw/CwUnitEstimator.cs > src/Hamlet.RadioEngine/Cw/CwUnitEstimator.cs
git diff --stat 755373d2 -- src data
echo "src and data against entry: $(git diff 755373d2 -- src data | wc -l) diff lines"

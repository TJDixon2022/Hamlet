#!/bin/sh
# unit 443 - read what the tree held at 1fb0bad6, changing nothing.
cd /c/Source/HamLet || exit 1
git ls-tree -r --name-only 1fb0bad6 tests/Hamlet.RadioEngine.Tests/Cw | grep -i "Requirement\|Stray\|Metric\|Captured\|Adjudicated\|SureWrong"
echo "--- CwUnitEstimator at 1fb0bad6"
git show 1fb0bad6:src/Hamlet.RadioEngine/Cw/CwUnitEstimator.cs | grep -n "public \|internal " | head -30
echo "--- CwCharacter at 1fb0bad6"
git show 1fb0bad6:src/Hamlet.RadioEngine/Cw/CwCharacter.cs | grep -n "public \|internal " | head -40
echo "--- metrics type at 1fb0bad6"
git show 1fb0bad6:tests/Hamlet.RadioEngine.Tests/Cw/TheRequirementsAreMeasuredTests.cs | grep -n "internal static\|public void\|Covered" | head -30
echo "--- folder"
grep -rn "Folder =" tests/Hamlet.RadioEngine.Tests/Cw/CapturedSignalTests.cs
git worktree list

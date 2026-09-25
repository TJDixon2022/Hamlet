#!/bin/sh
# unit 432 task 0 - section 5's checks against the tree that need no test run.
cd /c/Source/HamLet || exit 1
echo "== CwDecoder.cs 600 to 603 and 617 to 621"
sed -n "600,603p" src/Hamlet.RadioEngine/Cw/CwDecoder.cs
sed -n "617,621p" src/Hamlet.RadioEngine/Cw/CwDecoder.cs
echo "== the four commits"
for c in 5b6b704c a7e6e2f2 ec76051e fa64edc5
do
  git log --oneline -1 $c
done
echo "== does 5b6b704c re-apply to HEAD"
git diff 5b6b704c~1 5b6b704c -- src > .run-unit/unit432-5b6b.patch
git apply --check .run-unit/unit432-5b6b.patch && echo "5b6b704c applies cleanly to HEAD"
echo "== CwDecoder.cs commits since a7e6e2f2"
git log --oneline a7e6e2f2..HEAD -- src/Hamlet.RadioEngine/Cw/CwDecoder.cs
echo "== CwDecoder.cs at HEAD against a7e6e2f2"
git diff --stat a7e6e2f2 HEAD -- src/Hamlet.RadioEngine/Cw/CwDecoder.cs
echo "== WhyTheMixMoved"
grep -n "WhyTheMixMoved\|Verdict" tests/Hamlet.RadioEngine.Tests/Cw/WhatTheOpeningHeardTests.cs | head -20
echo "== 032113 in the captures type"
grep -rn "032113" tests/Hamlet.RadioEngine.Tests/Cw/TheCapturesThatDecodeKeepDecodingTests.cs | head
grep -rn "032113" tests/Hamlet.RadioEngine.Tests/Cw/TheNumberCannotBeGamedTests.cs | head
echo "== version"
grep -n "<Version>" Directory.Build.props
echo "== git status of root files"
git status --short -- PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md

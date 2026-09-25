#!/bin/sh
# unit 433 task 0 - section 5's checks against the tree that need no test run.
cd /c/Source/HamLet || exit 1
C=src/Hamlet.RadioEngine/Cw
echo "== CwDecoder.cs 596 to 625"
sed -n "596,625p" $C/CwDecoder.cs
echo "== CwDecoder.cs at HEAD against a7e6e2f2"
git diff --stat a7e6e2f2 HEAD -- $C/CwDecoder.cs
echo "== Envelope and IntegratorBandwidthHz"
grep -n "IntegratorBandwidthHz\|static .*Envelope(" $C/CwProbabilisticDecoder.cs
echo "== CwToneSurvey seconds"
grep -n "seconds = \|double seconds" $C/CwToneSurvey.cs
echo "== the two printers"
grep -n "public void WhyTheMixMoved\|public void WhenEachRuleFollows\|\[Fact\|\[Theory" tests/Hamlet.RadioEngine.Tests/Cw/WhatTheOpeningHeardTests.cs
git log --oneline -1 f0845a92 -- tests/Hamlet.RadioEngine.Tests/Cw/WhatTheOpeningHeardTests.cs
echo "== 032113 in the captures and floors"
grep -rn "032113" tests/Hamlet.RadioEngine.Tests/Cw/TheCapturesThatDecodeKeepDecodingTests.cs | head -5
grep -rn "032113" tests/Hamlet.RadioEngine.Tests/Cw/TheNumberCannotBeGamedTests.cs | head -5
echo "== version"
grep -n "<Version>" Directory.Build.props
echo "== PHASE_STATUS head"
head -20 PHASE_STATUS.md
echo "== git status of root files"
git status --short -- PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md
echo "== PARKED tail ids"
grep -n "^## P4\|^### P4\|^\*\*P4" docs/phase-correctness/PARKED.md | tail -12

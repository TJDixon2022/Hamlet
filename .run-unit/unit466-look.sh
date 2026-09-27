#!/bin/sh
# unit 466 - read-only lookups in the tree, rewritten as needed.
cd /c/Source/HamLet || exit 1
grep -a "live |" .run-unit/unit465-live-t3.txt | sed 's/^ *//' | cut -c1-200
echo "== 083 tests"
grep -rln "HM-REQ-083" tests | head
echo "== metric step"
grep -n "record struct CwMetricStep" -A 8 tests/Hamlet.RadioEngine.Tests/Cw/CwMetrics.cs
sed -n 44,58p tests/Hamlet.RadioEngine.Tests/Cw/CwMetrics.cs
echo "== named words"
grep -n "Fact\|void \|WEEKEND\|internal static\|class " tests/Hamlet.RadioEngine.Tests/Cw/WhatTheNamedWordsReadTests.cs | head -40

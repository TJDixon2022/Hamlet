#!/bin/sh
# unit 451 - HM-REQ-036's final test at the entry code f96cd04e, in a separate worktree, then the worktree removed.
# Usage: sh .run-unit/unit451-athead.sh
cd /c/Source/HamLet || exit 1
W=/c/Source/HamLet-unit451-head
T=ARefinementKeepsTheTimingTests
OUT=.run-unit/unit451-athead-$T.txt
sh tools/status.sh EXECUTING "3 of 3" code none "Exit round: HM-REQ-036's final cases at the entry code in a worktree"
git worktree add --detach "$W" f96cd04e > "$OUT" 2>&1
cp tests/Hamlet.RadioEngine.Tests/Cw/$T.cs "$W/tests/Hamlet.RadioEngine.Tests/Cw/$T.cs"
START=$(date +%s)
timeout 900 dotnet test "$W/tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj" --logger "console;verbosity=detailed" --filter "FullyQualifiedName~.$T." >> "$OUT" 2>&1
echo "RC=$? WALL=$(( $(date +%s) - START ))s" | tee -a "$OUT"
grep -E "^\s+Failed |Total tests|Passed:|Failed:" "$OUT" | cut -c1-200
grep -a "^ *HM-REQ-036 |" "$OUT" | grep -c "green"
grep -a "^ *HM-REQ-036 |" "$OUT" | grep "RED"
git worktree remove --force "$W"
git worktree list

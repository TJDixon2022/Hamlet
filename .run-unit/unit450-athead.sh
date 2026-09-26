#!/bin/sh
# unit 450 - one engine type at the entry HEAD 706e3874, in a separate worktree, then the worktree removed.
# Usage: sh .run-unit/unit450-athead.sh <TypeName>
cd /c/Source/HamLet || exit 1
W=/c/Source/HamLet-unit450-head
OUT=.run-unit/unit450-athead-$1.txt
sh tools/status.sh EXECUTING "3 of 3" code none "Exit round: $1 at the entry HEAD in a worktree, to show its reds are not this unit's"
git worktree add --detach "$W" 706e3874 > "$OUT" 2>&1
START=$(date +%s)
timeout 900 dotnet test "$W/tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj" --filter "FullyQualifiedName~.$1." >> "$OUT" 2>&1
echo "RC=$? WALL=$(( $(date +%s) - START ))s" | tee -a "$OUT"
grep -E "^\s+Failed |Total tests|Passed:|Failed:" "$OUT" | cut -c1-200
git worktree remove --force "$W"
git worktree list

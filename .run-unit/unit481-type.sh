#!/bin/sh
# unit 481 - one named app test type, one invocation, its own timeout (HM-DEC-155); no recording is read by the types it is given.
# Usage: sh .run-unit/unit481-type.sh <TypeName> <out-suffix> <timeout-seconds> [build]
cd /c/Source/HamLet || exit 1
PROJ=tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj
OUT=.run-unit/unit481-t-$2.txt
NB=--no-build
[ "$4" = "build" ] && NB=
START=$(date +%s)
timeout "$3" dotnet test "$PROJ" $NB --logger "console;verbosity=detailed" --filter "FullyQualifiedName~.$1." > "$OUT" 2>&1
RC=$?
echo "RC=$RC WALL=$(( $(date +%s) - START ))s" | tee -a "$OUT"
grep -a -E "error [A-Z]+[0-9]+" "$OUT" | sort -u | head -10
grep -a -E "^\s+(Passed|Failed) " "$OUT" | sed -E "s/^\s+//" | cut -c1-160
grep -a -E "Total tests|^\s+Passed: |^\s+Failed: " "$OUT" | tail -3

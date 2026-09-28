#!/bin/sh
# unit 480 - the app carry-forward line only (R88: no engine line, no floors, nothing reading the CW corpus).
# The filter is read from docs/carry-forward-tests.txt.
# Usage: sh .run-unit/unit480-cf.sh <suffix> "<n of 4>" "<note>"
cd /c/Source/HamLet || exit 1
PROJ=tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj
FILTER=$(grep -m1 "dotnet test $PROJ --filter" docs/carry-forward-tests.txt | sed -e 's/^.*--filter "//' -e 's/"[[:space:]]*$//')
[ -n "$FILTER" ] || { echo "no filter read"; exit 2; }
OUT=.run-unit/unit480-cf-app-$1.txt
sh .run-unit/unit480-status.sh EXECUTING "$2" code none "$3" > /dev/null
START=$(date +%s)
timeout 600 dotnet test "$PROJ" --no-build --logger "console;verbosity=detailed" --filter "$FILTER" > "$OUT" 2>&1
RC=$?
echo "RC=$RC WALL=$(( $(date +%s) - START ))s" | tee -a "$OUT"
grep -a -E "^\s+Failed " "$OUT" | sed -E "s/^\s+//" | cut -c1-200 | head -20
grep -a -c "dispatcher loop" "$OUT"
grep -a -E "^(Passed!|Failed!)|Total tests|^\s+Passed: |^\s+Failed: " "$OUT" | tail -4

#!/bin/sh
# unit 461 - one carry-forward line, the filter read from docs/carry-forward-tests.txt, foregrounded, 600 s cap, status first.
# tools/run-carry-forward.sh carries an older, shorter list (133 engine tests); the document's list is the line (178 engine, 278 app).
# Usage: sh .run-unit/unit464-cf.sh <engine|app> <suffix> "<n of 4>" "<note>"
cd /c/Source/HamLet || exit 1
case "$1" in
  engine) PROJ=tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj ;;
  app) PROJ=tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj ;;
  *) echo "engine or app"; exit 2 ;;
esac
FILTER=$(grep -m1 "dotnet test $PROJ --filter" docs/carry-forward-tests.txt | sed -e 's/^.*--filter "//' -e 's/"[[:space:]]*$//')
[ -n "$FILTER" ] || { echo "no filter read"; exit 2; }
OUT=.run-unit/unit464-cf-$1-$2.txt
sh tools/status.sh EXECUTING "$3" code none "$4"
START=$(date +%s)
timeout 600 dotnet test "$PROJ" --no-build --logger "console;verbosity=detailed" --filter "$FILTER" > "$OUT" 2>&1
RC=$?
echo "RC=$RC WALL=$(( $(date +%s) - START ))s" | tee -a "$OUT"
grep -a -E "^\s+Failed " "$OUT" | sed -E "s/^\s+//" | cut -c1-200 | head -20
grep -a -c "dispatcher loop" "$OUT"
grep -a -E "^(Passed!|Failed!)|Total tests|^\s+Passed: |^\s+Failed: " "$OUT" | tail -4

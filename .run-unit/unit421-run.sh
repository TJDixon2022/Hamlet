#!/bin/sh
# unit 421 - one dotnet test invocation, one type, status written immediately before it.
# Usage: sh .run-unit/unit421-run.sh <outname> <engine|app> <timeout-s> "<TASK n of 4>" "<note>" "<filter>" [--no-build]
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit421-$1.txt
case "$2" in
  engine) PROJ=tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj ;;
  app) PROJ=tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj ;;
esac
sh tools/status.sh EXECUTING "$4" code none "$5"
START=$(date +%s)
timeout "$3" dotnet test "$PROJ" $7 --logger "console;verbosity=detailed" --filter "$6" > "$OUT" 2>&1
RC=$?
END=$(date +%s)
echo "RC=$RC WALL=$((END-START))s" >> "$OUT"
echo "RC=$RC WALL=$((END-START))s"
grep -E "^\s+Failed " "$OUT" | sed -E "s/^\s+//" | cut -c1-200
grep -E "^(Passed!|Failed!)|Total tests|Passed:|Failed:|error CS" "$OUT" | tail -6

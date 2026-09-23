#!/bin/sh
# unit 404 - one dotnet test invocation, status written immediately before it.
# Usage: sh .run-unit/unit404-run.sh <outname> <engine|app> <timeout-s> "<task>" "<note>" "<filter>" [--no-build]
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit404-$1.txt
case "$2" in
  engine) PROJ=tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj ;;
  app) PROJ=tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj ;;
esac
sh tools/status.sh ACTIVE "$4" claude "none" "$5"
START=$(date +%s)
timeout "$3" dotnet test "$PROJ" $7 --logger "console;verbosity=detailed" --filter "$6" > "$OUT" 2>&1
RC=$?
END=$(date +%s)
echo "RC=$RC WALL=$((END-START))s" >> "$OUT"
echo "RC=$RC WALL=$((END-START))s"
grep -E "^(Passed!|Failed!)|Total tests|Passed:|Failed:|error CS|warning CS" "$OUT" | tail -8

#!/bin/sh
# unit 474 - one dotnet test invocation, one app type, status written immediately before it.
# Usage: sh .run-unit/unit474-run.sh <outname> <timeout-s> "<n of 4>" "<note>" <TypeName> [--no-build]
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit474-$1.txt
PROJ=tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj
sh .run-unit/unit474-status.sh EXECUTING "$3" code none "$4" > /dev/null
START=$(date +%s)
timeout "$2" dotnet test "$PROJ" $6 --logger "console;verbosity=detailed" --filter "FullyQualifiedName~.$5." > "$OUT" 2>&1
RC=$?
END=$(date +%s)
echo "RC=$RC WALL=$((END-START))s" >> "$OUT"
echo "RC=$RC WALL=$((END-START))s"
grep -E "^\s+(Failed|Passed) " "$OUT" | sed -E "s/^\s+//" | cut -c1-200
grep -E "error CS|Assert\.|Expected:|Actual:" "$OUT" | sort -u | head -12
grep -E "^(Passed!|Failed!)|Total tests|Passed:|Failed:" "$OUT" | tail -4

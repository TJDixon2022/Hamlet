#!/bin/sh
# unit 475 - one dotnet test invocation, one named type, status written immediately before it.
# Usage: sh .run-unit/unit475-run.sh <outname> <timeout-s> "<n of 3>" "<note>" <TypeName> <app|engine> [--no-build]
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit475-$1.txt
PROJ=tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj
[ "$6" = "engine" ] && PROJ=tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
sh .run-unit/unit475-status.sh EXECUTING "$3" code none "$4" > /dev/null
START=$(date +%s)
timeout "$2" dotnet test "$PROJ" $7 --logger "console;verbosity=detailed" --filter "FullyQualifiedName~.$5." > "$OUT" 2>&1
RC=$?
END=$(date +%s)
echo "RC=$RC WALL=$((END-START))s" >> "$OUT"
echo "RC=$RC WALL=$((END-START))s"
grep -E "^\s+(Failed|Passed) " "$OUT" | sed -E "s/^\s+//" | cut -c1-200
grep -E "error CS|Assert\.|Expected:|Actual:|Keying at|NoKeying at|Listening at" "$OUT" | sort -u | head -12
grep -E "^(Passed!|Failed!)|Total tests|Passed:|Failed:" "$OUT" | tail -4

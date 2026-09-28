#!/bin/sh
# unit 480 - one named test type, one invocation, its own timeout (HM-DEC-155); no recording is read by the types it is given.
# Usage: sh .run-unit/unit480-type.sh <engine|app> <TypeName> <out-suffix> <timeout-seconds> [build]
cd /c/Source/HamLet || exit 1
case "$1" in
  engine) PROJ=tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj ;;
  app) PROJ=tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj ;;
  *) echo "engine or app"; exit 2 ;;
esac
OUT=.run-unit/unit480-t-$3.txt
NB=--no-build
[ "$5" = "build" ] && NB=
START=$(date +%s)
timeout "$4" dotnet test "$PROJ" $NB --logger "console;verbosity=detailed" --filter "FullyQualifiedName~.$2." > "$OUT" 2>&1
RC=$?
echo "RC=$RC WALL=$(( $(date +%s) - START ))s" | tee -a "$OUT"
grep -a -E "error [A-Z]+[0-9]+" "$OUT" | sort -u | head -10
grep -a -E "^\s+(Passed|Failed) " "$OUT" | sed -E "s/^\s+//" | cut -c1-160
grep -a -E "^\s*(key down|keyed:|noise:|chunks read|tone over|first keying|carrier:|longest|swing|meter)" "$OUT" | cut -c1-240 | head -30
grep -a -E "Total tests|^\s+Passed: |^\s+Failed: " "$OUT" | tail -3

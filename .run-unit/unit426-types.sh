#!/bin/sh
# unit 426 - app test types one per invocation, --no-build, a summary line each.
# Usage: sh .run-unit/unit426-types.sh <suffix> "<TASK n of 4>" <Type>...
cd /c/Source/HamLet || exit 1
SUF=$1
TK=$2
shift 2
N=0
for T in "$@"
do
  N=$((N+1))
  OUT=.run-unit/unit426-type-$SUF-$T.txt
  sh tools/status.sh EXECUTING "$TK" code none "Touched types round $SUF: $T, $N of $#"
  START=$(date +%s)
  timeout 300 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --no-build --logger "console;verbosity=detailed" --filter "FullyQualifiedName~.$T." > "$OUT" 2>&1
  RC=$?
  WALL=$(( $(date +%s) - START ))
  echo "RC=$RC WALL=${WALL}s" >> "$OUT"
  P=$(grep -E "^\s+Passed:" "$OUT" | tail -1 | tr -dc 0-9)
  F=$(grep -E "^\s+Failed:" "$OUT" | tail -1 | tr -dc 0-9)
  TOT=$(grep -E "^Total tests:" "$OUT" | tail -1 | tr -dc 0-9)
  echo "$T rc=$RC passed=${P:-0} failed=${F:-0} total=${TOT:-0} ${WALL}s"
  grep -E "^\s+Failed " "$OUT" | sed -E "s/^\s+//" | cut -c1-200
done

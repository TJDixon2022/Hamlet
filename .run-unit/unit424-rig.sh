#!/bin/sh
# unit 424 - every Rig test type, one type per invocation, --no-build after one build.
# Usage: sh .run-unit/unit424-rig.sh <suffix> "<TASK n of 4>" <glob, e.g. [A-O]>
cd /c/Source/HamLet || exit 1
SUM=.run-unit/unit424-rig-$1.txt
touch "$SUM"
for f in tests/Hamlet.RadioEngine.Tests/Rig/$3*Tests.cs
do
  T=$(basename "$f" .cs)
  OUT=.run-unit/unit424-rig-$1-$T.txt
  sh tools/status.sh EXECUTING "$2" code none "Rig round $1 - running $T alone"
  timeout 180 dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --no-build --logger "console;verbosity=normal" --filter "FullyQualifiedName~Hamlet.RadioEngine.Tests.Rig.$T." > "$OUT" 2>&1
  RC=$?
  LINE=$(grep -E "^(Passed!|Failed!)" "$OUT" | tail -1)
  echo "$T RC=$RC $LINE" | tee -a "$SUM"
  grep -E "^\s+Failed " "$OUT" | cut -c1-200 | tee -a "$SUM"
done

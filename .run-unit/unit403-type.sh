#!/bin/sh
# Usage: sh .run-unit/unit403-type.sh <project: engine|app> <filter> <outfile> <timeout-s> <note> <task-line>
# One type per invocation, foreground, its own timeout. Status written immediately before the run.
cd /c/Source/HamLet
if [ "$1" = "app" ]; then
  PROJ=tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj
else
  PROJ=tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
fi
sh tools/status.sh EXECUTING "$6" code none "$5" > /dev/null
START=$(date +%s)
timeout "$4" dotnet test "$PROJ" --no-build --filter "$2" --logger "console;verbosity=detailed" > "$3" 2>&1
RC=$?
END=$(date +%s)
echo "rc=$RC elapsed=$((END - START))s"
grep -E "^(Passed!|Failed!)|Total tests|Passed:|Failed:" "$3" | tail -4

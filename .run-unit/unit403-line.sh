#!/bin/sh
# Usage: sh .run-unit/unit403-line.sh <app|engine> <outfile> <note> <task-line>
# Runs one carry-forward line verbatim as docs/carry-forward-tests.txt prints it.
cd /c/Source/HamLet
if [ "$1" = "app" ]; then
  KEY=Hamlet.App.Tests.csproj
else
  KEY=Hamlet.RadioEngine.Tests.csproj
fi
LINE=$(grep -m1 "timeout 480 dotnet test tests/.*/$KEY --filter" docs/carry-forward-tests.txt | sed 's/^ *//')
sh tools/status.sh EXECUTING "$4" code none "$3" > /dev/null
START=$(date +%s)
eval "$LINE" > "$2" 2>&1
RC=$?
END=$(date +%s)
echo "rc=$RC elapsed=$((END - START))s"
grep -E "^(Passed!|Failed!)|Total tests|Passed:|Failed:|^  Failed " "$2" | tail -12

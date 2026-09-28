#!/bin/sh
# unit 480 task 4 - full no-incremental build of Hamlet.sln with warnings as errors, status first.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit480-status.sh EXECUTING "4 of 4" code none "Task 4 exit round: full no-incremental build of Hamlet.sln with warnings as errors" > /dev/null
START=$(date +%s)
timeout 580 dotnet build Hamlet.sln --no-incremental -warnaserror -nologo -v q > .run-unit/unit480-build-exit.txt 2>&1
RC=$?
echo "RC=$RC WALL=$(( $(date +%s) - START ))s" | tee -a .run-unit/unit480-build-exit.txt
grep -E "Warning\(s\)|Error\(s\)" .run-unit/unit480-build-exit.txt
grep -E "error [A-Z]+[0-9]+" .run-unit/unit480-build-exit.txt | sort -u | head -20

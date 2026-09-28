#!/bin/sh
# unit 480 (letter over the bars) task 3 - full no-incremental build of Hamlet.sln with warnings as errors, status first.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit480-status.sh EXECUTING "3 of 3" code none "Task 3 exit round: full no-incremental build of Hamlet.sln with warnings as errors" > /dev/null
START=$(date +%s)
timeout 580 dotnet build Hamlet.sln --no-incremental -warnaserror -nologo -v q > .run-unit/unit480-build-r2-exit.txt 2>&1
RC=$?
echo "RC=$RC WALL=$(( $(date +%s) - START ))s" | tee -a .run-unit/unit480-build-r2-exit.txt
grep -E "Warning\(s\)|Error\(s\)" .run-unit/unit480-build-r2-exit.txt
grep -E "error [A-Z]+[0-9]+" .run-unit/unit480-build-r2-exit.txt | sort -u | head -20

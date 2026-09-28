#!/bin/sh
# unit 478 task 3 - Hamlet.sln rebuilt from scratch with warnings as errors, status first.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit478-status.sh EXECUTING "3 of 3" code none "Task 3 exit round - full non-incremental build of Hamlet.sln with warnings as errors" > /dev/null
START=$(date +%s)
timeout 580 dotnet build Hamlet.sln -warnaserror --no-incremental -nologo -v q > .run-unit/unit478-build-exit.txt 2>&1
RC=$?
echo "RC=$RC WALL=$(( $(date +%s) - START ))s" | tee -a .run-unit/unit478-build-exit.txt
echo "warning or error lines: $(grep -a -c -E "warning [A-Z]+[0-9]+|error [A-Z]+[0-9]+" .run-unit/unit478-build-exit.txt)"
tail -4 .run-unit/unit478-build-exit.txt

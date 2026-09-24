#!/bin/sh
# unit 423 - build Hamlet.sln with warnings as errors, non-incremental, status first.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "TASK 4 of 4" code none "Exit round - Hamlet.sln non-incremental build with warnings as errors"
START=$(date +%s)
timeout 500 dotnet build Hamlet.sln -warnaserror --no-incremental -nologo -v q > .run-unit/unit423-build-exit-full.txt 2>&1
RC=$?
echo "RC=$RC WALL=$(( $(date +%s) - START ))s" | tee -a .run-unit/unit423-build-exit-full.txt
grep -cE "warning [A-Z]+[0-9]+" .run-unit/unit423-build-exit-full.txt
grep -E "error [A-Z]+[0-9]+" .run-unit/unit423-build-exit-full.txt | sort -u | head -20

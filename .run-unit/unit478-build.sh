#!/bin/sh
# unit 478 - build Hamlet.sln with warnings as errors, status first.
# Usage: sh .run-unit/unit478-build.sh <name> "<n of 3>" "<note>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit478-status.sh EXECUTING "$2" code none "$3" > /dev/null
START=$(date +%s)
timeout 400 dotnet build Hamlet.sln -warnaserror -nologo -v q > .run-unit/unit478-build-$1.txt 2>&1
RC=$?
echo "RC=$RC WALL=$(( $(date +%s) - START ))s" | tee -a .run-unit/unit478-build-$1.txt
grep -E "error [A-Z]+[0-9]+" .run-unit/unit478-build-$1.txt | sed 's#^.*src\\#src\\#' | sort -u | head -40

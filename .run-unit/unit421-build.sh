#!/bin/sh
# unit 421 - build Hamlet.sln with warnings as errors, status first.
# Usage: sh .run-unit/unit421-build.sh <name> "<TASK n of 4>" "<note>"
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "$2" code none "$3"
START=$(date +%s)
timeout 400 dotnet build Hamlet.sln -warnaserror -nologo -v q > .run-unit/unit421-build-$1.txt 2>&1
RC=$?
echo "RC=$RC WALL=$(( $(date +%s) - START ))s" | tee -a .run-unit/unit421-build-$1.txt
grep -E "error [A-Z]+[0-9]+" .run-unit/unit421-build-$1.txt | sed 's#^.*src\\#src\\#' | sort -u | head -40

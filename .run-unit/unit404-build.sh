#!/bin/sh
# unit 404 task 2 - build Hamlet.sln with warnings as errors, status first.
# Usage: sh .run-unit/unit404-build.sh <name> "<note>"
cd /c/Source/HamLet || exit 1
sh tools/status.sh ACTIVE "TASK 2 of 4" claude "none" "$2"
START=$(date +%s)
timeout 400 dotnet build Hamlet.sln -warnaserror -nologo -v q > .run-unit/unit404-build-$1.txt 2>&1
RC=$?
echo "RC=$RC WALL=$(( $(date +%s) - START ))s" | tee -a .run-unit/unit404-build-$1.txt
grep -E "error [A-Z]+[0-9]+" .run-unit/unit404-build-$1.txt | sed 's#^.*src\\#src\\#' | sort -u | head -40
grep -E "^\s*[0-9]+ Error|^\s*[0-9]+ Warning" .run-unit/unit404-build-$1.txt

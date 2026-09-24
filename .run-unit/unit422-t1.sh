#!/bin/sh
# unit 422 task 1 - build the app tests, then run the inventory fact alone.
# Usage: sh .run-unit/unit422-t1.sh <suffix> "<TASK n of 4>" "<note>"
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "$2" code none "$3 - building the app tests"
timeout 400 dotnet build tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj -warnaserror -nologo -v q > .run-unit/unit422-build-t1-$1.txt 2>&1
RC=$?
echo "build RC=$RC"
grep -E "error [A-Z]+[0-9]+" .run-unit/unit422-build-t1-$1.txt | sed 's#^.*tests\\#tests\\#' | sort -u | head -30
[ $RC -eq 0 ] || exit 1
sh .run-unit/unit422-run.sh "inventory-$1" app 300 "$2" "$3 - running the inventory fact alone" "FullyQualifiedName~.WhatEveryControlSaysOnHoverTests." --no-build

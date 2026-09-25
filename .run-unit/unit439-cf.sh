#!/bin/sh
# unit 439 - a carry-forward line exactly as docs/carry-forward-tests.txt prints it, one project per invocation.
# Usage: sh .run-unit/unit439-cf.sh <engine|app> <outname> "<TASK n of 3>" "<note>" [--no-build]
cd /c/Source/HamLet || exit 1
case "$1" in
  engine) KEY=Hamlet.RadioEngine.Tests.csproj ; TO=600 ;;
  app) KEY=Hamlet.App.Tests.csproj ; TO=480 ;;
esac
F=$(grep -m1 "timeout 480 dotnet test tests/.*/$KEY --filter" docs/carry-forward-tests.txt | sed 's/.*--filter "//; s/"[[:space:]]*$//')
sh .run-unit/unit439-run.sh "$2" "$1" $TO "$3" "$4" "$F" $5

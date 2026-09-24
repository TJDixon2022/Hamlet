#!/bin/sh
# unit 421 - which names on the app carry-forward line never printed a result in a given output.
# Usage: sh .run-unit/unit421-missing.sh <outname>
cd /c/Source/HamLet || exit 1
OUT=.run-unit/unit421-$1.txt
F=$(grep -m1 "timeout 480 dotnet test tests/.*/Hamlet.App.Tests.csproj --filter" docs/carry-forward-tests.txt | sed 's/.*--filter "//; s/"[[:space:]]*$//')
echo "$F" | tr "|" "\n" | sed 's/FullyQualifiedName~//' | while read -r N
do
  if ! grep -E "^\s+(Passed|Failed|Skipped) .*$N" "$OUT" > /dev/null
  then
    echo "no result: $N"
  fi
done

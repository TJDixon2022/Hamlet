#!/bin/sh
# unit 474 - R88: list every type on the app carry-forward line whose source mentions a recording.
cd /c/Source/HamLet || exit 1
FILTER=$(grep -m1 "dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --filter" docs/carry-forward-tests.txt | sed -e 's/^.*--filter "//' -e 's/"[[:space:]]*$//')
echo "$FILTER" | tr '|' '\n' | sed 's/^FullyQualifiedName~//' | cut -d. -f1 | sort -u > .run-unit/unit474-app-types.tmp
echo "types: $(wc -l < .run-unit/unit474-app-types.tmp)"
while read -r T
do
  F=$(grep -rl --include=*.cs "class $T\b" tests/Hamlet.App.Tests | head -1)
  if [ -z "$F" ]; then echo "NOFILE $T"; continue; fi
  grep -n -i -E "fixtures|\.wav\b" "$F" | head -3 | sed "s#^#$T: #"
done < .run-unit/unit474-app-types.tmp

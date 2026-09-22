#!/bin/sh
# Unit 388 - runs one app test filter ($1), one build, and prints the failures with their messages.
cd /c/Source/HamLet
timeout 480 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --filter "$1" 2>&1 | grep -E -A 14 "^\s*Failed |Passed!|Failed!|error CS|^\s*Standard Output" | grep -v -E "^--$"

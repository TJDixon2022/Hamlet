#!/bin/sh
# Unit 389 - runs one app test filter ($1) with its standard output printed whole.
timeout 480 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --filter "$1" --logger "console;verbosity=detailed" 2>&1 | grep -v -E "^\s*at |^Build|warning|^\s*$|Determining|Restored|->"

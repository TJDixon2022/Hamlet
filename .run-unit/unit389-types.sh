#!/bin/sh
# Unit 389: run an app filter ($1) in one build, printing only failures and totals.
timeout 480 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --filter "$1" 2>&1 | grep -E "^\s*Failed |Passed!|Failed!|error CS|dispatcher loop|Assert|Expected|Actual|\.cs:line" | head -120

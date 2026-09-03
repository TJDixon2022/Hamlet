#!/bin/sh
# Unit 230 task 1b - declared census by discovery, one project at a time.
cd /c/Source/HamLet
mkdir -p .unit230
dotnet test tests/Ft8Sharp.Tests/Ft8Sharp.Tests.csproj --list-tests -p:OutputPath=bin/unit230/ > .unit230/list-ft8sharp.txt 2>&1
echo "FT8SHARP EXIT $?" >> .unit230/census-progress.txt
dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --list-tests -p:OutputPath=bin/unit230/ > .unit230/list-app.txt 2>&1
echo "APP EXIT $?" >> .unit230/census-progress.txt
dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --list-tests -p:OutputPath=bin/unit230/ > .unit230/list-engine.txt 2>&1
echo "ENGINE EXIT $?" >> .unit230/census-progress.txt
echo "CENSUS DONE" >> .unit230/census-progress.txt

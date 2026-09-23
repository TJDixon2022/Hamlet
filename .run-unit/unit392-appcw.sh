cd /c/Source/HamLet
# Usage: sh .run-unit/unit392-appcw.sh <tag>
sh tools/status.sh EXECUTING "TASK 3 of 6" code none "floors: synthetics 2 of 2 red as R53 expects; the four app CW tests running in one invocation, timeout 480"
S=$(date +%s)
timeout 480 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --filter "FullyQualifiedName~TheSheetSaysWhatEachElementWasSentAtTests|FullyQualifiedName~ReturningToCwShowsCwTests|FullyQualifiedName~BindingHealthTests.TheMainWindowBindsWithoutOneComplaint|FullyQualifiedName~VoiceTests" --logger "console;verbosity=detailed" > .run-unit/unit392-appcw-$1.txt 2>&1
echo "exit $? in $(( $(date +%s) - S )) s"
grep -E "^\s+(Passed|Failed) " .run-unit/unit392-appcw-$1.txt | sed -E "s/^\s+//" | sed -E "s/ \[[^]]*\]$//" | cut -c1-170
tail -5 .run-unit/unit392-appcw-$1.txt

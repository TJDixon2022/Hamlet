cd /c/Source/HamLet
# One method of OneDecoderNotTwoTests alone, decision 5, timeout 580 to stay inside the harness's 600 s foreground cap
T=OneDecoderNotTwoTests-BufferSize
sh tools/status.sh EXECUTING "TASK 1 of 5" code none "task 1: OneDecoderNotTwoTests split by method, TheBufferSizeChangesNothing alone, timeout 580, running"
S=$(date +%s)
timeout 580 dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --no-build --filter "FullyQualifiedName~OneDecoderNotTwoTests.TheBufferSizeChangesNothing" --logger "console;verbosity=detailed" > .run-unit/unit394-set-$T.txt 2>&1
echo "exit $? in $(( $(date +%s) - S )) s"
echo "passed lines:"
grep -cE "^\s+Passed Hamlet" .run-unit/unit394-set-$T.txt
grep -E "^\s+Failed Hamlet|Test Run Aborted|crashed|^\s+(Passed|Failed|Total):" .run-unit/unit394-set-$T.txt | cut -c1-240

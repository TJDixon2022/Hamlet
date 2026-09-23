cd /c/Source/HamLet
# Usage: sh .run-unit/unit394-set.sh <Type> <filter> "<note>"
T=$1
F=$2
sh tools/status.sh EXECUTING "TASK 1 of 5" code none "$3"
S=$(date +%s)
timeout 600 dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --no-build --filter "$F" --logger "console;verbosity=detailed" > .run-unit/unit394-set-$T.txt 2>&1
echo "exit $? in $(( $(date +%s) - S )) s"
grep -E "^\s+(Passed|Failed) " .run-unit/unit394-set-$T.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | cut -c1-260
grep -E "Test Run Aborted|active test run was aborted|crashed" .run-unit/unit394-set-$T.txt | head -3
grep -E "^\s+(Passed|Failed|Total):" .run-unit/unit394-set-$T.txt

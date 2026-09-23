cd /c/Source/HamLet
# Usage: sh .run-unit/unit393-floors.sh <outname> <timeout> <filter> "TASK n of 5" "<note>"
N=$1
T=$2
F=$3
sh tools/status.sh EXECUTING "$4" code none "$5"
S=$(date +%s)
timeout $T dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --filter "$F" --logger "console;verbosity=detailed" > .run-unit/unit393-$N.txt 2>&1
echo "exit $? in $(( $(date +%s) - S )) s"
grep -E "^\s+(Passed|Failed) " .run-unit/unit393-$N.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | cut -c1-220
grep -E "Test Run Aborted|active test run was aborted|crashed" .run-unit/unit393-$N.txt | head -3
tail -2 .run-unit/unit393-$N.txt

cd /c/Source/HamLet
# Usage: sh .run-unit/unit398-floors.sh <outname> <timeout> <filter> "TASK n of 5" "<note>" [--no-build]
N=$1
T=$2
F=$3
sh tools/status.sh EXECUTING "$4" code none "$5"
S=$(date +%s)
timeout $T dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj $6 --filter "$F" --logger "console;verbosity=detailed" > .run-unit/unit398-$N.txt 2>&1
echo "exit $? in $(( $(date +%s) - S )) s"
grep -E "^\s+Failed " .run-unit/unit398-$N.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | cut -c1-220
grep -cE "^\s+Passed " .run-unit/unit398-$N.txt
grep -E "Test Run Aborted|active test run was aborted|crashed" .run-unit/unit398-$N.txt | head -3
grep -E "Total tests|Passed:|Failed:|Total time" .run-unit/unit398-$N.txt | tail -4

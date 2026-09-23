cd /c/Source/HamLet
# Usage: sh .run-unit/unit392-floors.sh <n> <timeout> <filter> <note>
N=$1
T=$2
F=$3
sh tools/status.sh EXECUTING "TASK 3 of 6" code none "$4"
S=$(date +%s)
timeout $T dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --filter "$F" --logger "console;verbosity=detailed" > .run-unit/unit392-floors-$N.txt 2>&1
echo "exit $? in $(( $(date +%s) - S )) s"
grep -E "^\s+(Passed|Failed) " .run-unit/unit392-floors-$N.txt | sed -E "s/^\s+//" | cut -c1-200
tail -2 .run-unit/unit392-floors-$N.txt

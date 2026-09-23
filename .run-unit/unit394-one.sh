cd /c/Source/HamLet
# Usage: sh .run-unit/unit394-one.sh <outname> <timeout> <csproj> <filter> "TASK n of 5" "<note>"
sh tools/status.sh EXECUTING "$5" code none "$6"
S=$(date +%s)
timeout $2 dotnet test $3 --filter "$4" --logger "console;verbosity=detailed" > .run-unit/unit394-$1.txt 2>&1
echo "exit $? in $(( $(date +%s) - S )) s"
grep -E "^\s+(Passed|Failed) " .run-unit/unit394-$1.txt | sed -E "s/^\s+//" | cut -c1-240
grep -E "Test Run Aborted|active test run was aborted|IOException" .run-unit/unit394-$1.txt | head -3
tail -2 .run-unit/unit394-$1.txt

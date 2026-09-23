cd /c/Source/HamLet
# Usage: sh .run-unit/unit400-carry.sh <app|eng> <tag> "TASK n of 5" "<note>"
# One line per call: line 7 is the app invocation, line 9 the engine invocation, run as printed.
WHICH=$1
TAG=$2
if [ "$WHICH" = app ]; then N=7; else N=9; fi
CMD=$(sed -n ${N}p docs/carry-forward-tests.txt | sed "s/^ *//" | tr -d "\r")
sh tools/status.sh EXECUTING "$3" code none "$4"
S=$(date +%s)
eval "$CMD" > .run-unit/unit400-carry-$TAG-$WHICH.txt 2>&1
echo "$WHICH exit $? in $(( $(date +%s) - S )) s"
grep -E "^\s+Failed |error |Test Run Aborted|active test run was aborted" .run-unit/unit400-carry-$TAG-$WHICH.txt | cut -c1-240
tail -3 .run-unit/unit400-carry-$TAG-$WHICH.txt

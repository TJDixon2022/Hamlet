cd /c/Source/HamLet
# Usage: sh .run-unit/unit392-carry-app.sh <tag> "TASK n of 6"
TAG=$1
APP=$(sed -n 7p docs/carry-forward-tests.txt | sed "s/^ *//")
sh tools/status.sh EXECUTING "$2" code none "carry-forward $TAG: app line re-run once after a dispatcher-loop loss, one build, timeout 480"
S=$(date +%s)
eval "$APP" > .run-unit/unit392-carry-$TAG-app.txt 2>&1
echo "app exit $? in $(( $(date +%s) - S )) s"
grep "^  Failed " .run-unit/unit392-carry-$TAG-app.txt
tail -1 .run-unit/unit392-carry-$TAG-app.txt

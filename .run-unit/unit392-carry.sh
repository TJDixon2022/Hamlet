cd /c/Source/HamLet
# Usage: sh .run-unit/unit392-carry.sh <tag> "TASK n of 6"
TAG=$1
TASK=$2
APP=$(sed -n 7p docs/carry-forward-tests.txt | sed "s/^ *//")
ENG=$(sed -n 9p docs/carry-forward-tests.txt | sed "s/^ *//")
sh tools/status.sh EXECUTING "$TASK" code none "carry-forward $TAG: app line running, one build, timeout 480"
S=$(date +%s)
eval "$APP" > .run-unit/unit392-carry-$TAG-app.txt 2>&1
echo "app exit $? in $(( $(date +%s) - S )) s"
tail -5 .run-unit/unit392-carry-$TAG-app.txt
sh tools/status.sh EXECUTING "$TASK" code none "carry-forward $TAG: app line done, engine line running, one build, timeout 480"
S=$(date +%s)
eval "$ENG" > .run-unit/unit392-carry-$TAG-eng.txt 2>&1
echo "engine exit $? in $(( $(date +%s) - S )) s"
tail -5 .run-unit/unit392-carry-$TAG-eng.txt

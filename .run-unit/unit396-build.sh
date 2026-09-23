cd /c/Source/HamLet
# Usage: sh .run-unit/unit396-build.sh <tag> "<note>"
sh tools/status.sh EXECUTING "$(cat .run-unit/unit396-task.txt)" code none "$2"
S=$(date +%s)
timeout 600 dotnet build Hamlet.sln -warnaserror > .run-unit/unit396-build-$1.txt 2>&1
echo "build exit $? in $(( $(date +%s) - S )) s"
grep -E " error | warning " .run-unit/unit396-build-$1.txt | sort -u | head -8 | cut -c1-300
tail -3 .run-unit/unit396-build-$1.txt

cd /c/Source/HamLet
# Usage: sh .run-unit/unit392-build.sh <n>  - one solution build, warnings as errors, errors deduplicated.
N=$1
sh tools/status.sh EXECUTING "TASK 2 of 6" code none "restore: solution build $N running, warnings as errors"
S=$(date +%s)
dotnet build Hamlet.sln -warnaserror -nologo -v q > .run-unit/unit392-build-$N.txt 2>&1
echo "build exit $? in $(( $(date +%s) - S )) s"
grep -E "error [A-Z]+[0-9]+" .run-unit/unit392-build-$N.txt | sed -E "s/ \[[^]]*\]$//" | sed "s#C:\\\\Source\\\\HamLet\\\\##" | sort -u > .run-unit/unit392-errors-$N.txt
echo "distinct errors: $(wc -l < .run-unit/unit392-errors-$N.txt)"
echo "by file:"
sed -E "s/\(.*//" .run-unit/unit392-errors-$N.txt | sort | uniq -c | sort -rn

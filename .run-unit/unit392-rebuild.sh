cd /c/Source/HamLet
sh tools/status.sh EXECUTING "$1" code none "restore: full non-incremental solution build, warnings as errors, to confirm the build"
S=$(date +%s)
dotnet build Hamlet.sln -warnaserror --no-incremental -nologo > .run-unit/unit392-rebuild.txt 2>&1
echo "rebuild exit $? in $(( $(date +%s) - S )) s"
grep -E " -> " .run-unit/unit392-rebuild.txt | sed "s#C:\\\\Source\\\\HamLet\\\\##"
tail -6 .run-unit/unit392-rebuild.txt

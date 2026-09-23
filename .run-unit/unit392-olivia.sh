cd /c/Source/HamLet
# Diagnostic, not a counted run: TheOliviaRowsTests' IOException, before and after the restore.
F="FullyQualifiedName~TheOliviaRowsTests.WithRowsPresentNothingIsComposedUntilAPressAndEachPressCarriesItsRowsVariant"
WT=C:/Source/HamLet-wt392
git worktree add --detach $WT 617215f0 > /dev/null 2>&1
echo "worktree at $(git -C $WT rev-parse --short HEAD)"
sh tools/status.sh EXECUTING "TASK 4 of 6" code none "carry-forward exit: one Olivia rows red twice by IOException; diagnostic run on the pre-restore tree in a worktree"
S=$(date +%s)
timeout 480 dotnet test $WT/tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --filter "$F" > .run-unit/unit392-olivia-before.txt 2>&1
echo "pre-restore 617215f0: exit $? in $(( $(date +%s) - S )) s"
grep -E "Failed |IOException|Passed!|Failed!" .run-unit/unit392-olivia-before.txt | cut -c1-200
sh tools/status.sh EXECUTING "TASK 4 of 6" code none "carry-forward exit: Olivia rows diagnostic, now the same test alone on the restored tree"
S=$(date +%s)
timeout 480 dotnet test tests/Hamlet.App.Tests/Hamlet.App.Tests.csproj --filter "$F" > .run-unit/unit392-olivia-after.txt 2>&1
echo "restored HEAD: exit $? in $(( $(date +%s) - S )) s"
grep -E "Failed |IOException|Passed!|Failed!" .run-unit/unit392-olivia-after.txt | cut -c1-200
git worktree remove --force $WT
git worktree list

cd /c/Source/HamLet
# Task 1: the corrected harness, uncommitted, against every asserting caller.
echo "app at 7e209cb4:"; git show 7e209cb4:src/Hamlet.App/ViewModels/MainWindowViewModel.cs | grep -n "CharacterSettled\|CharacterDecoded"
echo "harness diff:"; git diff -- tests/Hamlet.RadioEngine.Tests/Cw/CwDecodeHarness.cs
sh .run-unit/unit400-build.sh after "task 1: harness line 71 changed to CharacterSettled in the working tree, building warnings as errors"
sh .run-unit/unit400-floors.sh floors-after-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 1 of 5" "task 1: clean synthetics under the corrected harness running" --no-build
grep -h "^Actual" .run-unit/unit400-floors-after-3.txt
sh .run-unit/unit400-readers.sh after "TASK 1 of 5" "task 1: corrected harness, uncommitted;"
sh .run-unit/unit400-callers.sh after "TASK 1 of 5" "task 1: corrected harness, uncommitted; caller"
sh .run-unit/unit400-floors.sh printer-after 300 "FullyQualifiedName~TheCleanSyntheticsFourWaysTests" "TASK 1 of 5" "task 1: the four-ways printer under the corrected harness, for the record" --no-build
date +%H:%M:%S

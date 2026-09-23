cd /c/Source/HamLet
# Task 4: the three floor types at exit, after the engine line's build, then the tree checks.
sh .run-unit/unit398-floors.sh floors-exit-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 4 of 5" "task 4: exit round, lines green; captures type running" --no-build
sh .run-unit/unit398-floors.sh floors-exit-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 4 of 5" "task 4: exit round, adjudicated type running" --no-build
sh .run-unit/unit398-floors.sh floors-exit-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 4 of 5" "task 4: exit round, clean synthetics running" --no-build
grep -h "^Actual" .run-unit/unit398-floors-exit-3.txt
echo "captures against unit 397 exit:"; sh .run-unit/unit398-cmp.sh .run-unit/unit397-floors-exit-1.txt .run-unit/unit398-floors-exit-1.txt
sh .run-unit/unit398-transmit.sh
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo "end"
echo "tests vs 7e2abb7e:"; git diff --stat 7e2abb7e HEAD -- tests; echo "end"
echo "carry-forward list vs 7e2abb7e:"; git diff --stat 7e2abb7e HEAD -- docs/carry-forward-tests.txt docs/unit239-failing-set.txt; echo "end"
echo "worktrees:"; git worktree list
git status --short -- src tests docs
date +%H:%M:%S

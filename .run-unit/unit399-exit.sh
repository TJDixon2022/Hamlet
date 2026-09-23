cd /c/Source/HamLet
# Task 4: the three floor types at exit, after the engine line's build, the fixture-reading types, then the tree checks.
sh .run-unit/unit399-floors.sh floors-exit-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 4 of 5" "task 4: exit round, both lines green; captures type running" --no-build
sh .run-unit/unit399-floors.sh floors-exit-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 4 of 5" "task 4: exit round, adjudicated type running" --no-build
sh .run-unit/unit399-floors.sh floors-exit-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 4 of 5" "task 4: exit round, clean synthetics running" --no-build
grep -h "^Actual" .run-unit/unit399-floors-exit-3.txt
echo "captures against entry:"; sh .run-unit/unit399-cmp.sh .run-unit/unit399-floors-1.txt .run-unit/unit399-floors-exit-1.txt
echo "adjudicated against entry:"; sh .run-unit/unit399-adjcmp.sh .run-unit/unit399-floors-2.txt .run-unit/unit399-floors-exit-2.txt
sh .run-unit/unit399-readers.sh exit "TASK 4 of 5" "task 4: exit round, floors done;" > .run-unit/unit399-readers-exit.txt 2>&1
grep -E "^exit|Total tests|Passed:|Failed:" .run-unit/unit399-readers-exit.txt
grep -hE "^\s+(Passed|Failed) " .run-unit/unit399-fixtures-entry.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit399-fx-a.txt
grep -hE "^\s+(Passed|Failed) " .run-unit/unit399-fixtures-exit.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit399-fx-b.txt
echo "CwFixtureTests entry against exit:"; diff .run-unit/unit399-fx-a.txt .run-unit/unit399-fx-b.txt; echo "diff rc $?"
grep -hE "^\s+(Passed|Failed) " .run-unit/unit399-cleanreads-entry.txt .run-unit/unit399-survey-entry.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit399-cr-a.txt
grep -hE "^\s+(Passed|Failed) " .run-unit/unit399-cleanreads-exit.txt .run-unit/unit399-survey-exit.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit399-cr-b.txt
echo "clean reads and survey entry against exit:"; diff .run-unit/unit399-cr-a.txt .run-unit/unit399-cr-b.txt; echo "diff rc $?"
sh .run-unit/unit399-transmit.sh
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo "end"
echo "tests vs 7345a4f9:"; git diff --stat 7345a4f9 HEAD -- tests; echo "end"
echo "status tests:"; git status --short -- tests; echo "end"
echo "carry-forward list and failing set vs 7345a4f9:"; git diff --stat 7345a4f9 HEAD -- docs/carry-forward-tests.txt docs/unit239-failing-set.txt docs/cw-retired-tests.txt; echo "end"
echo "worktrees:"; git worktree list
date +%H:%M:%S

cd /c/Source/HamLet
# Task 3: the three floor types and the printer at exit, after the engine line's build, then the tree checks.
sh .run-unit/unit396-floors.sh floors-exit-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 3 of 4" "task 3: exit round, lines green; captures type running" --no-build
sh .run-unit/unit396-floors.sh floors-exit-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 3 of 4" "task 3: exit round, adjudicated type running" --no-build
sh .run-unit/unit396-floors.sh floors-exit-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 3 of 4" "task 3: exit round, clean synthetics running" --no-build
sh .run-unit/unit396-floors.sh printer-exit 300 "FullyQualifiedName~TheReworkNumbersPrinterTests" "TASK 3 of 4" "task 3: exit round, printer running" --no-build
grep -E "SETTLED" .run-unit/unit396-printer-exit.txt | cut -c1-240
grep -h "^Actual" .run-unit/unit396-floors-exit-3.txt
sh .run-unit/unit396-cmp.sh .run-unit/unit396-floors-1.txt .run-unit/unit396-floors-exit-1.txt
sh .run-unit/unit396-transmit.sh
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo "end"
echo "worktrees:"; git worktree list
git status --short -- src tests docs
date +%H:%M:%S

cd /c/Source/HamLet
# Task 0: build, the three floor types at entry, then the tree checks.
sh .run-unit/unit398-build.sh entry "task 0: entry build running warnings as errors, decision 4 diff empty so the lines are unit 397 exit runs"
sh .run-unit/unit398-floors.sh floors-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 0 of 5" "task 0: entry floors, captures type running" --no-build
sh .run-unit/unit398-floors.sh floors-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 0 of 5" "task 0: entry floors, adjudicated type running" --no-build
sh .run-unit/unit398-floors.sh floors-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 0 of 5" "task 0: entry floors, clean synthetics running" --no-build
grep -h "^Actual" .run-unit/unit398-floors-3.txt
sh .run-unit/unit398-cmp.sh .run-unit/unit397-floors-exit-1.txt .run-unit/unit398-floors-1.txt
sh .run-unit/unit398-transmit.sh
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo "end"
date +%H:%M:%S

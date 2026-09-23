cd /c/Source/HamLet
# Task 0: build, the three floor types at entry, the three fixture-reading types, then the tree checks.
sh .run-unit/unit399-build.sh entry "task 0: entry build running warnings as errors, decision 5 diff empty so the lines are unit 398 exit runs"
sh .run-unit/unit399-floors.sh floors-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 0 of 5" "task 0: entry floors, captures type running" --no-build
sh .run-unit/unit399-floors.sh floors-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 0 of 5" "task 0: entry floors, adjudicated type running" --no-build
sh .run-unit/unit399-floors.sh floors-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 0 of 5" "task 0: entry floors, clean synthetics running" --no-build
grep -h "^Actual" .run-unit/unit399-floors-3.txt
echo "captures against unit 398 exit:"; sh .run-unit/unit399-cmp.sh .run-unit/unit398-floors-exit-1.txt .run-unit/unit399-floors-1.txt
date +%H:%M:%S

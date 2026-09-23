cd /c/Source/HamLet
# Task 0: build, the three floor types at entry, the three fixture-reading types.
sh .run-unit/unit400-build.sh entry "task 0: entry build running warnings as errors, decision 6 diff empty so the lines are unit 399 exit runs"
sh .run-unit/unit400-floors.sh floors-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 0 of 5" "task 0: entry floors, captures type running" --no-build
sh .run-unit/unit400-floors.sh floors-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 0 of 5" "task 0: entry floors, adjudicated type running" --no-build
sh .run-unit/unit400-floors.sh floors-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 0 of 5" "task 0: entry floors, clean synthetics running" --no-build
grep -h "^Actual" .run-unit/unit400-floors-3.txt
echo "captures against unit 399 exit:"; sh .run-unit/unit400-cmp.sh .run-unit/unit399-floors-exit-1.txt .run-unit/unit400-floors-1.txt
echo "adjudicated against unit 399 exit:"; sh .run-unit/unit400-adjcmp.sh .run-unit/unit399-floors-exit-2.txt .run-unit/unit400-floors-2.txt
sh .run-unit/unit400-readers.sh entry "TASK 0 of 5" "task 0: entry, floors done;"
date +%H:%M:%S

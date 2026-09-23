cd /c/Source/HamLet
# Task 0: version bump, build, the three floor types and the printer at entry.
sed -i "1254s#<Version>1.13.83</Version>#<Version>1.13.84</Version>#" Directory.Build.props
sed -n 1254p Directory.Build.props
sh .run-unit/unit397-build.sh entry "task 0: entry build running warnings as errors, decision 20 diff empty so the lines are unit 396 exit runs"
sh .run-unit/unit397-floors.sh floors-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 0 of 3" "task 0: entry floors, captures type running" --no-build
sh .run-unit/unit397-floors.sh floors-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 0 of 3" "task 0: entry floors, adjudicated type running" --no-build
sh .run-unit/unit397-floors.sh floors-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 0 of 3" "task 0: entry floors, clean synthetics running" --no-build
sh .run-unit/unit397-floors.sh printer-entry 300 "FullyQualifiedName~TheReworkNumbersPrinterTests" "TASK 0 of 3" "task 0: entry printer running" --no-build
grep -E "SETTLED" .run-unit/unit397-printer-entry.txt | cut -c1-240
grep -h "^Actual" .run-unit/unit397-floors-3.txt
sh .run-unit/unit397-cmp.sh .run-unit/unit396-floors-exit-1.txt .run-unit/unit397-floors-1.txt
sh .run-unit/unit397-transmit.sh
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo "end"
date +%H:%M:%S

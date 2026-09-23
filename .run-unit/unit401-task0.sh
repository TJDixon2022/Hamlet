cd /c/Source/HamLet
sed -i "1254s/1.13.87/1.13.88/" Directory.Build.props
sed -n 1254p Directory.Build.props
sh .run-unit/unit401-build.sh entry "task 0: entry build warnings as errors; decision 6 diff printed nothing so the lines are unit 400 exit runs"
sh .run-unit/unit401-floors.sh floors-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 0 of 5" "task 0: entry floors, captures type running" --no-build
sh .run-unit/unit401-floors.sh floors-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 0 of 5" "task 0: entry floors, adjudicated type running" --no-build
sh .run-unit/unit401-floors.sh floors-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 0 of 5" "task 0: entry floors, clean synthetics running" --no-build
grep -h "^Actual\|CQ DE" .run-unit/unit401-floors-3.txt | head -6
echo "captures against unit 400 exit:"; sh .run-unit/unit401-cmp.sh .run-unit/unit400-floors-exit-1.txt .run-unit/unit401-floors-1.txt
echo "adjudicated against unit 400 exit:"; sh .run-unit/unit401-adjcmp.sh .run-unit/unit400-floors-exit-2.txt .run-unit/unit401-floors-2.txt
date +%H:%M:%S

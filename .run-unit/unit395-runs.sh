cd /c/Source/HamLet
# Usage: sh .run-unit/unit395-runs.sh <n> <hash> <before captures output>
# The three floor types and the printer after a built piece, the transmit check, the compare.
N=$1
H=$2
sh .run-unit/unit395-floors.sh p$N-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 2 of 4" "task 2: piece $N of 45, $H, built, captures type running" --no-build
sh .run-unit/unit395-floors.sh p$N-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 2 of 4" "task 2: piece $N of 45, $H, adjudicated type running" --no-build
sh .run-unit/unit395-floors.sh p$N-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 2 of 4" "task 2: piece $N of 45, $H, clean synthetics running" --no-build
sh .run-unit/unit395-floors.sh p$N-printer 300 "FullyQualifiedName~TheReworkNumbersPrinterTests" "TASK 2 of 4" "task 2: piece $N of 45, $H, printer running" --no-build
grep -E "SETTLED" .run-unit/unit395-p$N-printer.txt | cut -c1-200
sh .run-unit/unit395-cmp.sh $3 .run-unit/unit395-p$N-1.txt
sh .run-unit/unit394-transmit.sh
sh tools/status.sh EXECUTING "TASK 2 of 4" code none "task 2: piece $N of 45, $H, measured, judging"
date +%H:%M

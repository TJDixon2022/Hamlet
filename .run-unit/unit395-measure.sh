cd /c/Source/HamLet
# Usage: sh .run-unit/unit395-measure.sh <n> <hash> "<subject>" <before captures output>
# Commits the applied piece, builds, runs the three floor types and the printer, checks transmit, compares.
N=$1
H=$2
sh .run-unit/unit395-commit.sh "step 4 piece $N of 45: $H $3" "TASK 2 of 4" "task 2: piece $N of 45, $H, committed, building" src/Hamlet.RadioEngine/Cw
sh .run-unit/unit395-build.sh p$N "task 2: piece $N of 45, $H, solution build running warnings as errors"
if grep -q "Build succeeded" .run-unit/unit395-build-p$N.txt; then
  sh .run-unit/unit395-floors.sh p$N-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 2 of 4" "task 2: piece $N of 45, $H, built, captures type running" --no-build
  sh .run-unit/unit395-floors.sh p$N-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 2 of 4" "task 2: piece $N of 45, $H, adjudicated type running" --no-build
  sh .run-unit/unit395-floors.sh p$N-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 2 of 4" "task 2: piece $N of 45, $H, clean synthetics running" --no-build
  sh .run-unit/unit395-floors.sh p$N-printer 300 "FullyQualifiedName~TheReworkNumbersPrinterTests" "TASK 2 of 4" "task 2: piece $N of 45, $H, printer running" --no-build
  grep -E "SETTLED" .run-unit/unit395-p$N-printer.txt | cut -c1-200
  sh .run-unit/unit395-cmp.sh $4 .run-unit/unit395-p$N-1.txt
else
  echo "BUILD FAILED"
fi
sh .run-unit/unit394-transmit.sh
sh tools/status.sh EXECUTING "TASK 2 of 4" code none "task 2: piece $N of 45, $H, measured, judging"
date +%H:%M

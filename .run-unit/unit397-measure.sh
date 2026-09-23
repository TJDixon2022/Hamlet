cd /c/Source/HamLet
# Usage: sh .run-unit/unit397-measure.sh <n> <hash> "<subject>" <before captures output>
# Commits the applied piece, builds, runs the three floor types and the printer, checks transmit, compares.
N=$1
H=$2
sh .run-unit/unit397-commit.sh "step 4 piece $N of 45: $H $3" "$(cat .run-unit/unit397-task.txt)" "task 1: piece $N of 45, $H, committed, building" src/Hamlet.RadioEngine/Cw
sh .run-unit/unit397-build.sh p$N "task 1: piece $N of 45, $H, solution build running warnings as errors"
if grep -q "Build succeeded" .run-unit/unit397-build-p$N.txt; then
  sh .run-unit/unit397-floors.sh p$N-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "$(cat .run-unit/unit397-task.txt)" "task 1: piece $N of 45, $H, built, captures type running" --no-build
  sh .run-unit/unit397-floors.sh p$N-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "$(cat .run-unit/unit397-task.txt)" "task 1: piece $N of 45, $H, adjudicated type running" --no-build
  sh .run-unit/unit397-floors.sh p$N-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "$(cat .run-unit/unit397-task.txt)" "task 1: piece $N of 45, $H, clean synthetics running" --no-build
  sh .run-unit/unit397-floors.sh p$N-printer 300 "FullyQualifiedName~TheReworkNumbersPrinterTests" "$(cat .run-unit/unit397-task.txt)" "task 1: piece $N of 45, $H, printer running" --no-build
  grep -E "SETTLED" .run-unit/unit397-p$N-printer.txt | cut -c1-200
  sh .run-unit/unit397-cmp.sh $4 .run-unit/unit397-p$N-1.txt
else
  echo "BUILD FAILED"
fi
sh .run-unit/unit397-transmit.sh
sh tools/status.sh EXECUTING "$(cat .run-unit/unit397-task.txt)" code none "task 1: piece $N of 45, $H, measured, judging"
date +%H:%M

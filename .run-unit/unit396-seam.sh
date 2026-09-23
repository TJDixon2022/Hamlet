cd /c/Source/HamLet
# Usage: sh .run-unit/unit396-seam.sh <n> <hash> "<seam message>" <before captures output>
# Commits a decision 4 seam follow-up for the piece in flight, then builds and measures as unit396-measure.sh does.
N=$1
H=$2
T="$(cat .run-unit/unit396-task.txt)"
sh .run-unit/unit396-commit.sh "step 4 piece $N seam: $3" "$T" "task 2: piece $N of 45, $H, seam follow-up committed, building" src/Hamlet.RadioEngine/Cw
sh .run-unit/unit396-build.sh p$N "task 2: piece $N of 45, $H, solution build running warnings as errors"
if grep -q "Build succeeded" .run-unit/unit396-build-p$N.txt; then
  sh .run-unit/unit396-floors.sh p$N-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "$T" "task 2: piece $N of 45, $H, built, captures type running" --no-build
  sh .run-unit/unit396-floors.sh p$N-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "$T" "task 2: piece $N of 45, $H, adjudicated type running" --no-build
  sh .run-unit/unit396-floors.sh p$N-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "$T" "task 2: piece $N of 45, $H, clean synthetics running" --no-build
  sh .run-unit/unit396-floors.sh p$N-printer 300 "FullyQualifiedName~TheReworkNumbersPrinterTests" "$T" "task 2: piece $N of 45, $H, printer running" --no-build
  grep -E "SETTLED" .run-unit/unit396-p$N-printer.txt | cut -c1-200
  sh .run-unit/unit396-cmp.sh $4 .run-unit/unit396-p$N-1.txt
else
  echo "BUILD FAILED"
fi
sh .run-unit/unit396-transmit.sh
sh tools/status.sh EXECUTING "$T" code none "task 2: piece $N of 45, $H, measured, judging"
date +%H:%M

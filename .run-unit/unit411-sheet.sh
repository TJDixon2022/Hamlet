#!/bin/sh
# unit 411 - the app types that read the sidecar's keying and elementHz lines, one per invocation.
# Usage: sh .run-unit/unit411-sheet.sh <suffix> "<TASK n of 4>"
cd /c/Source/HamLet || exit 1
for t in TheSidecarDoesNotContradictItselfTests TheSheetSaysWhatEachElementWasSentAtTests AHeldVerdictPrintsNoMeasurementsTests TheCaptureButtonTests
do
  echo "== $t"
  sh .run-unit/unit411-run.sh "$t-$1" app 300 "$2" "Running $t alone, a neighbor of the sidecar rewording" "FullyQualifiedName~.$t." --no-build
done

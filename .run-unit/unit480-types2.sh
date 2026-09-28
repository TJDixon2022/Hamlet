#!/bin/sh
# unit 480 task 2 - the touched types that read no recording, one per invocation (HM-DEC-155).
# Usage: sh .run-unit/unit480-types2.sh <suffix>
cd /c/Source/HamLet || exit 1
for t in TheRadioPointsTheDetectorTests TheTrackerObeysTheMeterTests AMarkIsTheEnvelopeOverAThresholdTests ABarIsALevelThatHoldsTests TheScopeOutputIsACwConditionTests
do
  echo "== $t"
  sh .run-unit/unit480-type.sh engine "$t" "$t-$1" 300 | grep -E "^RC=|^Failed|Total tests|Passed: |Failed: "
done
for t in TheOwnersVerdictIsARowTests TheVerdictCarriesTheScopeTests TheScopeIsTheMiddlePictureTests TheScopeShowsTheMarksTests
do
  echo "== app $t"
  sh .run-unit/unit480-type.sh app "$t" "$t-$1" 300 | grep -E "^RC=|^Failed|Total tests|Passed: |Failed: "
done

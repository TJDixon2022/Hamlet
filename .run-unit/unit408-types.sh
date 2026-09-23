#!/bin/sh
# unit 408 - the CW types around the gate, one type per invocation, --no-build after one build.
# Usage: sh .run-unit/unit408-types.sh <suffix> "<TASK n of 4>"
cd /c/Source/HamLet || exit 1
for t in CwFixtureTests CwReceiverFixtureTests CwAcquisitionWindowTests CwEmissionGateTests CapturedSignalTests CwAdjudicationTests CwDisplacementFloorTests CwSpeedSilenceTests WhyTheGateDidNotFireTests CwTwoStationTests EachCharacterAnswersForItselfTests
do
  echo "== $t"
  sh .run-unit/unit408-run.sh "$t-$1" engine 300 "$2" "Running $t alone, one of eleven CW types around the gate" "FullyQualifiedName~.$t." --no-build
done

#!/bin/sh
# unit 447 - exit round: every type touched, one type per invocation, --no-build.
cd /c/Source/HamLet || exit 1
R=.run-unit/unit447-run.sh
for t in ThePitchInstrumentIsProvedTests TheTrackedPitchIsChosenByKeyingTests TheSyntheticCqRebuildsTests WhatTheGeneratorMakesTests CwFixtureCommitTests CwSurveyThresholdPinTests CwToneSurveyTests CwTrackerSwitchTests TheGateHasItsOwnWindowNowTests ThePeakAgainstASecondSignalTests WhatBandwidthTheDecoderListensThroughTests; do
  echo "== $t"
  sh $R "touched-$t" engine 600 "4 of 4" "Exit round: every type touched, $t alone" "FullyQualifiedName~.$t." --no-build
done

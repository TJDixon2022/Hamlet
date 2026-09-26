#!/bin/sh
# unit 448 - exit round: every type touched, one type per invocation, --no-build.
cd /c/Source/HamLet || exit 1
R=.run-unit/unit448-run.sh
for t in CwToneSurveyTests CwSurveyThresholdPinTests CwTrackerSwitchTests TheTrackedPitchIsChosenByKeyingTests WhatTheDecoderDoesWhileAcquiringTests; do
  echo "== $t"
  sh $R "touched-$t" engine 600 "3 of 3" "Exit round: every type touched, $t alone" "FullyQualifiedName~.$t." --no-build
done

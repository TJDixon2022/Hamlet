#!/bin/sh
# unit 477 task 4 - every touched type that reads no recording, one named type per invocation, each with its own timeout (HM-DEC-155).
cd /c/Source/HamLet || exit 1
for t in ABarIsALevelThatHoldsTests AMarkIsTheEnvelopeOverAThresholdTests TheTrackerObeysTheMeterTests
do
  echo "== $t"
  sh .run-unit/unit477-run.sh "exit-$t" 300 "4 of 4" "Task 4 exit round: $t, alone" "$t" engine --no-build | grep -E "^RC=|Total tests|Passed:|Failed:|^Failed "
done
for t in TheScopeShowsTheMarksTests TheVerdictCarriesTheScopeTests TheOwnersVerdictIsARowTests TheLightSaysWhatHamletThinksItHearsTests TheStripShowsWhereTheDetectorLooksTests BindingHealthTests EveryControlSaysWhatItDoesTests WhatEveryControlSaysOnHoverTests
do
  echo "== $t"
  sh .run-unit/unit477-run.sh "exit-$t" 300 "4 of 4" "Task 4 exit round: $t, alone" "$t" app --no-build | grep -E "^RC=|Total tests|Passed:|Failed:|^Failed "
done

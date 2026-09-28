#!/bin/sh
# unit 474 - every app type this unit touched or could disturb, one per invocation, --no-build, status before each.
# Usage: sh .run-unit/unit474-touched.sh <suffix> "<n of 4>"
cd /c/Source/HamLet || exit 1
for T in TheLightSaysWhatHamletThinksItHearsTests TheStripShowsWhereTheDetectorLooksTests TheOwnersVerdictIsARowTests EveryControlSaysWhatItDoesTests WhatEveryControlSaysOnHoverTests BindingHealthTests TheCapturePressIsOnTheScreenTests SettingsRoundTripTests Unit303ClockRecordTests
do
  echo "== $T"
  sh .run-unit/unit474-run.sh "$1-$T" 400 "$2" "Touched types - $T" "$T" --no-build | grep -E "^RC=|Total tests|Passed:|Failed:|^Failed "
done

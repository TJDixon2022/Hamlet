#!/bin/sh
# unit 449 - every type touching the changed tracker floor or the rival reading, each alone.
cd /c/Source/HamLet || exit 1
grep -a "floor\|reads\|Assert" .run-unit/unit449-named-exit.txt | grep -a "032113\|032129\|173723" | cut -c1-220 | head -6
for t in TheTrackerStaysWithTheStationItReadsTests CwTrackerSwitchTests TheTrackedPitchIsChosenByKeyingTests CwSurveyThresholdPinTests CwToneSurveyTests TheOperatorIsToldAboutASecondStationTests ThePeakAgainstASecondSignalTests TheQuietestBinNoLongerWinsTests TheTrackerSwitchTraceTests TheStationStillKeyingTraceTests; do
  echo "== $t"
  sh .run-unit/unit449-run.sh "touched-$t" engine 300 "3 of 3" "Exit round: touched type $t alone" "FullyQualifiedName~.$t." --no-build
done

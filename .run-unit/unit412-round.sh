#!/bin/sh
# unit 412 - the round's types, one type per invocation, --no-build after one build.
# Usage: sh .run-unit/unit412-round.sh <floors|gate|readers> <suffix> "<TASK n of 4>"
cd /c/Source/HamLet || exit 1
R=.run-unit/unit412-run.sh
case "$1" in
  floors)
    sh $R "captures-$2" engine 300 "$3" "Running the captures floor, 37 rows, alone" "FullyQualifiedName~.TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid" --no-build
    sh $R "adjudicated-$2" engine 300 "$3" "Running the adjudicated floor, 13 readings, alone" "FullyQualifiedName~.TheAdjudicatedReadingsKeepReadingTests." --no-build
    sh $R "clean-$2" engine 120 "$3" "Running the two clean synthetics alone" "FullyQualifiedName~.CwFixtureTests.TheCleanRecordingsDecodeExactly" --no-build
    ;;
  gate)
    sh $R "receiver-$2" engine 300 "$3" "Running CwReceiverFixtureTests alone, 27 cases" "FullyQualifiedName~.CwReceiverFixtureTests." --no-build
    sh $R "acquisition-$2" engine 300 "$3" "Running CwAcquisitionWindowTests alone, 12 cases" "FullyQualifiedName~.CwAcquisitionWindowTests." --no-build
    sh $R "1737-$2" engine 300 "$3" "Running TheSeventeenThirtySevenCaptureTests alone" "FullyQualifiedName~.TheSeventeenThirtySevenCaptureTests." --no-build
    ;;
  readers)
    for t in ThePitchCanBeHeldTests CwSurveyThresholdPinTests CwToneSurveyTests CwTrackerSwitchTests TheKeyingWitnessSaysNothingImpossibleTests TheOperatorIsToldAboutASecondStationTests TheSurveyAlreadyUsesAShortWindowTests WhatDecodeScoringCostsTests
    do
      echo "== $t"
      sh $R "$t-$2" engine 300 "$3" "Running tracker reader $t alone" "FullyQualifiedName~.$t." --no-build
    done
    echo "== TheGateHasItsOwnWindowNowTests short methods"
    sh $R "gatewindow-$2" engine 120 "$3" "Running the two short methods of TheGateHasItsOwnWindowNowTests" "FullyQualifiedName~.TheGateHasItsOwnWindowNowTests.UnsetTheGateStillTakesTheSurveysWindow|FullyQualifiedName~.TheGateHasItsOwnWindowNowTests.TheSurveyKeepsItsWindowWhateverTheGateTakes" --no-build
    ;;
  bandwidth)
    sh $R "bandwidth-$2" engine 400 "$3" "Running WhatBandwidthTheDecoderListensThroughTests alone, 6 cases" "FullyQualifiedName~.WhatBandwidthTheDecoderListensThroughTests." --no-build
    ;;
esac

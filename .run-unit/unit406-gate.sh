#!/bin/sh
# unit 406 - run named types one per invocation, each diffed against its entry list.
# Usage: sh .run-unit/unit406-gate.sh <tag> "<TASK n of 5>" "<note prefix>" <key>...
cd /c/Source/HamLet || exit 1
TAG=$1
TK=$2
NP=$3
shift 3
for W in "$@"
do
  T=300
  case $W in
    cap) F="FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" ;;
    adj) F="FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" ; T=180 ;;
    syn) F="FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" ; T=120 ;;
    acq) F="FullyQualifiedName~CwAcquisitionWindowTests" ;;
    recv) F="FullyQualifiedName~CwReceiverFixtureTests" ;;
    fix) F="FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.CwFixtureTests" ;;
    adjt) F="FullyQualifiedName~CwAdjudicationTests" ;;
    gate) F="FullyQualifiedName~CwEmissionGateTests" ;;
    disp) F="FullyQualifiedName~CwDisplacementFloorTests" ;;
    capsig) F="FullyQualifiedName~CapturedSignalTests" ;;
    silence) F="FullyQualifiedName~CwSpeedSilenceTests" ;;
    whygate) F="FullyQualifiedName~WhyTheGateDidNotFireTests" ;;
    twostation) F="FullyQualifiedName~CwTwoStationTests" ;;
    held) F="FullyQualifiedName~ThePitchCanBeHeldTests" ;;
    pin) F="FullyQualifiedName~CwSurveyThresholdPinTests" ;;
    survey) F="FullyQualifiedName~CwToneSurveyTests" ;;
    switch) F="FullyQualifiedName~CwTrackerSwitchTests" ;;
    gatewin) F="FullyQualifiedName~TheGateHasItsOwnWindowNowTests" ;;
    witness) F="FullyQualifiedName~TheKeyingWitnessSaysNothingImpossibleTests" ;;
    second) F="FullyQualifiedName~TheOperatorIsToldAboutASecondStationTests" ;;
    short) F="FullyQualifiedName~TheSurveyAlreadyUsesAShortWindowTests" ;;
    bw) F="FullyQualifiedName~WhatBandwidthTheDecoderListensThroughTests" ;;
    scoring) F="FullyQualifiedName~WhatDecodeScoringCostsTests" ;;
    *) echo "unknown key $W" ; continue ;;
  esac
  echo "== $W"
  sh .run-unit/unit406-run.sh "$W-$TAG" engine $T "$TK" "$NP: $W running" "$F" --no-build
  if [ "$TAG" = "entry" ]; then
    sh .run-unit/unit406-list.sh "$W-$TAG" | tail -3
  else
    sh .run-unit/unit406-list.sh "$W-$TAG" "$W-entry" | tail -12
  fi
  if [ "$W" = "cap" ]; then
    if [ "$TAG" = "entry" ]; then
      sh .run-unit/unit406-table.sh "$W-$TAG" .run-unit/unit405-cap-exit-table.txt
    else
      sh .run-unit/unit406-table.sh "$W-$TAG" .run-unit/unit406-cap-entry-table.txt
    fi
  fi
done
date +%H:%M:%S

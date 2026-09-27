#!/bin/sh
# unit 468 - the round's types not in unit469-round.sh, one type per invocation, --no-build after one build.
# Usage: sh .run-unit/unit469-extra.sh <case> <suffix> "<n of 3>"
cd /c/Source/HamLet || exit 1
export LC_ALL=C
R=.run-unit/unit469-run.sh
T=FullyQualifiedName~.TheArbitrationEarnsItsPlaceTests
case "$1" in
  a)
    sh $R "a-$2" engine 120 "$3" "HM-REQ-128 test part (a), the five hand-built cases" "$T.Where" --no-build
    ;;
  bh)
    sh $R "b-harness-$2" engine 600 "$3" "HM-REQ-128 test part (b), harness rows" "$T.EveryHarnessRowIsNoWorseThanTheBetterDecoderAlone" --no-build
    ;;
  bl)
    sh $R "b-live-$2" engine 600 "$3" "HM-REQ-128 test part (b), the live row, and the port-alone live check" "$T.TheLiveRow|$T.ThePortAlone" --no-build
    ;;
  channels)
    sh $R "channels-$2" engine 600 "$3" "Unit 461's channel proof, TheChannelProfilesAreWhatTheySayTests" "FullyQualifiedName~.TheChannelProfilesAreWhatTheySayTests." --no-build
    ;;
  senders)
    sh $R "senders-$2" engine 600 "$3" "The TX-* sender proof, TheSenderProfilesAreWhatTheySayTests" "FullyQualifiedName~.TheSenderProfilesAreWhatTheySayTests." --no-build
    ;;
  fists)
    sh $R "fists-$2" engine 600 "$3" "HM-REQ-050 measured: the must-tier fists at 15 dB on CH-AWGN" "FullyQualifiedName~.TheMustFistsAtFifteenDecibelsFact." --no-build
    ;;
esac

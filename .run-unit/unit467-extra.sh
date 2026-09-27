#!/bin/sh
# unit 467 - the round's types not in unit467-round.sh, one type per invocation, --no-build after one build.
# Usage: sh .run-unit/unit467-extra.sh <case> <suffix> "<n of 2>"
cd /c/Source/HamLet || exit 1
export LC_ALL=C
R=.run-unit/unit467-run.sh
T=FullyQualifiedName~.TheArbitrationEarnsItsPlaceTests
case "$1" in
  one)
    sh $R "one-$2" app 300 "$3" "465's test: the operator sees one transcript (app)" "FullyQualifiedName~.TheOperatorSeesOneTranscriptTests." --no-build
    ;;
  same)
    sh $R "same-$2" engine 300 "$3" "465's test: both decoders read the same samples" "FullyQualifiedName~.BothDecodersReadTheSameSamplesTests." --no-build
    ;;
  wins)
    sh $R "wins-$2" engine 300 "$3" "465's test: the higher calibrated reading wins" "FullyQualifiedName~.TheHigherCalibratedReadingWinsTests." --no-build
    ;;
  confidence)
    sh $R "confidence-$2" engine 300 "$3" "Every character carries a confidence" "FullyQualifiedName~.EveryCharacterCarriesAConfidenceTests." --no-build
    ;;
  a)
    sh $R "a-$2" engine 120 "$3" "HM-REQ-128 test part (a), the five hand-built cases" "$T.Where" --no-build
    ;;
  bh)
    sh $R "b-harness-$2" engine 600 "$3" "HM-REQ-128 test part (b), harness rows" "$T.EveryHarnessRowIsNoWorseThanTheBetterDecoderAlone" --no-build
    grep -a -E "HM-REQ-128|row |Assert|Expected|worse|says" .run-unit/unit467-b-harness-$2.txt | cut -c1-300 | head -40
    ;;
  bl)
    sh $R "b-live-$2" engine 600 "$3" "HM-REQ-128 test part (b), the live row, and the port-alone live check" "$T.TheLiveRow|$T.ThePortAlone" --no-build
    ;;
  emitted)
    sh $R "save-emitted-$2" engine 600 "$3" "Saving the harness output emitted under the switch table, all 35, with class and p" "FullyQualifiedName~.TheArbitrationEarnsItsPlaceFact.TheEmittedTranscriptWithClassAndP" --no-build
    grep -a "^ *save | ours | " .run-unit/unit467-save-emitted-$2.txt | sed 's/^ *//' > .run-unit/unit467-emitted-$2.txt
    wc -l .run-unit/unit467-emitted-$2.txt
    cmp .run-unit/unit466-emitted-exit.txt .run-unit/unit467-emitted-$2.txt && echo "emitted harness BYTE-IDENTICAL to 466's exit save"
    [ "$2" = before ] || { cmp .run-unit/unit467-emitted-before.txt .run-unit/unit467-emitted-$2.txt && echo "emitted harness BYTE-IDENTICAL to task 0's save"; }
    ;;
  live)
    sh $R "save-live-$2" engine 600 "$3" "Saving the transcript emitted on the live path, all 35, with class and p, and timing it" "FullyQualifiedName~.WhereTheTwoReadingsMeetFact.TheLivePathOverTheCorpus" --no-build
    grep -a "^ *save | ours | " .run-unit/unit467-save-live-$2.txt | sed 's/^ *//' > .run-unit/unit467-emitted-live-$2.txt
    wc -l .run-unit/unit467-emitted-live-$2.txt
    cmp .run-unit/unit466-arb-live-exit.txt .run-unit/unit467-emitted-live-$2.txt && echo "emitted live BYTE-IDENTICAL to 466's exit save"
    [ "$2" = before ] || { cmp .run-unit/unit467-emitted-live-before.txt .run-unit/unit467-emitted-live-$2.txt && echo "emitted live BYTE-IDENTICAL to task 0's save"; }
    grep -a "decode time |" .run-unit/unit467-save-live-$2.txt | sed 's/^ *//'
    ;;
  pairs)
    sh $R "pairs-$2" engine 300 "$3" "459's red test, reported only" "FullyQualifiedName~.TheSpeedFollowsTheSendersMarkPairsTests." --no-build
    ;;
esac

#!/bin/sh
# unit 457 - the round's types, one type per invocation, --no-build after one build.
# Usage: sh .run-unit/unit457-round.sh <captures|adjudicated|named|metrics|hand|trace|g1> <suffix> "<n of 5>"
cd /c/Source/HamLet || exit 1
R=.run-unit/unit457-run.sh
case "$1" in
  captures)
    sh $R "captures-$2" engine 600 "$3" "Captures floor alone, 51 rows" "FullyQualifiedName~.TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid" --no-build
    ;;
  adjudicated)
    sh $R "adjudicated-$2" engine 300 "$3" "Adjudicated floor alone, 13 readings" "FullyQualifiedName~.TheAdjudicatedReadingsKeepReadingTests." --no-build
    ;;
  named)
    sh $R "named-$2" engine 300 "$3" "Named floors alone" "FullyQualifiedName~.TheNumberCannotBeGamedTests." --no-build
    ;;
  metrics)
    sh $R "metrics-$2" engine 300 "$3" "The four metrics over every keyed recording" "FullyQualifiedName~.TheRequirementsAreMeasuredTests." --no-build
    ;;
  speed)
    sh $R "speed-$2" engine 600 "$3" "Speed cases at 5, 8, 40 and 45 WPM and the trace of every bound on the search" "FullyQualifiedName~.TheSpeedSearchReachesBothEndsTests." --no-build
    ;;
  hand)
    sh $R "hand-$2" engine 120 "$3" "The metrics against hand-built pairs" "FullyQualifiedName~.TheMetricsCountWhatAHandCountsTests." --no-build
    ;;
  shape)
    sh $R "shape-$2" engine 600 "$3" "Mark-shape trace: every sure letter's marks and inner gaps against a dit, a dah and an element gap, real then synthetic" "FullyQualifiedName~.WhatTheSureLettersMarksLookLikeTests." --no-build
    ;;
  pitch)
    sh $R "pitch-$2" engine 600 "$3" "MET-PITCH-ERR over every capture with 447's printer" "FullyQualifiedName~.WhatPitchTheDecoderIsOnTests.OnEveryCaptureInTheTree" --no-build
    ;;
  trace449)
    sh $R "trace449-$2" engine 600 "$3" "The forty sure-wrong letters at HEAD with the instrument's pitch beside each" "FullyQualifiedName~.WhatTheFortySureWrongLettersRestOnTests." --no-build
    ;;
  state)
    sh $R "trace-pitch-state-$2" engine 600 "$3" "Trace: every hop's proposed pitch state beside the instrument, 23 keyed, the over-25 files and the synthetic set" "FullyQualifiedName~.WhatThePitchCanSayItProvedTests.EveryHopsStateBesideTheInstrument" --no-build
    ;;
  speedstate)
    sh $R "trace-speed-state-$2" engine 600 "$3" "Trace: every hop's proposed speed state, 23 keyed and the synthetic set with the speed error of proved hops" "FullyQualifiedName~.WhatTheSpeedCanSayItProvedTests.EveryHopsSpeedState" --no-build
    ;;
  req034)
    sh $R "req034-$2" engine 300 "$3" "Test: the HM-REQ-034 cases alone" "FullyQualifiedName~.TheSpeedSaysWhetherItWasProvedTests." --no-build
    grep -a "HM-REQ-034 |" .run-unit/unit457-req034-$2.txt | cut -c1-400
    ;;
  req036)
    sh $R "req036-$2" engine 300 "$3" "Test: the HM-REQ-036 cases alone" "FullyQualifiedName~.ARefinementKeepsTheTimingTests." --no-build
    grep -a "HM-REQ-036 |" .run-unit/unit457-req036-$2.txt | cut -c1-400
    ;;
  req093)
    sh $R "req093-$2" engine 300 "$3" "Test: the HM-REQ-093 cases alone" "FullyQualifiedName~.ThePitchSaysWhetherItWasProvedTests." --no-build
    grep -a "HM-REQ-093 |" .run-unit/unit457-req093-$2.txt | cut -c1-400
    ;;
  wbetrace)
    sh $R "wbe-trace-$2" engine 600 "$3" "Trace: MET-WBE per condition and every wrong word boundary with its gap, thresholds, marks and spacing" "FullyQualifiedName~.WhereTheWordBoundariesGoWrongTests." --no-build
    ;;
  trace)
    sh $R "trace-$2" engine 600 "$3" "Tracing the sure-wrong letters with their speeds" "FullyQualifiedName~.WhereTheSureWrongLettersComeFromTests." --no-build
    ;;
esac

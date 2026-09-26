#!/bin/sh
# unit 445 - the round's types, one type per invocation, --no-build after one build.
# Usage: sh .run-unit/unit445-round.sh <captures|adjudicated|named|metrics|hand|trace|g1> <suffix> "<n of 3>"
cd /c/Source/HamLet || exit 1
R=.run-unit/unit445-run.sh
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
  hand)
    sh $R "hand-$2" engine 120 "$3" "The metrics against hand-built pairs" "FullyQualifiedName~.TheMetricsCountWhatAHandCountsTests." --no-build
    ;;
  trace)
    sh $R "trace-$2" engine 600 "$3" "Tracing the sure-wrong letters with their speeds" "FullyQualifiedName~.WhereTheSureWrongLettersComeFromTests." --no-build
    ;;
esac

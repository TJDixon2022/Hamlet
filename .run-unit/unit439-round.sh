#!/bin/sh
# unit 439 - the round's types, one type per invocation, --no-build after one build.
# Usage: sh .run-unit/unit439-round.sh <type> <suffix> "<TASK n of 3>"
cd /c/Source/HamLet || exit 1
R=.run-unit/unit439-run.sh
case "$1" in
  captures)
    sh $R "captures-$2" engine 600 "$3" "Running the captures floor alone, 51 rows" "FullyQualifiedName~.TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid" --no-build
    ;;
  adjudicated)
    sh $R "adjudicated-$2" engine 600 "$3" "Running the adjudicated floor alone, 13 readings" "FullyQualifiedName~.TheAdjudicatedReadingsKeepReadingTests." --no-build
    ;;
  named)
    sh $R "named-$2" engine 600 "$3" "Running TheNumberCannotBeGamedTests alone, the 13 keyed floors" "FullyQualifiedName~.TheNumberCannotBeGamedTests." --no-build
    ;;
  keyed)
    sh $R "keyed-$2" engine 600 "$3" "Running TheBenchmarkIsKeyedTests alone, the keyed totals" "FullyQualifiedName~.TheBenchmarkIsKeyedTests." --no-build
    ;;
  baseline)
    sh $R "baseline-$2" engine 600 "$3" "Running TheBaselineIsScoredTests alone, the baseline and outside totals" "FullyQualifiedName~.TheBaselineIsScoredTests." --no-build
    ;;
  strays)
    sh $R "strays-$2" engine 600 "$3" "Running WhatTheStrayLettersRestOnTests alone, 165 over 565 and the added letters" "FullyQualifiedName~.WhatTheStrayLettersRestOnTests." --no-build
    ;;
  opening)
    sh $R "opening-$2" engine 600 "$3" "Running WhatTheOpeningHeardTests alone, the opening before and after" "FullyQualifiedName~.WhatTheOpeningHeardTests." --no-build
    ;;
  plateau)
    sh $R "plateau-$2" engine 600 "$3" "Running TheUnitIsMeasuredNotSearchedTests alone, the plateau red recorded only" "FullyQualifiedName~.TheUnitIsMeasuredNotSearchedTests." --no-build
    ;;
  stream)
    sh $R "stream-$2" engine 600 "$3" "Running CwProbabilisticStream tests alone, the type the change touches" "FullyQualifiedName~.CwProbabilisticStream" --no-build
    ;;
  estimator)
    sh $R "estimator-$2" engine 600 "$3" "Running CwUnitEstimator tests alone, the type the change touches" "FullyQualifiedName~.CwUnitEstimator" --no-build
    ;;
esac

#!/bin/sh
# unit 415 - the round's types, one type per invocation, --no-build after one build.
# Usage: sh .run-unit/unit415-round.sh <captures|adjudicated|clean|baseline|keyed|named|trace> <suffix> "<TASK n of 4>"
cd /c/Source/HamLet || exit 1
R=.run-unit/unit415-run.sh
case "$1" in
  captures)
    sh $R "captures-$2" engine 600 "$3" "Running the captures floor alone, 51 rows" "FullyQualifiedName~.TheCapturesThatDecodeKeepDecodingTests.EachStillProducesWhatItDid" --no-build
    ;;
  adjudicated)
    sh $R "adjudicated-$2" engine 300 "$3" "Running the adjudicated floor alone, 13 readings" "FullyQualifiedName~.TheAdjudicatedReadingsKeepReadingTests." --no-build
    ;;
  clean)
    sh $R "clean-$2" engine 120 "$3" "Running the two clean synthetics alone" "FullyQualifiedName~.CwFixtureTests.TheCleanRecordingsDecodeExactly" --no-build
    ;;
  baseline)
    sh $R "baseline-$2" engine 300 "$3" "Running TheBaselineIsScoredTests alone, the baseline and outside totals" "FullyQualifiedName~.TheBaselineIsScoredTests." --no-build
    ;;
  keyed)
    sh $R "keyed-$2" engine 300 "$3" "Running TheBenchmarkIsKeyedTests alone, the ten keyed captures" "FullyQualifiedName~.TheBenchmarkIsKeyedTests." --no-build
    ;;
  named)
    sh $R "named-$2" engine 300 "$3" "Running TheNumberCannotBeGamedTests alone, the named floors" "FullyQualifiedName~.TheNumberCannotBeGamedTests." --no-build
    ;;
  trace)
    sh $R "trace-$2" engine 300 "$3" "Running WhereTheWordsBreakTests alone, unit 413's trace" "FullyQualifiedName~.WhereTheWordsBreakTests." --no-build
    ;;
  synth)
    sh $R "synth-$2" engine 300 "$3" "Running TheSyntheticCqIsScoredTests alone, the grid and the five-unit row, exact keys, never evidence" "FullyQualifiedName~.TheSyntheticCqIsScoredTests." --no-build
    ;;
  relabel)
    sh $R "relabel-$2" engine 600 "$3" "Running WhereTheSpaceIsDecidedTests alone, the relabel trace over keyed recordings and 51 capture rows" "FullyQualifiedName~.WhereTheSpaceIsDecidedTests." --no-build
    ;;
esac

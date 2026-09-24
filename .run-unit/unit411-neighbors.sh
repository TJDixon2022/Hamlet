#!/bin/sh
# unit 411 - the types that read ReceiverSetup, ReceiverSetupVoice or the rig's write result, one per invocation.
# Usage: sh .run-unit/unit411-neighbors.sh <suffix> "<TASK n of 4>"
cd /c/Source/HamLet || exit 1
R=.run-unit/unit411-run.sh
for t in HamletSaysWhatItChangedTests TheTuneInSetsOnlyWhatIsInTheWayTests TheRoundTripLandsInTheSamePlaceTests TheBannerSaysWhatTheRadioReadBackTests
do
  echo "== $t"
  sh $R "$t-$1" engine 300 "$2" "Running $t alone, a neighbor of the banner change" "FullyQualifiedName~.$t." --no-build
done
for t in TheStatusBarStopsLecturingTests HowMuchTheApplicationSaysTests WhichPathsPutNarrationOnTheBarTests
do
  echo "== $t"
  sh $R "$t-$1" app 300 "$2" "Running $t alone, a neighbor of the banner change" "FullyQualifiedName~.$t." --no-build
done

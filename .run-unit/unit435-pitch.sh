#!/bin/sh
# unit 435 - every Cw type whose name speaks of pitch, tone, tracker or survey, and the two that carry ASignalAtTheWrongPitchIsStillFound's name, one type per invocation.
# Usage: sh .run-unit/unit435-pitch.sh <suffix> "<TASK n of 3>"
cd /c/Source/HamLet || exit 1
N=0
for T in AHeldPitchDoesNotOutliveItsEvidenceTests ContactTrackerTests CwSurveyThresholdPinTests CwToneSurveyTests CwTrackerSwitchTests EveryElementCarriesItsOwnPitchTests ThePeakFindsThePitchTheTrackerMissedTests ThePitchCanBeHeldTests TheSurveyAlreadyUsesAShortWindowTests TheTrackerSwitchTraceTests TheTwoPitchesTableTests CwAdjudicationTests CwDisplacementFloorTests
do
  N=$((N+1))
  OUT=.run-unit/unit435-pitch-$1-$T.txt
  sh tools/status.sh EXECUTING "$2" code none "Pitch types round $1: $T alone, $N of 13"
  START=$(date +%s)
  timeout 600 dotnet test tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj --no-build --logger "console;verbosity=detailed" --filter "FullyQualifiedName~.$T." > "$OUT" 2>&1
  RC=$?
  WALL=$(( $(date +%s) - START ))
  echo "RC=$RC WALL=${WALL}s" >> "$OUT"
  P=$(grep -E "^\s+Passed:" "$OUT" | tail -1 | tr -dc 0-9)
  F=$(grep -E "^\s+Failed:" "$OUT" | tail -1 | tr -dc 0-9)
  TOT=$(grep -E "^Total tests:" "$OUT" | tail -1 | tr -dc 0-9)
  echo "$T rc=$RC passed=${P:-0} failed=${F:-0} total=${TOT:-0} ${WALL}s"
  grep -E "^\s+Failed " "$OUT" | sed -E "s/^\s+//" | cut -c1-220
done

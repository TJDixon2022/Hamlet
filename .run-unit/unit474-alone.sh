#!/bin/sh
# unit 474 - every app type lost in either exit run of the line, run alone; each must pass.
cd /c/Source/HamLet || exit 1
for T in TheStopIsAlwaysOnScreenTests TheCarrierHoldsTheButtonsTests TheFavoritesAreChipsTests ThePsk31ConversationCardTests
do
  echo "== $T"
  sh .run-unit/unit474-run.sh "app-alone-$T-exit" 400 "4 of 4" "Task 4 - $T alone after a loss in the app line" "$T" --no-build | grep -E "^RC=|Total tests|Passed:|Failed:|^Failed "
done

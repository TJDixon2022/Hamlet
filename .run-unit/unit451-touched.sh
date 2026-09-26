#!/bin/sh
# unit 451 - every engine type touched or reading what changed, each alone, --no-build.
# Usage: sh .run-unit/unit451-touched.sh "<n of 3>"
cd /c/Source/HamLet || exit 1
for t in TheSpeedSaysWhetherItWasProvedTests ARefinementKeepsTheTimingTests WhatTheSpeedCanSayItProvedTests CwSpeedSilenceTests CwCaseCountsSayWhatTheyCountTests TheSwingIsTheFigureThatHoldsTests ThePitchSaysWhetherItWasProvedTests AHeldPitchDoesNotOutliveItsEvidenceTests; do
  echo "== $t"
  sh .run-unit/unit451-run.sh "touched-$t" engine 600 "$1" "Exit: engine type $t alone" "FullyQualifiedName~.$t." --no-build
done

#!/bin/sh
# unit 451 - build, then the app types that cover the sheet and every speed display, each alone.
# Usage: sh .run-unit/unit451-sheet.sh <suffix> "<n of 3>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit451-build.sh "sheet-$1" "$2" "Displays: building before the app types that cover the speed line and the speed displays"
for t in TheSpeedLineSaysWhatWasProvedTests AClearKeepsWhatTheDecoderWorkedOutTests ThePitchLineSaysWhatWasProvedTests TheRestOfTheSheetIsTrueTests EverySentenceOnTheSheetTests TheSheetSaysWhatEachElementWasSentAtTests CaseRosterSurvivesAnEveningTests ASheetSaysWhichInstrumentSpokeTests; do
  echo "== $t"
  sh .run-unit/unit451-run.sh "app-$t-$1" app 300 "$2" "Displays: app type $t alone" "FullyQualifiedName~.$t." --no-build
done
grep -a "^ *total |" .run-unit/unit451-app-TheSpeedLineSaysWhatWasProvedTests-$1.txt | cut -c1-300

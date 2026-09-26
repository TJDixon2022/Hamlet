#!/bin/sh
# unit 450 - build, then the app types that cover the sheet, each alone.
# Usage: sh .run-unit/unit450-sheet.sh <suffix> "<n of 3>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit450-build.sh "sheet-$1" "$2" "Sheet: building before the app types that cover the pitch line"
for t in ThePitchLineSaysWhatWasProvedTests TheRestOfTheSheetIsTrueTests EverySentenceOnTheSheetTests TheSheetSaysWhatEachElementWasSentAtTests TheTonePeakIsAboutThisRecordingTests WhatTheTonePeakIsAboutTests ThePitchControlsAreOffThePanelTests; do
  echo "== $t"
  sh .run-unit/unit450-run.sh "app-$t-$1" app 300 "$2" "Sheet: app type $t alone" "FullyQualifiedName~.$t." --no-build
done
grep -a "^ *total |" .run-unit/unit450-app-ThePitchLineSaysWhatWasProvedTests-$1.txt | cut -c1-300

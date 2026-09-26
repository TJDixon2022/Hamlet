#!/bin/sh
# unit 450 - every type touching the report's pitch fields, the tracker or the sheet, each alone.
cd /c/Source/HamLet || exit 1
for t in ThePitchSaysWhetherItWasProvedTests WhatThePitchCanSayItProvedTests AHeldPitchDoesNotOutliveItsEvidenceTests NothingActsOnTheAdmissionVerdictTests TheQuietestBinNoLongerWinsTests WhereAcquisitionPointsTests TheCaptureOfTheTwentyThirdTests CwTrackerSwitchTests TheTrackedPitchIsChosenByKeyingTests CwToneSurveyTests; do
  echo "== $t"
  sh .run-unit/unit450-run.sh "touched-$t" engine 600 "3 of 3" "Exit round: touched type $t alone" "FullyQualifiedName~.$t." --no-build
done
for t in ThePitchLineSaysWhatWasProvedTests TheRestOfTheSheetIsTrueTests EverySentenceOnTheSheetTests TheSheetSaysWhatEachElementWasSentAtTests TheTonePeakIsAboutThisRecordingTests WhatTheTonePeakIsAboutTests; do
  echo "== $t"
  sh .run-unit/unit450-run.sh "touched-$t" app 300 "3 of 3" "Exit round: touched app type $t alone" "FullyQualifiedName~.$t." --no-build
done

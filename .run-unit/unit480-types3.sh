#!/bin/sh
# unit 480 task 3 - the app types the training graph touches, one per invocation (HM-DEC-155). None reads a recording.
# Usage: sh .run-unit/unit480-types3.sh <suffix>
cd /c/Source/HamLet || exit 1
for t in TheBarsCarryTheirLettersTests TheScopeIsTheMiddlePictureTests TheScopeShowsTheMarksTests TheVerdictCarriesTheScopeTests TheOwnersVerdictIsARowTests VoiceTests
do
  echo "== app $t"
  sh .run-unit/unit480-type.sh app "$t" "$t-$1" 300 | grep -E "^RC=|^Failed|Total tests|Passed: |Failed: "
done

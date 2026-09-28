#!/bin/sh
# unit 480 task 1 - the touched types and ReceiverSetup's types with ScriptedRadio, one per invocation (HM-DEC-155). None reads a recording.
# Usage: sh .run-unit/unit480-types1.sh <suffix>
cd /c/Source/HamLet || exit 1
for t in TheScopeOutputIsACwConditionTests ScopeIsNeverTurnedOnTests EveryModeAnswersForEverySettingTests EveryMorseBlockSetsWhatCwSetsTests WhatEnteringAModeSetsTests AValueAlreadyRightIsNotWrittenTests TheTuneInSetsOnlyWhatIsInTheWayTests OneVoicePerFieldTests HamletSaysWhatItChangedTests TheOperatorsHandStandsTests TheOperatorsHandCrossesTheMorseBlocksTests ThePreampFollowsItsOwnTextTests ThePreampIsWhatTheManualSaysTests TheRoundTripLandsInTheSamePlaceTests TheBannerSaysWhatTheRadioReadBackTests TheBlockStatesWhatTheModeNeedsTests ScopeHonestyTests ScopeStreamTests ScopeOutputWriteTests
do
  echo "== $t"
  sh .run-unit/unit480-type.sh engine "$t" "$t-$1" 300 | grep -E "^RC=|^Failed|Total tests|Passed: |Failed: "
done
for t in WhichPathsPutNarrationOnTheBarTests OneVoiceOnThePreampTests WhatIsSaidAboutThePreampTests WhatHappensWhenTheBandOverloadsTests
do
  echo "== app $t"
  sh .run-unit/unit480-type.sh app "$t" "$t-$1" 300 | grep -E "^RC=|^Failed|Total tests|Passed: |Failed: "
done

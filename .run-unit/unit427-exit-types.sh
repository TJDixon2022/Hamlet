#!/bin/sh
# unit 427 task 4 - every type the unit touched, one per invocation, then the source diffs.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit427-types.sh exit "TASK 4 of 4" TheGridBeatsThePrefixTests TheFourteenOnScreenTests TheFavoritesAreChipsTests TheFavoritesAreUnderTheGreenZoneTests TheCqListNudgeTests TheCqReceiptTests TheGlobePlacesStationsTests TheMapOpensTests TheMarkIsSeenAndClickedTests TheNudgeHoverTests TheOliviaRowsReadLikePsk31Tests ThePsk31CardIsHandedHisGridTests ThePsk31ConversationCardTests ThePsk31ReadsTheConversationTests TheQuillAndThePageMeanTheSameTests TheQuillPopupTests TheRowHoverSaysWhatItKnowsTests Unit297CardSentenceTests Unit299GlobeTests Unit302CardNameTests TheGlobeOnTheCardFaceTests Unit299HeaderProbeTests BindingHealthTests TheCardsRightColumnTests TheCardOffersLogAndAnXTests Unit376TheTopBandTests DecisionLogOrderTests
for T in TheCallsignCountryIsCertainOrSilentTests WhatTheEntityTableResolvesTests
do
  sh .run-unit/unit427-run.sh "exit-engine-$T" engine 300 "TASK 4 of 4" "Exit round - touched engine type $T" "FullyQualifiedName~.$T." --no-build
done
echo "== src/Hamlet.RadioEngine/Cw against entry 79ac7034"
git diff --stat 79ac7034 -- src/Hamlet.RadioEngine/Cw
echo "== end Cw"
sh .run-unit/unit427-transmit.sh 79ac7034
echo "== all of src against entry 79ac7034"
git diff --stat 79ac7034 -- src data
echo "== end src"

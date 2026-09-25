#!/bin/sh
# unit 427 task 1 - every type the grid change touches, one per invocation.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit427-types.sh t1 "TASK 1 of 4" TheCqListNudgeTests TheCqReceiptTests TheGlobePlacesStationsTests TheMapOpensTests TheMarkIsSeenAndClickedTests TheNudgeHoverTests TheOliviaRowsReadLikePsk31Tests ThePsk31CardIsHandedHisGridTests ThePsk31ConversationCardTests ThePsk31ReadsTheConversationTests TheQuillAndThePageMeanTheSameTests TheQuillPopupTests TheRowHoverSaysWhatItKnowsTests Unit297CardSentenceTests Unit299GlobeTests Unit302CardNameTests TheContactStandsAfterHisLastTransmissionTests TheGlobeOnTheCardFaceTests Unit299HeaderProbeTests BindingHealthTests TheCardsRightColumnTests TheCardOffersLogAndAnXTests
for T in TheCallsignCountryIsCertainOrSilentTests WhatTheEntityTableResolvesTests
do
  sh .run-unit/unit427-run.sh "t1-engine-$T" engine 300 "TASK 1 of 4" "Touched engine type $T" "FullyQualifiedName~.$T."
done

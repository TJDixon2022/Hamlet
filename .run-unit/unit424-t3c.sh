#!/bin/sh
# unit 424 task 3 - build, the voice test, the trace, then the Rig types the voices touch.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit424-build.sh t3-c "TASK 3 of 4" "Task 3: building with the narrowed P.AMP/ATT rule and the attenuator-in fact" || exit 1
sh .run-unit/unit424-run.sh test78v-t3c app 240 "TASK 3 of 4" "Task 3: OneVoiceOnThePreampTests after the voice fixes" "FullyQualifiedName~.OneVoiceOnThePreampTests." --no-build
sh .run-unit/unit424-types.sh t3c "TASK 3 of 4" WhatIsSaidAboutThePreampTests
for T in HamletSaysWhatItChangedTests OneVoicePerFieldTests TheTuneInSetsOnlyWhatIsInTheWayTests WhatIsInTheWayOnTheReceiveSideTests RigObservationTests TheBannerSaysWhatTheRadioReadBackTests
do
  sh .run-unit/unit424-run.sh "rig-t3c-$T" engine 180 "TASK 3 of 4" "Task 3: $T alone after the voice fixes" "FullyQualifiedName~Hamlet.RadioEngine.Tests.Rig.$T." --no-build
done

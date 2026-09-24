#!/bin/sh
# unit 424 task 3 - build, the voice test, the trace, then the types the three voices touch.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit424-build.sh t3-a "TASK 3 of 4" "Task 3: building the three voice fixes - the setup says the value it set, advice and panel true while overloading" || exit 1
sh .run-unit/unit424-run.sh test78v-t3a app 240 "TASK 3 of 4" "Task 3: OneVoiceOnThePreampTests after the voice fixes" "FullyQualifiedName~.OneVoiceOnThePreampTests." --no-build
sh .run-unit/unit424-types.sh t3 "TASK 3 of 4" WhatIsSaidAboutThePreampTests TheOverloadSentenceLeavesTheModesFieldsTests TheFrontEndIsOnThePanelTests HowMuchTheApplicationSaysTests WhatElseIsComposedAtRuntimeTests TheStatusBarStopsLecturingTests ReceiveHelpViewModelTests RigDiagnosticsTests

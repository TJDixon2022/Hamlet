#!/bin/sh
# unit 424 task 3 - build, then OneVoiceOnThePreampTests alone, watched red.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit424-build.sh t3-red "TASK 3 of 4" "Task 3: building OneVoiceOnThePreampTests to watch it fail at 14.050" || exit 1
sh .run-unit/unit424-run.sh test78v-red app 240 "TASK 3 of 4" "Task 3: OneVoiceOnThePreampTests red, before any voice changes" "FullyQualifiedName~.OneVoiceOnThePreampTests." --no-build

#!/bin/sh
# unit 426 task 0 - the entry round, one type per invocation, each with its own timeout.
# Usage: sh .run-unit/unit426-entry.sh <part>
cd /c/Source/HamLet || exit 1
T="TASK 0 of 4"
case "$1" in
  build)
    sh .run-unit/unit426-build.sh entry "$T" "Entry round - Hamlet.sln build with warnings as errors before any test"
    ;;
  cfengine)
    sh .run-unit/unit426-cf.sh engine cf-engine-entry "$T" "Entry round - engine carry-forward line, one invocation" --no-build
    ;;
  cfapp)
    sh .run-unit/unit426-cf.sh app cf-app-entry "$T" "Entry round - app carry-forward line, one invocation" --no-build
    ;;
  floors)
    sh .run-unit/unit426-round.sh captures entry "$T"
    sh .run-unit/unit426-round.sh adjudicated entry "$T"
    sh .run-unit/unit426-round.sh named entry "$T"
    ;;
  rigA)
    sh .run-unit/unit426-rig.sh entry "$T" "[A-S]"
    ;;
  rigB)
    sh .run-unit/unit426-rig.sh entry "$T" "[T-Z]"
    ;;
  named)
    sh .run-unit/unit426-types.sh entry "$T" OneVoiceOnThePreampTests WhatIsSaidAboutThePreampTests TheOverloadSentenceLeavesTheModesFieldsTests DecisionLogOrderTests
    ;;
esac

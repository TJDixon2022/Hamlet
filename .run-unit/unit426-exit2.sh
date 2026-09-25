#!/bin/sh
# unit 426 task 4 - the exit round again on the final source, one type per invocation.
# Usage: sh .run-unit/unit426-exit2.sh <part>
cd /c/Source/HamLet || exit 1
T="TASK 4 of 4"
case "$1" in
  build)
    sh .run-unit/unit426-fullbuild.sh
    ;;
  cfengine)
    sh .run-unit/unit426-cf.sh engine cf-engine-exit2 "$T" "Exit round again on the final source - engine carry-forward line" --no-build
    ;;
  cfapp)
    sh .run-unit/unit426-cf.sh app cf-app-exit2 "$T" "Exit round again on the final source - app carry-forward line" --no-build
    ;;
  floors)
    sh .run-unit/unit426-round.sh captures exit2 "$T"
    sh .run-unit/unit426-round.sh adjudicated exit2 "$T"
    sh .run-unit/unit426-round.sh named exit2 "$T"
    ;;
  rig)
    sh .run-unit/unit426-rig.sh exit2 "$T" "[A-Z]" > /dev/null
    sh .run-unit/unit426-rigsum.sh exit2
    ;;
  types)
    sh .run-unit/unit426-types.sh exit2 "$T" WhatHappensWhenTheBandOverloadsTests TheOverloadSentenceSaysWhereThePreampIsTests OneVoiceOnThePreampTests WhatIsSaidAboutThePreampTests TheOverloadSentenceLeavesTheModesFieldsTests TheFrontEndIsOnThePanelTests HowMuchTheApplicationSaysTests BindingHealthTests WhichPathsPutNarrationOnTheBarTests TheStatusBarStopsLecturingTests WhatElseIsComposedAtRuntimeTests RigDiagnosticsTests DecisionLogOrderTests
    ;;
esac

#!/bin/sh
# unit 429 task 0 - the entry round: build, both carry-forward lines, the three floors, the keyed totals and baseline, the stray trace, the decision log order.
cd /c/Source/HamLet || exit 1
T="TASK 0 of 4"
echo "== build"
sh .run-unit/unit429-build.sh entry "$T" "Entry round: building Hamlet.sln with warnings as errors before any test"
echo "== cf engine"
sh .run-unit/unit429-cf.sh engine cf-engine-entry "$T" "Entry round: engine carry-forward line" --no-build
echo "== cf app"
sh .run-unit/unit429-cf.sh app cf-app-entry "$T" "Entry round: app carry-forward line" --no-build
echo "== captures"
sh .run-unit/unit429-round.sh captures entry "$T"
echo "== adjudicated"
sh .run-unit/unit429-round.sh adjudicated entry "$T"
echo "== named"
sh .run-unit/unit429-round.sh named entry "$T"
echo "== keyed"
sh .run-unit/unit429-round.sh keyed entry "$T"
echo "== baseline"
sh .run-unit/unit429-round.sh baseline entry "$T"
echo "== strays"
sh .run-unit/unit429-run.sh strays-entry engine 600 "$T" "Entry round: WhatTheStrayLettersRestOnTests alone, the added single-element letters" "FullyQualifiedName~.WhatTheStrayLettersRestOnTests." --no-build
echo "== decision log"
sh .run-unit/unit429-run.sh type-t0-DecisionLogOrderTests app 240 "$T" "Entry round: DecisionLogOrderTests, expected red on HM-DEC-166 (P25)" "FullyQualifiedName~.DecisionLogOrderTests." --no-build

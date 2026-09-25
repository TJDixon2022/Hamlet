#!/bin/sh
# unit 431 task 0 - the entry round: build, both carry-forward lines, the three floors, the keyed totals and baseline, the stray trace.
cd /c/Source/HamLet || exit 1
T="TASK 0 of 4"
echo "== build"
sh .run-unit/unit431-build.sh entry "$T" "Entry round: building Hamlet.sln with warnings as errors before any test"
echo "== cf engine"
sh .run-unit/unit431-cf.sh engine cf-engine-entry "$T" "Entry round: engine carry-forward line, about six minutes" --no-build
echo "== cf app"
sh .run-unit/unit431-cf.sh app cf-app-entry "$T" "Entry round: app carry-forward line" --no-build
echo "== captures"
sh .run-unit/unit431-round.sh captures entry "$T"
echo "== adjudicated"
sh .run-unit/unit431-round.sh adjudicated entry "$T"
echo "== named"
sh .run-unit/unit431-round.sh named entry "$T"
echo "== keyed"
sh .run-unit/unit431-round.sh keyed entry "$T"
echo "== baseline"
sh .run-unit/unit431-round.sh baseline entry "$T"
echo "== strays"
sh .run-unit/unit431-run.sh strays-entry engine 600 "$T" "Entry round: WhatTheStrayLettersRestOnTests alone, the added single-element letters" "FullyQualifiedName~.WhatTheStrayLettersRestOnTests." --no-build

#!/bin/sh
# unit 451 - task 1 commit: the trace printer, its output, the HEAD speed-line printer and the notes.
cd /c/Source/HamLet || exit 1
cp .run-unit/unit451-trace-speed-state-run2.txt .run-unit/unit451-trace-speed-state.txt
sh tools/status.sh COMPLETED "1 of 3" code none "Trace done: 30042 real hops that show a number at HEAD would say hypothesis; next the three tests, then the state"
sh .run-unit/unit451-commit.sh .run-unit/unit451-msg1.txt PROJECT_STATUS.md tests/Hamlet.RadioEngine.Tests/Cw/WhatTheSpeedCanSayItProvedTests.cs tests/Hamlet.App.Tests/Cw/TheSpeedLineSaysWhatWasProvedTests.cs .run-unit/unit451-*

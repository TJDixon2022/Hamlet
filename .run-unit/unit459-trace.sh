#!/bin/sh
# unit 459 task 1 - rebuild, re-run the trace printer, and assemble .run-unit/unit459-trace.txt from its head and the printout.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit459-build.sh t1d "1 of 4" "Task 1: rebuilding the trace printer with the cw.cxx lines checked against upstream"
sh .run-unit/unit459-run.sh trace-t1 engine 600 "1 of 4" "Task 1: final printout of the 18 departures, grouped; (D) speed tracking chosen" "FullyQualifiedName~.WhereOursLosesWhatThePortKeepsTests." --no-build
cp .run-unit/unit459-trace-head.txt .run-unit/unit459-trace.txt
grep -aE "^ *(departure|evidence|mechanism|group|behind|sameness) \|" .run-unit/unit459-trace-t1.txt >> .run-unit/unit459-trace.txt
wc -l .run-unit/unit459-trace.txt

#!/bin/sh
# unit 439 (by hand) task 0 - the entry round: build, both carry-forward lines, the three floors, keyed totals, strays.
cd /c/Source/HamLet || exit 1
T="TASK 0 of 4"
echo "== build"; sh .run-unit/unit439h-build.sh entry "$T" "Entry round: building Hamlet.sln with warnings as errors before any test"

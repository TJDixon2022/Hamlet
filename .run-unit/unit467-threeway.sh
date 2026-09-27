#!/bin/sh
# unit 466 task 2 - run the three-way fact and keep its printout as .run-unit/unit467-threeway<suffix>.txt.
# Usage: sh .run-unit/unit467-threeway.sh <suffix> "<n of 4>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit467-run.sh "threeway-raw$1" engine 600 "$2" "Task 2 - scoring the arbitrated output, ours alone and the port alone on every metric per row, harness and live, all 35" "FullyQualifiedName~.TheArbitrationEarnsItsPlaceFact.TheThreeWayTable" --no-build
grep -a -E "^ *(check|threeway|verdict|switch|span084|req) \|" .run-unit/unit467-threeway-raw$1.txt | sed 's/^ *//' > .run-unit/unit467-threeway$1.txt
wc -l .run-unit/unit467-threeway$1.txt
grep -c "same symbols" .run-unit/unit467-threeway$1.txt
grep "DIFFERS\|not run" .run-unit/unit467-threeway$1.txt
grep "^switch |" .run-unit/unit467-threeway$1.txt | cut -c1-260

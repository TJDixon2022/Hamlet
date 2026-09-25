#!/bin/sh
# unit 427 task 2 - build, then the sweep type alone.
# Usage: sh .run-unit/unit427-t2.sh <suffix>
cd /c/Source/HamLet || exit 1
sh .run-unit/unit427-build.sh "t2-$1" "TASK 2 of 4" "Task 2 - building the sweep of the owner's items 1 to 13"
sh .run-unit/unit427-types.sh "t2-$1" "TASK 2 of 4" TheFourteenOnScreenTests

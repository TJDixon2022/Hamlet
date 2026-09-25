#!/bin/sh
# unit 427 task 1 - build, then the grid test type alone.
# Usage: sh .run-unit/unit427-t1.sh <suffix> "<note>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit427-build.sh "t1-$1" "TASK 1 of 4" "$2 - building first"
sh .run-unit/unit427-types.sh "t1-$1" "TASK 1 of 4" TheGridBeatsThePrefixTests

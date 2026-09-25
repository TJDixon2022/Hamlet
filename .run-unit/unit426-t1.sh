#!/bin/sh
# unit 426 task 1 - build, then the trace type alone.
# Usage: sh .run-unit/unit426-t1.sh <suffix> "<TASK n of 4>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit426-build.sh "t1-$1" "$2" "Building the live-overload trace, $1"
sh .run-unit/unit426-types.sh "t1-$1" "$2" WhatHappensWhenTheBandOverloadsTests

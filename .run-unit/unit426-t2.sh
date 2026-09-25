#!/bin/sh
# unit 426 task 2 - build, then each red type alone and the trace, one type per invocation.
# Usage: sh .run-unit/unit426-t2.sh <suffix> "<TASK n of 4>"
cd /c/Source/HamLet || exit 1
sh .run-unit/unit426-build.sh "t2-$1" "$2" "Building task 2's tests, $1"
for T in ThePreampFollowsTheOverloadTests TheReceiveGainIsReadOnItsOwnScaleTests
do
  sh .run-unit/unit426-run.sh "t2-$1-$T" engine 300 "$2" "Task 2 - running $T alone" "FullyQualifiedName~.Rig.$T." --no-build
done
sh .run-unit/unit426-types.sh "t2-$1" "$2" TheOverloadSentenceSaysWhereThePreampIsTests WhatHappensWhenTheBandOverloadsTests

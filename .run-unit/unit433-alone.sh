#!/bin/sh
# unit 433 - re-run app types lost to the dispatcher loop, one type per invocation.
# Usage: sh .run-unit/unit433-alone.sh <suffix> "<TASK n of 3>" <Type>...
cd /c/Source/HamLet || exit 1
SUF=$1
TK=$2
shift 2
for T in "$@"
do
  sh .run-unit/unit433-run.sh "app-alone-$SUF-$T" app 240 "$TK" "Re-running $T alone after the dispatcher loop" "FullyQualifiedName~.$T." --no-build
done

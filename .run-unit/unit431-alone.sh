#!/bin/sh
# unit 431 - re-run app types lost to the dispatcher loop, one type per invocation.
# Usage: sh .run-unit/unit431-alone.sh <suffix> "<TASK n of 4>" <Type>...
cd /c/Source/HamLet || exit 1
SUF=$1
TK=$2
shift 2
for T in "$@"
do
  sh .run-unit/unit431-run.sh "app-alone-$SUF-$T" app 240 "$TK" "Re-running $T alone after the dispatcher loop" "FullyQualifiedName~.$T." --no-build
done

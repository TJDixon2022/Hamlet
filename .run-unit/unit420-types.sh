#!/bin/sh
# unit 420 - run named engine types, one type per invocation, --no-build.
# Usage: sh .run-unit/unit420-types.sh <suffix> "<TASK n of 3>" <engine|app> <Type>...
cd /c/Source/HamLet || exit 1
SUF=$1
TK=$2
P=$3
shift 3
for T in "$@"
do
  sh .run-unit/unit420-run.sh "$SUF-$T" "$P" 300 "$TK" "Running $T alone, $SUF" "FullyQualifiedName~.$T." --no-build
done

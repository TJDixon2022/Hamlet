#!/bin/sh
# unit 421 - engine types one per invocation, --no-build, each with its own timeout.
# Usage: sh .run-unit/unit421-types.sh <suffix> "<TASK n of 4>" <Type>...
cd /c/Source/HamLet || exit 1
SUF=$1
TK=$2
shift 2
for T in "$@"
do
  echo "== $T"
  sh .run-unit/unit421-run.sh "$SUF-$T" engine 300 "$TK" "Running $T alone against the stray-element change" "FullyQualifiedName~.$T." --no-build
done

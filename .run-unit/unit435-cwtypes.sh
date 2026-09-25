#!/bin/sh
# unit 435 - engine Cw types one per invocation, --no-build, each with its own timeout.
# Usage: sh .run-unit/unit435-cwtypes.sh <suffix> "<TASK n of 3>" <Type>...
cd /c/Source/HamLet || exit 1
SUF=$1
TK=$2
shift 2
for T in "$@"
do
  echo "== $T"
  sh .run-unit/unit435-run.sh "type-$SUF-$T" engine 300 "$TK" "Exit round: $T alone, a Cw type unit 421 ran at its task 3" "FullyQualifiedName~.$T." --no-build
done

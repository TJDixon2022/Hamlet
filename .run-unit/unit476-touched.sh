#!/bin/sh
# unit 476 - the touched app types, one dotnet test invocation each, own timeout, no build.
# Usage: sh .run-unit/unit476-touched.sh <suffix> "<n of 4>" Type1 Type2 ...
cd /c/Source/HamLet || exit 1
SUF=$1
TASK=$2
shift 2
for T in "$@"
do
  sh .run-unit/unit476-run.sh "$SUF-$T" 400 "$TASK" "$TASK - running touched type $T alone" "$T" app --no-build > /dev/null
  printf "%s: " "$T"
  grep -a -E "^(Passed!|Failed!)|Total tests|^\s+(Passed|Failed): " ".run-unit/unit476-$SUF-$T.txt" | tr -s " " | tr "\n" " "
  echo
  grep -a -E "^\s+Failed " ".run-unit/unit476-$SUF-$T.txt" | sed -E "s/^\s+//" | cut -c1-160
done

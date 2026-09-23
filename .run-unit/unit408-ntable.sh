#!/bin/sh
# unit 408 - one sorted row per capture from a named-floor captures run:
# name named named-elements placeholders emitted tone
# Usage: sh .run-unit/unit408-ntable.sh <outname> [<against-outname>]
cd /c/Source/HamLet || exit 1
grep -E "^ [a-z/0-9-]+: [0-9]+ named against a floor" ".run-unit/unit408-$1.txt" \
  | sed -E 's/^ ([^:]+): ([0-9]+) named against a floor of [0-9]+, ([0-9]+) named elements against [0-9]+, ([0-9]+) placeholders where [0-9]+ settled when the floor was set, ([0-9]+) emitted in all, at ([0-9]+) Hz.*/\1 \2 \3 \4 \5 \6/' \
  | sed 's#unadjudicated/##' | sort -u > ".run-unit/unit408-$1-ntable.txt"
wc -l < ".run-unit/unit408-$1-ntable.txt"
if [ -n "$2" ]; then
  echo "== rows that differ from $2: name named elements placeholders emitted tone"
  diff ".run-unit/unit408-$2-ntable.txt" ".run-unit/unit408-$1-ntable.txt" && echo "IDENTICAL"
  echo "== named or named elements lower than $2"
  join ".run-unit/unit408-$2-ntable.txt" ".run-unit/unit408-$1-ntable.txt" \
    | awk '{ if ($7 < $2 || $8 < $3) print "LOWER " $0 }'
  echo "== end"
fi

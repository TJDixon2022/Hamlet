#!/bin/sh
# unit 409 - pull the table's row lines out of a run, optionally diffed against another run's.
# Usage: sh .run-unit/unit409-rows.sh <outname> [<against-outname>]
cd /c/Source/HamLet || exit 1
grep -E "^\s*(row|seg|sweeps-differ|count) \|" ".run-unit/unit409-$1.txt" | sed -E "s/^\s+//" > ".run-unit/unit409-$1-rows.txt"
wc -l < ".run-unit/unit409-$1-rows.txt"
if [ -n "$2" ]; then
  diff ".run-unit/unit409-$2-rows.txt" ".run-unit/unit409-$1-rows.txt" && echo "IDENTICAL"
fi
